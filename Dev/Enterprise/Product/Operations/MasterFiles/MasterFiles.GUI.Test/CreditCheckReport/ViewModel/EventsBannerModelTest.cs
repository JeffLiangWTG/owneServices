using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Moq;
using NUnit.Framework;
using WTG.CreditCheck;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class EventsBannerModelTest : TestCaseWithFactory
	{
		public void TestAlertsBannerViewModel()
		{
			using (var configForTest = new SupportCreditCheckForTest())
			{
				var model = new EventsBannerModel(configForTest, new CreditCheckServiceForTest(), null);
				model.SetEvents(new List<CreditEvent>()
				{
					new CreditEvent() { Type = CreditEventType.CollectionChange, EventDate = new DateTime(2020, 4, 1) }
				});

				CombineAssertions(() =>
				{
					AssertEquals(ResourceStringHelper.LatestCreditEvents, model.EventsBannerCaption);
					AssertEquals(ResourceStringHelper.LoadingEvents, model.LoadingEvents);
					AssertEquals(1, model.Events.Count);
				});
			}
		}

		public void TestSetErrorInfo()
		{
			using (var configForTest = new SupportCreditCheckForTest())
			{
				var model = new EventsBannerModel(configForTest, new CreditCheckServiceForTest(), null);
				AssertNotNull(model.ErrorMessage); //Default model has error message
				Assert(model.ErrorOccur);

				model.SetEvents(new List<CreditEvent>());
				AssertNull(model.ErrorMessage);
				Assert(!model.ErrorOccur);

				var changedTriggerTimes = 0;
				model.PropertyChanged += Model_PropertyChanged;
				model.ErrorMessage = "Dummy";
				CombineAssertions(() =>
				{
					AssertEquals("Dummy", model.ErrorMessage);
					Assert(model.ErrorOccur);
					AssertEquals(2, changedTriggerTimes);
				});

				model.ErrorMessage = " ";
				CombineAssertions(() =>
				{
					AssertEquals(" ", model.ErrorMessage);
					Assert(!model.ErrorOccur);
					AssertEquals(4, changedTriggerTimes);
				});

				model.ErrorMessage = string.Empty;
				AssertEquals(string.Empty, model.ErrorMessage);
				Assert(!model.ErrorOccur);

				void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
				{
					changedTriggerTimes++;
				}
			}
		}

		public void TestSetLoading()
		{
			using (var configForTest = new SupportCreditCheckForTest())
			{
				var model = new EventsBannerModel(configForTest, new CreditCheckServiceForTest(), null);

				AssertEquals(8, model.Events.Count); //Default model has dummy events
				AssertEquals(false, model.IsLoading);

				model.IsLoading = true;
				model.SetEvents(new List<CreditEvent>());
				AssertEquals(0, model.Events.Count);
				AssertEquals(null, model.ErrorMessage);

				model.ErrorMessage = "Unknown exception happened!";
				AssertEquals(false, model.IsLoading);
				AssertEquals("Unknown exception happened!", model.ErrorMessage);
			}
		}

		public void TestBackgroundCompanyLookupRequestIncludeLicenceCode()
		{
			using (var configForTest = new SupportCreditCheckForTest())
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "FullName";
				org.OH_RL_NKClosestPort = "AU";
				Factory.Save();

				configForTest.BindingSource.DataSource = org;
				var creditCheckServiceMock = new Mock<ICreditCheckService>();
				var licenceCode = string.Empty;
				creditCheckServiceMock.Setup(o => o.CompanyLookupAsync(It.IsAny<CompanyLookupRequest>(), It.IsAny<CancellationToken>()))
					.Callback<CompanyLookupRequest, CancellationToken>((o, c) => licenceCode = o.LicenseCode)
					.Returns(Task.FromResult(new CompanyLookupResponse()));
				var model = new EventsBannerModel(configForTest, creditCheckServiceMock.Object, null);
				model.TokenSource = new CancellationTokenSource();
				AsyncTaskSynchronizer.Run(model.BackgroundCompanyLookup);

				CombineAssertions(() =>
				{
					AssertNotNullOrEmpty(licenceCode);
					AssertEquals(GlbCompany.CurrentCompany.LicenceKeyIdentifier, licenceCode);
				});
			}
		}

		public void TestBackgroundCompanyLookupRequestIncludeStateCode()
		{
			using (var configForTest = new SupportCreditCheckForTest())
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "FullName";
				org.OH_RL_NKClosestPort = "AU";
				org.MainAddress.State = "AUCKLAND";
				org.MainAddress.StateCode = "AUK";
				Factory.Save();

				configForTest.BindingSource.DataSource = org;
				var mockCreditCheckService = new Mock<ICreditCheckService>();
				var state = string.Empty;
				mockCreditCheckService.Setup(service => service.CompanyLookupAsync(It.IsAny<CompanyLookupRequest>(), It.IsAny<CancellationToken>()))
					.Callback<CompanyLookupRequest, CancellationToken>((o, c) => state = o.State)
					.Returns(Task.FromResult(new CompanyLookupResponse()));
				var mainPageModel = new MainPageModel();
				var eventsBannerModel = new EventsBannerModel(configForTest, mockCreditCheckService.Object, mainPageModel);
				eventsBannerModel.TokenSource = new CancellationTokenSource();
				AsyncTaskSynchronizer.Run(eventsBannerModel.BackgroundCompanyLookup);

				mockCreditCheckService.Verify(service => service.CompanyLookupAsync(It.Is<CompanyLookupRequest>(request => request.State == "AUK"), It.IsAny<CancellationToken>()), Times.Once);
				CombineAssertions(() =>
				{
					AssertNotNullOrEmpty(state);
					AssertEquals("AUK", state);
				});
			}
		}

		public void TestLoadCreditEvents_ErrorHappened()
		{
			AsyncTaskSynchronizer.Run(AssertLoadCreditEvents_ErrorHappened);
		}

		async Task AssertLoadCreditEvents_ErrorHappened()
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

			using (var configForTest = new SupportCreditCheckForTest())
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			{
				var orgCusCode = Factory.New<OrgCusCode>();
				orgCusCode.OK_CustomsRegNo = "213";
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;

				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "Throw Exception";
				org.OH_RL_NKClosestPort = "AU";
				org.CustomsCodes.Add(orgCusCode);
				Factory.Save();

				configForTest.BindingSource.DataSource = org;
				configForTest.PurchasedReportsForTest = new[] { (CreditReportType.CommercialBureauEnquiry, DateTime.Now.AddDays(-30)) };

				var mainPageModel = new MainPageModel();
				var client = new CreditCheckServiceForTest();
				mainPageModel.TopBannerModel = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "WiseTech Global Limited");
				mainPageModel.ReportsModel = new ReportsModel(new List<ReportItemModel>(), configForTest, client, mainPageModel);
				var model = new EventsBannerModel(configForTest, client, mainPageModel);
				mainPageModel.EventsBannerModel = model;

				await model.LoadCreditEventsOrSilentCompanyLookup();
				AssertEquals(false, model.IsLoading);
				AssertEquals("Error occur when try to get retrospective monitor", model.ErrorMessage);
			}
		}

		public void TestLoadCreditEvents_HasEvents()
		{
			AsyncTaskSynchronizer.Run(AssertLoadCreditEvents_HasEvents);
		}

		async Task AssertLoadCreditEvents_HasEvents()
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

			using (var configForTest = new SupportCreditCheckForTest())
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			{
				var orgCusCode = Factory.New<OrgCusCode>();
				orgCusCode.OK_CustomsRegNo = "213";
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;

				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "I have events";
				org.OH_RL_NKClosestPort = "AU";
				org.CustomsCodes.Add(orgCusCode);
				Factory.Save();

				configForTest.BindingSource.DataSource = org;

				var mainPageModel = new MainPageModel();
				var client = new CreditCheckServiceForTest();
				mainPageModel.TopBannerModel = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "WiseTech Global Limited");
				mainPageModel.ReportsModel = new ReportsModel(new List<ReportItemModel>(), configForTest, client, mainPageModel);
				var model = new EventsBannerModel(configForTest, client, mainPageModel);
				mainPageModel.EventsBannerModel = model;

				await model.LoadCreditEventsOrSilentCompanyLookup();
				CombineAssertions(() =>
				{
					AssertEquals(false, model.IsLoading);
					AssertEquals(null, model.ErrorMessage);
					AssertEquals(2, model.Events.Count);
				});
			}
		}

		public void TestLoadCreditEvents_HasNoEvents()
		{
			AsyncTaskSynchronizer.Run(AssertLoadCreditEvents_HasNoEvents);
		}

		async Task AssertLoadCreditEvents_HasNoEvents()
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

			using (var configForTest = new SupportCreditCheckForTest())
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			{
				var orgCusCode = Factory.New<OrgCusCode>();
				orgCusCode.OK_CustomsRegNo = "213";
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;

				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_RL_NKClosestPort = "AU";
				org.CustomsCodes.Add(orgCusCode);
				Factory.Save();

				configForTest.BindingSource.DataSource = org;
				configForTest.PurchasedReportsForTest = new[] { (CreditReportType.CommercialBureauEnquiry, DateTime.Now.AddDays(-30)) };

				var mainPageModel = new MainPageModel();
				mainPageModel.TopBannerModel = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "WiseTech Global Limited");
				mainPageModel.ReportsModel = new ReportsModel(new List<ReportItemModel>(), configForTest, new CreditCheckServiceForTest(), mainPageModel);
				var model = new EventsBannerModel(configForTest, new CreditCheckServiceForTest(), mainPageModel);
				mainPageModel.EventsBannerModel = model;

				await model.LoadCreditEventsOrSilentCompanyLookup();
				CombineAssertions(() =>
				{
					AssertEquals(false, model.IsLoading);
					AssertEquals(ResourceStringHelper.EventsUnavailable, model.ErrorMessage);
					AssertEquals(8, model.Events.Count); // Dummy Events for show error message
				});
			}
		}

		[TestDate(2023, 1, 1, 10, 0, 0)]
		public void TestLoadCreditEvents_CreditEventQueryInfo()
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

			using (var configForTest = new SupportCreditCheckForTest())
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_RL_NKClosestPort = "AU";
				Factory.Save();

				configForTest.BindingSource.DataSource = org;
				configForTest.PurchasedReportsForTest = new[] { (CreditReportType.CommercialBureauEnquiry, DateTime.Today.AddDays(-30)) };

				var model = new EventsBannerModel(configForTest, new CreditCheckServiceForTest(), null);

				var creditEventQueryInfo = model.CreditEventQueryInfo;
				var expectedFromDate = ZDateTime.Today.AddMonths(-12).ToDateTime();
				var expectedToDate = ZDateTime.Today.ToDateTime();
				AssertContainsExactElementsInAnyOrder(
					new List<(CreditEventType EventType, DateTime FromDate, DateTime ToDate)>()
					{
						(CreditEventType.ScoreChange, expectedFromDate, expectedToDate),
						(CreditEventType.CollectionChange, expectedFromDate, expectedToDate),
						(CreditEventType.CourtChange, expectedFromDate, expectedToDate),
						(CreditEventType.PublicFilingChange, expectedFromDate, expectedToDate),
						(CreditEventType.DirectorChange, expectedFromDate, expectedToDate),
						(CreditEventType.ShareholderChange, expectedFromDate, expectedToDate),
						(CreditEventType.FinancialChange, expectedFromDate, expectedToDate),
						(CreditEventType.StatusChange, expectedFromDate, expectedToDate),
					}, creditEventQueryInfo.Select(u => (u.CreditEventType, u.FromDate, u.ToDate)));
			}
		}

		public void TestFilterAndRestoreEvents()
		{
			AsyncTaskSynchronizer.Run(AssertFilterAndRestoreEvents);
		}

		async Task AssertFilterAndRestoreEvents()
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

			using (var configForTest = new SupportCreditCheckForTest())
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			{
				var orgCusCode = Factory.New<OrgCusCode>();
				orgCusCode.OK_CustomsRegNo = "213";
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;

				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "I have events";
				org.OH_RL_NKClosestPort = "AU";
				org.CustomsCodes.Add(orgCusCode);
				Factory.Save();

				configForTest.BindingSource.DataSource = org;
				configForTest.PurchasedReportsForTest = new[] { (CreditReportType.CommercialBureauEnquiry, DateTime.Now.AddDays(-30)) };

				var mainPageModel = new MainPageModel();
				var client = new CreditCheckServiceForTest();
				mainPageModel.TopBannerModel = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "WiseTech Global Limited");
				mainPageModel.ReportsModel = new ReportsModel(new List<ReportItemModel>(), configForTest, client, mainPageModel);
				var model = new EventsBannerModel(configForTest, client, mainPageModel);
				mainPageModel.EventsBannerModel = model;

				await model.LoadCreditEventsOrSilentCompanyLookup();
				AssertEquals(2, model.Events.Count);

				model.FilterEvents(new List<CreditEvent> { new CreditEvent() { EventDate = DateTime.Today, Type = CreditEventType.CollectionChange } });
				AssertEquals(1, model.Events.Count);

				model.RestoreEvents();
				AssertEquals(2, model.Events.Count);
			}
		}

		public void TestLoadCreditEvents_Silent_NoDuns_NoEvents()
		{
			AsyncTaskSynchronizer.Run(AssertLoadCreditEvents_Silent_NoDuns_NoEvents);
		}

		async Task AssertLoadCreditEvents_Silent_NoDuns_NoEvents()
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

			using (var configForTest = new SupportCreditCheckForTest())
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			{
				var orgCusCode = Factory.New<OrgCusCode>();
				orgCusCode.OK_CustomsRegNo = "456";
				orgCusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;

				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "I have events";
				org.OH_RL_NKClosestPort = "AU";
				org.CustomsCodes.Add(orgCusCode);
				Factory.Save();

				configForTest.BindingSource.DataSource = org;

				var mainPageModel = new MainPageModel();
				mainPageModel.TopBannerModel = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "WiseTech Global Limited");
				mainPageModel.ReportsModel = new ReportsModel(new List<ReportItemModel>(), configForTest, new CreditCheckServiceForTest(), mainPageModel);
				var model = new EventsBannerModel(configForTest, new CreditCheckServiceForTest(), mainPageModel);
				AssertEquals(8, model.Events.Count); //Default model has dummy events
				await model.LoadCreditEventsOrSilentCompanyLookup();

				AssertNull(configForTest.Identifiers.FirstOrDefault(x => x.Type == IdentifierType.DUNS));
				model.SetEvents(new List<CreditEvent>());
				AssertEquals(0, model.Events.Count);
			}
		}

		public void TestLoadCreditEvents_Silent_Successful()
		{
			AsyncTaskSynchronizer.Run(AssertLoadCreditEvents_Silent_Successful);
		}

		async Task AssertLoadCreditEvents_Silent_Successful()
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

			using (var configForTest = new SupportCreditCheckForTest())
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			{
				var orgCusCode = Factory.New<OrgCusCode>();
				orgCusCode.OK_CustomsRegNo = "123";
				orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
				orgCusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;

				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "I have events";
				org.OH_RL_NKClosestPort = "AU";
				org.CustomsCodes.Add(orgCusCode);
				Factory.Save();

				configForTest.BindingSource.DataSource = org;

				var mainPageModel = new MainPageModel();
				mainPageModel.TopBannerModel = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "WiseTech Global Limited");
				mainPageModel.ReportsModel = new ReportsModel(new List<ReportItemModel>(), configForTest, new CreditCheckServiceForTest(), mainPageModel);
				var model = new EventsBannerModel(configForTest, new CreditCheckServiceForTest(), mainPageModel);

				await model.LoadCreditEventsOrSilentCompanyLookup();

				AssertNotNull(configForTest.Identifiers.FirstOrDefault(x => x.Type == IdentifierType.DUNS));
				AssertEquals(2, model.Events.Count);
			}
		}
	}
}
