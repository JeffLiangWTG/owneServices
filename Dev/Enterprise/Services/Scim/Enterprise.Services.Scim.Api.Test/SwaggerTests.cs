using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Microsoft.Owin.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.Scim.Api.Test
{
	[TestFixture]
	internal class SwaggerTests
	{
		[Test]
		public async Task TestConfigSwaggerOnProduction()
		{
			var keyMock = new Mock<IProductRegistrationKey>();
			keyMock.Setup(a => a.DatabaseType).Returns(DatabaseTypes.Codes.Test);
			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.SetupGet(r => r.Key).Returns(keyMock.Object);

			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			using (var server = TestServer.Create<TestStartup>())
			{
				Assert.That(server, Is.Not.Null);

				var response = await server.HttpClient.GetAsync("/swagger/ui/index");
				Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			}
		}

		[Test]
		public async Task TestShouldNotConfigSwaggerOnNoProduction()
		{
			var keyMock = new Mock<IProductRegistrationKey>();
			keyMock.Setup(a => a.DatabaseType).Returns(DatabaseTypes.Codes.Production);
			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.SetupGet(r => r.Key).Returns(keyMock.Object);

			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			using (var server = TestServer.Create<TestStartup>())
			{
				Assert.That(server, Is.Not.Null);

				var response = await server.HttpClient.GetAsync("/swagger/ui/index");
				Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
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
