using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessTaskExtraResourceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestPE_GS_NKStaffOrResource()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "STF";

			GlbStaff staffResource = Factory.New<GlbStaff>();
			staffResource.GS_IsResource = true;
			staffResource.GS_Code = "$RS";

			ProcessTaskExtraResource resource = Factory.New<ProcessTaskExtraResource>();

			resource.PE_GS_NKStaffOrResource = "ZZZ";
			AssertHasErrors(resource.PE_GS_NKStaffOrResourceInfo);

			resource.PE_GS_NKStaffOrResource = staff.GS_Code;
			AssertNoErrors(resource.PE_GS_NKStaffOrResourceInfo);

			resource.PE_GS_NKStaffOrResource = "";
			AssertHasErrors(resource.PE_GS_NKStaffOrResourceInfo);

			resource.PE_GS_NKStaffOrResource = staffResource.GS_Code;
			AssertNoErrors(resource.PE_GS_NKStaffOrResourceInfo);
		}
	}
}
