using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DefaultOrgTimetableSettingsCollection))]
	public class DefaultOrgTimetableSettingsCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DefaultOrgTimetableSettingsCollection>
	{
		protected override DefaultOrgTimetableSettingsCollection GetCollectionToTest()
		{
			return new DefaultOrgTimetableSettingsCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DefaultOrgTimetableSettings(Factory);
		}

		protected override bool RequiresFactory
		{
			get
			{
				return true;
			}
		}
		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		public void TestValidateCountry()
		{
			var defaultOrgTimetableSettingsCollection = new DefaultOrgTimetableSettingsCollection(Factory);
			var settings1 = new DefaultOrgTimetableSettings();
			Assert("Default country code is empty", settings1.CountryCode.IsEmpty);
			defaultOrgTimetableSettingsCollection.Add(settings1);
			settings1.ValidateCountryCode();
			AssertNoRowError(settings1, "Duplicate Country/Region Code.");
			AssertNoRowError(settings1, "No settings for default countries");

			var settings2 = new DefaultOrgTimetableSettings(Factory) { CountryCode = Core.Constants.CountryCodes.Australia };
			defaultOrgTimetableSettingsCollection.Add(settings2);
			settings2.ValidateCountryCode();
			AssertNoRowError(settings2, "Duplicate Country/Region Code.");
			AssertNoRowError(settings2, "No settings for default countries");

			var settings3 = new DefaultOrgTimetableSettings(Factory) { CountryCode = Core.Constants.CountryCodes.Australia };
			defaultOrgTimetableSettingsCollection.Add(settings3);
			settings3.ValidateCountryCode();
			AssertHasRowError(settings3, "Duplicate Country/Region Code.");
			AssertNoRowError(settings3, "No settings for default countries");

			settings3.CountryCode = Core.Constants.CountryCodes.Germany;
			settings3.ValidateCountryCode();
			AssertNoRowError(settings3, "Duplicate Country/Region Code.");
			AssertNoRowError(settings3, "No settings for default countries");

			settings1.CountryCode = Core.Constants.CountryCodes.Brazil;
			settings1.ValidateCountryCode();
			AssertNoRowError(settings1, "Duplicate Country/Region Code.");
			AssertHasRowError(settings1, "No settings for default countries");
		}

		public void TestDefaultOrgTimetablesForCountry()
		{
			var defaultOrgTimetableSettingsCollection = new DefaultOrgTimetableSettingsCollection(Factory);
			var settings1 = new DefaultOrgTimetableSettings(Factory);
			var settings2 = new DefaultOrgTimetableSettings(Factory) { CountryCode = Core.Constants.CountryCodes.Australia };
			defaultOrgTimetableSettingsCollection.Add(settings1);
			defaultOrgTimetableSettingsCollection.Add(settings2);

			AssertEquals(settings1.Timetables, defaultOrgTimetableSettingsCollection.DefaultOrgTimetablesForCountry(ZString.Empty));
			AssertEquals(settings2.Timetables, defaultOrgTimetableSettingsCollection.DefaultOrgTimetablesForCountry(Core.Constants.CountryCodes.Australia));
			AssertEquals(settings1.Timetables, defaultOrgTimetableSettingsCollection.DefaultOrgTimetablesForCountry(Core.Constants.CountryCodes.Germany));
		}
	}
}
