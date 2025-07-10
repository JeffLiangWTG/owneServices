using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class AddInfoJobComInvoiceLineValidation : EU.Business.Declaration.AddInfoJobComInvoiceLineValidation
{
	public AddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent)
		: base(parent)
	{
	}
	public new AddInfoJobComInvoiceLine Parent => (AddInfoJobComInvoiceLine)base.Parent;

	protected JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)Parent.Parent;

	protected override void CheckZG_CountryOfDestination()
	{
		base.CheckZG_CountryOfDestination();
		var invoiceLine = Parent.InvoiceLine;
		if (invoiceLine == null)
		{
			return;
		}

		if (invoiceLine.Declaration?.IsUCC6AndIsExport ?? false)
		{
			IsCountryOfDestinationEmpty(invoiceLine);
		}
	}

	void IsCountryOfDestinationEmpty(JobComInvoiceLine invoiceLine)
	{
		var entryInstruction = invoiceLine.EntryInstruction;
		var declaration = invoiceLine.Declaration;

		if (entryInstruction is null || declaration.ZG_IsSecurityDeclaration)
		{
			return;
		}

		var entryInstructionSubStyle = entryInstruction.CEI_SubStyle;

		new HeaderOrLineValueValidator<ZString>(
			headerValueProvider: () => declaration.JE_GoodsDestination,
			lineValuesProvider: () => entryInstruction
										.InvoiceLines
										.Cast<JobComInvoiceLine>()
										.Select(x => x.ZG_CountryOfDestination))
		{
			IsEmptyFunc = x => x.IsEmpty
		}
		.ValidateLine(invoiceLine.ZG_CountryOfDestinationInfo, invoiceLine.ZG_CountryOfDestination);
	}
}
