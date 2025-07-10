using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	public class MergeManager : Customs.Business.MergeManager
	{
		public MergeManager(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		/// <summary>
		/// Should not cause HasChanges as Merge is marked as done at this point
		/// </summary>
		protected override void OnMerged()
		{
			base.OnMerged();

			if (Declaration.IsImport)
			{
				foreach (CusEntryHeader entry in Declaration.ActiveEntryHeaders)
				{
					entry.AddInfoValidation.ValidateUS_EntryType();

					if (entry.IsFormalEntry)
					{
						foreach (JobComInvoiceHeader invoice in entry.InvoiceHeaders)
						{
							AddInfoJobComInvoiceHeaderValidation addInfoValidation = invoice.AddInfoValidation;
							addInfoValidation.ValidateUS_ManifestQty();
						}

						foreach (MessageBuilders.ICusEntryLine entryLine in entry.MergedLines)
						{
							foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
							{
								var addInfoValidation = invoiceLine.AddInfoValidation;
								addInfoValidation.ValidateUS_ManifestQty();
								addInfoValidation.ValidateUS_ADDCaseNo();
								addInfoValidation.ValidateUS_ADDDepositValue();
								addInfoValidation.ValidateUS_CVDCaseNo();
								addInfoValidation.ValidateUS_CVDDepositValue();

								if (entryLine.CottonCertificateNumberOrganicExemptionCertificateNumber == CottonFeeCalculator.ExemptCottonFeeCertificate)
								{
									addInfoValidation.ValidateUS_CottonFeeExempt();
								}

								addInfoValidation.ValidateUS_98GoodsValue();
								addInfoValidation.ValidateUS_98ValueInvCurr();
								invoiceLine.Validation.ValidateJI_LinePrice();
								invoiceLine.Validation.ValidateJI_Tariff();

								invoiceLine.RefreshOGAValueValidation();
							}
						}
					}
					else if (entry.IsCargoRelease || entry.IsBorderCargoRelease)
					{
						foreach (JobComInvoiceLine invoiceLine in entry.InvoiceLines)
						{
							invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
						}
					}

					entry.Validation.ValidateEntryNumber();
				}

				foreach (Bill bill in Declaration.Bills)
				{
					bill.Validation.ValidateCU_NoOfPacks();
				}

				if (Declaration.US_EnableENS)//Entry Summary Entries have been created
				{
					Declaration.AddInfoValidation.ValidateUS_TaxDeferIndicator();
					Declaration.AddInfoValidation.ValidateUS_BondAmount();
				}

				if (Declaration.US_EnableCRL)
				{
					Declaration.Validation.ValidateJE_OA_ConsigneeAddress();
				}

				if (Declaration.IsConsolidatedMonthlyFilingOverPipeline)
				{
					Declaration.AddInfoValidation.ValidateUS_PayableMPF();
				}

				if (Declaration.IsACE)
				{
					Declaration.AddInfoValidation.ValidateUS_EntryType();
				}
			}
		}

		protected override Customs.Business.LineMerger GetNewLineMergerCore()
		{
			return new LineMerger(Declaration);
		}

		public const string NoImportMessagingModeEnabled = "Merge attempted.   Either Enable Entry Summary or Enable Cargo Release must be selected to merge.";
		public const string NoEntryFilerCodeForBranch = "An 'Entry Filer Code' is required for messaging; this job does not have an 'Entry Filer Code'. Please select a different Branch with 'Entry Filer Code' or setup an 'Entry Filer Code' for this Branch before merging";
		public const string EntriesAboutToBeDiscarded = "There is an entry with Customs transactions about to be discarded due to changes you have made. Please send a delete message.";
		public const string CargoReleaseEntriesAboutToBeDiscarded = " Or if the entry is manually deleted at Customs, wait until Cargo Release Processing Result message is received and processed. System will update the status accordingly.";

		protected override string GetReasonCannotMerge()
		{
			string result = base.GetReasonCannotMerge();

			if (string.IsNullOrEmpty(result) && Declaration.IsImport)
			{
				if (Declaration.NoImportMessagingModesEnabled)
				{
					result = NoImportMessagingModeEnabled;
				}
				else if (!Declaration.IsImportByExternalBroker && !Declaration.IsFTZAdmission)
				{
					if (Declaration.US_EntryFilerCode.IsEmpty)
					{
						result = NoEntryFilerCodeForBranch;
					}
					else if ((Declaration.US_EnableENS || Declaration.US_EnableCRL) && Declaration.ImportEntryNumber.IsEmpty)
					{
						result = GetReasonCannotMergeForFormalEntryNumber();
					}

					if (string.IsNullOrEmpty(result) && Declaration.IsInBond)
					{
						result = GetReasonCannotMergeForInBondNumber();
					}
				}
			}

			if (string.IsNullOrEmpty(result))
			{
				foreach (CusEntryHeader entry in Declaration.ActiveEntryHeaders.GetEntriesAboutToBeDiscarded())
				{
					if (!entry.IsOKToBeDeactivated)
					{
						result = EntriesAboutToBeDiscarded +
							((entry.IsCargoRelease || entry.IsBorderCargoRelease) ? CargoReleaseEntriesAboutToBeDiscarded : "");
						break;
					}
				}
			}

			return result;
		}

		string GetReasonCannotMergeForFormalEntryNumber()
		{
			return ACEEntryStmNumsSetting.GetAnyReasonForNotAbleToAllocateNumber(Declaration.Branch, Declaration.US_EntryFilerCode);
		}

		string GetReasonCannotMergeForInBondNumber()
		{
			return InBondNumberAvailabilityChecker.Check(Declaration.Branch);
		}

		protected override bool ShouldCheckExistenceOfInvoices
		{
			get { return !Declaration.IsInBondOnly && !Declaration.IsTemporaryDeposit; }
		}

		protected override bool ShouldCheckExistenceOfInvoiceLineForAllInvoices
		{
			get { return !Declaration.IsInBondOnly && !Declaration.IsTemporaryDeposit; }
		}

		protected override bool ShouldBillsBeTypesThatAffectMerge
		{
			get { return Declaration.IsENSFormalImport || Declaration.IsFTZAdmission; }
		}

		protected override List<HasChangesHunterExclusionDetails> GetTypesWhichDoNotEffectMerge()
		{
			List<HasChangesHunterExclusionDetails> result = base.GetTypesWhichDoNotEffectMerge();

			result.Add(new HasChangesHunterExclusionDetails(typeof(AddInfoCusEntryHeader)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(AddInfoCusEntryLine)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(AddInfoBill)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(HouseBillRefNo)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(HouseBillRefNoCollection)));
			return result;
		}
	}
}
