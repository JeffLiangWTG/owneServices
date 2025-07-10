using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.DataTransfer;

sealed class ForwardingShipmentValidationToolSettings : ValidationToolSettings
{
	new ForwardingShipmentWorkflowDescriptor WorkflowDescriptor => (ForwardingShipmentWorkflowDescriptor)base.WorkflowDescriptor;

	public ForwardingShipmentValidationToolSettings(ForwardingShipmentWorkflowDescriptor workflowDescriptor) : base(workflowDescriptor)
	{
	}

	protected override bool IsValidationRulesAvailableForGlobalTemplatesCore() => false;

	protected override Type GetProcessTemplateValidationRootObjectTypeCore(ZString condition1, ZString countryCode)
	{
		var result = typeof(ForwardingShipment);
		if (condition1 == JobShipmentWorkflowCondition1CodeList.Codes.BrokerageAttached)
		{
			result = ((CountrySpecificTypeDecider)TypeDecider.GetTypeDeciderFromType(WorkflowDescriptor.DeclarationType)).GetTypeForCountryCode(countryCode);
		}
		return result;
	}

	protected override CodeDescriptionPairList GetProcessTemplateValidationCondition1ListCore()
	{
		var result = new CodeDescriptionPairList();
		result.AddPair(string.Empty, string.Empty);
		result.AddPair(JobShipmentWorkflowCondition1CodeList.Codes.BrokerageAttached, JobShipmentWorkflowCondition1CodeList.Descriptions.BrokerageAttached);
		return result;
	}

	protected override CodeDescriptionPairList GetProcessTemplateValidationCondition2ListCore()
	{
		return new JobShipmentWorkflowCondition2CodeList();
	}

	protected override MasterFiles.Business.ProcessTemplateValidationConditionChecker GetProcessTemplateValidationConditionCheckerCore(IProcessTemplateValidation rule, IBusiness businessEntity)
	{
		return businessEntity is ForwardingShipment shipment ?  new ProcessTemplateValidationConditionChecker(rule, shipment) : null;
	}

	public override string RequestTypeJobType => ExternalRequestTypeJobTypes.Codes.SHP;
}
