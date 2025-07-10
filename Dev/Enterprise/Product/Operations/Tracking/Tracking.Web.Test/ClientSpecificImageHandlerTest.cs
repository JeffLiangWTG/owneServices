using System.Web;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class ClientSpecificImageHandlerTest : TestCaseWithFactory, IHttpContextEnabledTestWithAppInstance
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[RequestString("App_Themes\\Standard\\Images\\Logo.gif\"alert('Stop')\"")]
		public void TestIllegalPathCharactersExceptionIsIgnored()
		{
			var handler = new ClientSpecificImageHandler();

			handler.ProcessRequest(HttpContext.Current);

			AssertEquals(200, HttpContext.Current.Response.StatusCode);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[RequestString("App_Themes\\Standard\\Images\\Logo.gif")]
		public void TestStandardThemeLogo()
		{
			var handler = new ClientSpecificImageHandler();

			handler.ProcessRequest(HttpContext.Current);

			AssertEquals("image/gif", HttpContext.Current.Response.ContentType);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[RequestString("App_Themes\\Classic\\Images\\Logo.gif")]
		public void TestClassicThemeLogo()
		{
			var handler = new ClientSpecificImageHandler();

			handler.ProcessRequest(HttpContext.Current);

			AssertEquals("image/gif", HttpContext.Current.Response.ContentType);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[RequestString("App_Themes\\Alternate\\Images\\Logo.gif")]
		public void TestAlternateThemeLogo()
		{
			var handler = new ClientSpecificImageHandler();

			handler.ProcessRequest(HttpContext.Current);

			AssertEquals("image/gif", HttpContext.Current.Response.ContentType);
		}

		class TestGlobalWebAppMapper : TestGlobal
		{
			public override string MapPath(string path)
			{
				path = path.Replace("/webapp/", "/");

				return base.MapPath(path);
			}
		}

		public ZEnterpriseGlobalBase AppInstance => new TestGlobalWebAppMapper();
	}
}
