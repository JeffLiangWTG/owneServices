using System.Threading.Tasks;
using Hawking.CSI.Monitoring.Models;

namespace Hawking.CSI.Monitoring.Services
{
    public interface IMetricsStoreClient
    {
        Task StoreMetrics(TransactionMetrics metrics);
    }
}