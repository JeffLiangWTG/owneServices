using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.JSInterop;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;

namespace WinzorFramework.JSInterop;

class FileServiceInteropTest
{
	Mock<IJSRuntimeWithMonitor> mockJsRuntime;
	Mock<IJSObjectReference> mockModule;
	FileServiceJSInterop interop;

	[SetUp]
	public void Setup()
	{
		mockModule = new Mock<IJSObjectReference>();

		mockJsRuntime = new Mock<IJSRuntimeWithMonitor>();
		mockJsRuntime.Setup(i => i.InvokeAsync<IJSObjectReference>(It.IsAny<string>(), It.IsAny<object[]>())).ReturnsAsync(mockModule.Object);

		interop = new FileServiceJSInterop(mockJsRuntime.Object, new DummyFileVersionHash());
	}

	[Test]
	public async Task TestOpenFileDialogAsync_CallsInvokeJSNoTimeoutAsync()
	{
		mockModule.Setup(js => js.InvokeAsync<BrowserFile[]>(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<object[]>()));

		await interop.OpenFileDialogAsync(false);

		mockModule.Verify(js => js.InvokeAsync<BrowserFile[]>(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<object[]>()), Times.Once());
	}

	[Test]
	public async Task TestSaveFileDialogAsync_CallsInvokeJSNoTimeoutAsync()
	{
		mockModule
			.Setup(js => js.InvokeAsync<SaveFileDialogResult>(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<object[]>()))
			.ReturnsAsync(new SaveFileDialogResult(false, "mock", default));

		await interop.SaveFileDialogAsync("any", default);

		mockModule.Verify(js => js.InvokeAsync<SaveFileDialogResult>(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<object[]>()), Times.Once());
	}

	[Test]
	public async Task TestOpenDirectoryDialogAsync_CallsInvokeJSNoTimeoutAsync()
	{
		mockModule.Setup(js => js.InvokeAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<object[]>()));

		await interop.OpenDirectoryDialogAsync("any", "readwrite");

		mockModule.Verify(js => js.InvokeAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<object[]>()), Times.Once());
	}

	[Test]
	public async Task FileServiceInteropLoadModuleAsync_ImportsModuleWithHashVersion()
	{
		var fileInfoMock = new Mock<IFileInfo>();
		fileInfoMock.Setup(f => f.Exists).Returns(true);
		fileInfoMock.Setup(f => f.CreateReadStream()).Returns(() => new MemoryStream(new byte[] { 2 }));
		var fileProviderMock = new Mock<IFileProvider>();
		fileProviderMock.Setup(f => f.GetFileInfo(It.IsAny<string>())).Returns(fileInfoMock.Object);
		var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
		webHostEnvironmentMock.Setup(w => w.WebRootFileProvider).Returns(fileProviderMock.Object);

		var fileVersionHash = new FileVersionHash(webHostEnvironmentMock.Object);
		interop = new FileServiceJSInterop(mockJsRuntime.Object, fileVersionHash);
		var importParams = "";
		var path = "/_content/WinzorFramework/js/module/fileService.js";
		mockJsRuntime.Setup(i => i.InvokeAsync<IJSObjectReference>("import", new object[] { path + fileVersionHash.Get(path) })).Callback((string _, object[] p) => importParams = (string)p[0]).ReturnsAsync(mockModule.Object);
		// To cause the module to be loaded
		await interop.OpenDirectoryDialogAsync("any", "readwrite");

		Assert.That(importParams, Does.Contain("?v="));
	}

	[TearDown]
	public async Task TearDown()
	{
		if (interop != null)
		{
			await interop.DisposeAsync();
		}
	}
}
