using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	sealed class ValidationToolUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			using var testForm = new ZForm(template);
			var validationControl = new ValidationToolUserControl();
			testForm.Controls.Add(validationControl);

			testForm.Show();

			AssertEquals(typeof(IValidationToolParent), validationControl.BindingSource.DataSourceType);
		}

		public void TestValidationRuleActionsGrid() => CombineAssertions(() =>
		{
			var template = Factory.New<ProcessTaskTemplate>();
			using var testForm = new ZForm(template);
			var validationControl = new ValidationToolUserControl();
			testForm.Controls.Add(validationControl);

			testForm.Show();

			var grid = validationControl.ValidationRuleActionsGrid;
			AssertEquals("Binding Member", "ProcessTemplateValidations.ProcessTemplateValidationActions", grid.GetBindingMember());
			var columnNames = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
			AssertContainsExactElementsInExactOrder("Column Names", new[]
			{
				"P0A_ActionSource",
				"ActionSourceDescription"
			}, columnNames);
		});

		public void TestValidationRulesGrid() => CombineAssertions(() =>
		{
			var template = Factory.New<ProcessTaskTemplate>();
			using var testForm = new ZForm(template);
			var validationControl = new ValidationToolUserControl();
			testForm.Controls.Add(validationControl);
			testForm.Show();

			var grid = validationControl.ValidationRulesGrid;

			AssertEquals("Binding Member", "ProcessTemplateValidations", grid.GetBindingMember());
			var columnNames = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
			AssertContainsExactElementsInExactOrder("Column Names", new[]
			{
				ProcessTemplateValidation.Schema.P0V_Description,
				ProcessTemplateValidation.Schema.P0V_Condition1,
				ProcessTemplateValidation.Schema.P0V_Condition2,
				ProcessTemplateValidation.Schema.P0V_Condition2Value,
				ProcessTemplateValidation.Schema.P0V_GC_Company,
				ProcessTemplateValidation.Schema.P0V_ValidationRule,
				ProcessTemplateValidation.Schema.P0V_Severity,
				ProcessTemplateValidation.Schema.P0V_Message,
				ProcessTemplateValidation.Schema.P0V_FieldToDisplayValidation,
				ProcessTemplateValidation.Schema.P0V_LogValidationFailEvent,
				ProcessTemplateValidation.Schema.P0V_ContextType,
				ProcessTemplateValidation.Schema.P0V_RQT_RequestTypeOnFailure
			}, columnNames);
		});

		public void TestFilter()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var rule1 = template.ProcessTemplateValidations.AddNew();
			rule1.P0V_Description = "Rule 1";
			rule1.P0V_Message = "Test Rule 1";
			var action1 = rule1.ProcessTemplateValidationActions.AddNew();
			action1.P0A_ActionSource = "AAA";

			var rule2 = template.ProcessTemplateValidations.AddNew();
			rule2.P0V_Description = "Rule 2";
			rule2.P0V_Message = "Test Rule 2";
			var action2 = rule2.ProcessTemplateValidationActions.AddNew();
			action2.P0A_ActionSource = "BBB";

			Factory.Save();

			using var testForm = new ZForm(template);
			var validationControl = new ValidationToolUserControl();
			testForm.Controls.Add(validationControl);
			testForm.Show();

			var filter = (ZFilterStripBaseControl)validationControl.Controls.Find("ValidationRulesFilter", true)[0];
			AssertType<ProcessTemplateValidationFilterStripBusinessObject>(filter.FilterBusinessObject);

			var actionSourceFilter = (ModuleTextFilter)filter.FilterBusinessObject["Action Source"];
			actionSourceFilter.IsActive = true;
			actionSourceFilter.Property = "AAA";
			filter.FirePerformSearch();
			AssertContainsExactElementsInAnyOrder(new[] { "Rule 1" }, template.ProcessTemplateValidations.Select(x => x.P0V_Description));
		}
	}
}
