using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class BuyerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestFormatAEONumber()
		{
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			importerDocumentaryAddress.AEOCode = "11111111";
			NUnit.Framework.Assert.That(Buyer.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("TWAEO-11111111").Using(CustomComparers.TypeComparison));
			importerDocumentaryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			importerDocumentaryAddress.AEOCode = "11111111";
			NUnit.Framework.Assert.That(Buyer.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("AEOSG11111111").Using(CustomComparers.TypeComparison));
			importerDocumentaryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			importerDocumentaryAddress.AEOCode = "11111111";
			NUnit.Framework.Assert.That(Buyer.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("AEOCN11111111").Using(CustomComparers.TypeComparison));
			importerDocumentaryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Israel;
			importerDocumentaryAddress.AEOCode = "11111111";
			NUnit.Framework.Assert.That(Buyer.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("ILAEO11111111").Using(CustomComparers.TypeComparison));
			importerDocumentaryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			importerDocumentaryAddress.AEOCode = "11111111";
			NUnit.Framework.Assert.That(Buyer.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("KRAEO11111111").Using(CustomComparers.TypeComparison));
			importerDocumentaryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			importerDocumentaryAddress.AEOCode = "11111111";
			NUnit.Framework.Assert.That(Buyer.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("AU11111111").Using(CustomComparers.TypeComparison));
			importerDocumentaryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.India;
			importerDocumentaryAddress.AEOCode = "11111111";
			NUnit.Framework.Assert.That(Buyer.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("IN11111111").Using(CustomComparers.TypeComparison));
			importerDocumentaryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			importerDocumentaryAddress.AEOCode = "11111111";
			NUnit.Framework.Assert.That(Buyer.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("11111111").Using(CustomComparers.TypeComparison));
		}

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
			docAddress.DocAddressType = DocAddressType.ImporterDocumentaryAddress;
			docAddress.OrganisationPK = testOrg.PK;
			IPartyDetails buyer = new Buyer(declaration, testOrg, docAddress);
			NUnit.Framework.Assert.That(buyer.Name, NUnit.Framework.Is.EqualTo("NEN").Using(CustomComparers.TypeComparison));
		}

		#region Properties
		[ExpectNoExceptions]
		public void TestID()
		{
			IPartyDetails buyer;
			CombineAssertions("Buyer ID", () =>
			{
				importer.OH_RL_NKClosestPort = "TWKEL";
				buyer = new Buyer(declaration, importer, importerDocumentaryAddress);
				NUnit.Framework.Assert.That(buyer.ID, NUnit.Framework.Is.EqualTo("I222222").Using(CustomComparers.TypeComparison), "Buyer.ID should be");
				var buyerCustomsCodes = importer.CustomsCodes;
				buyerCustomsCodes.RemoveAndDelete(buyerCustomsCodes.Cast<OrgCusCode>().Single(x => x.OK_CodeType == OrgCusCode.CodeTypes.VATCode));
				buyer = new Buyer(declaration, importer, importerDocumentaryAddress);
				NUnit.Framework.Assert.That(buyer.ID, NUnit.Framework.Is.EqualTo("NOI111111").Using(CustomComparers.TypeComparison), "Buyer.ID should be");
				buyerCustomsCodes.RemoveAndDelete(buyerCustomsCodes.Cast<OrgCusCode>().Single(x => x.OK_CodeType == OrgCusCode.CodeTypes.PassportID));
				buyer = new Buyer(declaration, importer, importerDocumentaryAddress);
				NUnit.Framework.Assert.That(buyer.ID, NUnit.Framework.Is.EqualTo("I333333").Using(CustomComparers.TypeComparison), "Buyer.ID should be");

				importer.OH_RL_NKClosestPort = "CNSHA";
				buyer = new Buyer(declaration, importer, importerDocumentaryAddress);
				NUnit.Framework.Assert.That(buyer.ID, NUnit.Framework.Is.EqualTo("IRCONE").Using(CustomComparers.TypeComparison), "Buyer.ID should be");
				importer.MainAddress.OA_CompanyNameOverride = "Wisetech Global";
				buyer = new Buyer(declaration, importer, importerDocumentaryAddress);
				NUnit.Framework.Assert.That(buyer.ID, NUnit.Framework.Is.EqualTo("WHGL").Using(CustomComparers.TypeComparison), "Buyer.ID should be");
				importer.MainAddress.OA_CompanyNameOverride = "Type A company";
				buyer = new Buyer(declaration, importer, importerDocumentaryAddress);
				NUnit.Framework.Assert.That(buyer.ID, NUnit.Framework.Is.EqualTo("TEAACO").Using(CustomComparers.TypeComparison), "Buyer.ID should be");
				importer.MainAddress.OA_CompanyNameOverride = "Black & Gold Foods";
				buyer = new Buyer(declaration, importer, importerDocumentaryAddress);
				NUnit.Framework.Assert.That(buyer.ID, NUnit.Framework.Is.EqualTo("BKGDFS").Using(CustomComparers.TypeComparison), "Buyer.ID should be");
				importer.MainAddress.OA_CompanyNameOverride = "Ariston Pty. Ltd";
				buyer = new Buyer(declaration, importer, importerDocumentaryAddress);
				NUnit.Framework.Assert.That(buyer.ID, NUnit.Framework.Is.EqualTo("ANPYLD").Using(CustomComparers.TypeComparison), "Buyer.ID should be");

				importer.OH_RL_NKClosestPort = "USLAX";
				importer.MainAddress.State = "CA";
				importer.MainAddress.OA_CompanyNameOverride = "OVERRIDEN COMPANY NAME";
				buyer = new Buyer(declaration, importer, importerDocumentaryAddress);
				NUnit.Framework.Assert.That(buyer.ID, NUnit.Framework.Is.EqualTo("ONCONECA").Using(CustomComparers.TypeComparison), "Buyer.ID should be");
				importer.MainAddress.OA_CompanyNameOverride = "World Trading Company";
				buyer = new Buyer(declaration, importer, importerDocumentaryAddress);
				NUnit.Framework.Assert.That(buyer.ID, NUnit.Framework.Is.EqualTo("WDTGCOCA").Using(CustomComparers.TypeComparison), "Buyer.ID should be");
				importer.MainAddress.OA_CompanyNameOverride = "KYNDRYL INC";
				buyer = new Buyer(declaration, importer, importerDocumentaryAddress);
				NUnit.Framework.Assert.That(buyer.ID, NUnit.Framework.Is.EqualTo("KLIC  CA").Using(CustomComparers.TypeComparison), "Buyer.ID should be");
				importerDocumentaryAddress.E2_AddressOverride = true;
				importerDocumentaryAddress.E2_RN_NKCountryCode = "TW";
				importerDocumentaryAddress.IDCode = "PAS12345";
				buyer = new Buyer(declaration, importer, importerDocumentaryAddress);
				NUnit.Framework.Assert.That(buyer.ID, NUnit.Framework.Is.EqualTo("PAS12345").Using(CustomComparers.TypeComparison), "Buyer.ID should be");
				importerDocumentaryAddress.E2_RN_NKCountryCode = "US";
				importerDocumentaryAddress.E2_State = "CA";
				buyer = new Buyer(declaration, importer, importerDocumentaryAddress);
				NUnit.Framework.Assert.That(buyer.ID, NUnit.Framework.Is.EqualTo("KLIC  CA").Using(CustomComparers.TypeComparison), "Buyer.ID should be");
			});
		}

		[ExpectNoExceptions]
		public void TestName()
		{
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.E2_CompanyName = ZString.Empty;
			NUnit.Framework.Assert.That(Buyer.Name, NUnit.Framework.Is.EqualTo("NEN").Using(CustomComparers.TypeComparison));
			importerDocumentaryAddress.E2_CompanyName = "XO001";
			NUnit.Framework.Assert.That(Buyer.Name, NUnit.Framework.Is.EqualTo("XO001").Using(CustomComparers.TypeComparison));
			importerDocumentaryAddress.E2_AddressOverride = false;
			NUnit.Framework.Assert.That(Buyer.Name, NUnit.Framework.Is.EqualTo("Importer company name xxx.").Using(CustomComparers.TypeComparison));
			var enAddress = importer.Addresses.AddNew();
			enAddress.OA_RN_NKCountryCode = "TW";
			enAddress.OA_CompanyNameOverride = "Importer company name e2.";
			enAddress.OA_Language = Core.SharedConstants.Languages.English;
			enAddress.OA_Address1 = "ADDRESS 11";
			enAddress.OA_Address2 = "ADDRESS 22";
			var importerChineseAddress = enAddress.TranslatedAddresses.AddNew();
			importerChineseAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			importerChineseAddress.Address1 = "忠孝東路1";
			importerChineseAddress.Address2 = "三段232號1";
			importerChineseAddress.OTA_CompanyName = "公司名稱X12(OTA)";
			var importerEnglishAddress = enAddress.TranslatedAddresses.AddNew();
			importerEnglishAddress.OTA_Language = Core.SharedConstants.Languages.English;
			importerEnglishAddress.Address1 = "No. 232, Sec. 5, ZhongXiao N. Rd.,";
			importerEnglishAddress.Address2 = "Zhongshan Dist., Taipei City 106, Taiwan (R.O.C.)";
			importerEnglishAddress.OTA_CompanyName = "Importer3 company name(OTA).";
			var cnAddress = importer.Addresses.AddNew();
			cnAddress.OA_RN_NKCountryCode = "TW";
			cnAddress.OA_CompanyNameOverride = "Importer company name e3.";
			cnAddress.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
			cnAddress.OA_Address1 = "地址1";
			cnAddress.OA_Address2 = "地址2";
			importerChineseAddress = cnAddress.TranslatedAddresses.AddNew();
			importerChineseAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			importerChineseAddress.Address1 = "忠孝東路1";
			importerChineseAddress.Address2 = "三段232號1";
			importerChineseAddress.OTA_CompanyName = "公司名稱X12(OTA)";
			importerEnglishAddress = cnAddress.TranslatedAddresses.AddNew();
			importerEnglishAddress.OTA_Language = Core.SharedConstants.Languages.English;
			importerEnglishAddress.Address1 = "No. 232, Sec. 8, ZhongXiao N. Rd.,";
			importerEnglishAddress.Address2 = "Zhongshan Dist., Taipei City 109, Taiwan (R.O.C.)";
			importerEnglishAddress.OTA_CompanyName = "Importer3 company name(OTA).";
			importerEnglishAddress.Postcode = "65";
			importerDocumentaryAddress.E2_OA_Address = enAddress.PK;
			Factory.Save();
			NUnit.Framework.Assert.That(Buyer.Name, NUnit.Framework.Is.EqualTo("Importer company name e2.").Using(CustomComparers.TypeComparison));
			importerDocumentaryAddress.E2_OA_Address = cnAddress.PK;
			Factory.Save();
			NUnit.Framework.Assert.That(Buyer.Name, NUnit.Framework.Is.EqualTo("Importer3 company name(OTA).").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestChineseName()
		{
			NUnit.Framework.Assert.That(Buyer.ChineseName, NUnit.Framework.Is.EqualTo("公司名稱X1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			NUnit.Framework.Assert.That(Buyer.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison));
			var buyerCustomsCodes = importer.CustomsCodes;
			buyerCustomsCodes.RemoveAndDelete(buyerCustomsCodes.Cast<OrgCusCode>().Single(x => x.OK_CodeType == OrgCusCode.CodeTypes.VATCode));
			NUnit.Framework.Assert.That(Buyer.TypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison));
			buyerCustomsCodes.RemoveAndDelete(buyerCustomsCodes.Cast<OrgCusCode>().Single(x => x.OK_CodeType == OrgCusCode.CodeTypes.PassportID));
			NUnit.Framework.Assert.That(Buyer.TypeCode, NUnit.Framework.Is.EqualTo("174").Using(CustomComparers.TypeComparison));
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
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.CBPCodeType = "EPZ";
			importerDocumentaryAddress.CBPCode = "12345678";
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			IPartyDetails exporter = new Buyer(declaration, org, importerDocumentaryAddress);
			NUnit.Framework.Assert.That(exporter.CustomsControlID, NUnit.Framework.Is.EqualTo("12345678").Using(CustomComparers.TypeComparison));
			importerDocumentaryAddress.E2_AddressOverride = false;
			importerDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			exporter = new Buyer(declaration, org, importerDocumentaryAddress);
			NUnit.Framework.Assert.That(exporter.CustomsControlID, NUnit.Framework.Is.EqualTo("EPZ12345").Using(CustomComparers.TypeComparison));
			org.MainAddress.CustomsCodes.Delete(epzCustomsCode);
			exporter = new Buyer(declaration, org, importerDocumentaryAddress);
			NUnit.Framework.Assert.That(exporter.CustomsControlID, NUnit.Framework.Is.EqualTo("CBF54321").Using(CustomComparers.TypeComparison));
			org.MainAddress.CustomsCodes.Delete(cbfCustomsCode);
			exporter = new Buyer(declaration, org, importerDocumentaryAddress);
			NUnit.Framework.Assert.That(exporter.CustomsControlID, NUnit.Framework.Is.EqualTo("FTZ13579").Using(CustomComparers.TypeComparison));
			org.MainAddress.CustomsCodes.Delete(ftzCustomsCode);
			exporter = new Buyer(declaration, org, importerDocumentaryAddress);
			NUnit.Framework.Assert.That(exporter.CustomsControlID.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestPaymentOnAccountBusinessID()
		{
			NUnit.Framework.Assert.That(Buyer.PaymentOnAccountBusinessID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		public void RoleCode()
		{
			NUnit.Framework.Assert.That(Buyer.RoleCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		public void SubBoxID()
		{
			NUnit.Framework.Assert.That(Buyer.SubBoxID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestCommunications()
		{
			NUnit.Framework.Assert.That(Buyer.Communications, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<ICommunication>)));
		}

		[ExpectNoExceptions]
		public void TestContactName()
		{
			NUnit.Framework.Assert.That(Buyer.ContactName, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestAddressOverride()
		{
			NUnit.Framework.Assert.That(Buyer.ChineseName, NUnit.Framework.Is.EqualTo("公司名稱X1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(Buyer.Address.ChineseLine, NUnit.Framework.Is.EqualTo("忠孝東路三段232號").Using(CustomComparers.TypeComparison));
			importerDocumentaryAddress.E2_AddressOverride = true;
			var localAddress = importerDocumentaryAddress.LocalAddress;
			localAddress.E2_CompanyName = "綠晃科技股份有限公司";
			localAddress.E2_Address1 = "臺北加工出口區園東街6號";
			localAddress.E2_Address2 = string.Empty;
			localAddress.AdditionalAddressInformation = "5樓";
			localAddress.E2_RN_NKCountryCode = "TW";
			localAddress.E2_City = "臺北巿";
			localAddress.E2_Postcode = "90093";
			localAddress.E2_State = "TPE";
			NUnit.Framework.Assert.That(Buyer.ChineseName, NUnit.Framework.Is.EqualTo("綠晃科技股份有限公司").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(Buyer.Address.ChineseLine, NUnit.Framework.Is.EqualTo("90093臺北巿臺北加工出口區園東街6號5樓").Using(CustomComparers.TypeComparison));
		}

		#endregion
		#region IAddress
		[ExpectNoExceptions]
		public void TestLine()
		{
			NUnit.Framework.Assert.That(Buyer.Address.Line, NUnit.Framework.Is.EqualTo("XX231 321XX TAIWAN").Using(CustomComparers.TypeComparison));
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.E2_Address1 = "A1";
			importerDocumentaryAddress.E2_Address2 = "A2";
			importerDocumentaryAddress.E2_City = "TPE";
			importerDocumentaryAddress.E2_State = "TPE";
			importerDocumentaryAddress.E2_RN_NKCountryCode = "TW";
			importerDocumentaryAddress.E2_Postcode = "105";
			NUnit.Framework.Assert.That(Buyer.Address.Line, NUnit.Framework.Is.EqualTo("A1 A2 TPE 105 TAIWAN").Using(CustomComparers.TypeComparison));
			importerDocumentaryAddress.E2_Address1 = ZString.Empty;
			importerDocumentaryAddress.E2_Address2 = ZString.Empty;
			importerDocumentaryAddress.E2_City = ZString.Empty;
			importerDocumentaryAddress.E2_State = ZString.Empty;
			importerDocumentaryAddress.E2_RN_NKCountryCode = ZString.Empty;
			importerDocumentaryAddress.E2_Postcode = ZString.Empty;
			NUnit.Framework.Assert.That(Buyer.Address.Line, NUnit.Framework.Is.EqualTo(ZString.Empty));
			importerDocumentaryAddress.E2_AddressOverride = false;
			var enAddress = importer.Addresses.AddNew();
			enAddress.OA_RN_NKCountryCode = "TW";
			enAddress.OA_CompanyNameOverride = "Importer company name e2(OTA).";
			enAddress.OA_Language = Core.SharedConstants.Languages.English;
			enAddress.OA_Address1 = "ADDRESS 11";
			enAddress.OA_Address2 = "ADDRESS 22";
			enAddress.OA_PostCode = "106";
			var importerChineseAddress = enAddress.TranslatedAddresses.AddNew();
			importerChineseAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			importerChineseAddress.Address1 = "忠孝東路1";
			importerChineseAddress.Address2 = "三段232號1";
			importerChineseAddress.CompanyName = "公司名稱X12";
			var importerEnglishAddress = enAddress.TranslatedAddresses.AddNew();
			importerEnglishAddress.OTA_Language = Core.SharedConstants.Languages.English;
			importerEnglishAddress.Address1 = "No. 232, Sec. 5, ZhongXiao N. Rd.,";
			importerEnglishAddress.Address2 = "Zhongshan Dist., Taipei City 106, Taiwan (R.O.C.)";
			importerEnglishAddress.CompanyName = "Importer2 company name(OTA).";
			var cnAddress = importer.Addresses.AddNew();
			cnAddress.OA_RN_NKCountryCode = "TW";
			cnAddress.OA_CompanyNameOverride = "Importer company name e3(OTA).";
			cnAddress.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
			cnAddress.OA_Address1 = "地址1";
			cnAddress.OA_Address2 = "地址2";
			cnAddress.OA_PostCode = "108";
			importerChineseAddress = cnAddress.TranslatedAddresses.AddNew();
			importerChineseAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			importerChineseAddress.Address1 = "忠孝東路1";
			importerChineseAddress.Address2 = "三段232號1";
			importerChineseAddress.CompanyName = "公司名稱X12";
			importerEnglishAddress = cnAddress.TranslatedAddresses.AddNew();
			importerEnglishAddress.OTA_Language = Core.SharedConstants.Languages.English;
			importerEnglishAddress.Address1 = "No. 232, Sec. 8, ZhongXiao N. Rd.,";
			importerEnglishAddress.Address2 = "Zhongshan Dist., Taipei City 109, Taiwan (R.O.C.)";
			importerEnglishAddress.CompanyName = "Importer3 company name(OTA).";
			importerEnglishAddress.OTA_PostCode = "65";
			importerDocumentaryAddress.E2_OA_Address = enAddress.PK;
			Factory.Save();
			NUnit.Framework.Assert.That(Buyer.Address.Line, NUnit.Framework.Is.EqualTo("ADDRESS 11 ADDRESS 22 106 TAIWAN").Using(CustomComparers.TypeComparison));
			importerDocumentaryAddress.E2_OA_Address = cnAddress.PK;
			Factory.Save();
			NUnit.Framework.Assert.That(Buyer.Address.Line, NUnit.Framework.Is.EqualTo("NO. 232, SEC. 8, ZHONGXIAO N. RD., ZHONGSHAN DIST., TAIPEI CITY 109, TAIWAN (R.O.C.) 65 TAIWAN").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestChineseLine()
		{
			NUnit.Framework.Assert.That(Buyer.Address.ChineseLine, NUnit.Framework.Is.EqualTo("忠孝東路三段232號").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCountryCode()
		{
			NUnit.Framework.Assert.That(Buyer.Address.CountryCode, NUnit.Framework.Is.EqualTo(Core.Constants.CountryCodes.Taiwan).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCountrySubDivisionID()
		{
			NUnit.Framework.Assert.That(Buyer.Address.CountrySubDivisionID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestCountrySubDivisionName()
		{
			NUnit.Framework.Assert.That(Buyer.Address.CountrySubDivisionName, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		#endregion
		#region ILPCOAuthorizedParty
		[ExpectNoExceptions]
		public void TestLPCOAuthorizedPartyID()
		{
			NUnit.Framework.Assert.That(Buyer.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("TWAEO-IAEO001").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLPCOAuthorizedPartyName()
		{
			NUnit.Framework.Assert.That(Buyer.LPCOAuthorizedParty.Name, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestLPCOAuthorizedPartyTypeCode()
		{
			NUnit.Framework.Assert.That(Buyer.LPCOAuthorizedParty.TypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			testHelper = new TestTWCreator(Factory);
			importer = testHelper.CreateOrganizationForImporter();
			entryHeader = testHelper.CreateEntryHeaderForN5203();
			declaration = entryHeader.Declaration;
			declaration.JE_OH_Importer = importer.PK;
			Factory.Save();
			importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.OrganisationPK = importer.PK;
			var test = new JobDocAddressDependentCollection(declaration);
			test.Load();
		}

		TestTWCreator testHelper;
		CusEntryHeader entryHeader;
		IPartyDetails Buyer => new Buyer(declaration, importer, importerDocumentaryAddress);
		OrgHeader importer;
		JobDeclaration declaration;
		TWJobDocAddress importerDocumentaryAddress;
	}
}
