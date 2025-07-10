using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;

namespace WinzorFramework.JSInterop;

class TextBoxInteropTest
{
	Mock<IJSRuntimeWithMonitor> mockJsRuntime;
	Mock<IJSObjectReference> mockModule;
	TextBoxJSInterop interop;

	[SetUp]
	public void Setup()
	{
		mockJsRuntime = new Mock<IJSRuntimeWithMonitor>();
		mockModule = new Mock<IJSObjectReference>();
		mockModule.Setup(i => i.InvokeAsync<object>(It.IsAny<string>(), It.IsAny<object[]>()));
		interop = new TextBoxJSInterop(mockJsRuntime.Object, new DummyFileVersionHash());
		var path = "/_content/WinzorFramework/js/module/textbox.js";
		mockJsRuntime.Setup(i => i.InvokeAsync<IJSObjectReference>("import", new object[] { path })).ReturnsAsync(mockModule.Object);
	}

	[Test]
	public async Task SetSelection()
	{
		var reference = new ElementReference();
		await interop.SetSelectionAsync(reference, 1, 2);

		Assert.That(mockModule.Invocations.FirstOrDefault(i => i.Arguments[0].ToString() == "setSelection"
		&& (int)((object[])i.Arguments[1])[1] == 1 && (int)((object[])i.Arguments[1])[2] == 2), Is.Not.Null);
	}

	[Test]
	public async Task ValidateTextOnKeyPress()
	{
		var reference = new ElementReference();
		await interop.ValidateTextOnKeyPressAsync(reference, "[^* ]");

		Assert.That(mockModule.Invocations.FirstOrDefault(i => i.Arguments[0].ToString() == "validateTextOnKeyPress"), Is.Not.Null);
	}

	[Test]
	public async Task PreloadTextBoxJSInterop()
	{
		using var ctx = new WinzorTestContext();

		var interop = new Mock<ITextBoxJSInterop>();
		interop.Setup(i => i.PreloadInterop());
		ctx.Services.AddScoped(_ => interop.Object);

		var rendered = await ctx.RenderControlOnFormAsync(() => new TextBox());

		interop.Verify(e => e.PreloadInterop(), Times.Once());
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
