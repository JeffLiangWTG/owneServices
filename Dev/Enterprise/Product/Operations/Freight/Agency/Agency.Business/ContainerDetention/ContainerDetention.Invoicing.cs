using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;

namespace Enterprise.Freight.Agency.Business
{
	partial class ContainerDetention : IJobInvoicingPlugIn
	{
		#region IJobInvoicingPlugIn Members

		ContainerDetentionInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = new ContainerDetentionInvoicingSupporter(this)); }
		}

		#endregion

		#region IJobHeaderParent Members

		[System.Diagnostics.DebuggerStepThrough]
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

		[System.Diagnostics.DebuggerStepThrough]
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
			[System.Diagnostics.DebuggerStepThrough]
			get { return NC_JobNumber; }
		}

		#endregion
	}

	public class ContainerDetentionInvoicingSupporter : JobInvoicingSupporter
	{
		public ContainerDetentionInvoicingSupporter(ContainerDetention parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected readonly ContainerDetention Parent;

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.AgencyContainerDetentionAuditBilling;
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override OrgHeader Consignee
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return Parent.Client; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override OrgHeader Consignor
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return Parent.Client; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override ZString ConsolType
		{
			get { return Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override JobInvoicingConsumerType ConsumerType
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return JobInvoicingConsumerTypes.AgencyDetentionInvoice; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override ZString ContainerMode
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return Constants.ContainerModes.FCL; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override bool CreateAccountingJobOnSavingOfOperationsJob
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return true; }
		}

		public override bool IsExport
		{
			get { return Parent.NC_DetentionType == DetentionInvoiceType.Codes.Export; }
		}

		public override bool IsImport
		{
			get { return Parent.NC_DetentionType == DetentionInvoiceType.Codes.Import; }
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.AgencyContainerDetentionJobInvoicing;
		}

		public override ZGuid OverriddenDepartmentPK
		{
			get
			{
				switch (Parent.NC_DetentionType)
				{
					case DetentionInvoiceType.Codes.Import:
						return LinerAgencyDataRegistry.Instance.ImportDetentionDefaultDepartment.Value;

					case DetentionInvoiceType.Codes.Export:
						return LinerAgencyDataRegistry.Instance.ExportDetentionDefaultDepartment.Value;

					default:
						return ZGuid.Empty;
				}
			}
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override ZString TransportMode
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return Constants.TransportModes.Sea; }
		}

		public override OrgHeader GetDefaultCreditor(DefaultCreditorSetting defaultCreditorSetting)
		{
			if (AgencyRegistry.Instance.DefaultCreditorFromPrincipal.Value &&
				defaultCreditorSetting.ChargeCode != null &&
				defaultCreditorSetting.ChargeCode.AC_ChargeOtherGroups == ChargeOtherGroupsList.Codes.Principal)
			{
				return Parent.Principal;
			}
			else
			{
				return null;
			}
		}
	}
}



