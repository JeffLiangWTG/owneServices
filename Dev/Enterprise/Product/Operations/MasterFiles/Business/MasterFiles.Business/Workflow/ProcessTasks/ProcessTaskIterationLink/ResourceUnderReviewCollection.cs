using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ResourceUnderReviewCollection : ActiveBusinessObjectCollection<GlbStaff>
	{
		public ResourceUnderReviewCollection(ProcessTask qcbTask, BusinessObjectFactory factory)
			: base(factory, new AdhocCollectionRelationship(typeof(GlbStaff)))
		{
			var currentWorkflow = qcbTask?.ProcessHeader;

			if (currentWorkflow != null)
			{
				var prerequisites = currentWorkflow.GetPrerequisitesUpTheTree().ToList();

				var workflows = prerequisites
					.Union(currentWorkflow.GetParentWorkflowsUpTheHierarchy().Where(workflow => workflow.FH_ParentId == currentWorkflow.FH_ParentId))
					.Union(prerequisites.SelectMany(workflow => workflow.GetChildWorkflowsDownTheHierarchy()))
					.Union(currentWorkflow.GetChildWorkflowsDownTheHierarchy())
					.Union(new[] { currentWorkflow });

				var applicableResources = workflows
					.Where(workflow => workflow.IsWorkflow)
					.SelectMany(workflow =>
						workflow.Tasks.Cast<ProcessTask>()
							.Where(task => task.P9_Status == ProcessTaskStatusCodeList.Codes.Closed && !task.P9_GS_NKAssignedStaffMember.IsEmpty)
							.Select(task => task.P9_GS_NKAssignedStaffMember))
							.Distinct();

				var query = new ZQuery(GlbStaffSchema.GS_Code, applicableResources);
				var resourceUnderReviewList = new GlbStaffCollection(Factory, query);
				foreach (var resource in resourceUnderReviewList)
				{
					Add(resource);
				}
			}

			SetReadOnlyIncludingChildren(true);
		}

		protected override bool AllowNew => false;
	}
}
