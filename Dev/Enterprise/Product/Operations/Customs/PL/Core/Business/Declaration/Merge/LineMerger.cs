using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class LineMerger : EU.Business.Declaration.LineMerger
{
	public LineMerger(JobDeclaration declaration) : base(declaration)
	{
	}

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override void PerformCountrySpecificOperationAfterMergeAfterCalculateDuty()
	{
		base.PerformCountrySpecificOperationAfterMergeAfterCalculateDuty();

		SetGuaranteeAmountIfNeededForEntries();
		SaveMergedLineNumbersIfNeeded();
	}

	void SetGuaranteeAmountIfNeededForEntries()
	{
		foreach (var entryHeader in Declaration.CustomsEntryHeaders)
		{
			entryHeader.EntryInstruction?.SetGuaranteeAmountIfNeeded();
		}
	}

	void SaveMergedLineNumbersIfNeeded()
	{
		foreach (var invoiceLine in Declaration.CustomsEntryInstructions
			.Where(x => x.IsSimplifiedEntryInstruction)
			.SelectMany(x => x.InvoiceLines))
		{
			if (ZShort.TryParse(invoiceLine.MergedLineNumber, out var mergedLineNumber))
			{
				invoiceLine.JI_TargetEntryLineNumber = mergedLineNumber;
			}
		}
	}

	protected override IDutyCalculatorStrategy GetNewDutyCalculatorStrategy() => new DutyCalculatorStrategy(Declaration);

	void SetMergeKeyForSupplementaryDeclaration()
	{
		var relatedDeclaration = Declaration.RelatedDeclarations.FirstOrDefault();
		if (relatedDeclaration is not null && !relatedDeclaration.JE_MergeBy.IsEmpty)
		{
			Declaration.JE_MergeBy = relatedDeclaration.JE_MergeBy;
		}
	}

	protected override ILineNumberAssigner GetLineNumberAssigner(Customs.Business.CusEntryHeader entryHeader)
	{
		var header = entryHeader as CusEntryHeader;

		if (Declaration.IsSupplementaryDeclaration)
		{
			SetMergeKeyForSupplementaryDeclaration();

			return new LineNumberAssigner(header);
		}
		else
		{
			return base.GetLineNumberAssigner(entryHeader);
		}
	}
}
