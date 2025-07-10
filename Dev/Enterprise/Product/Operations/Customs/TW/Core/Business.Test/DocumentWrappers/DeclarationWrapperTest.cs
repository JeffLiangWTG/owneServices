using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(DocumentWrapperForTesting))]
	sealed class DeclarationWrapperTest : DocumentWrapperTest
	{
		public void TestPortOfOriginName()
		{
			declarationForTest.JE_RL_NKOrigin = "TWTPE";
			AssertEquals("Taiwan - Taipei", DeclarationWrapper.PortOfOriginName);

			declarationForTest.JE_RL_NKOrigin = "TWZ99";
			declarationForTest.JE_Z99PortOfOrigin = "Test From";
			AssertEquals("Taiwan - Test From", DeclarationWrapper.PortOfOriginName);

			declarationForTest.JE_RL_NKOrigin = "AAZ99";
			declarationForTest.JE_Z99PortOfOrigin = "Test From";
			AssertEquals("AAZ99 - Test From", DeclarationWrapper.PortOfOriginName);
		}

		public void TestFinalDestinationName()
		{
			declarationForTest.JE_RL_NKFinalDestination = "TWTPE";
			AssertEquals("Taiwan - Taipei", DeclarationWrapper.FinalDestinationName);

			declarationForTest.JE_RL_NKFinalDestination = "TWZ99";
			declarationForTest.JE_Z99FinalDestination = "Test To";
			AssertEquals("Taiwan - Test To", DeclarationWrapper.FinalDestinationName);

			declarationForTest.JE_RL_NKFinalDestination = "AAZ99";
			declarationForTest.JE_Z99FinalDestination = "Test To";
			AssertEquals("AAZ99 - Test To", DeclarationWrapper.FinalDestinationName);
		}

		public void TestTransportation()
		{
			declarationForTest.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("AIR FREIGHT", DeclarationWrapper.Transportation);

			declarationForTest.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals(declarationForTest.JE_VesselName, DeclarationWrapper.Transportation);
		}

		public void TestGoodsDescription()
		{
			declarationForTest.JE_GoodsDescription = "Test Goods Description";
			AssertEquals("Test Goods Description", DeclarationWrapper.GoodsDescription);
		}

		public void TestPackageDescription()
		{
			declarationForTest.CusEntryInstruction.CEI_PackageDescription = "Test Package Description";
			AssertEquals("Test Package Description", DeclarationWrapper.PackageDescription);
		}

		public void TestSupplierJobDocAddress()
		{
			AssertEquals(declarationForTest.SupplierDocumentaryAddress, DeclarationWrapper.SupplierJobDocAddress.WrappedObject);
		}

		public void TestImporterJobDocAddress()
		{
			AssertEquals(declarationForTest.ImporterDocumentaryAddress, DeclarationWrapper.ImporterJobDocAddress.WrappedObject);
		}

		public void TestSupplierAddressData()
		{
			declarationForTest.JE_TransportMode = TransportTypeList.Codes.Air;
			var supplierOrganization = Factory.New<OrgHeader>();
			supplierOrganization.OH_Code = "org01";
			supplierOrganization.OH_RL_NKClosestPort = "TWKEL";
			supplierOrganization.Addresses.RemoveAndDeleteAll();

			var mainAddress = supplierOrganization.Addresses.MainAddress;
			mainAddress.CompanyName = "Main CompanyName";
			mainAddress.Address1 = "Main Address1";
			mainAddress.Address2 = "Main Address2";
			mainAddress.City = "Main City";
			mainAddress.State = "Main State";
			mainAddress.Postcode = "123";
			mainAddress.Language = "EN-GB";

			var transAddress1 = mainAddress.TranslatedAddresses.AddNew();
			transAddress1.CompanyName = "Trans CompanyName1";
			transAddress1.Address1 = "Trans Address11";
			transAddress1.Address2 = "Trans Address21";
			transAddress1.City = "Trans City1";
			transAddress1.State = "Trans State1";
			transAddress1.Postcode = "456";
			transAddress1.Language = "EN";

			var transAddress2 = mainAddress.TranslatedAddresses.AddNew();
			transAddress2.CompanyName = "Trans CompanyName2";
			transAddress2.Address1 = "Trans Address12";
			transAddress2.Address2 = "Trans Address22";
			transAddress2.City = "Trans City2";
			transAddress2.State = "Trans State2";
			transAddress2.Postcode = "789";
			transAddress2.Language = "EN-US";

			declarationForTest.JE_OH_Supplier = supplierOrganization.PK;
			Factory.Save();

			AssertEquals("MAIN ADDRESS1 MAIN ADDRESS2 MAIN CITY 123 TAIWAN", DeclarationWrapper.SupplierAddressData.Address);
			AssertEquals("Main CompanyName", DeclarationWrapper.SupplierAddressData.CompanyName);

			mainAddress.Language = "ZH-TW";

			AssertEquals("TRANS ADDRESS11 TRANS ADDRESS21 TRANS CITY1 456 TAIWAN", DeclarationWrapper.SupplierAddressData.Address);
			AssertEquals("Trans CompanyName1", DeclarationWrapper.SupplierAddressData.CompanyName);

			transAddress1.Language = "EN-GB";

			AssertEquals("TRANS ADDRESS12 TRANS ADDRESS22 TRANS CITY2 789 TAIWAN", DeclarationWrapper.SupplierAddressData.Address);
			AssertEquals("Trans CompanyName2", DeclarationWrapper.SupplierAddressData.CompanyName);

			var supplierDocumentaryAddress = declarationForTest.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_CompanyName = "XO001";
			supplierDocumentaryAddress.E2_Address1 = "A1";
			supplierDocumentaryAddress.E2_Address2 = "A2";
			supplierDocumentaryAddress.E2_City = "TPE";
			supplierDocumentaryAddress.E2_State = "TPE";
			supplierDocumentaryAddress.E2_RN_NKCountryCode = "TW";
			supplierDocumentaryAddress.E2_Postcode = "105";

			AssertEquals("XO001", DeclarationWrapper.SupplierAddressData.CompanyName);
			AssertEquals("A1 A2 TPE 105 TAIWAN", DeclarationWrapper.SupplierAddressData.Address);

			supplierDocumentaryAddress.E2_CompanyName = ZString.Empty;
			supplierDocumentaryAddress.E2_Address1 = ZString.Empty;
			supplierDocumentaryAddress.E2_Address2 = ZString.Empty;
			supplierDocumentaryAddress.E2_City = ZString.Empty;
			supplierDocumentaryAddress.E2_State = ZString.Empty;
			supplierDocumentaryAddress.E2_RN_NKCountryCode = ZString.Empty;
			supplierDocumentaryAddress.E2_Postcode = ZString.Empty;

			AssertEquals(ZString.Empty, DeclarationWrapper.SupplierAddressData.CompanyName);
			AssertEquals(ZString.Empty, DeclarationWrapper.SupplierAddressData.Address);

			supplierDocumentaryAddress.E2_AddressOverride = false;
			var enAddress = supplierOrganization.Addresses.AddNew();
			enAddress.OA_RN_NKCountryCode = "TW";
			enAddress.OA_CompanyNameOverride = "Importer company name e2";
			enAddress.OA_Language = Core.SharedConstants.Languages.English;
			enAddress.OA_Address1 = "ADDRESS 11";
			enAddress.OA_Address2 = "ADDRESS 22";
			enAddress.OA_PostCode = "106";
			var supplierChineseAddress = enAddress.TranslatedAddresses.AddNew();
			supplierChineseAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			supplierChineseAddress.Address1 = "忠孝東路1";
			supplierChineseAddress.Address2 = "三段232號1";
			supplierChineseAddress.OTA_CompanyName = "公司名稱X1221";
			var supplierEnglishAddress = enAddress.TranslatedAddresses.AddNew();
			supplierEnglishAddress.OTA_Language = Core.SharedConstants.Languages.English;
			supplierEnglishAddress.Address1 = "No. 232, Sec. 5, ZhongXiao N. Rd.,";
			supplierEnglishAddress.Address2 = "Zhongshan Dist., Taipei City 106, Taiwan (R.O.C.)";
			supplierEnglishAddress.OTA_CompanyName = "Importer2 company name(OTA).";
			var cnAddress = supplierOrganization.Addresses.AddNew();
			cnAddress.OA_RN_NKCountryCode = "TW";
			cnAddress.OA_CompanyNameOverride = "Importer company name e3";
			cnAddress.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
			cnAddress.OA_Address1 = "地址1";
			cnAddress.OA_Address2 = "地址2";
			cnAddress.OA_PostCode = "108";
			supplierChineseAddress = cnAddress.TranslatedAddresses.AddNew();
			supplierChineseAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			supplierChineseAddress.Address1 = "忠孝東路1";
			supplierChineseAddress.Address2 = "三段232號1";
			supplierChineseAddress.OTA_CompanyName = "公司名稱X12";
			supplierEnglishAddress = cnAddress.TranslatedAddresses.AddNew();
			supplierEnglishAddress.OTA_Language = Core.SharedConstants.Languages.English;
			supplierEnglishAddress.Address1 = "No. 232, Sec. 8, ZhongXiao N. Rd.,";
			supplierEnglishAddress.Address2 = "Zhongshan Dist., Taipei City 109, Taiwan (R.O.C.)";
			supplierEnglishAddress.OTA_CompanyName = "Importer3 company name(OTA).";
			supplierEnglishAddress.OTA_PostCode = "65";
			supplierDocumentaryAddress.E2_OA_Address = enAddress.PK;
			Factory.Save();

			AssertEquals("Importer company name e2", DeclarationWrapper.SupplierAddressData.CompanyName);
			AssertEquals("ADDRESS 11 ADDRESS 22 106 TAIWAN", DeclarationWrapper.SupplierAddressData.Address);

			supplierDocumentaryAddress.E2_OA_Address = cnAddress.PK;
			Factory.Save();

			AssertEquals("Importer3 company name(OTA).", DeclarationWrapper.SupplierAddressData.CompanyName);
			AssertEquals("NO. 232, SEC. 8, ZHONGXIAO N. RD., ZHONGSHAN DIST., TAIPEI CITY 109, TAIWAN (R.O.C.) 65 TAIWAN", DeclarationWrapper.SupplierAddressData.Address);
		}

		public void TestImporterAddressAndCompany()
		{
			declarationForTest.JE_TransportMode = TransportTypeList.Codes.Air;

			var importerOrganization = Factory.New<OrgHeader>();
			importerOrganization.OH_Code = "org01";
			importerOrganization.OH_RL_NKClosestPort = "TWKEL";
			importerOrganization.Addresses.RemoveAndDeleteAll();

			var mainAddress = importerOrganization.Addresses.MainAddress;
			mainAddress.CompanyName = "Main CompanyName";
			mainAddress.Address1 = "Main Address1";
			mainAddress.Address2 = "Main Address2";
			mainAddress.City = "Main City";
			mainAddress.State = "Main State";
			mainAddress.Postcode = "123";
			mainAddress.Language = "EN-GB";

			var transAddress1 = mainAddress.TranslatedAddresses.AddNew();
			transAddress1.CompanyName = "Trans CompanyName1";
			transAddress1.Address1 = "Trans Address11";
			transAddress1.Address2 = "Trans Address21";
			transAddress1.City = "Trans City1";
			transAddress1.State = "Trans State1";
			transAddress1.Postcode = "456";
			transAddress1.Language = "EN";

			var transAddress2 = mainAddress.TranslatedAddresses.AddNew();
			transAddress2.CompanyName = "Trans CompanyName2";
			transAddress2.Address1 = "Trans Address12";
			transAddress2.Address2 = "Trans Address22";
			transAddress2.City = "Trans City2";
			transAddress2.State = "Trans State2";
			transAddress2.Postcode = "789";
			transAddress2.Language = "EN-US";

			declarationForTest.JE_OH_Importer = importerOrganization.PK;
			Factory.Save();

			AssertEquals("MAIN ADDRESS1 MAIN ADDRESS2 MAIN CITY 123 TAIWAN", DeclarationWrapper.ImporterAddressData.Address);
			AssertEquals("Main CompanyName", DeclarationWrapper.ImporterAddressData.CompanyName);

			mainAddress.Language = "ZH-TW";

			AssertEquals("TRANS ADDRESS11 TRANS ADDRESS21 TRANS CITY1 456 TAIWAN", DeclarationWrapper.ImporterAddressData.Address);
			AssertEquals("Trans CompanyName1", DeclarationWrapper.ImporterAddressData.CompanyName);

			transAddress1.Language = "EN-GB";

			AssertEquals("TRANS ADDRESS12 TRANS ADDRESS22 TRANS CITY2 789 TAIWAN", DeclarationWrapper.ImporterAddressData.Address);
			AssertEquals("Trans CompanyName2", DeclarationWrapper.ImporterAddressData.CompanyName);

			var importerDocumentaryAddress = declarationForTest.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.E2_CompanyName = "XO001";
			importerDocumentaryAddress.E2_Address1 = "A1";
			importerDocumentaryAddress.E2_Address2 = "A2";
			importerDocumentaryAddress.E2_City = "TPE";
			importerDocumentaryAddress.E2_State = "TPE";
			importerDocumentaryAddress.E2_RN_NKCountryCode = "TW";
			importerDocumentaryAddress.E2_Postcode = "105";

			AssertEquals("XO001", DeclarationWrapper.ImporterAddressData.CompanyName);
			AssertEquals("A1 A2 TPE 105 TAIWAN", DeclarationWrapper.ImporterAddressData.Address);

			importerDocumentaryAddress.E2_CompanyName = ZString.Empty;
			importerDocumentaryAddress.E2_Address1 = ZString.Empty;
			importerDocumentaryAddress.E2_Address2 = ZString.Empty;
			importerDocumentaryAddress.E2_City = ZString.Empty;
			importerDocumentaryAddress.E2_State = ZString.Empty;
			importerDocumentaryAddress.E2_RN_NKCountryCode = ZString.Empty;
			importerDocumentaryAddress.E2_Postcode = ZString.Empty;

			AssertEquals(ZString.Empty, DeclarationWrapper.ImporterAddressData.CompanyName);
			AssertEquals(ZString.Empty, DeclarationWrapper.ImporterAddressData.Address);

			importerDocumentaryAddress.E2_AddressOverride = false;
			var enAddress = importerOrganization.Addresses.AddNew();
			enAddress.OA_RN_NKCountryCode = "TW";
			enAddress.OA_CompanyNameOverride = "Importer company name e2.";
			enAddress.OA_Language = Core.SharedConstants.Languages.English;
			enAddress.OA_Address1 = "ADDRESS 11";
			enAddress.OA_Address2 = "ADDRESS 22";
			enAddress.OA_PostCode = "106";
			var importerChineseAddress = enAddress.TranslatedAddresses.AddNew();
			importerChineseAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			importerChineseAddress.Address1 = "忠孝東路1";
			importerChineseAddress.Address2 = "三段232號1";
			importerChineseAddress.CompanyName = "公司名稱X12(OTA)";
			var importerEnglishAddress = enAddress.TranslatedAddresses.AddNew();
			importerEnglishAddress.OTA_Language = Core.SharedConstants.Languages.English;
			importerEnglishAddress.Address1 = "No. 232, Sec. 5, ZhongXiao N. Rd.,";
			importerEnglishAddress.Address2 = "Zhongshan Dist., Taipei City 106, Taiwan (R.O.C.)";
			importerEnglishAddress.CompanyName = "Importer2 company name(OTA).";
			var cnAddress = importerOrganization.Addresses.AddNew();
			cnAddress.OA_RN_NKCountryCode = "TW";
			cnAddress.OA_CompanyNameOverride = "Importer company name e3.";
			cnAddress.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
			cnAddress.OA_Address1 = "地址1";
			cnAddress.OA_Address2 = "地址2";
			cnAddress.OA_PostCode = "108";
			importerChineseAddress = cnAddress.TranslatedAddresses.AddNew();
			importerChineseAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			importerChineseAddress.Address1 = "忠孝東路1";
			importerChineseAddress.Address2 = "三段232號1";
			importerChineseAddress.CompanyName = "公司名稱X12 (OTA)";
			importerEnglishAddress = cnAddress.TranslatedAddresses.AddNew();
			importerEnglishAddress.OTA_Language = Core.SharedConstants.Languages.English;
			importerEnglishAddress.Address1 = "No. 232, Sec. 8, ZhongXiao N. Rd.,";
			importerEnglishAddress.Address2 = "Zhongshan Dist., Taipei City 109, Taiwan (R.O.C.)";
			importerEnglishAddress.CompanyName = "Importer3 company name(OTA).";
			importerEnglishAddress.OTA_PostCode = "65";
			importerDocumentaryAddress.E2_OA_Address = enAddress.PK;
			Factory.Save();

			AssertEquals("Importer company name e2.", DeclarationWrapper.ImporterAddressData.CompanyName);
			AssertEquals("ADDRESS 11 ADDRESS 22 106 TAIWAN", DeclarationWrapper.ImporterAddressData.Address);

			importerDocumentaryAddress.E2_OA_Address = cnAddress.PK;
			Factory.Save();

			AssertEquals("Importer3 company name(OTA).", DeclarationWrapper.ImporterAddressData.CompanyName);
			AssertEquals("NO. 232, SEC. 8, ZHONGXIAO N. RD., ZHONGSHAN DIST., TAIPEI CITY 109, TAIWAN (R.O.C.) 65 TAIWAN", DeclarationWrapper.ImporterAddressData.Address);
		}

		public void TestNotify()
		{
			var notifyOrganization = Factory.New<OrgHeader>();
			declarationForTest.JE_OH_NotifyParty = notifyOrganization.PK;
			AssertEquals(notifyOrganization, ((OrgHeaderSource)DeclarationWrapper.Notify.WrappedObject).WrappedObject);
		}

		public void TestNotifyPartyDetails()
		{
			var notifyOrganization = Factory.New<OrgHeader>();
			var notifyMainAddress = notifyOrganization.MainAddress;
			notifyMainAddress.CompanyName = "AWG Company";
			notifyMainAddress.Address1 = "No. 232, Sec. 8, ZhongXiao N. Rd.,";
			notifyMainAddress.Address2 = "Zhongshan Dist., Taipei City 109, Taiwan (R.O.C.)";
			notifyMainAddress.OA_PostCode = "65";
			notifyMainAddress.OA_RN_NKCountryCode = "TW";
			declarationForTest.JE_OH_NotifyParty = notifyOrganization.PK;
			var notifyPartyDetails = DeclarationWrapper.NotifyPartyDetails;
			AssertEquals("AWG Company", notifyPartyDetails.Name);
			AssertEquals("NO. 232, SEC. 8, ZHONGXIAO N. RD., ZHONGSHAN DIST., TAIPEI CITY 109, TAIWAN (R.O.C.) 65 TAIWAN", notifyPartyDetails.Address.Line);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declarationForTest = Factory.NewWithValidTestData<JobDeclaration>();
		}

		DeclarationWrapper DeclarationWrapper => DeclarationWrapper.New(declarationForTest, Factory);

		JobDeclaration declarationForTest;
	}
}
