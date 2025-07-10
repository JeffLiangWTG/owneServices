using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class TriggerConditionsViewModelLookups : ZLookups
	{
		public TriggerConditionsViewModelLookups(TriggerConditionsViewModel viewModel)
			: base(viewModel)
		{
			ViewModel = viewModel;
		}

		TriggerConditionsViewModel ViewModel { get; }

		protected IBaseTrigger Trigger => ViewModel.Trigger;

		IWorkflowTrigger JobTrigger => ViewModel.Trigger as IWorkflowTrigger;

		#region MilestoneEventTypes

		public CodeDescriptionPairList MilestoneEventTypes
		{
			get
			{
				var isCompleted = HasTriggerFired();
				var cacheKey = string.Format(CultureInfo.InvariantCulture, "TriggerConditionsViewModelLookups|MilestoneEventTypes|{0}|{1}|{2}",
					Trigger.WorkflowItemType, Trigger.TriggerEventCode, isCompleted);

				return Factory.GetCachedValue(cacheKey, () => EventTypeListProvider.CreateMilestoneEventTypeList(Trigger.WorkflowItemType, Trigger.TriggerEventCode, isCompleted, Factory));
			}
		}

		protected virtual bool HasTriggerFired()
		{
			return !Trigger.IsTask() && (JobTrigger?.LastFiredTime.IsValid ?? false);
		}

		#endregion

		#region Default Event Types

		public static CodeDescriptionPairList GetDefaultEventTypes(BusinessObjectFactory factory)
		{
			var result = new CodeDescriptionPairList();

			result.AddRange(factory.GetCachedValue("TriggerConditionsViewModelLookups|DefaultEventTypes", () => EventTypeListProvider.CreateDefaultEventTypeList()));

			return result;
		}

		#endregion

		#region Trigger Conditions

		public CodeDescriptionPairList TriggerConditionList
		{
			get
			{
				var list = new CodeDescriptionPairList();

				if (Trigger.TriggerEventCode == Events.ExceptionRaisedCode)
				{
					list.AddRange(ExceptionActionConditions);
				}

				list.AddRange(EventReferenceConditions);

				return list;
			}
		}

		#endregion

		#region Action Conditions

		EventReferenceConditionList EventReferenceConditions
		{
			get { return Factory.GetCachedValue<EventReferenceConditionList>(); }
		}

		ExceptionActionConditionList ExceptionActionConditions
		{
			get { return Factory.GetCachedValue<ExceptionActionConditionList>(); }
		}

		#endregion

		#region Trigger Field Names

		public CodeDescriptionPairList WorkflowTriggerFieldNames
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var trigger = Trigger;
				var workflowDescriptor = trigger.GetWorkflowDescriptor();
				if (workflowDescriptor != null)
				{
					foreach (var fieldColumn in workflowDescriptor.GetWorkflowTriggerFieldColumns(trigger.GetJob()))
					{
						var description = workflowDescriptor.GetFieldColumnDescription(Factory, fieldColumn);
						result.AddPair(fieldColumn.Name, description);
					}
				}

				return result;
			}
		}

		#endregion

		#region TriggerUserContexts

		public CodeDescriptionPairList TriggerUserContexts => Factory.GetCachedValue<TriggerUserContextList>();

		#endregion

		#region TriggerStaff

		public GlbStaffCollection TriggerStaff => Factory.GetCachedValue("TriggerConditionViewModelLookups.TriggerStaff", () => new GlbStaffCollection(Factory));

		#endregion

		#region TriggerBranches

		public IBusinessObjectCollection TriggerBranches
		{
			get
			{
				var company = ViewModel.TriggerCompanyBizo;
				if (company != null)
				{
					return company.Branches;
				}
				return Factory.GetCachedValue("TriggerConditionViewModelLookups.TriggerBranches", () => new GlbBranchCollection(Factory));
			}
		}

		#endregion

		#region TriggerCompanies

		public GlbCompanyCollection TriggerCompanies => Factory.GetCachedValue("TriggerConditionViewModelLookups.TriggerCompanies", () => new GlbCompanyCollection(Factory));

		#endregion

		#region TriggerDepartments

		public GlbDepartmentCollection TriggerDepartments => Factory.GetCachedValue("TriggerConditionViewModelLookups.TriggerDepartments", () => new GlbDepartmentCollection(Factory));

		#endregion
	}
}
