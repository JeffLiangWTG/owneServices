using Blazor.Diagrams.Core.Geometry;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.Integration;
using Microsoft.JSInterop;
using Moq;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;
using WinzorFramework.JSInterop;

namespace NetworkVisualisation.GUI.Winzor.Test;

static class TestHelpers
{
	public static NetworkUserControl SetupNetworkUserControl(IEnumerable<Entity>? entities = null, bool supportDiagramVisualStyles = true, bool isDiagramScaled = false)
	{
		var diagramEntity = Stub.DiagramEntity(supportDiagramVisualStyles, isDiagramScaled, showNonscheduledSection: false);
		var network = new NetworkBuilder().WithEntities(entities ?? Enumerable.Empty<INetworkEntity>()).WithDiagramEntity(diagramEntity).Build();
		return new NetworkUserControlBuilder().WithNetwork(network).Build();
	}

	public static void SetBoundingClientRect(this BunitJSInterop jsInterop, Rectangle rect)
	{
		jsInterop.Setup<Rectangle>("ZBlazorDiagrams.getBoundingClientRect", _ => true).SetResult(rect);
	}

	public static IJSRuntime GetMockedJSRuntime()
	{
		var jsModule = new Mock<IJSObjectReference>();
		jsModule.Setup(i => i.InvokeAsync<AvailableClipboardActions>("clipboard.getSupportedActions", It.IsAny<object[]>())).Returns(ValueTask.FromResult(new AvailableClipboardActions(true, true, true)));
		var jsRuntime = new Mock<IJSRuntime>();
		var path = "/_content/WinzorFramework/js/module/clipboard.js";
		jsRuntime.Setup(i => i.InvokeAsync<IJSObjectReference>("import", new object[] { path })).ReturnsAsync(jsModule.Object);
		return jsRuntime.Object;
	}
}
