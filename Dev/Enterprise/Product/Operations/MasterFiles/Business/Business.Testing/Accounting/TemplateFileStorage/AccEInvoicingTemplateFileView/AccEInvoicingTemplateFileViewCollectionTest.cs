using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccEInvoicingTemplateFileViewCollection))]
	sealed class AccEInvoicingTemplateFileViewCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCompanyLevelCollectionReturnsCorrectSets()
		{
			var configsPk = PrepTestData();

			var companyLevelCollection = new AccEInvoicingTemplateFileViewCollection(Factory, Env.CurrentCompanyPK);

			AssertEquals(AccEInvoicingTemplateFileLevelEnum.Company, companyLevelCollection.Level);

			companyLevelCollection.Load();

			Assert(companyLevelCollection.IsLoaded);
			AssertEquals(1, companyLevelCollection.Count);

			var templateConfig = companyLevelCollection.Cast<AccEInvoicingTemplateFileView>().FirstOrDefault();

			AssertNotNull(templateConfig);
			AssertEquals(AccEInvoicingTemplateFileLevelEnum.Company, templateConfig.Level);
			AssertEquals(configsPk.cmpLevelPk, templateConfig.PK);

			TestAddNew(companyLevelCollection, ZGuid.Empty);
		}

		public void TestBranchLevelCollectionReturnsCorrectSets()
		{
			var configsPk = PrepTestData();

			var branchLevelCollection = new AccEInvoicingTemplateFileViewCollection(Factory, Env.CurrentCompanyPK, Env.CurrentBranchPK);
			AssertEquals(AccEInvoicingTemplateFileLevelEnum.Branch, branchLevelCollection.Level);

			branchLevelCollection.Load();

			Assert(branchLevelCollection.IsLoaded);
			AssertEquals(2, branchLevelCollection.Count);

			var templateCmpConfig = branchLevelCollection.Cast<AccEInvoicingTemplateFileView>().FirstOrDefault(c => c.Level == AccEInvoicingTemplateFileLevelEnum.Company);

			AssertNotNull(templateCmpConfig);
			AssertEquals(configsPk.cmpLevelPk, templateCmpConfig.PK);

			var templateBrnConfig = branchLevelCollection.Cast<AccEInvoicingTemplateFileView>().FirstOrDefault(c => c.Level == AccEInvoicingTemplateFileLevelEnum.Branch);

			AssertNotNull(templateBrnConfig);
			AssertEquals(configsPk.brnLevelPk, templateBrnConfig.PK);

			TestAddNew(branchLevelCollection, Env.CurrentBranchPK);
		}

		public void TestOrgLevelCollectionReturnsCorrectSets()
		{
			var configsPk = PrepTestData();

			var orgLevelCollection = new AccEInvoicingTemplateFileViewCollection(Factory, Env.CurrentCompanyPK, Env.CurrentBranchPK, Env.CurrentCompany.OrganisationPK);
			AssertEquals(AccEInvoicingTemplateFileLevelEnum.Organisation, orgLevelCollection.Level);

			orgLevelCollection.Load();

			Assert(orgLevelCollection.IsLoaded);
			AssertEquals(3, orgLevelCollection.Count);

			var templateCmpConfig = orgLevelCollection.Cast<AccEInvoicingTemplateFileView>().FirstOrDefault(c => c.Level == AccEInvoicingTemplateFileLevelEnum.Company);

			AssertNotNull(templateCmpConfig);
			AssertEquals(configsPk.cmpLevelPk, templateCmpConfig.PK);
			Assert(templateCmpConfig.ReadOnly);

			var templateBrnConfig = orgLevelCollection.Cast<AccEInvoicingTemplateFileView>().FirstOrDefault(c => c.Level == AccEInvoicingTemplateFileLevelEnum.Branch);

			AssertNotNull(templateBrnConfig);
			AssertEquals(configsPk.brnLevelPk, templateBrnConfig.PK);

			var templateOrgConfig = orgLevelCollection.Cast<AccEInvoicingTemplateFileView>().FirstOrDefault(c => c.Level == AccEInvoicingTemplateFileLevelEnum.Organisation);

			AssertNotNull(templateOrgConfig);
			AssertEquals(configsPk.orgLevelPk, templateOrgConfig.PK);

			TestAddNew(orgLevelCollection, Env.CurrentCompany.OrganisationPK);
		}

		public void TestGetRecordAndSetTemplateFiles()
		{
			TestCaseHelper.ClearTable(AccEInvoicingTemplateFileViewSchema.Constants.TableName);

			var record1 = CreateTemplateConfig(AccEInvoicingTemplateFileLevelEnum.Company, ZGuid.Empty, "ALL", "ALL", "ALL", DefaultTemplate.TFS_Code);
			var record2 = CreateTemplateConfig(AccEInvoicingTemplateFileLevelEnum.Company, ZGuid.Empty, "SHP", "IMP", "ROA", RoadTemplate.TFS_Code);
			var record3 = CreateTemplateConfig(AccEInvoicingTemplateFileLevelEnum.Branch, Env.CurrentBranchPK, "ALL", "IMP", "AIR", AirTemplate.TFS_Code);
			var record4 = CreateTemplateConfig(AccEInvoicingTemplateFileLevelEnum.Branch, Env.CurrentBranchPK, "SHP", "DOM", "SEA", SeaTemplate.TFS_Code);
			var record5 = CreateTemplateConfig(AccEInvoicingTemplateFileLevelEnum.Organisation, Env.CurrentCompany.OrganisationPK, "ALL", "EXP", "AIR", AirTemplate.TFS_Code);
			var record6 = CreateTemplateConfig(AccEInvoicingTemplateFileLevelEnum.Organisation, Env.CurrentCompany.OrganisationPK, "SHP", "ALL", "ROA", RoadTemplate.TFS_Code);
			var record7 = CreateTemplateConfig(AccEInvoicingTemplateFileLevelEnum.Organisation, Env.CurrentCompany.OrganisationPK, "SHP", "EXP", "RAI", RailTemplate.TFS_Code);

			Factory.Save();

			var collection = new AccEInvoicingTemplateFileViewCollection(Factory, Env.CurrentCompanyPK, Env.CurrentBranchPK, Env.CurrentCompany.OrganisationPK);
			AssertEquals(AccEInvoicingTemplateFileLevelEnum.Organisation, collection.Level);

			collection.Load();

			AssertEquals(record6, collection.GetRecord("SHP", "ALL", "ROA", "ROA").PK);
			AssertEquals(record5, collection.GetRecord("ALL", "EXP", "AIR", "AIR").PK);
			AssertEquals(record3, collection.GetRecord("ALL", "IMP", "AIR", "AIR").PK);
			AssertEquals(record2, collection.GetRecord("SHP", "IMP", "ROA", "ROA").PK);
			AssertEquals(record1, collection.GetRecord("ALL", "ALL", "ALL", "DEF").PK);
			AssertEquals(record7, collection.GetRecord("SHP", "EXP", "RAI", "RAI").PK);

			collection.SetTemplateFile("SHP", "ALL", "ROA", DefaultTemplate.TFS_Code);

			AssertEquals("No new records added", 7, collection.Count);

			var updatedRecord = collection.GetRecord("SHP", "ALL", "ROA", "DEF");
			AssertNotNull(updatedRecord);

			collection.SetTemplateFile("SHP", "EXP", "ROA", AirTemplate.TFS_Code);
			AssertEquals("A new record added", 8, collection.Count);

			var addedRecord = collection.GetRecord("SHP", "EXP", "ROA", "AIR");
			AssertNotNull(addedRecord);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccEInvoicingTemplateFileViewCollection(Factory, ZGuid.NewZGuid());
		}

		(ZGuid cmpLevelPk, ZGuid brnLevelPk, ZGuid orgLevelPk) PrepTestData()
		{
			TestCaseHelper.ClearTable(AccEInvoicingTemplateFileViewSchema.Constants.TableName);
			var cmpLevelPk = CreateTemplateConfig(AccEInvoicingTemplateFileLevelEnum.Company, ZGuid.Empty, "ALL", "ALL", "ALL", DefaultTemplate.TFS_Code);
			var brnLevelPk = CreateTemplateConfig(AccEInvoicingTemplateFileLevelEnum.Branch, Env.CurrentBranchPK, "ALL", "IMP", "SEA", SeaTemplate.TFS_Code);
			var orgLevelPk = CreateTemplateConfig(AccEInvoicingTemplateFileLevelEnum.Organisation, Env.CurrentCompany.OrganisationPK, "ALL", "EXP", "AIR", AirTemplate.TFS_Code);

			Factory.Save();

			return (cmpLevelPk, brnLevelPk, orgLevelPk);
		}

		ZGuid CreateTemplateConfig(AccEInvoicingTemplateFileLevelEnum level, ZGuid parentPk, ZString jobType, ZString serviceDirection, ZString transportMode, ZString templateCode)
		{
			var templateConfig = Factory.New<AccEInvoicingTemplateFileView>();
			templateConfig.ETF_GC = Env.CurrentCompanyPK;
			templateConfig.ETF_JobType = jobType;
			templateConfig.ETF_ServiceDirection = serviceDirection;
			templateConfig.ETF_TransportMode = transportMode;
			templateConfig.ETF_ParentTableCode = AccEInvoicingTemplateFileView.ToTablePrefix(level);
			templateConfig.ETF_ParentID = parentPk;
			templateConfig.ETF_TemplateCode = templateCode;
			Factory.Save();

			return templateConfig.PK;
		}

		void TestAddNew(AccEInvoicingTemplateFileViewCollection col, ZGuid parentPk)
		{
			var bizo = col.AddNew();
			AssertEquals(Env.CurrentCompanyPK, bizo.ETF_GC);
			AssertEquals(col.Level, bizo.Level);
			AssertEquals(parentPk, bizo.ETF_ParentID);
		}

		protected override void SetUp()
		{
			base.SetUp();

			DefaultTemplate = Factory.NewWithValidTestData<AccTemplateFileStorage>();
			DefaultTemplate.TFS_Code = "DEF";
			DefaultTemplate.TFS_ExternalReference = ZGuid.NewZGuid();
			SeaTemplate = Factory.NewWithValidTestData<AccTemplateFileStorage>();
			SeaTemplate.TFS_Code = "SEA";
			SeaTemplate.TFS_ExternalReference = ZGuid.NewZGuid();
			AirTemplate = Factory.NewWithValidTestData<AccTemplateFileStorage>();
			AirTemplate.TFS_Code = "AIR";
			AirTemplate.TFS_ExternalReference = ZGuid.NewZGuid();
			RoadTemplate = Factory.NewWithValidTestData<AccTemplateFileStorage>();
			RoadTemplate.TFS_Code = "ROA";
			RoadTemplate.TFS_ExternalReference = ZGuid.NewZGuid();
			RailTemplate = Factory.NewWithValidTestData<AccTemplateFileStorage>();
			RailTemplate.TFS_Code = "RAI";
			RailTemplate.TFS_ExternalReference = ZGuid.NewZGuid();

			Factory.Save();
		}

		AccTemplateFileStorage DefaultTemplate;
		AccTemplateFileStorage SeaTemplate;
		AccTemplateFileStorage AirTemplate;
		AccTemplateFileStorage RoadTemplate;
		AccTemplateFileStorage RailTemplate;
	}
}
