using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartLastCostDataLoad))]
	sealed class OrgSupplierPartLastCostDataLoadTest : DataLoadTestCase<OrgSupplierPartLastCostDataLoad>
	{
		[ExpectException(typeof(FileNotFoundException))]
		public void TestValidationOfFile()
		{
			loader.ImportPartLastCostData("non-existant file", false);
		}

		public void TestValidationOfHeader()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Wrong Header Info");
				}

				loader.ImportPartLastCostData(testFileName.Filename, false);
				AssertEquals(1, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(0, loader.RunCounters.RecsExcluded);
				AssertEquals(3, loader.Log.Count);
				AssertEquals("FileHeaderIsValid", false, loader.FileHeaderIsValid);
			}

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Owner,Supplier,Last_Cost");
					sw.Flush();
				}

				loader.ImportPartLastCostData(testFileName.Filename, false);
				AssertEquals(1, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(0, loader.RunCounters.RecsExcluded);
				AssertEquals(5, loader.Log.Count);
				AssertEquals("FileHeaderIsValid", true, loader.FileHeaderIsValid);
			}
		}

		public void TestPartLastCostIsUpdated()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			CreateTestProductRecords(testOrganisation);
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Owner,Supplier,Last_Cost");
					sw.WriteLine("1," + testOrganisation.OH_Code + ",,120");
					sw.WriteLine("912.226,," + testOrganisation.OH_Code + ",1.95");
					sw.WriteLine("123," + testOrganisation.OH_Code + ",,0");
					sw.WriteLine("1," + testOrganisation.OH_Code + ",,130");
					sw.Flush();
				}

				loader.ImportPartLastCostData(testFileName.Filename, false);

				BusinessObjectFactory newTestFactory = new BusinessObjectFactory();
				OrgSupplierPart enterprisePart = LoadPart(newTestFactory, "1");
				AssertEquals("Product Last Cost", 130m, enterprisePart.OP_LastCost);
				enterprisePart = LoadPart(newTestFactory, "912.226");
				AssertEquals("Product Last Cost", 1.95m, enterprisePart.OP_LastCost);
				enterprisePart = LoadPart(newTestFactory, "123");
				AssertEquals("Product Last Cost", 0m, enterprisePart.OP_LastCost);
			}
		}

		public void TestPartWithoutOrgsIsNotUpdated()
		{
			ClearCustomsRecordsBeforeTesting();
			CreateTestProduct(null, null, 0);
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Owner,Supplier,Last_Cost");
					sw.WriteLine("TestPart,,,129.99");
					sw.Flush();
				}

				loader.ImportPartLastCostData(testFileName.Filename, false);

				BusinessObjectFactory newTestFactory = new BusinessObjectFactory();
				OrgSupplierPart enterprisePart = LoadPart(newTestFactory, "TestPart");
				AssertEquals("Product Last Cost should not have been updated", 0m, enterprisePart.OP_LastCost);
			}
		}

		public void TestPartWithBuyerAndSupplierIsUpdated()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";
			OrgHeader owner = Factory.New<OrgHeader>();
			owner.OH_Code = "OWNER";
			CreateTestProduct(owner, supplier, 2);
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Owner,Supplier,Last_Cost");
					sw.WriteLine("TestPart,OWNER,SUPPLIER,129.99");
					sw.Flush();
				}

				loader.ImportPartLastCostData(testFileName.Filename, false);

				BusinessObjectFactory newTestFactory = new BusinessObjectFactory();
				OrgSupplierPart enterprisePart = LoadPart(newTestFactory, "TestPart");
				AssertEquals("Product Last Cost should have been updated", 129.99m, enterprisePart.OP_LastCost);
			}
		}

		public void TestPart_PreferencesActivePart()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";

			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "OWNER";

			var product1_Inactive = Factory.New<OrgSupplierPart>();
			product1_Inactive.OP_IsActive = false;
			product1_Inactive.OP_PartNum = "product1";
			product1_Inactive.RelatedOrganisations.AddOrganisationIfNotExist(owner.PK, OrgPartRelation.RelationshipTypes.Owner);
			product1_Inactive.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "product1";
			product1.RelatedOrganisations.AddOrganisationIfNotExist(owner.PK, OrgPartRelation.RelationshipTypes.Owner);
			product1.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);

			Factory.Save();
			AssertCorrectNumberOfPartsExpected(2);

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Owner,Supplier,Last_Cost");
					sw.WriteLine("product1,OWNER,SUPPLIER,111.11");
					sw.Flush();
				}

				loader.ImportPartLastCostData(testFileName.Filename, false);

				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var product1InNewFactory = newFactory.Load<OrgSupplierPart>(product1.PK);
				var product1_InactiveInNewFactory = newFactory.Load<OrgSupplierPart>(product1_Inactive.PK);
				AssertEquals("Product Last Cost should have been updated.", 111.11m, product1InNewFactory.OP_LastCost);
				AssertEquals("Product Last Cost should *not* have been updated.", 0m, product1_Inactive.OP_LastCost);
			}
		}

		#region TestPartIsUpdatedWhenRelationshipTypeIsBoth

		public void TestPartIsUpdatedWhenRelationshipTypeIsBoth_OwnerMatch_SupplierMatch_Both()
		{
			TestPartIsUpdatedWhenRelationshipTypeIsBothCore(line: "product1,OWNER,SUPPLIER,111.11", expectedValue: 111.11m, ownerRelationship: OrgPartRelation.RelationshipTypes.Both, supplierRelationship: OrgPartRelation.RelationshipTypes.Both);
		}

		public void TestPartIsUpdatedWhenRelationshipTypeIsBoth_OwnerMatch_SupplierMatch_Owner()
		{
			TestPartIsUpdatedWhenRelationshipTypeIsBothCore(line: "product1,OWNER,SUPPLIER,111.11", expectedValue: 111.11m, ownerRelationship: OrgPartRelation.RelationshipTypes.Owner, supplierRelationship: OrgPartRelation.RelationshipTypes.Both);
		}

		public void TestPartIsUpdatedWhenRelationshipTypeIsBoth_OwnerMatch_SupplierMatch_Supplier()
		{
			TestPartIsUpdatedWhenRelationshipTypeIsBothCore(line: "product1,OWNER,SUPPLIER,111.11", expectedValue: 111.11m, ownerRelationship: OrgPartRelation.RelationshipTypes.Both, supplierRelationship: OrgPartRelation.RelationshipTypes.Supplier);
		}

		public void TestPartIsUpdatedWhenRelationshipTypeIsBoth_OwnerMatch_SupplierMatch_WarehouseConsignee()
		{
			TestPartIsUpdatedWhenRelationshipTypeIsBothCore(line: "product1,OWNER,SUPPLIER,111.11", expectedValue: 0m, ownerRelationship: OrgPartRelation.RelationshipTypes.WarehouseConsignee, supplierRelationship: OrgPartRelation.RelationshipTypes.WarehouseConsignee);
		}

		public void TestPartIsUpdatedWhenRelationshipTypeIsBoth_OwnerMatch_SupplierMatch_WrongRelationship()
		{
			TestPartIsUpdatedWhenRelationshipTypeIsBothCore(line: "product1,OWNER,SUPPLIER,111.11", expectedValue: 0m, ownerRelationship: OrgPartRelation.RelationshipTypes.Supplier, supplierRelationship: OrgPartRelation.RelationshipTypes.Owner);
		}

		public void TestPartIsUpdatedWhenRelationshipTypeIsBoth_OwnerMatch_NoSupplier()
		{
			TestPartIsUpdatedWhenRelationshipTypeIsBothCore(line: "product1,OWNER,,222.22", expectedValue: 222.22m, ownerRelationship: OrgPartRelation.RelationshipTypes.Both, supplierRelationship: OrgPartRelation.RelationshipTypes.Both);
		}

		public void TestPartIsUpdatedWhenRelationshipTypeIsBoth_OwnerMatch_SupplierNotMatch()
		{
			TestPartIsUpdatedWhenRelationshipTypeIsBothCore(line: "product1,OWNER,SUPPLIER2,222.22", expectedValue: 0m, ownerRelationship: OrgPartRelation.RelationshipTypes.Both, supplierRelationship: OrgPartRelation.RelationshipTypes.Both);
		}

		public void TestPartIsUpdatedWhenRelationshipTypeIsBoth_NoOwner_SupplierMatch()
		{
			TestPartIsUpdatedWhenRelationshipTypeIsBothCore(line: "product1,,SUPPLIER,333.33", expectedValue: 333.33m, ownerRelationship: OrgPartRelation.RelationshipTypes.Both, supplierRelationship: OrgPartRelation.RelationshipTypes.Both);
		}
		public void TestPartIsUpdatedWhenRelationshipTypeIsBoth_OwnerNotMatch_SupplierMatch()
		{
			TestPartIsUpdatedWhenRelationshipTypeIsBothCore(line: "product1,OWNER2,SUPPLIER,333.33", expectedValue: 333.33m, ownerRelationship: OrgPartRelation.RelationshipTypes.Both, supplierRelationship: OrgPartRelation.RelationshipTypes.Both);
		}

		public void TestPartIsUpdatedWhenRelationshipTypeIsBoth_NoOwner_NoSupplier()
		{
			TestPartIsUpdatedWhenRelationshipTypeIsBothCore(line: "product1,,,444.44", expectedValue: 0m, ownerRelationship: OrgPartRelation.RelationshipTypes.Both, supplierRelationship: OrgPartRelation.RelationshipTypes.Both);
		}

		void TestPartIsUpdatedWhenRelationshipTypeIsBothCore(string line, decimal expectedValue, string ownerRelationship, string supplierRelationship)
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";

			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "OWNER";

			var otherSupplier = Factory.New<OrgHeader>();
			otherSupplier.OH_Code = "SUPPLIER2";

			var otherOwner = Factory.New<OrgHeader>();
			otherOwner.OH_Code = "OWNER2";

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "product1";
			product1.RelatedOrganisations.AddOrganisationIfNotExist(owner.PK, ownerRelationship);
			product1.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, supplierRelationship);

			Factory.Save();
			AssertCorrectNumberOfPartsExpected(1);
			AssertEquals("Precondition - should have two related organisations.", 2, product1.RelatedOrganisations.Count);

			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Owner,Supplier,Last_Cost");
					sw.WriteLine(line);
					sw.Flush();
				}

				loader.ImportPartLastCostData(testFileName.Filename, false);

				var productInNewFactory = new BusinessObjectFactory().Load<OrgSupplierPart>(product1.PK);
				AssertEquals("Product Last Cost should have been updated if owner and supplier are match or if owner is match and no supplier or if on owner and only supplier specify in text file.", expectedValue, productInNewFactory.OP_LastCost);
			}
		}

		#endregion

		#region TestLastCostIsDecimal

		public void TestLastCostIsDecimal()
		{
			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "OWNER";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TestPart";
			product.RelatedOrganisations.AddOrganisationIfNotExist(owner.PK, OrgPartRelation.RelationshipTypes.Both);
			Factory.Save();

			TestLastCostIsDecimalCore(line: "TestPart,OWNER,,$129.99", expectedlog: "Last Cost is not valid. Row 1 contains the following data : TestPart,OWNER,,$129.99 Last Cost: $129.99");
			TestLastCostIsDecimalCore(line: "TestPart,OWNER,,129.99$", expectedlog: "Last Cost is not valid. Row 1 contains the following data : TestPart,OWNER,,129.99$ Last Cost: 129.99$");
			TestLastCostIsDecimalCore(line: "TestPart,OWNER,,#129.99", expectedlog: "Last Cost is not valid. Row 1 contains the following data : TestPart,OWNER,,#129.99 Last Cost: #129.99");
			TestLastCostIsDecimalCore(line: "TestPart,OWNER,,129.A9", expectedlog: "Last Cost is not valid. Row 1 contains the following data : TestPart,OWNER,,129.A9 Last Cost: 129.A9");
			TestLastCostIsDecimalCore(line: "TestPart,OWNER,,129.99", expectedlog: "");
			TestLastCostIsDecimalCore(line: "TestPart,OWNER,,+129.99", expectedlog: "");
			TestLastCostIsDecimalCore(line: "TestPart,OWNER,,-129.99", expectedlog: "");
			TestLastCostIsDecimalCore(line: "TestPart,OWNER,,129", expectedlog: "");
			TestLastCostIsDecimalCore(line: "TestPart,OWNER,,0", expectedlog: "");
		}

		void TestLastCostIsDecimalCore(string line, string expectedlog)
		{
			var newLoader = new OrgSupplierPartLastCostDataLoadForTest();
			newLoader.RunCounters.CurrentRow = 1;
			newLoader.ProcessDataForThisLineExposed(new OCsvLine(line));
			if (string.IsNullOrEmpty(expectedlog))
			{
				AssertEquals(0, newLoader.Log.Count);
			}
			else
			{
				AssertEquals(1, newLoader.Log.Count);
				AssertEquals(expectedlog, newLoader.Log[0]);
			}
		}

		#endregion

		public void TestProcessDataForThisLineUserMessages()
		{
			var newLoader = new OrgSupplierPartLastCostDataLoadForTest();
			newLoader.RunCounters.CurrentRow = 99;
			newLoader.ProcessDataForThisLineExposed(new OCsvLine("Test Data"));
			AssertEquals(1, newLoader.Log.Count);
			AssertEquals("File contains inconsistent data. Row 99 contains the following data : Test Data", newLoader.Log[0]);
		}

		public void TestImportLastCostUsingLegacyCodes()
		{
			ClearCustomsRecordsBeforeTesting();
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));

			OrgCusCode ownerLSC = Factory.New<OrgCusCode>();
			ownerLSC.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
			ownerLSC.OK_CustomsRegNo = "TstOrgLSC";
			testOrganisation.CustomsCodes.Add(ownerLSC);

			OrgCusCode supplierLSC = Factory.New<OrgCusCode>();
			supplierLSC.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
			supplierLSC.OK_CustomsRegNo = "SpplrLSC";
			testSupplier.CustomsCodes.Add(supplierLSC);

			Factory.Save();

			CreateTestProductRecords(testOrganisation);
			OrgSupplierPart enterprisePart = LoadPart(Factory, "912.226");
			enterprisePart.Delete();

			Factory.Save();

			OrgSupplierPart testPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			testPart.OP_PartNum = "912.226";
			testPart.RelatedOrganisations.AddOrganisationIfNotExist(testSupplier.PK, OrgPartRelation.RelationshipTypes.Supplier);

			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Code,Owner,Supplier,Last_Cost");
					sw.WriteLine("1,TstOrgLSC,,65.90");
					sw.WriteLine("912.226,,SpplrLSC,1500");
					sw.WriteLine("123,TstOrgLSC,,0.45");
					sw.WriteLine("1,TstOrgLSC,,67.99");
					sw.Flush();
				}

				loader.ImportPartLastCostData(testFileName.Filename, true);
			}

			BusinessObjectFactory newTestFactory = new BusinessObjectFactory();
			enterprisePart = LoadPart(newTestFactory, "1");
			AssertEquals("Product Last Cost", 67.99m, enterprisePart.OP_LastCost);
			enterprisePart = LoadPart(newTestFactory, "912.226");
			AssertEquals("Product Last Cost", 1500m, enterprisePart.OP_LastCost);
			enterprisePart = LoadPart(newTestFactory, "123");
			AssertEquals("Product Last Cost", 0.45m, enterprisePart.OP_LastCost);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			loader = new OrgSupplierPartLastCostDataLoadForTest();
		}

		OrgSupplierPartLastCostDataLoadForTest loader;

		protected override OrgSupplierPartLastCostDataLoad GetNewDataLoader()
		{
			return new OrgSupplierPartLastCostDataLoad();
		}

		void ClearCustomsRecordsBeforeTesting()
		{
			TestCaseHelper.ClearTable("OrgPartUnit");
			TestCaseHelper.ClearTable("CusClassPartPivot");
			TestCaseHelper.ClearTable("CusClassification");
			TestCaseHelper.ClearTable("OrgPartRelation");
			TestCaseHelper.ClearTable("OrgSupplierPart");
			AssertCorrectNumberOfPartsExpected(0);
		}

		void CreateTestProductRecords(OrgHeader testOrganisation)
		{
			OrgSupplierPart testPart1 = Factory.New<OrgSupplierPart>();
			testPart1.OP_PartNum = "1";
			testPart1.RelatedOrganisations.AddOrganisationIfNotExist(testOrganisation.PK, OrgPartRelation.RelationshipTypes.Owner);

			OrgSupplierPart testPart2 = Factory.New<OrgSupplierPart>();
			testPart2.OP_PartNum = "123";
			testPart2.RelatedOrganisations.AddOrganisationIfNotExist(testOrganisation.PK, OrgPartRelation.RelationshipTypes.Owner);

			OrgSupplierPart testPart3 = Factory.NewWithValidTestData<OrgSupplierPart>();
			testPart3.OP_PartNum = "912.226";
			testPart3.RelatedOrganisations.AddOrganisationIfNotExist(testOrganisation.PK, OrgPartRelation.RelationshipTypes.Supplier);

			Factory.Save();
			AssertCorrectNumberOfPartsExpected(3);
		}

		void CreateTestProduct(OrgHeader owner, OrgHeader supplier, int relationsExpected)
		{
			OrgSupplierPart testPart = Factory.New<OrgSupplierPart>();
			testPart.OP_PartNum = "TestPart";
			if (owner != null)
			{
				testPart.RelatedOrganisations.AddOrganisationIfNotExist(owner.PK, OrgPartRelation.RelationshipTypes.Owner);
			}

			if (supplier != null)
			{
				testPart.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			}

			Factory.Save();
			AssertCorrectNumberOfPartsExpected(1);
			AssertEquals("Test Precondition - Part related orgs", relationsExpected, testPart.RelatedOrganisations.Count);
		}

		OrgSupplierPart LoadPart(BusinessObjectFactory testFactory, ZString lookupPart)
		{
			ZQuery partFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, lookupPart);
			OrgSupplierPart enterprisePart = testFactory.LoadTop1<OrgSupplierPart>(partFilter);
			AssertNotNull("Expecting Part " + lookupPart + " to be found", enterprisePart);
			return enterprisePart;
		}

		void AssertCorrectNumberOfPartsExpected(int partsExpected)
		{
			AssertEquals("Test Precondition - Part records", partsExpected, Factory.GetDatabaseCount(typeof(OrgSupplierPart)));
		}

		#endregion

		sealed class OrgSupplierPartLastCostDataLoadForTest : OrgSupplierPartLastCostDataLoad
		{
			public OrgSupplierPartLastCostDataLoadForTest() : base()
			{
			}

			internal void ProcessDataForThisLineExposed(OCsvLine line) => ProcessDataForThisLine(line);
		}
	}
}
