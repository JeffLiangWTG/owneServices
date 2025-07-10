using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EPaymentConfigurationCollection))]
	sealed class EPaymentConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<EPaymentConfigurationCollection>
	{
		public void TestAllowNew()
		{
			var collection = new EPaymentConfigurationCollection();
			Assert("Users should not be able to add new items to the collection.", !collection.AllowNew);
		}

		public void TestIsEPaymentEnabledForAnyProvider()
		{
			var collection1 = new EPaymentConfigurationCollection();
			collection1.PopulateDefaultConfiguration(Core.Constants.CountryCodes.Austria, "Austria");
			Assert(collection1.IsEPaymentEnabledForAnyProvider);

			var collection2 = new EPaymentConfigurationCollection();
			collection2.PopulateDefaultConfiguration(Core.Constants.CountryCodes.Afghanistan, "Afghanistan");
			Assert(!collection2.IsEPaymentEnabledForAnyProvider);

			var collection3 = new EPaymentConfigurationCollection();              // testing the empty collection
			Assert(!collection3.IsEPaymentEnabledForAnyProvider);
		}

		#region OFX

		public void TestIsOFXEPaymentEnabled()
		{
			var collection1 = new EPaymentConfigurationCollection();
			collection1.PopulateDefaultConfiguration(Core.Constants.CountryCodes.Austria, "Austria");
			Assert(collection1.IsOFXEPaymentEnabled);

			var collection2 = new EPaymentConfigurationCollection();
			collection2.PopulateDefaultConfiguration(Core.Constants.CountryCodes.Afghanistan, "Afghanistan");
			Assert(!collection2.IsOFXEPaymentEnabled);
		}

		public void TestOFXEPaymentConfiguration()
		{
			var countryCodeList = Country.LicenceKeyBuilderSupportedCountryCodes;
			foreach (var countryCode in countryCodeList)
			{
				var refCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
				AssertNotNull(refCountry);
				var countryDescription = refCountry.Description;

				var collection = new EPaymentConfigurationCollection();
				AssertEquals(0, collection.Count);
				collection.PopulateDefaultConfiguration(countryCode, countryDescription);
				AssertEquals(1, collection.Count);
				AssertEquals(countryCode, collection[0].CountryCode);
				AssertEquals(countryDescription, collection[0].CountryDescription);
				var isOFXEPaymentSupported = GetOFXSupportedCountries().Contains(countryCode);
				var message = $"OFX E-Payment for {countryDescription} should" + (isOFXEPaymentSupported ? "" : " NOT") + " be enabled.";
				AssertEquals(message, isOFXEPaymentSupported, collection[0].OFXEPaymentEnabled);
			}
		}

		List<string> GetOFXSupportedCountries()
		{
			return new List<string>()
			{
				Core.Constants.CountryCodes.Austria,
				Core.Constants.CountryCodes.Australia,
				Core.Constants.CountryCodes.Belgium,
				Core.Constants.CountryCodes.Canada,
				Core.Constants.CountryCodes.Switzerland,
				Core.Constants.CountryCodes.Cyprus,
				Core.Constants.CountryCodes.Germany,
				Core.Constants.CountryCodes.Denmark,
				Core.Constants.CountryCodes.Estonia,
				Core.Constants.CountryCodes.Spain,
				Core.Constants.CountryCodes.Finland,
				Core.Constants.CountryCodes.France,
				Core.Constants.CountryCodes.UnitedKingdom,
				Core.Constants.CountryCodes.Greece,
				Core.Constants.CountryCodes.HongKong,
				Core.Constants.CountryCodes.Hungary,
				Core.Constants.CountryCodes.Ireland,
				Core.Constants.CountryCodes.Italy,
				Core.Constants.CountryCodes.Liechtenstein,
				Core.Constants.CountryCodes.Lithuania,
				Core.Constants.CountryCodes.Luxembourg,
				Core.Constants.CountryCodes.Latvia,
				Core.Constants.CountryCodes.Malta,
				Core.Constants.CountryCodes.Netherlands,
				Core.Constants.CountryCodes.Norway,
				Core.Constants.CountryCodes.NewZealand,
				Core.Constants.CountryCodes.Poland,
				Core.Constants.CountryCodes.Portugal,
				Core.Constants.CountryCodes.Sweden,
				Core.Constants.CountryCodes.Singapore,
				Core.Constants.CountryCodes.Slovenia,
				Core.Constants.CountryCodes.Slovakia,
				Core.Constants.CountryCodes.UnitedStates
			};
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override EPaymentConfigurationCollection GetCollectionToTest() => new EPaymentConfigurationCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new EPaymentConfiguration();

		#endregion
	}
}
