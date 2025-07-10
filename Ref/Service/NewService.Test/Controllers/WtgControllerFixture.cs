using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test.Controllers
{
	class WtgControllerFixture
	{
		[Test]
		public async Task GetWtgReadyResponse()
		{
			var url = new Uri("http://localhost/wtg/ready");
			using (var factory = WebApplicationFactoryHelper.WebAppFactory.WithWebHostBuilder(builder => builder.UseEnvironment("WtgReadyTest")))
			using (var client = factory.CreateClient())
			{
				foreach (var method in GetAllHttpMethods())
				{
					using (var request = new HttpRequestMessage(method, url))
					{
						var response = await client.SendAsync(request);
						if (method == HttpMethod.Get || method == HttpMethod.Head)
						{
							Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
						}
						else
						{
							Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
						}
					}
				}
			}
		}

		List<HttpMethod> GetAllHttpMethods()
		{
			var methodList = new List<HttpMethod>();
			var properties = typeof(HttpMethod).GetProperties(BindingFlags.Public | BindingFlags.Static);
			foreach (var  property in properties)
			{
				if (property.PropertyType == typeof(HttpMethod))
				{
					methodList.Add(property.GetValue(null) as HttpMethod);
				}
			}
			return methodList;
		}
	}
}
