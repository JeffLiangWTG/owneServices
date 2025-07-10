using CargoWise.EntityFramework;
using Enterprise.Registry.Business;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskCollectionViewFilterLookups : ZLookups
	{
		public ProcessTaskCollectionViewFilterLookups(ProcessTaskCollectionViewFilter parent)
			: base(parent)
		{
		}

		protected new ProcessTaskCollectionViewFilter Parent
		{
			get { return (ProcessTaskCollectionViewFilter)base.Parent; }
		}

		#region Types

		public WorkflowTaskTypeCollection Types
		{
			get
			{
				if (fTypes == null)
				{
					ProcessTaskCollection collection = (ProcessTaskCollection)Parent.TasksView.CollectionToFilter;
					fTypes = WorkflowDataRegistry.Instance.TaskTypes.Value.GetTaskTypesFromWorkflowCode(collection.WorkflowType);
				}
				return fTypes;
			}
		}

		WorkflowTaskTypeCollection fTypes;

		#endregion

		#region Statuses

		public CodeDescriptionPairList Statuses
		{
			get
			{
				if (fStatuses == null)
				{
					fStatuses = new ProcessTasksLookups(Factory).Statuses;
				}
				return fStatuses;
			}
		}

		CodeDescriptionPairList fStatuses;

		#endregion

		#region Staff

		public GlbStaffCollection Staff
		{
			get
			{
				if (fStaff == null)
				{
					fStaff = new GlbStaffCollection(Factory);
				}
				return fStaff;
			}
		}

		GlbStaffCollection fStaff;

		#endregion

		#region Groups

		public GlbGroupCollection Groups
		{
			get
			{
				if (fGroups == null)
				{
					fGroups = new GlbGroupCollection(Factory);
				}
				return fGroups;
			}
		}

		GlbGroupCollection fGroups;

		#endregion

		protected override BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = base.Factory ?? new BusinessObjectFactory();
				}
				return fFactory;
			}
		}

		BusinessObjectFactory fFactory;
	}
}
