using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EDICommunicationPartyConfig))]
	public class EDICommunicationPartyConfigTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUniquenessCheckOfUsernameOnInboundConfigWithBasicAuthOnAdd()
		{
			var auth1 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth1.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;
			auth1.ECA_Username = "dummy";

			var auth2 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth2.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;
			auth2.ECA_Username = "dummy";

			Factory.Save();

			var oldCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var oldConfig = oldCommParty.Configs.AddNew();
			oldConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			oldConfig.ECC_ECA_Auth = auth1.PK;

			Factory.Save();

			var newCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var newConfig = oldCommParty.Configs.AddNew();
			newConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			newConfig.ECC_ECA_Auth = auth2.PK;

			AssertExceptionThrown<ZCannotSaveException>("Save() should fail trying to save inbound config with duplicate username", "Cannot have two inbound configurations with basic authentication using the same username. Please choose another username.", () => Factory.Save());
		}

		public void TestUniquenessCheckOfUsernameOnInboundConfigWithBasicAuthOnEdit()
		{
			var auth1 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth1.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;
			auth1.ECA_Username = "dummy";

			var auth2 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth2.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;
			auth2.ECA_Username = "dummy";

			Factory.Save();

			var oldCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var oldConfig = oldCommParty.Configs.AddNew();
			oldConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			oldConfig.ECC_ECA_Auth = auth1.PK;

			var newCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var newConfig = oldCommParty.Configs.AddNew();
			newConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;
			newConfig.ECC_ECA_Auth = auth2.PK;

			Factory.Save();

			newConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;

			AssertExceptionThrown<ZCannotSaveException>("Save() should fail trying to save inbound config with duplicate username", "Cannot have two inbound configurations with basic authentication using the same username. Please choose another username.", () => Factory.Save());
		}

		public void TestNoUniquenessCheckOfUsernameOnOutboundConfigWithBasicAuthOnAdd()
		{
			var auth1 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth1.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;
			auth1.ECA_Username = "dummy";

			var auth2 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth2.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;
			auth2.ECA_Username = "dummy";

			Factory.Save();

			var oldCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var oldConfig = oldCommParty.Configs.AddNew();
			oldConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;
			oldConfig.ECC_ECA_Auth = auth1.PK;

			Factory.Save();

			var newCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var newConfig = oldCommParty.Configs.AddNew();
			newConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;
			newConfig.ECC_ECA_Auth = auth2.PK;

			AssertNoExceptionThrown("Save() shouldn't fail trying to save outbound config with duplicate username", () => Factory.Save());
		}

		public void TestNoUniquenessCheckOfUsernameOnOutboundConfigWithBasicAuthOnEdit()
		{
			var auth1 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth1.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;
			auth1.ECA_Username = "dummy";

			var auth2 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth2.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;
			auth2.ECA_Username = "dummy";

			Factory.Save();

			var oldCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var oldConfig = oldCommParty.Configs.AddNew();
			oldConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;
			oldConfig.ECC_ECA_Auth = auth1.PK;

			var newCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var newConfig = oldCommParty.Configs.AddNew();
			newConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			newConfig.ECC_ECA_Auth = auth2.PK;

			Factory.Save();

			newConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;

			AssertNoExceptionThrown("Save() shouldn't fail trying to save outbound config with duplicate username", () => Factory.Save());
		}

		#region UniquenessCheck of client ID
		public void TestUniquenessCheckOfClientIDOnInboundConfigWithOAuthAuthOnAdd()
		{
			var auth1 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth1.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			auth1.ECA_ClientID = "dummy";

			var auth2 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth2.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			auth2.ECA_ClientID = "dummy";

			Factory.Save();

			var oldCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var oldConfig = oldCommParty.Configs.AddNew();
			oldConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			oldConfig.ECC_ECA_Auth = auth1.PK;

			Factory.Save();

			var newCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var newConfig = oldCommParty.Configs.AddNew();
			newConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			newConfig.ECC_ECA_Auth = auth2.PK;

			AssertExceptionThrown<ZCannotSaveException>("Save() should fail trying to save inbound config with duplicate Client ID", "Cannot have two inbound configurations with OAuth authentication using the same Client ID. Please choose another Client ID.", () => Factory.Save());
		}

		public void TestUniquenessCheckOfClientIDOnInboundConfigWithOAuthAuthOnEdit()
		{
			var auth1 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth1.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			auth1.ECA_ClientID = "dummy";

			var auth2 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth2.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			auth2.ECA_ClientID = "dummy";

			Factory.Save();

			var oldCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var oldConfig = oldCommParty.Configs.AddNew();
			oldConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			oldConfig.ECC_ECA_Auth = auth1.PK;

			var newCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var newConfig = oldCommParty.Configs.AddNew();
			newConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;
			newConfig.ECC_ECA_Auth = auth2.PK;

			Factory.Save();

			newConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;

			AssertExceptionThrown<ZCannotSaveException>("Save() should fail trying to save inbound config with duplicate Client ID", "Cannot have two inbound configurations with OAuth authentication using the same Client ID. Please choose another Client ID.", () => Factory.Save());
		}

		public void TestNoUniquenessCheckOfClientIDOnOutboundConfigWithOAuthAuthOnAdd()
		{
			var auth1 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth1.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			auth1.ECA_ClientID = "dummy";

			var auth2 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth2.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			auth2.ECA_ClientID = "dummy";

			Factory.Save();

			var oldCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var oldConfig = oldCommParty.Configs.AddNew();
			oldConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;
			oldConfig.ECC_ECA_Auth = auth1.PK;

			Factory.Save();

			var newCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var newConfig = oldCommParty.Configs.AddNew();
			newConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;
			newConfig.ECC_ECA_Auth = auth2.PK;

			AssertNoExceptionThrown("Save() shouldn't fail trying to save outbound config with duplicate Client ID", () => Factory.Save());
		}

		public void TestNoUniquenessCheckOfClientIDOnOutboundConfigWithOAuthAuthOnEdit()
		{
			var auth1 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth1.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;
			auth1.ECA_ClientID = "dummy";

			var auth2 = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth2.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;
			auth2.ECA_ClientID = "dummy";

			Factory.Save();

			var oldCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var oldConfig = oldCommParty.Configs.AddNew();
			oldConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;
			oldConfig.ECC_ECA_Auth = auth1.PK;

			var newCommParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var newConfig = oldCommParty.Configs.AddNew();
			newConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			newConfig.ECC_ECA_Auth = auth2.PK;

			Factory.Save();

			newConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;

			AssertNoExceptionThrown("Save() shouldn't fail trying to save outbound config with duplicate Client ID", () => Factory.Save());
		}
		#endregion

		public void TestMandatoryCheckOfBranchOnInboundConfig()
		{
			var commParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var config = commParty.Configs.AddNew();
			config.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			config.ECC_GB_Branch = Guid.Empty;

			Factory.Save();

			AssertHasErrorContaining(config.ECC_GB_BranchInfo, "Please enter a value.");
		}

		public void TestMandatoryCheckOfDepartmentOnInboundConfig()
		{
			var commParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var config = commParty.Configs.AddNew();
			config.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			config.ECC_GE_Department = Guid.Empty;

			Factory.Save();

			AssertHasErrorContaining(config.ECC_GE_DepartmentInfo, "Please enter a value.");
		}

		public void TestMandatoryCheckInboundConfigWithConnectionType()
		{
			var connectionType = "SUP";
			var accessTypes = new HashSet<AccessRequirement>() { AccessRequirement.SupportsInbound, AccessRequirement.RequiresBranch };

			var applicationDescriptor = new Mock<IEDIClientApplicationDescriptor>();
			applicationDescriptor.SetupGet(d => d.Code).Returns(connectionType);
			applicationDescriptor.SetupGet(d => d.Description).Returns("eAdaptorSupport");
			applicationDescriptor.SetupGet(d => d.AccessTypes).Returns(accessTypes);

			IEnumerable<IEDIClientApplicationDescriptor> applicationDescriptors = new List<IEDIClientApplicationDescriptor> { applicationDescriptor.Object };
			var mockDescriptors = new Mock<IEDIClientApplicationDescriptors>();
			mockDescriptors.Setup(d => d.GetValue(connectionType)).Returns(applicationDescriptor.Object);
			mockDescriptors.SetupGet(d => d.Values).Returns(applicationDescriptors);

			using (ObjectFactory.Substitute(mockDescriptors.Object))
			{
				var party = Factory.NewWithValidTestData<EDICommunicationParty>();
				party.ECP_ApplicationCode = connectionType;

				var config = party.Configs.AddNew();
				config.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
				config.ECC_GB_Branch = Guid.Empty;

				AssertHasErrorContaining(config.ECC_GB_BranchInfo, "Please enter a value.");
			}
		}

		public void TestMandatoryCheckOutboundConfigWithConnectionType()
		{
			var connectionType = "SUP";
			var accessTypes = new HashSet<AccessRequirement>() { AccessRequirement.SupportsOutbound, AccessRequirement.SupportsOutboundNoAuth };

			var applicationDescriptor = new Mock<IEDIClientApplicationDescriptor>();
			applicationDescriptor.SetupGet(d => d.Code).Returns(connectionType);
			applicationDescriptor.SetupGet(d => d.Description).Returns("eAdaptorSupport");
			applicationDescriptor.SetupGet(d => d.AccessTypes).Returns(accessTypes);

			IEnumerable<IEDIClientApplicationDescriptor> applicationDescriptors = new List<IEDIClientApplicationDescriptor> { applicationDescriptor.Object };
			var mockDescriptors = new Mock<IEDIClientApplicationDescriptors>();
			mockDescriptors.Setup(d => d.GetValue(connectionType)).Returns(applicationDescriptor.Object);
			mockDescriptors.SetupGet(d => d.Values).Returns(applicationDescriptors);

			using (ObjectFactory.Substitute(mockDescriptors.Object))
			{
				var auth = Factory.NewWithValidTestData<EDICommunicationAuth>();

				var party = Factory.NewWithValidTestData<EDICommunicationParty>();
				party.ECP_ApplicationCode = connectionType;

				var config = party.Configs.AddNew();
				config.ECC_ECA_Auth = auth.PK;
				config.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;
				config.Party.OutboundConfig.ECC_Endpoint = ZString.Empty;

				AssertHasErrorContaining(config.ECC_EndpointInfo, "Not a valid URI");
			}
		}

		public void TestNotMandatoryCheckInboundConfigWithConnectionType()
		{
			var connectionType = "SUP";
			var accessTypes = new HashSet<AccessRequirement>() { AccessRequirement.SupportsInbound };

			var applicationDescriptor = new Mock<IEDIClientApplicationDescriptor>();
			applicationDescriptor.SetupGet(d => d.Code).Returns(connectionType);
			applicationDescriptor.SetupGet(d => d.Description).Returns("eAdaptorSupport");
			applicationDescriptor.SetupGet(d => d.AccessTypes).Returns(accessTypes);

			IEnumerable<IEDIClientApplicationDescriptor> applicationDescriptors = new List<IEDIClientApplicationDescriptor> { applicationDescriptor.Object };
			var mockDescriptors = new Mock<IEDIClientApplicationDescriptors>();
			mockDescriptors.Setup(d => d.GetValue(connectionType)).Returns(applicationDescriptor.Object);
			mockDescriptors.SetupGet(d => d.Values).Returns(applicationDescriptors);

			using (ObjectFactory.Substitute(mockDescriptors.Object))
			{
				var party = Factory.NewWithValidTestData<EDICommunicationParty>();
				party.ECP_ApplicationCode = connectionType;

				var config = party.Configs.AddNew();
				config.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
				config.ECC_GB_Branch = Guid.Empty;
				config.ECC_GE_Department = Guid.Empty;

				AssertNoErrors(config.ECC_GB_BranchInfo);
				AssertNoErrors(config.ECC_GE_DepartmentInfo);
			}
		}

		public void TestNotMandatoryCheckOutboundConfigWithConnectionType()
		{
			var connectionType = "SUP";
			var accessTypes = new HashSet<AccessRequirement>() { AccessRequirement.SupportsOutbound };

			var applicationDescriptor = new Mock<IEDIClientApplicationDescriptor>();
			applicationDescriptor.SetupGet(d => d.Code).Returns(connectionType);
			applicationDescriptor.SetupGet(d => d.Description).Returns("eAdaptorSupport");
			applicationDescriptor.SetupGet(d => d.AccessTypes).Returns(accessTypes);

			IEnumerable<IEDIClientApplicationDescriptor> applicationDescriptors = new List<IEDIClientApplicationDescriptor> { applicationDescriptor.Object };
			var mockDescriptors = new Mock<IEDIClientApplicationDescriptors>();
			mockDescriptors.Setup(d => d.GetValue(connectionType)).Returns(applicationDescriptor.Object);
			mockDescriptors.SetupGet(d => d.Values).Returns(applicationDescriptors);

			using (ObjectFactory.Substitute(mockDescriptors.Object))
			{
				var party = Factory.NewWithValidTestData<EDICommunicationParty>();
				party.ECP_ApplicationCode = connectionType;

				var config = party.Configs.AddNew();
				config.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;
				config.Party.ECP_IsActive = false;
				config.Party.OutboundConfig.ECC_Endpoint = ZString.Empty;

				AssertNoErrors(config.ECC_EndpointInfo);
			}
		}

		public void TestECC_IsSelfManagedCannotBeTrueWhenIsNotSelfHosted()
		{
			var auth = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;

			var commParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var config = commParty.Configs.AddNew();
			config.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			config.ECC_ECA_Auth = auth.PK;
			config.ECC_IsSelfManaged = true;

			EnvProxy.SetHostedLocationForTest("hosted-cw1.test");
			Assert(EnvProxy.IsHostedWithCargowise);

			AssertExceptionThrown<ZCannotSaveException>("Save() should fail trying to save inbound config with IsSelfManaged settings when customer is not self-hosted ", "Invalid operation. This customer is not self-hosted and therefore cannot configure a Self-Managed OAuth Identity Provider.", () => Factory.Save());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var party = Factory.NewWithValidTestData<EDICommunicationParty>();
			var config = party.Configs.AddNew();
			return config;
		}
	}
}
