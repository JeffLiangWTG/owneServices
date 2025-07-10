using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX101PartyDetailsWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestTypeCodeID()
		{
			var controllingMessageHeader = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = "DUMMY COMP";
			var jobDocAddress = controllingMessageHeader.ImporterDocumentaryAddress;
			jobDocAddress.OrganisationPK = org.PK;

			CombineAssertions(() =>
			{
				org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "PAS001", Core.Constants.CountryCodes.Taiwan);
				IPartyDetails importerWrapper = new NX101PartyDetailsWrapper(jobDocAddress, false);
				NUnit.Framework.Assert.That(importerWrapper.TypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison), "TypeCode for PassportID");
				NUnit.Framework.Assert.That(importerWrapper.ID, NUnit.Framework.Is.EqualTo("PAS001").Using(CustomComparers.TypeComparison), "ID for PassportID");

				org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.PID, "PID001", Core.Constants.CountryCodes.Taiwan);
				importerWrapper = new NX101PartyDetailsWrapper(jobDocAddress, false);
				NUnit.Framework.Assert.That(importerWrapper.TypeCode, NUnit.Framework.Is.EqualTo("174").Using(CustomComparers.TypeComparison), "TypeCode for PID");
				NUnit.Framework.Assert.That(importerWrapper.ID, NUnit.Framework.Is.EqualTo("PID001").Using(CustomComparers.TypeComparison), "ID for PID");

				org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "VAT001", Core.Constants.CountryCodes.Taiwan);
				importerWrapper = new NX101PartyDetailsWrapper(jobDocAddress, false);
				NUnit.Framework.Assert.That(importerWrapper.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison), "TypeCode for VATCode");
				NUnit.Framework.Assert.That(importerWrapper.ID, NUnit.Framework.Is.EqualTo("VAT001").Using(CustomComparers.TypeComparison), "ID for VATCode");
			});
		}

		[ExpectNoExceptions]
		public void TestChineseName()
		{
			var controllingMessageHeader = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = "DUMMY COMP";
			var address = org.MainAddress;
			address.OA_CompanyNameOverride = "測試";
			address.OA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			var jobDocAddress = controllingMessageHeader.ImporterDocumentaryAddress;
			jobDocAddress.OrganisationPK = org.PK;
			IPartyDetails importerWrapper = new NX101PartyDetailsWrapper(jobDocAddress, false);
			NUnit.Framework.Assert.That(importerWrapper.ChineseName, NUnit.Framework.Is.EqualTo("測試").Using(CustomComparers.TypeComparison));

			var address2 = org.Addresses.AddNew();
			address2.OA_CompanyNameOverride = "test aad2";
			address2.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;
			var tAddress = address2.TranslatedAddresses.AddNew();
			tAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			tAddress.OTA_CompanyName = "T公司";
			jobDocAddress.E2_OA_Address = address2.PK;
			importerWrapper = new NX101PartyDetailsWrapper(jobDocAddress, false);
			NUnit.Framework.Assert.That(importerWrapper.ChineseName, NUnit.Framework.Is.EqualTo("T公司").Using(CustomComparers.TypeComparison));

			jobDocAddress.E2_AddressOverride = true;
			var twAddress = jobDocAddress.LocalAddress;
			twAddress.E2_AddressType = "ITA";
			twAddress.E2_ParentTableCode = "TW1";
			twAddress.E2_CompanyName = "產證進口人中文名稱";
			twAddress.E2_AddressOverride = true;
		}

		[ExpectNoExceptions]
		public void TestName()
		{
			var controllingMessageHeader = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = "DUMMY COMP";
			var address = org.MainAddress;
			address.OA_CompanyNameOverride = "C Name";
			address.OA_Language = Core.SharedConstants.Languages.English;
			var jobDocAddress = controllingMessageHeader.ImporterDocumentaryAddress;
			jobDocAddress.OrganisationPK = org.PK;
			IPartyDetails importerWrapper = new NX101PartyDetailsWrapper(jobDocAddress, false);
			NUnit.Framework.Assert.That(importerWrapper.Name, NUnit.Framework.Is.EqualTo("C Name").Using(CustomComparers.TypeComparison));

			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_CompanyName = "J Name";
			importerWrapper = new NX101PartyDetailsWrapper(jobDocAddress, false);
			NUnit.Framework.Assert.That(importerWrapper.Name, NUnit.Framework.Is.EqualTo("J Name").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAddressLine()
		{
			var controllingMessageHeader = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = "DUMMY COMP";
			var address = org.MainAddress;
			address.OA_CompanyNameOverride = "C Name";
			address.OA_Language = Core.SharedConstants.Languages.English;
			address.OA_Address1 = "Addr 1";
			address.OA_Address2 = "Addr 2";
			address.OA_City = "Sydney";
			address.OA_State = "NSW";
			address.OA_PostCode = "105";
			address.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Australia;
			var jobDocAddress = controllingMessageHeader.ImporterDocumentaryAddress;
			jobDocAddress.OrganisationPK = org.PK;
			IPartyDetails importerWrapper = new NX101PartyDetailsWrapper(jobDocAddress, false);
			NUnit.Framework.Assert.That(importerWrapper.Address.Line, NUnit.Framework.Is.EqualTo("ADDR 1 ADDR 2 SYDNEY NSW 105 AUSTRALIA").Using(CustomComparers.TypeComparison));

			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_Address1 = "E Addr 1";
			jobDocAddress.E2_Address2 = "E Addr 2";
			jobDocAddress.E2_AdditionalAddressInformation = "E Additional Addr";
			jobDocAddress.E2_City = "Los Angeles";
			jobDocAddress.E2_State = "LA";
			jobDocAddress.E2_Postcode = "90001";
			jobDocAddress.E2_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedStates;
			importerWrapper = new NX101PartyDetailsWrapper(jobDocAddress, false);
			NUnit.Framework.Assert.That(importerWrapper.Address.Line, NUnit.Framework.Is.EqualTo("E ADDR 1 E ADDR 2 E ADDITIONAL ADDR LOS ANGELES LA 90001 UNITED STATES").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAddressChineseLine()
		{
			var controllingMessageHeader = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = "DUMMY COMP";
			var address = org.MainAddress;
			address.OA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			address.OA_Address1 = "地址 1";
			address.OA_Address2 = "地址 2";
			address.OA_City = "台北";
			address.OA_State = "TPE";
			address.OA_PostCode = "105";
			address.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Taiwan;
			var jobDocAddress = controllingMessageHeader.ImporterDocumentaryAddress;
			jobDocAddress.OrganisationPK = org.PK;
			IPartyDetails importerWrapper = new NX101PartyDetailsWrapper(jobDocAddress, false);
			NUnit.Framework.Assert.That(importerWrapper.Address.ChineseLine, NUnit.Framework.Is.EqualTo("105台灣TPE台北地址 1地址 2").Using(CustomComparers.TypeComparison));

			var address2 = org.Addresses.AddNew();
			address2.OA_CompanyNameOverride = "test aad2";
			address2.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;
			address2.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Taiwan;
			var tAddress = address2.TranslatedAddresses.AddNew();
			tAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			tAddress.OTA_Address1 = "T地址 1";
			tAddress.OTA_Address2 = "T地址 2";
			tAddress.OTA_City = "台北";
			tAddress.OTA_State = "TPE";
			tAddress.OTA_PostCode = "105";
			jobDocAddress.E2_OA_Address = address2.PK;
			importerWrapper = new NX101PartyDetailsWrapper(jobDocAddress, false);
			NUnit.Framework.Assert.That(importerWrapper.Address.ChineseLine, NUnit.Framework.Is.EqualTo("105台灣TPE台北T地址 1T地址 2").Using(CustomComparers.TypeComparison));

			jobDocAddress.E2_AddressOverride = true;
			var importerTranslatedDocumentaryAddress = jobDocAddress.LocalAddress;
			importerTranslatedDocumentaryAddress.E2_Address1 = "產證進口人";
			importerTranslatedDocumentaryAddress.E2_Address2 = "中文地址";
			importerTranslatedDocumentaryAddress.E2_City = "台北";
			importerTranslatedDocumentaryAddress.E2_State = "TPE";
			importerTranslatedDocumentaryAddress.E2_Postcode = "105";

			CombineAssertions(() =>
			{
				importerWrapper = new NX101PartyDetailsWrapper(jobDocAddress, false);
				NUnit.Framework.Assert.That(importerWrapper.Address.ChineseLine, NUnit.Framework.Is.EqualTo("105台灣TPE台北產證進口人中文地址").Using(CustomComparers.TypeComparison), "ChineseLine is not expected when is not Certificate15");
				importerWrapper = new NX101PartyDetailsWrapper(jobDocAddress, true);
				NUnit.Framework.Assert.That(importerWrapper.Address.ChineseLine, NUnit.Framework.Is.EqualTo("TPE台北產證進口人中文地址").Using(CustomComparers.TypeComparison), "ChineseLine is not expected when is Certificate15");
			});
		}

		[ExpectNoExceptions]
		public void TestCommunications()
		{
			var controllingMessageHeader = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = "DUMMY COMP";
			var address = org.MainAddress;
			address.OA_CompanyNameOverride = "測試";
			address.OA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			var jobDocAddress = controllingMessageHeader.ImporterDocumentaryAddress;
			jobDocAddress.OrganisationPK = org.PK;
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_Fax = "021111";
			jobDocAddress.E2_Phone = "035855";
			jobDocAddress.E2_Email = "xxx@ppp.com";
			IPartyDetails importerWrapper = new NX101PartyDetailsWrapper(jobDocAddress, false);
			NUnit.Framework.Assert.That(importerWrapper.Communications.Any(x => x.TypeID == "FX" && x.ID == "021111"), NUnit.Framework.Is.True, "Fax");
			NUnit.Framework.Assert.That(importerWrapper.Communications.Any(x => x.TypeID == "MA" && x.ID == "xxx@ppp.com"), NUnit.Framework.Is.True, "Email");
			NUnit.Framework.Assert.That(importerWrapper.Communications.Any(x => x.TypeID == "TE" && x.ID == "035855"), NUnit.Framework.Is.True, "Phone");
		}
	}
}
