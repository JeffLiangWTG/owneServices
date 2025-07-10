using System.Collections.Generic;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.Services;
using Moq;

namespace Enterprise.Rating.Testing.GUITests.RateSelector.Models
{
	public abstract class ChargeViewModelTest : RatingTestCase
	{
		public void TestLocalAmount_AmountIsZero_ShouldBeZero()
		{
			var mock = new Mock<ICurrencyConverter>();
			var instance = GetInstance("FRT", 0, "USD", mock.Object);

			AssertEquals("LocalAmount should be 0", 0m, instance.LocalAmount);
			AssertEquals("LocalCurrency should be 'EUR'", "EUR", instance.LocalCurrency);
			AssertNullOrEmpty("LocalAmountError should be null or empty", instance.LocalAmountError);
		}

		public void TestLocalAmount_ExchangeRateExists_ConvertToLocalAmount()
		{
			var mock = new Mock<ICurrencyConverter>();
			mock.Setup(s => s.Convert(It.Is<Money>(m => m.Amount == 50 && m.Currency.Code == "USD"), It.Is<RefCurrency>(r => r.RX_Code == "EUR")))
				.Returns(new Money(100, GlbCompany.CurrentCompany.LocalCurrency));

			var instance = GetInstance("FRT", 50, "USD", mock.Object);

			AssertEquals("Expected local amount to be 100.", 100m, instance.LocalAmount);
			AssertEquals("Expected local currency to be EUR.", "EUR", instance.LocalCurrency);
			AssertNullOrEmpty("Expected local amount error to be null or empty.", instance.LocalAmountError);
		}

		public void TestLocalAmount_ExchangeRateDoesntExist_PopulateLocalAmountError()
		{
			var mock = new Mock<ICurrencyConverter>();
			mock.Setup(s => s.Convert(It.Is<Money>(m => m.Amount == 50 && m.Currency.Code == "USD"), It.Is<RefCurrency>(r => r.RX_Code == "EUR")))
				.Returns(new Money(-1, GlbCompany.CurrentCompany.LocalCurrency, false));

			var instance = GetInstance("XXX", 50, "USD", mock.Object);

			AssertEquals("Expected local amount to be 0.", 0m, instance.LocalAmount);
			AssertEquals("Expected local currency to be EUR.", "EUR", instance.LocalCurrency);
			AssertEquals(
				"Expected local amount error to indicate missing exchange rate.",
				"No valid exchange rate between USD and EUR is found for Charge XXX",
				instance.LocalAmountError
			);
		}

		public void TestIsSelected_ChargeIsNotOptional_ReturnTrue()
		{
			var viewModel = GetInstance("FRT", 100, "UAH", new RefCurrenciesCurrencyConverter(Factory));
			viewModel.IsOptional = false;

			AssertEquals(true, viewModel.IsSelected);

			viewModel.IsSelected = false;
			AssertEquals("Should always be true for non-optional charges", true, viewModel.IsSelected);
		}

		public void TestIsSelected_ChargeIsOptional_ShouldUpdateAndRaisePropertyChanged()
		{
			var viewModel = GetInstance("FRT", 100, "UAH", new RefCurrenciesCurrencyConverter(Factory));
			viewModel.IsOptional = true;
			viewModel.IsSelected = false;

			var propertyChanges1 = new List<string>();
			viewModel.PropertyChanged += (s, e) => propertyChanges1.Add(e.PropertyName);

			viewModel.IsSelected = true;

			AssertCollectionContains(
				"PropertyChanged event should be raised when IsSelected is set to true",
				"IsSelected",
				propertyChanges1
			);
			AssertEquals("IsSelected should be true after updating", true, viewModel.IsSelected);

			var propertyChanges2 = new List<string>();
			viewModel.PropertyChanged += (s, e) => propertyChanges2.Add(e.PropertyName);

			viewModel.IsSelected = false;

			AssertCollectionContains(
				"PropertyChanged event should be raised when IsSelected is set to false",
				"IsSelected",
				propertyChanges2
			);
			AssertEquals("IsSelected should be false after updating", false, viewModel.IsSelected);
		}

		public void TestDisplayPrice()
		{
			var originalValue = Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed;

			try
			{
				var charge = GetInstance("FRT", 66, "UAH", new RefCurrenciesCurrencyConverter(Factory));

				Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed = false;
				AssertEquals("DisplayPrice should be false when IsAllowed is set to false.", false, charge.DisplayPrice);

				Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed = true;
				AssertEquals("DisplayPrice should be true when IsAllowed is set to true.", true, charge.DisplayPrice);
			}
			finally
			{
				Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed = originalValue;
			}
		}

		public void TestIsValid()
		{
			void AssertIsValid(ErrorLevel chargeErrorLevel, ErrorLevel expectedErrorLevel)
			{
				var viewModel = new ChargeViewModelSample();
				viewModel.ChargeCodeErrorLevel = chargeErrorLevel;

				AssertEquals("Expected error level does not match the actual error level.", expectedErrorLevel, viewModel.ErrorLevel);
				AssertEquals("IsValid flag does not match the expectation.", !expectedErrorLevel.HasFlag(ErrorLevel.Error), viewModel.IsValid);
			}

			AssertIsValid(ErrorLevel.None, expectedErrorLevel: ErrorLevel.None);
			AssertIsValid(ErrorLevel.Warning, expectedErrorLevel: ErrorLevel.Warning);
			AssertIsValid(ErrorLevel.Error, expectedErrorLevel: ErrorLevel.Error);
		}

		protected RateSelectorFilterStripBusinessObject GetValidFilters()
		{
			var invoicingSupporter = new Mock<IJobInvoicingSupporter>();
			invoicingSupporter.Setup(i => i.OperationalJobRef).Returns("Consol 666");

			var criteria = new TestRatingCriteria("AUSYD", "UAIEV", FreightMode.AIR, 66, 9, null);
			criteria.InvoicingSupporter = invoicingSupporter.Object;

			return new RateSelectorFilterStripBusinessObject(criteria, new TestLogger());
		}

		protected override void SetUp()
		{
			base.SetUp();

			originalCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			GlbCompany.CurrentCompany.SetCurrency("EUR");
		}

		protected override void TearDown()
		{
			base.TearDown();

			GlbCompany.CurrentCompany.SetCurrency(originalCurrency.Code);
		}

		protected abstract ChargeViewModel GetInstance(string chargeCode, decimal amount, string currency, ICurrencyConverter currencyConverter);

		RefCurrency originalCurrency;
	}
}
