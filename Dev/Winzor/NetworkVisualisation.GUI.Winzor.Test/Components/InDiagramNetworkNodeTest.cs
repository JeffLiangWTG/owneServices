using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Moq;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace NetworkVisualisation.GUI.Winzor.Test.Components;
using Bunit;
using Enterprise.Winzor.Architecture.Test;
using Microsoft.AspNetCore.Components.Web;
using static PlaywrightTestContext;

public class InDiagramNetworkNodeTest : BunitTestContext
{
	static IEnumerable<TestCaseData> NodeTypeCases => WinzorShapeTypes.Values.Select(s => new TestCaseData(s));

	[WithPlaywrightPage]
	[TestCaseSource(nameof(NodeTypeCases))]
	public async Task TestNodeCursorIsMoveIfNodeIsNotPinnedAsync(string shapeType)
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var entity = new EntityForTest()
		{
			Name = "Node Name",
			Width = 200,
			Height = 200,
			ShapeType = shapeType
		};
		var network = new NetworkBuilder().WithEntities(entity).Build();

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			networkUserControl.Width = 1250;
			networkUserControl.Height = 700;
			return networkUserControl;
		});

		var nodeLocator = page.GetByTitle("Node: Node Name");
		await nodeLocator.HoverAsync();

		Assert.That(await nodeLocator.GetComputedStyleAsync("cursor"), Is.EqualTo("move"));

		var box = (await nodeLocator.BoundingBoxAsync())!;

		Assert.That(box, Is.Not.Null);

		var elementUnderCursor = (await Page.EvaluateHandleAsync(
			$"() => document.elementFromPoint({box.X + box.Width / 2}, {box.Y + box.Height / 2})"
		))
			.AsElement()!;

		Assert.That(await elementUnderCursor.GetComputedStyleAsync("cursor"), Is.EqualTo("move"));
	}

	[Test, WithPlaywrightPage]
	[TestCaseSource(nameof(NodeTypeCases))]
	public async Task TestNodeCursorIsDefaultIfNodeIsPinnedAsync(string shapeType)
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var entity = new EntityForTest()
		{
			Name = "Node Name",
			Width = 200,
			Height = 200,
			ShapeType = shapeType,
			EntityState = EntityState.Fixed
		};
		var network = new NetworkBuilder().WithEntities(entity).Build();

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			networkUserControl.Width = 1250;
			networkUserControl.Height = 700;
			return networkUserControl;
		});

		var nodeLocator = page.GetByTitle("Node: Node Name");
		await nodeLocator.HoverAsync();

		Assert.That(await nodeLocator.GetComputedStyleAsync("cursor"), Is.EqualTo("default"));

		var box = (await nodeLocator.BoundingBoxAsync())!;

		Assert.That(box, Is.Not.Null);

		var elementUnderCursor = (await Page.EvaluateHandleAsync(
			$"() => document.elementFromPoint({box.X + box.Width / 2}, {box.Y + box.Height / 2})"
		))
			.AsElement()!;

		Assert.That(await elementUnderCursor.GetComputedStyleAsync("cursor"), Is.EqualTo("default"));
	}

	[Test]
	public void DragWorksWhenMouseEntersNotesSection()
	{
		var entity = Stub.Entity(e => e.ShapeType = ShapeTypes.Shape);
		var diagramEntity = Stub.Entity();
		var entities = new ImpObservableSet<INetworkEntity>(new[] { entity });
		var network = Mock.Of<INetwork>(x => x.Entities == entities && x.DiagramEntity == diagramEntity && x.EntityPositionStrategy == new EntityPositionStrategy());

		var networkUserControl = new NetworkUserControlForTestBuilder().WithNetwork(network).Build();
		using var diagramModel = new NCNDiagramModelBuilder().WithNetworkUserControl(networkUserControl).Build();
		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var notes = diagram.Find(".jobnode__notes > .textarea__resizable");

		Assert.DoesNotThrowAsync(async () =>
		{
			await notes.TriggerEventAsync("onpointermove", new PointerEventArgs());
		});
	}
}
