using Clockify.Net;
using Clockify.Net.Models.TimeEntries;
using Serilog;

namespace App
{
    internal class ClockifyTimeTracker : ITimeTracker
    {
        private readonly ClockifyClient _clockifyClient;
        private readonly ILogger _logger;

        private ClockifyTimeTracker(){}

        public ClockifyTimeTracker(string workspaceName, string apiKey, ILogger logger)
        {
            WorkspaceName = workspaceName;
            ApiKey = apiKey;
            _clockifyClient = new(ApiKey);
            _logger = logger;
        }

        private async Task<string> GetWorkspaceIdFromNameAsync(string workspaceName)
        {
            var workspacesResponse = await _clockifyClient.GetWorkspacesAsync();
            var workspace = workspacesResponse.Data.SingleOrDefault(dto => dto.Name == workspaceName);
            if (workspace == null)
            {
                _logger.Warning("Could not find a workspace with name {@workspaceName}", workspaceName);
                return string.Empty;
            }

            return workspace.Id;
        }

        private async Task<string> GetCurrentUserId()
        {
            var userResponse = await _clockifyClient.GetCurrentUserAsync();
            if (!userResponse.IsSuccessful)
            {
                _logger.Warning("Unable to fetch the current user from Clockify");
                return string.Empty;
            }

            return userResponse.Data.Id;
        }

        private async Task<List<TimeEntryDtoImpl>> GetTimeEntriesForWorkspaceAndUserAsync(string workspaceId, string userId, int daysAgo = -1)
        {
            List<TimeEntryDtoImpl> fetchedEntries = new();

            _logger.Information("Fetching Clockify's time entries for workspace ID {@workspaceId}, user ID {@userId} and days ago set to {@daysAgo}",
                workspaceId, userId, daysAgo);

            var pageSize = 50;
            var pageIndex = 1;
            int fetchedItems = -1;

            do
            {
                _logger.Information("Fetching page {@page} with page size of {@pageItems}", pageIndex, pageSize);
                var timeEntriesResponse = await _clockifyClient.FindAllTimeEntriesForUserAsync(workspaceId, userId,
                    start: DateTimeOffset.Now.AddDays(daysAgo),
                    end: DateTimeOffset.Now,
                    page: pageIndex,
                    pageSize: pageSize);

                fetchedItems = (int)(timeEntriesResponse?.Data?.Count);

                if (timeEntriesResponse.IsSuccessful)
                {
                    fetchedEntries.AddRange(timeEntriesResponse?.Data);
                    _logger.Information("Fetched {@pageItems} from page {@page}", timeEntriesResponse?.Data?.Count, pageIndex);
                }

                pageIndex++;
            } while (fetchedItems == -1 || fetchedItems == 50);

            _logger.Information("Fetched all time entries from Clockify");

            return fetchedEntries;
        }

        public string WorkspaceName { get; init; }
        public string ApiKey { get; init; }

        public async Task<List<ITrackEntity>> GetTimeTrackingEntityAsync(int daysAgo = -1)
        {
            var workspaceId = await GetWorkspaceIdFromNameAsync(WorkspaceName);
            var userId = await GetCurrentUserId();

            var timeEntries = await GetTimeEntriesForWorkspaceAndUserAsync(workspaceId, userId, daysAgo);
            List<ITrackEntity> trackEntities = new();

            foreach (var timeEntry in timeEntries)
            {
                ClockifyTrackEntity clockifyTrackEntity = new()
                {
                    Description = timeEntry.Description,
                    Start = timeEntry.TimeInterval.Start,
                    End = timeEntry.TimeInterval.End
                };

                trackEntities.Add(clockifyTrackEntity);
            }

            return trackEntities;
        }
    }
}
