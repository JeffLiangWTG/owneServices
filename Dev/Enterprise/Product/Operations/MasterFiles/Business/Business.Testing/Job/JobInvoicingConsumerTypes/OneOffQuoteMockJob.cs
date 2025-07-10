using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OneOffQuoteMockJob : NonPersistentBusinessObject, IJobInvoicingPlugIn
	{
		public OneOffQuoteMockJob(BusinessObjectFactory factory, JobInvoicingConsumerType consumerType)
			: base(factory)
		{
			this.consumerType = consumerType;
		}

		readonly JobInvoicingConsumerType consumerType;

		#region IJobInvoicingPlugIn Members

		OneOffQuoteMockJobInvoicingSupporter fInvoicingSupporter;

		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = new OneOffQuoteMockJobInvoicingSupporter(consumerType)); }
		}

		#endregion

		#region IJobHeaderParent Members

		public void SetJobNumberFieldOnSaving()
		{
			throw new NotImplementedException();
		}

		public void OnJobCreating(JobHeader job)
		{
			throw new NotImplementedException();
		}

		public void OnJobCreated(JobHeader job)
		{
			throw new NotImplementedException();
		}

		public void OnJobDeleting(JobHeader job) { }

		public void OnJobDeleted(JobHeader job) { }

		public bool AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IJobNumber Members

		public string JobNumber
		{
			get { throw new NotImplementedException(); }
		}

		#endregion
	}
}
