using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.GUI.Testing;

abstract class BaseMessageSendingStrategyTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var factory = new BusinessObjectFactory();
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Value cannot be null.\r\nParameter name: factory", () => GetInstanceForTest(null, null));
			AssertExceptionThrown<ArgumentNullException>("Value cannot be null.\r\nParameter name: declaration", () => GetInstanceForTest(factory, null));
		});
	}

	protected abstract BaseMessageSendingStrategy GetInstanceForTest(BusinessObjectFactory factory, JobDeclaration declaration);

	protected override void SetUp()
	{
		base.SetUp();
		factory = new BusinessObjectFactory();
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
	}

	protected BusinessObjectFactory factory;
	protected JobDeclaration declaration;
}
