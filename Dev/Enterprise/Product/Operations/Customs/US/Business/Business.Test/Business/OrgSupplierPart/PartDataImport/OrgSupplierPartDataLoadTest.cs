using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartDataLoad))]
	sealed class OrgSupplierPartDataLoadTest : DataLoadTestCase<OrgSupplierPartDataLoad>
	{
		[ExpectNoExceptions]
		public void TestImportingCountrySpecificFields()
		{
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "POOLCUE";
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;

			var org = OrgHeader.LoadFromCode(Factory, "ABIGAS");
			org.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "ABC1234ZZZA");

			Factory.Save();

			var dataLoad = new OrgSupplierPartDataLoad();
			using (var tempFile = TempFile.New())
			{
				using (var sw = new StreamWriter(tempFile.Filename))
				{
					sw.WriteLine("Code   ,Description,UQ,Supplier,ImportClassification,ProductClaim,SPI,CBTPACertificateNo,CottonCertificateNo,MiscPermitNo,PIRPRulingNo,PIRPRulingType,WoolLicenceNo,IsNAFTANet,CAExportCertificate,AgricultureLicNo,CargoStorageCode,ZoneStatus,PercentageActiveIngredient,ManufacturerID,Origin,ADDCaseNo ,CVDCaseNo, ProductExclusion, ExclusionNumber");
					sw.WriteLine("PoolCue,Pool Cue   ,NO,ABIGAS  ,PoolCue             ,B           ,AU ,CBTPA             ,Cotton             ,MiscPmt     ,PIRPNo      ,X             ,Wool         ,Y         ,CA                 ,Ag              ,C               ,Z         ,3.52                      ,ABC1234ZZZA   ,KRxxxx,A428201001,C475819054, 1, EN20180910");
				}

				dataLoad.ImportProductData(tempFile.Filename, false, false);
				AssertEquals("Records Created", 1, dataLoad.RunCounters.RecsCreated);
				OrgHeader supplier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
				OrgSupplierPart product = (OrgSupplierPart)new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("PoolCue", null, supplier);
				AssertNotNull("Product", product);
				CusClassPartPivot exportPivot = product.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, ZGuid.Empty);
				CusClassPartPivot importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNull("Export Pivot", exportPivot);
				AssertNotNull("Import pivot", importPivot);
				AssertEquals("B", importPivot.CD_ProductClaim);
				AssertEquals("AU", importPivot.CD_SPI);
				AssertEquals("CBTPA", importPivot.CD_CBTPACertificate);
				AssertEquals("Cotton", importPivot.CD_CottonCertificate);
				AssertEquals("MiscPmt", importPivot.CD_MiscLicenceNo);
				AssertEquals("PIRPNo", importPivot.CD_RulingNumber);
				AssertEquals("X", importPivot.CD_RulingType);
				AssertEquals("Wool", importPivot.CD_WoolLicenceNo);
				AssertEquals(ZBool.True, importPivot.CD_NAFTANetCost);
				AssertEquals("CA", importPivot.CD_SugarCertificate);
				AssertEquals("Ag", importPivot.CD_AgricultureLicenceNo);
				AssertEquals("Z", importPivot.CD_ZoneStatus);
				AssertEquals(3.52m, importPivot.CD_ActiveIngredientPercentage);
				AssertEquals(org.MainAddress.PK, importPivot.CD_OA_Manufacturer);
				AssertEquals("KRxxxx is too long, is ignored", "", importPivot.CD_UC_NKCountryOfOrigin);
				AssertEquals("A428201001", importPivot.CD_ADDCaseNo);
				AssertEquals("C475819054", importPivot.CD_CVDCaseNo);
				AssertEquals("1", importPivot.CD_ProductExclusion);
				AssertEquals("EN2018091", importPivot.CD_ExclusionNumber);
			}
		}

		public void TestSettingUQFromTariff()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "1020304051";
			tariff1.UE_Unit1 = "KG";
			tariff1.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff1.UE_DateTo = ZDateTime.Today.AddYears(1);

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "1020304052";
			tariff2.UE_Unit1 = "G";
			tariff2.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(1);

			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "1020304053";
			tariff3.UE_Unit1 = "X";
			tariff3.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff3.UE_DateTo = ZDateTime.Today.AddYears(1);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffTypeSB = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();

			var scheduleB1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, "2030405061", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTariffUOM(scheduleB1, "CU1", "LB");

			var scheduleB2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, "2030405062", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTariffUOM(scheduleB2, "CU1", "L");

			var scheduleB3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, "2030405063", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTariffUOM(scheduleB3, "CU1", "X");

			var expClass = Factory.New<CusClassification>();
			expClass.CC_LookupCode = "EXPPOOLCUE";
			expClass.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			expClass.CC_TariffNum = scheduleB1.ZZ1_TariffCode;

			var impClass = Factory.New<CusClassification>();
			impClass.CC_LookupCode = "IMPPOOLCUE";
			impClass.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			impClass.CC_TariffNum = tariff1.UE_Tariff;

			Factory.Save();

			var dataLoad = new OrgSupplierPartDataLoad();
			using (var tempFile = TempFile.New())
			{
				using (var sw = new StreamWriter(tempFile.Filename))
				{
					sw.WriteLine("Code     ,Description,UQ,Supplier,ImportClassification,ExportClassification,SupImportTariff,ImportTariff,ExportTariff");
					sw.WriteLine("PoolCue1 ,Pool Cue   ,,ABIGAS  ,IMPPOOLCUE          ,                    ,                 ,            ,            ");
					sw.WriteLine("PoolCue2 ,Pool Cue   ,,ABIGAS  ,                    ,EXPPOOLCUE          ,                 ,            ,            ");
					sw.WriteLine("PoolCue3 ,Pool Cue   ,,ABIGAS  ,                    ,                    ,                 ,            ,            ");
					sw.WriteLine("PoolCue4 ,Pool Cue   ,,ABIGAS  ,                    ,                    ,9999999999       ,1020304051  ,            ");
					sw.WriteLine("PoolCue5 ,Pool Cue   ,,ABIGAS  ,                    ,                    ,9999999999       ,1020304052  ,            ");
					sw.WriteLine("PoolCue6 ,Pool Cue   ,,ABIGAS  ,                    ,                    ,                 ,1020304053  ,            ");
					sw.WriteLine("PoolCue7 ,Pool Cue   ,,ABIGAS  ,                    ,                    ,                 ,1020304054  ,            ");
					sw.WriteLine("PoolCue8 ,Pool Cue   ,,ABIGAS  ,                    ,                    ,                 ,            ,2030405061  ");
					sw.WriteLine("PoolCue9 ,Pool Cue   ,,ABIGAS  ,                    ,                    ,                 ,            ,2030405062  ");
					sw.WriteLine("PoolCue10 ,Pool Cue   ,,ABIGAS  ,                    ,                    ,                ,            ,2030405063  ");
					sw.WriteLine("PoolCue11,Pool Cue   ,,ABIGAS  ,                    ,                    ,                 ,            ,2030405064  ");
				}

				dataLoad.ImportProductData(tempFile.Filename, false, false);
				AssertEquals("Records Created", 11, dataLoad.RunCounters.RecsCreated);
				var supplier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
				var loader = new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart));
				AssertEquals("KG", loader.Load("PoolCue1", null, supplier).OP_StockKeepingUnit);
				AssertEquals("LB", loader.Load("PoolCue2", null, supplier).OP_StockKeepingUnit);
				AssertEquals("UNT", loader.Load("PoolCue3", null, supplier).OP_StockKeepingUnit);
				AssertEquals("KG", loader.Load("PoolCue4", null, supplier).OP_StockKeepingUnit);
				AssertEquals("G", loader.Load("PoolCue5", null, supplier).OP_StockKeepingUnit);
				AssertEquals("UNT", loader.Load("PoolCue6", null, supplier).OP_StockKeepingUnit);
				AssertEquals("UNT", loader.Load("PoolCue7", null, supplier).OP_StockKeepingUnit);
				AssertEquals("LB", loader.Load("PoolCue8", null, supplier).OP_StockKeepingUnit);
				AssertEquals("L", loader.Load("PoolCue9", null, supplier).OP_StockKeepingUnit);
				AssertEquals("UNT", loader.Load("PoolCue10", null, supplier).OP_StockKeepingUnit);
				AssertEquals("UNT", loader.Load("PoolCue11", null, supplier).OP_StockKeepingUnit);

				AssertEquals("SupImportTariff", "9999.99.9999", loader.Load("PoolCue4", null, supplier).PivotsForBinding[0].CI_FormattedSupplementalTariff);
				AssertEquals("SupImportTariff", "9999.99.9999", loader.Load("PoolCue5", null, supplier).PivotsForBinding[0].CI_FormattedSupplementalTariff);
			}
		}

		public void TestSaveAndClearPreviousImportTariffDetailsIfPresentAndDifferent()
		{
			var testLookup = CreateClassification("Test Lookup", "6112.11.00 25", CusClassification.ClassificationType.IMP);

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("P1234,RUBBER GASKET,KG,," + testLookup.CC_LookupCode + ",," + testOrganisation.OH_Code + ",,0");
					sw.Flush();
				}

				MasterFiles.Business.OrgSupplierPartDataLoad testLoader = MasterFiles.Business.OrgSupplierPartDataLoad.New();
				Assert(testLoader is OrgSupplierPartDataLoad);
				testLoader.ImportProductData(testFileName.Filename, false, false);

				var checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				var enterprisePartsCreated = Factory.Load(typeof(OrgSupplierPart), checkFilter);
				AssertEquals("There should have been 1 part record created", 1, enterprisePartsCreated.Length);

				var enterprisePart = LoadPart("P1234");
				AssertEquals("Stock UQ", "KG", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Division should be empty", "", enterprisePart.OP_Division);
				AssertEquals("Stock Qty should be 0", 0M, enterprisePart.OP_QtyInStock);
			}

			var changedLookup = CreateClassification("Changed Lookup", "6112.11.00 25", CusClassification.ClassificationType.IMP);

			using (var updateFile = TempFile.New())
			{
				using (var sw = new StreamWriter(updateFile.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("P1234,RUBBER O RING,NO,," + changedLookup.CC_LookupCode + ",," + testOrganisation.OH_Code + ",PLUMBING,6000");
					sw.Flush();
				}

				MasterFiles.Business.OrgSupplierPartDataLoad testLoader = MasterFiles.Business.OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(updateFile.Filename, true, false);
				Factory.Save();

				var checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				var enterprisePartsCreated = Factory.Load(typeof(OrgSupplierPart), checkFilter);
				AssertEquals("There should still only be 1 part record", 1, enterprisePartsCreated.Length);

				var enterprisePart = LoadPart("P1234");
				enterprisePart.Reload();
				AssertEquals("Part Description", "RUBBER O RING", enterprisePart.OP_Desc);
				AssertEquals("Stock UQ", "NO", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Division", "PLUMBING", enterprisePart.OP_Division);
				AssertEquals("Stock Qty", 6000M, enterprisePart.OP_QtyInStock);

				var cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				var cusClassPartLinks = (CusClassPartPivot[])enterprisePart.Factory.Load(typeof(CusClassPartPivot), cusClassPartFilter);
				AssertEquals("Should be 1 pivot record", 1, cusClassPartLinks.Length);
				AssertEquals("Link to this classification", changedLookup.PK, cusClassPartLinks[0].CI_CC);

				Assert(enterprisePart.Notes.HasNotes);
				var testNotes = (StmNoteCollection)enterprisePart.Notes.GetAllNotes();
				AssertEquals("Data Import Update: Previous HTI Details", testNotes[0].ST_Description);
				AssertEquals("Previous Lookup: Test Lookup", testNotes[0].ST_NoteDataAsText);
			}
		}

		public void TestSaveAndClearPreviousExportTariffDetailsIfPresentAndDifferent()
		{
			var exportLookup = CreateClassification("Export Lookup", "4901.10.00", CusClassification.ClassificationType.EXP);

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("LEAFLETS,PAPER BOOKLETS,NO," + exportLookup.CC_LookupCode + ",,," + testOrganisation.OH_Code + ",,0");
					sw.Flush();
				}

				MasterFiles.Business.OrgSupplierPartDataLoad testLoader = MasterFiles.Business.OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, false, false);

				var checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				var enterprisePartsCreated = Factory.Load(typeof(OrgSupplierPart), checkFilter);
				AssertEquals("There should have been 1 part record created", 1, enterprisePartsCreated.Length);

				var enterprisePart = LoadPart("LEAFLETS");
				AssertEquals("Stock UQ", "NO", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Division should be empty", "", enterprisePart.OP_Division);
				AssertEquals("Stock Qty should be 0", 0M, enterprisePart.OP_QtyInStock);
			}

			var changedLookup = CreateClassification("Changed Lookup", "4901.10.00", CusClassification.ClassificationType.EXP);

			using (var updateFile = TempFile.New())
			{
				using (var sw = new StreamWriter(updateFile.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("LEAFLETS,ONE PAGE BROCHURES,NO," + changedLookup.CC_LookupCode + ",,," + testOrganisation.OH_Code + ",PRINTING,25850");
					sw.Flush();
				}

				MasterFiles.Business.OrgSupplierPartDataLoad testLoader = MasterFiles.Business.OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(updateFile.Filename, true, false);
				Factory.Save();

				var checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				var enterprisePartsCreated = Factory.Load(typeof(OrgSupplierPart), checkFilter);
				AssertEquals("There should still only be 1 part record", 1, enterprisePartsCreated.Length);

				var enterprisePart = LoadPart("LEAFLETS");
				enterprisePart.Reload();
				AssertEquals("Part Description", "ONE PAGE BROCHURES", enterprisePart.OP_Desc);
				AssertEquals("Stock UQ", "NO", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Division", "PRINTING", enterprisePart.OP_Division);
				AssertEquals("Stock Qty", 25850M, enterprisePart.OP_QtyInStock);

				var cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				var cusClassPartLinks = (CusClassPartPivot[])enterprisePart.Factory.Load(typeof(CusClassPartPivot), cusClassPartFilter);
				AssertEquals("Should be 1 pivot record", 1, cusClassPartLinks.Length);
				AssertEquals("Link to this classification", changedLookup.PK, cusClassPartLinks[0].CI_CC);

				Assert(enterprisePart.Notes.HasNotes);
				var testNotes = (StmNoteCollection)enterprisePart.Notes.GetAllNotes();
				AssertEquals("Data Import Update: Previous SHB Details", testNotes[0].ST_Description);
				AssertEquals("Previous Lookup: Export Lookup", testNotes[0].ST_NoteDataAsText);
			}
		}

		public void TestClassificationDoesNotAutoCreated()
		{
			fileHeader = "Code,Description,UQ,ImportTariff,ExportTariff,Supplier";

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("LEAFLETS,PAPER BOOKLETS,NO,4901.10.00,01010000," + testOrganisation.OH_Code);
					sw.Flush();
				}

				MasterFiles.Business.OrgSupplierPartDataLoad testLoader = MasterFiles.Business.OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, false, false);

				var enterprisePart = LoadPart("LEAFLETS");

				var importPivot = enterprisePart.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
				AssertNull(importPivot.Classification);
				AssertEquals("49011000", importPivot.CI_TariffNum);

				var exportPivot = enterprisePart.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, ZGuid.Empty);
				AssertNull(exportPivot.Classification);
				AssertEquals("01010000", exportPivot.CI_TariffNum);
			}
		}

		public void TestProductImportRelationship_UnitPrice_MultipleRecordsWithInvalidData_InvalidUnitPrice()
		{
			var client = Helper.CreateClient("C1");
			var product1 = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			product1.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			var product2 = (OrgSupplierPart)Helper.CreateProduct(client, "P2");
			product2.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			var dataLoad = new OrgSupplierPartDataLoad();
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,UQ,OWNER,UnitPrice,UnitPriceCurrency");
					sw.WriteLine("P1, UNT, C1, 2, AUD");
					sw.WriteLine("P2, UNT, C1, -2, AUD");
					sw.Flush();
				}

				AssertEquals("Precondtion", 0, dataLoad.Log.Count);
				dataLoad.ImportProductData(testFileName.Filename, true, false);
			}

			AssertEquals("Precondtion", 5, dataLoad.Log.Count);
			AssertEquals("Products to Import = 2", dataLoad.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", dataLoad.Log[1]);
			AssertEquals("The Unit Price of -2 is invalid. Valid value range for unit price is between 0 and 922337203685477.58. The unit price will not be imported.", dataLoad.Log[2]);
			AssertEquals("PART NO: P2 - Part has been UPDATED", dataLoad.Log[3]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 2, Products excluded = 0\r\n", dataLoad.Log[4]);

			var relation1InAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product1.PK)).Single();
			AssertEquals("Should update.", 2m, relation1InAnotherFactory.OU_UnitPrice);
			AssertEquals("Should update.", "AUD", relation1InAnotherFactory.OU_RX_NKUnitPriceCurrency);

			var relation2InAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product2.PK)).Single();
			AssertEquals("Should not update.", 0m, relation2InAnotherFactory.OU_UnitPrice);
			AssertEquals("Should not update.", "", relation2InAnotherFactory.OU_RX_NKUnitPriceCurrency);
		}

		public void TestProductImportRelationship_UnitPrice_MultipleRecordsWithInvalidData_InvalidCurrency()
		{
			var client = Helper.CreateClient("C1");
			var product1 = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			product1.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			var product2 = (OrgSupplierPart)Helper.CreateProduct(client, "P2");
			product2.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			var dataLoad = new OrgSupplierPartDataLoad();
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,UQ,OWNER,UnitPrice,UnitPriceCurrency");
					sw.WriteLine("P1, UNT, C1, 2, AUD");
					sw.WriteLine("P2, UNT, C1, 2, XXX");
					sw.Flush();
				}

				AssertEquals("Precondtion", 0, dataLoad.Log.Count);
				dataLoad.ImportProductData(testFileName.Filename, true, false);
			}

			AssertEquals("Precondtion", 5, dataLoad.Log.Count);
			AssertEquals("Products to Import = 2", dataLoad.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", dataLoad.Log[1]);
			AssertEquals("The Unit Price is invalid because 'XXX' is not a valid currency. The unit price will not be imported. Please enter a valid currency to import a unit price.", dataLoad.Log[2]);
			AssertEquals("PART NO: P2 - Part has been UPDATED", dataLoad.Log[3]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 2, Products excluded = 0\r\n", dataLoad.Log[4]);

			var relation1InAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product1.PK)).Single();
			AssertEquals("Should update.", 2m, relation1InAnotherFactory.OU_UnitPrice);
			AssertEquals("Should update.", "AUD", relation1InAnotherFactory.OU_RX_NKUnitPriceCurrency);

			var relation2InAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product2.PK)).Single();
			AssertEquals("Should not update.", 0m, relation2InAnotherFactory.OU_UnitPrice);
			AssertEquals("Should not update.", "", relation2InAnotherFactory.OU_RX_NKUnitPriceCurrency);
		}

		public void TestImportingSupAdditionalTariffs()
		{
			var dataLoad = new OrgSupplierPartDataLoad();
			using (var tempFile = TempFile.New())
			{
				using (var sw = new StreamWriter(tempFile.Filename))
				{
					sw.WriteLine("Code     ,Description,UQ,Supplier,ImportClassification,ExportClassification,ImportTariff,SupImportTariff,SupAdditionalTariff1,SupAdditionalTariff2,SupAdditionalTariff3,SupAdditionalTariff4,SupAdditionalTariff5");
					sw.WriteLine("PoolCue1 ,Pool Cue   ,,ABIGAS  ,                    ,                    ,1111111111       ,            ,                   ,                   ,                   ,                   ,                   ");
					sw.WriteLine("PoolCue2 ,Pool Cue   ,,ABIGAS  ,                    ,                    ,1111122222       ,2222222222  ,                   ,                   ,                   ,                   ,                   ");
					sw.WriteLine("PoolCue3 ,Pool Cue   ,,ABIGAS  ,                    ,                    ,1111133333       ,2222233333  ,3333333333         ,                   ,                   ,                   ,                   ");
					sw.WriteLine("PoolCue4 ,Pool Cue   ,,ABIGAS  ,                    ,                    ,1111144444       ,2222244444  ,3333344444         ,4444444444         ,                   ,                   ,                   ");
					sw.WriteLine("PoolCue5 ,Pool Cue   ,,ABIGAS  ,                    ,                    ,1111155555       ,2222255555  ,3333355555         ,4444455555         ,5555555555         ,                   ,                   ");
					sw.WriteLine("PoolCue6 ,Pool Cue   ,,ABIGAS  ,                    ,                    ,1111166666       ,2222266666  ,3333366666         ,4444466666         ,5555566666         ,6666666666         ,                   ");
					sw.WriteLine("PoolCue7 ,Pool Cue   ,,ABIGAS  ,                    ,                    ,1111177777       ,2222277777  ,3333377777         ,4444477777         ,5555577777         ,6666677777         ,7777777777         ");
				}

				dataLoad.ImportProductData(tempFile.Filename, false, false);
				AssertEquals("Records Created", 7, dataLoad.RunCounters.RecsCreated);
				var supplier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
				var loader = new Customs.Business.OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart));

				AssertSupAdditionalTariffs("PoolCue1", "1111.11.1111", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertSupAdditionalTariffs("PoolCue2", "1111.12.2222", "2222.22.2222", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertSupAdditionalTariffs("PoolCue3", "1111.13.3333", "2222.23.3333", "3333.33.3333", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertSupAdditionalTariffs("PoolCue4", "1111.14.4444", "2222.24.4444", "3333.34.4444", "4444.44.4444", ZString.Empty, ZString.Empty, ZString.Empty);
				AssertSupAdditionalTariffs("PoolCue5", "1111.15.5555", "2222.25.5555", "3333.35.5555", "4444.45.5555", "5555.55.5555", ZString.Empty, ZString.Empty);
				AssertSupAdditionalTariffs("PoolCue6", "1111.16.6666", "2222.26.6666", "3333.36.6666", "4444.46.6666", "5555.56.6666", "6666.66.6666", ZString.Empty);
				AssertSupAdditionalTariffs("PoolCue7", "1111.17.7777", "2222.27.7777", "3333.37.7777", "4444.47.7777", "5555.57.7777", "6666.67.7777", "7777.77.7777");

				void AssertSupAdditionalTariffs(ZString partCode, ZString expectedTariff, ZString expectedProvTariff, ZString expectedAdditionalTariff1, ZString expectedAdditionalTariff2, ZString expectedAdditionalTariff3, ZString expectedAdditionalTariff4, ZString expectedAdditionalTariff5)
				{
					CombineAssertions($"Check tariffs in {partCode}", () =>
					{
						var matchedPivotForBinding = (CusClassPartPivot)loader.Load(partCode, null, supplier).PivotsForBinding[0];
						AssertEquals("CI_FormattedTariffNum", expectedTariff, matchedPivotForBinding.CI_FormattedTariffNum);
						AssertEquals("CI_FormattedSupplementalTariff", expectedProvTariff, matchedPivotForBinding.CI_FormattedSupplementalTariff);
						AssertEquals("SupFormattedAdditionalTariff1", expectedAdditionalTariff1, matchedPivotForBinding.SupFormattedAdditionalTariff1);
						AssertEquals("SupFormattedAdditionalTariff2", expectedAdditionalTariff2, matchedPivotForBinding.SupFormattedAdditionalTariff2);
						AssertEquals("SupFormattedAdditionalTariff3", expectedAdditionalTariff3, matchedPivotForBinding.SupFormattedAdditionalTariff3);
						AssertEquals("SupFormattedAdditionalTariff4", expectedAdditionalTariff4, matchedPivotForBinding.SupFormattedAdditionalTariff4);
						AssertEquals("SupFormattedAdditionalTariff5", expectedAdditionalTariff5, matchedPivotForBinding.SupFormattedAdditionalTariff5);
					});
				}
			}
		}

		protected override OrgSupplierPartDataLoad GetNewDataLoader() => new OrgSupplierPartDataLoad();

		protected override void SetUp()
		{
			base.SetUp();
			ClearCustomsRecordsBeforeTesting();
			fileHeader = "Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Division,QtyinStock";
			testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
		}

		CusClassification CreateClassification(ZString lookupCode, ZString tariff, ZString type)
		{
			CusClassification testLookup = Factory.NewWithValidTestData<CusClassification>();
			testLookup.CC_LookupCode = lookupCode;
			if (!tariff.IsEmpty)
			{
				testLookup.CC_TariffNum = tariff;
			}

			testLookup.CC_ClassificationType = type;
			Factory.Save();

			return testLookup;
		}

		void ClearCustomsRecordsBeforeTesting()
		{
			TestCaseHelper.ClearTable("JobComInvoiceLine");
			TestCaseHelper.ClearTable("CusClassPartPivot");
			TestCaseHelper.ClearTable("CusClassification");
			TestCaseHelper.ClearTable("OrgPartRelation");
			TestCaseHelper.ClearTable("OrgPartUnit");
			TestCaseHelper.ClearTable("OrgSupplierPart");
		}

		OrgSupplierPart LoadPart(ZString lookupPart)
		{
			var partFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, lookupPart);
			var enterprisePart = Factory.LoadTop1<OrgSupplierPart>(partFilter);
			AssertNotNull("Expecting Part " + lookupPart + " to be found", enterprisePart);

			return enterprisePart;
		}

		IWhsTransactionTestHelper Helper => helper ?? (helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory));
		IWhsTransactionTestHelper helper;

		ZString fileHeader;
		OrgHeader testOrganisation;
	}
}
