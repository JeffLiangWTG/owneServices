using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(RelatedContainer))]
	public class RelatedContainerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetContainerInvoiceLinePivotAndGenPivot()
		{
			RelatedContainer relatedContainer = SetRelatedContainer();
			AssertEquals(false, relatedContainer.IsForPGALine);
			AssertEquals("MAEUVVVVV", relatedContainer.ContainerNumber);
			AssertNull(relatedContainer.Pivot);

			relatedContainer.IsForPGALine = true;
			AssertNotNull(relatedContainer.Pivot);
		}

		public void TestNotAddedToFactoryCache()
		{
			var relatedContainer = SetRelatedContainer();
			AssertEquals("Data should not be cached in the factory", 0, Factory.GetBizOsForPK(relatedContainer.PK.ToGuid()).Length);
		}

		public void TestIsForPGALine()
		{
			RelatedContainer relatedContainer = SetRelatedContainer();

			PGA.ContainersForInvoiceLine.Add(relatedContainer);
			relatedContainer.IsForPGALine = true;
			AssertEquals(1, PGA.ContainersForPGALine.Count);

			relatedContainer.IsForPGALine = false;
			AssertEquals(0, PGA.ContainersForPGALine.Count);
		}

		public void TestValidateIsForPGALine()
		{
			CusContainer container = Factory.New<CusContainer>();
			container.CO_ContainerNumber = "MAEUVVVVV";

			CusContainerInvoiceLinePivot containerInvoiceLinePivot = Factory.New<CusContainerInvoiceLinePivot>();
			containerInvoiceLinePivot.C2_CO = container.PK;

			RelatedContainer relatedContainer = new RelatedContainer(PGA);
			relatedContainer.SetContainerInvoiceLinePivot(containerInvoiceLinePivot);
			PGA.ContainersForInvoiceLine.Add(relatedContainer);
			PGARelatedContainersGenPivot pivot1 = PGA.ContainersForPGALine.AddNew();
			pivot1.Relation2Object = containerInvoiceLinePivot;
			PGARelatedContainersGenPivot pivot2 = PGA.ContainersForPGALine.AddNew();
			pivot2.Relation2Object = containerInvoiceLinePivot;
			relatedContainer.IsForPGALine = true;
			AssertHasErrorContaining(relatedContainer.IsForPGALineInfo, "A possible fix for this is to untick and re-tick the related container.");
			AssertEquals(pivot1, relatedContainer.Pivot);

			relatedContainer.IsForPGALine = false;
			AssertNoErrorContaining(relatedContainer.IsForPGALineInfo, "A possible fix for this is to untick and re-tick the related container.");
			AssertEquals(pivot2, relatedContainer.Pivot);
			AssertEquals(true, pivot1.IsDeleted);

			relatedContainer.IsForPGALine = true;
			AssertNoErrorContaining(relatedContainer.IsForPGALineInfo, "A possible fix for this is to untick and re-tick the related container.");
			AssertEquals(pivot2, relatedContainer.Pivot);

			relatedContainer.IsForPGALine = false;
			AssertNoErrorContaining(relatedContainer.IsForPGALineInfo, "A possible fix for this is to untick and re-tick the related container.");
			AssertNull(relatedContainer.Pivot);
			AssertEquals(true, pivot2.IsDeleted);
		}

		RelatedContainer SetRelatedContainer()
		{
			CusContainer container = Factory.New<CusContainer>();
			container.CO_ContainerNumber = "MAEUVVVVV";

			CusContainerInvoiceLinePivot containerInvoiceLinePivot = Factory.New<CusContainerInvoiceLinePivot>();
			containerInvoiceLinePivot.C2_CO = container.PK;

			RelatedContainer relatedContainer = new RelatedContainer(PGA);
			relatedContainer.SetContainerInvoiceLinePivot(containerInvoiceLinePivot);

			return relatedContainer;
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

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RelatedContainer(Factory.New<PGA>());
		}

		public void TestContainerInvoiceLinePivotIsNull()
		{
			RelatedContainer relatedContainer = SetRelatedContainer();
			relatedContainer.SetContainerInvoiceLinePivot(null);
			ZString cn = "initial value";
			AssertNoExceptionThrown("should not throw exception", () => { cn = relatedContainer.ContainerNumber; });
			AssertEquals(ZString.Empty, cn);
		}
	}
}
