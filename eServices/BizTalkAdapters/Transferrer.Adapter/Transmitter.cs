using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Core;
using Common.Logging;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter
{
	[ExcludeFromCodeCoverage]
	public abstract class Transmitter : AsyncTransmitter
	{
		internal AdapterHandler Handler { get; private set; }

		protected Transmitter(string name, string version, string description, string transportType, Guid clsid, string propertyNamespace, Type endpointType, int maxBatchSize)
			: this(name, version, description, transportType, clsid, propertyNamespace, endpointType, maxBatchSize, Logging.CreateLogger)
		{ }

		internal Transmitter(string name, string version, string description, string transportType, Guid clsid, string propertyNamespace, Type endpointType, int maxBatchSize, 
								 Func<string, XmlDocument, ILog> loggerFactory)
			: base(name, version, description, transportType, clsid, propertyNamespace, endpointType, maxBatchSize)
		{
			Handler = new AdapterHandler(AdapterHandler.Direction.Send, transportType, loggerFactory);
		}

		protected override void HandlerPropertyBagLoaded()
		{
			Handler.Configure(HandlerPropertyBag);
		}

		public override void Terminate()
		{
			Handler.Terminate();
			base.Terminate();
		}
	}
}
