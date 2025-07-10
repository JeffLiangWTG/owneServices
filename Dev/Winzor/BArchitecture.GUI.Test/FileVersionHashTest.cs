using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;

namespace WinzorFramework.Test;

internal class FileVersionHashTest
{
	[Test]
	public void TestFileVersionHashGet_InvalidPath_ReturnsEmptyHash()
	{
		var fileInfoMock = new Mock<IFileInfo>();
		fileInfoMock.Setup(f => f.Exists).Returns(false);
		var fileProviderMock = new Mock<IFileProvider>();
		fileProviderMock.Setup(f => f.GetFileInfo(It.IsAny<string>())).Returns(fileInfoMock.Object);
		var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
		webHostEnvironmentMock.Setup(w => w.WebRootFileProvider).Returns(fileProviderMock.Object);

		var fileVersionHash = new FileVersionHash(webHostEnvironmentMock.Object);
		Assert.That(fileVersionHash.Get("a/file/that/will/not/exist.css"), Is.EqualTo(""));
	}

	[Test]
	public void TestFileVersionHashGet_ValidPath_ReturnsVersionHash()
	{
		var fileInfoMock = new Mock<IFileInfo>();
		fileInfoMock.Setup(f => f.Exists).Returns(true);
		fileInfoMock.Setup(f => f.CreateReadStream()).Returns(() => new MemoryStream(new byte[] { 2 }));
		var fileProviderMock = new Mock<IFileProvider>();
		fileProviderMock.Setup(f => f.GetFileInfo(It.IsAny<string>())).Returns(fileInfoMock.Object);
		var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
		webHostEnvironmentMock.Setup(w => w.WebRootFileProvider).Returns(fileProviderMock.Object);

		var fileVersionHash = new FileVersionHash(webHostEnvironmentMock.Object);
		Assert.That(fileVersionHash.Get("a/file/that/will/exist.css"), Does.Contain("?v="));
	}

	[Test]
	public void TestGetHash()
	{
		using var stream = new MemoryStream(new byte[] { 2 });
		var version = new FileVersionHash(Mock.Of<IWebHostEnvironment>()).GetHash(stream);
		Assert.That(version, Is.EqualTo("28G0yQD_5I1XW12lxjgEASX2XbD-PiRJS3bqmGRX2YY"));
	}
}
