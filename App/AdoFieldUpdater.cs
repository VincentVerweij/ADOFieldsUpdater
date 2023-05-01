using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Serilog;

namespace App
{
    internal class AdoFieldUpdater : IPlatformFieldUpdater
    {
        private readonly string _completedWorkFieldName = "Microsoft.VSTS.Scheduling.CompletedWork";
        private string _azureDevOpsOrganizationUrl;
        private VssConnection _connection;
        private AdoFieldUpdater() { }

        private async Task<WorkItem> GetWorkItemForIdAsync(int workItemId)
        {
            var adoClient = _connection.GetClient<WorkItemTrackingHttpClient>();

            try
            {
                var workItem = await adoClient.GetWorkItemAsync(workItemId);
                return workItem;
            }
            catch (Exception ex) 
            {
                _logger.Error("Was not able to fetch the work item because of error: {@exception}", ex.Message);
            }

            return null;
        }

        public IBindingPart BindingPart { get; init; }
        public string personalAccessToken { get; init; }
        private readonly ILogger _logger;

        public AdoFieldUpdater(IBindingPart bindingPart, string pat, ILogger logger)
        {
            BindingPart = bindingPart;
            personalAccessToken = pat;
            _logger = logger;
            _logger.Information("Connecting to Azure DevOps organization with name '{@organization}'", BindingPart.ConnectionNameToBind);
            _azureDevOpsOrganizationUrl = $"https://dev.azure.com/{BindingPart.ConnectionNameToBind}";
            _connection = new VssConnection(new Uri(_azureDevOpsOrganizationUrl), new VssBasicCredential(string.Empty, pat));
        }

        public async Task UpdateCompletedTimeAsync(double workDone)
        {
            WorkItem workItem = await GetWorkItemForIdAsync(BindingPart.ConnectionItemIdToUpdate);

            if (workItem is null)
            {
                _logger.Warning("No work item was found with ID '{@workItemId}' in organization '{@organization}'", BindingPart.ConnectionItemIdToUpdate, BindingPart.ConnectionNameToBind);
                return;
            }

            var fields = workItem?.Fields;
            double completedWork = 0;

            if (fields is not null && fields.ContainsKey(_completedWorkFieldName))
            {
                _ = double.TryParse(workItem?.Fields[_completedWorkFieldName].ToString(), out completedWork);
                _logger.Information("Fetched completed work with value: {@completedWork}", workItem?.Fields[_completedWorkFieldName]);
            }

            var patchDocument = new JsonPatchDocument
            {
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = $"/fields/{_completedWorkFieldName}",
                    Value = completedWork + workDone
                }
            };

            try
            {
                var adoClient = _connection.GetClient<WorkItemTrackingHttpClient>();

                _logger.Information("Skipping update in DevOps for now");
                var result = await adoClient.UpdateWorkItemAsync(patchDocument, (int)workItem.Id);
                _logger.Information("Updated Azure DevOps work item with URL '{@workItemUrl}'", result.Url);
            }
            catch (Exception ex)
            {
                _logger.Error("Was not able to update the work item because of error: {@exception}", ex.Message);
            }
        }
    }
}
