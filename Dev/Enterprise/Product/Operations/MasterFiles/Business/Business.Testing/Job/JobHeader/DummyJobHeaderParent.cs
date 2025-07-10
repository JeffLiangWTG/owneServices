using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class DummyJobHeaderParent : DummyJobHeaderOnlyParent, IJobInvoicingPlugIn
	{
		public DummyJobHeaderParent(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
			Assertion.AssertEquals("Current company should be same as job company", job.Company.PK, GlbCompany.CurrentCompany.PK);

			if (ProcessLogs != null)
			{
				ProcessLogs.Add("JobCreating");
			}
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
			Assertion.AssertEquals("Current company should be same as job company", job.Company.PK, GlbCompany.CurrentCompany.PK);

			if (ProcessLogs != null)
			{
				ProcessLogs.Add("JobCreated");
			}
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
			if (ProcessLogs != null)
			{
				ProcessLogs.Add("JobDeleting");
			}

			IsJobDeletedInJobDeleting = job.IsDeleted;
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
			if (ProcessLogs != null)
			{
				ProcessLogs.Add("JobDeleted");
			}

			IsJobDeletedInJobDeleted = job.IsDeleted;
		}

		public bool IsJobDeletedInJobDeleting;
		public bool IsJobDeletedInJobDeleted;
		public List<string> ProcessLogs;

		#region IJobInvoicingPlugIn Members

		DummyJobHeaderParentJobInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = new DummyJobHeaderParentJobInvoicingSupporter()); }
		}

		#endregion
	}
}
