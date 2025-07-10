using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(RelatedContainersCollection))]
	public class RelatedContainersCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RelatedContainersCollection>
	{
		public void TestFindByContainerNumber()
		{
			var relatedContainersCollection = PGA.ContainersForPGALine;
			relatedContainersCollection.AddPivotFor(GetCusContainerInvoiceLinePivot());

			CusContainerInvoiceLinePivot pivot2 = GetCusContainerInvoiceLinePivot();
			pivot2.Container.CO_ContainerNumber = "MAEUAAAAAAA";
			relatedContainersCollection.AddPivotFor(pivot2);

			CusContainerInvoiceLinePivot pivot3 = GetCusContainerInvoiceLinePivot();
			pivot3.Container.CO_ContainerNumber = "MAEUJJJJ";
			relatedContainersCollection.AddPivotFor(pivot3);

			var containersForInvoiceLine = PGA.ContainersForInvoiceLine;
			AssertNotNull(containersForInvoiceLine.FindByContainerNumber("MAEUAAAAAAA"));
			AssertNotNull(containersForInvoiceLine.FindByContainerNumber("MAEUJJJJ"));
		}

		public void TestDeleteContainerFromDeclRebuildCollection()
		{
			AddContainerForInvoiceLine("MAEUJJJJ");
			AddContainerForInvoiceLine("MAEUAAAAAAA");
			AddContainerForInvoiceLine("MAEUVVVVV");
			var pgaContainersForInvoiceLine = PGA.ContainersForInvoiceLine;
			AssertEquals(3, pgaContainersForInvoiceLine.Count);

			InvoiceLine.Declaration.CusContainers[1].Delete();
			pgaContainersForInvoiceLine = PGA.ContainersForInvoiceLine;
			AssertEquals(2, pgaContainersForInvoiceLine.Count);
			AssertEquals(false, pgaContainersForInvoiceLine[0].IsForPGALine);
			AssertEquals(false, pgaContainersForInvoiceLine[1].IsForPGALine);

			InvoiceLine.Declaration.CusContainers[1].Delete();
			pgaContainersForInvoiceLine = PGA.ContainersForInvoiceLine;
			AssertEquals(1, pgaContainersForInvoiceLine.Count);
			AssertEquals(true, pgaContainersForInvoiceLine[0].IsForPGALine);
		}

		protected override RelatedContainersCollection GetCollectionToTest()
		{
			return new RelatedContainersCollection(PGA);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RelatedContainer(PGA);
		}

		void AddContainerForInvoiceLine(ZString containerNumber)
		{
			var container = InvoiceLine.Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = containerNumber;
			InvoiceLine.ContainersPivot.AddPivotFor(container);
		}

		CusContainerInvoiceLinePivot GetCusContainerInvoiceLinePivot()
		{
			var container = InvoiceLine.Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "MAEUVVVVV";

			return (CusContainerInvoiceLinePivot)InvoiceLine.ContainersPivot.AddPivotFor(container);
		}

		PGA PGA
		{
			get
			{
				if (pga == null)
				{
					pga = InvoiceLine.LaceyActLines.AddNew();
				}
				return pga;
			}
		}
		PGA pga;

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
