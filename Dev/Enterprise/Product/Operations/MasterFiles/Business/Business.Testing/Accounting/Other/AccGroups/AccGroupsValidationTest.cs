using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccGroupsValidationTest : BusinessObjectValidationTestCase
	{
		#region Implementation

		AccGroups TestAccGroups;

		protected override void SetUp()
		{
			base.SetUp();
			TestAccGroups = Factory.New<AccGroups>();
		}

		#endregion

		#region Tests

		public void TestValidateAR_Code()
		{
			TestAccGroups.AR_Code = "";
			Assert("Property Info should have errors - " + TestAccGroups.AR_Code, TestAccGroups.AR_CodeInfo.HasErrors());
			TestAccGroups.AR_Code = "COD";
			Assert("Property Info should not have errors - " + TestAccGroups.AR_Code, !TestAccGroups.AR_CodeInfo.HasErrors());
			TestAccGroups.AR_Code = "C_D";
			Assert("Property Info should have errors - " + TestAccGroups.AR_Code, TestAccGroups.AR_CodeInfo.HasErrors());
			TestAccGroups.AR_Code = "CORRREEE";
			Assert("Property Info should not have errors - " + TestAccGroups.AR_Code, !TestAccGroups.AR_CodeInfo.HasErrors());
		}

		public void TestValidateAR_Desc()
		{
			TestAccGroups.AR_Desc = "";
			Assert("error: no description", TestAccGroups.AR_DescInfo.HasErrors());
			TestAccGroups.AR_Desc = "description";
			Assert("should pass", !TestAccGroups.AR_DescInfo.HasErrors());
		}

		#endregion
	}
}
