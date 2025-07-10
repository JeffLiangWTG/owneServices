using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class JobDeclarationDocumentGoodsDescriptionConfigValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckField()
		{
			var targetInfo = jobDeclarationDocumentGoodsDescriptionConfig.FieldInfo;
			jobDeclarationDocumentGoodsDescriptionConfig.Field = ExportDeclarationDocumentFieldList.Codes.SupplierPartNumber;
			AssertNoErrorContaining(targetInfo, ListValidation.InvalidCodeError);
			AssertNoErrorContaining(targetInfo, MandatoryValidation.MustBeEntered);

			jobDeclarationDocumentGoodsDescriptionConfig.Field = ZString.Empty;
			AssertHasErrorContaining(targetInfo, MandatoryValidation.MustBeEntered);

			jobDeclarationDocumentGoodsDescriptionConfig.Field = "A";
			AssertHasErrorContaining(targetInfo, ListValidation.InvalidCodeError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var jobDeclarationDocumentAddressConfig = new JobDeclarationDocumentAddressConfig(declaration);
			jobDeclarationDocumentGoodsDescriptionConfig = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs.AddNew();
		}

		JobDeclarationDocumentGoodsDescriptionConfig jobDeclarationDocumentGoodsDescriptionConfig;
	}
}
