using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.Services;
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
using Urs.Api.Integration;
using Urs.Api.Integration.DTOs;
using Urs.Api.Integration.DTOs.Request;
using WiseRates.Tools;

namespace Enterprise.Rating.Testing.GUITests.RateSelector
{
	[TestedType(typeof(RateSelectorForm))]
	public class RateSelectorFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var carrier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "SCACCARRIER");
			var criteria = new TestRatingCriteria("AUSYD", "UAIEV", FreightMode.LSE, 10, 1, carrier);
			var ratingContext = new RatingContext();
			var form = new RateSelectorForm(criteria, ratingContext, new Mock<IDialogService>().Object);
			return form;
		}
	}

	public class RateSelectorLseFormTest : RatingTestCase
	{
		#region Apply Zero Amount

		public void TestApply_SelectedRateHasZeroChargeAndPromptUserApplyZeroCharges_WhenUserSelectYes_ThenApplyZeroChargesShouldBeTrue()
			=> TestApply_SelectedRateHasZeroChargeAndPromptUserApplyZeroCharges(promptUserApplyZeroChargesResult: ZDialogResult.Yes, expectedApplyZeroCharges: true);

		public void TestApply_SelectedRateHasZeroChargeAndPromptUserApplyZeroCharges_WhenUserSelectNo_ThenApplyZeroChargesShouldBeFalse()
			=> TestApply_SelectedRateHasZeroChargeAndPromptUserApplyZeroCharges(promptUserApplyZeroChargesResult: ZDialogResult.No, expectedApplyZeroCharges: false);

		void TestApply_SelectedRateHasZeroChargeAndPromptUserApplyZeroCharges(ZDialogResult promptUserApplyZeroChargesResult, bool expectedApplyZeroCharges)
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var rateLine = costing.CreateRate(contractNumber: "AAA").AddFlatCharge("BAF", 0);

			var consol = CreateConsol();

			Factory.Save();

			var autoRatingProxy = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var context = new RateSelectorContext()
			{
				Factory = Factory,
				CurrencyConverter = new Mock<ICurrencyConverter>().Object
			};

			var dialogServiceMock = new Mock<IDialogService>();
			dialogServiceMock
				.Setup(dialog => dialog.PromptUserApplyZeroCharges(It.Is<List<ZString>>(zeroCharges => zeroCharges.IsEquivalentTo(new List<ZString> { "BAF" }))))
				.Returns(promptUserApplyZeroChargesResult);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogServiceMock.Object))
			{
				ShowTestForm(form);

				var selectedRate = new CW1RateViewModel(context, criteria, ZGuid.Empty, ZString.Empty, new[] { rateLine }, CargoWise.Common.DisposableAction.NoAction);
				selectedRate.Calculate(criteria, criteria.Creditors?.ChargeCodeGroups);

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = selectedRate;
				form.BtnApplyClick(this, EventArgs.Empty);

				dialogServiceMock.Verify(dialog => dialog.PromptUserApplyZeroCharges(It.IsAny<List<ZString>>()), Times.Once());
				AssertEquals("Expected ApplyZeroCharges to match the value derived from the prompt result.", expectedApplyZeroCharges, form.ApplyZeroCharges);
			}

			dialogServiceMock.Reset();
		}

		public void TestApply_WhenChargeCodeIsZeroAndItIsInclusive_ShouldNotShowConfirmationMessage()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			var frtCharge = Helper.ChargeCodes["FRT"];

			var quotedBooking = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, "AIR", "LSE", ZString.Empty, consignor, consignor, consignee, TransportProvider1, "UAIEV", "AUSYD", 10m, 1m, QuotedBookingState.QuoteOnly);

			Factory.Save();

			var carrierCost = CreateCargoGuideRate(TransportProvider1, contractNumber: "AAA");
			var autoRateInfo = new AutoRateInfo(Factory) { ChargeCode = Helper.ChargeCodes["FRT"] };
			autoRateInfo.IsInclusiveCalculator = true;
			AssertEquals("Precondition: charge code is zero", 0m, autoRateInfo.Amount);
			carrierCost.AutoRateInfos.Add(autoRateInfo);

			var dialogService = new Mock<IDialogService>();
			var ratingAdapter = quotedBooking.GetFirstAdapter();
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);
			criteria.IsManualCostSelectMode = true;

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				ShowTestForm(form);

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = carrierCost;
				form.BtnApplyClick(this, EventArgs.Empty);

				dialogService.Verify(d => d.PromptUserApplyZeroCharges(It.IsAny<List<ZString>>()), Times.Never());
				Assert(true);
			}
		}

		#endregion

		public void TestApply_SelectedRateDestinationIsDifferentFromJob_ShowsConfirmationMessage()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var quotedBooking = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, "AIR", "LSE", ZString.Empty, consignor, consignor, consignee, TransportProvider1, "USLAX", "SGSIN", 10m, 1m, QuotedBookingState.QuoteOnly);
			var container = quotedBooking.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.TC_ContainerCount = 1;

			Factory.Save();

			var ratingAdapter = quotedBooking.GetFirstAdapter();
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext()))
			{
				ShowTestForm(form);

				var rate = new CargoguideRateViewModelSample
				{
					CarrierCode = TransportProvider1.OH_Code,
					CarrierOrg = TransportProvider1,
					CarrierErrorLevel = ErrorLevel.None,
					PaymentTerms = "CCX",
					Origin = "USLAX",
					Destination = "HKHKG",
					ContractNumber = "12345",
					CarrierServiceLevel = "EXP",
					CarrierServiceLevelErrorLevel = ErrorLevel.None,
					Commodities = null,
					CommodityGroupErrorLevel = ErrorLevel.None
				};

				rate.Charges.ForEach(c => c.Charges.ForEach(cc => cc.ChargeCodeErrorLevel = ErrorLevel.None));

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = rate;
				ApplyButtonPerformClick(form);

				var shownMessages = UnitTestUserNotification.Instance.LastMessage.Text;
				var expectedMessage = "During this operation, would you like to update the Destination of the One Off Quote to be the Destination 'HKHKG' of the chosen rates?";

				AssertContains("The confirmation message should contain the expected text.", expectedMessage, shownMessages);
			}
		}

		public void TestApply_SelectedRateOriginIsDifferentFromJob_ShowsConfirmationMessage()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var quotedBooking = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, "AIR", "LSE", ZString.Empty, consignor, consignor, consignee, TransportProvider1, "AUSYD", "HKHKG", 10m, 1m, QuotedBookingState.QuoteOnly);
			var container = quotedBooking.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.TC_ContainerCount = 1;

			Factory.Save();

			var ratingAdapter = quotedBooking.GetFirstAdapter();
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext()))
			{
				ShowTestForm(form);

				var rate = new CargoguideRateViewModelSample
				{
					CarrierCode = TransportProvider1.OH_Code,
					CarrierOrg = TransportProvider1,
					CarrierErrorLevel = ErrorLevel.None,
					PaymentTerms = "CCX",
					Origin = "USLAX",
					Destination = "HKHKG",
					ContractNumber = "12345",
					CarrierServiceLevel = "EXP",
					CarrierServiceLevelErrorLevel = ErrorLevel.None,
					Commodities = null,
					CommodityGroupErrorLevel = ErrorLevel.None
				};

				rate.Charges.ForEach(c => c.Charges.ForEach(cc => cc.ChargeCodeErrorLevel = ErrorLevel.None));

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = rate;
				ApplyButtonPerformClick(form);

				var shownMessages = UnitTestUserNotification.Instance.LastMessage.Text;
				var expectedMessage = "During this operation, would you like to update the Origin of the One Off Quote to be the Origin 'USLAX' of the chosen rates?";

				AssertContains(
					"The confirmation message should indicate the origin update option.",
					expectedMessage,
					shownMessages
				);
			}
		}

		public void TestPressApplyWhenServiceProviderIsNotCarrier()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var ratingContext = new RatingContext();

			using (var form = new RateSelectorForm(criteria, ratingContext, new Mock<IDialogService>().Object))
			{
				ShowTestForm(form);

				var filterControl = (RateSelectorFilterStripControl)form.Controls.Find("stripControl", true).Single();
				var rateSelectorStripObject = (RateSelectorFilterStripBusinessObject)filterControl.FilterBusinessObject;
				AssertEquals("Service Provider Filter should not have errors", false, rateSelectorStripObject.ServiceProviderFilter.HasErrors);

				var toolStrip = (ZToolStrip)form.Controls.Find("zToolStrip1", true).Single();
				var btnApply = toolStrip.Items.Find("btnApply", false).Single();
				AssertNoExceptionThrown(() => btnApply.PerformClick());
			}
		}

		public void TestPressCancelWhenFormHasError()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>(); // MiscServ.OM_RM_Airline is empty by default
			Factory.Save();
			newOrg.CompanyData.OB_IsCreditor = false;
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var ratingContext = new RatingContext();

			using (var form = new RateSelectorForm(criteria, ratingContext, new Mock<IDialogService>().Object))
			{
				ShowTestForm(form);

				var filterControl = (RateSelectorFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var rateSelectorStripObject = (RateSelectorFilterStripBusinessObject)filterControl.FilterBusinessObject;
				rateSelectorStripObject.ServiceProviderFilter.Property = Guid.NewGuid();
				AssertEquals(
					"The service provider is invalid",
					true,
					rateSelectorStripObject.ServiceProviderFilter.HasErrors
				);

				var toolStrip = (ZToolStrip)form.Controls.Find("zToolStrip1", true).Single();
				var cancelButton = toolStrip.Items.Find("btnCancel", false).Single();

				AssertNoExceptionThrown(() => cancelButton.PerformClick());
			}
		}

		public void TestSearchOnShow()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var ratingContext = new RatingContext();
			var mockProvider = new Mock<IRateViewModelsProvider>();
			mockProvider
				.Setup(m => m.IsApplicable(It.IsAny<RateSelectorFilterStripBusinessObject>()))
				.Returns(true);

			using (var form = new RateSelectorForm(criteria, ratingContext, new Mock<IDialogService>().Object, new[] { mockProvider.Object }))
			{
				AssertEquals(true, form.ShouldRunSearchOnShowingForm);
				form.Show();
				Application.DoEvents();
				mockProvider.Verify(x => x.GetRatesAsync(It.IsAny<RateSelectorFilterStripBusinessObject>(), It.IsAny<CancellationToken>()), Times.Once);
			}
		}

		public void TestPressSkipWhenFormHasError()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var ratingContext = new RatingContext();

			using (var form = new RateSelectorForm(criteria, ratingContext, new Mock<IDialogService>().Object))
			{
				ShowTestForm(form);

				var filterControl = (RateSelectorFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var rateSelectorStripObject = (RateSelectorFilterStripBusinessObject)filterControl.FilterBusinessObject;
				rateSelectorStripObject.ServiceProviderFilter.Property = Guid.NewGuid();
				AssertEquals("The service provider is invalid", true, rateSelectorStripObject.ServiceProviderFilter.HasErrors);

				var toolStrip = (ZToolStrip)form.Controls.Find("zToolStrip1", true).Single();
				var skipButton = toolStrip.Items.Find("btnSkip", false).Single();

				AssertNoExceptionThrown(() => skipButton.PerformClick());
			}
		}

		public void TestGivenMultiCosting_WhenOpenRateSelectorForm_ThenAllChargesShouldBeCalculatedCorrectly()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.MiscServ.OM_AutoratingDateFiltering = Constants.RatingDateFilterTypes.Codes.Custom;

			Helper.CreateOrganizationRatingDateConfig(carrier, ChargeCodeGroupList.Codes.Freight, JobInvoicingConsumerTypes.ForwardingConsolCode, Constants.FreightShipmentDirection.Code.All, Constants.RateMode.AIR, JobRateTypes.Codes.All, Constants.ContainerModes.Loose, "", JobDateTypes.Codes.ArrivalDate);
			Helper.CreateOrganizationRatingDateConfig(carrier, ChargeCodeGroupList.Codes.Origin, JobInvoicingConsumerTypes.ForwardingConsolCode, Constants.FreightShipmentDirection.Code.All, Constants.RateMode.AIR, JobRateTypes.Codes.All, Constants.ContainerModes.Loose, "", JobDateTypes.Codes.DepartureDate);

			var costing = Helper.NewCosting(carrier);
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.ContainerModes.Loose, "AU", "");
			var rateLine1 = rateEntry1.AddFlatCharge("FRT", 1m);
			rateLine1.TL_RateStartDate = new ZDate(2024, 03, 01);
			rateLine1.TL_RateEndDate = new ZDate(2024, 03, 03);

			var rateLine2 = rateEntry1.AddFlatCharge("FRT", 10m);
			rateLine2.TL_RateStartDate = new ZDate(2024, 03, 04);
			rateLine2.TL_RateEndDate = new ZDate(2024, 09, 04);

			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.ORG, Constants.ContainerModes.Loose, "AU", "");
			var rateLine3 = rateEntry2.AddFlatCharge("BAF", 5m);
			rateLine3.TL_RateStartDate = new ZDate(2024, 03, 01);
			rateLine3.TL_RateEndDate = new ZDate(2024, 03, 03);

			var rateLine4 = rateEntry2.AddFlatCharge("BAF", 50m);
			rateLine4.TL_RateStartDate = new ZDate(2024, 03, 04);
			rateLine4.TL_RateEndDate = new ZDate(2024, 09, 04);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2024, 03, 01);
			consol.Transports.MostInterestingTransport.JW_ETA = new ZDateTime(2024, 03, 04);
			consol.Transports[0].CarrierPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;

			Factory.Save();

			var autoRatingProxy = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var context = new RateSelectorContext()
			{
				Factory = Factory,
				CurrencyConverter = new RefCurrenciesCurrencyConverter(Factory)
			};

			using (var form = new RateSelectorForm(criteria, new RatingContext()))
			{
				form.ShouldRunSearchOnShowingForm = true;
				form.Show();
				Application.DoEvents();

				var viewModel = new CW1RateViewModel(context, criteria, ZGuid.Empty, ZString.Empty, new[] { rateLine1, rateLine2, rateLine3, rateLine4 }, CargoWise.Common.DisposableAction.NoAction);
				viewModel.Calculate(criteria, criteria.Creditors?.ChargeCodeGroups);

				var actualFreightCharges = viewModel.FreightCharges.Charges
					.Select(c => $"{c.ChargeCode}|{c.Amount}|{c.Currency}|{c.ChargeCodeError}")
					.ToArray();

				var expectedFreightCharges = new[]
				{
					"FRT|10|AUD|"
				};

				AssertContainsExactElementsInAnyOrder(
					"Freight charges should be calculated correctly",
					expectedFreightCharges,
					actualFreightCharges
				);

				var actualOtherCharges = viewModel.OtherCharges.Charges
					.Select(c => $"{c.ChargeCode}|{c.Amount}|{c.Currency}|{c.ChargeCodeError}")
					.ToArray();

				var expectedOtherCharges = new[]
				{
					"BAF|50|AUD|"
				};

				AssertContainsExactElementsInAnyOrder(
					"Other charges should be calculated correctly",
					expectedOtherCharges,
					actualOtherCharges
				);
			}
		}

		public void TestGivenSelectedRateInRateSelectorForm_WhenApply_ThenPopulateEffectiveDateToAutoratingDate()
		{
			var costing = Helper.NewCosting(null);
			var rateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 100m);
			rateEntry.TI_RateStartDate = new ZDate(2022, 08, 15);
			rateEntry.TI_RateEndDate = new ZDate(2022, 10, 15);
			rateEntry.TI_OH_TransportProvider = TransportProvider2.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.Transports[0].JW_ETD = new ZDateTime(2022, 08, 16);
			consol.Transports[0].JW_ETA = new ZDateTime(2022, 08, 17);
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_AWBServiceLevel = "STD";

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var context = new RateSelectorContext()
			{
				Factory = Factory,
				CurrencyConverter = new RefCurrenciesCurrencyConverter(Factory)
			};

			var cw1RateViewModel = new CW1RateViewModel(context, criteria, ZGuid.Empty, ZString.Empty, rateEntry.RateLines.Cast<RateLine>(), CargoWise.Common.DisposableAction.NoAction);
			cw1RateViewModel.Calculate(criteria, criteria.Creditors?.ChargeCodeGroups);

			var dialogService = new Mock<IDialogService>();
			dialogService.Setup(d => d.PromptToApplyCarrierToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				ShowTestForm(form);
				var filterControl = (RateSelectorFilterStripControl)form.Controls.Find("stripControl", true).Single();
				var rateSelectorStripObject = (RateSelectorFilterStripBusinessObject)filterControl.FilterBusinessObject;
				AssertEquals("EffectiveOn", new ZDateTime(2022, 08, 16), rateSelectorStripObject.EffectiveOnFilter.Property1);

				rateSelectorStripObject.EffectiveOnFilter.Property1 = new ZDateTime(2022, 09, 15);
				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = cw1RateViewModel;

				var toolStrip = (ZToolStrip)form.Controls.Find("zToolStrip1", true).Single();
				var btnApply = toolStrip.Items.Find("btnApply", false).Single();
				btnApply.PerformClick();

				AssertEquals("Autorating date", new ZDateTime(2022, 09, 15), consol.AutoratingDate);
				AssertEquals("DialogResult should be OK", DialogResult.OK, form.DialogResult);
			}
		}

		public void TestApply_CarrierIsDifferentToAJob_PopulateRateDataBackToJobIfUserAgrees()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_AWBServiceLevel = "STD";

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var dialogService = new Mock<IDialogService>();
			dialogService.Setup(d => d.PromptToApplyCarrierToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
			dialogService.Setup(d => d.PromptToApplyOriginToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(ZDialogResult.Yes);
			dialogService.Setup(d => d.PromptToApplyDestinationToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(ZDialogResult.Yes);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				ShowTestForm(form);

				var rate = new CargoguideRateViewModelSample
				{
					CarrierCode = TransportProvider2.OH_Code,
					CarrierOrg = TransportProvider2,
					CarrierErrorLevel = ErrorLevel.None,
					PaymentTerms = "CCX",
					Origin = "UAIEV",
					Destination = "SGSIN",
					ContractNumber = "12345",
					CarrierServiceLevel = "EXP",
					CarrierServiceLevelErrorLevel = ErrorLevel.None,
					Commodities = null,
					CommodityGroupErrorLevel = ErrorLevel.None
				};

				rate.Charges.ForEach(c => c.Charges.ForEach(cc => cc.ChargeCodeErrorLevel = ErrorLevel.None));

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = rate;
				ApplyButtonPerformClick(form);

				AssertEquals("The load port should match the provided origin.", "UAIEV", consol.JK_RL_NKLoadPort);
				AssertEquals("The discharge port should match the provided destination.", "SGSIN", consol.JK_RL_NKDischargePort);
				AssertEquals("The shipping line address should match the carrier's main address.", TransportProvider2.MainAddress.PK, consol.JK_OA_ShippingLineAddress);
				AssertEquals("The payment terms should be updated to the provided value.", "CCX", consol.JK_PrepaidCollect);
				AssertEquals("The service level should be updated to the provided value.", "EXP", consol.JK_AWBServiceLevel);
				AssertEquals("The form's dialog result should be OK.", DialogResult.OK, form.DialogResult);
			}
		}

		public void TestApply_CW1StandardRate_CarrierIsDifferentToAJob_PopulateRateDataBackToJobIfUserAgrees()
		{
			var costing = Helper.NewCosting(null);
			var rateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 100m);
			rateEntry.TI_OH_TransportProvider = TransportProvider2.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_AWBServiceLevel = "STD";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort;
			shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort;

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var context = new RateSelectorContext()
			{
				Factory = Factory,
				CurrencyConverter = new RefCurrenciesCurrencyConverter(Factory)
			};

			var cw1RateViewModel = new CW1RateViewModel(context, criteria, ZGuid.Empty, ZString.Empty, rateEntry.RateLines.Cast<RateLine>(), CargoWise.Common.DisposableAction.NoAction);
			cw1RateViewModel.Calculate(criteria, criteria.Creditors?.ChargeCodeGroups);

			var dialogService = new Mock<IDialogService>();
			dialogService.Setup(d => d.PromptToApplyCarrierToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				ShowTestForm(form);

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = cw1RateViewModel;

				var toolStrip = (ZToolStrip)form.Controls.Find("zToolStrip1", true).Single();
				var btnApply = toolStrip.Items.Find("btnApply", false).Single();
				btnApply.PerformClick();

				AssertEquals("The shipping line address should match the transport provider's main address.",
					TransportProvider2.MainAddress.PK,
					consol.JK_OA_ShippingLineAddress);

				AssertEquals("The dialog result should be OK after applying the rate.",
					DialogResult.OK,
					form.DialogResult);
			}

			dialogService.VerifyAll();
		}

		public void TestApply_CW1Rate_ServiceProviderIsDifferentToAJob_PopulateRateDataBackToJobIfUserAgrees()
		{
			var costing = Helper.NewCosting(TransportProvider2);
			var rateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 100m);

			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_AWBServiceLevel = "STD";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort;
			shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort;

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var context = new RateSelectorContext()
			{
				Factory = Factory,
				CurrencyConverter = new RefCurrenciesCurrencyConverter(Factory)
			};

			var cw1RateViewModel = new CW1RateViewModel(context, criteria, ZGuid.Empty, ZString.Empty, rateEntry.RateLines.Cast<RateLine>(), CargoWise.Common.DisposableAction.NoAction);
			cw1RateViewModel.Calculate(criteria, criteria.Creditors?.ChargeCodeGroups);

			var dialogService = new Mock<IDialogService>();
			dialogService.Setup(d => d.PromptToApplyCarrierToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				ShowTestForm(form);

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = cw1RateViewModel;

				var toolStrip = (ZToolStrip)form.Controls.Find("zToolStrip1", true).Single();
				var btnApply = toolStrip.Items.Find("btnApply", false).Single();
				btnApply.PerformClick();

				AssertEquals("The ShippingLineAddress of the consol should be updated with the rate's provider address.",
					TransportProvider2.MainAddress.PK, consol.JK_OA_ShippingLineAddress);

				AssertEquals("The dialog result of the form should be OK after applying the rate.",
					DialogResult.OK, form.DialogResult);
			}

			dialogService.VerifyAll();
		}

		public void TestApply_CarrierIsDifferentToAJob_DoNothingWhenUserDoesntAgreeToApplyRateData()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_AWBServiceLevel = "STD";

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			criteria.UpdateCarrierConfirmationIsNeeded(TransportProvider2.OH_Code, out var message);

			var dialogService = new Mock<IDialogService>();
			dialogService.Setup(d => d.PromptToApplyCarrierToJob(criteria.HumanReadableName(), message)).Returns(false);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				ShowTestForm(form);

				var rate = new CargoguideRateViewModelSample
				{
					CarrierCode = TransportProvider2.OH_Code,
					CarrierOrg = TransportProvider2,
					CarrierErrorLevel = ErrorLevel.None,
					PaymentTerms = "CCX",
					Origin = "UAIEV",
					Destination = "SGSIN",
					ContractNumber = "12345",
					CarrierServiceLevel = "EXP",
					CarrierServiceLevelErrorLevel = ErrorLevel.None,
					CommodityGroupErrorLevel = ErrorLevel.None
				};

				rate.Charges.ForEach(c => c.Charges.ForEach(cc => cc.ChargeCodeErrorLevel = ErrorLevel.None));

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = rate;
				ApplyButtonPerformClick(form);

				AssertEquals("Unexpected load port value.", "AUSYD", consol.JK_RL_NKLoadPort);
				AssertEquals("Unexpected discharge port value.", "USLAX", consol.JK_RL_NKDischargePort);
				AssertEquals("Unexpected shipping line address value.", TransportProvider1.MainAddress.PK, consol.JK_OA_ShippingLineAddress);
				AssertEquals("Unexpected prepaid/collect value.", "PPD", consol.JK_PrepaidCollect);
				AssertEquals("Unexpected AWB service level value.", "STD", consol.JK_AWBServiceLevel);
				AssertEquals("Unexpected dialog result value.", DialogResult.None, form.DialogResult);
			}
		}

		public void TestApply_ConsolCarrierIsEmpty_ApplyRateWithoutAsking_SingleRoute()
		{
			AssertApply_ConsolCarrierIsEmpty_ApplyRateWithoutAsking(
				isMultiRouteSupported: false,
				unexpectedMessage: $@"During this operation, Service Provider '{TransportProvider2.OH_FullName}' of the chosen rates will be populated as the Carrier of your Consol. 

You may need to manually adjust/remove any of the following if they are no longer valid:
	- Routing Legs
	- Containers Info
	- Consol Costing Charges

If your chosen rate is a spot rate linked with schedule, routing legs will be updated by the chosen schedule.

Do you wish to continue?");
		}

		public void TestApply_ConsolCarrierIsEmpty_ApplyRateWithoutAsking_MultiRoute()
		{
			AssertApply_ConsolCarrierIsEmpty_ApplyRateWithoutAsking(
				isMultiRouteSupported: true,
				unexpectedMessage: $@"The Service Provider '{TransportProvider2.OH_FullName}' of the selected rate is different from the Carrier / Creditor on your Consol and the Routing Legs in current Autorating process.

For the Routing Legs you are Autorating, they will be unlinked by un-ticking 'Is Linked' checkbox and their Carrier will be replaced with 'Service Provider Name'.

You may need to review and also adjust the information of Routing Legs, Containers Info and Costing Charges if they are no longer valid.

Do you wish to continue?");
		}

		void AssertApply_ConsolCarrierIsEmpty_ApplyRateWithoutAsking(bool isMultiRouteSupported, string unexpectedMessage)
		{
			RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isMultiRouteSupported);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_RL_NKLoadPort = "UAIEV";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_AWBServiceLevel = "STD";
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

			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var dialogService = new Mock<IDialogService>();
			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				ShowTestForm(form);

				AssertEquals("Precondition: consol has empty Carrier field", ZGuid.Empty, consol.ShippingLinePK);

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = CreateCargoGuideRate(TransportProvider2);
				ApplyButtonPerformClick(form);

				dialogService.Verify(d => d.PromptToApplyCarrierToJob(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

				AssertEquals("Consol's carrier should be updated", TransportProvider2.PK, consol.ShippingLinePK);

				if (isMultiRouteSupported)
				{
					AssertEquals("Related leg's carrier should be updated", TransportProvider2.PK, routes[0].Carrier.PK);
					AssertNull("Unrelated leg's carrier should not be updated", routes[1].Carrier);
				}
			}
		}

		public void TestApply_QuickBookingCarrierIsEmpty_ApplyRateWithoutAsking()
		{
			var quickBooking = BaseRatingIntegrationTest.CreateQuotedBooking(
				Factory,
				Constants.TransportModes.Air,
				Constants.ContainerModes.Loose,
				ZString.Empty,
				Consignor,
				Consignor,
				Consignee,
				null,
				"UAIEV",
				"AUSYD",
				10m,
				1m,
				QuotedBookingState.BookingOnly
			);
			quickBooking.StartDate = ZDate.Today;
			quickBooking.EndDate = ZDate.Today;

			var autoRatingProxy = new AutoRatingProxy(quickBooking.GetRatingAdapters().Single());
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var dialogService = new Mock<IDialogService>();
			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				ShowTestForm(form);

				AssertEquals(
					"Precondition: Carrier field is empty",
					ZGuid.Empty,
					quickBooking.OH_Carrier
				);

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = CreateCargoGuideRate(TransportProvider2);
				ApplyButtonPerformClick(form);

				var shownMessages = UnitTestUserNotification.Instance.PreviousMessages.Select(m => m.Text);
				AssertCollectionNotContains(
					$@"During this operation, Service Provider '{TransportProvider2.OH_FullName}' of the chosen rates will be populated as the Carrier of your Quick Booking. 

		You may need to manually adjust/remove Charges if they are no longer valid.

		Do you wish to continue?",
					shownMessages
				);

				dialogService.Verify(
					d => d.PromptToApplyCarrierToJob(It.IsAny<string>(), It.IsAny<string>()),
					Times.Never
				);

				AssertEquals("Carrier field should be updated", TransportProvider2.PK, quickBooking.OH_Carrier);
			}
		}

		public void TestApply_CommodityDoesNotPresent_ApplyRateWithoutAsking()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var dialogService = new Mock<IDialogService>();

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				ShowTestForm(form);

				var rate = new CargoguideRateViewModelSample
				{
					CarrierCode = TransportProvider1.OH_Code,
					CarrierOrg = TransportProvider1,
					PaymentTerms = "PPD",
					Origin = "AUSYD",
					Destination = "USLAX",
					CarrierServiceLevelErrorLevel = ErrorLevel.None,
					Commodities = null,
					CommodityGroupErrorLevel = ErrorLevel.None
				};

				rate.Charges.ForEach(c => c.Charges.ForEach(cc => cc.ChargeCodeErrorLevel = ErrorLevel.None));

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = rate;
				ApplyButtonPerformClick(form);

				AssertEquals("Dialog result should be OK after applying the rate.", DialogResult.OK, form.DialogResult);

				dialogService.Verify(
					d => d.PromptToApplyRateWithDifferentUniversalCommodityGroups(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()),
					Times.Never());
			}
		}

		public void TestApply_CommodityIsEmpty_ApplyCargoguideRate_WithoutAsking()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var dialogService = new Mock<IDialogService>();

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				ShowTestForm(form);

				var rate = new CargoguideRateViewModelSample
				{
					CarrierCode = TransportProvider1.OH_Code,
					CarrierOrg = TransportProvider1,
					PaymentTerms = "PPD",
					Origin = "AUSYD",
					Destination = "USLAX",
					CarrierServiceLevelErrorLevel = ErrorLevel.None,
					Commodities = string.Empty,
					CommodityGroupErrorLevel = ErrorLevel.None
				};

				rate.Charges.ForEach(c => c.Charges.ForEach(cc => cc.ChargeCodeErrorLevel = ErrorLevel.None));

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = rate;
				ApplyButtonPerformClick(form);

				AssertEquals("Dialog result should be OK after applying the rate.", DialogResult.OK, form.DialogResult);

				dialogService.Verify(
					d => d.PromptToApplyRateWithDifferentUniversalCommodityGroups(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()),
					Times.Never());
				dialogService.Verify(
					d => d.PromptToApplyRateWithDifferentCommodityCode(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()),
					Times.Never());
			}
		}

		public void TestApply_UniversalCommodityGroupMapped_ApplyCargoguideRate_WithoutAsking()
		{
			var commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity.RH_Code = "XXXX";
			commodity.RH_UniversalCommodityGroup = "CMMGROUP";
			Factory.Save();

			var containerTypeLD1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-1");

			var consol = CreateConsol();
			var container = consol.Containers.AddNew();
			container.JC_RH_NKContainerCommodityCode = "XXXX";
			container.JC_ContainerMode = Constants.ContainerModes.ULD;
			container.JC_ContainerCount = 1;
			container.JC_RC = containerTypeLD1.PK;

			var dialogService = new Mock<IDialogService>();

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				ShowTestForm(form);

				var rate = CreateCargoGuideRate();
				rate.CommodityGroups = new List<string>() { "CMMGROUP" };
				rate.CommodityGroupErrorLevel = ErrorLevel.None;

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = rate;
				ApplyButtonPerformClick(form);

				AssertEquals(
					"Expected form dialog result to be OK after applying rate",
					DialogResult.OK,
					form.DialogResult
				);

				dialogService.Verify(
					d => d.PromptToApplyRateWithDifferentUniversalCommodityGroups(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()),
					Times.Never()
				);
				dialogService.Verify(
					d => d.PromptToApplyRateWithDifferentCommodityCode(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()),
					Times.Never()
				);
			}
		}

		public void TestApply_UniversalCommodityGroupNotMapped_JobHasNoCommodity_ApplyCargoguideRate_WithoutAsking()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var dialogService = new Mock<IDialogService>();

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				ShowTestForm(form);

				var rate = CreateCargoGuideRate();
				rate.CommodityGroups = new string[] { "UCG1" };
				rate.CommodityGroupErrorLevel = ErrorLevel.None;

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = rate;
				ApplyButtonPerformClick(form);

				dialogService.Verify(
					d => d.PromptToApplyRateWithDifferentUniversalCommodityGroups(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()),
					Times.Never);
			}

			Assert(true);
		}

		public void TestApply_UniversalCommodityGroupNotMapped_JobCommoditiesNotMatchRate_ShouldShowPopup()
		{
			var commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity.RH_Code = "XXXX";
			Factory.Save();

			var containerTypeLD1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-1");

			var consol = CreateConsol();
			var container = consol.Containers.AddNew();
			container.JC_RH_NKContainerCommodityCode = "XXXX";
			container.JC_ContainerMode = Constants.ContainerModes.ULD;
			container.JC_ContainerCount = 1;
			container.JC_RC = containerTypeLD1.PK;

			var dialogService = new Mock<IDialogService>();

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				ShowTestForm(form);

				var rate = CreateCargoGuideRate();
				rate.CommodityGroupErrorLevel = ErrorLevel.None;
				rate.CommodityGroups = new List<string> { "Personal effects" };
				rate.Charges.ForEach(c => c.Charges.ForEach(cc => cc.ChargeCodeErrorLevel = ErrorLevel.None));

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = rate;
				ApplyButtonPerformClick(form);

				dialogService.Verify(
					d => d.PromptToApplyRateWithDifferentUniversalCommodityGroups(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()),
					Times.Once());
			}

			Assert(true);
		}
		public void TestApply_CommodityGroupMatchesJobButNotCommodityCode_ApplyRateCW1Rate_WithoutAsking()
		{
			var commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity.RH_Code = "XXXX";
			commodity.RH_UniversalCommodityGroup = null;
			commodity.RH_IsFlammable = true; // FLAM

			commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity.RH_Code = "YYYY";
			commodity.RH_UniversalCommodityGroup = "CMMGROUP2";
			Factory.Save();

			var containerTypeLD1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-1");

			var consol = CreateConsol();
			var container = consol.Containers.AddNew();
			container.JC_RH_NKContainerCommodityCode = "XXXX";
			container.JC_ContainerMode = Constants.ContainerModes.ULD;
			container.JC_ContainerCount = 1;
			container.JC_RC = containerTypeLD1.PK;

			container = consol.Containers.AddNew();
			container.JC_RH_NKContainerCommodityCode = "YYYY";
			container.JC_ContainerMode = Constants.ContainerModes.ULD;
			container.JC_ContainerCount = 1;
			container.JC_RC = containerTypeLD1.PK;

			var dialogService = new Mock<IDialogService>();

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				ShowTestForm(form);

				var rate = CreateCW1Rate();
				rate.Commodities = "CMM";
				rate.CommodityGroups = new List<string> { "FLAM", "PERS" };
				rate.Charges.ForEach(c => c.Charges.ForEach(cc => cc.ChargeCodeErrorLevel = ErrorLevel.None));

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = rate;
				ApplyButtonPerformClick(form);

				dialogService.Verify(
					d => d.PromptToApplyRateWithDifferentUniversalCommodityGroups(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()),
					Times.Never());
			}

			Assert(true);
		}

		public void TestApply_UniversalCommodityGroupMappedToOtherCode_ShouldShowPopup()
		{
			var commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity.RH_Code = "XXXX";
			commodity.RH_UniversalCommodityGroup = "CMMGROUP";
			Factory.Save();

			var containerTypeLD1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-1");

			var consol = CreateConsol();
			var container = consol.Containers.AddNew();
			container.JC_RH_NKContainerCommodityCode = "XXXX";
			container.JC_ContainerMode = Constants.ContainerModes.ULD;
			container.JC_ContainerCount = 1;
			container.JC_RC = containerTypeLD1.PK;

			var dialogService = new Mock<IDialogService>();

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				ShowTestForm(form);

				var rate = CreateCargoGuideRate();
				rate.Commodities = "???";
				rate.CommodityGroups = new List<string> { "Personal effects" };
				rate.CommodityGroupErrorLevel = ErrorLevel.None;

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = rate;
				ApplyButtonPerformClick(form);

				dialogService.Verify(
					d => d.PromptToApplyRateWithDifferentCommodityCode(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()),
					Times.Never());
				dialogService.Verify(
					d => d.PromptToApplyRateWithDifferentUniversalCommodityGroups(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()),
					Times.Once());
			}

			Assert(true);
		}

		public void TestApply_UniversalCommodityGroupMappedToOtherCode_NoContainer_ShouldNotShowPopup()
		{
			var commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity.RH_Code = "XXXX";
			commodity.RH_UniversalCommodityGroup = "CMMGROUP";
			Factory.Save();

			var containerTypeLD1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-1");
			var consol = CreateConsol();

			var dialogService = new Mock<IDialogService>();

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				form.ShouldRunSearchOnShowingForm = false;
				form.Show();
				Application.DoEvents();

				var rate = CreateCargoGuideRate();
				rate.Commodities = "Personal effects";
				rate.CommodityGroups = new List<string> { "Personal effects" };
				rate.CommodityGroupErrorLevel = ErrorLevel.None;

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = rate;
				ApplyButtonPerformClick(form);

				dialogService.Verify(
					d => d.PromptToApplyRateWithDifferentCommodityCode(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()),
					Times.Never());
				dialogService.Verify(
					d => d.PromptToApplyRateWithDifferentUniversalCommodityGroups(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()),
					Times.Never());
			}

			Assert(true);
		}

		public void TestApply_UniversalCommodityGroupMappedToOtherCode_SkipRateWhenUserChoosesNoFromPopup()
		{
			var commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity.RH_Code = "XXXX";
			commodity.RH_UniversalCommodityGroup = "CMMGROUP";
			Factory.Save();

			var containerTypeLD1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-1");

			var consol = CreateConsol();
			var container = consol.Containers.AddNew();
			container.JC_RH_NKContainerCommodityCode = "XXXX";
			container.JC_ContainerMode = Constants.ContainerModes.ULD;
			container.JC_ContainerCount = 1;
			container.JC_RC = containerTypeLD1.PK;

			var dialogService = new Mock<IDialogService>();
			dialogService
				.Setup(d => d.PromptToApplyRateWithDifferentUniversalCommodityGroups(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
				.Returns(false);

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				ShowTestForm(form);

				var rate = CreateCargoGuideRate();
				rate.CommodityGroupErrorLevel = ErrorLevel.None;
				rate.CommodityGroups = new List<string> { "Personal effects" };
				rate.Charges.ForEach(c => c.Charges.ForEach(cc => cc.ChargeCodeErrorLevel = ErrorLevel.None));

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = rate;
				ApplyButtonPerformClick(form);

				AssertEquals("Form dialog result should remain unchanged.", DialogResult.None, form.DialogResult);
			}
		}

		public void TestApply_UniversalCommodityGroupMappedToOtherCode_DoNothingWhenUserChoosesCancelFromPopup()
		{
			var commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity.RH_Code = "XXXX";
			commodity.RH_UniversalCommodityGroup = "CMMGROUP";
			Factory.Save();

			var containerTypeLD1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-1");

			var consol = CreateConsol();
			var container = consol.Containers.AddNew();
			container.JC_RH_NKContainerCommodityCode = "XXXX";
			container.JC_ContainerMode = Constants.ContainerModes.ULD;
			container.JC_ContainerCount = 1;
			container.JC_RC = containerTypeLD1.PK;

			var dialogService = new Mock<IDialogService>();
			dialogService
				.Setup(d => d.PromptToApplyRateWithDifferentUniversalCommodityGroups(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
				.Returns(false);

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				ShowTestForm(form);

				var rate = CreateCargoGuideRate();
				rate.CommodityGroupErrorLevel = ErrorLevel.None;
				rate.CommodityGroups = new List<string> { "Personal effects" };

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = rate;
				ApplyButtonPerformClick(form);

				AssertEquals("Dialog result should indicate no action taken", DialogResult.None, form.DialogResult);
			}
		}

		public void TestApply_UniversalCommodityGroupMappedToOtherCode_ApplyRateWhenUserChoosesYesFromPopup()
		{
			var commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity.RH_Code = "XXXX";
			commodity.RH_UniversalCommodityGroup = "CMMGROUP";
			Factory.Save();

			var containerTypeLD1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-1");

			var consol = CreateConsol();
			var container = consol.Containers.AddNew();
			container.JC_RH_NKContainerCommodityCode = "XXXX";
			container.JC_ContainerMode = Constants.ContainerModes.ULD;
			container.JC_ContainerCount = 1;
			container.JC_RC = containerTypeLD1.PK;

			var dialogService = new Mock<IDialogService>();
			dialogService
				.Setup(d => d.PromptToApplyRateWithDifferentUniversalCommodityGroups(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
				.Returns(true);

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				ShowTestForm(form);

				var rate = CreateCargoGuideRate();
				rate.CommodityGroupErrorLevel = ErrorLevel.None;
				rate.CommodityGroups = new List<string> { "Personal effects" };
				rate.Charges.ForEach(c => c.Charges.ForEach(cc => cc.ChargeCodeErrorLevel = ErrorLevel.None));

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = rate;
				ApplyButtonPerformClick(form);

				AssertEquals("Form dialog result should be OK", DialogResult.OK, form.DialogResult);
			}
		}

		public void TestApply_CommodityCodeNotMatchedToContainer_ShouldShowPopup()
		{
			var commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity.RH_Code = "XXXX";
			Factory.Save();

			var containerTypeLD1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-1");

			var consol = CreateConsol();
			var container = consol.Containers.AddNew();
			container.JC_RH_NKContainerCommodityCode = "XXXX";
			container.JC_ContainerMode = Constants.ContainerModes.ULD;
			container.JC_ContainerCount = 1;
			container.JC_RC = containerTypeLD1.PK;

			var dialogService = new Mock<IDialogService>();

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				form.ShouldRunSearchOnShowingForm = false;
				form.Show();
				Application.DoEvents();

				var rate = CreateCW1Rate();
				rate.Commodities = "YYYY";
				rate.Charges.ForEach(c => c.Charges.ForEach(cc => cc.ChargeCodeErrorLevel = ErrorLevel.None));

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = rate;
				ApplyButtonPerformClick(form);

				dialogService.Verify(
					d => d.PromptToApplyRateWithDifferentCommodityCode(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()),
					Times.Once());
			}

			Assert(true);
		}

		public void TestApply_CommodityCodeMatchedToContainer_ApplyRateWithoutAsking()
		{
			var commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity.RH_Code = "XXXX";
			var commodity2 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity2.RH_Code = "YYYY";
			Factory.Save();

			var containerTypeLD1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-1");

			var consol = CreateConsol();
			var container = consol.Containers.AddNew();
			container.JC_ContainerMode = Constants.ContainerModes.ULD;
			container.JC_ContainerCount = 1;
			container.JC_RH_NKContainerCommodityCode = "YYYY";
			container.JC_RC = containerTypeLD1.PK;

			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_RH_NKCommodityCode = "XXXX";

			container.PackLines.Add(packLine);
			var dialogService = new Mock<IDialogService>();

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				form.ShouldRunSearchOnShowingForm = false;
				form.Show();
				Application.DoEvents();

				var rate = CreateCW1Rate();
				rate.Commodities = "YYYY";
				rate.Charges.ForEach(c => c.Charges.ForEach(cc => cc.ChargeCodeErrorLevel = ErrorLevel.None));

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = rate;
				ApplyButtonPerformClick(form);

				dialogService.Verify(
					d => d.PromptToApplyRateWithDifferentCommodityCode(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()),
					Times.Never());
				dialogService.Verify(
					d => d.PromptToApplyRateWithDifferentUniversalCommodityGroups(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()),
					Times.Never());
			}
			Assert(true);
		}

		public void TestApply_CommodityCodeMappedToOtherCode_SkipRateWhenUserChoosesNoFromPopups()
		{
			var commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity.RH_Code = "XXXX";
			commodity.RH_UniversalCommodityGroup = "CMMGROUP";
			Factory.Save();

			var containerTypeLD1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-1");

			var consol = CreateConsol();
			var container = consol.Containers.AddNew();
			container.JC_RH_NKContainerCommodityCode = "XXXX";
			container.JC_ContainerMode = Constants.ContainerModes.ULD;
			container.JC_ContainerCount = 1;
			container.JC_RC = containerTypeLD1.PK;

			var dialogService = new Mock<IDialogService>();
			dialogService
				.Setup(d => d.PromptToApplyRateWithDifferentCommodityCode(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
				.Returns(false);

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				form.ShouldRunSearchOnShowingForm = false;
				form.Show();
				Application.DoEvents();

				var rate = CreateCW1Rate();
				rate.Commodities = "Random";
				rate.Charges.ForEach(c => c.Charges.ForEach(cc => cc.ChargeCodeErrorLevel = ErrorLevel.None));

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = rate;
				ApplyButtonPerformClick(form);

				AssertEquals("Form dialog result should remain None when user chooses no from popups.", DialogResult.None, form.DialogResult);
			}
		}

		public void TestApply_CommodityCodeMappedToOtherCode_ApplyRateWhenUserChoosesYesFromPopups()
		{
			var commodity1 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity1.RH_Code = "XXXX";
			commodity1.RH_UniversalCommodityGroup = "CMMGROUP1";
			var commodity2 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity2.RH_Code = "YYYY";
			commodity2.RH_UniversalCommodityGroup = "CMMGROUP2";
			Factory.Save();

			var containerTypeLD1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-1");

			var consol = CreateConsol();
			var container = consol.Containers.AddNew();
			container.JC_RH_NKContainerCommodityCode = "XXXX";
			container.JC_ContainerMode = Constants.ContainerModes.ULD;
			container.JC_ContainerCount = 1;
			container.JC_RC = containerTypeLD1.PK;

			var dialogService = new Mock<IDialogService>();
			dialogService
				.Setup(d => d.PromptToApplyRateWithDifferentCommodityCode(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
				.Returns(true);

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				form.ShouldRunSearchOnShowingForm = false;
				form.Show();
				Application.DoEvents();

				var rate = CreateCW1Rate();
				rate.Commodities = "YYYY";
				rate.Charges.ForEach(c => c.Charges.ForEach(cc => cc.ChargeCodeErrorLevel = ErrorLevel.None));

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = rate;
				ApplyButtonPerformClick(form);

				AssertEquals("DialogResult should be OK when a valid rate is applied", DialogResult.OK, form.DialogResult);
			}
		}

		public void TestApply_SelectedRateAttributesAreDifferentFromJob_UserDoesNotConfirmsPopulationConfirmations_NothingHappens()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var quotedBooking = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, "AIR", "LSE", ZString.Empty, consignor, consignor, consignee, TransportProvider1, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			var ratingAdapter = quotedBooking.GetFirstAdapter();
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var dialogService = new Mock<IDialogService>();
			dialogService.Setup(d => d.PromptToApplyDestinationToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(ZDialogResult.Cancel);
			dialogService.Setup(d => d.PromptToApplyOriginToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(ZDialogResult.Yes);
			dialogService.Setup(d => d.PromptToApplyCarrierToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

			AssertJobAttributesHasNotChangedToSelectedRateAttributes(dialogService.Object, criteria, ratingAdapter);

			dialogService.Setup(d => d.PromptToApplyCarrierToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
			dialogService.Setup(d => d.PromptToApplyOriginToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(ZDialogResult.Yes);
			dialogService.Setup(d => d.PromptToApplyDestinationToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(ZDialogResult.Yes);

			AssertJobAttributesHasNotChangedToSelectedRateAttributes(dialogService.Object, criteria, ratingAdapter);

			dialogService.Setup(d => d.PromptToApplyOriginToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(ZDialogResult.Cancel);
			dialogService.Setup(d => d.PromptToApplyCarrierToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
			dialogService.Setup(d => d.PromptToApplyDestinationToJob(It.IsAny<string>(), It.IsAny<string>())).Returns(ZDialogResult.Yes);

			AssertJobAttributesHasNotChangedToSelectedRateAttributes(dialogService.Object, criteria, ratingAdapter);
		}

		public void TestApply_CW1StandardCost_ApplyRateWithoutErrorReporting()
		{
			var standardCosting = Helper.NewCosting(null);
			var rateEntry = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "UAIEV", "FRT", 100, "AUD");
			var rateLine = rateEntry.RateLines[0];
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "UAIEV";

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext()))
			{
				ShowTestForm(form);

				var rate = new CW1RateViewModel();

				var ratingCriteria = new RatingCriteria(null, Factory);
				ratingCriteria.ValuesCanBeSet = true;
				ratingCriteria.AutoRatedFor = new Collection<IBusiness>() { rateEntry };

				var parameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));

				var calculationResult = new CalculationResult(
					rateLine,
					new CalculatorOutput(
						new List<PaymentBasis>
						{
							parameters.Criteria.CreatePaymentBasis(RateInfo.CreateFLT(100, "AUD"), new Quantity(5, "KG"))
						}));
				var autoRateInfo = new AutoRateInfo(calculationResult, parameters, Factory);
				var rateSelectorContext = new RateSelectorContext()
				{
					Factory = Factory,
					Logger = new MemoryLogger(),
					CurrencyConverter = new RefCurrenciesCurrencyConverter(Factory),
				};

				rate.FreightCharges.Add(new CW1ChargeViewModel(autoRateInfo, rateSelectorContext));

				AssertNull(nameof(rate.CarrierOrg), rate.CarrierOrg);

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = rate;
				ApplyButtonPerformClick(form);

				AssertNullOrEmpty(ErrorReporter.LastMessageReported);

				var rateInfoCollection = form.SelectedRates;
				var rateInfo = rateInfoCollection.Single();
				AssertEquals("Expected charge code to be 'FRT'.", "FRT", rateInfo.ChargeCode.AC_Code);
				AssertEquals("Expected amount to be 100.", 100m, rateInfo.Amount);
			}
		}

		public void TestApply_ShouldReportUsage_WithRatesDetails()
		{
			var consol = CreateConsol();
			var autoRatingProxy = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), new Mock<IDialogService>().Object))
			{
				var rates = new RateViewModel[]
				{
					CreateCargoGuideRate(),
					CreateCargoGuideRate(contractNumber: "1"),
					CreateCargoGuideRate(contractNumber: "2"),
					CreateCW1Rate(contractNumber: "A"),
					CreateCW1Rate(contractNumber: "B"),
				};

				form.Show();
				Application.DoEvents();

				var viewModel = (NonContainerizedRatesViewModel)form.ViewModel;
				((IList<RateViewModel>)viewModel.Rates).UniqueAddRange(rates);
				viewModel.SelectedRate = rates[0];

				var toolStrip = (ZToolStrip)form.Controls.Find("zToolStrip1", true).Single();
				var btnApply = toolStrip.Items.Find("btnApply", false).Single();
				btnApply.PerformClick();

				AssertEquals("Dialog result should be OK after applying a rate.", DialogResult.OK, form.DialogResult);

				var messages = UsageCollectorTestHelper.LoadUsageMessages(UsageFeatures.Codes.RateSelector);
				var properties = messages[0].UsageProperties;

				AssertEquals("Action property should be 'Select'.", "Select", properties.Value<string>(UsageProperties.Action));
				AssertEquals("Selected provider should be 'Cargoguide'.", "Cargoguide", properties.Value<string>(UsageProperties.SelectedProvider));

				var ratesResult = properties.Value<JObject>(UsageProperties.RatesSearchResult);

				AssertEquals(
					"Total rates for 'Cargoguide' should be 3.",
					3,
					ratesResult.Value<JObject>("Cargoguide").Value<int>("TotalRates")
				);

				AssertEquals(
					"Total rates for 'CW1' should be 2.",
					2,
					ratesResult.Value<JObject>("CW1").Value<int>("TotalRates")
				);
			}
		}

		public void TestSkipRateSelection_ShouldReportUsage_WithSkipAction()
		{
			var consol = CreateConsol();
			var autoRatingProxy = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), new Mock<IDialogService>().Object))
			{
				form.Show();
				Application.DoEvents();

				var toolStrip = (ZToolStrip)form.Controls.Find("zToolStrip1", true).Single();
				var btnSkip = toolStrip.Items.Find("btnSkip", false).Single();
				btnSkip.PerformClick();

				AssertEquals("Expected DialogResult.Cancel after Skip action.", DialogResult.Cancel, form.DialogResult);

				var messages = UsageCollectorTestHelper.LoadUsageMessages(UsageFeatures.Codes.RateSelector);
				var message = messages[0];
				AssertEquals("Expected action to be 'Skip'.", "Skip", message.GetProperty<string>(UsageProperties.Action));
			}
		}

		public void TestCancelRateSelection_ShouldReportUsage_WithCancelAction()
		{
			var consol = CreateConsol();
			var autoRatingProxy = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext(), new Mock<IDialogService>().Object))
			{
				form.Show();
				Application.DoEvents();

				var toolStrip = (ZToolStrip)form.Controls.Find("zToolStrip1", true).Single();
				var btnCancel = toolStrip.Items.Find("btnCancel", false).Single();
				btnCancel.PerformClick();

				AssertEquals("Dialog result should be Cancel after cancel button click.", DialogResult.Cancel, form.DialogResult);

				var messages = UsageCollectorTestHelper.LoadUsageMessages(UsageFeatures.Codes.RateSelector);
				var message = messages[0];
				AssertEquals("Expected action to be 'Cancel' in the usage message.", "Cancel", message.GetProperty<string>(UsageProperties.Action));
			}
		}

		public void TestRateSearch_IsCancelled_WhenBtnApplyClick()
		{
			var promptUserApplyZeroChargesResult = ZDialogResult.Yes;
			var costing = Helper.NewCosting(TransportProvider1);
			var rateLine = costing.CreateRate(contractNumber: "AAA").AddFlatCharge("BAF", 0);
			var consol = CreateConsol();

			var autoRatingProxy = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var context = new RateSelectorContext()
			{
				Factory = Factory,
				CurrencyConverter = new Mock<ICurrencyConverter>().Object
			};

			var dialogServiceMock = new Mock<IDialogService>();

			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogServiceMock.Object))
			{
				dialogServiceMock
					.Setup(dialog => dialog.PromptUserApplyZeroCharges(It.Is<List<ZString>>(zeroCharges => zeroCharges.IsEquivalentTo(new List<ZString> { "BAF" }))))
					.Callback(() => Assert(form.search.IsCancellationRequested))
					.Returns(promptUserApplyZeroChargesResult);
				ShowTestForm(form);

				form.search = new CancellationTokenSource();
				var selectedRate = new CW1RateViewModel(context, criteria, ZGuid.Empty, ZString.Empty, new[] { rateLine }, CargoWise.Common.DisposableAction.NoAction);
				selectedRate.Calculate(criteria, criteria.Creditors.ChargeCodeGroups);

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = selectedRate;

				// When the Apply button is clicked, the `form.search` token will have
				// cancel requested before the `PromptUserApplyZeroCharges` is called.
				form.BtnApplyClick(this, EventArgs.Empty);
			}
		}

		#region GetSelectedRate

		public void TestGetSelectedRate_EmptyCarrier()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var creditor = Factory.NewWithValidTestData<OrgHeader>();

			var creditorCost = Helper.NewCosting(creditor);
			creditorCost.CreateRate(contractNumber: "AAA").AddFlatCharge("BAF", 100);

			var carrierCost = CreateCargoGuideRate(carrier, contractNumber: "AAA");
			carrierCost.AutoRateInfos.Add(new AutoRateInfo(Factory) { ChargeCode = Helper.ChargeCodes["FRT"] });

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty; // empty carrier
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			Factory.Save();

			AutoRateInfoCollection charges = null;
			AssertNoExceptionThrown(() => charges = GetSelectedRate(consol, selectedRate: carrierCost));

			var actual = charges
				.Select(c => $"{c.ChargeCode.AC_Code}|{c.Amount}")
				.ToArray();

			var expected = new[]
			{
				"FRT|0",
				"BAF|100.00"
			};

			AssertContainsExactElementsInAnyOrder(
				"The selected charges should match the expected values",
				expected,
				actual
			);
		}

		public void TestGetSelectedRate_JobHasNoContractNumberAndTheSelectedCarrierRateHasContractNumber_OtherRatesShouldBeLoadedForTheContractNumber()
		{
			var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();

			var otherCosts = Helper.NewCosting(otherOrg);
			otherCosts.CreateRate(contractNumber: null).AddFlatCharge("BAF", 100);
			otherCosts.CreateRate(contractNumber: "AAA").AddFlatCharge("BAF", 200);
			otherCosts.CreateRate(contractNumber: "BBB").AddFlatCharge("BAF", 300);

			var carrierCost = CreateCargoGuideRate(carrierOrg, contractNumber: "AAA");
			carrierCost.AutoRateInfos.Add(new AutoRateInfo(Factory) { ChargeCode = Helper.ChargeCodes["FRT"] });

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrierOrg.MainAddress.PK;
			consol.JK_OA_CreditorAddress = otherOrg.MainAddress.PK;

			Factory.Save();

			var charges = GetSelectedRate(consol, selectedRate: carrierCost, "AAA");

			var actual = charges.Select(c => $"{c.ChargeCode.AC_Code}|{c.Amount}").ToArray();

			var expected = new[]
			{
				"FRT|0",
				"BAF|200.00",
			};

			AssertContainsExactElementsInAnyOrder(
				"BAF charge should be loaded from the rate with AAA contract number since the selected carrier rate has AAA contract number",
				expected,
				actual
			);
		}

		public void TestGetSelectedRate_TheSelectedCarrierRateHasDifferentContractNumber_OtherRatesShouldBeLoadedForNewContractNumber()
		{
			var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();

			var otherCosts = Helper.NewCosting(otherOrg);
			otherCosts.CreateRate(contractNumber: null).AddFlatCharge("BAF", 100);
			otherCosts.CreateRate(contractNumber: "AAA").AddFlatCharge("BAF", 200);
			otherCosts.CreateRate(contractNumber: "BBB").AddFlatCharge("BAF", 300);

			var carrierCost = CreateCargoGuideRate(carrierOrg, contractNumber: "AAA");
			carrierCost.AutoRateInfos.Add(new AutoRateInfo(Factory) { ChargeCode = Helper.ChargeCodes["FRT"] });

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrierOrg.MainAddress.PK;
			consol.JK_OA_CreditorAddress = otherOrg.MainAddress.PK;
			consol.JK_CarrierContractNumber = "BBB";
			consol.Numbers.AddNewIfNotExist("CLN", "YYY");

			Factory.Save();

			var charges = GetSelectedRate(consol, selectedRate: carrierCost, "AAA");
			var actual = charges.Select(c => $"{c.ChargeCode.AC_Code}|{c.Amount}").ToArray();
			var expected = new[]
			{
				"FRT|0",
				"BAF|200.00"
			};

			AssertContainsExactElementsInAnyOrder(
				"BAF charge for rate with AAA contract number is loaded since the selected rate has contract number AAA which updates the initial consol number of BBB",
				expected,
				actual
			);
		}

		public void TestLegacyRateSelectorFormAirRates_ShouldNotUseUrsRatesProvider()
		{
			var consol = CreateConsol();
			var autoRatingProxy = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);
			var mockUrsFactory = new Mock<IUrsRatesClientFactory>();
			var mockUrsClient = new Mock<IUrsClient>();
			mockUrsFactory
				.Setup(f => f.TryCreate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ILogger>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				.Returns(mockUrsClient.Object);
			mockUrsClient
				.Setup(f => f.GetTradeServicesAsync(It.IsAny<QueryRequestDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult<IEnumerable<TradeServiceDto>>([]));
			ObjectFactory.Substitute(mockUrsFactory.Object);

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";

			// Given
			// Rate Service is ENABLED
			// URS feature is ENABLED
			// legacy Rate Selector is ENABLED
			using (var form = new RateSelectorForm(criteria, new RatingContext(), new Mock<IDialogService>().Object))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (ObjectFactory.Substitute(MockURSFeatureHelper(true)))
			{
				// When doing Autorating
				form.Show();
				Application.DoEvents();

				// Then Urs Provider should not be called to get rates
				mockUrsFactory.Verify(f => f.TryCreate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ILogger>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
				mockUrsClient.Verify(f => f.GetTradeServicesAsync(It.IsAny<QueryRequestDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
				Assert(true);
			}
		}

		IFeatureControlManager MockURSFeatureHelper(bool enabled)
		{
			var ursRule = new RatingFeatureHelper.Urs.UrsFeatureRule { Enabled = enabled, Url = "https://fcm.cargowise.com" };
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out ursRule)).Returns(true);
			featureControlMock
				.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.UniversalRatesService, CancellationToken.None))
				.Returns(Task.FromResult(featureDataMock.Object));

			return featureControlMock.Object;
		}

		AutoRateInfoCollection GetSelectedRate(IRatingSupporterWithAdapter job, RateViewModel selectedRate, string selectedNumberFromDialog = null)
		{
			var dialogService = new Mock<IDialogService>();
			dialogService
				.Setup(x => x.SelectSingleCarrierContractNumber(It.IsAny<IEnumerable<string>>()))
				.Returns(new SingleCarrierContractNumberSelectionResult(selectedNumberFromDialog));
			var ratingAdapter = job.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);
			criteria.IsManualCostSelectMode = true;
			var interactor = new TestInteractor();
			using (_Rating.Start(interactor))
			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService.Object))
			{
				ShowTestForm(form);

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = selectedRate;
				form.BtnApplyClick(this, EventArgs.Empty);

				return form.SelectedRates;
			}
		}

		#endregion

		void AssertJobAttributesHasNotChangedToSelectedRateAttributes(IDialogService dialogService, RatingCriteria criteria, IAutoRating ratingAdapter)
		{
			using (var form = new RateSelectorForm(criteria, new RatingContext(), dialogService))
			{
				ShowTestForm(form);

				var rate = new CargoguideRateViewModelSample
				{
					CarrierCode = TransportProvider2.OH_Code,
					CarrierOrg = TransportProvider2,
					CarrierErrorLevel = ErrorLevel.None,
					PaymentTerms = "CCX",
					Origin = "UAIEV",
					Destination = "SGSIN",
					ContractNumber = "12345",
					CarrierServiceLevel = "EXP",
					CarrierServiceLevelErrorLevel = ErrorLevel.None,
					Commodities = null,
					CommodityGroupErrorLevel = ErrorLevel.None
				};
				rate.Charges.ForEach(c => c.Charges.ForEach(cc => cc.ChargeCodeErrorLevel = ErrorLevel.None));

				((NonContainerizedRatesViewModel)form.ViewModel).SelectedRate = rate;
				ApplyButtonPerformClick(form);

				AssertNotEquals("Expected origin to not be AUSYD", "AUSYD", ratingAdapter.Origin);
				AssertNotEquals("Expected destination to not be SGSIN", "SGSIN", ratingAdapter.Destination);
				AssertNotEquals("Expected carrier PK to not match TransportProvider2.PK", TransportProvider2.PK, ratingAdapter.Carrier.PK);
				AssertNotEquals("Expected service level to not be EXP", "EXP", ratingAdapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Carrier));

				AssertNotEquals("DialogResult should not be OK", DialogResult.OK, form.DialogResult);
			}
		}

		protected UsageCollectorTestHelper UsageCollectorTestHelper => usageCollectorTestHelper ?? (usageCollectorTestHelper = new UsageCollectorTestHelper(Factory));
		UsageCollectorTestHelper usageCollectorTestHelper;

		ForwardingConsol CreateConsol(
			string transportMode = "AIR",
			string containerMode = "LSE",
			string origin = "UAIEV",
			string destination = "AUSYD")
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.Transports[0].JW_IsLinked = false;
			consol.Transports[0].CarrierPK = TransportProvider1.PK;
			consol.JK_OA_ShippingLineAddress = TransportProvider1.MainAddress.PK;
			consol.JK_ConsolMode = containerMode;
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;

			return consol;
		}

		CargoGuideRateForTest CreateCargoGuideRate(OrgHeader carrier = null, string contractNumber = null)
		{
			carrier = carrier ?? TransportProvider1;

			var rate = new CargoGuideRateForTest
			{
				CarrierCode = carrier.OH_Code,
				CarrierOrg = carrier,
				CarrierErrorLevel = ErrorLevel.None,
				PaymentTerms = "CCX",
				Origin = "UAIEV",
				Destination = "AUSYD",
				ContractNumber = contractNumber,
				CarrierServiceLevel = "STD",
				CarrierServiceLevelErrorLevel = ErrorLevel.None,
				Commodities = null,
				CommodityGroupErrorLevel = ErrorLevel.None
			};

			return rate;
		}

		CW1RateViewModelSample CreateCW1Rate(OrgHeader carrier = null, string contractNumber = null)
		{
			var rate = new CW1RateViewModelSample()
			{
				CarrierCode = (carrier ?? TransportProvider1).OH_Code,
				CarrierOrg = carrier,
				PaymentTerms = "CCX",
				Origin = "UAIEV",
				Destination = "AUSYD",
				CarrierServiceLevel = "STD",
				Commodities = null,
				CommodityGroups = new List<string>(),
				ContractNumber = contractNumber
			};

			return rate;
		}

		static void ApplyButtonPerformClick(RateSelectorForm form)
		{
			var toolStrip = (ZToolStrip)form.Controls.Find("zToolStrip1", true).Single();
			var btnApply = toolStrip.Items.Find("btnApply", false).Single();
			btnApply.PerformClick();
		}

		static void ShowTestForm(RateSelectorForm form)
		{
			form.ShouldRunSearchOnShowingForm = false;
			form.Show();
			Application.DoEvents();
		}
	}

	class CargoGuideRateForTest : CargoguideRateViewModel
	{
		public List<AutoRateInfo> AutoRateInfos { get; } = new List<AutoRateInfo>();

		public override IEnumerable<AutoRateInfo> GetAutoRateInfos() => AutoRateInfos;
	}

	static class RateSelectorFormTestExtensions
	{
		public static RateEntry CreateRate(
			this RatingHeader header,
			string category = "AIR",
			string mode = "LSE",
			string origin = "UAIEV",
			string destination = "AUSYD",
			string container = null,
			string commodity = "GEN",
			string contractNumber = null)
		{
			var entry = header.AddRateEntry(category, mode, origin, destination, "STD", container, commodity);
			entry.TI_ContractNumber = contractNumber;
			entry.RateLines.RemoveAndDeleteAll();

			return entry;
		}
	}
}
