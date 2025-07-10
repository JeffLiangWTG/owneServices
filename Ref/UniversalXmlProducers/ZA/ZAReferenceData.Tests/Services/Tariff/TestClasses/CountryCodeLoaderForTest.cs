using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Interfaces;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Loader;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common;
using Moq;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestClasses
{
	internal class CountryCodeLoaderForTest : CountryCodeLoader
	{
		public CountryCodeLoaderForTest() : this(SetupMockMatcher(), new TestLogger()) { }
		public CountryCodeLoaderForTest(ILogger logger) : this(SetupMockMatcher(), logger) { }
		public CountryCodeLoaderForTest(ICountryMatcher countryMatcher, ILogger logger) : base(countryMatcher, logger)
		{
		}

		static ICountryMatcher SetupMockMatcher()
		{
			var mockCountryMatcher = new Mock<ICountryMatcher>();

			mockCountryMatcher.Setup(m => m.GetCountryCodes(It.IsAny<string[]>())).Returns((string[] countryCodes) =>
			{
				var data = new List<string>();

				if (countryCodes.Contains("ARGENTINA"))	{ data.Add("AR"); }
				if (countryCodes.Contains("BRAZIL")) { data.Add("BR"); }
				if (countryCodes.Contains("CHINA")) { data.Add("CN"); }
				if (countryCodes.Contains("EGYPT")) { data.Add("EG"); }
				if (countryCodes.Contains("GERMANY")) { data.Add("DE"); }
				if (countryCodes.Contains("INDIA")) { data.Add("IN"); }
				if (countryCodes.Contains("INDONESIA")) { data.Add("ID"); }
				if (countryCodes.Contains("MALAYSIA")) { data.Add("MY"); }
				if (countryCodes.Contains("NETHERLANDS"))	{ data.Add("NL"); }
				if (countryCodes.Contains("PAKISTAN"))	{ data.Add("PK"); }
				if (countryCodes.Contains("PARAGUAY"))	{ data.Add("PY"); }
				if (countryCodes.Contains("REPUBLIC OF CHINA"))	{ data.Add("CN"); }
				if (countryCodes.Contains("REPUBLIC OF KOREA"))	{ data.Add("KR"); }
				if (countryCodes.Contains("SAUDI ARABIA"))	{ data.Add("SA"); }
				if (countryCodes.Contains("TAIWAN")) { data.Add("TW"); }
				if (countryCodes.Contains("THAILAND")) { data.Add("TH"); }
				if (countryCodes.Contains("UNITED ARAB EMIRATES"))	{ data.Add("AE"); }
				if (countryCodes.Contains("UNITED KINGDOM")) { data.Add("UK"); }
				if (countryCodes.Contains("UNITED STATES OF AMERICA")) { data.Add("US"); }
				if (countryCodes.Contains("URUGUAY")) { data.Add("UY"); }

				return data.ToArray();
			});

			return mockCountryMatcher.Object;
		}
	}
}
