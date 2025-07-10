using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class JobDeclarationDocumentAddressConfigValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDocumentName()
		{
			var targetInfo = jobDeclarationDocumentAddressConfig.DocumentNameInfo;
			validation.ValidateDocumentName();
			AssertHasErrorContaining(targetInfo, "Must have at least a row where Field is Goods Description.");

			jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs.AddNew();
			validation.ValidateDocumentName();
			AssertNoErrorContaining(targetInfo, "Must have at least a row where Field is Goods Description.");
		}

		public void TestCheckCustomizeSectionBodyRow()
		{
			var targetInfo = jobDeclarationDocumentAddressConfig.CustomizeSectionBodyRowInfo;
			foreach (var code in jobDeclarationDocumentAddressConfig.CustomizeSectionBodyRowList.GetAllCodes())
			{
				ValidationTestHelper.AssertErrorIfInvalidCode(targetInfo, "X", code);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclarationDocumentAddressConfig = new JobDeclarationDocumentAddressConfig(declaration);
			validation = jobDeclarationDocumentAddressConfig.Validation;
		}

		JobDeclarationDocumentAddressConfig jobDeclarationDocumentAddressConfig;
		JobDeclarationDocumentAddressConfigValidation validation;
	}
}
