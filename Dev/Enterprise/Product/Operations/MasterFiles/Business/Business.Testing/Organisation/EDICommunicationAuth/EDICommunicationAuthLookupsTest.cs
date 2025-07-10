using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Licensing;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing;

public class EDICommunicationAuthLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestEnableInboundAuth()
	{
		var connectionType = "SUP";
		var accessTypes = new HashSet<AccessRequirement>() { AccessRequirement.SupportsInbound, AccessRequirement.SupportsInboundOAuth };

		using (ObjectFactory.Substitute(SetupMockDescriptors(accessTypes, connectionType).Object))
		{
			var party = Factory.NewWithValidTestData<EDICommunicationParty>();
			party.ECP_ApplicationCode = connectionType;

			var modesList = party.InboundConfig.Auth.Lookups.AuthModesList;
			AssertEquals(1, modesList.Count);
			AssertEquals(EDICommunicationAuthModesList.Codes.OAuthAuthentication, modesList[0].Code);
		}
	}

	public void TestEnableOutboundNoAuth()
	{
		var connectionType = "SUP";
		var accessTypes = new HashSet<AccessRequirement>() { AccessRequirement.SupportsOutbound, AccessRequirement.SupportsOutboundNoAuth };

		using (ObjectFactory.Substitute(SetupMockDescriptors(accessTypes, connectionType).Object))
		{
			var party = Factory.NewWithValidTestData<EDICommunicationParty>();
			party.ECP_ApplicationCode = connectionType;

			var modesList = party.OutboundConfig.Auth.Lookups.AuthModesList;
			AssertEquals(1, modesList.Count);
			AssertEquals(EDICommunicationAuthModesList.Codes.NoAuthentication, modesList[0].Code);
		}
	}

	public void TestDisableInboundWithoutSupportsInbound()
	{
		var connectionType = "SUP";
		var accessTypes = new HashSet<AccessRequirement>() { AccessRequirement.SupportsInboundOAuth, AccessRequirement.RequiresBranch };

		using (ObjectFactory.Substitute(SetupMockDescriptors(accessTypes, connectionType).Object))
		{
			var party = Factory.NewWithValidTestData<EDICommunicationParty>();
			party.ECP_ApplicationCode = connectionType;

			var modesList = party.InboundConfig.Auth.Lookups.AuthModesList;
			AssertEquals(0, modesList.Count);
		}
	}

	public void TestDisableOutboundWithoutSupportsOutbound()
	{
		var connectionType = "SUP";
		var accessTypes = new HashSet<AccessRequirement>() { AccessRequirement.SupportsOutboundNoAuth };
		using (ObjectFactory.Substitute(SetupMockDescriptors(accessTypes, connectionType).Object))
		{
			var party = Factory.NewWithValidTestData<EDICommunicationParty>();
			party.ECP_ApplicationCode = connectionType;

			var modesList = party.OutboundConfig.Auth.Lookups.AuthModesList;
			AssertEquals(0, modesList.Count);
		}
	}

	Mock<IEDIClientApplicationDescriptors> SetupMockDescriptors(HashSet<AccessRequirement> accessTypes, string connectionType)
	{
		var applicationDescriptor = new Mock<IEDIClientApplicationDescriptor>();
		applicationDescriptor.SetupGet(d => d.Code).Returns(connectionType);
		applicationDescriptor.SetupGet(d => d.Description).Returns("eAdaptorSupport");
		applicationDescriptor.SetupGet(d => d.AccessTypes).Returns(accessTypes);

		IEnumerable<IEDIClientApplicationDescriptor> applicationDescriptors = new List<IEDIClientApplicationDescriptor> { applicationDescriptor.Object };
		var mockDescriptors = new Mock<IEDIClientApplicationDescriptors>();
		mockDescriptors.Setup(d => d.GetValue(connectionType)).Returns(applicationDescriptor.Object);
		mockDescriptors.SetupGet(d => d.Values).Returns(applicationDescriptors);

		return mockDescriptors;
	}

	public void TestBasicAuthIsNotAvailableForCloudHostedCustomers()
	{
		DoTestAndVerifyBasicAuthOptionAvailability("SYD", false);
	}

	public void TestBasicAuthIsAvailableForSelfHostedCustomers()
	{
		DoTestAndVerifyBasicAuthOptionAvailability(LicenceConstants.NotHostedWithCargoWise, true);
	}

	void DoTestAndVerifyBasicAuthOptionAvailability(string hostedLocation, bool shouldBeAvailable)
	{
		var productRegistrationKeyMock = new Mock<IProductRegistrationKey>();
		productRegistrationKeyMock.SetupGet(k => k.HostedLocation).Returns(hostedLocation);

		var productRegistrationMock = new Mock<IProductRegistration>();
		productRegistrationMock.SetupGet(r => r.Key).Returns(productRegistrationKeyMock.Object);

		using (ObjectFactory.Substitute(productRegistrationMock.Object))
		{
			var party = Factory.NewWithValidTestData<EDICommunicationParty>();
			AssertEquals(shouldBeAvailable, party.InboundConfig.Auth.Lookups.AuthModesList.ContainsCode(EDICommunicationAuthModesList.Codes.BasicAuthentication));
		}
	}
}
