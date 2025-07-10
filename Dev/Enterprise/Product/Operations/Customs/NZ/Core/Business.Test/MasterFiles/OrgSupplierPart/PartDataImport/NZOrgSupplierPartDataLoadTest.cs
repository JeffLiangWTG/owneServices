using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.MasterFiles.Testing
{
	using System;
	using System.IO;
	using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
	using Enterprise.Customs.NZ.Business.Testing;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.MasterFiles.Business.Testing;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;
	using OrgSupplierPartDataLoad = Enterprise.MasterFiles.Business.OrgSupplierPartDataLoad;

	[TestedType(typeof(NZOrgSupplierPartDataLoad))]
	public class NZOrgSupplierPartDataLoadTest : DataLoadTestCase<NZOrgSupplierPartDataLoad>
	{
		#region TestOrgSupplierPartDataLoadType

		public void TestOrgSupplierPartDataLoadType()
		{
			OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
			AssertEquals("OrgSupplierPartDataLoad Type", typeof(NZOrgSupplierPartDataLoad), testLoader.GetType());
		}

		#endregion

		#region TestPartNotCreatedIfLookupInOtherCountry

		public void TestPartNotCreatedIfLookupInOtherCountry()
		{
			var testLookup = CreateClassification("Test Lookup", "");
			testLookup.CC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			Factory.Save();

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine($"P1234-4848X,RUBBER GASKET,KG, {testLookup.CC_LookupCode}, {ClassificationTypeList.Codes.HTI},{testOrganisation.OH_Code},12,KG,100,M3,,0");
					sw.Flush();
				}

				var testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, updateParts: false, legacyCodes: false);

				var checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				var enterprisePartsCreated = Factory.Load<OrgSupplierPart>(checkFilter);

				CombineAssertions(() =>
				{
					AssertEquals("Part created", 1, enterprisePartsCreated.Length);
					AssertEquals("Pivot should not have been created", 0, enterprisePartsCreated[0].PivotsForBinding.Count);
				});
			}
		}

		#endregion

		#region TestLoadCusClassPartPivot

		public void TestLoadCusClassPartPivot()
		{
			CusClassification testLookup = CreateClassification("Test Lookup", "");

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("P1234-4848X,RUBBER GASKET,KG," + testLookup.CC_LookupCode + ",HTB,," + testOrganisation.OH_Code + ",12,KG,100,M3,,0");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, updateParts: false, legacyCodes: false);

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load<OrgSupplierPart>(checkFilter);
				AssertEquals("There should have been 1 part record created", 1, enterprisePartsCreated.Length);

				OrgSupplierPart enterprisePart = LoadPart("P1234-4848X");
				ZQuery cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_CC, testLookup.PK);
				cusClassPartFilter.AddToFilter(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				BaseCusClassPartPivot cusClassPartLink = Factory.LoadTop1<BaseCusClassPartPivot>(cusClassPartFilter);
				AssertNotNull("Classification Part Pivot should have been created", cusClassPartLink);
				AssertEquals("Type is HTB", ClassificationTypeList.Codes.HTB, cusClassPartLink.CI_ChildType);
			}
		}

		#endregion

		#region TestPartCreatedWithOrigin

		public void TestImportCSVProductsWithOrigin()
		{
			ClearCustomsRecordsBeforeTesting();
			var testLookup = CreateClassification("Test Lookup", "");
			var testLookup2 = CreateClassification("Lookup2", "");
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",KG," + testLookup.CC_LookupCode + ",HTB," + testOrganisation.OH_Code + ",,,0,,,,,CN");
					sw.WriteLine("1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",NO," + testLookup.CC_LookupCode + ",HTB," + testOrganisation.OH_Code + ",,,0,,,,,JP");
					sw.WriteLine("912.226,\"TAPS,COCKS,VALVES AND SIMILAR APPLIANCES FOR PIPES BOILER SHELLS,TANKS,VATS OR LIKE,INCL PRESSURE RE- DUCING VALVES & THERMOSTATICALLY CONTROLLED VALVES\",NO," + testLookup2.CC_LookupCode + ",HTB," + testOrganisation.OH_Code + ",,,0,,,,,US");
					sw.WriteLine("123,,,,," + testOrganisation.OH_Code + ",,,0,,,,,FR");
					sw.Flush();
				}

				var testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, updateParts: false, legacyCodes: false);

				var checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load<OrgSupplierPart>(checkFilter);

				CombineAssertions(() =>
				{
					AssertEquals("There should have been 3 part records created", 3, enterprisePartsCreated.Length);

					var enterprisePart = LoadPart("912.226");
					AssertEquals("TAPS,COCKS,VALVES AND SIMILAR APPLIANCES FOR PIPES BOILER SHELLS,TANKS,VATS OR LIKE,INCL PRESSURE RE- DUCING VALVES & THERMOSTAT", enterprisePart.OP_Desc);
					AssertEquals("NO", enterprisePart.OP_StockKeepingUnit);
					AssertEquals("Product Origin", 1, enterprisePart.PivotsForBinding.Count);
					AssertEquals("Product Origin", "US", enterprisePart.PivotsForBinding[0].CI_RN_NKCountryOfOrigin);

					enterprisePart = LoadPart("123");
					AssertEquals("Product Origin", 0, enterprisePart.PivotsForBinding.Count);

					enterprisePart = LoadPart("1");
					AssertEquals("Product Origin", 1, enterprisePart.PivotsForBinding.Count);
					AssertEquals("Product Origin", "CN", enterprisePart.PivotsForBinding[0].CI_RN_NKCountryOfOrigin);
				});
			}
		}

		public void TestImportCSVProductsUpdateOrigin()
		{
			// TODO: This UT is Obsoleted as we are having multiple Pivots on Parts, so single line cannot be identified as a single product
			ClearCustomsRecordsBeforeTesting();
			var testLookup = CreateClassification("Test Lookup", "");
			var testLookup2 = CreateClassification("Lookup2", "");

			// Load data without updating origins.
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",KG," + testLookup.CC_LookupCode + ",HTB," + testOrganisation.OH_Code + ",,,0,,,,,");
					sw.WriteLine("1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",NO," + testLookup.CC_LookupCode + ",HTB," + testOrganisation.OH_Code + ",,,0,,,,,");
					sw.WriteLine("912.226,\"TAPS,COCKS,VALVES AND SIMILAR APPLIANCES FOR PIPES BOILER SHELLS,TANKS,VATS OR LIKE,INCL PRESSURE RE- DUCING VALVES & THERMOSTATICALLY CONTROLLED VALVES\",NO," + testLookup2.CC_LookupCode + ",HTB," + testOrganisation.OH_Code + ",,,0,,,,,");
					sw.WriteLine("123,,,,," + testOrganisation.OH_Code + ",,,0,,,,,");
					sw.Flush();
				}

				var testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, updateParts: false, legacyCodes: false);
				Factory.Save();

				var checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load<OrgSupplierPart>(checkFilter);
				AssertEquals("There should have been 3 part records created", 3, enterprisePartsCreated.Length);

				var enterprisePart = LoadPart("912.226");
				AssertEquals("TAPS,COCKS,VALVES AND SIMILAR APPLIANCES FOR PIPES BOILER SHELLS,TANKS,VATS OR LIKE,INCL PRESSURE RE- DUCING VALVES & THERMOSTAT", enterprisePart.OP_Desc);
				AssertEquals("NO", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Product Origin", 1, enterprisePart.PivotsForBinding.Count);
				AssertEquals("Product Origin", "", enterprisePart.PivotsForBinding[0].CI_RN_NKCountryOfOrigin);

				enterprisePart = LoadPart("123");
				AssertEquals("Product Origin", 0, enterprisePart.PivotsForBinding.Count);

				enterprisePart = LoadPart("1");
				AssertEquals("Product Origin", 1, enterprisePart.PivotsForBinding.Count);
				AssertEquals("Product Origin", "", enterprisePart.PivotsForBinding[0].CI_RN_NKCountryOfOrigin);
			}

			// Update products with origins now
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",KG," + testLookup.CC_LookupCode + ",HTB," + testOrganisation.OH_Code + ",,,0,,,,,CN");
					sw.WriteLine("1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",NO," + testLookup.CC_LookupCode + ",HTB," + testOrganisation.OH_Code + ",,,0,,,,,US");
					sw.WriteLine("912.226,\"TAPS,COCKS,VALVES AND SIMILAR APPLIANCES FOR PIPES BOILER SHELLS,TANKS,VATS OR LIKE,INCL PRESSURE RE- DUCING VALVES & THERMOSTATICALLY CONTROLLED VALVES\",NO," + testLookup2.CC_LookupCode + ",HTB," + testOrganisation.OH_Code + ",,,0,,,,,US");
					sw.WriteLine("123,,,,," + testOrganisation.OH_Code + ",,,0,,,,,FR");
					sw.Flush();
				}

				var checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load<OrgSupplierPart>(checkFilter);
				AssertEquals("There should be 3 part records in the file", 3, enterprisePartsCreated.Length);

				var testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, updateParts: true, legacyCodes: false);
				Factory.Save();

				enterprisePartsCreated = Factory.Load<OrgSupplierPart>(checkFilter);
				AssertEquals("There should still be 3 part records after the update", 3, enterprisePartsCreated.Length);

				var enterprisePart = ReLoadPart("912.226");
				AssertEquals("TAPS,COCKS,VALVES AND SIMILAR APPLIANCES FOR PIPES BOILER SHELLS,TANKS,VATS OR LIKE,INCL PRESSURE RE- DUCING VALVES & THERMOSTAT", enterprisePart.OP_Desc);
				AssertEquals("NO", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Product Origin", 1, enterprisePart.PivotsForBinding.Count);
				AssertEquals("Product Origin", "US", enterprisePart.PivotsForBinding[0].CI_RN_NKCountryOfOrigin);

				enterprisePart = ReLoadPart("123");
				AssertEquals("Product Origin", 0, enterprisePart.PivotsForBinding.Count);

				enterprisePart = ReLoadPart("1");
				AssertEquals("Product Origin", 1, enterprisePart.PivotsForBinding.Count);
				AssertEquals("Product Origin", "US", enterprisePart.PivotsForBinding[0].CI_RN_NKCountryOfOrigin);
			}
		}

		#endregion

		#region TestUQCanBeObtainedFromTariff

		public void TestUQCanBeObtainedFromTariff()
		{
			NZCClassification tariff = Factory.LoadTop1<NZCClassification>(new ZQuery());
			tariff.U0_StatisticalUnit = "KGM";
			tariff.U0_Tariff = "3907.30.09.09D";
			Factory.Save();

			CusClassification testLookup = CreateClassification("Test Lookup", tariff.U0_Tariff);

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("P1234,RUBBER GASKET,," + testLookup.CC_LookupCode + ",HTB,," + testOrganisation.OH_Code + ",12,KG,100,M3,0");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, updateParts: false, legacyCodes: false);

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load<OrgSupplierPart>(checkFilter);
				AssertEquals("There should have been 1 part record created", 1, enterprisePartsCreated.Length);

				OrgSupplierPart enterprisePart = LoadPart("P1234");
				AssertEquals("Stock UQ should have been obtained from tariff", "KGM", enterprisePart.OP_StockKeepingUnit);
			}
		}

		public void TestUQCanBeObtainedFromTariffWhenUseRefDb()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				UniversalTariffHelperTest.SetupTariffData(Factory);
				Factory.Save();

				CusClassification testLookup = CreateClassification("Test Lookup", "123456789");

				using (TempFile testFileName = TempFile.New())
				{
					using (StreamWriter sw = new StreamWriter(testFileName.Filename))
					{
						sw.WriteLine(fileHeader);
						sw.WriteLine("P1234,RUBBER GASKET,," + testLookup.CC_LookupCode + ",HTB,," + testOrganisation.OH_Code + ",12,KG,100,M3,0");
						sw.Flush();
					}

					OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
					testLoader.ImportProductData(testFileName.Filename, updateParts: false, legacyCodes: false);

					ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
					BusinessObject[] enterprisePartsCreated = Factory.Load<OrgSupplierPart>(checkFilter);
					AssertEquals("There should have been 1 part record created", 1, enterprisePartsCreated.Length);

					OrgSupplierPart enterprisePart = LoadPart("P1234");
					AssertEquals("Stock UQ should have been obtained from tariff", "AAA", enterprisePart.OP_StockKeepingUnit);
				}
			}
		}

		#endregion

		#region TestSaveAndClearPreviousTariffDetailsIfPresentAndDifferent

		public void TestSaveAndClearPreviousTariffDetailsIfPresentAndDifferent()
		{
			CusClassification testLookup = CreateClassification("Test Lookup", "8529.90.11.01H");

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("LEAFLETS,PAPER BOOKLETS,NO," + testLookup.CC_LookupCode + ",HTB,," + testOrganisation.OH_Code + ",12,KG,100,M3,,35");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, updateParts: false, legacyCodes: false);

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load<OrgSupplierPart>(checkFilter);
				AssertEquals("There should have been 1 part record created", 1, enterprisePartsCreated.Length);

				OrgSupplierPart enterprisePart = LoadPart("LEAFLETS");
				AssertEquals("Stock UQ", "NO", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Division should be empty", "", enterprisePart.OP_Division);
				AssertEquals("Stock Qty should be 35", 35M, enterprisePart.OP_QtyInStock);
				AssertEquals("Stock Weight", 12M, enterprisePart.OP_Weight);
				AssertEquals("Stock Weight UQ", "KG", enterprisePart.OP_WeightUQ);
				AssertEquals("Stock Volume", 100M, enterprisePart.OP_Cubic);
				AssertEquals("Stock Volume UQ", "M3", enterprisePart.OP_CubicUQ);
			}

			BaseCusClassification changedLookup = CreateClassification("Changed Lookup", "8529.90.11.01H");

			using (TempFile updateFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(updateFile.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("LEAFLETS,ONE PAGE BROCHURES,PK," + changedLookup.CC_LookupCode + ",HTB,," + testOrganisation.OH_Code + ",99,,400,M3,PRINTING,25850");
					sw.Flush();
				}

				OrgSupplierPartDataLoad testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(updateFile.Filename, updateParts: true, legacyCodes: false);
				Factory.Save();

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load<OrgSupplierPart>(checkFilter);
				AssertEquals("There should still only be 1 part record", 1, enterprisePartsCreated.Length);

				OrgSupplierPart enterprisePart = LoadPart("LEAFLETS");
				enterprisePart.Reload();
				AssertEquals("Part Description", "ONE PAGE BROCHURES", enterprisePart.OP_Desc);
				AssertEquals("Stock UQ", "PK", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Division", "PRINTING", enterprisePart.OP_Division);
				AssertEquals("Stock Qty", 25850M, enterprisePart.OP_QtyInStock);
				AssertEquals("Stock Weight", 99M, enterprisePart.OP_Weight);
				AssertEquals("Stock Weight UQ", "", enterprisePart.OP_WeightUQ);
				AssertEquals("Stock Volume", 400M, enterprisePart.OP_Cubic);
				AssertEquals("Stock Volume UQ", "M3", enterprisePart.OP_CubicUQ);

				ZQuery cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				BaseCusClassPartPivot[] cusClassPartLinks = enterprisePart.Factory.Load<BaseCusClassPartPivot>(cusClassPartFilter);
				AssertEquals("Should be 1 pivot record", 1, cusClassPartLinks.Length);
				AssertEquals("Link to this classification", changedLookup.PK, cusClassPartLinks[0].CI_CC);

				Assert(enterprisePart.Notes.HasNotes);
				StmNoteCollection testNotes = (StmNoteCollection)enterprisePart.Notes.GetAllNotes();
				AssertEquals("Data Import Update: Previous HTB Details", testNotes[0].ST_Description);
				AssertMultilineASCIIEquals("notes", "Previous Lookup: Test Lookup" + System.Environment.NewLine, testNotes[0].ST_NoteDataAsText);
			}
		}

		#endregion

		#region FindExistingPivot

		public void TestFindExistingPivot_NoExistingPivots()
		{
			// verify new pivots are always HTB when none exist
			var classification = CreateClassification("Test Lookup", "8529.90.11.01H");

			AssertPivotType("P1234-001", classification, ClassificationTypeList.Codes.HTB, ClassificationTypeList.Codes.HTB);
			AssertPivotType("P1234-002", classification, ClassificationTypeList.Codes.HTE, ClassificationTypeList.Codes.HTB);
			AssertPivotType("P1234-003", classification, ClassificationTypeList.Codes.HTI, ClassificationTypeList.Codes.HTB);
			AssertPivotType("P1234-004", classification, "", ClassificationTypeList.Codes.HTB);
		}

		public void TestFindExistingPivot_ExistingHTBPivotRemainsHTB()
		{
			//verify a HTB pivot is not replaced with anything else
			var classification = CreateClassification("Old Lookup", "");
			var classificationNew = CreateClassification("New Lookup", "");
			var pivot = CreatePartPivot("P1234-001", ClassificationTypeList.Codes.HTB, classification);
			Factory.Save();

			AssertPivotType("P1234-001", classificationNew, ClassificationTypeList.Codes.HTB, ClassificationTypeList.Codes.HTB);

			SetPivotClassificationAndType(pivot, classification, ClassificationTypeList.Codes.HTB);
			AssertPivotType("P1234-001", classificationNew, ClassificationTypeList.Codes.HTE, ClassificationTypeList.Codes.HTB);

			SetPivotClassificationAndType(pivot, classification, ClassificationTypeList.Codes.HTB);
			AssertPivotType("P1234-001", classificationNew, ClassificationTypeList.Codes.HTI, ClassificationTypeList.Codes.HTB);

			SetPivotClassificationAndType(pivot, classification, ClassificationTypeList.Codes.HTB);
			AssertPivotType("P1234-001", classificationNew, "", ClassificationTypeList.Codes.HTB);
		}

		public void TestFindExistingPivot_ExistingHTEPivotBecomesHTB()
		{
			//verify a HTE pivot is replaced with HTB
			var classification = CreateClassification("Old Lookup", "");
			var classificationNew = CreateClassification("New Lookup", "");
			var pivot = CreatePartPivot("P1234-001", ClassificationTypeList.Codes.HTE, classification);
			Factory.Save();

			AssertPivotType("P1234-001", classificationNew, ClassificationTypeList.Codes.HTB, ClassificationTypeList.Codes.HTB);

			SetPivotClassificationAndType(pivot, classification, ClassificationTypeList.Codes.HTE);
			AssertPivotType("P1234-001", classificationNew, ClassificationTypeList.Codes.HTE, ClassificationTypeList.Codes.HTB);

			SetPivotClassificationAndType(pivot, classification, ClassificationTypeList.Codes.HTE);
			AssertPivotType("P1234-001", classificationNew, ClassificationTypeList.Codes.HTI, ClassificationTypeList.Codes.HTB);

			SetPivotClassificationAndType(pivot, classification, ClassificationTypeList.Codes.HTE);
			AssertPivotType("P1234-001", classificationNew, "", ClassificationTypeList.Codes.HTB);
		}

		public void TestFindExistingPivot_ExistingHTIPivotBecomesHTB()
		{
			//verify a HTI pivot is replaced with HTB
			var classification = CreateClassification("Old Lookup", "");
			var classificationNew = CreateClassification("New Lookup", "");
			var pivot = CreatePartPivot("P1234-001", ClassificationTypeList.Codes.HTI, classification);
			Factory.Save();

			AssertPivotType("P1234-001", classificationNew, ClassificationTypeList.Codes.HTB, ClassificationTypeList.Codes.HTB);

			SetPivotClassificationAndType(pivot, classification, ClassificationTypeList.Codes.HTI);
			AssertPivotType("P1234-001", classificationNew, ClassificationTypeList.Codes.HTE, ClassificationTypeList.Codes.HTB);

			SetPivotClassificationAndType(pivot, classification, ClassificationTypeList.Codes.HTI);
			AssertPivotType("P1234-001", classificationNew, ClassificationTypeList.Codes.HTI, ClassificationTypeList.Codes.HTB);

			SetPivotClassificationAndType(pivot, classification, ClassificationTypeList.Codes.HTI);
			AssertPivotType("P1234-001", classificationNew, "", ClassificationTypeList.Codes.HTB);
		}

		void SetPivotClassificationAndType(BaseCusClassPartPivot pivot, CusClassification classification, string classificationTypeCode)
		{
			pivot.CI_ChildType = classificationTypeCode;
			pivot.CI_CC = classification.PK;
			Factory.Save();
		}

		public void TestFindExistingPivot_ExistingHTEandHTIPivotsRemainHTEandHTI_Lookup()
		{
			//verify HTE + HTI pivots are not replaced with anything else, and are updated.
			var classification = CreateClassification("Old Lookup", "");
			var classificationNew = CreateClassification("New Lookup", "");
			var htePivot = CreatePartPivot("P1234-001", ClassificationTypeList.Codes.HTE, classification);
			var htiPivot = CreatePartPivot("P1234-001", ClassificationTypeList.Codes.HTI, classification);
			var part = LoadPart("P1234-001");
			Factory.Save();

			var logText = ImportProductData("P1234-001", classificationNew, ClassificationTypeList.Codes.HTB);
			part.PivotsForBinding.Reload(reLoadExistingRows: true, assumeRowsMissingFromQueryResultsAreDeleted: true);
			AssertEquals(ClassificationTypeList.Codes.HTE, htePivot.CI_ChildType);
			AssertEquals("HTE classification unchanged", "Old Lookup", htePivot.Classification.CC_LookupCode);
			AssertEquals(ClassificationTypeList.Codes.HTI, htiPivot.CI_ChildType);
			AssertEquals("HTI classification unchanged", "Old Lookup", htiPivot.Classification.CC_LookupCode);
			AssertEquals("No extra Pivots", 2, part.PivotsForBinding.Count);
			AssertContains("PART NO/DESC: P1234-001 / RUBBER GASKET  Unable to update this part - Part has HTE and HTI entries which must be updated separately.", logText);
			AssertContains("T O T A L : Products created = 0, Products updated = 0, Products excluded = 1", logText);

			htePivot.CI_CC = htiPivot.CI_CC = classification.PK;
			Factory.Save();
			logText = ImportProductData("P1234-001", classificationNew, ClassificationTypeList.Codes.HTE);
			part.PivotsForBinding.Reload(reLoadExistingRows: true, assumeRowsMissingFromQueryResultsAreDeleted: true);
			AssertEquals(ClassificationTypeList.Codes.HTE, htePivot.CI_ChildType);
			AssertEquals("HTE classification updated", "New Lookup", htePivot.Classification.CC_LookupCode);
			AssertEquals(ClassificationTypeList.Codes.HTI, htiPivot.CI_ChildType);
			AssertEquals("HTI classification unchanged", "Old Lookup", htiPivot.Classification.CC_LookupCode);
			AssertEquals("No extra Pivots", 2, part.PivotsForBinding.Count);
			AssertContains("PART NO: P1234-001 - Part has been UPDATED", logText);
			AssertContains("T O T A L : Products created = 0, Products updated = 1, Products excluded = 0", logText);

			htePivot.CI_CC = htiPivot.CI_CC = classification.PK;
			Factory.Save();
			logText = ImportProductData("P1234-001", classificationNew, ClassificationTypeList.Codes.HTI);
			part.PivotsForBinding.Reload(reLoadExistingRows: true, assumeRowsMissingFromQueryResultsAreDeleted: true);
			AssertEquals(ClassificationTypeList.Codes.HTE, htePivot.CI_ChildType);
			AssertEquals("HTE classification unchanged", "Old Lookup", htePivot.Classification.CC_LookupCode);
			AssertEquals(ClassificationTypeList.Codes.HTI, htiPivot.CI_ChildType);
			AssertEquals("HTI classification updated", "New Lookup", htiPivot.Classification.CC_LookupCode);
			AssertEquals("No extra Pivots", 2, part.PivotsForBinding.Count);
			AssertContains("PART NO: P1234-001 - Part has been UPDATED", logText);
			AssertContains("T O T A L : Products created = 0, Products updated = 1, Products excluded = 0", logText);

			htePivot.CI_CC = htiPivot.CI_CC = classification.PK;
			Factory.Save();
			logText = ImportProductData("P1234-001", classificationNew, "XXX");
			part.PivotsForBinding.Reload(reLoadExistingRows: true, assumeRowsMissingFromQueryResultsAreDeleted: true);
			AssertEquals(ClassificationTypeList.Codes.HTE, htePivot.CI_ChildType);
			AssertEquals("HTE classification unchanged", "Old Lookup", htePivot.Classification.CC_LookupCode);
			AssertEquals(ClassificationTypeList.Codes.HTI, htiPivot.CI_ChildType);
			AssertEquals("HTI classification unchanged", "Old Lookup", htiPivot.Classification.CC_LookupCode);
			AssertEquals("No extra Pivots", 2, part.PivotsForBinding.Count);
			AssertContains("PART NO/DESC: P1234-001 / RUBBER GASKET  Unable to update this part - Part has HTE and HTI entries which must be updated separately.", logText);
			AssertContains("T O T A L : Products created = 0, Products updated = 0, Products excluded = 1", logText);
		}

		public void TestFindExistingPivot_ExistingHTEandHTIPivotsRemainHTEandHTI_Tariff()
		{
			//verify HTE + HTI pivots are not replaced with anything else, and are updated.
			var oldTariff = "8529.90.11.01H";
			var newTariff = "8529.90.11.02H";
			var htePivot = CreatePartPivot("P1234-001", ClassificationTypeList.Codes.HTE, oldTariff);
			var htiPivot = CreatePartPivot("P1234-001", ClassificationTypeList.Codes.HTI, oldTariff);
			var part = LoadPart("P1234-001");
			Factory.Save();

			var logText = ImportProductData("P1234-001", null, ClassificationTypeList.Codes.HTB, newTariff);
			part.PivotsForBinding.Reload(reLoadExistingRows: true, assumeRowsMissingFromQueryResultsAreDeleted: true);
			var pivots = part.PivotsForBinding.GetNonDeletedPivots();
			htePivot = pivots.FirstOrDefault(x => x.CI_ChildType == ClassificationTypeList.Codes.HTE);
			htiPivot = pivots.FirstOrDefault(x => x.CI_ChildType == ClassificationTypeList.Codes.HTI);
			AssertEquals("HTE classification unchanged", oldTariff, htePivot.CI_TariffNum);
			AssertEquals("HTI classification unchanged", oldTariff, htiPivot.CI_TariffNum);
			AssertEquals("No extra Pivots", 2, part.PivotsForBinding.Count);
			AssertContains("PART NO/DESC: P1234-001 / RUBBER GASKET  Unable to update this part - Part has HTE and HTI entries which must be updated separately.", logText);
			AssertContains("T O T A L : Products created = 0, Products updated = 0, Products excluded = 1", logText);

			htePivot.CI_TariffNum = htiPivot.CI_TariffNum = oldTariff;
			Factory.Save();
			logText = ImportProductData("P1234-001", null, ClassificationTypeList.Codes.HTE, newTariff);
			part.PivotsForBinding.Reload(reLoadExistingRows: true, assumeRowsMissingFromQueryResultsAreDeleted: true);
			pivots = part.PivotsForBinding.GetNonDeletedPivots();
			htePivot = pivots.FirstOrDefault(x => x.CI_ChildType == ClassificationTypeList.Codes.HTE);
			htiPivot = pivots.FirstOrDefault(x => x.CI_ChildType == ClassificationTypeList.Codes.HTI);
			AssertEquals("HTE classification updated", newTariff, htePivot.CI_TariffNum);
			AssertEquals("HTI classification unchanged", oldTariff, htiPivot.CI_TariffNum);
			AssertEquals("No extra Pivots", 2, part.PivotsForBinding.Count);
			AssertContains("PART NO: P1234-001 - Part has been UPDATED", logText);
			AssertContains("T O T A L : Products created = 0, Products updated = 1, Products excluded = 0", logText);

			htePivot.CI_TariffNum = htiPivot.CI_TariffNum = oldTariff;
			Factory.Save();
			logText = ImportProductData("P1234-001", null, ClassificationTypeList.Codes.HTI, newTariff);
			part.PivotsForBinding.Reload(reLoadExistingRows: true, assumeRowsMissingFromQueryResultsAreDeleted: true);
			pivots = part.PivotsForBinding.GetNonDeletedPivots();
			htePivot = pivots.FirstOrDefault(x => x.CI_ChildType == ClassificationTypeList.Codes.HTE);
			htiPivot = pivots.FirstOrDefault(x => x.CI_ChildType == ClassificationTypeList.Codes.HTI);
			AssertEquals("HTE classification unchanged", oldTariff, htePivot.CI_TariffNum);
			AssertEquals("HTI classification updated", newTariff, htiPivot.CI_TariffNum);
			AssertEquals("No extra Pivots", 2, part.PivotsForBinding.Count);
			AssertContains("PART NO: P1234-001 - Part has been UPDATED", logText);
			AssertContains("T O T A L : Products created = 0, Products updated = 1, Products excluded = 0", logText);

			htePivot.CI_TariffNum = htiPivot.CI_TariffNum = oldTariff;
			Factory.Save();
			logText = ImportProductData("P1234-001", null, "XXX", newTariff);
			part.PivotsForBinding.Reload(reLoadExistingRows: true, assumeRowsMissingFromQueryResultsAreDeleted: true);
			pivots = part.PivotsForBinding.GetNonDeletedPivots();
			htePivot = pivots.FirstOrDefault(x => x.CI_ChildType == ClassificationTypeList.Codes.HTE);
			htiPivot = pivots.FirstOrDefault(x => x.CI_ChildType == ClassificationTypeList.Codes.HTI);
			AssertEquals("HTE classification unchanged", oldTariff, htePivot.CI_TariffNum);
			AssertEquals("HTI classification unchanged", oldTariff, htiPivot.CI_TariffNum);
			AssertEquals("No extra Pivots", 2, part.PivotsForBinding.Count);
			AssertContains("PART NO/DESC: P1234-001 / RUBBER GASKET  Unable to update this part - Part has HTE and HTI entries which must be updated separately.", logText);
			AssertContains("T O T A L : Products created = 0, Products updated = 0, Products excluded = 1", logText);

			htePivot.CI_TariffNum = htiPivot.CI_TariffNum = oldTariff;
			var htbPivot = CreatePartPivot("P1234-001", ClassificationTypeList.Codes.HTB, oldTariff);
			Factory.Save();

			logText = ImportProductData("P1234-001", null, ClassificationTypeList.Codes.HTE, newTariff);
			part.PivotsForBinding.Reload(reLoadExistingRows: true, assumeRowsMissingFromQueryResultsAreDeleted: true);
			pivots = part.PivotsForBinding.GetNonDeletedPivots();
			htePivot = pivots.FirstOrDefault(x => x.CI_ChildType == ClassificationTypeList.Codes.HTE);
			htiPivot = pivots.FirstOrDefault(x => x.CI_ChildType == ClassificationTypeList.Codes.HTI);
			htbPivot = pivots.FirstOrDefault(x => x.CI_ChildType == ClassificationTypeList.Codes.HTB);
			AssertEquals("HTE classification unchanged", oldTariff, htePivot.CI_TariffNum);
			AssertEquals("HTI classification unchanged", oldTariff, htiPivot.CI_TariffNum);
			AssertEquals("HTB classification unchanged", oldTariff, htbPivot.CI_TariffNum);
			AssertEquals("No extra Pivots", 3, part.PivotsForBinding.Count);
			AssertContains("PART NO/DESC: P1234-001 / RUBBER GASKET  Unable to update this part - Part has unexpected entries: HTE,HTI,HTB", logText);
			AssertContains("T O T A L : Products created = 0, Products updated = 0, Products excluded = 1", logText);
		}

		void AssertPivotType(string partNumber, CusClassification testLookup, string importingPivotType, string expectedPivotType)
		{
			ImportProductData(partNumber, testLookup, importingPivotType);

			var part = LoadPart(partNumber);
			part.PivotsForBinding.Reload(reLoadExistingRows: true, assumeRowsMissingFromQueryResultsAreDeleted: true);
			AssertEquals("No extra Pivots", 1, part.PivotsForBinding.Count);
			var pivot = part.PivotsForBinding[0];
			AssertEquals("expectedPivotType from " + importingPivotType, expectedPivotType, pivot.CI_ChildType);
			AssertEquals("expected classification", testLookup.CC_LookupCode, pivot.Classification.CC_LookupCode);
		}

		string ImportProductData(string partNumber, CusClassification testLookup, string importingPivotType, string tariffNum = null)
		{
			string logText;

			var headertext = fileHeader;
			var dataText = $"{partNumber},RUBBER GASKET,KG,{testLookup?.CC_LookupCode ?? string.Empty},{importingPivotType},,{testOrganisation.OH_Code},12,KG,100,M3,,0,";

			if (tariffNum != null)
			{
				headertext += ",Tariff";
				dataText += $",{tariffNum}";
			}

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(headertext);
					sw.WriteLine(dataText);
					sw.Flush();
				}

				var testLoader = OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, updateParts: true, legacyCodes: false);

				logText = string.Concat(testLoader.Log.ToList<string>());
			}

			return logText;
		}

		BaseCusClassPartPivot CreatePartPivot(string partNumber, string pivotType, CusClassification classification)
		{
			var pivot = CreatePartPivot(partNumber, pivotType, ZString.Empty);
			pivot.CI_CC = classification.PK;
			return pivot;
		}

		BaseCusClassPartPivot CreatePartPivot(string partNumber, string pivotType, ZString tariffNum)
		{
			var partFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, partNumber);
			var enterprisePart = Factory.LoadTop1<OrgSupplierPart>(partFilter);
			if (enterprisePart == null)
			{
				enterprisePart = Factory.New<OrgSupplierPart>();
				enterprisePart.OP_PartNum = partNumber;
				var owner = enterprisePart.RelatedOrganisations.AddNew();
				owner.OU_OH = testOrganisation.PK;
				owner.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			}

			var pivot = enterprisePart.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = tariffNum;
			pivot.CI_ChildType = pivotType;
			return pivot;
		}

		#endregion

		#region Implementation

		OrgHeader testOrganisation;
		ZString fileHeader;

		protected override void SetUp()
		{
			base.SetUp();
			ClearCustomsRecordsBeforeTesting();
			fileHeader = "Code,Description,UQ,ClassificationLookup,ClassificationType,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyinStock,Origin";
			testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
		}

		protected override NZOrgSupplierPartDataLoad GetNewDataLoader()
		{
			return new NZOrgSupplierPartDataLoad();
		}

		protected void ClearCustomsRecordsBeforeTesting()
		{
			TestCaseHelper.ClearTable("JobComInvoiceLine");
			TestCaseHelper.ClearTable("CusClassPartPivot");
			TestCaseHelper.ClearTable("CusClassification");
			TestCaseHelper.ClearTable("OrgPartRelation");
			TestCaseHelper.ClearTable("OrgPartUnit");
			TestCaseHelper.ClearTable("OrgSupplierPart");
		}

		protected OrgSupplierPart LoadPart(ZString lookupPart)
		{
			ZQuery partFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, lookupPart);
			OrgSupplierPart enterprisePart = Factory.LoadTop1<OrgSupplierPart>(partFilter);
			AssertNotNull("Expecting Part " + lookupPart + " to be found", enterprisePart);

			return enterprisePart;
		}

		OrgSupplierPart ReLoadPart(ZString lookupPart)
		{
			var newFactory = new BusinessObjectFactory();
			var partFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, lookupPart);
			var enterprisePart = newFactory.LoadTop1<OrgSupplierPart>(partFilter);
			AssertNotNull("Expecting Part " + lookupPart + " to be found", enterprisePart);
			return enterprisePart;
		}

		CusClassification CreateClassification(ZString lookupCode, ZString tariff)
		{
			CusClassification testLookup = Factory.NewWithValidTestData<CusClassification>();
			testLookup.CC_LookupCode = lookupCode;
			if (!tariff.IsEmpty)
			{
				testLookup.CC_TariffNum = tariff;
			}

			testLookup.CC_ClassificationType = CusClassification.ClassificationType.Both;
			Factory.Save();

			return testLookup;
		}

		#endregion
	}
}
