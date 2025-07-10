
using System.Globalization;

namespace Enterprise.MasterFiles.Business
{
	using System;
	using System.Collections.Generic;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using NUnit.Framework;

#if DEBUG

	public class DecimalPlacesAttributeTester
	{
		public DecimalPlacesAttributeTester(BusinessObject objectToTest, GlbCompany company = null)
			: base()
		{
			this.objectToTest = objectToTest ?? throw new ArgumentException("Object used for testing is null");
			this.company = company ?? GlbCompany.CurrentCompany ?? throw new ArgumentException("No Company available to use");
		}

		BusinessObject objectToTest { get; set; }
		GlbCompany company { get; set; }

		#region Checks

		#region LocalCurrency

		public void CheckLocalCurrency(List<string> propertiesToTest, string decimalSetterProperty)
		{
			CheckSetter(propertiesToTest, decimalSetterProperty);

			company.TemporarilySetCountry(Core.Constants.CountryCodes.Bahrain);
			Assertion.AssertEquals(string.Format(CultureInfo.InvariantCulture, "{0} should match local currency", decimalSetterProperty), 3, (int)GetValue(decimalSetterProperty));
			company.TemporarilySetCountry(Core.Constants.CountryCodes.Australia);
			Assertion.AssertEquals(string.Format(CultureInfo.InvariantCulture, "{0} should match local currency", decimalSetterProperty), 2, (int)GetValue(decimalSetterProperty));
		}

		#endregion

		#region CompanyLocalCurrency

		public void CheckCompanyLocalCurrency(List<string> propertiesToTest, string decimalSetterProperty)
		{
			CheckSetter(propertiesToTest, decimalSetterProperty);

			company.TemporarilySetCountry(Core.Constants.CountryCodes.Bahrain);
			Assertion.AssertEquals(string.Format(CultureInfo.InvariantCulture, "{0} should match local currency", decimalSetterProperty), 3, (int)GetValue(decimalSetterProperty));
			company.TemporarilySetCountry(Core.Constants.CountryCodes.Australia);
			Assertion.AssertEquals(string.Format(CultureInfo.InvariantCulture, "{0} should match local currency", decimalSetterProperty), 2, (int)GetValue(decimalSetterProperty));
		}

		#endregion

		#region NonLocalCurrency

		public void CheckNonLocalCurrency(List<string> propertiesToTest, string decimalSetterProperty, string relatedCurrency, object relatedCurrencyHolder)
		{
			CheckSetter(propertiesToTest, decimalSetterProperty);

			Assertion.Assert("Requires currency to not be null", !String.IsNullOrEmpty(relatedCurrency));

			company.TemporarilySetCountry(Core.Constants.CountryCodes.Bahrain);
			SetCurrency(relatedCurrencyHolder, relatedCurrency, (ZString)Core.Constants.CurrencyCodes.Australia);
			Assertion.AssertEquals(String.Format(CultureInfo.InvariantCulture, "{0} should match currency", decimalSetterProperty), 2, (int)GetValue(decimalSetterProperty));

			company.TemporarilySetCountry(Core.Constants.CountryCodes.Australia);
			SetCurrency(relatedCurrencyHolder, relatedCurrency, (ZString)Core.Constants.CurrencyCodes.Bahrain);
			Assertion.AssertEquals(String.Format(CultureInfo.InvariantCulture, "{0} should match currency", decimalSetterProperty), 3, (int)GetValue(decimalSetterProperty));
		}

		#endregion

		#region ExchangeRate

		public void CheckExchangeRate(List<string> propertiesToTest, string decimalSetterProperty)
		{
			CheckSetter(propertiesToTest, decimalSetterProperty);

			company.GC_IsReciprocal = true;
			Assertion.AssertEquals(string.Format(CultureInfo.InvariantCulture, "{0} should be equal to Company.ExchangeRateDecimalPlaces", decimalSetterProperty), company.ExchangeRateDecimalPlaces, (int)GetValue(decimalSetterProperty));
			company.GC_IsReciprocal = false;
			Assertion.AssertEquals(string.Format(CultureInfo.InvariantCulture, "{0} should be equal to Company.ExchangeRateDecimalPlaces", decimalSetterProperty), company.ExchangeRateDecimalPlaces, (int)GetValue(decimalSetterProperty));
		}

		#endregion

		#region Constant

		public void CheckConstant(List<string> propertiesToTest, string decimalSetterProperty, int constant)
		{
			CheckSetter(propertiesToTest, decimalSetterProperty);
			Assertion.AssertEquals(string.Format(CultureInfo.InvariantCulture, "{0} should equal {1}", decimalSetterProperty, constant), constant, (int)GetValue(decimalSetterProperty));
		}

		#endregion

		#region Setter

		public void CheckSetter(List<string> propertiesToTest, string decimalSetterProperty)
		{
			propertiesToTest.ForEach(x => Assertion.AssertEquals(string.Format(CultureInfo.InvariantCulture, "{0} Property should have the correct Decimal Places Member", x), decimalSetterProperty, GetDecimalMember(x)));
		}

		#endregion

		#endregion

		#region General Functions

		string GetDecimalMember(string propertyToTest)
		{
			var result = "";
			var info = objectToTest.GetType().GetProperty(propertyToTest)
				?? throw new AssertionFailedError(String.Format(CultureInfo.InvariantCulture, "property {0} not found in Type {1}", propertyToTest, objectToTest.GetType().Name));
			var attr = Attribute.GetCustomAttributes(info, typeof(DecimalPlacesAttribute), true);
			result = attr.Length > 0 ? ((DecimalPlacesAttribute)attr[0]).DecimalPlacesMember : "";
			return result;
		}

		object GetValue(string propertyName)
		{
			var info = objectToTest.GetType().GetProperty(propertyName) ?? throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Can't find property {0}", propertyName));
			return info.GetValue(objectToTest);
		}

		void SetCurrency(object currencyHolder, string currencyName, object value)
		{
			var info = currencyHolder.GetType().GetProperty(currencyName) ?? throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Can't find property {0} in {1}", currencyName, currencyHolder));
			info.SetValue(currencyHolder, value);
		}

		#endregion

	}

#endif

}
