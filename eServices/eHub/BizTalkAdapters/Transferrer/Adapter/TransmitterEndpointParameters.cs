using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter
{
	public class TransmitterEndpointParameters : EndpointParameters
	{
		public TransmitterEndpointParameters(string uri, string portName) : base(portName)
		{
			Uri = uri;
			PortName = portName;
			sessionKey = $"{PortName}:{Uri}";
		}

		public string Uri { get; }
		public string PortName { get; }

		private string sessionKey;
		public override string SessionKey => sessionKey;

	}
}
