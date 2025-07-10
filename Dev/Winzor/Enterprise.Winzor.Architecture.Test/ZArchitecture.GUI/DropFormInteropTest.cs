using System.Linq;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.GUI.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;

namespace Enterprise.ZArchitecture.GUI.Test.JSInterop;

class DropFormInteropTest
{
	Mock<IJSRuntimeWithMonitor> mockJsRuntime;
	Mock<IJSObjectReference> mockModule;
	DropFormJSInterop interop;

	[SetUp]
	public void Setup()
	{
		mockJsRuntime = new Mock<IJSRuntimeWithMonitor>();
		mockModule = new Mock<IJSObjectReference>();
		mockModule.Setup(i => i.InvokeAsync<object>(It.IsAny<string>(), It.IsAny<object[]>()));
		interop = new DropFormJSInterop(mockJsRuntime.Object, new DummyFileVersionHash());
		var path = "/_content/Enterprise.ZArchitecture.GUI/js/dropForm.js";
		mockJsRuntime.Setup(i => i.InvokeAsync<IJSObjectReference>("import", new object[] { path })).ReturnsAsync(mockModule.Object);
	}

	[Test]
	public async Task TestScrollTo()
	{
		var reference = new ElementReference();
		await interop.ScrollToAsync(reference, 0, 0);
		Assert.That(mockModule.Invocations.FirstOrDefault(i => i.Arguments[0].ToString() == "scrollTo"), Is.Not.Null);
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
