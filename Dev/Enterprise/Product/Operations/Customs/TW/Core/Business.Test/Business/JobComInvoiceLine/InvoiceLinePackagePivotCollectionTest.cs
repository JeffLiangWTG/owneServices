using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(InvoiceLinePackagePivotCollection))]
	sealed class InvoiceLinePackagePivotCollectionTest : Customs.Business.Testing.SupporterPackagePivotCollectionTest
	{
		public void TestCalculateDefaultQuantity()
		{
			var supporter = GetSupporter();
			var collection = supporter.CusPackPivots;
			var bill = supporter.Declaration.PrimaryMasterBill;
			var invoiceLine = (JobComInvoiceLine)supporter;
			invoiceLine.JI_InvoiceQuantity = 100;
			var packingInformation = bill.Declaration.PackingInformationCollection.AddNew();
			packingInformation.HouseBillContainer = new HouseBillContainer(bill, null);
			var package1 = bill.PackingGroups[0].Packages.AddNew();
			var pivot1 = (ICusQuantityPivot)collection.AddPivotFor(package1);
			AssertEquals(pivot1.Quantity, 100m);
			pivot1.Quantity = 30;
			var package2 = bill.PackingGroups[0].Packages.AddNew();
			var pivot2 = (ICusQuantityPivot)collection.AddPivotFor(package2);
			AssertEquals(pivot2.Quantity, 70m);
			pivot2.Quantity = 80;
			var package3 = bill.PackingGroups[0].Packages.AddNew();
			var pivot3 = (ICusQuantityPivot)collection.AddPivotFor(package3);
			AssertEquals("the default value is 0.(Invoice Quantity is 100,Quantity is 30 on package1,Quantity is 80 on package2)", pivot3.Quantity, 0m);
		}

		protected override ICusLinkPackageSupporter GetSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			declaration.JE_MasterBill = "X";
			return invoiceLine;
		}

		protected override ICusPackagePivot GetNewPivotOnSameDeclaration(BaseJobDeclaration declaration, BasePackage package)
		{
			var invoiceHeader = declaration.Invoices[0];
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			return invoiceLine.PackagesPivot.AddPivotFor(package);
		}
	}
}
