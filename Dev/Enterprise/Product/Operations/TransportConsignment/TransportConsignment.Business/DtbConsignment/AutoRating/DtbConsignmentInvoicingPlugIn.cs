using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.TransportConsignment.Business
{
	class DtbConsignmentInvoicingPlugIn : IJobInvoicingPlugIn
	{
		public DtbConsignmentInvoicingPlugIn(DtbConsignment consignment)
		{
			Consignment = consignment;
		}

		protected readonly DtbConsignment Consignment;

		#region IJobInvoicingPlugIn Members

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = new DtbConsignmentInvoicingSupporter(Consignment)); }
		}

		IJobInvoicingSupporter invoicingSupporter;

		#endregion

		#region IJobHeaderParent Members

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			Consignment.PopulateUniqueIDIfNeeded();
		}

		bool IJobHeaderParent.IsDeleted
		{
			get { return Consignment.IsDeleted; }
		}

		#endregion

		#region IJobHeaderParentCore Members

		BusinessObjectFactory IJobHeaderParentCore.Factory
		{
			get { return Consignment.Factory; }
		}

		ZGuid IJobHeaderParentCore.PK
		{
			get { return Consignment.PK; }
		}

		string IJobHeaderParentCore.TableName
		{
			get { return Consignment.TableName; }
		}

		bool IJobHeaderParentCore.IsInDatabase
		{
			get { return Consignment.IsInDatabase; }
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return Consignment.LTC_JobID; }
		}

		#endregion
	}
}
