using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Transit.Business
{
	public class TransitJobInvoicingPlugIn<T> : IJobInvoicingPlugIn
		where T : BusinessObject,
		IJobHeaderParent,
		IJobNumber,
		ITransitJobInvoicingPlugIn
	{
		public TransitJobInvoicingPlugIn(T businessObject)
		{
			this.businessObject = businessObject;
		}
		readonly T businessObject;

		#region IJobInvoicingPlugIn Members

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter => invoicingSupporter ?? (invoicingSupporter = businessObject.InvoicingSupporter);
		IJobInvoicingSupporter invoicingSupporter;

		#endregion

		#region IJobHeaderParent Members

		bool IJobHeaderParent.AllowInvoiceDeletion => true;

		bool IJobHeaderParent.IsDeleted => businessObject.IsDeleted;

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
		}

		#endregion

		#region IJobHeaderParentCore Members

		ZGuid IJobHeaderParentCore.PK => businessObject.PK;

		string IJobHeaderParentCore.TableName => businessObject.TableName;

		bool IJobHeaderParentCore.IsInDatabase => businessObject.IsInDatabase;

		BusinessObjectFactory IJobHeaderParentCore.Factory => businessObject.Factory;

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber => businessObject.JobNumber;

		#endregion
	}
}
