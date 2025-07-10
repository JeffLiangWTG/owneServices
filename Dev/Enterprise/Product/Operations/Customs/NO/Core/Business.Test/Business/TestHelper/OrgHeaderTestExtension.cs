using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NO.Business.Testing;

static class OrgHeaderTestExtension
{
	public static OrgHeader AsDeferredDutiesAccount(this OrgHeader self, string approvalNumber = "12345612")
		=> self.WithOrgCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, approvalNumber);

	public static OrgHeader AsMVARegistered(this OrgHeader self, string orgNo = "123456789")
		=> self.WithOrgCusCode(OrgCusCode.NorwayCodeTypes.MVA, orgNo);

	public static OrgHeader WithOrgCusCode(this OrgHeader self, ZString codeType, ZString orgNo)
	{
		var declarantCustomsCode = self.CustomsCodes.AddNew();
		declarantCustomsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Norway;
		declarantCustomsCode.OK_CodeType = codeType;
		declarantCustomsCode.OK_CustomsRegNo = orgNo;
		return self;
	}
}
