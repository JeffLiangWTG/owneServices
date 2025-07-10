using System.IO;
using System.Web;
using Enterprise.Warehouse.Web.WebService.Common;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public abstract class BaseServiceTestCase<T> : TransactionedTestCase
			where T : BaseService
	{
		#region Test Cases

		public void TestConstructors()
		{
			var webService = GetNewWebService();

			AssertNotNull(webService);
			AssertNotEquals(webService, GetNewWebService());
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			HttpContext.Current = new HttpContext(new HttpRequest("", "https://www.google.com", ""), new HttpResponse(new StringWriter()));
		}

		protected override void TearDown()
		{
			base.TearDown();

			HttpContext.Current = null;
			LoginHelper.ClearAllActiveSemaphoreHandlers_ForTesting();
		}

		protected abstract T GetNewWebService();

		#endregion
	}
}
