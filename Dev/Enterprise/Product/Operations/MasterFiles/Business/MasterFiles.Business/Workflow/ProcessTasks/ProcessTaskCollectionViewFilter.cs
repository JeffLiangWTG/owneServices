using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskCollectionViewFilter : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ProcessTaskCollectionViewFilter(ProcessTaskCollection workflowItems)
			: base(workflowItems.Factory)
		{
			tasksView = workflowItems.Tasks;
			RegisterEditableChildObject(tasksView);
			workflowItems.Parent.RegisterEditableChildObject(this);
			tasksView.TasksViewFilter = this;
		}

		public ProcessTaskCollectionView TasksView
		{
			get { return tasksView; }
		}
		readonly ProcessTaskCollectionView tasksView;

		#region Filter

		#region P9_Status
		[List("Lookups.Statuses")]
		[MaxLength(ProcessTask.Schema.P9_StatusMaxLength)]
		public ZString P9_Status
		{
			get { return fP9_Status; }
			set
			{
				CheckMaximumLength(P9_StatusInfo, value);
				fP9_Status = value;
				P9_StatusInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo P9_StatusInfo
		{
			get { return GetZPropertyInfo(nameof(P9_Status)); }
		}

		ZString fP9_Status;

		#endregion

		#region P9_Type
		[List("Lookups.Types")]
		[MaxLength(ProcessTask.Schema.P9_TypeMaxLength)]
		public ZString P9_Type
		{
			get { return fP9_Type; }
			set
			{
				CheckMaximumLength(P9_TypeInfo, value);
				fP9_Type = value;
				P9_TypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo P9_TypeInfo
		{
			get { return GetZPropertyInfo(nameof(P9_Type)); }
		}

		ZString fP9_Type;

		#endregion

		#region P9_GS_NKAssignedStaffMember

		[MaxLength(3)]
		[List("Lookups.Staff")]
		public ZString P9_GS_NKAssignedStaffMember
		{
			get { return fP9_GS_NKAssignedStaffMember; }
			set
			{
				CheckMaximumLength(P9_GS_NKAssignedStaffMemberInfo, value);
				fP9_GS_NKAssignedStaffMember = value;
				P9_GS_NKAssignedStaffMemberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo P9_GS_NKAssignedStaffMemberInfo
		{
			get { return GetZPropertyInfo(nameof(P9_GS_NKAssignedStaffMember)); }
		}

		ZString fP9_GS_NKAssignedStaffMember;

		#endregion

		#region P9_GG_AssignedGroup
		[List("Lookups.Groups")]
		public ZGuid P9_GG_AssignedGroup
		{
			get { return fP9_GG_AssignedGroup; }
			set
			{
				fP9_GG_AssignedGroup = value;
				P9_GG_AssignedGroupInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo P9_GG_AssignedGroupInfo
		{
			get { return GetZPropertyInfo(nameof(P9_GG_AssignedGroup)); }
		}

		ZGuid fP9_GG_AssignedGroup;

		#endregion

		public void FilterTasks()
		{
			SortInfo info = TasksView.SortInformation;
			TasksView.Rebuild(P9_Status, P9_Type, P9_GS_NKAssignedStaffMember, P9_GG_AssignedGroup);

			if (info != null)
			{
				TasksView.Sort(info);
			}
		}

		public void ClearTasksFilter()
		{
			P9_Status = "";
			P9_Type = "";
			P9_GS_NKAssignedStaffMember = "";
			P9_GG_AssignedGroup = ZGuid.Empty;

			P9_StatusInfo.RefreshBinding();
			P9_TypeInfo.RefreshBinding();
			P9_GS_NKAssignedStaffMemberInfo.RefreshBinding();
			P9_GG_AssignedGroupInfo.RefreshBinding();

			TasksView.Rebuild(P9_Status, P9_Type, P9_GS_NKAssignedStaffMember, P9_GG_AssignedGroup);
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearTasksFilter();
			FilterTasks();
			base.RunPreSaveValidationCore();
		}

		#endregion

		#region Lookups

		public ProcessTaskCollectionViewFilterLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = new ProcessTaskCollectionViewFilterLookups(this);
				}
				return fLookups;
			}
		}

		ProcessTaskCollectionViewFilterLookups fLookups;

		#endregion
	}
}
