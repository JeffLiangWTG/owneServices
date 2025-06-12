using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Management;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter
{
	public abstract class Receiver : Microsoft.Samples.BizTalk.Adapter.Common.Receiver
	{
		internal static ConcurrentDictionary<string, Receiver> Receivers { get; } = new ConcurrentDictionary<string, Receiver>();
		public AdapterHandler Handler { get; }
		internal ConcurrentDictionary<string, string> ReceiveLocationNames { get; } = new ConcurrentDictionary<string, string>();

		protected Receiver(string name, string version, string description, string transportType, Guid clsid, string propertyNamespace, Type endpointType)
			: this(name, version, description, transportType, clsid, propertyNamespace, endpointType, LoggerFactory.Instance, false)
		{ }

		protected Receiver(string name, string version, string description, string transportType, Guid clsid, string propertyNamespace, Type endpointType, bool skipLocationsLoad)
			: this(name, version, description, transportType, clsid, propertyNamespace, endpointType, LoggerFactory.Instance, skipLocationsLoad)
		{ }

		private Receiver(string name, string version, string description, string transportType, Guid clsid, string propertyNamespace, Type endpointType, ILoggerFactory loggerFactory, bool skipLocationsLoad)
			: base(name, version, description, transportType, clsid, propertyNamespace, endpointType)
		{
			Handler = new AdapterHandler(AdapterHandler.Direction.Receive, transportType, loggerFactory);
			if (Receivers.TryAdd(transportType, this) && !skipLocationsLoad)
			{
				ReceiveLocationNames = new ConcurrentDictionary<string, string>(GetReceiveLocationNames(transportType));
			}
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

		[ExcludeFromCodeCoverage]
		private IEnumerable<KeyValuePair<string, string>> GetReceiveLocationNames(string transportType)
		{
			using (var wmiSearcher = new ManagementObjectSearcher())
			{
				wmiSearcher.Scope = new ManagementScope("root\\MicrosoftBizTalkServer");
				wmiSearcher.Query = new SelectQuery
				{
					QueryString =
						$"SELECT InboundTransportURL, Name FROM MSBTS_ReceiveLocation WHERE AdapterName = '{transportType}'"
				};
				using (var results = wmiSearcher.Get())
				{
					foreach (var item in results)
					{
						yield return new KeyValuePair<string, string>(
							item.Properties["InboundTransportURL"].Value.ToString(),
							item.Properties["Name"].Value.ToString());
					}
				}
			}
		}
	}
}
