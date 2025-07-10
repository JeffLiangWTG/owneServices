using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonWorkSheetInvoicingPlugIn : IJobInvoicingPlugIn
	{
		readonly CommonWorkSheet WorkSheet;

		readonly CommonCartage Cartage;

		public CommonWorkSheetInvoicingPlugIn(CommonWorkSheet workSheet, CommonCartage cartage)
		{
			WorkSheet = workSheet;
			Cartage = cartage;
		}

		string IJobNumber.JobNumber
		{
			get { return ((IJobNumber)Cartage).JobNumber; }
		}

		ZGuid IJobHeaderParentCore.PK
		{
			get { return Cartage.PK; }
		}

		string IJobHeaderParentCore.TableName
		{
			get { return Cartage.TableName; }
		}

		bool IJobHeaderParentCore.IsInDatabase
		{
			get { return Cartage.IsInDatabase; }
		}

		BusinessObjectFactory IJobHeaderParentCore.Factory
		{
			get { return Cartage.Factory; }
		}

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			((IJobHeaderParent)Cartage).SetJobNumberFieldOnSaving();
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
			((IJobHeaderParent)Cartage).OnJobCreating(job);
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
			((IJobHeaderParent)Cartage).OnJobCreated(job);
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
			((IJobHeaderParent)Cartage).OnJobDeleting(job);
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
			((IJobHeaderParent)Cartage).OnJobDeleted(job);
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return ((IJobHeaderParent)Cartage).AllowInvoiceDeletion; }
		}

		bool IJobHeaderParent.IsDeleted
		{
			get { return ((IJobHeaderParent)Cartage).IsDeleted; }
		}

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = new CommonWorkSheetInvoicingSupporter(WorkSheet, Cartage)); }
		}

		IJobInvoicingSupporter invoicingSupporter;
	}
}
