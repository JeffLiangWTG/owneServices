using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class AUAirCargoReportCommandTest : BaseHVLVRelatedJobCommandTest
	{
		protected override string ExpectedRelatedJobName => "HVLV AirCargo Report";

		protected override string[] ExpectedApplicableLoginCountries => new[] { CountryCodes.Australia };

		protected override string[] ExpectedShipmentTransportModes => new[] { TransportModes.Air };

		protected override Directions[] ExpectedShipmentDirections => new[] { Directions.Import };

		protected override Type ExpectedRelatedJobConverterType => typeof(AirManifestConverter);

		protected override bool ExpectedShouldValidateWaybill => false;

		protected override bool ExpectedShouldTrackPrimaryFieldChanges => true;

		protected override string ExpectedUsageCode => "ACR";

		protected override string ExpectedUsageCategory => "LVD";

		protected override bool ExpectedNeedPreScreening => true;

		protected override Type ExpectedCustomsJobsType => typeof(CusMAWB);

		protected override BaseHVLVRelatedJobCommand GetCommandForTest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			HVLVConsignmentHeader.GetOrCreate(shipment);
			return new AUAirCargoReportCommand(shipment);
		}
	}
}
