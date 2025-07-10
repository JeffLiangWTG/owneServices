using System;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(OrderTransportModeToXmlCodeMappings))]
	public class OrderTransportModeToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(Core.Constants.TransportModes) }; }
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return true; }
		}

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst()
		{
			return new[]
			{
				Core.Constants.TransportModes.All,
				Core.Constants.TransportModes.AirSea,
				Core.Constants.TransportModes.SeaAir,
				Core.Constants.TransportModes.Other,
				Core.Constants.TransportModes.Storage,
				Core.Constants.TransportModes.WarehouseHandling,
				Core.Constants.TransportModes.BorderWaterBorne,
				Core.Constants.TransportModes.Truck,
				Core.Constants.TransportModes.Auto,
				Core.Constants.TransportModes.Pedestrian,
				Core.Constants.TransportModes.PassengerHandCarried,
				Core.Constants.TransportModes.FixedTransportInstallations,
				Core.Constants.TransportModes.InlandWaterwayTransport,
				Core.Constants.TransportModes.OwnPropulsion
			};
		}
	}
}
