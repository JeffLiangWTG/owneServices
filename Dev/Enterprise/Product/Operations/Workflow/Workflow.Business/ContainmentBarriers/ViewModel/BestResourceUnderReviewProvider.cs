using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	internal class BestResourceUnderReviewProvider
	{
		internal static ZString FindDefaultResourceUnderReview(ProcessTask qcbTask)
		{
			return GetDefaultResourceNK(qcbTask.ProcessHeader, qcbTask.P9_Sequence);
		}

		static ZString GetDefaultResourceNK(IProcessHeader rootWorkflow, int qcbTaskSequence = -1)
		{
			var defaultResourceNK = ZString.Empty;

			if (rootWorkflow != null)
			{
				var existingWorkflowsPK = new HashSet<ZGuid>();

				var queue = new Queue<IProcessHeader>();
				queue.Enqueue(rootWorkflow);

				while (queue.Count > 0)
				{
					var currentWorkflow = queue.Dequeue();

					if (!existingWorkflowsPK.Add(currentWorkflow.PK))
					{
						continue;
					}

					defaultResourceNK = GetDefaultResourceNKInWorkflow(currentWorkflow, rootWorkflow.PK == currentWorkflow.PK ? qcbTaskSequence : -1);

					if (defaultResourceNK.IsEmpty)
					{
						var prerequisites = GetPrerequisites(currentWorkflow);

						foreach (var prerequisite in prerequisites)
						{
							queue.Enqueue(prerequisite);
						}

						var parents = GetParents(currentWorkflow);

						foreach (var parent in parents)
						{
							queue.Enqueue(parent);
						}
					}
					else
					{
						break;
					}
				}
			}

			return GetResourceNKIfExist(defaultResourceNK, rootWorkflow?.Factory);
		}

		static IEnumerable<IProcessHeader> GetPrerequisites(IProcessHeader workflow)
		{
			return workflow
				.PrerequisiteLinks
				.Where(link => link.HeaderFrom != null && link.HeaderFrom.IsWorkflow)
				.Select(link => link.HeaderFrom)
				.OrderByDescending(w => w.FH_SystemLastEditTimeUtc);
		}

		static IEnumerable<IProcessHeader> GetParents(IProcessHeader workflow)
		{
			return workflow
				.ParentLinks
				.Where(link => link.HeaderTo != null && link.HeaderTo.IsWorkflow)
				.Select(link => link.HeaderTo)
				.OrderByDescending(w => w.FH_SystemLastEditTimeUtc);
		}

		static ZString GetResourceNKIfExist(ZString resourceNK, BusinessObjectFactory factory)
		{
			return !resourceNK.IsEmpty && factory != null && factory.Exists(typeof(GlbStaff), new ZQuery(GlbStaffSchema.GS_Code, resourceNK))
				? resourceNK
				: ZString.Empty;
		}

		static ZString GetDefaultResourceNKInWorkflow(IProcessHeader workflow, int qcbSequence = -1)
		{
			var maxDurationClosedTask = workflow.Tasks.Cast<ProcessTask>()
				.Where(task => task.P9_Status == ProcessTaskStatusCodeList.Codes.Closed && (qcbSequence == -1 || task.P9_Sequence < qcbSequence))
				.GroupBy(task => task.P9_GS_NKAssignedStaffMember)
				.Select(tasksGroup => new { tasksGroup.Key, TotalDuration = tasksGroup.Sum(task => task.ActualDurationHours) })
				.OrderByDescending(tasksGroup => tasksGroup.TotalDuration)
				.FirstOrDefault();

			return maxDurationClosedTask?.Key ?? ZString.Empty;
		}
	}
}
