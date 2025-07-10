using AngleSharp.Css.Dom;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Components;
using CargoWise.NetworkVisualisation.GUI.Controls.NodeResizing;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.Winzor.Architecture.Test;
using Microsoft.AspNetCore.Components;
using Microsoft.Playwright;
using Moq;
using NetworkVisualisation.GUI.Winzor.Test.Extensions;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Extentions;
using WTG.PlaywrightTesting;
using Point = Blazor.Diagrams.Core.Geometry.Point;
using PointerEventArgs = Blazor.Diagrams.Core.Events.PointerEventArgs;
using Size = Blazor.Diagrams.Core.Geometry.Size;

namespace NetworkVisualisation.GUI.Winzor.Test.Components;

public abstract class NetworkNodeTest<TNodeViewModel, TNetworkNode, TNetworkNodeModel> : BunitTestContext
	   where TNodeViewModel : NodeViewModel
	   where TNetworkNode : NetworkNode<TNetworkNodeModel>
	   where TNetworkNodeModel : NetworkNodeModel
{
	protected string NodeElement => ".networknode";
	protected string NotesElement => ".networknode__notes";
	protected string NotesTextAreaElement => ".networknode__notes > .textarea__resizable";
	protected string DiagramNameElement => ".networknode__diagramname";
	protected string DiagramNameTextAreaElement => ".networknode__diagramname > .textarea__resizable";
	protected string CardElement => ".networknode__card";
	protected string ResizerElement => ".networknode__resizer";
	protected string CompletionCriteriaElement => ".networknode__completioncriteria";
	protected string CompletionCriteriaTextAreaElement => ".networknode__completioncriteria > .textarea__resizable";
	protected string JobNameElement => ".networknode__jobname";
	protected string NodeElementInDiagram => ".networknode--indiagram";
	protected string InDiagramWrapperElement => ".networknode--indiagram_wrapper";

	protected abstract string ShapeType { get; }
	protected abstract TNetworkNodeModel CreateNodeModel(INetworkEntity entity);
	protected abstract TNetworkNodeModel CreateNodeModel(TNodeViewModel nodeViewModel);
	protected INetworkEntity CreateEntityWithSelectableText() => Stub.Entity(e =>
	{
		e.ShapeType = ShapeType;
		e.Width = 200;
		e.Height = 200;
		e.Notes = "aaaaaaaaaaaaaaaaaaaaaaaaa";
		e.Name = "aaaaaaaaaaaaaaaaaaaaaaaaa";
	});

	(NetworkViewModel, EntityForTest, TNodeViewModel) CreateTestEntity(double entityWidth = 0.0, double entityHeight = 0.0)
	{
		var entity = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Width = entityWidth;
			e.Height = entityHeight;
		});

		var network = new NetworkBuilder()
			.WithEntities(entity)
			.Build();

		var networkViewModel = new NetworkViewModelBuilder()
			.WithNetwork(network)
			.Build();

		var nodeViewModel = networkViewModel.CreateNodeViewModel(entity) as TNodeViewModel;

		return (networkViewModel, entity, nodeViewModel!);
	}

	[Test]
	public void TestNotes()
	{
		var entity = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Notes = "Notes";
		});
		var networkViewModel = new NetworkViewModelBuilder().Build();
		var nodeViewModel = networkViewModel.CreateNodeViewModel(entity);

		var node = RenderNode(nodeViewModel);

		var notesTextArea = node.Find(NotesTextAreaElement);
		Assert.That(notesTextArea.GetAttribute("value"), Is.EqualTo("Notes"));
		Assert.That(notesTextArea.GetAttribute("placeholder"), Is.EqualTo("Notes"));
	}

	[Test]
	public void TestSize()
	{
		var (_, _, viewModel) = CreateTestEntity(100, 200);
		var node = RenderNodeInDiagram(viewModel);
		var style = node.Find(InDiagramWrapperElement).GetStyle();

		Assert.That(style.GetHeight(), Is.EqualTo("200px"));
		Assert.That(style.GetWidth(), Is.EqualTo("100px"));
	}

	[Test]
	public void TestSizeStyleIsBoundToViewModelInDiagram()
	{
		var entity = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Width = 100;
			e.Height = 200;
		});

		var node = RenderNodeInDiagram(entity);
		var nodeModel = node.Instance.Node;

		nodeModel!.Size = new Size(300, 400);
		nodeModel.TriggerResized();
		var root = node.Find(InDiagramWrapperElement);
		Assert.That(root.GetStyle().GetWidth(), Is.EqualTo("300px"));
		Assert.That(root.GetStyle().GetHeight(), Is.EqualTo("400px"));
	}

	[Test]
	public void TestSizeStyleIsNotBoundToViewModelOutOfDiagram()
	{
		var entity = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Width = 100;
			e.Height = 200;
		});

		var node = RenderComponent<TNetworkNode>(parameters =>
		{
			parameters.Add(p => p.Node, CreateNodeModel(entity));
		});
		var root = node.Find(NodeElement);
		Assert.That(root.GetStyle().GetWidth(), Is.EqualTo(string.Empty));
		Assert.That(root.GetStyle().GetHeight(), Is.EqualTo(string.Empty));
	}

	[Test]
	public void TestDiagramName()
	{
		var (_, _, viewModel) = CreateTestEntity();
		viewModel.DiagramName = "DiagramName";

		var node = RenderNode(viewModel);

		var diagramName = node.Find(DiagramNameElement);
		var diagramNameTextArea = node.Find(DiagramNameTextAreaElement);
		Assert.That(diagramNameTextArea.GetAttribute("value"), Is.EqualTo("DiagramName"));
		Assert.That(diagramName.GetAttribute("style"), Is.EqualTo("font-weight: 900;"));
	}

	[Test]
	public async Task TestDiagramNameIsBoundToViewModelAsync()
	{
		var (_, _, viewModel) = CreateTestEntity();
		viewModel.DiagramName = "DiagramName";

		var node = RenderNode(viewModel);
		var nodeModel = node.Instance.Node;

		var diagramNameTextArea = node.Find(DiagramNameTextAreaElement);
		await diagramNameTextArea.ChangeAsync(new ChangeEventArgs() { Value = "Updated DiagramName" });
		Assert.That(nodeModel!.DiagramName, Is.EqualTo("Updated DiagramName"));
		Assert.That(viewModel.DiagramName, Is.EqualTo("Updated DiagramName"));
	}

	[Test]
	public async Task TestNotesIsBoundToViewModelAsync()
	{
		var entity = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Notes = "Notes";
		});
		var networkViewModel = new NetworkViewModelBuilder().Build();
		var viewModel = networkViewModel.CreateNodeViewModel(entity);

		var node = RenderNode(viewModel);
		var nodeModel = node.Instance.Node;

		var notesTextArea = node.Find(NotesTextAreaElement);
		await notesTextArea.ChangeAsync(new ChangeEventArgs() { Value = "Updated Notes" });
		Assert.That(nodeModel!.Notes, Is.EqualTo("Updated Notes"));
		Assert.That(viewModel.Notes, Is.EqualTo("Updated Notes"));
	}

	[Test]
	public void TestBackgroundColor()
	{
		var (_, _, viewModel) = CreateTestEntity();

		var node = RenderNode(viewModel);

		var card = node.Find(CardElement);
		if (viewModel.StatusColors.colors.Count() > 1)
		{
			var background = card.GetStyle().GetBackgroundImage();
			string degree = viewModel.StatusColors.colors.Count() == 4 ? "90deg" : "180deg";
			Assert.That(background, Does.StartWith($"linear-gradient({degree}"));
			foreach (var color in viewModel.StatusColors.colors)
			{
				var c = color.color;
				Assert.That(background, Does.Contain($"rgba({c.R}, {c.G}, {c.B}, 1)"));
			}
		}
		else
		{
			var background = card.GetStyle().GetBackgroundColor();
			var c = viewModel.StatusColors.colors.First().color;
			Assert.That(background, Does.Contain($"rgba({c.R}, {c.G}, {c.B}, 1)"));
		}
	}

	[Test]
	public void TestSelectionStyle()
	{
		var entity = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Height = 10;
			e.Width = 10;
		});

		var diagram = RenderDiagram(entity);
		var node = diagram.FindComponent<InDiagramNetworkNode<TNetworkNodeModel, TNetworkNode>>();

		Assert.That(node.Find(NodeElementInDiagram).Attributes["data-selected"], Is.Null);

		diagram.Instance.DiagramModel!.SelectModel(node!.Instance!.Node!, true);
		Assert.That(node.Find(NodeElementInDiagram).Attributes["data-selected"], Is.Not.Null);

		diagram.Instance.DiagramModel!.UnselectAll();
		Assert.That(node.Find(NodeElementInDiagram).Attributes["data-selected"], Is.Null);
	}

	[Test]
	public void TestCardTooltip()
	{
		var (_, _, viewModel) = CreateTestEntity();
		var node = RenderNode(viewModel);

		var component = node.Find(NotesElement);

		Assert.That(component.GetAttribute("title"), Is.Null);
	}

	[Test]
	public void TestDeleteIconTooltipForNode()
	{
		var (_, _, viewModel) = CreateTestEntity();
		var jobNode = RenderNodeInDiagram(viewModel);
		var node = jobNode.Instance.Node!;

		var deleteIconComponent = jobNode.Find(".deleteicon__deletecircle");

		Assert.That(node.DeleteTooltip, Is.EqualTo(Res.GetString("38903928-25fb-420a-b677-4c80df1cbc87", "Remove shape from diagram.")));
		Assert.That(deleteIconComponent.GetAttribute("title"), Is.EqualTo(node.DeleteTooltip));
	}

	[Test, WithPlaywrightPage]
	public async Task TestDeleteIconDisplaysWhenCoveredByAnotherShapeAsync()
	{
		var entity1 = Stub.Entity(e =>
		{
			e.Name = "Node 1";
			e.Width = 100;
			e.Height = 100;
			e.X = 100;
			e.Y = 100;
			e.ZIndex = 0;
		});

		var entity2 = Stub.Entity(e =>
		{
			e.Name = "Node 2";
			e.Width = 100;
			e.Height = 100;
			e.X = 150;
			e.Y = 70;
			e.ZIndex = 1;
		});

		var network = new NetworkBuilder()
			.WithEntities(entity1, entity2)
			.Build();

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => new NetworkUserControlBuilder().WithNetwork(network).Build());

		var node1 = page.GetByTitle("Node: Node 1");
		await node1.ClickAsync(new LocatorClickOptions { Position = new Position { X = 30, Y = 30 } });

		var deleteIcon = await page.QuerySelectorAsync(".deleteicon__deleteimage");
		Assert.That(deleteIcon, Is.Not.Null);

		var box = (await deleteIcon!.BoundingBoxAsync())!;

		var element = (await page.EvaluateHandleAsync(
		$"() => document.elementFromPoint({box.X + box.Width / 2}, {box.Y + box.Height / 2})"
		))
		.AsElement();
		Assert.That(element, Is.Not.Null);

		string? elementClass = await element!.GetAttributeAsync("class");
		string? deleteIconClass = await deleteIcon.GetAttributeAsync("class");

		Assert.That(elementClass, Is.EqualTo(deleteIconClass));
	}

	[Test]
	public void DoubleClickInvokesViewEntity()
	{
		var entity = Stub.Entity(e => e.ShapeType = ShapeType);
		var entities = new ImpObservableSet<INetworkEntity>(new[] { entity });
		var diagramEntity = Stub.Entity();

		var network = Mock.Of<INetwork>(
			x => x.Entities == entities &&
			x.DiagramEntity == diagramEntity &&
			x.EntityPositionStrategy == new EntityPositionStrategy() &&
			x.Refresher == Mock.Of<INetworkRefresher>());

		var networkUserControl = new NetworkUserControlForTestBuilder()
			.WithTestRenderer(Renderer)
			.WithNetwork(network)
			.Build();

		networkUserControl.AddScheduledNode(entity, (NodeViewModel nvm) =>
		{
			nvm.X = 0;
			nvm.Y = 0;
		});

		using var diagramModel = new NCNDiagramModelBuilder()
			.WithNetworkUserControl(networkUserControl)
			.Build();

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});
		var node = diagram.Find(NodeElement);
		node.DoubleClick();
		Mock.Get(network).Verify(x => x.ViewEntity(entity));
	}

	[Test]
	public void TestZIndex()
	{
		var (networkViewModel, entity, _) = CreateTestEntity(100, 200);

		var networkUserControl = new NetworkUserControlForTestBuilder()
			.WithNetwork(networkViewModel.Network)
			.WithTestRenderer(Renderer)
			.Build();

		networkUserControl.AddScheduledNode(entity, nvm => nvm.ZIndex = 0);

		var node = RenderNodeInDiagram(networkUserControl);

		Assert.That(node.Find(NodeElementInDiagram).GetStyle().GetZIndex(), Is.EqualTo("0"));

		var nvm = networkUserControl.ScheduledNodes().First();
		nvm.ZIndex = 1;

		node.Render();

		Assert.That(node.Find(NodeElementInDiagram).GetStyle().GetZIndex(), Is.EqualTo("1"));
	}

	[Test]
	public void DisposesSafelyWhenOutOfDiagram()
	{
		var (_, _, viewModel) = CreateTestEntity();
		viewModel.IsSelected = true;

		var node = RenderComponent<TNetworkNode>(parameters =>
		{
			parameters.Add(p => p.Node, CreateNodeModel(viewModel));
		});
		Assert.That(() => node.Instance.Dispose(), Throws.Nothing);
	}

	[Test]
	public void TestResizersAreNotRenderedIfOutsideOfDiagram()
	{
		var entity = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Width = 100;
			e.Height = 200;
		});

		var node = RenderComponent<TNetworkNode>(parameters =>
		{
			parameters.Add(p => p.Node, CreateNodeModel(entity));
		});
		var resizers = node.FindAll(ResizerElement);
		Assert.That(resizers.Count, Is.EqualTo(0));
	}

	[Test]
	public void ClickingOnDeleteIconDoesNotSelectNode()
	{
		var (_, _, nodeViewModel) = CreateTestEntity(100, 200);

		var node = RenderNodeInDiagram(nodeViewModel);

		var nodeModel = node.Instance.Node!;

		Assert.That(nodeModel.Selected, Is.False);

		var deleteIcon = node.Find(".deleteicon__deletecircle");
		Assert.Throws<MissingEventHandlerException>(() => deleteIcon.TriggerEvent("onpointerdown", new Microsoft.AspNetCore.Components.Web.PointerEventArgs()));

		Assert.That(nodeModel.Selected, Is.False);
	}

	[Test, WithTransaction, WithPlaywrightPage]
	public async Task TestSelectTextDoesNotDragNodeAsync()
	{
		var entity = CreateEntityWithSelectableText();
		var diagramEntity = new Entity() { BackColor = System.Drawing.Color.Red, SupportedActions = NetworkActions.StyleDiagram };

		var network = new NetworkBuilder()
			.WithDiagramEntity(diagramEntity)
			.WithEntities(entity)
			.Build();

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var networkUserControl = new NetworkUserControlBuilder()
				.WithNetwork(network)
				.Build();

			networkUserControl.AddScheduledNode(entity);

			return networkUserControl;
		});

		await page.SetViewportSizeAsync(1250, 700);
		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

		var node = await page.WaitForSelectorAsync(".networknode");
		var textInputs = await page.GetByRole(AriaRole.Textbox).AllAsync();

		foreach (var input in textInputs)
		{
			Assert.That(entity.X, Is.EqualTo(0));
			Assert.That(entity.Y, Is.EqualTo(0));

			await node!.ClickAsync();
			await input!.HoverAsync();
			await page.Mouse.DownAsync();
			await page.Mouse.MoveAsync(500, 500);
			await page.Mouse.UpAsync();
			await Task.Delay(50);

			Assert.That(entity.X, Is.EqualTo(0), $"select text on element with class \"{await input.GetAttributeAsync("class")}\" should not move node");
			Assert.That(entity.Y, Is.EqualTo(0), $"select text on element with class \"{await input.GetAttributeAsync("class")}\" should not move node");
		}
	}

	[Test, WithTransaction, WithPlaywrightPage]
	public async Task TestDeleteCharacterDoesNotDeleteNodeAsync()
	{
		var entity = CreateEntityWithSelectableText();
		var diagramEntity = new Entity() { BackColor = System.Drawing.Color.Red, SupportedActions = NetworkActions.StyleDiagram };

		var network = new NetworkBuilder()
			.WithDiagramEntity(diagramEntity)
			.WithEntities(entity)
			.Build();

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var networkUserControl = new NetworkUserControlBuilder()
				.WithNetwork(network)
				.Build();

			networkUserControl.AddScheduledNode(entity);

			return networkUserControl;
		});

		await page.SetViewportSizeAsync(1250, 700);
		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

		var node = await page.WaitForSelectorAsync(".networknode");

		var textInputs = await page.GetByRole(AriaRole.Textbox).AllAsync();

		foreach (var input in textInputs)
		{
			await node!.ClickAsync();
			await input!.ClickAsync();
			await input.FillAsync("test text");

			Assert.That(network.Entities.Count, Is.EqualTo(1));
			Assert.That(await input.InputValueAsync(), Is.EqualTo("test text"));

			await input.PressAsync("ArrowLeft");
			await input.PressAsync("Delete");
			Assert.That(network.Entities.Count, Is.EqualTo(1), $"press delete on element with class \"{await input.GetAttributeAsync("class")}\" should not delete node");
			Assert.That(await input.InputValueAsync(), Is.EqualTo("test tex"));
		}
	}

	static IEnumerable<TestCaseData> SizeAndPosition_TestData
	{
		get
		{
			yield return new TestCaseData("topleft", new Point(10, 15), new Size(190, 185));
			yield return new TestCaseData("topright", new Point(0, 15), new Size(210, 185));
			yield return new TestCaseData("bottomleft", new Point(10, 0), new Size(190, 215));
			yield return new TestCaseData("bottomright", new Point(0, 0), new Size(210, 215));
		}
	}

	[TestCaseSource(nameof(SizeAndPosition_TestData))]
	public async Task RecalculateSizeAndPositionOnResizeAsync(string resizerClass, Point expectedPosition, Size expectedSize)
	{
		var (_, _, viewModel) = CreateTestEntity(200, 200);

		var node = RenderNodeInDiagram(viewModel, CreateNodeResizeControlFactory());

		var nodeModel = node.Instance.Node!;
		var diagramModel = node.Instance.Diagram!;

		diagramModel.SelectModel(nodeModel, false);
		var resizerControl = (NodeResizeControl)diagramModel.Controls.GetFor(nodeModel)!.First(c => c is NodeResizeControl rc && rc.Class!.Contains(resizerClass))!;

		Assert.That(nodeModel.Position.X, Is.EqualTo(0));
		Assert.That(nodeModel.Position.Y, Is.EqualTo(0));
		Assert.That(nodeModel.Size!.Width, Is.EqualTo(200));
		Assert.That(nodeModel.Size.Height, Is.EqualTo(200));

		var eventArgs = new PointerEventArgs(0, 0, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
		await resizerControl!.OnPointerDown(diagramModel, nodeModel, eventArgs);
		eventArgs = new PointerEventArgs(10, 15, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
		diagramModel.TriggerPointerMove(null, eventArgs);

		Assert.That(nodeModel.Position, Is.EqualTo(expectedPosition));
		Assert.That(nodeModel.Size, Is.EqualTo(expectedSize));

		diagramModel.TriggerPointerUp(null, eventArgs);
		eventArgs = new PointerEventArgs(0, 0, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
		await resizerControl!.OnPointerDown(diagramModel, nodeModel, eventArgs);
		eventArgs = new PointerEventArgs(-10, -15, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
		diagramModel.TriggerPointerMove(null, eventArgs);

		Assert.That(nodeModel.Position.X, Is.EqualTo(0));
		Assert.That(nodeModel.Position.Y, Is.EqualTo(0));
		Assert.That(nodeModel.Size!.Width, Is.EqualTo(200));
		Assert.That(nodeModel.Size.Height, Is.EqualTo(200));
	}

	static IEnumerable<TestCaseData> RecalculateSizeAndPositionOnResize_ScaledDiagram_TestData
	{
		get
		{
			yield return new TestCaseData("topleft", 80, 40, new Point(400, 220), new Size(100, 160));
			yield return new TestCaseData("topleft", 51, 40, new Point(400, 220), new Size(100, 160));
			yield return new TestCaseData("topleft", 49, 40, new Point(300, 220), new Size(200, 160));
			yield return new TestCaseData("topleft", -51, -40, new Point(200, 140), new Size(300, 240));
			yield return new TestCaseData("topleft", -49, -40, new Point(300, 140), new Size(200, 240));
			yield return new TestCaseData("topleft", -60, -40, new Point(200, 140), new Size(300, 240));
			yield return new TestCaseData("topright", 51, -40, new Point(300, 140), new Size(300, 240));
			yield return new TestCaseData("topright", 49, -40, new Point(300, 140), new Size(200, 240));
			yield return new TestCaseData("topright", 60, -40, new Point(300, 140), new Size(300, 240));
			yield return new TestCaseData("topright", -51, 40, new Point(300, 220), new Size(100, 160));
			yield return new TestCaseData("topright", -49, 40, new Point(300, 220), new Size(200, 160));
			yield return new TestCaseData("topright", -60, 40, new Point(300, 220), new Size(100, 160));
			yield return new TestCaseData("bottomleft", -51, 40, new Point(200, 180), new Size(300, 240));
			yield return new TestCaseData("bottomleft", -49, 40, new Point(300, 180), new Size(200, 240));
			yield return new TestCaseData("bottomleft", -71, 40, new Point(200, 180), new Size(300, 240));
			yield return new TestCaseData("bottomleft", 51, -40, new Point(400, 180), new Size(100, 160));
			yield return new TestCaseData("bottomleft", 49, -40, new Point(300, 180), new Size(200, 160));
			yield return new TestCaseData("bottomleft", 80, -40, new Point(400, 180), new Size(100, 160));
			yield return new TestCaseData("bottomright", 51, 40, new Point(300, 180), new Size(300, 240));
			yield return new TestCaseData("bottomright", 49, 40, new Point(300, 180), new Size(200, 240));
			yield return new TestCaseData("bottomright", 71, 40, new Point(300, 180), new Size(300, 240));
			yield return new TestCaseData("bottomright", -51, -40, new Point(300, 180), new Size(100, 160));
			yield return new TestCaseData("bottomright", -49, -40, new Point(300, 180), new Size(200, 160));
			yield return new TestCaseData("bottomright", -80, -40, new Point(300, 180), new Size(100, 160));
		}
	}

	[TestCaseSource(nameof(RecalculateSizeAndPositionOnResize_ScaledDiagram_TestData))]
	public async Task RecalculateSizeAndPositionOnResize_ScaledDiagramAsync(string resizerClass, double deltaX, double deltaY, Point expectedPosition, Size expectedSize)
	{
		var diagramEntity = Stub.Entity(e =>
		{
			e.IsDiagramScaled = true;
			e.ShouldShowNonScheduledSection = true;
			e.SupportedActions = NetworkActions.StyleDiagram;
		});

		var entity = Stub.Entity(e =>
		{
			e.Width = 200;
			e.Height = 200;
			e.X = 300;
			e.Y = 180;
		});

		var network = new NetworkBuilder()
			.WithDiagramEntity(diagramEntity)
			.WithEntities(entity)
			.Build();

		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new NetworkUserControlBuilder().WithNetwork(network).Build());

		var jobNode = rendered.FindComponent<InDiagramNetworkNode<JobNodeModel, JobNode>>();

		var jobNodeModel = jobNode.Instance.Node!;
		var diagramModel = jobNode.Instance.Diagram!;

		await ctx.Renderer.Dispatcher.InvokeAsync(() => diagramModel.SelectModel(jobNodeModel, false));
		rendered.WaitForState(() => diagramModel.Controls.GetFor(jobNodeModel)!.Where(c => c is NodeResizeControl).Count() == 4, TimeSpan.FromSeconds(3));
		var resizerControl = (NodeResizeControl)diagramModel.Controls.GetFor(jobNodeModel)!.First(c => c is NodeResizeControl rc && rc.Class!.Contains(resizerClass))!;

		Assert.That(jobNodeModel.Position.X, Is.EqualTo(300));
		Assert.That(jobNodeModel.Position.Y, Is.EqualTo(180));
		Assert.That(jobNodeModel.Size!.Width, Is.EqualTo(200));
		Assert.That(jobNodeModel.Size!.Height, Is.EqualTo(200));

		await ctx.Renderer.Dispatcher.InvokeAsync(async () =>
		{
			var eventArgs = new PointerEventArgs(0, 0, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
			await resizerControl!.OnPointerDown(diagramModel, jobNodeModel, eventArgs);
			eventArgs = new PointerEventArgs(deltaX, deltaY, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
			diagramModel!.TriggerPointerMove(null, eventArgs);
			diagramModel!.TriggerPointerUp(null, eventArgs);
		});

		Assert.That(() => jobNodeModel.Position, Is.EqualTo(expectedPosition).After(3000, 100));
		Assert.That(() => jobNodeModel.Size, Is.EqualTo(expectedSize).After(3000, 100));
	}

	static IEnumerable<TestCaseData> ShouldStopResizingSmallerThanMinimumSize_TestData
	{
		get
		{
			yield return new TestCaseData("topleft", 300, 300, new Point(100, 150));
			yield return new TestCaseData("topright", -300, 300, new Point(0, 150));
			yield return new TestCaseData("bottomleft", 300, -300, new Point(100, 0));
			yield return new TestCaseData("bottomright", -300, -300, new Point(0, 0));
		}
	}

	[TestCaseSource(nameof(ShouldStopResizingSmallerThanMinimumSize_TestData))]
	public async Task ShouldStopResizingSmallerThanMinimumSizeAsync(string resizerClass, double deltaX, double deltaY, Point expectedPositionAfterResize)
	{
		var (_, _, viewModel) = CreateTestEntity(200, 200);

		var node = RenderNodeInDiagram(viewModel, CreateNodeResizeControlFactory());

		var nodeModel = node.Instance.Node!;
		var diagramModel = node.Instance.Diagram!;

		diagramModel.SelectModel(nodeModel, unselectOthers: false);
		var resizerControl = (NodeResizeControl)diagramModel.Controls.GetFor(nodeModel)!.First(c => c is NodeResizeControl rc && rc.Class!.Contains(resizerClass))!;

		Assert.Multiple(() =>
		{
			Assert.That(nodeModel.Position.X, Is.EqualTo(0));
			Assert.That(nodeModel.Position.Y, Is.EqualTo(0));
			Assert.That(nodeModel.Size!.Width, Is.EqualTo(200));
			Assert.That(nodeModel.Size.Height, Is.EqualTo(200));
		});

		var eventArgs = new PointerEventArgs(0, 0, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
		await resizerControl!.OnPointerDown(diagramModel, nodeModel, eventArgs);
		eventArgs = new PointerEventArgs(deltaX, deltaY, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
		diagramModel.TriggerPointerMove(null, eventArgs);

		Assert.Multiple(() =>
		{
			Assert.That(nodeModel.Position, Is.EqualTo(expectedPositionAfterResize));
			Assert.That(nodeModel.Size!.Width, Is.EqualTo(nodeModel.MinimumDimensions.Width));
			Assert.That(nodeModel.Size.Height, Is.EqualTo(nodeModel.MinimumDimensions.Height));
		});
	}

	[TestCaseSource(nameof(SizeAndPosition_TestData))]
	public async Task ShouldStopResizeOnPointerUpAsync(string resizerClass, Point expectedPosition, Size expectedSize)
	{
		var (_, _, viewModel) = CreateTestEntity(200, 200);

		var node = RenderNodeInDiagram(viewModel, CreateNodeResizeControlFactory());

		var nodeModel = node.Instance.Node!;
		var diagramModel = node.Instance.Diagram!;

		diagramModel.SelectModel(nodeModel, false);
		var resizerControl = (NodeResizeControl)diagramModel.Controls.GetFor(nodeModel)!.First(c => c is NodeResizeControl rc && rc.Class!.Contains(resizerClass))!;

		var eventArgs = new PointerEventArgs(0, 0, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
		await resizerControl!.OnPointerDown(diagramModel, nodeModel, eventArgs);
		eventArgs = new PointerEventArgs(10, 15, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
		diagramModel.TriggerPointerMove(null, eventArgs);

		Assert.That(nodeModel.Position, Is.EqualTo(expectedPosition));
		Assert.That(nodeModel.Size, Is.EqualTo(expectedSize));

		diagramModel.TriggerPointerUp(null, eventArgs);

		eventArgs = new PointerEventArgs(30, 50, 1, 1, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
		diagramModel.TriggerPointerMove(null, eventArgs);

		Assert.That(nodeModel.Position, Is.EqualTo(expectedPosition));
		Assert.That(nodeModel.Size, Is.EqualTo(expectedSize));
	}

	[TestCase("topleft")]
	[TestCase("topright")]
	[TestCase("bottomleft")]
	[TestCase("bottomright")]
	public async Task ShouldSubscribeToPointerEventsOutsideDiagramOnResizeAsync(string resizerClass)
	{
		var entityDiagram = Stub.Entity(e =>
		{
			e.IsDiagramScaled = true;
			e.ShouldShowNonScheduledSection = true;
			e.SupportedActions = NetworkActions.StyleDiagram;
		});

		var entity = Stub.Entity(e =>
		{
			e.Width = 200;
			e.Height = 200;
			e.X = 300;
			e.Y = 180;
		});

		var network = new NetworkBuilder()
			.WithDiagramEntity(entityDiagram)
			.WithEntities(entity)
			.Build();

		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new NetworkUserControlBuilder().WithNetwork(network).Build());

		var jobNode = rendered.FindComponent<InDiagramNetworkNode<JobNodeModel, JobNode>>();

		var jobNodeModel = jobNode.Instance.Node!;
		var diagramModel = jobNode.Instance.Diagram!;

		await ctx.Renderer.Dispatcher.InvokeAsync(() => diagramModel.SelectModel(jobNodeModel, false));
		rendered.WaitForState(() => diagramModel.Controls.GetFor(jobNodeModel)!.Where(c => c is NodeResizeControl).Count() == 4, TimeSpan.FromSeconds(3));
		var resizerControl = (NodeResizeControl)diagramModel.Controls.GetFor(jobNodeModel)!.First(c => c is NodeResizeControl rc && rc.Class!.Contains(resizerClass))!;

		Assert.That(jobNodeModel.Position.X, Is.EqualTo(300));
		Assert.That(jobNodeModel.Position.Y, Is.EqualTo(180));
		Assert.That(jobNodeModel.Size!.Width, Is.EqualTo(200));
		Assert.That(jobNodeModel.Size!.Height, Is.EqualTo(200));

		await ctx.Renderer.Dispatcher.InvokeAsync(async () =>
		{
			var eventArgs = new PointerEventArgs(0, 0, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
			await resizerControl!.OnPointerDown(diagramModel, jobNodeModel, eventArgs);
		});

		Assert.That(ctx.JSInterop.Invocations.Select(i => i.Identifier), Has.One.EqualTo("subscribeToPointerEventsOutsideElement"));
	}

	[Test]
	public void TestSendToBackMovesNodeToBack()
	{
		var diagramEntity = Stub.Entity();
		var entity1 = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Name = "Node 1";
			e.Width = 100;
			e.Height = 100;
			e.X = 100;
			e.Y = 100;
			e.ZIndex = 10;
			e.Parent = diagramEntity;
		});

		var entity2 = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Name = "Node 2";
			e.Width = 100;
			e.Height = 100;
			e.X = 150;
			e.Y = 70;
			e.ZIndex = 20;
			e.Parent = diagramEntity;
		});

		var network = new NetworkBuilder()
			.WithDiagramEntity(diagramEntity)
			.WithEntities(entity1, entity2)
			.Build();

		var networkViewModel = new NetworkViewModelBuilder()
			.WithNetwork(network)
			.Build();

		var viewModel1 = networkViewModel.CreateNodeViewModel(entity1) as TNodeViewModel;
		var viewModel2 = networkViewModel.CreateNodeViewModel(entity2) as TNodeViewModel;

		Assert.That(viewModel1, Is.Not.Null);
		Assert.That(viewModel2, Is.Not.Null);

		var node1 = RenderNodeInDiagram(viewModel1);
		var node2 = RenderNodeInDiagram(viewModel2);

		Assert.That(node1.Find(NodeElementInDiagram).GetStyle().GetZIndex(), Is.EqualTo("10"));
		Assert.That(node2.Find(NodeElementInDiagram).GetStyle().GetZIndex(), Is.EqualTo("20"));

		viewModel2.SendToBack();

		node1.Render();
		node2.Render();

		Assert.That(node2.Find(NodeElementInDiagram).GetStyle().GetZIndex(), Is.EqualTo("1"));
	}

	[Test]
	public void TestBringToFrontMovesNodeToFront()
	{
		var entity1 = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Name = "Node 1";
			e.Width = 100;
			e.Height = 100;
			e.X = 100;
			e.Y = 100;
			e.ZIndex = 0;
		});

		var entity2 = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Name = "Node 2";
			e.Width = 100;
			e.Height = 100;
			e.X = 150;
			e.Y = 70;
			e.ZIndex = 1;
		});

		var network = new NetworkBuilder()
			.WithEntities(entity1, entity2)
			.Build();

		var networkViewModel = new NetworkViewModelBuilder()
			.WithNetwork(network)
			.Build();

		var nodeViewModel1 = networkViewModel.CreateNodeViewModel(entity1) as TNodeViewModel;
		var nodeViewModel2 = networkViewModel.CreateNodeViewModel(entity2) as TNodeViewModel;

		Assert.That(nodeViewModel1, Is.Not.Null);
		Assert.That(nodeViewModel2, Is.Not.Null);

		var node1 = RenderNodeInDiagram(nodeViewModel1);
		var node2 = RenderNodeInDiagram(nodeViewModel2);

		Assert.That(node1.Find(NodeElementInDiagram).GetStyle().GetZIndex(), Is.EqualTo("0"));
		Assert.That(node2.Find(NodeElementInDiagram).GetStyle().GetZIndex(), Is.EqualTo("1"));

		nodeViewModel1.BringToFront();
		node1.Render();
		node2.Render();

		Assert.That(node1.Find(NodeElementInDiagram).GetStyle().GetZIndex(), Is.EqualTo("2"));
		Assert.That(node2.Find(NodeElementInDiagram).GetStyle().GetZIndex(), Is.EqualTo("1"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestDeleteIconDisplaysAfterSendToBackAsync()
	{
		var entityDiagram = Stub.Entity(e =>
		{
			e.IsDiagramScaled = false;
			e.ShouldShowNonScheduledSection = true;
			e.SupportedActions = NetworkActions.StyleDiagram;
		});

		var entity1 = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Name = "Node 1";
			e.Width = 100;
			e.Height = 100;
			e.X = 100;
			e.Y = 100;
			e.ZIndex = 2;
		});

		var entity2 = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Name = "Node 2";
			e.Width = 100;
			e.Height = 100;
			e.X = 150;
			e.Y = 70;
			e.ZIndex = 1;
		});

		var network = new NetworkBuilder()
			.WithDiagramEntity(entityDiagram)
			.WithEntities(entity1, entity2)
			.Build();

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => new NetworkUserControlBuilder().WithNetwork(network).Build());

		var inDiagramNode = page.Locator(NodeElementInDiagram).First;
		await inDiagramNode.EvaluateAsync("(el) => el.style.zIndex = 0");

		var node1 = page.GetByTitle("Node: Node 1");

		await node1.ClickAsync(new LocatorClickOptions { Position = new Position { X = 30, Y = 30 } });

		var deleteIcon = await page.QuerySelectorAsync(".deleteicon__deleteimage");
		Assert.That(deleteIcon, Is.Not.Null);

		var box = (await deleteIcon.BoundingBoxAsync())!;

		var element = (await page.EvaluateHandleAsync(
		$"() => document.elementFromPoint({box.X + box.Width / 2}, {box.Y + box.Height / 2})"
		))
		.AsElement();
		Assert.That(element, Is.Not.Null);

		string? elementClass = await element.GetAttributeAsync("class");
		string? deleteIconClass = await deleteIcon.GetAttributeAsync("class");

		Assert.That(elementClass, Is.EqualTo(deleteIconClass));
	}

	[Test, WithPlaywrightPage]
	public async Task TestNodesAreStackedWithCorrectZIndexAsync()
	{
		var entityDiagram = Stub.Entity(e =>
		{
			e.IsDiagramScaled = false;
			e.ShouldShowNonScheduledSection = true;
			e.SupportedActions = NetworkActions.StyleDiagram;
		});

		var entity1 = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Name = "Node 1";
			e.Width = 100;
			e.Height = 100;
			e.X = 100;
			e.Y = 100;
			e.ZIndex = 2;
		});

		var entity2 = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Name = "Node 2";
			e.Width = 100;
			e.Height = 100;
			e.X = 130;
			e.Y = 100;
			e.ZIndex = 1;
		});

		var network = new NetworkBuilder()
			.WithDiagramEntity(entityDiagram)
			.WithEntities(entity1, entity2)
			.Build();

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => new NetworkUserControlBuilder().WithNetwork(network).Build());

		var node1 = page.GetByTitle("Node: Node 1");

		var box = (await node1.BoundingBoxAsync())!;

		var element = (await page.EvaluateHandleAsync(
		$"() => document.elementFromPoint({box.X + 50}, {box.Y})"
		))
		.AsElement();
		Assert.That(element, Is.Not.Null);

		var actualTitle = await element.GetAttributeAsync("title");

		Assert.That(actualTitle, Is.EqualTo("Node: Node 1"));
	}

	[Test]
	public void ShouldNotDragIfShapeIsPinned()
	{
		var entity = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Width = 100;
			e.Height = 100;
			e.EntityState = EntityState.Fixed;
		});

		var node = RenderNodeInDiagram(entity);

		var nodeModel = node.Instance.Node!;
		var diagramModel = node.Instance.Diagram!;

		Assert.That(nodeModel.Position.X, Is.EqualTo(0));
		Assert.That(nodeModel.Position.Y, Is.EqualTo(0));

		var eventArgs = new PointerEventArgs(0, 0, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
		diagramModel.TriggerPointerDown(nodeModel, eventArgs);
		eventArgs = new PointerEventArgs(100, 150, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
		diagramModel.TriggerPointerMove(null, eventArgs);

		Assert.That(nodeModel.Position.X, Is.EqualTo(0));
		Assert.That(nodeModel.Position.Y, Is.EqualTo(0));
	}

	[Test, WithPlaywrightPage]
	public async Task WhenDragOverTheDiagramWithMiddleMouseButtonPressedAsync()
	{
		var entityDiagram = Stub.Entity(e =>
		{
			e.IsDiagramScaled = false;
			e.ShouldShowNonScheduledSection = true;
			e.SupportedActions = NetworkActions.StyleDiagram;
		});

		var entity = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.X = 10;
			e.Y = 10;
			e.Width = 100;
			e.Height = 100;
			e.EntityState = EntityState.Fixed;
		});

		var network = new NetworkBuilder()
			.WithDiagramEntity(entityDiagram)
			.WithEntities(entity)
			.Build();

		NetworkUserControl networkUserControl = null!;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			return networkUserControl;
		});

		var diagramModel = networkUserControl.MainDiagramControl.DiagramModel;
		var nodeModel = networkUserControl.NetworkViewModel.Nodes.First();

		Assert.That(nodeModel.IsSelected, Is.False);

		var pointerArgs = Pointer.MiddleButton;
		diagramModel!.TriggerPointerDown(null, pointerArgs.At(x: 5, y: 5));
		diagramModel!.TriggerPointerMove(null, pointerArgs.At(x: 200, y: 200));
		diagramModel!.TriggerPointerUp(null, pointerArgs.At(x: 200, y: 200));

		Assert.That(nodeModel.IsSelected, Is.False);
	}

	[Test]
	public void ResizersHaveSameZIndexAsNode()
	{
		var (_, _, viewModel) = CreateTestEntity(200, 200);

		var diagram = CreateDiagramWithNode(viewModel, CreateNodeResizeControlFactory());

		var networkNode = diagram.FindComponent<InDiagramNetworkNode<TNetworkNodeModel, TNetworkNode>>();

		var networkNodeModel = networkNode.Instance.Node!;
		var diagramModel = networkNode.Instance.Diagram!;

		diagramModel.SelectModel(networkNodeModel, false);
		var resizerControls = diagram.FindAll(ResizerElement);

		Assert.That(resizerControls[0].GetAttribute("style"), Does.Contain($"z-index: {networkNodeModel.ZIndex}"));
		Assert.That(resizerControls[1].GetAttribute("style"), Does.Contain($"z-index: {networkNodeModel.ZIndex}"));
		Assert.That(resizerControls[2].GetAttribute("style"), Does.Contain($"z-index: {networkNodeModel.ZIndex}"));
		Assert.That(resizerControls[3].GetAttribute("style"), Does.Contain($"z-index: {networkNodeModel.ZIndex}"));
	}

	[Test]
	public void ResizersAreRendered()
	{
		var (_, _, viewModel) = CreateTestEntity(200, 200);

		var diagram = CreateDiagramWithNode(viewModel, CreateNodeResizeControlFactory());

		var networkNode = diagram.FindComponent<InDiagramNetworkNode<TNetworkNodeModel, TNetworkNode>>();

		var networkNodeModel = networkNode.Instance.Node!;
		var diagramModel = networkNode.Instance.Diagram!;

		var resizerControls = diagram.FindAll(ResizerElement);

		Assert.That(resizerControls.Count, Is.EqualTo(1));
		Assert.That(resizerControls[0].ClassList, Does.Contain("bottomright"));

		diagramModel.SelectModel(networkNodeModel, false);
		resizerControls = diagram.FindAll(ResizerElement);

		Assert.That(resizerControls.Count, Is.EqualTo(4));
		Assert.That(resizerControls.First(c => c.ClassList.Contains("topleft")), Is.Not.Null);
		Assert.That(resizerControls.First(c => c.ClassList.Contains("topright")), Is.Not.Null);
		Assert.That(resizerControls.First(c => c.ClassList.Contains("bottomleft")), Is.Not.Null);
		Assert.That(resizerControls.First(c => c.ClassList.Contains("bottomright")), Is.Not.Null);

		diagramModel.UnselectModel(networkNodeModel);
		resizerControls = diagram.FindAll(ResizerElement);

		Assert.That(resizerControls.Count, Is.EqualTo(1));
		Assert.That(resizerControls[0].ClassList, Does.Contain("bottomright"));
	}

	[Test]
	public void ResizersRemovedAfterShapeIsPinned()
	{
		var (_, entity, viewModel) = CreateTestEntity(200, 200);

		var diagram = CreateDiagramWithNode(viewModel, CreateNodeResizeControlFactory());

		var networkNode = diagram.FindComponent<InDiagramNetworkNode<TNetworkNodeModel, TNetworkNode>>();

		var networkNodeModel = networkNode.Instance.Node!;

		var resizerControls = diagram.FindAll(ResizerElement);

		Assert.That(resizerControls.Count, Is.EqualTo(1));

		entity.EntityState = EntityState.Fixed;
		entity.TriggerPropertyChanged(nameof(NodeViewModel.Entity.EntityState));

		resizerControls = diagram.FindAll(ResizerElement);
		Assert.That(resizerControls.Count, Is.EqualTo(0));
	}

	[Test]
	public void ResizersRemovedAfterSelectedShapeIsPinned()
	{
		var (_, entity, viewModel) = CreateTestEntity(200, 200);

		var diagram = CreateDiagramWithNode(viewModel, CreateNodeResizeControlFactory());

		var networkNode = diagram.FindComponent<InDiagramNetworkNode<TNetworkNodeModel, TNetworkNode>>();

		var networkNodeModel = networkNode.Instance.Node!;
		var diagramModel = networkNode.Instance.Diagram!;
		diagramModel.SelectModel(networkNodeModel, false);

		var resizerControls = diagram.FindAll(ResizerElement);

		Assert.That(resizerControls.Count, Is.EqualTo(4));

		entity.EntityState = EntityState.Fixed;
		entity.TriggerPropertyChanged(nameof(NodeViewModel.Entity.EntityState));

		resizerControls = diagram.FindAll(ResizerElement);
		Assert.That(resizerControls.Count, Is.EqualTo(0));
	}

	[Test]
	public void ResizersAddedAfterShapeIsUnpinned()
	{
		var entity = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Width = 100;
			e.Height = 100;
			e.EntityState = EntityState.Fixed;
		});

		var networkViewModel = new NetworkViewModelBuilder().Build();
		var nodeViewModel = networkViewModel.CreateNodeViewModel(entity) as TNodeViewModel;

		Assert.That(nodeViewModel, Is.Not.Null);

		var diagram = CreateDiagramWithNode(nodeViewModel, CreateNodeResizeControlFactory());

		var networkNode = diagram.FindComponent<InDiagramNetworkNode<TNetworkNodeModel, TNetworkNode>>();

		var networkNodeModel = networkNode.Instance.Node!;

		var resizerControls = diagram.FindAll(ResizerElement);
		Assert.That(resizerControls.Count, Is.EqualTo(0));

		entity.EntityState = EntityState.None;
		entity.TriggerPropertyChanged(nameof(NodeViewModel.Entity.EntityState));

		resizerControls = diagram.FindAll(ResizerElement);

		Assert.That(resizerControls.Count, Is.EqualTo(1));
		Assert.That(resizerControls[0].ClassList, Does.Contain("bottomright"));
	}

	[Test]
	public void ResizersAddedAfterSelectedShapeIsUnpinned()
	{
		var entity = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Width = 100;
			e.Height = 100;
			e.EntityState = EntityState.Fixed;
		});

		var networkViewModel = new NetworkViewModelBuilder().Build();
		var nodeViewModel = networkViewModel.CreateNodeViewModel(entity) as TNodeViewModel;

		Assert.That(nodeViewModel, Is.Not.Null);

		var diagram = CreateDiagramWithNode(nodeViewModel, CreateNodeResizeControlFactory());

		var networkNode = diagram.FindComponent<InDiagramNetworkNode<TNetworkNodeModel, TNetworkNode>>();

		var networkNodeModel = networkNode.Instance.Node!;
		var diagramModel = networkNode.Instance.Diagram!;
		diagramModel.SelectModel(networkNodeModel, false);

		var resizerControls = diagram.FindAll(ResizerElement);
		Assert.That(resizerControls.Count, Is.EqualTo(0));

		entity.EntityState = EntityState.None;
		entity.TriggerPropertyChanged(nameof(NodeViewModel.Entity.EntityState));
		resizerControls = diagram.FindAll(ResizerElement);

		Assert.That(resizerControls.Count, Is.EqualTo(4));
		Assert.That(resizerControls.First(c => c.ClassList.Contains("topleft")), Is.Not.Null);
		Assert.That(resizerControls.First(c => c.ClassList.Contains("topright")), Is.Not.Null);
		Assert.That(resizerControls.First(c => c.ClassList.Contains("bottomleft")), Is.Not.Null);
		Assert.That(resizerControls.First(c => c.ClassList.Contains("bottomright")), Is.Not.Null);
	}

	[Test]
	public void TestResizersVisibleWhenNodeIsSelected()
	{
		var entity = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Width = 100;
			e.Height = 100;
		});

		var networkViewModel = new NetworkViewModelBuilder().Build();
		var nodeViewModel = networkViewModel.CreateNodeViewModel(entity) as TNodeViewModel;

		Assert.That(nodeViewModel, Is.Not.Null);

		var diagram = CreateDiagramWithNode(nodeViewModel, CreateNodeResizeControlFactory());

		var networkNode = diagram.FindComponent<InDiagramNetworkNode<TNetworkNodeModel, TNetworkNode>>();

		var networkNodeModel = networkNode.Instance.Node!;
		var diagramModel = networkNode.Instance.Diagram!;

		var resizerControls = diagram.FindAll(ResizerElement);
		Assert.That(resizerControls.Count, Is.EqualTo(1));

		Assert.That(resizerControls.First(c => c.ClassList.Contains("bottomright")), Is.Not.Null);

		diagramModel.SelectModel(networkNodeModel, false);

		resizerControls = diagram.FindAll(ResizerElement);

		Assert.That(resizerControls.Count, Is.EqualTo(4));
		Assert.That(resizerControls.First(c => c.ClassList.Contains("topleft")), Is.Not.Null);
		Assert.That(resizerControls.First(c => c.ClassList.Contains("topright")), Is.Not.Null);
		Assert.That(resizerControls.First(c => c.ClassList.Contains("bottomleft")), Is.Not.Null);
		Assert.That(resizerControls.First(c => c.ClassList.Contains("bottomright")), Is.Not.Null);

		diagramModel.UnselectModel(networkNodeModel);

		resizerControls = diagram.FindAll(ResizerElement);
		Assert.That(resizerControls.Count, Is.EqualTo(1));

		Assert.That(resizerControls.First(c => c.ClassList.Contains("bottomright")), Is.Not.Null);
	}

	[Test]
	public void ResizersNotRenderedIfShapeIsPinned()
	{
		var entity = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Width = 100;
			e.Height = 100;
			e.EntityState = EntityState.Fixed;
		});
		var networkViewModel = new NetworkViewModelBuilder().Build();
		var nodeViewModel = networkViewModel.CreateNodeViewModel(entity) as TNodeViewModel;

		Assert.That(nodeViewModel, Is.Not.Null);

		var diagram = CreateDiagramWithNode(nodeViewModel, CreateNodeResizeControlFactory());

		var networkNode = diagram.FindComponent<InDiagramNetworkNode<TNetworkNodeModel, TNetworkNode>>();

		Assert.Throws<ElementNotFoundException>(() => diagram.Find(ResizerElement));

		var networkNodeModel = networkNode.Instance.Node!;
		var diagramModel = networkNode.Instance.Diagram!;

		diagramModel.SelectModel(networkNodeModel, false);
		Assert.Throws<ElementNotFoundException>(() => diagram.Find(ResizerElement));

		diagramModel.UnselectModel(networkNodeModel);
		Assert.Throws<ElementNotFoundException>(() => diagram.Find(ResizerElement));
	}

	protected IRenderedComponent<TNetworkNode> RenderNode(TNetworkNodeModel networkNodeModel)
		=> RenderComponent<TNetworkNode>(parameters => parameters.Add(networkNode => networkNode.Node, networkNodeModel));

	protected IRenderedComponent<TNetworkNode> RenderNode(NodeViewModel nodeViewModel)
	{
		var diagram = CreateDiagramWithNode(nodeViewModel);

		return diagram.FindComponent<TNetworkNode>();
	}

	protected IRenderedComponent<InDiagramNetworkNode<TNetworkNodeModel, TNetworkNode>> RenderNodeInDiagram(NodeViewModel nodeViewModel, NodeResizeControlFactory? nodeResizeControlFactory = null)
	{
		var diagram = CreateDiagramWithNode(nodeViewModel, nodeResizeControlFactory);

		return diagram.FindComponent<InDiagramNetworkNode<TNetworkNodeModel, TNetworkNode>>();
	}

	protected IRenderedComponent<NCNDiagramForTest> CreateDiagramWithNode(NodeViewModel nodeViewModel, NodeResizeControlFactory? nodeResizeControlFactory = null)
	{
		var networkUserControl = new NetworkUserControlForTestBuilder()
			.WithTestRenderer(Renderer)
			.Build();

		networkUserControl.AddScheduledNode(nodeViewModel);

		using var diagramModel = new NCNDiagramModelBuilder()
			.WithNodeResizeControlFactory(nodeResizeControlFactory)
			.WithNetworkUserControl(networkUserControl)
			.Build();

		return RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});
	}

	protected NodeResizeControlFactory CreateNodeResizeControlFactory(IGlobalEventProvider? globalEventProvider = null)
	{
		var globalEventProviderMock = new Mock<IGlobalEventProvider>();

		var nodeResizeControlFactory = new NodeResizeControlFactory(globalEventProvider ?? globalEventProviderMock.Object);
		return nodeResizeControlFactory;
	}

	protected IRenderedComponent<InDiagramNetworkNode<TNetworkNodeModel, TNetworkNode>> RenderNodeInDiagram(INetworkUserControl networkUserControl)
	{
		var diagram = RenderDiagram(networkUserControl);

		return diagram.FindComponent<InDiagramNetworkNode<TNetworkNodeModel, TNetworkNode>>();
	}

	protected IRenderedComponent<InDiagramNetworkNode<TNetworkNodeModel, TNetworkNode>> RenderNodeInDiagram(params INetworkEntity[] entities)
	{
		var diagram = RenderDiagram(entities);

		return diagram.FindComponent<InDiagramNetworkNode<TNetworkNodeModel, TNetworkNode>>();
	}

	protected IRenderedComponent<NCNDiagramForTest> RenderDiagram(params INetworkEntity[] entities)
	{
		var network = new NetworkBuilder().WithEntities(entities).Build();
		var networkUserControl = new NetworkUserControlForTestBuilder().WithNetwork(network).Build();
		networkUserControl.AddScheduledNodes(entities);

		return RenderDiagram(networkUserControl);
	}

	protected IRenderedComponent<NCNDiagramForTest> RenderDiagram(INetworkUserControl networkUserControl)
	{
		using var diagramModel = new NCNDiagramModelBuilder()
			.WithNetworkUserControl(networkUserControl)
			.Build();

		return RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});
	}
}
