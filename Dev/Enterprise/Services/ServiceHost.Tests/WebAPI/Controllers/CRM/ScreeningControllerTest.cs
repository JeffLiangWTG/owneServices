using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class ScreeningControllerTest : TestCaseWithFactory
	{
		public void TestScreening_AllPartiesClearAsync()
		{
			var dpsResponse = new DpsResponse { NameMatches = Array.Empty<NameMatchInfo>(), AddressMatches = Array.Empty<AddressMatchInfo>(), RegistrationCodeMatches = Array.Empty<RegistrationCodeMatchInfo>(), CountryMatches = Array.Empty<CountryMatchInfo>(), Profiles = Array.Empty<ProfileHeaderInfo>() };
			var request = new ScreeningRequest()
			{
				Name = "Test Full Name 1",
				Address1 = "Address1",
				Address2 = "Address2",
				CountryRegion = "AU",
				City = "Sydney",
				Postcode = "2015",
				State = "NSW"
			};
			var expectedResponse = new ScreeningResponse()
			{
				HasRisk = false,
				ResponseCode = DpsResponseCode.Successful,
				ExtraMessage = string.Empty
			};
			TestGetScreeningStatusCore(dpsResponse, request, expectedResponse, (response, actionResult) => actionResult.AssertJsonResultEquals(response));
		}

		public void TestScreening_HasMatches()
		{
			var headerPk = Guid.NewGuid();
			var headerNamePk = Guid.NewGuid();
			var nameMatchInfos = new List<NameMatchInfo>()
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "Primary Name", FullName = "Test Full Name 1" }, MatchingNameID = headerNamePk, MatchingNameScore = 100, SourceProfileID = headerPk },
			};
			var profileHeaderInfo = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = headerPk, ProfileNotes = Array.Empty<byte>(), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = headerNamePk, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = headerPk } }, SourceListCodes = new List<string>() { IncludedSourceListCode }, TypeOfEntity = "PER" },
				new ProfileHeaderInfo { SourceProfileID = headerPk, ProfileNotes = Array.Empty<byte>(), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = headerNamePk, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = headerPk } }, SourceListCodes = new List<string>() { ExcludedSourceListCode }, TypeOfEntity = "PER" },
			};
			var dpsResponse = new DpsResponse { NameMatches = nameMatchInfos, AddressMatches = Array.Empty<AddressMatchInfo>(), RegistrationCodeMatches = Array.Empty<RegistrationCodeMatchInfo>(), CountryMatches = Array.Empty<CountryMatchInfo>(), Profiles = profileHeaderInfo };
			var request = new ScreeningRequest()
			{
				Name = "Test Full Name 1",
				Address1 = "Address1",
				Address2 = "Address2",
				CountryRegion = "AU",
				City = "Sydney",
				Postcode = "2015",
				State = "NSW"
			};
			var expectedResponse = new ScreeningResponse()
			{
				HasRisk = true,
				ResponseCode = DpsResponseCode.Successful,
				ExtraMessage = "Screened as risky"
			};
			TestGetScreeningStatusCore(dpsResponse, request, expectedResponse, (response, actionResult) => actionResult.AssertJsonResultEquals(response));
		}

		public void TestScreening_WithError()
		{
			var dpsResponse = new DpsResponse { NameMatches = Array.Empty<NameMatchInfo>(), AddressMatches = Array.Empty<AddressMatchInfo>(), RegistrationCodeMatches = Array.Empty<RegistrationCodeMatchInfo>(), CountryMatches = Array.Empty<CountryMatchInfo>(), Profiles = Array.Empty<ProfileHeaderInfo>(), ResponseCode = DpsResponseCode.Failed, ExtraMessage = "Dummy Error" };
			var request = new ScreeningRequest()
			{
				Name = "Test Full Name 1",
				Address1 = "Address1",
				Address2 = "Address2",
				CountryRegion = "AU",
				City = "Sydney",
				Postcode = "2015",
				State = "NSW"
			};
			var expectedResponse = new ScreeningResponse()
			{
				HasRisk = false,
				ResponseCode = DpsResponseCode.Exception,
				ExtraMessage = string.Empty
			};
			TestGetScreeningStatusCore(dpsResponse, request, expectedResponse, (response, actionResult) =>
			{
				var res = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
				var actualJson = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				var actualResponse = JsonConvert.DeserializeObject<ScreeningResponse>(actualJson);
				AssertEquals(response.HasRisk, actualResponse.HasRisk);
				AssertEquals(response.ResponseCode, actualResponse.ResponseCode);
			}, true);
		}

		public void TestScreening_NeedMoreInfo()
		{
			var request = new ScreeningRequest();
			var expectedResponse = new ScreeningResponse()
			{
				HasRisk = false,
				ResponseCode = DpsResponseCode.Failed,
				ExtraMessage = "Need more information for screening!"
			};
			TestGetScreeningStatusCore(new DpsResponse(), request, expectedResponse, (response, actionResult) => actionResult.AssertJsonResultEquals(response));
		}

		void TestGetScreeningStatusCore(DpsResponse response, ScreeningRequest request, ScreeningResponse expectedResponse, Action<ScreeningResponse, IHttpActionResult> assertActionResult, bool mockException = false)
		{
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dpsCollection))
			using (ObjectFactory.Substitute(nameof(IAuthorizationTokenProvider), new AuthorizationTokenProviderForTest()))
			using (OrganisationsDataRegistry.Instance.MDMSupportCertificateForDPS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certificateInfo))
			using (new StartServicesForTest(response, testUri, mockException))
			{
				var result = controller.GetScreeningStatus(request);
				assertActionResult(expectedResponse, result);
			}
		}

		#region implementation

		ScreeningController controller;

		protected override void SetUp()
		{
			base.SetUp();

			controller = new ScreeningController();
			controller.ControllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage())
			{
				Controller = controller
			};

			var complianceList1 = Factory.NewWithValidTestData<RefComplianceList>();
			complianceList1.RCL_ListCode = ExcludedSourceListCode;
			complianceList1.RCL_IsExcluded = true;

			var complianceList2 = Factory.NewWithValidTestData<RefComplianceList>();
			complianceList2.RCL_ListCode = IncludedSourceListCode;
			complianceList2.RCL_IsExcluded = false;
			Factory.Save();

			var testUrl1 = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}";
			var testUrl2 = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}";

			dpsCollection = new DpsWebServiceItemCollection
			{
				new DpsWebServiceItem()
				{
					Code = "SYD1", WebServiceUrl = (NoResString)testUrl1, Role = RoleHelper.Code.Production
				},
				new DpsWebServiceItem()
				{
					Code = "STG1", WebServiceUrl = (NoResString)testUri, Role = RoleHelper.Code.Staging
				},
				new DpsWebServiceItem()
				{
					Code = "SYD2", WebServiceUrl = (NoResString)testUrl2, Role = RoleHelper.Code.ProductionFailover
				}
			};
		}

		public class StartServicesForTest : IDisposable
		{
			readonly HttpServiceForTest service;

			public void Dispose()
			{
				if (service != null && service.IsStarted)
				{
					service.Stop();
				}
			}

			public StartServicesForTest(DpsResponse response, string testUri, bool mockException)
			{
				service = new HttpServiceForTest
				{
					Delay = 0,
					Methods = new[] { "POST" },
					Processor = (uri, request) => mockException ? new Tuple<int, string>(500, string.Empty) : new Tuple<int, string>(200, JsonConvert.SerializeObject(response)),
					Uri = new Uri(testUri),
					ContentType = "application/json"
				};
				service.Start();
			}
		}

		readonly string testUri = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}";
		readonly SystemToSystemTrustInfo certificateInfo = AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo();
		readonly string ExcludedSourceListCode = "SourceList1";
		readonly string IncludedSourceListCode = "SourceList2";
		DpsWebServiceItemCollection dpsCollection;

		#endregion
	}
}
