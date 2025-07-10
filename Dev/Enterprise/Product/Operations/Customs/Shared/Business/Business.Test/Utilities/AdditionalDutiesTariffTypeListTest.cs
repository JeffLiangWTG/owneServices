using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class AdditionalDutiesTariffTypeListTest : TestCaseWithFactory
	{
		public void TestAdditionalFilter_IsCloned()
		{
			var (tariffTypeEXC, _, _) = SetupTestData();
			var query = new ZQuery();
			var list = new AdditionalDutiesTariffTypeList(Factory, Core.Constants.CountryCodes.Germany, query);
			AssertEquals(tariffTypeEXC, list["EXC"]);

			query.IsNoResultQuery = true;
			Factory.ClearCachedValue<List<RefCusTariffType>>($"AdditionalDutiesTariffTypeList|List|DE|{query.LiteralTextSqlFormatted}");
			AssertEquals(tariffTypeEXC, list["EXC"]);

			query = new ZQuery();
			list = new AdditionalDutiesTariffTypeList(Factory, Core.Constants.CountryCodes.Germany, query);
			query.IsNoResultQuery = true;
			Factory.ClearCachedValue<List<RefCusTariffType>>($"AdditionalDutiesTariffTypeList|List|DE|{query.LiteralTextSqlFormatted}");
			AssertEquals(tariffTypeEXC, list["EXC"]);
		}

		public void TestListIsCached()
		{
			var list1 = new AdditionalDutiesTariffTypeListExposed(Factory);
			var list2 = new AdditionalDutiesTariffTypeListExposed(Factory);
			AssertSame("Internal List should be cached in the Factory", list1.ListExposed, list2.ListExposed);
		}

		public void TestIndexer_String()
		{
			var (tariffTypeEXC, tariffTypeTES, _) = SetupTestData();
			var list = new AdditionalDutiesTariffTypeList(Factory, Core.Constants.CountryCodes.Germany);

			AssertEquals(tariffTypeEXC, list["EXC"]);
			AssertEquals(tariffTypeTES, list["TES"]);
			AssertEquals(null, list["ITA"]);
		}

		public void TestIndexer_Int()
		{
			var (tariffTypeEXC, tariffTypeTES, _) = SetupTestData();
			var list = new AdditionalDutiesTariffTypeListExposed(Factory);

			var tariffTypeEXCIndex = list.ListExposed.IndexOf(tariffTypeEXC);
			var tariffTypeTESIndex = list.ListExposed.IndexOf(tariffTypeTES);

			var iList = (IList)list;

			AssertEquals(tariffTypeEXC, iList[tariffTypeEXCIndex]);
			AssertEquals(tariffTypeTES, iList[tariffTypeTESIndex]);
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => _ = iList[2]);
		}

		public void TestContainsCode()
		{
			SetupTestData();
			var list = new AdditionalDutiesTariffTypeList(Factory, Core.Constants.CountryCodes.Germany);

			AssertEquals(true, list.ContainsCode("EXC"));
			AssertEquals(true, list.ContainsCode("TES"));
			AssertEquals(false, list.ContainsCode("ITA"));
		}

		public void TestGetDescriptionFromCode()
		{
			SetupTestData();
			var list = new AdditionalDutiesTariffTypeList(Factory, Core.Constants.CountryCodes.Germany);

			AssertEquals("EXC DESC", list.GetDescriptionFromCode("EXC"));
			AssertEquals("TES DESC", list.GetDescriptionFromCode("TES"));
			AssertEquals(null, list.GetDescriptionFromCode("ITA"));
		}

		public void TestAdd()
		{
			var list = (IList)new AdditionalDutiesTariffTypeList(Factory, Core.Constants.CountryCodes.Germany);
			AssertExceptionThrown<NotSupportedException>(() => list.Add("123"));
		}

		public void TestContains()
		{
			var (tariffTypeEXC, tariffTypeTES, tariffTypeITA) = SetupTestData();
			var list = (IList)new AdditionalDutiesTariffTypeList(Factory, Core.Constants.CountryCodes.Germany);

			AssertEquals(true, list.Contains(tariffTypeEXC));
			AssertEquals(true, list.Contains(tariffTypeTES));
			AssertEquals(false, list.Contains(tariffTypeITA));
		}

		public void TestClear()
		{
			var list = (IList)new AdditionalDutiesTariffTypeList(Factory, Core.Constants.CountryCodes.Germany);
			AssertExceptionThrown<NotSupportedException>(() => list.Clear());
		}

		public void TestIndexOf()
		{
			var (tariffTypeEXC, tariffTypeTES, tariffTypeITA) = SetupTestData();
			var list = new AdditionalDutiesTariffTypeListExposed(Factory);

			var tariffTypeEXCIndex = list.ListExposed.IndexOf(tariffTypeEXC);
			var tariffTypeTESIndex = list.ListExposed.IndexOf(tariffTypeTES);

			var iList = (IList)list;

			AssertEquals(tariffTypeEXCIndex, iList.IndexOf(tariffTypeEXC));
			AssertEquals(tariffTypeTESIndex, iList.IndexOf(tariffTypeTES));
			AssertEquals(-1, iList.IndexOf(tariffTypeITA));
		}

		public void TestInsert()
		{
			var (tariffTypeEXC, _, _) = SetupTestData();
			var list = (IList)new AdditionalDutiesTariffTypeList(Factory, Core.Constants.CountryCodes.Germany);
			AssertExceptionThrown<NotSupportedException>(() => list.Insert(0, tariffTypeEXC));
		}

		public void TestRemoveAt()
		{
			SetupTestData();
			var list = (IList)new AdditionalDutiesTariffTypeList(Factory, Core.Constants.CountryCodes.Germany);
			AssertExceptionThrown<NotSupportedException>(() => list.RemoveAt(0));
		}

		public void TestRemove()
		{
			var (tariffTypeEXC, _, _) = SetupTestData();
			var list = (IList)new AdditionalDutiesTariffTypeList(Factory, Core.Constants.CountryCodes.Germany);
			AssertExceptionThrown<NotSupportedException>(() => list.Remove(tariffTypeEXC));
		}

		public void TestCopyTo()
		{
			var list = (IList)new AdditionalDutiesTariffTypeList(Factory, Core.Constants.CountryCodes.Germany);
			var array = Array.Empty<RefCusTariffType>();
			AssertExceptionThrown<NotSupportedException>(() => list.CopyTo(array, 0));
		}

		public void TestGetEnumerator()
		{
			SetupTestData();
			var list = new AdditionalDutiesTariffTypeListExposed(Factory);
			var enumerator = ((IList)list).GetEnumerator();
			AssertEquals(true, enumerator.MoveNext());
			AssertEquals(list.ListExposed.First(), enumerator.Current);
			AssertEquals(true, enumerator.MoveNext());
			AssertEquals(list.ListExposed.Skip(1).First(), enumerator.Current);
		}

		public void TestIsReadOnly()
		{
			var list = (IList)new AdditionalDutiesTariffTypeList(Factory, Core.Constants.CountryCodes.Germany);
			AssertEquals(true, list.IsReadOnly);
		}

		public void TestIsFixedSize()
		{
			var list = (IList)new AdditionalDutiesTariffTypeList(Factory, Core.Constants.CountryCodes.Germany);
			AssertEquals(false, list.IsFixedSize);
		}

		public void TestCount()
		{
			SetupTestData();
			var list = (IList)new AdditionalDutiesTariffTypeList(Factory, Core.Constants.CountryCodes.Germany);
			AssertEquals(2, list.Count);
		}

		public void TestSyncRoot()
		{
			var list = (ICollection)new AdditionalDutiesTariffTypeList(Factory, Core.Constants.CountryCodes.Germany);
			AssertEquals(null, list.SyncRoot);
		}

		public void TestIsSynchronized()
		{
			var list = (ICollection)new AdditionalDutiesTariffTypeList(Factory, Core.Constants.CountryCodes.Germany);
			AssertEquals(false, list.IsSynchronized);
		}

		(RefCusTariffType tariffTypeEXC, RefCusTariffType tariffTypeTES, RefCusTariffType tariffTypeITA) SetupTestData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var de = Core.Constants.CountryCodes.Germany;

			var tariffTypeEXC = helper.CreateTariffType(de, "EXC");
			var tariffTypeTES = helper.CreateTariffType(de, "TES");
			var tariffTypeITA = helper.CreateTariffType(Core.Constants.CountryCodes.Italy, "ITA");
			Factory.Save();

			return (tariffTypeEXC, tariffTypeTES, tariffTypeITA);
		}

		class AdditionalDutiesTariffTypeListExposed : AdditionalDutiesTariffTypeList
		{
			public AdditionalDutiesTariffTypeListExposed(BusinessObjectFactory factory)
				: base(factory, Core.Constants.CountryCodes.Germany)
			{
			}

			public List<RefCusTariffType> ListExposed => base.List;
		}
	}
}
