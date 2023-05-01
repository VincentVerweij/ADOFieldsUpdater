# App

This app uses entries in Clockify to set time in Azure DevOps.  
It does that by checking a pattern.  
When the pattern is detected it will try to connect to the Azure DevOps organization and fill in the `Completed work` field.  

## Pattern

The description is scanned for a specific pattern.  
The pattern consist out of 3 parts:
- A placeholder: `ADO`
- An organization name: `fabricam`
- A work item ID: `42`

When we have a time entry with for example this description:
`Investigating how to use the Clockify API ADO:fabricam:42`  

It will try to connect to Azure DevOps, it does use this URL 'https://dev.azure.com/fabricam' to connect.  
And then it looks for a work item with ID 42.

Multiple patterns can be put inside the description.

## Usage

The application is using `System.Commandline` to provide us with a CLI interface.  
When running the application, it will show you which options can be specified (and which are required):  
![image](https://user-images.githubusercontent.com/41574930/235513762-26fc3f1b-00a6-466d-855f-6e622a766b84.png)

This is an example of how the application can be called:  
`dotnet .\App.dll --clockify-workspace-name "Vincent Verweij's test workspace" --clockify-api-key <CLOCKIFY_API_KEY_HERE> --entity-pat cloudfuelbe:<ADO_PAT_FOR_CLOUDFUELBE_ORG> --entity-pat quaxconsulting:<ADO_PAT_FOR_QUAXCONSULTING_ORG>`  

If you would like to it to write to multiple organizations, provide multiple `--entity-pat` or `-p` options.  
This expects a format of `organization_name`:`pat_that_belongs_to_organization`

## Logs

The application uses `Serilog` to provide you with more insights.
This is an example of how the logs are shown:  
![example logs](https://user-images.githubusercontent.com/41574930/235512813-9a11fc70-32cd-4163-8f63-4fbe7f4ea717.png)

# Release-please

For release-please we need to make sure the following things are done:

1. Go into the settings of the repository
2. In `General` under section `Pull Requests` disable `Allow merge commits`
3. In `General` under section `Pull Requests` for `Allow squash merging` change it to `Default to pull request title and commit details`
4. In `General` under section `Pull Requests` enable `Always suggest updating pull request branches`
5. In `Branches` create a new `Branch protection rule`
6. Enfore pull requests and these things, unfortunately this does not work for a free account.
7. In `Actions/General` under section `Workflow permissions` change the option to `Read and write permissions`
8. In `Actions/General` under section `Workflow permissions` enable `Allow GitHub Actions to create and approve pull requests`
9. Prepare a new release with a tag, this way release-please can start from that one onwards
