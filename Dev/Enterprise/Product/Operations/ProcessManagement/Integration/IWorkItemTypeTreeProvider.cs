using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ProcessManagement.Integration
{
	public interface IWorkItemTypeTreeProvider
	{
		CodeDescriptionPairList GetWorkItemTypes(bool activeOnly);
		CodeDescriptionPairList GetWorkItemAreas(ZString parentType, bool activeOnly);
		CodeDescriptionPairList GetActivityTypes(ZString parentType, ZString parentArea, bool activeOnly);
		CodeDescriptionPairList GetActivitySubtypes(ZString parentType, ZString parentArea, ZString activityType, bool activeOnly);
		CodeDescriptionPairList GetPriorities(ZString parentType, ZString parentArea, ZString activityType, ZString activitySubType, bool activeOnly);
		IMultilingualString GetWorkItemCriterionLabel(int number);
	}
}
