using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	public class TriggerActionDiagnostics : ITriggerActionDiagnostics
	{
		public ZString GetUnsavedFiredTriggersInformation(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				return ZString.Empty;
			}

			var messageBuilder = new ZStringBuilder();

			var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode);
			filter.FetchOnlyFromLocalCache = true;
			filter.MaximumRows = 100;
			var logs = factory.Load<StmALog>(filter);
			var triggerPKs = logs.Select(x => x.SL_Parent).ToList();
			var triggers = factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.PK, triggerPKs));
			foreach (var trigger in triggers)
			{
				foreach (var action in trigger.ProcessTaskNotifications)
				{
					messageBuilder.AppendLine(action.GetDiagnosticLogInfo());
				}
			}

			return messageBuilder.ToString();
		}
	}
}
