using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbResourceValidationTest : GlbStaffValidationTestCase
	{
		public void TestGS_Code()
		{
			Bizo.GS_Code = "";
			AssertHasErrors(Bizo.GS_CodeInfo);

			Bizo.GS_Code = "$A";
			AssertNoErrors(Bizo.GS_CodeInfo);

			Bizo.GS_Code = "$";
			AssertHasErrors(Bizo.GS_CodeInfo);

			Bizo.GS_Code = "A$";
			AssertHasErrors(Bizo.GS_CodeInfo);
		}

		public void TestGS_ResourceType()
		{
			Bizo.GS_ResourceType = "...";
			AssertHasErrors(Bizo.GS_ResourceTypeInfo);

			Bizo.GS_ResourceType = Bizo.Lookups.ResourceTypes[0].Code;
			AssertNoErrors(Bizo.GS_ResourceTypeInfo);

			Bizo.GS_ResourceType = ZString.Empty;
			AssertHasErrors(Bizo.GS_ResourceTypeInfo);
		}

		protected override bool ShouldHaveADRelatedLoginNameErrors => false;

		protected override GlbStaff GetNewBusinessObject()
		{
			GlbStaff resource = Factory.New<GlbStaff>();
			resource.GS_IsResource = true;
			return resource;
		}
	}
}
