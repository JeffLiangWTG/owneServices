using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.Business.Test;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Testing;
using Enterprise.RatingTests.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using WiseRates.Api.Client;
using WiseRates.Api.Model;
using WiseRates.Constants;
using Application = System.Windows.Forms.Application;
using RefServiceLevel = WiseRates.Api.Model.RefServiceLevel;
using Schedule = WiseRates.Api.Model.Schedule;

namespace Enterprise.Rating.GUI.Test
{
	[TestedType(typeof(RateChooserForm))]
	public class RateChooserFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var carrier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "SCACCARRIER");
			if (carrier == null)
			{
				carrier = ChooserHelper.CreateCarrierOrg("SCAC");
				Factory.Save();
			}

			var consol = ChooserHelper.CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			ChooserHelper.AddContainer(consol, "20GP", "GEN", Core.Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRatingInfo = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);

			var model = new RateChooserModel(criteria, context);
			var viewModel = new RateChooserViewModel(model);

			var form = new RateChooserForm(viewModel);
			MissingResourceStringChecker.ExcludeFromTest(form.NotificationLabel);
			return form;
		}

		protected RateChooserTestHelper ChooserHelper
		{
			get { return chooserHelper ?? (chooserHelper = new RateChooserTestHelper(Factory)); }
		}
		RateChooserTestHelper chooserHelper;
	}

	public class RateChooserFormActionTest : RatingTestCase
	{
		public void TestChargeCodesStringAndTotalChargesPriceString()
		{
			var bolCharge1 = Helper.ChargeCodes.NewConsolChargeCode("BO1", "Bill of Lading 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight, companyPK: GlbCompany.CurrentCompany.PK);
			var bolCharge2 = Helper.ChargeCodes.NewConsolChargeCode("BO2", "Bill of Lading 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight, companyPK: GlbCompany.CurrentCompany.PK);
			var orgCharge1 = Helper.ChargeCodes.NewConsolChargeCode("OC1", "Origin Charge 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin, companyPK: GlbCompany.CurrentCompany.PK);
			var orgCharge2 = Helper.ChargeCodes.NewConsolChargeCode("OC2", "Origin Charge 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin, companyPK: GlbCompany.CurrentCompany.PK);
			var destCharge1 = Helper.ChargeCodes.NewConsolChargeCode("DC1", "Destination Charge 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination, companyPK: GlbCompany.CurrentCompany.PK);
			var destCharge2 = Helper.ChargeCodes.NewConsolChargeCode("DC2", "Destination Charge 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination, companyPK: GlbCompany.CurrentCompany.PK);

			var carrier = ChooserHelper.CreateCarrierOrg("SCA1");
			Factory.Save();

			var consol = ChooserHelper.CreateConsol();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			ChooserHelper.AddContainer(consol, "20GP", "GEN", "FCL");

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var apiCosting = ChooserHelper.CreateApiRate("20GP", carrier, "");
			apiCosting.Origin = "USLAX";
			apiCosting.Destination = "HKHKG";
			RateChooserTestHelper.AddFlatCharge(apiCosting, "OC1", 30).WithCustomCategory(WRConstants.ChargeCustomCategory.BOL);
			RateChooserTestHelper.AddFlatCharge(apiCosting, "OC2", 40).WithCustomCategory(WRConstants.ChargeCustomCategory.BOL).OfType(ChargeType.Optional);
			RateChooserTestHelper.AddFlatCharge(apiCosting, "BO1", 10).WithCustomCategory(WRConstants.ChargeCustomCategory.BOL);
			RateChooserTestHelper.AddFlatCharge(apiCosting, "BO2", 20).WithCustomCategory(WRConstants.ChargeCustomCategory.BOL).OfType(ChargeType.Optional);
			RateChooserTestHelper.AddFlatCharge(apiCosting, "DC1", 50).WithCustomCategory(WRConstants.ChargeCustomCategory.BOL);
			RateChooserTestHelper.AddFlatCharge(apiCosting, "DC2", 60).WithCustomCategory(WRConstants.ChargeCustomCategory.BOL).OfType(ChargeType.Optional);

			model.AddWiseRatesForTest(ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting }));

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection
				.Cast<ChooserRateEntry>()
				.First(r => r.WiseRateEntry != null);

			var rateChooserViewModel = new RateChooserViewModelForTest(model);
			using (var form = new RateChooserForm(rateChooserViewModel))
			{
				form.ShouldRunSearchOnShowingForm = false;
				form.Show();

				rateChooserViewModel.RefreshRates();

				rateChooserViewModel.ContainerTabs
					.SelectMany(chooserContainerCommodityViewModel => chooserContainerCommodityViewModel.Rates.SelectMany(r => r.BOLCharges.Charges))
				.ForEach(charge => charge.IsActive = true);

				var chooserRateRow = rateChooserViewModel.ContainerTabs.First().Rates.Single();
				chooserRateRow.ChargeDetailVisibility = true; // expand the "More Details" panel

				CombineAssertions("WHEN ticking charge THEN the ChargeCodesString and TotalChargesPricesString should be updated", () =>
				{
					AssertTextBlockText(form, "lblORGChargeCodesString", expectedValue: "OC1, OC2");
					AssertTextBlockText(form, "lblTotalORGChargesPriceString", expectedValue: "AUD $70.00");
					AssertTextBlockText(form, "lblFRTChargeCodesString", expectedValue: "BO1, BO2");
					AssertTextBlockText(form, "lblTotalFRTChargesPriceString", expectedValue: "AUD $30.00");
					AssertTextBlockText(form, "lblDSTChargeCodesString", expectedValue: "DC1, DC2");
					AssertTextBlockText(form, "lblTotalDSTChargesPriceString", expectedValue: "AUD $110.00");
				});
			}
		}

		internal class RateChooserViewModelForTest : RateChooserViewModel
		{
			public RateChooserViewModelForTest(RateChooserModel model)
				: base(model)
			{
			}

			public override void SendRatesRequest(IRateSelectorFilterValueProvider filter, RatesQuery ratesQuery, bool isValidForRatesService = true, IEnumerable<string> universalCarrierLevels = null)
			{
				RefreshRates();
			}
		}

		static void AssertTextBlockText(RateChooserForm form, string elementName, string expectedValue)
		{
			var element = form.Controls.Find(elementName, true)[0];
			AssertEquals($"The text of the element '{elementName}' does not match the expected value.", expectedValue, element.Text);
		}

		#region WPF control visibilities and sizing

		static void AssertElementVisibility(bool expectedVisibility, string elementName, RateChooserViewModel viewModel)
		{
			using (var form = new RateChooserForm(viewModel))
			{
				// use setup rate in viewModel
				form.ShouldRunSearchOnShowingForm = false;
				form.Show();

				viewModel.RefreshRates();

				var chooserRateRow = viewModel.ContainerTabs.First().Rates.Single();
				chooserRateRow.ChargeDetailVisibility = true; // expand the "More Details" panel

				var element = form.Controls.Find(elementName, true)[0];
				AssertEquals($"Element visibility for '{elementName}' did not match the expected value.", expectedVisibility, element.Visible);
			}
		}

		public void TestExpiryDateLabelVisibility()
		{
			const string elementName = "lblExpiryDateLabel";

			var viewModel = SetupModelAndViewModelWithCWRate(scac: "SCCW");
			AssertElementVisibility(true, elementName, viewModel);

			var (_, apiViewModel, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCWR");
			AssertElementVisibility(true, elementName, apiViewModel);

			var (_, spotViewModel, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCSP", hasTransportLegs: true); // Spot Rate
			AssertElementVisibility(false, elementName, spotViewModel);
		}

		public void TestExpiryDateVisibility()
		{
			const string elementName = "lblExpiryDate";

			var cwViewModel = SetupModelAndViewModelWithCWRate(scac: "SCCW");
			AssertElementVisibility(true, elementName, cwViewModel);

			var (_, apiViewModel, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCWR");
			AssertElementVisibility(true, elementName, apiViewModel);

			var (_, spotViewModel, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCSP", hasTransportLegs: true); // Spot Rate
			AssertElementVisibility(false, elementName, spotViewModel);
		}

		public void TestEffectiveDateLabelVisibility()
		{
			const string elementName = "lblEffectiveDateLabel";

			var cwViewModel = SetupModelAndViewModelWithCWRate(scac: "SCCW");
			AssertElementVisibility(true, elementName, cwViewModel);

			var (_, apiViewModel, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCWR");
			AssertElementVisibility(true, elementName, apiViewModel);

			var (_, spotViewModel, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCSP", hasTransportLegs: true); // Spot Rate
			AssertElementVisibility(false, elementName, spotViewModel);
		}

		public void TestEffectiveDateVisibility()
		{
			const string elementName = "lblEffectiveDate";

			var cwViewModel = SetupModelAndViewModelWithCWRate(scac: "SCCW");
			AssertElementVisibility(true, elementName, cwViewModel);

			var (_, apiViewModel, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCWR");
			AssertElementVisibility(true, elementName, apiViewModel);

			var (_, spotViewModel, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCSP", hasTransportLegs: true); // Spot Rate
			AssertElementVisibility(false, elementName, spotViewModel);
		}

		public void TestBookingInfoPanelVisibility()
		{
			const string elementName = "pnlRoute";

			var cwViewModel = SetupModelAndViewModelWithCWRate(scac: "SCCW");
			AssertElementVisibility(false, elementName, cwViewModel);

			var (_, apiViewModel, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCWR");
			AssertElementVisibility(false, elementName, apiViewModel);

			var (_, spotViewModel, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCSP", hasTransportLegs: true); // Spot Rate
			AssertElementVisibility(true, elementName, spotViewModel);
		}

		public void TestBookingInfoViewPanelVisibility()
		{
			const string elementName = "pnlThirdRow";

			var cwViewModel = SetupModelAndViewModelWithCWRate(scac: "SCCW");
			AssertElementVisibility(false, elementName, cwViewModel);

			var (_, apiViewModel, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCWR"); // BookingInfo is null or empty
			AssertElementVisibility(false, elementName, apiViewModel);

			var (_, apiViewModelWithPenalties, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCWT", hasPenalties: true); // It's not Spot Rate but has Penalties

			AssertElementVisibility(true, elementName, apiViewModelWithPenalties);

			var (_, spotViewModel, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCSP", hasTransportLegs: true); // Spot Rate

			AssertElementVisibility(true, elementName, spotViewModel);
		}

		public void TestRouteViewPanelVisibility()
		{
			const string elementName = "ucRoutes";

			var (_, apiViewModelWithPenalties, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCWT", hasPenalties: true); // It's not Spot Rate but has Penalties
			AssertElementVisibility(false, elementName, apiViewModelWithPenalties);

			var (_, spotViewModel, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCSP", hasTransportLegs: true); // Spot Rate
			AssertElementVisibility(true, elementName, spotViewModel);
		}

		public void TestPenaltyViewPanelVisibility()
		{
			const string elementName = "ucPenalties";

			var (_, apiViewModelWithPenalties, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCWT", hasPenalties: true); // It's not Spot Rate but has Penalties
			AssertElementVisibility(true, elementName, apiViewModelWithPenalties);

			var (_, spotViewModel, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCSP", hasTransportLegs: true); // Spot Rate without Penalties
			AssertElementVisibility(false, elementName, spotViewModel);

			var (_, spotViewModelWithPenalies, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCSY", hasTransportLegs: true, hasPenalties: true); // Spot Rate with Penalties
			AssertElementVisibility(true, elementName, spotViewModelWithPenalies);
		}

		public void TestBookingTermViewPanelVisibility()
		{
			const string elementName = "ucBookingTerms";

			var (_, apiViewModelWithPenalties, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCWT", hasBookingTerms: true); // It's not Spot Rate but has Booking Terms
			AssertElementVisibility(false, elementName, apiViewModelWithPenalties);

			var (_, spotViewModel, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCSP", hasTransportLegs: true); // Spot Rate without Booking Terms
			AssertElementVisibility(false, elementName, spotViewModel);

			var (_, spotViewModelWithPenalies, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCSY", hasTransportLegs: true, hasBookingTerms: true); // Spot Rate with Booking Terms
			AssertElementVisibility(true, elementName, spotViewModelWithPenalies);
		}

		public void TestRoutingOrTransitTimeLabelVisibility()
		{
			const string elementName = "lblRoutingOrTransitTime";

			var cwViewModel = SetupModelAndViewModelWithCWRate(scac: "SCCW");
			AssertElementVisibility(false, elementName, cwViewModel);

			var (_, apiViewModel, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCWR");
			AssertElementVisibility(true, elementName, apiViewModel);

			var (_, spotViewModel, _, _) = SetupModelAndViewModelWithApiRate(scac: "SCSP", hasTransportLegs: true); // Spot Rate
			AssertElementVisibility(false, elementName, spotViewModel);
		}

		public void TestRoutingOrTransitTimeLabelIsNotTruncated()
		{
			CombineAssertions("Routing string should not be truncated", () =>
			{
				AssertRoutingStringDoesNotTruncate("AAAA", "VNSGN > USHOU");
				AssertRoutingStringDoesNotTruncate("AAAB", "VNSGN > VNVUT > USHOU");
				AssertRoutingStringDoesNotTruncate("AAAC", "VNSGN > VNVUT > USLGB > USHOU");
				AssertRoutingStringDoesNotTruncate("AAAD", "VNSGN > VNVUT > USLGB > USHOU > AUSYD");
			});

			void AssertRoutingStringDoesNotTruncate(string scac, string routingString)
			{
				var providerCustomFields = new[]
				{
					new CustomField() { Code = Rate.CustomFields.Common.Routing, Value = routingString }
				};
				var (_, apiViewModel, _, _) = SetupModelAndViewModelWithApiRate(scac: scac, providerCustomFields: providerCustomFields);

				using (var form = new RateChooserForm(apiViewModel))
				{
					// use setup rate in viewModel
					form.ShouldRunSearchOnShowingForm = false;
					form.Show();

					apiViewModel.RefreshRates();

					Application.DoEvents();

					var control = (ZLabel)form.Controls.Find("lblRoutingOrTransitTime", searchAllChildren: true)[0];
					var textSize = TextRenderer.MeasureText(routingString, control.Font);

					AssertEquals($"Precondition: targeting the routing string {routingString}", routingString, control.Text);
					AssertGreaterThanOrEqualTo($"Label should be wide enough to fully contain routing string {routingString}", control.Size.Width, textSize.Width);
					AssertGreaterThanOrEqualTo($"Label should be tall enough to fully contain routing string {routingString}", control.Size.Height, textSize.Height);

					// Uncomment below for visual inspection

					//while (form.Visible)
					//{
					//	Application.DoEvents();
					//	Thread.Sleep(100);
					//}
				}
			}
		}

		public void TestChargeDetailPanelsAreSizedCorrectlyForDpiSettings()
		{
			var viewModel = SetupModelAndViewModelWithCWRate(scac: "SCCW");

			CombineAssertions("When DPI doubles, control size should also double", () =>
			{
				AssertSize("chargesCW1BillOfLadingCharges", dpi: 96, expectedWidth: 350, expectedHeight: 80);
				AssertSize("chargesCW1BillOfLadingCharges", dpi: 192, expectedWidth: 700, expectedHeight: 160);

				AssertSize("chargesCW1FreightCharges", dpi: 96, expectedWidth: 350, expectedHeight: 110);
				AssertSize("chargesCW1FreightCharges", dpi: 192, expectedWidth: 700, expectedHeight: 220);

				AssertSize("chargesBOLCharges", dpi: 96, expectedWidth: 350, expectedHeight: 110);
				AssertSize("chargesBOLCharges", dpi: 192, expectedWidth: 700, expectedHeight: 220);

				AssertSize("lcOceanCharges", dpi: 96, expectedWidth: 350, expectedHeight: 110);
				AssertSize("lcOceanCharges", dpi: 192, expectedWidth: 700, expectedHeight: 220);

				AssertSize("lcInlandCharges", dpi: 96, expectedWidth: 350, expectedHeight: 110);
				AssertSize("lcInlandCharges", dpi: 192, expectedWidth: 700, expectedHeight: 220);

				AssertSize("lcOutlandCharges", dpi: 96, expectedWidth: 350, expectedHeight: 110);
				AssertSize("lcOutlandCharges", dpi: 192, expectedWidth: 700, expectedHeight: 220);
			});

			void AssertSize(string controlName, int dpi, int expectedWidth, int expectedHeight)
			{
				using (ControlDpiScalingHelper.OverrideDPI_ForTesting(dpiX: dpi, dpiY: dpi))
				using (var form = new RateChooserForm(viewModel))
				{
					form.ShouldRunSearchOnShowingForm = false;
					viewModel.RefreshRates();

					var rateDetailsPanel = form.Controls.Find("pnlRateDetails", searchAllChildren: true)[0] as ZCollapsiblePanel;
					rateDetailsPanel.IsCollapsed = false;

					form.Show();
					Application.DoEvents();

					var foundControl = form.Controls.Find(controlName, searchAllChildren: true)[0];
					AssertEquals($"Control {controlName} width should be sized relative to {dpi} DPI", expectedWidth, foundControl.Size.Width);
					AssertEquals($"Control {controlName} height should be sized relative to {dpi} DPI", expectedHeight, foundControl.Size.Height);
				}
			}
		}

		#endregion

		public void TestApply_GivenUnmappedCarrierThenShouldShowOrganizationMappingPopup()
		{
			var carrier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "SCACCARRIER")
				?? ChooserHelper.CreateCarrierOrg("SCAC");

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "SCA?";
			Factory.Save();

			var consol = ChooserHelper.CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			ChooserHelper.AddContainer(consol, "20GP", "GEN", Core.Constants.ContainerModes.FCL);

			var (model, logger) = GetModel(Factory, consol.RatingAdapter);

			var helper = new RateChooserTestHelper(Factory);
			var apiCosting = ChooserHelper.CreateApiRate("20GP", carrier, "CO1");
			apiCosting.ServiceLevel = "STD";
			apiCosting.Carrier = "CARRIER?";
			var charge = RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT", 100);

			model.AddWiseRatesForTest
			(
				new RatesSearchResponse
				{
					Rates = new[] { apiCosting },
					Carriers = new[] { new RefCarrier { Code = "CARRIER?", SCACCode = "SCA?", Name = "Invalid Carrier" }, },
					ChargeCodes = new[] { new RefChargeCode { Code = "FRT", Group = "FRT", Description = "Invalid Charge" } },
				}
			);

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			CombineAssertions("GIVEN empty serviceProvider WHEN OnApplyButtonClick THEN should show carrier-search form mapping", () =>
			{
				var viewModel = new RateChooserViewModel(model);
				using (var form = new RateChooserForm(viewModel))
				{
					var toolStrip = (ZToolStrip)form.Controls.Find("toolStrip", true)[0];
					var applyButton = toolStrip.Items.Cast<ZToolStripButton>().FirstOrDefault(x => x.Text == "Apply");
					applyButton.Enabled = true;

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // The following Carrier '...' could NOT be found.  Would you like to map the missing Carrier?
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // You do not have the following security right to edit ... Do you wish to have a user with this right enter their credentials?

					var nonSupportUser = Factory.New<GlbStaff>();
					using (Env.SetTemporaryUserContext(new UserContext(nonSupportUser, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
					{
						applyButton.PerformClick();
					}
					AssertEquals("GIVEN no permission THEN should show security loginform", "LoginForm", ZFormModaliser.LastFormShownDialogForTest.Name);

					ZFormModaliser.LastFormShownDialogForTest.Dispose();
					ZFormModaliser.LastFormShownDialogForTest = null;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // The following Carrier '...' could NOT be found.  Would you like to map the missing Carrier?
					applyButton.PerformClick();
					AssertEquals("Carrier search form should shown", "Organization", ((ZForm)ZFormModaliser.LastFormShownDialogForTest).FormCaption);
				}
			});
		}

		public void TestPromptUserToMapServiceLevel_EmptyServiceProvider_ShouldNotThrowException()
		{
			var (_, viewModel, _, logger) = SetupModelAndViewModelWithApiRate("CH?", "SCAC?");

			CombineAssertions("GIVEN empty serviceProvider WHEN OnApplyButtonClick > PromptUserToMapServiceLevel THEN should not throw NullReferenceException and show warning", () =>
			{
				AssertNoExceptionThrown("No exception", () =>
				{
					using (var form = new RateChooserForm(viewModel))
					{
						ApplyButtonPerformClick(form);
					}
				});

				AssertContainsExactElementsInAnyOrder
				(
					"Warnings",
					new[]
					{
						"No active Carrier is assigned with SCAC Code 'SCAC?'", // RateChooserModel > AddWiseRates > Converter.GetCarrierOrgHeader
						"No active Carrier is assigned with SCAC Code 'SCAC?'"  // applyButton.PerformClick() > unmappedCharges > Model.UpdateMappings
					},
					logger.Warnings
				);
			});
		}

		public void TestApply_WhenUnmappedServiceLevel_ShouldShowServiceLevelMappingDialog()
		{
			var (model, viewModel, apiCosting, _) = SetupModelAndViewModelWithApiRate();
			// make service level unmapped
			apiCosting.ServiceLevel = "XXX";

			using (var form = new RateChooserForm(viewModel))
			{
				var toolStrip = (ZToolStrip)form.Controls.Find("toolStrip", true)[0];
				var applyButton = toolStrip.Items.Cast<ZToolStripButton>().Single(x => x.Text == "Apply");
				applyButton.Enabled = true;

				// first apply, say No map service level
				model.UpdateMappings();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				applyButton.PerformClick();
				var expectedMessage = "The Universal Service Level 'XXX' has NOT been assigned to Carrier 'SCAACARRIER'. Would you like to complete the assignment and apply rates to the job?";
				var previousMessages = UnitTestUserNotification.Instance.PreviousMessages.Select(m => m.Text);
				AssertCollectionContains(expectedMessage, previousMessages);

				// second apply, say Yes, organization form shows
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				applyButton.PerformClick();
				AssertType(typeof(OrgHeader), ZFormModaliser.LastIBusinessShownOnDialogForTest);
				AssertType(typeof(ZOrganisationsForm), ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestApply_WhenServiceLevelIsSTD_ShouldNotShowServiceLevelMappingDialog()
		{
			var (model, viewModel, apiCosting, _) = SetupModelAndViewModelWithApiRate();
			// default service level, no mapping required
			apiCosting.ServiceLevel = OrgCarrierServiceLevel.StandardCode;

			using (var form = new RateChooserForm(viewModel))
			{
				var toolStrip = (ZToolStrip)form.Controls.Find("toolStrip", true)[0];
				var applyButton = toolStrip.Items.Cast<ZToolStripButton>().Single(x => x.Text == "Apply");
				applyButton.Enabled = true;

				// no carrier service level popup
				model.UpdateMappings();
				applyButton.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.PreviousMessages.Single().Text);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestApply_WhenUnmappedChargeCode_ShouldShowChargeCodeMappingDialogAndApplyUnmappedChargeToJobWhenUserMappedIt()
		{
			var newChargeFRT = Helper.ChargeCodes.New("FRT1", "FRT1 Desc", UnitCalculator.Code);
			var (model, viewModel, _, _) = SetupModelAndViewModelWithApiRate("XXX");

			using (var form = new RateChooserForm(viewModel))
			{
				var toolStrip = (ZToolStrip)form.Controls.Find("toolStrip", true)[0];
				var applyButton = toolStrip.Items.Cast<ZToolStripButton>().Single(x => x.Text == "Apply");
				applyButton.Enabled = true;

				// first apply, no mapping
				ZFormModaliser.LastFormShownDialogForTest = null;
				applyButton.PerformClick();
				AssertType(typeof(MapChargeCodesForm), ZFormModaliser.LastFormShownDialogForTest);
				var selectedRate = model.GetSelectedRate();
				AssertEquals(0, selectedRate.Count);
				// map charge code
				var mappingFactory = new BusinessObjectFactory();
				var unmappedCharges = model.UnmappedCharges.CopyCodeAndDescriptionToAnotherFactory(mappingFactory);
				var chargeToMap = unmappedCharges.Single() as UniversalChargeCodeMapBizo;
				chargeToMap.LocalChargeCodePk = newChargeFRT.PK;
				unmappedCharges.ApplyUniversalCodeToChargeCodeForSaving();
				unmappedCharges.Factory.Save();

				// second apply, no popup
				model.UpdateMappings();
				ZFormModaliser.LastFormShownDialogForTest = null;
				applyButton.PerformClick();
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				selectedRate = model.GetSelectedRate();
				AssertEquals("Should have rate after we mapped the charge", 1, selectedRate.Count);
			}
		}

		public void TestApply_WhenChargeCodeIsZeroAndItIsInclusive_ShouldNotShowConfirmationMessage()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var frtCharge = Helper.ChargeCodes["FRT"];

			var quotedBooking = BaseRatingIntegrationTest.CreateQuotedBooking(Factory,"SEA", "FCL", ZString.Empty, consignor, consignor, consignee, TransportProvider1, "USLAX", "HKHKG", 10m, 1m, QuotedBookingState.QuoteOnly);
			var container = quotedBooking.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_RC = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.TC_ContainerCount = 1;

			Factory.Save();

			var (model, _) = GetModel(Factory, quotedBooking.GetFirstAdapter());
			var viewModel = new RateChooserViewModel(model);
			var apiCosting = ChooserHelper.CreateApiRate("20GP", TransportProvider1, "GEN");
			Helper.ChargeCodes.New("BAF", "BAF Desc", UnitCalculator.Code);
			RateChooserTestHelper.AddFlatCharge(apiCosting, "FRT", 25);
			RateChooserTestHelper.AddFlatCharge(apiCosting, "BAF", 0).OfType(ChargeType.Included).IncludedIn(frtCharge.AC_Code);

			var wiseRates = new[] { apiCosting };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);
			model.AddWiseRatesForTest(response);

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			var infos1 = new AutoRateInfoCollection(Factory);
			infos1.AddNew(Helper.ChargeCodes["FRT"], "USD", 0).IsInclusiveCalculator = true;

			chooseContainerCommodity.SelectedRate.SelectedCalculatedResult = infos1;

			var dialogService = new Mock<IDialogService>();

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new RateChooserForm(viewModel, dialogService.Object))
			{
				ApplyButtonPerformClick(form);

				dialogService.Verify(d => d.PromptUserApplyZeroCharges(It.IsAny<List<ZString>>()),
					Times.Never());

				Assert(true);
			}
		}

		public void TestApply_WhenUnmappedChargeCode_AndChargeCodeIsZeroAndUserSelectNoToApplyZeroCharge_ShouldNotShowChargeCodeMappingDialog()
		{
			Helper.ChargeCodes.New("FRT1", "FRT1 Desc", UnitCalculator.Code);
			var (_, viewModel, apiCosting, _) = SetupModelAndViewModelWithApiRate("XXX");
			apiCosting.Charges[0].PerUnitRate = 0; //make Charge zero

			var dialogService = new Mock<IDialogService>();
			dialogService
				.Setup(d => d.PromptUserApplyZeroCharges(It.IsAny<List<ZString>>()))
				.Returns(ZDialogResult.No);

			using (var form = new RateChooserForm(viewModel, dialogService.Object))
			{
				var toolStrip = (ZToolStrip)form.Controls.Find("toolStrip", true)[0];
				var applyButton = toolStrip.Items.Cast<ZToolStripButton>().Single(x => x.Text == "Apply");
				applyButton.Enabled = true;

				ZFormModaliser.LastFormShownDialogForTest = null;
				applyButton.PerformClick();

				AssertNull(ZFormModaliser.LastFormShownDialogForTest); //no mapping form shown
			}
		}

		public void TestApply_WhenUnmappedChargeCode_AndChargeCodeIsZeroAndUserSelectYesToApplyZeroCharge_ShouldShowChargeCodeMappingDialog()
		{
			Helper.ChargeCodes.New("FRT1", "FRT1 Desc", UnitCalculator.Code);
			var (_, viewModel, apiCosting, _) = SetupModelAndViewModelWithApiRate("XXX");
			apiCosting.Charges[0].PerUnitRate = 0; //make Charge zero

			var dialogService = new Mock<IDialogService>();
			dialogService
				.Setup(d => d.PromptUserApplyZeroCharges(It.IsAny<List<ZString>>()))
				.Returns(ZDialogResult.Yes);

			using (var form = new RateChooserForm(viewModel))
			{
				var toolStrip = (ZToolStrip)form.Controls.Find("toolStrip", true)[0];
				var applyButton = toolStrip.Items.Cast<ZToolStripButton>().Single(x => x.Text == "Apply");
				applyButton.Enabled = true;

				//no mapping and user selects to apply zero charges back to job
				ZFormModaliser.LastFormShownDialogForTest = null;
				applyButton.PerformClick();

				AssertType(typeof(MapChargeCodesForm), ZFormModaliser.LastFormShownDialogForTest);
				Assert(true);
			}
		}

		public void TestFind_WhenServiceProviderIsNotCarrier_ShouldNotThrowException()
		{
			var (_, viewModel, _, _) = SetupModelAndViewModelWithApiRate("XXX");

			using (var form = new RateChooserForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var filterControl = (RateChooserFilterStripControl)form.Controls.Find("stripControl", true).Single();
				var rateSelectorStripObject = (RateChooserFilterStripBusinessObject)filterControl.FilterBusinessObject;
				rateSelectorStripObject.ServiceProviderFilter.Property = TransportProvider1.PK;
				AssertEquals("The ServiceProviderFilter should not have errors", false, rateSelectorStripObject.ServiceProviderFilter.HasErrors);

				var toolStrip = (ZToolStrip)form.Controls.Find("toolStrip", true)[0];
				var applyButton = toolStrip.Items.Find("ToolStripFindDropButton", false).Single();
				AssertNoExceptionThrown(() => applyButton.PerformClick());
			}
		}

		public void TestOnLoad_SearchesRateService_Once()
		{
			var mockFactory = new Mock<IWiseRatesClientFactory>();
			var mockClient = new Mock<IWiseRatesClient>();

			mockFactory
				.Setup(f => f.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((mockClient.Object, string.Empty));

			mockClient
				.Setup(f => f.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new RatesSearchResponse()));

			ObjectFactory.Substitute(mockFactory.Object);

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (_, viewModel, _, _) = SetupModelAndViewModelWithApiRate("XXX");
				using (var form = new RateChooserForm(viewModel))
				{
					form.Show();
					Application.DoEvents();
				}
			}

			mockClient
				.Verify(f => f.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

			Assert(true);
		}

		public void TestApply_CheckAllMappings_InOrder()
		{
			var newChargeFRT = Helper.ChargeCodes.New("FRT1", "FRT1 Desc", UnitCalculator.Code);
			var (model, viewModel, apiCosting, _) = SetupModelAndViewModelWithApiRate("CCC");
			apiCosting.ServiceLevel = "XXX";

			using (var form = new RateChooserForm(viewModel))
			{
				var toolStrip = (ZToolStrip)form.Controls.Find("toolStrip", true)[0];
				var applyButton = toolStrip.Items.Cast<ZToolStripButton>().Single(x => x.Text == "Apply");
				applyButton.Enabled = true;

				// first apply and say Yes to map service level
				model.UpdateMappings();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				applyButton.PerformClick();
				var expectedMessage = "The Universal Service Level 'XXX' has NOT been assigned to Carrier 'SCAACARRIER'. Would you like to complete the assignment and apply rates to the job?";
				var previousMessages = UnitTestUserNotification.Instance.PreviousMessages.Select(m => m.Text);
				AssertCollectionContains(expectedMessage, previousMessages);

				// can't mock OrganizationForm to map service levels so let's map it manually
				var carrier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "SCAACARRIER");
				model.UniversalServiceLevels = new[] { new RefServiceLevel { Code = "UXXX", Description = "UXXX Description" } };
				var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
				carrierServiceLevel.PL_Code = "XXX";
				carrierServiceLevel.PL_CarrierServiceCode = "UXXX";
				carrierServiceLevel.PL_CarrierServiceLevelDescription = "UXXX Description";
				Factory.Save();
				model.UpdateMappings();

				// second apply requires charge code mapping
				ZFormModaliser.LastFormShownDialogForTest = null;
				applyButton.PerformClick();
				AssertType(typeof(MapChargeCodesForm), ZFormModaliser.LastFormShownDialogForTest);

				// map charge code
				var mappingFactory = new BusinessObjectFactory();
				var unmappedCharges = model.UnmappedCharges.CopyCodeAndDescriptionToAnotherFactory(mappingFactory);
				var chargeToMap = unmappedCharges.Single() as UniversalChargeCodeMapBizo;
				chargeToMap.LocalChargeCodePk = newChargeFRT.PK;
				unmappedCharges.ApplyUniversalCodeToChargeCodeForSaving();
				unmappedCharges.Factory.Save();

				// third apply, no popup
				model.UpdateMappings();
				ZFormModaliser.LastFormShownDialogForTest = null;
				applyButton.PerformClick();
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestApply_WhenSelectedRateLocationSameFromJob_WithMultiRouteOff_DoNotShowUserConfirmationAndNoChangeToJob() =>
			AssertUserConfirmationWhenRateLocationMismatchJob("AUSYD", "AUMEL", "AUSYD", "AUMEL", expectedPrompt: null, isMultiRouteEnabled: false, expectJobChanged: false);

		public void TestApply_WhenSelectedRateLocationDifferentFromJob_WithSpotRate_NoConfirmation_ThenApplyAndChangeJobLocation_Origin() =>
			AssertUserConfirmationWhenRateLocationMismatchJob("AUADL", "AUMEL", "AUSYD", "AUMEL", expectJobChanged: true, useResult: DialogResult.Yes, withSpotRate: true,
				expectedPrompt: null);

		public void TestApply_WhenSelectedRateLocationDifferentFromJob_WithSpotRate_NoConfirmation_ThenApplyAndChangeJobLocation_Destination() =>
			AssertUserConfirmationWhenRateLocationMismatchJob("AUSYD", "AUADL", "AUSYD", "AUMEL", expectJobChanged: true, useResult: DialogResult.Yes, withSpotRate: true,
				expectedPrompt: null);

		public void TestApply_WhenSelectedRateLocationDifferentFromJob_WithMultiRouteOff_ShowUserConfirmation_ThenApplyAndChangeJobLocation_Origin() =>
			AssertUserConfirmationWhenRateLocationMismatchJob("AUADL", "AUMEL", "AUSYD", "AUMEL", isMultiRouteEnabled: false, expectJobChanged: true, useResult: DialogResult.Yes,
				expectedPrompt: "During this operation, would you like to update the 1st Load of the Consol to be the Origin 'AUADL' of the chosen rates?");

		public void TestApply_WhenSelectedRateLocationDifferentFromJob_WithMultiRouteOff_ShowUserConfirmation_ThenApplyAndChangeJobLocation_Destinationn() =>
			AssertUserConfirmationWhenRateLocationMismatchJob("AUSYD", "AUADL", "AUSYD", "AUMEL", isMultiRouteEnabled: false, expectJobChanged: true, useResult: DialogResult.Yes,
				expectedPrompt: "During this operation, would you like to update the Last Discharge of the Consol to be the Destination 'AUADL' of the chosen rates?");

		public void TestApply_WhenSelectedRateLocationDifferentFromJob_WithMultiRouteOff_ShowUserConfirmation_ThenApplyAndDontChangeJobLocation_Origin() =>
			AssertUserConfirmationWhenRateLocationMismatchJob("AUADL", "AUMEL", "AUSYD", "AUMEL", isMultiRouteEnabled: false, expectJobChanged: false, useResult: DialogResult.No,
				expectedPrompt: "During this operation, would you like to update the 1st Load of the Consol to be the Origin 'AUADL' of the chosen rates?");

		public void TestApply_WhenSelectedRateLocationDifferentFromJob_WithMultiRouteOff_ShowUserConfirmation_ThenApplyAndDontChangeJobLocation_Destination() =>
			AssertUserConfirmationWhenRateLocationMismatchJob("AUSYD", "AUADL", "AUSYD", "AUMEL", isMultiRouteEnabled: false, expectJobChanged: false, useResult: DialogResult.No,
				expectedPrompt: "During this operation, would you like to update the Last Discharge of the Consol to be the Destination 'AUADL' of the chosen rates?");

		public void TestApply_WhenSelectedRateLocationDifferentFromJob_WithMultiRouteOn_DoNotShowUserConfirmationAndNoChangeToJob_Orign() =>
			AssertUserConfirmationWhenRateLocationMismatchJob("AUADL", "AUMEL", "AUSYD", "AUMEL", expectedPrompt: null, isMultiRouteEnabled: true, expectJobChanged: false);

		public void TestApply_WhenSelectedRateLocationDifferentFromJob_WithMultiRouteOn_DoNotShowUserConfirmationAndNoChangeToJob_Destination() =>
			AssertUserConfirmationWhenRateLocationMismatchJob("AUSYD", "AUADL", "AUSYD", "AUMEL", expectedPrompt: null, isMultiRouteEnabled: true, expectJobChanged: false);

		public void TestApply_WhenSelectedRateLocationSameFromJob_WithMultiRouteOn_DoNotShowUserConfirmationAndNoChangeToJob_Origin() =>
			AssertUserConfirmationWhenRateLocationMismatchJob("AUSYD", "AUMEL", "AUSYD", "AUMEL", expectedPrompt: null, isMultiRouteEnabled: true, expectJobChanged: false);

		public void TestApply_WhenSelectedRateLocationSameFromJob_WithMultiRouteOn_DoNotShowUserConfirmationAndNoChangeToJob_Destination() =>
			AssertUserConfirmationWhenRateLocationMismatchJob("AUSYD", "AUMEL", "AUSYD", "AUMEL", expectedPrompt: null, isMultiRouteEnabled: true, expectJobChanged: false);

		void AssertUserConfirmationWhenRateLocationMismatchJob(string rateOrigin, string rateDestination, string jobOrigin, string jobDestination, string expectedPrompt, bool expectJobChanged, bool isMultiRouteEnabled = false, bool withSpotRate = false, DialogResult useResult = DialogResult.None)
		{
			var serviceProvider = ChooserHelper.CreateCarrierOrg("SCAC");
			serviceProvider.OH_FullName = "Some carrier";
			var carrier = Helper.NewOrgHeader("SCAC");
			Factory.Save();

			var consol = ChooserHelper.CreateConsol(jobOrigin, jobDestination);
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			ChooserHelper.AddContainer(consol, "20GP", "GEN", Core.Constants.ContainerModes.FCL);

			var (model, _) = GetModel(Factory, consol.RatingAdapter);
			var viewModel = new RateChooserViewModel(model);

			var apiCosting = ChooserHelper.CreateApiRate("20GP", serviceProvider, "GEN");
			apiCosting.Origin = rateOrigin;
			apiCosting.Destination = rateDestination;
			apiCosting.ServiceLevel = OrgCarrierServiceLevel.StandardCode;
			RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT", 1000);
			if (withSpotRate)
			{
				apiCosting.BookingInfo = new BookingInfo();
			}

			var wiseRates = new[] { apiCosting };

			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);
			model.AddWiseRatesForTest(response);

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // For carrier updating
			if (useResult != DialogResult.None)
			{
				UnitTestUserNotification.Instance.AddAnswer(useResult); // For location updating
			}

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isMultiRouteEnabled))
			using (var form = new RateChooserForm(viewModel))
			{
				ApplyButtonPerformClick(form);
			}

			if (expectedPrompt != null)
			{
				var shownMessages = UnitTestUserNotification.Instance.PreviousMessages.Select(m => m.Text);
				AssertCollectionContains(expectedPrompt, shownMessages);
			}

			AssertEquals(expectJobChanged ? rateOrigin : jobOrigin, consol.JK_RL_NKLoadPort);
			AssertEquals(expectJobChanged ? rateDestination : jobDestination, consol.JK_RL_NKDischargePort);
		}

		public void TestApply_UserConfirmationToApplyCarrierBackToJob_ShowWhenCarrierWillBePopulated()
		{
			var serviceProvider = ChooserHelper.CreateCarrierOrg("SCAC");
			serviceProvider.OH_FullName = "Service Provider Name";
			var carrier = Helper.NewOrgHeader();
			Factory.Save();

			var consol = ChooserHelper.CreateConsol();
			consol.SetDefaultShippingLineAddress(carrier);
			ChooserHelper.AddContainer(consol, "20GP", "GEN", Core.Constants.ContainerModes.FCL);

			var (model, _) = GetModel(Factory, consol.RatingAdapter);
			var viewModel = new RateChooserViewModel(model);
			var apiCosting = ChooserHelper.CreateApiRate("20GP", serviceProvider, "GEN");
			apiCosting.ServiceLevel = OrgCarrierServiceLevel.StandardCode;
			RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT", 1000);

			var wiseRates = new[] { apiCosting };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);
			model.AddWiseRatesForTest(response);

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			// Single Route
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new RateChooserForm(viewModel))
			{
				ApplyButtonPerformClick(form);

				var shownMessages = UnitTestUserNotification.Instance.PreviousMessages.Select(m => m.Text);
				AssertCollectionContains("The expected confirmation message was not shown for a single route.", UpdateCarrierConfirmationMessage_SingleRoute, shownMessages);
				Assert("Model validation should pass.", model.IsValid);
			}

			// Multi Route
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new RateChooserForm(viewModel))
			{
				ApplyButtonPerformClick(form);

				var shownMessages = UnitTestUserNotification.Instance.PreviousMessages.Select(m => m.Text);
				AssertCollectionContains("The expected confirmation message was not shown for a multi route.", UpdateCarrierConfirmationMessage_MultiRoute, shownMessages);
				Assert("Model validation should pass.", model.IsValid);
			}
		}

		public void TestApply_ConfirmationToApplyCarrierBackToConsol_NotShowWhenJobCarrierIsEmpty_SingleRoute() =>
			AssertApply_ConfirmationToApplyCarrierBackToConsol_NotShowWhenJobCarrierIsEmpty(
				isMultiRouteSupported: false,
				unexpectedMessage: UpdateCarrierConfirmationMessage_SingleRoute);

		public void TestApply_ConfirmationToApplyCarrierBackToConsol_NotShowWhenJobCarrierIsEmpty_MultiRoute() =>
			AssertApply_ConfirmationToApplyCarrierBackToConsol_NotShowWhenJobCarrierIsEmpty(
				isMultiRouteSupported: true,
				unexpectedMessage: UpdateCarrierConfirmationMessage_MultiRoute);

		void AssertApply_ConfirmationToApplyCarrierBackToConsol_NotShowWhenJobCarrierIsEmpty(bool isMultiRouteSupported, string unexpectedMessage)
		{
			var serviceProvider = ChooserHelper.CreateCarrierOrg("SCAC");
			serviceProvider.OH_FullName = "Service Provider Name";
			Factory.Save();

			RatingDataRegistry.Instance.MultiModalRatingCost.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isMultiRouteSupported);

			var consol = ChooserHelper.CreateConsol();
			var routes = consol.Transports;
			IAutoRating ratingAdapter;
			if (isMultiRouteSupported)
			{
				var additionalRoute = routes.AddNew();
				additionalRoute.JW_ETD = ZDate.Today.AddDays(4);
				additionalRoute.JW_ETA = ZDate.Today.AddDays(6);
				additionalRoute.JW_RL_NKLoadPort = "HKHKG";
				additionalRoute.JW_RL_NKDiscPort = "SGSGN";

				var routeSet = new RouteSetRatingRoute(new RouteSet(Factory, 1, routes[0], routes[1]), consol);
				ratingAdapter = new ForwardingConsolRatingAdapter(routeSet, true);
			}
			else
			{
				ratingAdapter = consol.RatingAdapter;
			}
			ChooserHelper.AddContainer(consol, "20GP", "GEN", Core.Constants.ContainerModes.FCL);

			var (model, _) = GetModel(Factory, ratingAdapter);
			var viewModel = new RateChooserViewModel(model);
			var apiCosting = ChooserHelper.CreateApiRate("20GP", serviceProvider, "GEN");
			apiCosting.ServiceLevel = OrgCarrierServiceLevel.StandardCode;
			RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT", 1000);

			var wiseRates = new[] { apiCosting };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);
			model.AddWiseRatesForTest(response);

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			using (var form = new RateChooserForm(viewModel))
			{
				AssertEquals("Precondition: consol has empty Carrier field", ZGuid.Empty, consol.ShippingLinePK);
				Assert("Precondition: routes don't have carriers", routes.OfType<Transport>().All(x => x.Carrier == null));

				ApplyButtonPerformClick(form);

				var shownMessages = UnitTestUserNotification.Instance.PreviousMessages.Select(m => m.Text);
				AssertCollectionNotContains(unexpectedMessage, shownMessages);

				AssertEquals(true, model.IsValid);
				AssertEquals("Consol's carrier should be updated", serviceProvider.PK, consol.ShippingLinePK);
				if (isMultiRouteSupported)
				{
					AssertEquals("Related leg's carrier should be updated", serviceProvider.PK, routes[0].Carrier.PK);
					AssertNull("Unrelated leg's carrier should not be updated", routes[1].Carrier);
				}
			}
		}

		public void TestApply_ConfirmationToApplyCarrierBackToQuickBooking_NotShowWhenJobCarrierIsEmpty()
		{
			var serviceProvider = ChooserHelper.CreateCarrierOrg("SCAC");
			serviceProvider.OH_FullName = "Service Provider Name";
			Factory.Save();

			var quickBooking = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL, ZString.Empty, Consignor, Consignor, Consignee, null, "USLAX", "HKHKG", 10m, 1m, QuotedBookingState.BookingOnly);
			quickBooking.StartDate = ZDate.Today;
			quickBooking.EndDate = ZDate.Today;
			var container = quickBooking.QuotedBookingContainers.AddNew();
			container.JC_RC = GP20.PK;
			container.JC_ContainerCount = 1;

			var (model, _) = GetModel(Factory, quickBooking.GetRatingAdapters().Single());
			var viewModel = new RateChooserViewModel(model);
			var apiCosting = ChooserHelper.CreateApiRate("20GP", serviceProvider, "GEN");
			apiCosting.ServiceLevel = OrgCarrierServiceLevel.StandardCode;
			RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT", 1000);

			var wiseRates = new[] { apiCosting };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);
			model.AddWiseRatesForTest(response);

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			using (var form = new RateChooserForm(viewModel))
			{
				AssertEquals("Precondition: Carrier field is empty", ZGuid.Empty, quickBooking.OH_Carrier);
				ApplyButtonPerformClick(form);

				var shownMessages = UnitTestUserNotification.Instance.PreviousMessages.Select(m => m.Text);
				AssertCollectionNotContains(
					@"During this operation, Service Provider 'Service Provider Name' of the chosen rates will be populated as the Carrier of your Quick Booking. 

		You may need to manually adjust/remove Charges if they are no longer valid.

		Do you wish to continue?",
					shownMessages
				);

				Assert("Model should remain valid", model.IsValid);
				AssertEquals("Carrier field should be updated", serviceProvider.PK, quickBooking.OH_Carrier);
			}
		}

		public void TestApply_ConfirmationToApplyCarrierBackToConsol_NotShowWhenConsolCarrierWontBePopulated()
		{
			var serviceProvider = ChooserHelper.CreateCarrierOrg("SCAC");
			serviceProvider.OH_FullName = "Service Provider Name";
			var carrier = serviceProvider;
			Factory.Save();

			var consol = ChooserHelper.CreateConsol();
			consol.SetDefaultShippingLineAddress(carrier);
			ChooserHelper.AddContainer(consol, "20GP", "GEN", Core.Constants.ContainerModes.FCL);

			var (model, _) = GetModel(Factory, consol.RatingAdapter);
			var viewModel = new RateChooserViewModel(model);
			var apiCosting = ChooserHelper.CreateApiRate("20GP", serviceProvider, "GEN");
			apiCosting.ServiceLevel = OrgCarrierServiceLevel.StandardCode;
			RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT", 1000);

			var wiseRates = new[] { apiCosting };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);
			model.AddWiseRatesForTest(response);

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			// Single Route
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new RateChooserForm(viewModel))
			{
				ApplyButtonPerformClick(form);

				var shownMessages = UnitTestUserNotification.Instance.PreviousMessages.Select(m => m.Text);
				AssertEquals("Selected service provider should match the carrier.", carrier.PK, model.Criteria.SelectedServiceProviderFromRateSelector);
				AssertCollectionNotContains(UpdateCarrierConfirmationMessage_SingleRoute, shownMessages);
			}

			// Multi Route
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new RateChooserForm(viewModel))
			{
				ApplyButtonPerformClick(form);

				var shownMessages = UnitTestUserNotification.Instance.PreviousMessages.Select(m => m.Text);
				AssertEquals("Selected service provider should match the carrier.", carrier.PK, model.Criteria.SelectedServiceProviderFromRateSelector);
				AssertCollectionNotContains(UpdateCarrierConfirmationMessage_MultiRoute, shownMessages);
			}
		}

		public void TestApply_ConfirmationToApplyCarrierBackToConsol_NotShowWhenModelIsNotValid()
		{
			var serviceProvider = ChooserHelper.CreateCarrierOrg("SCAC");
			serviceProvider.OH_FullName = "Service Provider Name";
			var carrier = Helper.NewOrgHeader();
			Factory.Save();

			var consol = ChooserHelper.CreateConsol();
			consol.SetDefaultShippingLineAddress(carrier);
			ChooserHelper.AddContainer(consol, "20GP", "GEN", Core.Constants.ContainerModes.FCL);

			var (model, _) = GetModel(Factory, consol.RatingAdapter);
			var viewModel = new RateChooserViewModel(model);
			var apiCosting = ChooserHelper.CreateApiRate("20GP", serviceProvider, "GEN");
			apiCosting.ServiceLevel = OrgCarrierServiceLevel.StandardCode;
			RateChooserTestHelper.AddPerContainerCharge(apiCosting, "XXX", 1000);

			var wiseRates = new[] { apiCosting };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);
			model.AddWiseRatesForTest(response);

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			// Single Route
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new RateChooserForm(viewModel))
			{
				ApplyButtonPerformClick(form);

				var shownMessages = UnitTestUserNotification.Instance.PreviousMessages.Select(m => m.Text);
				AssertEquals("Model validity should be false", false, model.IsValid);
				AssertEquals("Selected service provider should be empty", ZGuid.Empty, model.Criteria.SelectedServiceProviderFromRateSelector);
				AssertCollectionNotContains(UpdateCarrierConfirmationMessage_SingleRoute, shownMessages);
			}

			// Multi Route
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new RateChooserForm(viewModel))
			{
				ApplyButtonPerformClick(form);

				var shownMessages = UnitTestUserNotification.Instance.PreviousMessages.Select(m => m.Text);
				AssertEquals("Model validity should be false", false, model.IsValid);
				AssertEquals("Selected service provider should be empty", ZGuid.Empty, model.Criteria.SelectedServiceProviderFromRateSelector);
				AssertCollectionNotContains(UpdateCarrierConfirmationMessage_MultiRoute, shownMessages);
			}
		}

		public void TestApply_SelectedRateAttributesAreDifferentFromJob_PopulateRateDataBackToJob()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var serviceProvider = ChooserHelper.CreateCarrierOrg("OOCL");
			serviceProvider.OH_FullName = "Service Provider Name";

			var quotedBooking = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, "SEA", "FCL", ZString.Empty, consignor, consignor, consignee, serviceProvider, "AUSYD", "SGSIN", 10m, 1m, QuotedBookingState.QuoteOnly);
			var container = quotedBooking.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_RC = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.TC_ContainerCount = 1;

			Factory.Save();

			var (model, _) = GetModel(Factory, quotedBooking.GetFirstAdapter());
			var viewModel = new RateChooserViewModel(model);
			var apiCosting = ChooserHelper.CreateApiRate("20GP", TransportProvider1, "GEN");
			apiCosting.ServiceLevel = OrgCarrierServiceLevel.StandardCode;
			RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT", 1000);

			var wiseRates = new[] { apiCosting };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);
			model.AddWiseRatesForTest(response);

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new RateChooserForm(viewModel))
			{
				UnitTestUserNotification.Instance.AddYesAnswer();
				UnitTestUserNotification.Instance.AddYesAnswer();
				UnitTestUserNotification.Instance.AddYesAnswer();

				ApplyButtonPerformClick(form);

				AssertEquals("The origin of the quoted booking should match the expected value.", "USLAX", quotedBooking.Origin);
				AssertEquals("The destination of the quoted booking should match the expected value.", "HKHKG", quotedBooking.Destination);
				AssertEquals("The carrier of the quoted booking should match the expected value.", TransportProvider1.PK, quotedBooking.Carrier.PK);
				AssertEquals("The service level of the quoted booking should match the expected value.", "STD", quotedBooking.CarrierServiceLevel);
			}
		}

		public void TestApply_SelectedRateDestinationIsDifferentFromJob_ShowsConfirmationMessage()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var quotedBooking = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, "SEA", "FCL", ZString.Empty, consignor, consignor, consignee, TransportProvider1, "USLAX", "SGSIN", 10m, 1m, QuotedBookingState.QuoteOnly);
			var container = quotedBooking.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_RC = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.TC_ContainerCount = 1;

			Factory.Save();

			var (model, _) = GetModel(Factory, quotedBooking.GetFirstAdapter());
			var viewModel = new RateChooserViewModel(model);
			var apiCosting = ChooserHelper.CreateApiRate("20GP", TransportProvider1, "GEN");
			RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT", 1000);

			var wiseRates = new[] { apiCosting };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);
			model.AddWiseRatesForTest(response);

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new RateChooserForm(viewModel))
			{
				ApplyButtonPerformClick(form);

				var shownMessages = UnitTestUserNotification.Instance.LastMessage.Text;
				var expectedMessage = @"During this operation, would you like to update the Destination of the One Off Quote to be the Destination 'HKHKG' of the chosen rates?";

				AssertContains("Expected confirmation message about updating the destination not shown.", expectedMessage, shownMessages);
			}
		}

		public void TestApply_SelectedRateOriginIsDifferentFromJob_ShowsConfirmationMessage()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var quotedBooking = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, "SEA", "FCL", ZString.Empty, consignor, consignor, consignee, TransportProvider1, "AUSYD", "HKHKG", 10m, 1m, QuotedBookingState.QuoteOnly);
			var container = quotedBooking.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_RC = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.TC_ContainerCount = 1;

			Factory.Save();

			var (model, _) = GetModel(Factory, quotedBooking.GetFirstAdapter());
			var viewModel = new RateChooserViewModel(model);
			var apiCosting = ChooserHelper.CreateApiRate("20GP", TransportProvider1, "GEN");
			RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT", 1000);

			var wiseRates = new[] { apiCosting };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);
			model.AddWiseRatesForTest(response);

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new RateChooserForm(viewModel))
			{
				ApplyButtonPerformClick(form);

				var shownMessages = UnitTestUserNotification.Instance.LastMessage.Text;
				var expectedMessage = "During this operation, would you like to update the Origin of the One Off Quote to be the Origin 'USLAX' of the chosen rates?";

				AssertContains(
					"Expected confirmation message was not shown.",
					expectedMessage,
					shownMessages
				);
			}
		}

		public void TestApply_SelectedRateHasZeroChargeAndUserSelectYes_ShowsConfirmationMessageAndPopulateBack()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var quotedBooking = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, "SEA", "FCL", ZString.Empty, consignor, consignor, consignee, TransportProvider1, "USLAX", "HKHKG", 10m, 1m, QuotedBookingState.QuoteOnly);
			var container = quotedBooking.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_RC = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.TC_ContainerCount = 1;

			Factory.Save();

			var (model, _) = GetModel(Factory, quotedBooking.GetFirstAdapter());
			var viewModel = new RateChooserViewModel(model);
			var apiCosting = ChooserHelper.CreateApiRate("20GP", TransportProvider1, "GEN");
			RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT", 0);

			var wiseRates = new[] { apiCosting };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);
			model.AddWiseRatesForTest(response);

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			var dialogService = new Mock<IDialogService>();
			dialogService
				.Setup(d => d.PromptUserApplyZeroCharges(
					It.Is<List<ZString>>(c => c
						.Select(b => b)
						.IsEquivalentTo(new List<ZString> { "FRT" }))))
				.Returns(ZDialogResult.Yes);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new RateChooserForm(viewModel, dialogService.Object))
			{
				ApplyButtonPerformClick(form);

				var selectedRate = model.GetSelectedRate();
				AssertEquals("Expected selected rate to match the specified format", "FRT: 1 20GP Container(s) @ AUD 0.00/Container", selectedRate.Single().SingleLineDescription);
			}
		}

		public void TestApply_SelectedRateHasZeroAmountAndUserSelectNo_ShowsConfirmationMessageAndNotPopulateBack()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var quotedBooking = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, "SEA", "FCL", ZString.Empty, consignor, consignor, consignee, TransportProvider1, "USLAX", "HKHKG", 10m, 1m, QuotedBookingState.QuoteOnly);
			var container = quotedBooking.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_RC = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.TC_ContainerCount = 1;

			Factory.Save();

			var (model, _) = GetModel(Factory, quotedBooking.GetFirstAdapter());
			var viewModel = new RateChooserViewModel(model);
			var apiCosting = ChooserHelper.CreateApiRate("20GP", TransportProvider1, "GEN");
			RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT", 0);

			var wiseRates = new[] { apiCosting };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);
			model.AddWiseRatesForTest(response);

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			var dialogService = new Mock<IDialogService>();
			dialogService
				.Setup(d => d.PromptUserApplyZeroCharges(
					It.Is<List<ZString>>(c => c
						.Select(b => b)
						.IsEquivalentTo(new List<ZString> { "FRT" }))))
				.Returns(ZDialogResult.No);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new RateChooserForm(viewModel, dialogService.Object))
			{
				ApplyButtonPerformClick(form);

				var selectedRate = model.GetSelectedRate();
				AssertEquals("Expected an empty selected rate when user chooses not to apply zero charges.", 0, selectedRate.Count);
			}
		}

		public void TestApply_SelectedRateHasZeroCalculatedAmountForPercentageCalculator_ShouldNotShowConfirmationMessage()
		{
			Helper.ChargeCodes.New("FRT1", "FRT1 Desc", UnitCalculator.Code);
			Helper.ChargeCodes.New("FRT2", "FRT2 Desc", PercentageCalculator.Code);

			Helper.ChargeCodes.New("FRT3", "FRT1 Desc", UnitCalculator.Code);
			Helper.ChargeCodes.New("FRT4", "FRT2 Desc", PercentageCalculator.Code);

			var carrier = ChooserHelper.CreateCarrierOrg("SCAA");
			Factory.Save();

			var consol = ChooserHelper.CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			ChooserHelper.AddContainer(consol, "20GP", "GEN", Core.Constants.ContainerModes.FCL);

			var (model, logger) = GetModel(Factory, consol.RatingAdapter);
			var apiCosting = ChooserHelper.CreateApiRate("20GP", carrier, "GEN");
			RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT1", 0);
			var chargePercent = RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT2", 0);
			chargePercent.Percentage = 15;
			chargePercent.PercentageAppliesTo = "FRT1";

			RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT3", 1000);
			var chargePercent2 = RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT4", 0);
			chargePercent2.Percentage = 15;
			chargePercent2.PercentageAppliesTo = "FRT3";

			var wiseRates = new[] { apiCosting };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);

			model.AddWiseRatesForTest(response);

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			var viewModel = new RateChooserViewModel(model);

			var dialogService = new Mock<IDialogService>();
			dialogService
				.Setup(d => d.PromptUserApplyZeroCharges(It.IsAny<List<ZString>>()))
				.Returns(ZDialogResult.Yes);

			using (var form = new RateChooserForm(viewModel))
			{
				ApplyButtonPerformClick(form);

				var message = "Charges: FRT1 are found to have Zero amount, do you want to populate those charges back to the job?";
				AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestApply_WhenSelectedRateIsCW1AndHasZeroChargeAndUserSelectYes_ShowsConfirmationMessageAndPopulateBack()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");

			var consol = ChooserHelper.CreateConsol();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			ChooserHelper.AddContainer(consol, "20GP", "GEN", "FCL");

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 0;

			Factory.Save();

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			var dialogService = new Mock<IDialogService>();
			dialogService
				.Setup(d => d.PromptUserApplyZeroCharges(
					It.Is<List<ZString>>(c => c
						.Select(b => b)
						.IsEquivalentTo(new List<ZString> { "FRT" }))))
				.Returns(ZDialogResult.Yes);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new RateChooserForm(new RateChooserViewModel(model), dialogService.Object))
			{
				ApplyButtonPerformClick(form);

				var selectedConvertedRates = model.GetSelectedRate();
				AssertEquals(
					"Expected description for the selected rate",
					"FRT: 1 20GP Container(s) @ AUD 0.00/Container",
					selectedConvertedRates.Single().SingleLineDescription
				);
			}
		}

		public void TestApply_WhenSelectedRateIsCW1AndHasZeroChargeAndUserSelectNo_ShowsConfirmationMessageAndNotPopulateBack()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");

			var consol = ChooserHelper.CreateConsol();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			ChooserHelper.AddContainer(consol, "20GP", "GEN", "FCL");

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 0;

			Factory.Save();

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			var dialogService = new Mock<IDialogService>();
			dialogService
				.Setup(d => d.PromptUserApplyZeroCharges(
					It.Is<List<ZString>>(c => c
						.Select(b => b)
						.IsEquivalentTo(new List<ZString> { "FRT" }))))
				.Returns(ZDialogResult.No);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new RateChooserForm(new RateChooserViewModel(model), dialogService.Object))
			{
				ApplyButtonPerformClick(form);

				var selectedRate = model.GetSelectedRate();
				AssertEquals("Selected rate should be empty when user selects 'No' for zero charges.", 0, selectedRate.Count);
			}
		}

		public void TestApply_WhenMultiRouteEnabledAndSelectedRateIsASpotRate_ShouldUseNewLegsInformationWhenNecessary()
		{
			var baseDate = Env.Time.CurrentLocalDateTime;

			var consol = ChooserHelper.CreateConsol();
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			ChooserHelper.AddContainer(consol, "20GP", "GEN", Core.Constants.ContainerModes.FCL);

			var t1 = consol.Transports[0];
			t1.JW_IsLinked = false;
			t1.JW_VoyageFlight = "VF01";
			t1.JW_RL_NKLoadPort = "USLAX";
			t1.JW_RL_NKDiscPort = "CNSGH";
			t1.JW_ETD = baseDate;
			t1.JW_ETA = baseDate.AddDays(30);
			t1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			t1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			t1.JW_CarrierBookingReference = "Route1";

			var t2 = consol.Transports.AddNew("AUSYD", "NZAKL");
			t2.JW_IsLinked = false;
			t2.JW_VoyageFlight = "123S";
			t2.JW_RL_NKLoadPort = "CNSGH";
			t2.JW_RL_NKDiscPort = "SGSIN";
			t2.JW_ETD = baseDate.AddDays(31);
			t2.JW_ETA = baseDate.AddDays(40);
			t2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			t2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			t2.JW_CarrierBookingReference = "Route1";

			var t3 = consol.Transports.AddNew("NZAKL", "SGSIN");
			t3.JW_IsLinked = false;
			t3.JW_VoyageFlight = "123K";
			t3.JW_RL_NKLoadPort = "SGSIN";
			t3.JW_RL_NKDiscPort = "HKHKG";
			t3.JW_ETD = baseDate.AddDays(41);
			t3.JW_ETA = baseDate.AddDays(60);
			t3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			t3.JW_TransportType = Core.Constants.TransportPlanningType.Other;

			var routeSet = new RouteSetRatingRoute(new RouteSet(Factory, 1, t1, t2), consol);
			var ratingAdapter = new ForwardingConsolRatingAdapter(routeSet, true);
			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRatingInfo = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			criteria.IsManualCostSelectMode = true;
			var model = new RateChooserModel(criteria, context);
			var viewModel = new RateChooserViewModel(model);

			var apiCosting = ChooserHelper.CreateApiRate("20GP", TransportProvider1, "GEN");
			apiCosting.ServiceLevel = OrgCarrierServiceLevel.StandardCode;
			RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT", 1000);

			var wiseRates = new[] { apiCosting };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);

			response.Rates[0].BookingInfo = new BookingInfo()
			{
				Schedule = new Schedule()
				{
					DepartureDate = baseDate.AddDays(15),
					ArrivalDate = baseDate.AddDays(25).AddHours(11),
					VesselName = "V For Vendetta",
					VoyageNumber = "V9829",

					ScheduleDetails = new[]
					{
						new ScheduleDetail()
						{
							DepartureDate = baseDate.AddDays(15),
							ArrivalDate = baseDate.AddDays(25).AddHours(11),
							Origin = "USLAX",
							Destination = "SGSIN",
						},
					}
				},
			};

			model.AddWiseRatesForTest(response);

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new RateChooserForm(viewModel))
			{
				AssertEquals("Departure date before applying should match base date.", baseDate, criteria.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); // Vessel Creation
				ApplyButtonPerformClick(form);

				AssertNullOrEmpty(ErrorReporter.LastMessageReported);

				AssertEquals("Departure date after applying should match the new date.", baseDate.AddDays(15), criteria.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
			}
		}

		public void TestApply_NothingWillBePopulatedBackToJob_WhenOneOfConfirmationsGetRejected()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var serviceProvider = ChooserHelper.CreateCarrierOrg("OOCL");
			serviceProvider.OH_FullName = "Service Provider Name";

			var quotedBooking = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, "SEA", "FCL", ZString.Empty, consignor, consignor, consignee, serviceProvider, "AUSYD", "SGSIN", 10m, 1m, QuotedBookingState.QuoteOnly);
			var container = quotedBooking.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_RC = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.TC_ContainerCount = 1;

			Factory.Save();

			var (model, _) = GetModel(Factory, quotedBooking.GetFirstAdapter());
			var viewModel = new RateChooserViewModel(model);
			var criteria = model.Criteria;

			var dialogService = new Mock<IDialogService>();

			dialogService.Setup(d => d.PromptToApplyDestinationToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(ZDialogResult.Cancel);
			dialogService.Setup(d => d.PromptToApplyCarrierToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
			dialogService.Setup(d => d.PromptToApplyOriginToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(ZDialogResult.Yes);

			AssertJobAttributesHasNotChangedToSelectedRateAttributes(dialogService.Object, model, viewModel, quotedBooking.GetFirstAdapter());

			dialogService.Setup(d => d.PromptToApplyCarrierToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
			dialogService.Setup(d => d.PromptToApplyOriginToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(ZDialogResult.Yes);
			dialogService.Setup(d => d.PromptToApplyDestinationToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(ZDialogResult.Yes);

			AssertJobAttributesHasNotChangedToSelectedRateAttributes(dialogService.Object, model, viewModel, quotedBooking.GetFirstAdapter());

			dialogService.Setup(d => d.PromptToApplyOriginToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(ZDialogResult.Cancel);
			dialogService.Setup(d => d.PromptToApplyDestinationToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(ZDialogResult.Yes);
			dialogService.Setup(d => d.PromptToApplyCarrierToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

			AssertJobAttributesHasNotChangedToSelectedRateAttributes(dialogService.Object, model, viewModel, quotedBooking.GetFirstAdapter());
		}

		public void TestApplyCarrierQuoteNumberBackToJob()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			Factory.Save();

			var consol = ChooserHelper.CreateConsol();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			ChooserHelper.AddContainer(consol, "20GP", "GEN", "FCL");

			var apiCosting20GP = ChooserHelper.CreateApiRate("20GP", carrier, "")
				.WithCarrierQuoteNumber("PI000GP20");

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);

			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting20GP });
			model.AddWiseRatesForTest(response);

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			using (var form = new RateChooserForm(new RateChooserViewModel(model)))
			{
				ApplyButtonPerformClick(form);
			}

			var values = consol.Numbers.GetAllReferenceNumbersByType("CQN");
			AssertEquals("should be one CQN entry", 1, values.Length);
			AssertEquals("CQN entry should exits", "PI000GP20", values[0]);
		}

		public void TestApplyBothCarrierAndLocationBackToJob_WhenDifferentFromSelectedRate()
		{
			var consolCarrier = ChooserHelper.CreateCarrierOrg("COD2");
			var rateServiceProvider = ChooserHelper.CreateCarrierOrg("SCAC");
			Factory.Save();

			var consol = ChooserHelper.CreateConsol("SGSIN", "DEHAM");
			consol.JK_OA_ShippingLineAddress = consolCarrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = consolCarrier.MainAddress.PK;
			consol.Transports[0].JW_Vessel = "ADMIRAL";
			consol.Transports[0].JW_VoyageFlight = "1235";
			ChooserHelper.AddContainer(consol, "20GP", "GEN", Core.Constants.ContainerModes.FCL, 3);

			var apiCosting = ChooserHelper.CreateApiRate("20GP", rateServiceProvider, "");
			apiCosting.Origin = "HKHKG";
			apiCosting.Destination = "USLAX";
			RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT", 1000);

			var logger = new ElementaryLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting });
			model.AddWiseRatesForTest(response);

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new RateChooserForm(new RateChooserViewModel(model)))
			{
				// Assumptions
				AssertEquals("SGSIN", consol.JK_RL_NKLoadPort);
				AssertEquals("DEHAM", consol.JK_RL_NKDischargePort);
				AssertEquals(consolCarrier.PK, consol.ShippingLinePK);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Update Carrier Confirmation
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Update Destination
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Update Origin
				ApplyButtonPerformClick(form);
			}

			var shownMessages = UnitTestUserNotification.Instance.PreviousMessages.Select(m => m.Text);
			AssertCollectionContains(
				"Expected confirmation messages during rate adjustments",
				"During this operation, would you like to update the Last Discharge of the Consol to be the Destination 'USLAX' of the chosen rates?",
				shownMessages
			);
			AssertCollectionContains(
				"Expected confirmation messages during rate adjustments",
				"During this operation, would you like to update the 1st Load of the Consol to be the Origin 'HKHKG' of the chosen rates?",
				shownMessages
			);

			// Assertions
			AssertEquals("HKHKG", consol.JK_RL_NKLoadPort);
			AssertEquals("USLAX", consol.JK_RL_NKDischargePort);
			AssertEquals(rateServiceProvider.PK, consol.ShippingLinePK);
		}

		public void TestApply_WhenThereIsOnlyOneNACOnSelectedRates_ShouldBeAppliedBackToJob()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			Factory.Save();

			var consol = ChooserHelper.CreateConsol();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			ChooserHelper.AddContainer(consol, "20GP", "GEN", "FCL");

			var apiCosting20GP = ChooserHelper.CreateApiRate("20GP", carrier, "")
				.WithNamedAccounts("OnlyOneNAC");

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);

			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting20GP });
			model.AddWiseRatesForTest(response);

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			using (var form = new RateChooserForm(new RateChooserViewModel(model)))
			{
				ApplyButtonPerformClick(form);
			}

			var values = consol.Numbers.GetAllReferenceNumbersByType("NAC");
			AssertEquals("should be one NAC entry", 1, values.Length);
			AssertEquals("NAC entry should exits", "OnlyOneNAC", values[0]);
		}

		public void TestApply_ShouldReportUsage_WithRatesDetails()
		{
			var carrier1 = ChooserHelper.CreateCarrierOrg("SCA1");
			var carrier2 = ChooserHelper.CreateCarrierOrg("SCA2");
			var carrier3 = ChooserHelper.CreateCarrierOrg("SCA3");
			Factory.Save();

			var consol = ChooserHelper.CreateConsol();
			consol.JK_OA_CreditorAddress = carrier1.MainAddress.PK;
			ChooserHelper.AddContainer(consol, "20GP", "GEN", "FCL");

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);

			var cargoSphereRates = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[]
			{
				ChooserHelper.CreateApiRate("20GP", carrier1, ""),
				ChooserHelper.CreateApiRate("20GP", carrier2, ""),
				ChooserHelper.CreateApiRate("20GP", carrier3, ""),
			});

			var costing1 = Helper.NewCosting(carrier1);
			var costing2 = Helper.NewCosting(carrier2);
			costing1.TH_GC = Env.CurrentCompanyPK;
			costing2.TH_GC = Env.CurrentCompanyPK;

			var container20PG = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var rateEntry1 = costing1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "HKHKG", "FRT", 11m);
			rateEntry1.TI_RC = container20PG.PK;
			rateEntry1.TI_ContractNumber = "CN1";
			var rateEntry2 = costing2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "HKHKG", "FRT", 21m);
			rateEntry2.TI_RC = container20PG.PK;
			rateEntry2.TI_ContractNumber = "CN2";

			model.AddWiseRatesForTest(cargoSphereRates);
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry2));

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection
				.Cast<ChooserRateEntry>()
				.First(r => r.WiseRateEntry != null);

			using (var form = new RateChooserForm(new RateChooserViewModel(model)))
			{
				form.ApplyButtonClick();

				var messages = UsageCollectorTestHelper.LoadUsageMessages(UsageFeatures.Codes.RateSelector);
				var properties = messages[0].UsageProperties;

				AssertEquals("Expected action to be 'Select'.", "Select", properties.Value<string>(UsageProperties.Action));
				AssertEquals("Expected selected provider to be 'CargoSphere'.", "CargoSphere", properties.Value<string>(UsageProperties.SelectedProvider));

				var ratesResult = properties.Value<JObject>(UsageProperties.RatesSearchResult);

				AssertEquals("Expected total CargoSphere rates count to be 3.", 3, ratesResult.Value<JObject>("CargoSphere").Value<int>("TotalRates"));
				AssertEquals("Expected total CW1 rates count to be 2.", 2, ratesResult.Value<JObject>("CW1").Value<int>("TotalRates"));
			}
		}

		public void TestSkipRateSelection_ShouldReportUsage_WithSkipAction()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			Factory.Save();

			var consol = ChooserHelper.CreateConsol();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			ChooserHelper.AddContainer(consol, "20GP", "GEN", "FCL");

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);

			using (var form = new RateChooserForm(new RateChooserViewModel(model)))
			{
				form.SkipButtonClick();

				var messages = UsageCollectorTestHelper.LoadUsageMessages(UsageFeatures.Codes.RateSelector);
				AssertEquals(
					"Expected action to be 'Skip' for the usage message",
					"Skip",
					messages[0].UsageProperties.Value<string>(UsageProperties.Action)
				);
			}
		}

		public void TestCancelRateSelection_ShouldReportUsage_WithCancelAction()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			Factory.Save();

			var consol = ChooserHelper.CreateConsol();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			ChooserHelper.AddContainer(consol, "20GP", "GEN", "FCL");

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);

			using (var form = new RateChooserForm(new RateChooserViewModel(model)))
			{
				form.CancelButtonClick();

				var messages = UsageCollectorTestHelper.LoadUsageMessages(UsageFeatures.Codes.RateSelector);
				AssertEquals(
					"The action in usage properties should be 'Cancel'.",
					"Cancel",
					messages[0].UsageProperties.Value<string>(UsageProperties.Action)
				);
			}
		}

		public void TestApply_WhenRateServiceDisabledInRegistry_AndCW1RateChosen_ShouldHaveWarningAboutDisablenessInAutoratingLog()
		{
			var carrierOrg = ChooserHelper.CreateCarrierOrg("ABCD");
			var costing = Helper.NewCosting(carrierOrg);
			var rate = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "FRCDG", "FRT", 11.0, "AUD", "20GP");
			rate.TI_RH_NKCommodityCode = "GEN";

			Factory.Save();

			var consol = ChooserHelper.CreateConsol("AUSYD", "FRCDG");
			ChooserHelper.AddContainer(consol, "20GP", "GEN", "FCL");

			var logger = new ElementaryLogger();
			var dialogueService = new Mock<IDialogService>();
			var context = RatingContext.CreateForManualSelect(logger, dialogueService.Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new RateChooserForm(new RateChooserViewModel(model)))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // The following Carrier '...' could NOT be found.  Would you like to map the missing Carrier?

				// A search should occur on form load.
				form.Show();
				Application.DoEvents(); // wait for the search to complete

				var chooseContainerCommodity = model.ContainerGroups.Single();
				chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

				AssertCollectionNotContains(
					"Logger warnings should not initially contain the message about disabled Rates Service subscription.",
					"Request will not be sent to Rates Service because Rates Service subscription is disabled in the registry for SEA-FCL: AutoRating -> Rates Service -> Rates Service Subscription",
					logger.Warnings
				);

				ApplyButtonPerformClick(form);

				AssertCollectionContains(
					"Logger warnings should contain the message about disabled Rates Service subscription after applying.",
					"Request will not be sent to Rates Service because Rates Service subscription is disabled in the registry for SEA-FCL: AutoRating -> Rates Service -> Rates Service Subscription",
					logger.Warnings
				);
			}

			logger.ClearLogs();

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new RateChooserForm(new RateChooserViewModel(model)))
			{
				// A search should occur on form load.
				form.Show();
				Application.DoEvents(); // wait for the search to complete

				var chooseContainerCommodity = model.ContainerGroups.Single();
				chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

				AssertCollectionNotContains(
					"Logger warnings should not contain the message about disabled Rates Service subscription in the enabled scenario.",
					"Request will not be sent to Rates Service because Rates Service subscription is disabled in the registry for SEA-FCL: AutoRating -> Rates Service -> Rates Service Subscription",
					logger.Warnings
				);

				ApplyButtonPerformClick(form);

				AssertCollectionNotContains(
					"Logger warnings should not contain the message about disabled Rates Service subscription after applying in the enabled scenario.",
					"Request will not be sent to Rates Service because Rates Service subscription is disabled in the registry for SEA-FCL: AutoRating -> Rates Service -> Rates Service Subscription",
					logger.Warnings
				);
			}
		}

		public void TestApply_SpotRateDialog()
		{
			var (model, viewModel, apiCosting, _) = SetupModelAndViewModelWithApiRate(carrierFullName: "Maersk");
			apiCosting.BookingInfo = new BookingInfo()
			{
				BookingTerms = new BookingTerms
				{
					Items = new[]
				{
						new BookingTermItem()
						{
							Name = "Amendment Fee",
							Currency = "USD",
							Fee = 180M,
							Type = "Amendment Fee"
						}
					}
				}
			};
			// default service level, no mapping required
			apiCosting.ServiceLevel = OrgCarrierServiceLevel.StandardCode;

			using (var form = new RateChooserForm(viewModel))
			{
				var toolStrip = (ZToolStrip)form.Controls.Find("toolStrip", true)[0];
				var applyButton = toolStrip.Items.Cast<ZToolStripButton>().Single(x => x.Text == "Apply");
				applyButton.Enabled = true;

				// no carrier service level popup
				model.UpdateMappings();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);  // Would you like to send Maersk Spot Booking now to reserve the rate and the schedule?

				applyButton.PerformClick();
				AssertEquals("Would you like to send Maersk Spot Booking now to reserve the rate and the schedule?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestApply_ShowBookingRequestForm()
		{
			var (model, viewModel, apiCosting, _) = SetupModelAndViewModelWithApiRate(carrierFullName: "Maersk");
			apiCosting.BookingInfo = new BookingInfo()
			{
				BookingTerms = new BookingTerms
				{
					Items = new[]
			{
						new BookingTermItem()
						{
							Name = "Amendment Fee",
							Currency = "USD",
							Fee = 180M,
							Type = "Amendment Fee"
						}
					}
				}
			};
			// default service level, no mapping required
			apiCosting.ServiceLevel = OrgCarrierServiceLevel.StandardCode;

			using (var form = new RateChooserForm(viewModel))
			{
				var toolStrip = (ZToolStrip)form.Controls.Find("toolStrip", true)[0];
				var applyButton = toolStrip.Items.Cast<ZToolStripButton>().Single(x => x.Text == "Apply");
				applyButton.Enabled = true;

				// no carrier service level popup
				model.UpdateMappings();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);  // Would you like to send Maersk Spot Booking now to reserve the rate and the schedule?

				applyButton.PerformClick();
				AssertEquals("Would you like to send Maersk Spot Booking now to reserve the rate and the schedule?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Booking Request", ZFormModaliser.LastFormShownDialogForTest.Text);
			}
		}

		public void TestGetSelectedRate_JobHasNoContractNumberAndTheSelectedCarrierRateHasContractNumber_OtherRatesShouldBeLoadedForTheContractNumber()
		{
			RateEntry CreateRate(RatingHeader header, string container, string contractNumber)
			{
				var entry = header.AddRateEntry("ORG", "SEA", "UAIEV", "AUSYD", "STD", container, null);
				entry.TI_ContractNumber = contractNumber;
				entry.RateLines.RemoveAndDeleteAll();

				return entry;
			}

			Rate CreateCargoSphereRate(OrgHeader carrier, string container, string contractNumber)
			{
				var rate = ChooserHelper.CreateApiRate(container, carrier, string.Empty);
				rate.Origin = "UAIEV";
				rate.Destination = "AUSYD";
				rate.ContractNumber = contractNumber;
				rate.ServiceLevel = "STD";

				return rate;
			}

			var carrierOrg = ChooserHelper.CreateCarrierOrg("ABCD");
			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();

			var otherCosts = Helper.NewCosting(otherOrg);
			CreateRate(otherCosts, "20GP", null).AddFlatCharge("BAF", 100);
			CreateRate(otherCosts, "20GP", "AAA").AddFlatCharge("BAF", 200);
			CreateRate(otherCosts, "20GP", "BBB").AddFlatCharge("BAF", 300);
			CreateRate(otherCosts, "40GP", null).AddFlatCharge("CAF", 10);
			CreateRate(otherCosts, "40GP", "AAA").AddFlatCharge("CAF", 20);
			CreateRate(otherCosts, "40GP", "BBB").AddFlatCharge("CAF", 30);
			CreateRate(otherCosts, "40GP", "CCC").AddFlatCharge("CAF", 40);

			var carrierCost1 = CreateCargoSphereRate(carrierOrg, "20GP", "AAA");
			var carrierCost2 = CreateCargoSphereRate(carrierOrg, "40GP", "CCC");
			RateChooserTestHelper.AddPerContainerCharge(carrierCost1, "FRT", 1m);
			RateChooserTestHelper.AddPerContainerCharge(carrierCost2, "FRT", 2m);

			var consol = ChooserHelper.CreateConsol("UAIEV", "AUSYD");
			consol.JK_OA_ShippingLineAddress = carrierOrg.MainAddress.PK;
			consol.JK_OA_CreditorAddress = otherOrg.MainAddress.PK;
			consol.AddContainer("20GP");
			consol.AddContainer("40GP");

			Factory.Save();

			using (_Rating.Start(new LoggerDecorator()))
			using (_Rating.StartCost())
			{
				var charges = GetSelectedRate(consol, carrierCost1, carrierCost2);
				var actual = charges.Select(c => $"{c.ChargeCode.AC_Code}|{c.Amount}").ToArray();
				var expected = new[]
				{
					"FRT|1",
					"FRT|2",
					"BAF|100.00",
					"BAF|200.00",
					"BAF|300.00",
					"CAF|10.00",
					"CAF|20.00",
					"CAF|30.00",
					"CAF|40.00"
				};

				AssertContainsExactElementsInAnyOrder(
					"BAF and CAF charges should be loaded for all contract numbers since job contract number is empty",
					expected,
					actual
				);
			}
		}

		public void TestGetSelectedRate_TheSelectedCarrierRateHasDifferentContractNumber_OtherRatesShouldBeLoadedForNewContractNumber()
		{
			RateEntry CreateRate(RatingHeader header, string container, string contractNumber)
			{
				var entry = header.AddRateEntry("ORG", "SEA", "UAIEV", "AUSYD", "STD", container, null);
				entry.TI_ContractNumber = contractNumber;
				entry.RateLines.RemoveAndDeleteAll();

				return entry;
			}

			Rate CreateCargoSphereRate(OrgHeader carrier, string containerType, string contractNumber)
			{
				var rate = ChooserHelper.CreateApiRate(containerType, carrier, string.Empty);
				rate.Origin = "UAIEV";
				rate.Destination = "AUSYD";
				rate.ContractNumber = contractNumber;
				rate.ServiceLevel = "STD";

				return rate;
			}

			var carrierOrg = ChooserHelper.CreateCarrierOrg("ABCD");
			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();

			var otherCosts = Helper.NewCosting(otherOrg);
			CreateRate(otherCosts, "20GP", null).AddFlatCharge("BAF", 100);
			CreateRate(otherCosts, "20GP", "AAA").AddFlatCharge("BAF", 200);
			CreateRate(otherCosts, "20GP", "BBB").AddFlatCharge("BAF", 300);
			CreateRate(otherCosts, "40GP", null).AddFlatCharge("CAF", 10);
			CreateRate(otherCosts, "40GP", "AAA").AddFlatCharge("CAF", 20);
			CreateRate(otherCosts, "40GP", "BBB").AddFlatCharge("CAF", 30);
			CreateRate(otherCosts, "40GP", "CCC").AddFlatCharge("CAF", 40);

			var carrierCost1 = CreateCargoSphereRate(carrierOrg, "20GP", "AAA");
			var carrierCost2 = CreateCargoSphereRate(carrierOrg, "40GP", "CCC");
			RateChooserTestHelper.AddPerContainerCharge(carrierCost1, "FRT", 1m);
			RateChooserTestHelper.AddPerContainerCharge(carrierCost2, "FRT", 2m);

			var consol = ChooserHelper.CreateConsol("UAIEV", "AUSYD");
			consol.JK_OA_ShippingLineAddress = carrierOrg.MainAddress.PK;
			consol.JK_OA_CreditorAddress = otherOrg.MainAddress.PK;
			consol.AddContainer("20GP");
			consol.AddContainer("40GP");
			consol.JK_CarrierContractNumber = "BBB";
			consol.Numbers.AddNewIfNotExist("CON", "BBB");

			Factory.Save();

			using (Globals.SetIsUserInteractiveForTest(true))
			using (_Rating.Start(new LoggerDecorator()))
			using (_Rating.StartCost())
			{
				var charges = GetSelectedRate(consol, carrierCost1, carrierCost2);

				var actualCharges = charges.Select(c => $"{(string)c.ChargeCode.AC_Code}|{c.Amount}").ToArray();

				var expectedCharges = new[]
				{
					"FRT|1",
					"FRT|2",
					"BAF|300.00",
					"CAF|30.00"
				};

				AssertContainsExactElementsInAnyOrder(
					"BAF and CAF charges should be loaded from the rate with BBB contract number from the job",
					expectedCharges,
					actualCharges
				);
			}
		}

		AutoRateInfoCollection GetSelectedRate(IRatingSupporterWithAdapter job, Rate selectedRate1, Rate selectedRate2)
		{
			var dialogService = new Mock<IDialogService>();
			var ratingAdapter = job.RatingAdapter;
			var (model, logger) = GetModel(Factory, ratingAdapter);

			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { selectedRate1, selectedRate2 });
			model.AddWiseRatesForTest(response);

			var gp20 = model.ContainerGroups.ElementAt(0);
			gp20.SelectedRate = gp20.RateCollection[0];

			var gp40 = model.ContainerGroups.ElementAt(1);
			gp40.SelectedRate = gp40.RateCollection[0];

			using (var form = new RateChooserForm(new RateChooserViewModel(model), dialogService.Object))
			{
				ApplyButtonPerformClick(form);

				return model.GetSelectedRate();
			}
		}

		void AssertJobAttributesHasNotChangedToSelectedRateAttributes(IDialogService dialogService, RateChooserModel model, RateChooserViewModel viewModel, IAutoRating ratingAdapter)
		{
			var apiCosting = ChooserHelper.CreateApiRate("20GP", TransportProvider1, "GEN");
			apiCosting.ServiceLevel = OrgCarrierServiceLevel.StandardCode;
			RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT", 1000);

			var wiseRates = new[] { apiCosting };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);
			model.AddWiseRatesForTest(response);

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new RateChooserForm(viewModel, dialogService))
			{
				UnitTestUserNotification.Instance.AddYesAnswer();
				UnitTestUserNotification.Instance.AddYesAnswer();
				UnitTestUserNotification.Instance.AddYesAnswer();

				ApplyButtonPerformClick(form);

				AssertNotEquals("The origin should not be 'USLAX'.", "USLAX", ratingAdapter.Origin);
				AssertNotEquals("The destination should not be 'HKHKG'.", "HKHKG", ratingAdapter.Destination);
				AssertNotEquals("The carrier PK should not match the TransportProvider1 PK.", TransportProvider1.PK, ratingAdapter.Carrier.PK);
			}
		}

		(RateChooserModel, RateChooserViewModel, Rate, ElementaryLogger) SetupModelAndViewModelWithApiRate(
			string overwrittenAPIChargeCode = null,
			string overwrittenAPICarrierSCAC = null,
			string[] warnings = null,
			string scac = "SCAA",
			bool hasTransportLegs = false, // make it true for Spot Rate
			bool hasPenalties = false,
			bool hasBookingTerms = false,
			string carrierFullName = null,
			IEnumerable<CustomField> providerCustomFields = null)
		{
			var carrier = ChooserHelper.CreateCarrierOrg(scac, carrierFullName);
			Factory.Save();

			var consol = ChooserHelper.CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			ChooserHelper.AddContainer(consol, "20GP", "GEN", Core.Constants.ContainerModes.FCL);

			var (model, logger) = GetModel(Factory, consol.RatingAdapter);
			var apiCosting = ChooserHelper.CreateApiRate("20GP", carrier, "GEN");

			if (providerCustomFields != null)
			{
				apiCosting.ProviderCustomFields = apiCosting.ProviderCustomFields.Concat(providerCustomFields);
			}

			RateChooserTestHelper.AddPerContainerCharge(apiCosting, overwrittenAPIChargeCode ?? "FRT", 1000);

			if (hasTransportLegs || hasPenalties || hasBookingTerms)
			{
				var baseDate = Env.Time.CurrentLocalDateTime;
				apiCosting.BookingInfo = new BookingInfo();

				if (hasTransportLegs)
				{
					apiCosting.BookingInfo.Schedule = new Schedule
					{
						ScheduleDetails = new[]
						{
							new ScheduleDetail
							{
								IMONumber = "12345",
								ServiceName = "Some Service Name",
								ServiceCode = "SC12345",
								TradeLane = "FAR/EUR",
								TransitTime = new TimeSpan(12, 21, 0, 0),
								ArrivalDate = baseDate,
								DepartureDate = baseDate.AddDays(-12).AddHours(-21),
								VesselName = "MAERSK RIDE",
								VoyageNumber = "45WTG",
								Destination = "USLAX",
								Origin = "HKHKG",
								FlagCode = "UK",
								DateInfos = new[]
								{
									new ScheduleDateInfo
									{
										Code = "012",
										Name = "Some Deadline",
										Type = "Documentation",
										Date = baseDate.AddDays(-5)
									}
								}
							}
						}
					};
				}

				if (hasBookingTerms)
				{
					apiCosting.BookingInfo.BookingTerms = new BookingTerms
					{
						Items = new[]
						{
							new BookingTermItem()
							{
								Name = "Amendment Fee",
								Currency = "USD",
								Fee = 180M,
								Type = "Amendment Fee"
							}
						}
					};
				}

				if (hasPenalties)
				{
					apiCosting.BookingInfo.Penalties = new[]
					{
						new Penalty()
						{
							Type = "Some Type",
							Name = "Some Penalty",
							Currency = "USD",
							Direction = "Import",
							StartDay = 6,
							EndDay = 16,
							PerUnitRate = 12.4m
						}
					};
				}
			}

			var wiseRates = new[] { apiCosting };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);

			if (warnings != null)
			{
				response.Warnings = warnings;
			}

			if (overwrittenAPICarrierSCAC != null)
			{
				var refCarrier = response.Carriers.Single();
				refCarrier.SCACCode = overwrittenAPICarrierSCAC;
				refCarrier.Code = overwrittenAPICarrierSCAC + "CARRIER";

				response.Rates.Single().Carrier = refCarrier.Code;
			}

			model.AddWiseRatesForTest(response);

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			return (model, viewModel, apiCosting, logger);
		}

		RateChooserViewModel SetupModelAndViewModelWithCWRate(string scac = "SCAC")
		{
			var carrier = ChooserHelper.CreateCarrierOrg(scac);
			var costing = Helper.NewCosting(carrier);
			var rateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "HKHKG", "FRT", 100, container: "20GP");

			var consol = ChooserHelper.CreateConsol();
			ChooserHelper.AddContainer(consol, "20GP", "GEN", Core.Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry));

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			return viewModel;
		}

		static (RateChooserModel, ElementaryLogger) GetModel(BusinessObjectFactory factory, IAutoRating autoRating)
		{
			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, factory);
			criteria.IsManualCostSelectMode = true;

			return (new RateChooserModel(criteria, context), logger);
		}

		static void ApplyButtonPerformClick(RateChooserForm form)
		{
			var applyButton = form.Controls.Find("toolStrip", true)
				.OfType<ZToolStrip>()
				.Select(t => t.Items.OfType<ZToolStripButton>().SingleOrDefault(x => x.Text == "Apply"))
				.WhereNotNull()
				.Single();
			applyButton.Enabled = true;
			applyButton.PerformClick();
		}

		RateChooserTestHelper ChooserHelper => chooserHelper ?? (chooserHelper = new RateChooserTestHelper(Factory));
		RateChooserTestHelper chooserHelper;

		protected UsageCollectorTestHelper UsageCollectorTestHelper => usageCollectorTestHelper ?? (usageCollectorTestHelper = new UsageCollectorTestHelper(Factory));
		UsageCollectorTestHelper usageCollectorTestHelper;

		const string UpdateCarrierConfirmationMessage_SingleRoute = @"During this operation, Service Provider 'Service Provider Name' of the chosen rates will be populated as the Carrier of your Consol. 

You may need to manually adjust/remove any of the following if they are no longer valid:
	- Routing Legs
	- Containers Info
	- Consol Costing Charges

If your chosen rate is a spot rate linked with schedule, routing legs will be updated by the chosen schedule.

Do you wish to continue?";

		const string UpdateCarrierConfirmationMessage_MultiRoute = @"The Service Provider 'Service Provider Name' of the selected rate is different from the Carrier / Creditor on your Consol and the Routing Legs in current Autorating process.

For the Routing Legs you are Autorating, they will be unlinked by un-ticking 'Is Linked' checkbox and their Carrier will be replaced with 'Service Provider Name'.

You may need to review and also adjust the information of Routing Legs, Containers Info and Costing Charges if they are no longer valid.

Do you wish to continue?";
	}
}
