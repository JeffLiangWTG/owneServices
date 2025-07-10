using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Exceptions.Testing
{
	public class WebServiceExceptionTest : TestCase
	{
		[ExpectExceptionMessage(typeof(WebServiceException), "Test Exception")]
		public void TestException()
		{
			throw new WebServiceException("Test Exception");
		}
	}
}
