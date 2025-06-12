using Common.Logging;
using System.Linq;
using System.Text.RegularExpressions;

namespace CargoWise.eHub.Products.GBCustoms.Core.Correlation.Handlers
{
    public class ParameterExtractionHandler : IExtractionHandler
    {
        public string Extract(ILog logger, string content, string regex)
        {
            try
            {
                Regex pattern = new Regex(regex);
                Match match = pattern.Match(content);
                return match.Groups["CorrelationId"].Value;
            }
            catch
            {
                logger.Warn($"GBCustomsCorrelation - Could not extract regex {regex} from {content}.");
                return string.Empty;
            }
        }
    }
}
