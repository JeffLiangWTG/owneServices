using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Accounting.EPayment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccEPaymentStaffTokenLookups;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccEPaymentStaffToken))]
	sealed class AccEPaymentStaffTokenTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPrepareOAuthURLHasCorrectTestEhubGatewayParam()
		{
			var bankAccountData = Factory.NewWithValidTestData<AccBankAccount>();
			var collection = new AccEPaymentStaffTokenDependentCollection(bankAccountData);
			var staffToken = collection.AddNew();
			string oAuthUrl;
			var ofxSecret = (new AESCrypto()).DecryptStringAES(AccountingMasterFilesRegistry.Instance.OFXEncryptionKey.Value, AESCrypto.RANDOM_SHAREDSECRET);
			using (eHubMessagingRegistry.Instance.eHubSendInterchangesToTestGateway.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (AccountingMasterFilesRegistry.Instance.OFXOAuthLoginWebURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.OFXOAuthWebsiteURL.TestingURL))
				using (AccountingMasterFilesRegistry.Instance.OFXOAuthWebURLEnvironment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);
					Assert(!Env.Instance.IsProductionSystem);
					oAuthUrl = staffToken.PrepareOAuthURL();
					var stateParameter = ExtractState();
					stateParameter = stateParameter.Replace(" ", "+");  //https://stackoverflow.com/questions/123994/querystring-malformed-after-urldecode
					var states = (new AESCrypto()).DecryptStringAES(stateParameter, ofxSecret);
					var stateArray = states.Split('.');
					AssertEquals("Expect 8 parts in the state parameter", 8, stateArray.Length);
					AssertEquals("useTestEhubGateway", "False", stateArray[6]);
					AssertEquals("isUsingProductionURL", "False", stateArray[7]);
				}

				using (AccountingMasterFilesRegistry.Instance.OFXOAuthLoginWebURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.OFXOAuthWebsiteURL.ProductionURL))
				using (AccountingMasterFilesRegistry.Instance.OFXOAuthWebURLEnvironment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
					Assert(Env.Instance.IsProductionSystem);
					oAuthUrl = staffToken.PrepareOAuthURL();
					var stateParameter = ExtractState();
					stateParameter = stateParameter.Replace(" ", "+");
					var states = (new AESCrypto()).DecryptStringAES(stateParameter, ofxSecret);
					var stateArray = states.Split('.');
					AssertEquals("Expect 8 parts in the state parameter", 8, stateArray.Length);
					AssertEquals("useTestEhubGateway", "False", stateArray[6]);
					AssertEquals("isUsingProductionURL", "True", stateArray[7]);
				}
			}

			using (eHubMessagingRegistry.Instance.eHubSendInterchangesToTestGateway.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (AccountingMasterFilesRegistry.Instance.OFXOAuthLoginWebURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.OFXOAuthWebsiteURL.TestingURL))
				using (AccountingMasterFilesRegistry.Instance.OFXOAuthWebURLEnvironment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);
					Assert(!Env.Instance.IsProductionSystem);
					oAuthUrl = staffToken.PrepareOAuthURL();
					var stateParameter = ExtractState();
					stateParameter = stateParameter.Replace(" ", "+");  //https://stackoverflow.com/questions/123994/querystring-malformed-after-urldecode
					var states = (new AESCrypto()).DecryptStringAES(stateParameter, ofxSecret);
					var stateArray = states.Split('.');
					AssertEquals("Expect 8 parts in the state parameter", 8, stateArray.Length);
					AssertEquals("useTestEhubGateway", "True", stateArray[6]);
					AssertEquals("isUsingProductionURL", "False", stateArray[7]);
				}

				using (AccountingMasterFilesRegistry.Instance.OFXOAuthLoginWebURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.OFXOAuthWebsiteURL.ProductionURL))
				using (AccountingMasterFilesRegistry.Instance.OFXOAuthWebURLEnvironment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
					Assert(Env.Instance.IsProductionSystem);
					oAuthUrl = staffToken.PrepareOAuthURL();
					var stateParameter = ExtractState();
					stateParameter = stateParameter.Replace(" ", "+");
					var states = (new AESCrypto()).DecryptStringAES(stateParameter, ofxSecret);
					var stateArray = states.Split('.');
					AssertEquals("Expect 8 parts in the state parameter", 8, stateArray.Length);
					AssertEquals("useTestEhubGateway", "False", stateArray[6]);
					AssertEquals("isUsingProductionURL", "True", stateArray[7]);
				}
			}

			string ExtractState()
			{
				var stateParameterName = "&state=";
				var stateBeginsAt = oAuthUrl.IndexOf(stateParameterName) + stateParameterName.Length;
				var stateEndAt = oAuthUrl.IndexOf("&scope=");
				return oAuthUrl.Substring(stateBeginsAt, stateEndAt - stateBeginsAt);
			}
		}

		public void TestPrepareOAuthURLUsesCorrectClientId()
		{
			var bankAccountData = Factory.NewWithValidTestData<AccBankAccount>();
			var collection = new AccEPaymentStaffTokenDependentCollection(bankAccountData);
			var staffToken = collection.AddNew();

			Assert(!Env.Instance.IsProductionSystem);
			var expectedClientIdForNonProduction = "xwFiSN389IiPKBdqXROEyUFpcG3w6lM6";
			var expectedClientIdForProduction = "4X04vlVSUTfF7HVKaO2WdyctbxGY0Sy6";
			var oAuthUrl = staffToken.PrepareOAuthURL();
			AssertEquals(expectedClientIdForNonProduction, ExtractClientId());

			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			Assert(Env.Instance.IsProductionSystem);
			using (AccountingMasterFilesRegistry.Instance.OFXOAuthLoginWebURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.OFXOAuthWebsiteURL.TestingURL))
			using (AccountingMasterFilesRegistry.Instance.OFXOAuthWebURLEnvironment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				oAuthUrl = staffToken.PrepareOAuthURL();
				AssertEquals(expectedClientIdForNonProduction, ExtractClientId());
			}

			using (AccountingMasterFilesRegistry.Instance.OFXOAuthLoginWebURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.OFXOAuthWebsiteURL.ProductionURL))
			using (AccountingMasterFilesRegistry.Instance.OFXOAuthWebURLEnvironment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				oAuthUrl = staffToken.PrepareOAuthURL();
				AssertEquals(expectedClientIdForProduction, ExtractClientId());
			}

			string ExtractClientId()
			{
				var clientIdParameterName = "&client_id=";
				var clientIdBeginsAt = oAuthUrl.IndexOf(clientIdParameterName) + clientIdParameterName.Length;
				var clientIdEndAt = oAuthUrl.IndexOf("&state=");
				return oAuthUrl.Substring(clientIdBeginsAt, clientIdEndAt - clientIdBeginsAt);
			}
		}

		[TestDate(2021, 4, 20, 23, 15, 55)]
		public void TestSetDefaultValues()
		{
			var bankAccountData = Factory.NewWithValidTestData<AccBankAccount>();
			var collection = new AccEPaymentStaffTokenDependentCollection(bankAccountData);
			var staffToken = collection.AddNew();
			AssertEquals("payments", staffToken.TK_Scope);
			AssertEquals(GlbCompany.CurrentCompany.PK, staffToken.TK_GC);
			AssertEquals(new ZDateTime(2021, 4, 20, 23, 15, 55), staffToken.TK_RequestedUtc);
		}

		[TestDate(2021, 4, 20, 23, 15, 55)]
		public void TestResetToPendingStatus()
		{
			var bankAccountData = Factory.NewWithValidTestData<AccBankAccount>();
			var collection = new AccEPaymentStaffTokenDependentCollection(bankAccountData);
			var staffToken = collection.AddNew();
			staffToken.TK_Status = StatusCodes.Error;
			staffToken.TK_RequestedUtc = new ZDateTime(2021, 3, 17, 12, 23, 35);
			staffToken.TK_ExpiryUtc = new ZDateTime(2021, 4, 17, 12, 23, 35);
			staffToken.TK_ErrorDescription = "some errors";
			staffToken.ResetToPendingStatus();
			AssertEquals(StatusCodes.Pending, staffToken.TK_Status);
			AssertEquals(new ZDateTime(2021, 4, 20, 23, 15, 55), staffToken.TK_RequestedUtc);
			Assert(staffToken.TK_ExpiryUtc.IsEmpty);
			Assert(staffToken.TK_ErrorDescription.IsEmpty);
		}

		public void TestFieldsReadonlyness()
		{
			var staffToken = GetNewBusinessObject() as AccEPaymentStaffToken;
			Assert(staffToken.TK_StatusInfo.ReadOnly);
			Assert(staffToken.TK_AccountNameInfo.ReadOnly);
			Assert(staffToken.TK_RequestedUtcInfo.ReadOnly);
			Assert(staffToken.TK_ExpiryUtcInfo.ReadOnly);
			Assert(staffToken.TK_ErrorDescriptionInfo.ReadOnly);
			Assert(!staffToken.TK_GS_NKStaffCodeInfo.ReadOnly);
			Factory.Save();
			Assert(staffToken.TK_GS_NKStaffCodeInfo.ReadOnly);
		}

		public void TestStatus()
		{
			var staffToken = GetNewBusinessObject() as AccEPaymentStaffToken;
			foreach (var code in staffToken.Lookups.StatusCodeList.GetAllCodes())
			{
				staffToken.TK_Status = code;
				AssertEquals(staffToken.Lookups.StatusCodeList.GetDescriptionFromCode(code), staffToken.Status);
			}
		}

		public void TestIsAuthorised()
		{
			var staffToken = GetNewBusinessObject() as AccEPaymentStaffToken;
			var invalidAuthorisedCodes = new[] { StatusCodes.NotAuthorised, StatusCodes.Pending, StatusCodes.Error };
			foreach (var code in staffToken.Lookups.StatusCodeList.GetAllCodes())
			{
				staffToken.TK_Status = code;
				AssertEquals(invalidAuthorisedCodes.Contains(code), !staffToken.IsAuthorised);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<AccEPaymentStaffToken>();
		}
	}
}
