using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Business
{
	public class CusISFBillWorkflowModifiedFieldChangeTriggerProcessorExtender : IWorkflowModifiedFieldChangeTriggerProcessorExtender
	{
		#region IWorkflowModifiedFieldChangeTriggerProcessorExtender Members

		public IWorkflowTrigger[] GetTriggersToRun(StmChangeLog changeLog, string[] changedPropertyNames)
		{
			string sql = @"
P9_PK IN (

SELECT P9_PK
FROM dbo.CusISFBill 
INNER JOIN dbo.CusISFHeader  ON BB_BF = BF_PK
INNER JOIN dbo.ProcessTasks  ON P9_ParentID = BF_PK
WHERE BB_PK = @ParentID
  AND P9_TriggerField IN ('" + string.Join("', '", changedPropertyNames) + @"')
  AND (P9_Type = 'TRG' OR P9_Type='MIL')

)
";
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ProcessTask));
			ZSqlParameterCollection parameter = new ZSqlParameterCollection(ZSqlParameter.New("@ParentID", changeLog.SY_ParentID.ToGuid(), StmChangeLogSchema.SY_ParentID));
			query.AddFilterAndZSQLParameterCollection(sql, parameter);

			List<ProcessTask> result = new List<ProcessTask>();
			foreach (ProcessTask trigger in changeLog.Factory.Load<ProcessTask>(query))
			{
				CusISFHeaderProcessTask isfWorkflowItem = trigger as CusISFHeaderProcessTask;
				if (trigger.P9_ReferencedID.IsEmpty || isfWorkflowItem.Parent != null)
				{
					result.Add(trigger);
				}
			}
			return result.ToArray();
		}

		#endregion
	}
}
