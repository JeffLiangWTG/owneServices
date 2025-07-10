using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Semaphores.Common;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Shared;
using Enterprise.ZArchitecture.Web.Shared.Testing;
using Moq;
using NUnit.Framework;
using WTG.Foundation.Cryptography;

namespace Enterprise.Warehouse.Web.WebService.Common.Testing
{
	class LoginHelperTestCase : TransactionedTestCase
	{
		#region TestCreateUserContextWithDifferentFactory

		public void TestCreateUserContextWithDifferentFactory()
		{
			var response = new WebServiceResponse();

			var securityHeader = CreateSecurityHeader(User.LoginName, EncryptedSupportToken, Branch.Code, Department.Code, "");

			using (MockHttpRequestManager())
			{
				LoginHelper.Validate(response, securityHeader);
				var factoryProvider = Env.CurrentUserContext.User as IFactoryProvider;
				AssertNotEquals(factoryProvider.Factory, Factory);
			}
		}

		#endregion

		#region TestGenerateKey

		[TestDate(2007, 10, 10, 10, 10, 10, 100)]
		public void TestGenerateKey()
		{
			var decrypter = new AESCryptographicProvider(LoginHelper.CryptographyKey.Value, LoginHelper.CryptographyIv.Value);

			var username = "TestUser";
			var password = "TestPassword";
			var branchCode = "USORD";
			var departmentCode = "CHI";
			var now = DateTime.Now;

			string expectedDescryptedKey = username + password + branchCode + departmentCode +
					now.Year.ToString() + now.Month.ToString() + now.Day.ToString() + now.Hour.ToString();

			string generatedKey = Utilities.GenerateSecurityToken(username, password, branchCode, departmentCode);
			AssertNotEquals(expectedDescryptedKey, generatedKey);
			var decryptedKey = Encoding.Unicode.GetString(decrypter.Decrypt(Convert.FromBase64String(generatedKey)));
			AssertEquals(expectedDescryptedKey, decryptedKey);
		}

		#endregion

		#region TestValidate

		#region TestValidate

		[TestDate(2007, 10, 10, 10, 10, 10, 100)]
		[ExpectNoExceptions()]
		public void TestValidate()
		{
			int originalLogCount = Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery);

			AssertNotEquals(Guid.Empty, GetBranchPK(Branch.Code));
			AssertNotEquals(Guid.Empty, GetDepartmentPK(Department.Code));
			AssertNotEquals(Guid.Empty, GetActiveUserPK(User.LoginName));

			var response = new WebServiceResponse();
			AssertEquals("", response.SecurityKey);

			using (MockHttpRequestManager())
			{
				var securityHeader = CreateSecurityHeader(User.LoginName, EncryptedSupportToken, Branch.Code, Department.Code, "");
				LoginHelper.Validate(response, securityHeader);
				AssertNotEquals("", response.SecurityKey);
				AssertEquals(Utilities.GenerateSecurityToken(User.LoginName, EncryptedSupportToken, Branch.Code, Department.Code), response.SecurityKey);
				AssertEquals("Should create a licence usage log record if validation is successful", originalLogCount + 1, Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery));
			}
		}

		#endregion

		#region TestValidate_LogsDeviceID

		[TestDate(2007, 10, 10, 10, 10, 10, 100)]
		[ExpectNoExceptions()]
		public void TestValidate_LogsDeviceID()
		{
			var user = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			user.StaffPlainTextPassword = "P@ssword1";
			user.GS_Code = "JCD";

			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(user, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (MockHttpRequestManager())
			{
				AssertNotEquals(Guid.Empty, GetActiveUserPK(user.LoginName));

				var response = new WebServiceResponse();
				AssertEquals("", response.SecurityKey);

				var deviceID = Guid.NewGuid().ToString();
				var securityHeader = CreateSecurityHeader(user.LoginName, GetEncryptedText(user.StaffPlainTextPassword), Branch.Code, Department.Code, "", deviceID);
				LoginHelper.Validate(response, securityHeader);
				AssertNotEquals("", response.SecurityKey);
				AssertEquals(Utilities.GenerateSecurityToken(user.LoginName, GetEncryptedText(user.StaffPlainTextPassword), Branch.Code, Department.Code), response.SecurityKey);

				var logEntry = Factory.Load<StmActivityLog>(GetLicenceUsageLogWithDeviceIDQuery(deviceID)).SingleOrDefault();
				AssertNotNull("Log Entry should be created", logEntry);
			}
		}

		[TestDate(2007, 10, 10, 10, 10, 10, 100)]
		[ExpectNoExceptions()]
		public void TestValidate_DoesNotLogDeviceIDForSupportUser()
		{
			AssertNotEquals(Guid.Empty, GetBranchPK(Branch.Code));
			AssertNotEquals(Guid.Empty, GetDepartmentPK(Department.Code));
			Assert(User.IsSupportUser);

			var response = new WebServiceResponse();
			AssertEquals("", response.SecurityKey);

			var deviceID = Guid.NewGuid().ToString();
			var securityHeader = CreateSecurityHeader(User.LoginName, EncryptedSupportToken, Branch.Code, Department.Code, "", deviceID);

			using (MockHttpRequestManager())
			{
				LoginHelper.Validate(response, securityHeader);
				AssertNotEquals("", response.SecurityKey);
				AssertEquals(Utilities.GenerateSecurityToken(User.LoginName, EncryptedSupportToken, Branch.Code, Department.Code), response.SecurityKey);

				var logEntry = Factory.Load<StmActivityLog>(GetLicenceUsageLogWithDeviceIDQuery(deviceID)).SingleOrDefault();
				AssertNull("Log Entry for device id should not be created", logEntry);
			}
		}

		#endregion

		#region TestValidate_LogsDeviceModelDetails

		[TestDate(2007, 10, 10, 10, 10, 10, 100)]
		[ExpectNoExceptions()]
		public void TestValidate_LogsDeviceModelDetails()
		{
			var user = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			user.StaffPlainTextPassword = "P@ssword1";
			user.GS_Code = "JCD";

			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(user, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (MockHttpRequestManager())
			{
				AssertNotEquals(Guid.Empty, GetActiveUserPK(user.LoginName));

				var response = new WebServiceResponse();
				AssertEquals("", response.SecurityKey);

				var deviceID = Guid.NewGuid().ToString();
				var deviceDetails = "MAN:Zebra|MDL:TC77|OS:AND";
				var securityHeader = CreateSecurityHeader(user.LoginName, GetEncryptedText(user.StaffPlainTextPassword), Branch.Code, Department.Code, string.Empty, "ABC", deviceModelDetails: deviceDetails);
				LoginHelper.Validate(response, securityHeader);
				AssertNotEquals("", response.SecurityKey);
				AssertEquals(Utilities.GenerateSecurityToken(user.LoginName, GetEncryptedText(user.StaffPlainTextPassword), Branch.Code, Department.Code), response.SecurityKey);

				var logEntry = Factory.LoadTop1<StmActivityLog>(GetLicenceUsageLogWithDeviceIDQuery("ABC"));
				AssertNotNull("Log Entry should be created", logEntry);
				AssertEquals(deviceDetails, logEntry.S7_DeviceDetails);
			}
		}

		[TestDate(2007, 10, 10, 10, 10, 10, 100)]
		[ExpectNoExceptions()]
		public void TestValidate_DoesNotLogDeviceModelDetailsForSupportUser()
		{
			AssertNotEquals(Guid.Empty, GetBranchPK(Branch.Code));
			AssertNotEquals(Guid.Empty, GetDepartmentPK(Department.Code));
			Assert(User.IsSupportUser);

			var response = new WebServiceResponse();
			AssertEquals("", response.SecurityKey);

			var deviceID = Guid.NewGuid().ToString();
			var deviceDetails = "MAN:Zebra|MDL:TC77|OS:AND";
			var securityHeader = CreateSecurityHeader(User.LoginName, EncryptedSupportToken, Branch.Code, Department.Code, "", deviceID, deviceModelDetails: deviceDetails);

			using (MockHttpRequestManager())
			{
				LoginHelper.Validate(response, securityHeader);
				AssertNotEquals("", response.SecurityKey);
				AssertEquals(Utilities.GenerateSecurityToken(User.LoginName, EncryptedSupportToken, Branch.Code, Department.Code), response.SecurityKey);

				var query = LicenceUsageLogQuery;
				query.AddToFilter(StmActivityLogSchema.S7_DeviceDetails, SQLComparisonOperator.Equal, deviceDetails);
				var logEntry = Factory.LoadTop1<StmActivityLog>(query);
				AssertNull("Log Entry with device details should not be created.", logEntry);
			}
		}

		#endregion

		#region TestValidate_LogsDeviceID

		[TestDate(2007, 10, 10, 10, 10, 10, 100)]
		[ExpectNoExceptions()]
		public void TestValidate_LogsHttpConnection()
		{
			TestValidate_LogsHttpConnectionCore(isSecureConnection: false, isRedirectedFromLoadBalancer: false, ConnectionType.HttpConnection);
		}

		[TestDate(2007, 10, 10, 10, 10, 10, 100)]
		[ExpectNoExceptions()]
		public void TestValidate_LogsHttpsConnection_SecureConnection()
		{
			TestValidate_LogsHttpConnectionCore(isSecureConnection: true, isRedirectedFromLoadBalancer: false, ConnectionType.HttpsConnection);
		}

		[TestDate(2007, 10, 10, 10, 10, 10, 100)]
		[ExpectNoExceptions()]
		public void TestValidate_LogsHttpsConnection_RedirectedByLoadBalancer()
		{
			TestValidate_LogsHttpConnectionCore(isSecureConnection: false, isRedirectedFromLoadBalancer: true, ConnectionType.HttpsConnection);
		}

		public void TestValidate_LogsHttpConnectionCore(bool isSecureConnection, bool isRedirectedFromLoadBalancer, ConnectionType expectedConnectionType)
		{
			var user = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			user.StaffPlainTextPassword = "P@ssword1";
			user.GS_Code = "JCD";

			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(user, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (MockHttpRequestManager(isSecureConnection, isRedirectedFromLoadBalancer))
			{
				AssertNotEquals(Guid.Empty, GetActiveUserPK(user.LoginName));

				var response = new WebServiceResponse();
				AssertEquals("", response.SecurityKey);

				var deviceID = Guid.NewGuid().ToString();
				var securityHeader = CreateSecurityHeader(user.LoginName, GetEncryptedText(user.StaffPlainTextPassword), Branch.Code, Department.Code, "", deviceID);
				LoginHelper.Validate(response, securityHeader);

				AssertEquals(Utilities.GenerateSecurityToken(user.LoginName, GetEncryptedText(user.StaffPlainTextPassword), Branch.Code, Department.Code), response.SecurityKey);

				var logEntry = Factory.Load<StmActivityLog>(GetLicenceUsageLogWithDeviceIDQuery(deviceID)).SingleOrDefault();
				AssertNotNull("Log Entry should be created", logEntry);

				AssertEquals(expectedConnectionType, (ConnectionType)(int)logEntry.S7_KeyStrokes);
			}
		}

		public void TestValidate_LogsHttpConnection_EmptyContext()
		{
			// Note that this shouldn't occur in practice, but have included this case to prevent crashes
			var user = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			user.StaffPlainTextPassword = "P@ssword1";
			user.GS_Code = "JCD";

			Factory.Save();

			var connectionTypeSupporterMock = new Mock<IHttpRequestManager>();
			connectionTypeSupporterMock.Setup(c => c.GetHttpContextBase()).Returns((HttpContextBase)null);

			using (Env.SetTemporaryUserContext(new UserContext(user, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (ObjectFactory.Substitute(connectionTypeSupporterMock.Object))
			{
				AssertNotEquals(Guid.Empty, GetActiveUserPK(user.LoginName));

				var response = new WebServiceResponse();
				AssertEquals("", response.SecurityKey);

				var deviceID = Guid.NewGuid().ToString();
				var securityHeader = CreateSecurityHeader(user.LoginName, GetEncryptedText(user.StaffPlainTextPassword), Branch.Code, Department.Code, "", deviceID);
				LoginHelper.Validate(response, securityHeader);

				AssertEquals(Utilities.GenerateSecurityToken(user.LoginName, GetEncryptedText(user.StaffPlainTextPassword), Branch.Code, Department.Code), response.SecurityKey);

				var logEntry = Factory.Load<StmActivityLog>(GetLicenceUsageLogWithDeviceIDQuery(deviceID)).SingleOrDefault();
				AssertNotNull("Log Entry should be created", logEntry);

				AssertEquals(ConnectionType.Undefined, (ConnectionType)(int)logEntry.S7_KeyStrokes);
			}
		}

		#endregion

		#region TestValidate_DBHits

		public void TestValidate_DBHits()
		{
			var response = new WebServiceResponse();
			var securityHeader = CreateSecurityHeader(User.LoginName, "somePassword", Branch.Code, "ZZZ", ""); // make only user & branch correct as we don't want to log in
			int originalCmdCount = TestConnection.ExecutedCommandCount;

			LoginHelper.Validate(response, securityHeader);
			AssertEquals("Should get User, Branch & Department in 1 hit.", originalCmdCount + 1, TestConnection.ExecutedCommandCount);
		}

		#endregion

		#region TestValidate_InvalidBranch

		public void TestValidate_InvalidBranch()
		{
			int originalLogCount = Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery);

			AssertNotEquals(Guid.Empty, GetDepartmentPK(Department.Code));
			AssertNotEquals(Guid.Empty, User.PK);
			AssertEquals(User.PK, GetActiveUserPK(User.LoginName));
			AssertEquals(Guid.Empty, GetBranchPK("TSTBNCH"));

			var securityHeader = CreateSecurityHeader(User.LoginName, EncryptedSupportToken, "TSTBNCH", Department.Code, "");
			var response = new WebServiceResponse();
			LoginHelper.Validate(response, securityHeader);
			AssertEquals(ErrorTypes.LoginFailed, response.Error);
			AssertEquals("Issue loading context for the warehouse. Please reload the application.", response.ErrorMessage);
			AssertEquals("Should not create licence usage log record if validation not successful", originalLogCount, Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery));
		}

		#endregion

		#region TestValidate_InvalidDepartment

		public void TestValidate_InvalidDepartment()
		{
			int originalLogCount = Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery);

			AssertNotEquals(Guid.Empty, GetBranchPK(Branch.Code));
			AssertNotEquals(Guid.Empty, User.PK);
			AssertEquals(User.PK, GetActiveUserPK(User.LoginName));
			AssertEquals(Guid.Empty, GetDepartmentPK("TSTDPMNT"));

			var securityHeader = CreateSecurityHeader(User.LoginName, EncryptedSupportToken, Branch.Code, "TSTDPMNT", "");
			var response = new WebServiceResponse();
			LoginHelper.Validate(response, securityHeader);
			AssertEquals(ErrorTypes.LoginFailed, response.Error);
			AssertEquals("Issue loading context for the warehouse. Please reload the application.", response.ErrorMessage);
			AssertEquals("Should not create licence usage log record if validation not successful", originalLogCount, Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery));
		}

		#endregion

		#region TestValidate_InvalidUserName

		public void TestValidate_InvalidUserName()
		{
			int originalLogCount = Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery);

			AssertNotEquals(Guid.Empty, GetBranchPK(Branch.Code));
			AssertNotEquals(Guid.Empty, GetDepartmentPK(Department.Code));
			AssertEquals(Guid.Empty, GetActiveUserPK("TSTUSER12345"));

			var securityHeader = CreateSecurityHeader("TSTUSER12345", EncryptedSupportToken, Branch.Code, Department.Code, "");
			var response = new WebServiceResponse();
			LoginHelper.Validate(response, securityHeader);
			AssertEquals("You must enter a correct user name and/or password. Please try again.", response.ErrorMessage);
			AssertEquals(ErrorTypes.LoginFailed, response.Error);
			AssertEquals("Should not create licence usage log record if validation not successful", originalLogCount, Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery));
		}

		#endregion

		#region TestValidate_InvalidUserName_WontSendErrorEmail

		public void TestValidate_InvalidUserName_WontSendErrorEmail()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				UserContext.ForceToNotSkipForTest = true;

				AssertNotEquals(Guid.Empty, GetBranchPK(Branch.Code));
				AssertNotEquals(Guid.Empty, GetDepartmentPK(Department.Code));
				AssertEquals(Guid.Empty, GetActiveUserPK("TSTUSER12345"));

				SetUpPostMaster();
				var lastErrorMessage = ErrorReporter.LastMessageReported;
				var securityHeader = CreateSecurityHeader("TSTUSER12345", EncryptedSupportToken, Branch.Code, Department.Code, "");
				var response = new WebServiceResponse();
				LoginHelper.Validate(response, securityHeader);

				AssertEquals(lastErrorMessage, ErrorReporter.LastMessageReported);
				Assert("Error email should not be created", Env.OutgoingMailManager.EmailsCreated.Count == 0);
			}
		}

		#endregion

		#region TestValidate_InvalidPassword

		public void TestValidate_InvalidPassword()
		{
			int originalLogCount = Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery);

			AssertNotEquals(Guid.Empty, GetBranchPK(Branch.Code));
			AssertNotEquals(Guid.Empty, GetDepartmentPK(Department.Code));
			AssertNotEquals(Guid.Empty, User.PK);
			AssertEquals(User.PK, GetActiveUserPK(User.LoginName));
			AssertEquals("This password should not work", false, User.VerifyPassword("TEST PASSWORD"));

			var securityHeader = CreateSecurityHeader(User.LoginName, GetEncryptedText("TEST PASSWORD"), Branch.Code, Department.Code, "");
			var response = new WebServiceResponse();
			LoginHelper.Validate(response, securityHeader);
			AssertEquals("The CWSupport token provided is invalid.", response.ErrorMessage);
			AssertEquals(ErrorTypes.LoginFailed, response.Error);
			AssertEquals("Should not create licence usage log record if validation not successful", originalLogCount, Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery));
		}

		#endregion

		#region TestResourceStringLanguageToUserPreference

		public void TestResourceStringLanguageToUserPreference_WinCEDevices()
		{
			TestResourceStringLanguageToUserPreferenceCore(isAndroidDevice: false);
		}

		public void TestResourceStringLanguageToUserPreference_AndroidDevices()
		{
			TestResourceStringLanguageToUserPreferenceCore(isAndroidDevice: true);
		}

		public void TestResourceStringLanguageToUserPreferenceCore(bool isAndroidDevice)
		{
			var chineseSimplifiedSpeaker = Helper.CreateGlbStaff("AAA", "AAA");
			var chineseTraditionalSpeaker = Helper.CreateGlbStaff("TTT", "TTT");
			var englishSpeaker = Helper.CreateGlbStaff("BBB", "BBB");
			var englishUSSpeaker = Helper.CreateGlbStaff("CCC", "CCC");
			var englishGBSpeaker = Helper.CreateGlbStaff("DDD", "DDD");
			var frenchSpeaker = Helper.CreateGlbStaff("EEE", "EEE");
			var spanishSpeaker = Helper.CreateGlbStaff("FFF", "FFF");
			var germanSpeaker = Helper.CreateGlbStaff("GGG", "GGG");

			chineseSimplifiedSpeaker.GS_IsDevice = true;
			chineseTraditionalSpeaker.GS_IsDevice = true;
			englishSpeaker.GS_IsDevice = true;
			englishUSSpeaker.GS_IsDevice = true;
			englishGBSpeaker.GS_IsDevice = true;
			frenchSpeaker.GS_IsDevice = true;
			spanishSpeaker.GS_IsDevice = true;
			germanSpeaker.GS_IsDevice = true;

			chineseSimplifiedSpeaker.GS_WorkingLanguage = Core.SharedConstants.Languages.ChineseSimplified;
			chineseTraditionalSpeaker.GS_WorkingLanguage = Core.SharedConstants.Languages.ChineseTraditional;
			englishSpeaker.GS_WorkingLanguage = Core.SharedConstants.Languages.English;
			englishUSSpeaker.GS_WorkingLanguage = Core.SharedConstants.Languages.EnglishAmerican;
			englishGBSpeaker.GS_WorkingLanguage = Core.SharedConstants.Languages.EnglishBritish;
			frenchSpeaker.GS_WorkingLanguage = Core.SharedConstants.Languages.French;
			spanishSpeaker.GS_WorkingLanguage = Core.SharedConstants.Languages.Spanish;
			germanSpeaker.GS_WorkingLanguage = Core.SharedConstants.Languages.German;

			Factory.Save();

			var securityHeaderChineseSimplified = CreateSecurityHeader("AAA", chineseSimplifiedSpeaker.StaffPlainTextPassword, "Device1");
			securityHeaderChineseSimplified.IsAndroidDevice = isAndroidDevice;
			var responseChineseSimplified = new WebServiceResponse();

			var securityHeaderEnglish = CreateSecurityHeader("BBB", englishSpeaker.StaffPlainTextPassword, "Device2");
			securityHeaderEnglish.IsAndroidDevice = isAndroidDevice;
			var responseEnglish = new WebServiceResponse();

			var securityHeaderEnglishUS = CreateSecurityHeader("CCC", englishUSSpeaker.StaffPlainTextPassword, "Device3");
			securityHeaderEnglishUS.IsAndroidDevice = isAndroidDevice;
			var responseEnglishUS = new WebServiceResponse();

			var securityHeaderEnglishGB = CreateSecurityHeader("DDD", englishGBSpeaker.StaffPlainTextPassword, "Device4");
			securityHeaderEnglishGB.IsAndroidDevice = isAndroidDevice;
			var responseEnglishGB = new WebServiceResponse();

			var securityHeaderFrench = CreateSecurityHeader("EEE", frenchSpeaker.StaffPlainTextPassword, "Device5");
			securityHeaderFrench.IsAndroidDevice = isAndroidDevice;
			var responseFrench = new WebServiceResponse();

			var securityHeaderSpanish = CreateSecurityHeader("FFF", spanishSpeaker.StaffPlainTextPassword, "Device6");
			securityHeaderSpanish.IsAndroidDevice = isAndroidDevice;
			var responseSpanish = new WebServiceResponse();

			var securityHeaderGerman = CreateSecurityHeader("GGG", germanSpeaker.StaffPlainTextPassword, "Device7");
			securityHeaderGerman.IsAndroidDevice = isAndroidDevice;
			var responseGerman = new WebServiceResponse();

			var securityHeaderChineseTraditional = CreateSecurityHeader("TTT", chineseTraditionalSpeaker.StaffPlainTextPassword, "Device8");
			securityHeaderChineseTraditional.IsAndroidDevice = isAndroidDevice;
			var responseChineseTradtional = new WebServiceResponse();

			AssertEquals("Default language is English.", Core.SharedConstants.Languages.English, ObjectFactory.Get<IResourceStrings>().CurrentLanguage);

			LoginHelper.Validate(responseChineseSimplified, securityHeaderChineseSimplified);
			AssertEquals("Validate successfully.", ErrorTypes.None, responseChineseSimplified.Error);

			AssertEquals("Language changed to user's preferred language.", Core.SharedConstants.Languages.ChineseSimplified, ObjectFactory.Get<IResourceStrings>().CurrentLanguage);

			LoginHelper.Validate(responseChineseTradtional, securityHeaderChineseTraditional);
			AssertEquals("Validate successfully.", ErrorTypes.None, responseChineseSimplified.Error);

			AssertEquals("Language changed to user's preferred language.", Core.SharedConstants.Languages.ChineseTraditional, ObjectFactory.Get<IResourceStrings>().CurrentLanguage);

			LoginHelper.Validate(responseEnglish, securityHeaderEnglish);
			AssertEquals("Validate successfully.", ErrorTypes.None, responseEnglish.Error);

			AssertEquals("Language changed to user's preferred language.", Core.SharedConstants.Languages.English, ObjectFactory.Get<IResourceStrings>().CurrentLanguage);

			LoginHelper.Validate(responseEnglishUS, securityHeaderEnglishUS);
			AssertEquals("Validate successfully.", ErrorTypes.None, responseEnglishUS.Error);

			AssertEquals("Language changed to user's preferred language.", Core.SharedConstants.Languages.English, ObjectFactory.Get<IResourceStrings>().CurrentLanguage);

			LoginHelper.Validate(responseEnglishGB, securityHeaderEnglishGB);
			AssertEquals("Validate successfully.", ErrorTypes.None, responseEnglishGB.Error);

			AssertEquals("Language changed to user's preferred language.", Core.SharedConstants.Languages.EnglishBritish, ObjectFactory.Get<IResourceStrings>().CurrentLanguage);

			LoginHelper.Validate(responseFrench, securityHeaderFrench);
			AssertEquals("Validate successfully.", ErrorTypes.None, responseFrench.Error);

			var expectedLanguageForFrenchUser = isAndroidDevice ? Core.SharedConstants.Languages.French : Core.SharedConstants.Languages.English;
			AssertEquals("Language should be set correctly.", expectedLanguageForFrenchUser, ObjectFactory.Get<IResourceStrings>().CurrentLanguage);

			LoginHelper.Validate(responseGerman, securityHeaderGerman);
			AssertEquals("Validate successfully.", ErrorTypes.None, responseGerman.Error);

			var expectedLanguageForGermanUser = isAndroidDevice ? Core.SharedConstants.Languages.German : Core.SharedConstants.Languages.English;
			AssertEquals("Language should be set correctly.", expectedLanguageForGermanUser, ObjectFactory.Get<IResourceStrings>().CurrentLanguage);

			LoginHelper.Validate(responseSpanish, securityHeaderSpanish);
			AssertEquals("Validate successfully.", ErrorTypes.None, responseSpanish.Error);

			var expectedLanguageForSpanishUser = isAndroidDevice ? Core.SharedConstants.Languages.Spanish : Core.SharedConstants.Languages.English;
			AssertEquals("Language should be set correctly.", expectedLanguageForSpanishUser, ObjectFactory.Get<IResourceStrings>().CurrentLanguage);
		}

		#endregion

		#region TestValidate_DeviceOnly

		public void TestValidate_DeviceOnly()
		{
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			staff.GS_IsDevice = true;
			var staff2 = Helper.CreateGlbStaff("BBB", "BBB");
			Factory.Save();

			Assert("Stuff 1 is Device Only User", staff.GS_IsDevice);
			Assert("Stuff 2 is not Device Only User", !staff2.GS_IsDevice);

			var securityHeader = CreateSecurityHeader("AAA", staff.StaffPlainTextPassword, "Device1");
			var response = new WebServiceResponse();
			var securityHeader2 = CreateSecurityHeader("BBB", staff2.StaffPlainTextPassword, "Device2");
			var response2 = new WebServiceResponse();

			LoginHelper.Validate(response, securityHeader);
			AssertEquals("System allow Device Only User Login", ErrorTypes.None, response.Error);

			LoginHelper.Validate(response2, securityHeader2);
			AssertEquals("System allow not Device Only User Login", ErrorTypes.None, response.Error);
		}

		#endregion

		#region TestValidate_ValidSecurityKey

		[TestDate(2007, 10, 10, 10, 10, 10, 100)]
		[ExpectNoExceptions()]
		public void TestValidate_ValidSecurityKey()
		{
			int originalLogCount = Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery);

			var response = new WebServiceResponse();
			var securityKey = Utilities.GenerateSecurityToken(User.LoginName, EncryptedSupportToken, Branch.Code, Department.Code);
			var securityHeader = CreateSecurityHeader(User.LoginName, EncryptedSupportToken, Branch.Code, Department.Code, securityKey);

			using (MockHttpRequestManager())
			{
				LoginHelper.Validate(response, securityHeader);
				AssertEquals(securityKey, response.SecurityKey);
				AssertEquals(Branch.Name, Env.CurrentBranch.Name);
				AssertEquals("Should not create licence usage log record if security key is valid", originalLogCount, Factory.GetDatabaseCount(typeof(StmActivityLog), LicenceUsageLogQuery));
			}
		}

		#endregion

		#region TestValidate_ValidateConcurencyLogin

		public void TestValidate_ValidateConcurencyLogin()
		{
			var staffA = Helper.CreateGlbStaff("AAA", "AAA");
			var staffB = Helper.CreateGlbStaff("BBB", "BBB");

			Factory.Save();

			var notLoggedInUser = CreateSecurityHeader("AAA", staffA.StaffPlainTextPassword, "PC1");
			var notLoggedInUserFromAnotherPC = CreateSecurityHeader("AAA", staffA.StaffPlainTextPassword, "PC2");
			var loggedInUserFromAnotherPC = CreateSecurityHeader("AAA", staffA.StaffPlainTextPassword, "PC2");
			var notLoggedInAnotherUser = CreateSecurityHeader("BBB", staffB.StaffPlainTextPassword, "PC1");
			notLoggedInUser.SecurityKey = "";
			notLoggedInUserFromAnotherPC.SecurityKey = "";
			notLoggedInAnotherUser.SecurityKey = "";
			AssertEquals("Precondition - no active logins expected.", 0, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);

			using (MockHttpRequestManager())
			{
				// not logged in user should have no trouble logging in.
				var response1 = new WebServiceResponse();
				LoginHelper.Validate(response1, notLoggedInUser);
				AssertEquals("User that was not logged in previously should have no errors during login.", ErrorTypes.None, response1.Error);
				AssertEquals("A new active login should be added.", 1, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);
				LoginHelper.ActiveSemaphoreHandlersForTesting.Single(p => p.Key.UserName == "AAA" && p.Key.DeviceID == "PC1");

				// not logged in user should be notified if he is still logged in from another device if active hearbeat for another device exist.
				var response2 = new WebServiceResponse();
				LoginHelper.Validate(response2, notLoggedInUserFromAnotherPC);
				AssertEquals("User that was not logged in previously should have no errors during login.", ErrorTypes.UserLoggedInFromAnotherDevice, response2.Error);
				AssertEquals("No new active logins should be added.", 1, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);
				LoginHelper.ActiveSemaphoreHandlersForTesting.Single(p => p.Key.UserName == "AAA" && p.Key.DeviceID == "PC1");

				// logged in User that doesn't have an active heartbeat should have been remotelly logged out.
				var response3 = new WebServiceResponse();
				LoginHelper.Validate(response3, loggedInUserFromAnotherPC);
				AssertEquals("User should be notified that he was remotelly logged out.", ErrorTypes.UserWasRemotelyLoggedOut, response3.Error);
				AssertEquals("No new active logins should be added.", 1, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);
				LoginHelper.ActiveSemaphoreHandlersForTesting.Single(p => p.Key.UserName == "AAA" && p.Key.DeviceID == "PC1");

				// not logged in user should have no trouble logging in even if another user is already logged in.
				var response4 = new WebServiceResponse();
				LoginHelper.Validate(response4, notLoggedInAnotherUser);
				AssertEquals("User that was not logged in previously should have no errors during login.", ErrorTypes.None, response4.Error);
				AssertEquals("A new active login should be added.", 2, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);
				LoginHelper.ActiveSemaphoreHandlersForTesting.Single(p => p.Key.UserName == "AAA" && p.Key.DeviceID == "PC1");
				LoginHelper.ActiveSemaphoreHandlersForTesting.Single(p => p.Key.UserName == "BBB" && p.Key.DeviceID == "PC1");
			}
		}

		public void TestValidate_ValidateConcurencyLogin_RequireValidateConcurencyLogin()
		{
			var staffA = Helper.CreateGlbStaff("AAA", "AAA");
			Helper.CreateGlbStaff("BBB", "BBB");

			Factory.Save();

			var requireValidateConcurencyLogin = false;

			var notLoggedInUser = CreateSecurityHeader("AAA", staffA.StaffPlainTextPassword, "PC1");
			var notLoggedInUserFromAnotherPC = CreateSecurityHeader("AAA", staffA.StaffPlainTextPassword, "PC2");
			var loggedInUserFromAnotherPC = CreateSecurityHeader("AAA", staffA.StaffPlainTextPassword, "PC2");
			notLoggedInUser.SecurityKey = "";
			notLoggedInUserFromAnotherPC.SecurityKey = "";

			AssertEquals("Precondition - no active logins expected.", 0, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);

			using (MockHttpRequestManager())
			{
				// not logged in user should have no trouble logging in.
				var response1 = new WebServiceResponse();
				LoginHelper.Validate(response1, notLoggedInUser, requireValidateConcurencyLogin);
				AssertEquals("User that was not logged in previously should have no errors during login.", ErrorTypes.None, response1.Error);

				var response2 = new WebServiceResponse();
				LoginHelper.Validate(response2, notLoggedInUserFromAnotherPC, requireValidateConcurencyLogin);
				AssertEquals("User that was not logged in previously should have no errors during login.", ErrorTypes.None, response2.Error);

				var response3 = new WebServiceResponse();
				LoginHelper.Validate(response3, loggedInUserFromAnotherPC, requireValidateConcurencyLogin);
				AssertEquals("User that was not logged in previously should have no errors during login.", ErrorTypes.None, response3.Error);
			}
		}

		public void TestValidate_ValidateConcurencyLogin_WithConcurrencyCheckingTurnedOff()
		{
			var staffA = Helper.CreateGlbStaff("AAA", "AAA");
			Helper.CreateGlbStaff("BBB", "BBB");

			Factory.Save();

			var notLoggedInUser = CreateSecurityHeader("AAA", staffA.StaffPlainTextPassword, "PC1");
			var notLoggedInUserFromAnotherPC = CreateSecurityHeader("AAA", staffA.StaffPlainTextPassword, "PC2");
			notLoggedInUser.SecurityKey = "";
			notLoggedInUserFromAnotherPC.SecurityKey = "";
			AssertEquals("Precondition - no active logins expected.", 0, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);

			using (WarehouseDataRegistry.Instance.ValidateConcurrentRFLogins.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (MockHttpRequestManager())
			{
				var response1 = new WebServiceResponse();
				LoginHelper.Validate(response1, notLoggedInUser);
				AssertEquals("User that was not logged in previously should have no errors during login.", ErrorTypes.None, response1.Error);
				AssertEquals("A new active login should be added.", 1, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);
				LoginHelper.ActiveSemaphoreHandlersForTesting.Single(p => p.Key.UserName == "AAA" && p.Key.DeviceID == "PC1");

				var response2 = new WebServiceResponse();
				LoginHelper.Validate(response2, notLoggedInUserFromAnotherPC);
				AssertEquals("User that was logged in previously should have no errors during login.", ErrorTypes.None, response2.Error);
				AssertEquals("A new active login should be added.", 2, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);
				LoginHelper.ActiveSemaphoreHandlersForTesting.Single(p => p.Key.UserName == "AAA" && p.Key.DeviceID == "PC1");
				LoginHelper.ActiveSemaphoreHandlersForTesting.Single(p => p.Key.UserName == "AAA" && p.Key.DeviceID == "PC2");

				var response3 = new WebServiceResponse();
				LoginHelper.Validate(response3, notLoggedInUserFromAnotherPC);
				AssertEquals("User that was logged in previously should have no errors during login.", ErrorTypes.None, response3.Error);
				AssertEquals("No new active logins should be added.", 2, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);
			}
		}

		public void TestValidate_ValidateConcurencyLogin_WithConcurrencyCheckingTurnedOff_SpamLogins()
		{
			var staffA = Helper.CreateGlbStaff("AAA", "AAA");
			Helper.CreateGlbStaff("BBB", "BBB");

			Factory.Save();

			var users = new List<SecuritySOAPHeader>(10);
			for (int i = 0; i < 10; i++)
			{
				users.Add(CreateSecurityHeader("AAA", staffA.StaffPlainTextPassword, $"PC{i}"));
			}

			var notLoggedInUser = CreateSecurityHeader("AAA", staffA.StaffPlainTextPassword, "PC1");
			var notLoggedInUserFromAnotherPC = CreateSecurityHeader("AAA", staffA.StaffPlainTextPassword, "PC2");
			notLoggedInUser.SecurityKey = "";
			notLoggedInUserFromAnotherPC.SecurityKey = "";
			AssertEquals("Precondition - no active logins expected.", 0, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);

			using (WarehouseDataRegistry.Instance.ValidateConcurrentRFLogins.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				for (int i = 0; i < 10; i++)
				{
					foreach (var user in users)
					{
						var response = new WebServiceResponse();
						LoginHelper.Validate(response, user);
						AssertEquals("Should have no errors during login.", ErrorTypes.None, response.Error);
					}
				}

				AssertEquals("Should have 10 active logins.", 10, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);
			}
		}

		public void TestValidate_ValidateConcurencyLogin_CurrentUserIsNull()
		{
			var staffA = Helper.CreateGlbStaff("XXX", "XXX");
			Factory.Save();

			var securityHeader = CreateSecurityHeader("XXX", staffA.StaffPlainTextPassword, "PC1");

			Env.SetUserContext(null);
			AssertNull(Env.CurrentUser);

			Env.Instance.UserContextChanged += OnContextChanged;

			AssertEquals("Precondition - no active logins expected.", 0, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);

			var response = new WebServiceResponse();
			AssertNoExceptionThrown(() => LoginHelper.Validate(response, securityHeader));
			AssertEquals("User is logged in, no error response.", ErrorTypes.LoginFailed, response.Error);
			AssertEquals("User is logged in, no error response.", "Issue loading the user context. Please try again.", response.ErrorMessage);
			AssertEquals(0, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);

			Env.Instance.UserContextChanged -= OnContextChanged;

			void OnContextChanged(object sender, IUserContextChangingEventArgs e)
			{
				// user context gets set on login, this is to set it to null after it gets set to throw the exception
				if (e.NewUserContext?.User?.LoginName == "XXX")
				{
					Env.SetUserContext(null);
					AssertNull(Env.CurrentUser);
				}
			}
		}

		#endregion

		#endregion

		#region TestLogoutUser

		public void TestLogoutUser()
		{
			var isRemoteLogout = true;
			var staffA = Helper.CreateGlbStaff("AAA", "AAA");
			Helper.CreateGlbStaff("BBB", "BBB");

			Factory.Save();

			var semaphoreType = new RFLoginSemaphoreType();
			var device1SecurityHeader = CreateSecurityHeader("AAA", staffA.StaffPlainTextPassword, "PC1");
			var device2SecurityHeader = CreateSecurityHeader("AAA", staffA.StaffPlainTextPassword, "PC2");
			var device1SemaphoreProvider = new RFSemaphoreProvider(device1SecurityHeader);
			var device2SemaphoreProvider = new RFSemaphoreProvider(device2SecurityHeader);

			// can logout even if the user is not logged in without errors.
			var activeSemaphores1 = ((ISemaphoreProvider)device1SemaphoreProvider).GetActiveSemaphoreHandles(semaphoreType);
			AssertEquals("Precondition - no active semaphore should exist beforehand.", 0, activeSemaphores1.Length);
			AssertNoExceptionThrown(() => LoginHelper.LogoutUser(device1SecurityHeader, false));
			AssertNoExceptionThrown(() => LoginHelper.LogoutUser(device1SecurityHeader, true));

			// local logout of a user
			LoginHelper.Validate(new WebServiceResponse(), device1SecurityHeader);
			Factory.Save();
			var activeSemaphores2 = ((ISemaphoreProvider)device1SemaphoreProvider).GetActiveSemaphoreHandles(semaphoreType);
			AssertEquals("Precondition - user should be logged in.", 1, activeSemaphores2.Length);
			AssertEquals("Precondition - user should be logged in.", 1, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);

			LoginHelper.LogoutUser(device2SecurityHeader, !isRemoteLogout);
			AssertEquals("User should not be logged out if device of logged in user doesn't match.", 1, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);

			LoginHelper.LogoutUser(device1SecurityHeader, !isRemoteLogout);
			AssertEquals("User should have been logged out.", 0, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);

			// remote logout of a user
			LoginHelper.Validate(new WebServiceResponse(), device1SecurityHeader);
			Factory.Save();
			var activeSemaphores3 = ((ISemaphoreProvider)device1SemaphoreProvider).GetActiveSemaphoreHandles(semaphoreType);
			AssertEquals("Precondition - user should be logged in.", 1, activeSemaphores3.Length);
			AssertEquals("Precondition - user should be logged in.", 1, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);

			LoginHelper.LogoutUser(device1SecurityHeader, isRemoteLogout);
			AssertEquals("User should not be remotelly logged out if device of logged in user match current device.", 1, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);

			LoginHelper.LogoutUser(device2SecurityHeader, isRemoteLogout);
			AssertEquals("User should have been logged out.", 0, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);

			// remote logout of a user logged in from different webservice place.
			using (RFSemaphoreProvider.CreateSemaphoreHandle(device1SemaphoreProvider, semaphoreType))
			{
				Factory.Save();
				var activeSemaphores4 = ((ISemaphoreProvider)device1SemaphoreProvider).GetActiveSemaphoreHandles(semaphoreType);
				AssertEquals("Precondition - user should be logged in.", 1, activeSemaphores4.Length);
				AssertEquals("Precondition - user should have been logged in from outside of current WebService.", 0, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);

				LoginHelper.LogoutUser(device2SecurityHeader, isRemoteLogout);
				var activeSemaphores5 = ((ISemaphoreProvider)device1SemaphoreProvider).GetActiveSemaphoreHandles(semaphoreType);
				AssertEquals("User should have been logged out remotely.", 0, activeSemaphores5.Length);
				AssertEquals("User should have been logged out remotely.", 0, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);
			}
		}

		public void TestLogoutUser_CurrentUserIsNull()
		{
			var isRemoteLogout = true;
			var staffA = Helper.CreateGlbStaff("AAA", "AAA");

			Factory.Save();

			// Login as a none existing user BBB, to avoid Env.SetUserContext during LoginHelper.LogoutUser.
			var device1SecurityHeader = CreateSecurityHeader("BBB", staffA.StaffPlainTextPassword, "PC1");

			Env.SetUserContext(null);
			AssertNull(Env.CurrentUser);

			AssertNoExceptionThrown(() => LoginHelper.LogoutUser(device1SecurityHeader, isRemoteLogout));
		}

		#endregion

		#region TestUserContextSwitchFromCWWeb

		public void TestUserContextSwithFromCWWeb_ValidateLogin()
		{
			TestUserContextSwitchFromCWWebCore((securityHeader) => LoginHelper.Validate(new WebServiceResponse(), securityHeader));
		}

		public void TestUserContextSwithFromCWWeb_LogoutUser()
		{
			TestUserContextSwitchFromCWWebCore((securityHeader) => LoginHelper.LogoutUser(securityHeader, false));
		}

		void TestUserContextSwitchFromCWWebCore(Action<SecuritySOAPHeader> userContextSwitchAction)
		{
			var securityHeader = CreateSecurityHeader(Env.CurrentUser.LoginName, EncryptedSupportToken, Env.CurrentBranch.Code, Env.CurrentDepartment.Code, "");

			using (MockHttpRequestManager())
			using (Env.Instance.SetTemporaryUserContext(new UserContext("CWWeb", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				AssertNoExceptionThrown(() => userContextSwitchAction(securityHeader));
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		#endregion

		#region Implementation

		#region CreateSecurityHeader

		SecuritySOAPHeader CreateSecurityHeader(string userName, string password, string deviceID, string deviceModelDetails = "")
		{
			var securityKey = Utilities.GenerateSecurityToken(userName, GetEncryptedText(password), Env.CurrentBranch.Code, Env.CurrentDepartment.Code);
			return CreateSecurityHeader(userName, GetEncryptedText(password), Env.CurrentBranch.Code, Env.CurrentDepartment.Code, securityKey, deviceID, deviceModelDetails);
		}

		SecuritySOAPHeader CreateSecurityHeader(string userName, string encryptedMasterPassword, string branchCode, string departmentCode, string securityKey)
		{
			return CreateSecurityHeader(userName, encryptedMasterPassword, branchCode, departmentCode, securityKey, "PC", "MAN:Zebra|MDL:TC77|OS:AND");
		}

		SecuritySOAPHeader CreateSecurityHeader(string userName, string encryptedPassword, string branchCode, string departmentCode, string securityKey, string deviceID, string deviceModelDetails = "")
		{
			var securityHeader = new SecuritySOAPHeader();
			securityHeader.UserName = userName;
			securityHeader.Password = encryptedPassword;
			securityHeader.BranchCode = branchCode;
			securityHeader.DepartmentCode = departmentCode;
			securityHeader.SecurityKey = securityKey;
			securityHeader.DeviceID = deviceID;
			securityHeader.DeviceModelDetails = deviceModelDetails;

			return securityHeader;
		}

		#endregion

		ZQuery GetLicenceUsageLogWithDeviceIDQuery(string deviceID)
		{
			var query = LicenceUsageLogQuery;
			query.AddToFilter(StmActivityLogSchema.S7_DeviceID, SQLComparisonOperator.Equal, deviceID);

			return query;
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

		string EncryptedSupportToken
		{
			get
			{
				return GetEncryptedText(CWSupportLoginToken.TokenForTest);
			}
		}

		protected string GetEncryptedText(string text)
		{
			var encoder = new AESCryptographicProvider(LoginHelper.CryptographyKey.Value, LoginHelper.CryptographyIv.Value);
			return Convert.ToBase64String(encoder.Encrypt(Encoding.Unicode.GetBytes(text)));
		}

		Guid GetActiveUserPK(string username)
		{
			Guid result = Guid.Empty;
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
			Guid result = Guid.Empty;
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
			Guid result = Guid.Empty;
			using (var reader = Db.Connection.Command("select GB_PK from dbo.GlbBranch where GB_Code = '" + branchCode + "'").ExecuteReader())      // It is cheaper and quicker to bypass using a BusinessObjectFactory here
			{
				if (reader.Read())
				{
					result = (Guid)reader[GlbBranchSchema.PK.Name];
				}
			}
			return result;
		}

		void SetUpPostMaster()
		{
			GlbGroup postmasterGroup = Factory.Load<GlbGroup>(EnvProxy.Instance.Registry.PostMasterGroup);
			var postMaster = postmasterGroup.Staff.AddNew();
			postMaster.GS_EmailAddress = "PostMaster@PostMaster.com";
			Factory.Save();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		protected override void TearDown()
		{
			base.TearDown();
			EnvProxy.Instance.SetUserContext(initialUserContext);
			LoginHelper.ClearAllActiveSemaphoreHandlers_ForTesting();
		}

		protected override void SetUp()
		{
			base.SetUp();
			Branch = EnvProxy.Instance.CurrentBranch;
			Department = EnvProxy.Instance.CurrentDepartment;
			User = EnvProxy.Instance.CurrentUser;
			initialUserContext = EnvProxy.Instance.CurrentUserContext;
			Factory = new BusinessObjectFactory();
		}

		IDisposable MockHttpRequestManager(bool isSecureConnection = true, bool isRedirectedFromLoadBalancer = false)
		{
			var connectionTypeSupporterMock = new Mock<IHttpRequestManager>();
			connectionTypeSupporterMock
				.Setup(c => c.GetHttpContextBase())
				.Returns(MockHttpContext.PrepareMockHttpContextWrapper(isSecureConnection, isRedirectedFromLoadBalancer));

			return ObjectFactory.Substitute(connectionTypeSupporterMock.Object);
		}

		#region Helper

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion

		IBranch Branch;
		IDepartment Department;
		IUser User;
		IUserContext initialUserContext;
		BusinessObjectFactory Factory;

		#endregion
	}
}
