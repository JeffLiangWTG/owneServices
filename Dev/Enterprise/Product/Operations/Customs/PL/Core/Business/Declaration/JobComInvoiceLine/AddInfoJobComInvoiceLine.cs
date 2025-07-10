using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class AddInfoJobComInvoiceLine : EU.Business.Declaration.AddInfoJobComInvoiceLine
{
	public AddInfoJobComInvoiceLine(ZPropertyInfo addInfoProperty)
		: base(addInfoProperty)
	{
	}

	public JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)Parent;

	protected override EUAddInfoValidation GetNewValidation() => InvoiceLine switch
	{
		{ IsImport: true } => new ImportAddInfoJobComInvoiceLineValidation(this),
		{ IsExport: true } => new AddInfoJobComInvoiceLineValidation(this),
		_ => new EU.Business.Declaration.AddInfoJobComInvoiceLineValidation(this)
	};

	public new AddInfoJobComInvoiceLineLookups Lookups => (AddInfoJobComInvoiceLineLookups)base.Lookups;

	protected override EUAddInfoLookups GetNewLookups() => new AddInfoJobComInvoiceLineLookups(this);

	[MaxLength(4)]
	public override ZString ZG_CountryOfSupply
	{
		get => base.ZG_CountryOfSupply;
		set
		{
			var oldValue = ZG_CountryOfSupply;
			base.ZG_CountryOfSupply = value;
			if (oldValue != ZG_CountryOfSupply && !IsCopying)
			{
				Parent?.Declaration?.MarkAsNeedingValidation();
			}
		}
	}

	[ResourceStringData("F7E6D5C4-B3A2-1987-6543-210FEDCBA987", Caption = "Country of Destination")]
	public override ZString ZG_CountryOfDestination
	{
		get => base.ZG_CountryOfDestination;
		set
		{
			var oldValue = ZG_CountryOfDestination;
			base.ZG_CountryOfDestination = value;
			if (oldValue != value && !IsCopying)
			{
				Parent?.Declaration?.MarkAsNeedingValidation();
			}
		}
	}
}
