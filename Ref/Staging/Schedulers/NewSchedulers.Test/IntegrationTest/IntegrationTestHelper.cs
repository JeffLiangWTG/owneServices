using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.RefDbRepo.Staging.NewSchedulers.Test.IntegrationTest
{
	public static class IntegrationTestHelper
	{
		public static WebApplicationFactory<Program> WebAppFactory
		{
			get
			{
				using (var factory = new WebApplicationFactory<Program>())
				{
					return factory.WithWebHostBuilder(builder =>
					{
						builder.UseContentRoot(Directory.GetCurrentDirectory());
						// Change the Environment to avoid System.IO.DirectoryNotFoundException : C:\BS\git\wtg\RefDataRepo\RefDataRepo\Staging\Schedulers\NewSchedulers\wwwroot\ in DAT
						builder.UseEnvironment("IntegrationTest");
						builder.ConfigureTestServices(services =>
						{
							services.Configure<AuthenticationOptions>(o =>
							{
								if (o.Schemes is List<AuthenticationSchemeBuilder> schemes)
								{
									schemes.RemoveAll(s => s.Name == NegotiateDefaults.AuthenticationScheme);
									o.SchemeMap.Remove(NegotiateDefaults.AuthenticationScheme);
								}
							});
						});
					});
				}
			}
		}
	}
}
