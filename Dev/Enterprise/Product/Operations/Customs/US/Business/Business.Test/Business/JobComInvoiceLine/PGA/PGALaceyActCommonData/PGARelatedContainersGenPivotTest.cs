using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PGARelatedContainersGenPivot))]
	public class PGARelatedContainersGenPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestContainerEquipmentID()
		{
			AddContainerForInvoiceLine("MAEUJJJJ");
			var pgaContainersForInvoiceLine = PGA.ContainersForInvoiceLine;
			AssertEquals(1, pgaContainersForInvoiceLine.Count);
			pgaContainersForInvoiceLine[0].IsForPGALine = true;

			AssertEquals(1, PGA.ContainersForPGALine.Count);
			AssertEquals("MAEUJJJJ", ((IContainerNumber)PGA.ContainersForPGALine[0]).ContainerEquipmentID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return PGARelatedContainersGenPivot;
		}

		PGARelatedContainersGenPivot PGARelatedContainersGenPivot
		{
			get { return pgaRelatedContainersGenPivot ?? (pgaRelatedContainersGenPivot = Factory.New<PGARelatedContainersGenPivot>()); }
		}
		PGARelatedContainersGenPivot pgaRelatedContainersGenPivot;

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

		PGA PGA
		{
			get { return pga ?? (pga = InvoiceLine.LaceyActLines.AddNew()); }
		}
		PGA pga;

		void AddContainerForInvoiceLine(ZString containerNumber)
		{
			CusContainer container = InvoiceLine.Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = containerNumber;
			InvoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(containerNumber).IsForInvoiceLine = true;
		}
	}
}
