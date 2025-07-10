using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusContainerInvoiceLinePivot))]
	public class CusContainerInvoiceLinePivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestContainerNumber()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT32";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			var pivot = invoiceLine.ContainersPivot[0];
			AssertEquals("ContainerNumber", "CONT32", pivot.ContainerNumber);
		}

		public void TestSupportsNotes()
		{
			AssertEquals("SupportsNotes should be false", false, Factory.GetNull<CusContainerInvoiceLinePivot>().SupportsNotes);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObject(Factory);
		}

		BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var declaration = factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var container = declaration.CusContainers.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;

			return invoiceLine.ContainersPivot[0];
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject(factory);
		}

		public void TestRelatedContainerProperty()
		{
			BaseCusContainer container = Factory.New<BaseCusContainer>();

			CusContainerInvoiceLinePivot pivot = Factory.New<CusContainerInvoiceLinePivot>();
			pivot.C2_CO = container.PK;
			AssertEquals("Container should be linked to CusContainerInvoiceLinePivot", pivot.C2_CO, pivot.Container.PK);
		}

		public void TestRelatedInvoiceLineProperty()
		{
			BaseJobComInvoiceHeader invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			CusContainerInvoiceLinePivot pivot = Factory.New<CusContainerInvoiceLinePivot>();
			pivot.C2_JI = invoiceLine.PK;
			AssertEquals("JobComInvoiceLine should be linked to CusContainerInvoiceLinePivot", pivot.C2_JI, pivot.InvoiceLine.PK);
		}
	}
}
