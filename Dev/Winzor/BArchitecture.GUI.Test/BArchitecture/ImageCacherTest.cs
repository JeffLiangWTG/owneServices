using Microsoft.AspNetCore.Hosting;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;

namespace WinzorFramework;

public class ImageCacherTest
{
	[Test]
	public void TestGetImageUri()
	{
		var mockWebHostEnvironment = Mock.Of<IWebHostEnvironment>();
		var fileVersionHash = new FileVersionHash(mockWebHostEnvironment);
		var imageCacher = new ImageCacher(fileVersionHash);

		imageCacher.AddImage(Base64Image);
		var imageUri = imageCacher.GetImageUri(Base64Image);

		Assert.That(imageUri.ToString(), Is.EqualTo($"{IImageCacher.ApiPrefix}e33muUVKv7gXqyfPBAGmyXjJzxm7C-erCOFExZ47jt0"));
	}

	[Test]
	public void TestAddImageUri()
	{
		var mockWebHostEnvironment = Mock.Of<IWebHostEnvironment>();
		var fileVersionHash = new FileVersionHash(mockWebHostEnvironment);
		var imageCacher = new ImageCacher(fileVersionHash);

		var imageUri = imageCacher.AddImage(Base64Image);

		Assert.That(imageUri.ToString(), Is.EqualTo($"{IImageCacher.ApiPrefix}e33muUVKv7gXqyfPBAGmyXjJzxm7C-erCOFExZ47jt0"));
	}

	[Test]
	public void TestGetImageBase64ById()
	{
		var mockWebHostEnvironment = Mock.Of<IWebHostEnvironment>();
		var fileVersionHash = new FileVersionHash(mockWebHostEnvironment);
		var imageCacher = new ImageCacher(fileVersionHash);

		var imageUri = imageCacher.AddImage(Base64Image);
		var imageId = imageUri.ToString().Replace(IImageCacher.ApiPrefix, string.Empty);

		var actualBase64 = imageCacher.GetImageBase64ById(imageId);

		Assert.That(actualBase64, Is.EqualTo(Base64Image));
	}

	const string Base64Image = "iVBORw0KGgoAAAANSUhEUgAAAAMAAAADCAYAAABWKLW/AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAANSURBVBhXYyAGMDAAAAAnAAF2ypRxAAAAAElFTkSuQmCC";
}
