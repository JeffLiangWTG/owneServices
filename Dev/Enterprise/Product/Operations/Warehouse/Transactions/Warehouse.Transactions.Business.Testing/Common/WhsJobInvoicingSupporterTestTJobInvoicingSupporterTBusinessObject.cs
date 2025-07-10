using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsJobInvoicingSupporterTest<TJobInvoicingSupporter, TBusinessObject> : JobInvoicingSupporterTest
			where TJobInvoicingSupporter : WhsJobInvoicingSupporter<TBusinessObject>
			where TBusinessObject : BusinessObject, IJobInvoicingPlugIn
	{
		#region TestDeleteRelatedJobHeaders

		public void TestDeleteRelatedJobHeaders()
		{
			var plugin = (TBusinessObject)GetNewBusinessObject();
			var supporter = GetNewSupporter(plugin);
			var jobHeader1 = Helper.CreateRatingJob(plugin);
			var jobHeader2 = Helper.CreateRatingJob(plugin);
			AssertEquals("Precondition", false, plugin.IsDeleted);
			AssertEquals("Precondition", false, jobHeader1.IsDeleted);
			AssertEquals("Precondition", false, jobHeader2.IsDeleted);

			supporter.DeleteRelatedJobHeaders();
			AssertEquals(false, plugin.IsDeleted);
			AssertEquals(true, jobHeader1.IsDeleted);
			AssertEquals(true, jobHeader2.IsDeleted);
		}

		#endregion

		#region Implementation

		protected abstract TJobInvoicingSupporter GetNewSupporter(TBusinessObject parent);

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion
	}
}
