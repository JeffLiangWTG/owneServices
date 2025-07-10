using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketInvoicingSupporter : WhsJobInvoicingSupporter<WhsDocket>
	{
		public WhsDocketInvoicingSupporter(WhsDocket parent)
			: base(parent)
		{
		}

		public override OrgHeader Consignor => Parent.Client;

		public override OrgHeader GetDefaultDebtor(AccChargeCode chargeCode, JobHeader job, ZString relatedJobNumber)
		{
			var debtor = Parent.Client?.GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo, ZString.Empty, Parent.WD_TransportMode, Parent.WD_ContainerMode);
			return debtor ?? job?.LocalCharges;     //Todo: it's preferable to get rid of job.LocalCharges
		}

		public override bool CreateAccountingJobOnSavingOfOperationsJob => !Parent.IsInDatabaseIncludingChildren && CreateAccountingJobOnSavingOfOperationsJobCore;

		protected virtual bool CreateAccountingJobOnSavingOfOperationsJobCore => true;

		public override GlbBranch OperationsBranch => RegistryHelper.GetBillingOperationsBranch(Parent.Warehouse, Parent.Client);

		protected override SecurityCheckpoint GetAuditSecurityCore() => null;

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore() => Env.Security.WhsDocketJobInvoicing;

		public override ZString EditSecurityMessage => EditSecurityMessageCore;

		protected virtual ZString EditSecurityMessageCore => ZString.Empty;

		public override bool EditSecurityLock => EditSecurityLockCore;

		protected virtual bool EditSecurityLockCore => false;

		public override JobInvoicingConsumerType ConsumerType => null;

		public override ZString ServiceLevel => Parent.WD_RS_NKServiceLevel;

		public override ZDateTimeOffset FIN => Parent.WD_FinalisedDate;

		public override ZDateTimeOffset REQ => Parent.WD_RequiredDate;

		public override void PostedStateChanged()
		{
			Parent.PostedStateChanged();
		}

		public override string GetReasonNotToAllowAutoRate(AutoRateOptions options = default) => GetReasonNotToAllowAutoRateCore(options);

		protected virtual string GetReasonNotToAllowAutoRateCore(AutoRateOptions options = default)
			=> Parent.WD_DocketStatus == DocketStatus.Codes.Cancelled ? Res.GetString("79c32480-c3db-4d68-9167-5382a18fd05d", "Canceled {0} cannot be Auto Rated.", Parent.HumanReadableName) : base.GetReasonNotToAllowAutoRate();

		#region SetDefaultsForNewChargeCore

		protected override void SetDefaultsForNewChargeCore(JobCharge charge)
		{
			base.SetDefaultsForNewChargeCore(charge);

			var docketReferenceAttrib = charge.FindJobChargeAttrib(JobChargeAttribTypeList.Codes.DocketReference);
			if (docketReferenceAttrib == null)
			{
				docketReferenceAttrib = charge.Factory.New<JobChargeAttrib>();
				using (docketReferenceAttrib.GetValidationSuspender())
				{
					charge.JobChargeAttributes.Add(docketReferenceAttrib);
				}

				docketReferenceAttrib.EC_Name = JobChargeAttribTypeList.Codes.DocketReference;
				docketReferenceAttrib.EC_Value = Parent.WD_ExternalReference;
			}
		}

		#endregion
	}
}
