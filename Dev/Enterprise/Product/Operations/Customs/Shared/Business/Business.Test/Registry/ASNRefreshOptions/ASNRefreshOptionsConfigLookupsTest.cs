using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	sealed class ASNRefreshOptionsConfigLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestFieldTypeList()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			var config = new ASNRefreshOptionsConfig(fallbackLevel, Factory);
			var list = config.Lookups.FieldTypeList;

			AssertNotNull(list);
			AssertEquals(4, list.Count);
			Assert(list.ContainsCode(Constants.Customs.ASNRefreshDefaultsOptions.Codes.Classification));
			Assert(list.ContainsCode(Constants.Customs.ASNRefreshDefaultsOptions.Codes.CountryOfOrigin));
			Assert(list.ContainsCode(Constants.Customs.ASNRefreshDefaultsOptions.Codes.Preference));
			Assert(list.ContainsCode(Constants.Customs.ASNRefreshDefaultsOptions.Codes.Tariff));
		}
	}
}
