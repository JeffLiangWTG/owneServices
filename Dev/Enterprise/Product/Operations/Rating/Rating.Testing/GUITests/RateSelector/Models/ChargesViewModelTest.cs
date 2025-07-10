using System.Collections.Generic;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.Testing.GUITests.RateSelector.Models
{
	public class ChargesViewModelTest : RatingTestCase
	{
		public void TestTotalAmount_ReturnLocalAmountsFromSelectedCharges()
		{
			var chargesViewModel = new ChargesViewModel("Freight");

			chargesViewModel.Add(new ChargeViewModelSample
			{
				LocalAmount = 100,
				LocalCurrency = "AUD",
				LocalAmountError = null,
				IsOptional = true,
				IsSelected = false
			});

			chargesViewModel.Add(new ChargeViewModelSample
			{
				LocalAmount = 0,
				LocalCurrency = "AUD",
				LocalAmountError = "No exchange rate 1",
				IsOptional = true,
				IsSelected = false
			});

			chargesViewModel.Add(new ChargeViewModelSample
			{
				LocalAmount = 200,
				LocalCurrency = "AUD",
				LocalAmountError = null,
				IsOptional = true,
				IsSelected = true
			});

			chargesViewModel.Add(new ChargeViewModelSample
			{
				LocalAmount = 0,
				LocalCurrency = "AUD",
				LocalAmountError = "No exchange rate 2",
				IsOptional = true,
				IsSelected = true
			});

			AssertEquals("TotalPrice should be 200", 200m, chargesViewModel.TotalPrice);
			AssertEquals("TotalPriceString should be 200.00 AUD", "200.00 AUD", chargesViewModel.TotalPriceString);
			AssertEquals("TotalPriceError should capture the last LocalAmountError of the selected charge", "No exchange rate 2", chargesViewModel.TotalPriceError);
		}

		public void TestChargeSelected_NotifyPropertiesChanged()
		{
			var charge1 = new ChargeViewModelSample
			{
				LocalAmount = 0,
				LocalCurrency = "AUD",
				Amount = 50,
				Currency = "BTC",
				LocalAmountError = "No exchange rate between BTC and AUD",
				IsSelected = false,
				IsOptional = true,
				ChargeCodeError = null,
				ChargeCodeErrorLevel = ErrorLevel.None
			};

			var charge2 = new ChargeViewModelSample
			{
				LocalAmount = 200,
				LocalCurrency = "AUD",
				Amount = 400,
				Currency = "USD",
				LocalAmountError = null,
				IsSelected = false,
				IsOptional = true,
				ChargeCodeError = "No mapping",
				ChargeCodeErrorLevel = ErrorLevel.Error,
			};

			var chargesViewModel = new ChargesViewModel("Freight");
			chargesViewModel.Add(charge1);
			chargesViewModel.Add(charge2);

			AssertEquals("Initial total price should be 0", 0m, chargesViewModel.TotalPrice);
			AssertNullOrEmpty("Total price error should initially be null or empty", chargesViewModel.TotalPriceError);
			AssertEquals("Initial total price string should be \"0.00 AUD\"", "0.00 AUD", chargesViewModel.TotalPriceString);
			Assert("ViewModel should initially be valid", chargesViewModel.IsValid);

			var propertyChanges1 = new List<string>();
			chargesViewModel.PropertyChanged += (s, e) => propertyChanges1.Add(e.PropertyName);

			charge1.IsSelected = true;

			AssertCollectionContains("Property change for IsValid should be raised", nameof(chargesViewModel.IsValid), propertyChanges1);
			AssertCollectionContains("Property change for TotalPrice should be raised", nameof(chargesViewModel.TotalPrice), propertyChanges1);
			AssertCollectionContains("Property change for TotalPriceError should be raised", nameof(chargesViewModel.TotalPriceError), propertyChanges1);
			AssertCollectionContains("Property change for TotalPriceString should be raised", nameof(chargesViewModel.TotalPriceString), propertyChanges1);

			AssertEquals("Total price should be 0 after first charge is selected", 0m, chargesViewModel.TotalPrice);
			AssertEquals("Total price error should reflect BTC to AUD exchange issue", "No exchange rate between BTC and AUD", chargesViewModel.TotalPriceError);
			AssertEquals("Total price string should be \"0.00 AUD\" after first charge is selected", "0.00 AUD", chargesViewModel.TotalPriceString);
			Assert("ViewModel should be valid after selecting first charge", chargesViewModel.IsValid);

			var propertyChanges2 = new List<string>();
			chargesViewModel.PropertyChanged += (s, e) => propertyChanges2.Add(e.PropertyName);

			charge2.IsSelected = true;

			AssertCollectionContains("Property change for IsValid should be raised after second charge is selected", nameof(chargesViewModel.IsValid), propertyChanges2);
			AssertCollectionContains("Property change for TotalPrice should be raised after second charge is selected", nameof(chargesViewModel.TotalPrice), propertyChanges2);
			AssertCollectionContains("Property change for TotalPriceError should be raised after second charge is selected", nameof(chargesViewModel.TotalPriceError), propertyChanges2);
			AssertCollectionContains("Property change for TotalPriceString should be raised after second charge is selected", nameof(chargesViewModel.TotalPriceString), propertyChanges2);

			AssertEquals("Total price should be 200 after second charge is selected", 200m, chargesViewModel.TotalPrice);
			AssertEquals("Total price error should still reflect BTC to AUD exchange issue", "No exchange rate between BTC and AUD", chargesViewModel.TotalPriceError);
			AssertEquals("Total price string should be \"200.00 AUD\" after second charge is selected", "200.00 AUD", chargesViewModel.TotalPriceString);
			Assert("ViewModel should not be valid after selecting second charge", !chargesViewModel.IsValid);
		}
	}
}
