using System;
using System.Net.Http;
using System.Threading;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.Foundation.Http;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class BoleroOnBoardingHelperTest : TestCaseWithFactory
	{
		public void TestSendEnrollmentRequest()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "contact1@email.em";
			org.Contacts.Add(contact);
			Factory.Save();

			var helper = new BoleroOnBoardingHelperForTest(org);
			var boleroEnrollmentService = new HttpServiceForTest
			{
				Delay = 0,
				Methods = ["POST"],
				Processor = (_, request) => new Tuple<int, string>(200, @"
{
  ""opCode"": 200,
  ""status"": ""SUCCESS"",
  ""additionalInfo"": { }
}"),
				Uri = new Uri(helper.GetRequestUrlForTest()),
				ContentType = "application/json"
			};

			using (ObjectFactory.Substitute<IAuthorizationTokenProvider>(new AuthorizationTokenProviderForTest()))
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo()))
			{
				try
				{
					boleroEnrollmentService.Start();
					Assert(boleroEnrollmentService.IsStarted);

					var invitationDetails = new BoleroInvitationDetails(org) { YourMessage = "Test Message" };
					invitationDetails.SelectedContactPK = contact.PK;
					var requestDTO = new OnboardingRequestDTO(invitationDetails);
					var errorMessage = AsyncTaskSynchronizer.Run(() => helper.SendEnrollmentRequestAsync(requestDTO, CancellationToken.None));
					AssertEquals(string.Empty, errorMessage);
				}
				finally
				{
					if (boleroEnrollmentService.IsStarted)
					{
						boleroEnrollmentService.Stop();
					}
				}
			}
		}

		public void TestSendEnrollmentRequest_HttpTimeoutException()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "contact1@email.em";
			org.Contacts.Add(contact);
			Factory.Save();

			var boleroEBLConfiguration = new BoleroEBLForOrganisationConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString(),
				Timeout = 100
			};

			using (OrganisationRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			using (ObjectFactory.Substitute<IAuthorizationTokenProvider>(new AuthorizationTokenProviderForTest()))
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo()))
			{
				var invitationDetails = new BoleroInvitationDetails(org) { YourMessage = "Test Message" };
				invitationDetails.SelectedContactPK = contact.PK;
				var requestDTO = new OnboardingRequestDTO(invitationDetails);

				var query = new ZQuery(StmALogSchema.SL_Parent, org.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.MessageRejectedCode);
				var logs = Factory.Load<StmALog>(query);
				AssertEquals("Precondition: ", 0, logs.Length);

				var helper = new BoleroOnBoardingHelperForTest(org, new HttpTimeoutException("Request Timed out."));
				var errorMessage = AsyncTaskSynchronizer.Run(() => helper.SendEnrollmentRequestAsync(requestDTO, CancellationToken.None));
				AssertEquals("The request to Bolero Service is timed out, please try again later. You can also increase the timeout value for calling the web service (current value is 100 seconds). If the issue persists please raise a Customer Service Incident.", errorMessage);

				logs = Factory.Load<StmALog>(query);
				AssertEquals(1, logs.Length);
				AssertEquals(logs[0].SL_Reference, "|DEP=CargoWise|MST=Enrolment Request|RES=In case of timeout");
			}
		}

		public void TestSendEnrollmentRequest_AuthenticationException()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "contact1@email.em";
			org.Contacts.Add(contact);
			Factory.Save();

			using (ObjectFactory.Substitute<IAuthorizationTokenProvider>(new AuthorizationTokenProviderForTest()))
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemToSystemTrustInfo()))
			{
				var invitationDetails = new BoleroInvitationDetails(org) { YourMessage = "Test Message" };
				invitationDetails.SelectedContactPK = contact.PK;

				var helper = new BoleroOnBoardingHelperForTest(org);
				var requestDTO = new OnboardingRequestDTO(invitationDetails);
				var errorMessage = AsyncTaskSynchronizer.Run(() => helper.SendEnrollmentRequestAsync(requestDTO, CancellationToken.None));
				AssertEquals("Failed to authenticate. Error Message: System to system trust certificate not found.", errorMessage);
			}
		}

		public void TestSendEnrollmentRequest_HttpRequestException()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "contact1@email.em";
			org.Contacts.Add(contact);
			Factory.Save();

			var invitationDetails = new BoleroInvitationDetails(org) { YourMessage = "Test Message" };
			invitationDetails.SelectedContactPK = contact.PK;
			var requestDTO = new OnboardingRequestDTO(invitationDetails);

			var boleroEBLConfiguration = new BoleroEBLForOrganisationConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (OrganisationRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			using (ObjectFactory.Substitute<IAuthorizationTokenProvider>(new AuthorizationTokenProviderForTest()))
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo()))
			{
				var helper = new BoleroOnBoardingHelper(org);
				var errorMessage = AsyncTaskSynchronizer.Run(() => helper.SendEnrollmentRequestAsync(requestDTO, CancellationToken.None));
				AssertContains("Failed to communicate successfully to Bolero Enrollment Web Service.\r\nPlease contact your I.T. person and confirm that the service URIs are up to date.", errorMessage);
			}
		}

		public void TestSendEnrollmentRequest_GeneralException()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "contact1@email.em";
			org.Contacts.Add(contact);
			Factory.Save();

			var helper = new BoleroOnBoardingHelperForTest(org);
			var boleroEnrollmentService = new HttpServiceForTest
			{
				Delay = 0,
				Methods = ["POST"],
				Processor = (_, request) => new Tuple<int, string>(200, @"Some Invalid Response."),
				Uri = new Uri(helper.GetRequestUrlForTest()),
				ContentType = "application/json"
			};

			using (ObjectFactory.Substitute<IAuthorizationTokenProvider>(new AuthorizationTokenProviderForTest()))
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo()))
			{
				try
				{
					boleroEnrollmentService.Start();
					Assert(boleroEnrollmentService.IsStarted);

					var invitationDetails = new BoleroInvitationDetails(org) { YourMessage = "Test Message" };
					invitationDetails.SelectedContactPK = contact.PK;
					var requestDTO = new OnboardingRequestDTO(invitationDetails);

					var errorMessage = AsyncTaskSynchronizer.Run(() => helper.SendEnrollmentRequestAsync(requestDTO, CancellationToken.None));
					AssertContains("Error Message: Unexpected character encountered while parsing value:", errorMessage);
				}
				finally
				{
					if (boleroEnrollmentService.IsStarted)
					{
						boleroEnrollmentService.Stop();
					}
				}
			}
		}

		public void TestHttpClient()
		{
			var boleroEBLConfiguration = new BoleroEBLForOrganisationConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString(),
				Timeout = 45
			};

			using (ObjectFactory.Substitute<IAuthorizationTokenProvider>(new AuthorizationTokenProviderForTest()))
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo()))
			using (OrganisationRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var helper = new BoleroOnBoardingHelperForTest(null);
				var httpClient = helper.GetHttpClientForTest();

				AssertEquals(TimeSpan.FromSeconds(45), httpClient.Timeout);
				AssertEquals("Bearer", httpClient.DefaultRequestHeaders.Authorization.Scheme);
				AssertEquals("application/json", httpClient.DefaultRequestHeaders.Accept.ToString());
				AssertEquals("CargoWise Next", httpClient.DefaultRequestHeaders.UserAgent.ToString());
			}
		}

		public void TestGetBoleroUrlFromRegistry()
		{
			var boleroEBLConfiguration = new BoleroEBLForOrganisationConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "https://www.production/galileo-portal/",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "https://www.test/galileo-portal/",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (OrganisationRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var helper = new BoleroOnBoardingHelperForTest(org);
				AssertEquals("https://www.test/galileo-portal/onboarding/enrolment/rest/enrolment-management/enrol", helper.GetOriginalUrl());
			}
		}
	}

	class BoleroOnBoardingHelperForTest : BoleroOnBoardingHelper
	{
		readonly int fixedPort = HttpServiceForTest.GetFreeTcpPort();

		readonly Exception mockedException;

		public BoleroOnBoardingHelperForTest(OrgHeader orgHeader, Exception mockedException = null) : base(orgHeader)
		{
			this.mockedException = mockedException;
		}

		protected override string GetRequestUrl()
		{
			if (mockedException == null)
			{
				return GetRequestUrlForTest();
			}

			throw mockedException;
		}

		public string GetRequestUrlForTest() => $"http://localhost:{fixedPort}/bolero/";

		public HttpClient GetHttpClientForTest() => GetHttpClient();

		public string GetOriginalUrl() => base.GetRequestUrl();
	}
}
