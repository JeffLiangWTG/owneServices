using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(OrgHeaderHelper))]
	sealed class OrgHeaderHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetChineseAddress()
		{
			OrgAddress address = null;
			NUnit.Framework.Assert.That(address.GetChineseAddress(), NUnit.Framework.Is.EqualTo(ZString.Empty), "address is null");
			NUnit.Framework.Assert.That(testAddress.GetChineseAddress(), NUnit.Framework.Is.EqualTo(ZString.Empty), "No chinese address");

			testAddress.OA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			NUnit.Framework.Assert.That(testAddress.GetChineseAddress(), NUnit.Framework.Is.EqualTo("Address1Address2").Using(CustomComparers.TypeComparison), "OA_Language is ZH-TW");
			testAddress.OA_Address1 = "";
			NUnit.Framework.Assert.That(testAddress.GetChineseAddress(), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "OA_Language is ZH-TW");

			testAddress.OA_Language = Core.SharedConstants.Languages.English;
			testAddress.OA_RN_NKCountryCode = "TW";
			var translatedAddress = testAddress.TranslatedAddresses.AddNew();
			translatedAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			translatedAddress.OTA_CompanyName = "綠晃科技股份有限公司";
			translatedAddress.OTA_Address1 = "臺北加工出口區園東街7號";
			translatedAddress.OTA_Address2 = string.Empty;
			translatedAddress.OTA_AdditionalAddressInformation = "5樓";
			translatedAddress.OTA_City = "臺北巿";
			translatedAddress.OTA_PostCode = "90093";
			translatedAddress.OTA_State = "TPE";
			NUnit.Framework.Assert.That(testAddress.GetChineseAddress(), NUnit.Framework.Is.EqualTo("90093台灣臺北巿臺北加工出口區園東街7號").Using(CustomComparers.TypeComparison), "Has ZH-TW translated address");
			translatedAddress.OTA_Address1 = "";
			NUnit.Framework.Assert.That(testAddress.GetChineseAddress(), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Has ZH-TW translated address");
		}

		[ExpectNoExceptions]
		public void TestGetPartyIdentifierCode()
		{
			NUnit.Framework.Assert.That(OrgHeaderHelper.GetPartyIdentifierCode(OrgCusCode.CodeTypes.VATCode), NUnit.Framework.Is.EqualTo(PartyIdentifierCodeList.Codes._58).Using(CustomComparers.TypeComparison), "VAT");
			NUnit.Framework.Assert.That(OrgHeaderHelper.GetPartyIdentifierCode(OrgCusCode.CodeTypes.PassportID), NUnit.Framework.Is.EqualTo(PartyIdentifierCodeList.Codes._53).Using(CustomComparers.TypeComparison), "PAS");
			NUnit.Framework.Assert.That(OrgHeaderHelper.GetPartyIdentifierCode(OrgCusCode.TaiwanCodeTypes.PID), NUnit.Framework.Is.EqualTo(PartyIdentifierCodeList.Codes._174).Using(CustomComparers.TypeComparison), "PID");
			NUnit.Framework.Assert.That(OrgHeaderHelper.GetPartyIdentifierCode(OrgCusCode.TaiwanCodeTypes.AEO), NUnit.Framework.Is.EqualTo(ZString.Empty), "Others");
		}

		[ExpectNoExceptions]
		public void TestCBPCodeTypes()
		{
			var expectedArray = new string[] { OrgCusCode.TaiwanCodeTypes.EPZ, OrgCusCode.TaiwanCodeTypes.CBF, OrgCusCode.TaiwanCodeTypes.FTZ, OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark, OrgCusCode.TaiwanCodeTypes.SciencePark };
			NUnit.Framework.Assert.That(OrgHeaderHelper.CBPCodeTypes, NUnit.Framework.Is.EqualTo(expectedArray));
		}

		[ExpectNoExceptions]
		public void TestIDCodeTypesWithCustomCode()
		{
			var expectedArray = new string[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID, Constants.OrgCusCodeType.CustomCode };
			NUnit.Framework.Assert.That(OrgHeaderHelper.IDCodeTypesWithCustomCode, NUnit.Framework.Is.EqualTo(expectedArray));
		}

		[ExpectNoExceptions]
		public void TestWareHoseCodeTypes()
		{
			var expectedArray = new string[] { OrgCusCode.CodeTypes.WarehouseControlledPremisesID, OrgCusCode.CodeTypes.ControlledPremisesID };
			NUnit.Framework.Assert.That(OrgHeaderHelper.WareHoseCodeTypes, NUnit.Framework.Is.EqualTo(expectedArray));
		}

		[ExpectNoExceptions]
		public void TestCheckHasCusCode()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var countryCode = Core.Constants.CountryCodes.Taiwan;
			var customCodes = new string[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.TaiwanCodeTypes.PID, OrgCusCode.CodeTypes.PassportID };
			NUnit.Framework.Assert.That(OrgHeaderHelper.CheckHasCusCode(orgHeader, countryCode, customCodes), NUnit.Framework.Is.EqualTo(false));

			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			NUnit.Framework.Assert.That(OrgHeaderHelper.CheckHasCusCode(orgHeader, countryCode, customCodes), NUnit.Framework.Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestGetRegNoWithOrganisationAddressInHelper()
		{
			NUnit.Framework.Assert.That(testAddress.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode), NUnit.Framework.Is.EqualTo("00612348").Using(CustomComparers.TypeComparison), "RegNo with CarrierCode");

			string[] codesToLookFor = new string[] { OrgCusCode.CodeTypes.CustomsCPPermitCode };
			NUnit.Framework.Assert.That(testAddress.GetCustomsRegNo(codesToLookFor), NUnit.Framework.Is.EqualTo("212233").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetRegNoWithOrganisationAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			NUnit.Framework.Assert.That(!OrgHeaderHelper.CheckHasVatInTW(null), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(!OrgHeaderHelper.CheckHasVatInTW(org), NUnit.Framework.Is.True);
			var address = org.MainAddress;
			address.Address1 = "Address1";
			address.Address2 = "Address2";
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "VAT", Core.Constants.CountryCodes.Taiwan);
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "PAS", Core.Constants.CountryCodes.Taiwan);
			org.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "PID", Core.Constants.CountryCodes.Taiwan);
			org.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.TPC, "TPC", Core.Constants.CountryCodes.Taiwan);
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CCC", Core.Constants.CountryCodes.Taiwan);
			org.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.AEO, "AEO", Core.Constants.CountryCodes.Taiwan);
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsCPPermitCode, "CPC", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();

			NUnit.Framework.Assert.That(address.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode), NUnit.Framework.Is.EqualTo("VAT").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(address.GetCustomsRegNo(OrgCusCode.CodeTypes.PassportID), NUnit.Framework.Is.EqualTo("PAS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(address.GetCustomsRegNo(OrgCusCode.TaiwanCodeTypes.PID), NUnit.Framework.Is.EqualTo("PID").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(address.GetCustomsRegNo(OrgCusCode.TaiwanCodeTypes.TPC), NUnit.Framework.Is.EqualTo("TPC").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(address.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode), NUnit.Framework.Is.EqualTo("CCC").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(address.GetCustomsRegNo(OrgCusCode.TaiwanCodeTypes.AEO), NUnit.Framework.Is.EqualTo("AEO").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(address.GetCustomsRegNo(OrgCusCode.CodeTypes.CustomsCPPermitCode), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(OrgHeaderHelper.CheckHasVatInTW(org), NUnit.Framework.Is.True);

			address.CustomsCodes.DeleteAll();
			Factory.Save();

			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "VAT", Core.Constants.CountryCodes.Taiwan);
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "PAS", Core.Constants.CountryCodes.Taiwan);
			address.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "PID", Core.Constants.CountryCodes.Taiwan);
			address.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.TPC, "TPC", Core.Constants.CountryCodes.Taiwan);
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CCC", Core.Constants.CountryCodes.Taiwan);
			address.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.AEO, "AEO", Core.Constants.CountryCodes.Taiwan);
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsCPPermitCode, "CPC", Core.Constants.CountryCodes.Taiwan);

			NUnit.Framework.Assert.That(address.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode), NUnit.Framework.Is.EqualTo("VAT").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(address.GetCustomsRegNo(OrgCusCode.CodeTypes.PassportID), NUnit.Framework.Is.EqualTo("PAS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(address.GetCustomsRegNo(OrgCusCode.TaiwanCodeTypes.PID), NUnit.Framework.Is.EqualTo("PID").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(address.GetCustomsRegNo(OrgCusCode.TaiwanCodeTypes.TPC), NUnit.Framework.Is.EqualTo("TPC").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(address.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode), NUnit.Framework.Is.EqualTo("CCC").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(address.GetCustomsRegNo(OrgCusCode.TaiwanCodeTypes.AEO), NUnit.Framework.Is.EqualTo("AEO").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(address.GetCustomsRegNo(OrgCusCode.CodeTypes.CustomsCPPermitCode), NUnit.Framework.Is.EqualTo("CPC").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(OrgHeaderHelper.CheckHasVatInTW(org), NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestGetOrgCusCode()
		{
			NUnit.Framework.Assert.That(testAddress.GetOrgCusCode(new string[] { OrgCusCode.CodeTypes.CustomsCPPermitCode, OrgCusCode.TaiwanCodeTypes.CBF }).OK_CustomsRegNo, NUnit.Framework.Is.EqualTo("212233").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(testAddress.GetOrgCusCode(new string[] { OrgCusCode.TaiwanCodeTypes.CBF, OrgCusCode.CodeTypes.CustomsCPPermitCode }).OK_CustomsRegNo, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(testAddress.GetOrgCusCode(new string[] { OrgCusCode.TaiwanCodeTypes.PID, OrgCusCode.TaiwanCodeTypes.CBF, OrgCusCode.CodeTypes.CustomsCPPermitCode }).OK_CustomsRegNo, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(testHeader.GetOrgCusCode(new string[] { OrgCusCode.TaiwanCodeTypes.MCI, OrgCusCode.CodeTypes.CarrierCode }).OK_CustomsRegNo, NUnit.Framework.Is.EqualTo("PID001").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(testHeader.GetOrgCusCode(new string[] { OrgCusCode.CodeTypes.CarrierCode, OrgCusCode.TaiwanCodeTypes.MCI }).OK_CustomsRegNo, NUnit.Framework.Is.EqualTo("00612348").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(testHeader.GetOrgCusCode(new string[] { OrgCusCode.CodeTypes.TaxFileCode, OrgCusCode.CodeTypes.CarrierCode, OrgCusCode.TaiwanCodeTypes.MCI }).OK_CustomsRegNo, NUnit.Framework.Is.EqualTo("00612348").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetIDOrgCusCode()
		{
			testHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.PID, "PID001", Core.Constants.CountryCodes.Taiwan);
			NUnit.Framework.Assert.That(testHeader.GetIDOrgCusCode().OK_CustomsRegNo, NUnit.Framework.Is.EqualTo("PID001").Using(CustomComparers.TypeComparison), "IDCode");

			testHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "PAS001", Core.Constants.CountryCodes.Taiwan);
			NUnit.Framework.Assert.That(testHeader.GetIDOrgCusCode().OK_CustomsRegNo, NUnit.Framework.Is.EqualTo("PAS001").Using(CustomComparers.TypeComparison), "IDCode");

			testHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "VAT001", Core.Constants.CountryCodes.Taiwan);
			NUnit.Framework.Assert.That(testHeader.GetIDOrgCusCode().OK_CustomsRegNo, NUnit.Framework.Is.EqualTo("VAT001").Using(CustomComparers.TypeComparison), "IDCode");
		}

		[ExpectNoExceptions]
		public void TestGetLocalProcessorIDOrgCusCode()
		{
			var address = testHeader.MainAddress;
			testHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "PAS001", Core.Constants.CountryCodes.Taiwan);
			NUnit.Framework.Assert.That(testHeader.GetLocalProcessorIDOrgCusCode(address).OK_CustomsRegNo, NUnit.Framework.Is.EqualTo("PAS001").Using(CustomComparers.TypeComparison), "IDCode");

			testHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.PID, "PID001", Core.Constants.CountryCodes.Taiwan);
			NUnit.Framework.Assert.That(testHeader.GetLocalProcessorIDOrgCusCode(address).OK_CustomsRegNo, NUnit.Framework.Is.EqualTo("PID001").Using(CustomComparers.TypeComparison), "IDCode");

			testHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "VAT001", Core.Constants.CountryCodes.Taiwan);
			NUnit.Framework.Assert.That(testHeader.GetLocalProcessorIDOrgCusCode(address).OK_CustomsRegNo, NUnit.Framework.Is.EqualTo("VAT001").Using(CustomComparers.TypeComparison), "IDCode");

			address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber, "FRI001", Core.Constants.CountryCodes.Taiwan);
			NUnit.Framework.Assert.That(testHeader.GetLocalProcessorIDOrgCusCode(address).OK_CustomsRegNo, NUnit.Framework.Is.EqualTo("FRI001").Using(CustomComparers.TypeComparison), "IDCode");
		}

		[ExpectNoExceptions]
		public void TestGetWareHouseOrgCusCode()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var addresse = header.MainAddress;
			addresse.Address1 = "Address1";
			addresse.Address2 = "Address2";

			addresse.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "CCP001", Core.Constants.CountryCodes.Taiwan);
			NUnit.Framework.Assert.That(addresse.GetWareHouseOrgCusCode().OK_CustomsRegNo, NUnit.Framework.Is.EqualTo("CCP001").Using(CustomComparers.TypeComparison));

			addresse.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "CPW001", Core.Constants.CountryCodes.Taiwan);
			NUnit.Framework.Assert.That(addresse.GetWareHouseOrgCusCode().OK_CustomsRegNo, NUnit.Framework.Is.EqualTo("CPW001").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetRegNoWithOrganisation()
		{
			NUnit.Framework.Assert.That(testHeader.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode), NUnit.Framework.Is.EqualTo("00612348").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(testHeader.GetCustomsRegNo(OrgCusCode.TaiwanCodeTypes.AEO).ToString(), NUnit.Framework.Is.Null.Or.Empty, "Should be empty. - should be [null] or [empty]");
			NUnit.Framework.Assert.That(testHeader.GetCustomsRegNo(OrgCusCode.TaiwanCodeTypes.AEO, "AU"), NUnit.Framework.Is.EqualTo("111111").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetChineseName()
		{
			testAddress.OA_CompanyNameOverride = "公司 1";
			var translatedAddress = testAddress.TranslatedAddresses.AddNew();
			translatedAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			translatedAddress.Address1 = "otaaddress 111";
			translatedAddress.CompanyName = "公司 2";
			NUnit.Framework.Assert.That(testAddress.GetChineseName(), NUnit.Framework.Is.EqualTo("公司 2").Using(CustomComparers.TypeComparison));

			testAddress.OA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			NUnit.Framework.Assert.That(testAddress.GetChineseName(), NUnit.Framework.Is.EqualTo("公司 1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestHasAddressOfLanguage()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Language = Core.SharedConstants.Languages.English;
			NUnit.Framework.Assert.That(orgAddress.HasAddressOfLanguage(Core.SharedConstants.Languages.English), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(orgAddress.HasAddressOfLanguage(Core.SharedConstants.Languages.ChineseTraditional), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));

			var chineseTraditionalAddress = orgAddress.TranslatedAddresses.AddNew();
			chineseTraditionalAddress.Language = Core.SharedConstants.Languages.ChineseTraditional;
			chineseTraditionalAddress.OTA_Address1 = "Address";
			NUnit.Framework.Assert.That(orgAddress.HasAddressOfLanguage(Core.SharedConstants.Languages.English), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(orgAddress.HasAddressOfLanguage(Core.SharedConstants.Languages.ChineseTraditional), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestHasCompanyNameOfLanguage()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Language = Core.SharedConstants.Languages.English;
			NUnit.Framework.Assert.That(orgAddress.HasCompanyNameOfLanguage(Core.SharedConstants.Languages.English), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(orgAddress.HasCompanyNameOfLanguage(Core.SharedConstants.Languages.ChineseTraditional), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));

			orgAddress.CompanyName = "Apple";
			NUnit.Framework.Assert.That(orgAddress.HasCompanyNameOfLanguage(Core.SharedConstants.Languages.English), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(orgAddress.HasCompanyNameOfLanguage(Core.SharedConstants.Languages.ChineseTraditional), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));

			var chineseTraditionalAddress = orgAddress.TranslatedAddresses.AddNew();
			chineseTraditionalAddress.Language = Core.SharedConstants.Languages.ChineseTraditional;
			NUnit.Framework.Assert.That(orgAddress.HasCompanyNameOfLanguage(Core.SharedConstants.Languages.English), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(orgAddress.HasCompanyNameOfLanguage(Core.SharedConstants.Languages.ChineseTraditional), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));

			chineseTraditionalAddress.OTA_CompanyName = "頻果";
			NUnit.Framework.Assert.That(orgAddress.HasCompanyNameOfLanguage(Core.SharedConstants.Languages.English), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(orgAddress.HasCompanyNameOfLanguage(Core.SharedConstants.Languages.ChineseTraditional), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetCodeTypeList()
		{
			var factory = new BusinessObjectFactory();
			var codeTypeList = factory.GetCodeTypeList(OrgCusCode.CodeTypes.VATCode, Constants.OrgCusCodeType.CustomCode, Constants.CCPPrefix);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(codeTypeList.GetDescriptionFromCode(OrgCusCode.CodeTypes.VATCode), NUnit.Framework.Is.EqualTo("Government VAT Code"), "VAT Description");
				NUnit.Framework.Assert.That(codeTypeList.GetDescriptionFromCode(Constants.OrgCusCodeType.CustomCode), NUnit.Framework.Is.EqualTo("Custom Code"), "ZZZ Description");
				NUnit.Framework.Assert.That(codeTypeList.GetDescriptionFromCode(Constants.CCPPrefix), NUnit.Framework.Is.EqualTo("Bonded ID for the foreign company"), "FFF Description");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			testHeader = Factory.NewWithValidTestData<OrgHeader>();
			testAddress = testHeader.MainAddress;
			testAddress.OA_Language = Core.SharedConstants.Languages.English;
			testAddress.Address1 = "Address1";
			testAddress.Address2 = "Address2";
			testHeader.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.MCI, "PID001", Core.Constants.CountryCodes.Taiwan);
			testHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "00612348", Core.Constants.CountryCodes.Taiwan);
			testHeader.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.AEO, "111111", Core.Constants.CountryCodes.Australia);

			testAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsCPPermitCode, "212233", Core.Constants.CountryCodes.Taiwan);
			testAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "123", Core.Constants.CountryCodes.Taiwan);
			testAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "456", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
		}

		OrgAddress testAddress;
		OrgHeader testHeader;
	}
}
