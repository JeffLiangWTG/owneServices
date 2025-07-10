using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class ModuleGridCollectionSyncronisationManagerTest : WhsTestCaseWithFactoryEnv
	{
		public void TestConstructor()
		{
			var validMock = new Mock<IModuleGridCollectionRefreshable>();
			validMock.Setup(m => m.GetTableNamesToMonitor()).Returns(new[] { WhsWarehouseSchema.Constants.TableName });
			AssertExceptionThrown(typeof(ArgumentNullException), () =>
			{
				var syncManager = new ModuleGridCollectionSynchronisationManager(null, validMock.Object);
			});

			AssertExceptionThrown(typeof(ArgumentNullException), () =>
			{
				var syncManager = new ModuleGridCollectionSynchronisationManager(new WhsPickFaceViewCollection(Factory), null);
			});

			var invalidMock = new Mock<IModuleGridCollectionRefreshable>();
			AssertExceptionThrown(typeof(InvalidOperationException), "No tables to monitor were provided.", () =>
			{
				var syncManager = new ModuleGridCollectionSynchronisationManager(new WhsPickFaceViewCollection(Factory), invalidMock.Object);
			});

			invalidMock.Setup(m => m.GetTableNamesToMonitor()).Returns(Array.Empty<string>());
			AssertExceptionThrown(typeof(InvalidOperationException), "No tables to monitor were provided.", () =>
			{
				var syncManager = new ModuleGridCollectionSynchronisationManager(new WhsPickFaceViewCollection(Factory), invalidMock.Object);
			});

			AssertNoExceptionThrown(() =>
			{
				var syncManager = new ModuleGridCollectionSynchronisationManager(new WhsPickFaceViewCollection(Factory), validMock.Object);
			});
		}

		public void TestPickFaceChangeInOtherFactoryWillRefreshPickFaceViewCollection()
		{
			var whs = Helper.CreateWarehouse("1");
			var row1 = Helper.CreateRow(whs, "R1");
			var row2 = Helper.CreateRow(whs, "R2");
			Factory.Save();

			var location1 = row1.Locations[0];
			var location2 = row2.Locations[0];

			var locationTypePFC = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "PFC"))
				// this can be removed when the system Location Type PFC is checked in
				?? Helper.CreateLocationType("PFC", "PFC Test", true, 1, LocationClasses.Codes.FIX);

			location1.WLV_WLT_LocationType = locationTypePFC.PK;
			location2.WLV_WLT_LocationType = locationTypePFC.PK;

			var client = Helper.CreateClient();
			var part1 = Helper.CreateProduct(client, "P1");
			var part2 = Helper.CreateProduct(client, "P2");

			Factory.Save();

			var pickFace = Helper.CreateProductPickFace(part1, client, location1);
			Factory.Save();

			var mockPickFaceRefreshable = new Mock<IModuleGridCollectionRefreshable>();
			mockPickFaceRefreshable.Setup(m => m.GetTableNamesToMonitor()).Returns(new[] { WhsPickFaceSchema.Constants.TableName });
			var pickFaceViewCollection = new WhsPickFaceViewCollection(Factory, mockPickFaceRefreshable.Object);
			mockPickFaceRefreshable.Verify(r => r.RefreshDisplay(), Times.Never);
			AssertNotNull("Pre-condition: PickFaceViewCollection is not null.", pickFaceViewCollection);
			AssertEquals("Pre-condition: PickFaceViewCollection should have 2 elements.", 2, pickFaceViewCollection.Count);

			var pickFaceViewLinksToLocation1 = pickFaceViewCollection.Cast<WhsPickFaceView>().Single(v => v.WPV_WL == location1.PK);
			var pickFaceViewLinksToLocation2 = pickFaceViewCollection.Cast<WhsPickFaceView>().Single(v => v.WPV_WL == location2.PK);
			AssertEquals("Pre-condition: PickFaceView has a Product allocated.", part1.PK, pickFaceViewLinksToLocation1.WPV_OP);
			AssertEquals("Pre-condition: PickFaceView without a Product allocated.", ZGuid.Empty, pickFaceViewLinksToLocation2.WPV_OP);

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = true };
			var helperWithOtherFactory = new WhsTestHelperFunctionsEnv(otherFactory);
			var newPickFaceInOtherFactory = helperWithOtherFactory.CreateProductPickFace(part2, client, location2);
			otherFactory.Save();

			mockPickFaceRefreshable.Verify(r => r.RefreshDisplay(), Times.Once);
		}

		public void TestDynamicPickFaceChangeInOtherFactoryWillRefreshPickFaceViewCollection()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var dynamicPFArea = Helper.CreateDynamicPF(data.Whs1, locationA1);
			var dynamicPFArea2 = Helper.CreateDynamicPF(data.Whs1, locationA1, "DYNAMIC2");
			Factory.Save();

			var mockRefreshable = new Mock<IModuleGridCollectionRefreshable>();
			mockRefreshable.Setup(m => m.GetTableNamesToMonitor()).Returns(new[] { WhsProductParamsByWhsAndClientSchema.Constants.TableName });
			var dynamicPickFaceViewCollection = new WhsDynamicPickFaceViewCollection(Factory, mockRefreshable.Object);

			Factory.Save();

			mockRefreshable.Verify(refreshable => refreshable.RefreshDisplay(), Times.Exactly(0), "Pre-condition: RefreshDisplay() not called");
			AssertNotNull("Pre-condition: dynamicPickFaceViewCollection is not null.", dynamicPickFaceViewCollection);
			AssertEquals("Pre-condition: dynamicPickFaceViewCollection should have A1", 1, dynamicPickFaceViewCollection.Count);

			// Add
			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = true };
			var w3 = AssignDynamicProductAndSave(otherFactory, data.Org1, data.Part1, dynamicPFArea);
			otherFactory.Save();
			mockRefreshable.Verify(refreshable => refreshable.RefreshDisplay(), Times.Exactly(1), "RefreshDisplay() should be called once");

			// Modify
			w3.W3_WA_DynamicPickFaceArea = dynamicPFArea2.PK;
			otherFactory.Save();
			mockRefreshable.Verify(refreshable => refreshable.RefreshDisplay(), Times.Exactly(2), "RefreshDisplay() should be called twice");

			// Delete
			((BusinessObject)w3).Delete();
			otherFactory.Save();
			mockRefreshable.Verify(refreshable => refreshable.RefreshDisplay(), Times.Exactly(3), "RefreshDisplay() should be called thrice");
		}

		IWhsProductParamsByWhsAndClient AssignDynamicProductAndSave(BusinessObjectFactory factory, OrgHeader client, OrgSupplierPart part, WhsArea area)
		{
			var w3 = factory.New<IWhsProductParamsByWhsAndClient>();
			w3.W3_OH = client.PK;
			w3.W3_OP = part.PK;
			w3.W3_WA_DynamicPickFaceArea = area.PK;
			w3.W3_WW = area.Warehouse.PK;
			factory.Save();
			return w3;
		}
	}
}
