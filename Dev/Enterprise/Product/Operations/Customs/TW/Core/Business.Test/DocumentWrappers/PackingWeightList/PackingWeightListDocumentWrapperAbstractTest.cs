using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(PackingWeightListDocumentWrapper))]
	abstract class PackingWeightListDocumentWrapperAbstractTest<TPackingWeightListDocumentWrapper> : NonPersistentBusinessObjectTestCase
		where TPackingWeightListDocumentWrapper : PackingWeightListDocumentWrapper
	{
		PackingWeightListDocumentWrapper wrapper;
		protected override BusinessObject GetNewBusinessObject()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new PackingWeightListDocumentWrapper(jobDeclaration, Factory);
		}

		protected override void SetUp()
		{
			var helper = new PackingWeightListDocumentWrapperTestHelper(Factory);
			wrapper = new PackingWeightListDocumentWrapper(helper.Declaration, Factory);
			base.SetUp();
		}

		protected abstract TPackingWeightListDocumentWrapper GetPackingWeightListDocumentWrapper();

		[ExpectNoExceptions]
		public void TestNewWhenJobDeclarationIsNull()
		{
			NUnit.Framework.Assert.That(new PackingWeightListDocumentWrapper(null, Factory), NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.DocumentWrappers.PackingWeightListDocumentWrapper)));
		}

		[ExpectNoExceptions]
		public void TestExportAgentReference()
		{
			NUnit.Framework.Assert.That(wrapper.ExportAgentsReference, NUnit.Framework.Is.EqualTo("ABC").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMainSupplierDetailsAndSupplierName()
		{
			NUnit.Framework.Assert.That(wrapper.SupplierDetails, NUnit.Framework.Is.EqualTo("Supplier company name.\n88899 ADDRESS LINE1. 88899 ADDRESS LINE2. JAPAN").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.SupplierName, NUnit.Framework.Is.EqualTo("Supplier company name.").Using(CustomComparers.TypeComparison));
			var factory = new BusinessObjectFactory();
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			var supplierOrganization = factory.New<OrgHeader>();
			supplierOrganization.OH_Code = "org01";
			supplierOrganization.OH_RL_NKClosestPort = "TWKEL";
			var mainAddress = supplierOrganization.Addresses.MainAddress;
			mainAddress.CompanyName = "Main CompanyName";
			mainAddress.Address1 = "Main Address1";
			mainAddress.Address2 = "Main Address2";
			mainAddress.City = "Main City";
			mainAddress.State = "Main State";
			mainAddress.Postcode = "123";
			mainAddress.Language = "EN-GB";
			declaration.JE_OH_Supplier = supplierOrganization.PK;
			factory.Save();
			wrapper = new PackingWeightListDocumentWrapper(declaration, factory);
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_CompanyName = "XO001";
			supplierDocumentaryAddress.E2_Address1 = "A1";
			supplierDocumentaryAddress.E2_Address2 = "A2";
			supplierDocumentaryAddress.E2_City = "TPE";
			supplierDocumentaryAddress.E2_State = "TPE";
			supplierDocumentaryAddress.E2_RN_NKCountryCode = "TW";
			supplierDocumentaryAddress.E2_Postcode = "105";
			wrapper = new PackingWeightListDocumentWrapper(declaration, factory);
			NUnit.Framework.Assert.That(wrapper.SupplierDetails, NUnit.Framework.Is.EqualTo("XO001\nA1 A2 TPE 105 TAIWAN").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.SupplierName, NUnit.Framework.Is.EqualTo("XO001").Using(CustomComparers.TypeComparison));
			supplierDocumentaryAddress.E2_CompanyName = ZString.Empty;
			supplierDocumentaryAddress.E2_Address1 = ZString.Empty;
			supplierDocumentaryAddress.E2_Address2 = ZString.Empty;
			supplierDocumentaryAddress.E2_City = ZString.Empty;
			supplierDocumentaryAddress.E2_State = ZString.Empty;
			supplierDocumentaryAddress.E2_RN_NKCountryCode = ZString.Empty;
			supplierDocumentaryAddress.E2_Postcode = ZString.Empty;
			wrapper = new PackingWeightListDocumentWrapper(declaration, factory);
			NUnit.Framework.Assert.That(wrapper.SupplierDetails.Trim(), NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(wrapper.SupplierName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			supplierDocumentaryAddress.E2_AddressOverride = false;
			var enAddress = declaration.Supplier.Addresses.AddNew();
			enAddress.OA_RN_NKCountryCode = "TW";
			enAddress.OA_CompanyNameOverride = "Supplier company name e2.";
			enAddress.OA_Language = Core.SharedConstants.Languages.English;
			enAddress.OA_Address1 = "ADDRESS 11";
			enAddress.OA_Address2 = "ADDRESS 22";
			enAddress.OA_PostCode = "106";
			var supplierChineseAddress = enAddress.TranslatedAddresses.AddNew();
			supplierChineseAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			supplierChineseAddress.Address1 = "忠孝東路1";
			supplierChineseAddress.Address2 = "三段232號1";
			supplierChineseAddress.CompanyName = "公司名稱X12(OTA)";
			var supplierEnglishAddress = enAddress.TranslatedAddresses.AddNew();
			supplierEnglishAddress.OTA_Language = Core.SharedConstants.Languages.English;
			supplierEnglishAddress.Address1 = "No. 232, Sec. 5, ZhongXiao N. Rd.,";
			supplierEnglishAddress.Address2 = "Zhongshan Dist., Taipei City 106, Taiwan (R.O.C.)";
			supplierEnglishAddress.CompanyName = "Supplier2 company name(OTA).";
			var cnAddress = declaration.Supplier.Addresses.AddNew();
			cnAddress.OA_RN_NKCountryCode = "TW";
			cnAddress.OA_CompanyNameOverride = "Supplier company name e3.";
			cnAddress.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
			cnAddress.OA_Address1 = "地址1";
			cnAddress.OA_Address2 = "地址2";
			cnAddress.OA_PostCode = "108";
			supplierChineseAddress = cnAddress.TranslatedAddresses.AddNew();
			supplierChineseAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			supplierChineseAddress.Address1 = "忠孝東路1";
			supplierChineseAddress.Address2 = "三段232號1";
			supplierChineseAddress.CompanyName = "公司名稱X12 (OTA)";
			supplierEnglishAddress = cnAddress.TranslatedAddresses.AddNew();
			supplierEnglishAddress.OTA_Language = Core.SharedConstants.Languages.English;
			supplierEnglishAddress.Address1 = "No. 232, Sec. 8, ZhongXiao N. Rd.,";
			supplierEnglishAddress.Address2 = "Zhongshan Dist., Taipei City 109, Taiwan (R.O.C.)";
			supplierEnglishAddress.CompanyName = "Supplier3 company name(OTA).";
			supplierEnglishAddress.OTA_PostCode = "65";
			supplierDocumentaryAddress.E2_OA_Address = enAddress.PK;
			factory.Save();
			wrapper = new PackingWeightListDocumentWrapper(declaration, factory);
			NUnit.Framework.Assert.That(wrapper.SupplierDetails, NUnit.Framework.Is.EqualTo("Supplier company name e2.\nADDRESS 11 ADDRESS 22 106 TAIWAN").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.SupplierName, NUnit.Framework.Is.EqualTo("Supplier company name e2.").Using(CustomComparers.TypeComparison));
			supplierDocumentaryAddress.E2_OA_Address = cnAddress.PK;
			factory.Save();
			wrapper = new PackingWeightListDocumentWrapper(declaration, factory);
			NUnit.Framework.Assert.That(wrapper.SupplierDetails, NUnit.Framework.Is.EqualTo("Supplier3 company name(OTA).\nNO. 232, SEC. 8, ZHONGXIAO N. RD., ZHONGSHAN DIST., TAIPEI CITY 109, TAIWAN (R.O.C.) 65 TAIWAN").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.SupplierName, NUnit.Framework.Is.EqualTo("Supplier3 company name(OTA).").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestImporterDetails()
		{
			NUnit.Framework.Assert.That(wrapper.ImporterDetails, NUnit.Framework.Is.EqualTo("Company 1111222\nXX231 321XX TAIWAN").Using(CustomComparers.TypeComparison));
			var factory = new BusinessObjectFactory();
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			var importerOrganization = factory.New<OrgHeader>();
			importerOrganization.OH_Code = "org01";
			importerOrganization.OH_RL_NKClosestPort = "TWKEL";
			var mainAddress = importerOrganization.Addresses.MainAddress;
			mainAddress.CompanyName = "Main CompanyName";
			mainAddress.Address1 = "Main Address1";
			mainAddress.Address2 = "Main Address2";
			mainAddress.City = "Main City";
			mainAddress.State = "Main State";
			mainAddress.Postcode = "123";
			mainAddress.Language = "EN-GB";
			declaration.JE_OH_Importer = importerOrganization.PK;
			factory.Save();
			wrapper = new PackingWeightListDocumentWrapper(declaration, factory);
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.E2_CompanyName = "XO001";
			importerDocumentaryAddress.E2_Address1 = "A1";
			importerDocumentaryAddress.E2_Address2 = "A2";
			importerDocumentaryAddress.E2_City = "TPE";
			importerDocumentaryAddress.E2_State = "TPE";
			importerDocumentaryAddress.E2_RN_NKCountryCode = "TW";
			importerDocumentaryAddress.E2_Postcode = "105";
			wrapper = new PackingWeightListDocumentWrapper(declaration, factory);
			NUnit.Framework.Assert.That(wrapper.ImporterDetails, NUnit.Framework.Is.EqualTo("XO001\nA1 A2 TPE 105 TAIWAN").Using(CustomComparers.TypeComparison));
			importerDocumentaryAddress.E2_CompanyName = ZString.Empty;
			importerDocumentaryAddress.E2_Address1 = ZString.Empty;
			importerDocumentaryAddress.E2_Address2 = ZString.Empty;
			importerDocumentaryAddress.E2_City = ZString.Empty;
			importerDocumentaryAddress.E2_State = ZString.Empty;
			importerDocumentaryAddress.E2_RN_NKCountryCode = ZString.Empty;
			importerDocumentaryAddress.E2_Postcode = ZString.Empty;
			wrapper = new PackingWeightListDocumentWrapper(declaration, factory);
			NUnit.Framework.Assert.That(wrapper.ImporterDetails.Trim(), NUnit.Framework.Is.EqualTo(ZString.Empty));
			importerDocumentaryAddress.E2_AddressOverride = false;
			var enAddress = declaration.Importer.Addresses.AddNew();
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
			var cnAddress = declaration.Importer.Addresses.AddNew();
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
			factory.Save();
			wrapper = new PackingWeightListDocumentWrapper(declaration, factory);
			NUnit.Framework.Assert.That(wrapper.ImporterDetails, NUnit.Framework.Is.EqualTo("Importer company name e2.\nADDRESS 11 ADDRESS 22 106 TAIWAN").Using(CustomComparers.TypeComparison));
			importerDocumentaryAddress.E2_OA_Address = cnAddress.PK;
			factory.Save();
			wrapper = new PackingWeightListDocumentWrapper(declaration, factory);
			NUnit.Framework.Assert.That(wrapper.ImporterDetails, NUnit.Framework.Is.EqualTo("Importer3 company name(OTA).\nNO. 232, SEC. 8, ZHONGXIAO N. RD., ZHONGSHAN DIST., TAIPEI CITY 109, TAIWAN (R.O.C.) 65 TAIWAN").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPortOfOrigin()
		{
			RefUNLOCO originUNLOCO = Factory.New<RefUNLOCO>();
			originUNLOCO.RL_Code = "TWTPE";
			originUNLOCO.Description = "origin desc";
			originUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_RL_NKOrigin = "TWTPE";
			var docWrapper = new InvoicePackingWeightListDocumentWrapper(invoiceHeader, Factory);
			NUnit.Framework.Assert.That(docWrapper.PortOfOrigin, NUnit.Framework.Is.EqualTo("Taiwan - origin desc").Using(CustomComparers.TypeComparison));
			declaration.JE_RL_NKOrigin = "TWZZZ";
			docWrapper = new InvoicePackingWeightListDocumentWrapper(invoiceHeader, Factory);
			NUnit.Framework.Assert.That(docWrapper.PortOfOrigin, NUnit.Framework.Is.EqualTo("Taiwan - ").Using(CustomComparers.TypeComparison));
			declaration.JE_RL_NKOrigin = "TWZ99";
			declaration.JE_Z99PortOfOrigin = "Z99 desc";
			docWrapper = new InvoicePackingWeightListDocumentWrapper(invoiceHeader, Factory);
			NUnit.Framework.Assert.That(docWrapper.PortOfOrigin, NUnit.Framework.Is.EqualTo("Taiwan - Z99 desc").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestFinalDestination()
		{
			RefUNLOCO destinationUNLOCO = Factory.New<RefUNLOCO>();
			destinationUNLOCO.RL_Code = "TWTPE";
			destinationUNLOCO.Description = "origin desc";
			destinationUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_RL_NKFinalDestination = "TWTPE";
			var docWrapper = new InvoicePackingWeightListDocumentWrapper(invoiceHeader, Factory);
			NUnit.Framework.Assert.That(docWrapper.FinalDestination, NUnit.Framework.Is.EqualTo("Taiwan - origin desc").Using(CustomComparers.TypeComparison));
			declaration.JE_RL_NKFinalDestination = "TWZZZ";
			docWrapper = new InvoicePackingWeightListDocumentWrapper(invoiceHeader, Factory);
			NUnit.Framework.Assert.That(docWrapper.FinalDestination, NUnit.Framework.Is.EqualTo("Taiwan - ").Using(CustomComparers.TypeComparison));
			declaration.JE_RL_NKFinalDestination = "TWZ99";
			declaration.JE_Z99FinalDestination = "Z99 desc";
			docWrapper = new InvoicePackingWeightListDocumentWrapper(invoiceHeader, Factory);
			NUnit.Framework.Assert.That(docWrapper.FinalDestination, NUnit.Framework.Is.EqualTo("Taiwan - Z99 desc").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTransportModeAndVessel()
		{
			NUnit.Framework.Assert.That(wrapper.TransportMode, NUnit.Framework.Is.EqualTo(TransportTypeList.Codes.Sea).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Vessel, NUnit.Framework.Is.EqualTo("VESSELNAME").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public virtual void TestMarksAndNumbers()
		{
			NUnit.Framework.Assert.That(wrapper.MarksAndNumbers, NUnit.Framework.Is.EqualTo(@"XXX1
MARKS AND NUMBER 1
MARKS AND NUMBER 2
MARKS AND NUMBER 3
MARKS AND NUMBER 4
MARKS AND NUMBER 5
MARKS AND NUMBER 6
MARKS AND NUMBER 7
MARKS AND NUMBER 8
MARKS AND NUMBER 9
MARKS AND NUMBER 10
MARKS AND NUMBER 11
MARKS AND NUMBER 12
MARKS AND NUMBER 13
MARKS AND NUMBER 14
MARKS AND NUMBER 15
MARKS AND NUMBER 16
MARKS AND NUMBER 17
MARKS AND NUMBER 18
MARKS AND NUMBER 19
MARKS AND NUMBER 20
MARKS AND NUMBER 21
MARKS AND NUMBER 22
MARKS AND NUMBER 23
MARKS AND NUMBER 24
MARKS AND NUMBER25
MARKS AND NUMBER 26
MARKS AND NUMBER 27
MARKS AND NUMBER 28
MARKS AND NUMBER 29
MARKS AND NUMBER 30
MARKS AND NUMBER 31
MARKS AND NUMBER 32
MARKS AND NUMBER 33
MARKS AND NUMBER 34
MARKS AND NUMBER 35
MARKS AND NUMBER 36
MARKS AND NUMBER 37
MARKS AND NUMBER 38
MARKS AND NUMBER 39
MARKS AND NUMBER 40
MARKS AND NUMBER 41
MARKS AND NUMBER 42
MARKS AND NUMBER 43
MARKS AND NUMBER 44
MARKS AND NUMBER 45
MARKS AND NUMBER 46
MARKS AND NUMBER 47
MARKS AND NUMBER 48
MARKS AND NUMBER 49").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.MarksAndNumbersPart1, NUnit.Framework.Is.EqualTo("Marks and numbers will be printed below...").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.MarksAndNumbersPart2, NUnit.Framework.Is.EqualTo(@"XXX1
MARKS AND NUMBER 1
MARKS AND NUMBER 2
MARKS AND NUMBER 3
MARKS AND NUMBER 4
MARKS AND NUMBER 5
MARKS AND NUMBER 6
MARKS AND NUMBER 7
MARKS AND NUMBER 8
MARKS AND NUMBER 9
MARKS AND NUMBER 10
MARKS AND NUMBER 11
MARKS AND NUMBER 12
MARKS AND NUMBER 13
MARKS AND NUMBER 14
MARKS AND NUMBER 15
MARKS AND NUMBER 16
MARKS AND NUMBER 17
MARKS AND NUMBER 18
MARKS AND NUMBER 19
MARKS AND NUMBER 20
MARKS AND NUMBER 21
MARKS AND NUMBER 22
MARKS AND NUMBER 23
MARKS AND NUMBER 24
MARKS AND NUMBER25
MARKS AND NUMBER 26
MARKS AND NUMBER 27
MARKS AND NUMBER 28
MARKS AND NUMBER 29
MARKS AND NUMBER 30
MARKS AND NUMBER 31
MARKS AND NUMBER 32
MARKS AND NUMBER 33
MARKS AND NUMBER 34
MARKS AND NUMBER 35
MARKS AND NUMBER 36
MARKS AND NUMBER 37
MARKS AND NUMBER 38
MARKS AND NUMBER 39
MARKS AND NUMBER 40
MARKS AND NUMBER 41
MARKS AND NUMBER 42
MARKS AND NUMBER 43
MARKS AND NUMBER 44
MARKS AND NUMBER 45
MARKS AND NUMBER 46
MARKS AND NUMBER 47
MARKS AND NUMBER 48
MARKS AND NUMBER 49").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public virtual void TestRemarks()
		{
			NUnit.Framework.Assert.That(wrapper.Remarks, NUnit.Framework.Is.EqualTo(@"Remarks1
Remarks2").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDocDataFields()
		{
			NUnit.Framework.Assert.That(wrapper.PackingListNumber, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.PackDate, NUnit.Framework.Is.EqualTo("1/1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPackingLinesCount()
		{
			NUnit.Framework.Assert.That(wrapper.PackingLines.Count, NUnit.Framework.Is.EqualTo(3));
		}

		[ExpectNoExceptions]
		public void TestPackNoInfo()
		{
			NUnit.Framework.Assert.That(wrapper.PackingLines[0].PackNoInfo, NUnit.Framework.Is.EqualTo("XXX1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.PackingLines[1].PackNoInfo, NUnit.Framework.Is.EqualTo("XXX2").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.PackingLines[2].PackNoInfo, NUnit.Framework.Is.EqualTo("XXX2").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSummaryLine()
		{
			NUnit.Framework.Assert.That(wrapper.PackTypeSummary, NUnit.Framework.Is.EqualTo("5 2\n4 4\r\nvvvvvvvvvvvvv").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.QuantitySummary, NUnit.Framework.Is.EqualTo("6 LB\n5 T\r\nvvvvvvvvvvvvvvv").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.NetWeightSummary, NUnit.Framework.Is.EqualTo("5.005 KG\n2.004 LB\r\nvvvvvvvvvvvvvvv").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.GrossWeightSummary, NUnit.Framework.Is.EqualTo("5.005 KG\n2.004 LB\r\nvvvvvvvvvvvvvvv").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.VolumeSummary, NUnit.Framework.Is.EqualTo("7.009 CBM\r\nvvvvvvvvvvvvvvv").Using(CustomComparers.TypeComparison));
		}

		internal sealed class PackingWeightListDocumentWrapperTestHelper
		{
			public PackingWeightListDocumentWrapperTestHelper(BusinessObjectFactory factory)
			{
				Factory = factory;
				GenerateTestData();
			}

			BusinessObjectFactory Factory
			{
				get;
			}

			void GenerateTestData()
			{
				var supplierOrg = Factory.NewWithValidTestData<OrgHeader>();
				supplierOrg.OH_RL_NKClosestPort = "TWKEL";
				var supplierOrgAddress = supplierOrg.Addresses.AddNew();
				supplierOrgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
				supplierOrgAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
				supplierOrgAddress.OA_IsActive = true;
				supplierOrgAddress.OA_Language = Core.SharedConstants.Languages.English;
				supplierOrgAddress.OA_CompanyNameOverride = "Supplier company name.";
				supplierOrgAddress.OA_Address1 = "88899 address line1.";
				supplierOrgAddress.OA_Address2 = "88899 address line2.";
				supplierOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
				supplierOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "5555555", Core.Constants.CountryCodes.Taiwan);
				supplierOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "6666666", Core.Constants.CountryCodes.Taiwan);
				supplierOrg.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "777777", Core.Constants.CountryCodes.Taiwan);
				supplierOrg.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.TPC, "888888", Core.Constants.CountryCodes.Taiwan);
				supplierOrg.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.AEO, "999999", Core.Constants.CountryCodes.Taiwan);
				supplierOrgAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "98555445", Core.Constants.CountryCodes.Taiwan);
				var importerOrg = Factory.NewWithValidTestData<OrgHeader>();
				importerOrg.OH_RL_NKClosestPort = "TWKEL";
				var importerOrgAddress = importerOrg.Addresses.AddNew();
				importerOrgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
				importerOrgAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
				importerOrgAddress.OA_IsActive = true;
				importerOrgAddress.OA_Language = Core.SharedConstants.Languages.English;
				importerOrgAddress.OA_CompanyNameOverride = "Company 1111222";
				importerOrgAddress.OA_Address1 = "XX231";
				importerOrgAddress.OA_Address2 = "321XX";
				importerOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
				importerOrgAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "98654785", Core.Constants.CountryCodes.Taiwan);
				importerOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123ABC", Core.Constants.CountryCodes.Taiwan);
				var refVessel = Factory.New<RefVessel>();
				refVessel.RV_LloydsNumber = "0982432";
				refVessel.RV_Code = "VESSELNAME";
				refVessel.RV_RadioCallSign = "X232";
				var borker = Factory.NewWithValidTestData<GlbStaff>();
				var brkCertificate = borker.Certificates.AddNew();
				brkCertificate.XZ_Type = CertificateTypePairList.Codes.BR1;
				brkCertificate.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Taiwan;
				brkCertificate.XZ_RefNumber = "1234";
				Declaration = Factory.New<JobDeclaration>();
				Declaration.JE_CustomsProfile = "AAA-BBB";
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
				Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				Declaration.JE_OH_Supplier = supplierOrg.PK;
				Declaration.JE_OH_Importer = importerOrg.PK;
				Declaration.JE_GS_NKCusAgent = borker.GS_Code;
				Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
				Declaration.JE_VesselName = refVessel.RV_Code;
				Declaration.JE_RL_NKPortOfLoading = Core.Constants.CountryCodes.Taiwan;
				Declaration.JE_RL_NKPortOfArrival = Core.Constants.CountryCodes.Taiwan;
				Declaration.JE_AgentsReference = "ABC";
				Declaration.DocNote.SetSystemDefinedFieldValue("Packing List Number", "123");
				Declaration.DocNote.SetSystemDefinedFieldValue("Pack Date", "1/1");
				var manufacturerOrg = Factory.NewWithValidTestData<OrgHeader>();
				var manufacturerOrgOrgAddress = manufacturerOrg.Addresses.AddNew();
				manufacturerOrgOrgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
				manufacturerOrgOrgAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
				manufacturerOrgOrgAddress.OA_IsActive = true;
				manufacturerOrgOrgAddress.OA_Language = Core.SharedConstants.Languages.English;
				manufacturerOrgOrgAddress.OA_CompanyNameOverride = "X21XXXX3344";
				manufacturerOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "99966665", Core.Constants.CountryCodes.Taiwan);
				#region Invoice1
				Invoice1 = Declaration.Invoices.AddNew();
				Invoice1.JZ_MarksAndNumbers = "XXX1";
				Invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				Invoice1.JZ_InvoiceCurrExRate = 31.37m;
				Invoice1.JZ_OH_Supplier = supplierOrg.PK;
				Invoice1.JZ_InvoiceNumber = "INV001";
				Invoice1.JZ_Remarks = "Remarks1";
				#region InvoiceLine1
				var classification = Factory.New<BaseCusClassification>();
				classification.CC_Description = "CUCKOO SQUEAKERS";
				classification.CC_LookupCode = "CKSQKS";
				classification.CC_TariffNum = "0000.00.00.00Y";
				InvoiceLine1 = Invoice1.JobComInvoiceLines.AddNew();
				InvoiceLine1.JI_InvoiceQuantity = 4;
				InvoiceLine1.JI_InvoiceUQ = Core.Constants.Weight.Pounds;
				InvoiceLine1.JI_NDescription = "Goods Description 1";
				var package1 = Declaration.Packages.AddNew();
				package1.CW_PackQty = 3;
				package1.CW_NetWeight = 3.003;
				package1.CW_NetWeightUQ = Core.Constants.Weight.Kilograms;
				package1.CW_GrossWeight = 3.003;
				package1.CW_GrossWeightUQ = Core.Constants.Weight.Kilograms;
				package1.CW_Volume = 3.003;
				package1.CW_VolumeUQ = Core.Constants.Volume.CubicMetres;
				package1.CW_PackType = CPT_115_InnerPackageTypeList.Codes._2;
				package1.CW_MarksAndNos = "XXX1";
				var supporter1 = (ICusLinkPackageSupporter)InvoiceLine1;
				var pivot1 = (ICusQuantityPivot)supporter1.CusPackPivots.AddPivotFor(package1);
				pivot1.Quantity = InvoiceLine1.JI_InvoiceQuantity;
				#endregion
				#region InvoiceLine2
				var invoiceLine2 = Invoice1.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_InvoiceQuantity = 2;
				invoiceLine2.JI_InvoiceUQ = Core.Constants.Weight.Pounds;
				invoiceLine2.JI_NDescription = "Goods Description 2";
				var package2 = Declaration.Packages.AddNew();
				package2.CW_PackQty = 2;
				package2.CW_NetWeight = 2.002;
				package2.CW_NetWeightUQ = Core.Constants.Weight.Kilograms;
				package2.CW_GrossWeight = 2.002;
				package2.CW_GrossWeightUQ = Core.Constants.Weight.Kilograms;
				package2.CW_Volume = 2.002;
				package2.CW_VolumeUQ = Core.Constants.Volume.CubicMetres;
				package2.CW_PackType = CPT_115_InnerPackageTypeList.Codes._2;
				package2.CW_MarksAndNos = "XXX2";
				var supporter2 = (ICusLinkPackageSupporter)invoiceLine2;
				var pivot2 = (ICusQuantityPivot)supporter2.CusPackPivots.AddPivotFor(package2);
				pivot2.Quantity = invoiceLine2.JI_InvoiceQuantity;
				#endregion
				#region InvoiceLine3
				var invoiceLine3 = Invoice1.JobComInvoiceLines.AddNew();
				invoiceLine3.JI_InvoiceQuantity = 5;
				invoiceLine3.JI_NDescription = "Line 3 Desc";
				invoiceLine3.JI_InvoiceUQ = Core.Constants.Weight.Tonnes;
				var package3 = Declaration.Packages.AddNew();
				package3.CW_PackQty = 4;
				package3.CW_NetWeight = 2.004;
				package3.CW_NetWeightUQ = Core.Constants.Weight.Pounds;
				package3.CW_GrossWeight = 2.004;
				package3.CW_GrossWeightUQ = Core.Constants.Weight.Pounds;
				package3.CW_Volume = 2.004;
				package3.CW_VolumeUQ = Core.Constants.Volume.CubicMetres;
				package3.CW_PackType = CPT_115_InnerPackageTypeList.Codes._4;
				package3.CW_MarksAndNos = "XXX2";
				var supporter3 = (ICusLinkPackageSupporter)invoiceLine3;
				var pivot3 = (ICusQuantityPivot)supporter3.CusPackPivots.AddPivotFor(package3);
				pivot3.Quantity = invoiceLine3.JI_InvoiceQuantity;
				#endregion
				#endregion
				#region Invoice2
				Invoice2 = Declaration.Invoices.AddNew();
				Invoice2.JZ_Remarks = "Remarks2";
				Invoice2.TW_MarksAndNumbers = @"MARKS AND NUMBER 1
MARKS AND NUMBER 2
MARKS AND NUMBER 3
MARKS AND NUMBER 4
MARKS AND NUMBER 5
MARKS AND NUMBER 6
MARKS AND NUMBER 7
MARKS AND NUMBER 8
MARKS AND NUMBER 9
MARKS AND NUMBER 10
MARKS AND NUMBER 11
MARKS AND NUMBER 12
MARKS AND NUMBER 13
MARKS AND NUMBER 14
MARKS AND NUMBER 15
MARKS AND NUMBER 16
MARKS AND NUMBER 17
MARKS AND NUMBER 18
MARKS AND NUMBER 19
MARKS AND NUMBER 20
MARKS AND NUMBER 21
MARKS AND NUMBER 22
MARKS AND NUMBER 23
MARKS AND NUMBER 24
MARKS AND NUMBER 25
MARKS AND NUMBER 26
MARKS AND NUMBER 27
MARKS AND NUMBER 28
MARKS AND NUMBER 29
MARKS AND NUMBER 30
MARKS AND NUMBER 31
MARKS AND NUMBER 32
MARKS AND NUMBER 33
MARKS AND NUMBER 34
MARKS AND NUMBER 35
MARKS AND NUMBER 36
MARKS AND NUMBER 37
MARKS AND NUMBER 38
MARKS AND NUMBER 39
MARKS AND NUMBER 40
MARKS AND NUMBER 41
MARKS AND NUMBER 42
MARKS AND NUMBER 43
MARKS AND NUMBER 44
MARKS AND NUMBER 45
MARKS AND NUMBER 46
MARKS AND NUMBER 47
MARKS AND NUMBER 48
MARKS AND NUMBER 49";
				Invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
				Invoice2.JZ_InvoiceCurrExRate = 35.60m;
				#region InvoiceLine4
				var invoiceLine4 = Invoice2.JobComInvoiceLines.AddNew();
				invoiceLine4.JI_InvoiceQuantity = 2;
				invoiceLine4.JI_NDescription = "Invoice 2 line 4";
				invoiceLine4.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;
				var supporter4 = (ICusLinkPackageSupporter)invoiceLine4;
				var pivot4 = (ICusQuantityPivot)supporter4.CusPackPivots.AddPivotFor(package3);
				pivot4.Quantity = invoiceLine4.JI_InvoiceQuantity;
				#endregion
				#endregion
				EntryLine = InvoiceLine1.CusEntryLine;
			}

			public JobDeclaration Declaration;
			public JobComInvoiceHeader Invoice1
			{
				get;
				private set;
			}

			public JobComInvoiceHeader Invoice2
			{
				get;
				private set;
			}

			public CusEntryLine EntryLine
			{
				get;
				private set;
			}

			public JobComInvoiceLine InvoiceLine1
			{
				get;
				private set;
			}
		}
	}
}
