using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EDICommunicationAuth))]
	public class EDICommunicationAuthTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUniquenessCheckOfInboundConfigOnAdd()
		{
			var auth = Factory.NewWithValidTestData<EDICommunicationAuth>();

			Factory.Save();

			var oldCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var oldConfig = oldCommParty.Configs.AddNew();
			oldConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			oldConfig.ECC_ECA_Auth = auth.PK;

			Factory.Save();

			var newCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var newConfig = newCommParty.Configs.AddNew();
			newConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			newConfig.ECC_ECA_Auth = auth.PK;

			AssertExceptionThrown<ZCannotSaveException>("Save() should fail trying to save inbound config with the same auth config", "Cannot have two configurations with authentication using the same authentication configuration. Please choose a different authentication configuration.", () => Factory.Save());
		}

		public void TestUniquenessCheckOfInboundConfigOnEdit()
		{
			var auth = Factory.NewWithValidTestData<EDICommunicationAuth>();

			Factory.Save();

			var oldCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var oldConfig = oldCommParty.Configs.AddNew();
			oldConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			oldConfig.ECC_ECA_Auth = auth.PK;

			Factory.Save();

			var newCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var newConfig = newCommParty.Configs.AddNew();
			newConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;

			Factory.Save();

			newConfig.ECC_ECA_Auth = auth.PK;

			AssertExceptionThrown<ZCannotSaveException>("Save() should fail trying to save inbound config with the same auth config", "Cannot have two configurations with authentication using the same authentication configuration. Please choose a different authentication configuration.",() => Factory.Save());
		}

		public void TestUniquenessCheckOfUsernameOnInboundConfigWithBasicAuthOnEdit()
		{
			var auth1 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth1.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;
			auth1.ECA_Username = "dummy1";

			var auth2 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth2.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;
			auth2.ECA_Username = "dummy2";

			Factory.Save();

			var oldCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var oldConfig = oldCommParty.Configs.AddNew();
			oldConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			oldConfig.ECC_ECA_Auth = auth1.PK;

			var newCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var newConfig = newCommParty.Configs.AddNew();
			newConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			newConfig.ECC_ECA_Auth = auth2.PK;

			Factory.Save();

			auth2.ECA_Username = auth1.ECA_Username;

			AssertExceptionThrown<ZCannotSaveException>("Save() should fail trying to save inbound config with duplicate username", () => Factory.Save());
		}

		public void TestNoUniquenessCheckOfUsernameOnOutboundConfigWithBasicAuthOnEdit()
		{
			var auth1 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth1.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;
			auth1.ECA_Username = "dummy1";

			var auth2 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth2.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;
			auth2.ECA_Username = "dummy2";

			Factory.Save();

			var oldCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var oldConfig = oldCommParty.Configs.AddNew();
			oldConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;
			oldConfig.ECC_ECA_Auth = auth1.PK;

			var newCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var newConfig = newCommParty.Configs.AddNew();
			newConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;
			newConfig.ECC_ECA_Auth = auth2.PK;

			Factory.Save();

			auth2.ECA_Username = auth1.ECA_Username;

			AssertNoExceptionThrown("Save() shouldn't fail trying to save outbound config with duplicate username", () => Factory.Save());
		}

		public void TestUniquenessCheckOfOutboundConfigOnAdd()
		{
			var auth = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;

			Factory.Save();

			var oldCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var oldConfig = oldCommParty.Configs.AddNew();
			oldConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;
			oldConfig.ECC_ECA_Auth = auth.PK;

			Factory.Save();

			var newCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var newConfig = newCommParty.Configs.AddNew();
			newConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;
			newConfig.ECC_ECA_Auth = auth.PK;

			AssertExceptionThrown<ZCannotSaveException>("Save() should fail trying to save outbound config with the same auth config", "Cannot have two configurations with authentication using the same authentication configuration. Please choose a different authentication configuration.", () => Factory.Save());
		}

		public void TestUniquenessCheckOfOutboundConfigOnEdit()
		{
			var auth = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;

			Factory.Save();

			var oldCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var oldConfig = oldCommParty.Configs.AddNew();
			oldConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;
			oldConfig.ECC_ECA_Auth = auth.PK;

			Factory.Save();

			var newCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var newConfig = newCommParty.Configs.AddNew();
			newConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;

			Factory.Save();

			newConfig.ECC_ECA_Auth = auth.PK;

			AssertExceptionThrown<ZCannotSaveException>("Save() should fail trying to save outbound config with the same auth config", "Cannot have two configurations with authentication using the same authentication configuration. Please choose a different authentication configuration.", () => Factory.Save());
		}

		public void TestUniquenessCheckOfClientIDOnInboundConfigWithOAuthAuthOnEdit()
		{
			var auth1 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth1.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			auth1.ECA_ClientID = "dummy1";

			var auth2 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth2.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			auth2.ECA_ClientID = "dummy2";

			Factory.Save();

			var oldCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var oldConfig = oldCommParty.Configs.AddNew();
			oldConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			oldConfig.ECC_ECA_Auth = auth1.PK;

			var newCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var newConfig = newCommParty.Configs.AddNew();
			newConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			newConfig.ECC_ECA_Auth = auth2.PK;

			Factory.Save();

			auth2.ECA_ClientID = auth1.ECA_ClientID;

			AssertExceptionThrown<ZCannotSaveException>("Save() should fail trying to save inbound config with duplicate Client ID", "Cannot have two inbound configurations with OAuth authentication using the same Client ID. Please choose another Client ID.", () => Factory.Save());
		}

		public void TestNoUniquenessCheckOfClientIDOnOutboundConfigWithOAuthAuthOnEdit()
		{
			var auth1 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth1.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			auth1.ECA_ClientID = "dummy1";

			var auth2 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth2.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			auth2.ECA_ClientID = "dummy2";

			Factory.Save();

			var oldCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var oldConfig = oldCommParty.Configs.AddNew();
			oldConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;
			oldConfig.ECC_ECA_Auth = auth1.PK;

			var newCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var newConfig = newCommParty.Configs.AddNew();
			newConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;
			newConfig.ECC_ECA_Auth = auth2.PK;

			Factory.Save();

			auth2.ECA_ClientID = auth1.ECA_ClientID;

			AssertNoExceptionThrown("Save() shouldn't fail trying to save outbound config with duplicate Client ID", () => Factory.Save());
		}

		public void TestRegisterCertificateCallsCertificateManagerCorrectly()
		{
			var certificateManagerMock = new Mock<ICertificateManager>();
			var clientID = "dummy-client-id";
			var csr = "dummy-csr";

			certificateManagerMock.Setup(x => x.RolloverCertificate(clientID, csr, EDICommunicationAuth.CaRoot)).Verifiable();

			var auth = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			auth.ECA_ClientID = clientID;
			auth.ECA_OperationId = "dummy-operation-id";

			using (ObjectFactory.Substitute<ICertificateManager>(certificateManagerMock.Object))
			{
				AssertNoExceptionThrown("Call to RegisterCertificate() should not throw any errors", () => auth.RegisterCertificate(csr, string.Empty));
				certificateManagerMock.Verify();
			}
		}

		public void TestValidateConfig()
		{
			var auth = Factory.NewWithValidTestData<EDICommunicationAuth>();

			PrepareCommunicationParty(auth, EDICommunicationPartyConfigDirectionsList.Codes.Outbound);
			PrepareCommunicationAuthWithDummyData(auth);
			auth.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			auth.IsVerified = false;
			Factory.Save();
			auth.Validation.ValidateAll();
			Assert("Has Row Error", auth.HasRowErrors);
			AssertEquals("Configuration must be verified before changes can be saved.", auth.RowErrors.First().Message);
		}

		public void TestMandatoryFieldsForBasic()
		{
			var auth1 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth1.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;
			var auth2 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth2.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;

			PrepareCommunicationParty(auth1, EDICommunicationPartyConfigDirectionsList.Codes.Inbound);
			PrepareCommunicationParty(auth2, EDICommunicationPartyConfigDirectionsList.Codes.Outbound);
			PrepareCommunicationAuthWithDummyData(auth1);
			PrepareCommunicationAuthWithDummyData(auth2);

			auth1.ECA_Username = "";
			auth1.ECA_Password = "";
			auth2.ECA_Username = "";
			auth2.ECA_Password = "";
			Factory.Save();

			AssertHasError(auth1.ECA_UsernameInfo, "Please enter a value.");
			AssertHasError(auth1.ECA_PasswordInfo, "Please enter a value.");
			AssertHasError(auth2.ECA_UsernameInfo, "Please enter a value.");
			AssertHasError(auth2.ECA_PasswordInfo, "Please enter a value.");
		}

		public void TestNonMandatoryFieldsForOAuth()
		{
			var auth1 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth1.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			var auth2 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth2.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;

			PrepareCommunicationParty(auth1, EDICommunicationPartyConfigDirectionsList.Codes.Inbound);
			PrepareCommunicationParty(auth2, EDICommunicationPartyConfigDirectionsList.Codes.Outbound);
			PrepareCommunicationAuthWithDummyData(auth1);
			PrepareCommunicationAuthWithDummyData(auth2);

			auth1.ECA_ClientID = "";
			auth1.ECA_FlowCode = "";
			auth2.ECA_ClientID = "";
			auth2.ECA_FlowCode = "";
			Factory.Save();

			AssertNoError(auth1.ECA_ClientIDInfo, "Please enter a value.");
			AssertNoError("Flow code is not mandatory for inbound", auth1.ECA_FlowCodeInfo, "Please enter a value.");
			AssertNoError(auth2.ECA_ClientIDInfo, "Please enter a value.");
			AssertHasError(auth2.ECA_FlowCodeInfo, "Please enter a value.");
		}

		public void TestMandatoryFieldsForOutboundOAuthWithFlowCode()
		{
			var auth1 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth1.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			var auth2 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth2.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;

			PrepareCommunicationParty(auth1, EDICommunicationPartyConfigDirectionsList.Codes.Outbound);
			PrepareCommunicationParty(auth2, EDICommunicationPartyConfigDirectionsList.Codes.Outbound);
			PrepareCommunicationAuthWithDummyData(auth1);
			PrepareCommunicationAuthWithDummyData(auth2);

			auth1.ECA_FlowCode = EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCredentials;
			auth1.ECA_Username = "";
			auth1.ECA_Password = "";
			auth1.ECA_ClientSecret = "";
			auth2.ECA_FlowCode = EDICommunicationAuthOutboundGrantTypesList.Codes.Password;
			auth2.ECA_Username = "";
			auth2.ECA_Password = "";
			auth2.ECA_ClientSecret = "";
			Factory.Save();

			AssertHasError(auth1.ECA_ClientSecretInfo, "Please enter a value.");
			AssertNoError("Username is not mandatory for ClientCredentials", auth1.ECA_UsernameInfo, "Please enter a value.");
			AssertNoError("Password is not mandatory for ClientCredentials", auth1.ECA_PasswordInfo, "Please enter a value.");
			AssertHasError(auth2.ECA_ClientSecretInfo, "Please enter a value.");
			AssertHasError(auth2.ECA_UsernameInfo, "Please enter a value.");
			AssertHasError(auth2.ECA_PasswordInfo, "Please enter a value.");
		}

		public void TestResetNonUsedFieldsForBasic()
		{
			var auth1 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth1.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;
			var auth2 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth2.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;

			PrepareCommunicationParty(auth1, EDICommunicationPartyConfigDirectionsList.Codes.Inbound);
			PrepareCommunicationParty(auth2, EDICommunicationPartyConfigDirectionsList.Codes.Outbound);
			PrepareCommunicationAuthWithDummyData(auth1);
			PrepareCommunicationAuthWithDummyData(auth2);

			Factory.Save();
			var auth1New = Factory.Load<EDICommunicationAuth>(auth1.PK);
			var auth2New = Factory.Load<EDICommunicationAuth>(auth2.PK);

			CombineAssertions(delegate
			{
				AssertEquals(auth1.ECA_Username, auth1New.ECA_Username);
				AssertEquals(auth1.ECA_Password, auth1New.ECA_Password);
				AssertEquals(auth1.ECA_AuthorizationMode, auth1New.ECA_AuthorizationMode);
				AssertEquals("", auth1New.ECA_AuthorizationEndpoint);
				AssertEquals(ZBlob.Empty, auth1New.ECA_Certificate);
				AssertEquals("", auth1New.ECA_ClientID);
				AssertEquals("", auth1New.ECA_ClientSecret);
				AssertEquals(ZBlob.Empty, auth1New.ECA_EncodedPrivateKey);
				AssertEquals(EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCertificate, auth1New.ECA_FlowCode);
				AssertEquals("", auth1New.ECA_OperationId);
				AssertEquals(ZBlob.Empty, auth1New.ECA_RenewalEncodedPrivateKey);
				AssertEquals("", auth1New.ECA_RenewalOperationId);
				AssertEquals("", auth1New.ECA_Scopes);
			});
			CombineAssertions(delegate
			{
				AssertEquals(auth2.ECA_Username, auth2New.ECA_Username);
				AssertEquals(auth2.ECA_Password, auth2New.ECA_Password);
				AssertEquals(auth2.ECA_AuthorizationMode, auth2New.ECA_AuthorizationMode);
				AssertEquals("", auth2New.ECA_AuthorizationEndpoint);
				AssertEquals(ZBlob.Empty, auth2New.ECA_Certificate);
				AssertEquals("", auth2New.ECA_ClientID);
				AssertEquals("", auth2New.ECA_ClientSecret);
				AssertEquals(ZBlob.Empty, auth2New.ECA_EncodedPrivateKey);
				AssertEquals(EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCertificate, auth2New.ECA_FlowCode);
				AssertEquals("", auth2New.ECA_OperationId);
				AssertEquals(ZBlob.Empty, auth2New.ECA_RenewalEncodedPrivateKey);
				AssertEquals("", auth2New.ECA_RenewalOperationId);
				AssertEquals("", auth2New.ECA_Scopes);
			});
		}

		public void TestResetNonUsedFieldsForNoAuth()
		{
			var auth1 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth1.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.NoAuthentication;

			PrepareCommunicationParty(auth1, EDICommunicationPartyConfigDirectionsList.Codes.Outbound);
			PrepareCommunicationAuthWithDummyData(auth1);

			Factory.Save();
			var auth1New = Factory.Load<EDICommunicationAuth>(auth1.PK);

			CombineAssertions(delegate
			{
				AssertEquals("", auth1New.ECA_Username);
				AssertEquals("", auth1New.ECA_Password);
				AssertEquals(auth1.ECA_AuthorizationMode, auth1New.ECA_AuthorizationMode);
				AssertEquals("", auth1New.ECA_AuthorizationEndpoint);
				AssertEquals(ZBlob.Empty, auth1New.ECA_Certificate);
				AssertEquals("", auth1New.ECA_ClientID);
				AssertEquals("", auth1New.ECA_ClientSecret);
				AssertEquals(ZBlob.Empty, auth1New.ECA_EncodedPrivateKey);
				AssertEquals(EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCertificate, auth1New.ECA_FlowCode);
				AssertEquals("", auth1New.ECA_OperationId);
				AssertEquals(ZBlob.Empty, auth1New.ECA_RenewalEncodedPrivateKey);
				AssertEquals("", auth1New.ECA_RenewalOperationId);
				AssertEquals("", auth1New.ECA_Scopes);
			});
		}

		public void TestResetNonUsedFieldsForOAuthInbound()
		{
			var auth1 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth1.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;

			PrepareCommunicationParty(auth1, EDICommunicationPartyConfigDirectionsList.Codes.Inbound);
			PrepareCommunicationAuthWithDummyData(auth1);

			Factory.Save();
			var auth1New = Factory.Load<EDICommunicationAuth>(auth1.PK);

			CombineAssertions(delegate
			{
				AssertEquals("", auth1New.ECA_Username);
				AssertEquals("", auth1New.ECA_Password);
				AssertEquals(auth1.ECA_AuthorizationMode, auth1New.ECA_AuthorizationMode);
				AssertEquals(auth1.ECA_AuthorizationEndpoint, auth1New.ECA_AuthorizationEndpoint);
				AssertEquals(auth1.ECA_Certificate, auth1New.ECA_Certificate);
				AssertEquals(auth1.ECA_ClientID, auth1New.ECA_ClientID);
				AssertEquals("Test Secret", auth1New.ECA_ClientSecret);
				AssertEquals(auth1.ECA_EncodedPrivateKey, auth1New.ECA_EncodedPrivateKey);
				AssertEquals(auth1.ECA_FlowCode, auth1New.ECA_FlowCode);
				AssertEquals(auth1.ECA_OperationId, auth1New.ECA_OperationId);
				AssertEquals(auth1.ECA_RenewalEncodedPrivateKey, auth1New.ECA_RenewalEncodedPrivateKey);
				AssertEquals(auth1.ECA_RenewalOperationId, auth1New.ECA_RenewalOperationId);
				AssertEquals(auth1.ECA_Scopes, auth1New.ECA_Scopes);
			});
		}

		public void TestResetNonUsedFieldsForOAuthOutbound()
		{
			var auth1 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth1.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			var auth2 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth2.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			var auth3 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth3.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;

			PrepareCommunicationParty(auth1, EDICommunicationPartyConfigDirectionsList.Codes.Outbound);
			PrepareCommunicationParty(auth2, EDICommunicationPartyConfigDirectionsList.Codes.Outbound);
			PrepareCommunicationParty(auth3, EDICommunicationPartyConfigDirectionsList.Codes.Outbound);
			PrepareCommunicationAuthWithDummyData(auth1, eAdaptorNextOutboundGrantTypesList.Codes.ClientCertificate);
			PrepareCommunicationAuthWithDummyData(auth2, eAdaptorNextOutboundGrantTypesList.Codes.ClientCredentials);
			PrepareCommunicationAuthWithDummyData(auth3, eAdaptorNextOutboundGrantTypesList.Codes.Password);

			Factory.Save();
			var auth1New = Factory.Load<EDICommunicationAuth>(auth1.PK);
			var auth2New = Factory.Load<EDICommunicationAuth>(auth2.PK);
			var auth3New = Factory.Load<EDICommunicationAuth>(auth3.PK);

			CombineAssertions(delegate
			{
				AssertEquals("", auth1New.ECA_Username);
				AssertEquals("", auth1New.ECA_Password);
				AssertEquals(auth1.ECA_AuthorizationMode, auth1New.ECA_AuthorizationMode);
				AssertEquals(auth1.ECA_AuthorizationEndpoint, auth1New.ECA_AuthorizationEndpoint);
				AssertEquals(auth1.ECA_Certificate, auth1New.ECA_Certificate);
				AssertEquals(auth1.ECA_ClientID, auth1New.ECA_ClientID);
				AssertEquals("", auth1New.ECA_ClientSecret);
				AssertEquals(auth1.ECA_EncodedPrivateKey, auth1New.ECA_EncodedPrivateKey);
				AssertEquals(auth1.ECA_FlowCode, auth1New.ECA_FlowCode);
				AssertEquals(auth1.ECA_OperationId, auth1New.ECA_OperationId);
				AssertEquals(auth1.ECA_RenewalEncodedPrivateKey, auth1New.ECA_RenewalEncodedPrivateKey);
				AssertEquals(auth1.ECA_RenewalOperationId, auth1New.ECA_RenewalOperationId);
				AssertEquals(auth1.ECA_Scopes, auth1New.ECA_Scopes);
			});
			CombineAssertions(delegate
			{
				AssertEquals("", auth2New.ECA_Username);
				AssertEquals("", auth2New.ECA_Password);
				AssertEquals(auth2.ECA_AuthorizationMode, auth2New.ECA_AuthorizationMode);
				AssertEquals(auth2.ECA_AuthorizationEndpoint, auth2New.ECA_AuthorizationEndpoint);
				AssertEquals(ZBlob.Empty, auth2New.ECA_Certificate);
				AssertEquals(auth2.ECA_ClientID, auth2New.ECA_ClientID);
				AssertEquals(auth2.ECA_ClientSecret, auth2New.ECA_ClientSecret);
				AssertEquals(ZBlob.Empty, auth2New.ECA_EncodedPrivateKey);
				AssertEquals(auth2.ECA_FlowCode, auth2New.ECA_FlowCode);
				AssertEquals("", auth2New.ECA_OperationId);
				AssertEquals(ZBlob.Empty, auth2New.ECA_RenewalEncodedPrivateKey);
				AssertEquals("", auth2New.ECA_RenewalOperationId);
				AssertEquals(auth2.ECA_Scopes, auth2New.ECA_Scopes);
			});
			CombineAssertions(delegate
			{
				AssertEquals(auth3.ECA_Username, auth3New.ECA_Username);
				AssertEquals(auth3.ECA_Password, auth3New.ECA_Password);
				AssertEquals(auth3.ECA_AuthorizationMode, auth3New.ECA_AuthorizationMode);
				AssertEquals(auth3.ECA_AuthorizationEndpoint, auth3New.ECA_AuthorizationEndpoint);
				AssertEquals(ZBlob.Empty, auth3New.ECA_Certificate);
				AssertEquals(auth3.ECA_ClientID, auth3New.ECA_ClientID);
				AssertEquals(auth3.ECA_ClientSecret, auth3New.ECA_ClientSecret);
				AssertEquals(ZBlob.Empty, auth3New.ECA_EncodedPrivateKey);
				AssertEquals(auth3.ECA_FlowCode, auth3New.ECA_FlowCode);
				AssertEquals("", auth3New.ECA_OperationId);
				AssertEquals(ZBlob.Empty, auth3New.ECA_RenewalEncodedPrivateKey);
				AssertEquals("", auth3New.ECA_RenewalOperationId);
				AssertEquals(auth3.ECA_Scopes, auth3New.ECA_Scopes);
			});
		}

		EDICommunicationAuth PrepareCommunicationAuthWithDummyData(EDICommunicationAuth auth, string flowCode = eAdaptorNextOutboundGrantTypesList.Codes.ClientCredentials)
		{
			auth.ECA_Username = "TestName";
			auth.ECA_Password = "TestPassword";
			auth.ECA_AuthorizationEndpoint = "www.test.com";
			auth.ECA_Certificate = Encoding.UTF8.GetBytes("Test Certificate");
			auth.ECA_ClientID = "Test Client ID";
			auth.ECA_ClientSecret = "Test Secret";
			auth.ECA_EncodedPrivateKey = Encoding.UTF8.GetBytes("Test Encoded PrivateKey");
			auth.ECA_FlowCode = flowCode;
			auth.ECA_OperationId = "Test Operation ID";
			auth.ECA_RenewalEncodedPrivateKey = Encoding.UTF8.GetBytes("Test Renewal Encoded PrivateKey");
			auth.ECA_RenewalOperationId = "Test Renewal Operation ID";
			auth.ECA_Scopes = "Test Scope 1, Test Scope 2";
			return auth;
		}

		EDICommunicationParty PrepareCommunicationParty(EDICommunicationAuth auth, string direction)
		{
			var commParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var config = commParty.Configs.AddNew();
			config.ECC_Direction = direction;
			config.ECC_ECA_Auth = auth.PK;
			return commParty;
		}

		public void TestRenewalEncodedPrivateKeyFillsAfterSetPrivateKey()
		{
			var auth = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;

			AssertNull(auth.TwoWayEncoder.Decrypt(auth.ECA_RenewalEncodedPrivateKey));

			auth.SetPrivateKeyAndCsr("EDIClientName");

			AssertEquals(false, auth.ECA_RenewalEncodedPrivateKey.IsEmpty);
			AssertEquals(true, auth.ECA_EncodedPrivateKey.IsEmpty);
		}

		public void TestInvalidValueForFlowCode()
		{
			var auth = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			PrepareCommunicationParty(auth, EDICommunicationPartyConfigDirectionsList.Codes.Outbound);
			PrepareCommunicationAuthWithDummyData(auth);

			auth.ECA_FlowCode = "AAA";

			AssertHasError(auth.ECA_FlowCodeInfo, "Enter a valid selection.");
		}

		public void TestInvalidValueForAuthorizationMode()
		{
			var auth1 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth1.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			var auth2 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth2.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;

			PrepareCommunicationParty(auth1, EDICommunicationPartyConfigDirectionsList.Codes.Inbound);
			PrepareCommunicationParty(auth2, EDICommunicationPartyConfigDirectionsList.Codes.Outbound);
			PrepareCommunicationAuthWithDummyData(auth1);
			PrepareCommunicationAuthWithDummyData(auth2);

			auth1.ECA_AuthorizationMode = "AAA";
			auth2.ECA_AuthorizationMode = "BBB";

			AssertHasError(auth1.ECA_AuthorizationModeInfo, "Enter a valid selection.");
			AssertHasError(auth2.ECA_AuthorizationModeInfo, "Enter a valid selection.");
		}

		public void TestMandatoryFieldsForInboundOAuthWhenIsSelfManaged()
		{
			EnvProxy.SetHostedLocationForTest("");

			var auth = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;

			var commParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var config = commParty.Configs.AddNew();
			config.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			config.ECC_ECA_Auth = auth.PK;
			config.ECC_IsSelfManaged = true;

			auth.ECA_AuthorizationEndpoint = "";
			auth.ECA_ClientID = "";
			auth.Validation.ValidateAll();

			AssertHasError(auth.ECA_AuthorizationEndpointInfo, "Please enter a value.");
			AssertHasError(auth.ECA_ClientIDInfo, "Please enter a value.");

			auth.ECA_AuthorizationEndpoint = "1";
			auth.ECA_ClientID = "2";
			auth.Validation.ValidateAll();

			AssertNoError(auth.ECA_AuthorizationEndpointInfo, "Please enter a value.");
			AssertNoError(auth.ECA_ClientIDInfo, "Please enter a value.");
		}

		public void TestWarningECA_AuthorizationEndpointWhenIsSelfManaged()
		{
			EnvProxy.SetHostedLocationForTest("");

			var auth = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;

			var commParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var config = commParty.Configs.AddNew();
			config.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			config.ECC_ECA_Auth = auth.PK;
			config.ECC_IsSelfManaged = true;

			auth.ECA_AuthorizationEndpoint = "google.com";
			auth.Validation.ValidateAll();
			Assert("Has Warnings", auth.HasWarnings);
			Assert(auth.ECA_AuthorizationEndpointInfo.HasWarning("We currently offer official support only for Azure AD B2C. Using any other identity provider may result in unexpected behavior, and such use is at your own risk."));

			auth.ECA_AuthorizationEndpoint = "https://login.microsoftonline.com/tenantId/v2.0.";
			auth.Validation.ValidateAll();
			Assert("Has Warnings", auth.HasWarnings);
			Assert(auth.ECA_AuthorizationEndpointInfo.HasWarning("We currently offer official support only for Azure AD B2C. Using any other identity provider may result in unexpected behavior, and such use is at your own risk."));

			auth.ECA_AuthorizationEndpoint = "https://login.microsoftonline.com/tenantId/v2.0";
			auth.Validation.ValidateAll();
			Assert(!auth.HasWarnings);

			auth.ECA_AuthorizationEndpoint = "https://login.microsoftonline.com/tenantId/v2.0/";
			auth.Validation.ValidateAll();
			Assert(!auth.HasWarnings);
		}
	}
}
