using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;

namespace WinzorFramework.JSInterop;

class ClipboardInteropTest
{
	Mock<IJSRuntimeWithMonitor> mockJsRuntime;
	Mock<IJSObjectReference> mockModule;
	ClipboardJSInterop interop;

	[SetUp]
	public void Setup()
	{
		mockJsRuntime = new Mock<IJSRuntimeWithMonitor>();
		mockModule = new Mock<IJSObjectReference>();
		mockModule.Setup(i => i.InvokeAsync<object>(It.IsAny<string>(), It.IsAny<object[]>()));
		interop = new ClipboardJSInterop(mockJsRuntime.Object, new DummyFileVersionHash());
		var path = "/_content/WinzorFramework/js/module/clipboard.js";
		mockJsRuntime.Setup(i => i.InvokeAsync<IJSObjectReference>("import", new object[] { path })).ReturnsAsync(mockModule.Object);
	}

	[Test]
	public async Task ClipboardActions()
	{
		await interop.CopyAsync();
		Assert.That(mockModule.Invocations.Count(i => i.Arguments[0].ToString() == "clipboard.copy"), Is.EqualTo(1));
		await interop.CutAsync();
		Assert.That(mockModule.Invocations.Count(i => i.Arguments[0].ToString() == "clipboard.cut"), Is.EqualTo(1));
		await interop.PasteAsync();
		Assert.That(mockModule.Invocations.Count(i => i.Arguments[0].ToString() == "clipboard.paste"), Is.EqualTo(1));
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
