using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CompanySpecificDefaultWithCacheRegistryItemImplTest : TestCaseWithFactory
	{
		public void TestDefaultValue_IsSet_WhenCompanyExistsAtStart()
		{
			var twCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			twCompany1.GC_RN_NKCountryCode = Constants.CountryCodes.Taiwan;
			var twCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			twCompany2.GC_RN_NKCountryCode = Constants.CountryCodes.Taiwan;
			var uzCompany = Factory.NewWithValidTestData<GlbCompany>();
			uzCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Uzbekistan;
			Factory.Save();

			var lookupTable = GetCountryLookupTable();
			var regItemImpl = new CountrySpecificDefaultRegistryItemImpl_ForTest("default", lookupTable);

			AssertEquals("TaiwanDefault", regItemImpl.GetDefaultValue(twCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("TaiwanDefault", regItemImpl.GetDefaultValue(twCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("UzbekistanDefault", regItemImpl.GetDefaultValue(uzCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestDefaultValue_IsFallback_WhenNoMatchingCompanyExists()
		{
			var lookupTable = GetCountryLookupTable();
			var regItemImpl = new CountrySpecificDefaultRegistryItemImpl_ForTest("default", lookupTable);

			AssertEquals("DefaultValue uses fallback when no company exists", "default", regItemImpl.GetDefaultValue(Guid.NewGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestDefaultValue_IsSet_WhenNewCompanyAdded()
		{
			var twCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			twCompany1.GC_RN_NKCountryCode = Constants.CountryCodes.Taiwan;
			var uzCompany = Factory.NewWithValidTestData<GlbCompany>();
			uzCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Uzbekistan;
			Factory.Save();

			var twCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			twCompany2.GC_RN_NKCountryCode = Constants.CountryCodes.Taiwan;

			var lookupTable = GetCountryLookupTable();
			var regItemImpl = new CountrySpecificDefaultRegistryItemImpl_ForTest("default", lookupTable);

			AssertEquals("TaiwanDefault", regItemImpl.GetDefaultValue(twCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("default", regItemImpl.GetDefaultValue(twCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("UzbekistanDefault", regItemImpl.GetDefaultValue(uzCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			Factory.Save();
			AssertEquals("TaiwanDefault", regItemImpl.GetDefaultValue(twCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("TaiwanDefault", regItemImpl.GetDefaultValue(twCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("UzbekistanDefault", regItemImpl.GetDefaultValue(uzCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestDefaultValue_IsCached_AfterFirstLoad()
		{
			var twCompany = Factory.NewWithValidTestData<GlbCompany>();
			twCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Taiwan;
			var uzCompany = Factory.NewWithValidTestData<GlbCompany>();
			uzCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Uzbekistan;
			Factory.Save();

			var lookupTable = GetCountryLookupTable();
			var regItemImpl = new CountrySpecificDefaultRegistryItemImpl_ForTest("default", lookupTable);

			AssertEquals("Precondition: no fetches", 0, regItemImpl.DatabaseFetchCount);

			AssertEquals("TaiwanDefault", regItemImpl.GetDefaultValue(twCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("One fetch on first load", 1, regItemImpl.DatabaseFetchCount);

			AssertEquals("TaiwanDefault", regItemImpl.GetDefaultValue(twCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("Defaults are cached by company after first load", 1, regItemImpl.DatabaseFetchCount);

			AssertEquals("UzbekistanDefault", regItemImpl.GetDefaultValue(uzCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("Cache is bulk loaded on first load, no further database hits for pre-existing companies", 1, regItemImpl.DatabaseFetchCount);

			AssertEquals("default", regItemImpl.GetDefaultValue(Guid.NewGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("Unknown companies trigger a database hit", 2, regItemImpl.DatabaseFetchCount);
			AssertEquals("default", regItemImpl.GetDefaultValue(Guid.NewGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("Unknown companies trigger a database hit every time (so don't do that)", 3, regItemImpl.DatabaseFetchCount);
		}

		#region Implementation

		class CountrySpecificDefaultRegistryItemImpl_ForTest : CompanySpecificDefaultWithCacheRegistryItemImpl<string>
		{
			public CountrySpecificDefaultRegistryItemImpl_ForTest(string fallbackDefault, IReadOnlyDictionary<string, string> lookupTableForDefaults)
				: base("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", new StringRegistryDataType(), RegistryStorageFlags.Company, RegistryOptions.Default, fallbackDefault)
			{
				this.lookupTableForDefaults = lookupTableForDefaults;
			}

			readonly IReadOnlyDictionary<string, string> lookupTableForDefaults;

			protected override string GetDefaultValueByCompany(GlbCompany company)
			{
				return lookupTableForDefaults.TryGetValue(company.GC_RN_NKCountryCode, out var result) ? result : base.fallbackDefault;
			}

			public int DatabaseFetchCount;
			protected override IReadOnlyCollection<GlbCompany> GetListOfCompanies()
			{
				DatabaseFetchCount++;
				return base.GetListOfCompanies();
			}
		}

		static Dictionary<string, string> GetCountryLookupTable()
			=> new Dictionary<string, string>()
			{
				{ Constants.CountryCodes.Albania,    "AlbaniaDefault" },
				{ Constants.CountryCodes.Taiwan,     "TaiwanDefault" },
				{ Constants.CountryCodes.Uzbekistan, "UzbekistanDefault" }
			};

		#endregion
	}
}
