using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing;

sealed class ValidationToolSettingsTest : TestCaseWithFactory
{
	public void TestSupportsValidationRules()
	{
		AssertEquals("Validation rules should be disabled", false, ValidationToolSettings.SupportsValidationRules);

		using (WorkflowDataRegistry.Instance.EnableWorkflowValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new WorkflowValidationProcessTypeCollection { new WorkflowValidationProcessType { ProcessType = "BRK" } }))
		{
			AssertEquals("Validation rules should be enabled", true, ValidationToolSettings.SupportsValidationRules);
		}
	}

	public void TestIsValidationRulesAvailableForGlobalTemplates()
	{
		AssertEquals(true, ValidationToolSettings.IsValidationRulesAvailableForGlobalTemplates);
	}

	public void TestGetProcessTemplateValidationRootObjectType()
	{
		AssertEquals(typeof(DummyWithWorkflow), ValidationToolSettings.GetProcessTemplateValidationRootObjectType("XXX", "AU"));
	}

	public void TestGetProcessTemplateValidationCondition1List()
	{
		AssertEquals(0, ValidationToolSettings.GetProcessTemplateValidationCondition1List().Count);
	}

	public void TestGetProcessTemplateValidationCondition2List()
	{
		AssertEquals(0, ValidationToolSettings.GetProcessTemplateValidationCondition2List().Count);
	}

	public void TestGetProcessTemplateValidationActionSourceList() => CombineAssertions(() =>
	{
		var list = ValidationToolSettings.GetProcessTemplateValidationActionSourceList("AU");
		AssertEquals("SAV", true, list.ContainsCode(ProcessTemplateValidationActionSourceList.Codes.Save));
		AssertEquals("FSV", false, list.ContainsCode(ProcessTemplateValidationActionSourceList.Codes.FieldSpecificValidation));
		using (ProcessTemplateValidationActionSourceListTest.TemporarilyEnableFSV())
		{
			list = ValidationToolSettings.GetProcessTemplateValidationActionSourceList("AU");
			AssertEquals("SAV", true, list.ContainsCode(ProcessTemplateValidationActionSourceList.Codes.Save));
			AssertEquals("FSV", true, list.ContainsCode(ProcessTemplateValidationActionSourceList.Codes.FieldSpecificValidation));
		}
	});

	public void TestGetProcessTemplateValidationDefaultCondition1()
	{
		AssertEquals(string.Empty, ValidationToolSettings.GetProcessTemplateValidationDefaultCondition1());
	}

	public void TestGetProcessTemplateValidationConditionChecker()
	{
		AssertType<ProcessTemplateValidationConditionChecker>(ValidationToolSettings.GetProcessTemplateValidationConditionChecker(Mock.Of<IProcessTemplateValidation>(), Mock.Of<IBusiness>()));
	}

	public void TestRequestTypeJobType()
	{
		AssertEquals(string.Empty, ValidationToolSettings.RequestTypeJobType);
	}

	WorkflowDescriptor MockWorkflowDescriptor => Mock.Of<WorkflowDescriptor>(x => x.WorkflowProviderType == typeof(DummyWithWorkflow) && x.ControllerID == DummyControllerIDs.Dummy && x.Code == "BRK");
	ValidationToolSettings ValidationToolSettings => validationToolSettings ??= new ValidationToolSettings(MockWorkflowDescriptor);
	ValidationToolSettings validationToolSettings;
}
