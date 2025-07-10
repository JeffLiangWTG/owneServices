using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusContainersInvoiceLinesCollection))]
	sealed class CusContainerInvoiceLinesCollectionBOTest : BusinessObjectCollectionTestCase
	{
		public void TestAddAndDeleteCusContainerInvoiceLinePivot()
		{
			var cusContainer = Factory.New<BaseCusContainer>();
			cusContainer.CO_ContainerNumber = "UUUU1234567";
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			var invoiceLinesCollection = new CusContainersInvoiceLinesCollection(invoiceLine);
			invoiceLinesCollection.AddPivotFor(cusContainer);
			AssertEquals("Should have returned true if container exists in pivot.", true, invoiceLinesCollection.Contains(cusContainer));
			invoiceLinesCollection.DeletePivotFor(cusContainer);
			AssertEquals("Container should have been deleted at this point.", false, invoiceLinesCollection.Contains(cusContainer));
		}

		public void TestUniqueContainer()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var container1 = declaration.CusContainers.AddNew();
			var container2 = declaration.CusContainers.AddNew();

			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var containersForInvoiceLines = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			AssertEquals("PreCondition:Two containers avaiable", 2, containersForInvoiceLines.Count);

			containersForInvoiceLines[0].IsForInvoiceLine = true;
			AssertEquals("UniqueContainer", container1, invoiceLine.ContainersPivot.UniqueContainer);

			containersForInvoiceLines[1].IsForInvoiceLine = true;
			AssertEquals("UniqueContainer", null, invoiceLine.ContainersPivot.UniqueContainer);
		}

		public void TestTotalSplitValue()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var container1 = declaration.CusContainers.AddNew();
			var container2 = declaration.CusContainers.AddNew();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var containersForInvoiceLines = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			containersForInvoiceLines[0].IsForInvoiceLine = true;
			containersForInvoiceLines[0].SplitValue = 100m;

			AssertEquals("TotalSplitValue", 100m, invoiceLine.ContainersPivot.TotalSplitValue);

			containersForInvoiceLines[1].IsForInvoiceLine = true;
			containersForInvoiceLines[1].SplitValue = 200m;
			AssertEquals("TotalSplitValue", 300m, invoiceLine.ContainersPivot.TotalSplitValue);
		}

		public void TestTotal()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var container1 = declaration.CusContainers.AddNew();
			var container2 = declaration.CusContainers.AddNew();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var containersForInvoiceLines = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			containersForInvoiceLines[0].IsForInvoiceLine = true;
			containersForInvoiceLines[1].IsForInvoiceLine = true;

			containersForInvoiceLines[0].NetWeightInKG = 10m;
			containersForInvoiceLines[0].SplitValue = 20m;
			containersForInvoiceLines[0].PackQty = 30;

			containersForInvoiceLines[1].NetWeightInKG = 11m;
			containersForInvoiceLines[1].SplitValue = 21m;
			containersForInvoiceLines[1].PackQty = 31;

			AssertEquals("TotalNetWeightInKG", 21m, invoiceLine.ContainersPivot.TotalNetWeightInKG);
			AssertEquals("TotalSplitValue", 41m, invoiceLine.ContainersPivot.TotalSplitValue);
			AssertEquals("TotalPackQty", 61, invoiceLine.ContainersPivot.TotalPackQty);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			return new CusContainersInvoiceLinesCollection(invoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var pivot = Factory.New<CusContainerInvoiceLinePivot>();
			return pivot;
		}
	}
}
