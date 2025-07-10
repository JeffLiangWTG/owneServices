using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderPackagePivot))]
	sealed class InvoiceHeaderPackagePivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestClearPivotsOnChildInvoiceLine()
		{
			var pivot = (InvoiceHeaderPackagePivot)GetNewBusinessObject();
			var header = pivot.InvoiceHeader;
			var declaration = header.JobDeclaration;

			var header2 = declaration.Invoices.AddNew();

			var package1 = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();

			var line1 = header.InvoiceLines.AddNew();
			var line2 = header.InvoiceLines.AddNew();

			var pivot1 = line1.PackagesPivot.AddPivotFor(package1);
			var pivot2 = line2.PackagesPivot.AddPivotFor(package1);

			var pivot3 = line1.PackagesPivot.AddPivotFor(package2);
			var pivot4 = line2.PackagesPivot.AddPivotFor(package2);

			AssertEquals(false, pivot1.IsDeleted);
			AssertEquals(false, pivot2.IsDeleted);
			AssertEquals(false, pivot3.IsDeleted);
			AssertEquals(false, pivot4.IsDeleted);

			header.PackagesPivot.AddPivotFor(package1);

			AssertEquals("Should be deleted as there is a matched pivot on invoice header.", true, pivot1.IsDeleted);
			AssertEquals("Should be deleted as there is a matched pivot on invoice header.", true, pivot2.IsDeleted);
			AssertEquals("Should not be deleted as there is no pivot with same package on invoice header.", false, pivot3.IsDeleted);
			AssertEquals("Should not be deleted as there is no pivot with same package on invoice header.", false, pivot4.IsDeleted);

			header2.PackagesPivot.AddPivotFor(package2);

			AssertEquals("Should not be deleted as the deleted pivot is on different invoice header.", false, pivot3.IsDeleted);
			AssertEquals("Should not be deleted as the deleted pivot is on different invoice header.", false, pivot4.IsDeleted);
		}

		public void TestNumberOfPacksOfParentPivotWhileSelectingPivot()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var bill = createBill(declaration, "HB","001");
			var parentPackage = CreatePackage(declaration, "te3", 100, bill);
			var package1 = CreatePackage(declaration, "te3", 40, bill, parentPackage.PK);
			var package2 = CreatePackage(declaration, "te3", 10, bill, parentPackage.PK);

			var invoiceHeader1 = declaration.Invoices.AddNew();

			invoiceHeader1.ToggleLinkageWithPackage(package1, true);
			var parentPivot1 = invoiceHeader1.PackagesPivot.Cast<InvoiceHeaderPackagePivot>().FirstOrDefault(x => x.CHZ_CW == parentPackage.PK);
			AssertEquals(40, parentPivot1.CHZ_NumberOfPacks);

			invoiceHeader1.ToggleLinkageWithPackage(package2, true);
			AssertEquals(50, parentPivot1.CHZ_NumberOfPacks);

			invoiceHeader1.ToggleLinkageWithPackage(package2, false);
			AssertEquals(40, parentPivot1.CHZ_NumberOfPacks);

			var package3 = CreatePackage(declaration, "te3", 50, bill, parentPackage.PK);
			var invoiceHeader2 = declaration.Invoices.AddNew();
			
			invoiceHeader2.ToggleLinkageWithPackage(package3, true);
			var parentPivot2 = invoiceHeader2.PackagesPivot.Cast<InvoiceHeaderPackagePivot>().FirstOrDefault(x => x.CHZ_CW == parentPackage.PK);
			AssertEquals(50, parentPivot2.CHZ_NumberOfPacks);
		}

		public void TestNumberOfPacksOfParentPivotWhileUpdatingQtyOfPivot()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var bill = createBill(declaration, "HB", "001");
			var parentPackage = CreatePackage(declaration, "te3", 100, bill);
			var package1 = CreatePackage(declaration, "te3", 40, bill, parentPackage.PK);
			var package2 = CreatePackage(declaration, "te3", 10, bill, parentPackage.PK);

			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.ToggleLinkageWithPackage(package1, true);

			var pivot1 = invoiceHeader1.PackagesPivot.Cast<InvoiceHeaderPackagePivot>().First(x => x.CHZ_CW == package1.PK);
			var parentPivot1 = invoiceHeader1.PackagesPivot.Cast<InvoiceHeaderPackagePivot>().First(x => x.CHZ_CW == parentPackage.PK);
			((ICusPackagePivot)pivot1).NumberOfPacks = 30;
			AssertEquals(30, parentPivot1.CHZ_NumberOfPacks);

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.ToggleLinkageWithPackage(package2, true);

			var pivot2 = invoiceHeader2.PackagesPivot.Cast<InvoiceHeaderPackagePivot>().First(x => x.CHZ_CW == package2.PK);
			var parentPivot2 = invoiceHeader2.PackagesPivot.Cast<InvoiceHeaderPackagePivot>().First(x => x.CHZ_CW == parentPackage.PK);
			((ICusPackagePivot)pivot2).NumberOfPacks = 5;
			AssertEquals(5, parentPivot2.CHZ_NumberOfPacks);
		}

		public void TestSupportsNotes()
		{
			AssertEquals("SupportsNotes should be false", false, Factory.GetNull<InvoiceHeaderPackagePivot>().SupportsNotes);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var pivot = Factory.New<InvoiceHeaderPackagePivot>();

			var declaration = Factory.New<BaseJobDeclaration>();
			pivot.CHZ_JE = declaration.PK;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_JE = declaration.PK;
			pivot.CHZ_JZ = invoiceHeader.PK;

			var package = declaration.Packages.AddNew();
			var bill = declaration.Bills.AddNew();
			var houseContainerPack = bill.PackingGroups.AddNew();
			houseContainerPack.CR_CU_HouseBill = bill.PK;

			var cusContainer = declaration.CusContainers.AddNew();
			houseContainerPack.CR_CO_Container = cusContainer.PK;

			package.CW_CR_HouseContainer = houseContainerPack.PK;
			package.CW_PackType = "te3";
			package.CW_PackQty = 3;
			package.CW_InBondPackQty = 3;
			package.CW_OuterPacks = 3;
			package.CW_MarksAndNos = "te2";
			package.CW_ShippingSymbol = "te1";

			pivot.CHZ_CW = package.PK;

			return pivot;
		}

		BasePackage CreatePackage(BaseJobDeclaration declaration,ZString packType, int packQty, Bill bill, ZGuid? parentPackagePK = null)
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
