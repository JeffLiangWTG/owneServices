using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FDARelatedContainersCollection))]
	public class FDARelatedContainersCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FDARelatedContainersCollection>
	{
		public void TestSelectAllContainersForFDALine()
		{
			AddContainerForInvoiceLine("MAEUJJJJ");
			AddContainerForInvoiceLine("MAEUAAAAAAA");
			AddContainerForInvoiceLine("MAEUVVVVV");
			var fdaContainersForInvoiceLine = FDA.ContainersForInvoiceLine;
			AssertEquals(3, fdaContainersForInvoiceLine.Count);
			AssertEquals(0, FDA.ContainersForFDALine.Count);

			fdaContainersForInvoiceLine[1].IsForFDALine = true;
			AssertEquals(3, FDA.ContainersForInvoiceLine.Count);
			AssertEquals(1, FDA.ContainersForFDALine.Count);

			fdaContainersForInvoiceLine[2].IsForFDALine = true;
			AssertEquals(3, FDA.ContainersForInvoiceLine.Count);
			AssertEquals(2, FDA.ContainersForFDALine.Count);
		}

		public void TestFDARelatedContainerAddNew()
		{
			FDARelatedContainersCollection.AddNew(GetCusContainerInvoiceLinePivot());
			AssertEquals("MAEUVVVVV", collection[0].ContainerNumber);
		}

		public void TestFindByContainerNumber()
		{
			FDARelatedContainersCollection.AddNew(GetCusContainerInvoiceLinePivot());

			CusContainerInvoiceLinePivot pivot2 = GetCusContainerInvoiceLinePivot();
			pivot2.Container.CO_ContainerNumber = "MAEUAAAAAAA";

			FDARelatedContainersCollection.AddNew(pivot2);

			CusContainerInvoiceLinePivot pivot3 = GetCusContainerInvoiceLinePivot();
			pivot3.Container.CO_ContainerNumber = "MAEUJJJJ";

			FDARelatedContainersCollection.AddNew(pivot3);

			AssertEquals(FDARelatedContainersCollection[1], FDARelatedContainersCollection.FindByContainerNumber("MAEUAAAAAAA"));
			AssertEquals(FDARelatedContainersCollection[2], FDARelatedContainersCollection.FindByContainerNumber("MAEUJJJJ"));
		}

		public void TestDeleteContainerFromDeclRebuildCollection()
		{
			AddContainerForInvoiceLine("MAEUJJJJ");
			AddContainerForInvoiceLine("MAEUAAAAAAA");
			AddContainerForInvoiceLine("MAEUVVVVV");

			AssertEquals(3, FDA.ContainersForInvoiceLine.Count);

			InvoiceLine.Declaration.CusContainers[1].Delete();
			AssertEquals(2, FDA.ContainersForInvoiceLine.Count);
		}

		protected override FDARelatedContainersCollection GetCollectionToTest()
		{
			return new FDARelatedContainersCollection(FDA);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new FDARelatedContainer(FDA);
		}

		void AddContainerForInvoiceLine(ZString containerNumber)
		{
			var container = InvoiceLine.Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = containerNumber;
			InvoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(containerNumber).IsForInvoiceLine = true;
		}

		CusContainerInvoiceLinePivot GetCusContainerInvoiceLinePivot()
		{
			var container = InvoiceLine.Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "MAEUVVVVV";

			var containerInvoiceLinePivot = Factory.New<CusContainerInvoiceLinePivot>();
			containerInvoiceLinePivot.C2_CO = container.PK;

			return containerInvoiceLinePivot;
		}

		FDARelatedContainersCollection FDARelatedContainersCollection
		{
			get { return collection ?? (collection = new FDARelatedContainersCollection(FDA)); }
		}
		FDARelatedContainersCollection collection;

		FDA FDA
		{
			get
			{
				if (fda == null)
				{
					fda = InvoiceLine.FDAs.AddNew();
				}
				return fda;
			}
		}
		FDA fda;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					JobDeclaration declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableCRL = true;
					JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				}
				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;
	}
}
