using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ProcessManagement.Integration
{
	[SupportGridColour]
	public interface IWorkTaskRelatedItem
	{
		ZGuid PK { get; }

		ZString ClientName { get; }
		ZPropertyInfo ClientNameInfo { get; }

		ZString ClientCode { get; }
		ZPropertyInfo ClientCodeInfo { get; }

		ZString Type { get; }
		ZPropertyInfo TypeInfo { get; }

		ZString Number { get; }
		ZPropertyInfo NumberInfo { get; }

		ZString StatusDescription { get; }
		ZPropertyInfo StatusDescriptionInfo { get; }

		ZString AssignedStaffCode { get; }
		ZPropertyInfo AssignedStaffCodeInfo { get; }

		ZString ItemDescription { get; }
		ZPropertyInfo ItemDescriptionInfo { get; }

		ZString Criticality { get; }
		ZPropertyInfo CriticalityInfo { get; }

		ZString Source { get; }
		ZPropertyInfo SourceInfo { get; }

		ZBool IsClosedOrCancelled { get; }

		ControllerID ControllerID { get; }

		ZString SelectionCriterion1 { get; }
		ZPropertyInfo SelectionCriterion1Info { get; }

		ZString SelectionCriterion2 { get; }
		ZPropertyInfo SelectionCriterion2Info { get; }

		ZString SelectionCriterion3 { get; }
		ZPropertyInfo SelectionCriterion3Info { get; }

		ZString SelectionCriterion4 { get; }
		ZPropertyInfo SelectionCriterion4Info { get; }

		ZString SelectionCriterion5 { get; }
		ZPropertyInfo SelectionCriterion5Info { get; }

		Type PivotCollectionType { get; }
	}
}
