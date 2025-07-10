using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.QuotedBookings.GUI.Test.QuotedBookingFormTest;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class QuotedBookingIntegrationBusinessTest : BaseRatingIntegrationTest
	{
		public void TestRateSelector_OneOffQuote()
		{
			var oneOffQuote = CreateQuotedBooking(TransportModes.Sea, RateMode.SEA, ZString.Empty, NewClient, NewClient, NewClient2, null, "AUSYD", "USLAX", 10m, 20m, QuotedBookingState.QuoteOnly);

			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var timesRateSelectorFormIsShown = 0;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is RateChooserForm rateChooser)
					{
						timesRateSelectorFormIsShown++;
						rateChooser.SkipRateSelectionButton_Click(null, EventArgs.Empty);
					}
				});

				AssertNoExceptionThrown("GIVEN OneOffQuote with SEA mode and no container THEN should not show RateSelector", () =>
				{
					AutorateWithManualSelectAndAssert("Autorate", Array.Empty<AssertionCharge>(), oneOffQuote, Consignee, autorateRevenue: false);
					AssertEquals("RateSelector", 0, timesRateSelectorFormIsShown);
				});

				var container = oneOffQuote.Quote.CurrentOneOffQuote.Containers.AddNew();
				container.TC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
				container.TC_ContainerCount = 1;
				AssertNoExceptionThrown("GIVEN OneOffQuote with SEA mode and container THEN should show RateSelector", () =>
				{
					AutorateWithManualSelectAndAssert("Autorate", Array.Empty<AssertionCharge>(), oneOffQuote, Consignee, autorateRevenue: false);
					AssertEquals("RateSelector", 1, timesRateSelectorFormIsShown);
				});
			}
		}

		#region JobChargeDescription

		#region One Off Quote (OOQ)

		public void TestAutoRating_JobChargeDescription_OneOffQuote_NonReceivableClient_RegistryDisable_NonLocalClient()
			=> AssertAutoRating_JobChargeDescription_NonReceivableClient(quotedBookingState: QuotedBookingState.QuoteOnly, enableLocalChargeCodeDescriptionDefault: false, isLocalClient: false, expectedLocalDescription: false);

		public void TestAutoRating_JobChargeDescription_OneOffQuote_NonReceivableClient_RegistryDisable_LocalClient()
			=> AssertAutoRating_JobChargeDescription_NonReceivableClient(quotedBookingState: QuotedBookingState.QuoteOnly, enableLocalChargeCodeDescriptionDefault: false, isLocalClient: true, expectedLocalDescription: false);

		public void TestAutoRating_JobChargeDescription_OneOffQuote_NonReceivableClient_RegistryEnable_NonLocalClient()
			=> AssertAutoRating_JobChargeDescription_NonReceivableClient(quotedBookingState: QuotedBookingState.QuoteOnly, enableLocalChargeCodeDescriptionDefault: true, isLocalClient: false, expectedLocalDescription: false);

		public void TestAutoRating_JobChargeDescription_OneOffQuote_NonReceivableClient_RegistryEnable_LocalClient()
			=> AssertAutoRating_JobChargeDescription_NonReceivableClient(quotedBookingState: QuotedBookingState.QuoteOnly, enableLocalChargeCodeDescriptionDefault: true, isLocalClient: true, expectedLocalDescription: true);

		#endregion

		#region Booking With Quote (BWQ)

		public void TestAutoRating_JobChargeDescription_BookingWithQuote_NonReceivableClient_RegistryDisable_NonLocalClient()
			=> AssertAutoRating_JobChargeDescription_NonReceivableClient(quotedBookingState: QuotedBookingState.AcceptedBookingWithQuote, enableLocalChargeCodeDescriptionDefault: false, isLocalClient: false, expectedLocalDescription: false);

		public void TestAutoRating_JobChargeDescription_BookingWithQuote_NonReceivableClient_RegistryDisable_LocalClient()
			=> AssertAutoRating_JobChargeDescription_NonReceivableClient(quotedBookingState: QuotedBookingState.AcceptedBookingWithQuote, enableLocalChargeCodeDescriptionDefault: false, isLocalClient: true, expectedLocalDescription: false);

		public void TestAutoRating_JobChargeDescription_BookingWithQuote_NonReceivableClient_RegistryEnable_NonLocalClient()
			=> AssertAutoRating_JobChargeDescription_NonReceivableClient(quotedBookingState: QuotedBookingState.AcceptedBookingWithQuote, enableLocalChargeCodeDescriptionDefault: true, isLocalClient: false, expectedLocalDescription: false);

		public void TestAutoRating_JobChargeDescription_BookingWithQuote_NonReceivableClient_RegistryEnable_LocalClient()
			=> AssertAutoRating_JobChargeDescription_NonReceivableClient(quotedBookingState: QuotedBookingState.AcceptedBookingWithQuote, enableLocalChargeCodeDescriptionDefault: true, isLocalClient: true, expectedLocalDescription: true);

		#endregion

		#region Quick Booking (QB)

		public void TestAutoRating_JobChargeDescription_QuickBooking_NonReceivableClient_RegistryDisable_NonLocalClient()
			=> AssertAutoRating_JobChargeDescription_NonReceivableClient(quotedBookingState: QuotedBookingState.BookingOnly, enableLocalChargeCodeDescriptionDefault: false, isLocalClient: false, expectedLocalDescription: false);

		public void TestAutoRating_JobChargeDescription_QuickBooking_NonReceivableClient_RegistryDisable_LocalClient()
			=> AssertAutoRating_JobChargeDescription_NonReceivableClient(quotedBookingState: QuotedBookingState.BookingOnly, enableLocalChargeCodeDescriptionDefault: false, isLocalClient: true, expectedLocalDescription: false);

		public void TestAutoRating_JobChargeDescription_QuickBooking_NonReceivableClient_RegistryEnable_NonLocalClient()
			=> AssertAutoRating_JobChargeDescription_NonReceivableClient(quotedBookingState: QuotedBookingState.BookingOnly, enableLocalChargeCodeDescriptionDefault: true, isLocalClient: false, expectedLocalDescription: false);

		public void TestAutoRating_JobChargeDescription_QuickBooking_NonReceivableClient_RegistryEnable_LocalClient()
			=> AssertAutoRating_JobChargeDescription_NonReceivableClient(quotedBookingState: QuotedBookingState.BookingOnly, enableLocalChargeCodeDescriptionDefault: true, isLocalClient: true, expectedLocalDescription: true);

		#endregion

		void AssertAutoRating_JobChargeDescription_NonReceivableClient(QuotedBookingState quotedBookingState, bool enableLocalChargeCodeDescriptionDefault, bool isLocalClient, bool expectedLocalDescription)
		{
			var chargeCode = Helper.ChargeCodes.New("CC1", "CC1 Description", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			chargeCode.AC_LocalLanguageDescription = "CC1 Local Description 地方";

			// this will make BaseCharge.SellAccount = null
			NewClient.OH_IsDebtor = false;

			Factory.Save();

			var companyTariff = Helper.NewLevel1CompanyTariffWithSingleRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "US", "CC1", 100m);
			var companyTariffRateEntry = companyTariff.AllEntries.Single();
			var companyTariffRateLine = (RateLine)companyTariffRateEntry.RateLines.Single();
			companyTariffRateLine.TL_RateDesc = "CC1 Updated Description";
			companyTariffRateLine.TL_RateDescLocal = "CC1 Updated Local Description 地方";
			companyTariff.Factory.Save();

			var oneOffQuote = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, NewClient, NewClient, NewClient2, null, "AUSYD", "USLAX", 10m, 1m, quotedBookingState);

			NewClient.CompanyData.RateTariffLevels.SetLevel("FRT", 1);

			if (isLocalClient)
			{
				NewClient.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				AssertEquals("Local Client", true, NewClient.IsLocalCountry);
			}
			else
			{
				NewClient.OH_RL_NKClosestPort = "USLAX";
				AssertEquals("NonLocal Client", false, NewClient.IsLocalCountry);
			}

			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableLocalChargeCodeDescriptionDefault))
			{
				AutorateAndAssert
				(
					expected: new[]
					{
						new AssertionCharge
						{
							ChargeCode = "CC1",
							JR_OSSellAmt = 100m,
							JR_Desc = expectedLocalDescription
								? "CC1 Updated Local Description 地方"
								: "CC1 Updated Description"
						},
					},
					oneOffQuote,
					NewClient,
					autorateCosts: false
				);
			}
		}

		public void TestAutoRatingOneOffQuote_JobChargeDescription_CompanyTariff_RegistryDisable_NonLocalClient()
			=> AssertAutoRatingOneOffQuote_JobChargeDescription_CompanyTariff(enableLocalChargeCodeDescriptionDefault: false, isLocalClient: false, expectedLocalDescription: false);

		public void TestAutoRatingOneOffQuote_JobChargeDescription_CompanyTariff_RegistryDisable_LocalClient()
			=> AssertAutoRatingOneOffQuote_JobChargeDescription_CompanyTariff(enableLocalChargeCodeDescriptionDefault: false, isLocalClient: true, expectedLocalDescription: false);

		public void TestAutoRatingOneOffQuote_JobChargeDescription_CompanyTariff_RegistryEnable_NonLocalClient()
			=> AssertAutoRatingOneOffQuote_JobChargeDescription_CompanyTariff(enableLocalChargeCodeDescriptionDefault: true, isLocalClient: false, expectedLocalDescription: false);

		public void TestAutoRatingOneOffQuote_JobChargeDescription_CompanyTariff_RegistryEnable_LocalClient()
			=> AssertAutoRatingOneOffQuote_JobChargeDescription_CompanyTariff(enableLocalChargeCodeDescriptionDefault: true, isLocalClient: true, expectedLocalDescription: true);

		void AssertAutoRatingOneOffQuote_JobChargeDescription_CompanyTariff(bool enableLocalChargeCodeDescriptionDefault, bool isLocalClient, bool expectedLocalDescription)
		{
			var chargeCode = Helper.ChargeCodes.New("CC1", "CC1 Description", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			chargeCode.AC_LocalLanguageDescription = "CC1 Local Description 地方";

			NewClient2.OH_IsDebtor = true;

			Factory.Save();

			var companyTariff = Helper.NewLevel1CompanyTariffWithSingleRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "US", "CC1", 100m);
			var companyTariffRateEntry = companyTariff.AllEntries.Single();
			var companyTariffRateLine = (RateLine)companyTariffRateEntry.RateLines.Single();
			companyTariffRateLine.TL_RateDesc = "CC1 Updated Description";
			companyTariffRateLine.TL_RateDescLocal = "CC1 Updated Local Description 地方";
			companyTariff.Factory.Save();

			var oneOffQuote = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, NewClient, NewClient, NewClient2, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.AcceptedBookingWithQuote);

			NewClient2.CompanyData.RateTariffLevels.SetLevel("FRT", 1);

			if (isLocalClient)
			{
				NewClient2.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				AssertEquals("Local Client", true, NewClient2.IsLocalCountry);
			}
			else
			{
				NewClient2.OH_RL_NKClosestPort = "USLAX";
				AssertEquals("NonLocal Client", false, NewClient2.IsLocalCountry);
			}

			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableLocalChargeCodeDescriptionDefault))
			{
				AutorateAndAssert
				(
					expected: new[]
					{
						new AssertionCharge
						{
							ChargeCode = "CC1",
							JR_OSSellAmt = 100m,
							JR_Desc = expectedLocalDescription
								? "CC1 Updated Local Description 地方"
								: "CC1 Updated Description"
						},
					},
					oneOffQuote,
					NewClient2,
					autorateCosts: false
				);
			}
		}

		public void TestAutoRatingOneOffQuote_JobChargeDescription_RegistryDisable()
		{
			var chargeCode = Helper.ChargeCodes.New("CC1", "CC1 Description", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			chargeCode.AC_LocalLanguageDescription = "CC1 Local Description 地方";

			var oneOffQuote = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, NewClient, NewClient, NewClient2, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.AcceptedBookingWithQuote);

			var clientRate = Helper.NewClientRate(NewClient2);
			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "US", "CC1", 100m);
			var clientRateLine = (RateLine)clientRateEntry.RateLines.Single();
			clientRateLine.TL_RateDesc = "CC1 Updated Description";
			clientRateLine.TL_RateDescLocal = "CC1 Updated Local Description 地方";

			Factory.Save();

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)Rating.Business.Testing.TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AutorateAndAssert
				(
					expected: new[]
					{
						new AssertionCharge
						{
							ChargeCode = "CC1",
							JR_OSSellAmt = 100m,
							JR_Desc = "CC1 Updated Description"
						},
					},
					oneOffQuote,
					NewClient2,
					autorateCosts: false
				);
			}
		}

		public void TestAutoRatingOneOffQuote_JobChargeDescription_RegistryEnable_NonLocalClient()
		{
			var chargeCode = Helper.ChargeCodes.New("CC1", "CC1 Description", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			chargeCode.AC_LocalLanguageDescription = "CC1 Local Description 地方";

			var oneOffQuote = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, NewClient, NewClient, NewClient2, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.AcceptedBookingWithQuote);

			var clientRate = Helper.NewClientRate(NewClient2);
			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "US", "CC1", 100m);
			var clientRateLine = (RateLine)clientRateEntry.RateLines.Single();
			clientRateLine.TL_RateDesc = "CC1 Updated Description";
			clientRateLine.TL_RateDescLocal = "CC1 Updated Local Description 地方";

			Factory.Save();

			AssertNotEquals("NonLocal Client", clientRateEntry.ParentRatingHeader.Header.OH_RL_NKClosestPort, GlbBranch.CurrentBranch.GB_RL_NKHomePort);

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)Rating.Business.Testing.TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutorateAndAssert
				(
					expected: new[]
					{
						new AssertionCharge
						{
							ChargeCode = "CC1",
							JR_OSSellAmt = 100m,
							JR_Desc = "CC1 Updated Description"
						},
					},
					oneOffQuote,
					NewClient2,
					autorateCosts: false
				);
			}
		}

		public void TestAutoRatingOneOffQuote_JobChargeDescription_RegistryEnable_LocalClient()
		{
			var chargeCode = Helper.ChargeCodes.New("CC1", "CC1 Description", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			chargeCode.AC_LocalLanguageDescription = "CC1 Local Description 地方";

			var oneOffQuote = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, NewClient, NewClient, NewClient2, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.AcceptedBookingWithQuote);
			oneOffQuote.StartDate = ZDate.Today;
			oneOffQuote.EndDate = ZDate.Today;

			var clientRate = Helper.NewClientRate(NewClient2);
			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "US", "CC1", 100m);
			var clientRateLine = (RateLine)clientRateEntry.RateLines.Single();
			clientRateLine.TL_RateDesc = "CC1 Updated Description";
			clientRateLine.TL_RateDescLocal = "CC1 Updated Local Description 地方";
			clientRateEntry.ParentRatingHeader.Header.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			Factory.Save();

			AssertEquals("Local Client", clientRateEntry.ParentRatingHeader.Header.OH_RL_NKClosestPort, GlbBranch.CurrentBranch.GB_RL_NKHomePort);

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)Rating.Business.Testing.TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutorateAndAssert
				(
					expected: new[]
					{
						new AssertionCharge
						{
							ChargeCode = "CC1",
							JR_OSSellAmt = 100m,
							JR_Desc = "CC1 Updated Local Description 地方"
						},
					},
					oneOffQuote,
					NewClient2,
					autorateCosts: false
				);
			}
		}

		#endregion

		#region RateSelector

		[GuiTest]
		public void TestAutoratingOneOffQuote_ShouldShowCargoSphereRateSelector()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(consignee);

			var oneOffQuote = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, consignor, consignor, consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			oneOffQuote.StartDate = ZDate.Today;
			oneOffQuote.EndDate = ZDate.Today;
			var container = oneOffQuote.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.TC_ContainerCount = 1;

			var job = CreateJob(oneOffQuote, oneOffQuote.Quote.TH_QuoteNumber);
			job.PlugInData = oneOffQuote;
			job.LocalChargesPK = consignee.PK;

			Factory.Save();

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var timesRateSelectorFormIsShown = 0;
				var rateSelectorIsShown = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is RateChooserForm rateChooser)
					{
						timesRateSelectorFormIsShown++;
						rateChooser.SkipRateSelectionButton_Click(null, EventArgs.Empty);
						rateSelectorIsShown = rateChooser.Model.Criteria.AdapterType == AdapterType.OneOffQuote;
					}
				});

				AutorateWithManualSelectAndAssert("Autorate", null, oneOffQuote, consignee);

				AssertEquals("Rate Selector should be shown once", 1, timesRateSelectorFormIsShown);
				AssertEquals("Rate Selector should be shown for consol", true, rateSelectorIsShown);
			}
		}

		[GuiTest]
		public void TestAutoratingOneOffQuote_ShouldShowCargoGuideRateSelector()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(consignee);

			var oneOffQuote = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, consignor, consignor, consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			oneOffQuote.StartDate = ZDate.Today;
			oneOffQuote.EndDate = ZDate.Today;

			var job = CreateJob(oneOffQuote, oneOffQuote.Quote.TH_QuoteNumber);
			job.PlugInData = oneOffQuote;
			job.LocalChargesPK = consignee.PK;

			Factory.Save();

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var timesRateSelectorFormIsShown = 0;
				var rateSelectorIsShown = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is RateSelectorForm rateSelector)
					{
						timesRateSelectorFormIsShown++;
						rateSelector.BtnSkipRateSelectionClick(this, null);
						rateSelectorIsShown = rateSelector.Criteria.AdapterType == AdapterType.OneOffQuote;
					}
				});

				AutorateWithManualSelectAndAssert("Autorate", null, oneOffQuote, consignee);

				AssertEquals("Rate Selector should be shown once", 1, timesRateSelectorFormIsShown);
				AssertEquals("Rate Selector should be shown for consol", true, rateSelectorIsShown);
			}
		}

		[GuiTest]
		public void TestAutoratingBookingWithQuote_ShouldShowCargoSphereRateSelector()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(consignee);

			var bookingWithQuote = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, consignor, consignor, consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.AcceptedBookingWithQuote);
			bookingWithQuote.StartDate = ZDate.Today;
			bookingWithQuote.EndDate = ZDate.Today;
			var container = bookingWithQuote.QuotedBookingContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerCount = 1;

			var job = CreateJob(bookingWithQuote, bookingWithQuote.Quote.TH_QuoteNumber);
			job.PlugInData = bookingWithQuote;
			job.LocalChargesPK = consignee.PK;

			Factory.Save();

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var timesRateSelectorFormIsShown = 0;
				var rateSelectorIsShown = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is RateChooserForm rateChooser)
					{
						timesRateSelectorFormIsShown++;
						rateChooser.SkipRateSelectionButton_Click(null, EventArgs.Empty);
						rateSelectorIsShown = rateChooser.Model.Criteria.AdapterType == AdapterType.BookingWithQuote;
					}
				});

				AutorateWithManualSelectAndAssert("Autorate", null, bookingWithQuote, consignee);

				AssertEquals("Rate Selector should be shown once", 1, timesRateSelectorFormIsShown);
				AssertEquals("Rate Selector should be shown for consol", true, rateSelectorIsShown);
			}
		}

		[GuiTest]
		public void TestAutoratingBookingWithQuote_ShouldNotShowCargoSphereRateSelectorAndContinueAutorating_WhenLoadPortIsEmpty()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(consignee);

			var bookingWithQuote = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, consignor, consignor, consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.AcceptedBookingWithQuote);
			bookingWithQuote.StartDate = ZDate.Today;
			bookingWithQuote.EndDate = ZDate.Today;
			var container = bookingWithQuote.QuotedBookingContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerCount = 1;

			var job = CreateJob(bookingWithQuote, bookingWithQuote.Quote.TH_QuoteNumber);
			job.PlugInData = bookingWithQuote;
			job.LocalChargesPK = consignee.PK;

			bookingWithQuote.LoadPort = null;

			Factory.Save();

			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US");
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.AddRateLine("FRT", "FLT").GetCalculator<FlatCalculator>().BaseRate = 500m;
			Factory.Save();

			AssertEquals("Load port should be empty", "", bookingWithQuote.LoadPort);

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var timesRateSelectorFormIsShown = 0;
				var rateSelectorIsShown = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is RateChooserForm rateChooser)
					{
						timesRateSelectorFormIsShown++;
						rateChooser.SkipRateSelectionButton_Click(null, EventArgs.Empty);
						rateSelectorIsShown = rateChooser.Model.Criteria.AdapterType == AdapterType.BookingWithQuote;
					}
				});

				var expectedCharges = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 500m
					}
				};

				AutorateWithManualSelectAndAssert("Autorate", expectedCharges, bookingWithQuote, consignee);

				AssertEquals("Rate Selector should not be shown", 0, timesRateSelectorFormIsShown);
				AssertEquals("Rate Selector should not be shown for Booking With Quote", false, rateSelectorIsShown);
			}
		}

		[GuiTest]
		public void TestAutoratingBookingWithQuote_ShouldNotShowCargoSphereRateSelectorAndContinueAutorating_WhenDischargePortIsEmpty()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(consignee);

			var bookingWithQuote = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, consignor, consignor, consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.AcceptedBookingWithQuote);
			bookingWithQuote.StartDate = ZDate.Today;
			bookingWithQuote.EndDate = ZDate.Today;
			var container = bookingWithQuote.QuotedBookingContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerCount = 1;

			var job = CreateJob(bookingWithQuote, bookingWithQuote.Quote.TH_QuoteNumber);
			job.PlugInData = bookingWithQuote;
			job.LocalChargesPK = consignee.PK;

			bookingWithQuote.DischargePort = null;

			Factory.Save();

			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US");
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.AddRateLine("FRT", "FLT").GetCalculator<FlatCalculator>().BaseRate = 500m;
			Factory.Save();

			AssertEquals("Discharge port should be empty", "", bookingWithQuote.DischargePort);

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var timesRateSelectorFormIsShown = 0;
				var rateSelectorIsShown = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is RateChooserForm rateChooser)
					{
						timesRateSelectorFormIsShown++;
						rateChooser.SkipRateSelectionButton_Click(null, EventArgs.Empty);
						rateSelectorIsShown = rateChooser.Model.Criteria.AdapterType == AdapterType.BookingWithQuote;
					}
				});

				var expectedCharges = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 500m
					}
				};

				AutorateWithManualSelectAndAssert("Autorate", expectedCharges, bookingWithQuote, consignee);

				AssertEquals("Rate Selector should not be shown", 0, timesRateSelectorFormIsShown);
				AssertEquals("Rate Selector should not be shown for Booking With Quote", false, rateSelectorIsShown);
			}
		}

		[GuiTest]
		public void TestAutoratingBookingWithQuote_ShouldShowCargoGuideRateSelector()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(consignee);

			var bookingWithQuote = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, consignor, consignor, consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.AcceptedBookingWithQuote);
			bookingWithQuote.StartDate = ZDate.Today;
			bookingWithQuote.EndDate = ZDate.Today;

			var job = CreateJob(bookingWithQuote, bookingWithQuote.Quote.TH_QuoteNumber);
			job.PlugInData = bookingWithQuote;
			job.LocalChargesPK = consignee.PK;

			Factory.Save();

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var timesRateSelectorFormIsShown = 0;
				var rateSelectorIsShown = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is RateSelectorForm rateSelector)
					{
						timesRateSelectorFormIsShown++;
						rateSelector.BtnSkipRateSelectionClick(this, null);
						rateSelectorIsShown = rateSelector.Criteria.AdapterType == AdapterType.BookingWithQuote;
					}
				});

				AutorateWithManualSelectAndAssert("Autorate", null, bookingWithQuote, consignee);

				AssertEquals("Rate Selector should be shown once", 1, timesRateSelectorFormIsShown);
				AssertEquals("Rate Selector should be shown for consol", true, rateSelectorIsShown);
			}
		}

		[GuiTest]
		public void TestAutoratingBookingWithQuote_ShouldNotShowCargoGuideRateSelectorAndContinueAutorating_WhenLoadPortIsEmpty()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(consignee);

			var bookingWithQuote = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, consignor, consignor, consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.AcceptedBookingWithQuote);
			bookingWithQuote.StartDate = ZDate.Today;
			bookingWithQuote.EndDate = ZDate.Today;

			var job = CreateJob(bookingWithQuote, bookingWithQuote.Quote.TH_QuoteNumber);
			job.PlugInData = bookingWithQuote;
			job.LocalChargesPK = consignee.PK;

			bookingWithQuote.LoadPort = null;

			Factory.Save();

			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "US");
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.AddRateLine("FRT", "FLT").GetCalculator<FlatCalculator>().BaseRate = 500m;
			Factory.Save();

			AssertEquals("Load port should be empty", "", bookingWithQuote.LoadPort);

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var timesRateSelectorFormIsShown = 0;
				var rateSelectorIsShown = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is RateSelectorForm rateSelector)
					{
						timesRateSelectorFormIsShown++;
						rateSelector.BtnSkipRateSelectionClick(this, null);
						rateSelectorIsShown = rateSelector.Criteria.AdapterType == AdapterType.BookingWithQuote;
					}
				});

				var expectedCharges = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 500m
					}
				};

				AutorateWithManualSelectAndAssert("Autorate", expectedCharges, bookingWithQuote, consignee);

				AssertEquals("Rate Selector should not be shown", 0, timesRateSelectorFormIsShown);
				AssertEquals("Rate Selector should not be shown for Booking With Quote", false, rateSelectorIsShown);
			}
		}

		[GuiTest]
		public void TestAutoratingBookingWithQuote_ShouldNotShowCargoGuideRateSelectorAndContinueAutorating_WhenDischargePortIsEmpty()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(consignee);

			var bookingWithQuote = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, consignor, consignor, consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.AcceptedBookingWithQuote);
			bookingWithQuote.StartDate = ZDate.Today;
			bookingWithQuote.EndDate = ZDate.Today;

			var job = CreateJob(bookingWithQuote, bookingWithQuote.Quote.TH_QuoteNumber);
			job.PlugInData = bookingWithQuote;
			job.LocalChargesPK = consignee.PK;

			bookingWithQuote.DischargePort = null;

			Factory.Save();

			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "US");
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.AddRateLine("FRT", "FLT").GetCalculator<FlatCalculator>().BaseRate = 500m;
			Factory.Save();

			AssertEquals("Discharge port should be empty", "", bookingWithQuote.DischargePort);

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var timesRateSelectorFormIsShown = 0;
				var rateSelectorIsShown = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is RateSelectorForm rateSelector)
					{
						timesRateSelectorFormIsShown++;
						rateSelector.BtnSkipRateSelectionClick(this, null);
						rateSelectorIsShown = rateSelector.Criteria.AdapterType == AdapterType.BookingWithQuote;
					}
				});

				var expectedCharges = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 500m
					}
				};

				AutorateWithManualSelectAndAssert("Autorate", expectedCharges, bookingWithQuote, consignee);

				AssertEquals("Rate Selector should not be shown", 0, timesRateSelectorFormIsShown);
				AssertEquals("Rate Selector should not be shown for Booking With Quote", false, rateSelectorIsShown);
			}
		}

		[GuiTest]
		public void TestAutoratingQuickBooking_ShouldShowCargoSphereRateSelector()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(consignee);

			var quickBooking = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, consignor, consignor, consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.BookingOnly);
			quickBooking.StartDate = ZDate.Today;
			quickBooking.EndDate = ZDate.Today;
			var container = quickBooking.QuotedBookingContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerCount = 1;

			var job = CreateJob(quickBooking, quickBooking.Booking.JS_UniqueConsignRef);
			job.PlugInData = quickBooking;
			job.LocalChargesPK = consignee.PK;

			Factory.Save();

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var timesRateSelectorFormIsShown = 0;
				var rateSelectorIsShown = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is RateChooserForm rateChooser)
					{
						timesRateSelectorFormIsShown++;
						rateChooser.SkipRateSelectionButton_Click(null, EventArgs.Empty);
						rateSelectorIsShown = rateChooser.Model.Criteria.AdapterType == AdapterType.Booking;
					}
				});

				AutorateWithManualSelectAndAssert("Autorate", null, quickBooking, consignee);

				AssertEquals("Rate Selector should be shown once", 1, timesRateSelectorFormIsShown);
				AssertEquals("Rate Selector should be shown for consol", true, rateSelectorIsShown);
			}
		}

		[GuiTest]
		public void TestAutoratingQuickBooking_ShouldNotShowCargoSphereRateSelectorAndContinueAutorating_WhenLoadPortIsEmpty()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(consignee);

			var quickBooking = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, consignor, consignor, consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.BookingOnly);
			quickBooking.StartDate = ZDate.Today;
			quickBooking.EndDate = ZDate.Today;
			var container = quickBooking.QuotedBookingContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerCount = 1;

			var job = CreateJob(quickBooking, quickBooking.Booking.JS_UniqueConsignRef);
			job.PlugInData = quickBooking;
			job.LocalChargesPK = consignee.PK;

			quickBooking.LoadPort = null;

			Factory.Save();

			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US");
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.AddRateLine("FRT", "FLT").GetCalculator<FlatCalculator>().BaseRate = 500m;
			Factory.Save();

			AssertEquals("Load port should be empty", "", quickBooking.LoadPort);

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var timesRateSelectorFormIsShown = 0;
				var rateSelectorIsShown = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is RateChooserForm rateChooser)
					{
						timesRateSelectorFormIsShown++;
						rateChooser.SkipRateSelectionButton_Click(null, EventArgs.Empty);
						rateSelectorIsShown = rateChooser.Model.Criteria.AdapterType == AdapterType.Booking;
					}
				});

				var expectedCharges = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 500m
					}
				};

				AutorateWithManualSelectAndAssert("Autorate", expectedCharges, quickBooking, consignee);

				AssertEquals("Rate Selector should not be shown", 0, timesRateSelectorFormIsShown);
				AssertEquals("Rate Selector should not be shown for Quick Booking", false, rateSelectorIsShown);
			}
		}

		[GuiTest]
		public void TestAutoratingQuickBooking_ShouldNotShowCargoSphereRateSelectorAndContinueAutorating_WhenDischargePortIsEmpty()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(consignee);

			var quickBooking = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, consignor, consignor, consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.BookingOnly);
			quickBooking.StartDate = ZDate.Today;
			quickBooking.EndDate = ZDate.Today;
			var container = quickBooking.QuotedBookingContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerCount = 1;

			var job = CreateJob(quickBooking, quickBooking.Booking.JS_UniqueConsignRef);
			job.PlugInData = quickBooking;
			job.LocalChargesPK = consignee.PK;

			quickBooking.DischargePort = null;

			Factory.Save();

			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US");
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.AddRateLine("FRT", "FLT").GetCalculator<FlatCalculator>().BaseRate = 500m;
			Factory.Save();

			AssertEquals("Discharge port should be empty", "", quickBooking.DischargePort);

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var timesRateSelectorFormIsShown = 0;
				var rateSelectorIsShown = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is RateChooserForm rateChooser)
					{
						timesRateSelectorFormIsShown++;
						rateChooser.SkipRateSelectionButton_Click(null, EventArgs.Empty);
						rateSelectorIsShown = rateChooser.Model.Criteria.AdapterType == AdapterType.Booking;
					}
				});

				var expectedCharges = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 500m
					}
				};

				AutorateWithManualSelectAndAssert("Autorate", expectedCharges, quickBooking, consignee);

				AssertEquals("Rate Selector should not be shown", 0, timesRateSelectorFormIsShown);
				AssertEquals("Rate Selector should not be shown for Quick Booking", false, rateSelectorIsShown);
			}
		}

		[GuiTest]
		public void TestAutoratingQuickBooking_ShouldShowCargoGuideRateSelector()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(consignee);

			var quickBooking = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, consignor, consignor, consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.BookingOnly);
			quickBooking.StartDate = ZDate.Today;
			quickBooking.EndDate = ZDate.Today;

			var job = CreateJob(quickBooking, quickBooking.Booking.JS_UniqueConsignRef);
			job.PlugInData = quickBooking;
			job.LocalChargesPK = consignee.PK;

			Factory.Save();

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var timesRateSelectorFormIsShown = 0;
				var rateSelectorIsShown = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is RateSelectorForm rateSelector)
					{
						timesRateSelectorFormIsShown++;
						rateSelector.BtnSkipRateSelectionClick(this, null);
						rateSelectorIsShown = rateSelector.Criteria.AdapterType == AdapterType.Booking;
					}
				});

				AutorateWithManualSelectAndAssert("Autorate", null, quickBooking, consignee);

				AssertEquals("Rate Selector should be shown once", 1, timesRateSelectorFormIsShown);
				AssertEquals("Rate Selector should be shown for consol", true, rateSelectorIsShown);
			}
		}

		[GuiTest]
		public void TestAutoratingQuickBooking_ShouldNotShowCargoGuideRateSelectorAndContinueAutorating_WhenLoadPortIsEmpty()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(consignee);

			var quickBooking = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, consignor, consignor, consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.BookingOnly);
			quickBooking.StartDate = ZDate.Today;
			quickBooking.EndDate = ZDate.Today;

			var job = CreateJob(quickBooking, quickBooking.Booking.JS_UniqueConsignRef);
			job.PlugInData = quickBooking;
			job.LocalChargesPK = consignee.PK;

			quickBooking.LoadPort = null;

			Factory.Save();

			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "US");
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.AddRateLine("FRT", "FLT").GetCalculator<FlatCalculator>().BaseRate = 500m;
			Factory.Save();

			AssertEquals("Load port should be empty", "", quickBooking.LoadPort);

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var timesRateSelectorFormIsShown = 0;
				var rateSelectorIsShown = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is RateSelectorForm rateSelector)
					{
						timesRateSelectorFormIsShown++;
						rateSelector.BtnSkipRateSelectionClick(this, null);
						rateSelectorIsShown = rateSelector.Criteria.AdapterType == AdapterType.Booking;
					}
				});

				var expectedCharges = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 500m
					}
				};

				AutorateWithManualSelectAndAssert("Autorate", expectedCharges, quickBooking, consignee);

				AssertEquals("Rate Selector should not be shown", 0, timesRateSelectorFormIsShown);
				AssertEquals("Rate Selector should be shown for quick booking", false, rateSelectorIsShown);
			}
		}

		[GuiTest]
		public void TestAutoratingQuickBooking_ShouldNotShowCargoGuideRateSelectorAndContinueAutorating_WhenDischargePortIsEmpty()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(consignee);

			var quickBooking = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, consignor, consignor, consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.BookingOnly);
			quickBooking.StartDate = ZDate.Today;
			quickBooking.EndDate = ZDate.Today;

			var job = CreateJob(quickBooking, quickBooking.Booking.JS_UniqueConsignRef);
			job.PlugInData = quickBooking;
			job.LocalChargesPK = consignee.PK;

			quickBooking.DischargePort = null;

			Factory.Save();

			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "US");
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.AddRateLine("FRT", "FLT").GetCalculator<FlatCalculator>().BaseRate = 500m;
			Factory.Save();

			AssertEquals("Discharge port should be empty", "", quickBooking.DischargePort);

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var timesRateSelectorFormIsShown = 0;
				var rateSelectorIsShown = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is RateSelectorForm rateSelector)
					{
						timesRateSelectorFormIsShown++;
						rateSelector.BtnSkipRateSelectionClick(this, null);
						rateSelectorIsShown = rateSelector.Criteria.AdapterType == AdapterType.Booking;
					}
				});

				var expectedCharges = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 500m
					}
				};

				AutorateWithManualSelectAndAssert("Autorate", expectedCharges, quickBooking, consignee);

				AssertEquals("Rate Selector should not be shown", 0, timesRateSelectorFormIsShown);
				AssertEquals("Rate Selector should be shown for quick booking", false, rateSelectorIsShown);
			}
		}

		#endregion

		public void TestAutoRateOneOffQuote_ShouldMatchTransitTime()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(consignee);

			var oneOffQuote = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, consignor, consignor, consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.AcceptedBookingWithQuote);
			oneOffQuote.StartDate = ZDate.Today;
			oneOffQuote.EndDate = ZDate.Today;

			var job = CreateJob(oneOffQuote, oneOffQuote.Quote.TH_QuoteNumber);
			job.PlugInData = oneOffQuote;
			job.LocalChargesPK = consignee.PK;

			Factory.Save();

			var rateEntry0 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "US");
			rateEntry0.RateLines.RemoveAndDeleteAll();
			rateEntry0.AddRateLine("FRT", "FLT").GetCalculator<FlatCalculator>().BaseRate = 500m;

			var rateEntry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "US");
			rateEntry1.TI_TransitTime = "1";
			rateEntry1.RateLines.RemoveAndDeleteAll();
			rateEntry1.AddRateLine("FRT", "FLT").GetCalculator<FlatCalculator>().BaseRate = 100m;

			var rateEntry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "US");
			rateEntry2.TI_TransitTime = "3";
			rateEntry2.RateLines.RemoveAndDeleteAll();
			rateEntry2.AddRateLine("FRT", "FLT").GetCalculator<FlatCalculator>().BaseRate = 300m;

			var rateEntry3 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "US");
			rateEntry3.TI_TransitTime = "SMD";
			rateEntry3.RateLines.RemoveAndDeleteAll();
			rateEntry3.AddRateLine("FRT", "FLT").GetCalculator<FlatCalculator>().BaseRate = 350m;

			var rateEntry4 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "US");
			rateEntry4.TI_TransitTime = "OVN";
			rateEntry4.RateLines.RemoveAndDeleteAll();
			rateEntry4.AddRateLine("FRT", "FLT").GetCalculator<FlatCalculator>().BaseRate = 370m;

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 500m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100
				},
				new AssertionCharge
				{
				ChargeCode = "FRT",
				JR_OSSellAmt = 300m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 350m
				},
				new AssertionCharge
				{
				ChargeCode = "FRT",
				JR_OSSellAmt = 370m
				}
			};
			AutorateAndAssert("No transit time defined. Should match all entries.", expectedCharges, oneOffQuote, consignee, autorateCosts: false);

			oneOffQuote.TransitTime = "2";
			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100m
				},
			};
			AutorateAndAssert("Should match entry with closest transit time 2/1", expectedCharges, oneOffQuote, consignee, autorateCosts: false);

			oneOffQuote.TransitTime = "SMD";
			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 350m
				},
			};
			AutorateAndAssert("Should match entry with SMD transit time", expectedCharges, oneOffQuote, consignee, autorateCosts: false);

			oneOffQuote.TransitTime = "OVN";
			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 370m
				},
			};
			AutorateAndAssert("Should match entry with OVN transit time", expectedCharges, oneOffQuote, consignee, autorateCosts: false);

			oneOffQuote.TransitTime = "3";
			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 300m
				},
			};
			AutorateAndAssert("Should match entry with closest transit time 3/3", expectedCharges, oneOffQuote, consignee, autorateCosts: false);

			rateEntry0.TI_TransitTime = string.Empty;
			rateEntry3.TI_TransitTime = "6";
			rateEntry4.TI_TransitTime = "7";

			Factory.Save();

			oneOffQuote.TransitTime = "5";
			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 350
				},
			};
			AutorateAndAssert("Should match entry with closest transit time 5/4", expectedCharges, oneOffQuote, consignee, autorateCosts: false);

			oneOffQuote.TransitTime = "SMD";
			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100m
				},
			};
			AutorateAndAssert("Should match entry with the least transit time SMD/1", expectedCharges, oneOffQuote, consignee, autorateCosts: false);

			oneOffQuote.TransitTime = "OVN";
			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100m
				},
			};
			AutorateAndAssert("Should match entry with the least transit time OVN/1", expectedCharges, oneOffQuote, consignee, autorateCosts: false);
		}

		public void TestAutoRateQuickBooking_ShouldMatchContractNumbers()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var clientRate = Helper.NewClientRate(consignee);

			var rateEntry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US");
			rateEntry1.TI_RC = GP20.PK;
			rateEntry1.TI_TransitTime = "1";
			rateEntry1.TI_ContractNumber = "A";
			rateEntry1.RateLines.RemoveAndDeleteAll();
			rateEntry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 100m;

			var rateEntry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US");
			rateEntry2.TI_RC = GP20.PK;
			rateEntry2.TI_TransitTime = "2";
			rateEntry2.TI_ContractNumber = "B";
			rateEntry2.RateLines.RemoveAndDeleteAll();
			rateEntry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 200m;

			var rateEntry3 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US");
			rateEntry3.TI_RC = GP20.PK;
			rateEntry3.RateLines.RemoveAndDeleteAll();
			rateEntry3.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 300m;

			var quickBooking = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, consignor, consignor, consignee, null, "AUSYD", "USLAX", 100m, 10m, QuotedBookingState.BookingOnly);
			var container = quickBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerCount = 1;
			container.JC_RC = GP20.PK;

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 200m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 300m
				}
			};

			AutorateAndAssert("There should be no filter. All charges created", expectedCharges, quickBooking, consignee, autorateCosts: false);

			var number = quickBooking.Booking.Numbers.AddNew();
			number.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CLC;
			number.CE_EntryNum = "B";

			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 200m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 300m
				}
			};

			AutorateAndAssert("Entry2 and Entry3 match the contract number", expectedCharges, quickBooking, consignee, autorateCosts: false);

			number.CE_EntryNum = "A";

			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 300m
				}
			};

			AutorateAndAssert("Entry1 and Entry3 match the contract number", expectedCharges, quickBooking, consignee, autorateCosts: false);

			number.CE_EntryNum = "";
			Factory.Save();

			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 300m
				}
			};

			AutorateAndAssert("Only Entry3 is a match", expectedCharges, quickBooking, consignee, autorateCosts: false);

			rateEntry1.TI_ContractNumber = ZString.Empty;
			rateEntry2.TI_ContractNumber = ZString.Empty;
			Factory.Save();

			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 200m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 300m
				}
			};

			AutorateAndAssert("All Charges should come through", expectedCharges, quickBooking, consignee, autorateCosts: false);

			quickBooking.Booking.Numbers.RemoveAndDeleteAll();

			AutorateAndAssert("All Charges should come through", expectedCharges, quickBooking, consignee, autorateCosts: false);
		}

		[TestDate(2021, 01, 01)]
		public void TestAutoRateBookingWithQuote_FilteringCarrierContractNumber()
		{
			var costing = Helper.NewCosting(TransportProvider1);

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US");
			rateEntry1.TI_RC = GP20.PK;
			rateEntry1.TI_ContractNumber = "B";
			rateEntry1.RateLines.RemoveAndDeleteAll();
			rateEntry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 200m;

			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US");
			rateEntry2.TI_RC = GP20.PK;
			rateEntry2.RateLines.RemoveAndDeleteAll();
			rateEntry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 300m;
			rateEntry2.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 400m;

			var quotedBooking = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, Consignor, Consignor, Consignee, null, "AUSYD", "USLAX", 0m, 0m, QuotedBookingState.AcceptedBookingWithQuote);
			quotedBooking.StartDate = ZDate.Today;
			quotedBooking.EndDate = ZDate.Today;
			quotedBooking.OH_Carrier = TransportProvider1.PK;
			quotedBooking.CarrierContractNumber = "B";

			var container = quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerCount = 1;
			container.JC_RC = GP20.PK;

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 200m
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 400m
				}
			};

			var message = @"When QB's Carrier Contract Number is not empty, rate entries with same Carrier Contract Number and blank Carrier Contract Number should be loaded.
Rates with same CCN has higher priority when same charges found.";
			AutorateAndAssert(message, expectedCharges, quotedBooking, Consignee, autorateRevenue: false);

			quotedBooking.CarrierContractNumber = ZString.Empty;

			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 300m
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 400m
				}
			};

			message = "When QB's Carrier Contract Number is empty, only rates with empty Carrier Contract Number should be loaded.";
			AutorateAndAssert(message, expectedCharges, quotedBooking, Consignee, autorateRevenue: false);
		}

		[TestDate(2021, 01, 01)]
		public void TestAutoRateOneOffQuote_FilteringCarrierContractNumber()
		{
			var costing = Helper.NewCosting(TransportProvider1);

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US");
			rateEntry1.TI_RC = GP20.PK;
			rateEntry1.TI_ContractNumber = "B";
			rateEntry1.RateLines.RemoveAndDeleteAll();
			rateEntry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 200m;

			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US");
			rateEntry2.TI_RC = GP20.PK;
			rateEntry2.RateLines.RemoveAndDeleteAll();
			rateEntry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 300m;

			var quotedBooking = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, Consignor, Consignor, Consignee, null, "AUSYD", "USLAX", 0m, 0m, QuotedBookingState.QuoteOnly);
			quotedBooking.StartDate = ZDate.Today;
			quotedBooking.EndDate = ZDate.Today;
			quotedBooking.OH_Carrier = TransportProvider1.PK;

			var contractNumber = quotedBooking.Quote.CurrentOneOffQuote.Numbers.AddNew();
			contractNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
			contractNumber.CE_EntryNum = "B";

			var container = quotedBooking.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_ContainerCount = 1;
			container.TC_RC = GP20.PK;

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 200m
				}
			};

			AutorateAndAssert("The rate entry matching job contract number should take priority", expectedCharges, quotedBooking, Consignee, autorateRevenue: false);

			quotedBooking.Quote.CurrentOneOffQuote.Numbers.RemoveAndDeleteAll();

			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 200m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 300m
				}
			};

			AutorateAndAssert("There is no filter and both rate should be applied", expectedCharges, quotedBooking, Consignee, autorateRevenue: false);
		}

		[TestDate(2013, 11, 29)]
		public void TestSpoteQuoteChargesWhenExchangeRateExists()
		{
			#region Setup Currency

			var usdCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));

			var sellRate = usdCurrency.ExchangeRates.AddNew();
			sellRate.RE_ExRateType = ExchangeRateTypes.Code.SellRate;
			sellRate.RE_StartDate = new ZDateTime(2013, 11, 1);
			sellRate.RE_ExpiryDate = new ZDateTime(2013, 11, 30);
			sellRate.RE_SellRate = 0.8m;

			var buyRate = usdCurrency.ExchangeRates.AddNew();
			buyRate.RE_ExRateType = ExchangeRateTypes.Code.BuyRate;
			buyRate.RE_StartDate = new ZDateTime(2013, 11, 1);
			buyRate.RE_ExpiryDate = new ZDateTime(2013, 11, 30);
			buyRate.RE_SellRate = 0.6m;

			#endregion

			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "US", "CN");
			rateEntry.TI_RX_NKCurrency = "USD";
			rateEntry.TI_RateStartDate = new ZDate(2013, 11, 5);
			rateEntry.TI_RateEndDate = new ZDate(2013, 11, 20);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var spotQuote = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, NewClient, Consignor, Consignee, null, "USLAX", "CNSHA", 100m, 10m, QuotedBookingState.QuoteOnly);
			spotQuote.StartDate = new ZDate(2013, 11, 6);
			spotQuote.EndDate = new ZDate(2013, 11, 20);

			var job = CreateJob(spotQuote, spotQuote.Quote.TH_QuoteNumber);
			job.PlugInData = spotQuote;
			job.LocalChargesPK = NewClient.PK;

			Factory.Save();

			var expected = new[]
							{
								new AssertionCharge
									{
										ChargeCode = "FRT",
										JR_RX_NKCostCurrency = "USD",
										JR_OSCostAmt = 166666.70m,
										JR_LocalCostAmt = 277777.83m,
										JR_OSSellAmt = 166666.70m,
										JR_LocalSellAmt = 277777.83m
									}
							};

			AutorateAndAssert(expected, spotQuote, NewClient);
		}

		void AssertAutoRateCopyChargesFromQuote(bool isCrossTrade)
		{
			var consignee = Helper.NewOrgHeader();
			var consignor = Helper.NewOrgHeader();

			var origin = isCrossTrade ? "USLAX" : "AUSYD";
			var destination = "CNSHA";
			var localClient = isCrossTrade ? consignor : consignee;

			var clientRate = Helper.NewClientRate(localClient);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination, ZString.Empty, ZString.Empty);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			rateLine.TL_RateCalculator = UnitCalculator.Code;
			rateLine.TL_WeightVolume = QuantityUnit.KG;
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var quotedBooking = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, consignee, consignor, consignee, null, origin, destination, 100m, 10m, QuotedBookingState.QuoteOnly);
			quotedBooking.Quote.TH_OH = localClient.PK;

			var job = CreateJob(quotedBooking, quotedBooking.Quote.TH_QuoteNumber);
			job.PlugInData = quotedBooking;
			job.LocalChargesPK = localClient.PK;

			var charge = job.Charges.AddNew();
			charge.JR_AC = Helper.ChargeCodes["FRT"].PK;
			charge.JR_Desc = "FRT For Quote";
			charge.JR_OSSellAmt = 235m;
			charge.JR_SellRatingOverride = true;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "LSE";
			shipment.JS_ActualWeight = 3000;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;

			Factory.Save();

			var expected = new[]
							{
								new AssertionCharge
								{
									ChargeCode = "FRT",
									JR_OSSellAmt = 235m,
									JR_Desc = "FRT For Quote",
								}
							};

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(
				form =>
				{
					var possibleOneOffQuoteMatchesForm = form as PossibleOneOffQuoteMatchesForm;

					if (possibleOneOffQuoteMatchesForm != null)
					{
						var simpleOneOffQuoteCollectionWrapper = possibleOneOffQuoteMatchesForm.BusinessEntity as SimpleOneOffQuoteCollectionWrapper;
						if (simpleOneOffQuoteCollectionWrapper != null && simpleOneOffQuoteCollectionWrapper.PossibleMatches.Count > 0)
						{
							simpleOneOffQuoteCollectionWrapper.SelectedQuote = simpleOneOffQuoteCollectionWrapper.PossibleMatches[0];
						}
					}
				});

			AutorateAndAssert(expected, shipment, localClient);
		}

		public void TestAutoRateCopyChargesFromQuote()
		{
			AssertAutoRateCopyChargesFromQuote(false);
		}

		public void TestAutoRateCopyChargesFromQuoteWithCrossTrade()
		{
			AssertAutoRateCopyChargesFromQuote(true);
		}

		public void TestAutoRateCostsOnQuotedBooking()
		{
			var cost = Helper.NewCosting(TransportProvider1);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "ZA", "AU");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costEntry.AddRateLine("FRT", FlatCalculator.Code, "", CurrencyCodes.Australia);
			costLine.GetCalculator<FlatCalculator>().BaseRate = 1500m;

			var desEntry = cost.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, ZString.Empty, "AU");
			desEntry.RateLines.RemoveAndDeleteAll();
			var desCostLine = desEntry.AddRateLine("DDOC", FlatCalculator.Code, "", CurrencyCodes.Australia);
			desCostLine.GetCalculator<FlatCalculator>().BaseRate = 500;

			var quotedBooking = CreateQuotedBooking(TransportModes.Air, "LSE", "FOB", NewClient, Consignor, Consignee, TransportProvider1, "ZAJNB", "AUSYD", 100m, 0m);

			Factory.Save();

			var expected = new[]
							{
								new AssertionCharge
									{
										ChargeCode = "FRT",
										JR_OSCostAmt = 1500m,
									},
								new AssertionCharge
									{
										ChargeCode = "DDOC",
										JR_OSCostAmt = 500m,
									}
							};

			AutorateAndAssert(expected, quotedBooking, Consignor);
		}

		public void TestAutoRating_OneOffQuoteJob_ShouldApplyPerHBRates()
		{
			var rate = Helper.NewClientRate(NewClient);

			var entry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUBNE", "NZAKL");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("OAWB", UnitCalculator.Code, "HB", "AUD");
			line.GetCalculator<UnitCalculator>().PerUnit = 55m;

			Factory.Save();

			var quotedBooking = CreateQuotedBooking(TransportModes.Air, "LSE", string.Empty, NewClient, null, Consignee, null, "AUBNE", "NZAKL", 1000m, 50m, QuotedBookingState.QuoteOnly);

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "OAWB",
					JR_OSSellAmt = 55M,
				}
			};

			AutorateAndAssert(expectedCharges, quotedBooking, NewClient, autorateCosts: false);
		}

		public void TestExecuteAutoratingCreatesAndRemovesFallbackNote_AllowSavingOfAutoRatingLogNote() => TestExecuteAutoratingCreatesAndRemovesFallbackNote(true, true);

		public void TestExecuteAutoratingCreatesAndRemovesFallbackNote_NotAllowSavingOfAutoRatingLogNote() => TestExecuteAutoratingCreatesAndRemovesFallbackNote(false, false);

		void TestExecuteAutoratingCreatesAndRemovesFallbackNote(bool allowSavingOfAutoRatingLogNote, bool expectedNoteAfterSave)
		{
			DataRegistryRating.Instance.AllowSavingOfAutoRatingLogNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowSavingOfAutoRatingLogNote);

			var rate = Helper.NewClientRate(NewClient);

			var entry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUBNE", "NZAKL");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("OAWB", UnitCalculator.Code, "HB", "AUD");
			line.GetCalculator<UnitCalculator>().PerUnit = 55m;

			Factory.Save();

			var quotedBooking = CreateQuotedBooking(TransportModes.Air, "LSE", string.Empty, NewClient, null, Consignee, null, "AUBNE", "NZAKL", 1000m, 50m, QuotedBookingState.QuoteOnly);

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "OAWB",
					JR_OSSellAmt = 55M,
				}
			};

			AutorateAndAssert(expectedCharges, quotedBooking, NewClient, autorateCosts: false);

			AssertEquals("AutoRate should create AutoRatingLog", true, quotedBooking.Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).Length > 0);
			Factory.Save();
			AssertEquals("ExpectedNoteAfterSave", expectedNoteAfterSave, quotedBooking.Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).Length > 0);
		}

		public void TestAutoRating_OneOffQuoteJob_ShouldNotFilterCharges_WithMessageTypeOnAgencyCalculator()
		{
			Helper.ChargeCodes.New("CLR", "Clearing", AgencyCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			Helper.ChargeCodes.New("OLDD", "Origin Labour", AgencyCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var rate = Helper.NewClientRate(NewClient);

			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUBNE", "NZAKL");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine_WithoutMessageType = rateEntry.AddRateLine("CLR", AgencyCalculator.Code, "", CurrencyCodes.Australia);
			var calculator_WithoutMessageType = rateLine_WithoutMessageType.GetCalculator<AgencyCalculator>();
			calculator_WithoutMessageType.AgencyLineType = RateLineTypeList.Codes.PerTariffLinePerEntry;
			calculator_WithoutMessageType.AgencyRate = 150m;
			calculator_WithoutMessageType.AgencyFeeType = RateFeeTypeList.Codes.PerInvoice;
			calculator_WithoutMessageType.IncludedHeaders = 1;

			var rateLine_WithMessageType = rateEntry.AddRateLine("OLDD", AgencyCalculator.Code, "", CurrencyCodes.Australia);
			var calculator_WithMessageType = rateLine_WithMessageType.GetCalculator<AgencyCalculator>();
			calculator_WithMessageType.AgencyLineType = RateLineTypeList.Codes.PerInvoiceLinePerEntry;
			calculator_WithMessageType.AgencyRate = 200m;
			calculator_WithMessageType.AgencyFeeType = RateFeeTypeList.Codes.PerEntry;
			calculator_WithMessageType.IncludedHeaders = 1;
			calculator_WithMessageType.MessageType = SharedJobMessageTypeList.Codes.Import;
			calculator_WithMessageType.MessageSubType = "FRM";

			Factory.Save();

			var quotedBooking = CreateQuotedBooking(TransportModes.Air, "LSE", "CFR", NewClient, null, Consignee, null, "AUBNE", "NZAKL", 1m, 1m, QuotedBookingState.QuoteOnly);

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "CLR",
					JR_OSSellAmt = 150
				},
				new AssertionCharge
				{
					ChargeCode = "OLDD",
					JR_OSSellAmt = 200
				}
			};

			AutorateAndAssert(expectedCharges, quotedBooking, NewClient, autorateCosts: false);
		}

		public void TestQuickBookingCarrierIsConsideredInAutorating()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_IsConsignee = true;
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;

			var rate = Helper.NewClientRate(localClient);
			var entry1 = rate.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD", "", "");
			entry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1m;

			var entry2 = rate.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD", "", "");
			entry2.TI_OH_TransportProvider = carrier.PK;
			var rateLine2 = entry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2m;

			Factory.Save();

			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			booking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			quotedBooking.Mode = "LSE";
			quotedBooking.PaymentTerms = "FOB";
			quotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			quotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = localClient.MainAddress.PK;
			quotedBooking.Origin = "USLAX";
			quotedBooking.Destination = "AUSYD";
			quotedBooking.Weight = 1m;
			quotedBooking.Volume = 3m;

			Job testJob = CreateJob(quotedBooking, quotedBooking.Booking.JS_UniqueConsignRef);
			testJob.PlugInData = quotedBooking;
			testJob.LocalChargesPK = localClient.PK;

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 500m
						}
				};

			AutorateAndAssert(expected, quotedBooking, localClient);

			quotedBooking.OH_Carrier = carrier.PK;

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 1000m
						}
				};

			AutorateAndAssert(expected, quotedBooking, localClient);
		}

		public void TestSpotQuoteCarrierIsConsideredInAutorating()
		{
			var carrier = Helper.NewOrgHeader();
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = Factory.NewWithValidTestData<RefAirline>().PK;

			Factory.Save();

			var rate = Helper.NewClientRate(NewClient);

			var entry1 = rate.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD");
			entry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = entry1.AddRateLine("FRT", UnitCalculator.Code, "KG");
			rateLine1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1m;

			var entry2 = rate.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD");
			entry2.TI_OH_TransportProvider = carrier.PK;
			var rateLine2 = entry2.AddRateLine("FRT", UnitCalculator.Code, "KG");
			rateLine2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2m;

			var quotedBooking = CreateQuotedBooking(TransportModes.Air, "LSE", "FOB", NewClient, null, Consignee, null, "USLAX", "AUSYD", 1m, 3m);

			Factory.Save();

			var testJob = CreateJob(quotedBooking, quotedBooking.Quote.TH_QuoteNumber);
			testJob.PlugInData = quotedBooking;
			testJob.LocalChargesPK = NewClient.PK;

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 500m
						}
				};

			AutorateAndAssert(expected, quotedBooking, NewClient);

			quotedBooking.OH_Carrier = carrier.PK;
			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 1000m
						}
				};

			AutorateAndAssert(expected, quotedBooking, NewClient);
		}

		public void TestPrintingQuotedBookingHandlesConcurrencyError()
		{
			var quotedBooking = CreateQuotedBooking(TransportModes.Air, "LSE", "EXW", Consignor, Consignor, null, null, "USLAX", "AUBNE", 1m, 1m, QuotedBookingState.QuoteOnly);

			var job = CreateJob(quotedBooking, quotedBooking.Quote.TH_QuoteNumber);
			var charge = job.Charges.AddNew();
			charge.JR_AC = Helper.ChargeCodes["FRT"].PK;

			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var quotedBooking2 = factory2.Load<QuotedBooking>(quotedBooking.PK);

			var documentSupporter = (Quote.QuoteDocumentSupporter)quotedBooking.DocumentSupporter;
			PrintTask task = new DummyPrintTask();
			quotedBooking.Quote.DocumentPrintMode = QuotationDocumentMode.Final;
			documentSupporter.RunTask(task);

			var documentSupporter2 = (Quote.QuoteDocumentSupporter)quotedBooking2.DocumentSupporter;
			PrintTask task2 = new DummyPrintTask();
			quotedBooking2.Quote.DocumentPrintMode = QuotationDocumentMode.Final;

			AssertNoExceptionThrown(() => documentSupporter2.RunTask(task2));
		}

		QuotedBooking GetAutoratedQuotedBooking(QuotedBookingState bookingState)
		{
			var clientRate = Helper.NewClientRate(Consignor);
			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			entry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var costing = Helper.NewCosting(null);
			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			entry2.RateLines.RemoveAndDeleteAll();
			var rateLine2 = entry2.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 10m;

			var oneOffQuoteOrBooking = CreateQuotedBooking(TransportModes.Air, RateMode.LSE, IncoTerms.CostAndFreight, Consignor, Consignor, Consignee, null, "AUSYD", "USLAX", 100m, 0m, bookingState);
			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 2000m,
					RevenueCalculationDescription = "FRT: 100 Kilogram(s) @ AUD 20.00/KG",
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSCostAmt = 1000m,
					CostCalculationDescription = "BAF: 100 Kilogram(s) @ AUD 10.00/KG"
				},
			};
			AutorateAndAssert(expectedCharges, oneOffQuoteOrBooking, Consignor);

			return oneOffQuoteOrBooking;
		}

		public void TestQuotedBookingFromOneOffQuoteCanReautorate_NoDuplicateChargeCreated()
		{
			var autoratedQuotedBooking = GetAutoratedQuotedBooking(QuotedBookingState.QuoteOnly);

			autoratedQuotedBooking.ConvertQuoteToQuotedBooking();
			Factory.Save();

			var quotedBooking = Helper.LoadInNewFactory(autoratedQuotedBooking);
			quotedBooking.Weight = 105m;

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 2100m,
					RevenueCalculationDescription = "FRT: 105 Kilogram(s) @ AUD 20.00/KG",
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSCostAmt = 1050m,
					CostCalculationDescription = "BAF: 105 Kilogram(s) @ AUD 10.00/KG"
				},
			};

			// load existing job to avoid charges clearing in AutorateAndAssert
			var quotedBookingJob = new Job.Loader(quotedBooking).TryLoadOrCreate();
			AutorateAndAssert("Autorating should not create duplicate or extra charges", expectedCharges, quotedBooking, Consignor, job: quotedBookingJob);
		}

		public void TestQuotedBookingFromOneOffQuoteCanAutorateFromOtherCompany_LogWarningForRateLines()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "NEW";
			company.GC_RN_NKCountryCode = "US";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			company.Branches.Add(branch);
			var allowedDepartment = Factory.NewWithValidTestData<AccAllowedBranchDepartmentCombo>();
			branch.AllowedDepartments.Add(allowedDepartment);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), allowedDepartment.Department.PK.ToGuid()))
			{
				Helper.ChargeCodes.New("NEWCHARGE", "New Charge Code", UnitCalculator.Code, ChargeCodeGroupList.Codes.Freight);

				var clientRate = Helper.NewClientRate(Consignor);
				var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
				entry.RateLines.RemoveAndDeleteAll();
				var rateLine = entry.AddRateLine("NEWCHARGE", UnitCalculator.Code, QuantityUnit.KG, "AUD");
				rateLine.GetCalculator<UnitCalculator>().PerUnit = 20m;
			}

			var quotedBooking = CreateQuotedBooking(TransportModes.Air, RateMode.LSE, IncoTerms.CostAndFreight, Consignor, Consignor, Consignee, null, "AUSYD", "USLAX", 100m, 0m, QuotedBookingState.QuoteOnly);
			quotedBooking.Quote.TH_GC = company.PK;
			Factory.Save();

			AutorateAndAssert("Autorating should return no charges", expected: null, quotedBooking, Consignor);
			AssertAutoratingAuditLogContains(quotedBooking, "The charge does not belong to this company. Please check and validate your rates data. If the problem persists, please raise an incident.");
		}

		public void TestAutorate_OneOffQuote_ShouldCreateCAREventWithAuditInfo()
		{
			var clientRate = Helper.NewClientRate(Consignee);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			var rateLine = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var quotedBooking = CreateQuotedBooking(
				TransportModes.Air,
				RateMode.LSE,
				IncoTerms.CostAndFreight,
				Consignee,
				Consignor,
				Consignee,
				null,
				"AUSYD",
				"USLAX",
				100m,
				0m,
				QuotedBookingState.QuoteOnly);

			quotedBooking.Quote.TH_QuoteNumber = "666";
			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 2000m,
					RevenueCalculationDescription = "FRT: 100 Kilogram(s) @ AUD 20.00/KG",
				},
			};

			AutorateAndAssert(expectedCharges, quotedBooking, Consignee, autorateRevenue: true);

			var log = quotedBooking.Logs.MostRecentLogByEventTime(AutoEvents.ChargesHaveBeenAutoRated);
			AssertEquals("Expected 'Type' to be 'OneOffQuote'.", "OneOffQuote", log.Parameters[EventReferenceParameters.Type]);
			AssertStartsWith("Expected 'JobNumber' to start with '666'.", "666", log.Parameters[EventReferenceParameters.JobNumber]);
		}

		public void TestAutorate_Booking_ShouldCreateCAREventWithAuditInfo()
		{
			var clientRate = Helper.NewClientRate(Consignee);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			var rateLine = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var quotedBooking = CreateQuotedBooking(
				TransportModes.Air,
				RateMode.LSE,
				IncoTerms.CostAndFreight,
				Consignor,
				Consignor,
				Consignee,
				null,
				"AUSYD",
				"USLAX",
				100m,
				0m,
				QuotedBookingState.BookingOnly);

			quotedBooking.Booking.JS_UniqueConsignRef = "666";
			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 2000m,
					RevenueCalculationDescription = "FRT: 100 Kilogram(s) @ AUD 20.00/KG",
				},
			};

			AutorateAndAssert(expectedCharges, quotedBooking, Consignee, autorateRevenue: true);

			var log = quotedBooking.Logs.MostRecentLogByEventTime(AutoEvents.ChargesHaveBeenAutoRated);
			AssertEquals("Expected log parameter to match booking type", "Booking", log.Parameters[EventReferenceParameters.Type]);
			AssertEquals("Expected log parameter to match job number reference", "666", log.Parameters[EventReferenceParameters.JobNumber]);
		}

		public void TestAutorate_BookingWithQuote_ShouldCreateCAREventWithAuditInfo()
		{
			var clientRate = Helper.NewClientRate(Consignee);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			var rateLine = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var quotedBooking = CreateQuotedBooking(
				TransportModes.Air,
				RateMode.LSE,
				IncoTerms.CostAndFreight,
				Consignor,
				Consignor,
				Consignee,
				null,
				"AUSYD",
				"USLAX",
				100m,
				0m,
				QuotedBookingState.AcceptedBookingWithQuote);

			quotedBooking.Quote.TH_QuoteNumber = "666";
			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 2000m,
					RevenueCalculationDescription = "FRT: 100 Kilogram(s) @ AUD 20.00/KG",
				},
			};

			AutorateAndAssert(expectedCharges, quotedBooking, Consignee, autorateRevenue: true);

			var log = quotedBooking.Logs.MostRecentLogByEventTime(AutoEvents.ChargesHaveBeenAutoRated);
			AssertEquals("Expected event type to be 'BookingWithQuote'.", "BookingWithQuote", log.Parameters[EventReferenceParameters.Type]);
			AssertStartsWith("Expected job number to start with '666'.", "666", log.Parameters[EventReferenceParameters.JobNumber]);
		}

		public void TestAutoRateOneOffQuote_PossibleCarriers_RevenueIsDuplicated_UnitCalculator()
		{
			var carrier1 = TransportProvider1;
			var carrier2 = TransportProvider2;
			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier3.OH_IsShippingLine = true;

			var costing1 = Helper.NewCosting(carrier1);
			var costing2 = Helper.NewCosting(carrier2);
			var costing3 = Helper.NewCosting(carrier3);
			var clientRate = Helper.NewClientRate(Consignor);

			var cost1Entry = costing1.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", removeLines: true);
			cost1Entry.TI_RC = GP20.PK;
			cost1Entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1001m;

			var cost2Entry = costing2.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", removeLines: true);
			cost2Entry.TI_RC = GP20.PK;
			cost2Entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1020m;

			var cost3Entry = costing3.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", removeLines: true);
			cost3Entry.TI_RC = GP20.PK;
			cost3Entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1300m;

			var revenueEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", removeLines: true);
			revenueEntry.TI_RC = GP20.PK;
			revenueEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1500m;

			var quotedBooking = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, Consignor, Consignor, Consignee, null, "AUSYD", "USLAX", 0m, 0m, QuotedBookingState.QuoteOnly);
			quotedBooking.StartDate = ZDate.Today;
			quotedBooking.EndDate = ZDate.Today;
			quotedBooking.OH_Carrier = carrier1.PK;

			var oneOffShipment = quotedBooking.Quote.CurrentOneOffQuote;
			oneOffShipment.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier1.PK;
			oneOffShipment.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier2.PK;
			oneOffShipment.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier3.PK;

			var container = quotedBooking.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_ContainerCount = 1;
			container.TC_RC = GP20.PK;

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1001m,
					JR_OSSellAmt = 1500m,
					CostAccountCode = carrier1.OH_Code,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1020m,
					JR_OSSellAmt = 1500m,
					CostAccountCode = carrier2.OH_Code,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1300m,
					JR_OSSellAmt = 1500m,
					CostAccountCode = carrier3.OH_Code,
				}
			};

			AutorateAndAssert("revenue is duplicated", expectedCharges, quotedBooking, Consignee, autorateCosts: true, autorateRevenue: true);
		}

		public void TestAutoRateOneOffQuote_PossibleCarriers_MultipleCosts_MultipleSellRates_ClientRate()
		{
			var carrier1 = TransportProvider1;
			var carrier2 = TransportProvider2;

			var costing1 = Helper.NewCosting(carrier1);
			costing1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", "FRT", 1);
			var costing2 = Helper.NewCosting(carrier2);
			costing2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", "FRT", 2);

			var clientRate = Helper.NewClientRate(Consignor);
			var clientRateEntry1 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", "FRT", 1);
			clientRateEntry1.TI_OH_TransportProvider = carrier1.PK;
			var clientRateEntry2 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", "FRT", 2);
			clientRateEntry2.TI_OH_TransportProvider = carrier2.PK;

			var quotedBooking = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, Consignor, Consignor, Consignee, null, "AUSYD", "USLAX", 0m, 0m, QuotedBookingState.QuoteOnly);
			var oneOffShipment = quotedBooking.Quote.CurrentOneOffQuote;
			oneOffShipment.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier1.PK;
			oneOffShipment.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier2.PK;

			Factory.Save();

			AutorateAndAssert
			(
				"Charge should get cost and sell from correct carrier costing and clientRate",
				expected: new[]
				{
					new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 1, JR_OSSellAmt = 1, CostAccountCode = carrier1.OH_Code, },
					new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 2, JR_OSSellAmt = 2, CostAccountCode = carrier2.OH_Code, },
				},
				quotedBooking,
				Consignee,
				autorateCosts: true,
				autorateRevenue: true
			);
		}

		public void TestAutoRateOneOffQuote_PossibleCarriersAndCreditors_MultipleCosts_MultipleSellRates_ClientRate()
		{
			var carrier1 = TransportProvider1;
			var carrier2 = TransportProvider2;
			var creditor1 = Helper.CreateCreditor("CREDITOR1");
			var creditor2 = Helper.CreateCreditor("CREDITOR2");

			var costing1 = Helper.NewCosting(carrier1);
			costing1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", "FRT", 100);
			var costing2 = Helper.NewCosting(carrier2);
			costing2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", "FRT", 200);

			var clientRate = Helper.NewClientRate(Consignor);
			var clientRateEntry1 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", "FRT", 10);
			clientRateEntry1.TI_OH_TransportProvider = carrier1.PK;
			var clientRateEntry2 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", "FRT", 20);
			clientRateEntry2.TI_OH_TransportProvider = carrier2.PK;

			var quotedBooking = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, Consignor, Consignor, Consignee, null, "AUSYD", "USLAX", 0m, 0m, QuotedBookingState.QuoteOnly);
			var oneOffShipment = quotedBooking.Quote.CurrentOneOffQuote;
			var possibleCarrier1 = oneOffShipment.PossibleCarriers.AddNew();
			possibleCarrier1.TTC_OH_Carrier = carrier1.PK;
			possibleCarrier1.TTC_OH_Creditor = creditor1.PK;
			var possibleCarrier2 = oneOffShipment.PossibleCarriers.AddNew();
			possibleCarrier2.TTC_OH_Carrier = carrier2.PK;
			possibleCarrier2.TTC_OH_Creditor = creditor2.PK;

			Factory.Save();

			AutorateAndAssert
			(
				"Charge should get cost and sell from correct carrier costing and clientRate",
				expected: new[]
				{
					new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 100, JR_OSSellAmt = 10, CostAccountCode = creditor1.OH_Code, },
					new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 200, JR_OSSellAmt = 20, CostAccountCode = creditor2.OH_Code, },
				},
				quotedBooking,
				Consignee,
				autorateCosts: true,
				autorateRevenue: true
			);
		}

		public void TestAutoRateOneOffQuote_PossibleCarriers_MultipleCosts_MultipleSellRates_CompanyTariff()
		{
			var carrier1 = TransportProvider1;
			var carrier2 = TransportProvider2;

			var costing1 = Helper.NewCosting(carrier1);
			costing1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", "FRT", 1002m);
			var costing2 = Helper.NewCosting(carrier2);
			costing2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", "FRT", 1001m);

			var companyTariff = Helper.NewCompanyTariff();
			companyTariff.TH_GlobalRateLevel = 1;
			var companyTariffRateEntry1 = companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", "FRT", 2001m);
			companyTariffRateEntry1.TI_OH_TransportProvider = carrier1.PK;
			var companyTariffRateEntry2 = companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", "FRT", 2002m);
			companyTariffRateEntry2.TI_OH_TransportProvider = carrier2.PK;

			companyTariff.Factory.Save();

			Consignor.CompanyData.RateTariffLevels.SetLevel("FRT", 1);

			var quotedBooking = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, Consignor, Consignor, Consignee, null, "AUSYD", "USLAX", 0m, 0m, QuotedBookingState.QuoteOnly);
			var oneOffShipment = quotedBooking.Quote.CurrentOneOffQuote;
			oneOffShipment.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier1.PK;
			oneOffShipment.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier2.PK;

			Factory.Save();

			AutorateAndAssert
			(
				"Charge should get cost and sell from correct carrier costing and companyTariff",
				expected: new[]
				{
					new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 1002m, JR_OSSellAmt = 2001m, CostAccountCode = carrier1.OH_Code, },
					new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 1001m, JR_OSSellAmt = 2002m, CostAccountCode = carrier2.OH_Code, },
				},
				quotedBooking,
				Consignee,
				autorateCosts: true,
				autorateRevenue: true
			);
		}

		public void TestAutoRateOneOffQuote_PossibleCarriers_RevenueNotDuplicatedWhenProviderNotEmpty()
		{
			var carrier1 = TransportProvider1;
			var carrier2 = TransportProvider2;
			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier3.OH_IsShippingLine = true;

			var costing1 = Helper.NewCosting(carrier1);
			var costing2 = Helper.NewCosting(carrier2);
			var costing3 = Helper.NewCosting(carrier3);
			var clientRate = Helper.NewClientRate(Consignor);

			var cost1Entry = costing1.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", removeLines: true);
			cost1Entry.TI_RC = GP20.PK;
			cost1Entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1001m;

			var cost2Entry = costing2.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", removeLines: true);
			cost2Entry.TI_RC = GP20.PK;
			cost2Entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1020m;

			var cost3Entry = costing3.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", removeLines: true);
			cost3Entry.TI_RC = GP20.PK;
			cost3Entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1300m;

			var revenueEntryProvider1 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", removeLines: true);
			revenueEntryProvider1.TI_RC = GP20.PK;
			revenueEntryProvider1.TI_OH_TransportProvider = carrier1.PK;
			revenueEntryProvider1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1500m;

			var revenueEntryProvider2 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", removeLines: true);
			revenueEntryProvider2.TI_RC = GP20.PK;
			revenueEntryProvider2.TI_OH_TransportProvider = carrier2.PK;
			revenueEntryProvider2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1501m;

			var revenueEntryProvider3 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", removeLines: true);
			revenueEntryProvider3.TI_RC = GP20.PK;
			revenueEntryProvider3.TI_OH_TransportProvider = carrier3.PK;
			revenueEntryProvider3.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1502m;

			var quotedBooking = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, Consignor, Consignor, Consignee, null, "AUSYD", "USLAX", 0m, 0m, QuotedBookingState.QuoteOnly);
			quotedBooking.StartDate = ZDate.Today;
			quotedBooking.EndDate = ZDate.Today;
			quotedBooking.OH_Carrier = carrier1.PK;

			var oneOffShipment = quotedBooking.Quote.CurrentOneOffQuote;
			oneOffShipment.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier1.PK;
			oneOffShipment.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier2.PK;
			oneOffShipment.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier3.PK;

			var container = quotedBooking.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_ContainerCount = 1;
			container.TC_RC = GP20.PK;

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1001m,
					JR_OSSellAmt = 1500m,
					CostAccountCode = carrier1.OH_Code,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1020m,
					JR_OSSellAmt = 1501m,
					CostAccountCode = carrier2.OH_Code,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1300m,
					JR_OSSellAmt = 1502m,
					CostAccountCode = carrier3.OH_Code,
				}
			};

			AutorateAndAssert("revenue is duplicated", expectedCharges, quotedBooking, Consignee, autorateCosts: true, autorateRevenue: true);
		}

		public void TestAutoRateOneOffQuote_PossibleCarriers_MainCarrierHasNoRates()
		{
			var carrier1 = TransportProvider1;
			var carrier2 = TransportProvider2;
			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier3.OH_IsShippingLine = true;

			// carrier 3 will be the main quote carrier and will have no rates
			var costing1 = Helper.NewCosting(carrier1);
			var costing2 = Helper.NewCosting(carrier2);
			var clientRate = Helper.NewClientRate(Consignor);

			var cost1Entry = costing1.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", removeLines: true);
			cost1Entry.TI_RC = GP20.PK;
			cost1Entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1001m;

			var cost2Entry = costing2.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", removeLines: true);
			cost2Entry.TI_RC = GP20.PK;
			cost2Entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1020m;

			var revenueEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", removeLines: true);
			revenueEntry.TI_RC = GP20.PK;
			revenueEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1500m;

			var quotedBooking = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, Consignor, Consignor, Consignee, null, "AUSYD", "USLAX", 0m, 0m, QuotedBookingState.QuoteOnly);
			quotedBooking.StartDate = ZDate.Today;
			quotedBooking.EndDate = ZDate.Today;
			quotedBooking.OH_Carrier = carrier3.PK;

			var oneOffShipment = quotedBooking.Quote.CurrentOneOffQuote;
			oneOffShipment.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier1.PK;
			oneOffShipment.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier2.PK;

			var container = quotedBooking.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_ContainerCount = 1;
			container.TC_RC = GP20.PK;

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1001m,
					JR_OSSellAmt = 1500m,
					CostAccountCode = carrier1.OH_Code,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1020m,
					JR_OSSellAmt = 1500m,
					CostAccountCode = carrier2.OH_Code,
				}
			};

			AutorateAndAssert("revenue is duplicated", expectedCharges, quotedBooking, Consignee, autorateCosts: true, autorateRevenue: true);
		}

		public void TestAutoRateOneOffQuote_PossibleCarriers_RevenueIsDuplicated_RatingCostAfterRevenue()
		{
			var carrier1 = TransportProvider1;
			var carrier2 = TransportProvider2;
			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier3.OH_IsShippingLine = true;

			var costing1 = Helper.NewCosting(carrier1);
			var costing2 = Helper.NewCosting(carrier2);
			var costing3 = Helper.NewCosting(carrier3);
			var clientRate = Helper.NewClientRate(Consignor);

			var cost1Entry = costing1.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", removeLines: true);
			cost1Entry.TI_RC = GP20.PK;
			cost1Entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1001m;

			var cost2Entry = costing2.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", removeLines: true);
			cost2Entry.TI_RC = GP20.PK;
			cost2Entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1020m;

			var cost3Entry = costing3.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", removeLines: true);
			cost3Entry.TI_RC = GP20.PK;
			cost3Entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1300m;

			var revenueEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", removeLines: true);
			revenueEntry.TI_RC = GP20.PK;
			revenueEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1500m;

			var quotedBooking = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, Consignor, Consignor, Consignee, null, "AUSYD", "USLAX", 0m, 0m, QuotedBookingState.QuoteOnly);
			quotedBooking.StartDate = ZDate.Today;
			quotedBooking.EndDate = ZDate.Today;
			quotedBooking.OH_Carrier = carrier1.PK;

			var oneOffShipment = quotedBooking.Quote.CurrentOneOffQuote;
			oneOffShipment.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier1.PK;
			oneOffShipment.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier2.PK;
			oneOffShipment.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier3.PK;

			var container = quotedBooking.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_ContainerCount = 1;
			container.TC_RC = GP20.PK;

			Factory.Save();

			// Due to creditor defaulting code, we have to make the carrier and creditor empty first, so that
			// the resulting charges have a blank creditor
			quotedBooking.OH_Carrier = ZGuid.Empty;
			quotedBooking.Creditor = ZGuid.Empty;
			var job = new JobHeader.Loader(quotedBooking).TryLoadOrCreate() as Job;
			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1500m,
					JR_OSSellAmt = 1500m,
					CostAccountCode = "",
				}
			};

			AutorateAndAssert("revenue only", expectedCharges, quotedBooking, Consignee, job: job, autorateCosts: false, autorateRevenue: true);

			quotedBooking.OH_Carrier = carrier1.PK;
			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1001m,
					JR_OSSellAmt = 1500m,
					CostAccountCode = carrier1.OH_Code,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1020m,
					JR_OSSellAmt = 1500m,
					CostAccountCode = carrier2.OH_Code,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1300m,
					JR_OSSellAmt = 1500m,
					CostAccountCode = carrier3.OH_Code,
				}
			};

			AutorateAndAssert("revenue is duplicated", expectedCharges, quotedBooking, Consignee, job: job, autorateCosts: true, autorateRevenue: false);
			AssertEquals("revenue payment basis is on charge1", 1, job.Charges[0].PaymentBases.Cast<JobPaymentBasis>().Count(x => x.PBS_PerUnitRate == 1500m));
			AssertEquals("revenue payment basis is on charge2", 1, job.Charges[1].PaymentBases.Cast<JobPaymentBasis>().Count(x => x.PBS_PerUnitRate == 1500m));
			AssertEquals("revenue payment basis is on charge3", 1, job.Charges[2].PaymentBases.Cast<JobPaymentBasis>().Count(x => x.PBS_PerUnitRate == 1500m));
		}

		public void TestAutoRateOneOffQuote_PossibleCarriers_RevenueIsDuplicated_CostBasedCalculator()
		{
			var carrier1 = TransportProvider1;
			var carrier2 = TransportProvider2;
			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier3.OH_IsShippingLine = true;

			var costing1 = Helper.NewCosting(carrier1);
			var costing2 = Helper.NewCosting(carrier2);
			var costing3 = Helper.NewCosting(carrier3);
			var clientRate = Helper.NewClientRate(Consignor);

			var cost1Entry = costing1.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", removeLines: true);
			cost1Entry.TI_RC = GP20.PK;
			cost1Entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1001m;

			var cost2Entry = costing2.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", removeLines: true);
			cost2Entry.TI_RC = GP20.PK;
			cost2Entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1020m;

			var cost3Entry = costing3.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", removeLines: true);
			cost3Entry.TI_RC = GP20.PK;
			cost3Entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1300m;

			var revenueEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", removeLines: true);
			var revenueLine = revenueEntry.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			var calc = revenueLine.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			calc.PerUnit = +1000;

			var quotedBooking = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, Consignor, Consignor, Consignee, null, "AUSYD", "USLAX", 0m, 0m, QuotedBookingState.QuoteOnly);
			quotedBooking.StartDate = ZDate.Today;
			quotedBooking.EndDate = ZDate.Today;
			quotedBooking.OH_Carrier = carrier1.PK;

			var oneOffShipment = quotedBooking.Quote.CurrentOneOffQuote;
			oneOffShipment.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier1.PK;
			oneOffShipment.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier2.PK;
			oneOffShipment.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier3.PK;

			var container = quotedBooking.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_ContainerCount = 1;
			container.TC_RC = GP20.PK;

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1001m,
					JR_OSSellAmt = 2001m,
					CostAccountCode = carrier1.OH_Code,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1020m,
					JR_OSSellAmt = 2020,
					CostAccountCode = carrier2.OH_Code,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1300m,
					JR_OSSellAmt = 2300m,
					CostAccountCode = carrier3.OH_Code,
				}
			};

			AutorateAndAssert("revenue is duplicated per carrier", expectedCharges, quotedBooking, Consignee, autorateCosts: true, autorateRevenue: true);
		}

		public void TestAutoRatingBookingWithQuote_GivenHBLDeliveryMode_ThenRateEntriesWithHBLDeliveryModeShouldBeMatched()
		{
			var consignor = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(consignor);
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "AUSYD");
			clientRateEntry.TI_HBLDeliveryMode = Core.Constants.HBLDeliveryModes.Codes.CFS_CY;
			clientRateEntry.RateLines.RemoveAndDeleteAll();
			clientRateEntry.AddRateLine("FRT", "FLT").GetCalculator<FlatCalculator>().BaseRate = 500m;

			var clientRateEntry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "AUSYD");
			clientRateEntry2.RateLines.RemoveAndDeleteAll();
			clientRateEntry2.AddRateLine("FRT", "FLT").GetCalculator<FlatCalculator>().BaseRate = 300m;

			var consignee = Helper.NewOrgHeader();
			var bookingWithQuote = CreateQuotedBooking(TransportModes.Sea, RatingConstants.RateCategory.FCL, ZString.Empty, consignor, consignor, consignee, null, "USLAX", "AUSYD", 10m, 1m, QuotedBookingState.AcceptedBookingWithQuote);
			bookingWithQuote.StartDate = ZDate.Today;
			bookingWithQuote.EndDate = ZDate.Today;
			bookingWithQuote.ContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CY;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 500m
						}
				};

			AutorateAndAssert("When Autorating Booking With Quote with HBL Delivery Mode, Client Rate with HBL Delivery should be matched", expected, bookingWithQuote, consignor, autorateCosts: false);
		}

		public void TestAutoRatingQuickBooking_GivenHBLDeliveryMode_ThenRateEntriesWithHBLDeliveryModeShouldBeMatched()
		{
			var consignor = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(consignor);
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "AUSYD");
			clientRateEntry.TI_HBLDeliveryMode = Core.Constants.HBLDeliveryModes.Codes.DOOR_ARPT;
			clientRateEntry.RateLines.RemoveAndDeleteAll();
			clientRateEntry.AddRateLine("FRT", "FLT").GetCalculator<FlatCalculator>().BaseRate = 500m;

			var clientRateEntry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "AUSYD");
			clientRateEntry2.RateLines.RemoveAndDeleteAll();
			clientRateEntry2.AddRateLine("FRT", "FLT").GetCalculator<FlatCalculator>().BaseRate = 300m;

			var consignee = Helper.NewOrgHeader();
			var quickBooking = CreateQuotedBooking(TransportModes.Sea, RatingConstants.RateCategory.FCL, ZString.Empty, consignor, consignor, consignee, null, "USLAX", "AUSYD", 10m, 1m, QuotedBookingState.BookingOnly);
			quickBooking.StartDate = ZDate.Today;
			quickBooking.EndDate = ZDate.Today;
			quickBooking.ContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_ARPT;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 500m
				}
			};

			AutorateAndAssert("When Autorating Quick Booking with HBL Delivery Mode, Client Rate with HBL Delivery should be matched", expected, quickBooking, consignor, autorateCosts: false);
		}

		#region Default Creditor

		public void TestDefaultCreditor_BookingWithQuote()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			var portTransport = Factory.NewWithValidTestData<OrgHeader>();
			portTransport.OH_IsShippingProvider = true;

			var origin = "AUSYD";
			var destination = "SGSIN";

			var costing1 = Helper.NewCosting(carrier);
			costing1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"FRT", 100m, "AUD");
			var costing2 = Helper.NewCosting(creditor);
			costing2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"BAF", 100m, "AUD");
			var standardCost = Helper.NewCosting(null);
			var standardEntry = standardCost.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"CAF", 100m, "AUD");
			standardEntry.TI_OH_TransportProvider = carrier.PK;
			var costing3 = Helper.NewCosting(portTransport);
			costing3.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.LSE, origin, destination,
				"OCART", 100m, "AUD");

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					CostAccountCode = creditor.OH_Code // Rate Provider matches OOQ Carrier and OOQ Creditor not empty
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					CostAccountCode = creditor.OH_Code // Rate Provider matches OOQ Creditor
				},
				new AssertionCharge
				{
					ChargeCode = "CAF",
					CostAccountCode = creditor.OH_Code // Standard costs with a matching transport provider with the OOQ carrier and OOQ creditor not empty
				},
				new AssertionCharge
				{
					ChargeCode = "OCART",
					CostAccountCode = portTransport.OH_Code // Costs from operator that should be their own creditor
				}
			};

			var bookingWithQuote = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, NewClient, Consignor, NewClient, carrier, origin,
				destination, 1000m, 2m, QuotedBookingState.AcceptedBookingWithQuote);
			bookingWithQuote.Booking.JS_OH_Creditor = creditor.PK;
			bookingWithQuote.Booking.DocsAndCartage.JP_OA_PickupCartageCoAddr_ZAddress.OrgPK = portTransport.PK;

			AutorateAndAssert(expected, bookingWithQuote, NewClient, autorateRevenue: false, autorateCosts: true);
		}

		public void TestDefaultCreditor_QuickBooking()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			var portTransport = Factory.NewWithValidTestData<OrgHeader>();
			portTransport.OH_IsShippingProvider = true;

			var origin = "AUSYD";
			var destination = "SGSIN";

			var costing1 = Helper.NewCosting(carrier);
			costing1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"FRT", 100m, "AUD");
			var costing2 = Helper.NewCosting(creditor);
			costing2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"BAF", 100m, "AUD");
			var standardCost = Helper.NewCosting(null);
			var standardEntry = standardCost.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"CAF", 100m, "AUD");
			standardEntry.TI_OH_TransportProvider = carrier.PK;
			var costing3 = Helper.NewCosting(portTransport);
			costing3.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.LSE, origin, destination,
				"OCART", 100m, "AUD");

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					CostAccountCode = creditor.OH_Code // Rate Provider matches OOQ Carrier and OOQ Creditor not empty
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					CostAccountCode = creditor.OH_Code // Rate Provider matches OOQ Creditor
				},
				new AssertionCharge
				{
					ChargeCode = "CAF",
					CostAccountCode = creditor.OH_Code // Standard costs with a matching transport provider with the OOQ carrier and OOQ creditor not empty
				},
				new AssertionCharge
				{
					ChargeCode = "OCART",
					CostAccountCode = portTransport.OH_Code // Costs from operator that should be their own creditor
				}
			};

			var quickBooking = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, NewClient, Consignor, NewClient, carrier, origin,
				destination, 1000m, 2m, QuotedBookingState.BookingOnly);
			quickBooking.Booking.JS_OH_Creditor = creditor.PK;
			quickBooking.Booking.DocsAndCartage.JP_OA_PickupCartageCoAddr_ZAddress.OrgPK = portTransport.PK;

			AutorateAndAssert(expected, quickBooking, NewClient, autorateRevenue: false, autorateCosts: true);
		}

		public void TestDefaultCreditor_OneOffQuote()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			var possibleCarrier1 = Factory.NewWithValidTestData<OrgHeader>();
			possibleCarrier1.OH_IsShippingProvider = true;
			var possibleCreditor = Factory.NewWithValidTestData<OrgHeader>();
			possibleCreditor.OH_IsCreditor = true;
			var possibleCarrier2 = Factory.NewWithValidTestData<OrgHeader>();
			possibleCarrier1.OH_IsShippingProvider = true;

			var origin = "AUSYD";
			var destination = "SGSIN";

			var standardCost = Helper.NewCosting(null);
			var standardRateEntry = standardCost.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"CAF", 100m, "AUD");
			standardRateEntry.TI_OH_TransportProvider = carrier.PK;
			var costing1 = Helper.NewCosting(carrier);
			costing1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"FRT", 100m, "AUD");
			var costing2 = Helper.NewCosting(creditor);
			costing2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"EFAF", 100m, "AUD");
			var costing3 = Helper.NewCosting(possibleCreditor);
			costing3.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"BAF", 100m, "AUD");
			var costing4 = Helper.NewCosting(possibleCarrier2);
			costing4.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"FSC", 100m, "AUD");

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "CAF",
					CostAccountCode = creditor.OH_Code // Standard costs with a matching transport provider with the OOQ carrier and OOQ creditor not empty
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					CostAccountCode = creditor.OH_Code // Rate Provider matches OOQ Carrier and OOQ Creditor not empty
				},
				new AssertionCharge
				{
					ChargeCode = "EFAF",
					CostAccountCode = creditor.OH_Code // Rate Provider matches OOQ Creditor
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					CostAccountCode = possibleCreditor.OH_Code // Rate Provider Matches Possible Carrier and Possible Creditor is not empty
				},
				new AssertionCharge
				{
					ChargeCode = "FSC",
					CostAccountCode = possibleCarrier2.OH_Code // Rate Provider Matches Possible Carrier and Possible Creditor is empty
				}
			};

			var oneOffQuote = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, NewClient, Consignor, NewClient, carrier, origin,
				destination, 1000m, 2m, QuotedBookingState.QuoteOnly);
			oneOffQuote.Quote.CurrentOneOffQuote.TT_OH_Creditor = creditor.PK;
			var oneOffCarrier1 = oneOffQuote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			oneOffCarrier1.TTC_OH_Carrier = possibleCarrier1.PK;
			oneOffCarrier1.TTC_OH_Creditor = possibleCreditor.PK;
			var oneOffCarrier2 = oneOffQuote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			oneOffCarrier2.TTC_OH_Carrier = possibleCarrier2.PK;

			AutorateAndAssert(expected, oneOffQuote, NewClient, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCopy_ConvertOOQ()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			var possibleCarrier1 = Factory.NewWithValidTestData<OrgHeader>();
			possibleCarrier1.OH_IsShippingProvider = true;
			var possibleCreditor = Factory.NewWithValidTestData<OrgHeader>();
			possibleCreditor.OH_IsCreditor = true;
			var possibleCarrier2 = Factory.NewWithValidTestData<OrgHeader>();
			possibleCarrier1.OH_IsShippingProvider = true;

			var origin = "AUSYD";
			var destination = "SGSIN";

			var standardCost = Helper.NewCosting(null);
			var standardRateEntry = standardCost.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"CAF", 100m, "AUD");
			standardRateEntry.TI_OH_TransportProvider = carrier.PK;
			var costing1 = Helper.NewCosting(carrier);
			costing1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"FRT", 100m, "AUD");
			var costing2 = Helper.NewCosting(creditor);
			costing2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"WAR", 100m, "AUD");
			var costing3 = Helper.NewCosting(possibleCreditor);
			costing3.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"BAF", 100m, "AUD");
			var costing4 = Helper.NewCosting(possibleCarrier2);
			costing4.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"FSC", 100m, "AUD");

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "CAF",
					CostAccountCode = creditor.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					CostAccountCode = creditor.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "WAR",
					CostAccountCode = creditor.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					CostAccountCode = possibleCreditor.OH_Code // Not included to be moved as charge creditor is not the chosen Carrier or Creditor
				},
				new AssertionCharge
				{
					ChargeCode = "FSC",
					CostAccountCode = possibleCarrier2.OH_Code // Not included to be moved as charge creditor is not the chosen Carrier or Creditor
				}
			};

			var oneOffQuote = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, NewClient, Consignor, NewClient, carrier, origin,
				destination, 1000m, 2m, QuotedBookingState.QuoteOnly);
			oneOffQuote.Quote.CurrentOneOffQuote.TT_OH_Creditor = creditor.PK;
			var oneOffCarrier1 = oneOffQuote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			oneOffCarrier1.TTC_OH_Carrier = possibleCarrier1.PK;
			oneOffCarrier1.TTC_OH_Creditor = possibleCreditor.PK;
			var oneOffCarrier2 = oneOffQuote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			oneOffCarrier2.TTC_OH_Carrier = possibleCarrier2.PK;

			AutorateAndAssert(expected, oneOffQuote, NewClient, autorateRevenue: false, autorateCosts: true);

			oneOffQuote.ConvertQuoteToQuotedBooking();

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "CAF",
					CostAccountCode = creditor.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					CostAccountCode = creditor.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "WAR",
					CostAccountCode = creditor.OH_Code
				}
			};

			AutorateAndAssert("Filter out ", expected, oneOffQuote, NewClient, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCopy_ConsolidateOOQ()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			var possibleCarrier1 = Factory.NewWithValidTestData<OrgHeader>();
			possibleCarrier1.OH_IsShippingProvider = true;
			var possibleCreditor = Factory.NewWithValidTestData<OrgHeader>();
			possibleCreditor.OH_IsCreditor = true;
			var possibleCarrier2 = Factory.NewWithValidTestData<OrgHeader>();
			possibleCarrier1.OH_IsShippingProvider = true;

			var origin = "AUSYD";
			var destination = "SGSIN";

			var standardCost = Helper.NewCosting(null);
			var standardRateEntry = standardCost.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"CAF", 100m, "AUD");
			standardRateEntry.TI_OH_TransportProvider = carrier.PK;
			var costing1 = Helper.NewCosting(carrier);
			costing1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"FRT", 100m, "AUD");
			var costing2 = Helper.NewCosting(creditor);
			costing2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"WAR", 100m, "AUD");
			var costing3 = Helper.NewCosting(possibleCreditor);
			costing3.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"BAF", 100m, "AUD");
			var costing4 = Helper.NewCosting(possibleCarrier2);
			costing4.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"FSC", 100m, "AUD");

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "CAF",
					CostAccountCode = creditor.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					CostAccountCode = creditor.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "WAR",
					CostAccountCode = creditor.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					CostAccountCode = possibleCreditor.OH_Code // Not included to be moved as charge creditor is not the chosen Carrier or Creditor
				},
				new AssertionCharge
				{
					ChargeCode = "FSC",
					CostAccountCode = possibleCarrier2.OH_Code // Not included to be moved as charge creditor is not the chosen Carrier or Creditor
				}
			};

			var oneOffQuote = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, NewClient, Consignor, NewClient, carrier, origin,
				destination, 1000m, 2m, QuotedBookingState.QuoteOnly);
			oneOffQuote.Quote.CurrentOneOffQuote.TT_OH_Creditor = creditor.PK;
			var oneOffCarrier1 = oneOffQuote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			oneOffCarrier1.TTC_OH_Carrier = possibleCarrier1.PK;
			oneOffCarrier1.TTC_OH_Creditor = possibleCreditor.PK;
			var oneOffCarrier2 = oneOffQuote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			oneOffCarrier2.TTC_OH_Carrier = possibleCarrier2.PK;

			AutorateAndAssert(expected, oneOffQuote, NewClient, autorateRevenue: false, autorateCosts: true);

			Factory.Save();

			var consolController = ZControllerFactory.Create(ControllerIDs.JobConsol);
			var viewQuotedBooking = consolController.Factory.Load<ViewQuotedBooking>(oneOffQuote.Quote.PK);

			using (var quotedBookingForm = new QuotedBookingFormForTest(oneOffQuote))
			{
				quotedBookingForm.ConsolidateToNewConsol();
				quotedBookingForm.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				var sameBooking = consolController.Factory.Load<ForwardingShipment>(viewQuotedBooking.QuotedBooking.Booking.PK);

				// simulate clicking charges in shipment form that will CopyValue
				var shipmentJob = new Job.Loader(sameBooking).TryLoadOrCreate();

				AssertCharges
				(
					message: "Charges after Consolidating",
					expected: new[]
					{
						new AssertionCharge
						{
							ChargeCode = "CAF",
							CostAccountCode = creditor.OH_Code
						},
						new AssertionCharge
						{
							ChargeCode = "FRT",
							CostAccountCode = creditor.OH_Code
						},
						new AssertionCharge
						{
							ChargeCode = "WAR",
							CostAccountCode = creditor.OH_Code
						}
					},
					job: shipmentJob
				);
			}
		}

		public void TestChargeCopy_LinkOOQ()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			var possibleCarrier1 = Factory.NewWithValidTestData<OrgHeader>();
			possibleCarrier1.OH_IsShippingProvider = true;
			var possibleCreditor = Factory.NewWithValidTestData<OrgHeader>();
			possibleCreditor.OH_IsCreditor = true;
			var possibleCarrier2 = Factory.NewWithValidTestData<OrgHeader>();
			possibleCarrier1.OH_IsShippingProvider = true;

			var origin = "AUSYD";
			var destination = "SGSIN";

			var standardCost = Helper.NewCosting(null);
			var standardRateEntry = standardCost.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"CAF", 100m, "AUD");
			standardRateEntry.TI_OH_TransportProvider = carrier.PK;
			var costing1 = Helper.NewCosting(carrier);
			costing1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"FRT", 100m, "AUD");
			var costing2 = Helper.NewCosting(creditor);
			costing2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"WAR", 100m, "AUD");
			var costing3 = Helper.NewCosting(possibleCreditor);
			costing3.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"BAF", 100m, "AUD");
			var costing4 = Helper.NewCosting(possibleCarrier2);
			costing4.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"FSC", 100m, "AUD");

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "CAF",
					CostAccountCode = creditor.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					CostAccountCode = creditor.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "WAR",
					CostAccountCode = creditor.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					CostAccountCode = possibleCreditor.OH_Code // Not included to be moved as charge creditor is not the chosen Carrier or Creditor
				},
				new AssertionCharge
				{
					ChargeCode = "FSC",
					CostAccountCode = possibleCarrier2.OH_Code // Not included to be moved as charge creditor is not the chosen Carrier or Creditor
				}
			};

			var oneOffQuote = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, NewClient, Consignor, NewClient, carrier, origin,
				destination, 1000m, 2m, QuotedBookingState.QuoteOnly);
			oneOffQuote.Quote.CurrentOneOffQuote.TT_OH_Creditor = creditor.PK;
			var oneOffCarrier1 = oneOffQuote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			oneOffCarrier1.TTC_OH_Carrier = possibleCarrier1.PK;
			oneOffCarrier1.TTC_OH_Creditor = possibleCreditor.PK;
			var oneOffCarrier2 = oneOffQuote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			oneOffCarrier2.TTC_OH_Carrier = possibleCarrier2.PK;

			AutorateAndAssert(expected, oneOffQuote, NewClient, autorateRevenue: false, autorateCosts: true);

			Factory.Save();

			var consolController = ZControllerFactory.Create(ControllerIDs.JobConsol);
			var viewQuotedBooking = consolController.Factory.Load<ViewQuotedBooking>(oneOffQuote.Quote.PK);

			using (var quotedBookingForm = new QuotedBookingFormForTest(oneOffQuote))
			{
				quotedBookingForm.ConsolidateToNewConsol();
				quotedBookingForm.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				var sameBooking = consolController.Factory.Load<ForwardingShipment>(viewQuotedBooking.QuotedBooking.Booking.PK);

				// simulate clicking charges in shipment form that will CopyValue
				var shipmentJob = new Job.Loader(sameBooking).TryLoadOrCreate();
				shipmentJob.JH_TH_NKQuoteNumber = oneOffQuote.Quote.TH_QuoteNumber;
				AssertCharges
				(
					message: "Charges after Consolidating",
					expected: new[]
					{
						new AssertionCharge
						{
							ChargeCode = "CAF",
							CostAccountCode = creditor.OH_Code
						},
						new AssertionCharge
						{
							ChargeCode = "FRT",
							CostAccountCode = creditor.OH_Code
						},
						new AssertionCharge
						{
							ChargeCode = "WAR",
							CostAccountCode = creditor.OH_Code
						}
					},
					job: shipmentJob
				);
			}
		}

		#endregion

		#region Same Charge Code Priority

		public void TestSameChargeCodePriority_QB()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var origin = "AUSYD";
			var destination = "SGSIN";

			var costing1 = Helper.NewCosting(carrier);
			costing1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"FRT", 100m, "AUD");
			var costing2 = Helper.NewCosting(creditor);
			costing2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"FRT", 200m, "AUD");

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_LocalCostAmt = 200m
				}
			};

			var quickBooking = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, NewClient, Consignor, NewClient, carrier, origin,
				destination, 1000m, 2m, QuotedBookingState.BookingOnly);
			quickBooking.Booking.JS_OH_Creditor = creditor.PK;

			AutorateAndAssert(expected, quickBooking, NewClient, autorateRevenue: false, autorateCosts: true);
		}

		public void TestSameChargeCodePriority_BWQ()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var origin = "AUSYD";
			var destination = "SGSIN";

			var costing1 = Helper.NewCosting(carrier);
			costing1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"FRT", 100m, "AUD");
			var costing2 = Helper.NewCosting(creditor);
			costing2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"FRT", 200m, "AUD");

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_LocalCostAmt = 200m
				}
			};

			var bookingWithQuote = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, NewClient, Consignor, NewClient, carrier, origin,
				destination, 1000m, 2m, QuotedBookingState.AcceptedBookingWithQuote);
			bookingWithQuote.Quote.CurrentOneOffQuote.TT_OH_Creditor = creditor.PK;
			bookingWithQuote.Booking.JS_OH_Creditor = creditor.PK;

			AutorateAndAssert(expected, bookingWithQuote, NewClient, autorateRevenue: false, autorateCosts: true);
		}

		public void TestSameChargeCodePriority_OOQ()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			var possibleCarrier1 = Factory.NewWithValidTestData<OrgHeader>();
			possibleCarrier1.OH_IsShippingProvider = true;
			var possibleCreditor = Factory.NewWithValidTestData<OrgHeader>();
			possibleCreditor.OH_IsCreditor = true;
			var possibleCarrier2 = Factory.NewWithValidTestData<OrgHeader>();
			possibleCarrier1.OH_IsShippingProvider = true;

			var origin = "AUSYD";
			var destination = "SGSIN";

			var costing1 = Helper.NewCosting(carrier);
			costing1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"FRT", 100m, "AUD");
			var costing2 = Helper.NewCosting(creditor);
			costing2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"FRT", 200m, "AUD");
			var costing3 = Helper.NewCosting(possibleCarrier1);
			costing3.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"FRT", 300m, "AUD");
			var costing4 = Helper.NewCosting(possibleCreditor);
			costing4.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"FRT", 400m, "AUD");
			var costing5 = Helper.NewCosting(possibleCarrier2);
			costing5.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"FRT", 500m, "AUD");

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_LocalCostAmt = 200m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_LocalCostAmt = 400m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_LocalCostAmt = 500m
				}
			};

			var oneOffQuote = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, NewClient, Consignor, NewClient, carrier, origin,
				destination, 1000m, 2m, QuotedBookingState.QuoteOnly);
			oneOffQuote.Quote.CurrentOneOffQuote.TT_OH_Creditor = creditor.PK;
			var oneOffCarrier1 = oneOffQuote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			oneOffCarrier1.TTC_OH_Carrier = possibleCarrier1.PK;
			oneOffCarrier1.TTC_OH_Creditor = possibleCreditor.PK;
			var oneOffCarrier2 = oneOffQuote.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
			oneOffCarrier2.TTC_OH_Carrier = possibleCarrier2.PK;

			AutorateAndAssert(expected, oneOffQuote, NewClient, autorateRevenue: false, autorateCosts: true);
		}

		#endregion

		#region Filtering OOQ by Payment Terms

		void AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions direction, string incoTerm, params AssertionCharge[] expectedCharges)
		{
			string origin = null;
			string destination = null;

			switch (direction)
			{
				case Directions.Export:
					origin = "AUSYD";
					destination = "USLAX";
					break;
				case Directions.Import:
					origin = "HKHKG";
					destination = "AUMEL";
					break;
				default:
					throw new NotSupportedException("Not supported direction");
			}

			var costing = Helper.NewCosting(null);

			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination,
				"FRT", 100m, "AUD");
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, origin, destination,
				"OCART", 200m, "AUD");
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, origin, destination,
				"DCART", 300m, "AUD");

			var oneOffQuote = CreateQuotedBooking(TransportModes.Air, "LSE", incoTerm, NewClient, Consignor, NewClient, null, origin,
				destination, 1000m, 2m, QuotedBookingState.QuoteOnly);

			Factory.Save();

			AutorateAndAssert(expectedCharges, oneOffQuote, NewClient, autorateRevenue: false, autorateCosts: true);
		}

		public void TestAutoRatingOneOffQuote_Export_ByPaymentTerm_CFR() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Export, IncoTerms.CostAndFreight,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "OCART" });

		public void TestAutoRatingOneOffQuote_Export_ByPaymentTerm_CIF() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Export, IncoTerms.CostInsuranceAndFreight,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "OCART" });

		public void TestAutoRatingOneOffQuote_Export_ByPaymentTerm_CIP() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Export, IncoTerms.CarriageAndInsurancePaidTo,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "OCART" }, new AssertionCharge { ChargeCode = "DCART" });

		public void TestAutoRatingOneOffQuote_Export_ByPaymentTerm_CPT() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Export, IncoTerms.CarriagePaidTo,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "OCART" }, new AssertionCharge { ChargeCode = "DCART" });

		public void TestAutoRatingOneOffQuote_Export_ByPaymentTerm_DAP() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Export, IncoTerms.DeliveredAtPlace,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "OCART" }, new AssertionCharge { ChargeCode = "DCART" });

		public void TestAutoRatingOneOffQuote_Export_ByPaymentTerm_DAT() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Export, IncoTerms.DeliveredAtTerminal,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "OCART" }, new AssertionCharge { ChargeCode = "DCART" });

		public void TestAutoRatingOneOffQuote_Export_ByPaymentTerm_DDP() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Export, IncoTerms.DeliveredDutyPaid,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "OCART" }, new AssertionCharge { ChargeCode = "DCART" });

		public void TestAutoRatingOneOffQuote_Export_ByPaymentTerm_DDU() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Export, IncoTerms.DeliveredDutyUnpaid,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "OCART" }, new AssertionCharge { ChargeCode = "DCART" });

		public void TestAutoRatingOneOffQuote_Export_ByPaymentTerm_EXW() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Export, IncoTerms.ExWorks,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "OCART" });

		public void TestAutoRatingOneOffQuote_Export_ByPaymentTerm_FAS() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Export, IncoTerms.FreeAlongsideShip,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "OCART" });

		public void TestAutoRatingOneOffQuote_Export_ByPaymentTerm_FC1() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Export, IncoTerms.FreeCarrierSeller,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "OCART" });

		public void TestAutoRatingOneOffQuote_Export_ByPaymentTerm_FC2() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Export, IncoTerms.FreeCarrierBuyer,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "OCART" });

		public void TestAutoRatingOneOffQuote_Export_ByPaymentTerm_FCA() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Export, IncoTerms.FreeCarrier,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "OCART" });

		public void TestAutoRatingOneOffQuote_Export_ByPaymentTerm_FOB() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Export, IncoTerms.FreeOnBoard,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "OCART" });

		public void TestAutoRatingOneOffQuote_Import_ByPaymentTerm_CFR() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Import, IncoTerms.CostAndFreight,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "DCART" });

		public void TestAutoRatingOneOffQuote_Import_ByPaymentTerm_CIF() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Import, IncoTerms.CostInsuranceAndFreight,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "DCART" });

		public void TestAutoRatingOneOffQuote_Import_ByPaymentTerm_CIP() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Import, IncoTerms.CarriageAndInsurancePaidTo,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "DCART" });

		public void TestAutoRatingOneOffQuote_Import_ByPaymentTerm_CPT() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Import, IncoTerms.CarriagePaidTo,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "DCART" });

		public void TestAutoRatingOneOffQuote_Import_ByPaymentTerm_DAP() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Import, IncoTerms.DeliveredAtPlace,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "DCART" });

		public void TestAutoRatingOneOffQuote_Import_ByPaymentTerm_DAT() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Import, IncoTerms.DeliveredAtTerminal,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "DCART" });

		public void TestAutoRatingOneOffQuote_Import_ByPaymentTerm_DDP() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Import, IncoTerms.DeliveredDutyPaid,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "DCART" });

		public void TestAutoRatingOneOffQuote_Import_ByPaymentTerm_DDU() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Import, IncoTerms.DeliveredDutyUnpaid,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "DCART" });

		public void TestAutoRatingOneOffQuote_Import_ByPaymentTerm_EXW() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Import, IncoTerms.ExWorks,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "OCART" }, new AssertionCharge { ChargeCode = "DCART" });

		public void TestAutoRatingOneOffQuote_Import_ByPaymentTerm_FAS() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Import, IncoTerms.FreeAlongsideShip,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "DCART" });

		public void TestAutoRatingOneOffQuote_Import_ByPaymentTerm_FC1() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Import, IncoTerms.FreeCarrierSeller,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "DCART" });

		public void TestAutoRatingOneOffQuote_Import_ByPaymentTerm_FC2() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Import, IncoTerms.FreeCarrierBuyer,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "OCART" }, new AssertionCharge { ChargeCode = "DCART" });

		public void TestAutoRatingOneOffQuote_Import_ByPaymentTerm_FCA() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Import, IncoTerms.FreeCarrier,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "DCART" });

		public void TestAutoRatingOneOffQuote_Import_ByPaymentTerm_FOB() =>
			AssertAutoRatingOneOffQuote_ByPaymentTerm(Directions.Import, IncoTerms.FreeOnBoard,
				new AssertionCharge { ChargeCode = "FRT" }, new AssertionCharge { ChargeCode = "DCART" });

		#endregion

		#region Convert To Shipment

		public void TestConvertToShipment_WhenAutorateShipment_ReautorateChargeShouldNotCreateNewCharge_SellRatingBehavior()
		{
			var autoratedQuotedBooking = GetAutoratedQuotedBooking(QuotedBookingState.AcceptedBookingWithQuote);

			var helper = new BuildConsolHelper();
			helper.TurnBookingIntoShipment(autoratedQuotedBooking.Booking, null, autoratedQuotedBooking.PK);
			Factory.Save();

			var forwardingShipment = Helper.LoadInNewFactory(autoratedQuotedBooking.Booking);
			forwardingShipment.JS_ActualWeight = 105m;

			// simulate clicking charges in shipment form that will CopyValue
			var shipmentJob = new Job.Loader(forwardingShipment).TryLoadOrCreate();

			AssertCharges
			(
				message: "Precondition: job charges",
				expected: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 1000m, JR_OSSellAmt = 1000m },
					new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 2000m, JR_OSCostAmt = 2000m },
				},
				job: shipmentJob
			);

			var bafCharge = shipmentJob.Charges.Cast<Charge>().Single(charge => charge.ChargeCode.AC_Code == "BAF");
			bafCharge.JR_Calc_CostRatingBehavior = JobChargeLookups.CreateNewCharge;
			var frtCharge = shipmentJob.Charges.Cast<Charge>().Single(charge => charge.ChargeCode.AC_Code == "FRT");
			frtCharge.JR_Calc_SellRatingBehavior = JobChargeLookups.ReAutorateCharge;

			AutorateAndAssert
			(
				message: "Autorating should not create duplicate or extra charges for ReAutorateCharge charge",
				expected: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 1000m, JR_OSSellAmt = 1000m, CostCalculationDescription = "BAF: 100 Kilogram(s) @ AUD 10.00/KG" },
					new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 1050m, JR_OSSellAmt = 1050m, CostCalculationDescription = "BAF: 105 Kilogram(s) @ AUD 10.00/KG" },
					new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 2100m, JR_OSSellAmt = 2100m, RevenueCalculationDescription = "FRT: 105 Kilogram(s) @ AUD 20.00/KG" },
				},
				jobParent: forwardingShipment,
				localClient: Consignor,
				job: shipmentJob
			);
		}

		public void TestConvertToShipment_WhenAutorateShipment_ReautorateChargeShouldNotCreateNewCharge_CostRatingBehavior()
		{
			var autoratedQuotedBooking = GetAutoratedQuotedBooking(QuotedBookingState.AcceptedBookingWithQuote);

			var helper = new BuildConsolHelper();
			helper.TurnBookingIntoShipment(autoratedQuotedBooking.Booking, null, autoratedQuotedBooking.PK);
			Factory.Save();

			var forwardingShipment = Helper.LoadInNewFactory(autoratedQuotedBooking.Booking);
			forwardingShipment.JS_ActualWeight = 105m;

			// simulate clicking charges in shipment form that will CopyValue
			var shipmentJob = new Job.Loader(forwardingShipment).TryLoadOrCreate();

			AssertCharges
			(
				message: "Precondition: job charges",
				expected: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 1000m, JR_OSSellAmt = 1000m },
					new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 2000m, JR_OSCostAmt = 2000m },
				},
				job: shipmentJob
			);

			var bafCharge = shipmentJob.Charges.Cast<Charge>().Single(charge => charge.ChargeCode.AC_Code == "BAF");
			bafCharge.JR_Calc_CostRatingBehavior = JobChargeLookups.ReAutorateCharge;

			AutorateAndAssert
			(
				message: "Autorating should not create duplicate or extra charges for ReAutorateCharge charge",
				expected: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 1050m, JR_OSSellAmt = 1000m, CostCalculationDescription = "BAF: 105 Kilogram(s) @ AUD 10.00/KG" },
					new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 2000m, JR_OSSellAmt = 2000m, RevenueCalculationDescription = "FRT: 100 Kilogram(s) @ AUD 20.00/KG" },
					new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 2100m, JR_OSSellAmt = 2100m, RevenueCalculationDescription = "FRT: 105 Kilogram(s) @ AUD 20.00/KG" },
				},
				jobParent: forwardingShipment,
				localClient: Consignor,
				job: shipmentJob
			);
		}

		public void TestConvertToShipment_RatingBehaviour_PreserveRegistryFalse()
		{
			var autoratedQuotedBooking = GetAutoratedQuotedBooking(QuotedBookingState.AcceptedBookingWithQuote);
			var chargeCode1 = Helper.ChargeCodes.New("RB1", "Test Charge Code 1", AgencyCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			var chargeCode2 = Helper.ChargeCodes.New("RB2", "Test Charge Code 2", AgencyCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			var chargeCode3 = Helper.ChargeCodes.New("RB3", "Test Charge Code 3", AgencyCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var quotedBookingJob = new Job.Loader(autoratedQuotedBooking).TryLoadOrCreate();

			AssertEquals(JobChargeLookups.ReAutorateCharge, quotedBookingJob.Charges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == "FRT").JR_Calc_CostRatingBehavior);
			AssertEquals(JobChargeLookups.ReAutorateCharge, quotedBookingJob.Charges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == "FRT").JR_Calc_SellRatingBehavior);

			var newCharge1 = quotedBookingJob.Charges.AddNew();
			newCharge1.JR_AC = chargeCode1.PK;
			newCharge1.JR_Desc = "RB1 For Quote";
			newCharge1.JR_OSSellAmt = 12m;
			newCharge1.JR_Calc_CostRatingBehavior = JobChargeLookups.CreateNewCharge;
			newCharge1.JR_Calc_SellRatingBehavior = JobChargeLookups.ReAutorateCharge;

			var newCharge2 = quotedBookingJob.Charges.AddNew();
			newCharge2.JR_AC = chargeCode2.PK;
			newCharge2.JR_Desc = "RB2 For Quote";
			newCharge2.JR_OSSellAmt = 34;
			newCharge2.JR_Calc_CostRatingBehavior = JobChargeLookups.ReAutorateCharge;
			newCharge2.JR_Calc_SellRatingBehavior = JobChargeLookups.CreateNewCharge;

			var newCharge3 = quotedBookingJob.Charges.AddNew();
			newCharge3.JR_AC = chargeCode3.PK;
			newCharge3.JR_Desc = "RB3 For Quote";
			newCharge3.JR_OSSellAmt = 56;
			newCharge3.JR_Calc_CostRatingBehavior = JobChargeLookups.CreateNewCharge;
			newCharge3.JR_Calc_SellRatingBehavior = JobChargeLookups.CreateNewCharge;
			AssertEquals(5, quotedBookingJob.Charges.Count);

			using (DataRegistryRating.Instance.PreserveQuoteRevenueRatingBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var helper = new BuildConsolHelper();
				helper.TurnBookingIntoShipment(autoratedQuotedBooking.Booking, null, autoratedQuotedBooking.PK);
				Factory.Save();

				var forwardingShipment = Helper.LoadInNewFactory(autoratedQuotedBooking.Booking);

				var shipmentJob = new Job.Loader(forwardingShipment).TryLoadOrCreate();

				AssertEquals("Cost Rating Behavior Preserved", JobChargeLookups.ReAutorateCharge, shipmentJob.Charges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == "FRT").JR_Calc_CostRatingBehavior);
				AssertEquals("Sell Rating Behavior is New", JobChargeLookups.CreateNewCharge, shipmentJob.Charges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == "FRT").JR_Calc_SellRatingBehavior);

				AssertEquals("Cost Rating Behavior Preserved", JobChargeLookups.CreateNewCharge, shipmentJob.Charges.Cast<Charge>().Single(x => x.ChargeCode.PK == chargeCode1.PK).JR_Calc_CostRatingBehavior);
				AssertEquals("Sell Rating Behavior is New", JobChargeLookups.CreateNewCharge, shipmentJob.Charges.Cast<Charge>().Single(x => x.ChargeCode.PK == chargeCode1.PK).JR_Calc_SellRatingBehavior);

				AssertEquals("Cost Rating Behavior Preserved", JobChargeLookups.ReAutorateCharge, shipmentJob.Charges.Cast<Charge>().Single(x => x.ChargeCode.PK == chargeCode2.PK).JR_Calc_CostRatingBehavior);
				AssertEquals("Sell Rating Behavior is New", JobChargeLookups.CreateNewCharge, shipmentJob.Charges.Cast<Charge>().Single(x => x.ChargeCode.PK == chargeCode2.PK).JR_Calc_SellRatingBehavior);

				AssertEquals("Cost Rating Behavior Preserved", JobChargeLookups.CreateNewCharge, shipmentJob.Charges.Cast<Charge>().Single(x => x.ChargeCode.PK == chargeCode3.PK).JR_Calc_CostRatingBehavior);
				AssertEquals("Sell Rating Behavior Preserved", JobChargeLookups.CreateNewCharge, shipmentJob.Charges.Cast<Charge>().Single(x => x.ChargeCode.PK == chargeCode3.PK).JR_Calc_SellRatingBehavior);
			}
		}

		public void TestConvertToShipment_RatingBehaviour_PreserveRegistryTrue()
		{
			var autoratedQuotedBooking = GetAutoratedQuotedBooking(QuotedBookingState.AcceptedBookingWithQuote);
			var chargeCode1 = Helper.ChargeCodes.New("RB1", "Test Charge Code 1", AgencyCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			var chargeCode2 = Helper.ChargeCodes.New("RB2", "Test Charge Code 2", AgencyCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			var chargeCode3 = Helper.ChargeCodes.New("RB3", "Test Charge Code 3", AgencyCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var quotedBookingJob = new Job.Loader(autoratedQuotedBooking).TryLoadOrCreate();

			AssertEquals(JobChargeLookups.ReAutorateCharge, quotedBookingJob.Charges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == "FRT").JR_Calc_CostRatingBehavior);
			AssertEquals(JobChargeLookups.ReAutorateCharge, quotedBookingJob.Charges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == "FRT").JR_Calc_SellRatingBehavior);

			var newCharge1 = quotedBookingJob.Charges.AddNew();
			newCharge1.JR_AC = chargeCode1.PK;
			newCharge1.JR_Desc = "RB1 For Quote";
			newCharge1.JR_OSSellAmt = 12m;
			newCharge1.JR_Calc_CostRatingBehavior = JobChargeLookups.CreateNewCharge;
			newCharge1.JR_Calc_SellRatingBehavior = JobChargeLookups.ReAutorateCharge;

			var newCharge2 = quotedBookingJob.Charges.AddNew();
			newCharge2.JR_AC = chargeCode2.PK;
			newCharge2.JR_Desc = "RB2 For Quote";
			newCharge2.JR_OSSellAmt = 34m;
			newCharge2.JR_Calc_CostRatingBehavior = JobChargeLookups.ReAutorateCharge;
			newCharge2.JR_Calc_SellRatingBehavior = JobChargeLookups.CreateNewCharge;

			var newCharge3 = quotedBookingJob.Charges.AddNew();
			newCharge3.JR_AC = chargeCode3.PK;
			newCharge3.JR_Desc = "RB3 For Quote";
			newCharge3.JR_OSSellAmt = 56m;
			newCharge3.JR_Calc_SellRatingBehavior = JobChargeLookups.CreateNewCharge;
			newCharge3.JR_Calc_CostRatingBehavior = JobChargeLookups.CreateNewCharge;

			AssertEquals(5, quotedBookingJob.Charges.Count);

			using (DataRegistryRating.Instance.PreserveQuoteRevenueRatingBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var helper = new BuildConsolHelper();
				helper.TurnBookingIntoShipment(autoratedQuotedBooking.Booking, null, autoratedQuotedBooking.PK);
				Factory.Save();

				var forwardingShipment = Helper.LoadInNewFactory(autoratedQuotedBooking.Booking);

				var shipmentJob = new Job.Loader(forwardingShipment).TryLoadOrCreate();

				AssertEquals("Cost Rating Behavior Preserved", JobChargeLookups.ReAutorateCharge, shipmentJob.Charges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == "FRT").JR_Calc_CostRatingBehavior);
				AssertEquals("Sell Rating Behavior Preserved", JobChargeLookups.ReAutorateCharge, shipmentJob.Charges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == "FRT").JR_Calc_SellRatingBehavior);

				AssertEquals("Cost Rating Behavior Preserved", JobChargeLookups.CreateNewCharge, shipmentJob.Charges.Cast<Charge>().Single(x => x.ChargeCode.PK == chargeCode1.PK).JR_Calc_CostRatingBehavior);
				AssertEquals("Sell Rating Behavior Preserved", JobChargeLookups.ReAutorateCharge, shipmentJob.Charges.Cast<Charge>().Single(x => x.ChargeCode.PK == chargeCode1.PK).JR_Calc_SellRatingBehavior);

				AssertEquals("Cost Rating Behavior Preserved", JobChargeLookups.ReAutorateCharge, shipmentJob.Charges.Cast<Charge>().Single(x => x.ChargeCode.PK == chargeCode2.PK).JR_Calc_CostRatingBehavior);
				AssertEquals("Sell Rating Behavior Preserved", JobChargeLookups.CreateNewCharge, shipmentJob.Charges.Cast<Charge>().Single(x => x.ChargeCode.PK == chargeCode2.PK).JR_Calc_SellRatingBehavior);

				AssertEquals("Cost Rating Behavior Preserved", JobChargeLookups.CreateNewCharge, shipmentJob.Charges.Cast<Charge>().Single(x => x.ChargeCode.PK == chargeCode3.PK).JR_Calc_CostRatingBehavior);
				AssertEquals("Sell Rating Behavior Preserved", JobChargeLookups.CreateNewCharge, shipmentJob.Charges.Cast<Charge>().Single(x => x.ChargeCode.PK == chargeCode3.PK).JR_Calc_SellRatingBehavior);
			}
		}

		public void TestConvertToShipment_CanReautorate_NoDuplicateChargeCreated()
		{
			var autoratedQuotedBooking = GetAutoratedQuotedBooking(QuotedBookingState.BookingOnly);

			var helper = new BuildConsolHelper();
			helper.TurnBookingIntoShipment(autoratedQuotedBooking.Booking, null, autoratedQuotedBooking.PK);
			Factory.Save();

			var forwardingShipment = Helper.LoadInNewFactory(autoratedQuotedBooking.Booking);
			forwardingShipment.JS_ActualWeight = 105m;

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 2100m,
					RevenueCalculationDescription = "FRT: 105 Kilogram(s) @ AUD 20.00/KG",
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSCostAmt = 1050m,
					CostCalculationDescription = "BAF: 105 Kilogram(s) @ AUD 10.00/KG"
				},
			};

			var shipmentJob = new Job.Loader(forwardingShipment).TryLoadOrCreate();
			AutorateAndAssert("Autorating should not create duplicate or extra charges", expectedCharges, forwardingShipment, Consignor, job: shipmentJob);
		}

		public void TestConvertToShipment_ImportAndExportBrokerIsSettingFromQuotedBooking()
		{
			var linkBroker = Factory.NewWithValidTestData<OrgHeader>();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "USLGB";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var link = consignor.BuyerLinks.AddNew();
			link.OL_OH_Buyer = consignee.PK;
			link.OL_OH_ImportBroker = linkBroker.PK;

			var linkDetails = link.OrgSupBuyLinkTrnModes.AddNew();
			linkDetails.PF_TransportMode = TransportModes.Air;
			linkDetails.PF_ContainerMode = ContainerModes.Loose;

			Factory.Save();

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			quotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			quotedBooking.Mode = RateMode.LSE;

			Factory.Save();

			var helper = new BuildConsolHelper();
			helper.TurnBookingIntoShipment(quotedBooking.Booking, null, quotedBooking.PK);

			var forwardingShipment = Helper.LoadInNewFactory(quotedBooking.Booking);
			AssertEquals("Import broker not carrying over from quoted booking when converting to shipment", linkBroker.PK, forwardingShipment.JS_OH_ImportBroker);
		}

		#endregion

		public void TestOOQ_WithTrigger_EDT_FLD_Setting_Job_RepSales()
		{
			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_Code = "PAI";

			var oneOffQuote = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, Consignor, Consignor, Consignee, null, "AUSYD", "USLAX", 0m, 0m, QuotedBookingState.QuoteOnly);
			oneOffQuote.StartDate = ZDate.Today;
			oneOffQuote.EndDate = ZDate.Today.AddDays(5);
			oneOffQuote.TryLoadOrCreateJob();

			var trigger = oneOffQuote.WorkflowItems.Triggers.AddNew();
			trigger.P9_Type = Workflow.WorkflowTriggerType;
			trigger.P9_Description = "Edited a record";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.EditedARecordCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<Job.JH_GS_NKRepSales>";
			action.PQ_FieldValue = "PAI";

			Factory.Save();

			var log = MasterFilesTestHelper.RunLogWalker();

			AssertNotContains("Noting triggered yet", "Action types in this batch: FLD", log);

			var expectedLog = @"[WorkflowEventTrigger] [Default] Action types in this batch: FLD
[WorkflowEventTrigger] [Default] Firing action: FLD
[WorkflowEventTrigger] [Default] FLD setting [Job.JH_GS_NKRepSales] with [PAI] succeeded on 1 properties. Unchanged on 0. Failed on 0
[WorkflowEventTrigger] [Default] Success logs:
Target Object: 'Invoicing Job', Source: , Target: PAI
[WorkflowEventTrigger] [Default] Action completed: FLD";

			oneOffQuote.Destination = "HKHKG";
			Factory.Save();

			log = MasterFilesTestHelper.RunLogWalker();

			AssertContains("Set Field action is triggered", expectedLog, log);
		}

		class DummyPrintTask : PrintTask
		{
			public override DocumentEngineCore.DocumentSupport.DeliveryInstructionDestination RunWithPartialInstructions(AllowedDeliveryOptions deliveryOptions, DeliveryInstructions deliveryInstructions, ISecurityCheckpoint modifyDocumentCheckPoint)
			{
				return DocumentEngineCore.DocumentSupport.DeliveryInstructionDestination.Print;
			}
		}
	}
}
