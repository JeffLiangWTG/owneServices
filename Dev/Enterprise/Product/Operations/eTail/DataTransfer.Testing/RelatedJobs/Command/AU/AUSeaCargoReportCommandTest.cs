using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class AUSeaCargoReportCommandTest : BaseHVLVRelatedJobCommandTest
	{
		protected override string ExpectedRelatedJobName => "HVLV SeaCargo Report";

		protected override string[] ExpectedApplicableLoginCountries => new[] { CountryCodes.Australia };

		protected override string[] ExpectedShipmentTransportModes => new[] { TransportModes.Sea };

		protected override Directions[] ExpectedShipmentDirections => new[] { Directions.Import };

		protected override Type ExpectedRelatedJobConverterType => typeof(SeaOceanBillConverter);

		protected override bool ExpectedShouldValidateWaybill => false;

		protected override bool ExpectedShouldTrackPrimaryFieldChanges => true;

		protected override bool ExpectedNeedPreScreening => true;

		protected override string ExpectedUsageCode => "SCR";

		protected override string ExpectedUsageCategory => "LVD";

		protected override Type ExpectedCustomsJobsType => typeof(CusSCAOceanBill);

		protected override BaseHVLVRelatedJobCommand GetCommandForTest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			HVLVConsignmentHeader.GetOrCreate(shipment);
			return new AUSeaCargoReportCommand(shipment);
		}
	}
}
