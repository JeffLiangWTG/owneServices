using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NO.Business;

sealed class CusNOEmmaMessageGeneratorWorkflowDescriptor : WorkflowDescriptor
{
	public override string Code => WorkflowDescriptors.CusNOEmmaMessageGeneratorWorkflowDescriptorCode;

	public override IMultilingualString Description => ResString.GetMultilingualString("de425fa0-b55e-4861-9467-1dd5bc710bd6", "EMMA Message Generation");

	public override Type WorkflowProviderType => typeof(CusEntryHeader);

	public override ControllerID ControllerID => null;

	public override bool SupportsEventTracking => true;

	public override bool AreTasksCompanySpecific => true;

	public override bool SupportsUniversalTemplates => false;

	public override bool SupportsBufferManagement => false;

	public override bool SupportsWorkflowTemplates => false;

	public override bool RequiresBranch => true;

	public override bool RequiresDepartment => false;

	public override bool RequiresClient => false;

	protected override bool SupportEmmaMessageGeneration => true;

	protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
	{
		var triggerType = source.Action?.PQ_TriggerType ?? ZString.Empty;
		return triggerType.ToString() switch
		{
			WorkflowTriggerActionTypeConstants.Codes.CusNOEmmaMessageGenerator => GetEmmaMessageGenerationProcessor(source.Job),
			_ => base.GetWorkflowTriggerActionCore(source, queuedLog)
		};
	}

	static IProcessor GetEmmaMessageGenerationProcessor(BusinessObject businessObject)
	{
		return businessObject is Integration.Customs.NO.IEmmaMessageGenerationProcessorProvider provider
			? provider.CreateStmProcessQueueProcessor(businessObject, WorkflowTriggerActionTypeConstants.Codes.CusNOEmmaMessageGenerator)
			: null;
	}
}
