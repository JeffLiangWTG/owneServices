using System.IO;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace CargoWise.RefDbRepo.Staging.NewService.Test
{
	static class IntegrationTestHelper
	{
		/// <summary>
		/// IMPORTANT!!!
		/// Please dispose it whenever use it, and always put WebApplicationFactory<Program>().WithWebHostBuilder in one line
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "It will be disposed after each test runs")]
		public static WebApplicationFactory<Program> GetWebAppFactory(IStagingRepository stagingRepo, IRefDbRepoCrypto refDbRepoCrypto)
		{
			return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
			{
				builder.UseContentRoot(Directory.GetCurrentDirectory());
				// UseStaticWebAssets is auto called when HostingEnvironment.IsDevelopment() by default. Change the Environment to avoid System.IO.DirectoryNotFoundException: C:\BS\git\wtg\...\wwwroot\ in DAT.
				builder.UseEnvironment("IntegrationTest");
				builder.ConfigureTestServices(services =>
				{
					services.RemoveAll<IRefDbRepoCrypto>();
					services.AddScoped(sp => stagingRepo);
					services.AddScoped(sp => refDbRepoCrypto);
				});
			});
		}

		public static ControllerContext SetupControllerContext(IStagingRepository repo)
		{
			var controllerContext = new ControllerContext();
			var httpContext = new Mock<HttpContext>();
			object sharedRepo = repo;
			httpContext.Setup(x => x.Items.TryGetValue("Batch_DbContext", out sharedRepo)).Returns(true);
			controllerContext.HttpContext = httpContext.Object;
			return controllerContext;
		}
	}
}
