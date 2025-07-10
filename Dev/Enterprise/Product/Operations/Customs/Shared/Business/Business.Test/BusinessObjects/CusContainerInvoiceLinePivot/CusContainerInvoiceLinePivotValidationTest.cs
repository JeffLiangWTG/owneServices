using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusContainerInvoiceLinePivotValidationTest : TestCaseWithFactory
	{
		public void TestValidateC2_CO()
		{
			var expectedMessage = "This container is not selected in the 'Invoice Headers/Invoice Lines - Packages' grid. Please untick 'Is For Invoice Line' on or select a package or packages for this container.";

			var declaration = Factory.New<PivotBetweenCWandJITest.BaseJobDeclarationWhichSupportsPackagesPivot>();

			var container1 = declaration.CusContainers.AddNew();
			var container2 = declaration.CusContainers.AddNew();

			container1.CO_ContainerNumber = "1111111";
			container2.CO_ContainerNumber = "2222222";
			declaration.JE_MasterBill = "111-22222222";  //creates bills

			var package1 = declaration.Bills[0].PackingGroups[0].Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;

			var package2 = declaration.Bills[0].PackingGroups[0].Packages.AddNew();
			package2.CW_PackQty = 2;
			package2.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = Factory.New<PivotBetweenCWandJITest.BaseJobComInvoiceLineWhichSupportsPackagesPivot>();
			invoiceLine.JI_JZ = invoiceHeader.PK;
			invoiceHeader.InvoiceLines.Add(invoiceLine);

			var invoiceLinePivot1 = invoiceLine.PackagesPivot.AddNew();
			invoiceLinePivot1.CHC_CW = package1.PK;
			invoiceLinePivot1.CHC_NumberOfPacks = 1;

			var containerPivot1 = invoiceLine.ContainersPivot.AddNew();
			containerPivot1.C2_CO = container1.PK;

			var containerPivot2 = invoiceLine.ContainersPivot.AddNew();
			containerPivot2.C2_CO = container2.PK;

			AssertNoMessageError(containerPivot1.C2_COInfo, expectedMessage);
			AssertHasMessageError(containerPivot2.C2_COInfo, expectedMessage); // C2's CO has no corresponding CHC

			var invoicePivot = invoiceHeader.PackagesPivot.AddNew();
			invoicePivot.CHZ_CW = package2.PK;
			invoicePivot.CHZ_NumberOfPacks = 5;

			containerPivot2.Validation.ValidateC2_CO();
			AssertNoMessageError(containerPivot2.C2_COInfo, expectedMessage);

			invoiceHeader.PackagesPivot.RemoveAndDeleteAll();

			containerPivot2.Validation.ValidateC2_CO();
			AssertHasMessageError(containerPivot2.C2_COInfo, expectedMessage);

			var invoiceLinePivot2 = invoiceLine.PackagesPivot.AddNew();
			invoiceLinePivot2.CHC_CW = package2.PK;
			invoiceLinePivot2.CHC_NumberOfPacks = 2;

			containerPivot2.Validation.ValidateC2_CO();
			AssertNoMessageError(containerPivot2.C2_COInfo, expectedMessage);
		}

		public void TestValidateAllWithInvalidContainer()
		{
			var jobDeclaration = Factory.New<PivotBetweenCWandJITest.BaseJobDeclarationWhichSupportsPackagesPivot>();
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var cusContainer = jobDeclaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "CON111111";
			var containerPivot = invoiceLine.ContainersPivot.AddPivotFor(cusContainer);
			var package = jobDeclaration.Packages.AddNew();
			var packagePivot = invoiceLine.PackagesPivot.AddPivotFor(package);
			AssertNoExceptionThrown(() => cusContainer.Validation.ValidateAll());
			containerPivot.C2_CO = ZGuid.Empty;
			AssertNoExceptionThrown(() => cusContainer.Validation.ValidateAll());
		}
	}
}
