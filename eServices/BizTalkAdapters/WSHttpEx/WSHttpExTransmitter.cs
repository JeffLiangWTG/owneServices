using CargoWise.eHub.BizTalkAdapters.Common;
using Common.Logging;
using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;
using System;
using System.Diagnostics;

namespace CargoWise.eHub.BizTalkAdapters.WSHttpEx
{
	sealed public class WSHttpExTransmitter : AsyncTransmitter
	{
		static string wsHttpExNamespace = "http://cargowise.com/ehub/biztalkadapters/wshttpex-properties";
		static string transportType = "WSHttpEx";

		internal readonly ILog logger;
		readonly string hostInstance;

		public WSHttpExTransmitter()
			: base(
			String.Format("{0} Transmitter", transportType),
			"1.0",
			"Custom WSHttp adapter",
			transportType,
			new Guid("B2B752CC-6EBA-4524-8158-C4D7680CCBD4"),
			wsHttpExNamespace,
			typeof(WSHttpExTransmitterEndpoint),
			10)
		{
			hostInstance = Process.GetCurrentProcess().ProcessName.StartsWith("BTSNTSvc") ? Environment.GetCommandLineArgs()[4] : "[Unknown]";
			logger = TransferrerHelpers.CreateAdapterLogger(this, transportType, hostInstance);
			TransferrerHelpers.Log(this, logger, LogLevel.Debug, "Creating transmitter of type '{0}' for adapter '{1}' in host instance '{2}'. Hash Code = {3}", GetType().Name, transportType, hostInstance, GetHashCode());
		}

		protected override IBTTransmitterBatch CreateAsyncTransmitterBatch()
		{
			return new WSHttpExTransmitAdapterBatch(10, wsHttpExNamespace, TransportProxy, this);
		}
	}
}
