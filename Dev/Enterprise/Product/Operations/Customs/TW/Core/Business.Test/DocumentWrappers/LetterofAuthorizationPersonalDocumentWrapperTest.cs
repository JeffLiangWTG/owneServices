using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LetterofAuthorizationPersonalDocumentWrapper))]
	sealed class LetterofAuthorizationPersonalDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		LetterofAuthorizationPersonalDocumentWrapper entryHeaderisNullWrapper;
		LetterofAuthorizationPersonalDocumentWrapper importWrapper;
		LetterofAuthorizationPersonalDocumentWrapper exportWrapper;
		[ExpectNoExceptions]
		public void TestNewWhenEntryHeaderIsNull()
		{
			NUnit.Framework.Assert.That(entryHeaderisNullWrapper, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.DocumentWrappers.LetterofAuthorizationPersonalDocumentWrapper)));
		}

		protected override void SetUp()
		{
			var declarationWithoutEntryHeader = Factory.New<JobDeclaration>();
			var entryHeader = declarationWithoutEntryHeader.CustomsEntryHeaders.AddNew();
			var entryInstruction = declarationWithoutEntryHeader.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			declarationWithoutEntryHeader.JE_OA_DeclarantAddress = ZGuid.Empty;
			entryHeaderisNullWrapper = new LetterofAuthorizationPersonalDocumentWrapper(declarationWithoutEntryHeader, Factory);
			var proxyOrg = Factory.NewWithValidTestData<OrgHeader>();
			proxyOrg.OH_Code = "PRX";
			proxyOrg.OH_FullName = "Proxy Org Name";
			var proxyOrgAddress = proxyOrg.Addresses.AddNew();
			proxyOrgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			proxyOrgAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			proxyOrgAddress.OA_IsActive = true;
			proxyOrgAddress.OA_Language = Core.SharedConstants.Languages.English;
			proxyOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			proxyOrgAddress.OA_CompanyNameOverride = "Agent company name.";
			proxyOrgAddress.OA_Phone = "Agent PHONE";
			proxyOrgAddress.OA_Email = "Agent EMAIL";
			proxyOrgAddress.OA_Address1 = "Agent address line1.";
			proxyOrgAddress.OA_Address2 = "Agent address line2.";
			var agentTranslatedAddress = proxyOrgAddress.TranslatedAddresses.AddNew();
			agentTranslatedAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			agentTranslatedAddress.Address1 = "Taipei Agent E.Rd.";
			agentTranslatedAddress.Address2 = "Taipei Agent W.Rd.";
			agentTranslatedAddress.CompanyName = "Taipei Agent company name";
			var exportHelper = new ExporterForTesting(Factory);
			exportHelper.SetUpOrganization();
			var exportDeclaration = exportHelper.EntryHeader.Declaration;
			exportDeclaration.JE_OA_DeclarantAddress = proxyOrg.MainAddress.PK;
			exportDeclaration.CusEntryInstruction.CEI_DateForDuty = ZDateTime.Today.AddDays(1);
			exportWrapper = new LetterofAuthorizationPersonalDocumentWrapper(exportHelper.Declaration, Factory);
			var importHelper = new ImporterForTesting(Factory);
			importHelper.SetUpOrganization();
			var importDeclaration = importHelper.EntryHeader.Declaration;
			importDeclaration.JE_OA_DeclarantAddress = proxyOrg.MainAddress.PK;
			importDeclaration.CusEntryInstruction.CEI_DateForDuty = ZDateTime.Today.AddDays(2);
			importWrapper = new LetterofAuthorizationPersonalDocumentWrapper(importHelper.Declaration, Factory);
			exportHelper.EntryHeader.EntryNumber = "AEB80860000003";
			importHelper.EntryHeader.EntryNumber = "BEB80860000003";
			base.SetUp();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new LetterofAuthorizationPersonalDocumentWrapper(new ImporterForTesting(Factory).Declaration, Factory);
		}

		#region Fields
		[ExpectNoExceptions]
		public void TestConsignorChineseAddress()
		{
			NUnit.Framework.Assert.That(entryHeaderisNullWrapper.ConsignorChineseAddress, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(importWrapper.ConsignorChineseAddress, NUnit.Framework.Is.EqualTo("NX5105 TW OTA ADDRESS 1TW OTA ADDRESS 2").Using(CustomComparers.TypeComparison), "importWrapper.ConsignorChineseAddress should be");
			NUnit.Framework.Assert.That(exportWrapper.ConsignorChineseAddress, NUnit.Framework.Is.EqualTo("N5203 TW OTA ADDRESS 1TW OTA ADDRESS 2").Using(CustomComparers.TypeComparison), "exportWrapper.ConsignorChineseAddress should be");
		}

		[ExpectNoExceptions]
		public void TestConsignorChineseName()
		{
			NUnit.Framework.Assert.That(entryHeaderisNullWrapper.ConsignorChineseName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(importWrapper.ConsignorChineseName, NUnit.Framework.Is.EqualTo("NX5105 TW OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison), "importWrapper.ConsignorChineseName should be");
			NUnit.Framework.Assert.That(exportWrapper.ConsignorChineseName, NUnit.Framework.Is.EqualTo("TW OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison), "exportWrapper.ConsignorChineseName should be");
		}

		[ExpectNoExceptions]
		public void TestConsignorTaxID()
		{
			NUnit.Framework.Assert.That(entryHeaderisNullWrapper.ConsignorTaxID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(importWrapper.ConsignorTaxID, NUnit.Framework.Is.EqualTo("969444490").Using(CustomComparers.TypeComparison), "importWrapper.ConsignorTaxID should be");
			NUnit.Framework.Assert.That(exportWrapper.ConsignorTaxID, NUnit.Framework.Is.EqualTo("123465789").Using(CustomComparers.TypeComparison), "exportWrapper.ConsignorTaxID should be");
		}

		[ExpectNoExceptions]
		public void TestConsignorPhone()
		{
			NUnit.Framework.Assert.That(entryHeaderisNullWrapper.ConsignorPhone, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(importWrapper.ConsignorPhone, NUnit.Framework.Is.EqualTo("PHONE").Using(CustomComparers.TypeComparison), "importWrapper.ConsignorPhone should be");
			NUnit.Framework.Assert.That(exportWrapper.ConsignorPhone, NUnit.Framework.Is.EqualTo("PHONE").Using(CustomComparers.TypeComparison), "exportWrapper.ConsignorPhone should be");
		}

		[ExpectNoExceptions]
		public void TestCustomsControlID()
		{
			NUnit.Framework.Assert.That(entryHeaderisNullWrapper.CustomsControlID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(importWrapper.CustomsControlID, NUnit.Framework.Is.EqualTo("22233").Using(CustomComparers.TypeComparison), "importWrapper.CustomsControlID should be");
			NUnit.Framework.Assert.That(exportWrapper.CustomsControlID, NUnit.Framework.Is.EqualTo("BAA123").Using(CustomComparers.TypeComparison), "exportWrapper.CustomsControlID should be");
		}

		[ExpectNoExceptions]
		public void TestImportDeclarationID()
		{
			NUnit.Framework.Assert.That(entryHeaderisNullWrapper.ImportDeclarationID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(importWrapper.ImportDeclarationID, NUnit.Framework.Is.EqualTo("BEB80860000003").Using(CustomComparers.TypeComparison), "importWrapper.ImportDeclarationID should be");
			NUnit.Framework.Assert.That(exportWrapper.ImportDeclarationID, NUnit.Framework.Is.EqualTo(ZString.Empty), "exportWrapper.ImportDeclarationID should be ");
		}

		[ExpectNoExceptions]
		public void TestImportGoodsShipmentID()
		{
			NUnit.Framework.Assert.That(entryHeaderisNullWrapper.ImportGoodsShipmentID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(importWrapper.ImportGoodsShipmentID, NUnit.Framework.Is.EqualTo("house").Using(CustomComparers.TypeComparison), "importWrapper.ImportGoodsShipmentID should be ");
			NUnit.Framework.Assert.That(exportWrapper.ImportGoodsShipmentID, NUnit.Framework.Is.EqualTo(ZString.Empty), "exportWrapper.ImportGoodsShipmentID should be ");
		}

		[ExpectNoExceptions]
		public void TestDeclarationID()
		{
			NUnit.Framework.Assert.That(entryHeaderisNullWrapper.DeclarationID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(importWrapper.DeclarationID, NUnit.Framework.Is.EqualTo("BEB80860000003").Using(CustomComparers.TypeComparison), "importWrapper.DeclarationID should be ");
			NUnit.Framework.Assert.That(exportWrapper.DeclarationID, NUnit.Framework.Is.EqualTo("AEB80860000003").Using(CustomComparers.TypeComparison), "exportWrapper.DeclarationID should be ");
		}

		[ExpectNoExceptions]
		public void TestExportDeclarationID()
		{
			NUnit.Framework.Assert.That(entryHeaderisNullWrapper.ExportDeclarationID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(importWrapper.ExportDeclarationID, NUnit.Framework.Is.EqualTo(ZString.Empty), "importWrapper.ExportDeclarationID should be ");
			NUnit.Framework.Assert.That(exportWrapper.ExportDeclarationID, NUnit.Framework.Is.EqualTo("AEB80860000003").Using(CustomComparers.TypeComparison), "exportWrapper.DeclarationID should be ");
		}

		[ExpectNoExceptions]
		public void TestExportGoodsShipmentID()
		{
			NUnit.Framework.Assert.That(entryHeaderisNullWrapper.ExportGoodsShipmentID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(importWrapper.ExportGoodsShipmentID, NUnit.Framework.Is.EqualTo(ZString.Empty), "importWrapper.ExportGoodsShipmentID should be ");
			NUnit.Framework.Assert.That(exportWrapper.ExportGoodsShipmentID, NUnit.Framework.Is.EqualTo("N5203 house").Using(CustomComparers.TypeComparison), "exportWrapper.GoodsShipmentID should be ");
		}

		[ExpectNoExceptions]
		public void TestBrokerageBoxNumber()
		{
			NUnit.Framework.Assert.That(entryHeaderisNullWrapper.BrokerageBoxNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(importWrapper.BrokerageBoxNumber, NUnit.Framework.Is.EqualTo("660").Using(CustomComparers.TypeComparison), "importWrapper.BrokerageBoxNumber should be");
			NUnit.Framework.Assert.That(exportWrapper.BrokerageBoxNumber, NUnit.Framework.Is.EqualTo("660").Using(CustomComparers.TypeComparison), "exportWrapper.BrokerageBoxNumber should be");
		}

		[ExpectNoExceptions]
		public void TestAgentChineseName()
		{
			NUnit.Framework.Assert.That(entryHeaderisNullWrapper.AgentChineseName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(importWrapper.AgentChineseName, NUnit.Framework.Is.EqualTo("Taipei Agent company name").Using(CustomComparers.TypeComparison), "importWrapper.AgentChineseName should be");
			NUnit.Framework.Assert.That(exportWrapper.AgentChineseName, NUnit.Framework.Is.EqualTo("Taipei Agent company name").Using(CustomComparers.TypeComparison), "exportWrapper.AgentChineseName should be");
		}

		[ExpectNoExceptions]
		public void TestAgentChineseAddress()
		{
			NUnit.Framework.Assert.That(entryHeaderisNullWrapper.AgentChineseAddress, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(importWrapper.AgentChineseAddress, NUnit.Framework.Is.EqualTo("Taipei Agent E.Rd.Taipei Agent W.Rd.").Using(CustomComparers.TypeComparison), "importWrapper.AgentChineseAddress should be");
			NUnit.Framework.Assert.That(exportWrapper.AgentChineseAddress, NUnit.Framework.Is.EqualTo("Taipei Agent E.Rd.Taipei Agent W.Rd.").Using(CustomComparers.TypeComparison), "exportWrapper.AgentChineseAddress should be");
		}

		[ExpectNoExceptions]
		public void TestAgentPhone()
		{
			NUnit.Framework.Assert.That(entryHeaderisNullWrapper.AgentPhone, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(importWrapper.AgentPhone, NUnit.Framework.Is.EqualTo("Agent PHONE").Using(CustomComparers.TypeComparison), "importWrapper.AgentPhone should be");
			NUnit.Framework.Assert.That(exportWrapper.AgentPhone, NUnit.Framework.Is.EqualTo("Agent PHONE").Using(CustomComparers.TypeComparison), "exportWrapper.AgentPhone should be");
		}

		[TestDate(2019, 09, 04)]
		[ExpectNoExceptions]
		public void TestAcceptanceDateTime()
		{
			NUnit.Framework.Assert.That(entryHeaderisNullWrapper.AcceptanceDateTimeYear, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(entryHeaderisNullWrapper.AcceptanceDateTimeMonth, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(entryHeaderisNullWrapper.AcceptanceDateTimeDay, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(importWrapper.AcceptanceDateTimeYear, NUnit.Framework.Is.EqualTo("108").Using(CustomComparers.TypeComparison), "importWrapper.AcceptanceDateTime should be");
			NUnit.Framework.Assert.That(importWrapper.AcceptanceDateTimeMonth, NUnit.Framework.Is.EqualTo("09").Using(CustomComparers.TypeComparison), "importWrapper.AcceptanceDateTime should be");
			NUnit.Framework.Assert.That(importWrapper.AcceptanceDateTimeDay, NUnit.Framework.Is.EqualTo("06").Using(CustomComparers.TypeComparison), "importWrapper.AcceptanceDateTime should be");
			NUnit.Framework.Assert.That(exportWrapper.AcceptanceDateTimeYear, NUnit.Framework.Is.EqualTo("108").Using(CustomComparers.TypeComparison), "exportWrapper.AcceptanceDateTime should be");
			NUnit.Framework.Assert.That(exportWrapper.AcceptanceDateTimeMonth, NUnit.Framework.Is.EqualTo("09").Using(CustomComparers.TypeComparison), "exportWrapper.AcceptanceDateTime should be");
			NUnit.Framework.Assert.That(exportWrapper.AcceptanceDateTimeDay, NUnit.Framework.Is.EqualTo("05").Using(CustomComparers.TypeComparison), "exportWrapper.AcceptanceDateTime should be");
		}

		[ExpectNoExceptions]
		public void TestIsExport()
		{
			NUnit.Framework.Assert.That(entryHeaderisNullWrapper.IsExport, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(importWrapper.IsExport, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "importWrapper.IsExport should be");
			NUnit.Framework.Assert.That(exportWrapper.IsExport, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "exportWrapper.IsExport should be");
		}

		[ExpectNoExceptions]
		public void TestIsImport()
		{
			NUnit.Framework.Assert.That(entryHeaderisNullWrapper.IsImport, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(importWrapper.IsImport, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "importWrapper.IsImport should be");
			NUnit.Framework.Assert.That(exportWrapper.IsImport, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "exportWrapper.IsImport should be");
		}

		[ExpectNoExceptions]
		public void TestAgentAddress()
		{
			NUnit.Framework.Assert.That(entryHeaderisNullWrapper.AgentAddress, NUnit.Framework.Is.EqualTo(default(Enterprise.MasterFiles.Business.OrgAddress)));
			NUnit.Framework.Assert.That(importWrapper.AgentAddress, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.MasterFiles.Business.OrgAddress)));
			NUnit.Framework.Assert.That(importWrapper.AgentAddress.Address1, NUnit.Framework.Is.EqualTo("Agent address line1.").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(importWrapper.AgentAddress.Address2, NUnit.Framework.Is.EqualTo("Agent address line2.").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(exportWrapper.AgentAddress, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.MasterFiles.Business.OrgAddress)));
			NUnit.Framework.Assert.That(exportWrapper.AgentAddress.Address1, NUnit.Framework.Is.EqualTo("Agent address line1.").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(exportWrapper.AgentAddress.Address2, NUnit.Framework.Is.EqualTo("Agent address line2.").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCustomsOffice()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "AB";
			var wrapper = new LetterofAuthorizationPersonalDocumentWrapper(declaration, Factory);
			NUnit.Framework.Assert.That(wrapper.CustomsOffice, NUnit.Framework.Is.EqualTo("基隆").Using(CustomComparers.TypeComparison));
			entryInstruction.CEI_CustomsOffice = "BD";
			NUnit.Framework.Assert.That(wrapper.CustomsOffice, NUnit.Framework.Is.EqualTo("高雄").Using(CustomComparers.TypeComparison));
			entryInstruction.CEI_CustomsOffice = "CT";
			NUnit.Framework.Assert.That(wrapper.CustomsOffice, NUnit.Framework.Is.EqualTo("台北").Using(CustomComparers.TypeComparison));
			entryInstruction.CEI_CustomsOffice = "DM";
			NUnit.Framework.Assert.That(wrapper.CustomsOffice, NUnit.Framework.Is.EqualTo("台中").Using(CustomComparers.TypeComparison));
		}
		#endregion
	}
}
