using Microsoft.BizTalk.TransportProxy.Interop;

namespace Microsoft.Samples.BizTalk.Adapter.Common
{
	public interface ISyncReceiveSubmitBatchFactory
	{
		ISyncReceiveSubmitBatch CreateBatch(IBTTransportProxy transportProxy, ControlledTermination control, int depth);
	}
}
