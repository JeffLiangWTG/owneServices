using Common.Logging;

using System.Xml;

namespace CargoWise.eHub.Products.GBCustoms.Core.Correlation.Handlers
{
    public class XMLBodyExtractionHandler : IExtractionHandler
    {
        public string Extract(ILog logger, string content, string searchPath)
        {
            var doc = new XmlDocument();
            try
            {
                doc.LoadXml(content);
                var value = doc.SelectSingleNode(searchPath);
                return value?.Value ?? string.Empty;
            }
            catch
            {
                logger.Warn($"GBCustomsCorrelation - Could not load XML [{content}]");
                return string.Empty;
            }
        }
    }
}
