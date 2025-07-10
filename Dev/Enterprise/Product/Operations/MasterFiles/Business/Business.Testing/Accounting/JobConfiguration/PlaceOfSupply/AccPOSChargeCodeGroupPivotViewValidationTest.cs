using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Testing.Accounting
{
	sealed class AccPOSChargeCodeGroupPivotViewValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateGRP_GroupType()
		{
			var pivot = Factory.New<AccPOSChargeCodeGroupPivot>();
			AssertEquals("Pre-condition: GRP_GroupType is set to valid value", "POS", pivot.GRP_GroupType);
			pivot.Validation.ValidateGRP_GroupType();
			AssertNoErrors(pivot.GRP_GroupTypeInfo);

			pivot.GRP_GroupType = ZString.Empty;
			AssertHasError(pivot.GRP_GroupTypeInfo, "Please enter a Group Type.");
			AssertHasError(pivot.GRP_GroupTypeInfo, "Must be 'POS'.");

			pivot.GRP_GroupType = "ABC";
			AssertNoError(pivot.GRP_GroupTypeInfo, "Please enter a Group Type.");
			AssertHasError(pivot.GRP_GroupTypeInfo, "Must be 'POS'.");

			pivot.GRP_GroupType = "Pos";
			AssertNoErrors(pivot.GRP_GroupTypeInfo);
		}

		public void TestValidateGRP_MemberTableCode()
		{
			var pivot = Factory.New<AccPOSChargeCodeGroupPivot>();
			AssertEquals("Pre-condition: GRP_MemberTableCode is set to valid value", "AC", pivot.GRP_MemberTableCode);
			pivot.Validation.ValidateGRP_MemberTableCode();
			AssertNoErrors(pivot.GRP_MemberTableCodeInfo);

			pivot.GRP_MemberTableCode = ZString.Empty;
			AssertHasError(pivot.GRP_MemberTableCodeInfo, "Please enter a Member Table Code.");
			AssertHasError(pivot.GRP_MemberTableCodeInfo, "Must be 'AC'.");

			pivot.GRP_MemberTableCode = "ABC";
			AssertNoError(pivot.GRP_MemberTableCodeInfo, "Please enter a Member Table Code.");
			AssertHasError(pivot.GRP_MemberTableCodeInfo, "Must be 'AC'.");

			pivot.GRP_MemberTableCode = "ac";
			AssertNoErrors(pivot.GRP_MemberTableCodeInfo);
		}

		public void TestValidateGRP_GRO_Group()
		{
			var pivot = Factory.New<AccPOSChargeCodeGroupPivot>();
			Assert("Pre-condition: GRP_GRO_Group is not set", pivot.GRP_GRO_Group.IsEmpty);
			pivot.Validation.ValidateGRP_GRO_Group();
			AssertHasError(pivot.GRP_GRO_GroupInfo, "Please enter a Group.");

			pivot.GRP_GRO_Group = ZGuid.NewZGuid();
			AssertNoError(pivot.GRP_GRO_GroupInfo, "Please enter a Group.");
			AssertHasError(pivot.GRP_GRO_GroupInfo, "Enter a valid Group.");

			var group = Factory.New<AccPOSChargeCodeGroup>();
			pivot.GRP_GRO_Group = group.PK;
			AssertNoErrors(pivot.GRP_GRO_GroupInfo);
		}

		public void TestValidateGRP_MemberID()
		{
			var pivot = Factory.New<AccPOSChargeCodeGroupPivot>();
			Assert("Pre-condition: GRP_MemberID is not set", pivot.GRP_MemberID.IsEmpty);
			pivot.Validation.ValidateGRP_MemberID();
			AssertHasError(pivot.GRP_MemberIDInfo, "Please enter a Charge Code.");

			pivot.GRP_MemberID = ZGuid.NewZGuid();
			AssertNoError(pivot.GRP_MemberIDInfo, "Please enter a Charge Code.");
			AssertHasError(pivot.GRP_MemberIDInfo, "Enter a valid Charge Code.");

			var chargeCode = Factory.New<AccChargeCode>();
			AssertEquals("Pre-condition: Charge Code created for the cCurrent Company", Env.CurrentCompanyPK, chargeCode.AC_GC);
			AssertNull("Pre-condition: Company is not defined because Group is not set", pivot.Company);
			pivot.GRP_MemberID = chargeCode.PK;
			AssertNoError(pivot.GRP_MemberIDInfo, "Please enter a Charge Code.");
			AssertHasError(pivot.GRP_MemberIDInfo, "Enter a valid Charge Code.");

			var group = Factory.New<AccPOSChargeCodeGroup>();
			pivot.GRP_GRO_Group = group.PK;
			AssertNotNull("Company is defined by Group", pivot.Company);
			pivot.Validation.ValidateGRP_MemberID();
			AssertNoErrors(pivot.GRP_MemberIDInfo);
		}
	}
}
