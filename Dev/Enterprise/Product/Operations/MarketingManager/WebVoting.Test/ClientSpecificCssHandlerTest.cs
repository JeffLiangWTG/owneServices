using System.Web;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.WebVoting.Testing
{
	sealed class ClientSpecificCssHandlerTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[RequestString("BaseStyle.css")]
		public void TestProcessRequest_NoUndisposedDbConnectionError()
		{
			WebTestHelper.AssertNoUndisposedDbConnectionError(this, () =>
			{
				var handler = new ClientSpecificCssHandler();
				handler.ProcessRequest(HttpContext.Current);
			});
		}
	}
}
