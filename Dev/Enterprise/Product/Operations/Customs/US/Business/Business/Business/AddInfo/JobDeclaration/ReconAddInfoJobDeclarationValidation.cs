using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class ReconAddInfoJobDeclarationValidation : AddInfoJobDeclarationValidation
	{
		public ReconAddInfoJobDeclarationValidation(AddInfoJobDeclaration addInfo)
			: base(addInfo)
		{
			if (!Declaration.IsRecon)
			{
				throw new ArgumentException("Declaration is not a Recon");
			}
		}

		ReconDeclaration ReconciliationDeclaration
		{
			get { return Declaration.ReconDeclaration; }
		}

		bool HasBeenLodgedInCustoms
		{
			get { return ReconciliationDeclaration.CanSendWithdrawal; }
		}
		public const string R10DataLodged = "A reconciliation entry has already been added as '{0}' for a field, {1}. This field cannot be changed in the replacement message.";

		protected override void CheckUS_ClientBranchDesignation()
		{
			base.CheckUS_ClientBranchDesignation();

			if (!Parent.US_ClientBranchDesignation.IsEmpty)
			{
				var clientRegistryBranchDesignation = (ZString)USCustomsDataRegistry.Instance.ClientBranchDesignation.GetFallBackValueAtAllLevels(Guid.Empty, ReconciliationDeclaration.RegistryBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid());
				if (!clientRegistryBranchDesignation.IsEmpty && Parent.US_ClientBranchDesignation != clientRegistryBranchDesignation)
				{
					Parent.US_ClientBranchDesignationInfo.AddMessageError(string.Format(ValidationConstants.ClientBranchDesignationNotMatchRegistrySetting, clientRegistryBranchDesignation));
				}
			}
		}

		protected override void CheckUS_IssueCode()
		{
			base.CheckUS_IssueCode();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_IssueCodeInfo, "issue code");

			ListValidation.MessageErrorIfInvalidCode(Parent.US_IssueCodeInfo, ReconciliationDeclaration.Lookups.US_IssueCodeList);

			if (Parent.US_IssueCode == ReconIssueCodeList.Codes.FTA)
			{
				bool hasIncrease = ReconciliationDeclaration.TotalDutyDifference > 0 ||
					ReconciliationDeclaration.TotalFeeDifference > 0 ||
					ReconciliationDeclaration.TotalTaxDifference > 0;

				if (hasIncrease)
				{
					Parent.US_IssueCodeInfo.AddMessageError(FTACannotIncreaseDuties);
				}
			}
		}
		public const string FTACannotIncreaseDuties = "Trade Agreement reconciliation can only reduce the duty and tax obligations and never increase them.";

		protected override void CheckUS_ImportEntrySource()
		{
			base.CheckUS_ImportEntrySource();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ImportEntrySourceInfo, "Import Entry Source");

			ListValidation.MessageErrorIfInvalidCode(Parent.US_ImportEntrySourceInfo, ReconciliationDeclaration.Lookups.ImportEntrySourceList);
		}

		protected override void CheckUS_SuretyCode()
		{
			base.CheckUS_SuretyCode();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_SuretyCodeInfo, "surety code");

			if (ReconciliationDeclaration != null && ReconciliationDeclaration.ImporterOfRecord != null)
			{
				OrgHeaderWrapper iorWrapper = OrgHeaderWrapper.New(Declaration.ReconDeclaration.ImporterOfRecord);

				CusBondDetail bondData = iorWrapper.BondDetails.GetBondDetailForSuretyCode(Parent.US_SuretyCode);
				if (bondData != null)
				{
					if (bondData.PW_BondEffectiveDate > ZDateTime.Today)
					{
						Parent.US_SuretyCodeInfo.AddMessageError(BondDataNotEffective);
					}
				}
			}
		}
		public const string BondDataNotEffective = "Importer Of Record Bond Data does not become effective until a future date for this Surety Code.";

		protected override void CheckUS_EstimatedEntryDate()
		{
			base.CheckUS_EstimatedEntryDate();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_EstimatedEntryDateInfo, "estimated reconciliation date");

			if (Parent.US_EstimatedEntryDate.IsValid &&
				ReconciliationDeclaration.CanSendOriginal && //has not been lodged yet
				Parent.US_EstimatedEntryDate.IsInThePastDatePartOnly)
			{
				Parent.US_EstimatedEntryDateInfo.AddMessageError(EstimatedReconDateCannotBePast);
			}

			if (Parent.US_EstimatedEntryDate.IsValid)
			{
				if (ReconInterestRateRetriever.GetRate(Parent.US_EstimatedEntryDate) == 0m)
				{
					Parent.US_EstimatedEntryDateInfo.AddMessageError(EstimatedReconDateMustHaveInterestRate);
				}
			}
		}

		public const string EstimatedReconDateCannotBePast = "Estimated reconciliation date cannot be in the past.";
		public const string EstimatedReconDateMustHaveInterestRate = "No reconciliation interest rate exists in the system for the date. Recon interest rates can be entered in the system registry (Customs > United States of America > Import > ABI > Recon. Interest Rates)";

		protected override void CheckUS_SchDEntry()
		{
			base.CheckUS_SchDEntry();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_SchDEntryInfo, "reconciliation port");
			ListValidation.MessageErrorIfInvalidCode(Parent.US_SchDEntryInfo, ReconciliationDeclaration.Lookups.SchDPortList);

			ValidateUS_TeamNo();
		}

		protected override void CheckUS_TeamNo()
		{
			base.CheckUS_TeamNo();

			if (!ReconciliationDeclaration.IsACE)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_TeamNoInfo, "recon team");

				if (!Parent.US_SchDEntry.IsEmpty && !Parent.US_TeamNo.IsEmpty)
				{
					ZString knownMapping = ReconTeamPortMapper.DefaultReconTeam(Parent.US_SchDEntry);

					if (!knownMapping.IsEmpty && knownMapping != Parent.US_TeamNo)
					{
						Parent.US_TeamNoInfo.AddMessageError(WrongTeamNoForTheSelectedPort + knownMapping);
					}
				}

				if (HasBeenLodgedInCustoms && Parent.US_TeamNo != ReconciliationDeclaration.US_R_TeamNoLodged)
				{
					Parent.US_TeamNoInfo.AddMessageError(string.Format(R10DataLodged, ReconciliationDeclaration.US_R_TeamNoLodged, "Team No"));
				}
			}
		}

		public const string WrongTeamNoForTheSelectedPort = "For the selected reconciliation port, the team known to deal with reconciliation is ";

		protected override void CheckUS_PaymentType()
		{
			base.CheckUS_PaymentType();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PaymentTypeInfo, "payment type");

			ListValidation.MessageErrorIfInvalidCode(Parent.US_PaymentTypeInfo, ReconciliationDeclaration.Lookups.US_PaymentTypeList);
			ValidateUS_PreliminaryStatementPrintDate();
		}

		protected override void CheckUS_PreliminaryStatementPrintDate()
		{
			base.CheckUS_PreliminaryStatementPrintDate();

			new PrelimStatementPrintDateValidator().ValidatePreliminaryStatementPrintDate(Parent.US_PreliminaryStatementPrintDateInfo, ReconciliationDeclaration);
		}

		protected override void CheckUS_Comment()
		{
			base.CheckUS_Comment();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CommentInfo, "comment");

			if (!Parent.US_Comment.IsEmpty && Parent.US_Comment.Length <= 5)
			{
				Parent.US_CommentInfo.AddMessageError(CommentLengthShouldBeMoreThan5);
			}
		}
		public const string CommentLengthShouldBeMoreThan5 = "Comment should be more than five characters.";

		protected override void CheckUS_IsAggregate()
		{
			base.CheckUS_IsAggregate();

			if (Parent.US_IsAggregate && !Parent.US_R_Waive &&
				new ReconDeclarationIReconciliation(ReconciliationDeclaration).IncreaseRefundIndicator != IncreaseRefundIndicatorCodes.IncreaseOrNoChange)
			{
				Parent.US_IsAggregateInfo.AddMessageError(AggregateReconciliationAllowedForIncreaseOrNoChange);
			}

			if (ReconciliationDeclaration?.IsACE ?? ZBool.False)
			{
				if (!ReconciliationDeclaration.IsNoChangeAggregate && !ReconciliationDeclaration.IsChangedForReconForOneLine())
				{
					Parent.US_IsAggregateInfo.AddMessageError(LineHasNoChangeBetweenOriginalAndRecon);
				}
			}
		}
		public const string AggregateReconciliationAllowedForIncreaseOrNoChange = "An aggregate recon is allowed only for import entries with no change or more to pay in terms of duties and fees. However there is at least one import entry which requires a refund. You can tick 'Waive Refund' box to waive any refunds of any duty and fee amounts.";

		public const string LineHasNoChangeBetweenOriginalAndRecon = "No lines have changed. Please confirm. If no lines have changed, the Recon job must be sent as a No Change Aggregate.";

		protected override void CheckUS_R_Waive()
		{
			base.CheckUS_R_Waive();
			ValidateUS_IsAggregate();
		}

		protected override void CheckUS_DocProvidedDate()
		{
			base.CheckUS_DocProvidedDate();
			if (ReconciliationDeclaration.IsACE && ReconciliationDeclaration.SummaryDocProvidedStatement)
			{
				MandatoryValidation.MessageErrorIfNotEntered(ReconciliationDeclaration.US_DocProvidedDateInfo);
			}
		}

		protected override void CheckUS_ClaimDate()
		{
			base.CheckUS_ClaimDate();
			if (ReconciliationDeclaration.IsACE && ReconciliationDeclaration.NAFTA303ClaimStatement)
			{
				MandatoryValidation.MessageErrorIfNotEntered(ReconciliationDeclaration.US_ClaimDateInfo);
			}
		}

		protected override void CheckUS_ClaimID()
		{
			base.CheckUS_ClaimID();
			if (ReconciliationDeclaration.IsACE && ReconciliationDeclaration.NAFTA303ClaimStatement)
			{
				MandatoryValidation.MessageErrorIfNotEntered(ReconciliationDeclaration.US_ClaimIDInfo);
			}
		}
	}
}
