using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(PackLocation))]
	sealed class PackLocationTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestPackLine()
		{
			AssertEquals(Line, Location1.PackLine);
		}

		public void TestValidateJQ_NoPackages()
		{
			Location1.JQ_NoPackages = -2;
			Assert("No of packages is negative, expecting errors.", Location1.JQ_NoPackagesInfo.HasErrors());

			Location1.JQ_NoPackages = 4;
			Assert("No of packages is not negative, not expecting errors.", !Location1.JQ_NoPackagesInfo.HasErrors());
		}

		#region TestILocationConsumer

		public void TestILocationConsumer()
		{
			var factory = new BusinessObjectFactory();
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(factory);
			var whs = (IWhsWarehouse)helper.CreateWarehouse("WH1");
			var row = helper.CreateRowAndGenerateLocations(whs, "A", 2, 2);
			factory.Save();

			var packLocation = factory.NewWithValidTestData<PackLocation>();
			var locationPK = ((IWhsLocation)row.Locations[0]).PK;
			packLocation.JQ_WL = locationPK;
			var locationConsumer = (ILocationConsumer)packLocation;

			AssertEquals(locationConsumer.LocationTypeForMessages, "");
			AssertEquals(locationConsumer.LocationPK, locationPK);

			locationConsumer.LocationTitle = "Test";
			AssertEquals(locationConsumer.LocationTitle, "Test");
		}

		#endregion

		public void TestLocationWhsGuidInfo()
		{
			AssertNotNull(Location1.LocationWhsGuidInfo);
			AssertEquals("LocationWhsGuidInfo name", PackLocation.Schema.LocationWhsGuid, Location1.LocationWhsGuidInfo.Name);
		}

		public void TestJQ_WL()
		{
			var factory = new BusinessObjectFactory();
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(factory);
			var whs = (IWhsWarehouse)helper.CreateWarehouse("1");
			var row = helper.CreateRowAndGenerateLocations(whs, "A", 4, 3, 2);
			var whsLoc = (IWhsLocation)row.Locations[23];
			factory.Save();

			var packLine = factory.New<PackLine>();
			var packLocation = packLine.PackLocations.AddNew();
			packLocation.JQ_WL = whsLoc.PK;

			AssertEquals("Location1.LocationWhsGuid", whs.PK, packLocation.LocationWhsGuid);
			AssertEquals("Location1.LocationString", "A-4-3-2", packLocation.LocationString);
		}

		public void TestLocationString()
		{
			WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var factory = new BusinessObjectFactory();
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(factory);
			var whs = (IWhsWarehouse)helper.CreateWarehouse("1");
			var row = helper.CreateRowAndGenerateLocations(whs, "A", 4, 3, 2);
			var whsLoc = row.Locations[23];
			factory.Save();

			var packLine = factory.New<PackLine>();
			var packLocation = packLine.PackLocations.AddNew();

			packLocation.LocationWhsGuid = whs.PK;
			packLocation.LocationString = "A-1-1-1";
			AssertEquals("LocationString", "A-1-1-1", packLocation.LocationString);
		}

		#region TestLocationValidation

		public void TestLocationValidation()
		{
			WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var factory = new BusinessObjectFactory();
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(factory);
			var whs = (IWhsWarehouse)helper.CreateWarehouse("1");
			var row = helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			factory.Save();

			var shipment = factory.NewWithValidTestData<CommonShipment>();
			var line = shipment.OuterPackLines.AddNew();
			line.JL_JS = shipment.PK;
			var packLocation = line.PackLocations.AddNew();
			packLocation.LocationWhsGuid = whs.PK;

			// Assign location string which do not exists
			packLocation.LocationString = "A-1";
			AssertHasErrors(packLocation.LocationStringInfo);

			// Assign location string which does exists
			packLocation.LocationString = "A";
			AssertNoErrors(packLocation.LocationStringInfo);
			factory.Save();

			row.WR_Columns = 2;
			factory.Save();

			// Reset the locationstring
			packLocation.LocationString = "A-1";
			AssertNoErrors(packLocation.LocationStringInfo);
		}

		#endregion

		public void TestLocationStringInfo()
		{
			AssertNotNull(Location1.LocationStringInfo);
			AssertEquals("LocationStringInfo name", PackLocation.Schema.LocationString, Location1.LocationStringInfo.Name);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var shipment = factory.NewWithValidTestData<CommonShipment>();

			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("Dbhit001");
			helper.CreateRowAndGenerateLocations(warehouse, "A", 1, 1);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JS = shipment.PK;

			var packLocation = packLine.PackLocations.AddNew();
			packLocation.LocationWhsGuid = warehouse.PK;

			return packLocation;
		}

		protected override void SetUp()
		{
			base.SetUp();

			Line = Factory.New<PackLine>();
			Location1 = Line.PackLocations.AddNew();
			_ = Line.PackLocations.AddNew();
		}

		PackLocation Location1;
		PackLine Line;

		#endregion
	}
}
