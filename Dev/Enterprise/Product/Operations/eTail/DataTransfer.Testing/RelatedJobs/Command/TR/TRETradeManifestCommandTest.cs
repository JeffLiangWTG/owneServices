using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.ASYCUDA;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class TRETradeManifestCommandTest : BaseHVLVRelatedJobCommandTest
	{
		protected override string ExpectedRelatedJobName => "TR E-Trade Manifest";

		protected override Type ExpectedRelatedJobConverterType => typeof(TRETradeManifestConverter);

		protected override bool ExpectedShouldValidateWaybill => false;

		protected override bool ExpectedShouldTrackPrimaryFieldChanges => false;

		protected override bool ExpectedNeedPreScreening => true;

		protected override string[] ExpectedApplicableLoginCountries => new[] { CountryCodes.Turkey };

		protected override string[] ExpectedShipmentTransportModes => new[] { TransportModes.Air, TransportModes.Sea };

		protected override Directions[] ExpectedShipmentDirections => new[] { Directions.Import, Directions.Export };

		protected override string ExpectedUsageCode => "TET";

		protected override string ExpectedUsageCategory => "LVD";

		protected override Type ExpectedCustomsJobsType => ObjectFactory.GetType<TRETrade.IAsycudaManifestHeader>();

		protected override BaseHVLVRelatedJobCommand GetCommandForTest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			HVLVConsignmentHeader.GetOrCreate(shipment);
			return new TRETradeManifestCommand(shipment);
		}

		public void TestShouldCreateETradeManifest()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Turkey))
			using (ObjectFactory.Get<Enterprise.Integration.Customs.TR.ITRCustomsDataRegistry>().ExposeETradeModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = CreateShipmentForTurkeyETradeManifest();

				var createdCollection = CommandTestHelper.RunCreateCommand(new TRETradeManifestCommand(shipment), out var error);
				AssertEquals(error, 1, createdCollection.Count);

				var foundHeader = new BusinessObjectFactory().LoadTop1<IAsycudaManifestHeader>(new ZQuery());
				AssertNotNull(foundHeader);
			}
		}

		public void TestShouldOpenExistingETradeManifest()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Turkey))
			using (ObjectFactory.Get<Enterprise.Integration.Customs.TR.ITRCustomsDataRegistry>().ExposeETradeModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = CreateShipmentForTurkeyETradeManifest();
				CommandTestHelper.RunCreateCommand(new TRETradeManifestCommand(shipment), out _);
				var reloadedShipment = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);
				var loadedCollection = CommandTestHelper.RunOpenCommand(new TRETradeManifestCommand(reloadedShipment), out var error);
				AssertEquals($"expected {nameof(loadedCollection)} to contain manifest, error: {error}", 1, loadedCollection.Count);
			}
		}

		public void TestShouldSyncExistingETradeManifest()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Turkey))
			using (ObjectFactory.Get<Enterprise.Integration.Customs.TR.ITRCustomsDataRegistry>().ExposeETradeModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = CreateShipmentForTurkeyETradeManifest();
				CommandTestHelper.RunCreateCommand(new TRETradeManifestCommand(shipment), out _);
				var reloadedShipment = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);
				var loadedCollection = CommandTestHelper.RunSyncCommand(new TRETradeManifestCommand(reloadedShipment), out var error);
				AssertEquals($"expected {nameof(loadedCollection)} to contain manifest, error: {error}", 1, loadedCollection.Count);
			}
		}

		ForwardingShipment CreateShipmentForTurkeyETradeManifest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKDestination = "TRIST";
			shipment.JS_RL_NKOrigin = "AUSYD";
			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			shipment.Consols.AddNew();
			var consignment1 = header.Consignments.AddNew();
			consignment1.Items.AddNew();
			var consignment2 = header.Consignments.AddNew();
			consignment2.Items.AddNew();
			Factory.Save();
			return shipment;
		}
	}
}
