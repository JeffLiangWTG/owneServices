using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.DataTransfer;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.Rating.Business.RatingConstants;

namespace Enterprise.Rating.GUI.Testing
{
	public class RatingFormTest : RatingTestCase
	{
		[TestDate(2017, 01, 01)]
		[CargoWise.Data.Testing.UseSnapshotProtection]
		public void TestSaveForm_ValidationMissedOverlappedRates_DontReportIssueIfEntryWasChangedInDB()
		{
			using (RunNonTransactioned())
			{
				var costing = Factory.NewWithValidTestData<Costing>();
				var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

				var entry1 = costing.AddRateEntry(RateCategory.AIR, Core.Constants.RateMode.LSE, "UA", "AU");
				entry1.TI_RateStartDate = new ZDate(2017, 1, 1);
				entry1.TI_RateEndDate = new ZDate(2017, 1, 5);

				try
				{
					costing.EntryCollectionValidator.Validate();
					AssertNoRowErrors(entry1);
					Factory.Save();
					Factory.SuspendValidation();

					var rateEntry1FromSecondFactory = factory2.Load<RateEntry>(entry1.PK);
					rateEntry1FromSecondFactory.TI_RateEndDate = new ZDate(2017, 1, 7);     //Now it overlaps in DB
					factory2.Save();

					var entry2 = costing.AddRateEntry(RateCategory.AIR, Core.Constants.RateMode.LSE, "UA", "AU");
					entry2.TI_RateStartDate = new ZDate(2017, 1, 6);
					entry2.TI_RateEndDate = new ZDate(2017, 1, 15);

					using (var form = new RatingFormForTest(costing))
					{
						form.DisplayMode = ODisplayMode.Edit;
						costing.AllEntries.ToArray(); //reload rate entries

						costing.EntryCollectionValidator.Validate();
						AssertNoRowErrors(entry1);
						AssertNoRowErrors(entry2);

						form.FireSaveButton();
					}

					Assert("No exception should be reported", ErrorReporter.LastKeyReported.IsNullOrEmpty());
					AssertEquals(ErrorMessages.OverlappingRatesCreatedByConcurrency, UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					var costingFromFactory2 = factory2.Load<Costing>(costing.PK);
					costingFromFactory2.Delete();
					factory2.Save();
				}
			}
		}

		[TestDate(2017, 01, 01)]
		public void TestSaveForm_ValidationStopsSavingOverlappedRates()
		{
			var costing = Factory.NewWithValidTestData<Costing>();
			var entry1 = costing.AddRateEntry(RateCategory.AIR, Core.Constants.RateMode.LSE, "UA", "AU");
			entry1.TI_RateStartDate = new ZDate(2017, 1, 1);
			entry1.TI_RateEndDate = new ZDate(2017, 1, 5);
			Factory.Save();

			var entry2 = costing.AddRateEntry(RateCategory.AIR, Core.Constants.RateMode.LSE, "UA", "AU");
			entry2.TI_RateStartDate = new ZDate(2017, 1, 2);
			entry2.TI_RateEndDate = new ZDate(2017, 1, 7);

			using (var form = new RatingForm(costing))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.FireSaveButton();
			}

			AssertHasRowError(entry1, ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(entry2, ErrorMessages.OverlappingDatesOnRateEntry);
			Assert("Should not have saved rate entry with error", !entry2.IsInDatabase);

			AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text.Trim());
			Assert("There should be no exception as validation stopped the invalid rate from saving", ErrorReporter.LastKeyReported.IsNullOrEmpty());
		}

		[TestDate(2014, 01, 01)]
		public void TestEntriesReloadedWithNoFilterSoThatExpiredEntryCouldBeSelected()
		{
			var rate = Factory.NewWithValidTestData<ClientRate>();
			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUSYD", "USLAX");
			entry1.TI_RateStartDate = new ZDate(2014, 01, 01);
			entry1.TI_RateEndDate = new ZDate(2014, 03, 01);
			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUBNE", "USLAX");
			entry2.TI_RateStartDate = new ZDate(2014, 01, 01);
			var entry3 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUMEL", "USLAX");
			entry3.TI_RateStartDate = new ZDate(2014, 01, 01);

			Factory.Save();

			using (var form = new ActiveRatesForm(rate))
			{
				form.Show();
				var elements = form.SelectSingleEntry(entry3);
				AssertContainsExactElementsInAnyOrder(new[] { entry3 }, elements);
				elements = form.SelectSingleEntry(entry2);
				AssertContainsExactElementsInAnyOrder(new[] { entry2 }, elements);
				elements = form.SelectSingleEntry(entry1);
				AssertContainsExactElementsInAnyOrder(new[] { entry1 }, elements);
			}
		}

		public void TestImportRates()
		{
			var quote = Factory.NewWithValidTestData<Quote>();
			using (var form = new RatingFormForTest(quote))
			{
				Assert(!form.FindActionMenuItem(RatingFormActionsMenu.ImportIATATACT).Visible);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var costing = Factory.NewWithValidTestData<Costing>();
			costing.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			using (var form = new RatingFormForTest(quote))
			{
				Assert(!form.FindActionMenuItem(RatingFormActionsMenu.ImportIATATACT).Visible);
			}

			try
			{
				RateDataImporter.RegisterType(typeof(RateDataImporterTestClass));
				using (var form = new RatingFormForTest(costing))
				{
					try
					{
						var actionMenuItem = form.FindActionMenuItem(RatingFormActionsMenu.ImportIATATACT);
						AssertNotNull(actionMenuItem);

						UnitTestUserNotification.Instance.AddOKAnswer();
						actionMenuItem.PerformClick();
						AssertEquals("Please save changes before importing.", UnitTestUserNotification.Instance.LastMessage.Text);

						Factory.Save();
						actionMenuItem.PerformClick();
						AssertEquals(typeof(DataImportForm), form.LastImportFormShown.GetType());
						AssertEquals(form, form.LastImportFormShown.ParentRatingFormToClose);
						AssertEquals(true, form.RateDataImporter.ShowClearRatesOptions);
					}
					finally
					{
						if (form.LastImportFormShown != null)
						{
							form.LastImportFormShown.Close();
						}
					}
				}
			}
			finally
			{
				RateDataImporter.UnRegisterType();
			}

			quote = Factory.New<Quote>();
			costing = Factory.New<Costing>();
			using (var form = new RatingFormForTest(costing))
			{
				AssertNull(form.FindActionMenuItem("Import Forward Air Costs"));
			}

			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				using (var form = new RatingFormForTest(costing))
				{
					var actionMenuItem = form.FindActionMenuItem("Import Forward Air Costs");
					AssertNotNull(actionMenuItem);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddOKAnswer();
					actionMenuItem.PerformClick();
					AssertEquals("Please save changes before importing.", UnitTestUserNotification.Instance.LastMessage.Text);

					Factory.Save();

					actionMenuItem.PerformClick();
					AssertEquals(typeof(DataImportForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
					AssertEquals(false, form.RateDataImporter.ShowClearRatesOptions);
					ZFormModaliser.LastFormShownDialogForTest = null;
					ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
				}

				using (var form = new RatingFormForTest(quote))
				{
					AssertNull(form.FindActionMenuItem("Import Forward Air Costs"));
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = currentCountry;
			}
		}

		public void TestImportIATAMenuItemVisibleWithTH_OHChanged()
		{
			var costing = Factory.NewWithValidTestData<Costing>();
			costing.TH_OH = ZGuid.Empty;
			using (var form = new RatingFormForTest(costing))
			{
				form.DisplayMode = ODisplayMode.New;
				var actionMenuItem = form.FindActionMenuItem("Import IATA Tact Rate File");
				AssertEquals(actionMenuItem.Visible, true);

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

				var orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
				orgCompanyData.OB_IsCreditor = true;
				orgCompanyData.OB_OH = orgHeader.PK;

				costing.TH_OH = orgHeader.PK;
				Factory.Save();
				form.FireSaveButton();

				AssertEquals(actionMenuItem.Visible, false);

				costing.TH_OH = ZGuid.Empty;
				Factory.Save();
				form.FireSaveButton();

				AssertEquals(actionMenuItem.Visible, true);
			}
		}

		[ExpectNoExceptions]
		public void TestFormCaptionDoesNotThrowShouldNotBeAccessingPropertyOnDeletedBizO()
		{
			var org = Factory.New<OrgHeader>();
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = org.PK;

			using (RatingFormForTest form = new RatingFormForTest(clientRate))
			{
				clientRate.Delete();
				string caption = form.FormCaption;
			}
		}

		public void TestFormCaptionForGlobalCosting()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var globalCosting = Factory.New<Costing>();
			globalCosting.TH_OH = org.PK;
			globalCosting.TH_GC = ZGuid.Empty;

			var localCosting = Factory.New<Costing>();
			localCosting.TH_OH = org.PK;
			localCosting.TH_GC = GlbCompany.CurrentCompany.PK;

			using (RatingFormForTest form = new RatingFormForTest(globalCosting))
			{
				AssertEquals("Should display Global Costing", "Global Costing XVBQP68SIYXQ", form.FormCaption);
			}

			using (RatingFormForTest form = new RatingFormForTest(localCosting))
			{
				AssertEquals("Should display Costing", "Costing XVBQP68SIYXQ", form.FormCaption);
			}
		}

		#region Publish as Global Rates

		public void TestPublishAsGlobalRates_CreatesNewGlobalCosting()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Costing);

			var helper = new TestHelper(controller.Factory);
			var serviceProvider = helper.CreateCreditor();
			var localCosting = helper.NewCosting(serviceProvider);
			var rateEntry1 = localCosting.AddRateEntry(RateCategory.AIR, Core.Constants.RateMode.LSE, "", "AU");
			var rateEntry2 = localCosting.AddRateEntry(RateCategory.LCL, Core.Constants.RateMode.LCL, "GB", "AU");
			var rateEntry3 = localCosting.AddRateEntry(RateCategory.LCL, Core.Constants.RateMode.LCL, "CN", "AU");

			controller.Factory.Save();

			AssertNull("Pre-condition", localCosting.GlobalRatingHeader);

			using (var form = new RatingFormForTest(localCosting))
			{
				form.Show();
				AssertNull(form.FindActionMenuItem(RatingFormActionsMenu.PublishAllRatesAsGlobalRates));
				AssertNotNull(form.FindActionMenuItem(RatingFormActionsMenu.PublishAllCostsAsGlobalCosts));

				var menuItem = form.FindActionMenuItem(RatingFormActionsMenu.PublishAllCostsAsGlobalCosts);
				menuItem.PerformClick();

				AssertNotNull("Global Rating Header should be created", localCosting.GlobalRatingHeader);
				Assert("Global Client Rate should be created", localCosting.GlobalRatingHeader.IsGlobal());
				Assert("Global Client Rate should be created", localCosting.GlobalRatingHeader.IsCosting());
				AssertEquals(rateEntry1.TI_TH, localCosting.GlobalRatingHeader.PK);
				AssertEquals(rateEntry2.TI_TH, localCosting.GlobalRatingHeader.PK);
				AssertEquals(rateEntry3.TI_TH, localCosting.GlobalRatingHeader.PK);
			}
		}

		public void TestPublishAsGlobalRates_GlobalCostingIsAlreadyOpen()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Costing);

			var helper = new TestHelper(controller.Factory);
			var globalCosting = helper.NewGlobalCosting(null);
			var localCosting = helper.NewCosting(null);
			var rateEntry1 = localCosting.AddRateEntry(RateCategory.AIR, Core.Constants.RateMode.LSE, "", "AU");
			var rateEntry2 = localCosting.AddRateEntry(RateCategory.PAC, Core.Constants.RateMode.LCL, "AU", "");
			var rateEntry3 = localCosting.AddRateEntry(RateCategory.LCL, Core.Constants.RateMode.LCL, "CN", "AU");

			controller.Factory.Save();

			AssertEquals("Pre-condition", globalCosting.PK, localCosting.GlobalRatingHeader.PK);

			using (var globalCostingForm = controller.ShowEditForm(globalCosting))
			using (var localCostingForm = new RatingFormForTest(localCosting))
			{
				localCostingForm.Show();
				globalCostingForm.Show();

				Assert("Pre-condition", controller.IsFormShownFor(globalCosting));

				var menuItem = localCostingForm.FindActionMenuItem(RatingFormActionsMenu.PublishAllCostsAsGlobalCosts);
				menuItem.PerformClick();

				AssertEquals("Rates should be moved from local Costing to Global Costing", rateEntry1.TI_TH, localCosting.GlobalRatingHeader.PK);
				AssertEquals(rateEntry2.TI_TH, localCosting.GlobalRatingHeader.PK);
				AssertEquals(rateEntry3.TI_TH, localCosting.GlobalRatingHeader.PK);
			}
		}

		public void TestPublishAsGlobalRates_GlobalCostingSecurity()
		{
			var isAllowed = Env.Security.GlobalCostingRatesPublishAndUnpublish.IsAllowed;
			try
			{
				Env.Security.GlobalCostingRatesPublishAndUnpublish.IsAllowed = false;
				var controller = ZControllerFactory.Create(ControllerIDs.Costing);

				var helper = new TestHelper(controller.Factory);
				var localCosting = helper.NewCosting(helper.CreateCreditor());
				localCosting.AddRateEntry(RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "");

				controller.Factory.Save();

				using (var form = new RatingFormForTest(localCosting))
				{
					form.Show();

					var menuItem = form.FindActionMenuItem(RatingFormActionsMenu.PublishAllCostsAsGlobalCosts);
					menuItem.PerformClick();

					AssertEquals(Env.Security.GlobalCostingRatesPublishAndUnpublish.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				Env.Security.GlobalCostingRatesPublishAndUnpublish.IsAllowed = isAllowed;
			}
		}

		public void TestPublishAsGlobalRates_NotAvailableForGlobalCosting()
		{
			var serviceProvider = Helper.CreateCreditor();
			var globalCosting = Helper.NewGlobalCosting(serviceProvider);
			globalCosting.AddRateEntry(RateCategory.LCL, Core.Constants.RateMode.LCL, "GB", "AU");

			using (var form = new RatingFormForTest(globalCosting))
			{
				form.Show();
				AssertNull(form.FindActionMenuItem(RatingFormActionsMenu.PublishAllCostsAsGlobalCosts));
			}
		}

		public void TestPublishAsGlobalRates_CreatesNewGlobalClientRate()
		{
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RateCategory.LCL, Core.Constants.RateMode.LCL, "CN", "");
			Factory.Save();

			AssertNull("Pre-condition", clientRate.GlobalRatingHeader);

			using (var form = new RatingFormForTest(clientRate))
			{
				form.Show();
				AssertNotNull(form.FindActionMenuItem(RatingFormActionsMenu.PublishAllRatesAsGlobalRates));
				AssertNull(form.FindActionMenuItem(RatingFormActionsMenu.PublishAllCostsAsGlobalCosts));

				var menuItem = form.FindActionMenuItem(RatingFormActionsMenu.PublishAllRatesAsGlobalRates);
				menuItem.PerformClick();

				AssertNotNull("Global Client Rate should be created", clientRate.GlobalRatingHeader);
				Assert("Global Client Rate should be created", clientRate.GlobalRatingHeader.IsGlobalClientRate());
				AssertEquals("Should have moved the rate entry from the local to global client rate", rateEntry.TI_TH, clientRate.GlobalRatingHeader.PK);
			}
		}

		public void TestPublishAsGlobalRates_GlobalClientRateSecurity()
		{
			var isAllowed = Env.Security.GlobalClientRatesPublishAndUnpublish.IsAllowed;
			try
			{
				Env.Security.GlobalClientRatesPublishAndUnpublish.IsAllowed = false;

				var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
				clientRate.AddRateEntry(RateCategory.LCL, Core.Constants.RateMode.LCL, "GB", "");
				Factory.Save();

				using (var form = new RatingFormForTest(clientRate))
				{
					form.Show();
					var menuItem = form.FindActionMenuItem(RatingFormActionsMenu.PublishAllRatesAsGlobalRates);
					menuItem.PerformClick();

					AssertEquals(Env.Security.GlobalClientRatesPublishAndUnpublish.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				Env.Security.GlobalClientRatesPublishAndUnpublish.IsAllowed = isAllowed;
			}
		}

		public void TestPublishAsGlobalRates_LoadsExistingGlobalClientRate()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.ClientRates);

			var helper = new TestHelper(controller.Factory);
			var client = helper.NewOrgHeader();
			var globalClientRate = helper.NewGlobalClientRate(client);
			var localClientRate = helper.NewClientRate(client);
			var rateEntry = localClientRate.AddRateEntry(RateCategory.DST, Core.Constants.RateMode.LSE, "", "AU");

			controller.Factory.Save();

			AssertEquals("Pre-condition", globalClientRate.PK, localClientRate.GlobalRatingHeader.PK);

			using (var globalClientRateForm = controller.ShowEditForm(globalClientRate))
			using (var localClientRateForm = new RatingFormForTest(localClientRate))
			{
				localClientRateForm.Show();
				globalClientRateForm.Show();

				Assert("Pre-condition", controller.IsFormShownFor(globalClientRate));

				var menuItem = localClientRateForm.FindActionMenuItem(RatingFormActionsMenu.PublishAllRatesAsGlobalRates);
				menuItem.PerformClick();

				AssertEquals("Should have moved the rate entry from the local to global client rate", rateEntry.TI_TH, globalClientRate.PK);
			}
		}

		public void TestPublishAsGlobalRates_NotAvailableForGlobalClientRate()
		{
			var globalClientRate = Helper.NewGlobalClientRate(Helper.NewOrgHeader());

			using (var form = new RatingFormForTest(globalClientRate))
			{
				form.Show();
				AssertNull(form.FindActionMenuItem(RatingFormActionsMenu.PublishAllRatesAsGlobalRates));
			}
		}

		public void TestPublishAsGlobalRates_CreatesNewGlobalTariff()
		{
			var companyTariff = Helper.NewCompanyTariff();
			var rateEntry1 = companyTariff.AddRateEntry(RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "");
			var rateEntry2 = companyTariff.AddRateEntry(RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "");
			companyTariff.Factory.Save();

			using (var form = new RatingFormForTest(companyTariff))
			{
				form.Show();
				AssertNotNull(form.FindActionMenuItem(RatingFormActionsMenu.PublishAllRatesAsGlobalRates));
				AssertNull(form.FindActionMenuItem(RatingFormActionsMenu.PublishAllCostsAsGlobalCosts));

				var menuItem = form.FindActionMenuItem(RatingFormActionsMenu.PublishAllRatesAsGlobalRates);
				AssertNotNull(menuItem);
				menuItem.PerformClick();

				AssertNotNull("Global Rating Header should be created", companyTariff.GlobalRatingHeader);
				Assert(companyTariff.GlobalRatingHeader.IsGlobalTariff());
				Assert(companyTariff.GlobalRatingHeader.IsLevelOneTariff());
				AssertEquals("Rates should be moved from Company Tariff to Global Tariff", rateEntry1.TI_TH, companyTariff.GlobalRatingHeader.PK);
				AssertEquals(rateEntry2.TI_TH, companyTariff.GlobalRatingHeader.PK);
			}
		}

		public void TestPublishAsGlobalRates_GlobalTariffSecurity()
		{
			var isAllowed = Env.Security.GlobalTariffRatesPublishAndUnpublish.IsAllowed;
			try
			{
				Env.Security.GlobalTariffRatesPublishAndUnpublish.IsAllowed = false;

				var companyTariff = Helper.NewCompanyTariff();
				companyTariff.AddRateEntry(RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "");
				companyTariff.Factory.Save();

				using (var form = new RatingFormForTest(companyTariff))
				{
					form.Show();

					var menuItem = form.FindActionMenuItem(RatingFormActionsMenu.PublishAllRatesAsGlobalRates);
					AssertNotNull(menuItem);
					menuItem.PerformClick();

					AssertEquals(Env.Security.GlobalTariffRatesPublishAndUnpublish.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				Env.Security.GlobalTariffRatesPublishAndUnpublish.IsAllowed = isAllowed;
			}
		}

		public void TestPublishAsGlobalRates_LoadsExistingGlobalTariff()
		{
			var globalTariff = Helper.NewGlobalTariff();
			globalTariff.Factory.Save();

			var companyTariff = Helper.NewCompanyTariff();
			var rateEntry1 = companyTariff.AddRateEntry(RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "");
			var rateEntry2 = companyTariff.AddRateEntry(RateCategory.TBC, Core.Constants.RateMode.FTL, "AU", "");
			companyTariff.Factory.Save();

			AssertEquals("Pre-condition", globalTariff.PK, companyTariff.GlobalRatingHeader.PK);

			using (var form = new RatingFormForTest(companyTariff))
			{
				form.Show();

				var menuItem = form.FindActionMenuItem(RatingFormActionsMenu.PublishAllRatesAsGlobalRates);
				menuItem.PerformClick();

				AssertEquals("Rates should be moved from Company Tariff to Global Tariff", rateEntry1.TI_TH, globalTariff.PK);
				AssertEquals(rateEntry2.TI_TH, globalTariff.PK);
			}
		}

		public void TestPublishAsGlobalRates_NotAvailableForAdditionalTariff()
		{
			var companyTariff = Helper.NewCompanyTariff();
			companyTariff.Factory.Save();

			var additionalCompanyTariff = Helper.NewCompanyTariff();
			Assert("Pre-condition", additionalCompanyTariff.IsAdditionalTariff());

			using (var form = new RatingFormForTest(additionalCompanyTariff))
			{
				form.Show();
				AssertNull(form.FindActionMenuItem(RatingFormActionsMenu.PublishAllRatesAsGlobalRates));
			}
		}

		public void TestPublishAsGlobalRates_NotAvailableForGlobalTariff()
		{
			var globalTariff = Helper.NewGlobalTariff();

			using (var form = new RatingFormForTest(globalTariff))
			{
				form.Show();
				AssertNull(form.FindActionMenuItem(RatingFormActionsMenu.PublishAllRatesAsGlobalRates));
			}
		}

		public void TestPublishAsGlobalRates_NotAvailableQuote()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			using (var form = new RatingFormForTest(quote))
			{
				form.Show();
				AssertNull(form.FindActionMenuItem(RatingFormActionsMenu.PublishAllRatesAsGlobalRates));
			}
		}

		#endregion

		#region RateDataImporterTestClass

		public class RateDataImporterTestClass : RateDataImporter
		{
			protected RateDataImporterTestClass(RatingHeader ratingHeader)
				: base(ratingHeader)
			{
			}

			protected override bool IsActiveCore
			{
				get { return true; }
			}
		}

		#endregion

		#region RatingFormForTest

		public class RatingFormForTest : RatingForm
		{
			public RatingFormForTest(RatingHeader header)
				: base(header)
			{
			}

			public MenuItem FindActionMenuItem(string text)
			{
				return ActionsMenuItem.MenuItems.FindByText(text);
			}
		}

		#endregion
	}
}
