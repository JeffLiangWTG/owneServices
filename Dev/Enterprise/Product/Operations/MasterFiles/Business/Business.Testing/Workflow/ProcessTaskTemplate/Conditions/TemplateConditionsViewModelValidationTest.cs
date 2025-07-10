using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TemplateConditionsViewModelValidationTest : BusinessObjectValidationTestCase
	{
		public void TestTemplateCondition1()
		{
			AssertListValidationInvalidCodeError(viewModel.TemplateCondition1Info, false);
			viewModel.TemplateCondition1 = "XXX";
			AssertListValidationInvalidCodeError(viewModel.TemplateCondition1Info, true);
		}

		public void TestTemplateCondition2()
		{
			AssertListValidationInvalidCodeError(viewModel.TemplateCondition2Info, false);
			viewModel.TemplateCondition2 = "XXX";
			AssertListValidationInvalidCodeError(viewModel.TemplateCondition2Info, true);
		}

		public void TestTemplateCondition2Value()
		{
			template.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;
			viewModel.TemplateCondition2 = JobShipmentWorkflowCondition2CodeList.Codes.ReleaseType;

			viewModel.TemplateCondition2Value = "XXX";
			AssertHasError(viewModel.TemplateCondition2ValueInfo, "Enter a valid Template Condition 2 Value.");

			viewModel.TemplateCondition2Value = viewModel.Lookups.TemplateCondition2ValueList[0].Code;
			AssertNoNotifications(viewModel.TemplateCondition2ValueInfo);

			viewModel.TemplateCondition2Value = "";
			AssertMandatoryValidationError(viewModel.TemplateCondition2ValueInfo, true);

			viewModel.TemplateCondition2 = JobShipmentWorkflowCondition2CodeList.Codes.AssemblyMaster;
			viewModel.TemplateCondition2Value = "";
			AssertNoNotifications(viewModel.TemplateCondition2ValueInfo);

			viewModel.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			viewModel.TemplateCondition2Value = "";
			AssertMandatoryValidationError(viewModel.TemplateCondition2ValueInfo, true);

			viewModel.TemplateCondition2Value = "\"<JS_UniqueConsignRef>\" == \"S0001000\"";
			AssertNoNotifications(viewModel.TemplateCondition2ValueInfo);

			viewModel.TemplateCondition2 = ProcessTasksLookups.MacroCondition;
			viewModel.TemplateCondition2Value = "";
			AssertMandatoryValidationError(viewModel.TemplateCondition2ValueInfo, true);

			viewModel.TemplateCondition2 = ProcessTasksLookups.MacroCondition;
			viewModel.TemplateCondition2Value = "JS_UniqueConsignRef == \"S0001000\"";
			AssertNoNotifications(viewModel.TemplateCondition2ValueInfo);
		}

		public void TestTemplateCondition2Value_NoUnicodeSupport()
		{
			viewModel.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			viewModel.TemplateCondition2Value = "Rubber baby buggy bumpers. M○○♣rty.";

			AssertEquals("Rubber baby buggy bumpers. M○○♣rty.", viewModel.TemplateCondition2Value);
			AssertHasError(viewModel.TemplateCondition2ValueInfo, "Template Condition 2 Value only accepts Western European languages characters.");
		}

		#region Implementation

		ProcessTaskTemplate template;
		TemplateConditionsViewModel viewModel;

		protected override void SetUp()
		{
			base.SetUp();

			template = Factory.New<ProcessTaskTemplate>();

			var trigger = (ITemplateTrigger)template.TemplateTriggers.AddNew();
			viewModel = new TemplateConditionsViewModel(trigger, template);
		}

		#endregion
	}
}
