using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using Enterprise.Registry.Business;
using Enterprise.Services.Scim.Api.Config;
using Enterprise.Services.Scim.Api.Filters;
using Enterprise.Services.Scim.Api.Helpers;
using Microsoft.Owin.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.Scim.Api.Test.Filters
{
	[TestFixture]
	public class ValidateIPAttributeTest
	{
		readonly Mock<ISafelistValidator> validatorMock;
		readonly Mock<IAppSettings> appSettingsMock;
		readonly ValidateIPAttribute attribute;

		public ValidateIPAttributeTest()
		{
			validatorMock = new Mock<ISafelistValidator>();
			appSettingsMock = new Mock<IAppSettings>();
			attribute = new ValidateIPAttribute(validatorMock.Object, appSettingsMock.Object);
		}

		[Test]
		public void TestOnActionExecuting_AllowedPath_DoesNotSetResponse()
		{
			var context = ReturnHttpActionContext();
			context.Request.RequestUri = new Uri("http://localhost/wtg/status");

			attribute.OnActionExecuting(context);

			Assert.That(context.Response, Is.Null);
		}

		[Test]
		public void TestOnActionExecuting_SafelistTypeNone_DoesNotSetResponse()
		{
			using var server = TestServer.Create<TestStartup>();
			TestingState.IsRunningTests = true;

			using (SystemDataRegistry.Instance.ScimSafeListType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ScimConstants.SafelistTypes.None))
			{
				var context = ReturnHttpActionContext();
				context.Request.RequestUri = new Uri("http://localhost/other/path");

				attribute.OnActionExecuting(context);

				Assert.That(context.Response, Is.Null);
			}
		}

		[Test]
		public void TestOnActionExecuting_InvalidIp_SetsForbiddenResponse()
		{
			using var server = TestServer.Create<TestStartup>();
			TestingState.IsRunningTests = true;

			using (SystemDataRegistry.Instance.ScimSafeListType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ScimConstants.SafelistTypes.AzureEntra))
			{
				validatorMock.Setup(v => v.ValidateIp(It.IsAny<string>(), It.IsAny<string[]>())).Returns(false);
				appSettingsMock.SetupGet(a => a.SafelistSkippedIps).Returns("127.0.0.1");
				var context = ReturnHttpActionContext();
				context.Request.RequestUri = new Uri("http://localhost/other/path");
				context.Request.Headers.Add("X-Real-IP", "192.168.0.1");

				attribute.OnActionExecuting(context);

				Assert.That(context.Response, Is.Not.Null);
				Assert.That(context.Response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
			}
		}

		[Test]
		public void TestOnActionExecuting_ValidIp_DoesNotSetResponse()
		{
			using var server = TestServer.Create<TestStartup>();
			TestingState.IsRunningTests = true;

			using (SystemDataRegistry.Instance.ScimSafeListType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ScimConstants.SafelistTypes.AzureEntra))
			{
				validatorMock.Setup(v => v.ValidateIp(It.IsAny<string>(), It.IsAny<string[]>())).Returns(true);
				appSettingsMock.SetupGet(a => a.SafelistSkippedIps).Returns("127.0.0.1");
				var context = ReturnHttpActionContext();
				context.Request.RequestUri = new Uri("http://localhost/other/path");
				context.Request.Headers.Add("X-Real-IP", "127.0.0.1");

				attribute.OnActionExecuting(context);

				Assert.That(context.Response, Is.Null);
			}
		}

		HttpActionContext ReturnHttpActionContext()
		{
			var context = new HttpActionContext();
			var request = new HttpRequestMessage();
			var controllerContext = new HttpControllerContext();

			controllerContext.Request = request;
			context.ControllerContext = controllerContext;

			return context;
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
