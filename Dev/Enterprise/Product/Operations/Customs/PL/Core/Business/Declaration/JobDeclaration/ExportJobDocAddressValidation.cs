using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ExportJobDocAddressValidation : JobDocAddressValidation
{
	public ExportJobDocAddressValidation(AutoJobDocAddress parent, JobDeclaration declaration) : base(parent)
	{
		isSupplierDocumentaryAddress = parent.E2_AddressType == DocAddressTypes.Codes.SupplierDocumentaryAddress;
		isExporter = parent.E2_AddressType == DocAddressTypes.Codes.Exporter;
		this.declaration = declaration;
	}

	readonly bool isSupplierDocumentaryAddress;
	readonly bool isExporter;
	readonly JobDeclaration declaration;

	protected override void CheckOrganisationPK()
	{
		base.CheckOrganisationPK();
		if (isSupplierDocumentaryAddress && Parent.Organisation != null)
		{
			var eori = AddressHelper.GetEORIForOrganisation(Parent.Organisation);
			if (eori.IsEmpty)
			{
				Parent.OrganisationPKInfo.AddMessageError(Res.GetString("PLExportJobDocAddressValidation|OrganisationNoEoriMessage", "EORI number is missing in Organization data for the Supplier."));
			}
		}

		if (isSupplierDocumentaryAddress && !Parent.OrganisationPK.IsValid && !(declaration.ExporterDocAddress?.E2_OA_Address.IsValid ?? false))
		{
			Parent.OrganisationPKInfo.AddMessageError(Res.GetString("PLExportJobDocAddressValidation|OrganisationPKNoAddress", "You have not selected Supplier / Exporter address."));
		}
	}

	protected override void CheckE2_OA_Address()
	{
		base.CheckE2_OA_Address();
		if ((isExporter || isSupplierDocumentaryAddress) && Parent.Address != null)
		{
			var eori = AddressHelper.GetEORIForOrganisationAddress(Parent.Address);
			if (eori.IsEmpty)
			{
				Parent.E2_OA_AddressInfo.AddMessageError(Res.GetString("PLExportJobDocAddressValidation|E2_OA_AddressNoEoriMessage", "Selected organization does not have an EORI number."));
				CheckRuleR0088E();
			}
		}
	}

	void CheckRuleR0088E()
	{
		var address = AddressHelper.GetExporterAddressWithSupplierFallback(declaration);
		if (address?.Header is OrgHeader orgHeader && orgHeader.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori).IsEmpty)
		{
			if (orgHeader.OH_Category == OrgConstants.Category.NaturalPersonIndividual)
			{
				if (CheckIdentifyDataIsEmptyWhenIsNaturalPerson(orgHeader))
				{
					Parent.E2_OA_AddressInfo.AddMessageError(Res.GetString("76bc2bd6-9fed-426e-9c02-deeea4e5bcd1", "[R0088E] Either PESEL and/or other identification number is required when EORI is not available."));
				}
			}
			else if (CheckIdentifyNumberIsEmptyWhenNotNaturalPerson(orgHeader))
			{
				Parent.E2_OA_AddressInfo.AddMessageError(Res.GetString("d29ba59a-6599-4bea-81b8-94352a1b1ccc", "[R0088E] Either NIP and/or REGON is required when EORI is not available."));
			}

			bool CheckIdentifyDataIsEmptyWhenIsNaturalPerson(OrgHeader header)
				=> header.CustomsCodes.GetCustomsRegNo(Constants.CusCodeTypes.PES, Core.Constants.CountryCodes.Poland).IsEmpty
					&& header.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID).IsEmpty;

			bool CheckIdentifyNumberIsEmptyWhenNotNaturalPerson(OrgHeader header)
				=> header.CustomsCodes.GetCustomsRegNo(OrgCusCode.PolandCodeTypes.NIP, Core.Constants.CountryCodes.Poland).IsEmpty
					&& header.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.GovBusinessCode, Core.Constants.CountryCodes.Poland).IsEmpty;
		}
	}
}
