using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public abstract class TaskStatusChangedEventParametersStrategy
	{
		[SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public IEnumerable<KeyValuePair<string, string>> GetLogReferenceParameters(ProcessTask processTask)
		{
			if (!processTask.P9_GS_NKAssignedStaffMember.IsEmpty)
			{
				yield return new KeyValuePair<string, string>(ProcessTaskStatusCodeList.Codes.Assigned, processTask.P9_GS_NKAssignedStaffMember);
			}

			if (!processTask.P9_GG_AssignedGroupCode.IsEmpty)
			{
				yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Group, processTask.P9_GG_AssignedGroupCode);
			}

			foreach (var param in GetAdditionalParameters(processTask))
			{
				yield return param;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures", Justification = "")]
		protected abstract IEnumerable<KeyValuePair<string, string>> GetAdditionalParameters(ProcessTask processTask);

		public void AddFetchHints(ProcessTask processTask, BusinessObjectFactory factory) => AddFetchHintsCore(processTask, factory);

		protected abstract void AddFetchHintsCore(ProcessTask processTask, BusinessObjectFactory factory);
	}
}
