using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class CreateOrLoadUserPreferredSettingsTest : WhsSecureServiceTestCase
	{
		public void TestCreateOrLoadUserPreferredSettings_EquipmentNotFound()
		{
			var webService = GetNewWebService();

			Helper.Factory.Save();

			var response = webService.CreateOrLoadUserPreferredSettings("NoMatchEquipment");
			AssertBusinessValidationError(webService, "Equipment 'NoMatchEquipment' could not be found.", response);
		}

		public void TestCreateOrLoadUserPreferredSettings_RegistryDoesNotExist()
		{
			TestCreateOrLoadUserPreferredSettings_RegistryDoesNotExistCore(false);
		}

		public void TestCreateOrLoadUserPreferredSettings_RegistryDoesNotExist_WithEquipment()
		{
			TestCreateOrLoadUserPreferredSettings_RegistryDoesNotExistCore(true);
		}

		void TestCreateOrLoadUserPreferredSettings_RegistryDoesNotExistCore(bool setEquipment)
		{
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			staff.GS_IsDevice = true;
			staff.GS_WorkingLanguage = SharedConstants.Languages.English;

			var webService = GetNewWebService();
			webService.SecurityHeader.UserName = staff.GS_Code;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);
			webService.SecurityHeader.WarehouseCode = Helper.CreateWarehouse("WH1").WW_WarehouseCode;

			var equipment = Helper.CreateEquipment("TR1", 5m, Constants.Weight.Kilograms, 5m, Constants.Volume.Litre);
			Helper.Factory.Save();

			webService.SecurityHeader.UserName = staff.GS_Code;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);
			var response = webService.CreateOrLoadUserPreferredSettings(setEquipment ? equipment.RQ_ShortCode : string.Empty);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var expectedEquipment = setEquipment ? equipment : null;
			var registryInfo = response.RFRegistryInfo;
			AssertNotNull(registryInfo);
			CombineAssertions(() =>
			{
				AssertEquals(nameof(registryInfo.ClientCode), null, registryInfo.ClientCode);
				AssertEquals(nameof(registryInfo.UOMType), WhsRFRegistry.DefaultUOMPackType, registryInfo.UOMType);
				AssertEquals(nameof(registryInfo.EnableErrorAudio), false, registryInfo.EnableErrorAudio);
				AssertEquals(nameof(registryInfo.Language), SharedConstants.Languages.English, registryInfo.Language);
				AssertEquals(nameof(registryInfo.EquipmentRegistrationNumber), expectedEquipment?.RQ_Registration, registryInfo.EquipmentRegistrationNumber);
				AssertEquals(nameof(registryInfo.PickGroup), (short)0, registryInfo.PickGroup.PickSequence);
				AssertEquals(nameof(registryInfo.PickGroup), "ANY", registryInfo.PickGroup.Description);
				AssertEquals(nameof(registryInfo.PickMethod), "ANY", registryInfo.PickMethod.Code);
				AssertEquals(nameof(registryInfo.PickMethod), "ANY", registryInfo.PickMethod.Description);
				AssertEquals(nameof(registryInfo.PickMethod), true, registryInfo.PickMethod.IsDefault);
				AssertEquals(nameof(registryInfo.PickArea), string.Empty, registryInfo.PickArea.Name);
				AssertEquals(nameof(registryInfo.PickArea), string.Empty, registryInfo.PickArea.Description);
				AssertEquals(nameof(registryInfo.PickArea), Guid.Empty, registryInfo.PickArea.AreaPK);
				AssertEquals(nameof(registryInfo.Printer), string.Empty, registryInfo.Printer.Name);
				AssertEquals(nameof(registryInfo.Printer), Guid.Empty, registryInfo.Printer.PK);
			});

			var dbRegistry = Helper.Factory.Load<WhsRFRegistry>(new ZQuery(WhsRFRegistrySchema.WRR_GS_NKAssignedTo, staff.GS_Code)).Single();
			AssertNotNull(dbRegistry);
			CombineAssertions(() =>
			{
				AssertEquals(nameof(dbRegistry.WRR_OH_Client), ZGuid.Empty, dbRegistry.WRR_OH_Client);
				AssertEquals(nameof(dbRegistry.WRR_UOMPackType), WhsRFRegistry.DefaultUOMPackType, dbRegistry.WRR_UOMPackType);
				AssertEquals(nameof(dbRegistry.WRR_EnableWarehouseErrorAudio), false, dbRegistry.WRR_EnableWarehouseErrorAudio);
				AssertEquals(nameof(dbRegistry.WRR_RQ_LastUsedEquipment), expectedEquipment?.PK ?? ZGuid.Empty, dbRegistry.WRR_RQ_LastUsedEquipment);
				AssertEquals(nameof(dbRegistry.WRR_PickGroupSequence), (short)0, dbRegistry.WRR_PickGroupSequence);
				AssertEquals(nameof(dbRegistry.WRR_PickMethodCode), "ANY", dbRegistry.WRR_PickMethodCode);
				AssertEquals(nameof(dbRegistry.WRR_WA_PickingArea), ZGuid.Empty, dbRegistry.WRR_WA_PickingArea);
				AssertEquals(nameof(dbRegistry.WRR_SQ_Printer), ZGuid.Empty, dbRegistry.WRR_SQ_Printer);
			});
		}

		public void TestCreateOrLoadUserPreferredSettings_ExistingRegistry()
		{
			TestCreateOrLoadUserPreferredSettings_ExistingRegistryCore(false);
		}

		public void TestCreateOrLoadUserPreferredSettings_ExistingRegistry_WithEquipment()
		{
			TestCreateOrLoadUserPreferredSettings_ExistingRegistryCore(true);
		}

		void TestCreateOrLoadUserPreferredSettings_ExistingRegistryCore(bool updateEquipment)
		{
			var warehouse = Helper.CreateWarehouse("WH1");
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			staff.GS_IsDevice = true;
			staff.GS_WorkingLanguage = SharedConstants.Languages.English;

			var equipment1 = Helper.CreateEquipment("TR1", 5m, Constants.Weight.Kilograms, 5m, Constants.Volume.Litre);
			var equipment2 = Helper.CreateEquipment("TR2", 5m, Constants.Weight.Kilograms, 5m, Constants.Volume.Litre);

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
				var registry = Helper.CreateWhsRFRegistry(staff, warehouse);
				registry.WRR_OH_Client = client.PK;
				registry.WRR_UOMPackType = UOMPackTypesList.Codes.SplitCase;
				registry.WRR_EnableWarehouseErrorAudio = true;
				registry.WRR_RQ_LastUsedEquipment = equipment2.PK;
				registry.WRR_PickGroupSequence = pickGroupCollection[0].PickSequence;
				registry.WRR_PickMethodCode = pickMethods[0].Code;
				registry.WRR_WA_PickingArea = pickArea.PK;
				registry.WRR_SQ_Printer = printer.PK;

				Helper.Factory.Save();

				var webService = GetNewWebService();
				webService.SecurityHeader.UserName = staff.GS_Code;
				webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

				var expectedEquipment = updateEquipment ? equipment1 : null;
				var response = webService.CreateOrLoadUserPreferredSettings(expectedEquipment?.RQ_Registration);
				AssertSuccessfulResponseWithNoErrors(response, webService);

				var registryInfo = response.RFRegistryInfo;
				AssertNotNull(registryInfo);
				CombineAssertions(() =>
				{
					AssertEquals(nameof(registryInfo.ClientCode), client.OH_Code, registryInfo.ClientCode);
					AssertEquals(nameof(registryInfo.UOMType), UOMPackTypesList.Codes.SplitCase, registryInfo.UOMType);
					AssertEquals(nameof(registryInfo.EnableErrorAudio), true, registryInfo.EnableErrorAudio);
					AssertEquals(nameof(registryInfo.Language), SharedConstants.Languages.English, registryInfo.Language);
					AssertEquals(nameof(registryInfo.EquipmentRegistrationNumber), expectedEquipment?.RQ_Registration, registryInfo.EquipmentRegistrationNumber);
					AssertEquals(nameof(registryInfo.PickGroup), pickGroupCollection[0].PickSequence, registryInfo.PickGroup.PickSequence);
					AssertEquals(nameof(registryInfo.PickGroup), pickGroupCollection[0].Description, registryInfo.PickGroup.Description);
					AssertEquals(nameof(registryInfo.PickMethod), pickMethods[0].Code, registryInfo.PickMethod.Code);
					AssertEquals(nameof(registryInfo.PickMethod), pickMethods[0].Description, registryInfo.PickMethod.Description);
					AssertEquals(nameof(registryInfo.PickMethod), true, registryInfo.PickMethod.IsDefault);
					AssertEquals(nameof(registryInfo.PickArea), pickArea.WA_Name, registryInfo.PickArea.Name);
					AssertEquals(nameof(registryInfo.PickArea), pickArea.WA_NameMultilingual, registryInfo.PickArea.Description);
					AssertEquals(nameof(registryInfo.PickArea), pickArea.PK.ToGuid(), registryInfo.PickArea.AreaPK);
					AssertEquals(nameof(registryInfo.Printer), printer.SQ_DisplayName, registryInfo.Printer.Name);
					AssertEquals(nameof(registryInfo.Printer), printer.PK, registryInfo.Printer.PK);
				});

				var dbRegistry = Helper.Factory.Load<WhsRFRegistry>(new ZQuery(WhsRFRegistrySchema.WRR_GS_NKAssignedTo, staff.GS_Code)).Single();
				AssertNotNull(dbRegistry);
				CombineAssertions(() =>
				{
					AssertEquals(nameof(dbRegistry.WRR_OH_Client), client.PK, dbRegistry.WRR_OH_Client);
					AssertEquals(nameof(dbRegistry.WRR_UOMPackType), UOMPackTypesList.Codes.SplitCase, dbRegistry.WRR_UOMPackType);
					AssertEquals(nameof(dbRegistry.WRR_EnableWarehouseErrorAudio), true, dbRegistry.WRR_EnableWarehouseErrorAudio);
					AssertEquals(nameof(dbRegistry.WRR_RQ_LastUsedEquipment), expectedEquipment?.PK ?? ZGuid.Empty, dbRegistry.WRR_RQ_LastUsedEquipment);
					AssertEquals(nameof(dbRegistry.WRR_PickGroupSequence), pickGroupCollection[0].PickSequence, dbRegistry.WRR_PickGroupSequence);
					AssertEquals(nameof(dbRegistry.WRR_PickMethodCode), pickMethods[0].Code, dbRegistry.WRR_PickMethodCode);
					AssertEquals(nameof(dbRegistry.WRR_WA_PickingArea), pickArea.PK, dbRegistry.WRR_WA_PickingArea);
					AssertEquals(nameof(dbRegistry.WRR_SQ_Printer), printer.PK, dbRegistry.WRR_SQ_Printer);
				});
			}
		}

		public void TestCreateOrLoadUserPreferredSettings_RegistryValuesWithNoMatch()
		{
			var warehouse = Helper.CreateWarehouse("WH1");
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			staff.GS_IsDevice = true;
			staff.GS_WorkingLanguage = SharedConstants.Languages.English;

			var equipment = Helper.CreateEquipment("TR1", 5m, Constants.Weight.Kilograms, 5m, Constants.Volume.Litre);

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
				var registry = Helper.CreateWhsRFRegistry(staff, warehouse);
				registry.WRR_OH_Client = client.PK;
				registry.WRR_UOMPackType = UOMPackTypesList.Codes.SplitCase;
				registry.WRR_EnableWarehouseErrorAudio = true;
				registry.WRR_RQ_LastUsedEquipment = equipment.PK;
				registry.WRR_PickGroupSequence = pickGroupCollection[0].PickSequence;
				registry.WRR_PickMethodCode = pickMethods[0].Code;
				registry.WRR_WA_PickingArea = pickArea.PK;
				registry.WRR_SQ_Printer = printer.PK;

				Helper.Factory.Save();
			}

			var webService = GetNewWebService();
			webService.SecurityHeader.UserName = staff.GS_Code;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var response = webService.CreateOrLoadUserPreferredSettings(equipment.RQ_Registration);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var registryInfo = response.RFRegistryInfo;
			AssertNotNull(registryInfo);
			CombineAssertions(() =>
			{
				AssertEquals(nameof(registryInfo.PickGroup), (short)0, registryInfo.PickGroup.PickSequence);
				AssertEquals(nameof(registryInfo.PickGroup), "ANY", registryInfo.PickGroup.Description);
				AssertEquals(nameof(registryInfo.PickMethod), WhsAreaAndPickMethodHelper.AnyCode, registryInfo.PickMethod.Code);
				AssertEquals(nameof(registryInfo.PickMethod), "ANY", registryInfo.PickMethod.Description);
				AssertEquals(nameof(registryInfo.PickMethod), true, registryInfo.PickMethod.IsDefault);

				AssertEquals(nameof(registryInfo.ClientCode), client.OH_Code, registryInfo.ClientCode);
				AssertEquals(nameof(registryInfo.UOMType), UOMPackTypesList.Codes.SplitCase, registryInfo.UOMType);
				AssertEquals(nameof(registryInfo.EnableErrorAudio), true, registryInfo.EnableErrorAudio);
				AssertEquals(nameof(registryInfo.Language), SharedConstants.Languages.English, registryInfo.Language);
				AssertEquals(nameof(registryInfo.EquipmentRegistrationNumber), equipment.RQ_Registration, registryInfo.EquipmentRegistrationNumber);
				AssertEquals(nameof(registryInfo.PickArea), pickArea.WA_Name, registryInfo.PickArea.Name);
				AssertEquals(nameof(registryInfo.PickArea), pickArea.WA_NameMultilingual, registryInfo.PickArea.Description);
				AssertEquals(nameof(registryInfo.PickArea), pickArea.PK.ToGuid(), registryInfo.PickArea.AreaPK);
				AssertEquals(nameof(registryInfo.Printer), printer.SQ_DisplayName, registryInfo.Printer.Name);
				AssertEquals(nameof(registryInfo.Printer), printer.PK, registryInfo.Printer.PK);
			});
		}

		public void TestCreateOrLoadUserPreferredSettings_ConcurrencyError()
		{
			var warehouse = Helper.CreateWarehouse("WH1");
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			staff.GS_IsDevice = true;
			staff.GS_WorkingLanguage = SharedConstants.Languages.English;

			var equipment1 = Helper.CreateEquipment("TR1", 5m, Constants.Weight.Kilograms, 5m, Constants.Volume.Litre);
			var equipment2 = Helper.CreateEquipment("TR2", 5m, Constants.Weight.Kilograms, 5m, Constants.Volume.Litre);

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
				var registry = Helper.CreateWhsRFRegistry(staff, warehouse);
				registry.WRR_OH_Client = client.PK;
				registry.WRR_UOMPackType = UOMPackTypesList.Codes.SplitCase;
				registry.WRR_EnableWarehouseErrorAudio = true;
				registry.WRR_RQ_LastUsedEquipment = equipment2.PK;
				registry.WRR_PickGroupSequence = pickGroupCollection[0].PickSequence;
				registry.WRR_PickMethodCode = pickMethods[0].Code;
				registry.WRR_WA_PickingArea = pickArea.PK;
				registry.WRR_SQ_Printer = printer.PK;

				Helper.Factory.Save();

				var webService = GetNewWebService();
				webService.SecurityHeader.UserName = staff.GS_Code;
				webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

				var innerException = new Exception();
				var exception = new ZSaveConcurrencyException(new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)registry).Row, Db.Connection), webService.Factory);
				webService.Factory.Saving += f => throw exception;

				var response = webService.CreateOrLoadUserPreferredSettings(equipment1.RQ_Registration);
				AssertBusinessValidationError(webService, "Unable to save user registry as another user made changes. Please refresh and try again.", response);
				AssertNull(response.RFRegistryInfo);

				var dbRegistry = Helper.Factory.Load<WhsRFRegistry>(new ZQuery(WhsRFRegistrySchema.WRR_GS_NKAssignedTo, staff.GS_Code)).Single();
				AssertNotNull(dbRegistry);
				CombineAssertions(() =>
				{
					AssertEquals(nameof(dbRegistry.WRR_OH_Client), client.PK, dbRegistry.WRR_OH_Client);
					AssertEquals(nameof(dbRegistry.WRR_UOMPackType), UOMPackTypesList.Codes.SplitCase, dbRegistry.WRR_UOMPackType);
					AssertEquals(nameof(dbRegistry.WRR_EnableWarehouseErrorAudio), true, dbRegistry.WRR_EnableWarehouseErrorAudio);
					AssertEquals(nameof(dbRegistry.WRR_RQ_LastUsedEquipment), equipment2.PK, dbRegistry.WRR_RQ_LastUsedEquipment);
					AssertEquals(nameof(dbRegistry.WRR_PickGroupSequence), pickGroupCollection[0].PickSequence, dbRegistry.WRR_PickGroupSequence);
					AssertEquals(nameof(dbRegistry.WRR_PickMethodCode), pickMethods[0].Code, dbRegistry.WRR_PickMethodCode);
					AssertEquals(nameof(dbRegistry.WRR_WA_PickingArea), pickArea.PK, dbRegistry.WRR_WA_PickingArea);
					AssertEquals(nameof(dbRegistry.WRR_SQ_Printer), printer.PK, dbRegistry.WRR_SQ_Printer);
				});
			}
		}

		public void TestCreateOrLoadUserPreferredSettingsTest_UserPreferredlanguage()
		{
			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = Helper.CreateWarehouse("WH1").WW_WarehouseCode;

			var chineseSimplifiedSpeaker = Helper.CreateGlbStaff("AAA", "AAA");
			var englishSpeaker = Helper.CreateGlbStaff("BBB", "BBB");
			var chineseTraditionalSpeaker = Helper.CreateGlbStaff("TTT", "TTT");

			chineseSimplifiedSpeaker.GS_IsDevice = true;
			englishSpeaker.GS_IsDevice = true;
			chineseTraditionalSpeaker.GS_IsDevice = true;

			chineseSimplifiedSpeaker.GS_WorkingLanguage = Core.SharedConstants.Languages.ChineseSimplified;
			englishSpeaker.GS_WorkingLanguage = Core.SharedConstants.Languages.EnglishAmerican;
			chineseTraditionalSpeaker.GS_WorkingLanguage = Core.SharedConstants.Languages.ChineseTraditional;
			Helper.Factory.Save();

			webService.SecurityHeader.UserName = "AAA";
			webService.SecurityHeader.Password = GetEncryptedText(chineseSimplifiedSpeaker.StaffPlainTextPassword);
			var response1 = webService.CreateOrLoadUserPreferredSettings(string.Empty);
			AssertEquals("Found user language in Chinese Simplified successfully.", Core.SharedConstants.Languages.ChineseSimplified, response1.RFRegistryInfo.Language);

			webService.SecurityHeader.UserName = "BBB";
			webService.SecurityHeader.Password = GetEncryptedText(englishSpeaker.StaffPlainTextPassword);
			var response2 = webService.CreateOrLoadUserPreferredSettings(string.Empty);
			AssertEquals("Found user language in English successfully.", Core.SharedConstants.Languages.EnglishAmerican, response2.RFRegistryInfo.Language);

			webService.SecurityHeader.UserName = "TTT";
			webService.SecurityHeader.Password = GetEncryptedText(chineseTraditionalSpeaker.StaffPlainTextPassword);
			var response3 = webService.CreateOrLoadUserPreferredSettings(string.Empty);
			AssertEquals("Found user language in Chinese Traditional successfully.", Core.SharedConstants.Languages.ChineseTraditional, response3.RFRegistryInfo.Language);
		}

		public void TestCreateOrLoadUserPreferredSettings_DBHits()
		{
			var warehouse = Helper.CreateWarehouse("WH1");
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			staff.GS_IsDevice = true;
			staff.GS_WorkingLanguage = SharedConstants.Languages.German;

			var equipment1 = Helper.CreateEquipment("TR1", 5m, Constants.Weight.Kilograms, 5m, Constants.Volume.Litre);
			var equipment2 = Helper.CreateEquipment("TR2", 5m, Constants.Weight.Kilograms, 5m, Constants.Volume.Litre);

			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var client3 = Helper.CreateClient("C3");
			var client4 = Helper.CreateClient("C4");
			var client5 = Helper.CreateClient("C5");

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
				var registry = Helper.CreateWhsRFRegistry(staff, warehouse);
				registry.WRR_OH_Client = client1.PK;
				registry.WRR_UOMPackType = UOMPackTypesList.Codes.SplitCase;
				registry.WRR_EnableWarehouseErrorAudio = true;
				registry.WRR_RQ_LastUsedEquipment = equipment2.PK;
				registry.WRR_PickGroupSequence = pickGroupCollection[0].PickSequence;
				registry.WRR_PickMethodCode = pickMethods[0].Code;
				registry.WRR_WA_PickingArea = pickArea1.PK;
				registry.WRR_SQ_Printer = printer1.PK;

				Helper.Factory.Save();

				var webService = GetNewWebService();
				webService.SecurityHeader.UserName = staff.GS_Code;
				webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

				var expectedDBHits = new Dictionary<string, int>
				{
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ RefEquipmentSchema.Constants.TableName, 1 },
					{ StmPrintQueueSchema.Constants.TableName, 1 },
					{ WhsAreaSchema.Constants.TableName, 1 },
					{ WhsRFRegistrySchema.Constants.TableName, 1 }
				};

				using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
				{
					var response = webService.CreateOrLoadUserPreferredSettings(equipment1.RQ_Registration);
					AssertSuccessfulResponseWithNoErrors(response, webService);
				}
			}
		}
	}
}
