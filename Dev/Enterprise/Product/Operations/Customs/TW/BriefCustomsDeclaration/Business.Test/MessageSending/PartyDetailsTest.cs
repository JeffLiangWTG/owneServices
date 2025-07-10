using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	sealed class PartyDetailsTest : TestCaseWithFactory
	{
		public void TestData()
		{
			var carrierOrg = new TestTWCreator(Factory).CreateOrganization();
			carrierOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "96944490", Core.Constants.CountryCodes.Taiwan);
			carrierOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "96944491", Core.Constants.CountryCodes.Taiwan);
			carrierOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.PID, "96944492", Core.Constants.CountryCodes.Taiwan);
			var party = new PartyDetails(carrierOrg.MainAddress) as IPartyDetails;
			CombineAssertions(() =>
			{
				AssertEquals("ID", "96944490", party.ID);
				AssertEquals("TypeCode", PartyIdentifierCodeList.Codes._58, party.TypeCode);
			});
		}
	}
}
