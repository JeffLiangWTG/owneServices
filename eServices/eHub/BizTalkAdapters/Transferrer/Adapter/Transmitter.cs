using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter
{
	[ExcludeFromCodeCoverage]
	public abstract class Transmitter : AsyncTransmitter
	{
		internal AdapterHandler Handler { get; }

		protected Transmitter(string name, string version, string description, string transportType, Guid clsid, string propertyNamespace, Type endpointType, int maxBatchSize)
			: this(name, version, description, transportType, clsid, propertyNamespace, endpointType, maxBatchSize, LoggerFactory.Instance)
		{ }

		protected Transmitter(string name, string version, string description, string transportType, Guid clsid, string propertyNamespace, Type endpointType, int maxBatchSize,
								 ILoggerFactory loggerFactory)
			: base(name, version, description, transportType, clsid, propertyNamespace, endpointType, maxBatchSize)
		{
			Handler = new AdapterHandler(AdapterHandler.Direction.Send, transportType, loggerFactory);
		}

		protected override void HandlerPropertyBagLoaded()
		{
			Handler.Configure(HandlerPropertyBag);
		}

		protected override EndpointParameters CreateEndpointParameters(IBaseMessage message)
		{
			var uri = (string)message.Context.Read("OutboundTransportLocation", "http://schemas.microsoft.com/BizTalk/2003/system-properties");
			var portName = (string)message.Context.Read("SPName", "http://schemas.microsoft.com/BizTalk/2003/system-properties");

			return new TransmitterEndpointParameters(uri, portName);
		}

		public override void Terminate()
		{
			Handler.Terminate();
			base.Terminate();
		}
	}
}
