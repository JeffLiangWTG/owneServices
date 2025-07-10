using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	public class ViewProcessTaskCollection : ActiveBusinessObjectCollection<ViewProcessTask>
	{
		public ViewProcessTaskCollection(IWorkflow workflow)
			: base(((IBusiness)workflow).Factory, GetQuery(ViewProcessTaskSchema.P9_FH_ProcessHeader, workflow))
		{
			parentID = workflow.ParentId;
			parentTableCode = workflow.ParentTableCode;
		}

		public ViewProcessTaskCollection(IWorkflowProvider job)
			: base(((IBusiness)job).Factory, GetQuery(ViewProcessTaskSchema.P9_ParentID, job))
		{
			parentID = job.PK;
			parentTableCode = ((BusinessObject)job).TablePrefix;
		}

		readonly ZGuid parentID;
		readonly ZString parentTableCode;

		static ZQuery GetQuery(SchemaGuidColumn pkColumn, IIdentified parent)
		{
			return new ZQuery(pkColumn, parent.Identifier) { FetchOnlyFromLocalCache = !((BusinessObject)parent).IsInDatabase };
		}

		#region ActiveBusinessObjectCollection Overrides

		protected override void SetDefaultsForNewElementCore(ViewProcessTask newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.P9_ParentID = parentID;
			newElement.P9_ParentTableCode = parentTableCode;
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		#endregion
	}
}
