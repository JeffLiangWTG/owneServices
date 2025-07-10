using NUnit.Framework;

namespace Enterprise.Warehouse.Integration.BondedWarehouse.Testing
{
	public class MissingDataExceptionTest : TestCase
	{
		public void TestConstructAndThrow()
		{
			bool exceptionThrown;
			try
			{
				throw new MissingDataException("TestingMessage");
			}
			catch (MissingDataException e)
			{
				AssertEquals("E.Message", "TestingMessage", e.Message);
				exceptionThrown = true;
			}

			AssertEquals("ExceptionThrown", true, exceptionThrown);
		}
	}
}