using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseSupplementaryCodeProvider))]
	sealed class BaseSupplementaryCodeProviderBaseOnlyTest : BaseSupplementaryCodeProviderTest<BaseSupplementaryCodeProvider, BaseSupplementaryCode>
	{
		public void TestGetByCountryCode()
		{
			AssertType<BaseSupplementaryCodeProvider>("When country code is empty", BaseSupplementaryCodeProvider.GetByCountryCode(ZString.Empty));

			var supplementaryCodeProvider = BaseSupplementaryCodeProvider.GetByCountryCode(CountryCodeForSupplementaryCodeProvider);
			AssertType<BaseSupplementaryCodeProvider>("Provider type", supplementaryCodeProvider);
			AssertEquals("Country Code", CountryCodeForSupplementaryCodeProvider, supplementaryCodeProvider.CountryCode);
		}

		public void TestGetBySupplementaryCodeSupporter()
		{
			AssertNull("Should be null when supplementary code supporter is null", BaseSupplementaryCodeProvider.GetBySupplementaryCodeSupporter(null));

			var supplementaryCodeProvider = BaseSupplementaryCodeProvider.GetBySupplementaryCodeSupporter(SupplementaryCodeSupporter);
			AssertNotNull("Should be not null when country parameter is not null", supplementaryCodeProvider);
			CombineAssertions("Checking SupplementaryCodeProvider", () =>
			{
				AssertType<BaseSupplementaryCodeProvider>("SupplementaryCodeProvider type", supplementaryCodeProvider);
				AssertEquals("Country Code", CountryCodeForSupplementaryCodeProvider, supplementaryCodeProvider.CountryCode);
			});
		}

		protected override ZShort ExpectedAdditionalSupplementaryCodesStartingOrder => 3;
		protected override ZShort ExpectedNumberOfAdditionalSupplementaryCodes => 8;
		protected override Type ExpectedGetNewValidationReturnType => typeof(BaseSupplementaryCodeValidation);
		protected override Type ExpectedGetNewLookupsReturnType => typeof(BaseSupplementaryCodeLookups);

		protected override BaseSupplementaryCodeProvider CreateSupplementaryCodeProvider() => new (CountryCodeForSupplementaryCodeProvider);

		ISupplementaryCodeSupporter SupplementaryCodeSupporter => supplementaryCodeSupporter ??= new SupplementaryCodeSupporterForTest(Factory, CountryCodeForSupplementaryCodeProvider);
		ISupplementaryCodeSupporter supplementaryCodeSupporter;
	}

	sealed class SupplementaryCodeSupporterForTest : ISupplementaryCodeSupporter
	{
		public SupplementaryCodeSupporterForTest(BusinessObjectFactory factory, ZString countryCode)
		{
			Factory = factory;
			this.countryCode = countryCode;
		}
		BusinessObjectFactory Factory { get; }

		readonly ZString countryCode;

		public IEnumerable<BaseSupplementaryCode> SupplementaryCodes => Enumerable.Empty<BaseSupplementaryCode>();

		public TariffView Tariff => Factory.New<TariffView>();

		public IZZRateSelectionCriteria RateSelectionCriteria => null;

		CodeDescriptionPairList ICusCodeDataWithOrderSupporter.CachedListOfAdditionalCodeDescriptions => null;

		ZString ISupplementaryCodeSupporter.SupplementaryCodesFieldType => nameof(FieldType.Text);

		ResourceStringData ISupplementaryCodeSupporter.SupplementaryCodeCaption => null;

		ICusCodeDataCollection<BaseSupplementaryCode> ISupplementaryCodeSupporter.AdditionalSupplementaryCodes => null;

		public ZString GetCountryCodeForCodeProvider() => countryCode;
		void ICusCodeDataWithOrderSupporter.OnCodesChanged()
		{
		}

		public ZString GetCountryCodeFromAdditionalCode(ZString additionalCode) => "XX";
	}
}
