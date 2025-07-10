using Microsoft.JSInterop;
using Moq;

namespace CargoWiseNext.Blazor.Components.Test.JsInterop;

public class DragAndDropJsInteropTest
{
	Mock<IJSRuntime> _mockJsRuntime;
	Mock<IJSObjectReference> _mockJsModule;
	Mock<IDragAndDropInvokables> _mockInvokables;
	DotNetObjectReference<IDragAndDropInvokables> _dotNetRef;

	[SetUp]
	public void Setup()
	{
		_mockJsRuntime = new Mock<IJSRuntime>();
		_mockJsModule = new Mock<IJSObjectReference>();
		_mockInvokables = new Mock<IDragAndDropInvokables>();

		_dotNetRef = DotNetObjectReference.Create(_mockInvokables.Object);

		_mockJsRuntime
			.Setup(js => js.InvokeAsync<IJSObjectReference>("import", It.Is<object[]>(o => o[0]!.ToString()!.Contains("dragAndDrop.js"))))
			.ReturnsAsync(_mockJsModule.Object);

		_mockJsModule
			.Setup(m => m.InvokeAsync<bool>("dragAndDrop.init", It.IsAny<object[]>()))
			.Returns(ValueTask.FromResult(true));
	}

	[TearDown]
	public void TearDown()
	{
		_dotNetRef.Dispose();
	}

	[Test]
	public async Task InitAsync_InvokesInitOnJsModule()
	{
		var interop = new DragAndDropJsInterop(_mockJsRuntime.Object);

		await interop.InitAsync("drop-area", "drag-over", _dotNetRef);

		_mockJsRuntime.Verify(js => js.InvokeAsync<IJSObjectReference>("import", It.IsAny<object[]>()), Times.Once);

		_mockJsModule.Verify(m => m.InvokeAsync<object>(
			"dragAndDrop.init",
			It.Is<object[]>(args =>
				args[0]!.ToString() == "drop-area" &&
				args[1]!.ToString() == "drag-over" &&
				args[2] == _dotNetRef
			)),
			Times.Once);
	}

	[Test]
	public async Task TeardownAsync_InvokesTeardownOnJsModule()
	{
		var interop = new DragAndDropJsInterop(_mockJsRuntime.Object);

		await interop.InitAsync("drop-area", "drag-over", _dotNetRef);

		await interop.TeardownAsync();

		_mockJsModule.Verify(m => m.InvokeAsync<object>(
			"dragAndDrop.teardown",
			It.IsAny<object[]>()
		), Times.Once);
	}
}
