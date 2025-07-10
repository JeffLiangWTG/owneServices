using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class CreateNewWhsReceiveTest : WhsSecureServiceTestCase
	{
		#region CreateNewWhsReceive

		[TestDate(2009, 1, 1)]
		public void TestCreateNewWhsReceive()
		{
			var webService = GetNewWebService();
			AssertEquals(0, webService.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_ExternalReference, "TESTREF")).Length);

			var data = new TestDataSimpleEnvironment(webService.Factory);
			var helper = new WhsTestHelperFunctions(GetNewWebService().Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory, "Attr1");
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");
			webService.Factory.Save();

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.AllowedToRunServiceHasBeenCalled = false;
			var response = webService.CreateNewWhsReceive("TESTREF", data.Org1.OH_Code, new DateTime(2008, 02, 12, 14, 30, 00, 00), ReceiveType.Codes.Receipt);
			var docket = response.Docket;
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(docket);
			AssertNotEquals(Guid.Empty, docket.PK);
			AssertNotNull(docket.PartAttributes);
			AssertEquals("Attr1", docket.PartAttributes.Attribute1Caption);
			AssertEquals(true, docket.PartAttributes.Attribute1IsMandatory);
			AssertEquals("Attr2", docket.PartAttributes.Attribute2Caption);
			AssertEquals(false, docket.PartAttributes.Attribute2IsMandatory);
			AssertEquals("", docket.PartAttributes.Attribute3Caption);
			AssertEquals(false, docket.PartAttributes.Attribute3IsMandatory);
			AssertEquals(Guid.Empty, docket.TaskPK);

			var receive = webService.Factory.Load<WhsReceive>(new ZGuid(docket.PK));
			AssertNotNull(receive);
			AssertEquals(docket.DocketID, receive.WD_DocketID);
			AssertNotNull(receive.Client);
			AssertEquals(data.Org1.OH_Code, receive.Client.OH_Code);
			AssertEquals("TESTREF", receive.WD_ExternalReference);
			AssertEquals(new ZDateTimeOffset(2008, 02, 12, 14, 30, 0), receive.WD_ArrivalDate);
			AssertEquals(true, receive.StartedReceiving);
			AssertEquals(ReceiveType.Codes.Receipt, receive.WD_DocketSubType);
		}

		#endregion

		#region TestCreateNewWhsReceive_CheckDockDoorLocations

		[TestDate(2009, 1, 1)]
		public void TestCreateNewWhsReceive_CheckDockDoorLocations_SingleDDL()
		{
			var webService = GetNewWebService();
			AssertEquals(0, webService.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_ExternalReference, "TESTREF")).Length);

			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.Factory.Save();

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.AllowedToRunServiceHasBeenCalled = false;
			var response = webService.CreateNewWhsReceive("TESTREF", data.Org1.OH_Code, new DateTime(2008, 02, 12, 14, 30, 00, 00), ReceiveType.Codes.Receipt);
			AssertSuccessfulResponse(response, webService);

			var expectedDDL = data.Whs1.DefaultInboundDockDoorLocation;
			AssertEquals("Response should return true for single DDL.", true, response.WarehouseHasSingleDockDoorLocation);
			AssertEquals("Response should retrieve appropriate DDL PK.", expectedDDL.PK, response.SingleDockDoorLocationPK);
			AssertEquals("Response should retrieve appropriate DDL string.", expectedDDL.WLV_LocationString, response.SingleDockDoorLocation);
		}

		[TestDate(2009, 1, 1)]
		public void TestCreateNewWhsReceive_CheckDockDoorLocations_MultipleDDL()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("TS1", "Test1", false, 0, LocationClasses.Codes.DDL);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[1].WLV_WLT_LocationType = dockDoorLocationType.PK;
			locations[2].WLV_WLT_LocationType = dockDoorLocationType.PK;
			Helper.Factory.Save();

			var webService = GetNewWebService();
			AssertEquals(0, webService.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_ExternalReference, "TESTREF")).Length);

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.AllowedToRunServiceHasBeenCalled = false;
			var response = webService.CreateNewWhsReceive("TESTREF", data.Org1.OH_Code, new DateTime(2008, 02, 12, 14, 30, 00, 00), ReceiveType.Codes.Receipt);
			AssertSuccessfulResponse(response, webService);

			AssertEquals("Response should return false if multiple DDL.", false, response.WarehouseHasSingleDockDoorLocation);
			AssertEquals("Response should not populate DDL PK if multiple DDL.", Guid.Empty, response.SingleDockDoorLocationPK);
			AssertEquals("Response should not populate DDL string if multiple DDL.", null, response.SingleDockDoorLocation);
		}

		#endregion

		#region TestCreateNewWhsReceive_ExistingExtRef

		[TestDate(2009, 1, 1)]
		public void TestCreateNewWhsReceive_ExistingExtRef()
		{
			var webService = GetNewWebService();

			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "TESTREF");

			webService.Factory.Save();

			AssertEquals(true, receive.IsInDatabase);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.AllowedToRunServiceHasBeenCalled = false;

			var expectedMessage = "Error - WD_ExternalReference: This Receive Reference is already used by another Receive for this client. To save this Receive you must enter a reference that is not already used";
			AssertBusinessValidationError(webService, expectedMessage, webService.CreateNewWhsReceive("TESTREF", data.Org1.OH_Code, new DateTime(2008, 02, 26), ReceiveType.Codes.Receipt));
		}

		#endregion

		#region TestCreateNewWhsReceive_NonExistingClient

		public void TestCreateNewWhsReceive_NonExistingClient()
		{
			var webService = GetNewWebService();
			AssertEquals(0, webService.Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "CLIENT")).Length);

			webService.AllowedToRunServiceHasBeenCalled = false;
			AssertBusinessValidationError(webService, "Please provide a valid client code.",
				webService.CreateNewWhsReceive("12345", "CLIENT", new DateTime(), ReceiveType.Codes.Receipt));
		}

		#endregion

		#region TestCreateNewWhsReceive_NoClient

		public void TestCreateNewWhsReceive_NoClient()
		{
			var webService = GetNewWebService();
			webService.AllowedToRunServiceHasBeenCalled = false;
			AssertBusinessValidationError(webService, "Please provide a valid client code.",
				webService.CreateNewWhsReceive("12345", "", new DateTime(), ReceiveType.Codes.Receipt));
		}

		#endregion

		#region TestCreateNewWhsReceive_EmptyRef

		[TestDate(2008, 04, 09, 10, 10, 10)]
		public void TestCreateNewWhsReceive_EmptyRef()
		{
			var webService = GetNewWebService();
			AssertEquals(0, webService.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_ExternalReference, "20080409101010")).Length);

			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.AllowedToRunServiceHasBeenCalled = false;
			var response = webService.CreateNewWhsReceive("", data.Org1.OH_Code, new DateTime(2008, 02, 26), ReceiveType.Codes.Receipt);

			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Docket);
			AssertNotEquals(ZGuid.Empty, response.Docket.PK);

			var receive = webService.Factory.Load<WhsReceive>(new ZGuid(response.Docket.PK));

			AssertNotNull(receive);
			AssertEquals(response.Docket.DocketID, receive.WD_DocketID);
			AssertNotNull(receive.Client);
			AssertEquals(data.Org1.OH_Code, receive.Client.OH_Code);
			AssertEquals("20080409101010", receive.WD_ExternalReference);
			AssertEquals(new ZDateTimeOffset(2008, 02, 26), receive.WD_ArrivalDate);
		}

		#endregion

		#region TestCreateNewWhsReceive_WithEmptyArrivalDate

		[TestDate(2012, 06, 14)]
		public void TestCreateNewWhsReceive_WithEmptyArrivalDate()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(webService.Factory);

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.CreateNewWhsReceive("", data.Org1.OH_Code, DateTime.MinValue, ReceiveType.Codes.Receipt);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Docket);
			AssertNotEquals(ZGuid.Empty, response.Docket.PK);

			var receive = webService.Factory.Load<WhsReceive>(new ZGuid(response.Docket.PK));
			AssertNotNull(receive);
			AssertEquals(response.Docket.DocketID, receive.WD_DocketID);
			AssertNotNull(receive.Client);
			AssertEquals(data.Org1.OH_Code, receive.Client.OH_Code);
			AssertEquals(new ZDateTimeOffset(2012, 06, 14), receive.WD_ArrivalDate);
		}

		#endregion

		#region TestCreateNewWhsReceive_ShowStockOnHandWarningOnPutaway

		public void TestCreateNewWhsReceive_ShowStockOnHandWarningOnPutaway()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var response1 = webService.CreateNewWhsReceive("R1", data.Org1.OH_Code, DateTime.MinValue, ReceiveType.Codes.Receipt);
			AssertEquals(true, response1.ShowStockOnHandWarningOnPutaway);

			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var response2 = webService.CreateNewWhsReceive("R2", data.Org1.OH_Code, DateTime.MinValue, ReceiveType.Codes.Receipt);
			AssertEquals(false, response2.ShowStockOnHandWarningOnPutaway);
		}

		#endregion

		#region TestCreateNewWhsReceive_HeldCodesAndLineDuplicateSecurity

		public void TestCreateNewWhsReceive_HeldCodesAndLineDuplicateSecurity()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(webService.Factory);
			var client2 = Helper.CreateClient("C2");
			var heldCode1 = Helper.CreateInventoryHeldCode("AAA", "AAA for system");
			var heldCode2 = Helper.CreateInventoryHeldCode("BBB", "BBB for client 1", data.Org1.PK);
			var heldCode3 = Helper.CreateInventoryHeldCode("CCC", "test", data.Org1.PK);
			var heldCode4 = Helper.CreateInventoryHeldCode("BBB2", "BBB for client 2", client2.PK);
			var heldCode5 = Helper.CreateInventoryHeldCode("DDD", "test", client2.PK);
			Helper.Factory.Save();

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			Env.Security.WhsRFScanningUnloadDuplicatePreviousLine.IsAllowed = true;
			var response1 = webService.CreateNewWhsReceive("R1", data.Org1.OH_Code, DateTime.Today, ReceiveType.Codes.Receipt);
			AssertNotNull(response1.Docket);
			AssertEquals(true, response1.CanDuplicatePreviousLine);
			AssertEquals(8, response1.HeldCodes.Length);
			response1.HeldCodes.Single(p => string.IsNullOrEmpty(p.Code) && string.IsNullOrEmpty(p.Description));
			response1.HeldCodes.Single(p => p.Code == "HEL" && p.Description == "Held");
			response1.HeldCodes.Single(p => p.Code == "DAM" && p.Description == "Damaged");
			response1.HeldCodes.Single(p => p.Code == "LCC" && p.Description == "Lost in Cycle Count");
			response1.HeldCodes.Single(p => p.Code == "SHORT" && p.Description == "Short Picked");
			response1.HeldCodes.Single(p => p.Code == "AAA" && p.Description == "AAA for system");
			response1.HeldCodes.Single(p => p.Code == "BBB" && p.Description == "BBB for client 1");
			response1.HeldCodes.Single(p => p.Code == "CCC" && p.Description == "test");

			Env.Security.WhsRFScanningUnloadDuplicatePreviousLine.IsAllowed = false;
			var response2 = webService.CreateNewWhsReceive("R2", data.Org1.OH_Code, DateTime.Today, ReceiveType.Codes.Receipt);
			AssertNotNull(response2.Docket);
			AssertEquals(false, response2.CanDuplicatePreviousLine);
			AssertEquals(8, response2.HeldCodes.Length);
			response2.HeldCodes.Single(p => string.IsNullOrEmpty(p.Code) && string.IsNullOrEmpty(p.Description));
			response2.HeldCodes.Single(p => p.Code == "HEL" && p.Description == "Held");
			response2.HeldCodes.Single(p => p.Code == "DAM" && p.Description == "Damaged");
			response2.HeldCodes.Single(p => p.Code == "LCC" && p.Description == "Lost in Cycle Count");
			response2.HeldCodes.Single(p => p.Code == "SHORT" && p.Description == "Short Picked");
			response2.HeldCodes.Single(p => p.Code == "AAA" && p.Description == "AAA for system");
			response2.HeldCodes.Single(p => p.Code == "BBB" && p.Description == "BBB for client 1");
			response2.HeldCodes.Single(p => p.Code == "CCC" && p.Description == "test");
		}

		#endregion

		#region TestCreateNewWhsReceive_Return

		public void TestCreateNewWhsReceive_Return()
		{
			var webService = GetNewWebService();

			var data = new TestDataSimpleEnvironment(webService.Factory);
			var helper = new WhsTestHelperFunctions((GetNewWebService()).Factory);
			webService.Factory.Save();

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.AllowedToRunServiceHasBeenCalled = false;
			var arrivalDate = new DateTime(DateTime.Today.Year, 02, 12, 14, 30, 00, 00);
			var response = webService.CreateNewWhsReceive("TESTREF", data.Org1.OH_Code, arrivalDate, ReceiveType.Codes.Returns);
			var docket = response.Docket;
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(docket);
			AssertNotEquals(Guid.Empty, docket.PK);

			var receive = webService.Factory.Load<WhsReceive>(new ZGuid(docket.PK));
			AssertNotNull(receive);
			AssertEquals(docket.DocketID, receive.WD_DocketID);
			AssertNotNull(receive.Client);
			AssertEquals(data.Org1.OH_Code, receive.Client.OH_Code);
			AssertEquals("TESTREF", receive.WD_ExternalReference);
			AssertEquals(new ZDateTimeOffset(arrivalDate), receive.WD_ArrivalDate);
			AssertEquals(true, receive.StartedReceiving);
			AssertEquals(ReceiveType.Codes.Returns, receive.WD_DocketSubType);
		}

		#endregion

		#region TestCreateNewWhsReceive_InvalidReceiveType

		public void TestCreateNewWhsReceive_InvalidReceiveType()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(webService.Factory);
			var helper = new WhsTestHelperFunctions((GetNewWebService()).Factory);
			webService.Factory.Save();

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.AllowedToRunServiceHasBeenCalled = false;
			var arrivalDate = new DateTime(DateTime.Today.Year, 02, 12, 14, 30, 00, 00);

			AssertBusinessValidationError(webService, "Error - WD_DocketSubType: Enter a valid Docket Sub Type.",
				webService.CreateNewWhsReceive("TESTREF", data.Org1.OH_Code, arrivalDate, "XXX"));
		}

		#endregion

		#region TestCreateNewWhsReceive_ExtraLongReference

		public void TestCreateNewWhsReceive_ExtraLongReference()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var warehouse = Helper.CreateWarehouse("Wh1");
			var client = Helper.CreateClient("Client");
			Helper.Factory.Save();

			var extraLongReference = "123456789012345678901234567890123456";
			Assert("Precondition", extraLongReference.Length > WhsDocketSchema.WD_ExternalReference.MaxLength);

			WebServiceResponse response = null;
			var webService = GetNewWebService(warehouse, staff);
			AssertNoExceptionThrown(() => response = webService.CreateNewWhsReceive(extraLongReference, client.OH_Code, DateTime.Today, ReceiveType.Codes.Receipt));
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Receive Reference exceeds maximum length allowed. The maximum length of this property is 35 characters, but 36 were entered.", response.ErrorMessage);
		}

		#endregion

		#region TestCreateNewWhsReceive_ReturnReceive

		[TestDate(2025, 1, 1)]
		public void TestCreateNewWhsReceive_ReturnReceive()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory);
			factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.CreateNewWhsReceive("TESTREF", data.Org1.OH_Code, new DateTime(2025, 02, 14, 13, 30, 00, 00), ReceiveType.Codes.Returns);
			var docket = response.Docket;
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(docket);
			AssertNotEquals(Guid.Empty, docket.PK);

			var receive = factory.Load<WhsReceive>(new ZGuid(docket.PK));
			AssertNotNull(receive);
			AssertEquals(docket.DocketID, receive.WD_DocketID);
			AssertNotNull(receive.Client);
			AssertEquals(data.Org1.OH_Code, receive.Client.OH_Code);
			AssertEquals("TESTREF", receive.WD_ExternalReference);
			AssertEquals(new ZDateTimeOffset(2025, 02, 14, 13, 30, 0), receive.WD_ArrivalDate);
			AssertEquals(true, receive.StartedReceiving);
			AssertEquals(ReceiveType.Codes.Returns, receive.WD_DocketSubType);

			AssertEquals(response.Error, ErrorTypes.WarningOnly);
			AssertEquals(response.ErrorMessage, WhsReceive.ReturnReceivesRequireAnOrderToReturnWarning);
		}

		[TestDate(2025, 1, 1)]
		public void TestCreateNewWhsReceive_ReturnReceive_WithOrderReference()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory);
			factory.Save();

			var inventory = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(inventory);
			factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O123", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.CreateNewWhsReceive("O123", data.Org1.OH_Code, new DateTime(2025, 02, 14, 13, 30, 00, 00), ReceiveType.Codes.Returns);
			var docket = response.Docket;
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(docket);
			AssertNotEquals(Guid.Empty, docket.PK);

			var receive = factory.Load<WhsReceive>(new ZGuid(docket.PK));
			AssertNotNull(receive);
			AssertEquals(docket.DocketID, receive.WD_DocketID);
			AssertNotNull(receive.Client);
			AssertEquals(data.Org1.OH_Code, receive.Client.OH_Code);
			AssertEquals("O123", receive.WD_ExternalReference);
			AssertEquals(new ZDateTimeOffset(2025, 02, 14, 13, 30, 0), receive.WD_ArrivalDate);
			AssertEquals(true, receive.StartedReceiving);
			AssertEquals(ReceiveType.Codes.Returns, receive.WD_DocketSubType);

			AssertEquals(response.Error, ErrorTypes.None);
			Assert(response.ErrorMessage.IsNullOrEmpty());
		}

		[TestDate(2025, 1, 1)]
		public void TestCreateNewWhsReceive_ReturnReceive_OrderFullyReturned()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory);
			factory.Save();

			var inventory = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(inventory);
			factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O123", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "O123");
			Helper.CreateWhsReceiveLine(returnReceive, data.Part1, 10m);
			returnReceive.WD_WD_ParentDocket = order.PK;
			factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.CreateNewWhsReceive("O123", data.Org1.OH_Code, new DateTime(2025, 02, 14, 13, 30, 00, 00), ReceiveType.Codes.Returns);
			AssertNotNull("DocketInfo is not null.", response.Docket);
			AssertEquals("DocketInfo has an empty PK.", ZGuid.Empty, response.Docket.PK);
			AssertEquals(response.Error, ErrorTypes.BusinessValidationError);
			Assert(response.ErrorMessage.Contains("This reference points to a departed order that has already been fully returned or is in the process of being fully returned."));
		}

		[TestDate(2025, 03, 18, 11, 12, 13)]
		public void TestCreateNewWhsReceive_ReturnReceive_EmptyRef()
		{
			var webService = GetNewWebService();
			AssertEquals(0, webService.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_ExternalReference, "20250318111213")).Length);

			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.AllowedToRunServiceHasBeenCalled = false;
			var response = webService.CreateNewWhsReceive("", data.Org1.OH_Code, new DateTime(2025, 03, 10), ReceiveType.Codes.Returns);

			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Docket);
			AssertNotEquals(ZGuid.Empty, response.Docket.PK);

			var receive = webService.Factory.Load<WhsReceive>(new ZGuid(response.Docket.PK));

			AssertNotNull(receive);
			AssertEquals(response.Docket.DocketID, receive.WD_DocketID);
			AssertNotNull(receive.Client);
			AssertEquals(data.Org1.OH_Code, receive.Client.OH_Code);
			AssertEquals("20250318111213", receive.WD_ExternalReference);
			AssertEquals(new ZDateTimeOffset(2025, 03, 10), receive.WD_ArrivalDate);

			AssertEquals(response.Error, ErrorTypes.WarningOnly);
			AssertEquals(response.ErrorMessage, WhsReceive.ReturnReceivesRequireAnOrderToReturnWarning);
		}

		#endregion

		#region TestCreateNewWhsReceive_TaskManagement

		public void TestCreateNewWhsReceive_TaskManagement()
		{
			TestCreateNewWhsReceive_TaskManagementCore(hasReleaseGroup: true);
		}

		public void TestCreateNewWhsReceive_TaskManagement_NoReleaseGroup()
		{
			TestCreateNewWhsReceive_TaskManagementCore(hasReleaseGroup: false);
		}

		void TestCreateNewWhsReceive_TaskManagementCore(bool hasReleaseGroup)
		{
			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryValue: true))
			{
				var data = new TestDataSimpleEnvironment(Helper.Factory);
				if (hasReleaseGroup)
				{
					var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
					data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
				}
				var staff = Helper.CreateGlbStaff("S2", "S2");
				Helper.Factory.Save();

				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.CreateNewWhsReceive("TESTREF", data.Org1.OH_Code, new DateTime(2025, 02, 14, 13, 30, 00, 00), ReceiveType.Codes.Receipt);
				AssertSuccessfulResponse(response, webService);

				var docket = response.Docket;
				var receive = webService.Factory.Load<WhsReceive>(new ZGuid(docket.PK));
				AssertNotNull(receive);
				AssertEquals(docket.DocketID, receive.WD_DocketID);
				if (hasReleaseGroup)
				{
					AssertEquals("Receive planning status should be Planned", TaskPlanningStatus.Codes.Planned, receive.WD_TaskPlanningStatus);
					AssertEquals("Response should have Task PK.", true, docket.TaskPK != Guid.Empty);
					var task = webService.Factory.Load<ProcessTask>(new ZGuid(docket.TaskPK));
					AssertEquals("Task should have correct receive parent PK.", receive.PK, task.P9_ParentID);
					AssertEquals("Task should have correct form flow type.", WarehouseTaskFormFlowTypes.UnloadJob, task.P9_FormFlowType);
					AssertEquals("Task should have correct Status.", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
					AssertEquals("Task should have correct Staff.", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
					AssertEquals("Task should have correct receive parent table code.", WhsDocketSchema.Constants.Prefix, task.P9_ParentTableCode);

					// Temporary Values
					AssertEquals("Task should have correct Temporary Value Type.", "UDF", task.P9_Type);
					AssertEquals("Task should have correct Temporary Value Description.", $"Temporary Unload Task Description", task.P9_Description);
				}
				else
				{
					AssertEquals("Receive planning status should be correct", string.Empty, receive.WD_TaskPlanningStatus);
					AssertEquals("Response should have NOT Task PK.", true, docket.TaskPK == Guid.Empty);
				}
			}
		}

		#endregion

		public void TestCreateNewWhsReceive_ConcurrencyException()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory);
			factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();
			var innerException = new Exception();
			var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)dummy).Row, TestConnection);
			webService.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Helper.Factory);

			var response = webService.CreateNewWhsReceive("TESTREF", data.Org1.OH_Code, new DateTime(2025, 02, 14, 13, 30, 00, 00), ReceiveType.Codes.Receipt);
			AssertEquals(response.Error, ErrorTypes.BusinessValidationError);
			Assert(response.ErrorMessage.Contains("Another user has made changes while you're creating the receive. Please restart the operation and try again."));

			var newFactory = new BusinessObjectFactory();
			AssertEquals("No new receive created.", 0, factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_ExternalReference, "TESTREF")).Length);
		}
	}
}
