
namespace Enterprise.eManifest.Business.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Freight.Business;
	using NUnit.Framework;

	[TestedType(typeof(SupplierBookingLinesManager))]
	internal class SuppierBookingLinesManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLines_SomeLinesAreNotInDatabase_LinesInDatabaseMarkAsReadonly()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var savedLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			savedLine.DL_JS_ApprovedShipment = shipment.PK;

			Factory.Save();

			var unsavedLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			unsavedLine.DL_JS_ApprovedShipment = shipment.PK;

			var linesManager = new SupplierBookingLinesManager(shipment);
			var lines = linesManager.Lines;
			Assert("Users should not be able to add a new line manually in the grid", !lines.AllowNew);

			AssertContainsExactElementsInAnyOrder("Loaded lines", new[] { savedLine, unsavedLine }, lines);
			AssertEquals("Is saved line readonly", true, savedLine.ReadOnly);
			AssertEquals("Is unsaved line readonly", false, unsavedLine.ReadOnly);

			shipment.JS_GoodsDescription = "McLaren";
			Factory.Save();
			AssertEquals("Is unsaved line readonly after save", true, unsavedLine.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			return new SupplierBookingLinesManager(shipment);
		}
	}
}
