using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ExitSummaryJobDocAddressValidation(AutoJobDocAddress parent) : JobDocAddressValidation(parent)
{
	protected override void CheckOrganisationPK()
	{
		base.CheckOrganisationPK();

		if (Parent.Organisation is null)
		{
			return;
		}

		var eori = AddressHelper.GetEORIForOrganisation(Parent.Organisation);
		if (Parent.E2_AddressType == AutoDocAddressTypes.Codes.SupplierDocumentaryAddress && eori.IsEmpty)
		{
			Parent.OrganisationPKInfo.AddMessageError(Res.GetString("PLExportJobDocAddressValidation|OrganisationNoEoriMessage", "EORI number is missing in Organization data for the Supplier."));
		}
	}
}
