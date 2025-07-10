using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseOnlyNonPersistentCusContainerTest : TestCaseWithFactory
	{
		public void TestValidateIsForInvoiceLine()
		{
			declaration.DisableDefaultPackingInformation = true;

			var bill = declaration.Bills.AddNew();
			container1.CO_ContainerNumber = "CRUX1234562";
			bill.PackingGroups.AddNew().CR_CO_Container = container1.PK;

			var bill2 = declaration.Bills.AddNew();
			container2.CO_ContainerNumber = "CRUX2345627";
			bill2.PackingGroups.AddNew().CR_CO_Container = container2.PK;

			invoice.JZ_CU_RelatedHouseBill = bill.PK;

			pivot1ValidationMock.Protected().Setup<NotificationTypes>("ValidationTypeForBillContainerPivotMatching").Returns(NotificationTypes.MessageError);
			pivot2ValidationMock.Protected().Setup<NotificationTypes>("ValidationTypeForBillContainerPivotMatching").Returns(NotificationTypes.MessageError);

			var containersForInvoiceLines = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			var npContainer1 = containersForInvoiceLines[0];
			var npContainer2 = containersForInvoiceLines[1];
			if (npContainer2.Container == container1)
			{
				npContainer1 = containersForInvoiceLines[1];
				npContainer2 = containersForInvoiceLines[0];
			}

			npContainer2.IsForInvoiceLine = true;
			AssertHasMessageError(npContainer2.IsForInvoiceLineInfo, string.Format(CusContainerInvoiceLinePivotValidation.InvoiceLineLinkedToContainerWhichIsNotLinkedToInvoiceHeaderBill, "CRUX1234562"));

			npContainer2.IsForInvoiceLine = false;
			AssertNoMessageError(npContainer2.IsForInvoiceLineInfo, string.Format(CusContainerInvoiceLinePivotValidation.InvoiceLineLinkedToContainerWhichIsNotLinkedToInvoiceHeaderBill, "CRUX1234562"));

			npContainer1.IsForInvoiceLine = true;
			AssertEquals(false, npContainer1.IsForInvoiceLineInfo.HasMessageError(string.Format(CusContainerInvoiceLinePivotValidation.InvoiceLineLinkedToContainerWhichIsNotLinkedToInvoiceHeaderBill, "CRUX2345627")));

			AssertEquals("Precondition: one container in pivot", 1, container1.InvoiceLinePivotCollection.Count);
			npContainer1.IsForInvoiceLine = true;
			AssertEquals("Ensure setting flag again has no affect on pivot", 1, container1.InvoiceLinePivotCollection.Count);
		}

		public void TestValidateSplitValue()
		{
			pivot2ValidationMock.Reset();
			pivot2ValidationMock.Protected().Setup<bool>("IsCusContainerInvoiceLineValidationRequired").Returns(false);

			invoice.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
			invoiceLine.JI_LinePrice = 100m;

			string expectedMessage = "Total of these values do not add up to invoice line price.";

			var containersForInvoiceLines = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			var npContainer1 = containersForInvoiceLines[0];
			var npContainer2 = containersForInvoiceLines[1];
			npContainer1.SplitValue = 20m;
			AssertHasWarning(npContainer1.SplitValueInfo, expectedMessage);

			npContainer2.SplitValue = 20m;
			AssertNoWarning(npContainer2.SplitValueInfo, expectedMessage);

			npContainer2.SplitValue = 80m;
			AssertNoWarning(npContainer2.SplitValueInfo, expectedMessage);

			npContainer1.SplitValue = 20m;
			AssertNoWarning(npContainer1.SplitValueInfo, expectedMessage);
		}

		public void TestValidateNetWeight()
		{
			pivot2ValidationMock.Reset();
			pivot2ValidationMock.Protected().Setup<bool>("IsCusContainerInvoiceLineValidationRequired").Returns(false);
			invoiceLine.JI_NetWeight = 10m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Tonnes;

			string expectedMessage = "Total of these values do not add up to invoice line net weight.";

			var containersForInvoiceLine1 = invoiceLine.ContainersForInvoiceLinesForBindingOnly[0];
			var containersForInvoiceLine2 = invoiceLine.ContainersForInvoiceLinesForBindingOnly[1];
			containersForInvoiceLine1.NetWeightInKG = 2000m;
			AssertHasWarning(containersForInvoiceLine1.NetWeightInKGInfo, expectedMessage);

			containersForInvoiceLine2.NetWeightInKG = 800m;
			AssertNoWarning(containersForInvoiceLine2.NetWeightInKGInfo, expectedMessage);

			containersForInvoiceLine2.NetWeightInKG = 8000m;
			AssertNoWarning(containersForInvoiceLine2.NetWeightInKGInfo, expectedMessage);

			containersForInvoiceLine1.NetWeightInKG = 2000m;
			AssertNoWarning(containersForInvoiceLine1.NetWeightInKGInfo, expectedMessage);
		}

		BaseJobDeclaration declaration;
		BaseCusContainer container1;
		BaseCusContainer container2;

		BaseJobComInvoiceHeader invoice;
		BaseJobComInvoiceLine invoiceLine;

		Mock<CusContainerInvoiceLinePivotValidation> pivot1ValidationMock;
		Mock<CusContainerInvoiceLinePivotValidation> pivot2ValidationMock;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<BaseJobDeclaration>();
			container1 = declaration.CusContainers.AddNew();
			container2 = declaration.CusContainers.AddNew();

			invoice = declaration.Invoices.AddNew();
			invoiceLine = declaration.InvoiceLines.AddNew();

			var pivot1Mock = Factory.NewMoq<CusContainerInvoiceLinePivot>();
			var pivot1 = pivot1Mock.Object;
			pivot1ValidationMock = new Mock<CusContainerInvoiceLinePivotValidation>(pivot1) { CallBase = true };
			pivot1ValidationMock.Protected().Setup<bool>("IsCusContainerInvoiceLineValidationRequired").Returns(true);
			var pivot1Validation = pivot1ValidationMock.Object;
			pivot1Mock.Protected().Setup<CusContainerInvoiceLinePivotValidation>("GetNewValidation").Returns(pivot1Validation);
			pivot1.C2_JI = invoiceLine.PK;
			pivot1.C2_CO = container1.PK;
			invoiceLine.ContainersPivot.Add(pivot1);

			var pivot2Mock = Factory.NewMoq<CusContainerInvoiceLinePivot>();
			var pivot2 = pivot2Mock.Object;
			pivot2ValidationMock = new Mock<CusContainerInvoiceLinePivotValidation>(pivot2) { CallBase = true };
			pivot2ValidationMock.Protected().Setup<bool>("IsCusContainerInvoiceLineValidationRequired").Returns(true);
			var pivot2Validation = pivot2ValidationMock.Object;
			pivot2Mock.Protected().Setup<CusContainerInvoiceLinePivotValidation>("GetNewValidation").Returns(pivot2Validation);
			pivot2.C2_JI = invoiceLine.PK;
			pivot2.C2_CO = container2.PK;
			invoiceLine.ContainersPivot.Add(pivot2);
		}
	}
}
