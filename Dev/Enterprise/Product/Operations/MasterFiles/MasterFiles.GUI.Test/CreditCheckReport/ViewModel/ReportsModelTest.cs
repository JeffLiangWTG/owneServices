using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class ReportsModelTest : TestCaseWithFactory
	{
		public void TestReportsModel()
		{
			var reportItemModels = new List<ReportItemModel>()
			{
				new ReportItemModel(CreditReportType.CommercialBureauEnquiry),
				new ReportItemModel(CreditReportType.LatePaymentRisk),
			};

			var model = new ReportsModel(reportItemModels, null, null, null);

			CombineAssertions(() =>
			{
				AssertEquals(2, model.Reports.Count);
				AssertEquals(true, model.Reports[0].GetReportLineVisible);
				AssertEquals(false, model.Reports[1].GetReportLineVisible);
			});
		}

		public void TestSetReportItemToolTip_NoPurchasedReport()
		{
			AsyncTaskSynchronizer.Run(AssertSetReportItemToolTip_NoPurchasedReport);
		}

		async Task AssertSetReportItemToolTip_NoPurchasedReport()
		{
			using (var configForTest = new SupportCreditCheckForTest())
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetCreditReportItemCollectionForTest()))
			{
				configForTest.BindingSource.DataSource = GetOrgForTest();

				var mainPageModel = new MainPageModel();
				var client = new CreditCheckServiceForTest();
				mainPageModel.TopBannerModel = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "WiseTech Global Limited");
				mainPageModel.ReportsModel = new ReportsModel(new List<ReportItemModel>() { new ReportItemModel(CreditReportType.ComprehensiveReport) }, configForTest, client, mainPageModel);

				var eventsBannerModel = new EventsBannerModel(configForTest, client, mainPageModel);
				mainPageModel.EventsBannerModel = eventsBannerModel;
				await eventsBannerModel.LoadCreditEventsOrSilentCompanyLookup();

				mainPageModel.ReportsModel.SetReportItemToolTip();
				var reportItemModel = mainPageModel.ReportsModel.Reports.Single();

				CombineAssertions(() =>
				{
					AssertEquals(true, reportItemModel.ToolTipIconVisible);
					AssertEquals(2, reportItemModel.RelatedLatestEvents.Count());
					AssertEquals(ResourceStringHelper.StatusHeaderWarning, mainPageModel.TopBannerModel.StatusHeader);
					AssertEquals(ResourceStringHelper.GetEventsInfo(2), mainPageModel.TopBannerModel.StatusDetailsEventsInfo);
				});
			}
		}

		public void TestSetReportItemToolTip_HasPurchasedReport()
		{
			AsyncTaskSynchronizer.Run(AssertSetReportItemToolTip_HasPurchasedReport);
		}

		async Task AssertSetReportItemToolTip_HasPurchasedReport()
		{
			using (var configForTest = new SupportCreditCheckForTest())
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetCreditReportItemCollectionForTest()))
			{
				configForTest.BindingSource.DataSource = GetOrgForTest();
				configForTest.PurchasedReportsForTest = new[] { (CreditReportType.ComprehensiveReport, DateTime.Today.AddMonths(-4)) };

				var mainPageModel = new MainPageModel();
				var client = new CreditCheckServiceForTest();
				mainPageModel.TopBannerModel = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "WiseTech Global Limited");
				mainPageModel.ReportsModel = new ReportsModel(new List<ReportItemModel>() { new ReportItemModel(CreditReportType.ComprehensiveReport) }, configForTest, client, mainPageModel);

				var eventsBannerModel = new EventsBannerModel(configForTest, client, mainPageModel);
				mainPageModel.EventsBannerModel = eventsBannerModel;
				await eventsBannerModel.LoadCreditEventsOrSilentCompanyLookup();

				mainPageModel.ReportsModel.SetReportItemToolTip();
				var reportItemModel = mainPageModel.ReportsModel.Reports.Single();

				CombineAssertions(() =>
				{
					AssertEquals(true, reportItemModel.ToolTipIconVisible);
					AssertEquals(1, reportItemModel.RelatedLatestEvents.Count());
					AssertEquals(ResourceStringHelper.StatusHeaderWarning, mainPageModel.TopBannerModel.StatusHeader);
					AssertEquals(ResourceStringHelper.GetEventsInfo(1), mainPageModel.TopBannerModel.StatusDetailsEventsInfo);
				});
			}
		}

		public void TestSetReportItemToolTip_WhenBindingDataSourceDisposed()
		{
			AsyncTaskSynchronizer.Run(AssertSetReportItemToolTip_NoBindingDataSource);
		}

		async Task AssertSetReportItemToolTip_NoBindingDataSource()
		{
			var creditReportItem = new CreditReportItem
			{
				CountryCode = "AU",
				CountryEnabledForCompany = true,
				CountryEnabledForOrganisation = true,
				CommercialBureauEnquiryEnabled = false,
				FailureRiskEnabled = false,
				ComprehensiveReportEnabled = true,
				LatePaymentRiskEnabled = false
			};

			var creditReportItemCollection = new CreditReportItemCollection();
			creditReportItemCollection.Add(creditReportItem);
			Factory.Save();

			using (var config = new CreditReportUserControl())
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			{
				config.BindingSource.DataSource = GetOrgForTest();

				var mainPageModel = new MainPageModel();
				var client = new CreditCheckServiceForTest();
				mainPageModel.TopBannerModel = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "WiseTech Global Limited");
				mainPageModel.ReportsModel = new ReportsModel(new List<ReportItemModel>() { new ReportItemModel(CreditReportType.ComprehensiveReport) }, config, client, mainPageModel);

				var eventsBannerModel = new EventsBannerModel(config, client, mainPageModel);
				mainPageModel.EventsBannerModel = eventsBannerModel;
				await eventsBannerModel.LoadCreditEventsOrSilentCompanyLookup();

				config.Dispose();

				AssertNoExceptionThrown(() => mainPageModel.ReportsModel.SetReportItemToolTip());
			}
		}

		CreditReportItemCollection GetCreditReportItemCollectionForTest()
		{
			var creditReportItem = new CreditReportItem
			{
				CountryEnabledForCompany = true,
				CountryEnabledForOrganisation = true,
				CountryCode = "AU",
				CommercialBureauEnquiryEnabled = false,
				FailureRiskEnabled = false,
				ComprehensiveReportEnabled = true,
				LatePaymentRiskEnabled = false
			};

			var creditReportItemCollection = new CreditReportItemCollection() { creditReportItem };

			Factory.Save();

			return creditReportItemCollection;
		}

		OrgHeader GetOrgForTest()
		{
			var orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_CustomsRegNo = "213";
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "I have events";
			org.OH_RL_NKClosestPort = "AU";
			org.CustomsCodes.Add(orgCusCode);

			Factory.Save();

			return org;
		}
	}
}
