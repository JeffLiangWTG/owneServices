using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	class AccTaxRateValidationTest : BusinessObjectValidationTestCase
	{
		#region Implementation

		protected virtual AutoAccTaxRate TaxRate
		{
			get
			{
				if (taxRate == null)
				{
					taxRate = Factory.New<AccTaxRate>();
				}
				return taxRate;
			}
		}

		protected AutoAccTaxRate taxRate;

		#endregion

		#region Validation
		public virtual void TestValidateAT_Code()
		{
			TaxRate.AT_Code = "";
			TaxRate.Validation.ValidateAT_Code();
			Assert("Code cannot be empty", TaxRate.AT_CodeInfo.HasErrors());

			TaxRate.AT_Code = "!!!";
			TaxRate.Validation.ValidateAT_Code();
			Assert("Should not have errors", !TaxRate.AT_CodeInfo.HasErrors());
		}

		public virtual void TestValidateAT_Description()
		{
			TaxRate.AT_Description = "";
			TaxRate.Validation.ValidateAT_Description();
			Assert("Description cannot be empty", TaxRate.AT_DescriptionInfo.HasErrors());

			TaxRate.AT_Description = "###";
			TaxRate.Validation.ValidateAT_Description();
			Assert("Should not have errors", !TaxRate.AT_DescriptionInfo.HasErrors());
		}

		#endregion

	}
}
