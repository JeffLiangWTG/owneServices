using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Invoicing.Business
{
	#region Invoicing Supporter

	public class PeriodicInvoicingInvoicingSupporter : JobStorage.JobStorageInvoicingSupporter
	{
		public PeriodicInvoicingInvoicingSupporter(PeriodicInvoicing parent)
			: base(parent)
		{
		}

		public override OrgHeader GetDefaultDebtor(AccChargeCode chargeCode, JobHeader job, ZString relatedJobNumber)
		{
			var debtor = Parent.Client?.GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo, ZString.Empty, TransportMode, ContainerMode);

			return debtor ?? job?.LocalCharges;
		}

		public override string GetReasonNotToAllowPosting()
		{
			String result = String.Empty;
			if (Parent.ET_StorageToDate > ZDateTime.Today)
			{
				result = PeriodicInvoicingValidation.ErrorMessages.FutureDateNotAllowPosting;
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

			if (errorMessage.IsEmpty && Invoice.InvalidClientThatWasReversed.HasValue)
			{
				errorMessage = Res.GetString("143886b0-bc6c-4647-80af-3d02b5fc8366", "You cannot Change the Client if there is at least one Posted Charge.");
			}

			return errorMessage;
		}

		public override string GetReasonNotToAllowAutoRate(AutoRateOptions options = default)
		{
			Invoice.Validation.ValidateAll();
			Invoice.RefreshBinding(); // display errors

			var errorMessage = Invoice.HasErrors
				? Res.GetString("PeriodicInvoicing|GetReasonNotToAllowAutoRate", "There are errors that need to be corrected before this Periodic Invoice can be Auto Rated.")
				: base.GetReasonNotToAllowAutoRate();

			if (string.IsNullOrEmpty(errorMessage) && !Invoice.IsInvoiceBillingCheckSuspended && !ObjectFactory.Get<IPeriodicInvoicingHelper>().IsNotLockByOtherProcess(Invoice.PK))
			{
				errorMessage = Res.GetString("PeriodicInvoicing|GetReasonNotToAllowAutoRate|AutoRateInProgress", "Another user or service is Autorating this Periodic Invoice. Please try again later.");
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
			get { return GetBillingOperationsBranch(Invoice.Warehouse, Invoice.Client); }
		}

		GlbBranch GetBillingOperationsBranch(WhsWarehouse warehouse, OrgHeader organisation)
		{
			GlbBranch result = null;

			var orderRulesForDefaulting = AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.Value;

			for (int i = 1; i <= 4 && (result == null || !result.GB_IsActive); i++)
			{
				if (i == orderRulesForDefaulting.DefaultToBlank)
				{
					result = null;
					break;
				}
				else if (i == orderRulesForDefaulting.DefaultToBranchRelatedToPortOrWarehouseBranch)
				{
					result = warehouse?.RelatedCompanyBranch;
				}
				else if (i == orderRulesForDefaulting.DefaultToBranchOfOrganisation)
				{
					result = organisation?.CompanyData.ControllingBranch;
				}
				else if (i == orderRulesForDefaulting.DefaultToLoginUserDefault)
				{
					result = GlbBranch.CurrentBranch;
				}
			}

			return result;
		}

		PeriodicInvoicing Invoice
		{
			get { return (PeriodicInvoicing)Parent; }
		}
	}

	#endregion
}
