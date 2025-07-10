using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;
using ShipmentTypes = Enterprise.Core.Constants.ShipmentTypes;
using TransportModes = Enterprise.Core.Constants.TransportModes;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class USRoadEManifestCommandTest : BaseHVLVRelatedJobCommandTest
	{
		public void TestShouldSyncUSRoadEManifest()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.Consols.AddNew();

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = header.Consignments.AddNew();
			consignment.Items.AddNew();

			Factory.Save();

			CommandTestHelper.RunCreateCommand(new USRoadEManifestCommand(shipment), out _);
			var reloadedShipment = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);

			var syncCommand = new USRoadEManifestCommand(reloadedShipment);
			Assert("Precondition: US e-Manifest can sync", syncCommand.CanSync);

			var loadedCollection = CommandTestHelper.RunSyncCommand(syncCommand, out var error);
			AssertEquals($"expected {nameof(loadedCollection)} to contain manifest, error: {error}", 1, loadedCollection.Count);
		}

		protected override string ExpectedRelatedJobName => "HVLV e-Manifest";

		protected override string[] ExpectedApplicableLoginCountries => new[] { CountryCodes.UnitedStates, CountryCodes.Canada };

		protected override string ExpectedShipmentDestinationCountry => CountryCodes.UnitedStates;

		protected override string[] ExpectedShipmentTransportModes => new[] { TransportModes.Road };

		protected override Type ExpectedRelatedJobConverterType => typeof(USeManifestConverter);

		protected override bool ExpectedShouldValidateWaybill => true;

		protected override bool ExpectedShouldTrackPrimaryFieldChanges => false;

		protected override bool ExpectedNeedPreScreening => false;

		protected override string ExpectedAllowLoginToDifferentCountryRegistry => HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.Name;

		protected override string ExpectedUsageCode => "USR";

		protected override string ExpectedUsageCategory => "LVD";

		protected override Type ExpectedCustomsJobsType => typeof(Trip);

		protected override BaseHVLVRelatedJobCommand GetCommandForTest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.GetOrCreateHVLVConsignmentHeader();
			return new USRoadEManifestCommand(shipment);
		}
	}
}
