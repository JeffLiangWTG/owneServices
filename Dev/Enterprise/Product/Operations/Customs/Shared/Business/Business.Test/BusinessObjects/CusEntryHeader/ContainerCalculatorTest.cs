using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class ContainerCalculatorTest : TestCaseWithFactory
	{
		public void TestContainerOnDeclarationFromAnotherCountry()
		{
			GlbCompany.CurrentCompany.SetCountry("US");
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.CusContainers.AddNew();
			CusEntryHeader header = declaration.CustomsEntryHeaders.AddNew();
			var calculator = new ContainerCalculator(header);
			BaseCusContainer[] containersWhenInUS = calculator.GetContainers();
			AssertEquals(1, containersWhenInUS.Length);
			AssertEquals(ObjectFactory.GetType<Integration.Customs.US.ICusContainer>(), containersWhenInUS[0].GetType());
			GlbCompany.CurrentCompany.SetCountry("AU");
			BaseCusContainer[] containersWhenInAU = calculator.GetContainers();
			AssertEquals(1, containersWhenInAU.Length);
			AssertEquals(ObjectFactory.GetType<Integration.Customs.US.ICusContainer>(), containersWhenInUS[0].GetType());
		}

		public void TestContainersReturnsEmptyArrayWhenNoContainers()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(0, entryHeader.Containers.Length);
		}

		public void TestContainersReturnsDeclarationContainerWhenOne()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			BaseCusContainer container = declaration.CusContainers.AddNew();
			AssertEquals(container, entryHeader.Containers[0]);
		}

		public void TestContainersReturnsAllContainersWhenOnlyOneEntry()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);

			BaseCusContainer container1 = declaration.CusContainers.AddNew();
			BaseCusContainer container2 = declaration.CusContainers.AddNew();

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;

			AssertEquals(2, entryHeader.Containers.Length);
			AssertCollectionContains(container1, entryHeader.Containers);
			AssertCollectionContains(container2, entryHeader.Containers);
		}

		public void TestContainersReturnsOnlyRelatedContainersFromMultipleEntries()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);

			BaseCusContainer container1 = declaration.CusContainers.AddNew();
			BaseCusContainer container2 = declaration.CusContainers.AddNew();

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			CusEntryHeader entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine2 = entryHeader2.MergedLines.AddNew();

			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;

			AssertEquals(1, entryHeader.Containers.Length);
			AssertEquals(invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].Container, entryHeader.Containers[0]);
		}

		public void TestContainersReturnsDeclarationContainerForAllEntriesOnSingleDeclarationContainer_Enabled()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			BaseCusContainer container1 = declaration.CusContainers.AddNew();

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			CusEntryHeader entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine2 = entryHeader2.MergedLines.AddNew();

			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;

			CombineAssertions(() =>
			{
				AssertEquals("entryHeader with bound container with enabled UseDeclarationContainersForSingleContainerWhenMultipleEntriesExist", 1, entryHeader.Containers.Length);
				AssertEquals("entryHeader2 without bound container with enabled UseDeclarationContainersForSingleContainerWhenMultipleEntriesExist", 1, entryHeader2.Containers.Length);
			});
		}

		public void TestContainersReturnsDeclarationContainerForAllEntriesOnSingleDeclarationContainer_Disabled()
		{
			BaseJobDeclarationForTest declaration = Factory.New<BaseJobDeclarationForTest>();

			BaseCusContainer container1 = declaration.CusContainers.AddNew();

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			CusEntryHeader entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine2 = entryHeader2.MergedLines.AddNew();

			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;

			declaration.UseDeclarationContainersIfNoneFoundOnEntry_ForTest = false;
			declaration.UseDeclarationContainersForSingleContainerWhenMultipleEntriesExist_ForTest = false;
			CombineAssertions(() =>
			{
				AssertEquals("entryHeader with bound container  with disabled UseDeclarationContainersForSingleContainerWhenMultipleEntriesExist", 1, entryHeader.Containers.Length);
				AssertEquals("entryHeader2 without bound container with disabled UseDeclarationContainersForSingleContainerWhenMultipleEntriesExist", 0, entryHeader2.Containers.Length);
			});
		}

		public void TestMultipleContainersReturnedInCorrectOrder()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);

			for (int i = 119; i >= 100; i--)
			{
				BaseCusContainer container = declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = i.ToString();
			}

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			for (int i = 0; i < 20; i++)
			{
				AssertEquals((i + 100).ToString(), entryHeader.Containers[i].CO_ContainerNumber);
			}
		}

		public void TestGetGetContainersDoNotThrowException()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var container = declaration.CusContainers.AddNew();
			declaration.CusContainers.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var containersPivot = invoiceLine.ContainersPivot.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			AssertNoExceptionThrown(() =>
			{
				var containerCalculator = new ContainerCalculator(entryHeader);
				containerCalculator.GetContainers();
			});
		}

		class BaseJobDeclarationForTest : BaseJobDeclaration
		{
			public BaseJobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool UseDeclarationContainersForSingleContainerWhenMultipleEntriesExist_ForTest = true;

			protected override bool UseDeclarationContainersForSingleContainerWhenMultipleEntriesExistCore => UseDeclarationContainersForSingleContainerWhenMultipleEntriesExist_ForTest;

			public bool UseDeclarationContainersIfNoneFoundOnEntry_ForTest = true;

			protected override bool UseDeclarationContainersIfNoneFoundOnEntryCore => UseDeclarationContainersIfNoneFoundOnEntry_ForTest;
		}
	}
}
