using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	class AccOrgTaxConfigurationTemplateValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateOCT_Code()
		{
			TestTemplate.OCT_Code = "";
			AssertHasError(TestTemplate.OCT_CodeInfo, "Please enter a Template Code.");

			TestTemplate.OCT_Code = "COD";
			AssertNoErrors(TestTemplate.OCT_CodeInfo);

			TestTemplate.OCT_Code = "C_D";
			AssertHasError(TestTemplate.OCT_CodeInfo, "C_D is not a valid Template Code.");

			TestTemplate.OCT_Code = "111";
			AssertNoErrors(TestTemplate.OCT_CodeInfo);

			AssertExceptionThrown<MaxLengthExceededException>(() =>
			{
				TestTemplate.OCT_Code = "123456789012345678901";
			});

			TestTemplate.OCT_Code = "COD";
			Factory.Save();

			var newTestTemplate = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			newTestTemplate.OCT_Code = "COD";
			AssertNoErrors(TestTemplate.OCT_CodeInfo);
			AssertHasError(newTestTemplate.OCT_CodeInfo, "A Tax Configuration Template with Template Code \"COD\" already exists in the current login company.");

			newTestTemplate.OCT_GC_Company = Factory.NewWithValidTestData<GlbCompany>().PK;
			newTestTemplate.Validation.ValidateOCT_Code();
			AssertNoErrors(newTestTemplate.OCT_CodeInfo);

			newTestTemplate.OCT_GC_Company = GlbCompany.CurrentCompany.PK;
			newTestTemplate.OCT_Code = "COD2";
			AssertNoErrors(newTestTemplate.OCT_CodeInfo);

			ErrorReporter.Clear();
		}

		public void TestValidateOCT_Description()
		{
			TestTemplate.OCT_Description = "";
			AssertHasError(TestTemplate.OCT_DescriptionInfo, "Please enter a Template Description.");

			TestTemplate.OCT_Description = "DESC";
			AssertNoErrors(TestTemplate.OCT_DescriptionInfo);

			AssertExceptionThrown<MaxLengthExceededException>(() =>
			{
				TestTemplate.OCT_Description = "123456789012345678901234567890123456789012345678901234567890123456789012345678901";
			});

			TestTemplate.OCT_Description = "DESC";
			Factory.Save();

			var newTestTemplate = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			newTestTemplate.OCT_Description = "DESC";
			AssertNoErrors(TestTemplate.OCT_DescriptionInfo);
			AssertHasError(newTestTemplate.OCT_DescriptionInfo, "A Tax Configuration Template with Template Description \"DESC\" already exists in the current login company.");

			newTestTemplate.OCT_GC_Company = Factory.NewWithValidTestData<GlbCompany>().PK;
			newTestTemplate.Validation.ValidateOCT_Description();
			AssertNoErrors(newTestTemplate.OCT_DescriptionInfo);

			newTestTemplate.OCT_GC_Company = GlbCompany.CurrentCompany.PK;
			newTestTemplate.OCT_Description = "DESC2";
			AssertNoErrors(newTestTemplate.OCT_DescriptionInfo);

			ErrorReporter.Clear();
		}

		protected AccOrgTaxConfigurationTemplate TestTemplate;

		protected override void SetUp()
		{
			base.SetUp();
			TestTemplate = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
		}
	}
}
