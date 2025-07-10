using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	class GetGRPCapabilitiesWithLastStaffPerGroupTest : TestCaseWithFactory
	{
		public void TestGetGRPCapabilitiesWithLastStaffPerGroup()
		{
			/* Capability-Group Combination		Staff
			 * ============================     =====
			 * C1-G1							S1, S2
			 * C1-G2							S1
			 * C2-G1							S1
			 * C2-G2							S1, S3
			 * C3-G1							S1
			 * C3-G2							S1
			 *
			 * deleting G1, G2 from S1 will make 2nd, 3rd, 5th and 6th Capability-Group combinations empty
			 */

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "G2";

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "C2";
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var capability3 = Factory.NewWithValidTestData<GlbCapability>();
			capability3.G4_Code = "C3";
			capability3.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Capabilities.Add(capability2);
			staff1.Capabilities.Add(capability3);
			staff1.Groups.Add(group1);
			staff1.Groups.Add(group2);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "S2";
			staff2.Capabilities.Add(capability1);
			staff2.Groups.Add(group1);

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "S3";
			staff3.Capabilities.Add(capability2);
			staff3.Groups.Add(group2);

			Factory.Save();

			var groupsSelectedForDelete = new BusinessObject[] { group1, group2 };
			var collection = StaffCapabilityGroupHelper.GetGRPCapabilitiesWithLastStaffPerGroup(Factory, groupsSelectedForDelete, staff1.PK);

			AssertEquals(2, collection.Count);
			Assert(collection["G1"].ContainsSameElementsInAnyOrder(new List<string> { "C2", "C3" }));
			Assert(collection["G2"].ContainsSameElementsInAnyOrder(new List<string> { "C1", "C3" }));
		}

		public void TestGetGRPCapabilitiesWithLastStaffPerGroup_GlobalCapability()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);

			Factory.Save();

			var groupsSelectedForDelete = new BusinessObject[] { group1 };
			var collection = StaffCapabilityGroupHelper.GetGRPCapabilitiesWithLastStaffPerGroup(Factory, groupsSelectedForDelete, staff1.PK);

			AssertEquals(0, collection.Count);
		}
	}
}
