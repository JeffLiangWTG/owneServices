using NUnit.Framework;

namespace Enterprise.Warehouse.Integration.BondedWarehouse.Testing
{
	public class CannotUpdateStockExceptionTest : TestCase
	{
		public void TestConstructAndThrow()
		{
			bool exceptionThrown;
			try
			{
				throw new CannotUpdateStockException("TestingMessage");
			}
			catch (CannotUpdateStockException e)
			{
				AssertEquals("E.Message", "TestingMessage", e.Message);
				exceptionThrown = true;
			}
			AssertEquals("ExceptionThrown", true, exceptionThrown);
		}
	}
}