using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MenuPivotHelperTest : TestCaseWithFactory
	{
		public void TestCreateMenuAndPivotIfNotInDatabase()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			StmTemplate template = factory.NewWithValidTestData<StmTemplate>();
			template.SO_Name = "This is a new template";
			factory.Save();
			StmMenuItem menu = StmMenuItem.FindDocumentMenu(factory, template, BusinessContext.ARInvoice);
			AssertNull("Precondition - menu does not exist", menu);

			MenuPivotHelper.CreateMenuAndPivotWithEmptyMenuPathIfNotInDatabase(factory, template, BusinessContext.ARInvoice, "Invoice", nameof(BusinessContext.ARInvoice));
			factory.Save();
			menu = StmMenuItem.FindDocumentMenu(factory, template, BusinessContext.ARInvoice);
			AssertNotNull("Postcondition - menu should be created", menu);
			AssertEquals("ARInvoice", menu.SU_BusinessContext);
			AssertEquals("This is a new template", menu.SU_MenuName);
			StmMenuTemplatePivot pivot = StmMenuTemplatePivot.FindDocumentPivot(factory, template, menu);
			AssertNotNull("Postcondition - pivot should be created", pivot);
		}
	}
}
