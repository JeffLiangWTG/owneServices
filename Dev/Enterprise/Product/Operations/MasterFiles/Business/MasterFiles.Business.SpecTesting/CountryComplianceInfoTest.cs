#pragma warning disable CW1161 // Res.GetString Analyzer - Strings are ok here

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Environment;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.TestHelpers.SpecTesting;

namespace Enterprise.MasterFiles.Business.SpecTesting
{
	// NOTE: We use human-readable property names rather than codenames because this is the "spec", designed for humans to read.
	// It's never parsed again, it's compared via a string comparison.
	public class CountryComplianceInfoDataForCountry
	{
		[JsonProperty("Is supported by the license system")]
		public bool IsSupportedByLicenseSystem { get; set; }
		[JsonProperty("Consumption tax code")]
		public string ConsumptionTaxCode { get; set; }
		[JsonProperty("Consumption tax registration code")]
		public string ConsumptionTaxRegistrationCode { get; set; }
		[JsonProperty("Local business registration number code type")]
		public string LocalBusinessRegNoCodeType { get; set; }
		[JsonProperty("Is right hand side address country")]
		public bool IsRightHandSideAddressCountry { get; set; }
		[JsonProperty("Is currency conversion reciprocal")]
		public bool? IsReciprocal { get; set; }
	}

	public class ComplianceDataSpecGenerator : ISpecProvider
	{
		public string SpecFolderPath => "Enterprise/Product/Operations/MasterFiles/Business/MasterFiles.Business.SpecTesting/Spec/Accounting/Countries/CountryComplianceInfo";
		public string SpecNamespacePrefix => "Enterprise.MasterFiles.Business.SpecTesting.Spec.Accounting.Countries.CountryComplianceInfo";
		public string RegenExecutableName => "MasterFiles.Business.SpecTesting.Regenerate.exe";

		string[] GetSpecTestedCountries()
		{
			var defaultCountries = Constants.CountryCodes.GetAll().ToList();

			// Add the "fallback" country test
			defaultCountries.Add("__");

			return defaultCountries.ToArray();
		}

		CountryComplianceInfoDataForCountry GetForCountry(string countryCode)
		{
			var result = new CountryComplianceInfoDataForCountry();

			result.IsReciprocal = GetIsReciprocal(countryCode);

			result.IsSupportedByLicenseSystem = Country.IsSupportedForLicenceBuilder(countryCode);
			result.ConsumptionTaxCode = Country.GetConsumptionTaxDescription(countryCode);
			result.ConsumptionTaxRegistrationCode = Country.GetConsumptionTaxRegistrationOrgCusCode(countryCode);
			result.IsRightHandSideAddressCountry = CountryComplianceInfo.GetIsRightHandSideAdressCountry(countryCode);
			result.LocalBusinessRegNoCodeType = CountryComplianceInfo.GetLocalBusinessRegNoCodeType(countryCode);

			return result;
		}

		string GetSpecTextForCountry(string countryCode)
		{
			var spec = GetForCountry(countryCode);
			var specText = JsonConvert.SerializeObject(spec, Formatting.Indented);

			if (countryCode == "__")
			{
				specText = "// This is the default \"fallback\" country case, for when no country was matched\r\n\r\n" + specText;
			}

			return specText;
		}

		bool? GetIsReciprocal(string countryCode)
		{
			try
			{
				var isReciprocal = Country.IsReciprocal(countryCode);
				return isReciprocal;
			}
			catch (NotSupportedException)
			{
				return null;
			}
		}

		public IEnumerable<SpecFile> GetSpecFiles()
		{
			var allCountries = GetSpecTestedCountries();
			foreach (var country in allCountries)
			{
				var filename = $"{country}.json";
				var countryData = GetSpecTextForCountry(country);
				yield return new SpecFile(filename, countryData);
			}
		}
	}

	class CountryComplianceInfoTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCountrySpecNotBroken()
		{
			SpecAssert.AssertAllSpecMatches(Assembly.GetExecutingAssembly(), new ComplianceDataSpecGenerator());
		}
	}
}
