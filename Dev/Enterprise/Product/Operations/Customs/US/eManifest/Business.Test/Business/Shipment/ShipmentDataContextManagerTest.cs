using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(ShipmentDataContextManager))]
	sealed class ShipmentDataContextManagerTest : ShipmentDataContextManagerTestCase<ShipmentDataContextManager, Shipment>
	{
		public void TestManagesEvents()
		{
			var manager = new ShipmentDataContextManager();
			AssertEquals(true, manager.ManagesEvents);
		}

		public void TestDataContextType()
		{
			var trip = Factory.New<Trip>();
			trip.BH_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
			trip.BH_OA_Importer = Factory.NewWithValidTestData<OrgAddress>().PK;
			var shipment = trip.Shipments.AddNew();
			var eventManager = shipment.GetUniversalDataContextManager();
			AssertEquals(DataContextType.USeManifestShipment, eventManager.DataContextType);
		}

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("No Job Number support", true);
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => System.Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML => string.Empty;

		protected override Shipment GetNewBusinessObjectForTesting() => Factory.New<Shipment>();
	}
}
