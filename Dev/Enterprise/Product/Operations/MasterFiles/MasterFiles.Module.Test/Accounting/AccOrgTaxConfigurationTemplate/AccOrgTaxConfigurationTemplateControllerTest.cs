using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccOrgTaxConfigurationTemplateController))]
	sealed class AccOrgTaxConfigurationTemplateControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccOrgTaxConfigurationTemplate;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			Factory.Save();
			return template;
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new AccOrgTaxConfigurationTemplateControllerForTest();
			AssertEquals(Env.Security.TaxConfigurationTemplate, controller.CheckPointForView);
			AssertEquals(Env.Security.TaxConfigurationTemplateNew, controller.CheckPointForNew);
			AssertEquals(Env.Security.TaxConfigurationTemplateModify, controller.CheckPointForEdit);
			AssertEquals(Env.Security.TaxConfigurationTemplateDelete, controller.CheckPointForDelete);
			AssertEquals(Env.Security.TaxConfigurationTemplateCopy, controller.CheckPointForCopy(null));
		}

		[RequiresSTA]
		public void TestForm()
		{
			var bizO = GetBusinessObjectThatIsInTheDatabase();
			AssertControllerNotNull();

			var form = Controller.ShowNewForm();
			AssertType<AccOrgTaxConfigurationTemplateForm>(form);
			form.Dispose();

			form = Controller.ShowViewForm(bizO);
			AssertType<AccOrgTaxConfigurationTemplateForm>(form);
			form.Dispose();

			form = Controller.ShowEditForm(bizO);
			AssertType<AccOrgTaxConfigurationTemplateForm>(form);
			form.Dispose();

			form = Controller.ShowDeleteForm(bizO);
			AssertType<AccOrgTaxConfigurationTemplateForm>(form);
			form.Dispose();

			form = Controller.ShowTemplateCopyForm(bizO);
			AssertType<AccOrgTaxConfigurationTemplateForm>(form);
			form.Dispose();
		}
	}
}
