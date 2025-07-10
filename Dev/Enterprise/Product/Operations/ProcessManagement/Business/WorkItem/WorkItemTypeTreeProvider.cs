using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkItemTypeTreeProvider : IWorkItemTypeTreeProvider
	{
		public CodeDescriptionPairList GetWorkItemTypes(bool activeOnly)
		{
			var tree = ProcessManagementRegistry.Instance.WorkItemTypeTree.Value;
			return tree.GetParents(activeOnly);
		}

		public CodeDescriptionPairList GetWorkItemAreas(ZString parentType, bool activeOnly)
		{
			var tree = ProcessManagementRegistry.Instance.WorkItemTypeTree.Value;
			return tree.GetChildren(parentType, activeOnly);
		}

		public CodeDescriptionPairList GetActivityTypes(ZString parentType, ZString parentArea, bool activeOnly)
		{
			var tree = ProcessManagementRegistry.Instance.WorkItemTypeTree.Value;
			return tree.GetChildren(parentType, parentArea, activeOnly);
		}

		public CodeDescriptionPairList GetActivitySubtypes(ZString parentType, ZString parentArea, ZString activityType, bool activeOnly)
		{
			var tree = ProcessManagementRegistry.Instance.WorkItemTypeTree.Value;
			return tree.GetChildren(activeOnly, parentType, parentArea, activityType);
		}

		public CodeDescriptionPairList GetPriorities(ZString parentType, ZString parentArea, ZString activityType, ZString activitySubType, bool activeOnly)
		{
			var tree = ProcessManagementRegistry.Instance.WorkItemTypeTree.Value;
			return tree.GetChildren(activeOnly, parentType, parentArea, activityType, activitySubType);
		}

		public IMultilingualString GetWorkItemCriterionLabel(int number)
		{
			switch (number)
			{
				case 1: return ProcessManagementRegistry.Instance.WorkItemTypeLabel.Value;
				case 2: return ProcessManagementRegistry.Instance.WorkItemAreaLabel.Value;
				case 3: return ProcessManagementRegistry.Instance.WorkItemActivityTypeLabel.Value;
				case 4: return ProcessManagementRegistry.Instance.WorkItemActivitySubTypeLabel.Value;
				case 5: return ProcessManagementRegistry.Instance.WorkItemPriorityLabel.Value;
				default: return null;
			}
		}
	}
}
