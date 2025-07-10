using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefCurrencyFilterBusinessObject))]
	sealed class RefCurrencyFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TextFilterTests

		public void TestCodeFilter()
		{
			RefCurrency code1 = Factory.NewWithValidTestData<RefCurrency>();
			RefCurrency code2 = Factory.NewWithValidTestData<RefCurrency>();
			code1.RX_Code = "Ind";
			code2.RX_Code = "Som";

			Factory.Save();

			RefCurrencyFilterBusinessObject filter = new RefCurrencyFilterBusinessObject();
			((ModuleTextFilter)filter["Code"]).Property = "Ind";
			((ModuleTextFilter)filter["Code"]).IsActive = true;

			RefCurrencyCollection currencies = new RefCurrencyCollection(Factory);
			currencies.AdditionalFilter = filter.Filter;

			AssertCollectionContains(code1, currencies);
			AssertCollectionNotContains(code2, currencies);
		}

		public void TestDescriptionFilter()
		{
			RefCurrency description1 = Factory.NewWithValidTestData<RefCurrency>();
			RefCurrency description2 = Factory.NewWithValidTestData<RefCurrency>();
			description1.RX_Desc = "Ind";
			description2.RX_Desc = "Som";

			Factory.Save();

			RefCurrencyFilterBusinessObject filter = new RefCurrencyFilterBusinessObject();
			((ModuleTextFilter)filter["Description"]).Property = "Ind";
			((ModuleTextFilter)filter["Description"]).IsActive = true;

			RefCurrencyCollection currencies = new RefCurrencyCollection(Factory);
			currencies.AdditionalFilter = filter.Filter;

			AssertCollectionContains(description1, currencies);
			AssertCollectionNotContains(description2, currencies);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1829:Use Length/Count property instead of Count() when available", Justification = "Test method")]
		public void TestCFXCalculationsStatusFilterWithNoCurrencyExcludedFromCFXCalculations()
		{
			var filter = new RefCurrencyFilterBusinessObject();
			((ModuleTextFilter)filter["CFX Calculations Status"]).Property = "EXC";
			((ModuleTextFilter)filter["CFX Calculations Status"]).IsActive = true;
			var currencies = new RefCurrencyCollection(Factory);
			currencies.AdditionalFilter = filter.Filter;
			AssertNoExceptionThrown(() => currencies.Count());

			((ModuleTextFilter)filter["CFX Calculations Status"]).Property = "INC";
			currencies = new RefCurrencyCollection(Factory);
			currencies.AdditionalFilter = filter.Filter;
			AssertNoExceptionThrown(() => currencies.Count());
		}

		public void TestCFXCalculationsStatusFilter()
		{
			var currency1 = Factory.NewWithValidTestData<RefCurrency>();
			currency1.RX_Code = "ABC";
			currency1.RX_IsExcludedCFXCalculation = true;

			var currency2 = Factory.NewWithValidTestData<RefCurrency>();
			currency2.RX_Code = "XYZ";

			Factory.Save();

			var filter = new RefCurrencyFilterBusinessObject();
			((ModuleTextFilter)filter["CFX Calculations Status"]).Property = "EXC";
			((ModuleTextFilter)filter["CFX Calculations Status"]).IsActive = true;
			var currencies = new RefCurrencyCollection(Factory);
			currencies.AdditionalFilter = filter.Filter;

			AssertCollectionContains("Should contain only currency1", currency1, currencies);
			AssertCollectionNotContains("Should not contain currency2", currency2, currencies);

			((ModuleTextFilter)filter["CFX Calculations Status"]).Property = "INC";
			currencies = new RefCurrencyCollection(Factory);
			currencies.AdditionalFilter = filter.Filter;

			AssertCollectionContains("Should contain only currency2", currency2, currencies);
			AssertCollectionNotContains("Should not contain currency1", currency1, currencies);

			((ModuleTextFilter)filter["CFX Calculations Status"]).Property = "ALL";
			currencies = new RefCurrencyCollection(Factory);
			currencies.AdditionalFilter = filter.Filter;

			AssertCollectionContains("Should contain currency1", currency1, currencies);
			AssertCollectionContains("Should contain currency2", currency2, currencies);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefCurrencyFilterBusinessObject();
		}

		#endregion
	}
}
