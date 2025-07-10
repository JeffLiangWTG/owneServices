using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	class QuickPackItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestQuickPackSeqList()
		{
			var decl = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoiceForTest = decl.Invoices.AddNew();
			var line = invoiceForTest.InvoiceLines.AddNew();
			var packingList = decl.LoadOrCreateCusPackingList(Factory);
			var packages = packingList.PackageJob.Packages;

			var packableItem = line.CreateNewCusPackableItem();
			packingList.PackableItems.Add(packableItem);
			var quickPackItem = new QuickPackItem(packableItem);

			var list = quickPackItem.Lookups.QuickPackSeqList;
			AssertContainsExactElementsInAnyOrder(new ZString[] { "0" }, list.GetAllCodes());
			Assert("Code 0's description should not be type of NoResString", !(list.GetMultilingualDescriptionFromCode("0") is NoResString));

			var package = packages.AddNew();
			package.KP_MarksAndNumbers = "Pack #1";
			quickPackItem = new QuickPackItem(packableItem);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "0", "1" }, quickPackItem.Lookups.QuickPackSeqList.GetAllCodes());

			package = packages.AddNew();
			package.KP_MarksAndNumbers = "Pack #2";
			quickPackItem = new QuickPackItem(packableItem);
			list = quickPackItem.Lookups.QuickPackSeqList;
			CombineAssertions(() =>
			{
				AssertEquals("0 - New Pack #", list.GetDescriptionFromCode("0"));
				AssertEquals("1 - Pack #1", list.GetDescriptionFromCode("1"));
				AssertEquals("2 - Pack #2", list.GetDescriptionFromCode("2"));
			});
		}
	}
}
