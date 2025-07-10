using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(NonPersistentSplitPackItem))]
	sealed class NonPersistentSplitPackItemTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSequenceNumberGenerator()
		{
			var collectionForTesting = Splitter.PackableItemParts;
			var splitPackItem1 = collectionForTesting.AddNew();
			AssertEquals((ZShort)1, splitPackItem1.Sequence);

			var splitPackItem2 = collectionForTesting.AddNew();
			AssertEquals((ZShort)2, splitPackItem2.Sequence);

			var splitPackItem3 = collectionForTesting.AddNew();
			AssertEquals((ZShort)3, splitPackItem3.Sequence);

			splitPackItem2.Delete();
			AssertEquals((ZShort)2, splitPackItem3.Sequence);
		}

		public void TestSequenceRecalculateWhenRenumbered()
		{
			var collectionForTesting = Splitter.PackableItemParts;
			var splitPackItem1 = collectionForTesting.AddNew();
			var splitPackItem2 = collectionForTesting.AddNew();
			var splitPackItem3 = collectionForTesting.AddNew();
			AssertEquals((ZShort)1, splitPackItem1.Sequence);
			AssertEquals((ZShort)2, splitPackItem2.Sequence);
			AssertEquals((ZShort)3, splitPackItem3.Sequence);

			splitPackItem2.Sequence = 3;
			AssertEquals((ZShort)1, splitPackItem1.Sequence);
			AssertEquals((ZShort)3, splitPackItem2.Sequence);
			AssertEquals((ZShort)2, splitPackItem3.Sequence);
		}

		public void TestPackableUQ()
		{
			AssertHasCustomAttribute<ListAttribute>(NonPersistentSplitPackItem.GetType(), NonPersistentSplitPackItem.Schema.PackableUQ, false, attrib => attrib.ListDataSourceMember == "PackTypes");
		}

		public void TestNetWeightUQ()
		{
			AssertHasCustomAttribute<ListAttribute>(NonPersistentSplitPackItem.GetType(), NonPersistentSplitPackItem.Schema.NetWeightUQ, false, attrib => attrib.ListDataSourceMember == "WeightUQs");
		}

		public void TestPackTypes()
		{
			AssertSame(PackageCusPackableItemRelation.Lookups.PackTypes, NonPersistentSplitPackItem.PackTypes);
		}

		public void TestWeightUQs()
		{
			AssertSame(PackageCusPackableItemRelation.Lookups.WeightUQs, NonPersistentSplitPackItem.WeightUQs);
		}

		protected override BusinessObject GetNewBusinessObject() => NonPersistentSplitPackItem;

		NonPersistentSplitPackItem NonPersistentSplitPackItem => nonPersistentSplitPackItem ?? (nonPersistentSplitPackItem = new NonPersistentSplitPackItem(PackageCusPackableItemRelation, Splitter));
		NonPersistentSplitPackItem nonPersistentSplitPackItem;

		CusPackageCusPackableItemRelation PackageCusPackableItemRelation => cusPackageCusPackableItemRelation ?? (cusPackageCusPackableItemRelation = Package.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().First());
		CusPackageCusPackableItemRelation cusPackageCusPackableItemRelation;

		PackableItemsSplitter Splitter => splitter ?? (splitter = new PackableItemsSplitter(Package, PackageCusPackableItemRelation));
		PackableItemsSplitter splitter;

		CusPackage Package
		{
			get
			{
				if (package == null)
				{
					var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
					declaration.Invoices.AddNew().InvoiceLines.AddNew();
					Factory.Save();
					var packingList = declaration.LoadOrCreateCusPackingList(Factory);
					package = packingList.PackageJob.Packages.AddNew();
				}
				return package;
			}
		}
		CusPackage package;
	}
}
