using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class CusEntryHeaderValidation : Customs.Business.CusEntryHeaderValidation
	{
		public CusEntryHeaderValidation(CusEntryHeader parent)
			: base(parent)
		{
		}

		public new CusEntryHeader Parent
		{
			get { return (CusEntryHeader)base.Parent; }
		}

		protected JobDeclaration Declaration
		{
			get { return Parent.Declaration; }
		}

		protected override void CheckCH_BGMReference()
		{
			base.CheckCH_BGMReference();

			if (Parent.IsReconImportEntry)
			{
				ValidateReconImportEntry();
			}
			else if (Parent.IsAESTIRMessagingMode)
			{
				ValidateAESExportEntry();
			}
		}

		void ValidateReconImportEntry()
		{
			var parent = Parent;
			if (parent.CH_BGMReference.IsEmpty)
			{
				parent.CH_BGMReferenceInfo.AddMessageError(ValidationConstants.Recon.EntryFilerEntryNumberMandatory);
			}
			else
			{
				EntryNumberValidator.ValidateFormatAndCheckDigit(parent.CH_BGMReferenceInfo, parent.Branch);

				if (parent.CH_BGMReference.Length > 11)
				{
					parent.CH_BGMReferenceInfo.AddError(ValidationConstants.Recon.MaxLengthOfEntryNumberIs11);
				}

				if (!parent.CH_BGMReferenceInfo.HasNotifications())
				{
					var query = CusEntryHeaderFetchStrategy.GetReconOriginalEntryOrExportDuplicateQuery(parent.CH_BGMReference, parent.PK, parent.CH_MessageType);
					query.MaximumRows = 1;
					var rowFactory = ((IBusinessObjectFactoryInternals)parent.Factory).RowFactory;
					var otherReconOriginalEntryRows = rowFactory.Load(CusEntryHeaderSchema.Constants.TableName, query);
					if (otherReconOriginalEntryRows != null && otherReconOriginalEntryRows.Length > 0)
					{
						if (otherReconOriginalEntryRows.Length == 1)
						{
							var previousReconDecPK = ((IColumnIndexer)otherReconOriginalEntryRows[0]).GetValue(CusEntryHeaderSchema.CH_JE);
							var previousReconDecEntry = parent.Factory.Load<JobDeclaration>(previousReconDecPK.ToGuid());

							if (previousReconDecEntry != null)
							{
								if (parent.Declaration.US_IssueCode == ReconIssueCodeList.Codes.FTA && previousReconDecEntry.US_IssueCode == ReconIssueCodeList.Codes.FTA)
								{
									parent.CH_BGMReferenceInfo.AddMessageError(ValidationConstants.Recon.OneEntryShouldHaveMaximum2Recons);
								}
								else if (parent.Declaration.US_IssueCode != ReconIssueCodeList.Codes.FTA && previousReconDecEntry.US_IssueCode != ReconIssueCodeList.Codes.FTA)
								{
									parent.CH_BGMReferenceInfo.AddMessageError(ValidationConstants.Recon.AlreadyReconciled + previousReconDecEntry.JE_DeclarationReference);
								}
							}
						}
						else
						{
							parent.CH_BGMReferenceInfo.AddMessageError(ValidationConstants.Recon.OneEntryShouldHaveMaximum2Recons);
						}
					}
				}

				var reconDeclaration = parent.ReconOriginalEntry?.ReconDeclaration;
				if (reconDeclaration != null && !reconDeclaration.US_R_IsNoChangeAgg)
				{
					var invoice = parent.ReconOriginalEntry?.Invoice;

					if (invoice != null && (invoice.InvoiceLines.Count == 0))
					{
						parent.CH_BGMReferenceInfo.AddMessageError(ValidationConstants.Recon.UnderlyingEntriesWithoutNoChangeAggregate);
					}
				}
			}

			if (parent.ReconOriginalEntry != null)
			{
				ValidateReconOrigianlEntry(parent.ReconOriginalEntry.OriginalDeclaration);
			}
		}

		void ValidateReconOrigianlEntry(ReconOriginalDeclaration originalDeclaration)
		{
			if (originalDeclaration != null)
			{
				if (!EntryTypeList.IsValidForRecon(originalDeclaration.US_EntryType))
				{
					Parent.CH_BGMReferenceInfo.AddMessageError(ValidationConstants.Recon.EntryTypeNotReconcilable);
				}
				if (Parent.Declaration.US_IssueCode != ReconIssueCodeList.Codes.FTA && originalDeclaration.US_OtherReconIndicator != Parent.Declaration.US_IssueCode
					|| Parent.Declaration.US_IssueCode == ReconIssueCodeList.Codes.FTA && !originalDeclaration.US_NAFTAReconIndicator)
				{
					Parent.CH_BGMReferenceInfo.AddWarning(ValidationConstants.Recon.IssueCodeConflict);
				}
				if (Parent.Declaration.ImporterOfRecordNumber != originalDeclaration.ImporterOfRecordNumber)
				{
					Parent.CH_BGMReferenceInfo.AddMessageError(ValidationConstants.Recon.IORConflict);
				}
				if (Parent.Declaration.US_SuretyCode != originalDeclaration.US_SuretyCode)
				{
					Parent.CH_BGMReferenceInfo.AddMessageError(ValidationConstants.Recon.SuretyCodeConflict);
				}

				var entrySummaryStatus = originalDeclaration.EntrySummaryStatus;

				if (entrySummaryStatus == ImportMessageStatusList.Codes.ClearEntrySummaryDelete)
				{
					Parent.CH_BGMReferenceInfo.AddMessageError(ValidationConstants.Recon.ClearEntrySummaryDelete);
				}
				else if (entrySummaryStatus == ImportMessageStatusList.Codes.EntrySummaryCanceled)
				{
					Parent.CH_BGMReferenceInfo.AddMessageError(ValidationConstants.Recon.EntrySummaryCanceled);
				}
			}
			CheckDuplicateFilerCodeAndEntryNumber();
		}

		void ValidateAESExportEntry()
		{
			if (!Parent.CH_BGMReferenceInfo.ReadOnly && !Parent.CH_BGMReference.IsEmpty)
			{
				var shipmentNoQuery = CusEntryHeaderFetchStrategy.GetReconOriginalEntryOrExportDuplicateQuery(Parent.CH_BGMReference, Parent.PK, CusEntryHeaderMessageTypeList.Codes.Export);

				var existingSEDEntry = Parent.Factory.LoadTop1<CusEntryHeader>(shipmentNoQuery);
				if (existingSEDEntry != null)
				{
					string jobNo = existingSEDEntry.Declaration.Shipment != null ? existingSEDEntry.Declaration.Shipment.JobNumber : existingSEDEntry.Declaration.JobNumber;
					Parent.CH_BGMReferenceInfo.AddError(string.Format(OriginalShipmentExists, jobNo));
				}
				else if (Parent.CH_BGMReference != Parent.Declaration.JE_DeclarationReference)
				{
					Parent.CH_BGMReferenceInfo.AddWarning(OriginalShipmentNoWarning);
				}
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		internal const string OriginalShipmentExists = "The Shipment No. entered already exists in CargoWise One. (Refer Job No. {0}).";
		internal const string OriginalShipmentNoWarning = "The Shipment No. entered here must be the, (XP - SC1 Record), ShipmentReferenceNumber of the original entry you wish to replace.";

		protected override void CheckDeactivatedEntry()
		{
			if (Parent.IsAESTIRMessagingMode && Parent.HasBeenLodgedAtCustoms && !Parent.HasBeenWithdrawn && (!Parent.IsWaitingForResponse || (Parent.IsInDatabase && Parent.CH_AddInfo.Contains("IsDeactivated=Y"))))
			{
				Parent.CH_BGMReferenceInfo.AddWarning(ValidationConstants.AES.DeactivatedEntryWithActiveCustomsTransaction);
			}
			else if (!Parent.HasBeenWithdrawn)
			{
				if (Parent.IsWaitingForResponse)
				{
					Parent.CH_BGMReferenceInfo.AddError(ValidationConstants.DeactivatedEntryWithActiveCustomsTransactionAwaitingResponse);
				}
				else if (Parent.HasBeenLodgedAtCustoms)
				{
					Parent.CH_BGMReferenceInfo.AddError(DeactivatedEntryWithActiveCustomsTransaction);
				}
			}
		}

		void CheckDuplicateFilerCodeAndEntryNumber()
		{
			if (!Parent.CH_BGMReference.IsEmpty)
			{
				JobDeclaration declaration = Parent.Declaration;
				if (declaration != null)
				{
					if (declaration.ReconDeclaration.US_IsAggregate && !declaration.ReconDeclaration.US_R_Waive)
					{
						ZDecimal dutyDifference = Parent.ReconOriginalEntry.ReconDuty - Parent.ReconOriginalEntry.OriginalDuty;
						ZDecimal taxDifference = Parent.ReconOriginalEntry.ReconTax - Parent.ReconOriginalEntry.OriginalTax;
						ZDecimal feeDifference = Parent.ReconOriginalEntry.ReconFee - Parent.ReconOriginalEntry.OriginalFee;

						if (dutyDifference < 0 || taxDifference < 0 || feeDifference < 0)
						{
							Parent.CH_BGMReferenceInfo.AddMessageError(ValidationConstants.Recon.NoImportEntriesShouldBeRefunded);
						}
					}

					foreach (ReconOriginalEntryHeader otherEntry in declaration.ReconDeclaration.OriginalEntries)
					{
						if (otherEntry.CH_PK != Parent.PK && otherEntry.CH_OrigEntryReference == Parent.CH_BGMReference)
						{
							Parent.CH_BGMReferenceInfo.AddError(DuplicateFilerCodeAndEntryNumber);
							break;
						}
					}
				}
			}
		}
		internal const string DuplicateFilerCodeAndEntryNumber = "The combination of Filer Code and Entry Number should be unique";
	}
}
