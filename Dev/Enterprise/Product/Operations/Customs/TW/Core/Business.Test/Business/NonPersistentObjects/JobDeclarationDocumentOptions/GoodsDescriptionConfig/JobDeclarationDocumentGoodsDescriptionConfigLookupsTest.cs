using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class JobDeclarationDocumentGoodsDescriptionConfigLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestFiledsList()
		{
			declaration.JE_MessageType = "EXP";
			var list = lookups.FiledsList;
			AssertType<ExportDeclarationDocumentFieldList>(list);

			declaration.JE_MessageType = "IMP";
			list = lookups.FiledsList;
			AssertType<ImportDeclarationDocumentFieldList>(list);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var jobDeclarationDocumentAddressConfig = new JobDeclarationDocumentAddressConfig(declaration);
			var jobDeclarationDocumentGoodsDescriptionConfig = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs.AddNew();
			lookups = jobDeclarationDocumentGoodsDescriptionConfig.Lookups;
		}

		JobDeclaration declaration;
		JobDeclarationDocumentGoodsDescriptionConfigLookups lookups;
	}
}
