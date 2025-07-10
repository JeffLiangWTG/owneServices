using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class AgentWrapperTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAgentWrapper()
		{
			IPartyDetails agent = new AgentWrapper("123465789", "RoleCode", "ZZZ", organization1.MainAddress);
			NUnit.Framework.Assert.That(agent.ID, NUnit.Framework.Is.EqualTo("123465789").Using(CustomComparers.TypeComparison), "ID should be");
			NUnit.Framework.Assert.That(agent.RoleCode, NUnit.Framework.Is.EqualTo("RoleCode").Using(CustomComparers.TypeComparison), "RoleCode should be");
			NUnit.Framework.Assert.That(agent.SubBoxID, NUnit.Framework.Is.EqualTo("ZZZ").Using(CustomComparers.TypeComparison), "SubBoxID should be");
			NUnit.Framework.Assert.That(agent.ChineseName, NUnit.Framework.Is.EqualTo("TW OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison), "ChineseName should be");
			NUnit.Framework.Assert.That(agent.Address.Line, NUnit.Framework.Is.EqualTo("ADDRESS 1 ADDRESS 2 TAIWAN").Using(CustomComparers.TypeComparison), "Address.Line should be");
			NUnit.Framework.Assert.That(agent.Address.ChineseLine, NUnit.Framework.Is.EqualTo("TW OTA ADDRESS 1TW OTA ADDRESS 2").Using(CustomComparers.TypeComparison), "Address.ChineseLine should be");
			NUnit.Framework.Assert.That(agent.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("TWAEO-123465789").Using(CustomComparers.TypeComparison), "Seller.LPCOAuthorizedParty.ID should be");
		}

		OrgHeader organization1;
		OrgCusCode vATCusCode;
		protected override void SetUp()
		{
			base.SetUp();
			SetupOrganizations();
		}

		void SetupOrganizations()
		{
			organization1 = Factory.New<OrgHeader>();
			organization1.Addresses.RemoveAndDeleteAll();
			organization1.OH_Code = "Org1";
			organization1.OH_RL_NKClosestPort = "TW";
			var address1 = organization1.Addresses[0];
			address1.OA_RN_NKCountryCode = "TW";
			address1.OA_CompanyNameOverride = "OVERRIDEN COMPANY NAME";
			address1.OA_Language = Core.SharedConstants.Languages.English;
			address1.OA_Address1 = "ADDRESS 1";
			address1.OA_Address2 = "ADDRESS 2";
			var zhTWtranslatedAddress1 = address1.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress1.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			zhTWtranslatedAddress1.OTA_CompanyName = "TW OVERRIDEN COMPANY NAME";
			zhTWtranslatedAddress1.OTA_Address1 = "TW OTA ADDRESS 1";
			zhTWtranslatedAddress1.OTA_Address2 = "TW OTA ADDRESS 2";
			organization1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.AEO, "123465789", "TW");
			vATCusCode = address1.CustomsCodes.AddNew();
			vATCusCode.OK_RN_NKCodeCountry = "TW";
			vATCusCode.OK_CodeType = "VAT";
			vATCusCode.OK_CustomsRegNo = "123465789";
			Factory.Save();
		}
	}
}
