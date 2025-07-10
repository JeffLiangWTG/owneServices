using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(NonPersistentSplitPackItemCollection))]
	sealed class NonPersistentSplitPackItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NonPersistentSplitPackItemCollection>
	{
		public void TestSetPackableUQDefaultValueFromPackableItem()
		{
			PackageCusPackableItemRelation.PackableUQ = "PCE";
			PackageCusPackableItemRelation.NetWeightUQ = "KG";
			var testBoCollection = GetCollectionToTest();
			var testBo = testBoCollection.AddNew();
			AssertEquals("PCE", testBo.PackableUQ);
			AssertEquals("KG", testBo.NetWeightUQ);
		}

		public void TestSetPackableUQFromPreviousSplitPackableItem()
		{
			PackageCusPackableItemRelation.PackableUQ = "PCE";
			PackageCusPackableItemRelation.NetWeightUQ = "KG";
			var testBoCollection = GetCollectionToTest();
			var splitPackableItem1 = testBoCollection.AddNew();
			AssertEquals("PCE", splitPackableItem1.PackableUQ);
			AssertEquals("KG", splitPackableItem1.NetWeightUQ);

			splitPackableItem1.PackableUQ = "CTN";
			splitPackableItem1.NetWeightUQ = "G";
			var splitPackableItem2 = testBoCollection.AddNew();
			AssertEquals("CTN", splitPackableItem2.PackableUQ);
			AssertEquals("G", splitPackableItem1.NetWeightUQ);

			var splitPackableItem3 = testBoCollection.AddNew();
			splitPackableItem3.PackableUQ = "PLN";
			splitPackableItem3.Sequence = 2;

			var splitPackableItem4 = testBoCollection.AddNew();
			AssertEquals("CTN", splitPackableItem4.PackableUQ);
		}

		protected override NonPersistentSplitPackItemCollection GetCollectionToTest() => new NonPersistentSplitPackItemCollection(PackageCusPackableItemRelation, Splitter);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new NonPersistentSplitPackItem(PackageCusPackableItemRelation, Splitter);

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
