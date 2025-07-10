using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CountrySpecificDefaultRegistryItemImplTest : TestCaseWithFactory
	{
		public void TestGetDefaultValue()
		{
			var twCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			twCompany1.GC_RN_NKCountryCode = Constants.CountryCodes.Taiwan;
			var uzCompany = Factory.NewWithValidTestData<GlbCompany>();
			uzCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Uzbekistan;
			var isCompany = Factory.NewWithValidTestData<GlbCompany>();
			isCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Iceland;
			Factory.Save();

			var twCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			twCompany2.GC_RN_NKCountryCode = Constants.CountryCodes.Taiwan;

			var lookupTable = GetCountryLookupTable();
			var regItemImpl = CreateObjectForTest((countryCode) => lookupTable.TryGetValue(countryCode, out var result) ? result : "NoCountryDefault");

			AssertEquals("default", regItemImpl.GetDefaultValue(Guid.NewGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("TaiwanDefault", regItemImpl.GetDefaultValue(twCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("default", regItemImpl.GetDefaultValue(twCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("UzbekistanDefault", regItemImpl.GetDefaultValue(uzCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("NoCountryDefault", regItemImpl.GetDefaultValue(isCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			Factory.Save();
			AssertEquals("TaiwanDefault", regItemImpl.GetDefaultValue(twCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		#region Implementation

		static CountrySpecificDefaultRegistryItemImpl<string> CreateObjectForTest(Func<ZString, string> defaultValueGetter)
			=> new CountrySpecificDefaultRegistryItemImpl<string>("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", new StringRegistryDataType(), RegistryStorageFlags.Company, RegistryOptions.Default, defaultValueGetter, "default");

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
