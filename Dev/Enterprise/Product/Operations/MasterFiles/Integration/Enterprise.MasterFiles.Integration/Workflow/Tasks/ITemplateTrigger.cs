using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Integration
{
	public interface ITemplateTrigger : IBaseTrigger, ITemplateConditionalWorkflowItem, ITriggerConditions
	{
		ZBool IsActive { get; set; }
		ZGuid SourceTemplatePK { get; }
		ITemplateTrigger Clone();
		IWorkflowTrigger GetOrCreateJobVersionOfTrigger(IBusiness job, bool createIfNotFound = true);
		ZQuery GetJobVersionOfTriggerQuery(IBusiness job);
	}
}
