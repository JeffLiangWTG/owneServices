using System.Web;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.WebVoting.Testing
{
	sealed class ClientSpecificImageHandlerTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[RequestString(@"Images\Logo.png")]
		public void TestProcessRequest_NoUndisposedDbConnectionError()
		{
			WebTestHelper.AssertNoUndisposedDbConnectionError(this, () =>
			{
				var handler = new ClientSpecificImageHandler();
				handler.ProcessRequest(HttpContext.Current);
			});
		}
	}
}
