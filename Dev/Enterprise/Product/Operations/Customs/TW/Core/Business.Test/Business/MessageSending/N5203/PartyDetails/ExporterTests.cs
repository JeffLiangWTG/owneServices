using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ExporterTestsTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestDocumentaryAddress()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VGMRegistrationNumber, "BB111", Core.Constants.CountryCodes.Taiwan);
			var testAddress = testOrg.Addresses.AddNew();
			testAddress.OA_Address1 = "test address1";
			testAddress.OA_Address2 = "test address2";
			testAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "00612348", Core.Constants.CountryCodes.Taiwan);
			testAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsCPPermitCode, "212233", Core.Constants.CountryCodes.Taiwan);
			testAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "AA111", Core.Constants.CountryCodes.Taiwan);
			testAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "BB111", Core.Constants.CountryCodes.Taiwan);
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_OA_Warehouse2 = testAddress.PK;
			declaration.JE_OH_Importer = testOrg.PK;
			Factory.Save();
			var docAddress = Factory.New<TWJobDocAddress>();
			docAddress.DocAddressType = DocAddressType.SupplierDocumentaryAddress;
			docAddress.OrganisationPK = testOrg.PK;
			IPartyDetails exporter = new Exporter(declaration, testOrg, docAddress);
			NUnit.Framework.Assert.That(exporter.Name, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		#region Properties
		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(Exporter.ID, NUnit.Framework.Is.EqualTo("6666666").Using(CustomComparers.TypeComparison));
			supplier.CustomsCodes.RemoveAndDelete(supplier.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == OrgCusCode.CodeTypes.VATCode));
			NUnit.Framework.Assert.That(Exporter.ID, NUnit.Framework.Is.EqualTo("NO5555555").Using(CustomComparers.TypeComparison));
			supplier.CustomsCodes.RemoveAndDelete(supplier.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == OrgCusCode.CodeTypes.PassportID));
			NUnit.Framework.Assert.That(Exporter.ID, NUnit.Framework.Is.EqualTo("777777").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestName()
		{
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_CompanyName = ZString.Empty;
			NUnit.Framework.Assert.That(Exporter.Name, NUnit.Framework.Is.EqualTo(ZString.Empty));
			supplierDocumentaryAddress.E2_CompanyName = "XO001";
			NUnit.Framework.Assert.That(Exporter.Name, NUnit.Framework.Is.EqualTo("XO001").Using(CustomComparers.TypeComparison));
			supplierDocumentaryAddress.E2_AddressOverride = false;
			NUnit.Framework.Assert.That(Exporter.Name, NUnit.Framework.Is.EqualTo("Supplier company name.").Using(CustomComparers.TypeComparison));
			var enAddress = supplier.Addresses.AddNew();
			enAddress.OA_RN_NKCountryCode = "TW";
			enAddress.OA_CompanyNameOverride = "Supplier company name e2.";
			enAddress.OA_Language = Core.SharedConstants.Languages.English;
			enAddress.OA_Address1 = "ADDRESS 11";
			enAddress.OA_Address2 = "ADDRESS 22";
			var supplierChineseAddress = enAddress.TranslatedAddresses.AddNew();
			supplierChineseAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			supplierChineseAddress.Address1 = "忠孝東路1";
			supplierChineseAddress.Address2 = "三段232號1";
			supplierChineseAddress.OTA_CompanyName = "公司名稱X12(OTA)";
			var supplierEnglishAddress = enAddress.TranslatedAddresses.AddNew();
			supplierEnglishAddress.OTA_Language = Core.SharedConstants.Languages.English;
			supplierEnglishAddress.Address1 = "No. 232, Sec. 5, ZhongXiao N. Rd.,";
			supplierEnglishAddress.Address2 = "Zhongshan Dist., Taipei City 106, Taiwan (R.O.C.)";
			supplierEnglishAddress.OTA_CompanyName = "Supplier3 company name(OTA).";
			var cnAddress = supplier.Addresses.AddNew();
			cnAddress.OA_RN_NKCountryCode = "TW";
			cnAddress.OA_CompanyNameOverride = "Supplier company name e3.";
			cnAddress.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
			cnAddress.OA_Address1 = "地址1";
			cnAddress.OA_Address2 = "地址2";
			supplierChineseAddress = cnAddress.TranslatedAddresses.AddNew();
			supplierChineseAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			supplierChineseAddress.Address1 = "忠孝東路1";
			supplierChineseAddress.Address2 = "三段232號1";
			supplierChineseAddress.OTA_CompanyName = "公司名稱X12(OTA)";
			supplierEnglishAddress = cnAddress.TranslatedAddresses.AddNew();
			supplierEnglishAddress.OTA_Language = Core.SharedConstants.Languages.English;
			supplierEnglishAddress.Address1 = "No. 232, Sec. 8, ZhongXiao N. Rd.,";
			supplierEnglishAddress.Address2 = "Zhongshan Dist., Taipei City 109, Taiwan (R.O.C.)";
			supplierEnglishAddress.OTA_CompanyName = "Supplier3 company name(OTA).";
			supplierEnglishAddress.Postcode = "65";
			supplierDocumentaryAddress.E2_OA_Address = enAddress.PK;
			Factory.Save();
			NUnit.Framework.Assert.That(Exporter.Name, NUnit.Framework.Is.EqualTo("Supplier company name e2.").Using(CustomComparers.TypeComparison));
			supplierDocumentaryAddress.E2_OA_Address = cnAddress.PK;
			Factory.Save();
			NUnit.Framework.Assert.That(Exporter.Name, NUnit.Framework.Is.EqualTo("Supplier3 company name(OTA).").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestChineseName()
		{
			NUnit.Framework.Assert.That(Exporter.ChineseName, NUnit.Framework.Is.EqualTo("TW Supplier company name").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			NUnit.Framework.Assert.That(Exporter.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison));
			supplier.CustomsCodes.RemoveAndDelete(supplier.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == OrgCusCode.CodeTypes.VATCode));
			NUnit.Framework.Assert.That(Exporter.TypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison));
			supplier.CustomsCodes.RemoveAndDelete(supplier.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == OrgCusCode.CodeTypes.PassportID));
			NUnit.Framework.Assert.That(Exporter.TypeCode, NUnit.Framework.Is.EqualTo("174").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCustomsControlID()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "Org1";
			org.OH_RL_NKClosestPort = "TW";
			var epzCustomsCode = org.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.EPZ, "EPZ12345", "TW");
			var cbfCustomsCode = org.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.CBF, "CBF54321", "TW");
			var ftzCustomsCode = org.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.FTZ, "FTZ13579", "TW");
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.CBPCodeType = "EPZ";
			supplierDocumentaryAddress.CBPCode = "12345678";
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			IPartyDetails exporter = new Exporter(declaration, org, supplierDocumentaryAddress);
			NUnit.Framework.Assert.That(exporter.CustomsControlID, NUnit.Framework.Is.EqualTo("12345678").Using(CustomComparers.TypeComparison));
			supplierDocumentaryAddress.E2_AddressOverride = false;
			supplierDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			exporter = new Exporter(declaration, org, supplierDocumentaryAddress);
			NUnit.Framework.Assert.That(exporter.CustomsControlID, NUnit.Framework.Is.EqualTo("EPZ12345").Using(CustomComparers.TypeComparison));
			org.MainAddress.CustomsCodes.Delete(epzCustomsCode);
			exporter = new Exporter(declaration, org, supplierDocumentaryAddress);
			NUnit.Framework.Assert.That(exporter.CustomsControlID, NUnit.Framework.Is.EqualTo("CBF54321").Using(CustomComparers.TypeComparison));
			org.MainAddress.CustomsCodes.Delete(cbfCustomsCode);
			exporter = new Exporter(declaration, org, supplierDocumentaryAddress);
			NUnit.Framework.Assert.That(exporter.CustomsControlID, NUnit.Framework.Is.EqualTo("FTZ13579").Using(CustomComparers.TypeComparison));
			org.MainAddress.CustomsCodes.Delete(ftzCustomsCode);
			exporter = new Exporter(declaration, org, supplierDocumentaryAddress);
			NUnit.Framework.Assert.That(exporter.CustomsControlID.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestPaymentOnAccountBusinessID()
		{
			NUnit.Framework.Assert.That(Exporter.PaymentOnAccountBusinessID, NUnit.Framework.Is.EqualTo("888888").Using(CustomComparers.TypeComparison));
		}

		public void RoleCode()
		{
			NUnit.Framework.Assert.That(Exporter.RoleCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		public void SubBoxID()
		{
			NUnit.Framework.Assert.That(Exporter.SubBoxID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestCommunications()
		{
			var exporter = Exporter;
			NUnit.Framework.Assert.That(exporter.Communications.ElementAt(0).ID, NUnit.Framework.Is.EqualTo("PHONE").Using(CustomComparers.TypeComparison), "Exporter.Communications[0].ID should be");
			NUnit.Framework.Assert.That(exporter.Communications.ElementAt(0).TypeID, NUnit.Framework.Is.EqualTo("TE").Using(CustomComparers.TypeComparison), "Exporter.Communications[0].TypeID should be");
			NUnit.Framework.Assert.That(exporter.Communications.ElementAt(1).ID, NUnit.Framework.Is.EqualTo("EMAIL").Using(CustomComparers.TypeComparison), "Exporter.Communications[1].ID should be");
			NUnit.Framework.Assert.That(exporter.Communications.ElementAt(1).TypeID, NUnit.Framework.Is.EqualTo("MA").Using(CustomComparers.TypeComparison), "Exporter.Communications[1].TypeID should be");
		}

		[ExpectNoExceptions]
		public void TestContactName()
		{
			NUnit.Framework.Assert.That(Exporter.ContactName, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestAddressOverride()
		{
			NUnit.Framework.Assert.That(Exporter.ChineseName, NUnit.Framework.Is.EqualTo("TW Supplier company name").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(Exporter.Address.ChineseLine, NUnit.Framework.Is.EqualTo("TW Taipei Minsheng E.Rd.TW Taipei Minsheng W.Rd.").Using(CustomComparers.TypeComparison));
			supplierDocumentaryAddress.E2_AddressOverride = true;
			var localAddress = supplierDocumentaryAddress.LocalAddress;
			localAddress.E2_CompanyName = "綠晃科技股份有限公司";
			localAddress.E2_Address1 = "臺北加工出口區園東街6號";
			localAddress.E2_Address2 = string.Empty;
			localAddress.AdditionalAddressInformation = "5樓";
			localAddress.E2_RN_NKCountryCode = "TW";
			localAddress.E2_City = "臺北巿";
			localAddress.E2_Postcode = "90093";
			localAddress.E2_State = "TPE";
			NUnit.Framework.Assert.That(Exporter.ChineseName, NUnit.Framework.Is.EqualTo("綠晃科技股份有限公司").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(Exporter.Address.ChineseLine, NUnit.Framework.Is.EqualTo("90093臺北巿臺北加工出口區園東街6號5樓").Using(CustomComparers.TypeComparison));
		}

		#endregion
		#region IAddress
		[ExpectNoExceptions]
		public void TestLine()
		{
			NUnit.Framework.Assert.That(Exporter.Address.Line, NUnit.Framework.Is.EqualTo("88899 ADDRESS LINE1. 88899 ADDRESS LINE2. TAIWAN").Using(CustomComparers.TypeComparison));
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_Address1 = "A1";
			supplierDocumentaryAddress.E2_Address2 = "A2";
			supplierDocumentaryAddress.E2_City = "TPE";
			supplierDocumentaryAddress.E2_State = "TPE";
			supplierDocumentaryAddress.E2_RN_NKCountryCode = "TW";
			supplierDocumentaryAddress.E2_Postcode = "105";
			NUnit.Framework.Assert.That(Exporter.Address.Line, NUnit.Framework.Is.EqualTo("A1 A2 TPE 105 TAIWAN").Using(CustomComparers.TypeComparison));
			supplierDocumentaryAddress.E2_Address1 = ZString.Empty;
			supplierDocumentaryAddress.E2_Address2 = ZString.Empty;
			supplierDocumentaryAddress.E2_City = ZString.Empty;
			supplierDocumentaryAddress.E2_State = ZString.Empty;
			supplierDocumentaryAddress.E2_RN_NKCountryCode = ZString.Empty;
			supplierDocumentaryAddress.E2_Postcode = ZString.Empty;
			NUnit.Framework.Assert.That(Exporter.Address.Line, NUnit.Framework.Is.EqualTo(ZString.Empty));
			supplierDocumentaryAddress.E2_AddressOverride = false;
			var enAddress = supplier.Addresses.AddNew();
			enAddress.OA_RN_NKCountryCode = "TW";
			enAddress.OA_CompanyNameOverride = "Importer company name e2(OTA).";
			enAddress.OA_Language = Core.SharedConstants.Languages.English;
			enAddress.OA_Address1 = "ADDRESS 11";
			enAddress.OA_Address2 = "ADDRESS 22";
			enAddress.OA_PostCode = "106";
			var supplierChineseAddress = enAddress.TranslatedAddresses.AddNew();
			supplierChineseAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			supplierChineseAddress.Address1 = "忠孝東路1";
			supplierChineseAddress.Address2 = "三段232號1";
			supplierChineseAddress.CompanyName = "公司名稱X12";
			var supplierEnglishAddress = enAddress.TranslatedAddresses.AddNew();
			supplierEnglishAddress.OTA_Language = Core.SharedConstants.Languages.English;
			supplierEnglishAddress.Address1 = "No. 232, Sec. 5, ZhongXiao N. Rd.,";
			supplierEnglishAddress.Address2 = "Zhongshan Dist., Taipei City 106, Taiwan (R.O.C.)";
			supplierEnglishAddress.CompanyName = "Importer2 company name(OTA).";
			var cnAddress = supplier.Addresses.AddNew();
			cnAddress.OA_RN_NKCountryCode = "TW";
			cnAddress.OA_CompanyNameOverride = "Importer company name e3(OTA).";
			cnAddress.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
			cnAddress.OA_Address1 = "地址1";
			cnAddress.OA_Address2 = "地址2";
			cnAddress.OA_PostCode = "108";
			supplierChineseAddress = cnAddress.TranslatedAddresses.AddNew();
			supplierChineseAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			supplierChineseAddress.Address1 = "忠孝東路1";
			supplierChineseAddress.Address2 = "三段232號1";
			supplierChineseAddress.CompanyName = "公司名稱X12";
			supplierEnglishAddress = cnAddress.TranslatedAddresses.AddNew();
			supplierEnglishAddress.OTA_Language = Core.SharedConstants.Languages.English;
			supplierEnglishAddress.Address1 = "No. 232, Sec. 8, ZhongXiao N. Rd.,";
			supplierEnglishAddress.Address2 = "Zhongshan Dist., Taipei City 109, Taiwan (R.O.C.)";
			supplierEnglishAddress.CompanyName = "Importer3 company name(OTA).";
			supplierEnglishAddress.OTA_PostCode = "65";
			supplierDocumentaryAddress.E2_OA_Address = enAddress.PK;
			Factory.Save();
			NUnit.Framework.Assert.That(Exporter.Address.Line, NUnit.Framework.Is.EqualTo("ADDRESS 11 ADDRESS 22 106 TAIWAN").Using(CustomComparers.TypeComparison));
			supplierDocumentaryAddress.E2_OA_Address = cnAddress.PK;
			Factory.Save();
			NUnit.Framework.Assert.That(Exporter.Address.Line, NUnit.Framework.Is.EqualTo("NO. 232, SEC. 8, ZHONGXIAO N. RD., ZHONGSHAN DIST., TAIPEI CITY 109, TAIWAN (R.O.C.) 65 TAIWAN").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestChineseLine()
		{
			NUnit.Framework.Assert.That(Exporter.Address.ChineseLine, NUnit.Framework.Is.EqualTo("TW Taipei Minsheng E.Rd.TW Taipei Minsheng W.Rd.").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCountryCode()
		{
			NUnit.Framework.Assert.That(Exporter.Address.CountryCode, NUnit.Framework.Is.EqualTo("TW").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCountrySubDivisionID()
		{
			NUnit.Framework.Assert.That(Exporter.Address.CountrySubDivisionID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestCountrySubDivisionName()
		{
			NUnit.Framework.Assert.That(Exporter.Address.CountrySubDivisionName, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		#endregion
		#region ILPCOAuthorizedParty
		[ExpectNoExceptions]
		public void TestLPCOAuthorizedPartyID()
		{
			NUnit.Framework.Assert.That(Exporter.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("TWAEO-999999").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLPCOAuthorizedPartyName()
		{
			NUnit.Framework.Assert.That(Exporter.LPCOAuthorizedParty.Name, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestLPCOAuthorizedPartyTypeCode()
		{
			NUnit.Framework.Assert.That(Exporter.LPCOAuthorizedParty.TypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		#endregion
		protected override void SetUp()
		{
			base.SetUp();
			testHelper = new TestTWCreator(Factory);
			supplier = testHelper.CreateOrganizationForSupplier();
			entryHeader = testHelper.CreateEntryHeaderForN5203();
			declaration = entryHeader.Declaration;
			declaration.JE_OH_Supplier = supplier.PK;
			Factory.Save();
			supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.OrganisationPK = supplier.PK;
		}

		TestTWCreator testHelper;
		CusEntryHeader entryHeader;
		OrgHeader supplier;
		JobDeclaration declaration;
		IPartyDetails Exporter => new Exporter(declaration, supplier, supplierDocumentaryAddress);
		TWJobDocAddress supplierDocumentaryAddress;
	}
}
