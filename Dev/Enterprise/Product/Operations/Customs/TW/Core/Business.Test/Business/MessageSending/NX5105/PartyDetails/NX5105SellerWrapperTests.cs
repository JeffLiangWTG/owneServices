using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105SellerWrapperTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestFormatAEONumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var docAddress = declaration.SupplierDocumentaryAddress;
			docAddress.E2_AddressOverride = true;
			docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			docAddress.AEOCode = "11111111";
			IPartyDetails seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
			NUnit.Framework.Assert.That(seller.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("TWAEO-11111111").Using(CustomComparers.TypeComparison));
			docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			docAddress.AEOCode = "11111111";
			seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
			NUnit.Framework.Assert.That(seller.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("AEOSG11111111").Using(CustomComparers.TypeComparison));
			docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			docAddress.AEOCode = "11111111";
			seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
			NUnit.Framework.Assert.That(seller.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("AEOCN11111111").Using(CustomComparers.TypeComparison));
			docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Israel;
			docAddress.AEOCode = "11111111";
			seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
			NUnit.Framework.Assert.That(seller.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("ILAEO11111111").Using(CustomComparers.TypeComparison));
			docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			docAddress.AEOCode = "11111111";
			seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
			NUnit.Framework.Assert.That(seller.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("KRAEO11111111").Using(CustomComparers.TypeComparison));
			docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			docAddress.AEOCode = "11111111";
			seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
			NUnit.Framework.Assert.That(seller.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("AU11111111").Using(CustomComparers.TypeComparison));
			docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.India;
			docAddress.AEOCode = "11111111";
			seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
			NUnit.Framework.Assert.That(seller.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("IN11111111").Using(CustomComparers.TypeComparison));
			docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			docAddress.AEOCode = "11111111";
			seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
			NUnit.Framework.Assert.That(seller.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("11111111").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSellerWrapper()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var docAddress = declaration.SupplierDocumentaryAddress;
			IPartyDetails seller;
			CombineAssertions("Seller ID", () =>
			{
				docAddress.OrganisationPK = organization1.PK;
				organization1.OH_RL_NKClosestPort = "TWKEL";
				seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
				NUnit.Framework.Assert.That(seller.ID, NUnit.Framework.Is.EqualTo("123465789").Using(CustomComparers.TypeComparison), "Seller.ID should be");
				organization1.OH_RL_NKClosestPort = "CNSHA";
				seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
				NUnit.Framework.Assert.That(seller.ID, NUnit.Framework.Is.EqualTo("ONCONE").Using(CustomComparers.TypeComparison), "Seller.ID should be");
				organization1.MainAddress.OA_CompanyNameOverride = "Wisetech Global";
				seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
				NUnit.Framework.Assert.That(seller.ID, NUnit.Framework.Is.EqualTo("WHGL").Using(CustomComparers.TypeComparison), "Seller.ID should be");
				organization1.MainAddress.OA_CompanyNameOverride = "Type A company";
				seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
				NUnit.Framework.Assert.That(seller.ID, NUnit.Framework.Is.EqualTo("TEAACO").Using(CustomComparers.TypeComparison), "Seller.ID should be");
				organization1.MainAddress.OA_CompanyNameOverride = "Black & Gold Foods";
				seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
				NUnit.Framework.Assert.That(seller.ID, NUnit.Framework.Is.EqualTo("BKGDFS").Using(CustomComparers.TypeComparison), "Seller.ID should be");
				organization1.MainAddress.OA_CompanyNameOverride = "Ariston Pty. Ltd";
				seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
				NUnit.Framework.Assert.That(seller.ID, NUnit.Framework.Is.EqualTo("ANPYLD").Using(CustomComparers.TypeComparison), "Seller.ID should be");
				organization1.OH_RL_NKClosestPort = "USLAX";
				organization1.MainAddress.State = "CA";
				organization1.MainAddress.OA_CompanyNameOverride = "OVERRIDEN COMPANY NAME";
				seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
				NUnit.Framework.Assert.That(seller.ID, NUnit.Framework.Is.EqualTo("ONCONECA").Using(CustomComparers.TypeComparison), "Seller.ID should be");
				organization1.MainAddress.OA_CompanyNameOverride = "World Trading Company";
				seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
				NUnit.Framework.Assert.That(seller.ID, NUnit.Framework.Is.EqualTo("WDTGCOCA").Using(CustomComparers.TypeComparison), "Seller.ID should be");
				organization1.MainAddress.OA_CompanyNameOverride = "KYNDRYL INC";
				seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
				NUnit.Framework.Assert.That(seller.ID, NUnit.Framework.Is.EqualTo("KLIC  CA").Using(CustomComparers.TypeComparison), "Seller.ID should be");
				docAddress.E2_AddressOverride = true;
				docAddress.IDCodeType = "PAS";
				docAddress.E2_RN_NKCountryCode = "TW";
				docAddress.IDCode = "PAS12345";
				seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
				NUnit.Framework.Assert.That(seller.ID, NUnit.Framework.Is.EqualTo("NOPAS12345").Using(CustomComparers.TypeComparison), "Seller.ID should be");
				docAddress.E2_RN_NKCountryCode = "US";
				docAddress.E2_State = "CA";
				seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
				NUnit.Framework.Assert.That(seller.ID, NUnit.Framework.Is.EqualTo("KLIC  CA").Using(CustomComparers.TypeComparison), "Seller.ID should be");
			}

			);
			docAddress.E2_AddressOverride = false;
			seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
			organization1.MainAddress.OA_CompanyNameOverride = "OVERRIDEN COMPANY NAME";
			NUnit.Framework.Assert.That(seller.Name, NUnit.Framework.Is.EqualTo("OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison), "Seller.Name should be");
			NUnit.Framework.Assert.That(seller.ChineseName, NUnit.Framework.Is.EqualTo("TW OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison), "Seller.ChineseName should be");
			organization1.MainAddress.OA_CompanyNameOverride = ZString.Empty;
			NUnit.Framework.Assert.That(seller.Name, NUnit.Framework.Is.EqualTo("NEN").Using(CustomComparers.TypeComparison), "Seller.Name should be");
			NUnit.Framework.Assert.That(seller.CustomsControlID, NUnit.Framework.Is.EqualTo("CUSCONID").Using(CustomComparers.TypeComparison), "Seller.CustomsControlID should be");
			entryInstruction.CEI_Style = "F5";
			organization1.OH_RL_NKClosestPort = "TW";
			seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
			NUnit.Framework.Assert.That(seller.CustomsControlID, NUnit.Framework.Is.EqualTo("CUSCONID").Using(CustomComparers.TypeComparison), "Seller.CustomsControlID should be");
			new TestTWCreator(Factory).CreateTPEState();
			organization1.MainAddress.State = "TPE";
			NUnit.Framework.Assert.That(seller.Address.CountryCode, NUnit.Framework.Is.EqualTo("TW").Using(CustomComparers.TypeComparison), "Seller.Address.CountryCode should be");
			NUnit.Framework.Assert.That(seller.Address.Line, NUnit.Framework.Is.EqualTo("ADDRESS 1 ADDRESS 2 TAIWAN").Using(CustomComparers.TypeComparison), "Seller.Address.Line should be");
			NUnit.Framework.Assert.That(seller.Address.ChineseLine, NUnit.Framework.Is.EqualTo("TW OTA ADDRESS 1TW OTA ADDRESS 2").Using(CustomComparers.TypeComparison), "Seller.Address.ChineseLine should be");
			NUnit.Framework.Assert.That(seller.Communications.ElementAt(0).ID, NUnit.Framework.Is.EqualTo("1234567891").Using(CustomComparers.TypeComparison), "Seller.Communications[0].ID should be");
			NUnit.Framework.Assert.That(seller.Communications.ElementAt(0).TypeID, NUnit.Framework.Is.EqualTo("TE").Using(CustomComparers.TypeComparison), "Seller.Communications[0].TypeID should be");
			docAddress.E2_Contact = "Glendy Guan";
			NUnit.Framework.Assert.That(seller.ContactName, NUnit.Framework.Is.EqualTo("Glendy Guan").Using(CustomComparers.TypeComparison), "Seller.ContactName should be");
			NUnit.Framework.Assert.That(seller.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("TWAEO-123465789").Using(CustomComparers.TypeComparison), "Seller.LPCOAuthorizedParty.ID should be");

			CombineAssertions("Seller TypeCode", () =>
			{
				NUnit.Framework.Assert.That(seller.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison), "Seller.TypeCode should be");
				organization1.MainAddress.CustomsCodes.Delete(vATCusCode);
				seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
				NUnit.Framework.Assert.That(seller.TypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison), "Seller.TypeCode should be");
				organization1.MainAddress.CustomsCodes.Delete(pASCusCode);
				seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
				NUnit.Framework.Assert.That(seller.TypeCode, NUnit.Framework.Is.EqualTo("174").Using(CustomComparers.TypeComparison), "Seller.TypeCode should be");
				organization1.MainAddress.CustomsCodes.Delete(pIDCusCode);
				seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
				docAddress.E2_AddressOverride = true;
				docAddress.IDCodeType = "ZZZ";
				NUnit.Framework.Assert.That(seller.TypeCode, NUnit.Framework.Is.EqualTo("ZZZ").Using(CustomComparers.TypeComparison), "Seller.TypeCode should be");
				docAddress.IDCodeType = "VAT";
				NUnit.Framework.Assert.That(seller.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison), "Seller.TypeCode should be");
				docAddress.IDCodeType = "PAS";
				NUnit.Framework.Assert.That(seller.TypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison), "Seller.TypeCode should be");
				docAddress.IDCodeType = "PID";
				NUnit.Framework.Assert.That(seller.TypeCode, NUnit.Framework.Is.EqualTo("174").Using(CustomComparers.TypeComparison), "Seller.TypeCode should be");
			}

			);
			CombineAssertions("Supplier Override", () =>
			{
				docAddress.E2_AddressOverride = true;
				docAddress.Address1 = "SUPPLIER ADDRESS1";
				docAddress.Address2 = "SUPPLIER ADDRESS2";
				docAddress.AEOCode = "987654321";
				docAddress.IDCodeType = "PAS";
				docAddress.E2_Phone = "0222221234";
				docAddress.E2_Contact = "Paul Lin";
				var supplierTranslatedDocumentaryAddress = docAddress.LocalAddress;
				supplierTranslatedDocumentaryAddress.E2_Address1 = "臺北加工出口區園東街6號";
				seller = new NX5105SellerWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
				NUnit.Framework.Assert.That(seller.Address.Line, NUnit.Framework.Is.EqualTo("SUPPLIER ADDRESS1 SUPPLIER ADDRESS2 TAIWAN").Using(CustomComparers.TypeComparison), "Seller.Address.Line should be");
				NUnit.Framework.Assert.That(seller.Address.ChineseLine, NUnit.Framework.Is.EqualTo("臺北加工出口區園東街6號TW OTA ADDRESS 2").Using(CustomComparers.TypeComparison), "Seller.Address.ChineseLine should be");
				NUnit.Framework.Assert.That(seller.TypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison), "Seller.TypeCode should be");
				NUnit.Framework.Assert.That(seller.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("TWAEO-987654321").Using(CustomComparers.TypeComparison), "Seller.LPCOAuthorizedParty.ID.Line should be");
				NUnit.Framework.Assert.That(seller.Communications.ElementAt(0).ID, NUnit.Framework.Is.EqualTo("0222221234").Using(CustomComparers.TypeComparison), "Seller.Communications[0].ID should be");
				NUnit.Framework.Assert.That(seller.Communications.ElementAt(0).TypeID, NUnit.Framework.Is.EqualTo("TE").Using(CustomComparers.TypeComparison), "Seller.Communications[0].TypeID should be");
				NUnit.Framework.Assert.That(seller.ContactName, NUnit.Framework.Is.EqualTo("Paul Lin").Using(CustomComparers.TypeComparison), "Seller.ContactName should be");
			}

			);
		}

		OrgHeader organization1;
		OrgCusCode vATCusCode;
		OrgCusCode cCPCusCode;
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
			var contact = organization1.Contacts.AddNew();
			contact.OC_ContactName = "Contact Name";
			var address1 = organization1.Addresses[0];
			address1.OA_RN_NKCountryCode = "TW";
			address1.OA_CompanyNameOverride = "OVERRIDEN COMPANY NAME";
			address1.OA_Language = "EN";
			address1.OA_Address1 = "ADDRESS 1";
			address1.OA_Address2 = "ADDRESS 2";
			address1.OA_Phone = "1234567891";
			var entranslatedAddress1 = address1.TranslatedAddresses.AddNew();
			entranslatedAddress1.OTA_Language = "EN";
			entranslatedAddress1.OTA_Address1 = "EN OTA ADDRESS 1";
			entranslatedAddress1.OTA_Address2 = "EN OTA ADDRESS 2";
			var zhTWtranslatedAddress1 = address1.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress1.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			zhTWtranslatedAddress1.OTA_CompanyName = "TW OVERRIDEN COMPANY NAME";
			zhTWtranslatedAddress1.OTA_Address1 = "TW OTA ADDRESS 1";
			zhTWtranslatedAddress1.OTA_Address2 = "TW OTA ADDRESS 2";
			organization1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("AEO", "123465789", "TW");
			vATCusCode = organization1.CustomsCodes.AddNew();
			vATCusCode.OK_RN_NKCodeCountry = "TW";
			vATCusCode.OK_CodeType = "VAT";
			vATCusCode.OK_CustomsRegNo = "123465789";
			cCPCusCode = address1.CustomsCodes.AddNew();
			cCPCusCode.OK_RN_NKCodeCountry = "TW";
			cCPCusCode.OK_CodeType = "CCP";
			cCPCusCode.OK_CustomsRegNo = "987654321";
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
