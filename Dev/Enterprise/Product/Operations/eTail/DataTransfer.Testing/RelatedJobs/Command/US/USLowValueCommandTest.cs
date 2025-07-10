using System;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class USLowValueCommandTest : BaseHVLVRelatedJobCommandTest
	{
		protected override string ExpectedRelatedJobName => "Low Value Entries";

		protected override string[] ExpectedApplicableLoginCountries => new[] { CountryCodes.UnitedStates };

		protected override string ExpectedShipmentDestinationCountry => CountryCodes.UnitedStates;

		protected override Type ExpectedRelatedJobConverterType => typeof(USLowValueConverter);

		protected override bool ExpectedShouldValidateWaybill => true;

		protected override bool ExpectedShouldTrackPrimaryFieldChanges => false;

		protected override bool ExpectedNeedPreScreening => true;

		protected override string ExpectedUsageCode => "USC";

		protected override string ExpectedUsageCategory => "LVD";

		protected override Type ExpectedCustomsJobsType => typeof(CusUSLVClearance);

		protected override BaseHVLVRelatedJobCommand GetCommandForTest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			HVLVConsignmentHeader.GetOrCreate(shipment);
			return new USLowValueCommand(shipment);
		}
	}
}
