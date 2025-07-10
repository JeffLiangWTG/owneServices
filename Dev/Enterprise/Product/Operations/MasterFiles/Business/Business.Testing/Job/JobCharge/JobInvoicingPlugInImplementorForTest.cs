using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobInvoicingPlugInImplementorForTest : NonPersistentBusinessObject, IJobInvoicingPlugIn
	{
		public JobInvoicingPlugInImplementorForTest(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region IJobInvoicingPlugIn Members

		DummyJobHeaderParentJobInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = new DummyJobHeaderParentJobInvoicingSupporter()); }
		}

		#endregion

		#region IJobHeaderParent Members

		public void SetJobNumberFieldOnSaving()
		{
		}

		public void OnJobCreating(JobHeader job)
		{
		}

		public void OnJobCreated(JobHeader job)
		{
		}

		public void OnJobDeleting(JobHeader job)
		{
		}

		public void OnJobDeleted(JobHeader job)
		{
		}

		public bool AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IJobNumber Members

		public string JobNumber
		{
			get { return "Job1"; }
		}

		#endregion
	}
}
