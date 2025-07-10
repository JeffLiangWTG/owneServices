using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;

namespace Enterprise.Freight.Agency.Business
{
	partial class SundryCharges : IJobInvoicingPlugIn
	{
		#region IJobInvoicingPlugIn Members

		SundryChargesInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = new SundryChargesInvoicingSupporter(this)); }
		}

		#endregion

		#region IJobHeaderParent Members

		void IJobHeaderParent.OnJobCreating(JobHeader jobHeader)
		{
		}

		void IJobHeaderParent.OnJobCreated(JobHeader jobHeader)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader jobHeader)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader jobHeader)
		{
		}

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			SetJobNumberIfNeeded();
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IJobNumber Members

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		string IJobNumber.JobNumber
		{
			get { return D4_JobNumber; }
		}

		#endregion
	}

	public class SundryChargesInvoicingSupporter : JobInvoicingSupporter
	{
		public SundryChargesInvoicingSupporter(SundryCharges parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected readonly SundryCharges Parent;

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.AgencySundryChargesAuditBilling;
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.AgencySundryChargesJobInvoicing;
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.AgencySundryCharges; }
		}

		public override void PostedStateChanged()
		{
			Parent.D4_OH_BillToPartyInfo.RefreshBinding();
			Parent.D4_FromDateInfo.RefreshBinding();
			Parent.D4_ToDateInfo.RefreshBinding();
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override GlbBranch OperationsBranch
		{
			get { return GlbBranch.CurrentBranch; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override ZString ConsolType
		{
			get { return Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override bool CreateAccountingJobOnSavingOfOperationsJob
		{
			get { return true; }
		}
		public override ZString TransportMode
		{
			get { return Constants.TransportModes.Sea; }
		}
	}
}




