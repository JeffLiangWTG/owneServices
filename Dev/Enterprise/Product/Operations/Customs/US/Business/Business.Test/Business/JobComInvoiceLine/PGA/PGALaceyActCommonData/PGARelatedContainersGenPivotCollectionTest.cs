using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PGARelatedContainersGenPivotCollection))]
	public class PGARelatedContainersGenPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetRelatedPivotAndContains()
		{
			var containerInvoiceLinePivot1 = GetContainerInvoiceLinePivot("MAEUDDDD");
			PGARelatedContainersGenPivots.AddPivotFor(containerInvoiceLinePivot1);

			var containerInvoiceLinePivot2 = GetContainerInvoiceLinePivot("MAEUAAAA");
			PGARelatedContainersGenPivots.AddPivotFor(containerInvoiceLinePivot2);

			AssertEquals(PGARelatedContainersGenPivots[0], PGARelatedContainersGenPivots.GetRelatedPivot(containerInvoiceLinePivot1));
			AssertEquals(true, PGARelatedContainersGenPivots.Contains(containerInvoiceLinePivot1));

			var pga = InvoiceLine.LaceyActLines[0];
			pga.ContainersForPGALine.RemoveAndDeleteAll();

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CRUX1234562";

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CRUX2345672";

			InvoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container1).IsForInvoiceLine = true;
			InvoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container2).IsForInvoiceLine = true;
			var pgaContainersForInvoiceLine = pga.ContainersForInvoiceLine;
			pgaContainersForInvoiceLine[0].IsForPGALine = true;
			pgaContainersForInvoiceLine[1].IsForPGALine = true;

			AssertNoExceptionThrown(delegate
			{ InvoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = false; });
		}

		public void TestAddPivotFor()
		{
			RelatedContainer relatedContainer1 = SetRelatedContainer("MAEUVVVVV");
			PGA pga = InvoiceLine.LaceyActLines[0];
			AssertEquals(0, pga.ContainersForPGALine.Count);
			pga.ContainersForInvoiceLine.Add(relatedContainer1);
			relatedContainer1.IsForPGALine = true;
			AssertEquals(1, pga.ContainersForPGALine.Count);

			CusContainerInvoiceLinePivot containerInvoiceLinePivot2 = GetContainerInvoiceLinePivot("MAEUDDDD");
			PGARelatedContainersGenPivots.AddPivotFor(containerInvoiceLinePivot2);
			AssertEquals(2, PGARelatedContainersGenPivots.Count);
		}

		public void TestDeletePivotFor()
		{
			CusContainerInvoiceLinePivot containerInvoiceLinePivot1 = GetContainerInvoiceLinePivot("MAEUDDDD");
			PGARelatedContainersGenPivots.AddPivotFor(containerInvoiceLinePivot1);

			CusContainerInvoiceLinePivot containerInvoiceLinePivot2 = GetContainerInvoiceLinePivot("MAEUAAAA");
			PGARelatedContainersGenPivots.AddPivotFor(containerInvoiceLinePivot2);

			CusContainerInvoiceLinePivot containerInvoiceLinePivot3 = GetContainerInvoiceLinePivot("MAEUVVVVV");
			PGARelatedContainersGenPivots.AddPivotFor(containerInvoiceLinePivot3);
			AssertEquals(3, PGARelatedContainersGenPivots.Count);

			PGARelatedContainersGenPivots.DeletePivotFor(containerInvoiceLinePivot2);
			AssertEquals(2, PGARelatedContainersGenPivots.Count);
			AssertEquals(false, PGARelatedContainersGenPivots.Contains(containerInvoiceLinePivot2));
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return PGARelatedContainersGenPivots;
		}

		PGARelatedContainersGenPivotCollection PGARelatedContainersGenPivots
		{
			get
			{
				if (pgaRelatedContainersGenPivotCollection == null)
				{
					pgaRelatedContainersGenPivotCollection = new PGARelatedContainersGenPivotCollection(InvoiceLine.LaceyActLines[0]);
					pgaRelatedContainersGenPivotCollection.Load();
				}
				return pgaRelatedContainersGenPivotCollection;
			}
		}
		PGARelatedContainersGenPivotCollection pgaRelatedContainersGenPivotCollection;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableCRL = true;
					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					invoiceLine.LaceyActLines.AddNew();
				}
				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;
		JobDeclaration declaration;

		RelatedContainer SetRelatedContainer(ZString containerNumber)
		{
			var containerInvoiceLinePivot = GetContainerInvoiceLinePivot(containerNumber);

			var relatedContainer = new RelatedContainer(InvoiceLine.LaceyActLines[0]);
			relatedContainer.SetContainerInvoiceLinePivot(containerInvoiceLinePivot);

			return relatedContainer;
		}

		CusContainerInvoiceLinePivot GetContainerInvoiceLinePivot(ZString containerNumber)
		{
			var container = Factory.New<CusContainer>();
			container.CO_ContainerNumber = containerNumber;

			var containerInvoiceLinePivot = Factory.New<CusContainerInvoiceLinePivot>();
			containerInvoiceLinePivot.C2_CO = container.PK;

			return containerInvoiceLinePivot;
		}

		#endregion
	}
}
