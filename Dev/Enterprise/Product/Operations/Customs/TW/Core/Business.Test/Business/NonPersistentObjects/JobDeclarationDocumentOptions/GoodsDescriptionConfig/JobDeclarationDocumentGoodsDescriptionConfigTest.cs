using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobDeclarationDocumentGoodsDescriptionConfig))]
	sealed class JobDeclarationDocumentGoodsDescriptionConfigTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsImport()
		{
			var goodsDescriptionConfig = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs.AddNew();
			declaration.JE_MessageType = "IMP";
			Assert(goodsDescriptionConfig.IsImport);

			declaration.JE_MessageType = "EXP";
			Assert(!goodsDescriptionConfig.IsImport);
		}

		public void TestPosition()
		{
			var goodsDescriptionsConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			var goodsDescriptionsConfig1 = goodsDescriptionsConfigs.AddNew();
			var goodsDescriptionsConfig2 = goodsDescriptionsConfigs.AddNew();
			var goodsDescriptionsConfig3 = goodsDescriptionsConfigs.AddNew();
			AssertEquals((ZShort)1, goodsDescriptionsConfig1.Position);
			AssertEquals((ZShort)2, goodsDescriptionsConfig2.Position);
			AssertEquals((ZShort)3, goodsDescriptionsConfig3.Position);

			goodsDescriptionsConfig1.Position = 0;
			AssertEquals((ZShort)1, goodsDescriptionsConfig1.Position);

			goodsDescriptionsConfig1.Position = 2;
			AssertEquals((ZShort)2, goodsDescriptionsConfig1.Position);
			AssertEquals((ZShort)1, goodsDescriptionsConfig2.Position);
			AssertEquals((ZShort)3, goodsDescriptionsConfig3.Position);

			goodsDescriptionsConfig3.Position = 1;
			AssertEquals((ZShort)3, goodsDescriptionsConfig1.Position);
			AssertEquals((ZShort)2, goodsDescriptionsConfig2.Position);
			AssertEquals((ZShort)1, goodsDescriptionsConfig3.Position);

			goodsDescriptionsConfigs.RemoveAndDelete(goodsDescriptionsConfig3);
			AssertEquals((ZShort)2, goodsDescriptionsConfig1.Position);
			AssertEquals((ZShort)1, goodsDescriptionsConfig2.Position);

			var goodsDescriptionsConfig4 = goodsDescriptionsConfigs.AddNew();
			AssertEquals((ZShort)3, goodsDescriptionsConfig4.Position);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new JobDeclarationDocumentAddressConfig(Factory.New<JobDeclaration>()).GoodsDescriptionConfigs.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclarationDocumentAddressConfig = new JobDeclarationDocumentAddressConfig(declaration);
		}

		JobDeclaration declaration;
		JobDeclarationDocumentAddressConfig jobDeclarationDocumentAddressConfig;
	}
}
