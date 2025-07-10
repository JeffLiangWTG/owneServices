using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ISFMessageBuilderFactoryTest : TestCaseWithFactory
	{
		public void TestCreateWebMessageBuilder()
		{
			var factory = new ISFMessageBuilderFactory();
			var isf = Factory.NewWithValidTestData<CusISFHeader>();
			var webMessageBuilder = factory.CreateWebMessageBuilder(isf, UpdateActionCode.Add);
			AssertType<ImporterSecurityFilingMessageBuilder<ABIInputBlockControlGenerator, APLB, APLY>>(webMessageBuilder);
		}
	}
}
