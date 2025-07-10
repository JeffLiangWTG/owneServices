using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Agency.Business
{
	[SupportExRateSource(ExRateSourceType.Voyage)]
	partial class VoyageAccount : IJobInvoicingPlugIn, IJobInvoicingExRateSourceProvider
	{
		#region IJobInvoicingPlugIn Members

		VoyageAccountInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = new VoyageAccountInvoicingSupporter(this)); }
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
			SetJobNumberIfNotSet();
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
			get { return NA_JobNumber; }
		}

		#endregion

		#region IJobInvoicingExRateSourceProvider Members

		public IExchangeRateSource GetExRateSource(ExRateSourceType sourceType)
		{
			switch (sourceType)
			{
				case ExRateSourceType.Voyage:
					return Voyage;

				default:
					return null;
			}
		}

		#endregion
	}

	public class VoyageAccountInvoicingSupporter : JobInvoicingSupporter
	{
		public VoyageAccountInvoicingSupporter(VoyageAccount parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected readonly VoyageAccount Parent;

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.AgencyVoyageAccountAuditBilling;
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override ZString ConsolType
		{
			get { return Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.AgencyVoyageAccounting; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override bool CreateAccountingJobOnSavingOfOperationsJob
		{
			get { return !Parent.IsInDatabaseIncludingChildren; }
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.AgencyVoyageAccountJobInvoicing;
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override GlbBranch OperationsBranch
		{
			get { return GlbBranch.CurrentBranch; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override ZString TransportMode
		{
			get { return Constants.TransportModes.Sea; }
		}

		public override void PostedStateChanged()
		{
			Parent.NA_JVInfo.RefreshBinding();
			Parent.NA_OHInfo.RefreshBinding();
		}

		public override ZString DefaultChargeGroup
		{
			get { return ChargeCodeGroupList.Codes.ShippingDisbursements; }
		}

		public override ZString VoyageVesselOrFlightDate
		{
			get
			{
				ZString voyage = (ZString)((BusinessObject)Parent)["NA_Calc_Voyage"]; // This is calculated field can't use schema
				ZString vessel = (ZString)((BusinessObject)Parent)["NA_Calc_Vessel"]; // This is calculated field can't use schema
				return GetVoyageVesselOrFlightDatesCore(Enterprise.Core.Constants.TransportModes.Sea, ZDateTime.Empty, vessel, voyage);
			}
		}
	}
}




