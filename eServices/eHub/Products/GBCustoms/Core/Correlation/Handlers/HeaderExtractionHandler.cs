using Common.Logging;

using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.eHub.Products.GBCustoms.Core.Correlation.Handlers
{
    public class HeaderExtractionHandler : IExtractionHandler
    {
        public string Extract(ILog logger, string content, string headerName)
        {
            try
            {
                var listOfHeaders = content.Trim().Split('\n').ToList();
                var comparer = StringComparer.OrdinalIgnoreCase;
                var headers = new Dictionary<string, string>(comparer);
                listOfHeaders.ForEach(x =>
                {
                    var splitHeader = x.Trim().Split(':');
                    headers.Add(splitHeader[0], splitHeader[1].Trim());
                });
                return headers[headerName];
            }
            catch
            {
                logger.Warn($"GBCustomsCorrelation - Could not extract Header {headerName} from {content}.");
                return string.Empty;
            }
        }
    }
}
