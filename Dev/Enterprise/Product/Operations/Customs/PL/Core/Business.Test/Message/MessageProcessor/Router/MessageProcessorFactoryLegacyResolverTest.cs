using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class MessageProcessorFactoryLegacyResolverTest : TestCaseWithFactory
{
	public void TestResolveFactory_PLC() => AssertFactoryTypeName("PLC", "Enterprise.Customs.PL.Business.MessageProcessorFactoryLegacy");

	public void TestResolveFactory_PLN() => AssertFactoryTypeName("PLN", "Enterprise.Customs.PL.NCTS.Business.MessageProcessorFactoryLegacy");

	public void TestResolveFactory_PLX() => AssertFactoryTypeName("PLX", "Enterprise.Customs.PL.ExitControl.Business.MessageProcessorFactoryLegacy");

	public void TestResolveFactory_Unknown() => AssertFactoryTypeName("ZZZ", null);

	void AssertFactoryTypeName(string applicationCode, string expectedTypeName)
	{
		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = applicationCode;
		var messageProcessorFactoryResolver = new MessageProcessorFactoryLegacyResolver();
		var logger = new LoggingInformation();
		var factory1 = messageProcessorFactoryResolver.ResolveFactory(logger, message);
		var factory2 = messageProcessorFactoryResolver.ResolveFactory(logger, message);

		var actualTypeName = factory1?.GetType().FullName;
		CombineAssertions(() =>
		{
			AssertEquals("Type name should be expected.", expectedTypeName, actualTypeName);
			if (expectedTypeName is not null)
			{
				AssertNotSame("Factory should not be a singleton.", factory1, factory2);
			}
			else
			{
				AssertNull("Instance 1 should be null.", factory1);
				AssertNull("Instance 2 should be null.", factory2);
			}
		});
	}
}
