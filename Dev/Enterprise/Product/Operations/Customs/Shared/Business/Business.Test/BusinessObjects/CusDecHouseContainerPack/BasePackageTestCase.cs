using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Customs.Business.Testing.PivotBetweenCWandJITest;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BasePackageTestCase : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestLinkToPackingGroupWithNulls()
		{
			package.LinkToPackingGroup(null, null);
		}

		public void TestHasChangesSuspendedWhileLinkingToPackingGroup()
		{
			AssertEquals(false, bill.IsSettingHasChangesSuspended);
			AssertEquals(false, container.IsSettingHasChangesSuspended);

			package = declaration.Packages.AddNew();
			AssertEquals(false, package.IsSettingHasChangesSuspended);
			AssertEquals(false, package.HasChanges);

			package.LinkToPackingGroup(bill, container);
			AssertEquals(false, package.IsSettingHasChangesSuspended);
			AssertEquals(true, package.HasChanges);

			using (bill.SuspendSettingHasChanges())
			{
				AssertEquals(true, bill.IsSettingHasChangesSuspended);

				package = declaration.Packages.AddNew();
				AssertEquals(false, package.IsSettingHasChangesSuspended);
				AssertEquals(false, package.HasChanges);

				package.LinkToPackingGroup(bill, container);
				AssertEquals("The package's suspender has been disposed.", false, package.IsSettingHasChangesSuspended);
				AssertEquals(false, package.HasChanges);
			}
		}

		public void TestSetHouseBillAndContainerThenRemoveHouseBill()
		{
			package.CW_HouseBill = bill.CU_BillUniqueCode;
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			AssertEquals("one pivot created", 1, container.PackingGroups.Count);
			AssertEquals("one pivot created", 1, declaration.PackingGroups.Count);
			AssertEquals(package.CW_CR_HouseContainer, container.PackingGroups[0].PK);
			AssertEquals(bill.PK, container.PackingGroups[0].CR_CU_HouseBill);
			AssertEquals(container.PK, container.PackingGroups[0].CR_CO_Container);

			package.CW_HouseBill = "";
			AssertEquals("there is still one pivot", 1, declaration.PackingGroups.Count);
			AssertEquals(ZGuid.Empty, declaration.PackingGroups[0].CR_CU_HouseBill);
			AssertEquals(container.PK, declaration.PackingGroups[0].CR_CO_Container);
		}

		public void TestPackingGroupsWithPackagesWontGetDeleted()
		{
			package.CW_HouseBill = bill.CU_BillUniqueCode;
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			package.CW_PackQty = 10;
			AssertEquals("one pivot created", 1, declaration.PackingGroups.Count);

			BasePackage package2 = declaration.Packages.AddNew();
			package2.CW_HouseBill = bill.CU_BillUniqueCode;
			package2.CW_PackQty = 10;
			AssertEquals("Another pivot created", 2, declaration.PackingGroups.Count);

			package2.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			AssertEquals("There is only one pivot in the declaration now as package and package2 are linked to the same combination", 1, declaration.PackingGroups.Count);

			AssertNoExceptionThrown(() => Factory.Save());

			package2.CW_HouseBill = "";
			AssertEquals("Now package and package2 are linked to different combinations and there should be two pivots", 2, declaration.PackingGroups.Count);
		}

		public void TestLinkToRightCombinationOfHouseBillAndContainer()
		{
			BasePackage packageWithContainer = declaration.Packages.AddNew();
			packageWithContainer.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;

			BasePackage packageWithHouseBillAndContainer = declaration.Packages.AddNew();
			packageWithHouseBillAndContainer.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			packageWithHouseBillAndContainer.CW_HouseBill = bill.CU_BillUniqueCode;
			AssertEquals("PreCondition:There should be two pivots created", 2, declaration.PackingGroups.Count);

			AssertEquals("PreCondition;Package is not linked yet", ZGuid.Empty, package.CW_CR_HouseContainer);
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			AssertEquals("It should be linked to the same pivot as packageWithContainer", packageWithContainer.PackingGroup, package.PackingGroup);
			AssertEquals("packageWithHouseBillAndContainer.PackingGroup should not be deleted", false, packageWithHouseBillAndContainer.PackingGroup.IsDeleted);

			package.CW_HouseBill = bill.CU_BillUniqueCode;
			AssertEquals("It should be linked to the same pivot as packageWithHouseBillAndContainer", packageWithHouseBillAndContainer.PackingGroup, package.PackingGroup);
			AssertEquals("packageWithContainer.PackingGroup should not be deleted", false, packageWithContainer.PackingGroup.IsDeleted);
		}

		public void TestValidationGetsTriggeredProperlyOnHouseBill()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);
			BaseJobDeclaration declaration = mockDeclaration.Object;
			declaration.DisableDefaultPackingInformation = true;

			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "DOESEXIST";
			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000006";
			Assert("House Bill should have Messaging Error", houseBill.CU_BillNumInfo.HasMessageError(CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill));
			Assert("Container should have Messaging Error", container.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.ContainersRequirePackages));
			BasePackage package = declaration.Packages.AddNew();
			package.CW_HouseBill = houseBill.CU_BillUniqueCode;
			Assert("House Bill should have no Messaging Error", !houseBill.CU_BillNumInfo.HasMessageError(CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill));
			Assert("Container should have Messaging Error", container.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.ContainersRequirePackages));
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			Assert("House Bill should have no Messaging Error", !houseBill.CU_BillNumInfo.HasMessageError(CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill));
			Assert("Container should have no Messaging Error", !container.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.ContainersRequirePackages));
			package.CW_ContainerNoOrEquipmentNo = "";
			Assert("House Bill should have no Messaging Error", !houseBill.CU_BillNumInfo.HasMessageError(CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill));
			Assert("Container should have Messaging Error", container.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.ContainersRequirePackages));
			package.CW_HouseBill = "";
			houseBill.Validation.ValidateCU_BillNum();
			Assert("House Bill should have Messaging Error", houseBill.CU_BillNumInfo.HasMessageError(CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill));
			container.Validation.ValidateCO_ContainerNumber();
			Assert("Container should have Messaging Error", container.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.ContainersRequirePackages));
		}

		public void TestSynchronizeInvoiceLineContainersWithPackages()
		{
			var declaration = Factory.New<BaseJobDeclarationWhichSupportsPackagesPivot>();
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CRUX1236665";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CRUX1236666";
			var package1 = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();

			package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			package2.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.ToggleLinkageWithPackage(package1, true);
			invoiceLine.ToggleLinkageWithPackage(package2, true);

			Assert("container1 is linked with invoice line", invoiceLine.ContainersPivot.Contains(container1));
			Assert("container2 is linked with invoice line", invoiceLine.ContainersPivot.Contains(container2));
			AssertNotNull("package1 is linked with invoice line", invoiceLine.PackagesPivot.GetRelatedPivot(package1));
			AssertNotNull("package2 is linked with invoice line", invoiceLine.PackagesPivot.GetRelatedPivot(package2));

			package1.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;
			Assert("container1 is NOT linked with invoice line", !invoiceLine.ContainersPivot.Contains(container1));
			Assert("container2 is linked with invoice line", invoiceLine.ContainersPivot.Contains(container2));
			AssertNotNull("package1 is linked with invoice line", invoiceLine.PackagesPivot.GetRelatedPivot(package1));
			AssertNotNull("package2 is linked with invoice line", invoiceLine.PackagesPivot.GetRelatedPivot(package2));

			package2.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			Assert("container1 is linked with invoice line", invoiceLine.ContainersPivot.Contains(container1));
			Assert("container2 is linked with invoice line", invoiceLine.ContainersPivot.Contains(container2));
			AssertNotNull("package1 is linked with invoice line", invoiceLine.PackagesPivot.GetRelatedPivot(package1));
			AssertNotNull("package2 is linked with invoice line", invoiceLine.PackagesPivot.GetRelatedPivot(package2));
		}

		public void TestValidationOnCW_PackQtyGetsInvokedOnChangesInInvoiceLineNumberOfPacks()
		{
			var declaration = Factory.New<BaseJobDeclarationWhichSupportsPackagesPivot>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 100;
			(invoiceLine1.PackagesPivot.AddPivotFor(package1) as InvoiceLinePackagePivot).CHC_NumberOfPacks = 50;
			(invoiceLine2.PackagesPivot.AddPivotFor(package1) as InvoiceLinePackagePivot).CHC_NumberOfPacks = 70;
			AssertHasMessageError(package1.CW_PackQtyInfo, PackQtyVsInvoiceLinesNumbersErrorMessage(120));

			invoiceLine1.PackagesPivot.DeletePivotFor(package1);
			AssertNoMessageError(package1.CW_PackQtyInfo, PackQtyVsInvoiceLinesNumbersErrorMessage(120));

			invoiceLine1.PackagesPivot.RemoveAll();
			invoiceLine2.PackagesPivot.RemoveAll();

			var package2 = declaration.Packages.AddNew();
			(invoiceLine1.PackagesPivot.AddPivotFor(package2) as InvoiceLinePackagePivot).CHC_NumberOfPacks = 50;
			(invoiceLine2.PackagesPivot.AddPivotFor(package2) as InvoiceLinePackagePivot).CHC_NumberOfPacks = 70;
			package2.CW_PackQty = 100;
			AssertHasMessageError(package2.CW_PackQtyInfo, PackQtyVsInvoiceLinesNumbersErrorMessage(120));
			(invoiceLine1.PackagesPivot.ToArray()[0] as InvoiceLinePackagePivot).CHC_NumberOfPacks = 5;
			AssertNoMessageError(package2.CW_PackQtyInfo, PackQtyVsInvoiceLinesNumbersErrorMessage(120));
		}

		public void TestValidationOnCW_PackQtyNotInvokedOnNonLowestPackages()
		{
			var declaration = Factory.New<BaseJobDeclarationWhichSupportsPackagesPivot>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var package1 = declaration.Packages.AddNew();
			var package1_1 = declaration.Packages.AddNew();
			package1_1.CW_CW_Parent = package1.PK;

			package1.CW_PackQty = 10;
			package1_1.CW_PackQty = 10;
			(invoiceLine.PackagesPivot.AddPivotFor(package1_1) as InvoiceLinePackagePivot).CHC_NumberOfPacks = 20;
			AssertHasMessageError(package1_1.CW_PackQtyInfo, PackQtyVsInvoiceLinesNumbersErrorMessage(20));
			AssertNoMessageError(package1.CW_PackQtyInfo, PackQtyVsInvoiceLinesNumbersErrorMessage(20));

			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 10;
			(invoiceLine.PackagesPivot.AddPivotFor(package2) as InvoiceLinePackagePivot).CHC_NumberOfPacks = 20;

			var package2_1 = declaration.Packages.AddNew();
			package2_1.CW_PackQty = 10;
			package2_1.CW_CW_Parent = package2.PK;
			(invoiceLine.PackagesPivot.AddPivotFor(package2_1) as InvoiceLinePackagePivot).CHC_NumberOfPacks = 20;

			AssertHasMessageError(package2_1.CW_PackQtyInfo, PackQtyVsInvoiceLinesNumbersErrorMessage(20));
			AssertNoMessageError(package2.CW_PackQtyInfo, PackQtyVsInvoiceLinesNumbersErrorMessage(20));

			var package3 = declaration.Packages.AddNew();
			var package3_1 = declaration.Packages.AddNew();
			var package4 = declaration.Packages.AddNew();

			package3.CW_PackQty = 10;
			package3_1.CW_PackQty = 10;
			package3_1.CW_CW_Parent = package3.PK;
			(invoiceLine.PackagesPivot.AddPivotFor(package3_1) as InvoiceLinePackagePivot).CHC_NumberOfPacks = 20;
			package3_1.CW_CW_Parent = package4.PK;

			AssertHasMessageError(package3_1.CW_PackQtyInfo, PackQtyVsInvoiceLinesNumbersErrorMessage(20));
			AssertNoMessageError(package3.CW_PackQtyInfo, PackQtyVsInvoiceLinesNumbersErrorMessage(20));
			AssertNoMessageError(package4.CW_PackQtyInfo, PackQtyVsInvoiceLinesNumbersErrorMessage(20));
		}

		string PackQtyVsInvoiceLinesNumbersErrorMessage(ZInt totalNumbers) => $"The total number of packs included in invoice(s) and invoice line(s) is {totalNumbers}, it exceeds this package quantity.";

		BaseJobDeclaration declaration;
		Bill bill;
		BaseCusContainer container;
		BasePackage package;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;

			bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			bill.CU_HouseBill = "11111";
			container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRUX1234562";

			package = declaration.Packages.AddNew();
			AssertEquals("No pivot yet", 0, bill.PackingGroups.Count);
		}
	}
}
