namespace Enterprise.ReportTesting
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.DocumentEngine.Business;
	using Enterprise.DocumentEngine.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	public abstract class ReportTestCase : TransactionedTestCase
	{
		BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}
				return factory;
			}
		}
		BusinessObjectFactory factory;

		protected override void TearDown()
		{
			base.TearDown();
			coreModuleToTest.Dispose();
		}

		public abstract string MenuName { get; }
		public abstract string Hint { get; }

		public virtual ModuleIdentifier ModuleIdToTest
		{
			get { return null; }
		}

		public virtual ZEmbeddedModule ModuleToTest
		{
			get
			{
				if (ModuleIdToTest == null)
				{
					throw new InvalidOperationException("Either ModuleToTest or preferably ModuleIdToTest should be overridden");
				}
				return (ZReportModule)ZModuleFactory.Instance.Create(ModuleIdToTest);
			}
		}

		protected abstract TemplateTestCase GetTemplateTestCase();

		public TemplateTestCase TemplateTestCase
		{
			get
			{
				if (templateTestCase == null)
				{
					templateTestCase = GetTemplateTestCase();
				}
				return templateTestCase;
			}
		}
		TemplateTestCase templateTestCase;

		ZReportModule coreModuleToTest
		{
			get
			{
				if (moduleToTest == null)
				{
					moduleToTest = (ZReportModule)ModuleToTest;
				}
				return moduleToTest;
			}
		}
		ZReportModule moduleToTest;

		public void TestMenuItemSetupCorrectly()
		{
			AssertEquals(BadMenuSetupMessage(CandidateMenuItems.Count), 1, CandidateMenuItems.Count);
			Assert("Hint must be less than 1024 characters", Hint.Length <= 1024);
			AssertMultilineASCIIEquals("Incorrect Menu hint", Hint, CandidateMenuItems[0].SU_Hint);
		}

		string BadMenuSetupMessage(int menuItemCount)
		{
			string result = "";
			if (menuItemCount < 1)
			{
				result = "Can't find a System Defined, Published menu item called " + MenuName + " for the " + coreModuleToTest.BusinessContext + " module.  Check the menu name, system defined and published flags, and business context";
			}
			else if (menuItemCount > 1)
			{
				result = "There should be 1 and only 1 System Defined, Published menu item called " + MenuName + " for the " + coreModuleToTest.BusinessContext + " module";
			}
			return result;
		}

		public void TestTemplateSetupCorrectly()
		{
			AssertEquals("There should be 1 and only 1 system defined template called " + TemplateTestCase.TemplateName, 1, TemplateTestCase.CandidateTemplates.Count);
			AssertEquals("Template Location incorrect or not defined", TemplateTestCase.TemplateLocation.ToUpper(), TemplateTestCase.CandidateTemplates[0].SO_ExcelTemplatePath.ToUpper());
		}

		public void TestTemplateMenuRelationship()
		{
			ZQuery pivotQuery = new ZQuery(StmMenuTemplatePivotSchema.SI_SO, TemplateTestCase.CandidateTemplates[0].PK);
			pivotQuery.AddToFilter(StmMenuTemplatePivotSchema.SI_SU, CandidateMenuItems[0].PK);
			StmMenuTemplatePivotBaseCollection pivots = new StmMenuTemplatePivotBaseCollection(Factory);
			pivots.Load(pivotQuery);
			AssertEquals("There should be 1 and only 1 pivot for template " + TemplateTestCase.TemplateName + " and menu " + MenuName, 1, pivots.Count);
		}

		protected virtual StmMenuItemBaseCollection CandidateMenuItems
		{
			get
			{
				if (candidateMenuItems == null)
				{
					DocumentZQuery menuQuery = new DocumentZQuery(coreModuleToTest.BusinessContext, MenuName);
					menuQuery.ReLoadExistingRows = true;
					menuQuery.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, true);
					menuQuery.AddToFilter(StmMenuItemSchema.SU_IsPublished, ExpectedIsPublished);
					if (AdditionalCandidateMenuItemsFilter != null)
					{
						menuQuery.AddToFilter(AdditionalCandidateMenuItemsFilter);
					}
					candidateMenuItems = new Enterprise.DocumentEngine.Business.StmMenuItemBaseCollection(Factory);
					candidateMenuItems.Load(menuQuery);
				}
				return candidateMenuItems;
			}
		}
		StmMenuItemBaseCollection candidateMenuItems;

		protected virtual bool ExpectedIsPublished
		{
			get { return true; }
		}

		protected void ResetCandidateMenuItems()
		{
			candidateMenuItems = null;
		}

		protected virtual ZQuery AdditionalCandidateMenuItemsFilter
		{
			get { return null; }
		}
	}
}
