using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.Services;
using Moq;

namespace Enterprise.Rating.Testing.GUITests.RateSelector.Models
{
	public class BookingEngineChargeViewModelTest : RatingTestCase
	{
		public void TestPopulateFromBookingRate()
		{
			var transportLeg = new Mock<IBookingTransportLeg>(MockBehavior.Loose);

			var rate = ModelsTestingHelper.GetMockedBookingRate(
				2000,
				Constants.CurrencyCodes.EuropeanUnion,
				"STANDARD",
				"BOOKABLE",
				"074",
				"rate1",
				new[] { transportLeg.Object }
			);

			var chargeViewModel = BookingEngineChargeViewModel.New(Factory, rate.Object);
			AssertEquals("Charge amount should be correct", 2000m, chargeViewModel.Amount);
			AssertEquals("Currency should be correct", Constants.CurrencyCodes.EuropeanUnion, chargeViewModel.Currency);
			AssertEquals("Charge code should be correct", "STANDARD", chargeViewModel.ChargeCode);
			AssertEquals("Charge code description should be correct", "BOOKABLE", chargeViewModel.ChargeCodeDescription);
		}

		public void TestAmountIsPopulatedCorrectly_When_ObjectCreated()
		{
			var mockCurrencyConvertor = new Mock<ICurrencyConverter>(MockBehavior.Strict);
			mockCurrencyConvertor
				.Setup(x => x.Convert(It.IsAny<Money>(), It.IsAny<RefCurrency>()))
				.Returns(new Money(120, GlbCompany.CurrentCompany.LocalCurrency));

			var rate = ModelsTestingHelper.GetMockedBookingRate(100, "AUD", "Test");
			var chargeViewModel = BookingEngineChargeViewModel.New(Factory, rate.Object, mockCurrencyConvertor.Object);

			AssertEquals("LocalAmount did not match the expected value.", 120m, chargeViewModel.LocalAmount);
			AssertEquals("LocalCurrency did not match the expected value.", "USD", chargeViewModel.LocalCurrency);
			AssertEquals("Amount did not match the expected value.", 100m, chargeViewModel.Amount);
			AssertEquals("Currency did not match the expected value.", "AUD", chargeViewModel.Currency);
			AssertEquals("ChargeCode did not match the expected value.", "Test", chargeViewModel.ChargeCode);
			AssertEquals("IsSelected did not match the expected value.", true, chargeViewModel.IsSelected);
			AssertEquals("IsIncluded did not match the expected value.", true, chargeViewModel.IsIncluded);
		}

		public void TestDisplayPriceIsAlwaysTrue()
		{
			var originalValue = Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed;

			try
			{
				var rate = ModelsTestingHelper.GetMockedBookingRate(100, "AUD", "Test");
				var chargeViewModel = BookingEngineChargeViewModel.New(Factory, rate.Object);

				Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed = false;
				AssertEquals("Expected DisplayPrice to be true when IsAllowed is false", true, chargeViewModel.DisplayPrice);

				Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed = true;
				AssertEquals("Expected DisplayPrice to be true when IsAllowed is true", true, chargeViewModel.DisplayPrice);
			}
			finally
			{
				Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed = originalValue;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			originalCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			GlbCompany.CurrentCompany.SetCurrency("USD");
		}

		protected override void TearDown()
		{
			base.TearDown();

			GlbCompany.CurrentCompany.SetCurrency(originalCurrency.Code);
		}

		RefCurrency originalCurrency;
	}
}
