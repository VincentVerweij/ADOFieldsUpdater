using Serilog;

namespace App
{
    internal class AdoConnectionBindingParser : IConnectionBindingParser
    {
        private readonly ILogger _logger;

        private AdoConnectionBindingParser() { }

        public AdoConnectionBindingParser(ILogger logger)
        {
            _logger = logger;
        }

        public List<IBindingPart> ParseBindingPartsFromText(string text)
        {
            List<IBindingPart> bindingParts = new();

            _logger.Information("Going to parse text '{@text}'", text);

            int indexWhereAdoPlaceholderIsAt = text.IndexOf(AdoBindingPart.BindingPlaceholder);

            while (indexWhereAdoPlaceholderIsAt != -1)
            {
                text = text.Substring(indexWhereAdoPlaceholderIsAt);
                var adoConnectionBindingChunks = text.Split(':');
                if (adoConnectionBindingChunks.Length >= 3)
                {
                    var adoPlaceholderChunk = adoConnectionBindingChunks[0];
                    var organizationChunk = adoConnectionBindingChunks[1];
                    var workItemIdChunk = adoConnectionBindingChunks[2];
                    workItemIdChunk = new string(workItemIdChunk.Trim().TakeWhile(c => char.IsDigit(c)).ToArray());

                    _logger.Information("Parsed organization '{@organization}' with work item ID '{@workitemid}'", organizationChunk, workItemIdChunk);

                    AdoBindingPart part = new(organizationChunk, int.Parse(workItemIdChunk));
                    bindingParts.Add(part);

                    var l = adoPlaceholderChunk.Length + 1 + organizationChunk.Length + 1 + workItemIdChunk.Length;
                    text = text.Substring(l);
                }

                indexWhereAdoPlaceholderIsAt = text.IndexOf(AdoBindingPart.BindingPlaceholder);
            }

            return bindingParts;
        }
    }
}
