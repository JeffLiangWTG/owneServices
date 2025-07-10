using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FDARelatedContainersGenPivotCollection))]
	public class FDARelatedContainersGenPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetRelatedPivotAndContains()
		{
			var containersForFDALine = FDA.ContainersForFDALine;
			var containerInvoiceLinePivot1 = GetContainerInvoiceLinePivot("MAEUDDDD");
			containersForFDALine.AddPivotFor(containerInvoiceLinePivot1);

			var containerInvoiceLinePivot2 = GetContainerInvoiceLinePivot("MAEUAAAA");
			containersForFDALine.AddPivotFor(containerInvoiceLinePivot2);

			AssertEquals(containersForFDALine[0], containersForFDALine.GetRelatedPivot(containerInvoiceLinePivot1));
			AssertEquals(true, containersForFDALine.Contains(containerInvoiceLinePivot1));
		}

		public void TestAddPivotFor()
		{
			var relatedContainer1 = SetRelatedContainer("MAEUVVVVV");
			AssertEquals(0, FDA.ContainersForFDALine.Count);
			relatedContainer1.IsForFDALine = true;
			AssertEquals(1, FDA.ContainersForFDALine.Count);

			var containerInvoiceLinePivot2 = GetContainerInvoiceLinePivot("MAEUDDDD");
			var containersForFDALine = FDA.ContainersForFDALine;
			containersForFDALine.AddPivotFor(containerInvoiceLinePivot2);
			AssertEquals(2, containersForFDALine.Count);
		}

		public void TestDeletePivotFor()
		{
			var containersForFDALine = FDA.ContainersForFDALine;
			var containerInvoiceLinePivot1 = GetContainerInvoiceLinePivot("MAEUDDDD");
			containersForFDALine.AddPivotFor(containerInvoiceLinePivot1);

			var containerInvoiceLinePivot2 = GetContainerInvoiceLinePivot("MAEUAAAA");
			containersForFDALine.AddPivotFor(containerInvoiceLinePivot2);

			var containerInvoiceLinePivot3 = GetContainerInvoiceLinePivot("MAEUVVVVV");
			containersForFDALine.AddPivotFor(containerInvoiceLinePivot3);
			AssertEquals(3, containersForFDALine.Count);

			containersForFDALine.DeletePivotFor(containerInvoiceLinePivot2);
			AssertEquals(2, containersForFDALine.Count);
			AssertEquals(false, containersForFDALine.Contains(containerInvoiceLinePivot2));
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return FDA.ContainersForFDALine;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<FDARelatedContainersGenPivot>();
		}

		FDA FDA
		{
			get { return fda ?? (fda = InvoiceLine.FDAs.AddNew()); }
		}
		FDA fda;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableCRL = true;
					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				}
				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;

		FDARelatedContainer SetRelatedContainer(ZString containerNumber)
		{
			var containerInvoiceLinePivot = GetContainerInvoiceLinePivot(containerNumber);

			var relatedContainer = new FDARelatedContainer(FDA);
			relatedContainer.SetContainerInvoiceLinePivot(containerInvoiceLinePivot);

			return relatedContainer;
		}

		CusContainerInvoiceLinePivot GetContainerInvoiceLinePivot(ZString containerNumber)
		{
			var container = InvoiceLine.Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = containerNumber;

			var containerInvoiceLinePivot = Factory.New<CusContainerInvoiceLinePivot>();
			containerInvoiceLinePivot.C2_CO = container.PK;

			return containerInvoiceLinePivot;
		}

		#endregion
	}
}
