using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Warehouse.Web.WebService.Testing.WhsSecureServiceTestCase;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	class WebServiceHelperTest : WhsTestCaseWithFactory
	{
		#region TestGetPackageByPackagePK

		public void TestGetPackageByPackagePK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			AssertNull(WebServiceHelper.GetPackageByPackagePK(order, Guid.Empty));

			var package = Factory.New<PkgPackage>();
			AssertNull(WebServiceHelper.GetPackageByPackagePK(order, package.PK.ToGuid()));

			order.PackageJob.Packages.Add(package);
			AssertEquals(package, WebServiceHelper.GetPackageByPackagePK(order, package.PK.ToGuid()));
		}

		#endregion

		#region TestGetPickPackParameter

		public void TestGetPickPackParameter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var salesChannel = Helper.CreateWhsSalesChannel("ECO", "eCommerce");
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_WSH_SalesChannel = salesChannel.PK;

			var param1 = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			param1.WPP_WSH_SalesChannel = salesChannel.PK;

			var param2 = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			AssertEquals(param1, WebServiceHelper.GetPickPackParameter(order));

			param1.WPP_IsPickAndPackEnabled = false;
			AssertNull(WebServiceHelper.GetPickPackParameter(order));
		}

		#endregion

		#region TestFindExistingLog

		public void TestFindExistingLog()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Factory.Save();

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			receive.Logs.AddNew(Events.EditedARecord, "RF", ZDateTimeOffset.Now);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			receiveLine.Logs.AddNew(Events.AddedARecordToTheSystem, "RF", ZDateTimeOffset.Now);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			AssertNotNull(WebServiceHelper.FindExistingLog(receive, Events.EditedARecord, "RF"));
			AssertNull(WebServiceHelper.FindExistingLog(receive, Events.AddedARecordToTheSystem, "RF"));
			AssertNull(WebServiceHelper.FindExistingLog(receiveLine, Events.EditedARecord, "RF"));
			AssertNotNull(WebServiceHelper.FindExistingLog(receiveLine, Events.AddedARecordToTheSystem, "RF"));
		}

		#endregion

		#region TestGetDockDoorLocations

		public void TestGetDockDoorLocations()
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 5, 1);
			var dockDoorLocationType = Helper.CreateLocationType("TS1", "Test1", false, 0, LocationClasses.Codes.DDL);
			var normalLocationType = Helper.CreateLocationType("TS2", "Test2", false, 0, LocationClasses.Codes.NOR);

			var locations = whs.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_WLT_LocationType = dockDoorLocationType.PK;
			locations[1].WLV_WLT_LocationType = normalLocationType.PK;
			locations[2].WLV_WLT_LocationType = dockDoorLocationType.PK;
			locations[3].WLV_WLT_LocationType = normalLocationType.PK;
			locations[4].WLV_WLT_LocationType = dockDoorLocationType.PK;

			Factory.Save();

			var dockDoorLocations = WebServiceHelper.GetDockDoorLocations(Factory, "WHS");

			AssertEquals(4, dockDoorLocations.Count());
			AssertContainsExactElementsInAnyOrder(new[] { whs.DefaultInboundDockDoorLocation, locations[0], locations[2], locations[4] }, dockDoorLocations);
		}

		#endregion

		#region GetOrgHeader

		public void TestGetOrgHeader()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var orgHeader1 = WebServiceHelper.GetOrgHeader(Factory, "");
			AssertNull(orgHeader1);

			var orgHeader2 = WebServiceHelper.GetOrgHeader(Factory, "FIHERFEFDSD");
			AssertNull(orgHeader2);

			var orgHeader3 = WebServiceHelper.GetOrgHeader(Factory, data.Org1.OH_Code);
			AssertNotNull(orgHeader3);
		}

		#endregion

		#region TestSaveFactoryWithExceptionHandling

		public void TestSaveFactoryWithExceptionHandling_WebServiceResponse_NoExceptionThrown()
		{
			var response = new WebServiceResponse();
			SaveFactoryWithExceptionHandlingNoConcurrencyThrown(Factory, response);
			AssertNullOrEmpty(response.ErrorMessage);
			AssertEquals(ErrorTypes.None, response.Error);
		}

		public void TestSaveFactoryWithExceptionHandling_WebServiceResponse_RandomExceptionThrown()
		{
			var response = new WebServiceResponse();
			Factory.Saving += f => throw new InvalidOperationException("Test");
			AssertExceptionThrown(typeof(InvalidOperationException), "Test", () => SaveFactoryWithExceptionHandlingNoConcurrencyThrown(Factory, response));
			AssertNullOrEmpty(response.ErrorMessage);
			AssertEquals(ErrorTypes.None, response.Error);
		}

		public void TestSaveFactoryWithExceptionHandling_WebServiceResponse_ZCannotSaveExceptionThrown()
		{
			var response = new WebServiceResponse();
			Factory.Saving += f => throw new ZCannotSaveException("Test", "Test");
			AssertNoExceptionThrown(() => SaveFactoryWithExceptionHandlingNoConcurrencyThrown(Factory, response));
			AssertEquals("Test", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestSaveFactoryWithExceptionHandling_WebServiceResponse_ZSaveConcurrencyExceptionThrown()
		{
			Factory.Saving += f => throw CreateZSaveConcurrencyException();

			var response1 = new WebServiceResponse();
			AssertNoExceptionThrown(() => WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response1, (ex) => ex.Message));
			AssertContains("Message from inner exception!", response1.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);

			var response2 = new WebServiceResponse();
			AssertNoExceptionThrown(() => WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response2, GetErrorMessageForConcurrencyException));
			AssertEquals("User will see a custom message!", response2.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
		}

		public void TestSaveFactoryWithExceptionHandling_Action_NoExceptionThrown()
		{
			var hitCount = 0;
			WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, ex => hitCount++);
			AssertEquals(0, hitCount);
		}

		public void TestSaveFactoryWithExceptionHandling_Action_RandomExceptionThrown()
		{
			var hitCount = 0;
			Factory.Saving += f => throw new InvalidOperationException("Test");
			AssertExceptionThrown(typeof(InvalidOperationException), "Test", () => WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, ex => hitCount++));
			AssertEquals(0, hitCount);
		}

		public void TestSaveFactoryWithExceptionHandling_Action_ZCannotSaveExceptionThrown()
		{
			var hitCount = 0;
			Factory.Saving += f => throw new ZCannotSaveException("Test", "Test");
			AssertNoExceptionThrown(() => WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, ex => hitCount++));
			AssertEquals(1, hitCount);
		}

		public void TestSaveFactoryWithExceptionHandling_Action_ZSaveConcurrencyExceptionThrown()
		{
			var hitCount = 0;
			Factory.Saving += f => throw CreateZSaveConcurrencyException();
			AssertNoExceptionThrown(() => WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, ex => hitCount++));
			AssertEquals(1, hitCount);
		}

		public void TestSaveFactoryWithExceptionHandling_Action_ZSaveException()
		{
			var hitCount = 0;
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			new ZSaveException(new DummyDataException(((IBusinessObjectInternals)dummy).Row, TestConnection), Factory);

			Factory.Saving += f => throw new InvalidOperationException("Test");
			AssertExceptionThrown(typeof(InvalidOperationException), "Test", () => WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, ex => hitCount++));
			AssertEquals(0, hitCount);
		}

		public void TestSaveFactoryWithExceptionHandling_Action_ZSaveException_IsInnermostLockTimeoutExpired()
		{
			var sqlException = SqlExceptionBuilder.CreateSqlException(
				SqlExceptionBuilder.CreateSqlErrorCollection(
				SqlExceptionBuilder.CreateSqlError(1222, byte.MaxValue, byte.MinValue, Core.Constants.ProductName,
				"Lock request time out period exceeded.", "", 1)
			));

			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Saving += f => throw new ZSaveException(new ZDataException(sqlException, ((IBusinessObjectInternals)dummy).Row, Db.Connection), Factory);

			var response = new WebServiceResponse();
			AssertNoExceptionThrown(() => WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (ex) => ex.Message));
			AssertEquals("The save operation timed out\r\nLock request time out period exceeded. Please try again.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		#endregion

		#region TestSaveFactoryWithExceptionHandling_GetErrorMessageForConcurrencyException

		public void TestSaveFactoryWithExceptionHandling_GetErrorMessageForConcurrencyException_InvalidArgument()
		{
			Factory.Saving += f => throw CreateZSaveConcurrencyException();

			AssertExceptionThrown<ArgumentException>(() => WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, new WebServiceResponse(), null));
			AssertNoExceptionThrown(() => WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, new WebServiceResponse(), (ex) => ex.Message));
		}

		public void TestSaveFactoryWithExceptionHandling_GetErrorMessageForConcurrencyException_ZSaveConcurrencyException()
		{
			Factory.Saving += f => throw CreateZSaveConcurrencyException();

			var response1 = new WebServiceResponse();
			AssertNoExceptionThrown(() => WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response1, (ex) => ex.Message));
			AssertContains("Message from inner exception!", response1.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);

			var response2 = new WebServiceResponse();
			AssertNoExceptionThrown(() => WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response2, GetErrorMessageForConcurrencyException));
			AssertEquals("User will see a custom message!", response2.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
		}

		public void TestSaveFactoryWithExceptionHandling_GetErrorMessageForConcurrencyException_ZCannotSaveException()
		{
			Factory.Saving += f => throw new ZCannotSaveException("Test", "Test");

			var response1 = new WebServiceResponse();
			AssertNoExceptionThrown(() => SaveFactoryWithExceptionHandlingNoConcurrencyThrown(Factory, response1));
			AssertEquals("Test", response1.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);

			var response2 = new WebServiceResponse();
			AssertNoExceptionThrown(() => WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response2, GetErrorMessageForConcurrencyException));
			AssertEquals("Test", response2.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
		}

		public void TestSaveFactoryWithExceptionHandling_GetErrorMessageForConcurrencyException_RandomExceptionThrown()
		{
			Factory.Saving += f => throw new ArgumentException("Test");

			var response1 = new WebServiceResponse();
			AssertExceptionThrown(typeof(ArgumentException), "Test", () => SaveFactoryWithExceptionHandlingNoConcurrencyThrown(Factory, response1));
			AssertNullOrEmpty(response1.ErrorMessage);
			AssertEquals(ErrorTypes.None, response1.Error);

			var response2 = new WebServiceResponse();
			AssertExceptionThrown(typeof(ArgumentException), "Test", () => SaveFactoryWithExceptionHandlingNoConcurrencyThrown(Factory, response2));
			AssertNullOrEmpty(response2.ErrorMessage);
			AssertEquals(ErrorTypes.None, response2.Error);
		}

		string GetErrorMessageForConcurrencyException(ZSaveConcurrencyException ex) => "User will see a custom message!";

		#endregion

		#region SaveFactoryWithExceptionHandlingNoConcurrencyThrown

		static void SaveFactoryWithExceptionHandlingNoConcurrencyThrown(BusinessObjectFactory factory, WebServiceResponse response)
		{
			WebServiceHelper.SaveFactoryWithExceptionHandling(factory, response, (ex) => throw new Exception("Should not call this!"));
		}

		#endregion

		#region CreateZSaveConcurrencyException

		ZSaveConcurrencyException CreateZSaveConcurrencyException()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var innerException = new Exception("Message from inner exception!");
			var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)dummy).Row, Db.Connection);
			return new ZSaveConcurrencyException(concurrencyException, Factory);
		}

		#endregion
	}
}
