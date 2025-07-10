using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Models;
using CargoWise.RefDbRepo.Common.Web.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class ServiceIntergrationFixture
	{
		[Test]
		public void IntegrationTest()
		{
			var providerMock = new Mock<IAuthenticationHandlerProvider>();
			var handlerMock = new Mock<IAuthenticationHandler>();
			var principal = new ClaimsPrincipal(new GenericIdentity("Dummy"));
			var authenticateTicket = new AuthenticationTicket(principal, AuthType.BasicAuth);
			var result = AuthenticateResult.Success(authenticateTicket);
			handlerMock.Setup(x => x.AuthenticateAsync()).ReturnsAsync(result);
			handlerMock.Setup(x => x.InitializeAsync(It.IsAny<AuthenticationScheme>(), It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
			providerMock.Setup(x => x.GetHandlerAsync(It.IsAny<HttpContext>(), It.IsAny<string>())).ReturnsAsync(handlerMock.Object);

			using (var factory = WebApplicationFactoryHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.AddScoped<IReferenceDataRepository, ObjectReferenceDataRepository>();
					services.AddScoped<IReadOnlyReferenceDataRepository, ObjectReferenceDataRepository>();
					services.AddScoped<IReferenceDataService<RefDataSet>, DummyDataSetService>();
					services.AddSingleton(providerMock.Object);
				});
			}))
			{
				var uri = new Uri("http://localhost/DummyDataSet/GetServerTimestamp");
				Assert.DoesNotThrowAsync(async () =>
				{
					using (var client = factory.CreateClient())
					using (var response = await client.GetAsync(uri))
					{
						response.EnsureSuccessStatusCode();
					}
				});
			}
		}
	}
}
