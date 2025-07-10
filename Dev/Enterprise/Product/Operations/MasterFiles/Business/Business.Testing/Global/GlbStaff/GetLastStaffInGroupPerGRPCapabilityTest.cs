using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	class GetLastStaffInGroupPerGRPCapabilityTest : TestCaseWithFactory
	{
		public void TestGetLastStaffInGroupPerGRPCapability()
		{
			/* Group-Capability Combination		Staff
			 * ============================     =====
			 * G1-C1							S1
			 * G1-C2							S2
			 * G1-C3							S1, S2
			 * G1-C4							S1, S3
			 * G1-C5							S2, S3
			 *
			 * deleting S1, S2 will leave first three Group-Capability combinations empty
			 */

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "C1";

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "C2";
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			var capability3 = Factory.NewWithValidTestData<GlbCapability>();
			capability3.G4_Code = "C3";
			capability3.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			var capability4 = Factory.NewWithValidTestData<GlbCapability>();
			capability4.G4_Code = "C4";
			capability4.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			var capability5 = Factory.NewWithValidTestData<GlbCapability>();
			capability5.G4_Code = "C5";
			capability5.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Capabilities.Add(capability3);
			staff1.Capabilities.Add(capability4);
			staff1.Groups.Add(group1);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "S2";
			staff2.Capabilities.Add(capability2);
			staff2.Capabilities.Add(capability3);
			staff2.Capabilities.Add(capability5);
			staff2.Groups.Add(group1);

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "S3";
			staff3.Capabilities.Add(capability4);
			staff3.Capabilities.Add(capability5);
			staff3.Groups.Add(group1);

			Factory.Save();

			var staffSelectedForDelete = new BusinessObject[] { staff1, staff2 };
			var collection = StaffCapabilityGroupHelper.GetLastStaffInGroupPerGRPCapability(Factory, staffSelectedForDelete, group1);

			AssertEquals(3, collection.Count);
			Assert(collection["C1"].ContainsSameElementsInAnyOrder(new List<string> { "S1" }));
			Assert(collection["C2"].ContainsSameElementsInAnyOrder(new List<string> { "S2" }));
			Assert(collection["C3"].ContainsSameElementsInAnyOrder(new List<string> { "S1", "S2" }));
		}

		public void TestGetLastStaffInGroupPerGRPCapability_GlobalCapability()
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
			var collection = StaffCapabilityGroupHelper.GetLastStaffInGroupPerGRPCapability(Factory, staffSelectedForDelete, group1);

			AssertEquals(0, collection.Count);
		}
	}
}
