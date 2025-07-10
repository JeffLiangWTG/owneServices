using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business;

public class LineMerger(JobDeclaration declaration) : Customs.Business.LineMerger(declaration)
{
	new JobDeclaration Declaration { get; } = declaration;

	protected override IDutyCalculatorStrategy GetNewDutyCalculatorStrategy() => new DutyCalculatorStrategy(Declaration);

	protected override Customs.Business.EntryCreationStrategy[] GetEntryCreationStrategies() =>
	[
		Declaration.CreateEntryCreationStrategy(),
	];

	protected override void OnMerged()
	{
		base.OnMerged();
		foreach (var entryHeader in Declaration.CustomsEntryHeaders)
		{
			entryHeader.DefaultPaymentMethod();
		}
		Declaration.CustomsEntryInstructions.RefreshBindingIncludingChildren();
	}

	protected override void PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty()
	{
		base.PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty();
		foreach (var line in Declaration.CustomsEntryHeaders.SelectMany(header => header.MergedLines))
		{
			line.CL_StatisticalValue = ((ZDecimal)line.InvoiceLines.OfType<JobComInvoiceLine>().Sum(x => x.JI_Calc_StatisticalValue)).Round(0);
			line.CL_InvoiceAmount = line.InvoiceLines.OfType<JobComInvoiceLine>().Sum(x => x.JI_LinePriceInLocalCurrency);
		}
	}
}
