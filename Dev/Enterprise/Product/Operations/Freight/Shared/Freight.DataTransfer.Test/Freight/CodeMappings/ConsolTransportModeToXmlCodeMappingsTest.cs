using System;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Testing
{
	[TestedType(typeof(ConsolTransportModeToXmlCodeMappings))]
	sealed class ConsolTransportModeToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(Core.Constants.TransportModes) }; }
		}

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst()
		{
			return new[]
			{
				Core.Constants.TransportModes.AirSea,
				Core.Constants.TransportModes.SeaAir,
				Core.Constants.TransportModes.Mail,
				Core.Constants.TransportModes.Courier,
				Core.Constants.TransportModes.Other,
				Core.Constants.TransportModes.All,
				Core.Constants.TransportModes.Storage,
				Core.Constants.TransportModes.WarehouseHandling,
				Core.Constants.TransportModes.BorderWaterBorne,
				Core.Constants.TransportModes.Truck,
				Core.Constants.TransportModes.Auto,
				Core.Constants.TransportModes.Pedestrian,
				Core.Constants.TransportModes.PassengerHandCarried,
				Core.Constants.TransportModes.FixedTransportInstallations,
				Core.Constants.TransportModes.OwnPropulsion,
				Core.Constants.TransportModes.InlandWaterwayTransport
			};
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return true; }
		}
	}
}
