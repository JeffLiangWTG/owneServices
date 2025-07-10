using System.Globalization;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ICountryTest : TestCaseWithFactory
	{
		public void TestAllSupportedCountriesHaveCultures()
		{
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "AE")).Culture.Name, "ar-AE");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "AL")).Culture.Name, "sq-AL");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "AM")).Culture.Name, "hy-AM");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "AR")).Culture.Name, "es-AR");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "AT")).Culture.Name, "de-AT");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "AU")).Culture.Name, "en-AU");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "AZ")).Culture.Name, "az");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "BE")).Culture.Name, "fr-BE");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "BG")).Culture.Name, "bg-BG");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "BH")).Culture.Name, "ar-BH");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "BN")).Culture.Name, "ms-BN");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "BO")).Culture.Name, "es-BO");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "BR")).Culture.Name, "pt-BR");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "BY")).Culture.Name, "be-BY");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "BZ")).Culture.Name, "en-BZ");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "CA")).Culture.Name, "en-CA");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "CH")).Culture.Name, "de-CH");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "CL")).Culture.Name, "es-CL");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "CN")).Culture.Name, "zh-CN");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "CO")).Culture.Name, "es-CO");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "CR")).Culture.Name, "es-CR");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "CZ")).Culture.Name, "cs-CZ");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "DE")).Culture.Name, "de-DE");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "DK")).Culture.Name, "da-DK");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "DO")).Culture.Name, "es-DO");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "DZ")).Culture.Name, "ar-DZ");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "EC")).Culture.Name, "es-EC");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "EE")).Culture.Name, "et-EE");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "EG")).Culture.Name, "ar-EG");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "ES")).Culture.Name, "es-ES");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "FI")).Culture.Name, "fi-FI");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "FO")).Culture.Name, "fo-FO");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "FR")).Culture.Name, "fr-FR");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "GB")).Culture.Name, "en-GB");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "GE")).Culture.Name, "ka-GE");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "GR")).Culture.Name, "el-GR");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "GT")).Culture.Name, "es-GT");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "HK")).Culture.Name, "zh-HK");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "HN")).Culture.Name, "es-HN");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "HR")).Culture.Name, "hr-HR");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "HU")).Culture.Name, "hu-HU");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "ID")).Culture.Name, "id-ID");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "IE")).Culture.Name, "en-IE");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "IL")).Culture.Name, "he-IL");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "IN")).Culture.Name, "hi-IN");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "IQ")).Culture.Name, "ar-IQ");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "IR")).Culture.Name, "fa-IR");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "IS")).Culture.Name, "is-IS");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "IT")).Culture.Name, "it-IT");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "JM")).Culture.Name, "en-JM");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "JO")).Culture.Name, "ar-JO");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "JP")).Culture.Name, "ja-JP");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "KE")).Culture.Name, "sw-KE");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "KG")).Culture.Name, "ky-KG");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "KR")).Culture.Name, "ko-KR");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "KW")).Culture.Name, "ar-KW");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "KZ")).Culture.Name, "kk-KZ");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "LB")).Culture.Name, "ar-LB");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "LI")).Culture.Name, "de-LI");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "LT")).Culture.Name, "lt-LT");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "LU")).Culture.Name, "de-LU");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "LV")).Culture.Name, "lv-LV");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "LY")).Culture.Name, "ar-LY");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "MA")).Culture.Name, "ar-MA");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "MC")).Culture.Name, "fr-MC");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "MK")).Culture.Name, "mk-MK");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "MN")).Culture.Name, "mn-MN");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "MO")).Culture.Name, "zh-MO");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "MX")).Culture.Name, "es-MX");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "MY")).Culture.Name, "ms-MY");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "NA")).Culture.Name, "en-US");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "NI")).Culture.Name, "es-NI");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "NL")).Culture.Name, "nl-NL");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "NO")).Culture.Name, "nb-NO");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "NZ")).Culture.Name, "en-NZ");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "OM")).Culture.Name, "ar-OM");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "PA")).Culture.Name, "es-PA");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "PE")).Culture.Name, "es-PE");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "PG")).Culture.Name, "en-US");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "PH")).Culture.Name, "en-PH");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "PK")).Culture.Name, "ur-PK");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "PL")).Culture.Name, "pl-PL");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "PR")).Culture.Name, "es-PR");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "PT")).Culture.Name, "pt-PT");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "PY")).Culture.Name, "es-PY");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "QA")).Culture.Name, "ar-QA");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "RO")).Culture.Name, "ro-RO");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "RU")).Culture.Name, "ru-RU");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "SA")).Culture.Name, "ar-SA");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "SE")).Culture.Name, "sv-SE");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "SG")).Culture.Name, "zh-SG");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "SI")).Culture.Name, "sl-SI");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "SK")).Culture.Name, "sk-SK");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "SV")).Culture.Name, "es-SV");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "SY")).Culture.Name, "syr-SY");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "TH")).Culture.Name, "th-TH");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "TN")).Culture.Name, "ar-TN");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "TR")).Culture.Name, "tr-TR");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "TT")).Culture.Name, "en-TT");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "TW")).Culture.Name, "zh-TW");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "UA")).Culture.Name, "uk-UA");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "US")).Culture.Name, "en-US");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "UY")).Culture.Name, "es-UY");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "UZ")).Culture.Name, "uz");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "VE")).Culture.Name, "es-VE");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "VN")).Culture.Name, "vi-VN");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "YE")).Culture.Name, "ar-YE");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "ZA")).Culture.Name, "en-ZA");
			AssertEquals("Wrong culture for country.", ((ICountry)RefCountry.LoadFromCountryCode(Factory, "ZW")).Culture.Name, "en-ZW");
		}

		public void TestCorrectSeparatorsUsedForMalaysia()
		{
			CultureInfo malaysianCulture = ((ICountry)RefCountry.LoadFromCountryCode(Factory, "MY")).Culture;
			AssertEquals("Incorrect Number Decimal Separator for Malaysia", ".", malaysianCulture.NumberFormat.NumberDecimalSeparator);
			AssertEquals("Incorrect Number Group Separator for Malaysia", ",", malaysianCulture.NumberFormat.NumberGroupSeparator);
			AssertEquals("Incorrect Currency Decimal Separator for Malaysia", ".", malaysianCulture.NumberFormat.CurrencyDecimalSeparator);
			AssertEquals("Incorrect Currency Group Separator for Malaysia", ",", malaysianCulture.NumberFormat.CurrencyGroupSeparator);
		}
	}
}
