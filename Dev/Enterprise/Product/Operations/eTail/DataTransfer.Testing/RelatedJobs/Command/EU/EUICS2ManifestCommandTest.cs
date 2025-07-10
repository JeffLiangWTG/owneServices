using System;
using CargoWise.Application;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class EUICS2ManifestCommandTest : BaseHVLVRelatedJobCommandTest
	{
		protected override string ExpectedRelatedJobName => "EU ICS2 Manifest";

		protected override Type ExpectedRelatedJobConverterType => typeof(EUICS2ManifestConverter);

		protected override string ExpectedShipmentDestinationCountry => CountryCodes.EuropeanUnion;

		protected override string[] ExpectedShipmentTransportModes => new[] { TransportModes.Air, TransportModes.Sea, TransportModes.Road };

		protected override bool ExpectedShouldValidateWaybill => false;

		protected override bool ExpectedShouldTrackPrimaryFieldChanges => false;

		protected override bool ExpectedNeedPreScreening => true;

		protected override string ExpectedUsageCode => "IC2";

		protected override string ExpectedUsageCategory => "SEC";

		protected override Type ExpectedCustomsJobsType => ObjectFactory.GetType<Enterprise.Integration.Customs.ASYCUDA.EUICS2.IAsycudaManifestHeader>();

		protected override BaseHVLVRelatedJobCommand GetCommandForTest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			HVLVConsignmentHeader.GetOrCreate(shipment);
			return new EUICS2ManifestCommand(shipment);
		}
	}
}
