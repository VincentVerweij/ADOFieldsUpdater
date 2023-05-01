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
