using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartDataLoad))]
	public class OrgSupplierPartDataLoadTest : DataLoadTestCase<OrgSupplierPartDataLoad>
	{
		#region TestNew

		public void TestNew()
		{
			var dictionary = new Dictionary<string, Type>()
			{
				{ Core.Constants.CountryCodes.Australia, typeof(Enterprise.Integration.Customs.AU.IAUCusOrgSupplierPartDataLoad) },
				{ Core.Constants.CountryCodes.NewZealand, typeof(NZ.INZCusOrgSupplierPartDataLoad) },
				{ Core.Constants.CountryCodes.UnitedStates, typeof(US.IOrgSupplierPartDataLoad) },
				{ Core.Constants.CountryCodes.Italy, typeof(EU.IEUOrgSupplierPartDataLoad) },
				{ Core.Constants.CountryCodes.UnitedKingdom, typeof(GB.IGBOrgSupplierPartDataLoad) },
				{ Core.Constants.CountryCodes.Canada, typeof(CA.ICAOrgSupplierPartDataLoad) },
				{ Core.Constants.CountryCodes.Singapore, typeof(SG.ISGOrgSupplierPartDataLoad) },
				{ Core.Constants.CountryCodes.SouthAfrica, typeof(ZA.IOrgSupplierPartDataLoad) },
				{ Core.Constants.CountryCodes.Japan, typeof(IGlobalOrgSupplierPartDataLoad) },
				{ Core.Constants.CountryCodes.Germany, typeof(IGlobalOrgSupplierPartDataLoad) },
				{ Core.Constants.CountryCodes.Taiwan, typeof(TW.IOrgSupplierPartDataLoad) }
			};

			foreach (var pair in dictionary)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(pair.Key))
				{
					Assert("PartsDataLoad for " + pair.Key, pair.Value.IsAssignableFrom(OrgSupplierPartDataLoad.New().GetType()));
				}
			}
		}
		#endregion

		#region TestNewForPuertoRico

		public void TestNewForPuertoRico()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			Assert(OrgSupplierPartDataLoad.New() is US.IOrgSupplierPartDataLoad);
		}

		#endregion

		#region TestValidationOfFile

		[ExpectException(typeof(FileNotFoundException))]
		public void TestValidationOfFile()
		{
			Loader.ImportProductData("non-existant file", false, false);
		}

		#endregion

		#region TestValidationOfContent

		public void TestValidationOfContent()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("This is the header for the file.");
					sw.WriteLine("This is the first part - invalid text");
				}
				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertEquals(2, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, Loader.RunCounters.RecsCreated);
				AssertEquals(0, Loader.RunCounters.RecsUpdated);
				AssertEquals(0, Loader.RunCounters.RecsExcluded);
				AssertEquals(4, Loader.Log.Count);
				AssertEquals("FileHeaderIsValid", false, Loader.FileHeaderIsValid);
			}
		}

		#endregion

		#region TestLocalProductNumberAndDescriptionUpdate

		[ExpectNoExceptions]
		public void TestLocalProductNumberAndDescriptionUpdate()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testOrganisation1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "AUMEL"));

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Owner,Description,LocalPartNumber,LocalPartDescription");
					sw.WriteLine("MFITest2," + testOrganisation1.OH_Code + ",MFI Test Product2,LocalPartNumberButWithAValueINTheTestFileThatExceedsTheLimitOfThirtyFiveCharacters,LocalPartDescription");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertCorrectNumberOfPartsNowExist(1);

				OrgSupplierPart enterprisePart = LoadPart("MFITest2");
				AssertEquals(1, enterprisePart.RelatedOrganisations.Count);
				AssertEquals("LocalPartNumberButWithAValueINTheTe", enterprisePart.RelatedOrganisations[0].OU_LocalPartNumber);
				AssertEquals("LocalPartDescription", enterprisePart.RelatedOrganisations[0].OU_LocalPartDescription);
				var editLogs = enterprisePart.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code));
				AssertEquals("Should have 1 edit log.", 1, editLogs.Length);

				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Owner,Description,LocalPartNumber,LocalPartDescription");
					sw.WriteLine("MFITest2," + testOrganisation1.OH_Code + ",MFI Test Product UPDATE2,LocalPartNumber UPDATE2,LocalPartDescription UPDATE2");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, true, false);

				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				OrgSupplierPart enterprisePart2 = factory2.Load<OrgSupplierPart>(enterprisePart.PK);
				AssertEquals(1, enterprisePart2.RelatedOrganisations.Count);
				AssertEquals("LocalPartNumber UPDATE2", enterprisePart2.RelatedOrganisations[0].OU_LocalPartNumber);
				AssertEquals("LocalPartDescription UPDATE2", enterprisePart2.RelatedOrganisations[0].OU_LocalPartDescription);
				editLogs = enterprisePart2.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code));
				AssertEquals("Should have 2 edit log.", 2, editLogs.Length);
			}
		}

		#endregion

		#region TestLocalProductNumberAndDescriptionUpdateWithSupplierOnly

		public void TestLocalProductNumberAndDescriptionUpdateWithSupplierOnly()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testOrganisation1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "AUMEL"));

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Supplier,Description,LocalPartNumber,LocalPartDescription");
					sw.WriteLine("MFITest2," + testOrganisation1.OH_Code + ",MFI Test Product2,LocalPartNumber,LocalPartDescription");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertCorrectNumberOfPartsNowExist(1);

				OrgSupplierPart enterprisePart = LoadPart("MFITest2");
				AssertEquals(1, enterprisePart.RelatedOrganisations.Count);
				AssertEquals("LocalPartNumber", enterprisePart.RelatedOrganisations[0].OU_LocalPartNumber);
				AssertEquals("LocalPartDescription", enterprisePart.RelatedOrganisations[0].OU_LocalPartDescription);

				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Supplier,Description,LocalPartNumber,LocalPartDescription");
					sw.WriteLine("MFITest2," + testOrganisation1.OH_Code + ",MFI Test Product UPDATE2,LocalPartNumber UPDATE2,LocalPartDescription UPDATE2");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, true, false);

				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				OrgSupplierPart enterprisePart2 = factory2.Load<OrgSupplierPart>(enterprisePart.PK);
				AssertEquals(1, enterprisePart2.RelatedOrganisations.Count);
				AssertEquals("LocalPartNumber UPDATE2", enterprisePart2.RelatedOrganisations[0].OU_LocalPartNumber);
				AssertEquals("LocalPartDescription UPDATE2", enterprisePart2.RelatedOrganisations[0].OU_LocalPartDescription);
			}
		}

		#endregion

		#region TestImportLocalPartNumberAndDescription

		public void TestImportLocalPartNumberAndDescription()
		{
			ClearCustomsRecordsBeforeTesting();
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "HKHKG"));
			var testOrganisation1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "AUSYD"));
			var testOrganisation2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "AUMEL"));
			var testOrganisation3 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "NZAKL"));
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					// Add some text to the file. This test file will have the header line + 4 lines of data, 2 lines that duplicate a part for the same buyer/supplier organisations
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,LocalPartNumber,LocalPartDescription");
					sw.WriteLine("THISISATEST1234567890ABCDEFGHIJKLM,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",KG,,," + testOrganisation.OH_Code + "," + testOrganisation1.OH_Code + ";" + testOrganisation2.OH_Code + ";" + testOrganisation3.OH_Code + ",,,LocalPartNumber,LocalPartDescription");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertCorrectNumberOfPartsNowExist(1);

				var enterprisePart = LoadPart("THISISATEST1234567890ABCDEFGHIJKLM");
				AssertNotNull(enterprisePart);
				AssertEquals("OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS", enterprisePart.OP_Desc);
				AssertEquals(4, enterprisePart.RelatedOrganisations.Count);

				var rel = GetOrgPartRelationByOrganisationCode(enterprisePart.RelatedOrganisations, testOrganisation.OH_Code);
				AssertNotNull(rel);
				AssertEquals("", rel.OU_LocalPartNumber);
				AssertEquals("", rel.OU_LocalPartDescription);

				var rel1 = GetOrgPartRelationByOrganisationCode(enterprisePart.RelatedOrganisations, testOrganisation1.OH_Code);
				AssertNotNull(rel1);
				AssertEquals("LocalPartNumber", rel1.OU_LocalPartNumber);
				AssertEquals("LocalPartDescription", rel1.OU_LocalPartDescription);

				var rel2 = GetOrgPartRelationByOrganisationCode(enterprisePart.RelatedOrganisations, testOrganisation2.OH_Code);
				AssertNotNull(rel2);
				AssertEquals("LocalPartNumber", rel2.OU_LocalPartNumber);
				AssertEquals("LocalPartDescription", rel2.OU_LocalPartDescription);

				var rel3 = GetOrgPartRelationByOrganisationCode(enterprisePart.RelatedOrganisations, testOrganisation3.OH_Code);
				AssertNotNull(rel3);
				AssertEquals("", rel3.OU_LocalPartNumber);
				AssertEquals("", rel3.OU_LocalPartDescription);
			}
		}

		#endregion

		#region TestImportCSVProducts

		public void TestImportCSVProducts()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					// Add some text to the file. This test file will have the header line + 4 lines of data, 2 lines that duplicate a part for the same buyer/supplier organisations
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo");
					sw.WriteLine("1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",KG,,," + testOrganisation.OH_Code + ",,,0");
					sw.WriteLine("1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",NO,,," + testOrganisation.OH_Code + ",,,0");
					sw.WriteLine("912.226,\"TAPS,COCKS,VALVES AND SIMILAR APPLIANCES FOR PIPES BOILER SHELLS,TANKS,VATS OR LIKE,INCL PRESSURE RE- DUCING VALVES & THERMOSTATICALLY CONTROLLED VALVES\",NO,,," + testOrganisation.OH_Code + ",,,0");
					sw.WriteLine("123,,,,," + testOrganisation.OH_Code + ",,,0");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertCorrectNumberOfPartsNowExist(3);

				OrgSupplierPart enterprisePart = LoadPart("912.226");
				AssertEquals("TAPS,COCKS,VALVES AND SIMILAR APPLIANCES FOR PIPES BOILER SHELLS,TANKS,VATS OR LIKE,INCL PRESSURE RE- DUCING VALVES & THERMOSTAT", enterprisePart.OP_Desc);
				AssertEquals("NO", enterprisePart.OP_StockKeepingUnit);
				Assert(enterprisePart.Notes.HasNotes);
				StmNoteCollection testNotes = (StmNoteCollection)enterprisePart.Notes.GetAllNotes();
				AssertEquals("Full Product Description", testNotes[0].ST_Description);
				AssertEquals("TAPS,COCKS,VALVES AND SIMILAR APPLIANCES FOR PIPES BOILER SHELLS,TANKS,VATS OR LIKE,INCL PRESSURE RE- DUCING VALVES & THERMOSTATICALLY CONTROLLED VALVES", testNotes[0].ST_NoteDataAsText);
				AssertEquals("Description should default from Product number when not present:", "123", LoadPart("123").OP_Desc);
			}
		}

		public void TestImportCSVProductsButPukeIfAmbiguousMatch()
		{
			ObjectFactory.Get<Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			ClearCustomsRecordsBeforeTesting();
			var queryA = new ZQuery();
			var queryB = new ZQuery();
			var queryC = new ZQuery();
			queryA.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A");
			queryB.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B");
			queryC.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "C");
			var testOrganisationA = Factory.LoadTop1<OrgHeader>(queryA);
			var testOrganisationB = Factory.LoadTop1<OrgHeader>(queryB);
			var testOrganisationC = Factory.LoadTop1<OrgHeader>(queryC);
			var existingProduct1 = Factory.New<OrgSupplierPart>();
			existingProduct1.OP_PartNum = "Product";
			var ou1A = existingProduct1.RelatedOrganisations.AddNew();
			ou1A.OU_Relationship = "OWN";
			ou1A.OU_OH = testOrganisationB.PK;
			var ou1B = existingProduct1.RelatedOrganisations.AddNew();
			ou1B.OU_Relationship = "SUP";
			ou1B.OU_OH = testOrganisationA.PK;
			var existingProduct2 = Factory.New<OrgSupplierPart>();
			existingProduct2.OP_PartNum = "Product";
			var ou2A = existingProduct2.RelatedOrganisations.AddNew();
			ou2A.OU_Relationship = "OWN";
			ou2A.OU_OH = testOrganisationC.PK;
			var ou2C = existingProduct2.RelatedOrganisations.AddNew();
			ou2C.OU_Relationship = "SUP";
			ou2C.OU_OH = testOrganisationA.PK;
			// Result:
			// Code=Product;OWN=A;SUP=B
			// Code=Product;OWN=A;SUP=C
			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,Owner,Supplier");
					sw.WriteLine("Product,CSV Data,," + testOrganisationA.OH_Code);
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, true, false);
				AssertCorrectNumberOfPartsNowExist(2);
				AssertContains("Ambiguous match", Loader.Log[1]);
			}

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,Owner,Supplier");
					sw.WriteLine("Product,CSV Data," + testOrganisationC.OH_Code + "," + testOrganisationA.OH_Code);
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, true, false);
				AssertCorrectNumberOfPartsNowExist(2);
				existingProduct1.Reload();
				existingProduct2.Reload();
				AssertEquals("", existingProduct1.OP_Desc);
				AssertEquals("CSV Data", existingProduct2.OP_Desc);
			}
		}

		public void TestImportedClassEmptyTariffNotEmpty()
		{
			/*
			 * TEST
			 *	a.	Remove the current functionality that creates a new Lookup code by setting a GUID as the name.
			 *	b.	Update Tariff and set Class. Lookup to ‘’ (if it isn’t already)
			 */
			ClearCustomsRecordsBeforeTesting();

			var loader = OrgSupplierPartDataLoad.New();
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ImportTariff,Supplier");
					sw.WriteLine("TEST,Test_Part_No_Tariff,KG,7301100001," + testOrganisation.OH_Code);
					sw.Flush();
				}

				loader.ImportProductData(testFileName.Filename, false, false);
				var enterprisePart = LoadPart("TEST");
				AssertEquals("Test_Part_No_Tariff", enterprisePart.OP_Desc);
				AssertEquals("KG", enterprisePart.OP_StockKeepingUnit);
				var classPivots = LoadPivots(enterprisePart);
				AssertEquals("CusClassPartPivot should have been created and linked to this part.", 1, classPivots.Length);

				classPivots = LoadPivots(enterprisePart);
				AssertEquals("Pre-condition - still only 1 pivot for this product", 1, classPivots.Length);

				// run another import file again with UpdateParts = true
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ImportTariff,Supplier");
					sw.WriteLine("TEST,Part_Updated,NO,6112.11.00 25," + testOrganisation.OH_Code);
					sw.Flush();
				}

				loader.ImportProductData(testFileName.Filename, true, false);
				Factory.Save();
				enterprisePart.Reload();
				AssertEquals("Product description has been updated", "Part_Updated", enterprisePart.OP_Desc);
				AssertEquals("Product UQ has been updated", "NO", enterprisePart.OP_StockKeepingUnit);

				classPivots = LoadPivots(enterprisePart);
				AssertEquals("There should still be only 1 CusClassPartPivot linked to this part.", 1, classPivots.Length);
			}
		}

		#endregion

		#region TestImportProductWithInvalidWeightUQ

		public void TestImportProductWithInvalidWeightUQ()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,Supplier,Weight_Unit");
					sw.WriteLine("123,Numbers," + testOrganisation.OH_Code + ",XX");
				}
				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertEquals("Line 2: PART NO/DESC: 123 / Numbers  Warning: Invalid Weight Code 'XX'", Loader.Log[1]);
			}
		}

		#endregion

		#region TestImportProductWithInvalidClassificationType
		public void TestImportProductWithInvalidClassificationType()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,ClassificationType,Owner,Supplier");
					sw.WriteLine($"123,Numbers,HTI:,{testOrganisation.OH_Code},{testOrganisation.OH_Code}");
				}
				OrgSupplierPartDataLoad loader = null;
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
				{
					loader = OrgSupplierPartDataLoad.New();
				}
				loader.ImportProductData(testFileName.Filename, false, false);
				AssertEquals(string.Format(CultureInfo.CurrentCulture, "Line 2: PART NO/DESC: 123 / Numbers  Classification Type length exceeds the maximum length of {0}", CusClassPartPivotSchema.CI_ChildType.MaxLength), loader.Log[1]);
			}
		}
		#endregion

		#region TestImportProductWithInvalidCubicUQ

		public void TestImportProductWithInvalidCubicUQ()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,Supplier,Volume_Unit");
					sw.WriteLine("123,Numbers," + testOrganisation.OH_Code + ",YY");
				}
				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertEquals("Line 2: PART NO/DESC: 123 / Numbers  Warning: Invalid Volume Code 'YY'", Loader.Log[1]);
			}
		}

		#endregion

		#region TestAutoCreateClassification

		public void TestAutoCreateClassification()
		{
			ClearCustomsRecordsBeforeTesting();

			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,UQ,ImportTariff,Supplier");
					sw.WriteLine("TEST,KG,01010000    ," + testOrganisation.OH_Code);
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, false);
				OrgSupplierPart enterprisePart = LoadPart("TEST");
				AssertEquals("KG", enterprisePart.OP_StockKeepingUnit);
			}

			ClearCustomsRecordsBeforeTesting();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,UQ,ExportTariff,Supplier");
					sw.WriteLine("TEST,KG,01010000    ," + testOrganisation.OH_Code);
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, false);
				OrgSupplierPart enterprisePart = LoadPart("TEST");
				AssertEquals("KG", enterprisePart.OP_StockKeepingUnit);
			}
		}

		#endregion

		#region TestImportCSVProductsExpandedData

		public void TestImportCSVProductsExpandedData()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var commodity1 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity1.RH_Code = "XX1";
			var commodity2 = Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, "XX2");
			if (commodity2 != null)
			{
				commodity2.Delete();
			}

			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					// This test file has the header line + 4 lines of data, 2 lines that duplicate a part for the same buyer/supplier organisations
					sw.WriteLine("Unit_NetWeight,Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo,Commodity,BrandName,Model");
					sw.WriteLine(",1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",KG,,," + testOrganisation.OH_Code + ",,10,KG,.005,M3,,0");
					sw.WriteLine(",1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",NO,,," + testOrganisation.OH_Code + ",,1.385,KG,0.002,M3,,0");
					sw.WriteLine("995.34,912.226,\"TAPS,COCKS,VALVES AND SIMILAR APPLIANCES FOR PIPES BOILER SHELLS,TANKS,VATS OR LIKE,INCL PRESSURE RE- DUCING VALVES & THERMOSTATICALLY CONTROLLED VALVES\",NO,,," + testOrganisation.OH_Code + ",,1.385,KG,0.002,M3,,0,,,,,,,XX1,Super Brand,Super Model");
					sw.WriteLine(",123,,,,," + testOrganisation.OH_Code + ",,,,,,,,0");
					sw.WriteLine(",Cordial,Flavoured Cordial,L,,," + testOrganisation.OH_Code + ",,1500.75,G,1.75,L,,0,,,,,,,XX2");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertCorrectNumberOfPartsNowExist(4);

				OrgSupplierPart enterprisePart = LoadPart("912.226");
				AssertEquals("TAPS,COCKS,VALVES AND SIMILAR APPLIANCES FOR PIPES BOILER SHELLS,TANKS,VATS OR LIKE,INCL PRESSURE RE- DUCING VALVES & THERMOSTAT", enterprisePart.OP_Desc);
				AssertEquals("NO", enterprisePart.OP_StockKeepingUnit);
				Assert(enterprisePart.Notes.HasNotes);
				StmNoteCollection testNotes = (StmNoteCollection)enterprisePart.Notes.GetAllNotes();
				AssertEquals("Full Product Description", testNotes[0].ST_Description);
				AssertEquals("TAPS,COCKS,VALVES AND SIMILAR APPLIANCES FOR PIPES BOILER SHELLS,TANKS,VATS OR LIKE,INCL PRESSURE RE- DUCING VALVES & THERMOSTATICALLY CONTROLLED VALVES", testNotes[0].ST_NoteDataAsText);
				AssertEquals("Weight should have been imported", 1.385m, enterprisePart.OP_Weight);
				AssertEquals("Weight UQ should have been imported", "KG", enterprisePart.OP_WeightUQ);
				AssertEquals("Volume should have been imported", 0.002m, enterprisePart.OP_Cubic);
				AssertEquals("Volume UQ should have been imported", "M3", enterprisePart.OP_CubicUQ);
				AssertEquals("Net weight should have been imported", 995.34m, enterprisePart.OP_NetWeight);

				AssertEquals("Description should default from Product number when not present:", "123", LoadPart("123").OP_Desc);
				AssertEquals("Commodity should be loaded", commodity1.RH_Code, enterprisePart.OP_RH_NKCommodityCode);
				AssertEquals("BrandName should be loaded", "Super Brand", enterprisePart.OP_Brand);
				AssertEquals("Model should be loaded", "Super Model", enterprisePart.OP_Model);

				enterprisePart = LoadPart("1");
				AssertEquals("Weight should have been imported", 10m, enterprisePart.OP_Weight);
				AssertEquals("Weight UQ should have been imported", "KG", enterprisePart.OP_WeightUQ);
				AssertEquals("Volume should have been imported", 0.005m, enterprisePart.OP_Cubic);
				AssertEquals("Volume UQ should have been imported", "M3", enterprisePart.OP_CubicUQ);

				enterprisePart = LoadPart("Cordial");
				AssertEquals("Weight should have been imported", 1500.75m, enterprisePart.OP_Weight);
				AssertEquals("Weight UQ should have been imported", "G", enterprisePart.OP_WeightUQ);
				AssertEquals("Volume should have been imported", 1.75m, enterprisePart.OP_Cubic);
				AssertEquals("Volume UQ should have been imported", "L", enterprisePart.OP_CubicUQ);
				Assert("Commodity should not be loaded", enterprisePart.OP_RH_NKCommodityCode.IsEmpty);
				Assert("BrandName", enterprisePart.OP_Brand.IsEmpty);
				Assert("Model", enterprisePart.OP_Model.IsEmpty);
			}
		}

		#endregion

		#region TestSamePartCreatedForDifferentBuyers

		public void TestSamePartCreatedForDifferentBuyers()
		{
			// re-run the same file but after setting up different buyer organisation codes - it should now create two part 1 records - one for each buyer
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testOrganisation1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			OrgHeader testOrganisation2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo");
					sw.WriteLine("1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",KG,,," + testOrganisation1.OH_Code + ",,,0");
					sw.WriteLine("1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",NO,,," + testOrganisation2.OH_Code + ",,,0");
					sw.WriteLine("912.226,\"TAPS,COCKS,VALVES AND SIMILAR APPLIANCES FOR PIPES BOILER SHELLS,TANKS,VATS OR LIKE,INCL PRESSURE RE- DUCING VALVES & THERMOSTATICALLY CONTROLLED VALVES\",NO,,," + testOrganisation1.OH_Code + ",,,0");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertCorrectNumberOfPartsNowExist(3);
			}
		}

		#endregion

		#region TestImportOfCostFields

		public void TestImportOfCostFields()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo,Last_Cost,Weighted_Cost,Cost_Currency");
					sw.WriteLine("123,,,,," + testOrganisation.OH_Code + ",,,,,,,,0,,,,,,123.1234,9.98,AUD");
					sw.WriteLine("Cordial,Flavoured Cordial,L,,," + testOrganisation.OH_Code + ",,1500.75,G,1.75,L,,0,,,,,,,2.35,,XXX");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertCorrectNumberOfPartsNowExist(2);

				OrgSupplierPart enterprisePart = LoadPart("123");
				AssertEquals("Description should default from Product number when not present:", "123", enterprisePart.OP_Desc);
				AssertEquals("Last Cost should have been imported", 123.1234m, enterprisePart.OP_LastCost);
				AssertEquals("Weighted Cost should have been imported", 9.98m, enterprisePart.OP_WeightedCost);
				AssertEquals("Cost Currency should have been imported", "AUD", enterprisePart.OP_RX_NKLastWeightedCostCurr);

				enterprisePart = LoadPart("Cordial");
				AssertEquals("Weight should have been imported", 1500.75m, enterprisePart.OP_Weight);
				AssertEquals("Weight UQ should have been imported", "G", enterprisePart.OP_WeightUQ);
				AssertEquals("Volume should have been imported", 1.75m, enterprisePart.OP_Cubic);
				AssertEquals("Volume UQ should have been imported", "L", enterprisePart.OP_CubicUQ);
				AssertEquals("Last Cost should have been imported", 2.35m, enterprisePart.OP_LastCost);
				AssertEquals("Weighted Cost should not have been imported", 0m, enterprisePart.OP_WeightedCost);
				AssertEquals("Cost Currency is Invalid and should not have been imported", "", enterprisePart.OP_RX_NKLastWeightedCostCurr);
			}
		}

		#endregion

		#region TestImportWithUpdateOfBarcodes

		public void TestImportWithUpdateOfBarcodes()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					// This test file has the header line + 3 lines of data, 2 lines that duplicate a part for the same buyer/supplier organisations
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo,Last_Cost,Weighted_Cost,Cost_Currency,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package,Barcode3,Barcode3_Package,Barcode4,Barcode4_Package,Barcode5,Barcode5_Package,Barcode1_UseForDocuments");
					sw.WriteLine("123,,,,," + testOrganisation.OH_Code + ",,,,,,,,0,,,,,,123.1234,9.98,AUD,1,DRM,UNT,2,CTN,UNT,3,BOX,UNT,4,PLT,UNT,5,UNT,BAG,UNTBarcode,UNT,DRMBarcode,DRM,CTNBarcode,CTN,,,,,Y");
					sw.WriteLine("123,,,,," + testOrganisation.OH_Code + ",,,,,,,,0,,,,,,123.1234,9.98,AUD,1,DRM,UNT,2,CTN,UNT,3,BOX,UNT,4,PLT,UNT,5,UNT,BAG,Barcode1,UNT,Barcode2,DRM,Barcode3,CTN,Barcode4,BOX,Barcode5,PLT,N");
					sw.WriteLine("Cordial,Flavoured Cordial,L,,," + testOrganisation.OH_Code + ",,,,,,,,0,,,,,,123.1234,9.98,AUD");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, true, false);
				AssertCorrectNumberOfPartsNowExist(2);

				var part1 = LoadPart("123");
				AssertEquals(8, part1.PartBarcodes.Count);
				AssertEquals(true, part1.PartBarcodes.FindPartBarcode("UNT", "UNTBarcode").PH_UseForDocuments);
				AssertEquals(false, part1.PartBarcodes.FindPartBarcode("DRM", "DRMBarcode").PH_UseForDocuments);
				AssertEquals(false, part1.PartBarcodes.FindPartBarcode("CTN", "CTNBarcode").PH_UseForDocuments);
				AssertEquals(false, part1.PartBarcodes.FindPartBarcode("UNT", "Barcode1").PH_UseForDocuments);
				AssertEquals(false, part1.PartBarcodes.FindPartBarcode("DRM", "Barcode2").PH_UseForDocuments);
				AssertEquals(false, part1.PartBarcodes.FindPartBarcode("CTN", "Barcode3").PH_UseForDocuments);
				AssertEquals(false, part1.PartBarcodes.FindPartBarcode("BOX", "Barcode4").PH_UseForDocuments);
				AssertEquals(false, part1.PartBarcodes.FindPartBarcode("PLT", "Barcode5").PH_UseForDocuments);

				var part2 = LoadPart("Cordial");
				AssertEquals(0, part2.PartBarcodes.Count);
			}
		}

		#endregion

		#region TestImportProductWithInvalidBarcodePackage

		public void TestImportProductWithInvalidBarcodePackage()
		{
			ClearCustomsRecordsBeforeTesting();
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo,Last_Cost,Weighted_Cost,Cost_Currency,UC1_QtyParent,UC1_Package,UC1_ParentPackage,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package,Barcode1_UseForDocuments");
					sw.WriteLine("123,,,,," + testOrganisation.OH_Code + ",,,,,,,,0,,,,,,123.1234,9.98,AUD,1,DRM,UNT,Barcode1,UNT,Barcode2,DRM_AAAA,Y");
					sw.Flush();
				}

				var loader = GetNewDataLoader();
				var logs = new List<string>();
				loader.LogUpdated += (s, e) => logs.Add(e.LogMessage);
				loader.ImportProductData(testFileName.Filename, true, false);
				AssertCorrectNumberOfPartsNowExist(1);

				OrgSupplierPart part1 = LoadPart("123");
				AssertEquals(2, part1.PartBarcodes.Count);
				AssertEquals("Barcode1", part1.PartBarcodes.FindPartBarcode("UNT", "Barcode1").PH_Barcode);
				AssertEquals("Barcode2", part1.PartBarcodes.FindPartBarcode("DRM", "Barcode2").PH_Barcode);

				AssertEquals(@"Part barcode package type 'DRM_AAAA' is too long. Storing 'DRM' instead.", loader.Log[1]);
				AssertEquals(@"Part barcode package type 'DRM_AAAA' is too long. Storing 'DRM' instead.", logs[1]);
			}
		}

		#endregion

		#region TestImportProduct_WithEmptyBarcodePackageType

		public void TestImportProduct_WithEmptyBarcodePackageType()
		{
			ClearCustomsRecordsBeforeTesting();
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo,Last_Cost,Weighted_Cost,Cost_Currency,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package,Barcode2_UseForDocuments");
					sw.WriteLine("123,,,,," + testOrganisation.OH_Code + ",,,,,,,,0,,,,,,123.1234,9.98,AUD,Barcode1,   ,Barcode2,UNT,Y");
					sw.Flush();
				}

				var logEventAddedToDisplayScreen = false;
				Loader.LogUpdated += (s, e) =>
				{
					if (e.LogMessage.Contains("Please enter a non-empty Barcode and Pack Type for each Part Barcode"))
					{
						logEventAddedToDisplayScreen = true;
					}
				};

				Loader.ImportProductData(testFileName.Filename, true, false);
				AssertCorrectNumberOfPartsNowExist(1);

				OrgSupplierPart part1 = LoadPart("123");
				AssertEquals(1, part1.PartBarcodes.Count);
				AssertEquals("Barcode2", part1.PartBarcodes.FindPartBarcode("UNT", "Barcode2").PH_Barcode);
				Assert("Log event should be added to display log.", logEventAddedToDisplayScreen);
				AssertEquals("Barcode1 (Barcode: 'Barcode1', Pack Type: '') for Product '123' is invalid. This Part Barcode will not be imported. Please enter a non-empty Barcode and Pack Type for each Part Barcode.", Loader.Log[1]);
			}
		}

		#endregion

		#region TestImportProduct_WithEmptyBarcode

		public void TestImportProduct_WithEmptyBarcode()
		{
			ClearCustomsRecordsBeforeTesting();
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo,Last_Cost,Weighted_Cost,Cost_Currency,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package,Barcode1_UseForDocuments");
					sw.WriteLine("123,,,,," + testOrganisation.OH_Code + ",,,,,,,,0,,,,,,123.1234,9.98,AUD,Barcode1,UNT,    ,PKG,Y");
					sw.Flush();
				}

				var logEventAddedToDisplayScreen = false;
				Loader.LogUpdated += (s, e) =>
				{
					if (e.LogMessage.Contains("Please enter a non-empty Barcode and Pack Type for each Part Barcode"))
					{
						logEventAddedToDisplayScreen = true;
					}
				};

				Loader.ImportProductData(testFileName.Filename, true, false);
				AssertCorrectNumberOfPartsNowExist(1);

				var part1 = LoadPart("123");
				AssertEquals(1, part1.PartBarcodes.Count);
				AssertEquals("Barcode1", part1.PartBarcodes.FindPartBarcode("UNT", "Barcode1").PH_Barcode);
				Assert("Log event should be added to display log.", logEventAddedToDisplayScreen);
				AssertEquals("Barcode2 (Barcode: '', Pack Type: 'PKG') for Product '123' is invalid. This Part Barcode will not be imported. Please enter a non-empty Barcode and Pack Type for each Part Barcode.", Loader.Log[1]);
			}
		}

		#endregion

		#region TestImportProduct_WithEmptyBarcodeAndPackType

		public void TestImportProduct_WithEmptyBarcodeAndPackType()
		{
			ClearCustomsRecordsBeforeTesting();
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo,Last_Cost,Weighted_Cost,Cost_Currency,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package");
					sw.WriteLine("123,,,,," + testOrganisation.OH_Code + ",,,,,,,,0,,,,,,123.1234,9.98,AUD,    ,   ,    ,   ");
					sw.Flush();
				}

				var logEventAddedToDisplayScreen = false;
				Loader.LogUpdated += (s, e) =>
				{
					if (e.LogMessage.Contains("Please enter a non-empty Barcode and Pack Type for each Part Barcode"))
					{
						logEventAddedToDisplayScreen = true;
					}
				};

				Loader.ImportProductData(testFileName.Filename, true, false);
				AssertCorrectNumberOfPartsNowExist(1);

				var part1 = LoadPart("123");
				AssertEquals(0, part1.PartBarcodes.Count);
				Assert("Log event should be added to display log.", logEventAddedToDisplayScreen);
				AssertEquals("Barcode1 (Barcode: '', Pack Type: '') for Product '123' is invalid. This Part Barcode will not be imported. Please enter a non-empty Barcode and Pack Type for each Part Barcode.", Loader.Log[1]);
				AssertEquals("Barcode2 (Barcode: '', Pack Type: '') for Product '123' is invalid. This Part Barcode will not be imported. Please enter a non-empty Barcode and Pack Type for each Part Barcode.", Loader.Log[2]);
			}
		}

		#endregion

		#region TestImportOfUNDG

		public void TestImportOfUNDG()
		{
			ClearCustomsRecordsBeforeTesting();
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var filter = new ZQuery(UNDGSubstanceSchema.DG_UNNO, "1009");
			filter.AddToFilter(UNDGSubstanceSchema.DG_Variant, "A");
			var testUNDG = Factory.LoadTop1<UNDGSubstance>(filter);

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo,Last_Cost,UNDG_Code");
					sw.WriteLine("REFRIGERANT GAS,Bromotrifluoromethane,L,,," + testOrganisation.OH_Code + ",,1500.75,G,1.75,L,,0,,,,,,,2.35,1009a");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertCorrectNumberOfPartsNowExist(1);

				var enterprisePart = LoadPart("REFRIGERANT GAS");
				AssertEquals("Description:", "Bromotrifluoromethane", enterprisePart.OP_Desc);
				AssertEquals("Last Cost should have been imported", 2.35m, enterprisePart.OP_LastCost);
				AssertEquals("UNDG should have been imported", testUNDG.DG_Code, enterprisePart.UNDGs[0].Substance.DG_Code);
			}
		}

		public void TestImportOfUNDG_SubstanceAlreadyExists()
		{
			ClearCustomsRecordsBeforeTesting();
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var filter = new ZQuery(UNDGSubstanceSchema.DG_UNNO, "1009");
			filter.AddToFilter(UNDGSubstanceSchema.DG_Variant, "A");
			var testUNDG = Factory.LoadTop1<UNDGSubstance>(filter);

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo,Last_Cost,UNDG_Code");
					sw.WriteLine("REFRIGERANT GAS,Bromotrifluoromethane,L,,," + testOrganisation.OH_Code + ",,1500.75,G,1.75,L,,0,,,,,,,2.35,1009a");
					sw.Flush();
				}

				// First import
				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertCorrectNumberOfPartsNowExist(1);

				// Second import
				Loader.ImportProductData(testFileName.Filename, true, false);
				AssertCorrectNumberOfPartsNowExist(1);

				var enterprisePart = LoadPart("REFRIGERANT GAS");
				AssertEquals("Description:", "Bromotrifluoromethane", enterprisePart.OP_Desc);
				AssertEquals("Last Cost should have been imported", 2.35m, enterprisePart.OP_LastCost);
				AssertEquals("Single UNDG should exist.", 1, enterprisePart.UNDGs.Count);
				AssertEquals("UNDG should have been imported", testUNDG.DG_Code, enterprisePart.UNDGs[0].Substance.DG_Code);
			}
		}

		public void TestImportOfUNDG_AdditionalSubstance()
		{
			ClearCustomsRecordsBeforeTesting();
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var filter1 = new ZQuery(UNDGSubstanceSchema.DG_UNNO, "1009");
			filter1.AddToFilter(UNDGSubstanceSchema.DG_Variant, "A");
			var testUNDG1 = Factory.LoadTop1<UNDGSubstance>(filter1);
			AssertNotNull(testUNDG1);

			var filter2 = new ZQuery(UNDGSubstanceSchema.DG_UNNO, "1009");
			filter2.AddToFilter(UNDGSubstanceSchema.DG_Variant, "B");
			var testUNDG2 = Factory.LoadTop1<UNDGSubstance>(filter2);
			AssertNotNull(testUNDG2);

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo,Last_Cost,UNDG_Code");
					sw.WriteLine("REFRIGERANT GAS,Bromotrifluoromethane,L,,," + testOrganisation.OH_Code + ",,1500.75,G,1.75,L,,0,,,,,,,2.35,1009a");
					sw.Flush();
				}

				// First import
				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertCorrectNumberOfPartsNowExist(1);
			}

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo,Last_Cost,UNDG_Code");
					sw.WriteLine("REFRIGERANT GAS,Bromotrifluoromethane,L,,," + testOrganisation.OH_Code + ",,1500.75,G,1.75,L,,0,,,,,,,2.35,1009b");
					sw.Flush();
				}

				// Second import
				Loader.ImportProductData(testFileName.Filename, true, false);
				AssertCorrectNumberOfPartsNowExist(1);

				var enterprisePart = LoadPart("REFRIGERANT GAS");
				AssertEquals("Description:", "Bromotrifluoromethane", enterprisePart.OP_Desc);
				AssertEquals("Last Cost should have been imported", 2.35m, enterprisePart.OP_LastCost);
				AssertEquals("Two UNDGs should exist.", 2, enterprisePart.UNDGs.Count);
				AssertContainsExactElementsInAnyOrder("UNDGs should have been imported", new[] { testUNDG1.DG_Code, testUNDG2.DG_Code }, enterprisePart.UNDGs.Select(u => u.Substance.DG_Code));
			}
		}

		#endregion

		#region TestImportOfUNDGWithNoVariant

		public void TestImportOfUNDGWithNoVariant()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			ZQuery filter = new ZQuery(UNDGSubstanceSchema.DG_UNNO, "2357");
			filter.AddToFilter(UNDGSubstanceSchema.DG_Variant, " ");
			UNDGSubstance testUNDG = Factory.LoadTop1<UNDGSubstance>(filter);

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo,Last_Cost,UNDG_Code");
					sw.WriteLine("xx-3948,Dangerous Goods,L,,," + testOrganisation.OH_Code + ",,1500.75,G,1.75,L,,0,,,,,,,8595,2357");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertCorrectNumberOfPartsNowExist(1);

				OrgSupplierPart enterprisePart = LoadPart("xx-3948");
				AssertEquals("Description:", "Dangerous Goods", enterprisePart.OP_Desc);
				AssertEquals("Last Cost should have been imported", 8595m, enterprisePart.OP_LastCost);
				AssertEquals("UNDG should have been imported", testUNDG.DG_Code, enterprisePart.UNDGs[0].Substance.DG_Code);
			}
		}

		#endregion

		#region TestImportProductsWithInconsistentData

		public void TestImportProductsWithInconsistentData()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo");
					sw.WriteLine("1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",KG,,," + testOrganisation.OH_Code);
					sw.WriteLine("912.226,\"TAPS,VALVES AND SIMILAR\",NO,,");
					sw.WriteLine("123,RUBBER GASKETS,,,," + testOrganisation.OH_Code);
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertCorrectNumberOfPartsNowExist(2);

				var testLog = Loader.Log;
				AssertEquals(4, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(2, Loader.RunCounters.RecsCreated);
				AssertEquals(0, Loader.RunCounters.RecsUpdated);
				AssertEquals(1, Loader.RunCounters.RecsExcluded);
				AssertEquals(3, Loader.Log.Count);
			}
		}

		#endregion

		#region TestMultipleSuppliersForSamePart

		public void TestMultipleSuppliersForSamePart()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testSupplier1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			OrgHeader testSupplier2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			OrgHeader testSupplier3 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "C"));

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo");
					sw.WriteLine("1,OTHER PARTS OF VULCANISED RUBBER,KG,,,," + testSupplier1.OH_Code + ",,0");
					sw.WriteLine("1,OTHER PARTS OF VULCANISED RUBBER,NO,,,," + testSupplier2.OH_Code + ",,0");
					sw.WriteLine("1,OTHER PARTS OF VULCANISED RUBBER,NO,,,," + testSupplier3.OH_Code + ",,0");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertCorrectNumberOfPartsNowExist(3);

				OrgSupplierPart enterprisePart = LoadPart("1");
				ZQuery suppliersFilter = new ZQuery(OrgPartRelationSchema.OU_OP, SQLComparisonOperator.Equal, enterprisePart.PK);
				suppliersFilter.AddToFilter(OrgPartRelationSchema.OU_Relationship, "SUP");
				BusinessObject[] suppliersLinked = Factory.Load<OrgPartRelation>(suppliersFilter);
				AssertEquals("Should have been only 1 supplier linked to this part:", 1, suppliersLinked.Length);
			}
		}

		#endregion

		#region TestPartIsNotCreatedWhenOwnerOrSupplierIsNotFound

		public void TestPartIsNotCreatedWhenOwnerOrSupplierIsNotFound()
		{
			ClearCustomsRecordsBeforeTesting();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo");
					sw.WriteLine("1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",KG,,,unknown owner,,,0");
					sw.WriteLine("912.226,\"TAPS,COCKS,VALVES AND SIMILAR APPLIANCES\",NO,,,,unknown supplier,,0");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertCorrectNumberOfPartsNowExist(0);
			}
		}

		#endregion

		#region TestPartIsNotCreatedWhenOwnerOrSupplierIsNotProvided

		public void TestPartIsNotCreatedWhenOwnerOrSupplierIsNotProvided()
		{
			ClearCustomsRecordsBeforeTesting();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo");
					sw.WriteLine("1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",KG,,,,,,0");
					sw.WriteLine("912.226,\"TAPS,COCKS,VALVES AND SIMILAR APPLIANCES\",NO,,,,,,0");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertCorrectNumberOfPartsNowExist(0);
			}
		}

		#endregion

		#region TestPartIsNotCreatedWhenLookupIsNotFound

		public void TestPartIsNotCreatedWhenLookupIsNotFound()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo");
					sw.WriteLine("1,RUBBER GASKET,KG,XXX,Class-Lookup," + testOrganisation.OH_Code + ",");
					sw.WriteLine("P1234-4848X,Test Part,,Exp Lookup,," + testOrganisation.OH_Code + "," + testSupplier1.OH_Code);
					sw.WriteLine("P1234-4848I,Test Part,,,Imp Lookup," + testOrganisation.OH_Code + "," + testSupplier1.OH_Code);
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertCorrectNumberOfPartsNowExist(0);
				AssertEquals(4, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, Loader.RunCounters.RecsCreated);
				AssertEquals(0, Loader.RunCounters.RecsUpdated);
				AssertEquals(3, Loader.RunCounters.RecsExcluded);
				AssertEquals(5, Loader.Log.Count);
			}
		}

		#endregion

		#region TestErrorInLoadingAndUpdating

		public void TestPartIsNotCreatedWhenErrorInLoading()
		{
			ClearCustomsRecordsBeforeTesting();
			var testOwner = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier");
					sw.WriteLine($"P1234,RUBBER GASKET,KG,,,{testOwner.OH_Code},");
					sw.Flush();
				}

				var testLoader = new OrgSupplierPartDataLoadForTest();
				testLoader.ImportProductData(testFileName.Filename, true, false);

				var part = new BusinessObjectFactory().LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "P1234"));
				AssertNull("Part was not created", part);

				var logText = string.Concat(testLoader.Log.ToList<string>());
				AssertContains(" PART NO/DESC: P1234 / RUBBER GASKET  Unable to load this part - Test Error.", logText);
				AssertContains("T O T A L : Products created = 0, Products updated = 0, Products excluded = 1", logText);
			}
		}

		public void TestPartIsNotUpdatedWhenErrorInUpdating()
		{
			ClearCustomsRecordsBeforeTesting();
			var testOwner = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier");
					sw.WriteLine($"P1234,RUBBER GASKET,KG,,,{testOwner.OH_Code},");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, true, false);
			}

			var part = new BusinessObjectFactory().LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "P1234"));
			AssertNotNull("Part is created", part);

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier");
					sw.WriteLine($"P1234,RUBBER GASKET SEAL,KG,,,{testOwner.OH_Code},");
					sw.Flush();
				}

				var testLoader = new OrgSupplierPartDataLoadForTest();
				testLoader.ImportProductData(testFileName.Filename, true, false);

				part.Reload();
				AssertEquals("Did not change the part description", "RUBBER GASKET", part.OP_Desc);

				var logText = string.Concat(testLoader.Log.ToList<string>());
				AssertContains("PART NO/DESC: P1234 / RUBBER GASKET SEAL  Unable to update this part - Test Error.", logText);
				AssertContains("T O T A L : Products created = 0, Products updated = 0, Products excluded = 1", logText);
			}
		}

		class OrgSupplierPartDataLoadForTest : OrgSupplierPartDataLoad
		{
			protected override void SetTariffAndClassificationDetails(OrgSupplierPart enterprisePart, PartsDataToLoad dataToLoad)
			{
				throw new ArgumentException("Test Error.");
			}
		}

		#endregion

		#region TestImportUsingLegacyCodes

		public void TestImportUsingLegacyCodes()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description");
					sw.WriteLine("1,RUBBER GASKET,KG,,,," + testOrganisation.OH_Code + ",,0");
					sw.WriteLine("P1234-4848X,Test Part,,,," + testOrganisation.OH_Code + "," + testSupplier1.OH_Code + ",Major Accounts,175850");
					sw.WriteLine("P1234-4848I,Test Part,,,,," + testSupplier1.OH_Code + ",Major Accounts,175850");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, true);
				AssertCorrectNumberOfPartsNowExist(0);
			}

			OrgCusCode ownerLSC = Factory.New<OrgCusCode>();
			ownerLSC.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
			ownerLSC.OK_CustomsRegNo = "TstOrgLSC";
			testOrganisation.CustomsCodes.Add(ownerLSC);

			OrgCusCode supplierLSC = Factory.New<OrgCusCode>();
			supplierLSC.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
			supplierLSC.OK_CustomsRegNo = "SpplrLSC";
			testSupplier1.CustomsCodes.Add(supplierLSC);

			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit");
					sw.WriteLine("1,RUBBER GASKET,KG,,,,TstOrgLSC,,0");
					sw.WriteLine("P1234-4848X,Test Part,,,,TstOrgLSC,SpplrLSC");
					sw.WriteLine("P1234-4848I,Test Part,,,,,SpplrLSC");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, true);
				AssertCorrectNumberOfPartsNowExist(3);

				OrgSupplierPart enterprisePart = LoadPart("1");
				AssertEquals("Description:", "RUBBER GASKET", enterprisePart.OP_Desc);
				AssertEquals("Relations", 1, enterprisePart.RelatedOrganisations.Count);
				OrgHeader ownerOrg = enterprisePart.RelatedOrganisations[0].Organisation;
				AssertEquals("Legacy code match not found", testOrganisation.PK, ownerOrg.PK);

				enterprisePart = LoadPart("P1234-4848X");
				AssertEquals("Description:", "Test Part", enterprisePart.OP_Desc);
				AssertEquals("Relations", 2, enterprisePart.RelatedOrganisations.Count);

				enterprisePart = LoadPart("P1234-4848I");
				AssertEquals("Description:", "Test Part", enterprisePart.OP_Desc);
				AssertEquals("Relations", 1, enterprisePart.RelatedOrganisations.Count);
				OrgHeader supplierOrg = enterprisePart.RelatedOrganisations[0].Organisation;
				AssertEquals("Legacy code match not found", testSupplier1.PK, supplierOrg.PK);
			}
		}

		#endregion

		#region TestImportWithAudit

		public void TestImportWithAuditEnabled_ImportExport()
		{
			ClearCustomsRecordsBeforeTesting();
			Env.Security.CustomsSupplierPartAuditImport.IsAllowed = true;
			Env.Security.CustomsSupplierPartAuditExport.IsAllowed = false;

			var owner = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var class1 = CreateClassification("IMP");
			var class2 = CreateClassification("EXP");
			Factory.Save();

			var contents = new[]
			{
				"Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier"
				, $"COTTON,COTTON's DESCRIPTION,,,{class1.CC_LookupCode},{owner.OH_Code},"
				, $"SILK,SILK's DESCRIPTION,,{class2.CC_LookupCode},,{owner.OH_Code},"
			};

			using (var csvFile = CreateCsvFileForTest(contents))
			{
				var loader = GetNewDataLoader();
				loader.AuditMessage = "My audit test message";
				loader.ImportProductData(csvFile.Filename, false, false, true);

				AssertCorrectNumberOfPartsNowExist(2);

				var part = LoadPart("COTTON");
				var pivot = LoadPivots(part)[0];
				Assert("Audited date", !pivot.CI_LastAuditedDate.IsEmpty);
				Assert("Audited user", !pivot.CI_LastAuditedUser.IsEmpty);
				var log = GetEvent((BusinessObject)pivot, Events.RecordAuditedCode);
				AssertNotNull("Should have an audit log record.", log);
				AssertEquals("Should have the audit message.", "My audit test message", log.SL_Reference);

				part = LoadPart("SILK");
				pivot = LoadPivots(part)[0];
				Assert("Audited date", pivot.CI_LastAuditedDate.IsEmpty);
				Assert("Audited user", pivot.CI_LastAuditedUser.IsEmpty);
				log = GetEvent((BusinessObject)pivot, Events.RecordAuditedCode);
				AssertNull("Should NOT have an audit log record.", log);
				AssertImportProductLogContains("Import log should contain a failed to audit message.", loader.Log, "PART NO/DESC: SILK / SILK's DESCRIPTION  Failed to audit Classification Line.");
			}
		}

		public void TestImportWithAuditEnabled_Both()
		{
			try
			{
				ClearCustomsRecordsBeforeTesting();
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
				Env.Security.CustomsSupplierPartAuditImport.IsAllowed = true;
				Env.Security.CustomsSupplierPartAuditExport.IsAllowed = true;

				var owner = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var class1 = CreateClassification("BTH");
				Factory.Save();

				var contents = new[]
				{
					"Code,Description,UQ,ClassificationType,ClassificationLookup,Tariff,Owner,Supplier"
					, $"COTTON,COTTON's DESCRIPTION,,HTB,{class1.CC_LookupCode},,{owner.OH_Code},"
				};

				using (var csvFile = CreateCsvFileForTest(contents))
				{
					var loader = GetNewDataLoader();
					loader.AuditMessage = "My audit test message";
					loader.ImportProductData(csvFile.Filename, false, false, true);

					AssertCorrectNumberOfPartsNowExist(1);
					var part = LoadPart("COTTON");
					var pivot = LoadPivots(part)[0];
					Assert("Audited date", !pivot.CI_LastAuditedDate.IsEmpty);
					Assert("Audited user", !pivot.CI_LastAuditedUser.IsEmpty);
					var log = GetEvent((BusinessObject)pivot, Events.RecordAuditedCode);
					AssertNotNull("Should have an audit log record.", log);
					AssertEquals("Should have the audit message.", "My audit test message", log.SL_Reference);
				}

				Env.Security.CustomsSupplierPartAuditImport.IsAllowed = true;
				Env.Security.CustomsSupplierPartAuditExport.IsAllowed = false;

				contents = new[]
				{
					"Code,Description,UQ,ClassificationType,ClassificationLookup,Tariff,Owner,Supplier"
					, $"SILK,SILK's DESCRIPTION,,HTB,{class1.CC_LookupCode},,{owner.OH_Code},"
				};

				using (var csvFile = CreateCsvFileForTest(contents))
				{
					var loader = GetNewDataLoader();
					loader.AuditMessage = "My audit test message";
					loader.ImportProductData(csvFile.Filename, false, false, true);

					AssertCorrectNumberOfPartsNowExist(2);
					var part = LoadPart("SILK");
					var pivot = LoadPivots(part)[0];
					Assert("Audited date", pivot.CI_LastAuditedDate.IsEmpty);
					Assert("Audited user", pivot.CI_LastAuditedUser.IsEmpty);
					var log = GetEvent((BusinessObject)pivot, Events.RecordAuditedCode);
					AssertNull("Should NOT have an audit log record.", log);
					AssertImportProductLogContains("Import log should contain a failed to audit message.", loader.Log, "PART NO/DESC: SILK / SILK's DESCRIPTION  Failed to audit Classification Line.");
				}
			}
			finally
			{
				SetupCountry();
			}
		}

		IBaseCusClassification CreateClassification(string type)
		{
			var result = Factory.New<IBaseCusClassification>();
			result[CusClassificationSchema.CC_ClassificationType.Name] = type;
			result[CusClassificationSchema.CC_LookupCode.Name] = "LOOKUP";
			result[CusClassificationSchema.CC_TariffNum.Name] = "x";
			result[CusClassificationSchema.CC_Description.Name] = "DESCRIPTION";
			return result;
		}

		StmALog GetEvent(BusinessObject parent, string eventCode)
		{
			var query = new ZQuery(StmALogSchema.SL_Table, parent.TableName)
				.AddToFilter(new ZQuery(StmALogSchema.SL_Parent, parent.PK))
				.AddToFilter(new ZQuery(StmALogSchema.SL_SE_NKEvent, eventCode));
			return Factory.LoadTop1<StmALog>(query);
		}

		void AssertImportProductLogContains(string message, IReadOnlyList<string> log, string text)
		{
			var result = log.OfType<string>().Any(x => x.Contains(text));
			Assert(message, result);
		}

		TempFile CreateCsvFileForTest(string[] contents)
		{
			var result = TempFile.NewWithExtension("csv");
			using (var sw = new StreamWriter(result.Filename))
			{
				Array.ForEach(contents, x => sw.WriteLine(x));
			}
			return result;
		}

		#endregion

		#region TestGetOrgPKFromRawOrgCode

		public void TestGetOrgPKFromRawOrgCode()
		{
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Division,QtyinStock");
					sw.WriteLine("P1234-4848X,Test Part,,,," + testOrganisation.OH_Code + "," + testSupplier1.OH_Code + ",Major Accounts,175850");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, false);

				OrgSupplierPart enterprisePart = LoadPart("P1234-4848X");
				AssertEquals("Test Part", enterprisePart.OP_Desc);
				Assert(enterprisePart.RelatedOrganisations.Count > 0);
				AssertEquals("Buyer link not found", testOrganisation.PK, enterprisePart.RelatedOrganisations[0].OU_OH);
				AssertEquals("Supplier link not found", testSupplier1.PK, enterprisePart.RelatedOrganisations[1].OU_OH);
			}
		}

		#endregion

		#region TestCheckPartsThatAlreadyExistAreExcluded

		public void TestCheckPartsThatAlreadyExistAreExcluded()
		{
			ClearCustomsRecordsBeforeTesting();

			ZQuery checkFilter1 = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
			BusinessObject[] enterprisePartsCreated1 = Factory.Load(typeof(OrgSupplierPart), checkFilter1);
			AssertEquals("There should 0 part records before testing", 0, enterprisePartsCreated1.Length);

			OrgHeader testBuyer = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			OrgHeader testSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));

			OrgSupplierPart testPart1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			testPart1.OP_PartNum = "Part with Owner";
			OrgSupplierPart testPart2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			testPart2.OP_PartNum = "Part with Owner and Supplier";
			OrgSupplierPart testPart3 = Factory.NewWithValidTestData<OrgSupplierPart>();
			testPart3.OP_PartNum = "Part with Supplier";
			OrgSupplierPart testPart4 = Factory.NewWithValidTestData<OrgSupplierPart>();
			testPart4.OP_PartNum = "Part with Both";

			OrgPartRelation relation1 = Factory.NewWithValidTestData<OrgPartRelation>();
			relation1.OU_OP = testPart1.PK;
			relation1.OU_OH = testBuyer.PK;
			relation1.OU_Relationship = "OWN";

			OrgPartRelation relation2 = Factory.NewWithValidTestData<OrgPartRelation>();
			relation2.OU_OP = testPart2.PK;
			relation2.OU_OH = testBuyer.PK;
			relation2.OU_Relationship = "OWN";

			OrgPartRelation relation3 = Factory.NewWithValidTestData<OrgPartRelation>();
			relation3.OU_OP = testPart2.PK;
			relation3.OU_OH = testSupplier.PK;
			relation3.OU_Relationship = "SUP";

			OrgPartRelation relation4 = Factory.NewWithValidTestData<OrgPartRelation>();
			relation4.OU_OP = testPart3.PK;
			relation4.OU_OH = testSupplier.PK;
			relation4.OU_Relationship = "SUP";

			OrgPartRelation relation5 = Factory.NewWithValidTestData<OrgPartRelation>();
			relation5.OU_OP = testPart4.PK;
			relation5.OU_OH = testSupplier.PK;
			relation5.OU_Relationship = "BTH";

			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo");
					sw.WriteLine("Part with Owner,WASHERS,KG,,," + testBuyer.OH_Code + ",");
					sw.WriteLine("Part with Owner and Supplier,VALVES,NO,,," + testBuyer.OH_Code + "," + testSupplier.OH_Code);
					sw.WriteLine("Part with Supplier,SPINDLES,NO,,,," + testSupplier.OH_Code);
					sw.WriteLine("Part with Both,LUGS,NO,,,," + testSupplier.OH_Code);
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, false);

				ZQuery checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load(typeof(OrgSupplierPart), checkFilter);
				AssertEquals("There should only be 4 part records - NO additional part records should be created", 9, enterprisePartsCreated.Length);
				AssertEquals(5, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, Loader.RunCounters.RecsCreated);
				AssertEquals(0, Loader.RunCounters.RecsUpdated);
				AssertEquals(4, Loader.RunCounters.RecsExcluded);
				AssertEquals(6, Loader.Log.Count);
			}
		}

		#endregion

		#region TestReactivateInactivePart

		public void TestReactivateInactivePart()
		{
			ClearCustomsRecordsBeforeTesting();

			var buyer1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "AUSJOH"));
			var supplier1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "BARNSO"));
			var buyer2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "JAYSCH"));
			var supplier2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "JANSEW"));

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "TEST PART 123";
			part1.RelatedOrganisations.AddOrganisationIfNotExist(buyer1.PK, OrgPartRelation.RelationshipTypes.Both);
			part1.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier);
			part1.RelatedOrganisations.AddOrganisationIfNotExist(buyer2.PK, OrgPartRelation.RelationshipTypes.Owner);

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "TEST PART 123";
			part2.OP_IsActive = ZBool.False;
			part2.RelatedOrganisations.AddOrganisationIfNotExist(buyer1.PK, OrgPartRelation.RelationshipTypes.Owner);
			part2.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier);

			var part3 = Factory.New<OrgSupplierPart>();
			part3.OP_PartNum = "TEST PART 456";
			part3.OP_IsActive = ZBool.False;
			part3.RelatedOrganisations.AddOrganisationIfNotExist(buyer2.PK, OrgPartRelation.RelationshipTypes.Owner);
			part3.RelatedOrganisations.AddOrganisationIfNotExist(supplier1.PK, OrgPartRelation.RelationshipTypes.Supplier);

			var part4 = Factory.New<OrgSupplierPart>();
			part4.OP_PartNum = "TEST PART 456";
			part4.RelatedOrganisations.AddOrganisationIfNotExist(buyer1.PK, OrgPartRelation.RelationshipTypes.Owner);
			part4.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier);

			var part5 = Factory.New<OrgSupplierPart>();
			part5.OP_PartNum = "TEST OWNER MATCH";
			part5.OP_IsActive = false;
			part5.RelatedOrganisations.AddOrganisationIfNotExist(buyer1.PK, OrgPartRelation.RelationshipTypes.Owner);
			part5.RelatedOrganisations.AddOrganisationIfNotExist(supplier1.PK, OrgPartRelation.RelationshipTypes.Supplier);

			var part6 = Factory.New<OrgSupplierPart>();
			part6.OP_PartNum = "TEST OWNER MATCH";
			part6.OP_IsActive = false;
			part6.RelatedOrganisations.AddOrganisationIfNotExist(buyer1.PK, OrgPartRelation.RelationshipTypes.Owner);

			var part7 = Factory.New<OrgSupplierPart>();
			part7.OP_PartNum = "TEST SUPPLIER MATCH";
			part7.OP_IsActive = false;
			part7.RelatedOrganisations.AddOrganisationIfNotExist(buyer1.PK, OrgPartRelation.RelationshipTypes.Owner);
			part7.RelatedOrganisations.AddOrganisationIfNotExist(supplier1.PK, OrgPartRelation.RelationshipTypes.Supplier);

			var part8 = Factory.New<OrgSupplierPart>();
			part8.OP_PartNum = "TEST SUPPLIER MATCH";
			part8.OP_IsActive = false;
			part8.RelatedOrganisations.AddOrganisationIfNotExist(supplier1.PK, OrgPartRelation.RelationshipTypes.Supplier);

			var part9 = Factory.New<OrgSupplierPart>();
			part9.OP_PartNum = "TEST OWNER MATCH";
			part9.OP_IsActive = false;
			part9.RelatedOrganisations.AddOrganisationIfNotExist(buyer2.PK, OrgPartRelation.RelationshipTypes.Owner);
			part9.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier);

			var part10 = Factory.New<OrgSupplierPart>();
			part10.OP_PartNum = "TEST OWNER MATCH";
			part10.OP_IsActive = false;
			part10.RelatedOrganisations.AddOrganisationIfNotExist(buyer2.PK, OrgPartRelation.RelationshipTypes.Both);

			var part11 = Factory.New<OrgSupplierPart>();
			part11.OP_PartNum = "TEST SUPPLIER MATCH";
			part11.OP_IsActive = false;
			part11.RelatedOrganisations.AddOrganisationIfNotExist(buyer2.PK, OrgPartRelation.RelationshipTypes.Owner);
			part11.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier);

			var part12 = Factory.New<OrgSupplierPart>();
			part12.OP_PartNum = "TEST SUPPLIER MATCH";
			part12.OP_IsActive = false;
			part12.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Both);

			Factory.Save();

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo");
					sw.WriteLine("TEST PART 123,WASHERS,KG,,," + supplier1.OH_Code + ";" + buyer1.OH_Code + "," + supplier2.OH_Code);
					sw.WriteLine("TEST PART 456,VALVES,NO,,," + buyer2.OH_Code + "," + supplier1.OH_Code);
					sw.WriteLine("TEST PART 456,SPINDLES,NO,,," + buyer1.OH_Code + "," + supplier2.OH_Code);
					sw.WriteLine("TEST OWNER MATCH,OWNER DESC,NO,,," + buyer1.OH_Code + ",");
					sw.WriteLine("TEST SUPPLIER MATCH,SUPPLIER DESC,NO,,,," + supplier1.OH_Code);
					sw.WriteLine("TEST OWNER MATCH,OWNER 2 DESC,KG,,," + buyer2.OH_Code + ",");
					sw.WriteLine("TEST SUPPLIER MATCH,SUPPLIER 2 DESC,KG,,,," + supplier2.OH_Code);
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, true, false);

				var checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				var enterprisePartsCreated = Factory.Load<OrgSupplierPart>(checkFilter);
				AssertEquals("There should only be 12 part records - NO additional part records should be created", 12, enterprisePartsCreated.Length);
				AssertEquals(8, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, Loader.RunCounters.RecsCreated);
				AssertEquals(7, Loader.RunCounters.RecsUpdated);
				AssertEquals(0, Loader.RunCounters.RecsExcluded);
				AssertEquals(14, Loader.Log.Count);
				AssertMultilineASCIIEquals("Log", @"Products to Import = 7
PART NO: TEST PART 123 - Part has been UPDATED
Line 3: PART NO/DESC: TEST PART 456 / VALVES  Owner [JAYSCH] Supplier [BARNSO]: Inactive Part has been re-activated.
PART NO: TEST PART 456 - Part has been UPDATED
PART NO: TEST PART 456 - Part has been UPDATED
Line 5: PART NO/DESC: TEST OWNER MATCH / OWNER DESC  Owner [AUSJOH] Supplier [<No Supplier>]: Inactive Part has been re-activated.
PART NO: TEST OWNER MATCH - Part has been UPDATED
Line 6: PART NO/DESC: TEST SUPPLIER MATCH / SUPPLIER DESC  Owner [<No Owner>] Supplier [BARNSO]: Inactive Part has been re-activated.
PART NO: TEST SUPPLIER MATCH - Part has been UPDATED
Line 7: PART NO/DESC: TEST OWNER MATCH / OWNER 2 DESC  Owner [JAYSCH] Supplier [<No Supplier>]: Inactive Part has been re-activated.
PART NO: TEST OWNER MATCH - Part has been UPDATED
Line 8: PART NO/DESC: TEST SUPPLIER MATCH / SUPPLIER 2 DESC  Owner [<No Owner>] Supplier [JANSEW]: Inactive Part has been re-activated.
PART NO: TEST SUPPLIER MATCH - Part has been UPDATED

T O T A L : Products created = 0, Products updated = 7, Products excluded = 0", new ZStringBuilder(new TypedEnumerable<string>(Loader.Log)).ToStringWithNewLineBetweenAppends());

				part1.Reload();
				AssertEquals("OP_Desc", "WASHERS", part1.OP_Desc);
				AssertEquals("OP_StockKeepingUnit", "KG", part1.OP_StockKeepingUnit);
				part1.RelatedOrganisations.Load();
				AssertEquals(4, part1.RelatedOrganisations.Count);
				AssertNotNull(part1.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(buyer1.PK, OrgPartRelation.RelationshipTypes.Both));
				AssertNotNull(part1.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier));
				AssertNotNull(part1.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(buyer2.PK, OrgPartRelation.RelationshipTypes.Owner));
				AssertNotNull(part1.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(supplier1.PK, OrgPartRelation.RelationshipTypes.Owner));

				part2.Reload();
				AssertEquals("OP_Desc", "", part2.OP_Desc);
				AssertEquals("OP_StockKeepingUnit", "UNT", part2.OP_StockKeepingUnit);
				part2.RelatedOrganisations.Load();
				AssertEquals(2, part2.RelatedOrganisations.Count);
				AssertNotNull(part2.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(buyer1.PK, OrgPartRelation.RelationshipTypes.Owner));
				AssertNotNull(part2.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier));

				part3.Reload();
				AssertEquals("OP_Desc", "VALVES", part3.OP_Desc);
				AssertEquals("OP_StockKeepingUnit", "NO", part3.OP_StockKeepingUnit);
				part3.RelatedOrganisations.Load();
				AssertEquals(2, part3.RelatedOrganisations.Count);
				AssertNotNull(part3.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(buyer2.PK, OrgPartRelation.RelationshipTypes.Owner));
				AssertNotNull(part3.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(supplier1.PK, OrgPartRelation.RelationshipTypes.Supplier));

				part4.Reload();
				AssertEquals("OP_Desc", "SPINDLES", part4.OP_Desc);
				AssertEquals("OP_StockKeepingUnit", "NO", part4.OP_StockKeepingUnit);
				part4.RelatedOrganisations.Load();
				AssertEquals(2, part4.RelatedOrganisations.Count);
				AssertNotNull(part4.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(buyer1.PK, OrgPartRelation.RelationshipTypes.Owner));
				AssertNotNull(part4.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier));

				part5.Reload();
				AssertEquals("OP_Desc", "", part5.OP_Desc);
				AssertEquals("OP_StockKeepingUnit", "UNT", part5.OP_StockKeepingUnit);
				AssertEquals("OP_IsActive", false, part5.OP_IsActive);
				part5.RelatedOrganisations.Load();
				AssertEquals(2, part5.RelatedOrganisations.Count);
				AssertNotNull(part5.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(buyer1.PK, OrgPartRelation.RelationshipTypes.Owner));
				AssertNotNull(part5.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(supplier1.PK, OrgPartRelation.RelationshipTypes.Supplier));

				part6.Reload();
				AssertEquals("OP_Desc", "OWNER DESC", part6.OP_Desc);
				AssertEquals("OP_StockKeepingUnit", "NO", part6.OP_StockKeepingUnit);
				AssertEquals("OP_IsActive", true, part6.OP_IsActive);
				part6.RelatedOrganisations.Load();
				AssertEquals(1, part6.RelatedOrganisations.Count);
				AssertNotNull(part6.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(buyer1.PK, OrgPartRelation.RelationshipTypes.Owner));

				part7.Reload();
				AssertEquals("OP_Desc", "", part7.OP_Desc);
				AssertEquals("OP_StockKeepingUnit", "UNT", part7.OP_StockKeepingUnit);
				AssertEquals("OP_IsActive", false, part7.OP_IsActive);
				part7.RelatedOrganisations.Load();
				AssertEquals(2, part7.RelatedOrganisations.Count);
				AssertNotNull(part7.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(buyer1.PK, OrgPartRelation.RelationshipTypes.Owner));
				AssertNotNull(part7.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(supplier1.PK, OrgPartRelation.RelationshipTypes.Supplier));

				part8.Reload();
				AssertEquals("OP_Desc", "SUPPLIER DESC", part8.OP_Desc);
				AssertEquals("OP_StockKeepingUnit", "NO", part8.OP_StockKeepingUnit);
				AssertEquals("OP_IsActive", true, part8.OP_IsActive);
				part8.RelatedOrganisations.Load();
				AssertEquals(1, part8.RelatedOrganisations.Count);
				AssertNotNull(part8.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(supplier1.PK, OrgPartRelation.RelationshipTypes.Supplier));

				part9.Reload();
				AssertEquals("OP_Desc", "", part9.OP_Desc);
				AssertEquals("OP_StockKeepingUnit", "UNT", part9.OP_StockKeepingUnit);
				AssertEquals("OP_IsActive", false, part9.OP_IsActive);
				part9.RelatedOrganisations.Load();
				AssertEquals(2, part9.RelatedOrganisations.Count);
				AssertNotNull(part9.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(buyer2.PK, OrgPartRelation.RelationshipTypes.Owner));
				AssertNotNull(part9.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier));

				part10.Reload();
				AssertEquals("OP_Desc", "OWNER 2 DESC", part10.OP_Desc);
				AssertEquals("OP_StockKeepingUnit", "KG", part10.OP_StockKeepingUnit);
				AssertEquals("OP_IsActive", true, part10.OP_IsActive);
				part10.RelatedOrganisations.Load();
				AssertEquals(1, part10.RelatedOrganisations.Count);
				AssertNotNull(part10.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(buyer2.PK, OrgPartRelation.RelationshipTypes.Both));

				part11.Reload();
				AssertEquals("OP_Desc", "", part11.OP_Desc);
				AssertEquals("OP_StockKeepingUnit", "UNT", part11.OP_StockKeepingUnit);
				AssertEquals("OP_IsActive", false, part11.OP_IsActive);
				part11.RelatedOrganisations.Load();
				AssertEquals(2, part11.RelatedOrganisations.Count);
				AssertNotNull(part11.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(buyer2.PK, OrgPartRelation.RelationshipTypes.Owner));
				AssertNotNull(part11.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier));

				part12.Reload();
				AssertEquals("OP_Desc", "SUPPLIER 2 DESC", part12.OP_Desc);
				AssertEquals("OP_StockKeepingUnit", "KG", part12.OP_StockKeepingUnit);
				AssertEquals("OP_IsActive", true, part12.OP_IsActive);
				part12.RelatedOrganisations.Load();
				AssertEquals(1, part12.RelatedOrganisations.Count);
				AssertNotNull(part12.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(supplier2.PK, OrgPartRelation.RelationshipTypes.Both));
			}
		}

		#endregion

		#region TestImportProductsWithUpdateIncludingUnits

		public void TestImportProductsWithUpdateIncludingUnits()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Department,Division,QtyinStock,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC1_Cubic,UC1_Depth,UC1_Height,UC1_Width,UC1_Weight,UC2_Cubic,UC2_Depth,UC2_Height,UC2_Width,UC2_Weight");
					sw.WriteLine("Part1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",KG,,," + testOrganisation.OH_Code + ",,,,0,1,L,BOT,,,,,,,,,,,,,");
					sw.WriteLine("Part1,RUBBER WASHERS,NO,,," + testOrganisation.OH_Code + ",,Department1,PLUMBING,5780,6,BOT,CTN,2,L,BOT,2,4,6,8,10,1,2,3,4,5");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, true, false);
				AssertCorrectNumberOfPartsNowExist(1);

				OrgSupplierPart updatedPart = LoadPart("Part1");
				AssertEquals("Part Description should have been overriden", "RUBBER WASHERS", updatedPart.OP_Desc);
				AssertEquals("Stock Unit should have been overriden", "NO", updatedPart.OP_StockKeepingUnit);
				AssertEquals("Division should have been updated", "PLUMBING", updatedPart.OP_Division);
				AssertEquals("Division should have been updated", "Department1", updatedPart.OP_Department);
				AssertEquals("Stock Qty should have been updated", 5780M, updatedPart.OP_QtyInStock);

				AssertEquals("2 units added", 2, updatedPart.PartUnits.Count);
				AssertEquals("first unit qty updated", (ZDecimal)2, updatedPart.PartUnits[0].OF_QuantityInParent);
				AssertEquals("first unit package", "L", updatedPart.PartUnits[0].OF_PackType);
				AssertEquals("first unit parent package", "BOT", updatedPart.PartUnits[0].OF_ParentPackType);
				AssertEquals("first unit cubic", 1m, updatedPart.PartUnits[0].OF_Cubic);
				AssertEquals("first unit depth", 2m, updatedPart.PartUnits[0].OF_Depth);
				AssertEquals("first unit height", 3m, updatedPart.PartUnits[0].OF_Height);
				AssertEquals("first unit width", 4m, updatedPart.PartUnits[0].OF_Width);
				AssertEquals("first unit weight", 5m, updatedPart.PartUnits[0].OF_Weight);
				AssertEquals("2nd unit qty", (ZDecimal)6, updatedPart.PartUnits[1].OF_QuantityInParent);
				AssertEquals("2nd unit package", "BOT", updatedPart.PartUnits[1].OF_PackType);
				AssertEquals("2nd unit parent package", "CTN", updatedPart.PartUnits[1].OF_ParentPackType);
				AssertEquals("2nd unit cubic", 2m, updatedPart.PartUnits[1].OF_Cubic);
				AssertEquals("2nd unit depth", 4m, updatedPart.PartUnits[1].OF_Depth);
				AssertEquals("2nd unit height", 6m, updatedPart.PartUnits[1].OF_Height);
				AssertEquals("2nd unit width", 8m, updatedPart.PartUnits[1].OF_Width);
				AssertEquals("2nd unit weight", 10m, updatedPart.PartUnits[1].OF_Weight);
			}
		}

		public void TestImportProductsWithUpdateUnitConversionMaxLength()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Department,Division,QtyinStock,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage");
					sw.WriteLine("Part1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",KG,,," + testOrganisation.OH_Code + ",,,,0,1,L,BOT,,,");
					sw.WriteLine("Part1,RUBBER WASHERS,NO,,," + testOrganisation.OH_Code + ",,Department1,PLUMBING,5780,6,BOTE,CTNE,2,L,BOT");
					sw.Flush();
				}

				var loader = GetNewDataLoader();
				var logs = new List<string>();
				loader.LogUpdated += (s, e) => logs.Add(e.LogMessage);
				loader.ImportProductData(testFileName.Filename, true, false);
				AssertCorrectNumberOfPartsNowExist(1);

				OrgSupplierPart updatedPart = LoadPart("Part1");
				AssertEquals("Part Description should have been overriden", "RUBBER WASHERS", updatedPart.OP_Desc);
				AssertEquals("Stock Unit should have been overriden", "NO", updatedPart.OP_StockKeepingUnit);
				AssertEquals("Division should have been updated", "PLUMBING", updatedPart.OP_Division);
				AssertEquals("Division should have been updated", "Department1", updatedPart.OP_Department);
				AssertEquals("Stock Qty should have been updated", 5780M, updatedPart.OP_QtyInStock);

				AssertEquals("2 units", 2, updatedPart.PartUnits.Count);
				AssertEquals(0, loader.RunCounters.RecsExcluded);
				AssertEquals("Package type 'BOTE' is too long. Storing 'BOT' instead.", loader.Log[1]);
				AssertEquals("Package type 'BOTE' is too long. Storing 'BOT' instead.", logs[1]);
				AssertEquals("Parent package type 'CTNE' is too long. Storing 'CTN' instead.", loader.Log[2]);
				AssertEquals("Parent package type 'CTNE' is too long. Storing 'CTN' instead.", logs[2]);
			}
		}

		public void TestImportProductsWithUnitConversionToUpperCase()
		{
			ClearCustomsRecordsBeforeTesting();
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Department,Division,QtyinStock,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage");
					sw.WriteLine("Part1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",xxx,,," + testOrganisation.OH_Code + ",,,,0,1,m,yyy,,,");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, true, false);
				AssertCorrectNumberOfPartsNowExist(1);
				AssertEquals(0, Loader.RunCounters.RecsExcluded);

				var part1 = LoadPart("Part1");
				AssertEquals("1 unit conversion", 1, part1.PartUnits.Count);

				CombineAssertions(() =>
				{
					AssertEquals("Stock Unit should be converted to upper case", "XXX", part1.OP_StockKeepingUnit);
					AssertEquals("Pack Type should be converted to upper case", "M", part1.PartUnits[0].OF_PackType);
					AssertEquals("Parent Pack Type should be converted to upper case", "YYY", part1.PartUnits[0].OF_ParentPackType);
				});
			}
		}

		public void TestImportProductsWithUnitConversions_CaseDifference_NotNewConversion()
		{
			ClearCustomsRecordsBeforeTesting();
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Department,Division,QtyinStock,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage");
					sw.WriteLine("Part1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",xxx,,," + testOrganisation.OH_Code + ",,,,0,1,m,yyy,2,M,YYY");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, true, false);
				AssertCorrectNumberOfPartsNowExist(1);
				AssertEquals(0, Loader.RunCounters.RecsExcluded);

				var part1 = LoadPart("Part1");
				AssertEquals("1 unit conversion", 1, part1.PartUnits.Count);
			}
		}

		#endregion

		#region TestImportProduct_UnitConversionUnitsPackTypeValidation

		public void TestImportProduct_UnitConversionPackTypeInvalid_ErrorRegistryEnabled()
		{
			ClearCustomsRecordsBeforeTesting();
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (RawDataRegistry.Instance.UnitConversionPackTypesValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (TempFile testFileName = TempFile.New())
			{
				var uc1_Package = "XXX";
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Department,Division,QtyinStock,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage");
					sw.WriteLine("Part1,A Product with description,KG," + testOrganisation.OH_Code + ",,,,0,1," + uc1_Package + ",UNT,,,");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, true, false);

				AssertCorrectNumberOfPartsNowExist(0);
				AssertEquals(1, Loader.RunCounters.RecsExcluded);
				AssertEquals("Line 2: PART NO/DESC: Part1 / A Product with description  Error during import: Invalid package types in unit conversion: XXX", Loader.Log[1]);

				uc1_Package = "L";
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Department,Division,QtyinStock,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage");
					sw.WriteLine("Part1,A Product with description,KG," + testOrganisation.OH_Code + ",,,,0,1," + uc1_Package + ",UNT,,,");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, true, false);

				AssertCorrectNumberOfPartsNowExist(1);
				var updatedPart = LoadPart("Part1");
				AssertEquals("1 units", 1, updatedPart.PartUnits.Count);
			}
		}

		public void TestImportProduct_UnitConversionParentPackTypeInvalid_ErrorRegistryEnabled()
		{
			ClearCustomsRecordsBeforeTesting();
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (RawDataRegistry.Instance.UnitConversionPackTypesValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (TempFile testFileName = TempFile.New())
			{
				var uc1_ParentPackage = "YYY";
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Department,Division,QtyinStock,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage");
					sw.WriteLine("Part1,A Product with description,KG," + testOrganisation.OH_Code + ",,,,0,1,UNT," + uc1_ParentPackage + ",,,");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, true, false);
				AssertCorrectNumberOfPartsNowExist(0);
				AssertEquals(1, Loader.RunCounters.RecsExcluded);
				AssertEquals("Line 2: PART NO/DESC: Part1 / A Product with description  Error during import: Invalid package types in unit conversion: YYY", Loader.Log[1]);

				uc1_ParentPackage = "L";
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Department,Division,QtyinStock,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage");
					sw.WriteLine("Part1,A Product with description,KG," + testOrganisation.OH_Code + ",,,,0,1,UNT," + uc1_ParentPackage + ",,,");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, true, false);

				AssertCorrectNumberOfPartsNowExist(1);
				var updatedPart = LoadPart("Part1");
				AssertEquals("1 units", 1, updatedPart.PartUnits.Count);
			}
		}

		public void TestImportProduct_UnitConversionPackTypesInvalid_NoLogMessage()
		{
			ClearCustomsRecordsBeforeTesting();
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Department,Division,QtyinStock,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage");
					sw.WriteLine("Part1,A Product with description,KG," + testOrganisation.OH_Code + ",,,,0,1,XXX,YYY,,,");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, true, false);
				AssertCorrectNumberOfPartsNowExist(1);
				AssertEquals("\r\nT O T A L : Products created = 1, Products updated = 0, Products excluded = 0\r\n", Loader.Log[1]);
			}
		}

		public void TestImportProduct_UnitConversionPackTypesValidLowerCaseWhitespace_ErrorRegistryEnabled()
		{
			ClearCustomsRecordsBeforeTesting();
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (RawDataRegistry.Instance.UnitConversionPackTypesValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (TempFile testFileName = TempFile.New())
			{
				var uc1_Package1 = " unt ";
				var uc1_ParentPackage1 = " plt ";
				var uc1_Package2 = " kg ";
				var uc1_ParentPackage2 = " cc ";
				var uc1_Package3 = "l ";
				var uc1_ParentPackage3 = " m";
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Department,Division,QtyinStock,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage");
					sw.WriteLine("Part1,A Product with description,KG," + testOrganisation.OH_Code + ",,,,0,1," + uc1_Package1 + "," + uc1_ParentPackage1 + ",,,");
					sw.WriteLine("Part2,A Product with description,KG," + testOrganisation.OH_Code + ",,,,0,1," + uc1_Package2 + "," + uc1_ParentPackage2 + ",,,");
					sw.WriteLine("Part3,A Product with description,KG," + testOrganisation.OH_Code + ",,,,0,1," + uc1_Package3 + "," + uc1_ParentPackage3 + ",,,");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, true, false);
				AssertCorrectNumberOfPartsNowExist(3);
				AssertEquals("\r\nT O T A L : Products created = 3, Products updated = 0, Products excluded = 0\r\n", Loader.Log[1]);
			}
		}

		#endregion

		#region TestImportProductsWithMultipleSuppliers

		public void TestImportProductsWithMultipleSuppliers()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG1";
			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ORG2";
			OrgHeader org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "ORG3";
			OrgHeader org4 = Factory.New<OrgHeader>();
			org4.OH_Code = "ORG4";
			OrgHeader org5 = Factory.New<OrgHeader>();
			org5.OH_Code = "ORG5";
			OrgHeader org6 = Factory.New<OrgHeader>();
			org6.OH_Code = "ORG6";
			Factory.Save();
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Department,Division,QtyinStock");
					sw.WriteLine("Part1,PART1 DESCRIPTION,KG,,,ORG1,ORG2;ORG3,,0");
					sw.WriteLine("Part1,PART1 DESCRIPTION,KG,,,ORG1,ORG2;ORG4;ORG3;ORG7;ORG5;ORG6,Department1,PLUMBING,5780");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, true, false);
				AssertCorrectNumberOfPartsNowExist(1);

				OrgSupplierPart updatedPart = LoadPart("Part1");
				AssertEquals("Part Description", "PART1 DESCRIPTION", updatedPart.OP_Desc);
				AssertEquals("Stock Unit", "KG", updatedPart.OP_StockKeepingUnit);
				AssertEquals("Department", "Department1", updatedPart.OP_Department);
				AssertEquals("Divisionted", "PLUMBING", updatedPart.OP_Division);
				AssertEquals("Stock Qty", 5780M, updatedPart.OP_QtyInStock);

				AssertEquals("Relations", 6, updatedPart.RelatedOrganisations.Count);

				bool org1OK = false;
				bool org2OK = false;
				bool org3OK = false;
				bool org4OK = false;
				bool org5OK = false;
				bool org6OK = false;
				foreach (OrgPartRelation orgPartRelation in updatedPart.RelatedOrganisations)
				{
					OrgHeader orgHeader = orgPartRelation.Organisation;
					if (orgHeader == org1 && orgPartRelation.OU_Relationship == Enterprise.MasterFiles.Business.OrgPartRelation.RelationshipTypes.Owner)
					{
						org1OK = true;
					}
					else if (orgHeader == org2 && orgPartRelation.OU_Relationship == Enterprise.MasterFiles.Business.OrgPartRelation.RelationshipTypes.Supplier)
					{
						org2OK = true;
					}
					else if (orgHeader == org3 && orgPartRelation.OU_Relationship == Enterprise.MasterFiles.Business.OrgPartRelation.RelationshipTypes.Supplier)
					{
						org3OK = true;
					}
					else if (orgHeader == org4 && orgPartRelation.OU_Relationship == Enterprise.MasterFiles.Business.OrgPartRelation.RelationshipTypes.Supplier)
					{
						org4OK = true;
					}
					else if (orgHeader == org5 && orgPartRelation.OU_Relationship == Enterprise.MasterFiles.Business.OrgPartRelation.RelationshipTypes.Supplier)
					{
						org5OK = true;
					}
					else if (orgHeader == org6 && orgPartRelation.OU_Relationship == Enterprise.MasterFiles.Business.OrgPartRelation.RelationshipTypes.Supplier)
					{
						org6OK = true;
					}
				}
				Assert("Org1", org1OK);
				Assert("Org2", org2OK);
				Assert("Org3", org3OK);
				Assert("Org4", org4OK);
				Assert("Org5", org5OK);
				Assert("Org6", org6OK);
			}
		}

		#endregion

		#region TestImportProductsWithIncompleteLineData

		public void TestImportProductsWithIncompleteLineData()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Department,Division,QtyinStock");
					sw.WriteLine("Part1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",KG,,," + testOrganisation.OH_Code + ",,PLUMBING,3000");
					sw.WriteLine("Part2,RUBBER WASHERS,CWT,,," + testOrganisation.OH_Code);
					sw.WriteLine("Part3,RUBBER O-RINGS,NO,,," + testOrganisation.OH_Code + ",,");
					sw.WriteLine("Part4,RUBBER SEALS,NO,,," + testOrganisation.OH_Code + ",");
					sw.WriteLine("PartWithNoOrg,PLASTIC SEALS,NO,,,"); // Should not import data as no buyer/supplier code
					sw.WriteLine("Part5,RUBBER,NO,,," + testOrganisation.OH_Code + ",,Department1,PLUMBING,5780");
					sw.WriteLine("Part6,\"MUFFLER\",,,," + testOrganisation.OH_Code + ",,10,KG");  // Should default part UQ to 'UNT'.
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, true, false);
				CombineAssertions(() =>
				{
					AssertCorrectNumberOfPartsNowExist(6);

					OrgSupplierPart enterpriseProduct = LoadPart("Part2");
					AssertEquals("Part Description", "RUBBER WASHERS", enterpriseProduct.OP_Desc);
					AssertEquals("Stock Unit", "CWT", enterpriseProduct.OP_StockKeepingUnit);
					AssertEquals("Department", "", enterpriseProduct.OP_Department);
					AssertEquals("Division", "", enterpriseProduct.OP_Division);
					AssertEquals("Stock Qty", 0M, enterpriseProduct.OP_QtyInStock);

					enterpriseProduct = LoadPart("Part5");
					AssertEquals("Part Description", "RUBBER", enterpriseProduct.OP_Desc);
					AssertEquals("Stock Unit", "NO", enterpriseProduct.OP_StockKeepingUnit);
					AssertEquals("Department", "Department1", enterpriseProduct.OP_Department);
					AssertEquals("Division", "PLUMBING", enterpriseProduct.OP_Division);
					AssertEquals("Stock Qty", 5780M, enterpriseProduct.OP_QtyInStock);

					enterpriseProduct = LoadPart("Part1");
					enterpriseProduct = LoadPart("Part3");
					enterpriseProduct = LoadPart("Part4");

					enterpriseProduct = LoadPart("Part6");
					AssertEquals("Default Stock Unit", "UNT", enterpriseProduct.OP_StockKeepingUnit);
				});
			}
		}

		#endregion

		#region TestDefaultStockKeepingUnit_SansUQColumn

		public void TestDefaultStockKeepingUnit_SansUQColumn() => TestDefaultStockKeepingUnit_SansUQColumn(registryValue: null, expectedStockKeepingUnit: "UNT");
		public void TestDefaultStockKeepingUnit_SansUQColumn_Empty() => TestDefaultStockKeepingUnit_SansUQColumn(registryValue: "", expectedStockKeepingUnit: "UNT");
		public void TestDefaultStockKeepingUnit_SansUQColumn_Pallet() => TestDefaultStockKeepingUnit_SansUQColumn(registryValue: "PLT", expectedStockKeepingUnit: "PLT");
		public void TestDefaultStockKeepingUnit_SansUQColumn_Bottle() => TestDefaultStockKeepingUnit_SansUQColumn(registryValue: "BOT", expectedStockKeepingUnit: "BOT");

		void TestDefaultStockKeepingUnit_SansUQColumn(string registryValue, string expectedStockKeepingUnit)
		{
			ClearCustomsRecordsBeforeTesting();
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,ExportClassification,ImportClassification,Owner,Supplier,Department,Division,QtyinStock");
					sw.WriteLine("Part6,\"MUFFLER\",,," + testOrganisation.OH_Code + ",,10,KG");
					sw.Flush();
				}

				var originalDefaultStockUnit = DataRegistry.Instance.DefaultStockUnit;
				try
				{
					if (registryValue != null)
					{
						DataRegistry.Instance.DefaultStockUnit = registryValue;
					}

					Loader.ImportProductData(testFileName.Filename, true, false);

					CombineAssertions(() =>
					{
						AssertCorrectNumberOfPartsNowExist(1);

						var enterpriseProduct = LoadPart("Part6");
						AssertEquals("Default Stock Unit", expectedStockKeepingUnit, enterpriseProduct.OP_StockKeepingUnit);
					});
				}
				finally
				{
					if (registryValue != null)
					{
						DataRegistry.Instance.DefaultStockUnit = originalDefaultStockUnit;
					}
				}
			}
		}

		#endregion

		#region TestImportExpandedDataWithIncompleteLineData

		public void TestImportExpandedDataWithIncompleteLineData()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo,Commodity,BrandName,Model");
					sw.WriteLine("1,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",KG,,," + testOrganisation.OH_Code + ",,10,KG");
					sw.WriteLine("PartWithNoOrg,PLASTIC SEALS,NO,,,"); // Should not import data as no buyer/supplier code
					sw.WriteLine("1A,\"OTHER PARTS OF VULCANISED RUBBER - OTHER = GASKETS, WASHERS AND OTHER SEALS\",NO,,," + testOrganisation.OH_Code + ",,1.385,KG,0.002,M3");
					sw.WriteLine("912.226,\"TAPS,COCKS,VALVES AND SIMILAR APPLIANCES FOR PIPES BOILER SHELLS,TANKS,VATS OR LIKE,INCL PRESSURE RE- DUCING VALVES & THERMOSTATICALLY CONTROLLED VALVES\",NO,,," + testOrganisation.OH_Code);
					sw.WriteLine("123,,,,," + testOrganisation.OH_Code + ",,,,,,,,0");
					sw.WriteLine("P1234,RUBBER GASKET,KG,,,," + testOrganisation.OH_Code + ",1.75,G,,,,6000,NZ,FJ,P50,NZ,AD,555821");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertCorrectNumberOfPartsNowExist(5);

				OrgSupplierPart enterprisePart = LoadPart("912.226");
				AssertEquals("TAPS,COCKS,VALVES AND SIMILAR APPLIANCES FOR PIPES BOILER SHELLS,TANKS,VATS OR LIKE,INCL PRESSURE RE- DUCING VALVES & THERMOSTAT", enterprisePart.OP_Desc);
				AssertEquals("NO", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Weight should be empty", 0m, enterprisePart.OP_Weight);
				AssertEquals("Weight UQ should be blank", "", enterprisePart.OP_WeightUQ);

				enterprisePart = LoadPart("1");
				AssertEquals("Weight should have been imported", 10m, enterprisePart.OP_Weight);
				AssertEquals("Weight UQ should have been imported", "KG", enterprisePart.OP_WeightUQ);
				AssertEquals("Volume should empty", 0m, enterprisePart.OP_Cubic);
				AssertEquals("Volume UQ should be blank", "", enterprisePart.OP_CubicUQ);

				enterprisePart = LoadPart("P1234");
				AssertEquals("Part with Addinfo detail", "RUBBER GASKET", enterprisePart.OP_Desc);
				AssertEquals("Qty in stock", 6000M, enterprisePart.OP_QtyInStock);
				Assert("Commodity is empty", enterprisePart.OP_RH_NKCommodityCode.IsEmpty);
				Assert("BrandName is empty", enterprisePart.OP_Brand.IsEmpty);
				Assert("Model is empty", enterprisePart.OP_Model.IsEmpty);
			}
		}

		#endregion

		#region TestAttributesSetOnOrganisations

		public void TestAttributesSetOnOrganisations()
		{
			ClearCustomsRecordsBeforeTesting();
			var testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Use_Attribute1,Use_Attribute2,Use_Attribute3");
					sw.WriteLine("1,VULCANISED RUBBER,KG," + testOrganisation.OH_Code + ",Y,N,Y");
				}
				Loader.ImportProductData(testFileName.Filename, false, false);
			}

			AssertNull("Product should not be created since the attributes are not matching.", new OrgSupplierPart.Loader(Factory).Load("1", testOrganisation, null));
			AssertEquals("Precondition", 3, Loader.Log.Count);
			AssertEquals("Products to Import = 1", Loader.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: 1 / VULCANISED RUBBER  Part attributes doesn't match with client.", Loader.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", Loader.Log[2]);
		}

		#endregion

		#region TestRFCompletePalletPickingOnOrganisations

		public void TestRFCompletePalletPickingOnOrganisations()
		{
			var client = CreateClientUsingSerialNumber("C1");
			var product = CreateProductWithSerial(client, "P1");
			var relation = product.RelatedOrganisations[0];
			Factory.Save();
			var loader1 = OrgSupplierPartDataLoad.New();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,RFCompletePalletPicking",
																"P1, Product description, UNT, C1, Y", loader1);
			var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
			AssertEquals("CompletePalletPicking", true, relationInDB.OU_CompletePalletPicking);

			var loader2 = OrgSupplierPartDataLoad.New();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,RFCompletePalletPicking",
																"P1, Product description, UNT, C1, N", loader2);

			relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
			AssertEquals("CompletePalletPicking", false, relationInDB.OU_CompletePalletPicking);
			var loader3 = OrgSupplierPartDataLoad.New();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,Use_SerialNumber,IsSerialNumberReleaseCaptured,RFCompletePalletPicking",
																"P1, Product description, UNT, C1, Y, Y, Y", loader3);

			AssertEquals("Precondition", 3, loader3.Log.Count);
			AssertEquals("Products to Import = 1", loader3.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: P1 / Product description  RFCompletePalletPicking: Complete Pallet Picking is not supported for products with Release Captured Attributes.", loader3.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", loader3.Log[2]);
		}

		#endregion

		#region TestRollUpAttributesOnOrganisations

		public void TestRollUpAttributesOnOrganisations()
		{
			var client = Helper.CreateClient("C1");
			var product = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			var relation = product.RelatedOrganisations[0];
			Factory.Save();
			var loader1 = OrgSupplierPartDataLoad.New();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,PickMode,RollUpAttributes,RFConfirm",
																"P1, Product description, UNT, C1, ANE, Y, " + RFAttributeConfirmCode.Codes.None, loader1);
			var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
			AssertEquals("RollUpAttributesOnDocuments", true, relationInDB.OU_RollUpAttributesOnDocuments);
			var loader2 = OrgSupplierPartDataLoad.New();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,PickMode,RollUpAttributes,RFConfirm",
																"P1, Product description, UNT, C1, ASP, Y, " + RFAttributeConfirmCode.Codes.None, loader2);

			AssertEquals("Precondition", 3, loader2.Log.Count);
			AssertEquals("Products to Import = 1", loader2.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: P1 / Product description  RollUpAttributes: Roll Up Attributes should be false when Pick Mode is not ANE.", loader2.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", loader2.Log[2]);
		}

		#endregion

		#region TestRFPackingDateFormatOnOrganisations

		public void TestRFPackingDateFormatOnOrganisations()
		{
			var client = Helper.CreateClient("C1");
			var product = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			var relation = product.RelatedOrganisations[0];
			Factory.Save();
			var loader1 = OrgSupplierPartDataLoad.New();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,RFPackingDateFormat",
																"P1, Product description, UNT, C1, ddMMyyyy", loader1);
			var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
			AssertEquals("PackingDateFormatString", "ddMMyyyy", relationInDB.OU_PackingDateFormatString);

			var loader2 = OrgSupplierPartDataLoad.New();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,RFPackingDateFormat",
																"P1, Product description, UNT, C1, ddyyyy", loader2);

			AssertEquals("Precondition", 3, loader2.Log.Count);
			AssertEquals("Products to Import = 1", loader2.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: P1 / Product description  RFPackingDateFormat: RF Packing Date Format does not contain enough information (must have a minimum of month and year, e.g. MM/yyyy)", loader2.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", loader2.Log[2]);
		}

		#endregion

		#region TestRFExpiryDateFormatOnOrganisations

		public void TestRFExpiryDateFormatOnOrganisations()
		{
			var client = Helper.CreateClient("C1");
			var product = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			var relation = product.RelatedOrganisations[0];
			Factory.Save();
			var loader1 = OrgSupplierPartDataLoad.New();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,RFExpiryDateFormat",
																"P1, Product description, UNT, C1, ddMMyyyy", loader1);
			var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
			AssertEquals("ExpiryDateFormatString", "ddMMyyyy", relationInDB.OU_ExpiryDateFormatString);
			var loader2 = OrgSupplierPartDataLoad.New();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,RFExpiryDateFormat",
																"P1, Product description, UNT, C1, ddyyyy", loader2);

			AssertEquals("Precondition", 3, loader2.Log.Count);
			AssertEquals("Products to Import = 1", loader2.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: P1 / Product description  RFExpiryDateFormat: RF Expiry Date Format does not contain enough information (must have a minimum of month and year, e.g. MM/yyyy)", loader2.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", loader2.Log[2]);
		}

		#endregion

		#region TestConsigneeMinShelfLifeAcceptedDaysOnOrganisations

		public void TestConsigneeMinShelfLifeAcceptedDaysOnOrganisations()
		{
			var client = Helper.CreateClient("C1");
			var product = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			var relation = product.RelatedOrganisations[0];
			Factory.Save();
			var loader1 = OrgSupplierPartDataLoad.New();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,ConsigneeMinShelfLifeAcceptedDays",
																"P1, Product description, UNT, C1, 30", loader1);
			var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
			AssertEquals("ConsigneeMinShelfLifeAccepted", (ZShort)30, relationInDB.OU_ConsigneeMinShelfLifeAccepted);
			var loader2 = OrgSupplierPartDataLoad.New();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,ConsigneeMinShelfLifeAcceptedDays",
																"P1, Product description, UNT, C1, -30", loader2);

			AssertEquals("Precondition", 3, loader2.Log.Count);
			AssertEquals("Products to Import = 1", loader2.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: P1 / Product description  ConsigneeMinShelfLifeAcceptedDays: Consignee Minimum Shelf Life Accepted Days cannot be negative.", loader2.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", loader2.Log[2]);
		}

		#endregion

		#region TestJulianBatchNumberFormatOnOrganisations

		public void TestJulianBatchNumberFormatOnOrganisations()
		{
			var client = Helper.CreateClient("C1");
			var product = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			var relation = product.RelatedOrganisations[0];
			Factory.Save();
			var loader1 = OrgSupplierPartDataLoad.New();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,JulianBatchNumberFormat",
																"P1, Product description, UNT, C1, #JJJJ", loader1);
			var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
			AssertEquals("JulianBatchNoFormat", "#JJJJ", relationInDB.OU_JulianBatchNoFormat);
			var loader2 = OrgSupplierPartDataLoad.New();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,JulianBatchNumberFormat",
																"P1, Product description, UNT, C1, #JJ", loader2);

			AssertEquals("Precondition", 3, loader2.Log.Count);
			AssertEquals("Products to Import = 1", loader2.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: P1 / Product description  JulianBatchNumberFormat: Enter a valid Julian Batch No Format.", loader2.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", loader2.Log[2]);
		}

		#endregion

		#region TestRFConfirmOnOrganisations

		public void TestRFConfirmOnOrganisations()
		{
			var client = Helper.CreateClient("C1");
			var product = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			var relation = product.RelatedOrganisations[0];
			Factory.Save();
			var loader1 = OrgSupplierPartDataLoad.New();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,RFConfirm",
																"P1, Product description, UNT, C1, " + RFAttributeConfirmCode.Codes.None, loader1);
			var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
			AssertEquals("RFAttributeConfirm", RFAttributeConfirmCode.Codes.None, relationInDB.OU_RFAttributeConfirm);
			var loader2 = OrgSupplierPartDataLoad.New();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,RFConfirm",
																"P1, Product description, UNT, C1, " + RFAttributeConfirmCode.Codes.SerialNumber, loader2);

			AssertEquals("Precondition", 3, loader2.Log.Count);
			AssertEquals("Products to Import = 1", loader2.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: P1 / Product description  RFConfirm: This attribute has not been selected for use", loader2.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", loader2.Log[2]);
		}

		#endregion

		#region TestPickModeOnOrganisations

		public void TestPickModeOnOrganisations()
		{
			var client = Helper.CreateClient("C1");
			var product = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			var relation = product.RelatedOrganisations[0];
			Factory.Save();
			var loader1 = OrgSupplierPartDataLoad.New();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,RFConfirm,PickMode",
																"P1, Product description, UNT, C1, " + RFAttributeConfirmCode.Codes.None + ", " + WhsPickMode.Codes.AttributeSpecified, loader1);
			var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
			AssertEquals("PickMode", WhsPickMode.Codes.AttributeSpecified, relationInDB.OU_PickMode);

			var loader2 = OrgSupplierPartDataLoad.New();
			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,RFConfirm,PickMode",
																"P1, Product description, UNT, C1, " + RFAttributeConfirmCode.Codes.None + ", " + WhsPickMode.Codes.AttributeNeutral, loader2);
			relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
			AssertEquals("PickMode", WhsPickMode.Codes.AttributeNeutral, relationInDB.OU_PickMode);
		}

		#endregion

		#region TestConversionUnitsSetOnProduct

		public void TestConversionUnitsSetOnProduct()
		{
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC1_Cubic,UC1_Depth,UC1_Height,UC1_Width,UC1_Weight,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC3_Cubic,UC3_Depth,UC3_Height,UC3_Width,UC3_Weight");
					sw.WriteLine("1,VULCANISED RUBBER,KG," + testOrganisation.OH_Code + ",2,CTN,BOX,1.1,2.2,3.3,4.4,5.5,3.5,LA,L,.01,.02,.03,.04,.05");
				}
				Loader.ImportProductData(testFileName.Filename, false, false);
			}

			OrgSupplierPart product = new OrgSupplierPart.Loader(Factory).Load("1", testOrganisation, null);
			AssertEquals(2, product.PartUnits.Count);
			product.PartUnits.Sort(OrgPartUnitSchema.OF_PackType.Name);
			AssertEquals(2m, product.PartUnits[0].OF_QuantityInParent);
			AssertEquals("CTN", product.PartUnits[0].OF_PackType);
			AssertEquals("BOX", product.PartUnits[0].OF_ParentPackType);
			AssertEquals(1.1m, product.PartUnits[0].OF_Cubic);
			AssertEquals(2.2m, product.PartUnits[0].OF_Depth);
			AssertEquals(3.3m, product.PartUnits[0].OF_Height);
			AssertEquals(4.4m, product.PartUnits[0].OF_Width);
			AssertEquals(5.5m, product.PartUnits[0].OF_Weight);
			AssertEquals(3.5m, product.PartUnits[1].OF_QuantityInParent);
			AssertEquals("LA", product.PartUnits[1].OF_PackType);
			AssertEquals("L", product.PartUnits[1].OF_ParentPackType);
			AssertEquals(0.01m, product.PartUnits[1].OF_Cubic);
			AssertEquals(0.02m, product.PartUnits[1].OF_Depth);
			AssertEquals(0.03m, product.PartUnits[1].OF_Height);
			AssertEquals(0.04m, product.PartUnits[1].OF_Width);
			AssertEquals(0.05m, product.PartUnits[1].OF_Weight);
		}

		#endregion

		#region TestInconsistentDataNumericDataNotCorrectFormat

		public void TestInconsistentDataNumericDataNotCorrectFormat()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo,Last_Cost");
					sw.WriteLine("1,VULCANISED RUBBER,KG,,," + testOrganisation.OH_Code + ",,10,KG");
					sw.WriteLine("1A,SEALS,NO,,," + testOrganisation.OH_Code + ",,1234567.385,KG,0.002,M3"); // Should not import - UnitWeight not in specified format (6.3N)
					sw.WriteLine("912.226,TAPS,NO,,," + testOrganisation.OH_Code);
					sw.WriteLine("123,,,,," + testOrganisation.OH_Code + ",,,,,,,88885555"); // Should not import - QtyInStock not specified format (7.2N)
					sw.WriteLine("1B,WASHERS,KG,,," + testOrganisation.OH_Code + ",,0,,3334445.55,M3"); // Should not import - Volume not in specified format (6.3N)
					sw.WriteLine("123,RUBBER GASKETS,,,," + testOrganisation.OH_Code + ",,888555.54321"); // This 123 part should import, insignificant digits can be truncated & previous 123 Part was rejected
					sw.WriteLine("1AB,SEALS,NO,,," + testOrganisation.OH_Code + ",,123456.999,KG,123456.999,M3,,1234567.99");
					sw.WriteLine("LastCost,,,,," + testOrganisation.OH_Code + ",,,,,,,,,,,,,,12345678335.123456"); // Should not import - Last Cost not in specified format (10.4N)
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertCorrectNumberOfPartsNowExist(4);

				var testLog = Loader.Log;
				AssertEquals(9, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(4, Loader.RunCounters.RecsCreated);
				AssertEquals(0, Loader.RunCounters.RecsUpdated);
				AssertEquals(4, Loader.RunCounters.RecsExcluded);
				AssertEquals(10, Loader.Log.Count);

				OrgSupplierPart enterprisePart = LoadPart("1AB");
				AssertEquals("Weight", 123456.999m, enterprisePart.OP_Weight);
				AssertEquals("Volume", 123456.999m, enterprisePart.OP_Cubic);
				AssertEquals("Qty In Stock", 1234567.99m, enterprisePart.OP_QtyInStock);

				enterprisePart = LoadPart("123");
				AssertEquals("Weight", 888555.543m, enterprisePart.OP_Weight);
			}
		}

		#endregion

		#region TestProcessDataForThisLineUserMessages

		public void TestProcessDataForThisLineUserMessages()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
					sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Unit_Weight,Weight_Unit,Unit_Volume,Volume_Unit,Division,QtyInStock,Origin,Pref_Origin,Pref_Rule,Pref_Scheme,Pref_InstrumentType,Pref_InstrumentNo");
					sw.WriteLine("123,,,,," + testOrganisation.OH_Code + ",,,,,,,88885555"); // Should not import - QtyInStock not specified format (7.2N)
				}
				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertEquals(1, Loader.RunCounters.RecsExcluded);
				AssertEquals("Row 2 excluded... data is inconsistent with required format.", Loader.Log[1]);
			}
		}

		#endregion

		#region TestDisplayFormattedLogMessageUserMessages

		public void TestDisplayFormattedLogMessageUserMessages()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description");
					sw.WriteLine("123,Numbers");
				}
				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertEquals("Line 2: PART NO/DESC: 123 / Numbers  Owner or Supplier not found", Loader.Log[1]);
			}
		}

		#endregion

		#region TestDisplayFormattedLookupErrorUserMessages

		public void TestDisplayFormattedLookupErrorUserMessages()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,EXPORTCLASSIFICATION,IMPORTCLASSIFICATION");
					sw.WriteLine("123,Numbers,MissingExport,MissingImport");
					sw.WriteLine("123,Numbers,MissingExport");
					sw.WriteLine("123,Numbers,,MissingImport");
				}
				Loader.ImportProductData(testFileName.Filename, false, false);
				AssertEquals("Line 2: PART NO: 123 excluded, Tariff Lookup MissingImport or MissingExport was not found", Loader.Log[1]);
				AssertEquals("Line 3: PART NO: 123 excluded, Tariff Lookup MissingExport was not found", Loader.Log[2]);
				AssertEquals("Line 4: PART NO: 123 excluded, Tariff Lookup MissingImport was not found", Loader.Log[3]);
			}
		}

		#endregion

		#region TestOrgSupplierPartDataLoadNew

		public void TestOrgSupplierPartDataLoadNew()
		{
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.UnitedKingdom);
				AssertEquals("GBOrgSupplierPartDataLoad", OrgSupplierPartDataLoad.New().GetType().Name);
			}
			finally
			{
				SetupCountry();
			}
		}

		#endregion

		#region TestProductImportWithAttributes

		public void TestProductImportWithAttributes()
		{
			// setup product and client
			var clientUsesAttribute1 = CreateClient("C1", 1);
			var clientUsesAttribute2 = CreateClient("C2", 2);
			var clientUsesAttribute3 = CreateClient("C3", 3);
			var clientWithoutAttributes = CreateClient("C4", 0);

			var productWithAttribute1 = CreateProduct(clientUsesAttribute1, "P1", 1);
			var productWithAttribute2 = CreateProduct(clientUsesAttribute2, "P2", 2);
			var productWithAttribute3 = CreateProduct(clientUsesAttribute3, "P3", 3);
			var productWithoutAttributes = CreateProduct(clientWithoutAttributes, "P4", 0);

			AssertEquals("Precondition", 1, productWithAttribute1.RelatedOrganisations.Count);
			AssertEquals("Precondition", 1, productWithAttribute2.RelatedOrganisations.Count);
			AssertEquals("Precondition", 1, productWithAttribute3.RelatedOrganisations.Count);
			AssertEquals("Precondition", 1, productWithoutAttributes.RelatedOrganisations.Count);

			var relationInProductWithAttribute1 = productWithAttribute1.RelatedOrganisations[0];
			var relationInProductWithAttribute2 = productWithAttribute2.RelatedOrganisations[0];
			var relationInProductWithAttribute3 = productWithAttribute3.RelatedOrganisations[0];
			var relationInProductWithoutAttributes = productWithoutAttributes.RelatedOrganisations[0];
			AssertOrgPartRelationAttributes(relationInProductWithAttribute1, true, false, false, false, true, true);
			AssertOrgPartRelationAttributes(relationInProductWithAttribute2, false, true, false, true, false, true);
			AssertOrgPartRelationAttributes(relationInProductWithAttribute3, false, false, true, true, true, false);
			AssertOrgPartRelationAttributes(relationInProductWithoutAttributes, false, false, false, true, true, true);

			Factory.Save();

			// import the product from csv file
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3");
					// wrong combinations
					sw.WriteLine("P1, New DESC FOR PRODWITHATT1, UNT, C1, Y, Y, N");
					sw.WriteLine("P1, New DESC FOR PRODWITHATT1, UNT, C1, Y, N, Y");
					sw.WriteLine("P1, New DESC FOR PRODWITHATT1, UNT, C1, Y, Y, Y");
					sw.WriteLine("P2, New DESC FOR PRODWITHATT2, UNT, C2, Y, Y, N");
					sw.WriteLine("P2, New DESC FOR PRODWITHATT2, UNT, C2, Y, N, Y");
					sw.WriteLine("P2, New DESC FOR PRODWITHATT2, UNT, C2, Y, Y, Y");
					sw.WriteLine("P3, New DESC FOR PRODWITHATT3, UNT, C3, Y, N, Y");
					sw.WriteLine("P3, New DESC FOR PRODWITHATT3, UNT, C3, N, Y, Y");
					sw.WriteLine("P3, New DESC FOR PRODWITHATT3, UNT, C3, Y, Y, Y");
					sw.WriteLine("P4, New DESC FOR PRODWITHNOATTR4, UNT, C4, Y, N, N");
					sw.WriteLine("P4, New DESC FOR PRODWITHNOATTR4, UNT, C4, N, Y, N");
					sw.WriteLine("P4, New DESC FOR PRODWITHNOATTR4, UNT, C4, N, N, Y");
					sw.WriteLine("P4, New DESC FOR PRODWITHNOATTR4, UNT, C4, Y, Y, Y");
					// correct combinations
					sw.WriteLine("P1, New DESC FOR PRODWITHATT1, UNT, C1, Y, N, N");
					sw.WriteLine("P2, New DESC FOR PRODWITHATT2, UNT, C2, N, Y, N");
					sw.WriteLine("P3, New DESC FOR PRODWITHATT3, UNT, C3, N, N, Y");
					sw.WriteLine("P4, New DESC FOR PRODWITHNOATTR, UNT, C4, N, N, N");
					sw.Flush();
				}
				AssertEquals("Precondition", 0, Loader.Log.Count);
				Loader.ImportProductData(testFileName.Filename, true, false);
			}

			AssertEquals("Precondition", 19, Loader.Log.Count);
			AssertEquals("Products to Import = 17", Loader.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: P1 / New DESC FOR PRODWITHATT1  Part attributes doesn't match with client.", Loader.Log[1]);
			AssertEquals("Line 3: PART NO/DESC: P1 / New DESC FOR PRODWITHATT1  Part attributes doesn't match with client.", Loader.Log[2]);
			AssertEquals("Line 4: PART NO/DESC: P1 / New DESC FOR PRODWITHATT1  Part attributes doesn't match with client.", Loader.Log[3]);
			AssertEquals("Line 5: PART NO/DESC: P2 / New DESC FOR PRODWITHATT2  Part attributes doesn't match with client.", Loader.Log[4]);
			AssertEquals("Line 6: PART NO/DESC: P2 / New DESC FOR PRODWITHATT2  Part attributes doesn't match with client.", Loader.Log[5]);
			AssertEquals("Line 7: PART NO/DESC: P2 / New DESC FOR PRODWITHATT2  Part attributes doesn't match with client.", Loader.Log[6]);
			AssertEquals("Line 8: PART NO/DESC: P3 / New DESC FOR PRODWITHATT3  Part attributes doesn't match with client.", Loader.Log[7]);
			AssertEquals("Line 9: PART NO/DESC: P3 / New DESC FOR PRODWITHATT3  Part attributes doesn't match with client.", Loader.Log[8]);
			AssertEquals("Line 10: PART NO/DESC: P3 / New DESC FOR PRODWITHATT3  Part attributes doesn't match with client.", Loader.Log[9]);
			AssertEquals("Line 11: PART NO/DESC: P4 / New DESC FOR PRODWITHNOATTR4  Part attributes doesn't match with client.", Loader.Log[10]);
			AssertEquals("Line 12: PART NO/DESC: P4 / New DESC FOR PRODWITHNOATTR4  Part attributes doesn't match with client.", Loader.Log[11]);
			AssertEquals("Line 13: PART NO/DESC: P4 / New DESC FOR PRODWITHNOATTR4  Part attributes doesn't match with client.", Loader.Log[12]);
			AssertEquals("Line 14: PART NO/DESC: P4 / New DESC FOR PRODWITHNOATTR4  Part attributes doesn't match with client.", Loader.Log[13]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", Loader.Log[14]);
			AssertEquals("PART NO: P2 - Part has been UPDATED", Loader.Log[15]);
			AssertEquals("PART NO: P3 - Part has been UPDATED", Loader.Log[16]);
			AssertEquals("PART NO: P4 - Part has been UPDATED", Loader.Log[17]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 4, Products excluded = 13\r\n", Loader.Log[18]);
			var newLoader = OrgSupplierPartDataLoad.New();

			// disable product attributes via an import
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3");
					sw.WriteLine("P1, New DESC FOR PRODWITHATT1, UNT, C1, N, N, N");
					sw.WriteLine("P2, New DESC FOR PRODWITHATT2, UNT, C2, N, N, N");
					sw.WriteLine("P3, New DESC FOR PRODWITHATT3, UNT, C3, N, N, N");
					sw.Flush();
				}
				AssertEquals("Precondition", 0, newLoader.Log.Count);
				newLoader.ImportProductData(testFileName.Filename, true, false);
			}

			AssertEquals("Precondition", 5, newLoader.Log.Count);
			AssertEquals("Products to Import = 3", newLoader.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", newLoader.Log[1]);
			AssertEquals("PART NO: P2 - Part has been UPDATED", newLoader.Log[2]);
			AssertEquals("PART NO: P3 - Part has been UPDATED", newLoader.Log[3]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 3, Products excluded = 0\r\n", newLoader.Log[4]);

			var newFactory = new BusinessObjectFactory();
			var relationInProductWithAttribute1InAnotherFactory = newFactory.Load<OrgPartRelation>(productWithAttribute1.RelatedOrganisations[0].PK);
			var relationInProductWithAttribute2InAnotherFactory = newFactory.Load<OrgPartRelation>(productWithAttribute2.RelatedOrganisations[0].PK);
			var relationInProductWithAttribute3InAnotherFactory = newFactory.Load<OrgPartRelation>(productWithAttribute3.RelatedOrganisations[0].PK);
			var relationInProductWithoutAttributesInAnotherFactory = newFactory.Load<OrgPartRelation>(productWithoutAttributes.RelatedOrganisations[0].PK);
			AssertOrgPartRelationAttributes(relationInProductWithAttribute1InAnotherFactory, false, false, false, false, true, true);
			AssertOrgPartRelationAttributes(relationInProductWithAttribute2InAnotherFactory, false, false, false, true, false, true);
			AssertOrgPartRelationAttributes(relationInProductWithAttribute3InAnotherFactory, false, false, false, true, true, false);
			AssertOrgPartRelationAttributes(relationInProductWithoutAttributesInAnotherFactory, false, false, false, true, true, true);
		}

		#endregion

		#region TestImportProduct_WhenWithUnfinlisedASNLines_CanNotChangePartAttribute

		public void TestImportProduct_WhenWithUnfinlisedASNLines_CanNotChangePartAttributeFromAbleToDisable()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = CreateClientWithNonMandatoryAttributes("Org1");
			client.MiscServ.OM_IMUseSerialNumber = true;
			var part = (OrgSupplierPart)helper.CreateProduct(client.PK, "P1");
			var relation = part.RelatedOrganisations[0];
			relation.OU_UsePartAttrib1 = true;
			relation.OU_UsePartAttrib2 = true;
			relation.OU_UsePartAttrib3 = true;
			relation.OU_UseSerialNumber = true;
			relation.OU_UseExpiryDate = true;
			relation.OU_UsePackingDate = true;
			var whs = helper.CreateWarehouse("1", "A");
			Factory.Save();

			var receivePk = helper.CreateWhsReceive(client.PK, whs.PK, "R1", null);
			helper.CreateWhsReceiveInventoryLine(receivePk, part.PK, 0m, "A");
			helper.CreateAsnLine(receivePk, part.PK, 0m);
			Factory.Save();
			AssertEquals(false, relation.HasCurrentStockIncludingInTransit());
			AssertEquals(true, relation.HasAsnLineOnUnfinalisedReceive);

			AssertAnotherRelationShouldNotBeAffectedByRelationWithUnfinalisedASNLines_PartAttributesCanBeChangedFromAbleToDisable(part);

			var loader1 = OrgSupplierPartDataLoad.New();
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3,Use_SerialNumber,UseExpiryDate,UsePackingDate");
					sw.WriteLine(part.OP_PartNum + ", New DESC FOR PRODWITHATT, UNT, Org1, N, N, N, N, N, N");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, loader1.Log.Count);
				loader1.ImportProductData(testFileName.Filename, true, false);
			}
			AssertEquals(5, loader1.Log.Count);
			AssertEquals("Products to Import = 1", loader1.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: P1 / New DESC FOR PRODWITHATT  UseExpiryDate: Attribute settings cannot be changed because there are ASN line(s) on un-finalized receives for this product.", loader1.Log[1]);
			AssertEquals("Line 2: PART NO/DESC: P1 / New DESC FOR PRODWITHATT  UsePackingDate: Attribute settings cannot be changed because there are ASN line(s) on un-finalized receives for this product.", loader1.Log[2]);
			AssertEquals("Line 2: PART NO/DESC: P1 / New DESC FOR PRODWITHATT  Inventory or Un-finalized ASNs exist(s) for this product with attribute settings different from those in the CSV file.", loader1.Log[3]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", loader1.Log[4]);

			var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
			AssertEquals("OU_UsePartAttrib1 should NOT be changed because of unfinalised ASN lines", true, relationInDB.OU_UsePartAttrib1);
			AssertEquals("OU_UsePartAttrib2 should NOT be changed because of unfinalised ASN lines", true, relationInDB.OU_UsePartAttrib2);
			AssertEquals("OU_UsePartAttrib3 should NOT be changed because of unfinalised ASN lines", true, relationInDB.OU_UsePartAttrib3);
			AssertEquals("OU_UseSerialNumber should NOT be changed because of unfinalised ASN lines", true, relationInDB.OU_UseSerialNumber);
			AssertEquals("OU_UseExpiryDate should NOT be changed because of unfinalised ASN lines", true, relationInDB.OU_UseExpiryDate);
			AssertEquals("OU_UsePackingDate should NOT be changed because of unfinalised ASN lines", true, relationInDB.OU_UsePackingDate);

			var loader2 = OrgSupplierPartDataLoad.New();
			helper.FinaliseDocketWithoutUserConfirmation(receivePk);
			Factory.Save();
			AssertEquals(false, relation.HasAsnLineOnUnfinalisedReceive);
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3,Use_SerialNumber,UseExpiryDate,UsePackingDate");
					sw.WriteLine(part.OP_PartNum + ", New DESC FOR PRODWITHATT, UNT, Org1, N, N, N, N, N, N");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, loader2.Log.Count);
				loader2.ImportProductData(testFileName.Filename, true, false);
			}
			AssertEquals(3, loader2.Log.Count);
			AssertEquals("Products to Import = 1", loader2.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", loader2.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", loader2.Log[2]);
			relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
			AssertEquals("OU_UsePartAttrib1 should be changed because of no unfinalised ASN lines", false, relationInDB.OU_UsePartAttrib1);
			AssertEquals("OU_UsePartAttrib2 should be changed because of no unfinalised ASN lines", false, relationInDB.OU_UsePartAttrib2);
			AssertEquals("OU_UsePartAttrib3 should be changed because of no unfinalised ASN lines", false, relationInDB.OU_UsePartAttrib3);
			AssertEquals("OU_UseSerialNumber should be changed because of no unfinalised ASN lines", false, relationInDB.OU_UseSerialNumber);
			AssertEquals("OU_UseExpiryDate should be changed because of no unfinalised ASN lines", false, relationInDB.OU_UseExpiryDate);
			AssertEquals("OU_UsePackingDate should be changed because of no unfinalised ASN lines", false, relationInDB.OU_UsePackingDate);
		}

		void AssertAnotherRelationShouldNotBeAffectedByRelationWithUnfinalisedASNLines_PartAttributesCanBeChangedFromAbleToDisable(OrgSupplierPart part)
		{
			var newLoader = OrgSupplierPartDataLoad.New();
			var anotherRelation = part.RelatedOrganisations.AddNew();
			var anotherClient = CreateClientWithNonMandatoryAttributes("Org6");
			anotherClient.MiscServ.OM_IMUseSerialNumber = true;
			anotherRelation.OU_OH = anotherClient.PK;
			anotherRelation.OU_UsePartAttrib1 = true;
			anotherRelation.OU_UsePartAttrib2 = true;
			anotherRelation.OU_UsePartAttrib3 = true;
			anotherRelation.OU_UseSerialNumber = true;
			anotherRelation.OU_UseExpiryDate = true;
			anotherRelation.OU_UsePackingDate = true;
			Factory.Save();
			AssertEquals(false, anotherRelation.HasAsnLineOnUnfinalisedReceive);

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3,Use_SerialNumber,UseExpiryDate,UsePackingDate");
					sw.WriteLine(part.OP_PartNum + ", New DESC FOR PRODWITHATT, UNT, Org6, N, N, N, N, N, N");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, newLoader.Log.Count);
				newLoader.ImportProductData(testFileName.Filename, true, false);
			}
			AssertEquals(3, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", newLoader.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", newLoader.Log[2]);
			var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(anotherRelation.PK);
			AssertEquals(false, relationInDB.OU_UsePartAttrib1);
			AssertEquals(false, relationInDB.OU_UsePartAttrib2);
			AssertEquals(false, relationInDB.OU_UsePartAttrib3);
			AssertEquals(false, relationInDB.OU_UseSerialNumber);
			AssertEquals(false, relationInDB.OU_UseExpiryDate);
			AssertEquals(false, relationInDB.OU_UsePackingDate);
		}

		public void TestImportProduct_WhenWithUnfinlisedASNLines_CanNotChangePartAttributeFromDisableToAble()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = CreateClientWithNonMandatoryAttributes("Org1");
			client.MiscServ.OM_IMUseSerialNumber = true;
			var part = (OrgSupplierPart)helper.CreateProduct(client.PK, "P1");
			var relation = part.RelatedOrganisations[0];
			AssertEquals(false, relation.OU_UsePartAttrib1);
			AssertEquals(false, relation.OU_UsePartAttrib2);
			AssertEquals(false, relation.OU_UsePartAttrib3);
			AssertEquals(false, relation.OU_UseSerialNumber);
			AssertEquals(false, relation.OU_UseExpiryDate);
			AssertEquals(false, relation.OU_UsePackingDate);
			var whs = helper.CreateWarehouse("1", "A");
			Factory.Save();

			var receivePk = helper.CreateWhsReceive(client.PK, whs.PK, "R1", null);
			helper.CreateWhsReceiveInventoryLine(receivePk, part.PK, 0m, "A");
			helper.CreateAsnLine(receivePk, part.PK, 0m);
			Factory.Save();
			AssertEquals(false, relation.HasCurrentStockIncludingInTransit());
			AssertEquals(true, relation.HasAsnLineOnUnfinalisedReceive);
			AssertAnotherRelationShouldNotBeAffectedByRelationWithUnfinalisedASNLines_PartAttributesCanBeChangedFromDisableToAble(part);

			var loader1 = OrgSupplierPartDataLoad.New();
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3,Use_SerialNumber,UseExpiryDate,UsePackingDate");
					sw.WriteLine(part.OP_PartNum + ", New DESC FOR PRODWITHATT, UNT, Org1, Y, Y, Y, Y, Y, Y");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, loader1.Log.Count);
				loader1.ImportProductData(testFileName.Filename, true, false);
			}
			AssertEquals(5, loader1.Log.Count);
			AssertEquals("Products to Import = 1", loader1.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: P1 / New DESC FOR PRODWITHATT  UseExpiryDate: Attribute settings cannot be changed because there are ASN line(s) on un-finalized receives for this product.", loader1.Log[1]);
			AssertEquals("Line 2: PART NO/DESC: P1 / New DESC FOR PRODWITHATT  UsePackingDate: Attribute settings cannot be changed because there are ASN line(s) on un-finalized receives for this product.", loader1.Log[2]);
			AssertEquals("Line 2: PART NO/DESC: P1 / New DESC FOR PRODWITHATT  Inventory or Un-finalized ASNs exist(s) for this product with attribute settings different from those in the CSV file.", loader1.Log[3]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", loader1.Log[4]);

			var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
			AssertEquals("OU_UsePartAttrib1 should NOT be changed because of unfinalised ASN lines", false, relationInDB.OU_UsePartAttrib1);
			AssertEquals("OU_UsePartAttrib2 should NOT be changed because of unfinalised ASN lines", false, relationInDB.OU_UsePartAttrib2);
			AssertEquals("OU_UsePartAttrib3 should NOT be changed because of unfinalised ASN lines", false, relationInDB.OU_UsePartAttrib3);
			AssertEquals("OU_UseSerialNumber should NOT be changed because of unfinalised ASN lines", false, relationInDB.OU_UseSerialNumber);
			AssertEquals("OU_UseExpiryDate should NOT be changed because of unfinalised ASN lines", false, relationInDB.OU_UseExpiryDate);
			AssertEquals("OU_UsePackingDate should NOT be changed because of unfinalised ASN lines", false, relationInDB.OU_UsePackingDate);

			var loader2 = OrgSupplierPartDataLoad.New();
			helper.FinaliseDocketWithoutUserConfirmation(receivePk);
			Factory.Save();
			AssertEquals(false, relation.HasAsnLineOnUnfinalisedReceive);

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3,Use_SerialNumber,UseExpiryDate,UsePackingDate");
					sw.WriteLine(part.OP_PartNum + ", New DESC FOR PRODWITHATT, UNT, Org1, Y, Y, Y, Y, Y, Y");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, loader2.Log.Count);
				loader2.ImportProductData(testFileName.Filename, true, false);
			}
			AssertEquals(3, loader2.Log.Count);
			AssertEquals("Products to Import = 1", loader2.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", loader2.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", loader2.Log[2]);
			relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
			AssertEquals("OU_UsePartAttrib1 should be changed because of no unfinalised ASN lines", true, relationInDB.OU_UsePartAttrib1);
			AssertEquals("OU_UsePartAttrib2 should be changed because of no unfinalised ASN lines", true, relationInDB.OU_UsePartAttrib2);
			AssertEquals("OU_UsePartAttrib3 should be changed because of no unfinalised ASN lines", true, relationInDB.OU_UsePartAttrib3);
			AssertEquals("OU_UseSerialNumber should be changed because of no unfinalised ASN lines", true, relationInDB.OU_UseSerialNumber);
			AssertEquals("OU_UseExpiryDate should be changed because of no unfinalised ASN lines", true, relationInDB.OU_UseExpiryDate);
			AssertEquals("OU_UsePackingDate should be changed because of no unfinalised ASN lines", true, relationInDB.OU_UsePackingDate);
		}

		void AssertAnotherRelationShouldNotBeAffectedByRelationWithUnfinalisedASNLines_PartAttributesCanBeChangedFromDisableToAble(OrgSupplierPart part)
		{
			var newLoader = OrgSupplierPartDataLoad.New();
			var anotherRelation = part.RelatedOrganisations.AddNew();
			var anotherClient = CreateClientWithNonMandatoryAttributes("Org6");
			anotherClient.MiscServ.OM_IMUseSerialNumber = true;
			anotherRelation.OU_OH = anotherClient.PK;
			Factory.Save();
			AssertEquals(false, anotherRelation.HasAsnLineOnUnfinalisedReceive);
			AssertEquals(false, anotherRelation.OU_UsePartAttrib1);
			AssertEquals(false, anotherRelation.OU_UsePartAttrib2);
			AssertEquals(false, anotherRelation.OU_UsePartAttrib3);
			AssertEquals(false, anotherRelation.OU_UseSerialNumber);
			AssertEquals(false, anotherRelation.OU_UseExpiryDate);
			AssertEquals(false, anotherRelation.OU_UsePackingDate);
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3,Use_SerialNumber,UseExpiryDate,UsePackingDate");
					sw.WriteLine(part.OP_PartNum + ", New DESC FOR PRODWITHATT, UNT, Org6, Y, Y, Y, Y, Y, Y");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, newLoader.Log.Count);
				newLoader.ImportProductData(testFileName.Filename, true, false);
			}
			AssertEquals(3, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", newLoader.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", newLoader.Log[2]);
			var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(anotherRelation.PK);
			AssertEquals(true, relationInDB.OU_UsePartAttrib1);
			AssertEquals(true, relationInDB.OU_UsePartAttrib2);
			AssertEquals(true, relationInDB.OU_UsePartAttrib3);
			AssertEquals(true, relationInDB.OU_UseSerialNumber);
			AssertEquals(true, relationInDB.OU_UseExpiryDate);
			AssertEquals(true, relationInDB.OU_UsePackingDate);
		}

		#endregion

		#region TestImportProduct_WhenWithUnfinlisedASNLines_CanNotChangeIsPartAttribReleaseCaptured

		public void TestImportProduct_WhenWithUnfinlisedASNLines_CanNotChangeIsPartAttribReleaseCapturedFromAbleToDisable()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = CreateClientWithNonMandatoryAttributes("Org1");
			client.MiscServ.OM_IMUseSerialNumber = true;
			var part = (OrgSupplierPart)helper.CreateProduct(client.PK, "P1");
			var relation = part.RelatedOrganisations[0];
			relation.OU_UsePartAttrib1 = true;
			relation.OU_UsePartAttrib2 = true;
			relation.OU_UsePartAttrib3 = true;
			relation.OU_UseSerialNumber = true;
			relation.OU_IsPartAttrib1ReleaseCaptured = true;
			relation.OU_IsPartAttrib2ReleaseCaptured = true;
			relation.OU_IsPartAttrib3ReleaseCaptured = true;
			relation.OU_IsSerialNumberReleaseCaptured = true;
			var whs = helper.CreateWarehouse("1", "A");
			Factory.Save();

			var receivePk = helper.CreateWhsReceive(client.PK, whs.PK, "R1", null);
			helper.CreateWhsReceiveInventoryLine(receivePk, part.PK, 0m, "A");
			helper.CreateAsnLine(receivePk, part.PK, 0m);
			Factory.Save();
			AssertEquals(false, relation.HasCurrentStockIncludingInTransit());
			AssertEquals(true, relation.HasAsnLineOnUnfinalisedReceive);
			AssertAnotherRelationShouldNotBeAffectedByRelationWithUnfinalisedASNLines_IsPartAttribReleaseCapturedCanBeChangedFromDisableToAble(part);

			var loader1 = OrgSupplierPartDataLoad.New();
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3,Use_SerialNumber,IsPartAttrib1ReleaseCaptured,IsPartAttrib2ReleaseCaptured,IsPartAttrib3ReleaseCaptured,IsSerialNumberReleaseCaptured");
					sw.WriteLine(part.OP_PartNum + ", New DESC FOR PRODWITHATT, UNT, Org1, Y, Y, Y, Y, N, N, N, N");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, loader1.Log.Count);
				loader1.ImportProductData(testFileName.Filename, true, false);
			}
			AssertEquals(6, loader1.Log.Count);
			AssertEquals("Products to Import = 1", loader1.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: P1 / New DESC FOR PRODWITHATT  IsPartAttrib1ReleaseCaptured: Attribute settings cannot be changed because there are ASN line(s) on un-finalized receives for this product.", loader1.Log[1]);
			AssertEquals("Line 2: PART NO/DESC: P1 / New DESC FOR PRODWITHATT  IsPartAttrib2ReleaseCaptured: Attribute settings cannot be changed because there are ASN line(s) on un-finalized receives for this product.", loader1.Log[2]);
			AssertEquals("Line 2: PART NO/DESC: P1 / New DESC FOR PRODWITHATT  IsPartAttrib3ReleaseCaptured: Attribute settings cannot be changed because there are ASN line(s) on un-finalized receives for this product.", loader1.Log[3]);
			AssertEquals("Line 2: PART NO/DESC: P1 / New DESC FOR PRODWITHATT  IsSerialNumberReleaseCaptured: Attribute settings cannot be changed because there are ASN line(s) on un-finalized receives for this product.", loader1.Log[4]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", loader1.Log[5]);

			var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);

			AssertEquals(true, relationInDB.OU_UsePartAttrib1);
			AssertEquals(true, relationInDB.OU_UsePartAttrib2);
			AssertEquals(true, relationInDB.OU_UsePartAttrib3);
			AssertEquals(true, relationInDB.OU_UseSerialNumber);
			AssertEquals("OU_IsPartAttrib1ReleaseCaptured should NOT be changed because of unfinalised ASN lines", true, relationInDB.OU_IsPartAttrib1ReleaseCaptured);
			AssertEquals("OU_IsPartAttrib2ReleaseCaptured should NOT be changed because of unfinalised ASN lines", true, relationInDB.OU_IsPartAttrib2ReleaseCaptured);
			AssertEquals("OU_IsPartAttrib3ReleaseCaptured should NOT be changed because of unfinalised ASN lines", true, relationInDB.OU_IsPartAttrib3ReleaseCaptured);
			AssertEquals("OU_IsSerialNumberReleaseCaptured should NOT be changed because of unfinalised ASN lines", true, relationInDB.OU_IsSerialNumberReleaseCaptured);

			var loader2 = OrgSupplierPartDataLoad.New();
			helper.FinaliseDocketWithoutUserConfirmation(receivePk);
			Factory.Save();
			AssertEquals(false, relation.HasAsnLineOnUnfinalisedReceive);

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3,Use_SerialNumber,IsPartAttrib1ReleaseCaptured,IsPartAttrib2ReleaseCaptured,IsPartAttrib3ReleaseCaptured,IsSerialNumberReleaseCaptured");
					sw.WriteLine(part.OP_PartNum + ", New DESC FOR PRODWITHATT, UNT, Org1, Y, Y, Y, Y, N, N, N, N");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, loader2.Log.Count);
				loader2.ImportProductData(testFileName.Filename, true, false);
			}
			AssertEquals(3, loader2.Log.Count);
			AssertEquals("Products to Import = 1", loader2.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", loader2.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", loader2.Log[2]);
			relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
			AssertEquals("OU_IsPartAttrib1ReleaseCaptured should be changed because of no unfinalised ASN lines", false, relationInDB.OU_IsPartAttrib1ReleaseCaptured);
			AssertEquals("OU_IsPartAttrib2ReleaseCaptured should be changed because of no unfinalised ASN lines", false, relationInDB.OU_IsPartAttrib2ReleaseCaptured);
			AssertEquals("OU_IsPartAttrib3ReleaseCaptured should be changed because of no unfinalised ASN lines", false, relationInDB.OU_IsPartAttrib3ReleaseCaptured);
			AssertEquals("OU_IsSerialNumberReleaseCaptured should be changed because of no unfinalised ASN lines", false, relationInDB.OU_IsSerialNumberReleaseCaptured);
		}

		void AssertAnotherRelationShouldNotBeAffectedByRelationWithUnfinalisedASNLines_IsPartAttribReleaseCapturedCanBeChangedFromDisableToAble(OrgSupplierPart part)
		{
			var newloader = OrgSupplierPartDataLoad.New();
			var anotherRelation = part.RelatedOrganisations.AddNew();
			var anotherClient = CreateClientWithNonMandatoryAttributes("Org6");
			anotherClient.MiscServ.OM_IMUseSerialNumber = true;
			anotherRelation.OU_OH = anotherClient.PK;

			anotherRelation.OU_UsePartAttrib1 = true;
			anotherRelation.OU_UsePartAttrib2 = true;
			anotherRelation.OU_UsePartAttrib3 = true;
			anotherRelation.OU_UseSerialNumber = true;
			anotherRelation.OU_IsPartAttrib1ReleaseCaptured = true;
			anotherRelation.OU_IsPartAttrib2ReleaseCaptured = true;
			anotherRelation.OU_IsPartAttrib3ReleaseCaptured = true;
			anotherRelation.OU_IsSerialNumberReleaseCaptured = true;
			Factory.Save();
			AssertEquals(false, anotherRelation.HasAsnLineOnUnfinalisedReceive);

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3,Use_SerialNumber,IsPartAttrib1ReleaseCaptured,IsPartAttrib2ReleaseCaptured,IsPartAttrib3ReleaseCaptured,IsSerialNumberReleaseCaptured");
					sw.WriteLine(part.OP_PartNum + ", New DESC FOR PRODWITHATT, UNT, Org6, Y, Y, Y, Y, N, N, N, N");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, newloader.Log.Count);
				newloader.ImportProductData(testFileName.Filename, true, false);
			}
			AssertEquals(3, newloader.Log.Count);
			AssertEquals("Products to Import = 1", newloader.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", newloader.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", newloader.Log[2]);
			var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(anotherRelation.PK);
			AssertEquals(false, relationInDB.OU_IsPartAttrib1ReleaseCaptured);
			AssertEquals(false, relationInDB.OU_IsPartAttrib2ReleaseCaptured);
			AssertEquals(false, relationInDB.OU_IsPartAttrib3ReleaseCaptured);
			AssertEquals(false, relationInDB.OU_IsSerialNumberReleaseCaptured);
		}

		public void TestImportProduct_WhenWithUnfinlisedASNLines_CanNotChangeIsPartAttribReleaseCapturedFromDisableToAble()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = CreateClientWithNonMandatoryAttributes("Org1");
			client.MiscServ.OM_IMUseSerialNumber = true;
			var part = (OrgSupplierPart)helper.CreateProduct(client.PK, "P1");
			var relation = part.RelatedOrganisations[0];
			relation.OU_UsePartAttrib1 = true;
			relation.OU_UsePartAttrib2 = true;
			relation.OU_UsePartAttrib3 = true;
			relation.OU_UseSerialNumber = true;
			relation.OU_IsPartAttrib1ReleaseCaptured = false;
			relation.OU_IsPartAttrib2ReleaseCaptured = false;
			relation.OU_IsPartAttrib3ReleaseCaptured = false;
			relation.OU_IsSerialNumberReleaseCaptured = false;
			var whs = helper.CreateWarehouse("1", "A");
			Factory.Save();

			var receivePk = helper.CreateWhsReceive(client.PK, whs.PK, "R1", null);
			helper.CreateWhsReceiveInventoryLine(receivePk, part.PK, 0m, "A");
			helper.CreateAsnLine(receivePk, part.PK, 0m);
			Factory.Save();
			AssertEquals(false, relation.HasCurrentStockIncludingInTransit());
			AssertEquals(true, relation.HasAsnLineOnUnfinalisedReceive);
			AssertAnotherRelationShouldNotBeAffectedByRelationWithUnfinalisedASNLines_IsPartAttribReleaseCapturedCanBeChangedFromAbleToDisable(part);

			var loader1 = OrgSupplierPartDataLoad.New();
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3,Use_SerialNumber,IsPartAttrib1ReleaseCaptured,IsPartAttrib2ReleaseCaptured,IsPartAttrib3ReleaseCaptured,IsSerialNumberReleaseCaptured");
					sw.WriteLine(part.OP_PartNum + ", New DESC FOR PRODWITHATT, UNT, Org1, Y, Y, Y, Y, Y, Y, Y, Y");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, loader1.Log.Count);
				loader1.ImportProductData(testFileName.Filename, true, false);
			}
			AssertEquals(6, loader1.Log.Count);
			AssertEquals("Products to Import = 1", loader1.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: P1 / New DESC FOR PRODWITHATT  IsPartAttrib1ReleaseCaptured: Attribute settings cannot be changed because there are ASN line(s) on un-finalized receives for this product.", loader1.Log[1]);
			AssertEquals("Line 2: PART NO/DESC: P1 / New DESC FOR PRODWITHATT  IsPartAttrib2ReleaseCaptured: Attribute settings cannot be changed because there are ASN line(s) on un-finalized receives for this product.", loader1.Log[2]);
			AssertEquals("Line 2: PART NO/DESC: P1 / New DESC FOR PRODWITHATT  IsPartAttrib3ReleaseCaptured: Attribute settings cannot be changed because there are ASN line(s) on un-finalized receives for this product.", loader1.Log[3]);
			AssertEquals("Line 2: PART NO/DESC: P1 / New DESC FOR PRODWITHATT  IsSerialNumberReleaseCaptured: Attribute settings cannot be changed because there are ASN line(s) on un-finalized receives for this product.", loader1.Log[4]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", loader1.Log[5]);

			var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);

			AssertEquals(true, relationInDB.OU_UsePartAttrib1);
			AssertEquals(true, relationInDB.OU_UsePartAttrib2);
			AssertEquals(true, relationInDB.OU_UsePartAttrib3);
			AssertEquals(true, relationInDB.OU_UseSerialNumber);
			AssertEquals("OU_IsPartAttrib1ReleaseCaptured should NOT be changed because of unfinalised ASN lines", false, relationInDB.OU_IsPartAttrib1ReleaseCaptured);
			AssertEquals("OU_IsPartAttrib2ReleaseCaptured should NOT be changed because of unfinalised ASN lines", false, relationInDB.OU_IsPartAttrib2ReleaseCaptured);
			AssertEquals("OU_IsPartAttrib3ReleaseCaptured should NOT be changed because of unfinalised ASN lines", false, relationInDB.OU_IsPartAttrib3ReleaseCaptured);
			AssertEquals("OU_IsSerialNumberReleaseCaptured should NOT be changed because of unfinalised ASN lines", false, relationInDB.OU_IsSerialNumberReleaseCaptured);

			var loader2 = OrgSupplierPartDataLoad.New();
			helper.FinaliseDocketWithoutUserConfirmation(receivePk);
			Factory.Save();
			AssertEquals(false, relation.HasAsnLineOnUnfinalisedReceive);
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3,Use_SerialNumber,IsPartAttrib1ReleaseCaptured,IsPartAttrib2ReleaseCaptured,IsPartAttrib3ReleaseCaptured,IsSerialNumberReleaseCaptured");
					sw.WriteLine(part.OP_PartNum + ", New DESC FOR PRODWITHATT, UNT, Org1, Y, Y, Y, Y, Y, Y, Y, Y");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, loader2.Log.Count);
				loader2.ImportProductData(testFileName.Filename, true, false);
			}
			AssertEquals(3, loader2.Log.Count);
			AssertEquals("Products to Import = 1", loader2.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", loader2.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", loader2.Log[2]);
			relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(relation.PK);
			AssertEquals("OU_IsPartAttrib1ReleaseCaptured should be changed because of no unfinalised ASN lines", true, relationInDB.OU_IsPartAttrib1ReleaseCaptured);
			AssertEquals("OU_IsPartAttrib2ReleaseCaptured should be changed because of no unfinalised ASN lines", true, relationInDB.OU_IsPartAttrib2ReleaseCaptured);
			AssertEquals("OU_IsPartAttrib3ReleaseCaptured should be changed because of no unfinalised ASN lines", true, relationInDB.OU_IsPartAttrib3ReleaseCaptured);
			AssertEquals("OU_IsSerialNumberReleaseCaptured should be changed because of no unfinalised ASN lines", true, relationInDB.OU_IsSerialNumberReleaseCaptured);
		}

		void AssertAnotherRelationShouldNotBeAffectedByRelationWithUnfinalisedASNLines_IsPartAttribReleaseCapturedCanBeChangedFromAbleToDisable(OrgSupplierPart part)
		{
			var newLoader = OrgSupplierPartDataLoad.New();
			var anotherRelation = part.RelatedOrganisations.AddNew();
			var anotherClient = CreateClientWithNonMandatoryAttributes("Org6");
			anotherClient.MiscServ.OM_IMUseSerialNumber = true;
			anotherRelation.OU_OH = anotherClient.PK;

			anotherRelation.OU_UsePartAttrib1 = true;
			anotherRelation.OU_UsePartAttrib2 = true;
			anotherRelation.OU_UsePartAttrib3 = true;
			anotherRelation.OU_UseSerialNumber = true;
			anotherRelation.OU_IsPartAttrib1ReleaseCaptured = false;
			anotherRelation.OU_IsPartAttrib2ReleaseCaptured = false;
			anotherRelation.OU_IsPartAttrib3ReleaseCaptured = false;
			anotherRelation.OU_IsSerialNumberReleaseCaptured = false;
			Factory.Save();
			AssertEquals(false, anotherRelation.HasAsnLineOnUnfinalisedReceive);

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3,Use_SerialNumber,IsPartAttrib1ReleaseCaptured,IsPartAttrib2ReleaseCaptured,IsPartAttrib3ReleaseCaptured,IsSerialNumberReleaseCaptured");
					sw.WriteLine(part.OP_PartNum + ", New DESC FOR PRODWITHATT, UNT, Org6, Y, Y, Y, Y, Y, Y, Y, Y");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, newLoader.Log.Count);
				newLoader.ImportProductData(testFileName.Filename, true, false);
			}
			AssertEquals(3, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", newLoader.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", newLoader.Log[2]);
			var relationInDB = new BusinessObjectFactory().Load<OrgPartRelation>(anotherRelation.PK);
			AssertEquals(true, relationInDB.OU_IsPartAttrib1ReleaseCaptured);
			AssertEquals(true, relationInDB.OU_IsPartAttrib2ReleaseCaptured);
			AssertEquals(true, relationInDB.OU_IsPartAttrib3ReleaseCaptured);
			AssertEquals(true, relationInDB.OU_IsSerialNumberReleaseCaptured);
		}

		#endregion

		#region TestImportProduct_UseAttributeCanEnableOrDisable

		#region ExistingStock

		public void TestImportProduct_CantDisableAttributes_ExistingStock_Mandatory()
		{
			var useAttribute = true;
			var product = CreateProduct("C1", "P1", allAttributesMandatory: true, attributeInUse: useAttribute, createStock: true);
			ImportProduct("C1", "P1", useAttribute);

			AssertImportLog(productCode: "P1", isImported: false);
			AssertOrgPartRelation(product: product, isAttributeInUse: useAttribute);
		}

		public void TestImportProduct_CantEnableAttributes_ExistingStock_Mandatory()
		{
			var useAttribute = false;
			var product = CreateProduct("C1", "P1", allAttributesMandatory: true, attributeInUse: useAttribute, createStock: true);
			ImportProduct("C1", "P1", useAttribute);

			AssertImportLog(productCode: "P1", isImported: false);
			AssertOrgPartRelation(product: product, isAttributeInUse: useAttribute);
		}

		public void TestImportProduct_CantDisableAttributes_ExistingStock_NonMandatory()
		{
			var useAttribute = true;
			var product = CreateProduct("C1", "P1", allAttributesMandatory: false, attributeInUse: useAttribute, createStock: true);
			ImportProduct("C1", "P1", useAttribute);

			AssertImportLog(productCode: "P1", isImported: false);
			AssertOrgPartRelation(product: product, isAttributeInUse: useAttribute);
		}

		public void TestImportProduct_CanEnableAttributes_ExistingStock_NonMandatory()
		{
			var useAttribute = false;
			var product = CreateProduct("C1", "P1", allAttributesMandatory: false, attributeInUse: useAttribute, createStock: true);
			ImportProduct("C1", "P1", useAttribute);

			AssertImportLog(productCode: "P1", isImported: true);
			AssertOrgPartRelation(product: product, isAttributeInUse: true);
		}

		#endregion

		#region NoExistingStock

		public void TestImportProduct_CanDisableAttributes_NoExistingStock_Mandatory()
		{
			var useAttribute = true;
			var product = CreateProduct("C1", "P1", allAttributesMandatory: true, attributeInUse: useAttribute, createStock: false);
			ImportProduct("C1", "P1", useAttribute);

			AssertImportLog(productCode: "P1", isImported: true);
			AssertOrgPartRelation(product: product, isAttributeInUse: false);
		}

		public void TestImportProduct_CanEnableAttributes_NoExistingStock_Mandatory()
		{
			var useAttribute = false;
			var product = CreateProduct("C1", "P1", allAttributesMandatory: true, attributeInUse: useAttribute, createStock: false);
			ImportProduct("C1", "P1", useAttribute);

			AssertImportLog(productCode: "P1", isImported: true);
			AssertOrgPartRelation(product: product, isAttributeInUse: true);
		}

		public void TestImportProduct_CanDisableAttributes_NoExistingStock_NonMandatory()
		{
			var useAttribute = true;
			var product = CreateProduct("C1", "P1", allAttributesMandatory: false, attributeInUse: useAttribute, createStock: false);
			ImportProduct("C1", "P1", useAttribute);

			AssertImportLog(productCode: "P1", isImported: true);
			AssertOrgPartRelation(product: product, isAttributeInUse: false);
		}

		public void TestImportProduct_CanEnableAttributes_NoExistingStock_NonMandatory()
		{
			var useAttribute = false;
			var product = CreateProduct("C1", "P1", allAttributesMandatory: false, attributeInUse: useAttribute, createStock: false);
			ImportProduct("C1", "P1", useAttribute);

			AssertImportLog(productCode: "P1", isImported: true);
			AssertOrgPartRelation(product: product, isAttributeInUse: true);
		}

		#endregion

		#region ExistingStock_BothMandatoryAndNonMandatory

		public void TestImportProduct_CantDisableAttributes_ExistingStock_BothMandatoryAndNonMandatory()
		{
			var useAttribute = true;
			var product = CreateProduct_WithMixedMandatoryAttributes("C1", "P1", attributeInUse: useAttribute, createStock: true);
			ImportProduct("C1", "P1", useAttribute);

			AssertImportLog(productCode: "P1", isImported: false);
			AssertOrgPartRelation(product: product, isAttributeInUse: true);
		}

		public void TestImportProduct_CantEnableAttributes_ExistingStock_BothMandatoryAndNonMandatory()
		{
			var useAttribute = false;
			var product = CreateProduct_WithMixedMandatoryAttributes("C1", "P1", attributeInUse: useAttribute, createStock: true);
			ImportProduct("C1", "P1", useAttribute);

			AssertImportLog(productCode: "P1", isImported: false);
			AssertOrgPartRelation(product: product, isAttributeInUse: false);
		}

		#endregion

		#region NoExistingStock_BothMandatoryAndNonMandatory

		public void TestImportProduct_CanDisableAttributes_NoExistingStock_BothMandatoryAndNonMandatory()
		{
			var useAttribute = true;
			var product = CreateProduct_WithMixedMandatoryAttributes("C1", "P1", attributeInUse: useAttribute, createStock: false);
			ImportProduct("C1", "P1", useAttribute);

			AssertImportLog(productCode: "P1", isImported: true);
			AssertOrgPartRelation(product: product, isAttributeInUse: false);
		}

		public void TestImportProduct_CanEnableAttributes_NoExistingStock_BothMandatoryAndNonMandatory()
		{
			var useAttribute = false;
			var product = CreateProduct_WithMixedMandatoryAttributes("C1", "P1", attributeInUse: useAttribute, createStock: false);
			ImportProduct("C1", "P1", useAttribute);

			AssertImportLog(productCode: "P1", isImported: true);
			AssertOrgPartRelation(product: product, isAttributeInUse: true);
		}

		#endregion

		OrgSupplierPart CreateProduct_WithMixedMandatoryAttributes(string clientCode, string productCode, bool attributeInUse, bool createStock)
		{
			return CreateProduct(clientCode, productCode, PartAttributeTypeList.Codes.Mandatory, PartAttributeTypeList.Codes.NonMandatory, PartAttributeTypeList.Codes.Mandatory, attributeInUse, createStock);
		}

		OrgSupplierPart CreateProduct(string clientCode, string productCode, bool allAttributesMandatory, bool attributeInUse, bool createStock)
		{
			var attributeType = allAttributesMandatory ? PartAttributeTypeList.Codes.Mandatory : PartAttributeTypeList.Codes.NonMandatory;
			return CreateProduct(clientCode, productCode, attributeType, attributeType, attributeType, attributeInUse, createStock);
		}

		OrgSupplierPart CreateProduct(string clientCode, string productCode, string attribute1Type, string attribute2Type, string attribute3Type, bool attributeInUse, bool createStock)
		{
			var warehouse = Helper.CreateWarehouse("1", "A");
			var client = CreateClientWithAttributes("C1", attribute1Type, attribute2Type, attribute3Type);
			var product = CreateProductWithAttributes(client, "P1", attributeInUse);
			Factory.Save();

			if (createStock)
			{
				CreateStock(warehouse.PK, client, product, attributeInUse);
			}

			return product;
		}

		OrgHeader CreateClientWithNonMandatoryAttributes(string clientCode)
		{
			var client = Factory.Load<OrgHeader>(Helper.CreateClient(clientCode));
			SetClientAttributeAndName(client, 1, "", PartAttributeTypeList.Codes.NonMandatory);
			SetClientAttributeAndName(client, 2, "", PartAttributeTypeList.Codes.NonMandatory);
			SetClientAttributeAndName(client, 3, "", PartAttributeTypeList.Codes.NonMandatory);

			return client;
		}

		OrgHeader CreateClientWithAttributes(string clientCode, string attribute1Type, string attribute2Type, string attribute3Type)
		{
			var client = Factory.Load<OrgHeader>(Helper.CreateClient(clientCode));
			SetClientAttributeAndName(client, 1, "", attribute1Type);
			SetClientAttributeAndName(client, 2, "", attribute2Type);
			SetClientAttributeAndName(client, 3, "", attribute3Type);

			return client;
		}

		OrgSupplierPart CreateProductWithAttributes(OrgHeader client, string productCode, bool usePartAttrib)
		{
			var product = (OrgSupplierPart)Helper.CreateProduct(client.PK, productCode);
			var relation = product.RelatedOrganisations.FindByOrganisationPKAndRelationship(client.PK, "OWN");
			relation.OU_UsePartAttrib1 = usePartAttrib;
			relation.OU_UsePartAttrib2 = usePartAttrib;
			relation.OU_UsePartAttrib3 = usePartAttrib;

			return product;
		}

		void CreateStock(ZGuid warehousePK, OrgHeader clientUsesAttributes, OrgSupplierPart productWithAttributes, bool useAttribute)
		{
			if (useAttribute)
			{
				Helper.CreateStock(warehousePK, clientUsesAttributes.PK, "R1", productWithAttributes.PK, 10m, "A1", "A2", "A3");
			}
			else
			{
				Helper.CreateStock(warehousePK, clientUsesAttributes.PK, "R1", productWithAttributes.PK, 10m);
			}
			Factory.Save();
		}

		void ImportProduct(string clientCode, string productCode, bool useAttribute)
		{
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3");
					if (useAttribute)
					{
						sw.WriteLine(productCode + ", New DESC FOR PRODWITHATT, UNT, " + clientCode + ", N, N, N");
					}
					else
					{
						sw.WriteLine(productCode + ", New DESC FOR PRODWITHATT, UNT, " + clientCode + ", Y, Y, Y");
					}
					sw.Flush();
				}

				AssertEquals("Precondition", 0, Loader.Log.Count);
				Loader.ImportProductData(testFileName.Filename, true, false);
			}

			AssertEquals("Precondition", 3, Loader.Log.Count);
			AssertEquals("Products to Import = 1", Loader.Log[0]);
		}

		void AssertImportLog(string productCode, bool isImported)
		{
			if (isImported)
			{
				AssertEquals("PART NO: " + productCode + " - Part has been UPDATED", Loader.Log[1]);
				AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", Loader.Log[2]);
			}
			else
			{
				AssertEquals("Line 2: PART NO/DESC: " + productCode + " / New DESC FOR PRODWITHATT  Inventory or Un-finalized ASNs exist(s) for this product with attribute settings different from those in the CSV file.", Loader.Log[1]);
				AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", Loader.Log[2]);
			}
			Loader = OrgSupplierPartDataLoad.New();
		}

		void AssertOrgPartRelation(OrgSupplierPart product, bool isAttributeInUse)
		{
			var isAttributeReadonly = false;
			var newFactory = new BusinessObjectFactory();
			var relationInProductWithAttribute1InAnotherFactory = newFactory.Load<OrgPartRelation>(product.RelatedOrganisations[0].PK);
			AssertOrgPartRelationAttributes(relationInProductWithAttribute1InAnotherFactory, isAttributeInUse, isAttributeInUse, isAttributeInUse, isAttributeReadonly, isAttributeReadonly, isAttributeReadonly);
		}

		#endregion

		#region TestProductImportWithAttributesForProductWithMultipleClients

		public void TestProductImportWithAttributesForProductWithMultipleClients()
		{
			// setup product and clients
			var clientUsesAttribute1 = CreateClient("C1", 1, "A1");
			var clientUsesAttribute1And2 = CreateClient("C2", 1, "A1");
			SetClientAttributeAndName(clientUsesAttribute1And2, 2, "A2");

			var product = (OrgSupplierPart)Helper.CreateProduct(clientUsesAttribute1.PK, "P1");

			// make the product client relationship with the client who has attribute1 and attribute 2
			var orgPartRelationForClientUsesAttribute1And2 = product.RelatedOrganisations.AddNew();
			orgPartRelationForClientUsesAttribute1And2.OU_OH = clientUsesAttribute1And2.PK;
			orgPartRelationForClientUsesAttribute1And2.OU_OP = product.PK;
			orgPartRelationForClientUsesAttribute1And2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var relationToClientUsesAttribute1 = product.RelatedOrganisations.FindByOrganisationPKAndRelationship(clientUsesAttribute1.PK, "OWN");
			relationToClientUsesAttribute1.OU_UsePartAttrib1 = true;
			AssertOrgPartRelationAttributes(relationToClientUsesAttribute1, true, false, false, false, true, true);

			var relationToClientUsesAttribute1And2 = product.RelatedOrganisations.FindByOrganisationPKAndRelationship(clientUsesAttribute1And2.PK, "OWN");
			relationToClientUsesAttribute1And2.OU_UsePartAttrib1 = true;
			AssertOrgPartRelationAttributes(relationToClientUsesAttribute1And2, true, false, false, false, false, true);

			Factory.Save();

			// import the product from csv file
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2");
					sw.WriteLine("P1, New DESC FOR Attributes not matching with client, UNT, C1, Y, Y");
					sw.WriteLine("P1, New DESC FOR Attributes match with client, UNT, C2, Y, Y");
					sw.Flush();
				}
				AssertEquals("Precondition", 0, Loader.Log.Count);
				Loader.ImportProductData(testFileName.Filename, true, false);
			}

			var relationToClientUsesAttribute1InAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(relationToClientUsesAttribute1.PK);
			var relationToClientUsesAttribute1And2InAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(relationToClientUsesAttribute1And2.PK);
			AssertEquals("Precondition", 4, Loader.Log.Count);
			AssertEquals("Products to Import = 2", Loader.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: P1 / New DESC FOR Attributes not matching with client  Part attributes doesn't match with client.", Loader.Log[1]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", Loader.Log[2]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 1\r\n", Loader.Log[3]);
			AssertOrgPartRelationAttributes(relationToClientUsesAttribute1InAnotherFactory, true, false, false, false, true, true);
			AssertOrgPartRelationAttributes(relationToClientUsesAttribute1And2InAnotherFactory, true, true, false, false, false, true);
		}

		#endregion

		#region TestProductImportRelationship_Hi_Ti

		public void TestProductImportRelationship_Hi_Ti()
		{
			// setup product and clients
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			Helper.CreateClient("C3");

			var product = (OrgSupplierPart)Helper.CreateProduct(client1, "P1");

			var relation1 = product.RelatedOrganisations.FindByOrganisationPKAndRelationship(client1, OrgPartRelation.RelationshipTypes.Owner);
			relation1.OU_Ti = 10;
			relation1.OU_Hi = 20;
			var relation2 = product.RelatedOrganisations.AddNew();
			relation2.OU_OH = client2;
			relation2.OU_OP = product.PK;
			relation2.OU_Ti = 30;
			relation2.OU_Hi = 40;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			Factory.Save();

			// import the product from csv file
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Ti,Hi");
					sw.WriteLine("P1, Product description, UNT, C2, 33, 44");
					sw.WriteLine("P1, Product description, UNT, C3, 55, 66");
					sw.Flush();
				}
				AssertEquals("Precondition", 0, Loader.Log.Count);
				Loader.ImportProductData(testFileName.Filename, true, false);
			}

			var relationInAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product.PK));
			var relation1nf = relationInAnotherFactory.Single(r => r.PK.Equals(relation1.PK));
			var relation2nf = relationInAnotherFactory.Single(r => r.PK.Equals(relation2.PK));
			var relation3nf = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, SQLComparisonOperator.NotEqual, product.PK)).Single();
			AssertEquals("Precondition", 3, Loader.Log.Count);
			AssertEquals("Products to Import = 2", Loader.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", Loader.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 1, Products updated = 1, Products excluded = 0\r\n", Loader.Log[2]);
			AssertEquals("Should not update.", (ZShort)10, relation1nf.OU_Ti);
			AssertEquals("Should not update.", (ZShort)20, relation1nf.OU_Hi);
			AssertEquals("Should update.", (ZShort)33, relation2nf.OU_Ti);
			AssertEquals("Should update.", (ZShort)44, relation2nf.OU_Hi);
			AssertEquals("Should add.", (ZShort)55, relation3nf.OU_Ti);
			AssertEquals("Should add.", (ZShort)66, relation3nf.OU_Hi);
		}

		#endregion

		#region TestImportProductTi_TooBig

		public void TestImportProductTi_TooBig()
		{
			Helper.CreateWarehouse("1", "A");
			var client = CreateClientWithNonMandatoryAttributes("C1");
			var product = CreateProductWithAttributes(client, "P1", true);
			Factory.Save();

			var newLoader = OrgSupplierPartDataLoad.New();
			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3,Ti",
																		$"P1, Product description, UNT, C1, Y, Y, Y, 200000", newLoader);
			
			AssertEquals("Precondition", 4, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("Row 2 excluded... data is inconsistent with required format.", newLoader.Log[1]);
#if NETFRAMEWORK
			AssertEquals("Specified argument was out of the range of valid values.\r\nParameter name: Value of 200000 is invalid or too large to store in Ti", newLoader.Log[2]);
#else
			AssertEquals("Specified argument was out of the range of valid values. (Parameter 'Value of 200000 is invalid or too large to store in Ti')", newLoader.Log[2]);
#endif
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", newLoader.Log[3]);
			AssertEquals("OU_Ti value is not set.", ZShort.Zero, new BusinessObjectFactory().Load<OrgPartRelation>(product.RelatedOrganisations[0].PK).OU_Ti);
		}

		#endregion

		#region TestImportProductHi_TooBig

		public void TestImportProductHi_TooBig()
		{
			Helper.CreateWarehouse("1", "A");
			var client = CreateClientWithNonMandatoryAttributes("C1");
			var product = CreateProductWithAttributes(client, "P1", true);
			Factory.Save();
			var newLoader = OrgSupplierPartDataLoad.New();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3,Hi",
																		$"P1, Product description, UNT, C1, Y, Y, Y, 200000", newLoader);
			
			AssertEquals("Precondition", 4, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("Row 2 excluded... data is inconsistent with required format.", newLoader.Log[1]);
#if NETFRAMEWORK
			AssertEquals("Specified argument was out of the range of valid values.\r\nParameter name: Value of 200000 is invalid or too large to store in Hi", newLoader.Log[2]);
#else
			AssertEquals("Specified argument was out of the range of valid values. (Parameter 'Value of 200000 is invalid or too large to store in Hi')", newLoader.Log[2]);
#endif
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", newLoader.Log[3]);

			AssertEquals("OU_Hi value is not set.", ZShort.Zero, new BusinessObjectFactory().Load<OrgPartRelation>(product.RelatedOrganisations[0].PK).OU_Hi);
		}

		#endregion

		#region TestImportProduct_DoesNotClobberOU_UnitsPerClientUQ

		public void TestImportProduct_DoesNotClobberOU_UnitsPerClientUQ()
		{
			var whsPK = Helper.CreateWarehouse("1", "A").PK;
			var client = Helper.CreateClient("C1");
			var product = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			var partRelation = product.RelatedOrganisations[0];
			partRelation.OU_ClientUQ = "CTN";
			partRelation.OU_UsePartAttrib1 = true;
			Factory.Save();

			partRelation.Reload();
			AssertEquals("Precondition.", 0.083333333m, partRelation.OU_UnitsPerClientUQ);

			var loader1 = GetNewDataLoader();
			var loader2 = GetNewDataLoader();
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,OWNER,ClientUQ");
					sw.WriteLine("P1, C1, CTN");

					sw.Flush();
				}

				loader1.ImportProductData(testFileName.Filename, true, false); // Run once to simulate two imports

				AssertEquals("Precondition", 0, loader2.Log.Count);
				loader2.ImportProductData(testFileName.Filename, true, false);
			}

			AssertEquals("Precondition", 3, loader2.Log.Count);
			AssertEquals("Products to Import = 1", loader2.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", loader2.Log[1]);
			AssertEquals($"\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", loader2.Log[2]);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var relationInDB = newFactory.Load<OrgPartRelation>(partRelation.PK);
			AssertEquals("OU_UnitsPerClientUQ should not be changed.", 0.083333333m, relationInDB.OU_UnitsPerClientUQ);
		}

		#endregion

		#region TestImportProductWithExistingInventory

		public void TestImportProductWithExistingInventory()
		{
			TestImportProductWithExistingInventoryCore(inTransit: false);
		}

		public void TestImportProductWithExistingInventory_WithInTransitQty()
		{
			TestImportProductWithExistingInventoryCore(inTransit: true);
		}

		void TestImportProductWithExistingInventoryCore(bool inTransit)
		{
			// setup product and client
			var whsPK = Helper.CreateWarehouse("1", "A").PK;
			var clientUsesAttribute1 = CreateClient("C1", 1, "A1");
			var clientUsesAttribute2 = CreateClient("C2", 2, "A2");
			var clientUsesAttribute3 = CreateClient("C3", 3, "A3");
			var clientUsesSerial = CreateClientUsingSerialNumber("C4");
			var productWithClientUsesAttribute1 = (OrgSupplierPart)Helper.CreateProduct(clientUsesAttribute1.PK, "P1");
			var productWithClientUsesAttribute2 = (OrgSupplierPart)Helper.CreateProduct(clientUsesAttribute2.PK, "P2");
			var productWithClientUsesAttribute3 = (OrgSupplierPart)Helper.CreateProduct(clientUsesAttribute3.PK, "P3");
			var productWithClientUsesSerial = (OrgSupplierPart)Helper.CreateProduct(clientUsesSerial.PK, "P4");
			var productUseAttribute1InStock = CreateProduct(clientUsesAttribute1, "P5", 1);
			var productUseAttribute2InStock = CreateProduct(clientUsesAttribute2, "P6", 2);
			var productUseAttribute3InStock = CreateProduct(clientUsesAttribute3, "P7", 3);
			var productUseSerialInStock = CreateProductWithSerial(clientUsesSerial, "P8");

			AssertEquals("Precondition", 1, productWithClientUsesAttribute1.RelatedOrganisations.Count);
			AssertEquals("Precondition", 1, productWithClientUsesAttribute2.RelatedOrganisations.Count);
			AssertEquals("Precondition", 1, productWithClientUsesAttribute3.RelatedOrganisations.Count);
			AssertEquals("Precondition", 1, productWithClientUsesSerial.RelatedOrganisations.Count);
			AssertEquals("Precondition", 1, productUseAttribute1InStock.RelatedOrganisations.Count);
			AssertEquals("Precondition", 1, productUseAttribute2InStock.RelatedOrganisations.Count);
			AssertEquals("Precondition", 1, productUseAttribute3InStock.RelatedOrganisations.Count);
			AssertEquals("Precondition", 1, productUseSerialInStock.RelatedOrganisations.Count);

			var relationForProductHasAttribute1 = productWithClientUsesAttribute1.RelatedOrganisations[0];
			var relationForProductHasAttribute2 = productWithClientUsesAttribute2.RelatedOrganisations[0];
			var relationForProductHasAttribute3 = productWithClientUsesAttribute3.RelatedOrganisations[0];
			var relationForProductHasSerial = productWithClientUsesSerial.RelatedOrganisations[0];
			var relationForProductUseAttribute1InStock = productUseAttribute1InStock.RelatedOrganisations[0];
			var relationForProductUseAttribute2InStock = productUseAttribute2InStock.RelatedOrganisations[0];
			var relationForProductUseAttribute3InStock = productUseAttribute3InStock.RelatedOrganisations[0];
			var relationForProductUseSerialInStock = productUseSerialInStock.RelatedOrganisations[0];

			AssertOrgPartRelationAttributes(relationForProductHasAttribute1, false, false, false, false, true, true);
			AssertOrgPartRelationSerialAttribute(relationForProductHasAttribute1, false, true);
			AssertOrgPartRelationAttributes(relationForProductHasAttribute2, false, false, false, true, false, true);
			AssertOrgPartRelationSerialAttribute(relationForProductHasAttribute2, false, true);
			AssertOrgPartRelationAttributes(relationForProductHasAttribute3, false, false, false, true, true, false);
			AssertOrgPartRelationSerialAttribute(relationForProductHasAttribute3, false, true);
			AssertOrgPartRelationAttributes(relationForProductHasSerial, false, false, false, true, true, true);
			AssertOrgPartRelationSerialAttribute(relationForProductHasSerial, false, false);

			AssertOrgPartRelationAttributes(relationForProductUseAttribute1InStock, true, false, false, false, true, true);
			AssertOrgPartRelationSerialAttribute(relationForProductUseAttribute1InStock, false, true);
			AssertOrgPartRelationAttributes(relationForProductUseAttribute2InStock, false, true, false, true, false, true);
			AssertOrgPartRelationSerialAttribute(relationForProductUseAttribute2InStock, false, true);
			AssertOrgPartRelationAttributes(relationForProductUseAttribute3InStock, false, false, true, true, true, false);
			AssertOrgPartRelationSerialAttribute(relationForProductUseAttribute3InStock, false, true);
			AssertOrgPartRelationAttributes(relationForProductUseSerialInStock, false, false, false, true, true, true);
			AssertOrgPartRelationSerialAttribute(relationForProductUseSerialInStock, true, false);
			Factory.Save();

			// create stock with products which doesn't use attributes
			Helper.CreateStock(whsPK, clientUsesAttribute1.PK, "R1", productWithClientUsesAttribute1.PK, 10m);
			Helper.CreateStock(whsPK, clientUsesAttribute2.PK, "R2", productWithClientUsesAttribute2.PK, 10m);
			Helper.CreateStock(whsPK, clientUsesAttribute3.PK, "R3", productWithClientUsesAttribute3.PK, 10m);
			Helper.CreateStock(whsPK, clientUsesSerial.PK, "R4", productWithClientUsesSerial.PK, 1m);
			Helper.CreateStock(whsPK, clientUsesAttribute1.PK, "R5", productUseAttribute1InStock.PK, 10m, "A1");
			Helper.CreateStock(whsPK, clientUsesAttribute2.PK, "R6", productUseAttribute2InStock.PK, 10m, "", "A2");
			Helper.CreateStock(whsPK, clientUsesAttribute3.PK, "R7", productUseAttribute3InStock.PK, 10m, "", "", "A3");
			Helper.CreateStock(whsPK, clientUsesSerial.PK, "R8", productUseSerialInStock.PK, 1m, "", "", "", "SER1");
			Factory.Save();

			if (inTransit)
			{
				var order1PK = Helper.CreateWhsOrder(clientUsesAttribute1.PK, whsPK, "O1", null);
				var order2PK = Helper.CreateWhsOrder(clientUsesAttribute2.PK, whsPK, "O2", null);
				var order3PK = Helper.CreateWhsOrder(clientUsesAttribute3.PK, whsPK, "O3", null);
				var order4PK = Helper.CreateWhsOrder(clientUsesSerial.PK, whsPK, "O4", null);
				Helper.CreateWhsOrderLine(order1PK, productWithClientUsesAttribute1.PK, 10m);
				Helper.CreateWhsOrderLine(order2PK, productWithClientUsesAttribute2.PK, 10m);
				Helper.CreateWhsOrderLine(order3PK, productWithClientUsesAttribute3.PK, 10m);
				Helper.CreateWhsOrderLine(order4PK, productWithClientUsesSerial.PK, 1m);
				var orderLineWithPA1PK = Helper.CreateWhsOrderLine(order1PK, productUseAttribute1InStock.PK, 10m);
				var orderLineWithPA2PK = Helper.CreateWhsOrderLine(order2PK, productUseAttribute2InStock.PK, 10m);
				var orderLineWithPA3PK = Helper.CreateWhsOrderLine(order3PK, productUseAttribute3InStock.PK, 10m);
				var orderLineWithSerialPK = Helper.CreateWhsOrderLine(order4PK, productUseSerialInStock.PK, 1m);
				var orderLineWithPA1 = Factory.Load<IWhsDocketLine>(orderLineWithPA1PK);
				var orderLineWithPA2 = Factory.Load<IWhsDocketLine>(orderLineWithPA2PK);
				var orderLineWithPA3 = Factory.Load<IWhsDocketLine>(orderLineWithPA3PK);
				var orderLineWithSerial = Factory.Load<IWhsDocketLine>(orderLineWithSerialPK);
				orderLineWithPA1[WhsDocketLineSchema.Constants.WE_PartAttrib1] = "A1";
				orderLineWithPA2[WhsDocketLineSchema.Constants.WE_PartAttrib2] = "A2";
				orderLineWithPA3[WhsDocketLineSchema.Constants.WE_PartAttrib3] = "A3";
				orderLineWithSerial[WhsDocketLineSchema.Constants.WE_SerialNumber] = "SER1";

				var pickPK = Helper.CreateWhsPick(new[] { order1PK, order2PK, order3PK, order4PK });
				foreach (var pickLine in helper.GetPickLines(pickPK).ToArray())
				{
					helper.PickAndMakeInTransitTransfer(pickLine, ZDateTime.Now);
				}

				Factory.Save();
			}

			// import the product from csv file
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3,Use_SerialNumber");
					sw.WriteLine("P1, Not Updated DESC FOR PRODWITHATT1, UNT, C1, Y, N, N, N");
					sw.WriteLine("P2, Not Updated DESC FOR PRODWITHATT2, UNT, C2, N, Y, N, N");
					sw.WriteLine("P3, Not Updated DESC FOR PRODWITHATT3, UNT, C3, N, N, Y, N");
					sw.WriteLine("P4, Not Updated DESC FOR PRODWITHSERIAL, UNT, C4, N, N, N, Y");

					sw.WriteLine("P1, New DESC FOR PRODWITHATT1, UNT, C1, N, N, N, N");
					sw.WriteLine("P2, New DESC FOR PRODWITHATT2, UNT, C2, N, N, N, N");
					sw.WriteLine("P3, New DESC FOR PRODWITHATT3, UNT, C3, N, N, N, N");
					sw.WriteLine("P4, New DESC FOR PRODWITHSERIAL, UNT, C4, N, N, N, N");

					sw.WriteLine("P5, Not Updated DESC FOR PRODWITHATT1, UNT, C1, N, N, N, N");
					sw.WriteLine("P6, Not Updated DESC FOR PRODWITHATT2, UNT, C2, N, N, N, N");
					sw.WriteLine("P7, Not Updated DESC FOR PRODWITHATT3, UNT, C3, N, N, N, N");
					sw.WriteLine("P8, Not Updated DESC FOR PRODWITHSERIAL, UNT, C4, N, N, N, N");

					sw.WriteLine("P5, Updated DESC FOR PRODWITHATT1, UNT, C1, Y, N, N, N");
					sw.WriteLine("P6, Updated DESC FOR PRODWITHATT2, UNT, C2, N, Y, N, N");
					sw.WriteLine("P7, Updated DESC FOR PRODWITHATT3, UNT, C3, N, N, Y, N");
					sw.WriteLine("P8, Updated DESC FOR PRODWITHSERIAL, UNT, C4, N, N, N, Y");
					sw.Flush();
				}
				AssertEquals("Precondition", 0, Loader.Log.Count);
				Loader.ImportProductData(testFileName.Filename, true, false);
			}

			AssertEquals("Precondition", 18, Loader.Log.Count);
			AssertEquals("Products to Import = 16", Loader.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: P1 / Not Updated DESC FOR PRODWITHATT1  Inventory or Un-finalized ASNs exist(s) for this product with attribute settings different from those in the CSV file.", Loader.Log[1]);
			AssertEquals("Line 3: PART NO/DESC: P2 / Not Updated DESC FOR PRODWITHATT2  Inventory or Un-finalized ASNs exist(s) for this product with attribute settings different from those in the CSV file.", Loader.Log[2]);
			AssertEquals("Line 4: PART NO/DESC: P3 / Not Updated DESC FOR PRODWITHATT3  Inventory or Un-finalized ASNs exist(s) for this product with attribute settings different from those in the CSV file.", Loader.Log[3]);
			AssertEquals("Line 5: PART NO/DESC: P4 / Not Updated DESC FOR PRODWITHSERIAL  Inventory or Un-finalized ASNs exist(s) for this product with attribute settings different from those in the CSV file.", Loader.Log[4]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", Loader.Log[5]);
			AssertEquals("PART NO: P2 - Part has been UPDATED", Loader.Log[6]);
			AssertEquals("PART NO: P3 - Part has been UPDATED", Loader.Log[7]);
			AssertEquals("PART NO: P4 - Part has been UPDATED", Loader.Log[8]);
			AssertEquals("Line 10: PART NO/DESC: P5 / Not Updated DESC FOR PRODWITHATT1  Inventory or Un-finalized ASNs exist(s) for this product with attribute settings different from those in the CSV file.", Loader.Log[9]);
			AssertEquals("Line 11: PART NO/DESC: P6 / Not Updated DESC FOR PRODWITHATT2  Inventory or Un-finalized ASNs exist(s) for this product with attribute settings different from those in the CSV file.", Loader.Log[10]);
			AssertEquals("Line 12: PART NO/DESC: P7 / Not Updated DESC FOR PRODWITHATT3  Inventory or Un-finalized ASNs exist(s) for this product with attribute settings different from those in the CSV file.", Loader.Log[11]);
			AssertEquals("Line 13: PART NO/DESC: P8 / Not Updated DESC FOR PRODWITHSERIAL  Inventory or Un-finalized ASNs exist(s) for this product with attribute settings different from those in the CSV file.", Loader.Log[12]);
			AssertEquals("PART NO: P5 - Part has been UPDATED", Loader.Log[13]);
			AssertEquals("PART NO: P6 - Part has been UPDATED", Loader.Log[14]);
			AssertEquals("PART NO: P7 - Part has been UPDATED", Loader.Log[15]);
			AssertEquals("PART NO: P8 - Part has been UPDATED", Loader.Log[16]);
			var expectedUpdated = 8;
			var expectedExcluded = 8;
			AssertEquals($"\r\nT O T A L : Products created = 0, Products updated = {expectedUpdated}, Products excluded = {expectedExcluded}\r\n", Loader.Log[17]);

			var newFactory = new BusinessObjectFactory();
			var relationForProductHasAttribute1InAnotherFactory = newFactory.Load<OrgPartRelation>(relationForProductHasAttribute1.PK);
			var relationForProductHasAttribute2InAnotherFactory = newFactory.Load<OrgPartRelation>(relationForProductHasAttribute2.PK);
			var relationForProductHasAttribute3InAnotherFactory = newFactory.Load<OrgPartRelation>(relationForProductHasAttribute3.PK);
			var relationForProductHasSerialInAnotherFactory = newFactory.Load<OrgPartRelation>(relationForProductHasSerial.PK);
			var relationForProductUseAttribute1InStockInAnotherFactory = newFactory.Load<OrgPartRelation>(productUseAttribute1InStock.RelatedOrganisations[0].PK);
			var relationForProductUseAttribute2InStockInAnotherFactory = newFactory.Load<OrgPartRelation>(productUseAttribute2InStock.RelatedOrganisations[0].PK);
			var relationForProductUseAttribute3InStockInAnotherFactory = newFactory.Load<OrgPartRelation>(productUseAttribute3InStock.RelatedOrganisations[0].PK);
			var relationForProductUseSerialInStockInAnotherFactory = newFactory.Load<OrgPartRelation>(productUseSerialInStock.RelatedOrganisations[0].PK);

			AssertOrgPartRelationAttributes(relationForProductHasAttribute1InAnotherFactory, false, false, false, false, true, true);
			AssertOrgPartRelationSerialAttribute(relationForProductHasAttribute1InAnotherFactory, false, true);
			AssertOrgPartRelationAttributes(relationForProductHasAttribute2InAnotherFactory, false, false, false, true, false, true);
			AssertOrgPartRelationSerialAttribute(relationForProductHasAttribute2InAnotherFactory, false, true);
			AssertOrgPartRelationAttributes(relationForProductHasAttribute3InAnotherFactory, false, false, false, true, true, false);
			AssertOrgPartRelationSerialAttribute(relationForProductHasAttribute3InAnotherFactory, false, true);
			AssertOrgPartRelationAttributes(relationForProductHasSerialInAnotherFactory, false, false, false, true, true, true);
			AssertOrgPartRelationSerialAttribute(relationForProductHasSerialInAnotherFactory, false, false);

			AssertOrgPartRelationAttributes(relationForProductUseAttribute1InStockInAnotherFactory, true, false, false, false, true, true);
			AssertOrgPartRelationSerialAttribute(relationForProductUseAttribute1InStockInAnotherFactory, false, true);
			AssertOrgPartRelationAttributes(relationForProductUseAttribute2InStockInAnotherFactory, false, true, false, true, false, true);
			AssertOrgPartRelationSerialAttribute(relationForProductUseAttribute2InStockInAnotherFactory, false, true);
			AssertOrgPartRelationAttributes(relationForProductUseAttribute3InStockInAnotherFactory, false, false, true, true, true, false);
			AssertOrgPartRelationSerialAttribute(relationForProductUseAttribute3InStockInAnotherFactory, false, true);
			AssertOrgPartRelationAttributes(relationForProductUseSerialInStockInAnotherFactory, false, false, false, true, true, true);
			AssertOrgPartRelationSerialAttribute(relationForProductUseSerialInStockInAnotherFactory, true, false);
		}

		#endregion

		#region TestImportProductWithExistingInventory_ForNonMandatoryAttributes

		public void TestImportProductWithExistingInventory_ForNonMandatoryAttributes()
		{
			// setup product and client
			var whsPK = Helper.CreateWarehouse("1", "A").PK;
			var clientUsesAttribute1 = CreateClient("C1", 1, "A1", PartAttributeTypeList.Codes.NonMandatory);
			var clientUsesAttribute2 = CreateClient("C2", 2, "A2", PartAttributeTypeList.Codes.NonMandatory);
			var clientUsesAttribute3 = CreateClient("C3", 3, "A3", PartAttributeTypeList.Codes.NonMandatory);

			var productWithClientUsesAttribute1 = (OrgSupplierPart)Helper.CreateProduct(clientUsesAttribute1.PK, "P1");
			var productWithClientUsesAttribute2 = (OrgSupplierPart)Helper.CreateProduct(clientUsesAttribute2.PK, "P2");
			var productWithClientUsesAttribute3 = (OrgSupplierPart)Helper.CreateProduct(clientUsesAttribute3.PK, "P3");

			AssertEquals("Precondition", 1, productWithClientUsesAttribute1.RelatedOrganisations.Count);
			AssertEquals("Precondition", 1, productWithClientUsesAttribute2.RelatedOrganisations.Count);
			AssertEquals("Precondition", 1, productWithClientUsesAttribute3.RelatedOrganisations.Count);
			var relationForProductHasAttribute1 = productWithClientUsesAttribute1.RelatedOrganisations[0];
			var relationForProductHasAttribute2 = productWithClientUsesAttribute2.RelatedOrganisations[0];
			var relationForProductHasAttribute3 = productWithClientUsesAttribute3.RelatedOrganisations[0];
			AssertOrgPartRelationAttributes(relationForProductHasAttribute1, false, false, false, false, true, true);
			AssertOrgPartRelationAttributes(relationForProductHasAttribute2, false, false, false, true, false, true);
			AssertOrgPartRelationAttributes(relationForProductHasAttribute3, false, false, false, true, true, false);
			Factory.Save();

			// test with no existing inventory -- try to enable non mandatory attributes
			var loader1 = OrgSupplierPartDataLoad.New();
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3");
					sw.WriteLine("P1, New DESC FOR PRODWITHATT1, UNT, C1, Y, N, N");
					sw.WriteLine("P2, New DESC FOR PRODWITHATT2, UNT, C2, N, Y, N");
					sw.WriteLine("P3, New DESC FOR PRODWITHATT3, UNT, C3, N, N, Y");
					sw.Flush();
				}
				AssertEquals("Precondition", 0, loader1.Log.Count);
				loader1.ImportProductData(testFileName.Filename, true, false);
			}

			AssertEquals("Precondition", 5, loader1.Log.Count);
			AssertEquals("Products to Import = 3", loader1.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", loader1.Log[1]);
			AssertEquals("PART NO: P2 - Part has been UPDATED", loader1.Log[2]);
			AssertEquals("PART NO: P3 - Part has been UPDATED", loader1.Log[3]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 3, Products excluded = 0\r\n", loader1.Log[4]);

			// test with no existing inventory -- try to disable non mandatory attributes
			var loader2 = OrgSupplierPartDataLoad.New();// Cleanup
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3");
					sw.WriteLine("P1, New DESC FOR PRODWITHATT1, UNT, C1, N, N, N");
					sw.WriteLine("P2, New DESC FOR PRODWITHATT2, UNT, C2, N, N, N");
					sw.WriteLine("P3, New DESC FOR PRODWITHATT3, UNT, C3, N, N, N");
					sw.Flush();
				}
				AssertEquals("Precondition", 0, loader2.Log.Count);
				loader2.ImportProductData(testFileName.Filename, true, false);
			}

			AssertEquals("Precondition", 5, loader2.Log.Count);
			AssertEquals("Products to Import = 3", loader2.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", loader2.Log[1]);
			AssertEquals("PART NO: P2 - Part has been UPDATED", loader2.Log[2]);
			AssertEquals("PART NO: P3 - Part has been UPDATED", loader2.Log[3]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 3, Products excluded = 0\r\n", loader2.Log[4]);

			var loader3 = OrgSupplierPartDataLoad.New();// Cleanup

			// create stock with products not using any attributes
			Helper.CreateStock(whsPK, clientUsesAttribute1.PK, "R1", productWithClientUsesAttribute1.PK, 10m);
			Helper.CreateStock(whsPK, clientUsesAttribute2.PK, "R2", productWithClientUsesAttribute2.PK, 10m);
			Helper.CreateStock(whsPK, clientUsesAttribute3.PK, "R3", productWithClientUsesAttribute3.PK, 10m);
			Factory.Save();

			// test with existing inventory -- try to enable non mandatory attributes
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3");
					sw.WriteLine("P1, New DESC FOR PRODWITHATT1, UNT, C1, Y, N, N");
					sw.WriteLine("P2, New DESC FOR PRODWITHATT2, UNT, C2, N, Y, N");
					sw.WriteLine("P3, New DESC FOR PRODWITHATT3, UNT, C3, N, N, Y");
					sw.Flush();
				}
				AssertEquals("Precondition", 0, loader3.Log.Count);
				loader3.ImportProductData(testFileName.Filename, true, false);
			}

			AssertEquals("Precondition", 5, loader3.Log.Count);
			AssertEquals("Products to Import = 3", loader3.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", loader3.Log[1]);
			AssertEquals("PART NO: P2 - Part has been UPDATED", loader3.Log[2]);
			AssertEquals("PART NO: P3 - Part has been UPDATED", loader3.Log[3]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 3, Products excluded = 0\r\n", loader3.Log[4]);

			var loader4 = OrgSupplierPartDataLoad.New(); // Cleanup

			// test with existing inventory -- try to disable non mandatory attributes -- should be failed
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3");
					sw.WriteLine("P1, New DESC FOR PRODWITHATT1, UNT, C1, N, N, N");
					sw.WriteLine("P2, New DESC FOR PRODWITHATT2, UNT, C2, N, N, N");
					sw.WriteLine("P3, New DESC FOR PRODWITHATT3, UNT, C3, N, N, N");
					sw.Flush();
				}
				AssertEquals("Precondition", 0, loader4.Log.Count);
				loader4.ImportProductData(testFileName.Filename, true, false);
			}

			AssertEquals("Precondition", 5, loader4.Log.Count);
			AssertEquals("Products to Import = 3", loader4.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: P1 / New DESC FOR PRODWITHATT1  Inventory or Un-finalized ASNs exist(s) for this product with attribute settings different from those in the CSV file.", loader4.Log[1]);
			AssertEquals("Line 3: PART NO/DESC: P2 / New DESC FOR PRODWITHATT2  Inventory or Un-finalized ASNs exist(s) for this product with attribute settings different from those in the CSV file.", loader4.Log[2]);
			AssertEquals("Line 4: PART NO/DESC: P3 / New DESC FOR PRODWITHATT3  Inventory or Un-finalized ASNs exist(s) for this product with attribute settings different from those in the CSV file.", loader4.Log[3]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 3\r\n", loader4.Log[4]);

			var newFactory = new BusinessObjectFactory();
			var relationForProductHasAttribute1InAnotherFactory = newFactory.Load<OrgPartRelation>(relationForProductHasAttribute1.PK);
			var relationForProductHasAttribute2InAnotherFactory = newFactory.Load<OrgPartRelation>(relationForProductHasAttribute2.PK);
			var relationForProductHasAttribute3InAnotherFactory = newFactory.Load<OrgPartRelation>(relationForProductHasAttribute3.PK);
			AssertOrgPartRelationAttributes(relationForProductHasAttribute1InAnotherFactory, true, false, false, false, true, true);
			AssertOrgPartRelationAttributes(relationForProductHasAttribute2InAnotherFactory, false, true, false, true, false, true);
			AssertOrgPartRelationAttributes(relationForProductHasAttribute3InAnotherFactory, false, false, true, true, true, false);
		}

		#endregion

		#region ImportProduct_ValidateOU_IsPartAttribReleaseCaptured

		public void TestImportProduct_ValidateOU_IsPartAttrib1ReleaseCaptured()
		{
			ImportProduct_ValidateOU_IsPartAttribReleaseCapturedCore(column: OrgPartRelationSchema.OU_IsPartAttrib1ReleaseCaptured, csvHeader: "IsPartAttrib1ReleaseCaptured");
		}

		public void TestImportProduct_ValidateOU_IsPartAttrib2ReleaseCaptured()
		{
			ImportProduct_ValidateOU_IsPartAttribReleaseCapturedCore(column: OrgPartRelationSchema.OU_IsPartAttrib2ReleaseCaptured, csvHeader: "IsPartAttrib2ReleaseCaptured");
		}

		public void TestImportProduct_ValidateOU_IsPartAttrib3ReleaseCaptured()
		{
			ImportProduct_ValidateOU_IsPartAttribReleaseCapturedCore(column: OrgPartRelationSchema.OU_IsPartAttrib3ReleaseCaptured, csvHeader: "IsPartAttrib3ReleaseCaptured");
		}

		void ImportProduct_ValidateOU_IsPartAttribReleaseCapturedCore(SchemaColumn column, string csvHeader)
		{
			var warehouse = Helper.CreateWarehouse("1", "A");
			var client = CreateClientWithNonMandatoryAttributes("C1");
			var product = CreateProductWithAttributes(client, "P1", true);
			Factory.Save();

			helper.CreateStock(warehouse.PK, client.PK, product.PK, 10m);
			Factory.Save();

			var newLoader = OrgSupplierPartDataLoad.New();
			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3,{csvHeader}",
																		"P1, Product description, UNT, C1, Y, Y, Y, Y", newLoader);

			AssertEquals("Precondition", 3, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals($"Line 2: PART NO/DESC: P1 / Product description  {csvHeader}: Release Capture cannot be changed because inventory already exists that uses this attribute.\r\nYou must first remove this inventory from the warehouse.", newLoader.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", newLoader.Log[2]);

			AssertEquals($"Should not update {column.Name} to true.", false, new BusinessObjectFactory().Load<OrgPartRelation>(product.RelatedOrganisations[0].PK)[column]);
		}

		public void TestImportProduct_ValidateOU_IsSerialNumberReleaseCaptured()
		{
			var warehouse = Helper.CreateWarehouse("1", "A");
			var client = CreateClientUsingSerialNumber("C1");
			var product = CreateProductWithSerial(client, "P1");
			Factory.Save();

			var loader1 = OrgSupplierPartDataLoad.New();
			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,Use_SerialNumber,IsSerialNumberReleaseCaptured",
																		"P1, Product description, UNT, C1, Y, Y", loader1);

			AssertEquals("Precondition", 3, loader1.Log.Count);
			AssertEquals("Products to Import = 1", loader1.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", loader1.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", loader1.Log[2]);

			AssertEquals($"Should update OU_IsSerialNumberReleaseCaptured to true.", true, new BusinessObjectFactory().Load<OrgPartRelation>(product.RelatedOrganisations[0].PK)[OrgPartRelationSchema.OU_IsSerialNumberReleaseCaptured]);

			var loader2 = OrgSupplierPartDataLoad.New();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,Use_SerialNumber,IsSerialNumberReleaseCaptured",
																		"P1, Product description, UNT, C1, Y, N",loader2);

			AssertEquals("Precondition", 3, loader2.Log.Count);
			AssertEquals("Products to Import = 1", loader2.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", loader2.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", loader2.Log[2]);

			AssertEquals($"Should update OU_IsSerialNumberReleaseCaptured to false.", false, new BusinessObjectFactory().Load<OrgPartRelation>(product.RelatedOrganisations[0].PK)[OrgPartRelationSchema.OU_IsSerialNumberReleaseCaptured]);

			var loader3 = OrgSupplierPartDataLoad.New();

			helper.CreateStock(warehouse.PK, client.PK, "R1", product.PK, 1m, sn: "SER1");
			Factory.Save();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,Use_SerialNumber,IsSerialNumberReleaseCaptured",
																		"P1, Product description, UNT, C1, Y, Y", loader3);

			AssertEquals("Precondition", 3, loader3.Log.Count);
			AssertEquals("Products to Import = 1", loader3.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: P1 / Product description  IsSerialNumberReleaseCaptured: Release Capture cannot be changed because inventory already exists that uses this attribute.\r\nYou must first remove this inventory from the warehouse.", loader3.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", loader3.Log[2]);

			AssertEquals($"Should not update OU_IsSerialNumberReleaseCaptured to true.", false, new BusinessObjectFactory().Load<OrgPartRelation>(product.RelatedOrganisations[0].PK)[OrgPartRelationSchema.OU_IsSerialNumberReleaseCaptured]);
		}

		public void TestImportProduct_ValidateOU_UseExpiryDate()
		{
			var warehouse = Helper.CreateWarehouse("1", "A");
			var client = CreateClientWithNonMandatoryAttributes("C1");
			var product = CreateProductWithAttributes(client, "P1", true);
			Factory.Save();

			helper.CreateStock(warehouse.PK, client.PK, product.PK, 10m);
			Factory.Save();

			var newLoader = OrgSupplierPartDataLoad.New();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3,UseExpiryDate",
																		"P1, Product description, UNT, C1, Y, Y, Y, Y", newLoader);

			AssertEquals("Precondition", 4, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: P1 / Product description  UseExpiryDate: " + "There is current inventory which is NOT using this attribute. This inventory must be removed from the warehouse before this attribute can be enabled.", newLoader.Log[1]);
			AssertEquals("Line 2: PART NO/DESC: P1 / Product description  Inventory or Un-finalized ASNs exist(s) for this product with attribute settings different from those in the CSV file.", newLoader.Log[2]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", newLoader.Log[3]);

			AssertEquals("Should not update OU_UseExpiryDate to true.", false, new BusinessObjectFactory().Load<OrgPartRelation>(product.RelatedOrganisations[0].PK).OU_UseExpiryDate);
		}

		public void TestImportProduct_ValidateOU_UsePackingDate()
		{
			var warehouse = Helper.CreateWarehouse("1", "A");
			var client = CreateClientWithNonMandatoryAttributes("C1");
			var product = CreateProductWithAttributes(client, "P1", true);
			Factory.Save();

			helper.CreateStock(warehouse.PK, client.PK, product.PK, 10m);
			Factory.Save();

			var newLoader = OrgSupplierPartDataLoad.New();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3,UsePackingDate",
																		"P1, Product description, UNT, C1, Y, Y, Y, Y", newLoader);

			AssertEquals("Precondition", 4, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: P1 / Product description  UsePackingDate: " + "There is current inventory which is NOT using this attribute. This inventory must be removed from the warehouse before this attribute can be enabled.", newLoader.Log[1]);
			AssertEquals("Line 2: PART NO/DESC: P1 / Product description  Inventory or Un-finalized ASNs exist(s) for this product with attribute settings different from those in the CSV file.", newLoader.Log[2]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", newLoader.Log[3]);

			AssertEquals("Should not update OU_UsePackingDate to true.", false, new BusinessObjectFactory().Load<OrgPartRelation>(product.RelatedOrganisations[0].PK).OU_UsePackingDate);
		}

		public void TestImportProduct_ValidateOU_UseSerialNumber()
		{
			var warehouse = Helper.CreateWarehouse("1", "A");
			var client = CreateClientUsingSerialNumber("C1");
			var product = CreateProductWithSerial(client, "P1");
			Factory.Save();

			helper.CreateStock(warehouse.PK, client.PK, "R1", product.PK, 1m, sn: "SER1");
			Factory.Save();

			// import the product from csv file
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Use_Attribute1,Use_SerialNumber");
					sw.WriteLine("P1, Product description, UNT, C1, Y, Y");
					sw.WriteLine("P1, Product description, UNT, C1, N, N");
					sw.WriteLine("P1, Product description, UNT, C1, N, Y");
					sw.Flush();
				}
				AssertEquals("Precondition", 0, Loader.Log.Count);
				Loader.ImportProductData(testFileName.Filename, true, false);
			}

			AssertEquals("Precondition", 5, Loader.Log.Count);
			AssertEquals("Products to Import = 3", Loader.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: P1 / Product description  Part attributes doesn't match with client.", Loader.Log[1]);
			AssertEquals("Line 3: PART NO/DESC: P1 / Product description  Inventory or Un-finalized ASNs exist(s) for this product with attribute settings different from those in the CSV file.", Loader.Log[2]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", Loader.Log[3]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 2\r\n", Loader.Log[4]);

			AssertEquals("Should not update OU_UseSerialNumber to true.", false, new BusinessObjectFactory().Load<OrgPartRelation>(product.RelatedOrganisations[0].PK).OU_UsePackingDate);
		}

		#endregion

		#region TestProductImportRelationship_DefaultHoldCode

		public void TestProductImportRelationship_DefaultHoldCode()
		{
			var client = Helper.CreateClient("C1");
			var product = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			product.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,DefaultHoldCode");
					sw.WriteLine("P1, Product description, UNT, C1, HEL");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, Loader.Log.Count);
				Loader.ImportProductData(testFileName.Filename, true, false);
			}

			var holdCode = Factory.Load<IWhsInventoryHeldCode>(new ZQuery(WhsInventoryHeldCodeSchema.WHC_Code, "HEL")).Single();
			var relationInAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product.PK)).Single();
			AssertEquals("Precondition", 3, Loader.Log.Count);
			AssertEquals("Products to Import = 1", Loader.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", Loader.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", Loader.Log[2]);
			AssertEquals("Should update.", holdCode.PK, relationInAnotherFactory.OU_WHC_DefaultInventoryHoldCode);
		}

		public void TestProductImportRelationship_InvalidHoldCode()
		{
			var client = Helper.CreateClient("C1");
			var product = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			product.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,DefaultHoldCode");
					sw.WriteLine("P1, Product description, UNT, C1, ABC");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, Loader.Log.Count);
				Loader.ImportProductData(testFileName.Filename, true, false);
			}

			var relationInAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product.PK)).Single();
			AssertEquals("Precondition", 3, Loader.Log.Count);
			AssertEquals("Products to Import = 1", Loader.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", Loader.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", Loader.Log[2]);
			AssertEquals("Should not update.", ZGuid.Empty, relationInAnotherFactory.OU_WHC_DefaultInventoryHoldCode);
		}

		#endregion

		#region TestProductImportRelationship_CartonGroup

		public void TestProductImportRelationship_CartonGroup()
		{
			var client = Helper.CreateClient("C1");
			var product = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			product.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			var cartonGroup = Factory.New<IWhsCartonGroup>();
			cartonGroup.WCG_Code = "ABC";
			cartonGroup.WCG_Description = "ABC123";
			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,CartonGroup");
					sw.WriteLine("P1, Product description, UNT, C1, ABC");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, Loader.Log.Count);
				Loader.ImportProductData(testFileName.Filename, true, false);
			}

			var relationInAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product.PK)).Single();
			AssertEquals("Precondition", 3, Loader.Log.Count);
			AssertEquals("Products to Import = 1", Loader.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", Loader.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", Loader.Log[2]);
			AssertEquals("Should update.", cartonGroup.PK, relationInAnotherFactory.OU_WCG_CartonGroup);
		}

		public void TestProductImportRelationship_InvalidCartonGroup()
		{
			var client = Helper.CreateClient("C1");
			var product = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			product.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,CartonGroup");
					sw.WriteLine("P1, Product description, UNT, C1, ABC");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, Loader.Log.Count);
				Loader.ImportProductData(testFileName.Filename, true, false);
			}

			var relationInAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product.PK)).Single();
			AssertEquals("Precondition", 3, Loader.Log.Count);
			AssertEquals("Products to Import = 1", Loader.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", Loader.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", Loader.Log[2]);
			AssertEquals("Should not update.", ZGuid.Empty, relationInAnotherFactory.OU_WCG_CartonGroup);
		}

		#endregion

		#region TestProductImportRelationship_UnitPrice

		public void TestProductImportRelationship_UnitPrice()
		{
			var client = Helper.CreateClient("C1");
			var product = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			product.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,UnitPrice,UnitPriceCurrency");
					sw.WriteLine("P1, Product description, UNT, C1, 10.25, AUD");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, Loader.Log.Count);
				Loader.ImportProductData(testFileName.Filename, true, false);
			}

			var relationInAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product.PK)).Single();
			AssertEquals("Precondition", 3, Loader.Log.Count);
			AssertEquals("Products to Import = 1", Loader.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", Loader.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", Loader.Log[2]);
			AssertEquals("Should update.", 10.25m, relationInAnotherFactory.OU_UnitPrice);
			AssertEquals("Should update.", "AUD", relationInAnotherFactory.OU_RX_NKUnitPriceCurrency);
		}

		public void TestProductImportRelationship_UnitPriceTooBig()
		{
			var client = Helper.CreateClient("C1");
			var product = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			product.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,UnitPrice,UnitPriceCurrency");
					sw.WriteLine("P1, Product description, UNT, C1, 999999999999999999999999, AUD");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, Loader.Log.Count);
				Loader.ImportProductData(testFileName.Filename, true, false);
			}

			var relationInAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product.PK)).Single();
			AssertEquals("Precondition", 4, Loader.Log.Count);
			AssertEquals("Products to Import = 1", Loader.Log[0]);
			AssertEquals("Row 2 excluded... data is inconsistent with required format.", Loader.Log[1]);
#if NETFRAMEWORK
			AssertEquals("Specified argument was out of the range of valid values.\r\nParameter name: Value of 999999999999999999999999 is too large to store in UnitPrice", Loader.Log[2]);
#else
			AssertEquals("Specified argument was out of the range of valid values. (Parameter 'Value of 999999999999999999999999 is too large to store in UnitPrice')", Loader.Log[2]);
#endif
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", Loader.Log[3]);
			AssertEquals("Should not update.", 0m, relationInAnotherFactory.OU_UnitPrice);
			AssertEquals("Should not update.", "", relationInAnotherFactory.OU_RX_NKUnitPriceCurrency);
		}

		public void TestProductImportRelationship_UnitPrice_ExtendedDecimalComponent()
		{
			var client = Helper.CreateClient("C1");
			var product = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			product.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,UnitPrice,UnitPriceCurrency");
					sw.WriteLine("P1, Product description, UNT, C1, 9.987654321, AUD");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, Loader.Log.Count);
				Loader.ImportProductData(testFileName.Filename, true, false);
			}

			var relationInAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product.PK)).Single();
			AssertEquals("Precondition", 4, Loader.Log.Count);
			AssertEquals("Products to Import = 1", Loader.Log[0]);
			AssertEquals("Row 2 excluded... data is inconsistent with required format.", Loader.Log[1]);
#if NETFRAMEWORK
			AssertEquals("Specified argument was out of the range of valid values.\r\nParameter name: Value of 9.987654321 has too many decimal places to store in UnitPrice", Loader.Log[2]);
#else
			AssertEquals("Specified argument was out of the range of valid values. (Parameter 'Value of 9.987654321 has too many decimal places to store in UnitPrice')", Loader.Log[2]);
#endif
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", Loader.Log[3]);
			AssertEquals("Should not update.", 0m, relationInAnotherFactory.OU_UnitPrice);
			AssertEquals("Should not update.", "", relationInAnotherFactory.OU_RX_NKUnitPriceCurrency);
		}

		public void TestProductImportRelationship_UnitPrice_MaxMoneyValue()
		{
			var client = Helper.CreateClient("C1");
			var product = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			product.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,UnitPrice,UnitPriceCurrency");
					sw.WriteLine("P1, Product description, UNT, C1, 922337203685477.58, AUD");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, Loader.Log.Count);
				Loader.ImportProductData(testFileName.Filename, true, false);
			}

			var relationInAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product.PK)).Single();
			AssertEquals("Precondition", 3, Loader.Log.Count);
			AssertEquals("Products to Import = 1", Loader.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", Loader.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", Loader.Log[2]);
			AssertEquals("Should update.", 922337203685477.58m, relationInAnotherFactory.OU_UnitPrice);
			AssertEquals("Should update.", "AUD", relationInAnotherFactory.OU_RX_NKUnitPriceCurrency);
		}

		public void TestProductImportRelationship_UnitPrice_MoreThanMaxMoneyValue()
		{
			var client = Helper.CreateClient("C1");
			var product = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			product.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,UnitPrice,UnitPriceCurrency");
					sw.WriteLine("P1, Product description, UNT, C1, 922337203685478, AUD");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, Loader.Log.Count);
				Loader.ImportProductData(testFileName.Filename, true, false);
			}

			var relationInAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product.PK)).Single();
			AssertEquals("Precondition", 4, Loader.Log.Count);
			AssertEquals("Products to Import = 1", Loader.Log[0]);
			AssertEquals("The Unit Price of 922337203685478 is invalid. Valid value range for unit price is between 0 and 922337203685477.58. The unit price will not be imported.", Loader.Log[1]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", Loader.Log[2]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", Loader.Log[3]);
			AssertEquals("Should not update.", 0m, relationInAnotherFactory.OU_UnitPrice);
			AssertEquals("Should not update.", "", relationInAnotherFactory.OU_RX_NKUnitPriceCurrency);
		}

		public void TestProductImportRelationship_UnitPrice_NegativeValue()
		{
			var client = Helper.CreateClient("C1");
			var product = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			product.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,UnitPrice,UnitPriceCurrency");
					sw.WriteLine("P1, Product description, UNT, C1, -1, AUD");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, Loader.Log.Count);
				Loader.ImportProductData(testFileName.Filename, true, false);
			}

			var relationInAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product.PK)).Single();
			AssertEquals("Precondition", 4, Loader.Log.Count);
			AssertEquals("Products to Import = 1", Loader.Log[0]);
			AssertEquals("The Unit Price of -1 is invalid. Valid value range for unit price is between 0 and 922337203685477.58. The unit price will not be imported.", Loader.Log[1]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", Loader.Log[2]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", Loader.Log[3]);
			AssertEquals("Should not update.", 0m, relationInAnotherFactory.OU_UnitPrice);
			AssertEquals("Should not update.", "", relationInAnotherFactory.OU_RX_NKUnitPriceCurrency);
		}

		public void TestProductImportRelationship_UnitPriceNoCurrency()
		{
			var client = Helper.CreateClient("C1");
			var product = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			product.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,UnitPrice");
					sw.WriteLine("P1, Product description, UNT, C1, 10.00");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, Loader.Log.Count);
				Loader.ImportProductData(testFileName.Filename, true, false);
			}

			var relationInAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product.PK)).Single();
			AssertEquals("Precondition", 4, Loader.Log.Count);
			AssertEquals("Products to Import = 1", Loader.Log[0]);
			AssertEquals("The Unit Price is invalid because '' is not a valid currency. The unit price will not be imported. Please enter a valid currency to import a unit price.", Loader.Log[1]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", Loader.Log[2]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", Loader.Log[3]);
			AssertEquals("Should not update.", 0m, relationInAnotherFactory.OU_UnitPrice);
			AssertEquals("Should not update.", "", relationInAnotherFactory.OU_RX_NKUnitPriceCurrency);
		}

		public void TestProductImportRelationship_UnitPriceInvalidCurrencyCode()
		{
			var client = Helper.CreateClient("C1");
			var product = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			product.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,UnitPrice,UnitPriceCurrency");
					sw.WriteLine("P1, Product description, UNT, C1, 10.00, XXX");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, Loader.Log.Count);
				Loader.ImportProductData(testFileName.Filename, true, false);
			}

			var relationInAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product.PK)).Single();
			AssertEquals("Precondition", 4, Loader.Log.Count);
			AssertEquals("Products to Import = 1", Loader.Log[0]);
			AssertEquals("The Unit Price is invalid because 'XXX' is not a valid currency. The unit price will not be imported. Please enter a valid currency to import a unit price.", Loader.Log[1]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", Loader.Log[2]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", Loader.Log[3]);
			AssertEquals("Should not update.", 0m, relationInAnotherFactory.OU_UnitPrice);
			AssertEquals("Should not update.", "", relationInAnotherFactory.OU_RX_NKUnitPriceCurrency);
		}

		public void TestProductImportRelationship_UnitPrice_MultipleRecordsWithInvalidData_InvalidUnitPrice()
		{
			var client = Helper.CreateClient("C1");
			var product1 = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			product1.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			var product2 = (OrgSupplierPart)Helper.CreateProduct(client, "P2");
			product2.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,UQ,OWNER,UnitPrice,UnitPriceCurrency");
					sw.WriteLine("P1, UNT, C1, 2, AUD");
					sw.WriteLine("P2, UNT, C1, -2, AUD");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, Loader.Log.Count);
				Loader.ImportProductData(testFileName.Filename, true, false);
			}

			AssertEquals("Precondition", 5, Loader.Log.Count);
			AssertEquals("Products to Import = 2", Loader.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", Loader.Log[1]);
			AssertEquals("The Unit Price of -2 is invalid. Valid value range for unit price is between 0 and 922337203685477.58. The unit price will not be imported.", Loader.Log[2]);
			AssertEquals("PART NO: P2 - Part has been UPDATED", Loader.Log[3]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 2, Products excluded = 0\r\n", Loader.Log[4]);

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

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,UQ,OWNER,UnitPrice,UnitPriceCurrency");
					sw.WriteLine("P1, UNT, C1, 2, AUD");
					sw.WriteLine("P2, UNT, C1, 2, XXX");
					sw.Flush();
				}

				AssertEquals("Precondition", 0, Loader.Log.Count);
				Loader.ImportProductData(testFileName.Filename, true, false);
			}

			AssertEquals("Precondition", 5, Loader.Log.Count);
			AssertEquals("Products to Import = 2", Loader.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", Loader.Log[1]);
			AssertEquals("The Unit Price is invalid because 'XXX' is not a valid currency. The unit price will not be imported. Please enter a valid currency to import a unit price.", Loader.Log[2]);
			AssertEquals("PART NO: P2 - Part has been UPDATED", Loader.Log[3]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 2, Products excluded = 0\r\n", Loader.Log[4]);

			var relation1InAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product1.PK)).Single();
			AssertEquals("Should update.", 2m, relation1InAnotherFactory.OU_UnitPrice);
			AssertEquals("Should update.", "AUD", relation1InAnotherFactory.OU_RX_NKUnitPriceCurrency);

			var relation2InAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product2.PK)).Single();
			AssertEquals("Should not update.", 0m, relation2InAnotherFactory.OU_UnitPrice);
			AssertEquals("Should not update.", "", relation2InAnotherFactory.OU_RX_NKUnitPriceCurrency);
		}

		#endregion

		#region TestImportProductDecimalPlaces

		public void TestImportProductDecimalPlaces_TooBig()
		{
			Helper.CreateWarehouse("1", "A");
			var client = CreateClientWithNonMandatoryAttributes("C1");
			var product = CreateProductWithAttributes(client, "P1", true);
			Factory.Save();

			var newLoader = OrgSupplierPartDataLoad.New();
			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,DecimalPlaces",
																		$"P1, Product description, UNT, C1, 256", newLoader);

			AssertEquals("Precondition", 4, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("Row 2 excluded... data is inconsistent with required format.", newLoader.Log[1]);
#if NETFRAMEWORK
			AssertEquals("Specified argument was out of the range of valid values.\r\nParameter name: Value of 256 is invalid or too large to store in DecimalPlaces", newLoader.Log[2]);
#else
			AssertEquals("Specified argument was out of the range of valid values. (Parameter 'Value of 256 is invalid or too large to store in DecimalPlaces')", newLoader.Log[2]);
#endif
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", newLoader.Log[3]);

			AssertEquals("Should not update.", (ZByte)0, new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK)[OrgSupplierPartSchema.OP_CountDecimalPlaces]);
		}

		public void TestImportProductDecimalPlaces_MaxValue()
		{
			Helper.CreateWarehouse("1", "A");
			var client = CreateClientWithNonMandatoryAttributes("C1");
			var product = CreateProductWithAttributes(client, "P1", true);
			Factory.Save();

			var newLoader = OrgSupplierPartDataLoad.New();
			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,DecimalPlaces",
																		$"P1, Product description, UNT, C1, 9", newLoader);

			AssertEquals("Precondition", 3, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", newLoader.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", newLoader.Log[2]);

			AssertEquals("Should update the product.", (ZByte)9, new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK)[OrgSupplierPartSchema.OP_CountDecimalPlaces]);
		}

		public void TestImportProductDecimalPlaces_MoreThanTheMaxValueForOP_CountDecimalPlaces()
		{
			Helper.CreateWarehouse("1", "A");
			var client = CreateClientWithNonMandatoryAttributes("C1");
			var product = CreateProductWithAttributes(client, "P1", true);
			Factory.Save();

			var newLoader = OrgSupplierPartDataLoad.New();
			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,DecimalPlaces",
																		$"P1, Product description, UNT, C1, 10", newLoader);

			AssertEquals("Precondition", 4, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("Row 2 excluded... data is inconsistent with required format.", newLoader.Log[1]);
#if NETFRAMEWORK
			AssertEquals("Specified argument was out of the range of valid values.\r\nParameter name: Value of 10 is invalid or too large to store in DecimalPlaces", newLoader.Log[2]);
#else
			AssertEquals("Specified argument was out of the range of valid values. (Parameter 'Value of 10 is invalid or too large to store in DecimalPlaces')", newLoader.Log[2]);
#endif
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", newLoader.Log[3]);

			AssertEquals("Should not update.", (ZByte)0, new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK)[OrgSupplierPartSchema.OP_CountDecimalPlaces]);
		}

		public void TestImportProductDecimalPlaces_MoreThanTheMaxValueForOP_InvalidByteValue()
		{
			Helper.CreateWarehouse("1", "A");
			var client = CreateClientWithNonMandatoryAttributes("C1");
			var product = CreateProductWithAttributes(client, "P1", true);
			Factory.Save();

			var newLoader = OrgSupplierPartDataLoad.New();
			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,DecimalPlaces",
																		$"P1, Product description, UNT, C1, A", newLoader);

			AssertEquals("Precondition", 4, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("Row 2 excluded... data is inconsistent with required format.", newLoader.Log[1]);
#if NETFRAMEWORK
			AssertEquals("Specified argument was out of the range of valid values.\r\nParameter name: Value of A is invalid or too large to store in DecimalPlaces", newLoader.Log[2]);
#else
			AssertEquals("Specified argument was out of the range of valid values. (Parameter 'Value of A is invalid or too large to store in DecimalPlaces')", newLoader.Log[2]);
#endif
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", newLoader.Log[3]);

			AssertEquals("Should not update.", (ZByte)0, new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK)[OrgSupplierPartSchema.OP_CountDecimalPlaces]);
		}

#endregion

		#region TestImportProductClientUQ

		public void TestImportProductClientUQ_CodeTooLong()
		{
			Helper.CreateWarehouse("1", "A");
			var client = CreateClientWithNonMandatoryAttributes("C1");
			var product = CreateProductWithAttributes(client, "P1", true);
			Factory.Save();

			var newLoader = OrgSupplierPartDataLoad.New();

			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,ClientUQ",
																		$"P1, Product description, UNT, C1, ABCD", newLoader);

			var relationInAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product.PK)).Single();
			AssertEquals("Precondition", 3, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", newLoader.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", newLoader.Log[2]);
			AssertEquals("Should update.", "ABC", relationInAnotherFactory.OU_ClientUQ);
		}

		#endregion

		#region TestImportProductOrgPartRelation

		public void TestImportProductIsPartAttrib1ReleaseCaptured_True()
		{
			ImportProductOrgPartRelationCore(column: OrgPartRelationSchema.OU_IsPartAttrib1ReleaseCaptured, csvHeader: "IsPartAttrib1ReleaseCaptured", csvValue: "Y", expectedValue: true);
		}
		public void TestImportProductIsPartAttrib1ReleaseCaptured_False()
		{
			ImportProductOrgPartRelationCore(column: OrgPartRelationSchema.OU_IsPartAttrib1ReleaseCaptured, csvHeader: "IsPartAttrib1ReleaseCaptured", csvValue: "N", expectedValue: false);
		}

		public void TestImportProductIsPartAttrib2ReleaseCaptured_True()
		{
			ImportProductOrgPartRelationCore(column: OrgPartRelationSchema.OU_IsPartAttrib2ReleaseCaptured, csvHeader: "IsPartAttrib2ReleaseCaptured", csvValue: "Y", expectedValue: true);
		}

		public void TestImportProductIsPartAttrib2ReleaseCaptured_False()
		{
			ImportProductOrgPartRelationCore(column: OrgPartRelationSchema.OU_IsPartAttrib2ReleaseCaptured, csvHeader: "IsPartAttrib2ReleaseCaptured", csvValue: "N", expectedValue: false);
		}

		public void TestImportProductIsPartAttrib3ReleaseCaptured_True()
		{
			ImportProductOrgPartRelationCore(column: OrgPartRelationSchema.OU_IsPartAttrib3ReleaseCaptured, csvHeader: "IsPartAttrib3ReleaseCaptured", csvValue: "Y", expectedValue: true);
		}

		public void TestImportProductIsPartAttrib3ReleaseCaptured_False()
		{
			ImportProductOrgPartRelationCore(column: OrgPartRelationSchema.OU_IsPartAttrib3ReleaseCaptured, csvHeader: "IsPartAttrib3ReleaseCaptured", csvValue: "N", expectedValue: false);
		}

		public void TestImportProductIsSerialNumberReleaseCaptured_True()
		{
			ImportProductOrgPartRelationCore(column: OrgPartRelationSchema.OU_IsSerialNumberReleaseCaptured, csvHeader: "IsSerialNumberReleaseCaptured", csvValue: "Y", expectedValue: true);
		}

		public void TestImportProductIsSerialNumberReleaseCaptured_False()
		{
			ImportProductOrgPartRelationCore(column: OrgPartRelationSchema.OU_IsSerialNumberReleaseCaptured, csvHeader: "IsSerialNumberReleaseCaptured", csvValue: "N", expectedValue: false);
		}

		public void TestImportProductUseExpiryDate_True()
		{
			ImportProductOrgPartRelationCore(column: OrgPartRelationSchema.OU_UseExpiryDate, csvHeader: "UseExpiryDate", csvValue: "Y", expectedValue: true);
		}

		public void TestImportProductUseExpiryDate_False()
		{
			ImportProductOrgPartRelationCore(column: OrgPartRelationSchema.OU_UseExpiryDate, csvHeader: "UseExpiryDate", csvValue: "N", expectedValue: false);
		}

		public void TestImportProductUsePackingDate_True()
		{
			ImportProductOrgPartRelationCore(column: OrgPartRelationSchema.OU_UsePackingDate, csvHeader: "UsePackingDate", csvValue: "Y", expectedValue: true);
		}

		public void TestImportProductUsePackingDate_False()
		{
			ImportProductOrgPartRelationCore(column: OrgPartRelationSchema.OU_UsePackingDate, csvHeader: "UsePackingDate", csvValue: "N", expectedValue: false);
		}

		#region TestImportProductUseSerialNumber

		public void TestImportProductUseSerialNumber_True()
		{
			TestImportProductUseSerialNumberCore(csvValue: "Y", initialValue: false, expectedValue: true);
		}

		public void TestImportProductUseSerialNumber_False()
		{
			TestImportProductUseSerialNumberCore(csvValue: "N", initialValue: true, expectedValue: false);
		}

		void TestImportProductUseSerialNumberCore(string csvValue, bool initialValue, bool expectedValue)
		{
			Helper.CreateWarehouse("1", "A");
			var client = CreateClientWithNonMandatoryAttributes("C1");
			client.MiscServ.OM_IMUseSerialNumber = true;

			var product = (OrgSupplierPart)Helper.CreateProduct(client.PK, "P1");
			var relation = product.RelatedOrganisations.FindByOrganisationPKAndRelationship(client.PK, "OWN");
			relation.OU_UseSerialNumber = initialValue;
			Factory.Save();

			var newLoader = OrgSupplierPartDataLoad.New();
			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,Use_SerialNumber",
																		$"P1, Product description, UNT, C1, {csvValue}", newLoader);

			AssertEquals("Precondition", 3, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", newLoader.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", newLoader.Log[2]);

			AssertEquals("OU_UseSerialNumber value is set to expected value.", expectedValue, new BusinessObjectFactory().Load<OrgPartRelation>(product.RelatedOrganisations[0].PK).OU_UseSerialNumber);
		}

		#endregion

		public void TestImportProductTi_True()
		{
			ImportProductOrgPartRelationCore(column: OrgPartRelationSchema.OU_Ti, csvHeader: "Ti", csvValue: "2", expectedValue: (ZShort)2);
		}

		public void TestImportProductTi_False()
		{
			ImportProductOrgPartRelationCore(column: OrgPartRelationSchema.OU_Ti, csvHeader: "Ti", csvValue: "0", expectedValue: ZShort.Zero);
		}

		public void TestImportProductHi_True()
		{
			ImportProductOrgPartRelationCore(column: OrgPartRelationSchema.OU_Hi, csvHeader: "Hi", csvValue: "3", expectedValue: (ZShort)3);
		}

		public void TestImportProductHi_False()
		{
			ImportProductOrgPartRelationCore(column: OrgPartRelationSchema.OU_Hi, csvHeader: "Hi", csvValue: "0", expectedValue: ZShort.Zero);
		}

		public void TestImportProductClientUQ()
		{
			ImportProductOrgPartRelationCore(column: OrgPartRelationSchema.OU_ClientUQ, csvHeader: "ClientUQ", csvValue: "KG", expectedValue: "KG");
		}

		public void TestImportProductUnitPriceCurrency()
		{
			ImportProductOrgPartRelationCore(column: OrgPartRelationSchema.OU_RX_NKUnitPriceCurrency, csvHeader: "UnitPriceCurrency", csvValue: "AUD", expectedValue: "AUD");
		}

		public void TestImportProductUnitInvalidPriceCurrency()
		{
			ImportProductOrgPartRelationCore(column: OrgPartRelationSchema.OU_RX_NKUnitPriceCurrency, csvHeader: "UnitPriceCurrency", csvValue: "XXX", expectedValue: "");
		}

		void ImportProductOrgPartRelationCore<T>(SchemaColumn column, string csvHeader, string csvValue, T expectedValue)
		{
			Helper.CreateWarehouse("1", "A");
			var client = CreateClientWithNonMandatoryAttributes("C1");
			var product = CreateProductWithAttributes(client, "P1", true);
			Factory.Save();

			var newLoader = OrgSupplierPartDataLoad.New();
			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,Use_Attribute1,Use_Attribute2,Use_Attribute3,{csvHeader}",
																		$"P1, Product description, UNT, C1, Y, Y, Y, {csvValue}", newLoader);

			AssertEquals("Precondition", 3, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", newLoader.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", newLoader.Log[2]);

			AssertEquals($"{column.Name} value is not set.", expectedValue, new BusinessObjectFactory().Load<OrgPartRelation>(product.RelatedOrganisations[0].PK)[column]);
		}

		void BuildCsvFileAndImportData(string header, string line, OrgSupplierPartDataLoad loader)
		{
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(header);
					sw.WriteLine(line);
					sw.Flush();
				}
				AssertEquals("Precondition", 0, loader.Log.Count);
				loader.ImportProductData(testFileName.Filename, true, false);
			}
		}

		#endregion

		#region TestImportProduct_Insert

		public void TestImportProductPartDepth_Insert()
		{
			ImportProductOrgPartInsert(column: OrgSupplierPartSchema.OP_Depth, csvHeader: "PartDepth", csvValue: "10", expectedValue: (ZDecimal)10);
		}

		public void TestImportProductPartWidth_Insert()
		{
			ImportProductOrgPartInsert(column: OrgSupplierPartSchema.OP_Width, csvHeader: "PartWidth", csvValue: "12", expectedValue: (ZDecimal)12);
		}

		public void TestImportProductPartHeight_Insert()
		{
			ImportProductOrgPartInsert(column: OrgSupplierPartSchema.OP_Height, csvHeader: "PartHeight", csvValue: "14", expectedValue: (ZDecimal)14);
		}

		public void TestImportProductPartMeasureUQ_Insert()
		{
			ImportProductOrgPartInsert(column: OrgSupplierPartSchema.OP_MeasureUQ, csvHeader: "PartMeasureUQ", csvValue: "KM", expectedValue: "KM");
		}

		public void TestImportProductKeepUpright_Insert()
		{
			ImportProductOrgPartInsert(column: OrgSupplierPartSchema.OP_KeepUpright, csvHeader: "KeepUpright", csvValue: "Y", expectedValue: ZBool.True);
		}

		public void TestImportProductDecimalPlaces_Insert()
		{
			ImportProductOrgPartInsert(column: OrgSupplierPartSchema.OP_CountDecimalPlaces, csvHeader: "DecimalPlaces", csvValue: "2", expectedValue: (ZByte)2);
		}

		void ImportProductOrgPartInsert<T>(SchemaColumn column, string csvHeader, string csvValue, T expectedValue)
		{
			Helper.CreateWarehouse("1", "A");
			CreateClientWithNonMandatoryAttributes("C1");
			Factory.Save();

			var newLoader = OrgSupplierPartDataLoad.New();
			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,{csvHeader}",
																		$"P1, Product description, UNT, C1, {csvValue}", newLoader);

			AssertEquals("Precondition", 2, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("\r\nT O T A L : Products created = 1, Products updated = 0, Products excluded = 0\r\n", newLoader.Log[1]);

			AssertEquals($"{column.Name} value is not set.", expectedValue, new BusinessObjectFactory().Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "P1")).Single()[column]);
		}

		#endregion

		#region TestImportProduct_Update 

		public void TestImportProductKeepUpright_Update()
		{
			ImportProductOrgPartUpdate(column: OrgSupplierPartSchema.OP_KeepUpright, csvHeader: "KeepUpright", csvValue: "Y", expectedValue: ZBool.True);
		}

		public void TestImportProductDecimalPlaces_Update()
		{
			ImportProductOrgPartUpdate(column: OrgSupplierPartSchema.OP_CountDecimalPlaces, csvHeader: "DecimalPlaces", csvValue: "2", expectedValue: (ZByte)2);
		}

		void ImportProductOrgPartUpdate<T>(SchemaColumn column, string csvHeader, string csvValue, T expectedValue)
		{
			Helper.CreateWarehouse("1", "A");
			var client = CreateClientWithNonMandatoryAttributes("C1");
			var product = CreateProductWithAttributes(client, "P1", true);
			Factory.Save();

			var newLoader = OrgSupplierPartDataLoad.New();
			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,{csvHeader}",
																		$"P1, Product description, UNT, C1, {csvValue}", newLoader);

			AssertEquals("Precondition", 3, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", newLoader.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", newLoader.Log[2]);

			AssertEquals($"{column.Name} value is not set.", expectedValue, new BusinessObjectFactory().Load<OrgSupplierPart>(product.PK)[column]);
		}

		#endregion

		#region TestImportProduct_Invalid

		#region TestImportProduct_Invalid_Insert

		public void TestImportProductPartMeasureUQ_Insert_Invalid()
		{
			Helper.CreateWarehouse("1", "A");
			CreateClientWithNonMandatoryAttributes("C1");
			Factory.Save();

			var newLoader = OrgSupplierPartDataLoad.New();
			BuildCsvFileAndImportData("Code,Description,UQ,OWNER,PartMeasureUQ",
																		"P1, Product description, UNT, C1, AA", newLoader);

			AssertEquals("Precondition", 3, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: P1 / Product description  PartMeasureUQ: Enter a valid Dimension UQ.", newLoader.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", newLoader.Log[2]);

			AssertEquals("Product not imported.", false, new BusinessObjectFactory().Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "P1")).Any());
		}

		public void TestImportProductPartMeasureUQ_Insert_Empty()
		{
			Helper.CreateWarehouse("1", "A");
			CreateClientWithNonMandatoryAttributes("C1");
			Factory.Save();

			var newLoader = OrgSupplierPartDataLoad.New();
			BuildCsvFileAndImportData("Code,Description,UQ,OWNER,PartDepth, PartWidth, PartHeight, PartMeasureUQ",
																		"P1, Product description, UNT, C1, 1,2,3,", newLoader);

			AssertEquals("Precondition", 3, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("Line 2: PART NO/DESC: P1 / Product description  PartMeasureUQ: Measurement UQ is required.", newLoader.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", newLoader.Log[2]);

			AssertEquals("Product not imported.", false, new BusinessObjectFactory().Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "P1")).Any());
		}

		#endregion

		#region TestImportProduct_Invalid_update

		public void TestImportProductPartMeasureUQ_Update_Invalid()
		{
			ImportProductOrgPartUpdateWithWarning(column: OrgSupplierPartSchema.OP_MeasureUQ, csvHeader: "PartMeasureUQ", csvValue: "AA", expectedValue: "", expectedLog: "Line 2: PART NO/DESC: P1 / Product description  PartMeasureUQ: Enter a valid Dimension UQ.");
		}

		public void TestImportProductPartMeasureUQ_Update_Empty()
		{
			ImportProductOrgPartUpdateWithWarning(column: OrgSupplierPartSchema.OP_MeasureUQ, csvHeader: "PartDepth, PartWidth, PartHeight, PartMeasureUQ", csvValue: "1,2,3,", expectedValue: "", expectedLog: "Line 2: PART NO/DESC: P1 / Product description  PartMeasureUQ: Measurement UQ is required.");
		}

		void ImportProductOrgPartUpdateWithWarning<T>(SchemaColumn column, string csvHeader, string csvValue, T expectedValue, string expectedLog)
		{
			Helper.CreateWarehouse("1", "A");
			var client = CreateClientWithNonMandatoryAttributes("C1");
			var product = CreateProductWithAttributes(client, "P1", true);
			Factory.Save();

			AssertEquals("Precondition", ZString.Empty, (ZString)product[column]);

			var newLoader = OrgSupplierPartDataLoad.New();
			BuildCsvFileAndImportData($"Code,Description,UQ,OWNER,{csvHeader}",
																		$"P1, Product description, UNT, C1, {csvValue}", newLoader);

			AssertEquals("Precondition", 3, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals(expectedLog, newLoader.Log[1]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", newLoader.Log[2]);

			var newFactory = new BusinessObjectFactory();
			var productInNewFactory = newFactory.Load<OrgSupplierPart>(product.PK);
			AssertNotNull(productInNewFactory);
			AssertEquals("Column did not change.", ZString.Empty, (ZString)productInNewFactory[column]);
		}

		public void TestImportProductWithInvalidData_MultipleLinesForSameProductWithOneInvalidUpdate()
		{
			Helper.CreateWarehouse("1", "A");
			var client = CreateClientWithNonMandatoryAttributes("C1");
			var product = CreateProductWithAttributes(client, "P1", true);
			Factory.Save();

			AssertEquals("Precondition", ZString.Empty, product.OP_MeasureUQ);

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,PartMeasureUQ");
					sw.WriteLine("P1, Product description, UNT, C1, M");
					sw.WriteLine("P1, Product description, UNT, C1, AA");
					sw.Flush();
				}

				var loader = GetNewDataLoader();
				loader.ImportProductData(testFileName.Filename, true, false);

				AssertEquals("Precondition", 4, loader.Log.Count);
				AssertEquals("Products to Import = 2", loader.Log[0]);
				AssertEquals("Line 3: PART NO/DESC: P1 / Product description  PartMeasureUQ: Enter a valid Dimension UQ.", loader.Log[2]);
				AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 1\r\n", loader.Log[3]);
			}

			var newFactory = new BusinessObjectFactory();
			var productInNewFactory = newFactory.Load<OrgSupplierPart>(product.PK);
			AssertNotNull(productInNewFactory);
			AssertEquals("Column changed.", "M", productInNewFactory.OP_MeasureUQ);
		}

		public void TestImportProductWithInvalidData_UpdateFromRelatedObjects_PartBarcode()
		{
			Helper.CreateWarehouse("1", "A");
			var client = CreateClientWithNonMandatoryAttributes("C1");
			var product = CreateProductWithAttributes(client, "P1", true);
			Factory.Save();

			AssertEquals("Precondition", ZString.Empty, product.OP_MeasureUQ);
			AssertEquals("Precondition", 0, product.PartBarcodes.Count);
			AssertEquals("Precondition", false, Factory.Load<OrgSupplierPartBarcode>(new ZQuery()).Any());

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,PartMeasureUQ,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package,Barcode3,Barcode3_Package,Barcode4,Barcode4_Package,Barcode5,Barcode5_Package,Barcode1_UseForDocuments");
					sw.WriteLine("P1, Product description, UNT, C1, AA, Barcode11,UNT,Barcode21,DRM,Barcode31,CTN,Barcode41,BOX,Barcode51,PLT,Y");
					sw.Flush();
				}

				var loader = GetNewDataLoader();
				loader.ImportProductData(testFileName.Filename, true, false);

				AssertEquals("Precondition", 3, loader.Log.Count);
				AssertEquals("Products to Import = 1", loader.Log[0]);
				AssertEquals("Line 2: PART NO/DESC: P1 / Product description  PartMeasureUQ: Enter a valid Dimension UQ.", loader.Log[1]);
				AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", loader.Log[2]);
			}

			var newFactory = new BusinessObjectFactory();
			var productInNewFactory = newFactory.Load<OrgSupplierPart>(product.PK);
			AssertNotNull(productInNewFactory);
			AssertEquals("Column did not change.", ZString.Empty, product.OP_MeasureUQ);
			AssertEquals("No part barcodes added.", 0, productInNewFactory.PartBarcodes.Count);
			AssertEquals("No part barcodes created.", false, Factory.Load<OrgSupplierPartBarcode>(new ZQuery()).Any());
		}

		public void TestImportProductWithInvalidData_UpdateFromRelatedObjects_PartRelation()
		{
			Helper.CreateWarehouse("1", "A");
			var client1 = CreateClientWithNonMandatoryAttributes("C1");
			CreateClientWithNonMandatoryAttributes("C2");
			var product = CreateProductWithAttributes(client1, "P1", true);
			Factory.Save();

			AssertEquals("Precondition", ZString.Empty, product.OP_MeasureUQ);
			AssertEquals("Precondition", 1, product.RelatedOrganisations.Count);
			AssertEquals("Precondition", 1, Factory.Load<OrgPartRelation>(new ZQuery()).Length);

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,PartMeasureUQ,Supplier");
					sw.WriteLine("P1, Product description, UNT, C1, AA, C2");
					sw.Flush();
				}

				var loader = GetNewDataLoader();
				loader.ImportProductData(testFileName.Filename, true, false);

				AssertEquals("Precondition", 3, loader.Log.Count);
				AssertEquals("Products to Import = 1", loader.Log[0]);
				AssertEquals("Line 2: PART NO/DESC: P1 / Product description  PartMeasureUQ: Enter a valid Dimension UQ.", loader.Log[1]);
				AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", loader.Log[2]);
			}

			var newFactory = new BusinessObjectFactory();
			var productInNewFactory = newFactory.Load<OrgSupplierPart>(product.PK);
			AssertNotNull(productInNewFactory);
			AssertEquals("Column did not change.", ZString.Empty, product.OP_MeasureUQ);
			AssertEquals("No part relations added.", 1, productInNewFactory.RelatedOrganisations.Count);
			AssertEquals("No part relations created.", 1, Factory.Load<OrgPartRelation>(new ZQuery()).Length);
		}

		public void TestImportProductWithInvalidData_UpdateFromRelatedObjects_UnitConversions()
		{
			Helper.CreateWarehouse("1", "A");
			var client1 = CreateClientWithNonMandatoryAttributes("C1");
			var product = CreateProductWithAttributes(client1, "P1", true);
			Factory.Save();

			AssertEquals("Precondition", ZString.Empty, product.OP_MeasureUQ);
			AssertEquals("Precondition", 3, product.PartUnits.Count);
			AssertEquals("Precondition", 3, Factory.Load<OrgPartUnit>(new ZQuery()).Length);
			AssertEquals("Precondition", "KG", product.OP_WeightUQ);
			AssertEquals("Precondition", "M3", product.OP_CubicUQ);
			AssertNotNull("Precondition", product.PartUnits.Cast<OrgPartUnit>().Single(partUnit => partUnit.OF_ParentPackType == "CTN"));

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,PartMeasureUQ,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC1_Cubic,UC1_Depth,UC1_Height,UC1_Width,UC1_Weight,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC3_Cubic,UC3_Depth,UC3_Height,UC3_Width,UC3_Weight");
					sw.WriteLine("P1,Product description,UNT,C1,AA,2,CTN,BOX,1.1,2.2,3.3,4.4,5.5,3.5,LA,L,.01,.02,.03,.04,.05");
					sw.Flush();
				}

				var loader = GetNewDataLoader();
				loader.ImportProductData(testFileName.Filename, true, false);

				CombineAssertions(() =>
				{
					AssertEquals("Precondition", 3, loader.Log.Count);
					AssertEquals("Products to Import = 1", loader.Log[0]);
					AssertEquals("Line 2: PART NO/DESC: P1 / Product description  PartMeasureUQ: Enter a valid Dimension UQ.", loader.Log[1]);
					AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", loader.Log[2]);
				});
			}

			var newFactory = new BusinessObjectFactory();
			var productInNewFactory = newFactory.Load<OrgSupplierPart>(product.PK);
			AssertNotNull(productInNewFactory);
			AssertEquals("Column did not change.", ZString.Empty, product.OP_MeasureUQ);
			AssertEquals("No part units added.", 3, productInNewFactory.PartUnits.Count);
			AssertEquals("No part units created.", 3, Factory.Load<OrgPartUnit>(new ZQuery()).Length);
			AssertEquals("No changes on weight UQ.", "KG", product.OP_WeightUQ);
			AssertEquals("No changes on cubic UQ.", "M3", product.OP_CubicUQ);
		}

		public void TestImportProductWithInvalidData_UpdateFromRelatedObjects_UNDG()
		{
			Helper.CreateWarehouse("1", "A");
			var client = CreateClientWithNonMandatoryAttributes("C1");
			var product = CreateProductWithAttributes(client, "P1", true);
			var filter = new ZQuery(UNDGSubstanceSchema.DG_UNNO, "1009");
			filter.AddToFilter(UNDGSubstanceSchema.DG_Variant, "A");
			var testUNDG = Factory.LoadTop1<UNDGSubstance>(filter);
			Factory.Save();

			AssertEquals("Precondition", ZString.Empty, product.OP_MeasureUQ);
			AssertEquals("Precondition", 0, product.UNDGs.Count);
			AssertEquals("Precondition", false, Factory.Load<UNDGDataItem>(new ZQuery()).Any());
			AssertNotNull("Precondition", testUNDG);

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,PartMeasureUQ,UNDG_Code");
					sw.WriteLine("P1, Product description, UNT, C1, AA, 1009a");
					sw.Flush();
				}

				var loader = GetNewDataLoader();
				loader.ImportProductData(testFileName.Filename, true, false);

				AssertEquals("Precondition", 3, loader.Log.Count);
				AssertEquals("Products to Import = 1", loader.Log[0]);
				AssertEquals("Line 2: PART NO/DESC: P1 / Product description  PartMeasureUQ: Enter a valid Dimension UQ.", loader.Log[1]);
				AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 0, Products excluded = 1\r\n", loader.Log[2]);
			}

			var newFactory = new BusinessObjectFactory();
			var productInNewFactory = newFactory.Load<OrgSupplierPart>(product.PK);
			AssertNotNull(productInNewFactory);
			AssertEquals("Column did not change.", ZString.Empty, product.OP_MeasureUQ);
			AssertEquals("No undg added.", 0, productInNewFactory.UNDGs.Count);
			AssertEquals("No undg created.", false, Factory.Load<UNDGDataItem>(new ZQuery()).Any());
		}

		#endregion

		#endregion

		#region TestLoadUnitConversions

		public void TestLoadUnitConversions()
		{
			var org = CreateClient("org", 1, "A1");
			Factory.Save();
			using (var csvFile = TempFile.New())
			{
				using (var sw = new StreamWriter(csvFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Unit_Weight,Unit_NetWeight,Weight_Unit,Unit_Volume,Volume_Unit,Department,Division,QtyInStock,Origin,Last_Cost,UNDG_Code,LocalPartNumber,LocalPartDescription,Use_Attribute1,Use_Attribute2,Use_Attribute3,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Commodity,BrandName,Model,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package,Barcode3,Barcode3_Package,Barcode4,Barcode4_Package,Barcode5,Barcode5_Package,UC1_Cubic,UC1_Depth,UC1_Height,UC1_Width,UC1_Weight,UC2_Cubic,UC2_Depth,UC2_Height,UC2_Width,UC2_Weight");
					sw.WriteLine($"1,P1,BAG,{org.OH_Code},,55  ,,LB,,,,,,,40 ,,,,,,,1,UNT,BAG,20,BAG,PLT,,,,,,,,,,GEN,,,,,,,,,,,,,1.1,2.1,3.1,4.1,5.1,,,,,");
					sw.WriteLine($"2,P2,BBG,{org.OH_Code},,1100,,LB,,,,,,,671,,,,,,,1,UNT,BBG,40,BBG,PLT,,,,,,,,,,GEN,,,,,,,,,,,,,,,,,,1.2,2.2,3.2,4.2,5.2");
				}
				GetNewDataLoader().ImportProductData(csvFile.Filename, true, false);
				Factory.Save();
			}

			var product = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "1")).Single();
			AssertEquals(string.Join("\r\n", product.PartUnits.Cast<OrgPartUnit>().Select(p => $"{p.OF_QuantityInParent} x {p.OF_PackType} in {p.OF_ParentPackType}")), 3, product.PartUnits.Count);
			AssertEquals("BAG", product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 1m).OF_ParentPackType);
			AssertEquals("PLT", product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 20m).OF_ParentPackType);
			AssertEquals("LB", product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 55m).OF_PackType);
			AssertEquals(1.1m, product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 1m).OF_Cubic);
			AssertEquals(2.1m, product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 1m).OF_Depth);
			AssertEquals(3.1m, product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 1m).OF_Height);
			AssertEquals(4.1m, product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 1m).OF_Width);
			AssertEquals(5.1m, product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 1m).OF_Weight);

			product = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "2")).Single();
			AssertEquals(string.Join("\r\n", product.PartUnits.Cast<OrgPartUnit>().Select(p => $"{p.OF_QuantityInParent} x {p.OF_PackType} in {p.OF_ParentPackType}")), 3, product.PartUnits.Count);
			AssertEquals("BBG", product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 1m).OF_ParentPackType);
			AssertEquals("PLT", product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 40m).OF_ParentPackType);
			AssertEquals("LB", product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 1100m).OF_PackType);
			AssertEquals(1.2m, product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 40m).OF_Cubic);
			AssertEquals(2.2m, product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 40m).OF_Depth);
			AssertEquals(3.2m, product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 40m).OF_Height);
			AssertEquals(4.2m, product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 40m).OF_Width);
			AssertEquals(5.2m, product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 40m).OF_Weight);
		}

		#endregion

		#region TestLoadBarCode

		public void TestLoadBarCode()
		{
			var org = CreateClient("org", 1, "A1");
			Factory.Save();
			using (var csvFile = TempFile.New())
			{
				using (var sw = new StreamWriter(csvFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Unit_Weight,Unit_NetWeight,Weight_Unit,Unit_Volume,Volume_Unit,Department,Division,QtyInStock,Origin,Last_Cost,UNDG_Code,LocalPartNumber,LocalPartDescription,Use_Attribute1,Use_Attribute2,Use_Attribute3,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Commodity,BrandName,Model,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package,Barcode3,Barcode3_Package,Barcode4,Barcode4_Package,Barcode5,Barcode5_Package");
					sw.WriteLine($"1,P1,BAG,{org.OH_Code},,55  ,,LB,,,,,,,40 ,,,,,,,1,UNT,BAG,2,DRM,BAG,3,CTN,BAG,4,BOX,BAG,5,PLT,BAG,GEN,,,Barcode11,UNT,Barcode21,DRM,Barcode31,CTN,Barcode41,BOX,Barcode51,PLT");
					sw.WriteLine($"2,P2,BBG,{org.OH_Code},,1100,,LB,,,,,,,671,,,,,,,1,UNT,BBG,2,DRM,BBG,3,CTN,BBG,4,BOX,BBG,5,PLT,BBG,GEN,,,Barcode12,UNT,Barcode22,DRM,Barcode32,CTN,Barcode42,BOX,Barcode52,PLT");
				}
				GetNewDataLoader().ImportProductData(csvFile.Filename, true, false);
				Factory.Save();
			}

			var product = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "1")).Single();
			AssertEquals(5, product.PartBarcodes.Count);
			AssertEquals("Barcode11", product.PartBarcodes.FindPartBarcode("UNT", "Barcode11").PH_Barcode);
			AssertEquals("Barcode21", product.PartBarcodes.FindPartBarcode("DRM", "Barcode21").PH_Barcode);
			AssertEquals("Barcode31", product.PartBarcodes.FindPartBarcode("CTN", "Barcode31").PH_Barcode);
			AssertEquals("Barcode41", product.PartBarcodes.FindPartBarcode("BOX", "Barcode41").PH_Barcode);
			AssertEquals("Barcode51", product.PartBarcodes.FindPartBarcode("PLT", "Barcode51").PH_Barcode);

			product = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "2")).Single();
			AssertEquals(5, product.PartBarcodes.Count);
			AssertEquals("Barcode12", product.PartBarcodes.FindPartBarcode("UNT", "Barcode12").PH_Barcode);
			AssertEquals("Barcode22", product.PartBarcodes.FindPartBarcode("DRM", "Barcode22").PH_Barcode);
			AssertEquals("Barcode32", product.PartBarcodes.FindPartBarcode("CTN", "Barcode32").PH_Barcode);
			AssertEquals("Barcode42", product.PartBarcodes.FindPartBarcode("BOX", "Barcode42").PH_Barcode);
			AssertEquals("Barcode52", product.PartBarcodes.FindPartBarcode("PLT", "Barcode52").PH_Barcode);
		}

		public void TestLoadBarCode_LongBarcode()
		{
			var org = CreateClient("org", 1, "A1");
			Factory.Save();

			var loader = GetNewDataLoader();
			var logs = new List<string>();
			loader.LogUpdated += (s, e) => logs.Add(e.LogMessage);

			using (var csvFile = TempFile.New())
			{
				using (var sw = new StreamWriter(csvFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Unit_Weight,Unit_NetWeight,Weight_Unit,Unit_Volume,Volume_Unit,Department,Division,QtyInStock,Origin,Last_Cost,UNDG_Code,LocalPartNumber,LocalPartDescription,Use_Attribute1,Use_Attribute2,Use_Attribute3,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Commodity,BrandName,Model,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package");
					sw.WriteLine($"1,P1,BAG,{org.OH_Code},,55  ,,LB,,,,,,,40 ,,,,,,,1,UNT,BAG,2,DRM,BAG,,,,,,,,,,GEN,,,Barcode_ABCDEFGHIJKLMNOPQRSTUVWX,UNT,Barcode_123ABCDEFGHIJKLMNOPQRSTUVWX,DRM");
				}
				loader.ImportProductData(csvFile.Filename, true, false);
				Factory.Save();
			}

			var product = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "1")).Single();
			AssertEquals(2, product.PartBarcodes.Count);
			AssertEquals("Should support up to the max length of barcode", "Barcode_ABCDEFGHIJKLMNOPQRSTUVWX", product.PartBarcodes.Cast<OrgSupplierPartBarcode>().Single(b => b.PH_F3_NKPackType.EqualsIgnoringCase("UNT")).PH_Barcode);
			AssertEquals("Should truncate long barcodes", "Barcode_123ABCDEFGHIJKLMNOPQRSTU", product.PartBarcodes.Cast<OrgSupplierPartBarcode>().Single(b => b.PH_F3_NKPackType.EqualsIgnoringCase("DRM")).PH_Barcode);

			AssertEquals("Part barcode 'Barcode_123ABCDEFGHIJKLMNOPQRSTUVWX' is too long. Storing 'Barcode_123ABCDEFGHIJKLMNOPQRSTU' instead.", loader.Log[1]);
			AssertEquals("Part barcode 'Barcode_123ABCDEFGHIJKLMNOPQRSTUVWX' is too long. Storing 'Barcode_123ABCDEFGHIJKLMNOPQRSTU' instead.", logs[1]);
		}

		public void TestLoadBarCode_MatchesExistingBarcode_CaseInsensitive()
		{
			var org = CreateClient("org", 1, "A1");
			var product = CreateProductWithAttributes(org, "P1", true);
			var barcode = product.PartBarcodes.AddNew();
			barcode.PH_F3_NKPackType = "UNT";
			barcode.PH_Barcode = "Barcode";
			Factory.Save();

			var loader = GetNewDataLoader();
			var logs = new List<string>();
			loader.LogUpdated += (s, e) => logs.Add(e.LogMessage);

			using (var csvFile = TempFile.New())
			{
				using (var sw = new StreamWriter(csvFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Unit_Weight,Unit_NetWeight,Weight_Unit,Unit_Volume,Volume_Unit,Department,Division,QtyInStock,Origin,Last_Cost,UNDG_Code,LocalPartNumber,LocalPartDescription,Use_Attribute1,Use_Attribute2,Use_Attribute3,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Commodity,BrandName,Model,Barcode1,Barcode1_Package");
					sw.WriteLine($"P1,P1,BAG,{org.OH_Code},,55  ,,LB,,,,,,,40 ,,,,,,,1,UNT,BAG,20,BAG,PLT,,,,,,,,,,GEN,,,BaRCode,UnT");
				}
				loader.ImportProductData(csvFile.Filename, true, false);
				Factory.Save();
			}

			AssertEquals(1, product.PartBarcodes.Count);
			AssertEquals("Barcode", product.PartBarcodes.Cast<OrgSupplierPartBarcode>().Single(b => b.PH_F3_NKPackType.EqualsIgnoringCase("UNT")).PH_Barcode);
		}

		public void TestLoadBarCode_LongBarcode_MatchesExistingBarcode()
		{
			var org = CreateClient("org", 1, "A1");
			var product = CreateProductWithAttributes(org, "P1", true);
			var barcode = product.PartBarcodes.AddNew();
			barcode.PH_F3_NKPackType = "UNT";
			barcode.PH_Barcode = "Barcode_123ABCDEFGHIJKLMNOPQRSTU";
			Factory.Save();

			var loader = GetNewDataLoader();
			var logs = new List<string>();
			loader.LogUpdated += (s, e) => logs.Add(e.LogMessage);

			using (var csvFile = TempFile.New())
			{
				using (var sw = new StreamWriter(csvFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Unit_Weight,Unit_NetWeight,Weight_Unit,Unit_Volume,Volume_Unit,Department,Division,QtyInStock,Origin,Last_Cost,UNDG_Code,LocalPartNumber,LocalPartDescription,Use_Attribute1,Use_Attribute2,Use_Attribute3,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Commodity,BrandName,Model,Barcode1,Barcode1_Package");
					sw.WriteLine($"P1,P1,BAG,{org.OH_Code},,55  ,,LB,,,,,,,40 ,,,,,,,1,UNT,BAG,20,BAG,PLT,,,,,,,,,,GEN,,,Barcode_123ABCDEFGHIJKLMNOPQRSTUVWX,UNT");
				}
				loader.ImportProductData(csvFile.Filename, true, false);
				Factory.Save();
			}

			AssertEquals(1, product.PartBarcodes.Count);
			AssertEquals("Should truncate and match the barcode", "Barcode_123ABCDEFGHIJKLMNOPQRSTU", product.PartBarcodes.Cast<OrgSupplierPartBarcode>().Single(b => b.PH_F3_NKPackType.EqualsIgnoringCase("UNT")).PH_Barcode);

			AssertEquals("Part barcode 'Barcode_123ABCDEFGHIJKLMNOPQRSTUVWX' is too long. Storing 'Barcode_123ABCDEFGHIJKLMNOPQRSTU' instead.", loader.Log[1]);
			AssertEquals("Part barcode 'Barcode_123ABCDEFGHIJKLMNOPQRSTUVWX' is too long. Storing 'Barcode_123ABCDEFGHIJKLMNOPQRSTU' instead.", logs[1]);
		}

		public void TestLoadBarCode_LongBarcode_DuplicateBarcode()
		{
			var org = CreateClient("org", 1, "A1");
			var product = CreateProductWithAttributes(org, "P1", true);
			var barcode = product.PartBarcodes.AddNew();
			barcode.PH_F3_NKPackType = "UNT";
			barcode.PH_Barcode = "Barcode_123ABCDEFGHIJKLMNOPQRSTU";
			Factory.Save();

			var loader = GetNewDataLoader();
			var logs = new List<string>();
			loader.LogUpdated += (s, e) => logs.Add(e.LogMessage);

			using (var csvFile = TempFile.New())
			{
				using (var sw = new StreamWriter(csvFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Unit_Weight,Unit_NetWeight,Weight_Unit,Unit_Volume,Volume_Unit,Department,Division,QtyInStock,Origin,Last_Cost,UNDG_Code,LocalPartNumber,LocalPartDescription,Use_Attribute1,Use_Attribute2,Use_Attribute3,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Commodity,BrandName,Model,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package");
					sw.WriteLine($"P1,P1,BAG,{org.OH_Code},,55  ,,LB,,,,,,,40 ,,,,,,,1,UNT,BAG,20,BAG,PLT,,,,,,,,,,GEN,,,Barcode_123ABCDEFGHIJKLMNOPQRSTUVWX,UNT");
				}
				loader.ImportProductData(csvFile.Filename, true, false);
				Factory.Save();
			}

			AssertEquals(1, product.PartBarcodes.Count);
			AssertEquals("Should truncate and match the barcode", "Barcode_123ABCDEFGHIJKLMNOPQRSTU", product.PartBarcodes.Cast<OrgSupplierPartBarcode>().Single(b => b.PH_F3_NKPackType.EqualsIgnoringCase("UNT")).PH_Barcode);

			AssertEquals("Part barcode 'Barcode_123ABCDEFGHIJKLMNOPQRSTUVWX' is too long. Storing 'Barcode_123ABCDEFGHIJKLMNOPQRSTU' instead.", loader.Log[2]);
			AssertEquals("Part barcode 'Barcode_123ABCDEFGHIJKLMNOPQRSTUVWX' is too long. Storing 'Barcode_123ABCDEFGHIJKLMNOPQRSTU' instead.", logs[2]);
		}

		public void TestLoadBarCode_DuplicateBarcode()
		{
			var org = CreateClient("org", 1, "A1");
			var product = CreateProductWithAttributes(org, "P1", true);
			var barcode = product.PartBarcodes.AddNew();
			barcode.PH_F3_NKPackType = "UNT";
			barcode.PH_Barcode = "BRC";
			Factory.Save();

			var loader = GetNewDataLoader();
			var logs = new List<string>();
			loader.LogUpdated += (s, e) => logs.Add(e.LogMessage);

			using (var csvFile = TempFile.New())
			{
				using (var sw = new StreamWriter(csvFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Unit_Weight,Unit_NetWeight,Weight_Unit,Unit_Volume,Volume_Unit,Department,Division,QtyInStock,Origin,Last_Cost,UNDG_Code,LocalPartNumber,LocalPartDescription,Use_Attribute1,Use_Attribute2,Use_Attribute3,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Commodity,BrandName,Model,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package");
					sw.WriteLine($"P1,P1,BAG,{org.OH_Code},,55  ,,LB,,,,,,,40 ,,,,,,,1,UNT,BAG,20,BAG,PLT,,,,,,,,,,GEN,,,BRC,UNT,BRC,DRM");
				}
				loader.ImportProductData(csvFile.Filename, true, false);
				Factory.Save();
			}

			AssertEquals(1, product.PartBarcodes.Count);
			AssertEquals("BRC", product.PartBarcodes[0].PH_Barcode);
			AssertEquals("UNT", product.PartBarcodes[0].PH_F3_NKPackType);

			AssertEquals("Part barcode 'BRC' already exists with package type 'UNT', skipping import of this barcode with the pack type 'DRM'.", loader.Log[1]);
			AssertEquals("Part barcode 'BRC' already exists with package type 'UNT', skipping import of this barcode with the pack type 'DRM'.", logs[1]);
		}

		#endregion

		#region TestImportUseForDocuments

		public void TestImportUseForDocuments()
		{
			var org = CreateClient("org", 1, "A1");
			Factory.Save();
			var newLoader = GetNewDataLoader();
			using (var csvFile = TempFile.New())
			{
				using (var sw = new StreamWriter(csvFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Unit_Weight,Unit_NetWeight,Weight_Unit,Unit_Volume,Volume_Unit,Department,Division,QtyInStock,Origin,Last_Cost,UNDG_Code,LocalPartNumber,LocalPartDescription,Use_Attribute1,Use_Attribute2,Use_Attribute3,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Commodity,BrandName,Model,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package,Barcode3,Barcode3_Package,Barcode4,Barcode4_Package,Barcode5,Barcode5_Package,Barcode1_UseForDocuments,Barcode2_UseForDocuments,Barcode3_UseForDocuments,Barcode4_UseForDocuments,Barcode5_UseForDocuments");
					sw.WriteLine($"1,P1,UNT,{org.OH_Code},,55  ,,LB,,,,,,,40 ,,,,,,,1,UNT,UNT,20,UNT,PLT,,,,,,,,,,GEN,,,Barcode11,UNT,Barcode21,UNT,Barcode31,UNT,Barcode41,UNT,Barcode51,UNT,Y,N,N,N,N");
				}
				newLoader.ImportProductData(csvFile.Filename, true, false);
				Factory.Save();
			}
			var product = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "1")).Single();
			AssertEquals(5, product.PartBarcodes.Count);
			AssertEquals(true, product.PartBarcodes.FindPartBarcode("UNT", "Barcode11").PH_UseForDocuments);
			AssertEquals(false, product.PartBarcodes.FindPartBarcode("UNT", "Barcode21").PH_UseForDocuments);
			AssertEquals(false, product.PartBarcodes.FindPartBarcode("UNT", "Barcode31").PH_UseForDocuments);
			AssertEquals(false, product.PartBarcodes.FindPartBarcode("UNT", "Barcode41").PH_UseForDocuments);
			AssertEquals(false, product.PartBarcodes.FindPartBarcode("UNT", "Barcode51").PH_UseForDocuments);
		}

		public void TestImportUseForDocuments_UseForDocumentsIsNotPresent()
		{
			var org = CreateClient("org", 1, "A1");
			Factory.Save();
			using (var csvFile = TempFile.New())
			{
				using (var sw = new StreamWriter(csvFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Unit_Weight,Unit_NetWeight,Weight_Unit,Unit_Volume,Volume_Unit,Department,Division,QtyInStock,Origin,Last_Cost,UNDG_Code,LocalPartNumber,LocalPartDescription,Use_Attribute1,Use_Attribute2,Use_Attribute3,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Commodity,BrandName,Model,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package,Barcode3,Barcode3_Package,Barcode4,Barcode4_Package,Barcode5,Barcode5_Package,Barcode1_UseForDocuments,Barcode3_UseForDocuments,Barcode5_UseForDocuments");
					sw.WriteLine($"1,P1,UNT,{org.OH_Code},,55  ,,LB,,,,,,,40 ,,,,,,,1,UNT,UNT,20,BAG,PLT,,,,,,,,,,GEN,,,Barcode11,UNT,Barcode21,UNT,Barcode31,UNT,Barcode41,UNT,Barcode51,UNT,,Y,N");
				}
				GetNewDataLoader().ImportProductData(csvFile.Filename, true, false);
				Factory.Save();
			}

			var product = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "1")).Single();
			AssertEquals(5, product.PartBarcodes.Count);
			AssertEquals(false, product.PartBarcodes.FindPartBarcode("UNT", "Barcode11").PH_UseForDocuments);
			AssertEquals(false, product.PartBarcodes.FindPartBarcode("UNT", "Barcode21").PH_UseForDocuments);
			AssertEquals(true, product.PartBarcodes.FindPartBarcode("UNT", "Barcode31").PH_UseForDocuments);
			AssertEquals(false, product.PartBarcodes.FindPartBarcode("UNT", "Barcode41").PH_UseForDocuments);
			AssertEquals(false, product.PartBarcodes.FindPartBarcode("UNT", "Barcode51").PH_UseForDocuments);
		}

		public void TestImportUseForDocuments_SwapBetweenTwoBarcodes()
		{
			var org = CreateClient("org", 1, "A1");
			var productInDB = CreateProductWithAttributes(org, "P1", true);
			var barcode1 = productInDB.PartBarcodes.AddNew();
			barcode1.PH_F3_NKPackType = "UNT";
			barcode1.PH_Barcode = "Barcode11";
			barcode1.PH_UseForDocuments = true;
			var barcode2 = productInDB.PartBarcodes.AddNew();
			barcode2.PH_F3_NKPackType = "UNT";
			barcode2.PH_Barcode = "Barcode21";
			barcode2.PH_UseForDocuments = false;
			Factory.Save();
			AssertEquals("Precondition:", true, barcode1.PH_UseForDocuments);
			AssertEquals("Precondition:", false, barcode2.PH_UseForDocuments);
			var newLoader = GetNewDataLoader();
			using (var csvFile = TempFile.New())
			{
				using (var sw = new StreamWriter(csvFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Unit_Weight,Unit_NetWeight,Weight_Unit,Unit_Volume,Volume_Unit,Department,Division,QtyInStock,Origin,Last_Cost,UNDG_Code,LocalPartNumber,LocalPartDescription,Use_Attribute1,Use_Attribute2,Use_Attribute3,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Commodity,BrandName,Model,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package,Barcode1_UseForDocuments,Barcode2_UseForDocuments");
					sw.WriteLine($"P1,P1 Description,UNT,{org.OH_Code},,13,,LB,,,,,,,40 ,,,,,,,1,UNT,UNT,20,UNT,PLT,,,,,,,,,,GEN,,,Barcode11,UNT,Barcode21,UNT,N,Y");
				}
				newLoader.ImportProductData(csvFile.Filename, true, false);
				Factory.Save();
			}

			var newFactory = new BusinessObjectFactory();
			var product = newFactory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "P1")).Single();
			AssertEquals(2, product.PartBarcodes.Count);
			AssertEquals("Should swap the flag between barcode1 and barcode2", false, product.PartBarcodes.FindPartBarcode("UNT", "Barcode11").PH_UseForDocuments);
			AssertEquals("Should swap the flag between barcode1 and barcode2", true, product.PartBarcodes.FindPartBarcode("UNT", "Barcode21").PH_UseForDocuments);
		}

		public void TestImportUseForDocuments_Validate_PackTypeIsNotStockUnit()
		{
			var org = CreateClient("org", 1, "A1");
			var productInDB = CreateProductWithAttributes(org, "P1", true);
			var bagUnit = productInDB.PartUnits.AddNew();
			bagUnit.OF_ParentPackType = "UNT";
			bagUnit.OF_PackType = "BAG";
			bagUnit.OF_QuantityInParent = 2m;
			Factory.Save();

			var newLoader = GetNewDataLoader();
			using (var csvFile = TempFile.New())
			{
				using (var sw = new StreamWriter(csvFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Unit_Weight,Unit_NetWeight,Weight_Unit,Unit_Volume,Volume_Unit,Department,Division,QtyInStock,Origin,Last_Cost,UNDG_Code,LocalPartNumber,LocalPartDescription,Use_Attribute1,Use_Attribute2,Use_Attribute3,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Commodity,BrandName,Model,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package,Barcode1_UseForDocuments,Barcode2_UseForDocuments");
					sw.WriteLine($"P1,P1 Description,UNT,{org.OH_Code},,13,,LB,,,,,,,40 ,,,,,,,1,UNT,UNT,20,UNT,PLT,,,,,,,,,,GEN,,,Barcode1,BAG,Barcode2,UNT,Y,Y");
				}
				newLoader.ImportProductData(csvFile.Filename, true, false);
				Factory.Save();
			}
			AssertEquals(4, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("Part barcode 'Barcode1' cannot set Use for Documents as its pack type 'BAG' is not the Stock Unit, skipping import of this barcode.", newLoader.Log[1]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", newLoader.Log[2]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", newLoader.Log[3]);

			var newFactory = new BusinessObjectFactory();
			var product = newFactory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "P1")).Single();
			AssertEquals(1, product.PartBarcodes.Count);
			AssertEquals("Should ONLY import barcode2", true, product.PartBarcodes.FindPartBarcode("UNT", "Barcode2").PH_UseForDocuments);
		}

		public void TestImportUseForDocuments_Validate_Update_PackTypeIsNotStockUnit()
		{
			var org = CreateClient("org", 1, "A1");
			var productInDB = CreateProductWithAttributes(org, "P1", true);
			var bagUnit = productInDB.PartUnits.AddNew();
			bagUnit.OF_ParentPackType = "UNT";
			bagUnit.OF_PackType = "BAG";
			bagUnit.OF_QuantityInParent = 2m;
			var barcode1 = productInDB.PartBarcodes.AddNew();
			barcode1.PH_F3_NKPackType = "BAG";
			barcode1.PH_Barcode = "Barcode11";
			barcode1.PH_UseForDocuments = false;
			var barcode2 = productInDB.PartBarcodes.AddNew();
			barcode2.PH_F3_NKPackType = "UNT";
			barcode2.PH_Barcode = "Barcode21";
			barcode2.PH_UseForDocuments = true;
			Factory.Save();
			AssertEquals("Precondition:", false, barcode1.PH_UseForDocuments);
			AssertEquals("Precondition:", true, barcode2.PH_UseForDocuments);
			var newLoader = GetNewDataLoader();
			using (var csvFile = TempFile.New())
			{
				using (var sw = new StreamWriter(csvFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Unit_Weight,Unit_NetWeight,Weight_Unit,Unit_Volume,Volume_Unit,Department,Division,QtyInStock,Origin,Last_Cost,UNDG_Code,LocalPartNumber,LocalPartDescription,Use_Attribute1,Use_Attribute2,Use_Attribute3,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Commodity,BrandName,Model,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package,Barcode1_UseForDocuments,Barcode2_UseForDocuments");
					sw.WriteLine($"P1,P1 Description,UNT,{org.OH_Code},,13,,LB,,,,,,,40 ,,,,,,,1,UNT,UNT,20,UNT,PLT,,,,,,,,,,GEN,,,Barcode11,BAG,Barcode21,UNT,Y,N");
				}
				newLoader.ImportProductData(csvFile.Filename, true, false);
				Factory.Save();
			}
			AssertEquals(4, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("Validation failed for Part barcodes: Use for Documents cannot be set for Barcodes where the Pack Type is not the Stock Unit. Skipping import of barcodes section.", newLoader.Log[1]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", newLoader.Log[2]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", newLoader.Log[3]);

			var newFactory = new BusinessObjectFactory();
			var product = newFactory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "P1")).Single();
			AssertEquals(2, product.PartBarcodes.Count);
			AssertEquals("Should NOT swap the flag between barcode1 and barcode2", false, product.PartBarcodes.FindPartBarcode("BAG", "Barcode11").PH_UseForDocuments);
			AssertEquals("Should NOT swap the flag between barcode1 and barcode2", true, product.PartBarcodes.FindPartBarcode("UNT", "Barcode21").PH_UseForDocuments);
		}

		public void TestImportUseForDocuments_Validate_MoreThanOneUseForDocumentsIsSet()
		{
			var org = CreateClient("org", 1, "A1");
			var productInDB = CreateProductWithAttributes(org, "P1", true);
			Factory.Save();
			var newLoader = GetNewDataLoader();
			using (var csvFile = TempFile.New())
			{
				using (var sw = new StreamWriter(csvFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Unit_Weight,Unit_NetWeight,Weight_Unit,Unit_Volume,Volume_Unit,Department,Division,QtyInStock,Origin,Last_Cost,UNDG_Code,LocalPartNumber,LocalPartDescription,Use_Attribute1,Use_Attribute2,Use_Attribute3,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Commodity,BrandName,Model,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package,Barcode1_UseForDocuments,Barcode2_UseForDocuments");
					sw.WriteLine($"P1,Product Description,UNT,{org.OH_Code},,13,,LB,,,,,,,40 ,,,,,,,1,UNT,UNT,20,UNT,PLT,,,,,,,,,,GEN,,,Barcode11,UNT,Barcode21,UNT,Y,Y");
				}
				newLoader.ImportProductData(csvFile.Filename, true, false);
				Factory.Save();
			}
			AssertEquals(4, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("Validation failed for Part barcodes: Another barcode for the Stock Unit already exists where Use for Documents is set, only one barcode can have this value set. Skipping import of barcodes section.", newLoader.Log[1]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", newLoader.Log[2]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", newLoader.Log[3]);

			var newFactory = new BusinessObjectFactory();
			var product = newFactory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "P1")).Single();
			AssertEquals(0, product.PartBarcodes.Count);
		}

		public void TestImportUseForDocuments_Validate_Update_MoreThanOneUseForDocumentsIsSet()
		{
			var org = CreateClient("org", 1, "A1");
			var productInDB = CreateProductWithAttributes(org, "P1", true);
			var barcode1 = productInDB.PartBarcodes.AddNew();
			barcode1.PH_F3_NKPackType = "UNT";
			barcode1.PH_Barcode = "Barcode11";
			barcode1.PH_UseForDocuments = false;
			var barcode2 = productInDB.PartBarcodes.AddNew();
			barcode2.PH_F3_NKPackType = "UNT";
			barcode2.PH_Barcode = "Barcode21";
			barcode2.PH_UseForDocuments = true;
			Factory.Save();
			AssertEquals("Precondition:", false, barcode1.PH_UseForDocuments);
			AssertEquals("Precondition:", true, barcode2.PH_UseForDocuments);
			var newLoader = GetNewDataLoader();
			using (var csvFile = TempFile.New())
			{
				using (var sw = new StreamWriter(csvFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Unit_Weight,Unit_NetWeight,Weight_Unit,Unit_Volume,Volume_Unit,Department,Division,QtyInStock,Origin,Last_Cost,UNDG_Code,LocalPartNumber,LocalPartDescription,Use_Attribute1,Use_Attribute2,Use_Attribute3,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Commodity,BrandName,Model,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package,Barcode1_UseForDocuments,Barcode2_UseForDocuments");
					sw.WriteLine($"P1,Product Description,UNT,{org.OH_Code},,13,,LB,,,,,,,40 ,,,,,,,1,UNT,UNT,20,UNT,PLT,,,,,,,,,,GEN,,,Barcode11,UNT,Barcode21,UNT,Y,Y");
				}
				newLoader.ImportProductData(csvFile.Filename, true, false);
				Factory.Save();
			}
			AssertEquals(4, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("Validation failed for Part barcodes: Another barcode for the Stock Unit already exists where Use for Documents is set, only one barcode can have this value set. Skipping import of barcodes section.", newLoader.Log[1]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", newLoader.Log[2]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", newLoader.Log[3]);

			var newFactory = new BusinessObjectFactory();
			var product = newFactory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "P1")).Single();
			AssertEquals(2, product.PartBarcodes.Count);
			AssertEquals("Should NOT change flag on barcode1", false, product.PartBarcodes.FindPartBarcode("UNT", "Barcode11").PH_UseForDocuments);
			AssertEquals("Should NOT change flag on barcode2", true, product.PartBarcodes.FindPartBarcode("UNT", "Barcode21").PH_UseForDocuments);
		}

		public void TestImportUseForDocuments_Validate_PackTypeIsStockUnitButNoneUseForDocumentsIsSet()
		{
			Helper.CreateWarehouse("1", "A");
			var client = CreateClientWithNonMandatoryAttributes("C1");
			Factory.Save();

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,OWNER,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package,Barcode1_UseForDocuments,Barcode2_UseForDocuments");
					sw.WriteLine("P1, Product description, UNT, C1, Barcode11,UNT,Barcode21,UNT,N,N");
					sw.Flush();
				}

				var newLoader = GetNewDataLoader();
				newLoader.ImportProductData(testFileName.Filename, true, false);

				AssertEquals(3, newLoader.Log.Count);
				AssertEquals("Products to Import = 1", newLoader.Log[0]);
				AssertEquals("Validation failed for Part barcodes: Another barcode for the Stock Unit already exists where Use for Documents is set, only one barcode can have this value set. Skipping import of barcodes section.", newLoader.Log[1]);
				AssertEquals("\r\nT O T A L : Products created = 1, Products updated = 0, Products excluded = 0\r\n", newLoader.Log[2]);
			}
		}

		public void TestImportUseForDocuments_Validate_PackTypeIsNotConvertible()
		{
			var org = CreateClient("org", 1, "A1");
			var productInDB = CreateProductWithAttributes(org, "P1", true);
			var palletUnit = productInDB.PartUnits.AddNew();
			palletUnit.OF_ParentPackType = "UNT";
			palletUnit.OF_PackType = "PLT";
			palletUnit.OF_QuantityInParent = 4m;
			Factory.Save();

			var newLoader = GetNewDataLoader();
			using (var csvFile = TempFile.New())
			{
				using (var sw = new StreamWriter(csvFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Unit_Weight,Unit_NetWeight,Weight_Unit,Unit_Volume,Volume_Unit,Department,Division,QtyInStock,Origin,Last_Cost,UNDG_Code,LocalPartNumber,LocalPartDescription,Use_Attribute1,Use_Attribute2,Use_Attribute3,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Commodity,BrandName,Model,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package,Barcode3,Barcode3_Package,Barcode1_UseForDocuments,Barcode2_UseForDocuments,Barcode3_UseForDocuments");
					sw.WriteLine($"P1,P1 Description,UNT,{org.OH_Code},,13,,LB,,,,,,,40 ,,,,,,,1,UNT,UNT,20,UNT,PLT,,,,,,,,,,GEN,,,Barcode1,BAG,Barcode2,PLT,Barcode3,UNT,Y,N,Y");
				}
				newLoader.ImportProductData(csvFile.Filename, true, false);
				Factory.Save();
			}
			AssertEquals(4, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("Part barcode 'Barcode1' cannot find conversion to Stock Unit from pack type 'BAG', skipping import of this barcode.", newLoader.Log[1]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", newLoader.Log[2]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", newLoader.Log[3]);

			var newFactory = new BusinessObjectFactory();
			var product = newFactory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "P1")).Single();
			AssertEquals(2, product.PartBarcodes.Count);
			AssertEquals("Should import barcode2", false, product.PartBarcodes.FindPartBarcode("PLT", "Barcode2").PH_UseForDocuments);
			AssertEquals("Should import barcode3", true, product.PartBarcodes.FindPartBarcode("UNT", "Barcode3").PH_UseForDocuments);
		}

		public void TestImportUseForDocuments_Validate_PackTypeDoesNotMatchesStockUnit()
		{
			var org = CreateClient("org", 1, "A1");
			var productInDB = CreateProductWithAttributes(org, "P1", true);
			var kiloUnit = productInDB.PartUnits.AddNew();
			kiloUnit.OF_ParentPackType = "UNT";
			kiloUnit.OF_PackType = "KG";
			kiloUnit.OF_QuantityInParent = 2m;

			var cubicUnit = productInDB.PartUnits.AddNew();
			cubicUnit.OF_ParentPackType = "UNT";
			cubicUnit.OF_PackType = "M3";
			cubicUnit.OF_QuantityInParent = 3m;

			var palletUnit = productInDB.PartUnits.AddNew();
			palletUnit.OF_ParentPackType = "UNT";
			palletUnit.OF_PackType = "PLT";
			palletUnit.OF_QuantityInParent = 4m;

			Factory.Save();

			var newLoader = GetNewDataLoader();
			using (var csvFile = TempFile.New())
			{
				using (var sw = new StreamWriter(csvFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Unit_Weight,Unit_NetWeight,Weight_Unit,Unit_Volume,Volume_Unit,Department,Division,QtyInStock,Origin,Last_Cost,UNDG_Code,LocalPartNumber,LocalPartDescription,Use_Attribute1,Use_Attribute2,Use_Attribute3,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Commodity,BrandName,Model,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package,Barcode3,Barcode3_Package,Barcode4,Barcode4_Package,Barcode1_UseForDocuments,Barcode2_UseForDocuments,Barcode3_UseForDocuments,Barcode4_UseForDocuments");
					sw.WriteLine($"P1,P1 Description,UNT,{org.OH_Code},,13,,LB,,,,,,,40 ,,,,,,,1,UNT,UNT,20,UNT,PLT,,,,,,,,,,GEN,,,Barcode1,KG,Barcode2,M3,Barcode3,PLT,Barcode4,UNT,N,N,N,Y");
				}
				newLoader.ImportProductData(csvFile.Filename, true, false);
				Factory.Save();
			}
			AssertEquals(5, newLoader.Log.Count);
			AssertEquals("Products to Import = 1", newLoader.Log[0]);
			AssertEquals("Part barcode 'Barcode1's pack type 'KG' is a weight or volume unit but it doesn't match the Stock Unit, skipping import of this barcode.", newLoader.Log[1]);
			AssertEquals("Part barcode 'Barcode2's pack type 'M3' is a weight or volume unit but it doesn't match the Stock Unit, skipping import of this barcode.", newLoader.Log[2]);
			AssertEquals("PART NO: P1 - Part has been UPDATED", newLoader.Log[3]);
			AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 1, Products excluded = 0\r\n", newLoader.Log[4]);

			var newFactory = new BusinessObjectFactory();
			var product = newFactory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "P1")).Single();
			AssertEquals(2, product.PartBarcodes.Count);
			AssertEquals("Should import barcode3", false, product.PartBarcodes.FindPartBarcode("PLT", "Barcode3").PH_UseForDocuments);
			AssertEquals("Should import barcode4", true, product.PartBarcodes.FindPartBarcode("UNT", "Barcode4").PH_UseForDocuments);
		}

		#endregion

		#region TestImportDuplicate

		public void TestImportDuplicate_NoUpdate()
		{
			var owner1 = Factory.NewWithValidTestData<OrgHeader>();
			owner1.OH_Code = "OWNER1";
			var owner2 = Factory.NewWithValidTestData<OrgHeader>();
			owner2.OH_Code = "OWNER2";
			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_Code = "SUPPLIER1";
			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			supplier2.OH_Code = "SUPPLIER2";
			var supplier3 = Factory.NewWithValidTestData<OrgHeader>();
			supplier3.OH_Code = "SUPPLIER3";

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "DUP-TEST";
			part1.OP_Desc = "Original";
			part1.RelatedOrganisations.AddOwner(owner1);
			part1.RelatedOrganisations.AddSupplier(supplier1);

			Factory.Save();

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier");
					sw.WriteLine("DUP-TEST,Duplicate,UNT,OWNER1,SUPPLIER2");
					sw.WriteLine("DUP-TEST,New,UNT,OWNER2,SUPPLIER2");
					sw.WriteLine("DUP-TEST,Duplicate of New,UNT,OWNER2,SUPPLIER3");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, false, false);

				string log = string.Join("\r\n", new TypedEnumerable<string>(Loader.Log));
				AssertContains("Line 2: PART NO/DESC: DUP-TEST / Duplicate  Duplicate Product detected: Owner = OWNER1, Supplier = SUPPLIER2", log);
				AssertContains("Line 4: PART NO/DESC: DUP-TEST / Duplicate of New  Duplicate Product detected: Owner = OWNER2, Supplier = SUPPLIER3", log);
				AssertContains("T O T A L : Products created = 1, Products updated = 0, Products excluded = 2", log);

				var parts = NewFactory().Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "DUP-TEST"));
				AssertEquals(
					string.Join("\r\n",
						"New OWN OWNER2",
						"New SUP SUPPLIER2",
						"Original OWN OWNER1",
						"Original SUP SUPPLIER1"
					),
					string.Join("\r\n", parts.SelectMany(p => p.RelatedOrganisations.Cast<OrgPartRelation>()).Select(r => $"{r.SupplierPart.OP_Desc} {r.OU_Relationship} {r.Organisation.OH_Code}").OrderBy(r => r))
				);
			}
		}

		public void TestImportDuplicate_Update()
		{
			var owner1 = Factory.NewWithValidTestData<OrgHeader>();
			owner1.OH_Code = "OWNER1";
			var owner2 = Factory.NewWithValidTestData<OrgHeader>();
			owner2.OH_Code = "OWNER2";
			var owner3 = Factory.NewWithValidTestData<OrgHeader>();
			owner3.OH_Code = "OWNER3";
			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_Code = "SUPPLIER1";
			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			supplier2.OH_Code = "SUPPLIER2";

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "DUP-TEST";
			part1.OP_Desc = "Original1";
			part1.RelatedOrganisations.AddOwner(owner1);
			part1.RelatedOrganisations.AddSupplier(supplier1);

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "DUP-TEST";
			part2.OP_Desc = "Original2";
			part2.RelatedOrganisations.AddOwner(owner2);
			part2.RelatedOrganisations.AddSupplier(supplier2);

			Factory.Save();

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier");
					sw.WriteLine("DUP-TEST,Duplicate,UNT,OWNER2;OWNER1,SUPPLIER2");
					sw.WriteLine("DUP-TEST,New,UNT,OWNER3,SUPPLIER1");
					sw.WriteLine("DUP-TEST,Updated,UNT,OWNER1,SUPPLIER1");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, true, false);

				string log = string.Join("\r\n", new TypedEnumerable<string>(Loader.Log));
				AssertContains("Line 2: PART NO/DESC: DUP-TEST / Duplicate  Duplicate Product detected: Owner = OWNER1, Supplier = SUPPLIER2", log);
				AssertContains("T O T A L : Products created = 1, Products updated = 1, Products excluded = 1", log);

				var parts = NewFactory().Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "DUP-TEST"));
				AssertEquals(
					string.Join("\r\n",
						"New OWN OWNER3",
						"New SUP SUPPLIER1",
						"Original2 OWN OWNER2",
						"Original2 SUP SUPPLIER2",
						"Updated OWN OWNER1",
						"Updated SUP SUPPLIER1"
					),
					string.Join("\r\n", parts.SelectMany(p => p.RelatedOrganisations.Cast<OrgPartRelation>()).Select(r => $"{r.SupplierPart.OP_Desc} {r.OU_Relationship} {r.Organisation.OH_Code}").OrderBy(r => r))
				);
			}
		}

		public void TestImportDuplicate_Reactivate()
		{
			var owner1 = Factory.NewWithValidTestData<OrgHeader>();
			owner1.OH_Code = "OWNER1";
			var owner2 = Factory.NewWithValidTestData<OrgHeader>();
			owner2.OH_Code = "OWNER2";
			var owner3 = Factory.NewWithValidTestData<OrgHeader>();
			owner3.OH_Code = "OWNER3";
			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_Code = "SUPPLIER1";
			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			supplier2.OH_Code = "SUPPLIER2";

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "DUP-TEST";
			part1.OP_Desc = "Original1";
			part1.RelatedOrganisations.AddOwner(owner1);
			part1.RelatedOrganisations.AddSupplier(supplier1);

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "DUP-TEST";
			part2.OP_Desc = "Original2";
			part2.RelatedOrganisations.AddOwner(owner1);
			part2.RelatedOrganisations.AddOwner(owner2);
			part2.RelatedOrganisations.AddSupplier(supplier2);
			part2.OP_IsActive = false;

			var part3 = Factory.New<OrgSupplierPart>();
			part3.OP_PartNum = "DUP-TEST";
			part3.OP_Desc = "Original3";
			part3.RelatedOrganisations.AddOwner(owner3);
			part3.RelatedOrganisations.AddSupplier(supplier1);
			part3.OP_IsActive = false;

			Factory.Save();

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier");
					sw.WriteLine("DUP-TEST,Inacative Duplicate,UNT,OWNER2;SUPPLIER2");
					sw.WriteLine("DUP-TEST,Reactivated,UNT,OWNER3,SUPPLIER1");
					sw.Flush();
				}

				Loader.ImportProductData(testFileName.Filename, true, false);

				string log = string.Join("\r\n", new TypedEnumerable<string>(Loader.Log));
				AssertContains("Line 2: PART NO/DESC: DUP-TEST / Inacative Duplicate  Duplicate Product detected: Owner = OWNER1, Supplier = SUPPLIER2", log);
				AssertContains("Line 3: PART NO/DESC: DUP-TEST / Reactivated  Owner [OWNER3] Supplier [SUPPLIER1]: Inactive Part has been re-activated.", log);
				AssertContains("T O T A L : Products created = 0, Products updated = 1, Products excluded = 1", log);

				var parts = NewFactory().Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "DUP-TEST"));
				AssertEquals(
					string.Join("\r\n",
						"Original1 Y OWN OWNER1",
						"Original1 Y SUP SUPPLIER1",
						"Original2 N OWN OWNER1",
						"Original2 N OWN OWNER2",
						"Original2 N SUP SUPPLIER2",
						"Reactivated Y OWN OWNER3",
						"Reactivated Y SUP SUPPLIER1"
					),
					string.Join("\r\n", parts.SelectMany(p => p.RelatedOrganisations.Cast<OrgPartRelation>()).Select(r =>
						$"{r.SupplierPart.OP_Desc} {r.SupplierPart.OP_IsActive} {r.OU_Relationship} {r.Organisation.OH_Code}"
					).OrderBy(r => r))
				);
			}
		}

		#endregion

		#region TestDisposables

		public void TestDisposables()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(string.Format("BrandName, Model"));
					sw.WriteLine(string.Format("TBrand,TModel"));
					sw.Flush();
				}
				var load = new TestOrgSupplierPartDataLoadDataLoad();
				load.ImportData(testFileName.Filename, "TestingType");
				AssertEquals(1, load.RunCounters.RecsCreated);
			}
		}

		class TestOrgSupplierPartDataLoadDataLoad : OrgSupplierPartDataLoad
		{
			public override string CSVTemplateHeading
			{
				get { return "BrandName, Model"; }
			}

			protected override OrgSupplierPart ProcessPartData(PartsDataToLoad partData)
			{
				partData.HasNoValidOwnerOrSupplier = false;
				partData.HasInvalidClassificationType = false;
				var part = base.ProcessPartData(partData);
				var suspender = part.SetterSuspender;
				TestCase.Assert(suspender.IsSetterSuspended("OP_Brand"));
				TestCase.Assert(suspender.IsSetterSuspended("OP_Model"));
				return part;
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			SetupCountry();
			Loader = OrgSupplierPartDataLoad.New();
		}

		protected override OrgSupplierPartDataLoad GetNewDataLoader()
		{
			return OrgSupplierPartDataLoad.New();
		}

		void SetupCountry()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
		}

		OrgSupplierPartDataLoad Loader;
		PartsDataToLoad TestDataToLoad;
		OrgSupplierPart NewEnterprisePart;

		protected void ClearCustomsRecordsBeforeTesting()
		{
			TestCaseHelper.ClearTable("CusClassPartPivot");
			TestCaseHelper.ClearTable("CusClassification");
			TestCaseHelper.ClearTable("OrgPartRelation");
			TestCaseHelper.ClearTable("OrgSupplierPart");
		}

		protected OrgSupplierPart LoadPart(ZString lookupPart)
		{
			ZQuery partFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, lookupPart);
			OrgSupplierPart enterprisePart = Factory.LoadTop1<OrgSupplierPart>(partFilter);
			AssertNotNull("Expecting Part " + lookupPart + " to be found", enterprisePart);
			return enterprisePart;
		}

		protected IBaseCusClassPartPivot[] LoadPivots(OrgSupplierPart lookupPart)
		{
			var query = new ZQuery(CusClassPartPivotSchema.CI_OP, lookupPart.PK);
			return Factory.Load<IBaseCusClassPartPivot>(query);
		}

		void AssertCorrectNumberOfPartsNowExist(int partsExpected)
		{
			AssertEquals("Part records expected to now exist in DB", partsExpected, Factory.GetDatabaseCount(typeof(OrgSupplierPart)));
		}

		public void TestRelationshipRrocessing()
		{
			OrgHeader testOrganisation1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			OrgHeader testOrganisation2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			NewEnterprisePart = Factory.New<OrgSupplierPart>();
			NewEnterprisePart.OP_PartNum = "PARTNUM";

			AssertCorrectOrgPartRelationshipProcessing(null, "", testOrganisation1, null, testOrganisation1, "OWN", null, "", null, "");
			AssertCorrectOrgPartRelationshipProcessing(null, "", null, testOrganisation1, testOrganisation1, "SUP", null, "", null, "");
			AssertCorrectOrgPartRelationshipProcessing(null, "", testOrganisation1, testOrganisation2, testOrganisation1, "OWN", testOrganisation2, "SUP", null, "");
			AssertCorrectOrgPartRelationshipProcessing(null, "", testOrganisation1, testOrganisation1, testOrganisation1, "BTH", null, "", null, "");
			AssertCorrectOrgPartRelationshipProcessing(null, "", testOrganisation2, testOrganisation2, testOrganisation2, "BTH", null, "", null, "");
			AssertCorrectOrgPartRelationshipProcessing(null, "", testOrganisation2, testOrganisation1, testOrganisation2, "OWN", testOrganisation1, "SUP", null, "");

			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "OWN", testOrganisation1, null, testOrganisation1, "OWN", null, "", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "OWN", null, testOrganisation1, testOrganisation1, "BTH", null, "", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "OWN", testOrganisation2, null, testOrganisation1, "OWN", testOrganisation2, "OWN", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "OWN", null, testOrganisation2, testOrganisation1, "OWN", testOrganisation2, "SUP", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "OWN", testOrganisation1, testOrganisation2, testOrganisation1, "OWN", testOrganisation2, "SUP", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "OWN", testOrganisation1, testOrganisation1, testOrganisation1, "BTH", null, "", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "OWN", testOrganisation2, testOrganisation2, testOrganisation1, "OWN", testOrganisation2, "BTH", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "OWN", testOrganisation2, testOrganisation1, testOrganisation1, "BTH", testOrganisation2, "OWN", null, "");

			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "SUP", testOrganisation1, null, testOrganisation1, "BTH", null, "", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "SUP", null, testOrganisation1, testOrganisation1, "SUP", null, "", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "SUP", testOrganisation2, null, testOrganisation1, "SUP", testOrganisation2, "OWN", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "SUP", null, testOrganisation2, testOrganisation1, "SUP", testOrganisation2, "SUP", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "SUP", testOrganisation1, testOrganisation2, testOrganisation1, "BTH", testOrganisation2, "SUP", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "SUP", testOrganisation1, testOrganisation1, testOrganisation1, "BTH", null, "", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "SUP", testOrganisation2, testOrganisation2, testOrganisation1, "SUP", testOrganisation2, "BTH", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "SUP", testOrganisation2, testOrganisation1, testOrganisation1, "SUP", testOrganisation2, "OWN", null, "");

			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "BTH", testOrganisation1, null, testOrganisation1, "BTH", null, "", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "BTH", null, testOrganisation1, testOrganisation1, "BTH", null, "", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "BTH", testOrganisation2, null, testOrganisation1, "BTH", testOrganisation2, "OWN", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "BTH", null, testOrganisation2, testOrganisation1, "BTH", testOrganisation2, "SUP", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "BTH", testOrganisation1, testOrganisation2, testOrganisation1, "BTH", testOrganisation2, "SUP", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "BTH", testOrganisation1, testOrganisation1, testOrganisation1, "BTH", null, "", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "BTH", testOrganisation2, testOrganisation2, testOrganisation1, "BTH", testOrganisation2, "BTH", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "BTH", testOrganisation2, testOrganisation1, testOrganisation1, "BTH", testOrganisation2, "OWN", null, "");

			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "XXX", testOrganisation1, null, testOrganisation1, "XXX", testOrganisation1, "OWN", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "XXX", null, testOrganisation1, testOrganisation1, "XXX", testOrganisation1, "SUP", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "XXX", testOrganisation2, null, testOrganisation1, "XXX", testOrganisation2, "OWN", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "XXX", null, testOrganisation2, testOrganisation1, "XXX", testOrganisation2, "SUP", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "XXX", testOrganisation1, testOrganisation2, testOrganisation1, "XXX", testOrganisation1, "OWN", testOrganisation2, "SUP");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "XXX", testOrganisation1, testOrganisation1, testOrganisation1, "XXX", testOrganisation1, "BTH", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "XXX", testOrganisation2, testOrganisation2, testOrganisation1, "XXX", testOrganisation2, "BTH", null, "");
			AssertCorrectOrgPartRelationshipProcessing(testOrganisation1, "XXX", testOrganisation2, testOrganisation1, testOrganisation1, "XXX", testOrganisation1, "SUP", testOrganisation2, "OWN");
		}

		void AssertCorrectOrgPartRelationshipProcessing(OrgHeader existingOrg, ZString existingRelationship, OrgHeader newOwner, OrgHeader newSupplier, OrgHeader expectedOrg1, ZString expectedRelationship1, OrgHeader expectedOrg2, ZString expectedRelationship2, OrgHeader expectedOrg3, ZString expectedRelationship3)
		{
			foreach (OrgPartRelation relationship in NewEnterprisePart.RelatedOrganisations.ToArray())
			{
				relationship.Delete();
			}
			if (existingOrg != null)
			{
				OrgPartRelation relation1 = NewEnterprisePart.RelatedOrganisations.AddNew();
				relation1.OU_OH = existingOrg.PK;
				relation1.OU_Relationship = existingRelationship;
			}
			Factory.Save();

			TestDataToLoad = new PartsDataToLoad();
			TestDataToLoad.PartOwnerCodePK = new List<ZGuid>();
			if (newOwner != null)
			{
				TestDataToLoad.PartOwnerCodePK.Add(newOwner.PK);
			}

			TestDataToLoad.PartSupplierCodePK = new List<ZGuid>();
			if (newSupplier != null)
			{
				TestDataToLoad.PartSupplierCodePK.Add(newSupplier.PK);
			}

			Loader.UpdateOrganisationRelationships(NewEnterprisePart, TestDataToLoad);
			int relationshipCount = 0;
			if (expectedOrg1 != null)
			{
				relationshipCount++;
			}

			if (expectedOrg2 != null)
			{
				relationshipCount++;
			}

			if (expectedOrg3 != null)
			{
				relationshipCount++;
			}

			Dictionary<ZString, bool> foundRelationships = new Dictionary<ZString, bool>();
			foreach (OrgPartRelation relationship in NewEnterprisePart.RelatedOrganisations)
			{
				Assert("Duplicated Organisation+Relationship found", !foundRelationships.ContainsKey(relationship.Organisation.PK.ToString() + relationship.OU_Relationship));
				foundRelationships.Add(relationship.Organisation.PK.ToString() + relationship.OU_Relationship, true);
			}
			AssertEquals("Correct number of relationships found", relationshipCount, foundRelationships.Count);
			if (expectedOrg1 != null)
			{
				Assert("Organisation with correct relationship found", foundRelationships.ContainsKey(expectedOrg1.PK.ToString() + expectedRelationship1));
			}
			if (expectedOrg2 != null)
			{
				Assert("Organisation with correct relationship found", foundRelationships.ContainsKey(expectedOrg2.PK.ToString() + expectedRelationship2));
			}
			if (expectedOrg3 != null)
			{
				Assert("Organisation with correct relationship found", foundRelationships.ContainsKey(expectedOrg3.PK.ToString() + expectedRelationship3));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodingConvention", "WTG1007:DoNotCompareBoolLiterals", Justification = "Assertion for testing")]
		void AssertOrgPartRelationAttributes(OrgPartRelation relation, bool useAttr1, bool useAttr2, bool useAttr3, bool isAttr1Readonly, bool isAttr2Readonly, bool isAttr3Readonly)
		{
			AssertEquals(string.Format("For product {0} use part attribute1 should {1}be readonly", relation.SupplierPart.OP_PartNum, isAttr1Readonly ? "" : "not "), isAttr1Readonly, relation.OU_UsePartAttrib1Info.ReadOnly);
			AssertEquals(string.Format("For product {0} Use part attribute1 should be {1}", relation.SupplierPart.OP_PartNum, useAttr1.ToString()), useAttr1, relation.OU_UsePartAttrib1);
			AssertEquals(string.Format("For product {0} Use part attribute2 should {1}be readonly", relation.SupplierPart.OP_PartNum, isAttr2Readonly ? "" : "not "), isAttr2Readonly, relation.OU_UsePartAttrib2Info.ReadOnly);
			AssertEquals(string.Format("For product {0} Use part attribute2 should be {1}", relation.SupplierPart.OP_PartNum, useAttr2.ToString()), useAttr2, relation.OU_UsePartAttrib2);
			AssertEquals(string.Format("For product {0} Use part attribute3 should {1}be readonly", relation.SupplierPart.OP_PartNum, isAttr3Readonly ? "" : "not "), isAttr3Readonly, relation.OU_UsePartAttrib3Info.ReadOnly);
			AssertEquals(string.Format("For product {0} Use part attribute3 should be {1}", relation.SupplierPart.OP_PartNum, useAttr3.ToString()), useAttr3, relation.OU_UsePartAttrib3);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodingConvention", "WTG1007:DoNotCompareBoolLiterals", Justification = "Assertion for testing")]
		void AssertOrgPartRelationSerialAttribute(OrgPartRelation relation, bool useSerial, bool isSerialReadonly)
		{
			AssertEquals(string.Format("For product {0} use serial number should {1}be readonly", relation.SupplierPart.OP_PartNum, isSerialReadonly ? "" : "not "), isSerialReadonly, relation.OU_UseSerialNumberInfo.ReadOnly);
			AssertEquals(string.Format("For product {0} Use serial number should be {1}", relation.SupplierPart.OP_PartNum, useSerial.ToString()), useSerial, relation.OU_UseSerialNumber);
		}

		OrgHeader CreateClientUsingSerialNumber(string clientCode)
		{
			var client = Factory.Load<OrgHeader>(Helper.CreateClient(clientCode));
			client.MiscServ.OM_IMUseSerialNumber = true;
			return client;
		}

		OrgHeader CreateClient(string clientCode, int attributeNumber, string attributeName = "", string attributeType = PartAttributeTypeList.Codes.Mandatory)
		{
			var client = Factory.Load<OrgHeader>(Helper.CreateClient(clientCode));
			SetClientAttributeAndName(client, attributeNumber, attributeName, attributeType);
			return client;
		}

		void SetClientAttributeAndName(OrgHeader client, int attributeNumber, string attributeName, string attributeType = PartAttributeTypeList.Codes.Mandatory)
		{
			switch (attributeNumber)
			{
				case 1:
					client.MiscServ.OM_IMPartAttrib1Type = attributeType;
					client.MiscServ.OM_IMPartAttrib1Name = attributeName;
					break;
				case 2:
					client.MiscServ.OM_IMPartAttrib2Type = attributeType;
					client.MiscServ.OM_IMPartAttrib2Name = attributeName;
					break;
				case 3:
					client.MiscServ.OM_IMPartAttrib3Type = attributeType;
					client.MiscServ.OM_IMPartAttrib3Name = attributeName;
					break;
			}
		}

		OrgSupplierPart CreateProduct(OrgHeader client, string productCode, int attributeNumber)
		{
			var product = (OrgSupplierPart)Helper.CreateProduct(client.PK, productCode);
			var relation = product.RelatedOrganisations.FindByOrganisationPKAndRelationship(client.PK, "OWN");

			switch (attributeNumber)
			{
				case 1:
					relation.OU_UsePartAttrib1 = true;
					break;
				case 2:
					relation.OU_UsePartAttrib2 = true;
					break;
				case 3:
					relation.OU_UsePartAttrib3 = true;
					break;
			}

			return product;
		}

		OrgSupplierPart CreateProductWithSerial(OrgHeader client, string productCode)
		{
			var product = (OrgSupplierPart)Helper.CreateProduct(client.PK, productCode);
			var relation = product.RelatedOrganisations.FindByOrganisationPKAndRelationship(client.PK, "OWN");

			relation.OU_UseSerialNumber = true;

			return product;
		}

		OrgPartRelation GetOrgPartRelationByOrganisationCode(OrgPartRelationCollection orgs, string code)
		{
			foreach (OrgPartRelation rel in orgs)
			{
				if (rel.Organisation.OH_Code == code)
				{
					return rel;
				}
			}
			return null;
		}

		IWhsTransactionTestHelper Helper
		{
			get { return helper ?? (helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory)); }
		}
		IWhsTransactionTestHelper helper;

		#endregion
	}
}
