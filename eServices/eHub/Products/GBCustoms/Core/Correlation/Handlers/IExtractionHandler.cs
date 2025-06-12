using Common.Logging;

namespace CargoWise.eHub.Products.GBCustoms.Core.Correlation.Handlers
{
    public interface IExtractionHandler
    {
        string Extract(ILog logger, string content, string searchCriteria);
    }
}
