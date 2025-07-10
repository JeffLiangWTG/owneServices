using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Testing.Accounting
{
	sealed class AccPOSChargeCodeGroupViewValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateGRO_GroupType()
		{
			var group = Factory.New<AccPOSChargeCodeGroup>();
			AssertEquals("Pre-condition: GRP_GroupType is set to valid value", "POS", group.GRO_GroupType);
			group.Validation.ValidateGRO_GroupType();
			AssertNoErrors(group.GRO_GroupTypeInfo);

			group.GRO_GroupType = ZString.Empty;
			AssertHasError(group.GRO_GroupTypeInfo, "Please enter a Group Type.");
			AssertHasError(group.GRO_GroupTypeInfo, "Must be 'POS'.");

			group.GRO_GroupType = "ABC";
			AssertNoError(group.GRO_GroupTypeInfo, "Please enter a Group Type.");
			AssertHasError(group.GRO_GroupTypeInfo, "Must be 'POS'.");

			group.GRO_GroupType = "Pos";
			AssertNoErrors(group.GRO_GroupTypeInfo);
		}

		public void TestValidateGRO_GC()
		{
			var group = Factory.New<AccPOSChargeCodeGroup>();
			AssertEquals("Pre-condition: GRP_GC is set to Current Company", Env.CurrentCompanyPK, group.GRO_GC);
			group.Validation.ValidateGRO_GC();
			AssertNoErrors(group.GRO_GCInfo);

			group.GRO_GC = ZGuid.Empty;
			AssertHasError(group.GRO_GCInfo, "Please enter a Company.");

			group.GRO_GC = ZGuid.NewZGuid();
			AssertNoError(group.GRO_GCInfo, "Please enter a Company.");
			AssertHasError(group.GRO_GCInfo, "Enter a valid Company.");

			var company = Factory.New<GlbCompany>();

			group.GRO_GC = company.PK;
			AssertNoErrors(group.GRO_GCInfo);
		}

		public void TestValidateGRO_Code()
		{
			var group = Factory.New<AccPOSChargeCodeGroup>();
			Assert("Pre-condition: GRP_Code is empty", group.GRO_Code.IsEmpty);
			group.Validation.ValidateGRO_Code();

			AssertHasError(group.GRO_CodeInfo, "Please enter a Code.");

			group.GRO_Code = "ABC";
			AssertNoErrors(group.GRO_CodeInfo);
		}

		public void TestValidateGRO_Description()
		{
			var group = Factory.New<AccPOSChargeCodeGroup>();
			Assert("Pre-condition: GRP_Description is empty", group.GRO_Description.IsEmpty);
			group.Validation.ValidateGRO_Description();

			AssertHasError(group.GRO_DescriptionInfo, "Please enter a Description.");

			group.GRO_Description = "BCD";
			AssertNoErrors(group.GRO_DescriptionInfo);
		}
	}
}
