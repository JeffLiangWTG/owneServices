using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.WSHttpEx
{
	public class WSHttpExTransmitAdapterBatch : AsyncTransmitterBatch
	{
		public WSHttpExTransmitAdapterBatch(int maxBatchSize, string propertyNamespace, IBTTransportProxy transportProxy, 
					AsyncTransmitter asyncTransmitter) :
			base(maxBatchSize, typeof(WSHttpExTransmitterEndpoint), propertyNamespace, null, transportProxy, asyncTransmitter)
		{
		}
	}
}
