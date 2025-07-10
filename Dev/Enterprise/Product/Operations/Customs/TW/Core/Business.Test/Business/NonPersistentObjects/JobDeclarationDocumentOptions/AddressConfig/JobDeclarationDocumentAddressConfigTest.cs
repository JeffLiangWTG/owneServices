using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobDeclarationDocumentAddressConfig))]
	sealed class JobDeclarationDocumentAddressConfigTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCopyConfigParams()
		{
			var decl1 = Factory.New<JobDeclaration>();
			var config1 = new JobDeclarationDocumentAddressConfig(decl1);
			config1.DocumentName = "name1";
			config1.HideEXPExporterTradChineseAddr = true;
			config1.HideEXPExporterEnglishAddr = true;
			config1.HideEXPBuyerTradChineseAddr = false;
			config1.HideEXPBuyerEnglishAddr = true;
			config1.HideIMPImporterTradChineseAddr = true;
			config1.HideIMPImporterEnglishAddr = true;
			config1.HideIMPSellerTradChineseAddr = true;
			config1.CustomizeSectionBodyRow = "12";
			config1.HideCustomizeSectionBodyRow = true;
			config1.GoodsDescriptionConfigs.AddNew();
			config1.GoodsDescriptionConfigs.AddNew();

			var decl2 = Factory.New<JobDeclaration>();
			var config2 = new JobDeclarationDocumentAddressConfig(decl2);
			CombineAssertions(() =>
			{
				config2.CopyConfigParams(config1);
				AssertEquals("DocumentName", "name1", config2.DocumentName);
				AssertEquals("HideEXPExporterTradChineseAddr", true, config2.HideEXPExporterTradChineseAddr);
				AssertEquals("HideEXPExporterEnglishAddr", true, config2.HideEXPExporterEnglishAddr);
				AssertEquals("HideEXPBuyerTradChineseAddr", false, config2.HideEXPBuyerTradChineseAddr);
				AssertEquals("HideEXPBuyerEnglishAddr", true, config2.HideEXPBuyerEnglishAddr);
				AssertEquals("HideIMPImporterTradChineseAddr", true, config2.HideIMPImporterTradChineseAddr);
				AssertEquals("HideIMPImporterEnglishAddr", true, config2.HideIMPImporterEnglishAddr);
				AssertEquals("HideIMPSellerTradChineseAddr", true, config2.HideIMPSellerTradChineseAddr);
				AssertEquals("CustomizeSectionBodyRow", "12", config2.CustomizeSectionBodyRow);
				AssertEquals("HideCustomizeSectionBodyRow", true, config2.HideCustomizeSectionBodyRow);
				AssertEquals("GoodsDescriptionConfigs", 2, config2.GoodsDescriptionConfigs.Count);
			});
		}

		public void TestSetDefaultValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			var jobDeclarationDocumentAddressConfig = new JobDeclarationDocumentAddressConfig(declaration);
			Assert(!jobDeclarationDocumentAddressConfig.AddressConfigPK.IsEmpty);
		}

		public void TestSetDefaultJobDeclarationDocumentAddressConfig()
		{
			var importerOrg = Factory.New<OrgHeader>();
			var importerOrgImpAddInfo = TWOrgImpAddInfo.Get(importerOrg);
			importerOrgImpAddInfo.ZO_TWHideIMPImporterZHTAddr = true;
			importerOrgImpAddInfo.ZO_TWHideIMPImporterAddr = true;
			importerOrgImpAddInfo.ZO_TWHideIMPSellerZHTAddr = true;
			var supplierOrg = Factory.NewWithValidTestData<OrgHeader>();
			var supplierOrgImpAddInfo = TWOrgImpAddInfo.Get(supplierOrg);
			supplierOrgImpAddInfo.ZO_TWHideEXPExporterZHTAddr = true;
			supplierOrgImpAddInfo.ZO_TWHideEXPExporterAddr = true;
			supplierOrgImpAddInfo.ZO_TWHideEXPBuyerZHTAddr = true;
			supplierOrgImpAddInfo.ZO_TWHideEXPBuyerAddr = true;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierOrg.MainAddress.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importerOrg.MainAddress.PK;
			var documentSupporter = declaration.DocumentSupporter;
			var menuItemForTesting = Factory.New<IStmMenuItem>();
			menuItemForTesting.SU_MenuName = "Test Export Customs Declaration";
			documentSupporter.GetDataStateBeforeRun(menuItemForTesting);
			var jobDeclarationDocumentAddressConfig = documentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			CombineAssertions(() =>
			{
				AssertEquals("DocumentName", "Test Export Customs Declaration", jobDeclarationDocumentAddressConfig.DocumentName);
				AssertEquals("HideEXPExporterTradChineseAddr", true, jobDeclarationDocumentAddressConfig.HideEXPExporterTradChineseAddr);
				AssertEquals("HideEXPExporterEnglishAddr", true, jobDeclarationDocumentAddressConfig.HideEXPExporterEnglishAddr);
				AssertEquals("HideEXPBuyerTradChineseAddr", false, jobDeclarationDocumentAddressConfig.HideEXPBuyerTradChineseAddr);
				AssertEquals("HideEXPBuyerEnglishAddr", false, jobDeclarationDocumentAddressConfig.HideEXPBuyerEnglishAddr);
			});

			declaration.JE_MessageType = "IMP";
			menuItemForTesting.SU_MenuName = "Test Import Customs Declaration";
			documentSupporter = declaration.DocumentSupporter;
			documentSupporter.GetDataStateBeforeRun(menuItemForTesting);
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			CombineAssertions(() =>
			{
				AssertEquals("DocumentName", "Test Import Customs Declaration", jobDeclarationDocumentAddressConfig.DocumentName);
				AssertEquals("HideIMPImporterTradChineseAddr", true, jobDeclarationDocumentAddressConfig.HideIMPImporterTradChineseAddr);
				AssertEquals("HideIMPImporterEnglishAddr", true, jobDeclarationDocumentAddressConfig.HideIMPImporterEnglishAddr);
				AssertEquals("HideIMPSellerTradChineseAddr", false, jobDeclarationDocumentAddressConfig.HideIMPSellerTradChineseAddr);
			});

			importerOrgImpAddInfo.ZO_TWHideIMPImporterZHTAddr = false;
			importerOrgImpAddInfo.ZO_TWHideIMPImporterAddr = false;
			importerOrgImpAddInfo.ZO_TWHideIMPSellerZHTAddr = false;
			importerOrgImpAddInfo.ZO_TWHideEXPExporterZHTAddr = true;
			importerOrgImpAddInfo.ZO_TWHideEXPExporterAddr = true;
			importerOrgImpAddInfo.ZO_TWHideEXPBuyerZHTAddr = true;
			importerOrgImpAddInfo.ZO_TWHideEXPBuyerAddr = true;
			supplierOrgImpAddInfo.ZO_TWHideIMPImporterZHTAddr = true;
			supplierOrgImpAddInfo.ZO_TWHideIMPImporterAddr = true;
			supplierOrgImpAddInfo.ZO_TWHideIMPSellerZHTAddr = true;
			supplierOrgImpAddInfo.ZO_TWHideEXPExporterZHTAddr = false;
			supplierOrgImpAddInfo.ZO_TWHideEXPExporterAddr = false;
			supplierOrgImpAddInfo.ZO_TWHideEXPBuyerZHTAddr = false;
			supplierOrgImpAddInfo.ZO_TWHideEXPBuyerAddr = false;

			declaration.JE_MessageType = "IMP";
			documentSupporter = declaration.DocumentSupporter;
			documentSupporter.GetDataStateBeforeRun(menuItemForTesting);
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			CombineAssertions(() =>
			{
				AssertEquals("DocumentName", "Test Import Customs Declaration", jobDeclarationDocumentAddressConfig.DocumentName);
				AssertEquals("HideIMPImporterTradChineseAddr", false, jobDeclarationDocumentAddressConfig.HideIMPImporterTradChineseAddr);
				AssertEquals("HideIMPImporterEnglishAddr", false, jobDeclarationDocumentAddressConfig.HideIMPImporterEnglishAddr);
				AssertEquals("HideIMPSellerTradChineseAddr", true, jobDeclarationDocumentAddressConfig.HideIMPSellerTradChineseAddr);
			});

			declaration.JE_MessageType = "EXP";
			menuItemForTesting.SU_MenuName = "Test Export Customs Declaration";
			documentSupporter = declaration.DocumentSupporter;
			documentSupporter.GetDataStateBeforeRun(menuItemForTesting);
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			CombineAssertions(() =>
			{
				AssertEquals("DocumentName", "Test Export Customs Declaration", jobDeclarationDocumentAddressConfig.DocumentName);
				AssertEquals("HideEXPExporterTradChineseAddr", false, jobDeclarationDocumentAddressConfig.HideEXPExporterTradChineseAddr);
				AssertEquals("HideEXPExporterEnglishAddr", false, jobDeclarationDocumentAddressConfig.HideEXPExporterEnglishAddr);
				AssertEquals("HideEXPBuyerTradChineseAddr", true, jobDeclarationDocumentAddressConfig.HideEXPBuyerTradChineseAddr);
				AssertEquals("HideEXPBuyerEnglishAddr", true, jobDeclarationDocumentAddressConfig.HideEXPBuyerEnglishAddr);
			});
		}

		public void TestSetDefaultJobDeclarationDocumentGoodsDescriptionConfigCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var documentSupporter = declaration.DocumentSupporter;
			var jobDeclarationDocumentAddressConfig = documentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>();
			CombineAssertions("Test Export without Custom Document Labels", () =>
			{
				AssertContainsExactElementsInExactOrder("Position", new ZShort[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, goodsDescriptionConfigs.Select(c => c.Position));
				AssertContainsExactElementsInExactOrder("Caption", new[] { "賣方料號:", "買方料號:", "", "型號:", "規格:", "原進倉報單號碼/項次:", "原報單號碼/項次:", "輸出入許可文件號碼/項次:", "產地證明書號碼/項次:", "主管機關指定代號:", "生產國別:" }, goodsDescriptionConfigs.Select(c => c.Caption));
				AssertContainsExactElementsInExactOrder("Field",
					new[] { ExportDeclarationDocumentFieldList.Codes.SupplierPartNumber,
							ExportDeclarationDocumentFieldList.Codes.OwnerPartNumber,
							ExportDeclarationDocumentFieldList.Codes.GoodsDescription,
							ExportDeclarationDocumentFieldList.Codes.Model,
							ExportDeclarationDocumentFieldList.Codes.Specification,
							ExportDeclarationDocumentFieldList.Codes.PreviousBondedEntryNumber,
							ExportDeclarationDocumentFieldList.Codes.PreviousEntryNumber,
							ExportDeclarationDocumentFieldList.Codes.Permits,
							ExportDeclarationDocumentFieldList.Codes.CertificateOfOrigin,
							ExportDeclarationDocumentFieldList.Codes.AssignedNumbers,
							ExportDeclarationDocumentFieldList.Codes.GoodsOrigin
					}, goodsDescriptionConfigs.Select(c => c.Field));
			});

			declaration.JE_OH_Supplier = CreateOrgFroExportTest().PK;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			CombineAssertions("Test Export with Custom Document Labels", () =>
			{
				AssertContainsExactElementsInExactOrder("Position", new ZShort[] { 1, 2 }, goodsDescriptionConfigs.Select(c => c.Position));
				AssertContainsExactElementsInExactOrder("Caption", new[] { "Test Export Caption1:", "Test Export Caption2:" }, goodsDescriptionConfigs.Select(c => c.Caption));
				AssertContainsExactElementsInExactOrder("Field",
					new[] { ExportDeclarationDocumentFieldList.Codes.SupplierPartNumber,
							ExportDeclarationDocumentFieldList.Codes.OwnerPartNumber
					}, goodsDescriptionConfigs.Select(c => c.Field));
			});

			declaration.JE_MessageType = "IMP";
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			CombineAssertions("Test Import without Custom Document Labels", () =>
			{
				AssertContainsExactElementsInExactOrder("Position", new ZShort[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, goodsDescriptionConfigs.Select(c => c.Position));
				AssertContainsExactElementsInExactOrder("Caption", new[] { "買方料號:", "賣方料號:", "", "商標(牌名):", "型號:", "規格:", "原進倉報單號碼/項次:", "原報單號碼/項次:", "輸出入許可文件號碼/項次:", "產地證明書號碼/項次:", "戰略性高科技貨品國際進口證明號碼:", "華盛頓公約進口許可證號碼:", "主管機關指定代號:" }, goodsDescriptionConfigs.Select(c => c.Caption));
				AssertContainsExactElementsInExactOrder("Field",
					new[] { ImportDeclarationDocumentFieldList.Codes.OwnerPartNumber,
							ImportDeclarationDocumentFieldList.Codes.SupplierPartNumber,
							ImportDeclarationDocumentFieldList.Codes.GoodsDescription,
							ImportDeclarationDocumentFieldList.Codes.Brand,
							ImportDeclarationDocumentFieldList.Codes.Model,
							ImportDeclarationDocumentFieldList.Codes.Specification,
							ImportDeclarationDocumentFieldList.Codes.PreviousBondedEntryNumber,
							ImportDeclarationDocumentFieldList.Codes.PreviousEntryNumber,
							ImportDeclarationDocumentFieldList.Codes.Permits,
							ImportDeclarationDocumentFieldList.Codes.CertificateOfOrigin,
							ImportDeclarationDocumentFieldList.Codes.SHTCImportPermit,
							ImportDeclarationDocumentFieldList.Codes.CITESImportPermit,
							ImportDeclarationDocumentFieldList.Codes.AssignedNumbers
					}, goodsDescriptionConfigs.Select(c => c.Field));
			});

			declaration.JE_OH_Importer = CreateOrgFroImportTest().PK;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			CombineAssertions("Test Import with Custom Document Labels", () =>
			{
				AssertContainsExactElementsInExactOrder("Position", new ZShort[] { 1, 2 }, goodsDescriptionConfigs.Select(c => c.Position));
				AssertContainsExactElementsInExactOrder("Caption", new[] { "Test Import Caption1:", "Test Import Caption2:" }, goodsDescriptionConfigs.Select(c => c.Caption));
				AssertContainsExactElementsInExactOrder("Field",
					new[] { ImportDeclarationDocumentFieldList.Codes.GoodsDescription,
							ImportDeclarationDocumentFieldList.Codes.Brand
					}, goodsDescriptionConfigs.Select(c => c.Field));
			});
		}

		public void TestHideCustomizeSectionBodyRow()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var documentSupporter = declaration.DocumentSupporter;
			var menuItemForTesting = Factory.New<IStmMenuItem>();
			menuItemForTesting.SU_BusinessContext = "Customs";
			menuItemForTesting.SU_MenuName = "Import Customs Declaration (Proof)";
			menuItemForTesting.SU_MenuPath = "Declaration Documents";
			menuItemForTesting.SU_ContactType = "CNE";
			documentSupporter.GetDataStateBeforeRun(menuItemForTesting);
			var jobDeclarationDocumentAddressConfig = documentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();

			CombineAssertions(() =>
			{
				AssertEquals("Should not hide for 'Import Customs Declaration (Proof)'", ZBool.False, jobDeclarationDocumentAddressConfig.HideCustomizeSectionBodyRow);
				menuItemForTesting.SU_ContactType = "XXX";
				jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
				AssertEquals("Should hide for other documents", ZBool.True, jobDeclarationDocumentAddressConfig.HideCustomizeSectionBodyRow);
				AssertNullOrEmpty(jobDeclarationDocumentAddressConfig.CustomizeSectionBodyRow);
			});
		}

		public void TestCustomizeSectionBodyRowList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var jobDeclarationDocumentAddressConfig = new JobDeclarationDocumentAddressConfig(declaration);
			var list = jobDeclarationDocumentAddressConfig.CustomizeSectionBodyRowList;
			CombineAssertions(() =>
			{
				AssertEquals("List", ", 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16", list.CodesAsString);
				AssertSame("Cached", list, jobDeclarationDocumentAddressConfig.CustomizeSectionBodyRowList);
			});
		}

		OrgHeader CreateOrgFroExportTest()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var documentLabel1 = org.CustomDocumentLabels.AddNew();
			documentLabel1.OT_Type = OrgConstants.CustomLabelType.OverrideExportDoc;
			documentLabel1.OT_Position = 1;
			documentLabel1.OT_Caption = "Test Export Caption1:";
			documentLabel1.OT_FieldName = ExportDeclarationDocumentFieldList.Codes.SupplierPartNumber;
			var documentLabel2 = org.CustomDocumentLabels.AddNew();
			documentLabel2.OT_Type = OrgConstants.CustomLabelType.OverrideExportDoc;
			documentLabel2.OT_Position = 2;
			documentLabel2.OT_Caption = "Test Export Caption2:";
			documentLabel2.OT_FieldName = ExportDeclarationDocumentFieldList.Codes.OwnerPartNumber;
			return org;
		}

		OrgHeader CreateOrgFroImportTest()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var documentLabel1 = org.CustomDocumentLabels.AddNew();
			documentLabel1.OT_Type = OrgConstants.CustomLabelType.OverrideImportDoc;
			documentLabel1.OT_Position = 1;
			documentLabel1.OT_Caption = "Test Import Caption1:";
			documentLabel1.OT_FieldName = ImportDeclarationDocumentFieldList.Codes.GoodsDescription;
			var documentLabel2 = org.CustomDocumentLabels.AddNew();
			documentLabel2.OT_Type = OrgConstants.CustomLabelType.OverrideImportDoc;
			documentLabel2.OT_Position = 2;
			documentLabel2.OT_Caption = "Test Import Caption2:";
			documentLabel2.OT_FieldName = ImportDeclarationDocumentFieldList.Codes.Brand;
			return org;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new JobDeclarationDocumentAddressConfig(Factory.New<JobDeclaration>());
		}
	}
}
