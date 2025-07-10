using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccDraftInvoiceHeaderProcessTask))]
	public class AccDraftInvoiceHeaderProcessTasksTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var invoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			return invoice.WorkflowItems.AddNew();
		}
	}
}
