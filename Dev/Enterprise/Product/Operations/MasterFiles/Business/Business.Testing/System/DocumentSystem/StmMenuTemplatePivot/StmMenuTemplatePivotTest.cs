using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmMenuTemplatePivot))]
	sealed class StmMenuTemplatePivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFindDocumentPivot()
		{
			StmTemplate newTemplate = Factory.NewWithValidTestData<StmTemplate>();
			newTemplate.SO_Name = "This is a new template";

			StmMenuItem newMenu = Factory.NewWithValidTestData<StmMenuItem>();
			newMenu.SU_MenuName = "This is a new menu";
			Factory.Save();

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(StmTemplate));
			query.AddToFilter(StmTemplateSchema.SO_Name, "Standard");
			StmTemplate existingTemplate = Factory.LoadTop1<StmTemplate>(query);

			query = new ZDBOnlyQuery(typeof(StmMenuItem));
			query.AddToFilter(StmMenuItemSchema.SU_MenuName, "Standard");
			StmMenuItem existingMenu = Factory.LoadTop1<StmMenuItem>(query);

			StmMenuTemplatePivot pivot = StmMenuTemplatePivot.FindDocumentPivot(Factory, newTemplate, newMenu);
			AssertNull("pivot should not exist", pivot);

			pivot = StmMenuTemplatePivot.FindDocumentPivot(Factory, existingTemplate, existingMenu);
			AssertNotNull("pivot should exist", pivot);
		}

		public void TestCreateDocumentPivot()
		{
			StmTemplate newTemplate = Factory.NewWithValidTestData<StmTemplate>();
			newTemplate.SO_Name = "This is a new template";

			StmMenuItem newMenu = Factory.NewWithValidTestData<StmMenuItem>();
			newMenu.SU_MenuName = "This is a new menu";
			newMenu.SU_BusinessContext = nameof(BusinessContext.APTransaction);
			Factory.Save();

			StmMenuTemplatePivot pivot = StmMenuTemplatePivot.FindDocumentPivot(Factory, newTemplate, newMenu);
			AssertNull("Precondition - pivot should not exist", pivot);
			StmMenuTemplatePivot.CreateDocumentPivot(Factory, newTemplate, newMenu, "Standard");
			Factory.Save();
			pivot = StmMenuTemplatePivot.FindDocumentPivot(Factory, newTemplate, newMenu);
			AssertNotNull("Postcondition - pivot should exist", pivot);
		}

		public void TestDocumentIndex()
		{
			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_Index = 1;
			AssertEquals("Template index: 1", pivot.DocumentIndex);
		}
	}
}
