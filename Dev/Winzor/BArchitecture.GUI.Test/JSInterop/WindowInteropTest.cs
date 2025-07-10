using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;

namespace WinzorFramework.JSInterop;

class WindowJSInteropTest
{
	Mock<IJSRuntimeWithMonitor> mockJsRuntime;
	Mock<IJSObjectReference> mockModule;
	WindowJSInterop interop;

	[SetUp]
	public void Setup()
	{
		mockJsRuntime = new Mock<IJSRuntimeWithMonitor>();
		mockModule = new Mock<IJSObjectReference>();
		mockModule.Setup(i => i.InvokeAsync<object>(It.IsAny<string>(), It.IsAny<object[]>()));
		interop = new WindowJSInterop(mockJsRuntime.Object, new DummyFileVersionHash());
		var path = "/_content/WinzorFramework/js/module/window.js";
		mockJsRuntime.Setup(i => i.InvokeAsync<IJSObjectReference>("import", new object[] { path })).ReturnsAsync(mockModule.Object);
	}

	[Test]
	[TestCase("tel:1234", false, "tel:1234", TestName = "{m}_TelUrlOpensInCurrentWindow")]
	[TestCase("callto:1234", false, "callto:1234", TestName = "{m}_SkypeUrlOpensInCurrentWindow")]
	[TestCase("http://wisetechglobal.com", true, "http://wisetechglobal.com", TestName = "{m}_HttpUrlOpensInNewWindow")]
	[TestCase("https://wisetechglobal.com", true, "https://wisetechglobal.com", TestName = "{m}_HttpsUrlOpensInNewWindow")]
	[TestCase("www.wisetechglobal.com", true, "http://www.wisetechglobal.com", TestName = "{m}_UrlWithoutProtocolOpensInNewWindow")]
	public async Task TestOpenUrl(string url, bool newWindow, string expectedUrl)
	{
		await interop.OpenUrlAsync(url);
		Assert.That(mockModule.Invocations.Count(i => i.ToString() == $"IJSObjectReference.InvokeAsync<IJSVoidResult>(\"openUrl\", [\"{expectedUrl}\", {newWindow}])"), Is.EqualTo(1));
	}

	[Test]
	[TestCase("afp://example.com/file", TestName = "{m}_afpURLOpensInCurrentWindow")]
	[TestCase("data:text/plain;base64,SGVsbG8sIFdvcmxkIQ==", TestName = "{m}_dataURLOpensInCurrentWindow")]
	[TestCase("disk://example.com/path/to/file", TestName = "{m}_diskURLOpensInCurrentWindow")]
	[TestCase("disks://example.com/path/to/file", TestName = "{m}_disksURLOpensInNewWindow")]
	[TestCase("file:///path/to/file", TestName = "{m}_fileURLOpensInNewWindow")]
	[TestCase("hcp://example.com/help", TestName = "{m}_hcpURLOpensInCurrentWindow")]
	[TestCase("ie.http://example.com", TestName = "{m}_ie.httpURLOpensInNewWindow")]
	[TestCase("javascript:alert('Hello, World!')", TestName = "{m}_javascriptURLOpensInCurrentWindow")]
	[TestCase("ms-help://example.com/help", TestName = "{m}_ms-helpURLOpensInNewWindow")]
	[TestCase("nntp://example.com/group/article", TestName = "{m}_nntpURLOpensInCurrentWindow")]
	[TestCase("res://example.com/resource", TestName = "{m}_resURLOpensInNewWindow")]
	[TestCase("shell://example.com/command", TestName = "{m}_shellURLOpensInCurrentWindow")]
	[TestCase("vbscript:MsgBox(\"Hello, World!\")", TestName = "{m}_vbscriptURLOpensInNewWindow")]
	[TestCase("view-source:http://example.com/page", TestName = "{m}_view-sourceURLOpensInCurrentWindow")]
	[TestCase("vnd.ms.radio://example.com/station", TestName = "{m}_vnd.ms.radioURLOpensInNewWindow")]
	public void TestBlockUrl(string url)
	{
		var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await interop.OpenUrlAsync(url));
		Assert.That(exception.Message, Is.EqualTo(url + " has a protocol that's dangerous to open"));
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
