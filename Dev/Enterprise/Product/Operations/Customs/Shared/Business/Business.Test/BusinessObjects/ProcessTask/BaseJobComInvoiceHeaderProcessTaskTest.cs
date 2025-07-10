using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseJobComInvoiceHeaderProcessTask))]
	sealed class BaseJobComInvoiceHeaderProcessTaskTest : ProcessTaskTest
	{
		public void TestControllerID()
		{
			var invoice1 = Factory.New<BaseJobComInvoiceHeader>();
			var workflowProvider1 = invoice1 as IWorkflowProvider;
			var processTask1 = workflowProvider1.WorkflowItems.AddNew();
			AssertEquals("ParentControllerID", ControllerIDs.CommercialInvoice, processTask1.ParentControllerID);

			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice2 = declaration.Invoices.AddNew();
			var workflowProvider2 = invoice2 as IWorkflowProvider;
			var processTask2 = workflowProvider2.WorkflowItems.AddNew();
			AssertEquals("ParentControllerID", ControllerIDs.Customs.JobDeclaration, processTask2.ParentControllerID);
		}

		#region Implementation

		ProcessTaskCollection WorkflowItems
		{
			get { return ((IWorkflowProvider)InvoiceHeader).WorkflowItems; }
		}

		BaseJobComInvoiceHeader InvoiceHeader
		{
			get { return invoiceHeader ?? (invoiceHeader = Factory.New<BaseJobComInvoiceHeader>()); }
		}

		BaseJobComInvoiceHeader invoiceHeader;

		protected override BusinessObject GetNewBusinessObject()
		{
			return WorkflowItems.AddNew();
		}

		#endregion
	}
}
