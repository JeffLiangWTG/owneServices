using System;
using System.Globalization;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class FakeCountry : ICountry
	{
		public FakeCountry(string code = CountryCodes.Australia)
		{
			Code = code;
		}

		public string Code { get; set; }

		public CultureInfo Culture { get; set; } = CargoWiseOne.ResourceStrings.Internal.ResInternal.DefaultLocale;

		public string Description { get; set; }

		public ICurrency Currency { get; set; } = new Currency(CurrencyCodes.UnitedStates);

		public bool IsGSTRegistered { get; set; }

		public Guid PK { get; set; }

		public string ConsumptionTaxDescription { get; set; }

		public IDisposable SetCultureForTest(CultureInfo culture)
		{
			throw new NotImplementedException("This is a fake class, just set the Culture property directly in your test!");
		}
	}
}
