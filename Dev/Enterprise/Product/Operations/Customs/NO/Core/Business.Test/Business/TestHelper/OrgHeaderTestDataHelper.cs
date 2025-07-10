using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NO.Business.Testing;

static class OrgHeaderTestDataHelper
{
	public static OrgHeader CreateOrgHeaderWithMainAddress(BusinessObjectFactory factory,
		string fullName,
		string address1,
		string address2,
		string postCode,
		string city,
		string country,
		string orgCode = "CD1")
	{
		var orgHeader = factory.New<OrgHeader>();
		orgHeader.OH_FullName = fullName;
		orgHeader.OH_Code = orgCode;
		var mainAddress = orgHeader.MainAddress;
		mainAddress.OA_Address1 = address1;
		mainAddress.OA_Address2 = address2;
		mainAddress.OA_PostCode = postCode;
		mainAddress.OA_City = city;
		mainAddress.OA_RN_NKCountryCode = country;
		return orgHeader;
	}

	public static CusAuthorisationHeader CreateCusAuthorisationHeader(BusinessObjectFactory factory, string authorizationType)
	{
		var declarant = factory.NewWithValidTestData<OrgHeader>();
		declarant.OH_Code = "CH123";
		var appliesTo = factory.NewWithValidTestData<OrgHeader>();
		appliesTo.OH_Code = "APT01";

		var authorization = factory.New<CusAuthorisationHeader>();
		authorization.CPH_Type = authorizationType;
		authorization.CPH_OH_PermitHolder = declarant.PK;
		authorization.CPH_Number = "09123";
		authorization.CPH_PermitDescription = "DESC123";
		authorization.CPH_OA_AppliesTo = appliesTo.MainAddress.PK;
		authorization.CPH_StartDate = ZDate.Today.AddDays(-10);
		return authorization;
	}
}
