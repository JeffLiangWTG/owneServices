using System;
using CargoWise.Application;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	sealed class SGCargoReportCommandTest : BaseHVLVRelatedJobCommandTest
	{
		public override void TestGetRelatedJobName()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Singapore))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "SGBKM";
				var command = new SGCargoReportCommand(shipment);
				AssertEquals("SG ACCESS Import Manifest", command.RelatedJobName);

				shipment.JS_RL_NKOrigin = "SGBKM";
				shipment.JS_RL_NKDestination = "NZAKL";
				AssertEquals("SG ACCESS Export Manifest", command.RelatedJobName);
			}
		}

		protected override string[] ExpectedApplicableLoginCountries => new[] { CountryCodes.Singapore };

		protected override string[] ExpectedShipmentTransportModes => new[] { TransportModes.Air, TransportModes.Road };

		protected override Type ExpectedRelatedJobConverterType => typeof(SGAccessManifestConverter);

		protected override bool ExpectedShouldValidateWaybill => false;

		protected override bool ExpectedShouldTrackPrimaryFieldChanges => false;

		protected override bool ExpectedNeedPreScreening => true;

		protected override string ExpectedUsageCode => "SGA";

		protected override string ExpectedUsageCategory => "LVD";

		protected override string ExpectedRegistryItemSetName => "SGCustomsDataRegistry";

		protected override string ExpectedRequiredRegistryItemName => "ACCESSEnable";

		protected override Type ExpectedCustomsJobsType =>
			ObjectFactory.GetType<Enterprise.Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();

		protected override BaseHVLVRelatedJobCommand GetCommandForTest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			HVLVConsignmentHeader.GetOrCreate(shipment);
			return new SGCargoReportCommand(shipment);
		}
	}
}
