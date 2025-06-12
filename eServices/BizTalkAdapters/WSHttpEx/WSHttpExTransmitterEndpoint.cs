using CargoWise.eHub.BizTalkAdapters.Common;
using Common.Logging;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;
using System.IO;

namespace CargoWise.eHub.BizTalkAdapters.WSHttpEx
{
	public class WSHttpExTransmitterEndpoint : AsyncTransmitterEndpoint
	{
		WSHttpExTransmitter transmitter;
		IHttpExTransferrerFactory transferrerFactory;

		string propertyNamespace;

		public WSHttpExTransmitterEndpoint(AsyncTransmitter transmitter)
			: this(transmitter, new WsHttpExTransferrerFactory())
		{
		}

		public WSHttpExTransmitterEndpoint(AsyncTransmitter transmitter, IHttpExTransferrerFactory transferrerFactory)
			: base(transmitter)
		{
			this.transmitter = (WSHttpExTransmitter)transmitter;
			this.transferrerFactory = transferrerFactory;

			TransferrerHelpers.Log(this, this.transmitter.logger, LogLevel.Debug, "Initialized transmitter endpoint of type '{0}'. Endpoint Hash Code = {1}.", GetType().Name, GetHashCode());
		}

		public override void Open(EndpointParameters endpointParameters, IPropertyBag handlerPropertyBag, string propertyNamespace)
		{
			this.propertyNamespace = propertyNamespace;
			TransferrerHelpers.Log(this, transmitter.logger, LogLevel.Debug, "Opened transmitter endpoint of type '{0}'. Endpoint Hash Code = {1}.", GetType().Name, GetHashCode());
		}

		public override IBaseMessage ProcessMessage(IBaseMessage message)
		{
			var transferrer = transferrerFactory.CreateTransferrer(message, propertyNamespace, transmitter.TransportProxy.GetMessageFactory());
			return transferrer.SendRequest(message);
		}
	}
}
