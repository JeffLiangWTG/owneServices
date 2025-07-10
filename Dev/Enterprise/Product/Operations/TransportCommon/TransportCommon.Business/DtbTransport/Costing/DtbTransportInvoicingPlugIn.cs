using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportInvoicingPlugIn : IJobInvoicingPlugIn
	{
		protected DtbTransportInvoicingPlugIn(DtbTransport transport)
		{
			Transport = transport;
		}

		protected readonly DtbTransport Transport;

		#region IJobInvoicingPlugIn Members

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = GetNewInvoicingSupporter()); }
		}

		IJobInvoicingSupporter invoicingSupporter;

		protected abstract DtbTransportInvoicingSupporter GetNewInvoicingSupporter();

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
			Transport.PopulateUnqiueIDIfNeeded();
		}

		bool IJobHeaderParent.IsDeleted
		{
			get { return Transport.IsDeleted; }
		}

		#endregion

		#region IJobHeaderParentCore Members

		BusinessObjectFactory IJobHeaderParentCore.Factory
		{
			get { return Transport.Factory; }
		}

		ZGuid IJobHeaderParentCore.PK
		{
			get { return Transport.PK; }
		}

		string IJobHeaderParentCore.TableName
		{
			get { return Transport.TableName; }
		}

		bool IJobHeaderParentCore.IsInDatabase
		{
			get { return Transport.IsInDatabase; }
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return Transport.KM_JobID; }
		}

		#endregion
	}
}
