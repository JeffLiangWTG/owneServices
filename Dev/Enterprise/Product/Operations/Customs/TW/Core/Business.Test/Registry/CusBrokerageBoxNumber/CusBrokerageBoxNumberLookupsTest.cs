using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusBrokerageBoxNumberLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestCustomsOfficeAreaList()
		{
			var lookupsList = parent.Lookups.CustomsOfficeAreaList;
			NUnit.Framework.Assert.That(lookupsList, NUnit.Framework.Is.EquivalentTo(Factory.GetCachedValue<TaiwanCustomsDistrictList>()).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(lookupsList.Count, NUnit.Framework.Is.EqualTo(4));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var collection = new CusBrokerageBoxNumberCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			parent = collection.AddNew();
		}

		CusBrokerageBoxNumber parent;
	}
}
