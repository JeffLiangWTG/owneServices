using System.Collections.Generic;
using System.Linq;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.Testing.GUITests.RateSelector.Models
{
	public abstract class RateViewModelTest : RatingTestCase
	{
		public void TestIsExpanded_ShouldRaisePropertyChangedWhenChanged()
		{
			var rate = GetInstance();

			AssertEquals("By default", false, rate.IsExpanded);

			var propertyChanges1 = new List<string>();
			rate.PropertyChanged += (s, e) => propertyChanges1.Add(e.PropertyName);

			rate.IsExpanded = true;

			AssertCollectionContains("Property change event should be raised for IsExpanded", nameof(rate.IsExpanded), propertyChanges1);
			AssertEquals(true, rate.IsExpanded);

			var propertyChanges2 = new List<string>();
			rate.PropertyChanged += (s, e) => propertyChanges2.Add(e.PropertyName);

			rate.IsExpanded = false;

			AssertCollectionContains("Property change event should be raised for IsExpanded", nameof(rate.IsExpanded), propertyChanges2);
			AssertEquals(false, rate.IsExpanded);
		}

		public void TestDisplayPrice()
		{
			var originalValue = Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed;

			try
			{
				var rate = GetInstance();

				Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed = false;
				AssertEquals("DisplayPrice should be false when IsAllowed is set to false", false, rate.DisplayPrice);

				Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed = true;
				AssertEquals("DisplayPrice should be true when IsAllowed is set to true", true, rate.DisplayPrice);
			}
			finally
			{
				Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed = originalValue;
			}
		}

		public void TestTotalPriceString_ShouldSumLocalAmountsOfSelectedCharges()
		{
			var rate = GetInstance();

			rate.FreightCharges.Clear();
			rate.FreightCharges.Add(new ChargeViewModelSample
			{
				LocalAmount = 50,
				LocalAmountError = null,
				IsOptional = false,
				IsSelected = true
			});

			rate.OptionalCharges.Clear();
			rate.OptionalCharges.Add(new ChargeViewModelSample { LocalAmount = 100, LocalAmountError = null, IsOptional = true, IsSelected = true });
			rate.OptionalCharges.Add(new ChargeViewModelSample { LocalAmount = 200, LocalAmountError = null, IsOptional = true, IsSelected = true });
			rate.OptionalCharges.Add(new ChargeViewModelSample { LocalAmount = 66, LocalAmountError = null, IsOptional = true, IsSelected = false });
			rate.OptionalCharges.Add(new ChargeViewModelSample { LocalAmount = 999, LocalAmountError = null, IsOptional = true, IsSelected = false });

			AssertEquals("Total price string should match expected value", "350.00 AUD", rate.TotalPriceString);
			AssertNullOrEmpty("Total price error should be null or empty", rate.TotalPriceError);

			var propertyChanges = new List<string>();
			rate.PropertyChanged += (s, e) => propertyChanges.Add(e.PropertyName);

			rate.OtherCharges.Add(new ChargeViewModelSample { LocalAmount = 0, LocalAmountError = "No rate from USD to AUD", IsOptional = true, IsSelected = true });
			rate.OtherCharges.Add(new ChargeViewModelSample { LocalAmount = 150, LocalAmountError = null, IsOptional = true, IsSelected = true });
			rate.OtherCharges.Add(new ChargeViewModelSample { LocalAmount = 0, LocalAmountError = "No rate from ETH to AUD", IsOptional = true, IsSelected = true });
			rate.OtherCharges.Add(new ChargeViewModelSample { LocalAmount = 0, LocalAmountError = "No rate from EUR to AUD", IsOptional = true, IsSelected = false });

			AssertCollectionContains(
				"Rate should raise property change for TotalPriceString",
				"TotalPriceString",
				propertyChanges
			);
			AssertCollectionContains(
				"Rate should raise property change for TotalPriceError",
				"TotalPriceError",
				propertyChanges
			);
			AssertEquals("Total price string should reflect updated value", "500.00 AUD", rate.TotalPriceString);
			AssertEquals(
				"Total price error should aggregate errors from selected charges",
				@"No rate from USD to AUD
No rate from ETH to AUD",
				rate.TotalPriceError
			);
		}

		public void TestTotalPriceString_ChargeSelected_ShouldChangeAndRaiseNotifyPropertyChanged()
		{
			var rate = GetInstance();

			rate.FreightCharges.Clear();
			rate.FreightCharges.Add(new ChargeViewModelSample { LocalAmount = 50, LocalAmountError = null, IsOptional = false, IsSelected = true });

			var ch1 = new ChargeViewModelSample { LocalAmount = 100, LocalAmountError = null, IsOptional = true, IsSelected = true };
			var ch2 = new ChargeViewModelSample { LocalAmount = 200, LocalAmountError = null, IsOptional = true, IsSelected = true };
			var ch3 = new ChargeViewModelSample { LocalAmount = 66, LocalAmountError = null, IsOptional = true, IsSelected = false };
			var ch4 = new ChargeViewModelSample { LocalAmount = 0, LocalAmountError = "No rate from USD to AUD", IsOptional = true, IsSelected = false };

			rate.OptionalCharges.Clear();
			rate.OptionalCharges.Add(ch1);
			rate.OptionalCharges.Add(ch2);
			rate.OptionalCharges.Add(ch3);
			rate.OptionalCharges.Add(ch4);

			AssertEquals("Expected total price string to match", "350.00 AUD", rate.TotalPriceString);
			AssertNullOrEmpty("Expected total price error to be null or empty", rate.TotalPriceError);

			var propertyChanges1 = new List<string>();
			rate.PropertyChanged += (s, e) => propertyChanges1.Add(e.PropertyName);

			ch3.IsSelected = true;

			AssertEquals("Expected property change for TotalPriceString", 1, propertyChanges1.Count(p => p == "TotalPriceString"));
			AssertEquals("Expected property change for TotalPriceError", 1, propertyChanges1.Count(p => p == "TotalPriceError"));

			AssertEquals("Expected updated total price string", "416.00 AUD", rate.TotalPriceString);
			AssertNullOrEmpty("Expected total price error to be null or empty after selection change", rate.TotalPriceError);

			var propertyChanges2 = new List<string>();
			rate.PropertyChanged += (s, e) => propertyChanges2.Add(e.PropertyName);

			ch4.IsSelected = true;

			AssertEquals("Expected property change for TotalPriceString", 1, propertyChanges2.Count(p => p == "TotalPriceString"));
			AssertEquals("Expected property change for TotalPriceError", 1, propertyChanges2.Count(p => p == "TotalPriceError"));

			AssertEquals("Expected total price string to remain unchanged", "416.00 AUD", rate.TotalPriceString);
			AssertEquals("Expected correct total price error message", "No rate from USD to AUD", rate.TotalPriceError);
		}

		protected RateSelectorFilterStripBusinessObject GetValidFilters()
		{
			var criteria = new TestRatingCriteria("AUSYD", "UAIEV", FreightMode.AIR, 66, 9, null);
			return new RateSelectorFilterStripBusinessObject(criteria, new TestLogger());
		}

		protected abstract RateViewModel GetInstance();
	}
}
