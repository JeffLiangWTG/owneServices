using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	class GetLastStaffWithGRPCapabilityPerGroupTest : TestCaseWithFactory
	{
		public void TestGetLastStaffWithGRPCapabilityPerGroup()
		{
			/* Capability-Group Combination		Staff
			 * ============================     =====
			 * C1-G1							S1
			 * C1-G2							S2
			 * C1-G3							S1, S2
			 * C1-G4							S1, S3
			 * C1-G5							S2, S3
			 *
			 * deleting S1, S2 will leave first three Capability-Group combinations empty
			 */

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "G2";
			var group3 = Factory.NewWithValidTestData<GlbGroup>();
			group3.GG_Code = "G3";
			var group4 = Factory.NewWithValidTestData<GlbGroup>();
			group4.GG_Code = "G4";
			var group5 = Factory.NewWithValidTestData<GlbGroup>();
			group5.GG_Code = "G5";

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);
			staff1.Groups.Add(group3);
			staff1.Groups.Add(group4);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "S2";
			staff2.Capabilities.Add(capability1);
			staff2.Groups.Add(group2);
			staff2.Groups.Add(group3);
			staff2.Groups.Add(group5);

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "S3";
			staff3.Capabilities.Add(capability1);
			staff3.Groups.Add(group4);
			staff3.Groups.Add(group5);

			Factory.Save();

			var staffSelectedForDelete = new BusinessObject[] { staff1, staff2 };
			var collection = StaffCapabilityGroupHelper.GetLastStaffWithGRPCapabilityPerGroup(Factory, staffSelectedForDelete, capability1);

			AssertEquals(3, collection.Count);
			Assert(collection["G1"].ContainsSameElementsInAnyOrder(new List<string> { "S1" }));
			Assert(collection["G2"].ContainsSameElementsInAnyOrder(new List<string> { "S2" }));
			Assert(collection["G3"].ContainsSameElementsInAnyOrder(new List<string> { "S1", "S2" }));
		}

		public void TestGetLastStaffWithGRPCapabilityPerGroup_GlobalCapability()
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

			var staffSelectedForDelete = new BusinessObject[] { staff1 };
			var collection = StaffCapabilityGroupHelper.GetLastStaffWithGRPCapabilityPerGroup(Factory, staffSelectedForDelete, capability1);

			AssertEquals(0, collection.Count);
		}
	}
}

