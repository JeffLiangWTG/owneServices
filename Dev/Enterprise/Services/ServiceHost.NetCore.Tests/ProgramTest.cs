using System.Collections.Immutable;
using System.IO;
using System.Net;
using CargoWise.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.NetCore
{
	class ProgramTest : TestCase
	{
		readonly ImmutableArray<string> SwaggerUrls =
		[
			"/swagger/index.html",
			"/swagger/v1/swagger.json",
		];

		public void TestSwaggerEndpoints_ShouldReturnOk()
		{
			using (Db.DisposableActionForDbConnection())
			{
				using var factory = new WebApplicationFactory<Program>();

				var client = factory.WithWebHostBuilder(builder =>
				{
					builder.UseUrls("http://localhost");
					// Critical to make integration tests work on DAT
					builder.UseContentRoot(Directory.GetCurrentDirectory());
				}).CreateClient();

				CombineAssertions(delegate
				{
					foreach (var url in SwaggerUrls)
					{
						var response = client.GetAsync(url).GetAwaiter().GetResult();
						AssertEquals(expected: HttpStatusCode.OK, actual: response.StatusCode);
					}
				});
			}
		}
	}
}
