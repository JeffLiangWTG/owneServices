using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.TW.Business.Testing
{
	public class TestTWCreator
	{
		public TestTWCreator(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public void CreateRegistryItemCusBrokerageBoxNumber()
		{
			var registryTemplates = new CusBrokerageBoxNumberCollection();
			var registryTemplate = registryTemplates.AddNew();
			registryTemplate.BoxNumber = "600";
			registryTemplate.CustomsOfficeArea = "A";
			registryTemplate.IsDefaultBoxNumber = ZBool.True;
			registryTemplate = registryTemplates.AddNew();
			registryTemplate.BoxNumber = "100";
			registryTemplate.CustomsOfficeArea = "B";
			registryTemplate.IsDefaultBoxNumber = ZBool.True;
			registryTemplate = registryTemplates.AddNew();
			registryTemplate.BoxNumber = "300";
			registryTemplate.CustomsOfficeArea = "B";
			registryTemplate.IsDefaultBoxNumber = ZBool.False;
			TWCustomsDataRegistry.Instance.CusBrokerageBoxNumber.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryTemplates);
		}

		public void CreateRegistryItemCusGoodsLocation()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "CustomsOffice");
			var codeList = helper.CreateNewOrGetExistingCusCodeList("TW", "CUSOF", "CC", "BABA THE BUILDER", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(codeList.PK, TransportTypeList.Codes.Sea);
			var codeList2 = helper.CreateNewOrGetExistingCusCodeList("TW", "CUSOF", "DD", "DD Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(codeList2.PK, TransportTypeList.Codes.Air);

			helper.CreateNewOrGetExistingCusCodeType("FAC", "Facilities");
			var facility = helper.CreateNewOrGetExistingCusCodeList("TW", "FAC", "ANP0060D", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "Desc.", "FAC", "TW");
			facility.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "CC");

			facility = helper.CreateNewOrGetExistingCusCodeList("TW", "FAC", "ANP0061D", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "Desc.", "FAC", "TW");
			facility.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "DD");

			facility = helper.CreateNewOrGetExistingCusCodeList("TW", "FAC", "ANP0062D", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "Desc.", "FAC", "TW");
			facility.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "DD");
			factory.Save();

			var registryTemplates = new CusGoodsLocationCollection();
			var registryTemplate = registryTemplates.AddNew();
			registryTemplate.MessageType = "IMP";
			registryTemplate.CustomsOffice = "CC";
			registryTemplate.GoodsLocation = "ANP0060D";
			registryTemplate = registryTemplates.AddNew();
			registryTemplate.MessageType = "IMP";
			registryTemplate.CustomsOffice = "DD";
			registryTemplate.GoodsLocation = "ANP0061D";

			registryTemplate = registryTemplates.AddNew();
			registryTemplate.MessageType = "EXP";
			registryTemplate.CustomsOffice = "CC";
			registryTemplate.GoodsLocation = "ANP0060D";
			registryTemplate = registryTemplates.AddNew();
			registryTemplate.MessageType = "EXP";
			registryTemplate.CustomsOffice = "DD";
			registryTemplate.GoodsLocation = "ANP0062D";
			TWCustomsDataRegistry.Instance.CusGoodsLocation.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryTemplates);
		}

		public void CreateRegistryItemCusBrokerStaff()
		{
			CreateBrokerStaff();
			factory.Save();

			var registryTemplate = new CusBrokerStaff();
			registryTemplate.BrokerStaffCode = "CYO";
			registryTemplate.Mailbox = "TBK0461-0";
			TWCustomsDataRegistry.Instance.CusBrokerStaff.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK, registryTemplate);
		}

		public void CreateRegistryItemCusCustomsOffice()
		{
			CreateCustomsOffice();
			var registryTemplate = new CusCustomsOffice();
			registryTemplate.CustomsOfficeCode = "CE";
			TWCustomsDataRegistry.Instance.CusCustomsOffice.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK, registryTemplate);
		}

		public void CreateInvoiceUQ()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateCusCodeType("TWCIU", "Taiwan Commercial Invoice Units of Measurement");
			helper.CreateNewOrGetExistingCusCodeList("TW", "TWCIU", "AMP", "Ampere", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();
		}

		public void CreatePackagingUQ()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateCusCodeType("TWCIU", "Taiwan Commercial Invoice Units of Measurement");
			helper.CreateNewOrGetExistingCusCodeList("TW", "TWCIU", "AMP", "Ampere", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();
		}

		public void CreateCustomsUQ()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateCusCodeType("TWPUM", "Taiwan Packing Units of Measurement");
			helper.CreateNewOrGetExistingCusCodeList("TW", "TWPUM", "AMP", "Ampere", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();
		}

		public void CreateRefVessel()
		{
			var refVessel = factory.New<RefVessel>();
			refVessel.RV_LloydsNumber = "0982432";
			refVessel.RV_Code = "VESSELNAME";
			refVessel.RV_RadioCallSign = "X232";
			factory.Save();
		}

		public void CreateOrgSupplierPart(OrgHeader supplier)
		{
			supplier.OH_IsConsignor = true;
			var part = factory.New<OrgSupplierPart>();
			part.OP_StockKeepingUnit = "BAG";
			part.OP_PartNum = "NEWPROD1";
			part.OP_Desc = "PRODUCT1";
			part.OP_Brand = "Apple";
			part.OP_Model = "Phone";
			var relatedOrganization = part.RelatedOrganisations.AddNew();
			relatedOrganization.OU_OH = supplier.PK;
			relatedOrganization.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ModeOfStatistics = "01";
			pivot.CI_DutyTreatment = "31";
			pivot.CI_Price = 100m;
			pivot.CI_PriceCurr = "TWD";
			pivot.CI_TariffAdditionalCode = "test";
			pivot.CI_Compositions = "規格：BOX 100*50mm and made from paper";
			pivot.CI_RN_NKCountryOfOrigin = "TW";
			pivot.CI_NDescription = "CI_NDescription: 中文";
			pivot.CI_CustomsOwnerPartNo = "CI_CustomsOwnerPartNo test";
			pivot.CI_CustomsSupplierPartNo = "CI_CustomsSupplierPartNo test";
			pivot.CI_AlcoholPercentage = 30m;
			pivot.CI_DeclGoodsDescMode = DeclarationGoodsDescriptionModeList.Codes.CHT;
			var productPermit = pivot.ProductPermitCusSupportingCollection.AddNew();
			productPermit.CSI_LineNo = 123;
			productPermit.CSI_ReferenceNumber = "12345678901234";
			var productAssigned = pivot.AssignedCusClassPartPivotRefCollection.AddNew();
			productAssigned.CIR_ReferenceNumber = "ReferenceNumber123";
			factory.Save();
		}

		public void CreateBrokerStaff()
		{
			CreateAndSetProxyOrganization();
			var currentBranch = GlbBranch.CurrentBranch;
			currentBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			currentBranch.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			var broker1 = factory.NewWithValidTestData<GlbStaff>();
			GlbStaff.CurrentUser.GS_GB_HomeBranch = currentBranch.PK;
			broker1.GS_GB_HomeBranch = currentBranch.PK;
			broker1.GS_LoginName = "CheckYao";
			broker1.GS_FullName = "Check Yao";
			broker1.GS_Code = "CYO";

			var certificateBrkTw = broker1.Certificates.AddNew();
			certificateBrkTw.XZ_Type = Enterprise.Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			certificateBrkTw.XZ_RN_NKCountryOfIssuance = "TW";
			certificateBrkTw.XZ_RefNumber = "CCC";

			var currentCompanyPk = GlbCompany.CurrentCompany.PK;

			var extPswUvc = factory.New<GlbExternalPassword>();
			extPswUvc.GP_GC = currentCompanyPk;
			extPswUvc.GP_GS = broker1.PK;
			extPswUvc.GP_PasswordType = PasswordTypesList.Codes.UVC;
			extPswUvc.GP_MailBoxID = "TBK0461-0";

			var extPswUva = factory.New<GlbExternalPassword>();
			extPswUva.GP_GC = currentCompanyPk;
			extPswUva.GP_PasswordType = "UVC";
			extPswUva.GP_MailBoxID = "BBB-2";

			var extPswAub = factory.New<GlbExternalPassword>();
			extPswAub.GP_GC = currentCompanyPk;
			extPswAub.GP_PasswordType = PasswordTypesList.Codes.AUB;
			extPswAub.GP_MailBoxID = "AUB-2";
		}

		public void CreateRefCusCodeForControllingMessageType()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Codes.TWControllingAgency, "TWControllingAgency");
			helper.CreateNewOrGetExistingCusCodeType(Codes.TWControllingMessageMessageType, "Taiwan Controlling Message Message Type");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.ControlAgency, "Taiwan Controlling Agency", Codes.TWControllingMessageMessageType, "TW");

			helper.CreateNewOrGetExistingCusCodeList("TW", Codes.TWControllingAgency, "CI", "CI DES.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("TW", Codes.TWControllingAgency, "20", "20 DES.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("TW", Codes.TWControllingAgency, "2Q", "2Q DES.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("TW", Codes.TWControllingAgency, "FT", "FT DES.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("TW", Codes.TWControllingAgency, "DN", "DN DES.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("TW", Codes.TWControllingAgency, "VP", "VP DES.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("TW", Codes.TWControllingAgency, "CD", "CD DES.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("TW", Codes.TWControllingAgency, "IF", "IF DES.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("TW", Codes.TWControllingAgency, "DH", "DH DES.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var codeList = helper.CreateNewOrGetExistingCusCodeList("TW", Codes.TWControllingMessageMessageType, "X101", "產地證明申辦訊息", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			codeList.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "FT");

			codeList = helper.CreateNewOrGetExistingCusCodeList("TW", Codes.TWControllingMessageMessageType, "NX301", "報驗申辦訊息", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			codeList.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "CI");
			codeList.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "20");
			codeList.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "2Q");

			codeList = helper.CreateNewOrGetExistingCusCodeList("TW", Codes.TWControllingMessageMessageType, "NX301_DN", "酒類查驗申辦訊息", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			codeList.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "DN");

			codeList = helper.CreateNewOrGetExistingCusCodeList("TW", Codes.TWControllingMessageMessageType, "NX401", "檢疫申辦訊息", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			codeList.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "VP");

			codeList = helper.CreateNewOrGetExistingCusCodeList("TW", Codes.TWControllingMessageMessageType, "NX601", "輸入食品及中藥材報驗申辦訊息", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			codeList.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "CD");
			codeList.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "IF");
			codeList.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "DH");

			codeList = helper.CreateNewOrGetExistingCusCodeList("TW", Codes.TWControllingMessageMessageType, "NX603", "輸入醫療器材及藥品報驗申辦訊息", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			codeList.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "CD");
			codeList.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "IF");
			codeList.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "DH");

			codeList = helper.CreateNewOrGetExistingCusCodeList("TW", Codes.TWControllingMessageMessageType, "NX101", "產地證明申辦訊息", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			codeList.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "FT");

			factory.Save();
		}

		public void CreateCustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BA", "Taipei office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CE", "TaoYuan office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();
		}

		public void CreateCustomsManifestStatus()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Codes.CustomsManifestStatus, "Customs Manifest Status", "TW");
			helper.CreateNewOrGetExistingCusCodeList("TW", Codes.CustomsManifestStatus, Constants.CustomsManifestStatus.AK, "要求", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("TW", Codes.CustomsManifestStatus, Constants.CustomsManifestStatus.AP, "接受", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("TW", Codes.CustomsManifestStatus, Constants.CustomsManifestStatus.EX, "需查驗(儀器查驗、加封電子封條或辦理車機封條加封確認作業)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("TW", Codes.CustomsManifestStatus, Constants.CustomsManifestStatus.RE, "拒絕", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();
		}

		public void CreateExchangeRateToUSD()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			var currency = RefCurrency.LoadFromCurrencyCode(factory, Core.Constants.CurrencyCodes.UnitedStates);

			var sellRate = currency.ExchangeRates.AddNew();
			sellRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			sellRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			sellRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			sellRate.RE_SellRate = 32m;

			var buyRate = currency.ExchangeRates.AddNew();
			buyRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			buyRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
			buyRate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			buyRate.RE_SellRate = 30m;

			var customsRate = currency.ExchangeRates.AddNew();
			customsRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			customsRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			customsRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			customsRate.RE_SellRate = 31m;
		}

		public void CreateRefCusCodeForDeclarationType()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "Ensty");
			var styleB1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "B1", "課稅區售與保稅廠", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleB1.PK, RefCusCodeListAttributeTypes.Codes.IsExport, "Y");
			var styleB2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "B2", "保稅廠相互交易或售與保稅倉", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleB2.PK, RefCusCodeListAttributeTypes.Codes.IsExport, "Y");
			var styleB8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "B8", "保稅廠進口貨物(原料)復出口", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleB8.PK, RefCusCodeListAttributeTypes.Codes.IsExport, "Y");
			var styleB9 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "B9", "保稅廠產品出口", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleB9.PK, RefCusCodeListAttributeTypes.Codes.IsExport, "Y");
			var styleD1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "D1", "課稅區售與或退回保稅倉", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleD1.PK, RefCusCodeListAttributeTypes.Codes.IsExport, "Y");
			var styleD5 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "D5", "保稅倉貨物出口", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleD5.PK, RefCusCodeListAttributeTypes.Codes.IsExport, "Y");
			var styleF4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "F4", "自由港區與他自由港區、課稅區間之交易", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleF4.PK, RefCusCodeListAttributeTypes.Codes.IsExport, "Y");
			var styleF5 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "F5", "自由港區貨物出口", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleF5.PK, RefCusCodeListAttributeTypes.Codes.IsExport, "Y");
			var styleG3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "G3", "外貨復出口", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleG3.PK, RefCusCodeListAttributeTypes.Codes.IsExport, "Y");
			var styleG5 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "G5", "國貨出口", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleG5.PK, RefCusCodeListAttributeTypes.Codes.IsExport, "Y");
			var styleTA = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "TA", "外貨進出口", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleTA.PK, RefCusCodeListAttributeTypes.Codes.IsExport, "Y");
			var styleX6 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "X6", "出口快遞文件", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleX6.PK, RefCusCodeListAttributeTypes.Codes.IsExport, "Y");
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleX6.PK, RefCusCodeListAttributeTypes.Codes.IsExpressDelivery, "Y");
			var styleX7 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "X7", "出口低價快遞貨物", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleX7.PK, RefCusCodeListAttributeTypes.Codes.IsExport, "Y");
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleX7.PK, RefCusCodeListAttributeTypes.Codes.IsExpressDelivery, "Y");
			var styleX8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "X8", "出口高價快遞貨物", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleX8.PK, RefCusCodeListAttributeTypes.Codes.IsExport, "Y");
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleX8.PK, RefCusCodeListAttributeTypes.Codes.IsExpressDelivery, "Y");
			var styleB6 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "B6", "保稅廠輸入貨物(原料)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleB6.PK, RefCusCodeListAttributeTypes.Codes.IsImport, "Y");
			var styleD2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "D2", "保稅貨出保稅倉進口", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleD2.PK, RefCusCodeListAttributeTypes.Codes.IsImport, "Y");
			var styleD7 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "D7", "保稅倉相互轉儲或運往保稅廠", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleD7.PK, RefCusCodeListAttributeTypes.Codes.IsImport, "Y");
			var styleD8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "D8", "外貨進保稅倉", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleD8.PK, RefCusCodeListAttributeTypes.Codes.IsImport, "Y");
			var styleF1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "F1", "外貨進儲自由港區", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleF1.PK, RefCusCodeListAttributeTypes.Codes.IsImport, "Y");
			var styleF2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "F2", "自由港區貨物進口", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleF2.PK, RefCusCodeListAttributeTypes.Codes.IsImport, "Y");
			var styleF3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "F3", "自由港區區內事業間之交易", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleF3.PK, RefCusCodeListAttributeTypes.Codes.IsImport, "Y");
			var styleG1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "G1", "外貨進口", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleG1.PK, RefCusCodeListAttributeTypes.Codes.IsImport, "Y");
			var styleG2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "G2", "本地補稅案件", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleG2.PK, RefCusCodeListAttributeTypes.Codes.IsImport, "Y");
			var styleG7 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "G7", "國貨復進口", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleG7.PK, RefCusCodeListAttributeTypes.Codes.IsImport, "Y");
			var styleL1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "L1", "外貨進儲物流中心", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleL1.PK, RefCusCodeListAttributeTypes.Codes.IsImport, "Y");
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleTA.PK, RefCusCodeListAttributeTypes.Codes.IsImport, "Y");
			var styleX1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "X1", "進口快遞文件", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleX1.PK, RefCusCodeListAttributeTypes.Codes.IsImport, "Y");
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleX1.PK, RefCusCodeListAttributeTypes.Codes.IsExpressDelivery, "Y");
			var styleX2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "X2", "進口低價免稅快遞貨物", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleX2.PK, RefCusCodeListAttributeTypes.Codes.IsImport, "Y");
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleX2.PK, RefCusCodeListAttributeTypes.Codes.IsExpressDelivery, "Y");
			var styleX3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "X3", "進口低價應稅快遞貨物」（「完稅價格」新臺幣二千零一元至五萬元）", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleX3.PK, RefCusCodeListAttributeTypes.Codes.IsImport, "Y");
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleX3.PK, RefCusCodeListAttributeTypes.Codes.IsExpressDelivery, "Y");
			var styleX4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "X4", "進口高價快遞貨物」（「完稅價格」超過新臺幣五萬元）", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleX4.PK, RefCusCodeListAttributeTypes.Codes.IsImport, "Y");
			helper.CreateNewOrGetExistingCusCodeListAttribute(styleX4.PK, RefCusCodeListAttributeTypes.Codes.IsExpressDelivery, "Y");
			factory.Save();
		}

		public OrgHeader CreateOrganization()
		{
			var header = factory.New<OrgHeader>();
			header.OH_Code = "OrgTest1";
			header.OH_RL_NKClosestPort = "TW";

			var mainAddress = header.MainAddress;
			mainAddress.OA_CompanyNameOverride = "HAPPY CO., LTD.";
			mainAddress.OA_Language = Core.SharedConstants.Languages.English;
			mainAddress.OA_Address1 = "1500 HAPPY RD";
			mainAddress.OA_Address2 = "ORANGE DISTRICT";
			mainAddress.OA_RN_NKCountryCode = "TW";
			mainAddress.OA_City = "APPLE CITY";
			mainAddress.Postcode = "12345";
			mainAddress.OA_State = "TPE";
			mainAddress.OA_Phone = "13925568211";
			mainAddress.OA_Email = "123@456.com";
			mainAddress.OA_Fax = "13925579322";

			var zhTWtranslatedAddress1 = mainAddress.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress1.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			zhTWtranslatedAddress1.OTA_CompanyName = "綠晃科技股份有限公司";
			zhTWtranslatedAddress1.OTA_Address1 = "臺北加工出口區園東街6號";
			zhTWtranslatedAddress1.OTA_Address2 = string.Empty;
			zhTWtranslatedAddress1.OTA_City = "臺北巿";
			zhTWtranslatedAddress1.OTA_PostCode = "90093";
			zhTWtranslatedAddress1.OTA_State = "TPE";
			zhTWtranslatedAddress1.ClosestPort = "TW";

			header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("AEO", "123465789", "TW");

			var contact = header.Contacts.AddNew();
			contact.OC_ContactName = "Contact1";
			CreateTPEState();
			factory.Save();
			return header;
		}

		public OrgHeader CreateOrganizationForJobDocAddress()
		{
			var header = factory.New<OrgHeader>();
			header.OH_Code = "OrgTest1";
			header.OH_RL_NKClosestPort = "TW";
			header.OH_IsConsignee = true;

			var mainAddress = header.MainAddress;
			mainAddress.OA_CompanyNameOverride = "HAPPY CO., LTD.";
			mainAddress.OA_Language = Core.SharedConstants.Languages.English;
			mainAddress.UnrestrictedAdditionalAddressInformation = "addinfo address";
			mainAddress.OA_Address1 = "1500 HAPPY RD";
			mainAddress.OA_Address2 = "ORANGE DISTRICT";
			mainAddress.OA_RN_NKCountryCode = "TW";
			mainAddress.OA_City = "APPLE CITY";
			mainAddress.Postcode = "12345";
			mainAddress.OA_State = "TPE";
			mainAddress.OA_Phone = "+1 (273) 5495200";
			mainAddress.OA_Email = "001@xx.com";
			mainAddress.OA_Fax = "001FAX";

			var zhTWtranslatedAddress1 = mainAddress.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress1.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			zhTWtranslatedAddress1.UnrestrictedAdditionalAddressInformation = "附加信息2";
			zhTWtranslatedAddress1.OTA_CompanyName = "綠晃科技股份有限公司";
			zhTWtranslatedAddress1.OTA_Address1 = "臺北加工出口區園東街6號";
			zhTWtranslatedAddress1.OTA_Address2 = string.Empty;
			zhTWtranslatedAddress1.OTA_City = "臺北巿";
			zhTWtranslatedAddress1.OTA_PostCode = "90093";
			zhTWtranslatedAddress1.OTA_State = "TPE";
			zhTWtranslatedAddress1.ClosestPort = "TW";

			var addresse = header.Addresses.AddNew();
			addresse.OA_CompanyNameOverride = "HAPPY CO., LTD.1";
			addresse.UnrestrictedAdditionalAddressInformation = "addinfo address1";
			addresse.OA_Language = Core.SharedConstants.Languages.English;
			addresse.OA_Address1 = "004 HAPPY RD";
			addresse.OA_Address2 = "ORANGE DISTRICT1";
			addresse.OA_RN_NKCountryCode = "TW";
			addresse.OA_City = "APPLE CITY1";
			addresse.Postcode = "001";
			addresse.OA_State = "TPE";
			addresse.OA_Phone = "+2 (273) 5495200";
			addresse.OA_Email = "002@xx.com";
			addresse.OA_Fax = "002FAX";

			addresse = header.Addresses.AddNew();
			addresse.OA_CompanyNameOverride = "台湾分公司";
			addresse.UnrestrictedAdditionalAddressInformation = "附加信息";
			addresse.OA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			addresse.OA_Address1 = "地址1";
			addresse.OA_Address2 = "地址2";
			addresse.OA_RN_NKCountryCode = "TW";
			addresse.OA_City = "台北";
			addresse.Postcode = "002";
			addresse.OA_State = "TPE";
			addresse.OA_Phone = "+3 (273) 5495200";
			addresse.OA_Email = "003@xx.com";
			addresse.OA_Fax = "003FAX";

			header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("AEO", "123465789", "TW");

			var contact = header.Contacts.AddNew();
			contact.OC_ContactName = "Contact1";
			CreateTPEState();
			factory.Save();
			return header;
		}

		public void CreateTPEState()
		{
			CreateStateIfNotExists("TW", "TPE", "TAIPEI");
		}

		void CreateStateIfNotExists(ZString countryCode, ZString state, ZString description)
		{
			var zquery = new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, countryCode);
			zquery.AddToFilter(RefCountryStatesSchema.RW_Code, state);

			if (factory.LoadTop1<RefCountryStates>(zquery) == null)
			{
				var activeState = factory.New<RefCountryStates>();
				activeState.RW_RN_NKCountryCode = countryCode;
				activeState.RW_Code = state;
				activeState.RW_Description = description;
				activeState.RW_IsActive = true;
			}
		}

		public void CreateAndSetProxyOrganization()
		{
			var header = factory.New<OrgHeader>();
			header.OH_Code = "AgentOrg01";
			header.OH_RL_NKClosestPort = "TW";
			header.OH_FullName = "Proxy Org Name";

			var mainAddress = header.MainAddress;
			mainAddress.OA_CompanyNameOverride = "AGENT CO., LTD.";
			mainAddress.OA_Phone = "Agent PHONE";
			mainAddress.OA_Email = "Agent EMAIL";
			mainAddress.OA_Language = Core.SharedConstants.Languages.English;
			mainAddress.OA_Address1 = "1500 AGENT RD";
			mainAddress.OA_Address2 = "ORANGE DISTRICT";
			mainAddress.OA_RN_NKCountryCode = "TW";
			mainAddress.OA_City = "APPLE CITY";
			mainAddress.Postcode = "12345";
			mainAddress.OA_State = "TPE";

			var zhTWtranslatedAddress1 = mainAddress.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress1.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			zhTWtranslatedAddress1.OTA_CompanyName = "代理股份有限公司";
			zhTWtranslatedAddress1.OTA_Address1 = "臺北代理出口區園東街6號";
			zhTWtranslatedAddress1.OTA_Address2 = string.Empty;
			zhTWtranslatedAddress1.OTA_City = "臺北巿";
			zhTWtranslatedAddress1.OTA_PostCode = "10093";
			zhTWtranslatedAddress1.ClosestPort = "TW";

			header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.AEO, "123465789", "TW");
			header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "96944490", "TW");
			factory.Save();
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = header.PK;
		}

		public OrgHeader CreateOrganizationForPowerOfAttorneyDocument()
		{
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.Addresses.RemoveAndDeleteAll();
			orgHeader.OH_Code = "testOrg";

			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;

			var mainAddress = orgHeader.Addresses[0];
			mainAddress.OA_Language = Core.SharedConstants.Languages.English;
			mainAddress.CompanyName = "Company Name";
			mainAddress.OA_Language = Core.SharedConstants.Languages.ChineseTraditional;

			var entranslatedAddress = mainAddress.TranslatedAddresses.AddNew();
			entranslatedAddress.OTA_Language = Core.SharedConstants.Languages.English;
			entranslatedAddress.OTA_CompanyName = "Translate CompanyName";

			var requiredDocument = orgHeader.RequiredDocuments.AddNew("POA");
			requiredDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			requiredDocument.EQ_DateReceived = new ZDateTimeOffset(2020, 1, 1);
			requiredDocument.EQ_ValidToDate = new ZDateTime(2020, 5, 1);
			requiredDocument.EQ_DocNumber = "1234";
			requiredDocument.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			var attribute = requiredDocument.Attributes.AddNew();
			attribute.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CustomsDistrict;
			attribute.D0_AttribValue = "B";
			return orgHeader;
		}

		public OrgHeader CreateOrganizationForImporter()
		{
			var importerOrg = factory.NewWithValidTestData<OrgHeader>();
			importerOrg.OH_Code = "Importer01";
			importerOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "I111111", Core.Constants.CountryCodes.Taiwan);
			importerOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "I222222", Core.Constants.CountryCodes.Taiwan);
			importerOrg.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "I333333", Core.Constants.CountryCodes.Taiwan);
			importerOrg.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.AEO, "IAEO001", Core.Constants.CountryCodes.Taiwan);
			importerOrg.OH_RL_NKClosestPort = "TWKEL";
			var importerOrgAddress = importerOrg.Addresses.AddNew();
			importerOrgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			importerOrgAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			importerOrgAddress.OA_IsActive = true;
			importerOrgAddress.OA_Language = Core.SharedConstants.Languages.English;
			importerOrgAddress.OA_CompanyNameOverride = "Importer company name xxx.";
			importerOrgAddress.OA_Address1 = "XX231";
			importerOrgAddress.OA_Address2 = "321XX";
			importerOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;

			importerOrgAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "ICPW001", Core.Constants.CountryCodes.Taiwan);
			importerOrgAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "ICCP001", Core.Constants.CountryCodes.Taiwan);

			var importerChineseAddress = importerOrgAddress.TranslatedAddresses.AddNew();
			importerChineseAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			importerChineseAddress.Address1 = "忠孝東路";
			importerChineseAddress.Address2 = "三段232號";
			importerChineseAddress.CompanyName = "公司名稱X1";
			var importerEnglishAddress = importerOrgAddress.TranslatedAddresses.AddNew();
			importerEnglishAddress.OTA_Language = Core.SharedConstants.Languages.English;
			importerEnglishAddress.Address1 = "No. 232, Sec. 3, ZhongXiao N. Rd.,";
			importerEnglishAddress.Address2 = "Zhongshan Dist., Taipei City 104, Taiwan (R.O.C.)";
			importerEnglishAddress.CompanyName = "Importer company name(OTA).";
			return importerOrg;
		}

		public OrgHeader CreateOrganizationForWarehouse2()
		{
			var warehouseOrg2 = factory.NewWithValidTestData<OrgHeader>();
			warehouseOrg2.OH_Code = "WMS02";
			var warehouseAddress2 = warehouseOrg2.Addresses.AddNew();
			warehouseAddress2.Address1 = "Warehouse Address to1";
			warehouseAddress2.Address2 = "Warehouse Address to2";
			warehouseOrg2.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "00612444", Core.Constants.CountryCodes.Taiwan);
			warehouseOrg2.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "9800003", Core.Constants.CountryCodes.Taiwan);
			warehouseOrg2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "00612349", Core.Constants.CountryCodes.Taiwan);
			warehouseOrg2.MainAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.FTZ, "PAA01", Core.Constants.CountryCodes.Taiwan);
			return warehouseOrg2;
		}

		public OrgHeader CreateOrganizationForWarehouse()
		{
			var warehouse = factory.NewWithValidTestData<OrgHeader>();
			warehouse.OH_Code = "WMS01";
			var warehouseAddress = warehouse.Addresses.AddNew();
			warehouseAddress.Address1 = "Warehouse Address1";
			warehouseAddress.Address2 = "Warehouse Address2";
			warehouse.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "00612348", Core.Constants.CountryCodes.Taiwan);
			warehouse.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "9800001", Core.Constants.CountryCodes.Taiwan);
			warehouse.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "9800002", Core.Constants.CountryCodes.Taiwan);
			warehouse.MainAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "22233", Core.Constants.CountryCodes.Taiwan);
			return warehouse;
		}

		public OrgHeader CreateOrganizationForConsignee()
		{
			var consigneeOrg = factory.NewWithValidTestData<OrgHeader>();
			consigneeOrg.OH_Code = "CNEE02";
			consigneeOrg.OH_Code = "JE CNEE";
			consigneeOrg.OH_RL_NKClosestPort = "TWKEL";
			var consigneeOrgAddress = consigneeOrg.Addresses.AddNew();
			consigneeOrgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			consigneeOrgAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			consigneeOrgAddress.OA_IsActive = true;
			consigneeOrgAddress.OA_Language = Core.SharedConstants.Languages.English;
			consigneeOrgAddress.OA_CompanyNameOverride = "Consignee Company Name Override";
			consigneeOrgAddress.OA_Address1 = "Consignee Address1";
			consigneeOrgAddress.OA_Address2 = "Consignee Address2";
			consigneeOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var consigneeChineseAddress = consigneeOrgAddress.TranslatedAddresses.AddNew();
			consigneeChineseAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			consigneeChineseAddress.Address1 = "收貨人 忠孝東路 地址";
			consigneeChineseAddress.Address2 = "三段232號 地址2";
			consigneeChineseAddress.CompanyName = "收貨人 公司名稱";
			consigneeOrgAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "33333333", Core.Constants.CountryCodes.Taiwan);
			consigneeOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "333CCC", Core.Constants.CountryCodes.Taiwan);
			return consigneeOrg;
		}

		public OrgHeader CreateOrganizationForConsignor()
		{
			var consignorOrg = factory.NewWithValidTestData<OrgHeader>();
			consignorOrg.OH_Code = "JE SHPR";
			consignorOrg.OH_RL_NKClosestPort = "CNSZX";
			var consignorOrgAddress = consignorOrg.Addresses.AddNew();
			consignorOrgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			consignorOrgAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			consignorOrgAddress.OA_IsActive = true;
			consignorOrgAddress.OA_Language = Core.SharedConstants.Languages.English;
			consignorOrgAddress.OA_CompanyNameOverride = "Consignor Company Name Override";
			consignorOrgAddress.OA_Address1 = "Consignor Address1";
			consignorOrgAddress.OA_Address2 = "Consignor Address2";
			consignorOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var consignorChineseAddress = consignorOrgAddress.TranslatedAddresses.AddNew();
			consignorChineseAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			consignorChineseAddress.Address1 = "發貨人 民生東路 地址";
			consignorChineseAddress.Address2 = "四段16號 地址2";
			consignorChineseAddress.CompanyName = "發貨人 公司名稱";
			consignorOrgAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "88888888", Core.Constants.CountryCodes.Taiwan);
			consignorOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "CR222222", Core.Constants.CountryCodes.Taiwan);
			consignorOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "CR111111", Core.Constants.CountryCodes.Taiwan);
			consignorOrg.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "CR333333", Core.Constants.CountryCodes.Taiwan);
			return consignorOrg;
		}

		public OrgHeader CreateOrganizationForSupplier()
		{
			var supplier = factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "Supplier01";
			supplier.OH_RL_NKClosestPort = "TWKEL";
			var supplierOrgAddress = supplier.Addresses.AddNew();
			supplierOrgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			supplierOrgAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			supplierOrgAddress.OA_IsActive = true;
			supplierOrgAddress.OA_Language = Core.SharedConstants.Languages.English;
			supplierOrgAddress.OA_CompanyNameOverride = "Supplier company name.";
			supplierOrgAddress.OA_Phone = "PHONE";
			supplierOrgAddress.OA_Email = "EMAIL";
			supplierOrgAddress.OA_Address1 = "88899 address line1.";
			supplierOrgAddress.OA_Address2 = "88899 address line2.";
			var supplierTranslatedAddress = supplierOrgAddress.TranslatedAddresses.AddNew();
			supplierTranslatedAddress.OTA_Language = Core.SharedConstants.Languages.English;
			supplierTranslatedAddress.Address1 = "Taipei Minsheng E.Rd.";
			supplierTranslatedAddress.Address2 = "Taipei Minsheng W.Rd.";
			supplierTranslatedAddress.CompanyName = "Supplier company name(OTA).";
			var supplierTranslatedAddress2 = supplierOrgAddress.TranslatedAddresses.AddNew();
			supplierTranslatedAddress2.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			supplierTranslatedAddress2.Address1 = "TW Taipei Minsheng E.Rd.";
			supplierTranslatedAddress2.Address2 = "TW Taipei Minsheng W.Rd.";
			supplierTranslatedAddress2.CompanyName = "TW Supplier company name";
			supplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "5555555", Core.Constants.CountryCodes.Taiwan);
			supplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "6666666", Core.Constants.CountryCodes.Taiwan);
			supplier.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "777777", Core.Constants.CountryCodes.Taiwan);
			supplier.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.TPC, "888888", Core.Constants.CountryCodes.Taiwan);
			supplier.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.AEO, "999999", Core.Constants.CountryCodes.Taiwan);
			supplierOrgAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "98555445", Core.Constants.CountryCodes.Taiwan);
			return supplier;
		}

		public OrgHeader CreateOrganizationForNotifyParty()
		{
			var notifyOrg = factory.NewWithValidTestData<OrgHeader>();
			notifyOrg.OH_Code = "Notify01";
			notifyOrg.OH_FullName = "XXX939393";
			notifyOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "N111111", Core.Constants.CountryCodes.Taiwan);
			notifyOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "N222222", Core.Constants.CountryCodes.Taiwan);
			notifyOrg.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "N333333", Core.Constants.CountryCodes.Taiwan);
			notifyOrg.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.AEO, "NAEO001", Core.Constants.CountryCodes.Taiwan);
			var notifyOrgAddress = notifyOrg.Addresses.AddNew();
			notifyOrgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			notifyOrgAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			notifyOrgAddress.OA_IsActive = true;
			notifyOrgAddress.OA_Language = Core.SharedConstants.Languages.English;
			notifyOrgAddress.OA_CompanyNameOverride = "Notify company name xxx.";
			notifyOrgAddress.OA_Address1 = "903/50 Clarence St,";
			notifyOrgAddress.OA_Address2 = "Sydney NSW 2000.";
			notifyOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;

			notifyOrgAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "NCPW001", Core.Constants.CountryCodes.Taiwan);
			notifyOrgAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "NCCP001", Core.Constants.CountryCodes.Taiwan);

			var notifyEnglishAddress = notifyOrgAddress.TranslatedAddresses.AddNew();
			notifyEnglishAddress.OTA_Language = Core.SharedConstants.Languages.English;
			notifyEnglishAddress.Address1 = "No. 3, Nanjing W. Rd., Datong Dist.,";
			notifyEnglishAddress.Address2 = "Taipei City 103, Taiwan (R.O.C.)";
			notifyEnglishAddress.CompanyName = "Notify company name(OTA).";
			var chineseNotifyTranslatedAddress = notifyOrgAddress.TranslatedAddresses.AddNew();
			chineseNotifyTranslatedAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			chineseNotifyTranslatedAddress.Address1 = "台北市松山區";
			chineseNotifyTranslatedAddress.Address2 = "民權東路四段342號";
			chineseNotifyTranslatedAddress.CompanyName = "通知人公司名稱(OTA).";
			return notifyOrg;
		}

		public OrgHeader CreateOrganizationForManufacturer()
		{
			var manufacturerOrg = factory.NewWithValidTestData<OrgHeader>();
			var manufacturerOrgOrgAddress = manufacturerOrg.Addresses.AddNew();
			manufacturerOrgOrgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			manufacturerOrgOrgAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			manufacturerOrgOrgAddress.OA_IsActive = true;
			manufacturerOrgOrgAddress.OA_Language = Core.SharedConstants.Languages.English;
			manufacturerOrgOrgAddress.OA_CompanyNameOverride = "X21XXXX3344";
			manufacturerOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "99966665", Core.Constants.CountryCodes.Taiwan);
			manufacturerOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "0123456789", Core.Constants.CountryCodes.Taiwan);
			return manufacturerOrg;
		}

		public OrgHeader CreateOrganizationForBuyer()
		{
			var buyerOrg = factory.NewWithValidTestData<OrgHeader>();
			buyerOrg.OH_Code = "BUYER01";
			buyerOrg.OH_FullName = "XX12BBCC12";
			buyerOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "B111111", Core.Constants.CountryCodes.Taiwan);
			buyerOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "B222222", Core.Constants.CountryCodes.Taiwan);
			buyerOrg.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "B333333", Core.Constants.CountryCodes.Taiwan);
			buyerOrg.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.AEO, "BAEO001", Core.Constants.CountryCodes.Taiwan);
			var buyerOrgAddress = buyerOrg.MainAddress;
			buyerOrgAddress.OA_IsActive = true;
			buyerOrgAddress.OA_Language = Core.SharedConstants.Languages.English;
			buyerOrgAddress.OA_CompanyNameOverride = "Buyer company name xxx.";
			buyerOrgAddress.OA_Address1 = "12345 address line1.";
			buyerOrgAddress.OA_Address2 = "12345 address line2.";
			buyerOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			buyerOrgAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "BCPW001", Core.Constants.CountryCodes.Taiwan);
			buyerOrgAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "BCCP001", Core.Constants.CountryCodes.Taiwan);

			var buyerTranslatedAddress = buyerOrgAddress.TranslatedAddresses.AddNew();
			buyerTranslatedAddress.OTA_Language = Core.SharedConstants.Languages.English;
			buyerTranslatedAddress.Address1 = "No. 195, Sec. 3, Jianguo N. Rd.,";
			buyerTranslatedAddress.Address2 = "Zhongshan Dist., Taipei City 104, Taiwan (R.O.C.)";
			buyerTranslatedAddress.CompanyName = "Buyer company name(OTA).";
			return buyerOrg;
		}

		public static void AddDeclarationReservedFields(JobDeclaration declaration)
		{
			var reservedField = declaration.ReservedFields.AddNew();
			reservedField.CY_Code = "A";
			reservedField.CY_Data = "AA";
			reservedField = declaration.ReservedFields.AddNew();
			reservedField.CY_Code = "B";
			reservedField.CY_Data = "BB";
			reservedField = declaration.ReservedFields.AddNew();
			reservedField.CY_Code = "1";
			reservedField.CY_Data = "11";
			reservedField = declaration.ReservedFields.AddNew();
			reservedField.CY_Code = "0";
			reservedField.CY_Data = "00";
		}

		public static void AddInvoiceLineReservedFields(JobComInvoiceLine invoiceLine)
		{
			var reservedField = invoiceLine.ReservedFields.AddNew();
			reservedField.CY_Code = "A";
			reservedField.CY_Data = "AA";
			reservedField = invoiceLine.ReservedFields.AddNew();
			reservedField.CY_Code = "B";
			reservedField.CY_Data = "BB";
			reservedField = invoiceLine.ReservedFields.AddNew();
			reservedField.CY_Code = "1";
			reservedField.CY_Data = "11";
			reservedField = invoiceLine.ReservedFields.AddNew();
			reservedField.CY_Code = "0";
			reservedField.CY_Data = "00";
		}

		public CusEntryHeader CreateEntryHeaderForN5203()
		{
			var declaration = factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			entryHeader.AllEntryLines.Add(entryLine);
			return entryHeader;
		}

		public JobComInvoiceLine CreateInvoiceLineForN5203(CusEntryHeader entryHeader)
		{
			var declaration = entryHeader.Declaration;
			var entryLine = entryHeader.MergedLines.First();
			var entryInstruction = entryHeader.EntryInstruction;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			return invoiceLine1;
		}

		readonly BusinessObjectFactory factory;
	}
}
