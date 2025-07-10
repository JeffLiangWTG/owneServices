using System;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration.Testing;
using Enterprise.Warehouse.Integration.Warehouse;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class TestLoginSecureService : SecureServiceTestCase<SecureService>
	{
	}

	public abstract class SecureServiceTestCase<T> : SecureServiceBaseTestCase<T>
		where T : SecureService, new()
	{
		#region Test Cases

		public void TestDefaultScanAllSetting_ScanAll() => TestDefaultScanAllSetting(true);
		public void TestDefaultScanAllSetting_ScanQty() => TestDefaultScanAllSetting(false);
		void TestDefaultScanAllSetting(bool setting)
		{
			var webService = (SecureService)GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var whs = helper.CreateWarehouse("WHS");
			whs.WW_ScanAll = setting;
			webService.SecurityHeader.WarehouseCode = "WHS";
			int originalLogCount = webService.Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery);

			AssertNotEquals("Pre-condition", Guid.Empty, GetBranchPK(webService.SecurityHeader.BranchCode));
			AssertNotEquals("Pre-condition", Guid.Empty, GetDepartmentPK(webService.SecurityHeader.DepartmentCode));
			AssertNotEquals("Pre-condition", Guid.Empty, GetActiveUserPK(webService.SecurityHeader.UserName));
			var response = webService.ValidateLogin();
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Should create a licence usage log record if validation is successful", originalLogCount + 1, webService.Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery));
			AssertEquals(setting, response.DefaultScanAll);
		}

		public void TestVolcamEnabled_RegistryEnabled() => TestVolcamEnabled(true);
		public void TestVolcamEnabled_RegistryDisabled() => TestVolcamEnabled(false);
		void TestVolcamEnabled(bool registrySetting)
		{
			AssertEquals("Precondition - RF Volcam false by default", false, WarehouseDataRegistry.Instance.EnableRFVolcam.Value);
			using (WarehouseDataRegistry.Instance.EnableRFVolcam.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting))
			{
				var webService = (SecureService)GetNewWebService();
				var response = webService.ValidateLogin();
				AssertSuccessfulResponse(response, webService);
				AssertEquals("Response RFVolcamEnabled should have correct value", registrySetting, response.RFVolcamEnabled);
			}
		}

		public void TestEnabledFeatureFlags_RegistryEnabled_SchemaRedesign() => TestEnabledFeatureFlags_SchemaRedesign(true);
		public void TestEnabledFeatureFlags_RegistryDisabled_SchemaRedesign() => TestEnabledFeatureFlags_SchemaRedesign(false);
		void TestEnabledFeatureFlags_SchemaRedesign(bool registrySetting)
		{
			using (WhsNonExposedFeatureName.SchemaRedesignChanges.StubFeature(enabled: registrySetting))
			{
				var webService = (SecureService)GetNewWebService();
				var response = webService.ValidateLogin();
				AssertSuccessfulResponse(response, webService);
				AssertCollectionContains("Response EnabledFeatureFlags should have correct value", Convert.ToInt32(registrySetting), response.EnabledFeatureFlags);
			}
		}

		public void TestDefaultScanAllSetting_NoWarehouse()
		{
			var webService = (SecureService)GetNewWebService();
			int originalLogCount = webService.Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery);

			AssertNotEquals("Pre-condition", Guid.Empty, GetBranchPK(webService.SecurityHeader.BranchCode));
			AssertNotEquals("Pre-condition", Guid.Empty, GetDepartmentPK(webService.SecurityHeader.DepartmentCode));
			AssertNotEquals("Pre-condition", Guid.Empty, GetActiveUserPK(webService.SecurityHeader.UserName));
			var response = webService.ValidateLogin();
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Should create a licence usage log record if validation is successful", originalLogCount + 1, webService.Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery));
			AssertEquals(false, response.DefaultScanAll);
		}

		public void TestLicenceCheckPointDuringLogin()
		{
			var webService = (SecureService)GetNewWebService();

			AssertNotEquals(Guid.Empty, GetBranchPK(webService.SecurityHeader.BranchCode));
			AssertNotEquals(Guid.Empty, GetDepartmentPK(webService.SecurityHeader.DepartmentCode));
			AssertNotEquals(Guid.Empty, GetActiveUserPK(webService.SecurityHeader.UserName));

			AssertSuccessfulResponse(webService.ValidateLogin(), webService);
		}

		public void TestValidateLoginWithMissingSecurityHeader()
		{
			var webService = GetNewWebService() as SecureService;
			webService.SecurityHeader = null;
			AssertValidateLogin("Invalid request. Security Header is not provided.", webService);
		}

		/// <summary>
		/// This Test reproduces logging in from the RF device where the initial state is not logged in.
		/// </summary>
		public void TestValidateLoginWithNullUserContext()
		{
			var webService = GetNewWebService();
			Env.ClearUserContext(); //Remove User context i.e. no current user.
			AssertNull("Environment Current User.", Env.CurrentUser);
			AssertEquals("Should not be logged in.", false, Env.IsLoggedIn);

			var response = webService.ValidateLogin();
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Should be logged in.", true, Env.IsLoggedIn);
			AssertEquals("Service logged in as expected.", "CWSupport", Env.CurrentUser.LoginName);
		}

		public void TestValidateLoginWithEmptySecurityHeader()
		{
			var webService = (SecureService)GetNewWebService();

			webService.SecurityHeader.BranchCode = "";
			AssertValidateLogin("Please provide login credentials to use this service.", webService);
			webService.SecurityHeader.BranchCode = null;
			AssertValidateLogin("Please provide login credentials to use this service.", webService);

			PopulateSecurityHeader(webService);
			webService.SecurityHeader.DepartmentCode = "";
			AssertValidateLogin("Please provide login credentials to use this service.", webService);
			webService.SecurityHeader.DepartmentCode = null;
			AssertValidateLogin("Please provide login credentials to use this service.", webService);

			PopulateSecurityHeader(webService);
			webService.SecurityHeader.UserName = "";
			AssertValidateLogin("Please provide login credentials to use this service.", webService);
			webService.SecurityHeader.UserName = null;
			AssertValidateLogin("Please provide login credentials to use this service.", webService);

			PopulateSecurityHeader(webService);
			webService.SecurityHeader.Password = "";
			AssertValidateLogin("Please provide login credentials to use this service.", webService);
			webService.SecurityHeader.Password = null;
			AssertValidateLogin("Please provide login credentials to use this service.", webService);
		}

		public void TestValidateWinCELoginAfterDeprecation()
		{
			var webService = (SecureService)GetNewWebService();

			var today = ZDateTime.Now;
			var deprecationDate = today.AddDays(-7).ToDateTime();
			using (WarehouseDataRegistry.Instance.WinCEApplicationUsageEnabledUntil.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, deprecationDate))
			{
				PopulateSecurityHeader(webService);
				webService.SecurityHeader.IsAndroidDevice = false;
				webService.SecurityHeader.DeviceVersion = new DataService().SystemVersion();
				AssertValidateLogin("Windows CE mobile devices are no longer supported. Please migrate to Android solutions to continue supporting Warehouse.RF operations in your warehouse.", webService);
			}
		}

		public void TestValidateWinCELoginBeforeDeprecation()
		{
			var webService = (SecureService)GetNewWebService();

			var today = ZDateTime.Now;
			var deprecationDate = today.AddDays(7).ToDateTime();
			using (WarehouseDataRegistry.Instance.WinCEApplicationUsageEnabledUntil.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, deprecationDate))
			{
				PopulateSecurityHeader(webService);
				webService.SecurityHeader.IsAndroidDevice = false;
				webService.SecurityHeader.DeviceVersion = new DataService().SystemVersion();
				AssertSuccessfulResponse(webService.ValidateLogin(), webService);
			}
		}

		public void TestValidateAndroidLoginAfterDeprecation()
		{
			var webService = (SecureService)GetNewWebService();

			var today = ZDateTime.Now;
			var deprecationDate = today.AddDays(-7).ToDateTime();
			using (WarehouseDataRegistry.Instance.WinCEApplicationUsageEnabledUntil.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, deprecationDate))
			{
				PopulateSecurityHeader(webService);

				var mockSupporter = new Mock<IRFVersionSupporter>();
				mockSupporter.Setup(v => v.GetAndroidWebServiceVersion(It.IsAny<string>())).Returns(webService.SecurityHeader.DeviceVersion);
				ObjectFactory.Substitute(mockSupporter.Object);

				webService.SecurityHeader.IsAndroidDevice = true;
				AssertSuccessfulResponse(webService.ValidateLogin(), webService);
			}
		}

		public void TestValidateLoginWithValidSecurityKey()
		{
			var webService = (SecureService)GetNewWebService();
			var originalLogCount = webService.Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery);

			webService.SecurityHeader.SecurityKey = Utilities.GenerateSecurityToken(webService.SecurityHeader.UserName, webService.SecurityHeader.Password, webService.SecurityHeader.BranchCode, webService.SecurityHeader.DepartmentCode);
			var response = webService.ValidateLogin();
			AssertNotNull(response);
			AssertEquals(webService.SecurityHeader.SecurityKey, response.SecurityKey);
			AssertEquals("Should not create licence usage log record if security key is valid", originalLogCount, webService.Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery));
		}

		public void TestValidateWithInvalidBranch()
		{
			var webService = (SecureService)GetNewWebService();
			webService.SecurityHeader.BranchCode = "TSTBNCH";

			var originalLogCount = webService.Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery);

			AssertNotEquals("Pre-condition", Guid.Empty, GetDepartmentPK(webService.SecurityHeader.DepartmentCode));
			AssertNotEquals("Pre-condition", Guid.Empty, GetActiveUserPK(webService.SecurityHeader.UserName));
			AssertEquals("Pre-condition", Guid.Empty, GetBranchPK(webService.SecurityHeader.BranchCode));
			AssertValidateLogin("Issue loading context for the warehouse. Please reload the application.", webService);
			AssertEquals("Should not create licence usage log record if validation not successful", originalLogCount, webService.Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery));
		}

		public void TestValidateWithInvalidDepartment()
		{
			var webService = (SecureService)GetNewWebService();
			webService.SecurityHeader.DepartmentCode = "TSTDPMNT";

			var originalLogCount = webService.Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery);

			AssertNotEquals("Pre-condition", Guid.Empty, GetBranchPK(webService.SecurityHeader.BranchCode));
			AssertNotEquals("Pre-condition", Guid.Empty, GetActiveUserPK(webService.SecurityHeader.UserName));
			AssertEquals("Pre-condition", Guid.Empty, GetDepartmentPK(webService.SecurityHeader.DepartmentCode));
			AssertValidateLogin("Issue loading context for the warehouse. Please reload the application.", webService);
			AssertEquals("Should not create licence usage log record if validation not successful", originalLogCount, webService.Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery));
		}

		public void TestValidateWithInvalidUserName()
		{
			var webService = (SecureService)GetNewWebService();
			webService.SecurityHeader.UserName = "TSTUSER12345";

			var originalLogCount = webService.Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery);

			AssertNotEquals("Pre-condition", Guid.Empty, GetBranchPK(webService.SecurityHeader.BranchCode));
			AssertNotEquals("Pre-condition", Guid.Empty, GetDepartmentPK(webService.SecurityHeader.DepartmentCode));
			AssertEquals("Pre-condition", Guid.Empty, GetActiveUserPK(webService.SecurityHeader.UserName));
			AssertValidateLogin("You must enter a correct user name and/or password. Please try again.", webService);
			AssertEquals("Should not create licence usage log record if validation not successful", originalLogCount, webService.Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery));
		}

		public void TestValidateWithInvalidPassword()
		{
			var webService = (SecureService)GetNewWebService();
			webService.SecurityHeader.Password = GetEncryptedText("TEST PASSWORD");

			int originalLogCount = webService.Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery);

			AssertNotEquals("Pre-condition", Guid.Empty, GetBranchPK(webService.SecurityHeader.BranchCode));
			AssertNotEquals("Pre-condition", Guid.Empty, GetDepartmentPK(webService.SecurityHeader.DepartmentCode));
			AssertNotEquals("Pre-condition", Guid.Empty, GetActiveUserPK(webService.SecurityHeader.UserName));
			AssertValidateLogin("The CWSupport token provided is invalid.", webService);
			AssertEquals("Should not create licence usage log record if validation not successful", originalLogCount, webService.Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery));
		}

		[NUnit.Framework.TestDate(2007, 10, 10, 10, 10, 10, 100)]
		public void TestValidateLogin()
		{
			var webService = (SecureService)GetNewWebService();
			int originalLogCount = webService.Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery);

			AssertNotEquals("Pre-condition", Guid.Empty, GetBranchPK(webService.SecurityHeader.BranchCode));
			AssertNotEquals("Pre-condition", Guid.Empty, GetDepartmentPK(webService.SecurityHeader.DepartmentCode));
			AssertNotEquals("Pre-condition", Guid.Empty, GetActiveUserPK(webService.SecurityHeader.UserName));
			AssertSuccessfulResponse(webService.ValidateLogin(), webService);
			AssertEquals("Should create a licence usage log record if validation is successful", originalLogCount + 1, webService.Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery));
		}

		public void TestValidateLogin_DeviceVersionDifferentFromServiceVersion_CheckAndroidVersionTrue_AndroidDevice()
		{
			TestValidateLogin_DeviceVersionDifferentFromServiceVersionCore(
				checkAndroidVersion: true,
				expectedErrorType: ErrorTypes.UpgradeRequired,
				expectedErrorMessage: "Device version and System version does not match. Upgrade required.",
				isAndroidDevice: true);
		}

		public void TestValidateLogin_DeviceVersionDifferentFromServiceVersion_CheckAndroidVersionFalse_AndroidDevice()
		{
			TestValidateLogin_DeviceVersionDifferentFromServiceVersionCore(
				checkAndroidVersion: false,
				expectedErrorType: ErrorTypes.None,
				expectedErrorMessage: null,
				isAndroidDevice: true);
		}

		public void TestValidateLogin_DeviceVersionDifferentFromServiceVersion_CheckAndroidVersionTrue_NotAndroidDevice()
		{
			var today = ZDateTime.Now;
			var deprecationDate = today.AddYears(1).ToDateTime();
			using (WarehouseDataRegistry.Instance.WinCEApplicationUsageEnabledUntil.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, deprecationDate))
			{
				TestValidateLogin_DeviceVersionDifferentFromServiceVersionCore(
				checkAndroidVersion: true,
				expectedErrorType: ErrorTypes.UpgradeRequired,
				expectedErrorMessage: "Device version and System version does not match. Upgrade required.",
				isAndroidDevice: false);
			}
		}

		public void TestValidateLogin_DeviceVersionDifferentFromServiceVersion_CheckAndroidVersionFalse_NotAndroidDevice()
		{
			var today = ZDateTime.Now;
			var deprecationDate = today.AddYears(1).ToDateTime();
			using (WarehouseDataRegistry.Instance.WinCEApplicationUsageEnabledUntil.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, deprecationDate))
			{
				TestValidateLogin_DeviceVersionDifferentFromServiceVersionCore(
				checkAndroidVersion: false,
				expectedErrorType: ErrorTypes.UpgradeRequired,
				expectedErrorMessage: "Device version and System version does not match. Upgrade required.",
				isAndroidDevice: false);
			}
		}

		public void TestValidateLogin_DeviceVersionDifferentFromServiceVersionCore(bool checkAndroidVersion, ErrorTypes expectedErrorType, string expectedErrorMessage, bool isAndroidDevice)
		{
			using (WarehouseDataRegistry.Instance.CheckAndroidDeviceVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryValue: checkAndroidVersion))
			{
				var webService = (SecureService)GetNewWebService();
				webService.SecurityHeader.DeviceVersion = new DataService().WinCEWebServiceVersion + ".111"; // Device version doesn't match with system version
				webService.SecurityHeader.SecurityKey = Utilities.GenerateSecurityToken(webService.SecurityHeader.UserName, webService.SecurityHeader.Password, webService.SecurityHeader.BranchCode, webService.SecurityHeader.DepartmentCode);
				webService.SecurityHeader.IsAndroidDevice = isAndroidDevice;
				var response = webService.ValidateLogin();
				AssertEquals(expectedErrorType, response.Error);
				AssertEquals(expectedErrorMessage, response.ErrorMessage);
			}
		}

		public void TestValidateLogin_DeviceVersionMatchWithServiceVersion()
		{
			var webService = (SecureService)GetNewWebService();
			webService.SecurityHeader.DeviceVersion = new DataService().AndroidWebServiceVersion; // Device version match with system version
			webService.SecurityHeader.SecurityKey = Utilities.GenerateSecurityToken(webService.SecurityHeader.UserName, webService.SecurityHeader.Password, webService.SecurityHeader.BranchCode, webService.SecurityHeader.DepartmentCode);
			var response = webService.ValidateLogin();
			AssertEquals(ErrorTypes.None, response.Error);
			AssertNullOrEmpty(response.ErrorMessage);
		}

		#region TestDeviceTaskManagementSupport

		public void TestDeviceTaskManagementSupportWinCEEnableTM2()
		{
			TestDeviceTaskManagementSupportCore(false, true, ErrorTypes.BusinessValidationError, "Warehouses using Task Management are not supported on Windows CE devices.");
		}

		public void TestDeviceTaskManagementSupportWinCEDisableTM2()
		{
			TestDeviceTaskManagementSupportCore(false, false, ErrorTypes.None, null);
		}

		public void TestDeviceTaskManagementSupportAndroidEnableTM2()
		{
			TestDeviceTaskManagementSupportCore(true, true, ErrorTypes.None, null);
		}

		public void TestDeviceTaskManagementSupportAndroidDisableTM2()
		{
			TestDeviceTaskManagementSupportCore(true, false, ErrorTypes.None, null);
		}

		void TestDeviceTaskManagementSupportCore(bool isAndroidDevice, bool enabledTaskManagement, ErrorTypes expectedErrorType, string expectedErrorMessage)
		{
			var webService = (SecureService)GetNewWebService();
			PopulateSecurityHeader(webService);

			var helper = new WhsTestHelperFunctions(webService.Factory);
			var whs = helper.CreateWarehouse("WHS");
			webService.SecurityHeader.WarehouseCode = "WHS";
			if (enabledTaskManagement)
			{
				var releaseGroup = helper.CreateReleaseGroup("RG1", "RG1");
				whs.WW_GG_ReleaseGroup = releaseGroup.PK;
			}

			webService.SecurityHeader.IsAndroidDevice = isAndroidDevice;
			webService.SecurityHeader.DeviceVersion = isAndroidDevice
				? new DataService().AndroidWebServiceVersion
				: new DataService().WinCEWebServiceVersion;

			var response = webService.ValidateLogin();
			CombineAssertions(() =>
			{
				AssertEquals(expectedErrorType, response.Error);
				AssertEquals(expectedErrorMessage, response.ErrorMessage);
			});
		}

		#endregion

		#endregion

		#region Implementation

		Guid GetActiveUserPK(string username)
		{
			var result = Guid.Empty;
			using (var reader = Db.Connection.Command("select GS_PK from dbo.GlbStaff where GS_IsActive = 1 and GS_LoginName = '" + username + "'").ExecuteReader())        // It is cheaper and quicker to bypass using a BusinessObjectFactory here
			{
				if (reader.Read())
				{
					result = (Guid)reader[GlbStaffSchema.PK.Name];
				}
			}
			return result;
		}

		Guid GetDepartmentPK(string departmentCode)
		{
			var result = Guid.Empty;
			using (var reader = Db.Connection.Command("select GE_PK from dbo.GlbDepartment where GE_Code = '" + departmentCode + "'").ExecuteReader())      // It is cheaper and quicker to bypass using a BusinessObjectFactory here
			{
				if (reader.Read())
				{
					result = (Guid)reader[GlbDepartmentSchema.PK.Name];
				}
			}
			return result;
		}

		Guid GetBranchPK(string branchCode)
		{
			var result = Guid.Empty;
			using (var reader = Db.Connection.Command("select GB_PK from dbo.GlbBranch where GB_Code = '" + branchCode + "'").ExecuteReader())      // It is cheaper and quicker to bypass using a BusinessObjectFactory here
			{
				if (reader.Read())
				{
					result = (Guid)reader[GlbBranchSchema.PK.Name];
				}
			}
			return result;
		}

		void AssertValidateLogin(string expectedMessage, SecureService webService)
		{
			var response = webService.ValidateLogin();
			AssertEquals(expectedMessage, response.ErrorMessage);
			AssertEquals(ErrorTypes.LoginFailed, response.Error);
		}

		ZQuery LicenceUsageLogQuery
		{
			get
			{
				if (licenceUsageLogQuery == null)
				{
					licenceUsageLogQuery = new ZQuery(StmActivityLogSchema.S7_ControllerID, SQLComparisonOperator.StartsWith, LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString());
					licenceUsageLogQuery.AddToFilter(StmActivityLogSchema.S7_FormCaption, SQLComparisonOperator.Equal, Env.Licence.RFScannerManager.Name);
				}

				return licenceUsageLogQuery;
			}
		}

		ZQuery licenceUsageLogQuery;

		#endregion
	}
}
