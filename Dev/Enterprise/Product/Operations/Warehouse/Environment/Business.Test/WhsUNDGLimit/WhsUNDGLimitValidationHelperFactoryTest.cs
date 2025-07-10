using CargoWise.Application;
using Enterprise.Warehouse.Integration.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public class WhsUNDGLimitValidationHelperFactoryTest : TestCase
	{
		public void TestObjectFactoryConfiguration()
		{
			AssertType<WhsUNDGLimitValidationHelperFactory>(ObjectFactory.Get<IWhsUNDGLimitValidationHelperFactory>());
		}

		public void TestGetWhsUNDGLimitValidationHelper()
		{
			var helperFactory = new WhsUNDGLimitValidationHelperFactory();
			AssertNull(helperFactory.GetWhsUNDGLimitValidationHelper(WarehouseTypes.Codes.ContainerYard));

			AssertNotNull(helperFactory.GetWhsUNDGLimitValidationHelper(WarehouseTypes.Codes.Product));
			AssertNotNull(helperFactory.GetWhsUNDGLimitValidationHelper(WarehouseTypes.Codes.FreeTradeZone));
			AssertNotNull(helperFactory.GetWhsUNDGLimitValidationHelper(WarehouseTypes.Codes.Transit));
		}

		public void TestGetWhsUNDGLimitValidationHelper_EndToEnd_ProductWarehouse()
		{
			var helperFactory = new WhsUNDGLimitValidationHelperFactory();
			var validationHelper = helperFactory.GetWhsUNDGLimitValidationHelper(WarehouseTypes.Codes.Product);
			AssertNotNull(validationHelper);

			var expectedValidationHelper = ObjectFactory.Get<IWhsUNDGLimitValidationHelper>("WhsProductWhsUNDGLimitValidationHelper");
			AssertNotNull(expectedValidationHelper);

			AssertEquals("Service Type is correct.", validationHelper.GetType(), expectedValidationHelper.GetType());
			AssertNotEquals("Services are not singletons.", validationHelper, expectedValidationHelper);
		}

		public void TestGetWhsUNDGLimitValidationHelper_EndToEnd_TransitWarehouse()
		{
			var helperFactory = new WhsUNDGLimitValidationHelperFactory();
			var validationHelper = helperFactory.GetWhsUNDGLimitValidationHelper(WarehouseTypes.Codes.Transit);
			AssertNotNull(validationHelper);

			var expectedValidationHelper = ObjectFactory.Get<IWhsUNDGLimitValidationHelper>("TransitWarehouseValidationHelper");
			AssertNotNull(expectedValidationHelper);

			AssertEquals("Service Type is correct.", validationHelper.GetType(), expectedValidationHelper.GetType());
			AssertNotEquals("Services are not singletons.", validationHelper, expectedValidationHelper);
		}

		public void TestGetWhsUNDGLimitValidationHelper_EndToEnd_FTZWarehouse()
		{
			var helperFactory = new WhsUNDGLimitValidationHelperFactory();
			var validationHelper = helperFactory.GetWhsUNDGLimitValidationHelper(WarehouseTypes.Codes.FreeTradeZone);
			AssertNotNull(validationHelper);

			var expectedValidationHelper = ObjectFactory.Get<IWhsUNDGLimitValidationHelper>("WhsProductWhsUNDGLimitValidationHelper");
			AssertNotNull(expectedValidationHelper);

			AssertEquals("Service Type is correct.", validationHelper.GetType(), expectedValidationHelper.GetType());
			AssertNotEquals("Services are not singletons.", validationHelper, expectedValidationHelper);
		}
	}
}
