using System;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsDataServiceTestCase : DataServiceTestCase
	{
		#region TestGetAvailableWarehouses

		public void TestGetAvailableWarehouses()
		{
			var webService = (WhsDataService)GetNewWebService();
			// setup warehouses
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctionsEnv(factory);
			var company = factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var branch1 = company.Branches.AddNew();
			var branch2 = company.Branches.AddNew();
			branch1.GB_Code = "ASD";
			branch1.GB_RN_NKCountryCode = "AU";
			branch2.GB_Code = "SYG";
			branch2.GB_RN_NKCountryCode = "US";
			var warehouse1 = helper.CreateWarehouse("Warehouse", "WHS", "A");
			var warehouse2 = helper.CreateWarehouse("Beach Warehouse", "BEA", "A");
			var warehouse3 = helper.CreateWarehouse("Transit Warehouse", "TRA", "A");
			var warehouse4 = helper.CreateWarehouse("FTZ Warehouse", "FRR", "A");
			var warehouse5 = helper.CreateWarehouse("Container Yard Warehouse", "CYD", "A");
			warehouse1.WW_GB_RelatedCompanyBranch = branch1.PK;
			warehouse2.WW_GB_RelatedCompanyBranch = branch2.PK;
			warehouse3.WW_GB_RelatedCompanyBranch = branch2.PK;
			warehouse4.WW_GB_RelatedCompanyBranch = branch1.PK;
			warehouse5.WW_GB_RelatedCompanyBranch = branch1.PK;
			warehouse3.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			warehouse4.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			warehouse5.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			factory.Save();

			// Assert correct warehouses are returned (Transit Warehouses should be filtered out).
			var warehouseResponse = webService.GetAvailableWarehouses();
			AssertEquals(ErrorTypes.None, warehouseResponse.Error);
			AssertContainsExactElementsInAnyOrder(new[] { "WHS|Warehouse|ASD|AU", "BEA|Beach Warehouse|SYG|US", "FRR|FTZ Warehouse|ASD|AU" },
				warehouseResponse.WarehouseInfos.Select(w => $"{w.Code}|{w.Name}|{w.BranchCode}|{w.CountryCode}"));
		}

		#endregion

		#region TestGetAvailableWarehouses_IgnoresVirtualWarehouses

		public void TestGetAvailableWarehouses_IgnoresVirtualWarehouses()
		{
			var webService = (WhsDataService)GetNewWebService();
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctionsEnv(factory);
			var branch1 = helper.CreateGlbBranch("ASD");
			branch1.GB_RN_NKCountryCode = "AU";
			var branch2 = helper.CreateGlbBranch("SYG");
			branch2.GB_RN_NKCountryCode = "US";
			var warehouse1 = helper.CreateWarehouse("WAREHOUSES1", "WH1", "A");
			var warehouse2 = helper.CreateWarehouse("Beach Warehouse", "BEA", "A");
			warehouse1.WW_GB_RelatedCompanyBranch = branch1.PK;
			warehouse2.WW_GB_RelatedCompanyBranch = branch2.PK;
			warehouse2.WW_IsVirtualWarehouse = true;
			factory.Save();

			var warehouseResponse = webService.GetAvailableWarehouses();
			AssertEquals(ErrorTypes.None, warehouseResponse.Error);
			AssertContainsExactElementsInAnyOrder(new[] { "WH1|WAREHOUSES1|ASD|AU" },
				warehouseResponse.WarehouseInfos.Select(w => $"{w.Code}|{w.Name}|{w.BranchCode}|{w.CountryCode}"));
		}

		#endregion

		#region TestGetAvailableWarehouses_IgnoresVirtualWarehouses_RegistryItemEnabled

		[TestDate(2023, 1, 1)]
		public void TestGetAvailableWarehouses_IgnoresVirtualWarehouses_RegistryItemEnabled()
		{
			using (WarehouseDataRegistry.Instance.VirtualWarehouseOnRFEnabledUntil.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2023, 6, 1)))
			{
				var webService = (WhsDataService)GetNewWebService();
				var factory = new BusinessObjectFactory();
				var helper = new WhsTestHelperFunctionsEnv(factory);
				var branch1 = helper.CreateGlbBranch("ASD");
				branch1.GB_RN_NKCountryCode = "AU";
				var branch2 = helper.CreateGlbBranch("SYG");
				branch2.GB_RN_NKCountryCode = "US";
				var warehouse1 = helper.CreateWarehouse("WAREHOUSES1", "WH1", "A");
				var warehouse2 = helper.CreateWarehouse("Beach Warehouse", "BEA", "A");
				warehouse1.WW_GB_RelatedCompanyBranch = branch1.PK;
				warehouse2.WW_GB_RelatedCompanyBranch = branch2.PK;
				warehouse2.WW_IsVirtualWarehouse = true;
				factory.Save();

				var warehouseResponse = webService.GetAvailableWarehouses();
				AssertEquals(ErrorTypes.None, warehouseResponse.Error);
				AssertContainsExactElementsInAnyOrder(new[] { "WH1|WAREHOUSES1|ASD|AU", "BEA|Beach Warehouse|SYG|US" },
					warehouseResponse.WarehouseInfos.Select(w => $"{w.Code}|{w.Name}|{w.BranchCode}|{w.CountryCode}"));
			}
		}

		#endregion

		#region TestGetAvailableWarehouses_IgnoresVirtualWarehouses_RegistryItemEnabled_Expired

		[TestDate(2023, 1, 1)]
		public void TestGetAvailableWarehouses_IgnoresVirtualWarehouses_RegistryItemEnabled_Expired()
		{
			using (WarehouseDataRegistry.Instance.VirtualWarehouseOnRFEnabledUntil.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2022, 6, 1)))
			{
				var webService = (WhsDataService)GetNewWebService();
				var factory = new BusinessObjectFactory();
				var helper = new WhsTestHelperFunctionsEnv(factory);
				var branch1 = helper.CreateGlbBranch("ASD");
				branch1.GB_RN_NKCountryCode = "AU";
				var branch2 = helper.CreateGlbBranch("SYG");
				branch2.GB_RN_NKCountryCode = "US";
				var warehouse1 = helper.CreateWarehouse("WAREHOUSES1", "WH1", "A");
				var warehouse2 = helper.CreateWarehouse("Beach Warehouse", "BEA", "A");
				warehouse1.WW_GB_RelatedCompanyBranch = branch1.PK;
				warehouse2.WW_GB_RelatedCompanyBranch = branch2.PK;
				warehouse2.WW_IsVirtualWarehouse = true;
				factory.Save();

				var warehouseResponse = webService.GetAvailableWarehouses();
				AssertEquals(ErrorTypes.None, warehouseResponse.Error);
				AssertContainsExactElementsInAnyOrder(new[] { "WH1|WAREHOUSES1|ASD|AU" },
					warehouseResponse.WarehouseInfos.Select(w => $"{w.Code}|{w.Name}|{w.BranchCode}|{w.CountryCode}"));
			}
		}

		#endregion

		#region TestGetAvailableWarehouses_DeviceVersionDifferentFromServiceVersion

		public void TestGetAvailableWarehouses_DeviceVersionDifferentFromServiceVersion_CheckAndroidVersionTrue_AndroidDevice()
		{
			TestGetAvailableWarehouses_DeviceVersionDifferentFromServiceVersionCore(
				checkAndroidVersion: true,
				expectedErrorType: ErrorTypes.UpgradeRequired,
				expectedErrorMessage: "Device version and System version does not match. Upgrade required.",
				isAndroidDevice: true);
		}

		public void TestGetAvailableWarehouses_DeviceVersionDifferentFromServiceVersion_CheckAndroidVersionFalse_AndroidDevice()
		{
			TestGetAvailableWarehouses_DeviceVersionDifferentFromServiceVersionCore(
				checkAndroidVersion: false,
				expectedErrorType: ErrorTypes.None,
				expectedErrorMessage: null,
				isAndroidDevice: true);
		}

		public void TestGetAvailableWarehouses_DeviceVersionDifferentFromServiceVersion_CheckAndroidVersionTrue_NotAndroidDevice()
		{
			TestGetAvailableWarehouses_DeviceVersionDifferentFromServiceVersionCore(
				checkAndroidVersion: true,
				expectedErrorType: ErrorTypes.UpgradeRequired,
				expectedErrorMessage: "Device version and System version does not match. Upgrade required.",
				isAndroidDevice: false);
		}

		public void TestGetAvailableWarehouses_DeviceVersionDifferentFromServiceVersion_CheckAndroidVersionFalse_NotAndroidDevice()
		{
			TestGetAvailableWarehouses_DeviceVersionDifferentFromServiceVersionCore(
				checkAndroidVersion: false,
				expectedErrorType: ErrorTypes.UpgradeRequired,
				expectedErrorMessage: "Device version and System version does not match. Upgrade required.",
				isAndroidDevice: false);
		}

		public void TestGetAvailableWarehouses_DeviceVersionDifferentFromServiceVersionCore(bool checkAndroidVersion, ErrorTypes expectedErrorType, string expectedErrorMessage, bool isAndroidDevice)
		{
			using (WarehouseDataRegistry.Instance.CheckAndroidDeviceVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryValue: checkAndroidVersion))
			{
				var webService = (WhsDataService)GetNewWebService();
				webService.SecurityHeader.DeviceVersion = new DataService().WinCEWebServiceVersion + ".111";
				webService.SecurityHeader.IsAndroidDevice = isAndroidDevice;
				var response = webService.GetAvailableWarehouses();
				AssertEquals(expectedErrorType, response.Error);
				AssertEquals(expectedErrorMessage, response.ErrorMessage);
			}
		}

		#endregion

		#region TestGetAvailableWarehouses_DeviceVersionMatchWithServiceVersion

		public void TestGetAvailableWarehouses_DeviceVersionMatchWithServiceVersion()
		{
			var webService = (WhsDataService)GetNewWebService();
			var response = webService.GetAvailableWarehouses();
			AssertEquals(ErrorTypes.None, response.Error);
			AssertNullOrEmpty(response.ErrorMessage);
		}

		#endregion

		#region TestGetAvailableWarehouses_LoginFailed

		public void TestGetAvailableWarehouses_LoginFailed()
		{
			var webService = (WhsDataService)GetNewWebService();
			webService.SecurityHeader = null;
			var response = webService.GetAvailableWarehouses();
			AssertEquals(ErrorTypes.LoginFailed, response.Error);
			AssertEquals("Invalid request. Security Header is not provided.", response.ErrorMessage);
		}

		#endregion

		#region TestVerifyEquipmentRegistrationCode

		public void TestVerifyEquipmentRegistrationCode()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctionsEnv(factory);
			var company = factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "ASD";

			var equipment = factory.NewWithValidTestData<RefEquipment>();
			equipment.RQ_Registration = "T1";
			factory.Save();
			var webService = (WhsDataService)GetNewWebService();
			var validResponse = webService.VerifyEquipmentRegistrationCode("T1", false);
			AssertEquals(ErrorTypes.None, validResponse.Error);

			var invalidResponse = webService.VerifyEquipmentRegistrationCode("E123", false);
			AssertEquals(ErrorTypes.BusinessValidationError, invalidResponse.Error);

			var responseForEmptyRegistrationCode = webService.VerifyEquipmentRegistrationCode("", false);
			AssertEquals(ErrorTypes.None, responseForEmptyRegistrationCode.Error);
		}

		#endregion

		#region TestVerifyEquipmentRegistrationCode_EquipmentWithPackTypeConstraint

		public void TestVerifyEquipmentRegistrationCode_EquipmentWithPackTypeConstraint()
		{
			var factory = new BusinessObjectFactory();
			var company = factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "ASD";

			CreateEquipment(factory, "E1", Core.Constants.PkgUnit.Carton); // with pack type constraint
			CreateEquipment(factory, "E2", "");
			factory.Save();

			var webService = (WhsDataService)GetNewWebService();
			var responseForPackTypeConstaintAndEmptyUOMType = webService.VerifyEquipmentRegistrationCode("E1", false);
			AssertEquals("Although E1 has pack type constraint, UOM type is not specified therefore should not have an error.",
				ErrorTypes.None, responseForPackTypeConstaintAndEmptyUOMType.Error);

			var responseForPackTypeConstaintAndNonEmptyUOMType = webService.VerifyEquipmentRegistrationCode("E1", true);
			AssertEquals("When equipment has a pack type constraint and UOM type is specified then there must be a validation error.,",
				ErrorTypes.BusinessValidationError, responseForPackTypeConstaintAndNonEmptyUOMType.Error);
			AssertEquals("You cannot set an equipment with packtype constraint when UOM type is set.",
				"Equipment Reference 'E1' has a pack type constraint. Therefore cannot be used with a UOM Type.", responseForPackTypeConstaintAndNonEmptyUOMType.ErrorMessage);

			var responseForNonPackTypeConstaintAndEmptyUOMType = webService.VerifyEquipmentRegistrationCode("E2", false);
			AssertEquals("Equipment does not have a pack type and UOM is not used therefore must not have errors.",
				ErrorTypes.None, responseForNonPackTypeConstaintAndEmptyUOMType.Error);

			var responseForNonPackTypeConstaintAndNonEmptyUOMType = webService.VerifyEquipmentRegistrationCode("E2", true);
			AssertEquals("Equipment does not have a pack type therefore must not have errors.",
				ErrorTypes.None, responseForNonPackTypeConstaintAndNonEmptyUOMType.Error);
		}

		static void CreateEquipment(BusinessObjectFactory factory, string rego, string packType)
		{
			var equipment = factory.NewWithValidTestData<RefEquipment>();
			equipment.RQ_Registration = rego;
			equipment.RQ_F3_NKPackType = packType;
		}

		#endregion

		public void TestVerifyEquipmentRegistrationCode_UsesDbConnectionCorrectly()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctionsEnv(factory);
			var company = factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "ASD";

			var equipment = factory.NewWithValidTestData<RefEquipment>();
			equipment.RQ_Registration = "T1";
			factory.Save();
			var webService = (WhsDataService)GetNewWebService();

			var lastError = string.Empty;
			var thread = new Thread(() =>
			{
				ErrorReporter.Clear();

				webService.VerifyEquipmentRegistrationCode("T1", false);

				lastError = ErrorReporter.LastMessageReported;
				ErrorReporter.Clear();
			});
			thread.Start();
			thread.Join(1000);

			// Should not be "Attempt to use Db.Connection without using Db.DisposableActionForDbConnection()"
			AssertEquals(string.Empty, lastError);
		}

		#region Implementation

		protected override DataService GetNewWebService()
		{
			var webService = new WhsDataService
			{
				// Device version match with system version
				SecurityHeader = new SecuritySOAPHeader { DeviceVersion = new DataService().WinCEWebServiceVersion }
			};

			webService.SecurityHeader.SecurityKey = Utilities.GenerateSecurityToken(webService.SecurityHeader.UserName, webService.SecurityHeader.Password, webService.SecurityHeader.BranchCode, webService.SecurityHeader.DepartmentCode);

			return webService;
		}

		#endregion
	}
}
