using System;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.eTail.Business.Testing;

public class DataObjectCacheFactoryServiceTest : TestCaseWithFactory
{
	public void TestBuildKey()
	{
		var property1 = "Property1";
		var property2 = "Property2";
		var property3 = "Property3";

		AssertEquals("Property1-Property2-Property3", dataObjectCacheFactoryService.BuildKey(property1, property2, property3));
	}

	public void TestGetValue_ShouldReturnCachedValue()
	{
		var key = "testKey";
		var expectedValue = new Mock<IDisposable>().Object;
		dataObjectCacheFactoryService.GetValue(key, () => expectedValue);
		var cachedValue = dataObjectCacheFactoryService.GetValue(key, () => new Mock<IDisposable>().Object);

		AssertEquals(expectedValue, cachedValue);
	}

	public void TestGetOrCreateNewInstance_ShouldReturnExistingInstance()
	{
		var service = DataObjectCacheFactoryService.GetOrCreateNewInstance(Factory);

		AssertEquals(service, dataObjectCacheFactoryService);
	}

	public void TestGetOrCreateNewInstance_ShouldCreateNewInstanceIfNotExists()
	{
		Factory.ServiceContainer.RemoveService<DataObjectCacheFactoryService>();
		var service = DataObjectCacheFactoryService.GetOrCreateNewInstance(Factory);

		AssertNotEquals(service, dataObjectCacheFactoryService);
	}

	public void TestDisposeInstance_ShouldRemoveService()
	{
		DataObjectCacheFactoryService factoryService = Factory.ServiceContainer.GetService<DataObjectCacheFactoryService>();
		AssertNotNull("Before Dispose", factoryService);
		DataObjectCacheFactoryService.DisposeInstance(Factory);

		factoryService = Factory.ServiceContainer.GetService<DataObjectCacheFactoryService>();
		AssertNull("After Dispose", factoryService);
	}

	protected override void SetUp()
	{
		dataObjectCacheFactoryService = DataObjectCacheFactoryService.GetOrCreateNewInstance(Factory);
	}

	protected override void TearDown()
	{
		DataObjectCacheFactoryService.DisposeInstance(Factory);
	}

	DataObjectCacheFactoryService dataObjectCacheFactoryService;
}
