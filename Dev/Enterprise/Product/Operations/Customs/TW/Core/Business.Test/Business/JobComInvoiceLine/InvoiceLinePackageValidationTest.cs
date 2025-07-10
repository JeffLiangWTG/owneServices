using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class InvoiceLinePackageValidationTest : TestCaseWithFactory
	{
		public void TestCheckQuantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 100;
			declaration.JE_MasterBill = "X";
			var bill = declaration.PrimaryMasterBill;
			var packageGroup = bill.PackingGroups.AddNew();
			packageGroup.Packages.AddNew();
			packageGroup.Packages.AddNew();
			var linePackageCollection = (InvoiceLineCusLinkPackageCollection)invoiceLine1.PackagesForInvoiceLinesForBindingOnly;
			var lineLinkPackage1 = linePackageCollection[0];
			lineLinkPackage1.IsLinked = true;
			lineLinkPackage1.Quantity = 30;
			var messageError = ValidationConstants.InvoiceLine.TotalQuantityForPackagesPivotNotEqualToInvoiceQuantity(30, invoiceLine1.JI_InvoiceQuantity);
			AssertHasWarning(lineLinkPackage1.QuantityInfo, messageError);
			var lineLinkPackage2 = linePackageCollection[1];
			lineLinkPackage2.IsLinked = true;
			lineLinkPackage2.Quantity = 70;
			AssertNoWarning(lineLinkPackage2.QuantityInfo, messageError);
			var lessThanOrEqualToZeroMessage = "Please enter a 'Quantity' greater than 0.";
			lineLinkPackage2.Quantity = 1;
			AssertNoMessageError(lineLinkPackage2.QuantityInfo, lessThanOrEqualToZeroMessage);
			lineLinkPackage2.Quantity = 0;
			AssertHasMessageError(lineLinkPackage2.QuantityInfo, lessThanOrEqualToZeroMessage);
			AssertHasWarning(lineLinkPackage2.QuantityInfo, messageError);
			lineLinkPackage2.IsLinked = false;
			AssertNoMessageError(lineLinkPackage2.QuantityInfo, lessThanOrEqualToZeroMessage);
			AssertNoWarning(lineLinkPackage2.QuantityInfo, messageError);
			lineLinkPackage2.IsLinked = true;
			lineLinkPackage2.Quantity = 1000000;
			AssertHasErrorContaining(lineLinkPackage2.QuantityInfo, "The number 1,000,000 is too large, the maximum value allowed for selection is 999,999.999.");
			lineLinkPackage2.Quantity = 999999.999;
			AssertNoErrors(lineLinkPackage2.QuantityInfo);
		}
	}
}
