using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105ImporterWrapperTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestImporterWrapper()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			organization1.OH_RL_NKClosestPort = "TW";
			var docAddress = declaration.ImporterDocumentaryAddress;
			docAddress.OrganisationPK = organization1.PK;
			IPartyDetails importer = new NX5105ImporterWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
			NUnit.Framework.Assert.That(importer.ID, NUnit.Framework.Is.EqualTo("123465789").Using(CustomComparers.TypeComparison), "Importer.ID should be");
			NUnit.Framework.Assert.That(importer.Name, NUnit.Framework.Is.EqualTo("OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison), "Importer.Name should be");
			organization1.MainAddress.OA_CompanyNameOverride = ZString.Empty;
			NUnit.Framework.Assert.That(importer.Name, NUnit.Framework.Is.EqualTo("NEN").Using(CustomComparers.TypeComparison), "Importer.Name should be");
			organization1.MainAddress.OA_CompanyNameOverride = "OVERRIDEN COMPANY NAME";
			NUnit.Framework.Assert.That(importer.ChineseName, NUnit.Framework.Is.EqualTo("TW OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison), "Importer.ChineseName should be");
			NUnit.Framework.Assert.That(importer.CustomsControlID, NUnit.Framework.Is.EqualTo("CUSCONID").Using(CustomComparers.TypeComparison), "Importer.CustomsControlID should be");
			entryInstruction.CEI_Style = "D7";
			importer = new NX5105ImporterWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
			NUnit.Framework.Assert.That(importer.CustomsControlID, NUnit.Framework.Is.EqualTo("CUSCONID").Using(CustomComparers.TypeComparison), "Importer.CustomsControlID should be");
			NUnit.Framework.Assert.That(importer.PaymentOnAccountBusinessID, NUnit.Framework.Is.EqualTo("TPCREGNO").Using(CustomComparers.TypeComparison), "Importer.PaymentOnAccountBusinessID should be");
			NUnit.Framework.Assert.That(importer.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison), "Importer.TypeCode should be");
			organization1.MainAddress.CustomsCodes.Delete(vATCusCode);
			importer = new NX5105ImporterWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
			NUnit.Framework.Assert.That(importer.TypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison), "Importer.TypeCode should be");
			organization1.MainAddress.CustomsCodes.Delete(pASCusCode);
			importer = new NX5105ImporterWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
			NUnit.Framework.Assert.That(importer.TypeCode, NUnit.Framework.Is.EqualTo("174").Using(CustomComparers.TypeComparison), "Importer.TypeCode should be");
			organization1.OH_RL_NKClosestPort = "TW";
			NUnit.Framework.Assert.That(importer.Address.Line, NUnit.Framework.Is.EqualTo("ADDRESS 1 ADDRESS 2 TAIWAN").Using(CustomComparers.TypeComparison), "Importer.Address.Line should be");
			NUnit.Framework.Assert.That(importer.Address.ChineseLine, NUnit.Framework.Is.EqualTo("TW OTA ADDRESS 1TW OTA ADDRESS 2").Using(CustomComparers.TypeComparison), "Importer.Address.ChineseLine should be");
			NUnit.Framework.Assert.That(importer.Communications.ElementAt(0).ID, NUnit.Framework.Is.EqualTo("PHONE").Using(CustomComparers.TypeComparison), "Importer.Communications[0].ID should be");
			NUnit.Framework.Assert.That(importer.Communications.ElementAt(0).TypeID, NUnit.Framework.Is.EqualTo("TE").Using(CustomComparers.TypeComparison), "Importer.Communications[0].TypeID should be");
			NUnit.Framework.Assert.That(importer.Communications.ElementAt(1).ID, NUnit.Framework.Is.EqualTo("EMAIL").Using(CustomComparers.TypeComparison), "Importer.Communications[1].ID should be");
			NUnit.Framework.Assert.That(importer.Communications.ElementAt(1).TypeID, NUnit.Framework.Is.EqualTo("MA").Using(CustomComparers.TypeComparison), "Importer.Communications[1].TypeID should be");
			NUnit.Framework.Assert.That(importer.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("TWAEO-123465789").Using(CustomComparers.TypeComparison), "Importer.LPCOAuthorizedParty.ID should be");
			CombineAssertions("Importer Override", () =>
			{
				docAddress.E2_AddressOverride = true;
				docAddress.Address1 = "IMPORTER ADDRESS1";
				docAddress.Address2 = "IMPORTER ADDRESS2";
				docAddress.TPCCode = "22334455";
				docAddress.AEOCode = "987654321";
				docAddress.IDCodeType = "PAS";
				docAddress.IDCode = "12345678";
				docAddress.E2_Phone = "0222221234";
				docAddress.E2_Email = "paul@pllink.com";
				var importerTranslatedDocumentaryAddress = docAddress.LocalAddress;
				importerTranslatedDocumentaryAddress.E2_Address1 = "臺北加工出口區園東街6號";
				importer = new NX5105ImporterWrapper(organization1.MainAddress, "CUSCONID", docAddress, "VAT", "PAS", "PID");
				NUnit.Framework.Assert.That(importer.Address.Line, NUnit.Framework.Is.EqualTo("IMPORTER ADDRESS1 IMPORTER ADDRESS2 TAIWAN").Using(CustomComparers.TypeComparison), "Importer.Address.Line should be");
				NUnit.Framework.Assert.That(importer.Address.ChineseLine, NUnit.Framework.Is.EqualTo("臺北加工出口區園東街6號TW OTA ADDRESS 2").Using(CustomComparers.TypeComparison), "Importer.Address.ChineseLine should be");
				NUnit.Framework.Assert.That(importer.PaymentOnAccountBusinessID, NUnit.Framework.Is.EqualTo("22334455").Using(CustomComparers.TypeComparison), "Importer.PaymentOnAccountBusinessID should be");
				NUnit.Framework.Assert.That(importer.TypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison), "Importer.TypeCode should be");
				NUnit.Framework.Assert.That(importer.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("TWAEO-987654321").Using(CustomComparers.TypeComparison), "Importer.LPCOAuthorizedParty.ID.Line should be");
				NUnit.Framework.Assert.That(importer.ID, NUnit.Framework.Is.EqualTo("NO12345678").Using(CustomComparers.TypeComparison), "Importer.ID should be");
				NUnit.Framework.Assert.That(importer.Communications.ElementAt(0).ID, NUnit.Framework.Is.EqualTo("0222221234").Using(CustomComparers.TypeComparison), "Importer.Communications[0].ID should be");
				NUnit.Framework.Assert.That(importer.Communications.ElementAt(0).TypeID, NUnit.Framework.Is.EqualTo("TE").Using(CustomComparers.TypeComparison), "Importer.Communications[0].TypeID should be");
				NUnit.Framework.Assert.That(importer.Communications.ElementAt(1).ID, NUnit.Framework.Is.EqualTo("paul@pllink.com").Using(CustomComparers.TypeComparison), "Importer.Communications[1].ID should be");
				NUnit.Framework.Assert.That(importer.Communications.ElementAt(1).TypeID, NUnit.Framework.Is.EqualTo("MA").Using(CustomComparers.TypeComparison), "Importer.Communications[1].TypeID should be");
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
			var address1 = organization1.MainAddress;
			address1.OA_RN_NKCountryCode = "TW";
			address1.OA_CompanyNameOverride = "OVERRIDEN COMPANY NAME";
			address1.OA_Language = "EN";
			address1.OA_Address1 = "ADDRESS 1";
			address1.OA_Address2 = "ADDRESS 2";
			address1.OA_Phone = "PHONE";
			address1.OA_Email = "EMAIL";
			var entranslatedAddress1 = address1.TranslatedAddresses.AddNew();
			entranslatedAddress1.OTA_Language = "EN";
			entranslatedAddress1.OTA_Address1 = "EN OTA ADDRESS 1";
			entranslatedAddress1.OTA_Address2 = "EN OTA ADDRESS 2";
			var zhTWtranslatedAddress1 = address1.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress1.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			zhTWtranslatedAddress1.OTA_CompanyName = "TW OVERRIDEN COMPANY NAME";
			zhTWtranslatedAddress1.OTA_Address1 = "TW OTA ADDRESS 1";
			zhTWtranslatedAddress1.OTA_Address2 = "TW OTA ADDRESS 2";
			organization1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.AEO, "123465789", "TW");
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
			var address2 = organization1.Addresses.AddNew();
			address2.OA_Address1 = "ADDRESS 1";
			address2.OA_Address2 = "ADDRESS 2";
			organization1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("TPC", "TPCREGNO", "TW");
			Factory.Save();
		}
	}
}
