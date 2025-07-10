using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.Business.Testing
{
	sealed class OrganizationAddressRegistrationNumberProviderTest : TestCaseWithFactory
	{
		public void TestGetTaxNumber()
		{
			var address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				GovRegNumType = new RegistrationNumberType() { Code = "AIN" },
				Country = new Country() { Code = "BD" },
				GovRegNum = "12345",
				AddressType = "NotifyParty"
			};

			IRegistrationNumberProvider provider = new OrganizationAddressRegistrationNumberProvider(address);

			AssertEquals("12345", provider.GetTaxNumber("BD", "AIN"));
		}

		public void TestGetTaxNumber_NoNullReferenceException()
		{
			var address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { };

			IRegistrationNumberProvider provider = new OrganizationAddressRegistrationNumberProvider(address);
			AssertEquals("", provider.GetTaxNumber("BD", "AIN"));

			address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			address.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
				{
					null,
					new RegistrationNumber() { },
					new RegistrationNumber()
					{
						Type = new RegistrationNumberType() { },
						CountryOfIssue = new Country() { }
					},
					new RegistrationNumber()
					{
						Type = new RegistrationNumberType() { Code = "AIN" }
					},
					new RegistrationNumber()
					{
						Type = new RegistrationNumberType() { Code = "AIN" },
						CountryOfIssue = new Country() { Code = "BD" }
					},
					new RegistrationNumber()
					{
						Type = new RegistrationNumberType() { Code = "AIN" },
						CountryOfIssue = new Country() { Code = "BD" },
						Value = "23456"
					}
				});

			provider = new OrganizationAddressRegistrationNumberProvider(address);
			AssertEquals("23456", provider.GetTaxNumber("BD", "AIN"));
		}

		public void TestGetTaxNumber_TaxNumberInRegistrationNumberCollection()
		{
			var address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Country = new Country() { Code = "BD" },
				AddressType = "NotifyParty"
			};
			address.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
				{
					new RegistrationNumber()
					{
						Type = new RegistrationNumberType() { Code = "AIN" },
						CountryOfIssue = new Country() { Code = "BD" },
						Value = "23456"
					}
			});

			IRegistrationNumberProvider provider = new OrganizationAddressRegistrationNumberProvider(address);

			AssertEquals("23456", provider.GetTaxNumber("BD", "AIN"));
		}

		public void TestGetTaxNumber_NoTaxNumber()
		{
			var address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				GovRegNumType = new RegistrationNumberType() { Code = "AIN" },
				Country = new Country() { Code = "BD" },
				AddressType = "NotifyParty"
			};

			IRegistrationNumberProvider provider = new OrganizationAddressRegistrationNumberProvider(address);

			AssertEquals("", provider.GetTaxNumber("BD", "AIN"));
		}

		public void TestGetTaxNumber_BangladeshFallback()
		{
			var address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			address.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
				{
					new RegistrationNumber()
					{
						Type = new RegistrationNumberType() { Code = "BRN" },
						CountryOfIssue = new Country() { Code = "BD" },
						Value = "23456"
					}
			});

			IRegistrationNumberProvider provider = new OrganizationAddressRegistrationNumberProvider(address);

			AssertEquals(string.Empty, provider.GetTaxNumber("BD", "AIN"));
		}

		public void TestGetTaxNumbers_OnlyByTypeCode()
		{
			var address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Country = new Country() { Code = "BD" },
				AddressType = "NotifyParty"
			};

			var provider = new OrganizationAddressRegistrationNumberProvider(address);

			AssertEquals(0, provider.GetTaxNumbers("").Count);
			AssertEquals(0, provider.GetTaxNumbers("AIN").Count);

			address.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
				{
					null,
					new RegistrationNumber() { },
					new RegistrationNumber()
					{
						Type = new RegistrationNumberType() { },
						CountryOfIssue = new Country() { }
					},
					new RegistrationNumber()
					{
						Type = new RegistrationNumberType() { Code = "AIN" }
					},
					new RegistrationNumber()
					{
						Type = new RegistrationNumberType() { Code = "AIN" },
						Value = "87654"
					},
					new RegistrationNumber()
					{
						Type = new RegistrationNumberType() { Code = "AIN" },
						CountryOfIssue = new Country() { Code = "BD" },
						Value = "23456"
					},
					new RegistrationNumber()
					{
						Type = new RegistrationNumberType() { Code = "AIN" },
						CountryOfIssue = new Country() { Code = "DE" },
						Value = "12354"
					}
			});

			var pairsOfTaxNumbersAndCountries = provider.GetTaxNumbers("AIN");
			var pairsOfTaxNumberAndCountry = pairsOfTaxNumbersAndCountries.FirstOrDefault(c => c.taxNumber == "23456");
			AssertNotNull(pairsOfTaxNumberAndCountry);
			AssertEquals(pairsOfTaxNumberAndCountry.countryOfIssue, "BD");

			pairsOfTaxNumberAndCountry = pairsOfTaxNumbersAndCountries.FirstOrDefault(c => c.taxNumber == "12354");
			AssertNotNull(pairsOfTaxNumberAndCountry);
			AssertEquals(pairsOfTaxNumberAndCountry.countryOfIssue, "DE");

			pairsOfTaxNumberAndCountry = pairsOfTaxNumbersAndCountries.FirstOrDefault(c => c.taxNumber == "87654");
			AssertNotNull(pairsOfTaxNumberAndCountry);
			AssertEquals(pairsOfTaxNumberAndCountry.countryOfIssue, ZString.Empty);
		}
	}
}
