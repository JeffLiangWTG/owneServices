using System;
using System.Collections.Generic;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class TWBriefCustomsDeclarationCommandTest : BaseHVLVRelatedJobCommandTest
	{
		public override void TestGetRelatedJobName()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "TWTPE";
				var command = new TWBriefCustomsDeclarationCommand(shipment);
				AssertEquals("Taiwan Brief Customs Declaration Import", command.RelatedJobName);

				shipment.JS_RL_NKOrigin = "TWTPE";
				shipment.JS_RL_NKDestination = "NZAKL";
				AssertEquals("Taiwan Brief Customs Declaration Export", command.RelatedJobName);
			}
		}

		public void TestCreatingBriefCustomsDeclarationWillNotAffectForwarderManifestMenuItems()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var forwarderManifest = Factory.NewWithValidTestData<Customs.TW.BriefCustomsDeclaration.Business.AsycudaManifestHeader>();

			consignmentHeader.GenPivotCollection.AddRelatedIfNotExist(forwarderManifest);
			Factory.Save();

			var briefCustomsDeclarationCommand = new TWBriefCustomsDeclarationCommand(shipment);
			CombineAssertions("Pre-condition: Brief Customs Declaration existed so cannot create but can open and sync", () =>
			{
				Assert(!briefCustomsDeclarationCommand.CanCreate);
				Assert(briefCustomsDeclarationCommand.CanOpen);
				Assert(briefCustomsDeclarationCommand.CanSync);
			});

			var forwarderManifestCommand = new TWForwarderManifestCommand(shipment);
			CombineAssertions("Forwarder Manifest is not existed so can create but cannot open or sync", () =>
			{
				Assert(forwarderManifestCommand.CanCreate);
				Assert(!forwarderManifestCommand.CanOpen);
				Assert(!forwarderManifestCommand.CanSync);
			});
		}

		public void TestHasRelatedCustomsJobsIgnoringHLRLogs()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.Logs.AddNew(AutoEvents.HVLVReady, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReportCreated));
			shipment.GetOrCreateHVLVConsignmentHeader();

			var briefCustomsDeclarationCommand = new TWBriefCustomsDeclarationCommand(shipment);
			CombineAssertions("Should ignore HLR log so still consider related customs job is not created. Thus can create but cannot open or sync", () =>
			{
				Assert(briefCustomsDeclarationCommand.CanCreate);
				Assert(!briefCustomsDeclarationCommand.CanOpen);
				Assert(!briefCustomsDeclarationCommand.CanSync);
			});
		}

		protected override string[] ExpectedApplicableLoginCountries => new[] { CountryCodes.Taiwan };

		protected override string[] ExpectedShipmentTransportModes => new[] { TransportModes.Air, TransportModes.Sea };

		protected override Directions[] ExpectedShipmentDirections => new[] { Directions.Import, Directions.Export };

		protected override Type ExpectedRelatedJobConverterType => typeof(TWBriefCustomsDeclarationConverter);

		protected override bool ExpectedShouldValidateWaybill => false;

		protected override bool ExpectedShouldTrackPrimaryFieldChanges => false;

		protected override bool ExpectedNeedPreScreening => true;

		protected override string ExpectedUsageCode => "TCD";

		protected override string ExpectedUsageCategory => "LVD";

		protected override Type ExpectedCustomsJobsType => typeof(Customs.TW.BriefCustomsDeclaration.Business.AsycudaManifestHeader);

		protected override BaseHVLVRelatedJobCommand GetCommandForTest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			HVLVConsignmentHeader.GetOrCreate(shipment);
			return new TWBriefCustomsDeclarationCommand(shipment);
		}
	}
}
