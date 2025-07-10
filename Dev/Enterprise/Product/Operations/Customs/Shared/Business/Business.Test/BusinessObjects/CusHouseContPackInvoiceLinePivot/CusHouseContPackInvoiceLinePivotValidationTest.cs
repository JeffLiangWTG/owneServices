using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusHouseContPackInvoiceLinePivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestNumberOfPacks()
		{
			var chc = Factory.New<InvoiceLinePackagePivot>();
			chc.CHC_NumberOfPacks = 1;
			AssertNoMessageErrorContaining(chc.CHC_NumberOfPacksInfo, "enter");
			chc.CHC_NumberOfPacks = 0;
			AssertHasMessageErrorContaining(chc.CHC_NumberOfPacksInfo, "enter");
		}

		public void TestPackageCountValidation()
		{
			var mockDeclaration = Factory.NewMoq<PivotBetweenCWandJITest.BaseJobDeclarationWhichSupportsPackagesPivot>();
			var maxNumberOfPacks = 99999;
			mockDeclaration.Protected().Setup<ZInt>("MaximumNumberOfPacksForEntryLineCore").Returns(maxNumberOfPacks);
			var errorMessage = $"The pack quantity ({maxNumberOfPacks}) has been exceeded.\r\nThis number reflects other packs within the invoice line or entry line.";

			PivotBetweenCWandJITest.BaseJobDeclarationWhichSupportsPackagesPivot dec = mockDeclaration.Object;

			dec.JE_TransportMode = "AIR";
			dec.JE_MasterBill = "123456789012";
			dec.JE_TotalNoOfPieces = 1;
			var pack1 = dec.Bills[0].PackingGroups[0].Packages[0];
			pack1.CW_PackQty = 100001;
			var pack2 = dec.Bills[0].PackingGroups[0].Packages.AddNew();
			pack2.CW_PackQty = 51000;
			var pack3 = dec.Bills[0].PackingGroups[0].Packages.AddNew();
			pack3.CW_PackQty = 50000;
			var pack4 = dec.Bills[0].PackingGroups[0].Packages.AddNew();
			pack4.CW_PackQty = 40000;

			var invoice = dec.Invoices.AddNew();

			var invLine1 = dec.InvoiceLines.AddNew();
			invLine1.JI_JZ = invoice.PK;
			invoice.InvoiceLines.Add(invLine1);

			ResetPackagesForInvoiceLine(invLine1);
			invLine1.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;
			AssertHasMessageErrorContaining(invLine1.PackagesForInvoiceLinesForBindingOnly[0].PackQtyInfo, errorMessage);

			ResetPackagesForInvoiceLine(invLine1);
			invLine1.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = true;
			AssertNoMessageErrorContaining(invLine1.PackagesForInvoiceLinesForBindingOnly[1].PackQtyInfo, errorMessage);

			ResetPackagesForInvoiceLine(invLine1);
			invLine1.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = true;
			invLine1.PackagesForInvoiceLinesForBindingOnly[2].IsLinked = true;
			AssertHasMessageErrorContaining(invLine1.PackagesForInvoiceLinesForBindingOnly[2].PackQtyInfo, errorMessage);

			ResetPackagesForInvoiceLine(invLine1);
			invLine1.PackagesForInvoiceLinesForBindingOnly[2].IsLinked = true;
			invLine1.PackagesForInvoiceLinesForBindingOnly[3].IsLinked = true;
			AssertNoMessageErrorContaining(invLine1.PackagesForInvoiceLinesForBindingOnly[3].PackQtyInfo, errorMessage);

			var invLine2 = dec.InvoiceLines.AddNew();
			invLine2.JI_JZ = invoice.PK;
			invoice.InvoiceLines.Add(invLine2);

			var cusEntryLine = dec.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			invLine1.JI_CL = cusEntryLine.PK;
			invLine2.JI_CL = cusEntryLine.PK;

			ResetPackagesForInvoiceLine(invLine1);
			invLine1.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = true;

			ResetPackagesForInvoiceLine(invLine2);
			invLine2.PackagesForInvoiceLinesForBindingOnly[2].IsLinked = true;
			AssertHasMessageErrorContaining(invLine2.PackagesForInvoiceLinesForBindingOnly[2].PackQtyInfo, errorMessage);

			invLine2.Delete();
			foreach (BaseCusLinkPackage package in invLine1.PackagesForInvoiceLinesForBindingOnly)
			{
				AssertNoMessageErrorContaining(package.PackQtyInfo, errorMessage);
			}

			ResetPackagesForInvoiceLine(invLine1);
			invLine1.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = true;
			invLine1.PackagesForInvoiceLinesForBindingOnly[2].IsLinked = true;
			invLine1.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = false;
			AssertNoMessageErrorContaining(invLine1.PackagesForInvoiceLinesForBindingOnly[1].PackQtyInfo, errorMessage);
			AssertNoMessageErrorContaining(invLine1.PackagesForInvoiceLinesForBindingOnly[2].PackQtyInfo, errorMessage);

			var mockDeclaration2 = Factory.NewMoq<PivotBetweenCWandJITest.BaseJobDeclarationWhichSupportsPackagesPivot>();
			maxNumberOfPacks = -1;
			mockDeclaration2.Protected().Setup<ZInt>("MaximumNumberOfPacksForEntryLineCore").Returns(maxNumberOfPacks);
			errorMessage = $"The pack quantity ({maxNumberOfPacks}) has been exceeded.\r\nThis number reflects other packs within the invoice line or entry line.";

			dec = mockDeclaration2.Object;

			dec.JE_TransportMode = "AIR";
			dec.JE_MasterBill = "123456789012";
			dec.JE_TotalNoOfPieces = 1;
			pack1 = dec.Bills[0].PackingGroups[0].Packages[0];
			pack1.CW_PackQty = 100001;
			pack2 = dec.Bills[0].PackingGroups[0].Packages.AddNew();
			pack2.CW_PackQty = 51000;
			pack3 = dec.Bills[0].PackingGroups[0].Packages.AddNew();
			pack3.CW_PackQty = 50000;
			pack4 = dec.Bills[0].PackingGroups[0].Packages.AddNew();
			pack4.CW_PackQty = 40000;

			invoice = dec.Invoices.AddNew();

			invLine1 = dec.InvoiceLines.AddNew();
			invLine1.JI_JZ = invoice.PK;
			invoice.InvoiceLines.Add(invLine1);

			ResetPackagesForInvoiceLine(invLine1);
			invLine1.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;
			invLine1.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = true;
			invLine1.PackagesForInvoiceLinesForBindingOnly[2].IsLinked = true;
			invLine1.PackagesForInvoiceLinesForBindingOnly[3].IsLinked = true;
			AssertEquals(0, invLine1.PackagesForInvoiceLinesForBindingOnly[0].PackQtyInfo.Notifications.Count());
			AssertEquals(0, invLine1.PackagesForInvoiceLinesForBindingOnly[1].PackQtyInfo.Notifications.Count());
			AssertEquals(0, invLine1.PackagesForInvoiceLinesForBindingOnly[2].PackQtyInfo.Notifications.Count());
			AssertEquals(0, invLine1.PackagesForInvoiceLinesForBindingOnly[3].PackQtyInfo.Notifications.Count());
		}

		void ResetPackagesForInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
			foreach (BaseCusLinkPackage packagePivot in invoiceLine.PackagesForInvoiceLinesForBindingOnly)
			{
				packagePivot.IsLinked = false;
			}
		}
	}
}
