using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCapabilityGroupPivot))]
	public class GlbCapabilityGroupPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGroupName()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var pivot = capability.ReleaseGroupPivots.AddNew();
			pivot.GGC_AutoAssignTasksAge = new ZDateTime("2013-01-01 00:00:00");

			AssertEquals(ZGuid.Empty, pivot.GGC_GG_Group);
			AssertEquals(ZString.Empty, pivot.GroupName);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Desc = "Capability Assignment Reference Group";
			pivot.GGC_GG_Group = group.PK;

			Factory.Save();

			AssertEquals("Capability Assignment Reference Group", pivot.GroupName);
		}

		public void TestGGC_AutoAssignTasksAge_ReadOnly()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var pivot = capability.ReleaseGroupPivots.AddNew();

			pivot.GGC_AllowTaskAutoAssignment = true;
			AssertEquals(false, pivot.GGC_AutoAssignTasksAgeInfo.ReadOnly);

			pivot.GGC_AllowTaskAutoAssignment = false;
			AssertEquals(true, pivot.GGC_AutoAssignTasksAgeInfo.ReadOnly);
		}

		public void TestGGC_AutoAssignTasksAge_ZeroByDefault()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var pivot = capability.ReleaseGroupPivots.AddNew();

			AssertEquals(new ZInt(0).GetDateTimeFromMinutes(), pivot.GGC_AutoAssignTasksAge);
		}

		public void TestGGC_AutoAssignTasksAge_SetToZeroIfInvalidOrEmpty_WhenGGC_AllowTaskAutoAssignmentIsFalse()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var invalidPivot = capability.ReleaseGroupPivots.AddNew();
			var emptyPivot = capability.ReleaseGroupPivots.AddNew();

			invalidPivot.GGC_AutoAssignTasksAge = ZDateTime.Invalid;
			invalidPivot.GGC_AllowTaskAutoAssignment = false;

			emptyPivot.GGC_AutoAssignTasksAge = ZDateTime.Empty;
			emptyPivot.GGC_AllowTaskAutoAssignment = false;

			AssertEquals(new ZInt(0).GetDateTimeFromMinutes(), invalidPivot.GGC_AutoAssignTasksAge);
			AssertEquals(new ZInt(0).GetDateTimeFromMinutes(), emptyPivot.GGC_AutoAssignTasksAge);
		}
	}
}
