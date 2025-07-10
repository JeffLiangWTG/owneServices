using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105NotifyPartyWrapperTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestNotifyPartyWrapper()
		{
			IPartyDetails notifyParty = new NX5105NotifyPartyWrapper(organization1.MainAddress, "VAT", "PAS", "PID");
			NUnit.Framework.Assert.That(notifyParty.Name, NUnit.Framework.Is.EqualTo("OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison), "NotifyParty.Name should be");
			NUnit.Framework.Assert.That(notifyParty.ChineseName, NUnit.Framework.Is.EqualTo("TW OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison), "NotifyParty.Name should be");
			NUnit.Framework.Assert.That(notifyParty.Address.Line, NUnit.Framework.Is.EqualTo("ADDRESS 1 ADDRESS 2 TAIWAN").Using(CustomComparers.TypeComparison), "NotifyParty.Address.Line should be");
			NUnit.Framework.Assert.That(notifyParty.Address.ChineseLine, NUnit.Framework.Is.EqualTo("TW OTA ADDRESS 1TW OTA ADDRESS 2").Using(CustomComparers.TypeComparison), "NotifyParty.Address.ChineseLine should be");
			NUnit.Framework.Assert.That(notifyParty.ID, NUnit.Framework.Is.EqualTo("123465789").Using(CustomComparers.TypeComparison), "NotifyParty.ID should be");
			NUnit.Framework.Assert.That(notifyParty.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison), "NotifyParty.TypeCode should be");
			organization1.MainAddress.CustomsCodes.Delete(vATCusCode);
			notifyParty = new NX5105NotifyPartyWrapper(organization1.MainAddress, "VAT", "PAS", "PID");
			NUnit.Framework.Assert.That(notifyParty.ID, NUnit.Framework.Is.EqualTo("NOPASREGNO").Using(CustomComparers.TypeComparison), "NotifyParty.ID should be");
			NUnit.Framework.Assert.That(notifyParty.TypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison), "NotifyParty.TypeCode should be");
			organization1.MainAddress.CustomsCodes.Delete(pASCusCode);
			notifyParty = new NX5105NotifyPartyWrapper(organization1.MainAddress, "VAT", "PAS", "PID");
			NUnit.Framework.Assert.That(notifyParty.ID, NUnit.Framework.Is.EqualTo("PIDREGNO").Using(CustomComparers.TypeComparison), "NotifyParty.ID should be");
			NUnit.Framework.Assert.That(notifyParty.TypeCode, NUnit.Framework.Is.EqualTo("174").Using(CustomComparers.TypeComparison), "NotifyParty.TypeCode should be");
		}

		OrgHeader organization1;
		OrgCusCode vATCusCode;
		OrgCusCode pASCusCode;
		OrgCusCode pIDCusCode;
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
			address1.OA_Language = "EN";
			address1.OA_Address1 = "ADDRESS 1";
			address1.OA_Address2 = "ADDRESS 2";
			var entranslatedAddress1 = address1.TranslatedAddresses.AddNew();
			entranslatedAddress1.OTA_Language = "EN";
			entranslatedAddress1.OTA_Address1 = "EN OTA ADDRESS 1";
			entranslatedAddress1.OTA_Address2 = "EN OTA ADDRESS 2";
			var zhTWtranslatedAddress1 = address1.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress1.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			zhTWtranslatedAddress1.OTA_CompanyName = "TW OVERRIDEN COMPANY NAME";
			zhTWtranslatedAddress1.OTA_Address1 = "TW OTA ADDRESS 1";
			zhTWtranslatedAddress1.OTA_Address2 = "TW OTA ADDRESS 2";
			vATCusCode = organization1.CustomsCodes.AddNew();
			vATCusCode.OK_RN_NKCodeCountry = "TW";
			vATCusCode.OK_CodeType = "VAT";
			vATCusCode.OK_CustomsRegNo = "123465789";
			pASCusCode = organization1.CustomsCodes.AddNew();
			pASCusCode.OK_RN_NKCodeCountry = "TW";
			pASCusCode.OK_CodeType = "PAS";
			pASCusCode.OK_CustomsRegNo = "PASREGNO";
			pIDCusCode = organization1.CustomsCodes.AddNew();
			pIDCusCode.OK_RN_NKCodeCountry = "TW";
			pIDCusCode.OK_CodeType = "PID";
			pIDCusCode.OK_CustomsRegNo = "PIDREGNO";
			Factory.Save();
		}
	}
}
