using App;
using Serilog;
using Serilog.Exceptions;
using System.CommandLine;

var clockifyWorkspaceOption = new Option<string>(
    name: "--clockify-workspace-name",
    description: "The name of the Clockify workspace.");
clockifyWorkspaceOption.IsRequired = true;
clockifyWorkspaceOption.AddAlias("-w");

var clockifyApiKeyOption = new Option<string>(
    name: "--clockify-api-key",
    description: "The API key that is used to query the Clockify API.");
clockifyApiKeyOption.IsRequired = true;
clockifyApiKeyOption.AddAlias("-a");

var bindingEntityPersonalAccessTokensOption = new Option<string[]>(
    name: "--entity-pat",
    description: "Mapping of the organization and its personal access token, delimited with a colon, to access the platform.");
bindingEntityPersonalAccessTokensOption.IsRequired = true;
bindingEntityPersonalAccessTokensOption.AddAlias("-p");

var rootCommand = new RootCommand("App that pushes completed time from Clockify into a platform");
rootCommand.AddGlobalOption(clockifyWorkspaceOption);
rootCommand.AddGlobalOption(clockifyApiKeyOption);
rootCommand.AddGlobalOption(bindingEntityPersonalAccessTokensOption);

rootCommand.SetHandler(
    async (workspaceName, clockifyApiKey, patMappings) =>
    {
        ILogger logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .WriteTo.Console()
            .CreateLogger();

        Dictionary<string, string> entityPat = new();

        foreach (var patMapping in patMappings)
        {
            var split = patMapping.Split(':');
            entityPat.Add(split[0], split[1]);
        }

        ITimeTracker clockifyTimeTracker = new ClockifyTimeTracker(workspaceName, clockifyApiKey, logger);
        var timeEntities = await clockifyTimeTracker.GetTimeTrackingEntityAsync();

        foreach (var timeEntry in timeEntities)
        {
            logger.Information("Working on time entry with description '{@description}', start {@start} and end {@end}", timeEntry.Description, timeEntry.Start, timeEntry.End);

            IConnectionBindingParser bindingParse = new AdoConnectionBindingParser(logger);
            List<IBindingPart> adoBindingParts = bindingParse.ParseBindingPartsFromText(timeEntry.Description);

            TimeSpan? duration = timeEntry.End - timeEntry.Start;

            foreach (var adoBindingPart in adoBindingParts)
            {
                if (!entityPat.ContainsKey(adoBindingPart.ConnectionNameToBind))
                {
                    logger.Warning("Skipping update for organization '{@organization}' because no PAT was provided for this organization with --entity-pat", 
                        adoBindingPart.ConnectionNameToBind);

                    continue;
                }

                var pat = entityPat[adoBindingPart.ConnectionNameToBind];
                IPlatformFieldUpdater adoFieldUpdater = new AdoFieldUpdater(adoBindingPart, pat, logger);

                await adoFieldUpdater.UpdateCompletedTimeAsync(duration.Value.TotalHours);
            }
        }

        logger.Information("Done processing");
    }, 
    clockifyWorkspaceOption, 
    clockifyApiKeyOption, 
    bindingEntityPersonalAccessTokensOption);

await rootCommand.InvokeAsync(args);