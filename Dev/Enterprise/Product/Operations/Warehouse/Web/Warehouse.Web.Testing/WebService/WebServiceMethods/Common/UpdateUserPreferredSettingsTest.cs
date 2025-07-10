using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class UpdateUserPreferredSettingsTest : WhsSecureServiceTestCase
	{
		public void TestUpdateUserPreferredSettings_UserRegistryNotFound()
		{
			var webService = GetNewWebService();
			var staff = Helper.CreateGlbStaff("AAA", "AAA");

			Helper.Factory.Save();

			webService.SecurityHeader.UserName = staff.GS_Code;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);
			var response = webService.UpdateUserPreferredSettings(string.Empty, WhsRFRegistry.DefaultUOMPackType, Guid.Empty, null, 0, Guid.Empty, false);
			AssertBusinessValidationError(webService, "RF Registry Configuration for user 'AAA' could not be found. Please log out and back in to recreate.", response);
		}

		public void TestUpdateUserPreferredSettings_ClientNotFound()
		{
			TestUpdateUserPreferredSettings_InvalidFieldCore(isValidClient: false, expectedErrorMessage: "Specified Client Code 'MadeUpClient' is not valid.");
		}

		public void TestUpdateUserPreferredSettings_UOMTypeNotFound()
		{
			TestUpdateUserPreferredSettings_InvalidFieldCore(isValidUOM: false, expectedErrorMessage: "Specified UOM Type '123' is not valid.");
		}

		public void TestUpdateUserPreferredSettings_PickAreaNotFound()
		{
			TestUpdateUserPreferredSettings_InvalidFieldCore(isValidPickArea: false, expectedErrorMessage: "Specified Pick Area is not valid.");
		}

		public void TestUpdateUserPreferredSettings_PickMethodNotFound()
		{
			TestUpdateUserPreferredSettings_InvalidFieldCore(isValidPickMethod: false, expectedErrorMessage: "Specified Pick Method is not valid.");
		}

		public void TestUpdateUserPreferredSettings_PickGroupNotFound()
		{
			TestUpdateUserPreferredSettings_InvalidFieldCore(isValidPickGroup: false, expectedErrorMessage: "Specified Pick Group is not valid.");
		}

		public void TestUpdateUserPreferredSettings_PrinterNotFound()
		{
			TestUpdateUserPreferredSettings_InvalidFieldCore(isValidPrinter: false, expectedErrorMessage: "Specified Printer is not valid.");
		}

		void TestUpdateUserPreferredSettings_InvalidFieldCore(bool isValidClient = true, bool isValidUOM = true, bool isValidPickArea = true, bool isValidPickMethod = true, bool isValidPickGroup = true, bool isValidPrinter = true, string expectedErrorMessage = null)
		{
			var webService = GetNewWebService();
			var warehouse = Helper.CreateWarehouse("WH1");
			var staff = Helper.CreateGlbStaff("AAA", "AAA");

			Helper.CreateClient("Client");
			Helper.Factory.Save();

			var registry = Helper.CreateWhsRFRegistry(staff, warehouse);
			Helper.Factory.Save();

			var selectedClient = isValidClient ? "Client" : "MadeUpClient";
			var selectedUOM = isValidUOM ? UOMPackTypesList.Codes.SplitCase : "123";
			var selectedPickArea = isValidPickArea ? Guid.Empty : Guid.NewGuid();
			var selectedPickMethod = isValidPickMethod ? "ANY" : "ABC";
			var selectedPickGroup = isValidPickGroup ? 0 : 123;
			var selectedPrinter = isValidPrinter ? Guid.Empty : Guid.NewGuid();

			webService.SecurityHeader.UserName = staff.GS_Code;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);
			var response = webService.UpdateUserPreferredSettings(selectedClient, selectedUOM, selectedPickArea, selectedPickMethod, (short)selectedPickGroup, selectedPrinter, false);
			AssertBusinessValidationError(webService, expectedErrorMessage, response);
		}

		public void TestUpdateUserPreferredSettings_ValidateUOMType()
		{
			var webService = GetNewWebService();
			var warehouse = Helper.CreateWarehouse("WH1");
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var pickMethod = "ANY";
			var clientCode = "Client";

			Helper.CreateClient(clientCode);
			Helper.Factory.Save();

			var registry = Helper.CreateWhsRFRegistry(staff, warehouse);
			Helper.Factory.Save();

			webService.SecurityHeader.UserName = staff.GS_Code;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var uomList = new UOMPackTypesList();
			uomList.AddPair(WhsRFRegistry.DefaultUOMPackType);
			foreach (CodeDescriptionPair uom in uomList)
			{
				var uomResponse = webService.UpdateUserPreferredSettings(clientCode, uom.Code, Guid.Empty, pickMethod, 0, Guid.Empty, false);
				AssertSuccessfulResponse(uomResponse, webService);
				AssertEquals(ErrorTypes.None, uomResponse.Error);
				AssertNull(uomResponse.ErrorMessage);
			}

			var response2 = webService.UpdateUserPreferredSettings(clientCode, "456", Guid.Empty, pickMethod, 0, Guid.Empty, false);
			AssertBusinessValidationError(webService, "Specified UOM Type '456' is not valid.", response2);
		}

		public void TestUpdateUserPreferredSettings_UpdateSuccessful()
		{
			var webService = GetNewWebService();
			var warehouse = Helper.CreateWarehouse("WH1");
			var staff = Helper.CreateGlbStaff("AAA", "AAA");

			var registry = Helper.CreateWhsRFRegistry(staff, warehouse);
			Helper.Factory.Save();

			var client = Helper.CreateClient();

			var pickMethods = new SystemDefinableCodeDescriptionBoolCollection();
			pickMethods.Add("TS1", (NoResString)"Test1", true);

			var pickGroupCollection = new PickGroupCollection();
			pickGroupCollection.Add(new PickGroup() { PickSequence = 5, Description = (NoResString)"ABC" });

			var pickArea = Helper.CreateArea(warehouse, "Area");
			var printer = Helper.CreatePrintQueue("Printer", "P1");
			Helper.Factory.Save();

			using (WarehouseDataRegistry.Instance.PickMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, pickMethods))
			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, pickGroupCollection))
			{
				webService.SecurityHeader.UserName = staff.GS_Code;
				webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);
				var response = webService.UpdateUserPreferredSettings(client.OH_Code, UOMPackTypesList.Codes.SplitCase, pickArea.PK.ToGuid(), pickMethods[0].Code, pickGroupCollection[0].PickSequence, printer.PK.ToGuid(), true);
				AssertSuccessfulResponseWithNoErrors(response, webService);

				var dbRegistry = Helper.Factory.Load<WhsRFRegistry>(new ZQuery(WhsRFRegistrySchema.WRR_GS_NKAssignedTo, staff.GS_Code)).Single();
				AssertNotNull(dbRegistry);
				CombineAssertions(() =>
				{
					AssertEquals(nameof(dbRegistry.WRR_OH_Client), client.PK, dbRegistry.WRR_OH_Client);
					AssertEquals(nameof(dbRegistry.WRR_UOMPackType), UOMPackTypesList.Codes.SplitCase, dbRegistry.WRR_UOMPackType);
					AssertEquals(nameof(dbRegistry.WRR_EnableWarehouseErrorAudio), true, dbRegistry.WRR_EnableWarehouseErrorAudio);
					AssertEquals(nameof(dbRegistry.WRR_PickGroupSequence), pickGroupCollection[0].PickSequence, dbRegistry.WRR_PickGroupSequence);
					AssertEquals(nameof(dbRegistry.WRR_PickMethodCode), pickMethods[0].Code, dbRegistry.WRR_PickMethodCode);
					AssertEquals(nameof(dbRegistry.WRR_WA_PickingArea), pickArea.PK, dbRegistry.WRR_WA_PickingArea);
					AssertEquals(nameof(dbRegistry.WRR_SQ_Printer), printer.PK, dbRegistry.WRR_SQ_Printer);
				});
			}
		}

		public void TestUpdateUserPreferredSettings_UpdateSuccessful_BlankValues()
		{
			TestUpdateUserPreferredSettings_UpdateSuccessful_BlankValuesCore(string.Empty);
		}

		public void TestUpdateUserPreferredSettings_UpdateSuccessful_BlankValues_NullClientCode()
		{
			TestUpdateUserPreferredSettings_UpdateSuccessful_BlankValuesCore(null);
		}

		void TestUpdateUserPreferredSettings_UpdateSuccessful_BlankValuesCore(string clientCode)
		{
			var webService = GetNewWebService();
			var warehouse = Helper.CreateWarehouse("WH1");
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			Helper.Factory.Save();

			var registry = Helper.CreateWhsRFRegistry(staff, warehouse);
			Helper.Factory.Save();

			webService.SecurityHeader.UserName = staff.GS_Code;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);
			var response = webService.UpdateUserPreferredSettings(clientCode, WhsRFRegistry.DefaultUOMPackType, Guid.Empty, "ANY", 0, Guid.Empty, false);
			AssertSuccessfulResponseWithNoErrors(response, webService);
		}

		public void TestUpdateUserPreferredSettings_ConcurrencyError()
		{
			var webService = GetNewWebService();
			var warehouse = Helper.CreateWarehouse("WH1");
			var staff = Helper.CreateGlbStaff("AAA", "AAA");

			var registry = Helper.CreateWhsRFRegistry(staff, warehouse);
			Helper.Factory.Save();

			var client = Helper.CreateClient();

			var pickMethods = new SystemDefinableCodeDescriptionBoolCollection();
			pickMethods.Add("TS1", (NoResString)"Test1", true);

			var pickGroupCollection = new PickGroupCollection();
			pickGroupCollection.Add(new PickGroup() { PickSequence = 5, Description = (NoResString)"ABC" });

			var pickArea = Helper.CreateArea(warehouse, "Area");
			var printer = Helper.CreatePrintQueue("Printer", "P1");
			Helper.Factory.Save();

			var innerException = new Exception();
			var exception = new ZSaveConcurrencyException(new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)registry).Row, Db.Connection), webService.Factory);
			webService.Factory.Saving += f => throw exception;

			using (WarehouseDataRegistry.Instance.PickMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, pickMethods))
			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, pickGroupCollection))
			{
				webService.SecurityHeader.UserName = staff.GS_Code;
				webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);
				var response = webService.UpdateUserPreferredSettings(client.OH_Code, UOMPackTypesList.Codes.SplitCase, pickArea.PK.ToGuid(), pickMethods[0].Code, pickGroupCollection[0].PickSequence, printer.PK.ToGuid(), true);
				AssertBusinessValidationError(webService, "Unable to save user registry as another user made changes. Please refresh and try again.", response);

				var dbRegistry = Helper.Factory.Load<WhsRFRegistry>(new ZQuery(WhsRFRegistrySchema.WRR_GS_NKAssignedTo, staff.GS_Code)).Single();
				AssertNotNull(dbRegistry);
				CombineAssertions(() =>
				{
					AssertEquals(nameof(dbRegistry.WRR_OH_Client), ZGuid.Empty, dbRegistry.WRR_OH_Client);
					AssertEquals(nameof(dbRegistry.WRR_UOMPackType), WhsRFRegistry.DefaultUOMPackType, dbRegistry.WRR_UOMPackType);
					AssertEquals(nameof(dbRegistry.WRR_EnableWarehouseErrorAudio), false, dbRegistry.WRR_EnableWarehouseErrorAudio);
					AssertEquals(nameof(dbRegistry.WRR_PickGroupSequence), (short)0, dbRegistry.WRR_PickGroupSequence);
					AssertEquals(nameof(dbRegistry.WRR_PickMethodCode), "ANY", dbRegistry.WRR_PickMethodCode);
					AssertEquals(nameof(dbRegistry.WRR_WA_PickingArea), ZGuid.Empty, dbRegistry.WRR_WA_PickingArea);
					AssertEquals(nameof(dbRegistry.WRR_SQ_Printer), ZGuid.Empty, dbRegistry.WRR_SQ_Printer);
				});
			}
		}

		public void TestUpdateUserPreferredSettings_DBHits()
		{
			var webService = GetNewWebService();
			var warehouse = Helper.CreateWarehouse("WH1");
			var staff = Helper.CreateGlbStaff("AAA", "AAA");

			var registry = Helper.CreateWhsRFRegistry(staff, warehouse);
			Helper.Factory.Save();

			var client1 = Helper.CreateClient("Client1");
			var client2 = Helper.CreateClient("Client2");
			var client3 = Helper.CreateClient("Client3");
			var client4 = Helper.CreateClient("Client4");
			var client5 = Helper.CreateClient("Client5");

			var pickMethods = new SystemDefinableCodeDescriptionBoolCollection();
			pickMethods.Add("TS1", (NoResString)"Test1", true);
			pickMethods.Add("TS2", (NoResString)"Test2", true);
			pickMethods.Add("TS3", (NoResString)"Test3", true);
			pickMethods.Add("TS4", (NoResString)"Test4", true);
			pickMethods.Add("TS5", (NoResString)"Test5", true);

			var pickGroupCollection = new PickGroupCollection();
			pickGroupCollection.Add(new PickGroup() { PickSequence = 1, Description = (NoResString)"ABC" });
			pickGroupCollection.Add(new PickGroup() { PickSequence = 2, Description = (NoResString)"DEF" });
			pickGroupCollection.Add(new PickGroup() { PickSequence = 3, Description = (NoResString)"GHI" });
			pickGroupCollection.Add(new PickGroup() { PickSequence = 4, Description = (NoResString)"JKL" });

			var pickArea1 = Helper.CreateArea(warehouse, "Area1");
			var pickArea2 = Helper.CreateArea(warehouse, "Area2");
			var pickArea3 = Helper.CreateArea(warehouse, "Area3");
			var pickArea4 = Helper.CreateArea(warehouse, "Area4");
			var pickArea5 = Helper.CreateArea(warehouse, "Area5");
			var printer1 = Helper.CreatePrintQueue("Printer1", "P1");
			var printer2 = Helper.CreatePrintQueue("Printer2", "P2");
			var printer3 = Helper.CreatePrintQueue("Printer3", "P3");
			var printer4 = Helper.CreatePrintQueue("Printer4", "P4");
			var printer5 = Helper.CreatePrintQueue("Printer5", "P5");
			Helper.Factory.Save();

			using (WarehouseDataRegistry.Instance.PickMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, pickMethods))
			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, pickGroupCollection))
			{
				webService.SecurityHeader.UserName = staff.GS_Code;
				webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

				var expectedDBHits = new Dictionary<string, int>
				{
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ StmPrintQueueSchema.Constants.TableName, 1 },
					{ WhsAreaSchema.Constants.TableName, 1 },
					{ WhsRFRegistrySchema.Constants.TableName, 1 }
				};

				using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
				{
					var response = webService.UpdateUserPreferredSettings(client1.OH_Code, UOMPackTypesList.Codes.SplitCase, pickArea1.PK.ToGuid(), pickMethods[0].Code, pickGroupCollection[0].PickSequence, printer1.PK.ToGuid(), true);
					AssertSuccessfulResponseWithNoErrors(response, webService);
				}
			}
		}
	}
}
