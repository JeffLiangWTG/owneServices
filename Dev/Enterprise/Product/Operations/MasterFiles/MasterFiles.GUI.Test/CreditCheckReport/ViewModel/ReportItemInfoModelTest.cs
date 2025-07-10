using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using WTG.CreditCheck;
using WTG.ROPE.Model;
using static Enterprise.MasterFiles.GUI.Test.ImageBitmapTestHelper;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class ReportItemInfoModelTest : TestCaseWithFactory
	{
		public void TestReportItemInfoModel()
		{
			var model = new ReportItemModel(CreditReportType.CommercialBureauEnquiry);

			var infoModel = new ReportItemInfoModel(model, null, null, null);
			CombineAssertions(() =>
			{
				AssertEquals(ResourceStringHelper.GetReportCaption(model.CreditReportType), infoModel.ReportCaption);
				AssertEquals(ResourceStringHelper.GetReport, infoModel.ButtonCaption);
				AssertEquals(true, infoModel.GetReportLineVisible);
				AssertEquals(ResourceStringHelper.MoreInfo, infoModel.LastReportDateOrMoreInfo);
				AssertEquals(ImageSourceHelper.GetCreditEventIconInfo(CreditEventType.ScoreChange).Color, infoModel.ToolTipBackgroundColor);
			});

			model.LastReportDate = new DateTime(2020, 4, 1);

			infoModel = new ReportItemInfoModel(model, null, null, null);
			CombineAssertions(() =>
			{
				AssertEquals(ResourceStringHelper.GetReportCaption(model.CreditReportType), infoModel.ReportCaption);
				AssertEquals(ResourceStringHelper.Renew, infoModel.ButtonCaption);
				AssertEquals(true, infoModel.GetReportLineVisible);
				AssertEquals(ResourceStringHelper.LastReportDate(new DateTime(2020, 4, 1)), infoModel.LastReportDateOrMoreInfo);
				AssertEquals(ImageSourceHelper.GetCreditEventIconInfo(CreditEventType.ScoreChange).Color, infoModel.ToolTipBackgroundColor);
			});
		}

		public void TestLastReportDate_LastReportDateOrMoreInfo_IsGet()
		{
			var model = new ReportItemInfoModel(new ReportItemModel(CreditReportType.FailureRisk), null, null, null);
			AssertEquals("More Info", model.LastReportDateOrMoreInfo);
			AssertNull(model.LastReportDate);
			Assert(model.IsGet);
			AssertEquals("Get Report", model.ButtonCaption);
			model.LastReportDate = new DateTime(2023, 8, 3);
			AssertEquals(new DateTime(2023, 8, 3), model.LastReportDate);
			AssertEquals("Last Report 03-Aug-23", model.LastReportDateOrMoreInfo);
			AssertEquals(false, model.IsGet);
			AssertEquals("Renew", model.ButtonCaption);
			model.LastReportDate = null;
			AssertEquals(null, model.LastReportDate);
			AssertEquals("More Info", model.LastReportDateOrMoreInfo);
			AssertEquals(true, model.IsGet);
			AssertEquals("Get Report", model.ButtonCaption);
		}

		public void TestSecurityCheckpointDefault()
		{
			var model = new ReportItemModel(CreditReportType.ComprehensiveReport);

			using (var configForTest = new SupportCreditCheckForTest())
			{
				configForTest.SecurityCheckpointsForTest = new Dictionary<(CreditReportType ReportType, bool IsGet), (bool IsAllowed, string ErrorMessageForNotAllowed)>();
				AssertEquals(false, configForTest.SecurityCheckpoints.Any());

				var infoModel = new ReportItemInfoModel(model, configForTest, null, null);
				AssertEquals("Security checkpoint is allowed when not found", true, infoModel.SecurityCheckpoint.IsAllowed);
				AssertEquals("Security checkpoint has an empty ErrorMessageForNotAllowed when not found", string.Empty, infoModel.SecurityCheckpoint.ErrorMessageForNotAllowed);
			}
		}

		public void TestRelatedLatestEventsAndRelatedTipInfo()
		{
			var model = new ReportItemInfoModel(new ReportItemModel(CreditReportType.FailureRisk), null, null, null);
			AssertEquals(false, model.ToolTipIconVisible);
			AssertImageBitsEquals(Properties.Resources.WarningRed, model.ToolTipIcon);
			AssertEquals("#EE443A", model.ToolTipBackgroundColor);
			AssertEquals("There have been 0 events since the last report. Renew now for current information.", model.ToolTipCaption);
			model.RelatedLatestEvents = new List<CreditEvent>
			{
				new CreditEvent
				{
					EventDate = new DateTime(2023, 08, 03), Type = CreditEventType.FinancialChange
				}
			};

			AssertEquals(1, model.RelatedLatestEvents.Count());
			Assert(model.ToolTipIconVisible);
			AssertImageBitsEquals(Properties.Resources.InfoGrey, model.ToolTipIcon);
			AssertEquals("#71747C", model.ToolTipBackgroundColor);
			AssertEquals("There have been 1 events since the last report. Renew now for current information.", model.ToolTipCaption);
		}

		public void TestGetReportLineVisible()
		{
			var model = new ReportItemInfoModel(new ReportItemModel(CreditReportType.FailureRisk), null, null, null);
			Assert(model.GetReportLineVisible);
			model.GetReportLineVisible = false;
			AssertEquals(false, model.GetReportLineVisible);
		}

		public void TestCreditReportType()
		{
			var model = new ReportItemInfoModel(new ReportItemModel(CreditReportType.FailureRisk), null, null, null);
			AssertEquals(CreditReportType.FailureRisk, model.CreditReportType);
		}

		public void TestGetReportSuccessfully()
		{
			using (var zForm = new ZForm())
			using (var creditForTest = new SupportCreditCheckForTest())
			{
				var org = GetOrgHeader();
				creditForTest.BindingSource.DataSource = org;

				zForm.Controls.Add(creditForTest);

				var mainPageModel = new MainPageModel()
				{
					TopBannerModel = new TopBannerModel(CreditReportStatusType.UpToDate, 0, "WiseTech Global Limited")
				};

				mainPageModel.ReportsModel = new ReportsModel(new List<ReportItemModel>(), null, null, mainPageModel);
				mainPageModel.EventsBannerModel = new EventsBannerModel(creditForTest, new CreditCheckServiceForTest(), mainPageModel);

				var model = new ReportItemModel(CreditReportType.ComprehensiveReport);
				var reportViewModel = new ReportItemInfoModelForTest(model, creditForTest, null, mainPageModel);
				reportViewModel.IsConfirmedValue = true;
				var log = org.GetLogs().Find(u => u.SL_Reference.StartsWith(CreditReportHelper.BuyCreditReportReference)).FirstOrDefault();
				AssertNull(log);

				reportViewModel.GetReportForTest().Wait();
				log = org.GetLogs().Find(u => u.SL_Reference.StartsWith(CreditReportHelper.BuyCreditReportReference)).FirstOrDefault();
				AssertNotNull(log);
			}
		}

		public void TestGetReportUnsuccessfully()
		{
			using (var zForm = new ZForm())
			using (var creditForTest = new SupportCreditCheckForTest())
			{
				var org = GetOrgHeader();
				creditForTest.BindingSource.DataSource = org;

				zForm.Controls.Add(creditForTest);

				var mainPageModel = new MainPageModel()
				{
					TopBannerModel = new TopBannerModel(CreditReportStatusType.UpToDate, 0, "WiseTech Global Limited")
				};

				mainPageModel.ReportsModel = new ReportsModel(new List<ReportItemModel>(), null, null, mainPageModel);
				mainPageModel.EventsBannerModel = new EventsBannerModel(creditForTest, new CreditCheckServiceForTest(), mainPageModel);

				var model = new ReportItemModel(CreditReportType.ComprehensiveReport);
				var reportViewModel = new ReportItemInfoModelForTest(model, creditForTest, null, mainPageModel);
				reportViewModel.IsConfirmedValue = false;

				reportViewModel.GetReportForTest().Wait();
				var log = org.GetLogs().Find(u => u.SL_Reference.StartsWith(CreditReportHelper.BuyCreditReportReference)).FirstOrDefault();

				AssertEquals("Report exist", 0, org.DocManagerInfo().AllEDocs.Count);
				AssertNull(log);
			}
		}

		protected override void TearDown()
		{
			Thread.Sleep(100);

			TempDirectory.DeleteDirectory(Temp.TempPath);
			base.TearDown();
		}

		OrgHeader GetOrgHeader()
		{
			var orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_CustomsRegNo = "200";
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.Add(orgCusCode);

			Factory.Save();

			return org;
		}

		public class ReportItemInfoModelForTest : ReportItemInfoModel
		{
			public ReportItemInfoModelForTest(ReportItemModel model, CreditReportUserControl supportCreditCheck, ICreditCheckService creditCheckService, MainPageModel parent) : base(model, supportCreditCheck, creditCheckService, parent)
			{
			}

			public bool IsConfirmedValue { get; set; }

			public async Task GetReportForTest(IEnumerable<Identifier> identifiers = null)
			{
				await GetReport(identifiers);
			}

			#region Overrides

			protected override Task<LoadingWindowResult<CreditReportResponse>> GetLoadingResult(IEnumerable<Identifier> availableIdentifiers)
			{
				var creditReportResponse = new CreditReportResponse();

				creditReportResponse.ReportType = CreditReportType.ComprehensiveReport;
				creditReportResponse.Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes("dummy"));
				creditReportResponse.PayloadExtension = ".pdf";
				creditReportResponse.DunsRating = "10";
				creditReportResponse.ResultCode = ResultCode.Successful;
				creditReportResponse.FailureRiskScore = "";
				creditReportResponse.LatePaymentScore = "";
				creditReportResponse.ErrorInfo = null;
				creditReportResponse.ExtractedInfo = new CreditReportExtractedInfo();

				return Task.FromResult(new LoadingWindowResult<CreditReportResponse>() { Exception = null, Result = creditReportResponse });
			}

			protected override Task<bool> ShowTermsAndAgreement()
			{
				return Task.FromResult(true);
			}

			protected override bool IsConfirmed(ConfirmGetReportModel confirmModel)
			{
				return IsConfirmedValue;
			}

			#endregion
		}
	}
}
