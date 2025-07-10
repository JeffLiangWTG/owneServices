using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using WinzorTestFramework;

namespace WinzorFramework.Extensions;

class BArchitectureMapEndpointExtensionTest
{
	[Test]
	public async Task TestGetImageReturnOk()
	{
		await using var ctx = new InMemoryTestServerContext();
		var imageCacher = (ImageCacher)ctx.HostServices.GetService(typeof(IImageCacher));
		const string base64Image = "iVBORw0KGgoAAAANSUhEUgAAAAMAAAADCAYAAABWKLW/AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAANSURBVBhXYyAGMDAAAAAnAAF2ypRxAAAAAElFTkSuQmCC";
		var imageUri = imageCacher.AddImage(base64Image);

		using var client = new HttpClient();
		var response = await client.GetAsync($"{ctx.ServerBaseUrl}{imageUri}");
		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

		var cacheControl = response.Headers.CacheControl;
		Assert.That(cacheControl.Public, Is.True);
		Assert.That(cacheControl.MaxAge, Is.EqualTo(TimeSpan.FromDays(1)));

		var content = await response.Content.ReadAsStringAsync();
		Assert.That(content, Is.EqualTo(base64Image));
	}

	[Test]
	public async Task TestGetImageReturnNotFound()
	{
		await using var ctx = new InMemoryTestServerContext();
		using var client = new HttpClient();

		var response = await client.GetAsync($"{ctx.ServerBaseUrl}{IImageCacher.ApiPrefix}{Guid.NewGuid().ToString()}");
		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

		var cacheControl = response.Headers.CacheControl;
		Assert.That(cacheControl, Is.Null);

		var content = await response.Content.ReadAsStringAsync();
		Assert.That(content, Is.EqualTo(string.Empty));
	}
}
