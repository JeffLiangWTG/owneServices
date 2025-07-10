using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(DocPkgPackageCollection))]
	sealed class DocPkgPackageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocPkgPackageCollection>
	{
		protected override DocPkgPackageCollection GetCollectionToTest()
		{
			return new DocPkgPackageCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var package = Factory.NewWithValidTestData<CusPackage>();
			return DocPkgPackage.New(package, Factory);
		}

		public void TestMaxPackedNetWeightDecimalPlace()
		{
			var relataion = GetNewCusPackableItemRelation();
			relataion.NetWeight = 2;
			relataion.NetWeightUQ = "KG";
			var docCusPackingList = new DocCusPackingList(cusPackingList);
			Assert(docCusPackingList.Packages.Cast<DocPkgPackage>().All(c => c.MaxPackedNetWeightDecimalPlace == 0));
			Assert(docCusPackingList.Packages.Cast<DocPkgPackage>().All(c => c.PackedItemRelationsRelation.Cast<DocCusPackageCusPackableItemRelation>().All(x => x.NetWeightDecimalPlace == 0)));

			relataion.NetWeight = 1;
			docCusPackingList = new DocCusPackingList(cusPackingList);
			Assert(docCusPackingList.Packages.Cast<DocPkgPackage>().All(c => c.MaxPackedNetWeightDecimalPlace == 1));
			Assert(docCusPackingList.Packages.Cast<DocPkgPackage>().All(c => c.PackedItemRelationsRelation.Cast<DocCusPackageCusPackableItemRelation>().All(x => x.NetWeightDecimalPlace == 1)));
		}

		Customs.Business.CusPackageCusPackableItemRelation GetNewCusPackableItemRelation()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1";
			var invoieceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoieceLine1.JI_Description = "line1";
			invoieceLine1.JI_InvoiceQuantity = 2m;
			invoieceLine1.JI_InvoiceUQ = "BAG";

			cusPackingList = (CusPackingList)dec.LoadOrCreateCusPackingList(Factory);
			var packageJob = cusPackingList.PackageJob;
			var package = packageJob.Packages.AddNew();
			package.KP_PackageQty = 2;
			package.PackableItemRelataions.RebuildElements();
			var cusPackableItem = cusPackingList.PackableItems.First();
			package.CustomsPackItem(cusPackableItem, cusPackableItem.CUI_PackableQty);

			return package.PackableItemRelataions.Cast<Customs.Business.CusPackageCusPackableItemRelation>().First();
		}

		CusPackingList cusPackingList;
	}
}
