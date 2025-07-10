using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Exceptions.Testing
{
	public class WebServiceLoginExceptionTest : TestCase
	{
		[ExpectExceptionMessage(typeof(WebServiceLoginException), "Test Exception")]
		public void TestException()
		{
			throw new WebServiceLoginException("Test Exception");
		}
	}
}
