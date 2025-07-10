using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TestAccWithholdingValidation : BusinessObjectValidationTestCase
	{
		#region Implementation
		BusinessObjectFactory TestFactory;
		AccWithholding Withholding;

		protected override void SetUp()
		{
			base.SetUp();
			TestFactory = new BusinessObjectFactory();
			Withholding = TestFactory.New(typeof(AccWithholding)) as AccWithholding;
		}
		#endregion

		#region Validation
		public void TestValidateAW_Code()
		{
			Withholding.AW_Code = "";
			Withholding.Validation.ValidateAW_Code();
			Assert("Code cannot be empty", Withholding.AW_CodeInfo.HasErrors());

			Withholding.AW_Code = "!!!";
			Withholding.Validation.ValidateAW_Code();
			Assert("Should not have errors", !Withholding.AW_CodeInfo.HasErrors());
		}

		public void TestValidateAW_Description()
		{
			Withholding.AW_Description = "";
			Withholding.Validation.ValidateAW_Description();
			Assert("Description cannot be empty", Withholding.AW_DescriptionInfo.HasErrors());

			Withholding.AW_Description = "###";
			Withholding.Validation.ValidateAW_Description();
			Assert("Should not have errors", !Withholding.AW_DescriptionInfo.HasErrors());
		}

		public void TestValidateAW_Rate()
		{
			Withholding.AW_Rate = -1;
			Withholding.Validation.ValidateAW_Rate();
			Assert("Tax rate cannot be negative", Withholding.AW_RateInfo.HasErrors());
		}

		#endregion
	}
}
