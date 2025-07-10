using System;
using System.Configuration;
using System.Web.Http.Controllers;
using Enterprise.Registry.Business;
using Enterprise.Services.Scim.Api.Filters;
using Microsoft.Owin.Testing;
using NUnit.Framework;

namespace Enterprise.Services.Scim.Api.Test
{
	[TestFixture]
	public class ScimEnabledAttributeTest
	{
		[Test]
		public void TestIsScimEnabled_Successful()
		{
			using (var server = TestServer.Create<TestStartup>())
			{
				TestingState.IsRunningTests = true;
				var attribute = new ScimEnabledAttribute();
				using (SystemDataRegistry.Instance.EnableScimService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					Assert.DoesNotThrow(() => attribute.OnActionExecuting(new HttpActionContext()));
				}
			}
		}

		[Test]
		public void TestIsScimEnabled_Failed()
		{
			var attribute = new ScimEnabledAttribute();
			TestingState.IsRunningTests = true;
			using (var server = TestServer.Create<TestStartup>())
			using (SystemDataRegistry.Instance.EnableScimService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert.Throws<ArgumentNullException>(() => attribute.OnActionExecuting(null));

				Assert.Throws<InvalidOperationException>(() => attribute.OnActionExecuting(new HttpActionContext()));
			}
		}

		[SetUp]
		public void Init()
		{
			ConfigurationManager.AppSettings["Issuer"] = "";
			ConfigurationManager.AppSettings["AudienceId"] = "";
			ConfigurationManager.AppSettings["AudienceSecret"] = "";
			ConfigurationManager.AppSettings["ServerName"] = System.Environment.MachineName;
			ConfigurationManager.AppSettings["DatabaseName"] = "Odyssey";
			ConfigurationManager.AppSettings["SchemaPath"] = "Schemas";
		}
	}
}
