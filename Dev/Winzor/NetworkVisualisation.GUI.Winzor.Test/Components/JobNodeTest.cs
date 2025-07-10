using System.Drawing;
using AngleSharp.Css.Dom;
using AngleSharp.Dom;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Components;
using CargoWise.NetworkVisualisation.GUI.Controls.NodeResizing;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.GUI.Services.NetworkNode;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWiseOne.ResourceStrings;
using Enterprise.Winzor.Architecture.Test;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Extentions;
using WTG.PlaywrightTesting;
using Point = Blazor.Diagrams.Core.Geometry.Point;
using PointerEventArgs = Blazor.Diagrams.Core.Events.PointerEventArgs;
using Size = Blazor.Diagrams.Core.Geometry.Size;

namespace NetworkVisualisation.GUI.Winzor.Test.Components;

class JobNodeTest : LinkableNodeTest<NodeViewModel, JobNode, JobNodeModel>
{
	protected override JobNodeModel CreateNodeModel(INetworkEntity entity)
	{
		return new JobNodeModelBuilder().WithEntity(entity).Build();
	}

	protected override JobNodeModel CreateNodeModel(NodeViewModel nodeViewModel)
	{
		return new JobNodeModelBuilder().WithNodeViewModel(nodeViewModel).Build();
	}

	[Test]
	public void TestAffinity()
	{
		var affinity = new Mock<IAffinity>();
		affinity.Setup(a => a.Name).Returns("affinity1");

		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.IsLinkedToWorkflow = true; });
		entity.AppliedAffinities.Add(affinity.Object);

		var nodeViewModel = new NetworkViewModelBuilder().Build()
			.CreateNodeViewModel(entity);

		var jobNode = RenderNode(nodeViewModel);

		var affinityText = jobNode.Find(".jobnode__affinity").InnerHtml;
		Assert.That(affinityText, Is.EqualTo("affinity1"));
	}

	[Test]
	public void TestAffinityColor()
	{
		var affinity = new Mock<IAffinity>();
		affinity.Setup(a => a.Name).Returns("affinity1");
		affinity.Setup(a => a.Colour).Returns(Color.Red);

		var entity = Stub.Entity(e => e.ShapeType = ShapeType);
		entity.AppliedAffinities.Add(affinity.Object);

		var nodeViewModel = new NetworkViewModelBuilder().Build()
			.CreateNodeViewModel(entity);

		var jobNode = RenderNode(nodeViewModel);

		var card = jobNode.Find(".jobnode__card");
		var background = card.GetStyle().GetBackgroundColor();
		Assert.That(background, Is.EqualTo("rgba(255, 0, 0, 1)"));
	}

	[Test]
	public void DoubleClickDoesNotInvokeViewEntity()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.HasLinkedEntity = true; });
		var diagram = RenderDiagram(entity);

		var diagramName = diagram.Find(".jobnode__diagramname > .textarea__resizable");
		Assert.Throws<MissingEventHandlerException>(() => diagramName.DoubleClick());

		var jobName = diagram.Find(".jobnode__jobname");
		Assert.Throws<MissingEventHandlerException>(() => jobName.DoubleClick());

		var completionCriteria = diagram.Find(".jobnode__completioncriteria > .textarea__resizable");
		Assert.Throws<MissingEventHandlerException>(() => completionCriteria.DoubleClick());

		var notes = diagram.Find(".jobnode__notes > .textarea__resizable");
		Assert.Throws<MissingEventHandlerException>(() => notes.DoubleClick());
	}

	[Test]
	public void TestDeleteIconRendered()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.X = 10; e.Y = 11; });
		var diagram = RenderDiagram(entity);

		Assert.That(diagram.Find(".deleteicon"), Is.Not.Null);
	}

#pragma warning disable CW1178 // Do Not Invoke Old Res.GetString Methods;  these tests don't require translation
	[Test]
	public void TestDeleteIconTooltipForNodeEntity()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.CanUnlinkEntity = true; });
		var jobNode = RenderNodeInDiagram(entity);

		var deleteIconComponent = jobNode.Find(".deleteicon__deletecircle");
		Assert.That(jobNode.Instance.Node!.DeleteTooltip, Is.EqualTo(Res.GetString("89202b29-7f13-4849-882b-95700ed4de4c", "Remove shape from diagram and delete underlying entity.")));
		Assert.That(deleteIconComponent.GetAttribute("title"), Is.EqualTo(jobNode.Instance.Node!.DeleteTooltip));
	}

	[Test]
	public void TestDuration()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.ExplicitDurationMinutes = 1; e.IsLinkedToWorkflow = true; });

		var diagram = RenderDiagram(entity);
		var jobNode = diagram.FindComponent<JobNode>();

		var duration = jobNode.Find(".jobnode__duration").InnerHtml;
		Assert.That(duration, Is.EqualTo("0:01"));
	}

	[Test]
	public void TestJobNumber()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.JobNumber = "JobNumber"; e.HasLinkedEntity = true; });

		var diagram = RenderDiagram(entity);
		var jobNode = diagram.FindComponent<JobNode>();

		var jobNumber = jobNode.Find(".jobnode__jobnumber").InnerHtml;
		Assert.That(jobNumber, Is.EqualTo("JobNumber"));
	}

	[Test]
	public void TestJobName()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.HasLinkedEntity = true; });
		var viewModel = new NetworkViewModelBuilder().Build().CreateNodeViewModel(entity);
		viewModel.JobName = "JobName";

		var jobNode = RenderNode(viewModel);

		var jobNameInput = jobNode.Find(".jobnode__jobname");
		Assert.That(jobNameInput.GetAttribute("value"), Is.EqualTo("JobName"));
	}

	[Test]
	public void TestCompletionCriteria()
	{
		var entity = Stub.Entity(e => e.ShapeType = ShapeType);
		var viewModel = new NetworkViewModelBuilder().Build().CreateNodeViewModel(entity);
		viewModel.CompletionCriteria = "CompletionCriteria";

		var jobNode = RenderNode(viewModel);

		var completionCriteriaTextArea = jobNode.Find(".jobnode__completioncriteria > .textarea__resizable");
		Assert.That(completionCriteriaTextArea.GetAttribute("value"), Is.EqualTo("CompletionCriteria"));
		Assert.That(completionCriteriaTextArea.GetAttribute("placeholder"), Is.EqualTo("Completion Criteria"));
	}

	[Test]
	public void TestNoStatusIcon()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.Status = WorkStatus.None; });

		var diagram = RenderDiagram(entity);
		var jobNode = diagram.FindComponent<JobNode>();

		Assert.Throws<ElementNotFoundException>(() => jobNode.Find(".jobnode__statusicon"));
	}

	[TestCase(WorkStatus.Startable, "asterisk.bmp")]
	[TestCase(WorkStatus.Suspended, "pause.bmp")]
	[TestCase(WorkStatus.Working, "play.bmp")]
	[TestCase(WorkStatus.Complete, "Complete-icon.png")]
	[TestCase(WorkStatus.Cancelled, "Cancel-icon.png")]
	public void TestStatusIcon(WorkStatus status, string iconFIle)
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.Status = status; });

		var diagram = RenderDiagram(entity);
		var jobNode = diagram.FindComponent<JobNode>();

		var icon = jobNode.Find(".jobnode__statusicon");
		Assert.That(icon.GetAttribute("src"), Does.EndWith(iconFIle));
	}

	[Test]
	public void TestProgressBarDoesNotRenderWithoutLowerBar()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.Height = 10; e.Width = 10; });

		var diagram = RenderDiagram(entity);
		var jobNode = diagram.FindComponent<JobNode>();

		Assert.Throws<ElementNotFoundException>(() => jobNode.Find(".ncn__progressbar"));
	}

	[Test]
	public void TestProgressBarRenders()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.Height = 10; e.Width = 10; });
		var network = new NetworkViewModelBuilder().Build();
		var mockNode = new Mock<NodeViewModel>(entity, network);
		mockNode.Setup(m => m.LowerBar).Returns(new NodeViewModel.ProgressBar(Color.Yellow, 35));

		var jobNode = RenderNode(mockNode.Object);

		var progressBar = jobNode.Find(".ncn__progressbar");
		Assert.That(progressBar, Is.Not.Null);
	}

	[Test]
	public void TestDurationHasTooltip()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.IsLinkedToWorkflow = true; });
		var viewModel = new NetworkViewModelBuilder().Build().CreateNodeViewModel(entity);

		var jobNode = RenderNode(viewModel);
		var component = jobNode.Find(".jobnode__duration");

		Assert.That(component.GetAttribute("title"), Is.EqualTo(viewModel.DurationTooltip));
	}

	[Test]
	public void TestStartDateHasTooltip()
	{
		var entity = Stub.Entity(e => e.ShapeType = ShapeType);
		var networkViewModel = new NetworkViewModelBuilder().Build();
		var nodeViewModelMock = new Mock<NodeViewModel>(entity, networkViewModel);
		nodeViewModelMock.Setup(m => m.ShowScheduleDetails).Returns(true);

		var jobNode = RenderNode(nodeViewModelMock.Object);
		var component = jobNode.Find(".jobnode__startdate");

		Assert.That(component.GetAttribute("title"), Is.EqualTo(nodeViewModelMock.Object.StartDateToolTip));
	}

	[Test]
	public void TestRemainingDurationHasTooltip()
	{
		var entity = Stub.Entity(e => e.ShapeType = ShapeType);
		var networkViewModel = new NetworkViewModelBuilder().Build();
		var nodeViewModelMock = new Mock<NodeViewModel>(entity, networkViewModel);
		nodeViewModelMock.Setup(m => m.ShowScheduleDetails).Returns(true);

		var jobNode = RenderNode(nodeViewModelMock.Object);
		var component = jobNode.Find(".jobnode__remainingduration");

		Assert.That(component.GetAttribute("title"), Is.EqualTo(nodeViewModelMock.Object.RemainingDurationToolTip));
	}

	[Test]
	public void TestFinishDateHasTooltip()
	{
		var entity = Stub.Entity(e => e.ShapeType = ShapeType);
		var networkViewModel = new NetworkViewModelBuilder().Build();
		var nodeViewModelMock = new Mock<NodeViewModel>(entity, networkViewModel);
		nodeViewModelMock.Setup(m => m.ShowScheduleDetails).Returns(true);

		var jobNode = RenderNode(nodeViewModelMock.Object);
		var component = jobNode.Find(".jobnode__finishdate");

		Assert.That(component.GetAttribute("title"), Is.EqualTo(nodeViewModelMock.Object.FinishDateToolTip));
	}

	[Test]
	public void TestStatusHasTooltip()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.Status = WorkStatus.Working; });
		var nodeViewModel = new NetworkViewModelBuilder().Build().CreateNodeViewModel(entity);

		var jobNode = RenderNode(nodeViewModel);
		var component = jobNode.Find(".jobnode__statusicon");

		Assert.That(component.GetAttribute("title"), Is.EqualTo(nodeViewModel.StatusTooltip));
	}

	[Test]
	public void TestJobNumberHasTooltip()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.IsLinkedToWorkflow = true; });
		var nodeViewModel = new NetworkViewModelBuilder().Build().CreateNodeViewModel(entity);

		var jobNode = RenderNode(nodeViewModel);
		var component = jobNode.Find(".jobnode__jobnumbertext");

		Assert.That(component.GetAttribute("title"), Is.EqualTo(nodeViewModel.JobNumberTooltip));
	}

	[Test]
	public void TestAffinityHasTooltip()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.IsLinkedToWorkflow = true; });
		var nodeViewModel = new NetworkViewModelBuilder().Build().CreateNodeViewModel(entity);

		var jobNode = RenderNode(nodeViewModel);
		var component = jobNode.Find(".jobnode__affinity");

		Assert.That(component.GetAttribute("title"), Is.EqualTo(nodeViewModel.AppliedAttributesTooltip));
	}

	[TestCase(100, 50)]
	[TestCase(270, 150)]
	[WithPlaywrightPage]
	public async Task TestJobNodeStyle_OverflowValueAsync(int width, int height)
	{
		var testCases = new[] {
			(selector: ".jobnode__jobnumbertext", expected: "visible"),
			(selector: ".jobnode__affinity", expected: "hidden"),
			(selector: ".jobnode__duration", expected: "hidden"),
		};

		var affinity = new Mock<IAffinity>();
		affinity.Setup(a => a.Name).Returns("A very very very loooooong affinity name");

		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.IsLinkedToWorkflow = true; e.Width = width; e.Height = height; });
		entity.AppliedAffinities.Add(affinity.Object);

		var network = new NetworkBuilder().WithEntities(entity).Build();

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => new NetworkUserControlBuilder().WithNetwork(network).Build());

		await Assert.MultipleAsync(async () =>
		{
			foreach (var (selector, expected) in testCases)
			{
				var element = await page.WaitForSelectorAsync(selector);
				Assert.That(element, Is.Not.Null, selector);

				var overflowStyle = await element!.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('overflow')");
				Assert.That(overflowStyle.Value.ToString(), Is.EqualTo(expected), selector);
			}
		});
	}

	[Test]
	public async Task TestJobNameIsBoundToViewModelAsync()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.HasLinkedEntity = true; });
		var nodeViewModel = new NetworkViewModelBuilder().Build().CreateNodeViewModel(entity);
		nodeViewModel.JobName = "JobName";

		var jobNode = RenderNode(nodeViewModel);
		var nodeModel = jobNode.Instance.Node!;

		var jobNameInput = jobNode.Find(".jobnode__jobname");
		await jobNameInput.ChangeAsync(new ChangeEventArgs() { Value = "Updated JobName" });
		Assert.That(nodeModel.JobName, Is.EqualTo("Updated JobName"));
		Assert.That(nodeViewModel.JobName, Is.EqualTo("Updated JobName"));
	}

	[Test]
	public async Task TestCompletionCriteriaIsBoundToViewModelAsync()
	{
		var entity = Stub.Entity(e => e.ShapeType = ShapeType);
		var nodeViewModel = new NetworkViewModelBuilder().Build().CreateNodeViewModel(entity);
		nodeViewModel.CompletionCriteria = "CompletionCriteria";

		var jobNode = RenderNode(nodeViewModel);
		var nodeModel = jobNode.Instance.Node!;

		var completionCriteriaTextArea = jobNode.Find(".jobnode__completioncriteria > .textarea__resizable");
		await completionCriteriaTextArea.ChangeAsync(new ChangeEventArgs() { Value = "Updated CompletionCriteria" });
		Assert.That(nodeModel.CompletionCriteria, Is.EqualTo("Updated CompletionCriteria"));
		Assert.That(nodeViewModel.CompletionCriteria, Is.EqualTo("Updated CompletionCriteria"));
	}

	[Test]
	public async Task TestPositionIsBoundToViewModelAsync()
	{
		var entity = Stub.Entity(e => e.ShapeType = ShapeType);
		var networkUserControl = new NetworkUserControlForTestBuilder().Build();
		var nodeViewModel = networkUserControl.CreateNodeViewModel(entity);
		networkUserControl.AddScheduledNode(nodeViewModel);

		var diagram = RenderDiagram(networkUserControl);
		var nodeModel = diagram.FindComponent<InDiagramNetworkNode<JobNodeModel, JobNode>>().Instance.Node!;

		Assert.That(nodeModel.Position, Is.EqualTo(new Point(0, 0)));
		Assert.That(nodeViewModel.X, Is.EqualTo(0));
		Assert.That(nodeViewModel.Y, Is.EqualTo(0));

		await diagram.InvokeAsync(() => nodeModel.SetPosition(100, 200));
		nodeModel.TriggerMoved();
		Assert.That(nodeModel.Position, Is.EqualTo(new Point(100, 200)));
		Assert.That(nodeViewModel.X, Is.EqualTo(100));
		Assert.That(nodeViewModel.Y, Is.EqualTo(200));
	}

	[Test]
	public void TestSizeIsBoundToViewModel()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.Width = 100; e.Height = 200; });
		var nodeViewModel = new NetworkViewModelBuilder().Build()
			.CreateNodeViewModel(entity);

		var jobNode = RenderNode(nodeViewModel);
		var nodeModel = jobNode.Instance.Node!;

		Assert.That(nodeModel.Size, Is.EqualTo(new Size(100, 200)));
		Assert.That(nodeViewModel.Width, Is.EqualTo(100));
		Assert.That(nodeViewModel.Height, Is.EqualTo(200));

		nodeModel.Size = new Size(300, 400);
		nodeModel.TriggerResized();
		Assert.That(nodeModel.Size, Is.EqualTo(new Size(300, 400)));
		Assert.That(nodeViewModel.Width, Is.EqualTo(300));
		Assert.That(nodeViewModel.Height, Is.EqualTo(400));
	}

	[Test]
	public void TestSelectedIsBoundToViewModelBothWays()
	{
		var entity = Stub.Entity(e => e.ShapeType = ShapeType);
		var networkUserControl = new NetworkUserControlForTestBuilder().Build();
		var nodeViewModel = networkUserControl.CreateNodeViewModel(entity);
		networkUserControl.AddScheduledNode(nodeViewModel);

		var diagram = RenderDiagram(networkUserControl);
		var nodeModel = diagram.Instance.DiagramModel!.Nodes.First();

		Assert.That(nodeModel.Selected, Is.False);
		Assert.That(nodeViewModel.IsSelected, Is.False);

		diagram.Instance.DiagramModel!.SelectModel(nodeModel, false);

		Assert.That(nodeModel.Selected, Is.True);
		Assert.That(nodeViewModel.IsSelected, Is.True);

		diagram.Instance.DiagramModel!.UnselectModel(nodeModel);

		Assert.That(nodeModel.Selected, Is.False);
		Assert.That(nodeViewModel.IsSelected, Is.False);

		nodeViewModel.IsSelected = true;

		Assert.That(nodeModel.Selected, Is.True);
		Assert.That(nodeViewModel.IsSelected, Is.True);

		nodeViewModel.IsSelected = false;

		Assert.That(nodeModel.Selected, Is.False);
		Assert.That(nodeViewModel.IsSelected, Is.False);
	}

	[Test]
	public async Task JobNodeRightClickOpensContextMenuAsync()
	{
		var action = new Mock<INetworkAction>();
		action.Setup(a => a.IsEnabled()).Returns(() =>
		{
			var actionAccessibility = new Mock<INetworkActionAccessibility>();
			actionAccessibility.Setup(i => i.IsAllowed).Returns(true);
			return actionAccessibility.Object;
		});
		action.Setup(a => a.IsApplicable()).Returns(() =>
		{
			var actionAccessibility = new Mock<INetworkActionAccessibility>();
			actionAccessibility.Setup(i => i.IsAllowed).Returns(true);
			return actionAccessibility.Object;
		});

		action.Setup(a => a.GetName()).Returns(ResString.GetMultilingualString(string.Empty, "Test Action"));
		action.Setup(a => a.GetDescription()).Returns(ResString.GetMultilingualString(string.Empty, "Test Action"));
		action.Setup(a => a.IsActivated()).Returns(false);
		action.Setup(a => a.GetChildActions()).Returns(Enumerable.Empty<INetworkAction>);

		var entity = Stub.Entity(e => e.ShapeType = ShapeType);
		var network = new NetworkBuilder()
			.WithEntities(entity)
			.WithCustomActions(action.Object)
			.Build();

		var menuDisplayer = new Mock<IMenuDisplayer>();
		Services.AddSingleton(typeof(IMenuDisplayer), menuDisplayer.Object);

		var networkUserControl = new NetworkUserControlForTestBuilder().WithNetwork(network).Build();
		var nodeViewModel = networkUserControl.CreateNodeViewModel(entity);
		networkUserControl.AddScheduledNode(nodeViewModel);

		var diagram = RenderDiagram(networkUserControl);

		var diagramArea = diagram.Find(".jobnode--indiagram");
		await diagramArea.ContextMenuAsync(new WebMouseEventArgs());

		menuDisplayer.Verify(m => m.SendShowMenuRequestAsync(
			It.IsAny<MenuInteropModel>(),
			It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(),
			It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);

		var menuInteropModel = menuDisplayer.Invocations.Single().Arguments.First() as MenuInteropModel;
		Assert.That(menuInteropModel!.MenuItems, Is.Not.Empty);
	}

	[Test]
	public async Task JobNodeActionsReloadsAsRightClickOpensContextMenuAsync()
	{
		var action = new Mock<INetworkAction>();
		action.Setup(a => a.IsEnabled()).Returns(() =>
		{
			var actionAccessibility = new Mock<INetworkActionAccessibility>();
			actionAccessibility.Setup(i => i.IsAllowed).Returns(true);
			return actionAccessibility.Object;
		});
		action.Setup(a => a.IsApplicable()).Returns(() =>
		{
			var actionAccessibility = new Mock<INetworkActionAccessibility>();
			actionAccessibility.Setup(i => i.IsAllowed).Returns(true);
			return actionAccessibility.Object;
		});

		var action2 = new Mock<INetworkAction>();
		action2.Setup(a => a.IsEnabled()).Returns(() =>
		{
			var actionAccessibility = new Mock<INetworkActionAccessibility>();
			actionAccessibility.Setup(i => i.IsAllowed).Returns(true);
			return actionAccessibility.Object;
		});
		action2.Setup(a => a.IsApplicable()).Returns(() =>
		{
			var actionAccessibility = new Mock<INetworkActionAccessibility>();
			actionAccessibility.Setup(i => i.IsAllowed).Returns(true);
			return actionAccessibility.Object;
		});

		action.Setup(a => a.GetName()).Returns(ResString.GetMultilingualString(string.Empty, "Test Action"));
		action.Setup(a => a.GetDescription()).Returns(ResString.GetMultilingualString(string.Empty, "Test Action"));
		action.Setup(a => a.IsActivated()).Returns(false);
		action.Setup(a => a.GetChildActions()).Returns(Enumerable.Empty<INetworkAction>);

		action2.Setup(a => a.GetName()).Returns(ResString.GetMultilingualString(string.Empty, "Test Action 2"));
		action2.Setup(a => a.GetDescription()).Returns(ResString.GetMultilingualString(string.Empty, "Test Action 2"));
		action2.Setup(a => a.IsActivated()).Returns(false);
		action2.Setup(a => a.GetChildActions()).Returns(Enumerable.Empty<INetworkAction>);

		var entity = Stub.Entity(e => e.ShapeType = ShapeType);
		var network = new NetworkBuilder()
			.WithEntities(entity)
			.WithCustomActions(action.Object, action2.Object)
			.Build();

		var menuDisplayer = new Mock<IMenuDisplayer>();
		Services.AddSingleton(typeof(IMenuDisplayer), menuDisplayer.Object);

		var userControl = new NetworkUserControlForTestBuilder().WithNetwork(network).Build();
		var node = userControl.CreateNodeViewModel(entity);
		userControl.AddScheduledNode(node);

		var diagram = RenderDiagram(userControl);

		var diagramArea = diagram.Find(".jobnode--indiagram");
		await diagramArea.ContextMenuAsync(new WebMouseEventArgs());

		var numberOfMenuItemsBeforeReload = 2;
		var menuInteropModel = menuDisplayer.Invocations.Single().Arguments.First() as MenuInteropModel;
		Assert.That(menuInteropModel!.MenuItems, Is.Not.Null);
		Assert.That(menuInteropModel!.MenuItems!.Length, Is.EqualTo(numberOfMenuItemsBeforeReload));

		network.CustomNetworkActions.Clear();
		menuDisplayer.Invocations.Clear();

		var action3 = new Mock<INetworkAction>();
		action3.Setup(a => a.IsEnabled()).Returns(() =>
		{
			var actionAccessibility = new Mock<INetworkActionAccessibility>();
			actionAccessibility.Setup(i => i.IsAllowed).Returns(true);
			return actionAccessibility.Object;
		});
		action3.Setup(a => a.IsApplicable()).Returns(() =>
		{
			var actionAccessibility = new Mock<INetworkActionAccessibility>();
			actionAccessibility.Setup(i => i.IsAllowed).Returns(true);
			return actionAccessibility.Object;
		});

		action3.Setup(a => a.GetName()).Returns(ResString.GetMultilingualString(string.Empty, "Test Action 3"));
		action3.Setup(a => a.GetDescription()).Returns(ResString.GetMultilingualString(string.Empty, "Test Action 3"));
		action3.Setup(a => a.IsActivated()).Returns(false);
		action3.Setup(a => a.GetChildActions()).Returns(Enumerable.Empty<INetworkAction>);
		network.CustomNetworkActions.Add(action3.Object);

		await diagramArea.ContextMenuAsync(new WebMouseEventArgs());

		var numberOfMenuItemsAfterReload = 1;
		menuInteropModel = menuDisplayer.Invocations.Single().Arguments.First() as MenuInteropModel;
		Assert.That(menuInteropModel!.MenuItems, Is.Not.Null);
		Assert.That(menuInteropModel!.MenuItems!.Length, Is.EqualTo(numberOfMenuItemsAfterReload));

		var expectedActionName = "Test Action 3";
		var actionMenu = menuInteropModel.MenuItems!.FirstOrDefault();
		Assert.That(actionMenu!.Text, Is.EqualTo(expectedActionName));
	}
#pragma warning restore CW1178 // Do Not Invoke Old Res.GetString Methods

	[Test]
	public void DoubleClickOnJobNumberInvokesViewEntity()
	{
		var diagramEntity = Stub.Entity();
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.HasLinkedEntity = true; });

		var entities = new ImpObservableSet<INetworkEntity>(new[] { entity });
		var network = Mock.Of<INetwork>(x => x.Entities == entities && x.DiagramEntity == diagramEntity && x.EntityPositionStrategy == new EntityPositionStrategy());

		var networkUserControl = new NetworkUserControlForTestBuilder().WithNetwork(network).Build();
		var diagram = RenderDiagram(networkUserControl);

		var jobNumber = diagram.Find(".jobnode__jobnumber");
		jobNumber.DoubleClick();
		Mock.Get(network).Verify(x => x.ViewEntity(entity));
	}

	[Test]
	public void TestJobBarDoesNotRenderInUnlinkedShape()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.IsLinkedToWorkflow = false; });
		var nodeViewModel = new NetworkViewModelBuilder().Build().CreateNodeViewModel(entity);

		var jobNode = RenderNode(nodeViewModel);

		Assert.Throws<ElementNotFoundException>(() => jobNode.Find(".jobnode__jobnumbertext"));
		Assert.Throws<ElementNotFoundException>(() => jobNode.Find(".jobnode__affinity"));
		Assert.Throws<ElementNotFoundException>(() => jobNode.Find(".jobnode__duration"));
	}

	[Test]
	public void TestJobBarRendersWhenShapeIsLinkedToWorkflow()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.IsLinkedToWorkflow = true; });
		var nodeViewModel = new NetworkViewModelBuilder().Build().CreateNodeViewModel(entity);

		var jobNode = RenderNode(nodeViewModel);

		var component1 = jobNode.Find(".jobnode__jobnumbertext");
		Assert.That(component1, Is.Not.Null);

		var component2 = jobNode.Find(".jobnode__affinity");
		Assert.That(component2, Is.Not.Null);
		Assert.That(component2.GetAttribute("style"), Does.Contain("grid-column: 3;"));

		var component3 = jobNode.Find(".jobnode__duration");
		Assert.That(component3, Is.Not.Null);
	}

	[Test]
	public void AppliedAffinitesRenderInUnlinkedShape()
	{
		var affinity = new Mock<IAffinity>();
		affinity.Setup(a => a.Name).Returns("affinity");

		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.IsLinkedToWorkflow = false; });
		entity.AppliedAffinities.Add(affinity.Object);

		var viewModel = new NetworkViewModelBuilder().Build().CreateNodeViewModel(entity);

		var jobNode = RenderNode(viewModel);

		Assert.Throws<ElementNotFoundException>(() => jobNode.Find(".jobnode__jobnumbertext"));
		Assert.Throws<ElementNotFoundException>(() => jobNode.Find(".jobnode__duration"));

		var affinityText = jobNode.Find(".jobnode__affinity");
		Assert.That(affinityText, Is.Not.Null);
		Assert.That(affinityText.GetAttribute("style"), Does.Contain("grid-column: 2 / 5;"));
	}

	[Test]
	public void AppliedAffinitesRendersAppliedAttributesReadableText()
	{
		var affinity = new Mock<IAffinity>();
		affinity.Setup(a => a.Name).Returns("affinity");

		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.IsLinkedToWorkflow = false; });
		entity.AppliedAffinities.Add(affinity.Object);

		var viewModel = new NetworkViewModelBuilder().Build().CreateNodeViewModel(entity);

		var jobNode = RenderNode(viewModel);

		var affinityText = jobNode.Find(".jobnode__affinity");

		Assert.That(affinityText.GetAttribute("style"), Does.Contain("grid-column: 2 / 5;"));
		Assert.That(affinityText.Text, Is.Not.Empty, "AppliedAttributes text should not be empty");
		Assert.That(affinityText.Text, Does.Contain("affinity"), "AppliedAttributes text should contain the expected value");
	}

	[Test]
	public void TestJobBarRefreshOnJobNameChangeFromServer()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.IsLinkedToWorkflow = false; });
		var nodeViewModel = new NetworkViewModelBuilder().Build().CreateNodeViewModel(entity);

		var jobNode = RenderNode(nodeViewModel);

		Assert.Throws<ElementNotFoundException>(() => jobNode.Find(".jobnode__jobnumbertext"));
		Assert.Throws<ElementNotFoundException>(() => jobNode.Find(".jobnode__affinity"));
		Assert.Throws<ElementNotFoundException>(() => jobNode.Find(".jobnode__duration"));

		entity.IsLinkedToWorkflow = true;
		entity.JobName = "a";
		entity.TriggerPropertyChanged(nameof(NodeViewModel.JobName));

		var component1 = jobNode.Find(".jobnode__jobnumbertext");
		Assert.That(component1, Is.Not.Null);

		var component2 = jobNode.Find(".jobnode__affinity");
		Assert.That(component2, Is.Not.Null);

		var component3 = jobNode.Find(".jobnode__duration");
		Assert.That(component3, Is.Not.Null);

		entity.IsLinkedToWorkflow = false;
		entity.JobName = "b";
		entity.TriggerPropertyChanged(nameof(NodeViewModel.JobName));

		Assert.Throws<ElementNotFoundException>(() => jobNode.Find(".jobnode__jobnumbertext"));
		Assert.Throws<ElementNotFoundException>(() => jobNode.Find(".jobnode__affinity"));
		Assert.Throws<ElementNotFoundException>(() => jobNode.Find(".jobnode__duration"));
	}

	[Test]
	public void TestJobDetailsDoNotRenderNoLinkedEntity()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.HasLinkedEntity = false; });
		var nodeViewModel = new NetworkViewModelBuilder().Build().CreateNodeViewModel(entity);

		var jobNode = RenderNode(nodeViewModel);

		Assert.Throws<ElementNotFoundException>(() => jobNode.Find(".jobnode__jobdetails"));
	}

	[Test]
	public void TestJobDetailsRender()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.HasLinkedEntity = true; });
		var nodeViewModel = new NetworkViewModelBuilder().Build().CreateNodeViewModel(entity);

		var jobNode = RenderNode(nodeViewModel);

		var component = jobNode.Find(".jobnode__jobdetails");
		Assert.That(component, Is.Not.Null);
	}

	[Test]
	public void TestJobDetailsRefreshOnJobNameChangeFromServer()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.HasLinkedEntity = false; });
		var nodeViewModel = new NetworkViewModelBuilder().Build().CreateNodeViewModel(entity);

		var jobNode = RenderNode(nodeViewModel);

		Assert.Throws<ElementNotFoundException>(() => jobNode.Find(".jobnode__jobdetails"));

		entity.HasLinkedEntity = true;
		entity.JobName = "a";
		entity.TriggerPropertyChanged(nameof(NodeViewModel.JobName));

		Assert.That(jobNode.Find(".jobnode__jobdetails"), Is.Not.Null);

		entity.HasLinkedEntity = false;
		entity.JobName = "b";
		entity.TriggerPropertyChanged(nameof(NodeViewModel.JobName));

		Assert.Throws<ElementNotFoundException>(() => jobNode.Find(".jobnode__jobdetails"));
	}

	[Test]
	public void BlackDotRenderedForOutgoingLinksOutOfDiagram()
	{
		var entity1 = Stub.Entity(e => { e.ShapeType = ShapeType; e.JobNumber = "job1"; });
		var entity2 = Stub.Entity(e => { e.ShapeType = ShapeType; e.JobNumber = "job2"; });
		var network = new NetworkBuilder().WithEntities(entity1, entity1).Build();
		var relationship = network.CreateRelationship(entity1, entity2);

		entity1.PostRequisiteLinks = new[] { relationship };
		entity2.PreRequisiteLinks = new[] { relationship };

		var jobNodeModelBuilder = new JobNodeModelBuilder().WithNetwork(network);

		var jobNodeModel1 = jobNodeModelBuilder.WithEntity(entity1).Build();
		var jobNode1 = RenderComponent<JobNode>(parameters =>
		{
			parameters.Add(p => p.Node, jobNodeModel1);
		});

		Assert.That(() => jobNode1.Find(".jobnode__postreqcircle"), Is.Not.Null);

		var jobNodeModel2 = jobNodeModelBuilder.WithEntity(entity2).Build();
		var jobNode2 = RenderComponent<JobNode>(parameters =>
		{
			parameters.Add(p => p.Node, jobNodeModel2);
		});

		Assert.That(() => jobNode2.Find(".jobnode__postreqcircle"), Throws.TypeOf<ElementNotFoundException>());
	}

	[Test]
	public void TestBlackDotAddedOnLinkAddition()
	{
		var entity1 = Stub.Entity(e => { e.ShapeType = ShapeType; e.JobNumber = "job1"; });
		var entity2 = Stub.Entity(e => { e.ShapeType = ShapeType; e.JobNumber = "job2"; });
		var network = new NetworkBuilder().WithEntities(entity1, entity2).Build();
		var networkUserControl = new NetworkUserControlForTestBuilder().WithNetwork(network).Build();
		using var diagramModel = new NCNDiagramModelBuilder().WithNetworkUserControl(networkUserControl).Build();

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		Assert.That(() => diagram.FindAll(".jobnode__postreqcircle").Count, Is.EqualTo(0));

		var jobNodes = diagram.FindComponents<JobNode>();

		diagramModel.Links.Add(new NetworkLinkModel(jobNodes[0].Instance.Node, jobNodes[1].Instance.Node));

		Assert.That(() => jobNodes[0].Find(".jobnode__postreqcircle"), Is.Not.Null);
		Assert.That(() => jobNodes[1].Find(".jobnode__postreqcircle"), Throws.TypeOf<ElementNotFoundException>());
	}

	[Test]
	public void TestBlackDotRemovedOnLinkDeletion()
	{
		var entity1 = Stub.Entity(e => { e.ShapeType = ShapeType; e.JobNumber = "job1"; });
		var entity2 = Stub.Entity(e => { e.ShapeType = ShapeType; e.JobNumber = "job2"; });
		var network = new NetworkBuilder().WithEntities(entity1, entity2).Build();
		var relationship = network.CreateRelationship(entity1, entity2);

		entity1.Links = new[] { relationship };
		entity1.PostRequisiteLinks = new[] { relationship };

		var userControl = new NetworkUserControlForTestBuilder().WithNetwork(network).Build();
		using var diagramModel = new NCNDiagramModelBuilder().WithNetworkUserControl(userControl).Build();

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var jobNodes = diagram.FindComponents<JobNode>();

		Assert.That(() => jobNodes[0].Find(".jobnode__postreqcircle"), Is.Not.Null);
		Assert.That(() => jobNodes[1].Find(".jobnode__postreqcircle"), Throws.TypeOf<ElementNotFoundException>());

		diagramModel.Links.Remove(diagramModel.Links[0]);

		Assert.That(() => diagram.FindAll(".jobnode__postreqcircle").Count, Is.EqualTo(0));
	}

	[Test]
	public void TestNodeNotificationsAreUpdated_WhenHasNotificationsChangedHasBeenNotified()
	{
		var entitySource = Stub.Entity(e => { e.X = 200; e.Y = 10; e.JobNumber = "1"; e.HasNotifications = true; e.EntityState = EntityState.HasWarnings; });
		var entityTarget = Stub.Entity(e => { e.X = 100; e.Y = 80; e.JobNumber = "2"; e.HasNotifications = true; e.EntityState = EntityState.HasWarnings; });

		var network = new NetworkBuilder().WithEntities(entitySource, entityTarget).Build();
		network.AddRelationship(entitySource, entityTarget);

		var userControl = new NetworkUserControlForTestBuilder().WithNetwork(network).Build();
		using var diagramModel = new NCNDiagramModelBuilder().WithNetworkUserControl(userControl).Build();

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		Assert.That(() => diagram.FindAll(".jobnode__entitystateicon").Count, Is.EqualTo(2));

		diagramModel.Links.Remove(diagramModel.Links[0]);

		entitySource.HasNotifications = false;
		entitySource.EntityState = EntityState.None;
		entitySource.TriggerPropertyChanged(nameof(NetworkNodeData.HasNotifications));

		Assert.That(() => diagram.FindAll(".jobnode__entitystateicon").Count, Is.EqualTo(1));

		entityTarget.HasNotifications = false;
		entityTarget.EntityState = EntityState.None;
		entityTarget.TriggerPropertyChanged(nameof(NetworkNodeData.HasNotifications));

		Assert.That(() => diagram.FindAll(".jobnode__entitystateicon").Count, Is.EqualTo(0));
	}

	[Test]
	public void TextAreasRenderedAsDynamicTextAreas()
	{
		var nodeViewModel = new JobNodeModelBuilder().Build();
		var jobNode = RenderNode(nodeViewModel);

		var textAreas = jobNode.FindComponents<DynamicTextArea>();
		Assert.That(textAreas.Count, Is.EqualTo(3));

		Assert.That(textAreas[0].Find(".jobnode__diagramname"), Is.Not.Null);
		Assert.That(textAreas[1].Find(".jobnode__completioncriteria"), Is.Not.Null);
		Assert.That(textAreas[2].Find(".jobnode__notes"), Is.Not.Null);
	}

	static IEnumerable<TestCaseData> RecalculatePortPositionOnResize_TestData
	{
		get
		{
			yield return new TestCaseData("topleft", new Point(20, 48), new Point(190, 48));
			yield return new TestCaseData("topright", new Point(10, 48), new Point(200, 48));
			yield return new TestCaseData("bottomleft", new Point(20, 33), new Point(190, 33));
			yield return new TestCaseData("bottomright", new Point(10, 33), new Point(200, 33));
		}
	}

	[TestCaseSource(nameof(RecalculatePortPositionOnResize_TestData))]
	public async Task RecalculatePortPositionOnResizeAsync(string resizerClass, Point inputPortPosition, Point outputPortPosition)
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.Width = 200; e.Height = 200; });
		var viewModel = new NetworkViewModelBuilder().Build().CreateNodeViewModel(entity);

		var jobNode = RenderNodeInDiagram(viewModel, CreateNodeResizeControlFactory());

		var jobNodeModel = jobNode.Instance.Node!;
		var diagramModel = jobNode.Instance.Diagram!;
		diagramModel.SelectModel(jobNodeModel, false);
		var resizerControl = (NodeResizeControl)diagramModel.Controls.GetFor(jobNodeModel)!.First(c => c is NodeResizeControl rc && rc.Class!.Contains(resizerClass))!;

		var inputPort = jobNodeModel.Input;
		var outputPort = jobNodeModel.Output;

		Assert.That(inputPort.Position.X, Is.EqualTo(10));
		Assert.That(inputPort.Position.Y, Is.EqualTo(33));
		Assert.That(outputPort.Position.X, Is.EqualTo(190));
		Assert.That(outputPort.Position.Y, Is.EqualTo(33));

		var eventArgs = new PointerEventArgs(0, 0, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
		await resizerControl!.OnPointerDown(diagramModel, jobNodeModel, eventArgs);
		eventArgs = new PointerEventArgs(10, 15, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
		diagramModel.TriggerPointerMove(null, eventArgs);

		Assert.That(inputPort.Position, Is.EqualTo(inputPortPosition));
		Assert.That(outputPort.Position, Is.EqualTo(outputPortPosition));

		diagramModel.TriggerPointerUp(null, eventArgs);
		eventArgs = new PointerEventArgs(0, 0, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
		await resizerControl!.OnPointerDown(diagramModel, jobNodeModel, eventArgs);
		eventArgs = new PointerEventArgs(-10, -15, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
		diagramModel.TriggerPointerMove(null, eventArgs);

		Assert.That(inputPort.Position.X, Is.EqualTo(10));
		Assert.That(inputPort.Position.Y, Is.EqualTo(33));
		Assert.That(outputPort.Position.X, Is.EqualTo(190));
		Assert.That(outputPort.Position.Y, Is.EqualTo(33));
	}

	static IEnumerable<TestCaseData> RecalculatePortPositionOnResize_ScaledDiagram_TestData
	{
		get
		{
			yield return new TestCaseData("topleft", -49, 20, new Point(210, 253), new Point(390, 253));
			yield return new TestCaseData("topleft", -49, -20, new Point(210, 213), new Point(390, 213));
			yield return new TestCaseData("topleft", -51, 20, new Point(110, 253), new Point(390, 253));
			yield return new TestCaseData("topleft", -51, -20, new Point(110, 213), new Point(390, 213));
			yield return new TestCaseData("topleft", 49, 20, new Point(210, 253), new Point(390, 253));
			yield return new TestCaseData("topleft", 49, -20, new Point(210, 213), new Point(390, 213));
			yield return new TestCaseData("topleft", 51, 20, new Point(310, 253), new Point(390, 253));
			yield return new TestCaseData("topleft", 51, -20, new Point(310, 213), new Point(390, 213));
			yield return new TestCaseData("topright", -49, 20, new Point(210, 253), new Point(390, 253));
			yield return new TestCaseData("topright", -49, -20, new Point(210, 213), new Point(390, 213));
			yield return new TestCaseData("topright", -51, 20, new Point(210, 253), new Point(290, 253));
			yield return new TestCaseData("topright", -51, -20, new Point(210, 213), new Point(290, 213));
			yield return new TestCaseData("topright", 49, 20, new Point(210, 253), new Point(390, 253));
			yield return new TestCaseData("topright", 49, -20, new Point(210, 213), new Point(390, 213));
			yield return new TestCaseData("topright", 51, 20, new Point(210, 253), new Point(490, 253));
			yield return new TestCaseData("topright", 51, -20, new Point(210, 213), new Point(490, 213));
			yield return new TestCaseData("bottomleft", -49, 20, new Point(210, 233), new Point(390, 233));
			yield return new TestCaseData("bottomleft", -49, -20, new Point(210, 233), new Point(390, 233));
			yield return new TestCaseData("bottomleft", -51, 20, new Point(110, 233), new Point(390, 233));
			yield return new TestCaseData("bottomleft", -51, -20, new Point(110, 233), new Point(390, 233));
			yield return new TestCaseData("bottomleft", 49, 20, new Point(210, 233), new Point(390, 233));
			yield return new TestCaseData("bottomleft", 49, -20, new Point(210, 233), new Point(390, 233));
			yield return new TestCaseData("bottomleft", 51, 20, new Point(310, 233), new Point(390, 233));
			yield return new TestCaseData("bottomleft", 51, -20, new Point(310, 233), new Point(390, 233));
			yield return new TestCaseData("bottomright", -49, 20, new Point(210, 233), new Point(390, 233));
			yield return new TestCaseData("bottomright", -49, -20, new Point(210, 233), new Point(390, 233));
			yield return new TestCaseData("bottomright", -51, 20, new Point(210, 233), new Point(290, 233));
			yield return new TestCaseData("bottomright", -51, -20, new Point(210, 233), new Point(290, 233));
			yield return new TestCaseData("bottomright", 49, 20, new Point(210, 233), new Point(390, 233));
			yield return new TestCaseData("bottomright", 49, -20, new Point(210, 233), new Point(390, 233));
			yield return new TestCaseData("bottomright", 51, 20, new Point(210, 233), new Point(490, 233));
			yield return new TestCaseData("bottomright", 51, -20, new Point(210, 233), new Point(490, 233));
		}
	}

	[TestCaseSource(nameof(RecalculatePortPositionOnResize_ScaledDiagram_TestData))]
	public async Task RecalculatePortPositionOnResize_ScaledDiagramAsync(string resizerClass, double deltaX, double deltaY, Point inputPortExpectedPosition, Point outputPortExpectedPosition)
	{
		var diagramEntity = Stub.Entity(e => {
			e.SupportedActions = NetworkActions.StyleDiagram;
			e.IsDiagramScaled = true;
		});
		var entity = Stub.Entity(e => {
			e.ShapeType = ShapeType;
			e.Width = 200;
			e.Height = 200;
			e.X = 200;
			e.Y = 200;
		});
		var network = new NetworkBuilder().WithDiagramEntity(diagramEntity).WithEntities(entity).Build();

		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new NetworkUserControlBuilder().WithNetwork(network).Build();
		});

		var jobNode = rendered.FindComponent<InDiagramNetworkNode<JobNodeModel, JobNode>>();
		var jobNodeModel = jobNode.Instance.Node!;
		var diagramModel = jobNode.Instance.Diagram!;

		await ctx.Renderer.Dispatcher.InvokeAsync(() => diagramModel.SelectModel(jobNodeModel, false));
		rendered.WaitForState(() => diagramModel.Controls.GetFor(jobNodeModel)!.Where(c => c is NodeResizeControl).Count() == 4, TimeSpan.FromSeconds(3));
		var resizerControl = (NodeResizeControl)diagramModel.Controls.GetFor(jobNodeModel)!.First(c => c is NodeResizeControl rc && rc.Class!.Contains(resizerClass))!;

		var inputPort = jobNodeModel.Input;
		var outputPort = jobNodeModel.Output;

		Assert.That(inputPort.Position.X, Is.EqualTo(210));
		Assert.That(inputPort.Position.Y, Is.EqualTo(233));
		Assert.That(outputPort.Position.X, Is.EqualTo(390));
		Assert.That(outputPort.Position.Y, Is.EqualTo(233));

		await ctx.Renderer.Dispatcher.InvokeAsync(async () =>
		{
			var eventArgs = new PointerEventArgs(0, 0, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
			await resizerControl!.OnPointerDown(diagramModel, jobNodeModel, eventArgs);
			eventArgs = new PointerEventArgs(deltaX, deltaY, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
			diagramModel.TriggerPointerMove(null, eventArgs);
			diagramModel.TriggerPointerUp(null, eventArgs);
		});

		Assert.That(() => inputPort.Position, Is.EqualTo(inputPortExpectedPosition).After(3000, 100));
		Assert.That(() => outputPort.Position, Is.EqualTo(outputPortExpectedPosition).After(3000, 100));
	}

	static IEnumerable<TestCaseData> ChildNodeStaysWithinParentBounds_WhenResizingChildNode_TestData
	{
		get
		{
			yield return new TestCaseData("topleft", -100, -100, new Point(20, 60), new Size(250, 210));
			yield return new TestCaseData("topright", 100, -100, new Point(70, 60), new Size(250, 210));
			yield return new TestCaseData("bottomleft", -100, 100, new Point(20, 70), new Size(250, 245));
			yield return new TestCaseData("bottomright", 100, 100, new Point(70, 70), new Size(250, 245));
		}
	}

	[TestCaseSource(nameof(ChildNodeStaysWithinParentBounds_WhenResizingChildNode_TestData))]
	public async Task ChildNodeStaysWithinParentBounds_WhenResizingChildNodeAsync(string resizerClass, double deltaX, double deltaY, Point expectedPosition, Size expectedSize)
	{
		var childEntity = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Width = 200;
			e.Height = 200;
			e.X = 70;
			e.Y = 70;
		});
		var parentEntity = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Width = 300;
			e.Height = 300;
			e.X = 20;
			e.Y = 20;
			e.CanHaveChildren = true;
			e.Children = new[] { childEntity };
		});

		var networkUserControl = new NetworkUserControlForTestBuilder().WithTestRenderer(Renderer).Build();
		networkUserControl.AddScheduledNode(networkUserControl.CreateNodeViewModel(parentEntity));
		networkUserControl.AddScheduledNode(networkUserControl.CreateNodeViewModel(childEntity));

		using var diagramModel = new NCNDiagramModelBuilder()
			.WithNodeResizeControlFactory(CreateNodeResizeControlFactory())
			.WithNetworkUserControl(networkUserControl)
			.Build();

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var childJobNodeModel = diagramModel.Nodes.Last();
		diagramModel.SelectModel(childJobNodeModel, false);

		var resizerControl = (NodeResizeControl)diagramModel.Controls.GetFor(childJobNodeModel)!.First(c => c is NodeResizeControl rc && rc.Class!.Contains(resizerClass))!;

		Assert.That(childJobNodeModel.Position.X, Is.EqualTo(70));
		Assert.That(childJobNodeModel.Position.Y, Is.EqualTo(70));
		Assert.That(childJobNodeModel.Size!.Width, Is.EqualTo(200));
		Assert.That(childJobNodeModel.Size.Height, Is.EqualTo(200));

		var eventArgs = new PointerEventArgs(0, 0, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
		await resizerControl!.OnPointerDown(diagramModel, childJobNodeModel, eventArgs);
		eventArgs = new PointerEventArgs(deltaX, deltaY, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
		diagramModel.TriggerPointerMove(null, eventArgs);

		Assert.That(childJobNodeModel.Position, Is.EqualTo(expectedPosition));
		Assert.That(childJobNodeModel.Size, Is.EqualTo(expectedSize));
	}

	static IEnumerable<TestCaseData> ParentNodeStaysBiggerThanChildNode_WhenResizingParentNode_TestData
	{
		get
		{
			yield return new TestCaseData("topleft", 100, 100, new Point(50, 10), new Size(200, 240));
			yield return new TestCaseData("topright", -100, 100, new Point(0, 10), new Size(200, 240));
			yield return new TestCaseData("bottomleft", 100, -100, new Point(50, 0), new Size(200, 205));
			yield return new TestCaseData("bottomright", -100, -100, new Point(0, 0), new Size(200, 205));
		}
	}

	protected override string ShapeType => WinzorShapeTypes.Shape;

	[TestCaseSource(nameof(ParentNodeStaysBiggerThanChildNode_WhenResizingParentNode_TestData))]
	public async Task ParentNodeStaysBiggerThanChildNode_WhenResizingParentNodeAsync(string resizerClass, double deltaX, double deltaY, Point expectedPosition, Size expectedSize)
	{
		var childEntity1 = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Width = 100;
			e.Height = 100;
			e.X = 50;
			e.Y = 50;
		});

		var childEntity2 = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Width = 100;
			e.Height = 100;
			e.X = 100;
			e.Y = 100;
		});

		var parentEntity = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Width = 250;
			e.Height = 250;
			e.X = 0;
			e.Y = 0;
			e.CanHaveChildren = true;
			e.Children = new[] { childEntity1, childEntity2 };
		});

		var networkUserControl = new NetworkUserControlForTestBuilder()
			.WithTestRenderer(Renderer)
			.Build();

		networkUserControl.AddScheduledNode(networkUserControl.CreateNodeViewModel(parentEntity));
		networkUserControl.AddScheduledNode(networkUserControl.CreateNodeViewModel(childEntity1));
		networkUserControl.AddScheduledNode(networkUserControl.CreateNodeViewModel(childEntity2));

		using var diagramModel = new NCNDiagramModelBuilder()
			.WithNodeResizeControlFactory(CreateNodeResizeControlFactory())
			.WithNetworkUserControl(networkUserControl)
			.Build();

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var parentJobNodeModel = diagramModel.Nodes.First();
		diagramModel.SelectModel(parentJobNodeModel, false);

		var resizerControl = (NodeResizeControl)diagramModel.Controls.GetFor(parentJobNodeModel)!.First(c => c is NodeResizeControl rc && rc.Class!.Contains(resizerClass))!;

		Assert.That(parentJobNodeModel.Position.X, Is.EqualTo(0));
		Assert.That(parentJobNodeModel.Position.Y, Is.EqualTo(0));
		Assert.That(parentJobNodeModel.Size!.Width, Is.EqualTo(250));
		Assert.That(parentJobNodeModel.Size.Height, Is.EqualTo(250));

		var eventArgs = new PointerEventArgs(0, 0, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
		await resizerControl!.OnPointerDown(diagramModel, parentJobNodeModel, eventArgs);
		eventArgs = new PointerEventArgs(deltaX, deltaY, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
		diagramModel.TriggerPointerMove(null, eventArgs);

		Assert.That(parentJobNodeModel.Position, Is.EqualTo(expectedPosition));
		Assert.That(parentJobNodeModel.Size, Is.EqualTo(expectedSize));
	}
}
