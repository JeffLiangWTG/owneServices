using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(InvoiceLinePackagePivot))]
	sealed class InvoiceLinePackagePivotPackNumberTest : TestCaseWithFactory
	{
		public void TestNumberOfPacksOfParentPivotWhileSelectingPivot()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var bill = createBill(declaration, "HB", "001");
			var parentPackage = CreatePackage(declaration, "te3", 100, bill);
			var package1 = CreatePackage(declaration, "te3", 40, bill, parentPackage.PK);
			var package2 = CreatePackage(declaration, "te3", 10, bill, parentPackage.PK);

			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();

			invoiceLine1.ToggleLinkageWithPackage(package1, true);
			var parentPivot1 = invoiceLine1.PackagesPivot.Cast<InvoiceLinePackagePivot>().FirstOrDefault(x => x.CHC_CW == parentPackage.PK);
			AssertEquals(40, parentPivot1.CHC_NumberOfPacks);

			invoiceLine1.ToggleLinkageWithPackage(package2, true);
			AssertEquals(50, parentPivot1.CHC_NumberOfPacks);

			invoiceLine1.ToggleLinkageWithPackage(package2, false);
			AssertEquals(40, parentPivot1.CHC_NumberOfPacks);

			var package3 = CreatePackage(declaration, "te3", 50, bill, parentPackage.PK);
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();

			invoiceLine2.ToggleLinkageWithPackage(package3, true);
			var parentPivot2 = invoiceLine2.PackagesPivot.Cast<InvoiceLinePackagePivot>().FirstOrDefault(x => x.CHC_CW == parentPackage.PK);
			AssertEquals(50, parentPivot2.CHC_NumberOfPacks);
		}

		public void TestNumberOfPacksOfParentPivotWhileUpdatingQtyOfPivot()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var bill = createBill(declaration, "HB", "001");
			var parentPackage = CreatePackage(declaration, "te3", 100, bill);
			var package1 = CreatePackage(declaration, "te3", 40, bill, parentPackage.PK);
			var package2 = CreatePackage(declaration, "te3", 10, bill, parentPackage.PK);

			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.ToggleLinkageWithPackage(package1, true);

			var pivot1 = invoiceLine1.PackagesPivot.Cast<InvoiceLinePackagePivot>().First(x => x.CHC_CW == package1.PK);
			var parentPivot1 = invoiceLine1.PackagesPivot.Cast<InvoiceLinePackagePivot>().First(x => x.CHC_CW == parentPackage.PK);
			((ICusPackagePivot)pivot1).NumberOfPacks = 30;
			AssertEquals(30, parentPivot1.CHC_NumberOfPacks);

			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.ToggleLinkageWithPackage(package2, true);

			var pivot2 = invoiceLine2.PackagesPivot.Cast<InvoiceLinePackagePivot>().First(x => x.CHC_CW == package2.PK);
			var parentPivot2 = invoiceLine2.PackagesPivot.Cast<InvoiceLinePackagePivot>().First(x => x.CHC_CW == parentPackage.PK);
			((ICusPackagePivot)pivot2).NumberOfPacks = 5;
			AssertEquals(5, parentPivot2.CHC_NumberOfPacks);
		}

		public void TestNumberOfPacksOfParentPivotWithInvoiceHeader()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var bill = createBill(declaration, "HB", "001");
			var parentPackage = CreatePackage(declaration, "te3", 100, bill);
			var package1 = CreatePackage(declaration, "te3", 40, bill, parentPackage.PK);
			var package2 = CreatePackage(declaration, "te3", 10, bill, parentPackage.PK);

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			invoiceLine.ToggleLinkageWithPackage(package1, true);
			invoiceHeader.ToggleLinkageWithPackage(package1, true);
			var parentPivot = invoiceHeader.PackagesPivot.Cast<InvoiceHeaderPackagePivot>().FirstOrDefault(x => x.CHZ_CW == parentPackage.PK);
			AssertEquals(40, parentPivot.CHZ_NumberOfPacks);

			invoiceLine.ToggleLinkageWithPackage(package2, true);
			AssertEquals(50, parentPivot.CHZ_NumberOfPacks);
		}

		BasePackage CreatePackage(BaseJobDeclaration declaration, ZString packType, int packQty, Bill bill, ZGuid? parentPackagePK = null)
		{
			var package = declaration.Packages.AddNew();
			package.CW_HouseBill = bill.CU_HouseBill;
			package.CW_PackType = packType;
			package.CW_PackQty = packQty;
			if (null != parentPackagePK)
			{
				package.CW_CW_Parent = (ZGuid)parentPackagePK;
			}
			return package;
		}

		Bill createBill(BaseJobDeclaration declaration, ZString billType, ZString billNum)
		{
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = billType;
			bill.CU_BillNum = billNum;
			return bill;
		}
	}
}

