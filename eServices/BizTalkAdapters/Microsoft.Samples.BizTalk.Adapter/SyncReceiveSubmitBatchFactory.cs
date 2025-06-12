using Microsoft.BizTalk.TransportProxy.Interop;

namespace Microsoft.Samples.BizTalk.Adapter.Common
{
	public class SyncReceiveSubmitBatchFactory : ISyncReceiveSubmitBatchFactory
	{
		public ISyncReceiveSubmitBatch CreateBatch(IBTTransportProxy transportProxy, ControlledTermination control, int depth)
		{
			return new SyncReceiveSubmitBatch(transportProxy, control, 1);
		}
	}
}
