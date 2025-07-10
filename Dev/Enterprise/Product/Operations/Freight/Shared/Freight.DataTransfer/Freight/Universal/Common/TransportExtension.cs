using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.DataTransfer.Universal
{
	static class TransportExtension
	{
		public static bool HasLogWith(this Transport transport, ZString eventType, ZBool isEstimate)
		{
			return transport.Logs.GetAllLogs().Any(l => ((StmALog)l).SL_SE_NKEvent == eventType && ((StmALog)l).SL_IsEstimate == isEstimate);
		}
	}
}
