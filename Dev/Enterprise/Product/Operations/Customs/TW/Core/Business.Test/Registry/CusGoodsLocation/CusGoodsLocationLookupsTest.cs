using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusGoodsLocationLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestLocationOfGoodsList()
		{
			var lookupsList = parent.Lookups.GoodsLocationList as ZZRefCusCodeListCombinedCollection;
			lookupsList.Load();
			NUnit.Framework.Assert.That(lookupsList.Count, NUnit.Framework.Is.EqualTo(3));
			NUnit.Framework.Assert.That(lookupsList.Cast<ZZRefCusCodeListCombined>().Any(x => x.GetAttribute(RefCusCodeListAttributeTypes.Codes.CustomsOffice) == "CC"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(lookupsList.Cast<ZZRefCusCodeListCombined>().Any(x => x.GetAttribute(RefCusCodeListAttributeTypes.Codes.CustomsOffice) == "DD"), NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestCustomsOfficeList()
		{
			var lookupsList = parent.Lookups.CustomsOfficeList as ZZRefCusCodeListCombinedCollection;
			lookupsList.Load();
			NUnit.Framework.Assert.That(lookupsList.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(lookupsList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "CC"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(lookupsList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "DD"), NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestMessageTypeList()
		{
			var lookupsList = parent.Lookups.MessageTypeList;
			NUnit.Framework.Assert.That(lookupsList.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(lookupsList.ElementsAsString, NUnit.Framework.Is.EqualTo(@"EXP - Export
IMP - Import"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(Factory).CreateRegistryItemCusGoodsLocation();
			var collection = new CusGoodsLocationCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			parent = collection.AddNew();
		}

		CusGoodsLocation parent;
	}
}
