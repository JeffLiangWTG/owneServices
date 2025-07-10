using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ProcessManagement.Business
{
	/// <summary>
	/// Validation actually for the WorkItem class, since WorkItemValidation is already used as the validation for the base WorkItemCommon class.
	/// </summary>
	public class WorkItemActualValidation : WorkItemCommonValidation
	{
		public WorkItemActualValidation(WorkItem parent)
			: base(parent)
		{
		}

		new WorkItem Parent
		{
			get { return (WorkItem)base.Parent; }
		}

		protected override void CheckWKI_WorkItemType()
		{
			CheckActiveCode(Parent.WKI_WorkItemTypeInfo, Parent.Lookups.AllTypes, Parent.Lookups.ActiveTypes);
			CheckMandatoryField(ProcessManagementRegistry.Instance.WorkItemTypeMandatory, Parent.WKI_WorkItemTypeInfo, Parent.Lookups.ActiveTypes);
		}

		protected override void CheckWKI_WorkItemArea()
		{
			CheckActiveCode(Parent.WKI_WorkItemAreaInfo, Parent.Lookups.AllAreas, Parent.Lookups.ActiveAreas);
			CheckMandatoryField(ProcessManagementRegistry.Instance.WorkItemAreaMandatory, Parent.WKI_WorkItemAreaInfo, Parent.Lookups.ActiveAreas);
		}

		protected override void CheckWKI_ActivityType()
		{
			CheckActiveCode(Parent.WKI_ActivityTypeInfo, Parent.Lookups.AllActivityTypes, Parent.Lookups.ActiveActivityTypes);
			CheckMandatoryField(ProcessManagementRegistry.Instance.WorkItemActivityTypeMandatory, Parent.WKI_ActivityTypeInfo, Parent.Lookups.ActiveActivityTypes);
		}

		protected override void CheckWKI_ActivitySubtype()
		{
			CheckActiveCode(Parent.WKI_ActivitySubtypeInfo, Parent.Lookups.AllActivitySubtypes, Parent.Lookups.ActiveActivitySubtypes);
			CheckMandatoryField(ProcessManagementRegistry.Instance.WorkItemActivitySubTypeMandatory, Parent.WKI_ActivitySubtypeInfo, Parent.Lookups.ActiveActivitySubtypes);
		}

		protected override void CheckWKI_Priority()
		{
			CheckActiveCode(Parent.WKI_PriorityInfo, Parent.Lookups.AllPriorities, Parent.Lookups.ActivePriorities);
			CheckMandatoryField(ProcessManagementRegistry.Instance.WorkItemPriorityMandatory, Parent.WKI_PriorityInfo, Parent.Lookups.ActivePriorities);
		}

		protected override void CheckWKI_Status()
		{
			if (!Parent.WKI_Status.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.WKI_StatusInfo);
			}
		}

		protected override void CheckWKI_Summary()
		{
			MandatoryValidation.CheckEntered(Parent.WKI_SummaryInfo);
		}

		void CheckActiveCode(ZPropertyInfo info, ICodeDescriptionPairList all, ICodeDescriptionPairList active)
		{
			if (!info.Value.IsEmpty)
			{
				if (!Parent.IsInDatabase || info.HasChanges || !all.ContainsCode(info.Value))
				{
					ListValidation.ErrorIfInvalidCode(info);
				}
				else
				{
					ListValidation.WarnIfInvalidCode(info, active, ListValidation.InactiveCodeMessage);
				}
			}
		}

		void CheckMandatoryField(BooleanRegistryItem registryItem, ZPropertyInfo info, ICodeDescriptionPairList valueList)
		{
			if (registryItem.Value && info.Value.IsEmpty && valueList.Count > 0)
			{
				MandatoryValidation.CheckEntered(info);
			}
		}

		#region Defect Caused By Work Item Validation

		public void ValidateDefectCausedByWorkItemPK()
		{
			ValidateCalculatedProperty(Parent.DefectCausedByWorkItemPKInfo);
		}

		protected void CheckDefectCausedByWorkItemPK()
		{
			ListValidation.ErrorIfInvalidPK(Parent.DefectCausedByWorkItemPKInfo);
			if (!Parent.DefectCausedByWorkItemPK.IsEmpty)
			{
				if (Parent.Lookups.DefectCausedByTasks.Count == 0)
				{
					Parent.DefectCausedByWorkItemPKInfo.AddError(Res.GetString("F9691AEC-06F4-4785-94D7-2C06C9C6EEF2", "Work Item must have at least one closed task of the type that can introduce a defect."));
				}
			}
		}

		#endregion

		#region Defect Caused By Work Item Process Task Validation

		public void ValidateDefectCausedByTaskPK()
		{
			ValidateCalculatedProperty(Parent.WKI_P9_DefectCausedByTaskInfo);
		}

		protected override void CheckWKI_P9_DefectCausedByTask()
		{
			if (!Parent.WKI_P9_DefectCausedByTask_ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.WKI_P9_DefectCausedByTaskInfo);
				ListValidation.ErrorIfInvalidPK(Parent.WKI_P9_DefectCausedByTaskInfo);

				if (!Parent.WKI_P9_DefectCausedByTask.IsEmpty && !Parent.WKI_P9_DefectCausedByTaskInfo.HasErrors())
				{
					if (!Parent.IsTargetTaskLinkedToTargetDefectCausedWI())
					{
						Parent.WKI_P9_DefectCausedByTaskInfo.AddError(Res.GetString("C994F40C-7F14-479B-B452-937150D59A93", "Choose a task that is linked to the Defect Introduced in Work Item."));
					}

					if (Parent.DefectCausedByTask.P9_Status != ProcessTaskStatusCodeList.Codes.Closed)
					{
						Parent.WKI_P9_DefectCausedByTaskInfo.AddWarning(Res.GetString("DB39819F-706C-44A2-B910-88B4EB271EED", "Non closed task cannot cause a Defect."));
					}
				}
			}
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDefectCausedByWorkItemPK();
			ValidateDefectCausedByTaskPK();
		}
	}
}
