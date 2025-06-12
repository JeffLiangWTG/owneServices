using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;
using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Loopback
{
	class LoopbackTransmitterEndpoint : AsyncTransmitterEndpoint
	{
		IBTTransportProxy transportProxy;
		IBaseMessageFactory messageFactory;
		string propertyNamespace;
		string uri;

		public LoopbackTransmitterEndpoint(AsyncTransmitter asyncTransmitter)
			: base(asyncTransmitter)
		{
			this.transportProxy = asyncTransmitter.TransportProxy;
			this.messageFactory = this.transportProxy.GetMessageFactory();
		}

		public override void Open(EndpointParameters endpointParameters, IPropertyBag handlerPropertyBag, string propertyNamespace)
		{
			this.propertyNamespace = propertyNamespace;
			this.uri = endpointParameters.OutboundLocation;
		}

		public override IBaseMessage ProcessMessage(IBaseMessage message)
		{
			var messageCopy = this.messageFactory.CreateMessage();
			messageCopy.AddPart("Body", this.messageFactory.CreateMessagePart(), true);
			messageCopy.BodyPart.Data = new ReadOnlySeekableStream(message.BodyPart.GetOriginalDataStream(), new VirtualStream());
			messageCopy.BodyPart.ContentType = message.BodyPart.ContentType;
			messageCopy.BodyPart.Charset = message.BodyPart.Charset;
			for (int i = 0; i < message.Context.CountProperties; i++)
			{
				string name, ns;
				object value = message.Context.ReadAt(i, out name, out ns);
				if (ns != "http://schemas.microsoft.com/BizTalk/2003/system-properties" && ns != "http://schemas.microsoft.com/BizTalk/2003/messageagent-properties" )
				{
					if (message.Context.IsPromoted(name, ns))
						messageCopy.Context.Promote(name, ns, value);
					else
						messageCopy.Context.Write(name, ns, value);
				}
			}
			return messageCopy;
		}
	}
}
