using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Customs.Business.Testing.PivotBetweenCWandJITest;

namespace Enterprise.Customs.Business.Testing
{
	public class CusDecHouseContainerPackValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCW_PackQtyValidationHasCorrectNotificationType()
		{
			var declaration = GetJobDeclaration();

			if (declaration.SupportsChzPivotBetweenInvoiceHeaderAndPacking)
			{
				AssertValidatePackQtyForInvoice();
			}

			if (declaration.SupportsChcPivotBetweenInvoiceLineAndPacking)
			{
				AssertValidatePackQtyForInvoiceLine();
			}

			if (declaration.SupportsChzPivotBetweenInvoiceHeaderAndPacking && declaration.SupportsChcPivotBetweenInvoiceLineAndPacking)
			{
				AssertValidatePackQtyForInvoiceAndLines();
			}

			Assert(true);
		}

		void AssertValidatePackQtyForInvoice()
		{
			var declaration = GetJobDeclaration();

			var invoice = declaration.Invoices.AddNew();

			var package = declaration.Packages.AddNew();
			package.CW_PackQty = 10;

			invoice.PackagesPivot.AddPivotFor(package).NumberOfPacks = 10;
			MakeAssertionsOnCW_PackQtyValidationNotificationType(declaration);
		}

		void AssertValidatePackQtyForInvoiceLine()
		{
			var declaration = GetJobDeclaration();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var package = declaration.Packages.AddNew();
			package.CW_PackQty = 10;

			invoiceLine.PackagesPivot.AddPivotFor(package).NumberOfPacks = 10;
			MakeAssertionsOnCW_PackQtyValidationNotificationType(declaration);
		}

		void AssertValidatePackQtyForInvoiceAndLines()
		{
			var declaration = GetJobDeclaration();

			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoice2.InvoiceLines.AddNew();

			var package = declaration.Packages.AddNew();
			package.CW_PackQty = 20;

			var invoiceLinePackPivot = invoiceLine1.PackagesPivot.AddPivotFor(package);
			invoiceLinePackPivot.NumberOfPacks = 10;

			var invoicePackPivot = invoice2.PackagesPivot.AddPivotFor(package);
			invoicePackPivot.NumberOfPacks = 10;

			MakeAssertionsOnCW_PackQtyValidationNotificationType(declaration);
		}

		public void TestCW_HouseBillWithDeclaration()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "12345";
			BasePackage package = declaration.Packages.AddNew();
			package.CW_HouseBill = "";
			AssertEquals("Package.CW_HouseBillInfo.HasErrors() when Empty", true, package.CW_HouseBillInfo.HasError(CusDecHouseContainerPackValidation.SelectAValidHouseBill));
			package.CW_HouseBill = houseBill.CU_BillUniqueCode;
			AssertEquals("Package.CW_HouseBillInfo.HasErrors() when Filled in Correctly", false, package.CW_HouseBillInfo.HasError(CusDecHouseContainerPackValidation.SelectAValidHouseBill));
			package.CW_HouseBill = "67890";
			AssertEquals("Package.CW_HouseBillInfo.HasErrors() when Filled in Incorrectly", true, package.CW_HouseBillInfo.HasError(CusDecHouseContainerPackValidation.SelectAValidHouseBill));
			package.CW_HouseBill = houseBill.CU_BillUniqueCode;
			AssertEquals("Package.CW_HouseBillInfo.HasErrors() when Filled in Correctly", false, package.CW_HouseBillInfo.HasError(CusDecHouseContainerPackValidation.SelectAValidHouseBill));
			package.CW_HouseBill = "";
			AssertEquals("Package.CW_HouseBillInfo.HasErrors() when Empty", true, package.CW_HouseBillInfo.HasError(CusDecHouseContainerPackValidation.SelectAValidHouseBill));
		}

		[ExpectNoExceptions()]
		public void TestCW_HouseBillWithoutDeclarationDoesntThrowAnException()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			BasePackage package = declaration.Packages.AddNew();
			package.CW_HouseBill = "";
		}

		[ExpectNoExceptions()]
		public void TestCW_ContainerNoOrEquipmentNoWithoutDeclarationThrowsNoException()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			BasePackage package = declaration.Packages.AddNew();
			package.CW_ContainerNoOrEquipmentNo = "";
		}

		protected virtual BaseJobDeclaration GetJobDeclaration()
		{
			return BaseJobDeclaration.New(Factory);
		}

		protected virtual void MakeAssertionsOnCW_PackQtyValidationNotificationType(BaseJobDeclaration declaration)
		{
			var message = "The total number of packs included in invoice(s) and invoice line(s) is 20, it exceeds this package quantity.";

			var invoiceLine1 = declaration.InvoiceLines[0];
			var package = declaration.Packages[0];

			AssertNoMessageError(package.CW_PackQtyInfo, message);

			invoiceLine1.PackagesPivot[0].CHC_NumberOfPacks = 20;

			AssertHasMessageError("Package.CW_PackQty when Filled in Incorrectly", package.CW_PackQtyInfo, message);
		}
	}

	sealed class CusDecHouseContainerPackValidationBASEONLYTest : BusinessObjectValidationTestCase
	{
		public void TestCW_ContainerNoOrEquipmentNoWithDeclaration()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000006";
			var package = declaration.Packages.AddNew();
			package.CW_ContainerNoOrEquipmentNo = "OOCL0000011";
			AssertHasErrorContaining(package.CW_ContainerNoOrEquipmentNoInfo, ListValidation.InvalidCodeError);
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			AssertNoErrors(package.CW_ContainerNoOrEquipmentNoInfo);
			package.CW_ContainerNoOrEquipmentNo = ZString.Empty;
			AssertNoErrors(package.CW_ContainerNoOrEquipmentNoInfo);
		}

		public void TestCheckCW_PackQty()
		{
			var declaration = Factory.New<BaseJobDeclarationWhichSupportsPackagesPivot>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var invoiceLine3 = invoice.InvoiceLines.AddNew();

			var package = declaration.Packages.AddNew();
			(invoiceLine1.PackagesPivot.AddPivotFor(package) as InvoiceLinePackagePivot).CHC_NumberOfPacks = 50;
			(invoiceLine2.PackagesPivot.AddPivotFor(package) as InvoiceLinePackagePivot).CHC_NumberOfPacks = 40;
			(invoiceLine3.PackagesPivot.AddPivotFor(package) as InvoiceLinePackagePivot).CHC_NumberOfPacks = 20;

			package.CW_PackQty = 100;
			AssertHasMessageError(package.CW_PackQtyInfo, packQtyVsInvoicesOrInvoiceLinesNumbersErrorMessage(110));

			package.CW_PackQty = 110;
			AssertNoMessageError(package.CW_PackQtyInfo, packQtyVsInvoicesOrInvoiceLinesNumbersErrorMessage(110));
		}

		public void TestCheckCW_PackQty_InvoiceLevel()
		{
			var declaration = Factory.New<BaseJobDeclarationWhichSupportsPackagesPivot>();
			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();

			var package = declaration.Packages.AddNew();
			(invoice1.PackagesPivot.AddPivotFor(package) as InvoiceHeaderPackagePivot).CHZ_NumberOfPacks = 50;
			(invoice2.PackagesPivot.AddPivotFor(package) as InvoiceHeaderPackagePivot).CHZ_NumberOfPacks = 40;

			package.CW_PackQty = 80;
			AssertHasMessageError(package.CW_PackQtyInfo, packQtyVsInvoicesOrInvoiceLinesNumbersErrorMessage(90));

			package.CW_PackQty = 90;
			AssertNoMessageError(package.CW_PackQtyInfo, packQtyVsInvoicesOrInvoiceLinesNumbersErrorMessage(90));
		}

		string packQtyVsInvoicesOrInvoiceLinesNumbersErrorMessage(ZInt totalNumbers) => $"The total number of packs included in invoice(s) and invoice line(s) is {totalNumbers}, it exceeds this package quantity.";

		public void TestCheckCW_MarksAndNos()
		{
			var dec = Factory.New<JobDeclarationThatWantsPackageMarks>();
			dec.JE_MasterBill = "12";
			var pack = dec.Packages.AddNew();
			pack.CW_MarksAndNos = "Any";
			AssertNoMessageErrorContaining(pack.CW_MarksAndNosInfo, "Show");
			pack.CW_MarksAndNos = "";
			AssertHasMessageErrorContaining(pack.CW_MarksAndNosInfo, "Show");
		}

		class JobDeclarationThatWantsPackageMarks : BaseJobDeclaration
		{
			public JobDeclarationThatWantsPackageMarks(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{ }

			protected override ZString PackageMarksAndNumbersAlwaysRequiredValidationMessageCore => "Show me the packages";
		}
	}
}
