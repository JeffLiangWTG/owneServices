using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

internal static class AddressHelper
{
	public static ZString GetEORIForOrganisationAddress(OrgAddress orgAddress) => new ZString(GetEORICusCodeForOrganisationAddress(orgAddress)?.OK_CustomsRegNo);

	public static OrgCusCode GetEORICusCodeForOrganisationAddress(OrgAddress orgAddress)
	{
		var eoriCusCodes = orgAddress.Header.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

		var addressCusCode = eoriCusCodes.FirstOrDefault(x => x.OK_OA_PremisesAddress == orgAddress.PK);
		if (addressCusCode != null)
		{
			return addressCusCode;
		}

		return eoriCusCodes.FirstOrDefault(x => !x.OK_OA_PremisesAddress.IsValid);
	}

	public static ZString GetEORIForOrganisation(OrgHeader orgHeader)
	{
		var eoriCusCodes = orgHeader.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

		var eoriWithNoPremissesAddress = eoriCusCodes.FirstOrDefault(x => !x.OK_OA_PremisesAddress.IsValid);

		return new ZString(eoriWithNoPremissesAddress != null
			? eoriWithNoPremissesAddress.OK_CustomsRegNo
			: eoriCusCodes.FirstOrDefault()?.OK_CustomsRegNo);
	}

	public static OrgAddress GetExporterAddressWithSupplierFallback(JobDeclaration declaration)
	{
		return declaration.ExporterDocAddress.IsValidAddress
			? declaration.ExporterDocAddress.Address
			: declaration.SupplierDocumentaryAddress.IsValidAddress
				? declaration.SupplierDocumentaryAddress.Address
				: declaration.SupplierAddress;
	}
}
