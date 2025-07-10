using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	public class DeniedPartyScreeningWorkerTest : TestCaseWithFactory
	{
		public void TestSubmitRequestWithResponse_ShouldThrowAggregateArgumentNullException()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			using (ObjectFactory.Substitute<IDpsManager>(new DpsManagerForTest()))
			using (var form = new ZForm(dummyBizO))
			{
				var exception = AssertExceptionThrown<AggregateException>(() => _ = AsyncTaskSynchronizer.Run(() => DeniedPartyScreeningWorker.SubmitRequestWithResponse(form, null)));
				AssertEquals(typeof(AggregateException), exception.GetType());
				AssertEquals(typeof(ArgumentNullException), exception.InnerException.GetType());
				AssertContains("Should provide proper screening request parties", exception.InnerException.Message);

				exception = AssertExceptionThrown<AggregateException>(() => _ = AsyncTaskSynchronizer.Run(() => DeniedPartyScreeningWorker.SubmitRequestWithResponse(null, new List<ScreeningParty>().ToArray())));
				AssertEquals(typeof(AggregateException), exception.GetType());
				AssertEquals(typeof(ArgumentNullException), exception.InnerException.GetType());
				AssertContains("Should provide proper parent form", exception.InnerException.Message);
			}
		}

		public void TestSubmitRequestWithResponse_WhenIsOrganization()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			using (ObjectFactory.Substitute<IDpsManager>(new DpsManagerForTest()))
			using (var form = new BaseOrganisationsForm(header))
			{
				var screeningParties = new List<ScreeningParty>()
				{
					new ScreeningParty(header, "Organization", header)
				};
				var result = AsyncTaskSynchronizer.Run(() => DeniedPartyScreeningWorker.SubmitRequestWithResponse(form, screeningParties.ToArray()));

				AssertScreeningPartyWithRequestHeaderAndResponse(header, result);
			}
		}

		public void TestSubmitRequestWithResponse_WhenIsVessel()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			using (ObjectFactory.Substitute<IDpsManager>(new DpsManagerForTest()))
			using (var form = new RefVesselForm(vessel))
			{
				var screeningParties = new List<ScreeningParty>()
				{
					new ScreeningParty(vessel, "Vessel", vessel)
				};
				var result = AsyncTaskSynchronizer.Run(() => DeniedPartyScreeningWorker.SubmitRequestWithResponse(form, screeningParties.ToArray()));

				AssertScreeningPartyWithRequestHeaderAndResponse(vessel, result);
			}
		}

		public void TestSubmitRequestWithResponse_WhenIsCountry()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			using (ObjectFactory.Substitute<IDpsManager>(new DpsManagerForTest()))
			using (var form = new RefCountryForm(country))
			{
				var screeningParties = new List<ScreeningParty>()
				{
					new ScreeningParty(country, "Country", country)
				};
				var result = AsyncTaskSynchronizer.Run(() => DeniedPartyScreeningWorker.SubmitRequestWithResponse(form, screeningParties.ToArray()));

				AssertScreeningPartyWithRequestHeaderAndResponse(country, result);
			}
		}

		public void TestSubmitRequestWithResponse_ShouldShowProgressMessage()
		{
			AssertEquals(@"Screening... 8 of 24 parties screened.", DeniedPartyScreeningWorker.ShowProgressMessage(8, 24, false));
			AssertEquals(@"Screening... 8 of 24 parties screened.
Please wait until current party is screened.", DeniedPartyScreeningWorker.ShowProgressMessage(8, 24, true));
		}

		public void TestSubmitRequestWithResponse_WithoutScreeningParties()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			using (ObjectFactory.Substitute<IDpsManager>(new DpsManagerForTest()))
			using (var form = new BaseOrganisationsForm(header))
			{
				var response = AsyncTaskSynchronizer.Run(() => DeniedPartyScreeningWorker.SubmitRequestWithResponse(form, Array.Empty<ScreeningParty>()));

				Assert("Response count should be zero", response.Count == 0);
				AssertNull("Progress form should have not displayed", (ProgressForm)ZFormModaliser.LastFormShownForTest);
			}
		}

		public void TestGetSourceWithParties()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var request = DeniedPartyScreeningWorker.GetSourceWithParties(new BusinessObject[] { country });

			AssertEquals(country, request.SourceWithParties.Single().SourceBizO);
			AssertEquals(country, request.ScreeningParties.Single().Country);

			var header = Factory.NewWithValidTestData<OrgHeader>();
			request = DeniedPartyScreeningWorker.GetSourceWithParties(new BusinessObject[] { header });

			AssertEquals(header, request.SourceWithParties.Single().SourceBizO);
			AssertEquals(header, request.ScreeningParties.Single().Header);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			request = DeniedPartyScreeningWorker.GetSourceWithParties(new BusinessObject[] { vessel });

			AssertEquals(vessel, request.SourceWithParties.Single().SourceBizO);
			AssertEquals(vessel, request.ScreeningParties.Single().Vessel);
		}

		public void TestExcludePermanentClearParties_WhenScreeningStatusIsCLP_ShouldReturmEmptyList()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;

			Factory.Save();

			var screeningParties = DeniedPartyScreeningWorker.ExcludePermanentClearParties(new[]
			{
				new ScreeningParty(header, "Organization", header),
				new ScreeningParty(vessel, "Vessel", vessel),
			});

			CombineAssertions(() =>
			{
				AssertEquals("Expected screening parties count should be 0", 0, screeningParties.Length);
				AssertEquals("Organization should be excluded from screening parties", false, screeningParties.Any(x => x.Header == header));
				AssertEquals("Vessel should be excluded from screening parties", false, screeningParties.Any(x => x.Vessel == vessel));
			});
		}

		public void TestExcludePermanentClearParties_WhenScreeningStatusIsNotCLP_ShouldReturnListOfParties()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();

			Factory.Save();

			var screeningParties = DeniedPartyScreeningWorker.ExcludePermanentClearParties(new[]
			{
				new ScreeningParty(header, "Organization", header),
				new ScreeningParty(vessel, "Vessel", vessel),
			});

			CombineAssertions(() =>
			{
				AssertEquals("Expected screening parties count should be 2", 2, screeningParties.Length);
				AssertEquals("Organization should be included in screening parties", true, screeningParties.Any(x => x.Header == header));
				AssertEquals("Vessel should be included in screening parties", true, screeningParties.Any(x => x.Vessel == vessel));
			});
		}

		void AssertScreeningPartyWithRequestHeaderAndResponse(BusinessObject bizO, List<DpsResponseWithScreeningParty> result)
		{
			CombineAssertions(() =>
			{
				AssertEquals(DpsResponseCode.Successful, result[0].Response.ResponseCode);
				AssertEquals(bizO, result[0].ScreeningParty.ScreeningEntity);
				AssertType<ScreeningParty>("Response should have a type 'ScreeningParty'", result[0].ScreeningParty);
				AssertType<DpsRequestHeaderWithAddressMatching>("Response should have a type 'DpsRequestHeaderWithAddressMatching'", result[0].RequestHeaderWithAddressMatching);
				AssertType<DpsResponse>("Response should have a type 'DpsResponse'", result[0].Response);

				var progressForm = ZFormModaliser.LastFormShownForTest;
				AssertType<ProgressForm>("Progress form should have been displayed", progressForm);
				Assert("Progress form should have been disposed", progressForm.IsDisposed);
				AssertContains("Screening... 1 of 1 parties screened.", (progressForm as ProgressForm).Status);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}
	}

	public class DpsManagerForTest : IDpsManager
	{
		public IDpsServiceV4 GetDpsService(string url)
		{
			return new DpsServiceV4(new HttpClient());
		}

		public List<IDpsServiceV4> GetDpsServices(IDpsServiceV4 service)
		{
			var services = new List<IDpsServiceV4>();
			services.Add(new DpsServiceV4(new HttpClient()));
			return services;
		}

		public void DeactivateBillingEntities(Guid[] entityPKs)
		{
			throw new NotImplementedException();
		}

		public async Task<DpsResponse> Screen(DpsRequestHeaderWithAddressMatching dpsRequestHeaderWithAddressMatching, IDpsServiceV4 service)
		{
			return await Task.FromResult(new DpsResponse { NameMatches = Array.Empty<NameMatchInfo>(), AddressMatches = Array.Empty<AddressMatchInfo>(), RegistrationCodeMatches = Array.Empty<RegistrationCodeMatchInfo>(), CountryMatches = Array.Empty<CountryMatchInfo>(), Profiles = Array.Empty<ProfileHeaderInfo>() });
		}

		public async Task<DpsResponse> GetScreenResult(DpsRequestHeaderWithAddressMatching dpsRequestHeaderWithAddressMatching, List<IDpsServiceV4> services)
		{
			return await Task.FromResult(new DpsResponse { NameMatches = Array.Empty<NameMatchInfo>(), AddressMatches = Array.Empty<AddressMatchInfo>(), RegistrationCodeMatches = Array.Empty<RegistrationCodeMatchInfo>(), CountryMatches = Array.Empty<CountryMatchInfo>(), Profiles = Array.Empty<ProfileHeaderInfo>() });
		}
	}
}
