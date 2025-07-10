using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[Immutable]
	public class TemplateApplicationRaceHandlingConfig
	{
		public static TemplateApplicationRaceHandlingConfig GetConfig()
		{
			var handleConflictsAutomatically = !Globals.IsUserInteractive || WorkflowDataRegistry.Instance.EnableTemplateApplicationConcurrencyProtection.Value == TemplateApplicationRaceConditionHandlerOptions.Codes.UserInterfaceAndServiceTasks;
			var handleConflictsWhenServiceTasksOnlySelected = Globals.IsUserInteractive && WorkflowDataRegistry.Instance.EnableTemplateApplicationConcurrencyProtection.Value == TemplateApplicationRaceConditionHandlerOptions.Codes.ServiceTasksOnly;
			return new TemplateApplicationRaceHandlingConfig
				(
					tryHandleProcessTaskConflictsAutomatically: handleConflictsAutomatically,
					tryHandleProcessHeaderConflictsAutomatically: handleConflictsAutomatically,
					tryHandleProcessTaskConflictsWhenServiceTasksOnlySelected: handleConflictsWhenServiceTasksOnlySelected
				);
		}

		public TemplateApplicationRaceHandlingConfig(bool tryHandleProcessTaskConflictsAutomatically, bool tryHandleProcessHeaderConflictsAutomatically, bool tryHandleProcessTaskConflictsWhenServiceTasksOnlySelected)
		{
			TryHandleProcessTaskConflictsAutomatically = tryHandleProcessTaskConflictsAutomatically;
			TryHandleProcessHeaderConflictsAutomatically = tryHandleProcessHeaderConflictsAutomatically;
			TryHandleProcessTaskConflictsWhenServiceTasksOnlySelected = tryHandleProcessTaskConflictsWhenServiceTasksOnlySelected;
		}

		public bool TryHandleProcessTaskConflictsAutomatically { get; }
		public bool TryHandleProcessHeaderConflictsAutomatically { get; }
		public bool TryHandleProcessTaskConflictsWhenServiceTasksOnlySelected { get; }
	}
}
