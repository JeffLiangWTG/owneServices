using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusPackageCollection))]
	public class CusPackageCollectionBaseTest : ActiveBusinessObjectCollectionTestCase<CusPackageCollection>
	{
		protected override CusPackageCollection GetCollectionToTest()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			Factory.Save();
			return cusPackingList.PackageJob.Packages;
		}
	}

	[TestedType(typeof(PkgPackageCollection))]
	public class CusPackageCollectionTest : PkgPackageCollectionTest
	{
		public void TestNewElementType()
		{
			var packageJob = Factory.NewWithValidTestData<CusPackageJob>();
			var newElement = packageJob.Packages.AddNew();
			AssertType(typeof(CusPackage), newElement);
			AssertType(typeof(CusPackage), packageJob.Packages[0]);
		}

		public void TestQuickPackSeqList()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();

			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			var packageJob = cusPackingList.PackageJob;
			var packages = packageJob.Packages;
			var list = packages.QuickPackSeqList;
			AssertContainsExactElementsInAnyOrder(new ZString[] { "0" }, list.GetAllCodes());
			Assert("Code 0's description should not be type of NoResString", !(list.GetMultilingualDescriptionFromCode("0") is NoResString));

			var package = packages.AddNew();
			package.KP_MarksAndNumbers = "Pack #1";
			AssertContainsExactElementsInAnyOrder(new ZString[] { "0", "1" }, packages.QuickPackSeqList.GetAllCodes());

			package = packages.AddNew();
			package.KP_MarksAndNumbers = "Pack #2";
			list = packages.QuickPackSeqList;
			CombineAssertions(() =>
			{
				AssertEquals("0 - New Pack #", list.GetDescriptionFromCode("0"));
				AssertEquals("1 - Pack #1", list.GetDescriptionFromCode("1"));
				AssertEquals("2 - Pack #2", list.GetDescriptionFromCode("2"));
			});
		}

		public void TestQuickPackSeqDictionary()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();

			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			var packageJob = cusPackingList.PackageJob;
			var packages = packageJob.Packages;
			AssertContainsExactElementsInAnyOrder(Array.Empty<ZString>(), packages.QuickPackSeqDictionary.Select(kv => kv.Key + " - " + kv.Value));

			var package = packages.AddNew();
			package.KP_MarksAndNumbers = "Pack #1";
			AssertContainsExactElementsInAnyOrder(new ZString[] { "1 - Pack #1" }, packages.QuickPackSeqDictionary.Select(kv => kv.Key + " - " + kv.Value));

			package = packages.AddNew();
			package.KP_MarksAndNumbers = "Pack #2";
			AssertContainsExactElementsInAnyOrder(new ZString[] { "1 - Pack #1", "2 - Pack #2" }, packages.QuickPackSeqDictionary.Select(kv => kv.Key + " - " + kv.Value));

			package.KP_MarksAndNumbers = "Pack #3";
			AssertContainsExactElementsInAnyOrder(new ZString[] { "1 - Pack #1", "2 - Pack #3" }, packages.QuickPackSeqDictionary.Select(kv => kv.Key + " - " + kv.Value));
		}

		public void TestEventHandlerOnRemovedItem()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();

			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			var packageJob = cusPackingList.PackageJob;
			var package = packageJob.Packages.AddNew();
			package.KP_MarksAndNumbers = "Pack #1";
			var package2 = packageJob.Packages.AddNew();
			package2.KP_MarksAndNumbers = "Pack #2";
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new ZString[] { "1 - Pack #1", "2 - Pack #2" }, packageJob.Packages.QuickPackSeqDictionary.Select(kv => kv.Key + " - " + kv.Value));
			packageJob.Packages.RemoveFromRelationship(package2);
			packageJob.Packages.QuickPackSeqDictionary["1"] = "modify";
			AssertContainsExactElementsInAnyOrder(new ZString[] { "1 - modify" }, packageJob.Packages.QuickPackSeqDictionary.Select(kv => kv.Key + " - " + kv.Value));

			package2.KP_Sequence = 3;
			package2.KP_MarksAndNumbers = "Pack #3";
			AssertContainsExactElementsInAnyOrder("should not trigger refresh when removed items change", new ZString[] { "1 - modify" }, packageJob.Packages.QuickPackSeqDictionary.Select(kv => kv.Key + " - " + kv.Value));
		}
	}
}
