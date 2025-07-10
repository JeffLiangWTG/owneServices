using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.RatingTests.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Testing.GUITests.RateSelector
{
	public class RateSelectorFilterStripControlTest : RatingTestCase
	{
		public void TestFilterControlDefaulting_JobIsOneOffQuote()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var quotedBooking = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, "AIR", "LSE", ZString.Empty, consignor, consignor, consignee, TransportProvider1, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			quotedBooking.StartDate = DateTime.Now.Date;
			quotedBooking.EndDate = ZDateTime.Now.Date;
			quotedBooking.Quote.CurrentOneOffQuote.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON, "CN123");
			quotedBooking.Quote.CurrentOneOffQuote.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, "NAC");
			quotedBooking.LoadPort = "AUMEL";
			quotedBooking.DischargePort = "SEGOT";

			var ratingAdapter = quotedBooking.GetRatingAdapters().FirstOrDefault();
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var ratingContext = new RatingContext();
			using (var form = new RateSelectorForm(criteria, ratingContext, new Mock<IDialogService>().Object))
			{
				form.ShouldRunSearchOnShowingForm = false;
				form.Show();
				Application.DoEvents();

				var filterControl = (RateSelectorFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var activeModuleFilters = filterControl.FilterBusinessObject.ActiveModuleFilters;
				AssertNotNull(activeModuleFilters.OfType<ModuleLocationFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.OriginDestination && x.Property1 == "AUSYD" && x.Property2 == "USLAX"));
			}
		}

		public void TestFilterControlDefaulting_JobIsQuickBooking()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var quickBooking = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, "AIR", "LSE", ZString.Empty, consignor, consignor, consignee, TransportProvider1, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.BookingOnly);
			quickBooking.StartDate = DateTime.Now.Date;
			quickBooking.EndDate = ZDateTime.Now.Date;
			quickBooking.Booking.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON, "CN123");
			quickBooking.Booking.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, "NAC");
			quickBooking.LoadPort = "AUMEL";
			quickBooking.DischargePort = "SEGOT";

			var ratingAdapter = quickBooking.GetRatingAdapters().FirstOrDefault();
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var ratingContext = new RatingContext();
			using (var form = new RateSelectorForm(criteria, ratingContext, new Mock<IDialogService>().Object))
			{
				form.ShouldRunSearchOnShowingForm = false;
				form.Show();
				Application.DoEvents();

				var filterControl = (RateSelectorFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var activeModuleFilters = filterControl.FilterBusinessObject.ActiveModuleFilters;
				AssertNotNull(activeModuleFilters.OfType<ModuleLocationFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.OriginDestination && x.Property1 == "AUMEL" && x.Property2 == "SEGOT"));
			}
		}

		public void TestFilterControlDefaulting_JobIsQuickBookingWithCode()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var quickBooking = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, "AIR", "LSE", ZString.Empty, consignor, consignor, consignee, TransportProvider1, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.AcceptedBookingWithQuote);
			quickBooking.StartDate = DateTime.Now.Date;
			quickBooking.EndDate = ZDateTime.Now.Date;
			quickBooking.Booking.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON, "CN123");
			quickBooking.Booking.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, "NAC");
			quickBooking.LoadPort = "AUMEL";
			quickBooking.DischargePort = "SEGOT";

			var ratingAdapter = quickBooking.GetRatingAdapters().FirstOrDefault();
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var ratingContext = new RatingContext();
			using (var form = new RateSelectorForm(criteria, ratingContext, new Mock<IDialogService>().Object))
			{
				form.ShouldRunSearchOnShowingForm = false;
				form.Show();
				Application.DoEvents();

				var filterControl = (RateSelectorFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var activeModuleFilters = filterControl.FilterBusinessObject.ActiveModuleFilters;
				AssertNotNull(activeModuleFilters.OfType<ModuleLocationFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.OriginDestination && x.Property1 == "AUMEL" && x.Property2 == "SEGOT"));
			}
		}

		public void TestFilterControlConstructorSetsDefaults()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var ratingAdapter = consol.RatingAdapter;

			AssertFilterStripAndDefaultValues(ratingAdapter, true);
		}

		public void TestListOfFilterStripControl()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			//Quick Booking
			var quickBookingForAirLSE = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, "AIR", "LSE", ZString.Empty, consignor, consignor, consignee, TransportProvider1, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.BookingOnly);
			var ratingAdapter = quickBookingForAirLSE.GetFirstAdapter();
			AssertFilterStripAndDefaultValues(ratingAdapter, false);

			//Booking With Quote
			var bookingWithQuoteAirLSE = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, "AIR", "LSE", ZString.Empty, consignor, consignor, consignee, TransportProvider1, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.AcceptedBookingWithQuote);
			ratingAdapter = bookingWithQuoteAirLSE.GetFirstAdapter();
			AssertFilterStripAndDefaultValues(ratingAdapter, false);

			//One Off Quote
			var oneOffQuoteAirLSE = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, "AIR", "LSE", ZString.Empty, consignor, consignor, consignee, TransportProvider1, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			ratingAdapter = oneOffQuoteAirLSE.GetFirstAdapter();
			AssertFilterStripAndDefaultValues(ratingAdapter, false);
		}

		void AssertFilterStripAndDefaultValues(IAutoRating ratingAdapter, bool isPaymentTermFilterExpected)
		{
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext()))
			{
				form.ShouldRunSearchOnShowingForm = false;
				form.Show();
				Application.DoEvents();

				var filterControl = (RateSelectorFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var activeModuleFilters = filterControl.FilterBusinessObject.ActiveModuleFilters;

				AssertNotNull(activeModuleFilters.OfType<ModuleSingleDateFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.EffectiveOn));

				AssertNotNull(activeModuleFilters.OfType<ModuleLocationFilter>()
						.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.OriginDestination && x.Property1 == "AUSYD" && x.Property2 == "USLAX"));

				AssertNotNull(activeModuleFilters.OfType<ModuleGuidFilter>()
				  .SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider && x.Property == TransportProvider1.PK));

				AssertNotNull(activeModuleFilters.OfType<WiseRatesModuleTextFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.CarrierServiceLevel && x.Property == "STD"));

				if (isPaymentTermFilterExpected)
				{
					AssertNotNull(activeModuleFilters.OfType<WiseRatesModuleTextFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.PaymentTerm && x.Property == "PPD"));
					AssertEquals("Date, Location, Carrier, Payment Term, and Service Level filters are expected", 5, activeModuleFilters.Count);
				}
				else
				{
					AssertNull(activeModuleFilters.OfType<WiseRatesModuleTextFilter>()
						.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.PaymentTerm));
					AssertEquals("Date, Location, Carrier and Service Level filters are expected", 4, activeModuleFilters.Count);
				}
			}
		}

		public void TestGivenAutoRatingConfiguration_WhenAutoRating_ThenEffectiveDateShouldBeSetByAutoRatingConfiguration()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Custom;

			Helper.CreateOrganizationRatingDateConfig(carrier,
				ChargeCodeGroupList.Codes.Freight,
				JobInvoicingConsumerTypes.ForwardingConsolCode,
				FreightShipmentDirection.Code.All,
				RateMode.AIR,
				JobRateTypes.Codes.All,
				ContainerModes.Loose,
				"",
				JobDateTypes.Codes.ArrivalDate);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2022, 3, 1);
			consol.Transports.MostInterestingTransport.JW_ETA = new ZDateTime(2022, 3, 4);
			consol.Transports[0].CarrierPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			var autoRatingProxy = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			Factory.Save();

			using (var form = new RateSelectorForm(criteria, new RatingContext()))
			{
				form.ShouldRunSearchOnShowingForm = false;
				form.Show();
				Application.DoEvents();

				var filterControl =
					(RateSelectorFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var activeModuleFilters = filterControl.FilterBusinessObject.ActiveModuleFilters;

				AssertEquals("Effective On Date", new ZDateTime(2022, 3, 4), activeModuleFilters
					.OfType<ModuleSingleDateFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.EffectiveOn)
					.Property1);
			}
		}

		public void TestFilter_CreateNewUniversalCommodityGroupFilter_FilterWithEmptyDefaultValueShouldBeAdded()
		{
			var commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity.RH_Code = "XXXX";
			commodity.RH_UniversalCommodityGroup = "CMMGROUP";
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var container = consol.Containers.AddNew();
			container.JC_RH_NKContainerCommodityCode = "XXXX";
			container.JC_ContainerMode = ContainerModes.ULD;
			container.JC_ContainerCount = 1;

			var containerTypeLD1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-1");
			container.JC_RC = containerTypeLD1.PK;

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			using (var form = new RateSelectorForm(criteria, new RatingContext()))
			{
				form.ShouldRunSearchOnShowingForm = false;
				form.Show();
				Application.DoEvents();

				var filterControl = (RateSelectorFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var filterBusinessObject = (RateSelectorFilterStripBusinessObject)filterControl.FilterBusinessObject;
				var universalCommodityGroupFilters = filterBusinessObject.Where(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.UniversalCommodityGroup);
				AssertEquals("The universal commodity group filters count should be 3.", 3, universalCommodityGroupFilters.Count());

				filterBusinessObject.ResetModuleFilters();
				filterBusinessObject.AddFilterStrip<WiseRatesModuleTextFilter>(RateEntryFilterUtility.Constants.Codes.UniversalCommodityGroup);

				universalCommodityGroupFilters = filterBusinessObject.Where(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.UniversalCommodityGroup);
				AssertEquals("The property of the single module text filter should be an empty string.", string.Empty, universalCommodityGroupFilters.Cast<ModuleTextFilter>().Single().Property);
			}
		}

		public void TestFilter_CommodityCodeFilter_SingleInstance()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);
			criteria.ValuesCanBeSet = true;
			criteria.ContainerMode = ContainerModes.Loose;

			using (var form = new RateSelectorForm(criteria, new RatingContext()))
			{
				form.ShouldRunSearchOnShowingForm = false;
				form.Show();
				Application.DoEvents();

				var filterControl = (RateSelectorFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var filterBusinessObject = (RateSelectorFilterStripBusinessObject)filterControl.FilterBusinessObject;

				filterBusinessObject.ResetModuleFilters();
				var strip1 = filterBusinessObject.AddFilterStrip<ModuleNkFilter>(RateEntryFilterUtility.Constants.Codes.CommodityCode);
				Assert("The filter strip should only allow a single instance", strip1.IsSingleInstanceOnly);

				AssertEquals("The filter strip's property should be an empty string", string.Empty, strip1.Property);
			}
		}
	}
}
