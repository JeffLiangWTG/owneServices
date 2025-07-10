using System;
using System.Collections.Generic;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class TWForwarderManifestCommandTest : BaseHVLVRelatedJobCommandTest
	{
		public override void TestGetRelatedJobName()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "TWTPE";
				var command = new TWForwarderManifestCommand(shipment);
				AssertEquals("TW Forwarder Manifest (Import)", command.RelatedJobName);

				shipment.JS_RL_NKOrigin = "TWTPE";
				shipment.JS_RL_NKDestination = "NZAKL";
				AssertEquals("TW Forwarder Manifest (Export)", command.RelatedJobName);
			}
		}

		public void TestCreatingForwarderManifestWillNotAffectBriefCustomsDeclarationMenuItems()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var forwarderManifest = Factory.NewWithValidTestData<Customs.TW.Manifest.Business.AsycudaManifestHeader>();

			consignmentHeader.GenPivotCollection.AddRelatedIfNotExist(forwarderManifest);
			Factory.Save();

			var forwarderManifestCommand = new TWForwarderManifestCommand(shipment);
			CombineAssertions("Pre-condition: Forwarder Manifest existed so cannot create but can open and sync", () =>
			{
				Assert(!forwarderManifestCommand.CanCreate);
				Assert(forwarderManifestCommand.CanOpen);
				Assert(forwarderManifestCommand.CanSync);
			});

			var briefCustomsDeclarationCommand = new TWBriefCustomsDeclarationCommand(shipment);
			CombineAssertions("Brief Customs Declaration is not existed so can create but cannot open or sync", () =>
			{
				Assert(briefCustomsDeclarationCommand.CanCreate);
				Assert(!briefCustomsDeclarationCommand.CanOpen);
				Assert(!briefCustomsDeclarationCommand.CanSync);
			});
		}

		public void TestHasRelatedCustomsJobsIgnoringHLRLogs()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.Logs.AddNew(AutoEvents.HVLVReady, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReportCreated));
			shipment.GetOrCreateHVLVConsignmentHeader();

			var forwarderManifestCommand = new TWForwarderManifestCommand(shipment);
			CombineAssertions("Should ignore HLR log so still consider related customs job is not created. Thus can create but cannot open or sync", () =>
			{
				Assert(forwarderManifestCommand.CanCreate);
				Assert(!forwarderManifestCommand.CanOpen);
				Assert(!forwarderManifestCommand.CanSync);
			});
		}

		protected override string[] ExpectedApplicableLoginCountries => new[] { CountryCodes.Taiwan };

		protected override string[] ExpectedShipmentTransportModes => new[] { TransportModes.Air, TransportModes.Sea };

		protected override Directions[] ExpectedShipmentDirections => new[] { Directions.Import, Directions.Export };

		protected override Type ExpectedRelatedJobConverterType => typeof(TWForwarderManifestConverter);

		protected override bool ExpectedShouldValidateWaybill => false;

		protected override bool ExpectedShouldTrackPrimaryFieldChanges => false;

		protected override bool ExpectedNeedPreScreening => true;

		protected override string ExpectedUsageCode => "TWM";

		protected override string ExpectedUsageCategory => "SEC";

		protected override Type ExpectedCustomsJobsType => typeof(Customs.TW.Manifest.Business.AsycudaManifestHeader);

		protected override BaseHVLVRelatedJobCommand GetCommandForTest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			HVLVConsignmentHeader.GetOrCreate(shipment);
			return new TWForwarderManifestCommand(shipment);
		}
	}
}
