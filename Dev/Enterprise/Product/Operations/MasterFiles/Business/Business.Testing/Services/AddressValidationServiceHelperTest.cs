using System;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AddressValidationServiceHelperTest : TestCaseWithFactory
	{
		public void TestBasicAuthenticationValue()
		{
			var header = new AddressValidationServiceHelperForTest().GetAuthenticationHeaderValue(enableSysToSysTrust: false);

			AssertEquals("Basic", header.Scheme);
			AssertEquals("Fake-System-ID:Fake-Password", Encoding.ASCII.GetString(Convert.FromBase64String(header.Parameter)));
		}

		public void TestSystemToSystemTrustAuthenticationValue()
		{
			var helper = new AddressValidationServiceHelperForTest();
			var certificateInfo = AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo();

			using (ObjectFactory.Substitute(nameof(IAuthorizationTokenProvider), new AuthorizationTokenProviderForTest()))
			using (MDMSupportCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certificateInfo))
			{
				var header = helper.GetAuthenticationHeaderValue(true);
				AssertEquals("Bearer", header.Scheme);
				AssertNotNullOrEmpty(header.Parameter);
			}
		}

		public void TestSystemIsNotRegistered()
		{
			var mockIProductRegistrationKey = new Mock<IProductRegistrationKey>();
			mockIProductRegistrationKey.Setup(x => x.SystemId).Returns(default(string));

			var helper = new AddressValidationServiceHelperForTest(mockIProductRegistrationKey.Object);

			AssertExceptionThrown<AuthenticationException>("Address Validation verifies the address information you have entered is correct and can provide suggestions if the address is not found. This service aims to improve data accuracy and reduce futile deliveries. It also provides Coordinates for displaying of addresses on a map and is a pre-requisite for optimization, routing and rating. Only registered systems can utilize this function. Please register and you will be able to obtain the various benefits of this function.", () => helper.GetAuthenticationHeaderValue(enableSysToSysTrust: true));

			AssertExceptionThrown<AuthenticationException>("Address Validation verifies the address information you have entered is correct and can provide suggestions if the address is not found. This service aims to improve data accuracy and reduce futile deliveries. It also provides Coordinates for displaying of addresses on a map and is a pre-requisite for optimization, routing and rating. Only registered systems can utilize this function. Please register and you will be able to obtain the various benefits of this function.", () => helper.GetAuthenticationHeaderValue(enableSysToSysTrust: false));
		}

		MDMSupportCertificateRegistryItem MDMSupportCertificate
		{
			get
			{
				mdmSupportCertificate ??= (MDMSupportCertificateRegistryItem)(typeof(OrganisationRegistry).GetProperty("MDMSupportCertificate", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(OrganisationRegistry.Instance));
				return mdmSupportCertificate;
			}
		}
		MDMSupportCertificateRegistryItem mdmSupportCertificate;
	}

	public class AddressValidationServiceHelperForTest : AddressValidationServiceHelper
	{
		readonly Mock<IProductRegistration> mockProductRegistration = new Mock<IProductRegistration>();

		public AddressValidationServiceHelperForTest()
		{
			var mockIProductRegistrationKey = new Mock<IProductRegistrationKey>();
			mockIProductRegistrationKey.Setup(x => x.SystemId).Returns("Fake-System-ID");
			mockIProductRegistrationKey.Setup(x => x.Password).Returns("Fake-Password");

			mockProductRegistration.Setup(x => x.Key).Returns(mockIProductRegistrationKey.Object);
		}

		public AddressValidationServiceHelperForTest(IProductRegistrationKey productRegistrationKey)
		{
			mockProductRegistration.Setup(x => x.Key).Returns(productRegistrationKey);
		}

		public new AuthenticationHeaderValue GetAuthenticationHeaderValue(bool enableSysToSysTrust)
		{
			using (ObjectFactory.Substitute(mockProductRegistration.Object))
			{
				return base.GetAuthenticationHeaderValue(enableSysToSysTrust);
			}
		}
	}
}
