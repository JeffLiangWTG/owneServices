using System;
using CargoWise.Application;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Registry.Business;
using Newtonsoft.Json;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class DeniedPartyScreeningHttpServiceForTest : IDisposable
	{
		public DeniedPartyScreeningHttpServiceForTest(bool mockException)
		{
			testUri = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}";
			this.mockException = mockException;
			service = GetServicesForTest();
			service.Start();

			var certificateInfo = AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo();

			setRegistryTemporaryValue = OrganisationsDataRegistry.Instance.DeniedPartyScreeningWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, SetRegistryURL());
			tokenProvider = ObjectFactory.Substitute(nameof(IAuthorizationTokenProvider), new AuthorizationTokenProviderForTest());
			mdmSupportCertificateForDPS = OrganisationsDataRegistry.Instance.MDMSupportCertificateForDPS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certificateInfo);
		}

		public HttpServiceForTest service;
		readonly bool mockException;
		readonly string testUri;
		readonly IDisposable setRegistryTemporaryValue;
		readonly IDisposable tokenProvider;
		readonly IDisposable mdmSupportCertificateForDPS;

		HttpServiceForTest GetServicesForTest()
		{
			var response = new DpsResponse { NameMatches = Array.Empty<NameMatchInfo>(), AddressMatches = Array.Empty<AddressMatchInfo>(), RegistrationCodeMatches = Array.Empty<RegistrationCodeMatchInfo>(), CountryMatches = Array.Empty<CountryMatchInfo>(), Profiles = Array.Empty<ProfileHeaderInfo>() };

			service = new HttpServiceForTest
			{
				Delay = 0,
				Methods = new[] { "POST" },
				Processor = (uri, request) => mockException ? new Tuple<int, string>(500, string.Empty) : new Tuple<int, string>(200, JsonConvert.SerializeObject(response)),
				Uri = new Uri(testUri),
				ContentType = "application/json"
			};

			return service;
		}

		DpsWebServiceItemCollection SetRegistryURL()
		{
			return new DpsWebServiceItemCollection
			{
				new DpsWebServiceItem() { Code = "TST1", WebServiceUrl = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}" , Role = RoleHelper.Code.Production },
				new DpsWebServiceItem() { Code = "TST2", WebServiceUrl = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}", Role = RoleHelper.Code.ProductionFailover },
				new DpsWebServiceItem() { Code = "TST3", WebServiceUrl = testUri, Role = RoleHelper.Code.Staging },
			};
		}

		public void Dispose()
		{
			if (service != null && service.IsStarted)
			{
				service.Stop();
			}

			setRegistryTemporaryValue.Dispose();
		}
	}
}
