using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class MergeManager : EU.Business.Declaration.MergeManager
{
	public MergeManager(JobDeclaration jobDec)
		: base(jobDec)
	{
	}

	protected override Customs.Business.LineMerger GetNewLineMergerCore()
	{
		var declaration = (JobDeclaration)Declaration;
		return new LineMerger(declaration);
	}

	public override ZString CheckAndGetPrerequisiteConditions(ISendsMessagesToCustoms notifier)
	{
		var result = base.CheckAndGetPrerequisiteConditions(notifier);

		if (result.IsEmpty)
		{
			if (IsAnyEntryInstructionDateForDutyObsolete)
			{
				var yesNoCancel = notifier.YesNoCancelQuery(UpdateEntryInstructionsDeclDateQuestion, UpdateEntryInstructionsDeclDateCaption);
				switch (yesNoCancel)
				{
					case YesNoCancel.Yes:
					{
						foreach (CusEntryInstruction entry in Declaration.CustomsEntryInstructions)
						{
							if (entry.DateForDutyIsObsolete)
							{
								entry.CEI_DateForDuty = ZDateTime.Today;
							}
						}
					}
						break;

					case YesNoCancel.Cancel:
						result = UpdateEntryInstructionsDeclDateCancelMerge;
						break;

					case YesNoCancel.No: // do nothing
						break;
				}
			}
		}

		if (result.IsEmpty && IsAnyJobComInvoiceLineWithEmptyJI_CEI)
		{
			if (Declaration.CustomsEntryInstructions.Count == 1)
			{
				if (Declaration.AreMultipleEntryInstructionsAllowed)
				{
					var yesNoMessage = notifier.YesNoQuery(UpdateInvoiceLineEntryInstructionDataQuestion, UpdateInvoiceLineEntryInstructionDataCaption);
					if (yesNoMessage)
					{
						SetEntryInstructionToInvoiceLine();
					}
					else
					{
						result = CannotMergeBecauseOfInvoiceLinesWithoutEntryInstruction;
					}
				}
				else
				{
					SetEntryInstructionToInvoiceLine();
				}
			}
			else
			{
				notifier.WarnUserAboutSomething(CannotMergeBecauseOfInvoiceLinesWithoutEntryInstruction, MultipleInstructionsPreventAutoAssignment);
				result = CannotMergeBecauseOfInvoiceLinesWithoutEntryInstruction;
			}
		}

		return result;
	}

	void SetEntryInstructionToInvoiceLine()
	{
		var singleInstructionPK = SingleCusEntryInstruction.PK;
		foreach (var invoiceLine in Declaration.InvoiceLines.Cast<JobComInvoiceLine>())
		{
			invoiceLine.JI_CEI = singleInstructionPK;
		}
	}

	protected override string GetReasonCannotMerge()
	{
		ZString result = base.GetReasonCannotMerge();

		if (result.IsEmpty && !IsEntryInstructionPresent)
		{
			result = Res.GetString("PLMergeManager|GetReasonCannotMergeEntryInstruction", "You can't merge this entry because there are no Entry Instructions.");
		}

		return result;
	}

	protected override void OnMerged()
	{
		base.OnMerged();
		foreach (var entryHeader in Declaration.ActiveEntryHeaders)
		{
			entryHeader.RunPreSaveValidation();
		}
	}

	bool IsEntryInstructionPresent => Declaration.CustomsEntryInstructions.Count > 0;
	bool IsAnyEntryInstructionDateForDutyObsolete => Declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(e => e.DateForDutyIsObsolete);
	bool IsAnyJobComInvoiceLineWithEmptyJI_CEI => Declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(invoiceLine => invoiceLine.JI_CEI.IsEmpty);

	static string UpdateEntryInstructionsDeclDateQuestion => Res.GetString("PLMergeManager|UpdateEntryInstructionsDeclDateQuestion", "Would you like to update all Entry Instructions Declaration Date. ?\r\n Click 'YES' to update, 'No' to ignore, 'Cancel' to cancel merge.");
	static string UpdateEntryInstructionsDeclDateCaption => Res.GetString("PLMergeManager|UpdateEntryInstructionsDeclDateCaption", "Entry Instruction Declaration Date is older than current date.");
	static string UpdateEntryInstructionsDeclDateCancelMerge => Res.GetString("PLMergeManager|UpdateEntryInstructionsDeclDateCancelMerge", "Entry Instruction Declaration Date is obsolete user canceled merge.");
	string UpdateInvoiceLineEntryInstructionDataQuestion => Res.GetString("PLMergeManager|UpdateInvoiceLineEntryInstructionDateQuestion", "You can’t merge this entry because there are Invoice Lines without Entry Instruction.\r\n Auto assign Entry Instruction < {0} - {1} > \r\n for all Invoice Lines?", SingleCusEntryInstruction.CEI_SubStyle, SingleCusEntryInstruction.CEI_Description);
	static string CannotMergeBecauseOfInvoiceLinesWithoutEntryInstruction => Res.GetString("PLMergeManager|SendCannotMergeMessage", "You can’t merge this entry because there are Inv. Lines without Entry Instruction.");
	static string UpdateInvoiceLineEntryInstructionDataCaption => Res.GetString("PLMergeManager|UpdateInvoiceLineEntryInstructionDateCaption", "There are Inv. Lines without Entry Instruction.");
	static string MultipleInstructionsPreventAutoAssignment => Res.GetString("PLMergeManager|SendCannotMergeMessageCaption", "There are multiple Entry Instructions that prevent automatic assignment.");

	CusEntryInstruction SingleCusEntryInstruction => Declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().First();
}
