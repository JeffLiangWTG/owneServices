using System;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.MasterFiles.Business.CountryCompliance.CountryComplianceInfoDisplay;
using Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class CountrySpecificRegistryDefaultValueTestHelper
	{
		public static void AssertCountrySpecificRegistryItemDefaultValues<TGet, TSet>(StronglyTypedRegistryItem<TGet, TSet> registryItem
			, Expression<Func<IDefaultValuesForCountrySpecificRegistryItems, TGet>> expression
			, (string displayValue, TGet testValue) displayAndTestValue1
			, (string displayValue, TGet testValue) displayAndTestValue2
			, string caption
			, int startingCallCount = 1)
		{
			var mockICountrySpecificRegistryDefaultValuesProvider = new Mock<ICountrySpecificRegistryDefaultValuesProvider>();
			ObjectFactory.Substitute(mockICountrySpecificRegistryDefaultValuesProvider.Object);

			var mockIDefaultValuesForCountrySpecificRegistryItems = new Mock<IDefaultValuesForCountrySpecificRegistryItems>();
			mockICountrySpecificRegistryDefaultValuesProvider.Setup(x => x.Get(It.IsAny<ZString>())).Returns(mockIDefaultValuesForCountrySpecificRegistryItems.Object);

			Assertion.AssertType<CountrySpecificDefaultValueRegistryItemImpl<TSet>>(registryItem.Inner);
			Assertion.AssertNoExceptionThrown(() => { var val = registryItem.Value; });
			int expressionCallCount = startingCallCount;
			mockIDefaultValuesForCountrySpecificRegistryItems.Verify(expression, Times.Exactly(expressionCallCount));

			Assertion.AssertEquals("Registry Caption", caption, registryItem.Caption);
			Assertion.Assert("HasOption CacheExpensiveDefaultValue", registryItem.HasOption(Enterprise.Integration.RegistryOptions.CacheExpensiveDefaultValue));

			mockIDefaultValuesForCountrySpecificRegistryItems.Setup(expression).Returns(displayAndTestValue1.testValue);
			var countryComplianceInfoDisplayTestObject = new CountryComplianceInfoDisplay();
			AssertCountrySpecificRegistryDefaultValueAfterCountryChange(Core.Constants.CountryCodes.Australia, displayAndTestValue1.displayValue, ++expressionCallCount);

			mockIDefaultValuesForCountrySpecificRegistryItems.Setup(expression).Returns(displayAndTestValue2.testValue);
			AssertCountrySpecificRegistryDefaultValueAfterCountryChange(Core.Constants.CountryCodes.India, displayAndTestValue2.displayValue, ++expressionCallCount);

			AssertCountrySpecificRegistryDefaultValueForCaching(Core.Constants.CountryCodes.Australia, displayAndTestValue1.displayValue, expressionCallCount);

			void AssertCountrySpecificRegistryDefaultValueAfterCountryChange(string countryCode, string displayValue, int expectedExpressionCallCount)
			{
				countryComplianceInfoDisplayTestObject.CountryCode = countryCode;
				mockIDefaultValuesForCountrySpecificRegistryItems.Verify(expression, Times.Exactly(expectedExpressionCallCount));
				mockICountrySpecificRegistryDefaultValuesProvider.Verify(x => x.Get(countryCode));
				var result = countryComplianceInfoDisplayTestObject.CountrySpecificRegistryDefaultValues.Find(x => x.Caption == caption);
				Assertion.AssertEquals("Count Of Caption In Default Registry Of Compliance Form ", 1, result.Count());
				Assertion.AssertEquals("Default Value Of The Registry Type In Compliance Form ", displayValue, result.First().DefaultValue);
			}

			void AssertCountrySpecificRegistryDefaultValueForCaching(string countryCode, string displayValue, int expectedExpressionCallCount)
			{
				countryComplianceInfoDisplayTestObject.CountryCode = countryCode;
				mockIDefaultValuesForCountrySpecificRegistryItems.Verify(expression, Times.Exactly(expectedExpressionCallCount));
				var result = countryComplianceInfoDisplayTestObject.CountrySpecificRegistryDefaultValues.Find(x => x.Caption == caption);
				Assertion.AssertEquals("Caching Check, Count Of Caption In Default Registry Of Compliance Form ", 1, result.Count());
				Assertion.AssertEquals("Caching Check, Default Value Of The Registry Type In Compliance Form ", displayValue, result.First().DefaultValue);
			}
		}
	}
}
