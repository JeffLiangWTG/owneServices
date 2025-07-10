using System.Collections.Specialized;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class MergeManager : Customs.Business.MergeManager
	{
		public MergeManager(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override bool ExecuteCore(ISendsMessagesToCustoms notifier)
		{
			var result = false;
			if (!Declaration.IsDeclarationIntegrated)
			{
				result = base.ExecuteCore(notifier);
			}

			return result;
		}

		protected override ZString HumanReadableNameForMergeCore => "frame";

		protected override bool PersistsMergeState => false;

		protected override bool SupportsAmendments => true;

		protected override bool SupportsAutoMergeCore =>
			!Declaration.IsDeclarationIntegrated && base.SupportsAutoMergeCore;

		protected override bool RequiresMergeCore => !Declaration.IsDeclarationIntegrated && base.RequiresMergeCore;

		protected override Customs.Business.LineMerger GetNewLineMergerCore() => new LineMerger(Declaration);

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override void OnMerged()
		{
			base.OnMerged();
			foreach (JobComInvoiceLine item in Declaration.InvoiceLines)
			{
				item.Validation.ValidateJI_TargetEntryLineNumber();
			}
			foreach (var instruction in Declaration.CustomsEntryInstructions)
			{
				instruction.TotalBondSuretyAmountInfo.RefreshBinding();
				instruction.Validation.ValidateCEI_OH_Carrier();
				instruction.Validation.ValidateCEI_OH_BondHolder();
				instruction.Validation.ValidateOH_SubContractor();
			}
		}

		protected override string GetReasonCannotMerge()
		{
			var result = base.GetReasonCannotMerge();
			if (string.IsNullOrEmpty(result) && !Declaration.IsMergeDone &&
				Declaration.Invoices.Any(x => x.JZ_InvoiceCurrExRate.IsEmpty))
			{
				result = ReasonCannotMergeInvoiceHadNoExchangeRate;
			}

			if (string.IsNullOrEmpty(result) && Declaration.IsImportByExternalBroker)
			{
				var lines = Declaration.InvoiceLines.Cast<JobComInvoiceLine>().ToList();
				if (lines.Any(x => x.JI_PreviousEntryLineNumber.IsEmpty))
				{
					result = ReasonCannotMergeInvoiceLineHasNoWHSMRNLineNumber;
				}
				else
				{
					JobComInvoiceLine duplicateInvoiceLine = null;
					foreach (var line in lines.ToArray())
					{
						lines.Remove(line);
						var previousEntryLineNumber = line.JI_PreviousEntryLineNumber;
						if (lines.Any(x =>
							x.JI_CEI == line.JI_CEI && x.JI_PreviousEntryLineNumber == previousEntryLineNumber))
						{
							duplicateInvoiceLine = line;
							break;
						}
					}

					if (duplicateInvoiceLine != null)
					{
						result = ReasonCannotMergeInvoiceLineHasDuplicateWHSMRNLineNumberPerEnteryInstruction(
							duplicateInvoiceLine.JI_CEI_Description, duplicateInvoiceLine.JI_PreviousEntryLineNumber);
					}
				}

				if (string.IsNullOrEmpty(result))
				{
					var instructions = Declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().ToList();
					if (instructions != null)
					{
						if (instructions.Any(x => x.CEI_PreviousMRN.IsEmpty))
						{
							result = ReasonCannotMergeEntryInstructionHasNoWHSMRNLineNumber;
						}
						else
						{
							CusEntryInstruction duplicateEntryInstruction = null;
							foreach (var line in instructions.ToArray())
							{
								instructions.Remove(line);
								var whsmrn = line.CEI_PreviousMRN;
								if (instructions.Any(x => x.CEI_PreviousMRN == whsmrn))
								{
									duplicateEntryInstruction = line;
									break;
								}
							}

							if (duplicateEntryInstruction != null)
							{
								result = ReasonCannotMergeEntryInstructionHasDuplicateWHSMRNLineNumbers;
							}
						}
					}
				}
			}

			return result;
		}

		public override ZString CheckAndGetPrerequisiteConditions(ISendsMessagesToCustoms notifier)
		{
			var result = base.CheckAndGetPrerequisiteConditions(notifier);

			var instructions = Declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().ToList();

			if (instructions != null)
			{
				var allowAutomaticSplitEntriesByBondAmount = ZACustomsRegistry.Instance.AllowAutomaticSplitEntriesByBondAmount.Value;

				if (allowAutomaticSplitEntriesByBondAmount)
				{
					var warnings = new StringCollection();

					foreach (var instruction in instructions)
					{
						if (allowAutomaticSplitEntriesByBondAmount)
						{
							if (instruction.IsBondHolderRequired && instruction.BondGuaranteeValue <= 0.0)
							{
								warnings.Add(ZString.Format(GuaranteeValueNotConfiguredWarning, instruction.CEI_Description));
								instruction.BHValid = ZBool.False;
							}
							else
							{
								instruction.BHValid = ZBool.True;
							}
						}
						else
						{
							instruction.BHValid = ZBool.False;
						}
					}

					if (warnings.Count > 0)
					{
						if (!notifier.ContinueWithSend(warnings))
						{
							if (result.IsEmpty)
							{
								result = warnings[0];
							}
						}
						else
						{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
							Declaration.Logs.AddNew(Events.EditedARecord, "User chose to continue with merge despite one or more entry instructions not having either a bond holder or a bond guarantee value");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						}
					}
				}
			}

			return result;
		}

		public static string GuaranteeValueNotConfiguredWarning
			=> ResString.GetMultilingualString("52DF4A48-4538-4937-BC16-00B87B9D286C",
				"Entry Instruction - {0}. Bond Holder not found or does not have a Bond Guarantee Value configured. Entries will not be split.");

		public static string ReasonCannotMergeInvoiceHadNoExchangeRate
			=> ResString.GetMultilingualString("c82d8e1d-d5a0-4d8b-ad34-36d53c358016",
				"You can't merge this entry because no exchange rate has been found for some invoices.");

		public static string ReasonCannotMergeInvoiceLineHasNoWHSMRNLineNumber
			=> ResString.GetMultilingualString("B8F3856E-E9D1-4CCB-9ADA-7ED005C63313",
				"You can't merge this Import By External Broker job because WHS MRN line number is not set on some invoice lines.");

		public static string ReasonCannotMergeInvoiceLineHasDuplicateWHSMRNLineNumberPerEnteryInstruction(
			ZString entryInstruction, ZShort whsMRNLine)
			=> ResString.GetMultilingualString("5FDD518C-2F9B-4A60-ACD8-C6BA25F71D8D",
				"You can't merge this Import By External Broker job because WHS MRN line numbers must be unique per Entry Instruction '{0}'; WHS MRN Line '{1}' has been entered more than once.",
				entryInstruction, whsMRNLine);

		public static string ReasonCannotMergeEntryInstructionHasNoWHSMRNLineNumber
			=> ResString.GetMultilingualString("E05A2CB8-79F3-423B-98BD-48A7C511F945",
				"You can't merge this Import By External Broker job because WHS MRN number is not set on some Entry Instructions.");

		public static string ReasonCannotMergeEntryInstructionHasDuplicateWHSMRNLineNumbers
			=> ResString.GetMultilingualString("4CA65CB9-172D-45D0-8BB9-4DFE98DA9D7E",
				"You can't merge this Import By External Broker job because WHS MRN number must be unique set on Entry Instructions.");
	}
}
