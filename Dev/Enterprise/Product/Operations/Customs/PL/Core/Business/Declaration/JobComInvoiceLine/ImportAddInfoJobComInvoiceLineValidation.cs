using CargoWise.Types;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ImportAddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent) : AddInfoJobComInvoiceLineValidation(parent)
{
	protected override void CheckZG_CountryOfSupply()
	{
		base.CheckZG_CountryOfSupply();

		if (Parent.ZG_CountryOfSupply.IsEmpty)
		{
			CheckDeclarationGoodsOriginWillBeUsed();
		}
	}

	void CheckDeclarationGoodsOriginWillBeUsed()
	{
		var declarationCountryOfSupply = Parent.InvoiceLine?.Declaration?.JE_GoodsOrigin ?? ZString.Empty;
		if (!declarationCountryOfSupply.IsEmpty)
		{
			Parent.ZG_CountryOfSupplyInfo.AddWarning(Res.GetString("PLImportAddInfoJobComInvoiceLineValidation|CheckIfCountryOfSupplyWillBeIgnored"
				, "The Declaration/[15] Dispatch country will be used."));
		}
	}
}
