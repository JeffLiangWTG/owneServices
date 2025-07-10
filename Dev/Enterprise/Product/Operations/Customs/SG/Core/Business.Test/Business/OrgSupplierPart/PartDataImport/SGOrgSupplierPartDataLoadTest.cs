using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(SGOrgSupplierPartDataLoad))]
	public class SGOrgSupplierPartDataLoadTest : DataLoadTestCase<SGOrgSupplierPartDataLoad>
	{
		public void TestOrgSupplierPartDataLoadType()
		{
			var testLoader = Customs.Business.OrgSupplierPartDataLoad.New();
			AssertEquals("OrgSupplierPartDataLoad Type", typeof(SGOrgSupplierPartDataLoad), testLoader.GetType());
		}

		public void TestPartIsNotCreatedIfLookupInAnotherCountry()
		{
			var testLookup = CreateClassification("Test Lookup", "");
			testLookup.CC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			Factory.Save();
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("P1234-4848X,RUBBER GASKET,KG," + testLookup.CC_LookupCode + ",HTB,," + testOrganisation.OH_Code + ",12,KG,100,M3,,0");
					sw.Flush();
				}

				var testLoader = Customs.Business.OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, false, false);
				var checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				var enterprisePartsCreated = Factory.Load<OrgSupplierPart>(checkFilter);
				AssertEquals("Part should have been created in this scenario.", 1, enterprisePartsCreated.Length);
				AssertEquals("Pivot should not have been created in this scenario.", 0, enterprisePartsCreated[0].PivotsForBinding.Count);
			}
		}

		public void TestLoadCreatesCusClassPartPivot()
		{
			var testLookup = CreateClassification("Test Lookup", "");
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("P1234-4848X,RUBBER GASKET,KG," + testLookup.CC_LookupCode + ",HTB,," + testOrganisation.OH_Code + ",12,KG,100,M3,,0");
					sw.Flush();
				}

				var testLoader = Customs.Business.OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, false, false);
				var checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load<OrgSupplierPart>(checkFilter);
				AssertEquals("There should have been 1 part record created", 1, enterprisePartsCreated.Length);
				var enterprisePart = LoadPart("P1234-4848X");
				var cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_CC, testLookup.PK);
				cusClassPartFilter.AddToFilter(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				var cusClassPartLink = Factory.LoadTop1<CusClassPartPivot>(cusClassPartFilter);
				AssertNotNull("Classification Part Pivot should have been created", cusClassPartLink);
			}
		}

		public void TestSaveAndClearPreviousTariffDetailsIfPresentAndDifferent()
		{
			var testLookup = CreateClassification("Test Lookup", "8529.90.11.01H");
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("LEAFLETS,PAPER BOOKLETS,NO," + testLookup.CC_LookupCode + ",HTB,," + testOrganisation.OH_Code + ",12,KG,100,M3,,35");
					sw.Flush();
				}

				var testLoader = Customs.Business.OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, false, false);
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

			Classification changedLookup = CreateClassification("Changed Lookup", "8529.90.11.01H");
			using (TempFile updateFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(updateFile.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("LEAFLETS,ONE PAGE BROCHURES,PK," + changedLookup.CC_LookupCode + ",HTB,," + testOrganisation.OH_Code + ",99,,400,M3,PRINTING,25850");
					sw.Flush();
				}

				var testLoader = Customs.Business.OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(updateFile.Filename, true, false);
				Factory.Save();
				var checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load<OrgSupplierPart>(checkFilter);
				AssertEquals("There should still only be 1 part record", 1, enterprisePartsCreated.Length);
				var enterprisePart = LoadPart("LEAFLETS");
				enterprisePart.Reload();
				AssertEquals("Part Description", "ONE PAGE BROCHURES", enterprisePart.OP_Desc);
				AssertEquals("Stock UQ", "PK", enterprisePart.OP_StockKeepingUnit);
				AssertEquals("Division", "PRINTING", enterprisePart.OP_Division);
				AssertEquals("Stock Qty", 25850M, enterprisePart.OP_QtyInStock);
				AssertEquals("Stock Weight", 99M, enterprisePart.OP_Weight);
				AssertEquals("Stock Weight UQ", "", enterprisePart.OP_WeightUQ);
				AssertEquals("Stock Volume", 400M, enterprisePart.OP_Cubic);
				AssertEquals("Stock Volume UQ", "M3", enterprisePart.OP_CubicUQ);
				var cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				CusClassPartPivot[] cusClassPartLinks = enterprisePart.Factory.Load<CusClassPartPivot>(cusClassPartFilter);
				AssertEquals("Should be only 1 pivot record", 1, cusClassPartLinks.Length);
				AssertEquals("Link to this classification", changedLookup.PK, cusClassPartLinks[0].CI_CC);
				Assert(enterprisePart.Notes.HasNotes);
				var testNotes = (StmNoteCollection)enterprisePart.Notes.GetAllNotes();
				AssertEquals("Data Import Update: Previous HTB Details", testNotes[0].ST_Description);
				AssertMultilineASCIIEquals("Notes", "Previous Lookup: Test Lookup" + System.Environment.NewLine, testNotes[0].ST_NoteDataAsText);
			}
		}

		public void TestLoadUsesValidExportLookupWhenNoImportLookup()
		{
			var testLookup = CreateClassification("Test Lookup", "");
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(fileHeader);
					sw.WriteLine("P1234-4848X,RUBBER GASKET,KG," + testLookup.CC_LookupCode + ",HTB,," + testOrganisation.OH_Code + ",12,KG,100,M3,,0");
					sw.Flush();
				}

				var testLoader = Customs.Business.OrgSupplierPartDataLoad.New();
				testLoader.ImportProductData(testFileName.Filename, false, false);
				var checkFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterprisePartsCreated = Factory.Load<OrgSupplierPart>(checkFilter);
				AssertEquals("There should have been 1 part record created even though an  import lookup was not entered - SG only has 1 lookup & can use either if valid", 1, enterprisePartsCreated.Length);
				var enterprisePart = LoadPart("P1234-4848X");
				var cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_CC, testLookup.PK);
				cusClassPartFilter.AddToFilter(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
				var cusClassPartLink = Factory.LoadTop1<CusClassPartPivot>(cusClassPartFilter);
				AssertNotNull("Classification Part Pivot should have been created", cusClassPartLink);
			}
		}

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

		protected override SGOrgSupplierPartDataLoad GetNewDataLoader()
		{
			return new SGOrgSupplierPartDataLoad();
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

		Classification CreateClassification(ZString lookupCode, ZString tariff)
		{
			var testLookup = Factory.NewWithValidTestData<Classification>();
			testLookup.CC_LookupCode = lookupCode;
			if (!tariff.IsEmpty)
			{
				testLookup.CC_TariffNum = tariff;
			}

			testLookup.CC_ClassificationType = Classification.DefaultClassificationType;
			Factory.Save();
			return testLookup;
		}
		#endregion
	}
}
