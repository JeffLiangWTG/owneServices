using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	class GetGroupsWithLastStaffPerGRPCapabilityTest : TestCaseWithFactory
	{
		public void TestGetGroupsWithLastStaffPerGRPCapabilityTest()
		{
			/* Group-Capability Combination		Staff
			 * ============================     =====
			 * G1-C1							S1, S2
			 * G1-C2							S1
			 * G2-C1							S1
			 * G2-C2							S1, S3
			 * G3-C1							S1
			 * G3-C2							S1
			 *
			 * deleting C1, C2 from S1 will make 2nd, 3rd, 5th and 6th Group-Capability combinations empty
			 */

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "G2";
			var group3 = Factory.NewWithValidTestData<GlbGroup>();
			group3.GG_Code = "G3";

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "C2";
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Capabilities.Add(capability2);
			staff1.Groups.Add(group1);
			staff1.Groups.Add(group2);
			staff1.Groups.Add(group3);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "S2";
			staff2.Capabilities.Add(capability1);
			staff2.Groups.Add(group1);

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "S3";
			staff3.Capabilities.Add(capability2);
			staff3.Groups.Add(group2);

			Factory.Save();

			var capabilitiesSelectedForDelete = new BusinessObject[] { capability1, capability2 };
			var collection = StaffCapabilityGroupHelper.GetGroupsWithLastStaffPerGRPCapability(Factory, capabilitiesSelectedForDelete, staff1.PK);

			AssertEquals(2, collection.Count);
			Assert(collection["C1"].ContainsSameElementsInAnyOrder(new List<string> { "G2", "G3" }));
			Assert(collection["C2"].ContainsSameElementsInAnyOrder(new List<string> { "G1", "G3" }));
		}

		public void TestGetGroupsWithLastStaffPerGRPCapabilityTest_GlobalCapability()
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

			var capabilitiesSelectedForDelete = new BusinessObject[] { capability1 };
			var collection = StaffCapabilityGroupHelper.GetGroupsWithLastStaffPerGRPCapability(Factory, capabilitiesSelectedForDelete, staff1.PK);

			AssertEquals(0, collection.Count);
		}
	}
}
