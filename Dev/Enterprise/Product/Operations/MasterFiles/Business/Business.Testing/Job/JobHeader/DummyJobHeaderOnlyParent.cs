using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class DummyJobHeaderOnlyParent : DummyBusinessObject, IJobHeaderParent
	{
		public DummyJobHeaderOnlyParent(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

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
			get;
			set;
		}

		#endregion

		#region IJobNumber Members

		public string JobNumber
		{
			get { return "JobNumber"; }
		}

		#endregion
	}
}
