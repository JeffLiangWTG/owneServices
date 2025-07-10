using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class IMProductValueDefaultOptionValidationTest : TestCaseWithFactory
	{
		public void TestCheckFieldType()
		{
			var companyData = Factory.New<OrgCompanyData>();
			var defaultOption1 = companyData.DefaultOptions.AddNew();
			defaultOption1.FieldType = "XXX";

			AssertHasErrorContaining(defaultOption1.FieldTypeInfo, ListValidation.InvalidCodeError);

			defaultOption1.FieldType = Core.Constants.Customs.ASNRefreshDefaultsOptions.Codes.Classification;
			AssertNoErrors(defaultOption1.FieldTypeInfo);

			var defaultOption2 = companyData.DefaultOptions.AddNew();
			defaultOption2.FieldType = Core.Constants.Customs.ASNRefreshDefaultsOptions.Codes.Classification;
			AssertHasError(defaultOption2.FieldTypeInfo, IMProductValueDefaultOptionValidation.DuplicatedError);
		}
	}
}
