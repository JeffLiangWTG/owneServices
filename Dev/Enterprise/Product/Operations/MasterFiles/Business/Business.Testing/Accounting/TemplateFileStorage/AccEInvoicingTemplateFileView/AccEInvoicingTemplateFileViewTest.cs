using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccEInvoicingTemplateFileView))]
	sealed class AccEInvoicingTemplateFileViewTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultsCorrectly()
		{
			var templateFileConfig = Factory.New<AccEInvoicingTemplateFileView>();

			AssertEquals("TES", templateFileConfig.ETF_ConfigType);
			AssertEquals("AR", templateFileConfig.ETF_Ledger);
			AssertEquals("ALL", templateFileConfig.ETF_JobType);
			AssertEquals("ALL", templateFileConfig.ETF_ServiceDirection);
			AssertEquals("ALL", templateFileConfig.ETF_TransportMode);
			AssertEquals(ZString.Empty, templateFileConfig.ETF_ParentTableCode);
			AssertEquals(GlbCompany.CurrentCompany.PK, templateFileConfig.ETF_GC);
			AssertEquals(ZString.Empty, templateFileConfig.ETF_ParentTableCode);
		}

		public void TestDeterminesDuplicateCorrectly()
		{
			var fakeCompanyGuid = ZGuid.NewZGuid();

			var templateFileConfig = (AccEInvoicingTemplateFileView)GetNewBusinessObject();
			var templateFileConfigDup = (AccEInvoicingTemplateFileView)GetNewBusinessObject();

			templateFileConfig.ETF_TemplateCode = templateFileConfigDup.ETF_TemplateCode;

			AssertPropertyIsConsideredInCheckForDuplication((b, v) => b.ETF_GC = v, b => b.ETF_GC, templateFileConfig, templateFileConfigDup, ZGuid.NewZGuid());
			AssertPropertyIsConsideredInCheckForDuplication((b, v) => b.ETF_ParentTableCode = v, b => b.ETF_ParentTableCode, templateFileConfig, templateFileConfigDup, new ZString("OH"));
			AssertPropertyIsConsideredInCheckForDuplication((b, v) => b.ETF_ParentID = v, b => b.ETF_ParentID, templateFileConfig, templateFileConfigDup, ZGuid.NewZGuid());
			AssertPropertyIsConsideredInCheckForDuplication((b, v) => b.ETF_JobType = v, b => b.ETF_JobType, templateFileConfig, templateFileConfigDup, new ZString("JOB"));
			AssertPropertyIsConsideredInCheckForDuplication((b, v) => b.ETF_TransportMode = v, b => b.ETF_TransportMode, templateFileConfig, templateFileConfigDup, new ZString("TRN"));
		}

		void AssertPropertyIsConsideredInCheckForDuplication<PropType>(Action<AccEInvoicingTemplateFileView, PropType> setter, Func<AccEInvoicingTemplateFileView, PropType> getter, AccEInvoicingTemplateFileView origBizo, AccEInvoicingTemplateFileView dupBizo, PropType randomValue)
		{
			Assert(origBizo.IsDuplicateOf(dupBizo));
			AssertEquals(getter(origBizo), getter(dupBizo));
			setter(dupBizo, randomValue);
			Assert(!origBizo.IsDuplicateOf(dupBizo));
			setter(dupBizo, getter(origBizo));
		}

		public void TestLevelIsCalculatedProperly()
		{
			var templateFileConfig = (AccEInvoicingTemplateFileView)GetNewBusinessObject();

			AssertEquals(AccEInvoicingTemplateFileLevelEnum.Company, templateFileConfig.Level);
			AssertEquals("Company", templateFileConfig.LevelName);

			templateFileConfig.ETF_ParentTableCode = OrgHeaderSchema.Constants.Prefix;

			AssertEquals(AccEInvoicingTemplateFileLevelEnum.Organisation, templateFileConfig.Level);
			AssertEquals("Organization", templateFileConfig.LevelName);
		}

		public void TestIJobConfiguration()
		{
			var bizo = (IJobConfiguration)Factory.New<AccEInvoicingTemplateFileView>();
			Assert(bizo.IncludeOptionsForAllJobTypes);
		}

		public override void TestCloneAuditProperties()
		{
			Assert(true);
		}

		public override void TestCloneAuditContextProperties()
		{
			Assert(true);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var templateFile = Factory.NewWithValidTestData<AccTemplateFileStorage>();
			templateFile.TFS_ExternalReference = ZGuid.NewZGuid();
			Factory.Save();

			var templateFileConfig = (AccEInvoicingTemplateFileView)base.GetNewBusinessObject();

			templateFileConfig.ETF_GC = Env.CurrentCompanyPK;
			templateFileConfig.ETF_ServiceDirection = "ALL";
			templateFileConfig.ETF_TransportMode = "AIR";
			templateFileConfig.ETF_TemplateCode = templateFile.TFS_Code;

			return templateFileConfig;
		}

		public void TestTESLevel()
		{
			AssertEquals(AccEInvoicingTemplateFileLevelEnum.Company, AccEInvoicingTemplateFileView.ToTESLevel(""));
			AssertEquals(AccEInvoicingTemplateFileLevelEnum.Branch, AccEInvoicingTemplateFileView.ToTESLevel("GB"));
			AssertEquals(AccEInvoicingTemplateFileLevelEnum.Organisation, AccEInvoicingTemplateFileView.ToTESLevel("OH"));

			AssertExceptionThrown<InvalidOperationException>("Unknown table prefix value XXX", () => AccEInvoicingTemplateFileView.ToTESLevel("XXX"));
		}

		public void TestTablePrefix()
		{
			AssertEquals(string.Empty, AccEInvoicingTemplateFileView.ToTablePrefix(AccEInvoicingTemplateFileLevelEnum.Company));
			AssertEquals("GB", AccEInvoicingTemplateFileView.ToTablePrefix(AccEInvoicingTemplateFileLevelEnum.Branch));
			AssertEquals("OH", AccEInvoicingTemplateFileView.ToTablePrefix(AccEInvoicingTemplateFileLevelEnum.Organisation));
		}
	}
}
