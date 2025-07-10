using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusLinkPackage))]
	sealed class BaseCusLinkPackageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsLinked_ReadOnly()
		{
			var npbo = (BaseCusLinkPackage)GetNewBusinessObject();
			AssertEquals("IsLinked should never be read only", false, npbo.IsLinkedInfo.ReadOnly);
		}

		public void TestNumberOfPacks()
		{
			var npbo = (BaseCusLinkPackage)GetNewBusinessObject();

			npbo.PackQty = 1;
			AssertNoMessageErrorContaining(npbo.PackQtyInfo, "enter");

			npbo.PackQty = 0;
			AssertHasMessageErrorContaining(npbo.PackQtyInfo, "enter");
		}

		public void TestValidateQuantity()
		{
			var npbo = (BaseCusLinkPackage)GetNewBusinessObject();
			npbo.ValidateQuantity();
			Assert(true);
		}

		public void TestQuantity()
		{
			var npbo = (BaseCusLinkPackage)GetNewBusinessObject();
			AssertEquals(0m, npbo.QuantityPivot.Quantity);
			npbo.Quantity = 1;
			AssertEquals(1m, npbo.QuantityPivot.Quantity);
		}

		public void TestIsQuantity_ReadOnly()
		{
			var npbo = (BaseCusLinkPackage)GetNewBusinessObject();
			AssertEquals("The default value of Islinked is true", true, npbo.IsLinked);
			AssertEquals("Quantity is not readOnly when Islinked is true.", false, npbo.IsQuantity_ReadOnly);
			npbo.IsLinked = false;
			AssertEquals("Quantity is readOnly when Islinked is false.", true, npbo.IsQuantity_ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			declaration.JE_MasterBill = "X";

			var bill = declaration.PrimaryMasterBill;
			var package = bill.PackingGroups[0].Packages.AddNew();
			invoiceLine.PackagesPivot.AddPivotFor(package);

			return new BaseCusLinkPackage(invoiceLine) { IsLinked = true, Package = package };
		}

		public void TestAutoAllocatePackageToInvoiceLine()
		{
			var declaration = Factory.New<PivotBetweenCWandJITest.BaseJobDeclarationWhichSupportsPackagesPivot>();
			declaration.JE_MasterBill = "M";
			declaration.JE_TotalNoOfPieces = 1;

			var package = declaration.Bills[0].PackingGroups[0].Packages[0];
			package.CW_PackQty = 1;

			var invoice = declaration.Invoices.AddNew();

			using (CustomsDataRegistry.Instance.AutoAllocatePackageToInvoiceLines.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_JZ = invoice.PK;
				invoice.InvoiceLines.Add(invoiceLine);
				AssertEquals("PackagesForInvoiceLinesForBindingOnly[0].IsLinked", false, invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);
			}

			using (CustomsDataRegistry.Instance.AutoAllocatePackageToInvoiceLines.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_JZ = invoice.PK;
				invoice.InvoiceLines.Add(invoiceLine);
				AssertEquals("PackagesForInvoiceLinesForBindingOnly[0].IsLinked", true, invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);

				var package2 = declaration.Bills[0].PackingGroups[0].Packages.AddNew();
				package2.CW_PackQty = 1;
				invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_JZ = invoice.PK;
				invoice.InvoiceLines.Add(invoiceLine);
				AssertEquals("PackagesForInvoiceLinesForBindingOnly[0].IsLinked - false - multiple packages", false, invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);
			}
		}

		public void TestPackQty_Caption()
		{
			var linkPackage = (BaseCusLinkPackage)GetNewBusinessObject();
			AssertEquals("Pack Quantity", linkPackage.PackQtyInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestPackageNumber_Caption()
		{
			var linkPackage = (BaseCusLinkPackage)GetNewBusinessObject();
			AssertEquals("Package Number", linkPackage.PackageNumberInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}
	}
}
