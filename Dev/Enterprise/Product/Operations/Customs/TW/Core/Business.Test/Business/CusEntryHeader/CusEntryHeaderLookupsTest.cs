using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.TW.Business.IncoTermsCodeDescriptionPairList;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestEntryHeader()
		{
			var parent = Factory.New<CusEntryHeader>();
			NUnit.Framework.Assert.That(parent, NUnit.Framework.Is.EqualTo(parent.Lookups.EntryHeader));
		}

		[ExpectNoExceptions]
		public void TestCurrencyList()
		{
			var parent = Factory.New<CusEntryHeader>();
			var fCurrencyList = (IActiveBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<MasterFiles.Integration.IRefCurrencyCollection>(), new object[] { Factory });
			NUnit.Framework.Assert.That(parent.Lookups.CurrencyList.Count, NUnit.Framework.Is.EqualTo(fCurrencyList.Count), "Count");
		}

		[ExpectNoExceptions]
		public void TestIncoTermList()
		{
			var parent = Factory.New<CusEntryHeader>();
			var list = parent.Lookups.IncoTermList;
			var sourceList = Factory.GetCachedValue("CusEntryHeaderLookups|IncoTermList", () =>
			{
				var listCache = new CodeDescriptionPairList();
				listCache.AddPair(IncoTerms.CostInsuranceAndFreight, Descriptions.CostInsuranceAndFreight);
				listCache.AddPair(IncoTerms.CostAndFreight, Descriptions.CostAndFreight);
				listCache.AddPair(IncoTerms.FreeOnBoard, Descriptions.FreeOnBoard);
				listCache.AddPair(IncoTerms.CostAndInsurance, Descriptions.CostAndInsurance);
				listCache.AddPair(IncoTerms.FreeAlongsideShip, Descriptions.FreeAlongsideShip);
				listCache.AddPair(IncoTerms.ExWorks, Descriptions.ExWorks);
				return listCache;
			});
			NUnit.Framework.Assert.That(list, NUnit.Framework.Is.EqualTo(sourceList));
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(6));
		}
	}
}
