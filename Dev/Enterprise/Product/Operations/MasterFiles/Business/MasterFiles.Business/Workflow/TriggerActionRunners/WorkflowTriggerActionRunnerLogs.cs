using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public static class WorkflowTriggerActionRunnerLogs
	{
		public static void FiringAction(IProcessTaskNotification action, INotifications notifications)
		{
			notifications.AddVerboseInfo(() => Res.GetString("bffc42ed-0a17-4e09-8337-dce818b2eefe", "Firing action: {0}", action.PQ_TriggerType));
		}

		public static void ActionComplete(IProcessTaskNotification action, INotifications notifications)
		{
			notifications.AddVerboseInfo(() =>
			{
				var factory = action.Factory;
				var bizoCount = ((IBusinessObjectFactoryInternals)factory).NumberOfBusinessObjects;
				var hitCount = factory.TableSelects.Length;
				return Res.GetString("e6d160f7-8570-4038-b7ec-6e2efbe2e459", "Action completed: {0} ({1} objects loaded, {2} table selects)", action.PQ_TriggerType, bizoCount, hitCount);
			});
		}
	}
}
