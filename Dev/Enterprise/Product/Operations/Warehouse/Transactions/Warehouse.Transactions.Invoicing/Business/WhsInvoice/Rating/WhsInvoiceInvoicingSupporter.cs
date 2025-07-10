using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Invoicing
{
	#region Invoicing Supporter

	public class WhsInvoiceInvoicingSupporter : Rating.Business.JobStorage.JobStorageInvoicingSupporter
	{
		public WhsInvoiceInvoicingSupporter(WhsInvoice parent)
			: base(parent)
		{
		}

		public override OrgHeader GetDefaultDebtor(AccChargeCode chargeCode, JobHeader job, ZString relatedJobNumber)
		{
			var debtor = Parent.Client != null
				? Parent.Client.GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo, ZString.Empty, TransportMode, ContainerMode)
				: null;

			return debtor ?? job?.LocalCharges;     //Todo: should not default to job.LocalCharges ideally. So that we could abolish the job.LocalCharges and job.Agent eventually
		}

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.WhsInvoicingAuditBilling;
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.WhsInvoicingJobInvoicing;
		}

		public override string GetReasonNotToAllowPosting()
		{
			String result = String.Empty;
			if (Parent.ET_StorageToDate > ZDateTime.Today)
			{
				result = WhsInvoiceValidation.ErrorMessages.FutureDateNotAllowPosting;
			}
			else
			{
				result = null;
			}
			return result;
		}

		public override ZString ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(ZGuid localChargesPK)
		{
			var errorMessage = base.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(localChargesPK);

			// tested in WhsInvoiceTest
			if (errorMessage.IsEmpty && Invoice.InvalidClientThatWasReversed.HasValue)
			{
				errorMessage = Res.GetString("4b3232a8-3bf6-4ee3-8203-ef4dd126fb11", "You cannot Change the Client if there is at least one Posted Charge.");
			}

			return errorMessage;
		}

		public override string GetReasonNotToAllowAutoRate(AutoRateOptions options = default)
		{
			Invoice.Validation.ValidateAll();
			Invoice.RefreshBinding(); // display errors

			var errorMessage = Invoice.HasErrors
				? Res.GetString("WhsInvoice|GetReasonNotToAllowAutoRate", "There are errors that need to be corrected before this Warehouse Periodic Invoice can be Auto Rated.")
				: base.GetReasonNotToAllowAutoRate();

			if (string.IsNullOrEmpty(errorMessage) && !Invoice.IsInvoiceBillingCheckSuspended && !ObjectFactory.Get<IWhsInvoiceHelper>().IsNotLockByOtherProcess(Invoice.PK))
			{
				errorMessage = Res.GetString("WhsInvoice|GetReasonNotToAllowAutoRate|AutoRateInProgress", "Another user or service is Autorating this Periodic Invoice. Please try again later.");
			}

			if (string.IsNullOrEmpty(errorMessage))
			{
				var factory = Invoice.Factory;
				factory.GetCachedValue("WhsInvoice|AddFetchHintsForChildInvoicingJobs|" + Invoice.PK, () =>
				{
					var header = Invoice.JobHeader;
					if (header != null)
					{
						// tested in WhsInvoiceJobStorageTest
						factory.AddFetchHint(StmALogSchema.SL_Parent, Invoice.PK);
						factory.AddFetchHint(StmALogSchema.SL_Parent, header.PK);
					}

					var companyPK = GlbCompany.CurrentCompany.PK;

					foreach (var job in ((IJobInvoicingPlugInAdditionalJobs)Invoice).AdditionalJobsToShowChargesFor)
					{
						var billingJob = new Job.Loader(job).Load();
						if (billingJob != null)
						{
							// tested in WhsInvoiceJobStorageTest
							var query = new ZQuery();
							query.AddToFilter(JobHeaderSchema.JH_JH_ParentJob, billingJob.PK);
							query.AddToFilter(JobHeaderSchema.JH_GC, companyPK);

							factory.AddFetchHint(JobHeaderSchema.Instance, query);
							factory.AddFetchHint(JobChargeSchema.JR_JH, billingJob.PK);
							factory.AddFetchHint(StmALogSchema.SL_Parent, job.PK);
							factory.AddFetchHint(StmALogSchema.SL_Parent, billingJob.PK);
						}
					}

					return true;
				}, CacheStalenessPolicy.StaleOnFactorySave);
			}

			return errorMessage;
		}

		public override void PostedStateChanged()
		{
			base.PostedStateChanged();
			Parent.ET_StorageFromDateInfo.RefreshBinding();
			Parent.ET_StorageToDateInfo.RefreshBinding();
			Parent.ET_WWInfo.RefreshBinding();
			Parent.ET_BillingDateInfo.RefreshBinding();
		}

		public override GlbBranch OperationsBranch
		{
			get { return RegistryHelper.GetBillingOperationsBranch(Invoice.Warehouse, Invoice.Client); }
		}

		WhsInvoice Invoice
		{
			get { return (WhsInvoice)Parent; }
		}
	}

	#endregion
}
