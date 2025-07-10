using System;
using System.Collections.Generic;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	/// <summary>
	/// Fake implementation of ICompany for test only
	/// </summary>
	public class FakeCompany : ICompany
	{
		public FakeCompany(string code = "", string countryCode = "")
		{
			Code = code;
			Country = new FakeCountry(countryCode);
		}

		public Guid PK { get; set; } = Guid.NewGuid();

		public string Code { get; set; }

		public string Name { get; set; }

		public Guid OrganisationPK { get; set; }

		public string Address1 { get; set; }

		public string Address2 { get; set; }

		public string City { get; set; }

		public string Postcode { get; set; }

		public string State { get; set; }

		public ICountry Country { get; set; } = new FakeCountry();

		public string Fax { get; set; }

		public string BusinessRegNo1 { get; set; }

		public string BusinessRegNo2 { get; set; }

		public CountryDateTimeFormat DateTimeFormat { get; set; }

		public ICurrency LocalCurrency { get; set; } = new Currency(CurrencyCodes.UnitedStates);

		public ExchangeRate ExchangeRate { get; set; } = new ExchangeRate(false, 2, Guid.Empty);

		public IEnumerable<IBranch> Branches { get; set; } = Array.Empty<IBranch>();

		public IEnumerable<IBranch> ActiveBranches { get; set; } = Array.Empty<IBranch>();

		public bool IsDemoCompany { get; set; }

		public bool IsGSTCashBasis { get; set; }

		public bool IsGSTRegistered { get; set; }

		public bool IsReciprocal { get; set; }

		public bool IsWHTCashBasis { get; set; }

		public bool IsWHTRegistered { get; set; }

		public int ExchangeRateDecimalPlaces { get; set; }

		public string HumanReadableNameForRegistry { get; set; }

		public string LicenceKeyIdentifier { get; set; }

		public int GetPeriod(DateTime date)
		{
			throw new NotImplementedException();
		}

		public void UpdateLicenceKeyIdentifier()
		{
			throw new NotImplementedException();
		}
	}
}
