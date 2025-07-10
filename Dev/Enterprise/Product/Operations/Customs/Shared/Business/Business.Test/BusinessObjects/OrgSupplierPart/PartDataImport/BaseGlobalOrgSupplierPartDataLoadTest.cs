using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class BaseGlobalOrgSupplierPartDataLoadTest : DataLoadTestCase<GlobalOrgSupplierPartDataLoad>
	{
		#region TestOnlySupportsSingleHTIorHTEPerProduct

		public void TestOnlySupportsSingleHTIorHTEPerProduct()
		{
			OrgHeader supplier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = supplier.PK;
			relation.OU_Relationship = "SUP";
			var pivot1 = Factory.NewWithValidTestData<BaseCusClassPartPivot>();
			pivot1.CI_OP = product.PK;
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_OP = product.PK;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;

			Factory.Save();

			var dataLoad = GetNewDataLoader();
			using (TempFile tempFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(tempFile.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine(product.OP_PartNum + OnlySupportsSingleHTIorHTEPerProduct_Line1);
				}

				dataLoad.ImportProductData(tempFile.Filename, true, false);

				var logText = string.Concat(dataLoad.Log.ToList());
				AssertContains($"Line 2: PART NO/DESC: {product.OP_PartNum} / DESCR  Unable to update this part - contains multiple HTI records and cannot be update by the import.", logText);
			}
		}

		protected abstract ZString OnlySupportsSingleHTIorHTEPerProduct_Line1 { get; }

		#endregion

		#region TestImportingCountrySpecificFields

		[ExpectNoExceptions]
		public void TestImportingCountrySpecificFields()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var classification = Factory.New<BaseCusClassification>();
				classification.CC_LookupCode = "POOLCUE";
				classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;

				Factory.Save();

				var dataLoad = GetNewDataLoader();
				using (TempFile tempFile = TempFile.New())
				{
					using (StreamWriter sw = new StreamWriter(tempFile.Filename))
					{
						sw.WriteLine(ImportingCountrySpecificFields_Header);
						sw.WriteLine(ImportingCountrySpecificFields_Line1);
					}

					dataLoad.ImportProductData(tempFile.Filename, false, false);
					AssertEquals("Records Created", 1, dataLoad.RunCounters.RecsCreated);
					OrgHeader supplier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
					OrgSupplierPart product = new OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("PoolCue", null, supplier);
					AssertNotNull("Product", product);
					var exportPivot = product.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, ZGuid.Empty);
					var importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
					AssertNull("Export Pivot", exportPivot);
					AssertNotNull("Import pivot", importPivot);
					AssertEquals(new ZDateTime(2016, 12, 21), importPivot.CI_DateStart);
					AssertEquals(new ZDateTime(2017, 12, 21), importPivot.CI_DateEnd);
					AssertEquals("DE", importPivot.CI_RN_NKCountryOfExport);
					AssertEquals("VAT", importPivot.CI_ZZF_NKTaxType);
					AssertEquals("CN", importPivot.CI_RW_NKOriginState);
					AssertEquals("PREF1", importPivot.CI_PrimaryPreference);
					AssertEquals("PREF2", importPivot.CI_SecondaryPreference);
					AssertEquals("AB", importPivot.CI_ValuationCode);
					AssertEquals(new ZDecimal("5.000"), importPivot.CI_ValuationMarkup);
					AssertEquals("Red", importPivot.GetType().GetProperty(ZACusClassPartPivotSchema.Constants.CI_Colour).GetValue(importPivot));
					AssertEquals(3000, importPivot.GetType().GetProperty(ZACusClassPartPivotSchema.Constants.CI_EngineCapacity).GetValue(importPivot));
					AssertEquals("N", importPivot.GetType().GetProperty(ZACusClassPartPivotSchema.Constants.CI_NewUsed).GetValue(importPivot));
					AssertEquals("CRT", importPivot.GetType().GetProperty(ZACusClassPartPivotSchema.Constants.CI_ROOCert).GetValue(importPivot));
					AssertEquals("FMT", importPivot.GetType().GetProperty(ZACusClassPartPivotSchema.Constants.CI_VehicleFormat).GetValue(importPivot));
					AssertEquals("TYP", importPivot.GetType().GetProperty(ZACusClassPartPivotSchema.Constants.CI_VehicleType).GetValue(importPivot));
					AssertEquals("UsageComment", importPivot.CI_UsageComment);
					AssertEquals("ClassificationDescription", importPivot.CI_Description);
				}

				using (TempFile updateFile = TempFile.New())
				{
					using (StreamWriter sw = new StreamWriter(updateFile.Filename))
					{
						sw.WriteLine(ImportingCountrySpecificFields_Header);
						sw.WriteLine(ImportingCountrySpecificFields_Line2);
					}

					dataLoad.ImportProductData(updateFile.Filename, true, false);
					AssertEquals("Records Updated", 1, dataLoad.RunCounters.RecsUpdated);

					OrgHeader supplier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
					OrgSupplierPart product = new OrgSupplierPart.Loader(new BusinessObjectFactory(), typeof(OrgSupplierPart)).Load("PoolCue", null, supplier);
					AssertNotNull("Product", product);

					var importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
					AssertNotNull("Import pivot", importPivot);
					AssertEquals("White", importPivot.GetType().GetProperty(ZACusClassPartPivotSchema.Constants.CI_Colour).GetValue(importPivot));

					Assert(product.Notes.HasNotes);
					var testNotes = (ZArchitecture.Business.StmNoteCollection)product.Notes.GetAllNotes();
					var note1 = testNotes.OfType<ZArchitecture.Business.StmNote>().FirstOrDefault(x => x.ST_Description == "Data Import Update: Previous HTI Details");
					var note2 = testNotes.OfType<ZArchitecture.Business.StmNote>().FirstOrDefault(x => x.ST_Description == "Data Import Update: Previous HTI Add Info Data");
					AssertNotNull("Note 1: Data Import Update: Previous HTI Details", note1);
					AssertEquals("Previous Lookup: POOLCUE", note1.ST_NoteDataAsText);
					AssertNotNull("Note 2: Data Import Update: Previous HTI Add Info Data", note2);
					AssertEquals("Previous Add Info: Colour=Red*EngineCapacity=3000*NewUsed=N*ROOCert=CRT*VehicleFormat=FMT*VehicleType=TYP", note2.ST_NoteDataAsText);
				}
			}
		}

		protected abstract ZString ImportingCountrySpecificFields_Header { get; }
		protected abstract ZString ImportingCountrySpecificFields_Line1 { get; }
		protected abstract ZString ImportingCountrySpecificFields_Line2 { get; }

		#endregion

		#region TestSettingUQFromTariff

		public void TestSettingUQFromTariff()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var startDate = ZDateTime.Today.AddYears(-1);
				var endDate = ZDateTime.Today.AddYears(1);
				var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
				var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
				Factory.Save();

				var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1020304051", startDate, endDate);
				helper.CreateTariffUOM(tariff1.PK, "CU1", "KG");

				var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1020304052", startDate, endDate);
				helper.CreateTariffUOM(tariff2.PK, "CU1", "G");

				var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1020304053", startDate, endDate);
				helper.CreateTariffUOM(tariff3.PK, "CU1", "X");

				var expClass = Factory.New<BaseCusClassification>();
				expClass.CC_LookupCode = "EXPPOOLCUE";
				expClass.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
				expClass.CC_TariffNum = tariff1.ZZ1_TariffCode;

				var impClass = Factory.New<BaseCusClassification>();
				impClass.CC_LookupCode = "IMPPOOLCUE";
				impClass.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
				impClass.CC_TariffNum = tariff1.ZZ1_TariffCode;

				Factory.Save();

				var dataLoad = GetNewDataLoader();
				using (TempFile tempFile = TempFile.New())
				{
					using (StreamWriter sw = new StreamWriter(tempFile.Filename))
					{
						sw.WriteLine(fileHeader);
						sw.WriteLine(SettingUQFromTariff_Line1);
						sw.WriteLine(SettingUQFromTariff_Line2);
						sw.WriteLine(SettingUQFromTariff_Line3);
						sw.WriteLine(SettingUQFromTariff_Line4);
						sw.WriteLine(SettingUQFromTariff_Line5);
						sw.WriteLine(SettingUQFromTariff_Line6);
						sw.WriteLine(SettingUQFromTariff_Line7);
						sw.WriteLine(SettingUQFromTariff_Line8);
						sw.WriteLine(SettingUQFromTariff_Line9);
						sw.WriteLine(SettingUQFromTariff_Line10);
						sw.WriteLine(SettingUQFromTariff_Line11);
					}

					dataLoad.ImportProductData(tempFile.Filename, false, false);
					AssertEquals("Records Created", 11, dataLoad.RunCounters.RecsCreated);
					OrgHeader supplier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
					OrgSupplierPart.Loader loader = new OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart));
					AssertEquals("KG", loader.Load("PoolCue1", null, supplier).OP_StockKeepingUnit);
					AssertEquals("KG", loader.Load("PoolCue2", null, supplier).OP_StockKeepingUnit);
					AssertEquals("UNT", loader.Load("PoolCue3", null, supplier).OP_StockKeepingUnit);
					AssertEquals("KG", loader.Load("PoolCue4", null, supplier).OP_StockKeepingUnit);
					AssertEquals("G", loader.Load("PoolCue5", null, supplier).OP_StockKeepingUnit);
					AssertEquals("UNT", loader.Load("PoolCue6", null, supplier).OP_StockKeepingUnit);
					AssertEquals("UNT", loader.Load("PoolCue7", null, supplier).OP_StockKeepingUnit);
					AssertEquals("KG", loader.Load("PoolCue8", null, supplier).OP_StockKeepingUnit);
					AssertEquals("G", loader.Load("PoolCue9", null, supplier).OP_StockKeepingUnit);
					AssertEquals("UNT", loader.Load("PoolCue10", null, supplier).OP_StockKeepingUnit);
					AssertEquals("UNT", loader.Load("PoolCue11", null, supplier).OP_StockKeepingUnit);
				}
			}
		}

		protected abstract ZString SettingUQFromTariff_Line1 { get; }
		protected abstract ZString SettingUQFromTariff_Line2 { get; }
		protected abstract ZString SettingUQFromTariff_Line3 { get; }
		protected abstract ZString SettingUQFromTariff_Line4 { get; }
		protected abstract ZString SettingUQFromTariff_Line5 { get; }
		protected abstract ZString SettingUQFromTariff_Line6 { get; }
		protected abstract ZString SettingUQFromTariff_Line7 { get; }
		protected abstract ZString SettingUQFromTariff_Line8 { get; }
		protected abstract ZString SettingUQFromTariff_Line9 { get; }
		protected abstract ZString SettingUQFromTariff_Line10 { get; }
		protected abstract ZString SettingUQFromTariff_Line11 { get; }

		#endregion

		#region TestSaveAndClearPreviousImportTariffDetailsIfPresentAndDifferent

		public void TestSaveAndClearPreviousImportTariffDetailsIfPresentAndDifferent()
		{
			var testLookup = CreateClassification("Test Lookup", "6112.11.00 25", BaseCusClassification.ClassificationType.IMP);

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine(SaveAndClearPreviousImportTariffDetailsIfPresentAndDifferent_Line1 + testOrganisation.OH_Code + ",,0");
					sw.Flush();
				}

				var testLoader = GetNewDataLoader();
				testLoader.ImportProductData(testFileName.Filename, false, false);

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(OrgSupplierPart), checkFilter);
				AssertEquals("There should have been 1 part record created", 1, enterprisePartsCreated.Length);

				OrgSupplierPart enterprisePart = LoadPart("P1234");
				AssertEquals("Stock UQ", "KG", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Division should be empty", "", enterprisePart.OP_Division);
				AssertEquals("Stock Qty should be 0", 0M, enterprisePart.OP_QtyInStock);
			}

			var changedLookup = CreateClassification("Changed Lookup", "6112.11.00 25", BaseCusClassification.ClassificationType.IMP);

			using (TempFile updateFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(updateFile.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine(SaveAndClearPreviousImportTariffDetailsIfPresentAndDifferent_Line2 + testOrganisation.OH_Code + ",PLUMBING,6000");
					sw.Flush();
				}

				var testLoader = GetNewDataLoader();
				testLoader.ImportProductData(updateFile.Filename, true, false);
				Factory.Save();

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(OrgSupplierPart), checkFilter);
				AssertEquals("There should still only be 1 part record", 1, enterprisePartsCreated.Length);

				OrgSupplierPart enterprisePart = LoadPart("P1234");
				enterprisePart.Reload();
				AssertEquals("Part Description", "RUBBER O RING", enterprisePart.OP_Desc);
				AssertEquals("Stock UQ", "NO", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Division", "PLUMBING", enterprisePart.OP_Division);
				AssertEquals("Stock Qty", 6000M, enterprisePart.OP_QtyInStock);

				ZQuery cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				var cusClassPartLinks = enterprisePart.Factory.Load<BaseCusClassPartPivot>(cusClassPartFilter);
				AssertEquals("Should be 1 pivot record", 1, cusClassPartLinks.Length);
				AssertEquals("Link to this classification", changedLookup.PK, cusClassPartLinks[0].CI_CC);

				Assert(enterprisePart.Notes.HasNotes);
				var testNotes = (ZArchitecture.Business.StmNoteCollection)enterprisePart.Notes.GetAllNotes();
				AssertEquals("Data Import Update: Previous HTI Details", testNotes[0].ST_Description);
				AssertEquals("Previous Lookup: Test Lookup", testNotes[0].ST_NoteDataAsText);
			}

			using (TempFile updateFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(updateFile.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine(SaveAndClearPreviousImportTariffDetailsIfPresentAndDifferent_LineWithTariff + testOrganisation.OH_Code + ",PLUMBING,6000");
					sw.Flush();
				}

				var testLoader = GetNewDataLoader();
				testLoader.ImportProductData(updateFile.Filename, true, false);
				Factory.Save();

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(OrgSupplierPart), checkFilter);
				AssertEquals("There should still only be 1 part record", 1, enterprisePartsCreated.Length);

				OrgSupplierPart enterprisePart = LoadPart("P1234");
				enterprisePart.Reload();
				AssertEquals("Part Description", "RUBBER GASKET", enterprisePart.OP_Desc);
				AssertEquals("Stock UQ", "KG", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Division", "PLUMBING", enterprisePart.OP_Division);
				AssertEquals("Stock Qty", 6000M, enterprisePart.OP_QtyInStock);

				ZQuery cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				var cusClassPartLinks = new BusinessObjectFactory().Load<BaseCusClassPartPivot>(cusClassPartFilter);
				AssertEquals("Should be 1 pivot record", 1, cusClassPartLinks.Length);
				Assert("No link to classification", !cusClassPartLinks[0].CI_CC.IsValid);
				AssertEquals("Tariff", "07061009", cusClassPartLinks[0].CI_TariffNum);

				Assert(enterprisePart.Notes.HasNotes);
				var testNotes = (ZArchitecture.Business.StmNoteCollection)enterprisePart.Notes.GetAllNotes();
				AssertNotNull("Previous Lookup: Changed Lookup", testNotes.OfType<ZArchitecture.Business.StmNote>().FirstOrDefault(x => x.ST_NoteDataAsText == "Previous Lookup: Changed Lookup"));
			}

			using (TempFile updateFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(updateFile.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine(SaveAndClearPreviousImportTariffDetailsIfPresentAndDifferent_LineWithAltTariff + testOrganisation.OH_Code + ",PLUMBING,6000");
					sw.Flush();
				}

				var testLoader = GetNewDataLoader();
				testLoader.ImportProductData(updateFile.Filename, true, false);
				Factory.Save();

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(OrgSupplierPart), checkFilter);
				AssertEquals("There should still only be 1 part record", 1, enterprisePartsCreated.Length);

				OrgSupplierPart enterprisePart = LoadPart("P1234");
				enterprisePart.Reload();
				AssertEquals("Part Description", "RUBBER O RING", enterprisePart.OP_Desc);
				AssertEquals("Stock UQ", "NO", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Division", "PLUMBING", enterprisePart.OP_Division);
				AssertEquals("Stock Qty", 6000M, enterprisePart.OP_QtyInStock);

				ZQuery cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				var cusClassPartLinks = new BusinessObjectFactory().Load<BaseCusClassPartPivot>(cusClassPartFilter);
				AssertEquals("Should be 1 pivot record", 1, cusClassPartLinks.Length);
				Assert("No link to classification", !cusClassPartLinks[0].CI_CC.IsValid);
				AssertEquals("Tariff", "07061011", cusClassPartLinks[0].CI_TariffNum);

				Assert(enterprisePart.Notes.HasNotes);
				var testNotes = (ZArchitecture.Business.StmNoteCollection)enterprisePart.Notes.GetAllNotes();
				AssertNotNull("Previous Tariff: 07061009", testNotes.OfType<ZArchitecture.Business.StmNote>().FirstOrDefault(x => x.ST_NoteDataAsText == "Previous Tariff: 07061009"));
			}
		}

		protected abstract ZString SaveAndClearPreviousImportTariffDetailsIfPresentAndDifferent_Line1 { get; }
		protected abstract ZString SaveAndClearPreviousImportTariffDetailsIfPresentAndDifferent_Line2 { get; }
		protected abstract ZString SaveAndClearPreviousImportTariffDetailsIfPresentAndDifferent_LineWithTariff { get; }
		protected abstract ZString SaveAndClearPreviousImportTariffDetailsIfPresentAndDifferent_LineWithAltTariff { get; }

		#endregion

		#region TestSaveAndClearPreviousExportTariffDetailsIfPresentAndDifferent

		public void TestSaveAndClearPreviousExportTariffDetailsIfPresentAndDifferent()
		{
			var exportLookup = CreateClassification("Export Lookup", "4901.10.00", BaseCusClassification.ClassificationType.EXP);

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine(SaveAndClearPreviousExportTariffDetailsIfPresentAndDifferent_Line1 + testOrganisation.OH_Code + ",,0");
					sw.Flush();
				}

				var testLoader = GetNewDataLoader();
				testLoader.ImportProductData(testFileName.Filename, false, false);

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(OrgSupplierPart), checkFilter);
				AssertEquals("There should have been 1 part record created", 1, enterprisePartsCreated.Length);

				OrgSupplierPart enterprisePart = LoadPart("LEAFLETS");
				AssertEquals("Stock UQ", "NO", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Division should be empty", "", enterprisePart.OP_Division);
				AssertEquals("Stock Qty should be 0", 0M, enterprisePart.OP_QtyInStock);
			}

			var changedLookup = CreateClassification("Changed Lookup", "4901.10.00", BaseCusClassification.ClassificationType.EXP);

			using (TempFile updateFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(updateFile.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine(SaveAndClearPreviousExportTariffDetailsIfPresentAndDifferent_Line2 + testOrganisation.OH_Code + ",PRINTING,25850");
					sw.Flush();
				}

				var testLoader = GetNewDataLoader();
				testLoader.ImportProductData(updateFile.Filename, true, false);
				Factory.Save();

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(OrgSupplierPart), checkFilter);
				AssertEquals("There should still only be 1 part record", 1, enterprisePartsCreated.Length);

				OrgSupplierPart enterprisePart = LoadPart("LEAFLETS");
				enterprisePart.Reload();
				AssertEquals("Part Description", "ONE PAGE BROCHURES", enterprisePart.OP_Desc);
				AssertEquals("Stock UQ", "NO", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Division", "PRINTING", enterprisePart.OP_Division);
				AssertEquals("Stock Qty", 25850M, enterprisePart.OP_QtyInStock);

				ZQuery cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				var cusClassPartLinks = enterprisePart.Factory.Load<BaseCusClassPartPivot>(cusClassPartFilter);
				AssertEquals("Should be 1 pivot record", 1, cusClassPartLinks.Length);
				AssertEquals("Link to this classification", changedLookup.PK, cusClassPartLinks[0].CI_CC);

				Assert(enterprisePart.Notes.HasNotes);
				var testNotes = (ZArchitecture.Business.StmNoteCollection)enterprisePart.Notes.GetAllNotes();
				AssertEquals("Data Import Update: Previous HTE Details", testNotes[0].ST_Description);
				AssertEquals("Previous Lookup: Export Lookup", testNotes[0].ST_NoteDataAsText);
			}
		}

		protected abstract ZString SaveAndClearPreviousExportTariffDetailsIfPresentAndDifferent_Line1 { get; }
		protected abstract ZString SaveAndClearPreviousExportTariffDetailsIfPresentAndDifferent_Line2 { get; }

		#endregion

		public void TestProductImportRelationship_UnitPrice_MultipleRecordsWithInvalidData_InvalidUnitPrice()
		{
			var client = Helper.CreateClient("C1");
			var product1 = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			product1.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			var product2 = (OrgSupplierPart)Helper.CreateProduct(client, "P2");
			product2.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			var dataLoad = GetNewDataLoader();
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

			var dataLoad = GetNewDataLoader();
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

		#region Implementation

		protected ZString fileHeader;
		protected OrgHeader testOrganisation;

		#region CreateClassification

		protected BaseCusClassification CreateClassification(ZString lookupCode, ZString tariff, ZString type)
		{
			var testLookup = Factory.NewWithValidTestData<BaseCusClassification>();
			testLookup.CC_LookupCode = lookupCode;
			if (!tariff.IsEmpty)
			{
				testLookup.CC_TariffNum = tariff;
			}

			testLookup.CC_ClassificationType = type;
			Factory.Save();

			return testLookup;
		}

		#endregion

		protected void ClearCustomsRecordsBeforeTesting()
		{
			TestCaseHelper.ClearTable("JobComInvoiceLine");
			TestCaseHelper.ClearTable("CusClassPartPivot");
			TestCaseHelper.ClearTable("CusClassification");
			TestCaseHelper.ClearTable("OrgPartRelation");
			TestCaseHelper.ClearTable("OrgPartUnit");
			TestCaseHelper.ClearTable("OrgSupplierPart");
		}

		protected override void SetUp()
		{
			base.SetUp();
			ClearCustomsRecordsBeforeTesting();
			testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
		}

		protected OrgSupplierPart LoadPart(ZString lookupPart)
		{
			ZQuery partFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, lookupPart);
			OrgSupplierPart enterprisePart = new BusinessObjectFactory().LoadTop1<OrgSupplierPart>(partFilter);
			AssertNotNull("Expecting Part " + lookupPart + " to be found", enterprisePart);

			return enterprisePart;
		}

		protected override GlobalOrgSupplierPartDataLoad GetNewDataLoader()
		{
			return new GlobalOrgSupplierPartDataLoad();
		}

		IWhsTransactionTestHelper Helper => helper ?? (helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory));
		IWhsTransactionTestHelper helper;

		#endregion
	}
}
