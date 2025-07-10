using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobServiceInfoTest : TestCaseWithFactory
	{
		public void TestCtor()
		{
			var contractor = Factory.NewWithValidTestData<OrgHeader>();
			var time = new TimeSpan(0);
			var info = new JobServiceInfo(true, "FOO", "BAR", "BAR Description", 1m, time, contractor, 70m, JobServiceInfo.Constants.Codes.Hour);
			AssertEquals(true, info.IsEnabled);
			AssertEquals("FOO", info.ChargeCodeGroup);
			AssertEquals("BAR", info.ServiceCode);
			AssertEquals("BAR Description", info.ServiceDescription);
			AssertEquals(contractor, info.Contractor);
			AssertEquals(70m, info.Rate);
			AssertEquals(time, info.ServiceDuration);
			AssertEquals("HR", info.Unit);

			AssertEquals(1m, info.ServiceCount);
			AssertEquals("", info.LocationCountryCode);
			AssertEquals(ZDateTime.Empty, info.CompletedDate);
		}

		public void TestCtorFromJobService()
		{
			var contractor = Factory.New<OrgHeader>();
			var location = Factory.New<OrgHeader>().MainAddress;
			location.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var jobService = Factory.New<JobService>();
			jobService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			jobService.ES_Completed = new ZDateTime(2014, 10, 20);
			jobService.ES_OH_Contractor = contractor.PK;
			jobService.ES_ServiceCount = 10m;
			jobService.ES_OA_Location = location.PK;
			jobService.ES_ServiceRate = 8.5m;
			jobService.ES_MeasurementBasis = JobServiceInfo.Constants.Codes.ServiceOccurrence;
			jobService.ES_References = "REF1236456";

			var info = new JobServiceInfo(jobService);
			AssertEquals(true, info.IsEnabled);
			AssertEquals("", info.ChargeCodeGroup);
			AssertEquals("FUM", info.ServiceCode);
			AssertEquals("Fumigation", info.ServiceDescription);

			AssertEquals(contractor, info.Contractor);
			AssertEquals(10m, info.ServiceCount);
			AssertEquals(location.OA_RN_NKCountryCode, info.LocationCountryCode);
			AssertEquals(new ZDateTime(2014, 10, 20), info.CompletedDate);

			AssertEquals("Has a contractor so is cost", true, info.IsCostForSpotRate);
			AssertEquals(8.5m, info.Rate);
			AssertEquals("Should have taken currency from location's currency", Core.Constants.CurrencyCodes.Australia, info.Currency);
			AssertEquals(JobServiceInfo.Constants.Codes.ServiceOccurrence, info.Unit);
			AssertEquals("SV", info.Unit);
			AssertEquals("REF1236456", info.ServiceReference);

			jobService.ES_OH_Contractor = ZGuid.Empty;
			info = new JobServiceInfo(jobService);

			AssertNull(info.Contractor);
			AssertEquals("Without a contractor, should not be cost", false, info.IsCostForSpotRate);
		}

		public void TestIsServiceFor()
		{
			var info = new JobServiceInfo(true, "", "BAR", "BAR Description");
			Assert(info.IsServiceFor("", "BAR"));
			Assert(info.IsServiceFor("FOO", "BAR"));

			Assert(!info.IsServiceFor("", "XXX"));
			Assert(!info.IsServiceFor("FOO", "XXX"));

			Assert(!info.IsServiceFor("FOO", ""));
			Assert(!info.IsServiceFor("", ""));
			Assert(!info.IsServiceFor("AAA", "BBB"));

			info = new JobServiceInfo(true, "FOO", "BAR", "BAR Description");
			Assert(info.IsServiceFor("FOO", "BAR"));
			Assert(!info.IsServiceFor("", "BAR"));
			Assert(!info.IsServiceFor("", "XXX"));
			Assert(!info.IsServiceFor("FOO", "XXX"));
			Assert(!info.IsServiceFor("FOO", ""));
			Assert(!info.IsServiceFor("", ""));
			Assert(!info.IsServiceFor("AAA", "BBB"));
		}

		public void TestIsForRateSearch()
		{
			var infoCost = new JobServiceInfo(true, "", "BAR", "BAR Description");
			infoCost.IsCostOrSellForRateSearch = JobServiceInfo.CostOrSell.Cost;
			var infoSell = new JobServiceInfo(true, "", "BAR", "BAR Description");
			infoSell.IsCostOrSellForRateSearch = JobServiceInfo.CostOrSell.Sell;
			var infoBoth = new JobServiceInfo(true, "", "BAR", "BAR Description");
			infoBoth.IsCostOrSellForRateSearch = JobServiceInfo.CostOrSell.Both;

			AssertEquals(true, infoCost.IsCostForRateSearch);
			AssertEquals(false, infoCost.IsSellForRateSearch);
			AssertEquals(true, infoCost.IsForRateSearch(true));
			AssertEquals(false, infoCost.IsForRateSearch(false));

			AssertEquals(false, infoSell.IsCostForRateSearch);
			AssertEquals(true, infoSell.IsSellForRateSearch);
			AssertEquals(false, infoSell.IsForRateSearch(true));
			AssertEquals(true, infoSell.IsForRateSearch(false));

			AssertEquals(true, infoBoth.IsCostForRateSearch);
			AssertEquals(true, infoBoth.IsSellForRateSearch);
			AssertEquals(true, infoBoth.IsForRateSearch(true));
			AssertEquals(true, infoBoth.IsForRateSearch(false));
		}

		public void TestLocationCodeFallback()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Test Client #1";
			orgHeader.MainAddress.OA_Address1 = "100 Fake Street";
			orgHeader.MainAddress.OA_City = "Sydney";
			orgHeader.MainAddress.OA_State = "NSW";
			orgHeader.MainAddress.OA_PostCode = "2000";
			orgHeader.OH_Code = "TESTORG";

			var dummyWithServices = Factory.New<DummyWithServices>();
			var service = dummyWithServices.Services.AddNew();
			service.ES_OA_Location = orgHeader.MainAddress.PK;

			var jobServiceInfo = new JobServiceInfo(service);
			AssertNullOrEmpty(jobServiceInfo.LocationCode);
			AssertNullOrEmpty(jobServiceInfo.LocationCountryCode);

			orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";
			jobServiceInfo = new JobServiceInfo(service);
			AssertEquals("AU", jobServiceInfo.LocationCode);
			AssertEquals("AU", jobServiceInfo.LocationCountryCode);

			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			jobServiceInfo = new JobServiceInfo(service);
			AssertEquals("AUSYD", jobServiceInfo.LocationCode);
			AssertEquals("AU", jobServiceInfo.LocationCountryCode);
		}
	}
}
