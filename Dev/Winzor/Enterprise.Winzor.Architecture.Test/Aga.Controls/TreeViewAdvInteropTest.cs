using System.IO;
using System.Threading.Tasks;
using Aga.Controls.JSInterop;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.JSInterop;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test.Aga.Controls;

internal class TreeViewAdvInteropTest
{
	Mock<IJSRuntimeWithMonitor> mockJsRuntime;
	Mock<IJSObjectReference> mockModule;
	TreeViewAdvJSInterop interop;

	[SetUp]
	public void Setup()
	{
		mockModule = new Mock<IJSObjectReference>();

		mockJsRuntime = new Mock<IJSRuntimeWithMonitor>();
		mockJsRuntime.Setup(i => i.InvokeAsync<IJSObjectReference>(It.IsAny<string>(), It.IsAny<object[]>())).ReturnsAsync(mockModule.Object);

		interop = new TreeViewAdvJSInterop(mockJsRuntime.Object, new DummyFileVersionHash());
	}

	[Test]
	public async Task TreeViewAdvInteropLoadModuleAsync_ImportsModuleWithHashVersion()
	{
		var fileInfoMock = new Mock<IFileInfo>();
		fileInfoMock.Setup(f => f.Exists).Returns(true);
		fileInfoMock.Setup(f => f.CreateReadStream()).Returns(() => new MemoryStream(new byte[] { 2 }));
		var fileProviderMock = new Mock<IFileProvider>();
		fileProviderMock.Setup(f => f.GetFileInfo(It.IsAny<string>())).Returns(fileInfoMock.Object);
		var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
		webHostEnvironmentMock.Setup(w => w.WebRootFileProvider).Returns(fileProviderMock.Object);

		var fileVersionHash = new FileVersionHash(webHostEnvironmentMock.Object);
		interop = new TreeViewAdvJSInterop(mockJsRuntime.Object, fileVersionHash);
		var importParams = "";
		var path = "/_content/Aga.Controls/js/treeviewadv.js";
		mockJsRuntime.Setup(i => i.InvokeAsync<IJSObjectReference>("import", new object[] { path + fileVersionHash.Get(path) })).Callback((string _, object[] p) => importParams = (string)p[0]).ReturnsAsync(mockModule.Object);
		// To cause the module to be loaded
		await interop.ChangeColumnWidthAsync(null, null, new Microsoft.AspNetCore.Components.ElementReference());

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
