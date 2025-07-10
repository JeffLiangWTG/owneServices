using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing;

sealed class ForwardingShipmentValidationToolSettingsTest : TestCaseWithFactory
{
	public void TestIsValidationRulesAvailableForGlobalTemplates()
	{
		AssertEquals(false, ValidationToolSettings.IsValidationRulesAvailableForGlobalTemplates);
	}

	public void TestGetProcessTemplateValidationRootObjectType()
	{
		AssertEquals(typeof(ForwardingShipment), ValidationToolSettings.GetProcessTemplateValidationRootObjectType(ZString.Empty, ZString.Empty));

		var type = ValidationToolSettings.GetProcessTemplateValidationRootObjectType(JobShipmentWorkflowCondition1CodeList.Codes.BrokerageAttached, ZString.Empty);
		AssertEquals(typeof(BaseJobDeclaration), ValidationToolSettings.GetProcessTemplateValidationRootObjectType(JobShipmentWorkflowCondition1CodeList.Codes.BrokerageAttached, ZString.Empty));

		type = ValidationToolSettings.GetProcessTemplateValidationRootObjectType(JobShipmentWorkflowCondition1CodeList.Codes.BrokerageAttached, Constants.CountryCodes.Australia);
		AssertEquals("Enterprise.Customs.AU.Declaration.Business.JobDeclaration", type.FullName);
	}

	public void TestGetProcessTemplateValidationCondition1List()
	{
		AssertEquals(", BRK", ValidationToolSettings.GetProcessTemplateValidationCondition1List().CodesAsString);
	}

	public void TestGetProcessTemplateValidationCondition2List()
	{
		AssertType<JobShipmentWorkflowCondition2CodeList>(ValidationToolSettings.GetProcessTemplateValidationCondition2List());
	}

	public void TestRequestTypeJobType()
	{
		AssertEquals("SHP", ValidationToolSettings.RequestTypeJobType);
	}

	public void TestGetProcessTemplateValidationConditionChecker() => CombineAssertions(() =>
	{
		AssertNull("Unknown Entity", ValidationToolSettings.GetProcessTemplateValidationConditionChecker(Mock.Of<IProcessTemplateValidation>(x => x.Factory == Factory), Mock.Of<IBusiness>()));
		AssertType<ProcessTemplateValidationConditionChecker>("ForwardingShipment", ValidationToolSettings.GetProcessTemplateValidationConditionChecker(Mock.Of<IProcessTemplateValidation>(x => x.Factory == Factory), Factory.New<ForwardingShipment>()));
	});

	ForwardingShipmentWorkflowDescriptor WorkflowDescriptor => workflowDescriptor ??= new ForwardingShipmentWorkflowDescriptor();
	ForwardingShipmentWorkflowDescriptor workflowDescriptor;

	ForwardingShipmentValidationToolSettings ValidationToolSettings => validationToolSettings ??= new ForwardingShipmentValidationToolSettings(WorkflowDescriptor);
	ForwardingShipmentValidationToolSettings validationToolSettings;
}
