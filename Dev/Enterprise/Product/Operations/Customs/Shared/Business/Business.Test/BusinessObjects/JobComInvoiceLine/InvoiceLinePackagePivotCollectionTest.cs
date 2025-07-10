using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(InvoiceLinePackagePivotCollection))]
	sealed class InvoiceLinePackagePivotCollectionTest : SupporterPackagePivotCollectionTest
	{
		public void TestCalculateDefaultQuantity()
		{
			var supporter = GetSupporter();
			var collection = supporter.CusPackPivots;
			var bill = supporter.Declaration.PrimaryMasterBill;
			var package = bill.PackingGroups[0].Packages.AddNew();

			var pivot = (ICusQuantityPivot)collection.AddPivotFor(package);
			AssertEquals("Quantity", 0m, pivot.Quantity);
		}

		public void TestMergeKey()
		{
			var declaration = Factory.New<DeclarationSupportsInvoiceLinePackage>();
			declaration.JE_MasterBill = "X";
			declaration.Packages.RemoveAndDeleteAll();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var collection = invoiceLine1.PackagesPivot;
			AssertEquals("MergeKey", ZString.Empty, collection.MergeKey);
			var bill = declaration.PrimaryMasterBill;
			var packingGroup = bill.PackingGroups[0];
			var package1 = packingGroup.Packages.AddNew();
			package1.CW_SystemCreateTimeUtc = new ZDateTime(2022, 12, 1);
			var pivot1 = collection.AddPivotFor(package1);
			AssertEquals("MergeKey", package1.PK.ToStringKey(), collection.MergeKey);
			var package2 = packingGroup.Packages.AddNew();
			package2.CW_SystemCreateTimeUtc = new ZDateTime(2022, 12, 2);
			pivot1.PackagePK = package2.PK;
			AssertEquals("MergeKey", package2.PK.ToStringKey(), collection.MergeKey);
			var pivot2 = collection.AddNew();
			AssertEquals("MergeKey", package2.PK.ToStringKey(), collection.MergeKey);
			pivot2.CHC_CW = package1.PK;
			AssertEquals("MergeKey", package1.PK.ToStringKey() + "|" + package2.PK.ToStringKey(), collection.MergeKey);
			collection.Remove(pivot2);
			AssertEquals("MergeKey", package2.PK.ToStringKey(), collection.MergeKey);
		}

		public void TestCalculateRemainingPackages()
		{
			var declaration = Factory.New<DeclarationSupportsInvoiceLinePackage>();
			declaration.JE_MasterBill = "X";
			declaration.JE_ClusterKey = 1;
			declaration.Packages.RemoveAndDeleteAll();

			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 100;
			package1.CW_PackType = "PKG";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			AssertNotNull(invoiceLine1.PackagesForInvoiceLinesForBindingOnly);
			AssertEquals(1, invoiceLine1.PackagesForInvoiceLinesForBindingOnly.Count);
			AssertEquals(100, invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0].PackQty);

			invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0].PackQty = 20;
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			AssertEquals(1, invoiceLine2.PackagesForInvoiceLinesForBindingOnly.Count);
			AssertEquals(80, invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0].PackQty);

			invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0].PackQty = 28;
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			AssertEquals(1, invoiceLine3.PackagesForInvoiceLinesForBindingOnly.Count);
			AssertEquals(52, invoiceLine3.PackagesForInvoiceLinesForBindingOnly[0].PackQty);
		}

		protected override ICusLinkPackageSupporter GetSupporter()
		{
			var declaration = Factory.New<DeclarationSupportsInvoiceLinePackage>();
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

		class DeclarationSupportsInvoiceLinePackage : BaseJobDeclaration
		{
			public DeclarationSupportsInvoiceLinePackage(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override bool SupportsChcPivotBetweenInvoiceLineAndPackingCore => true;
		}
	}
}
