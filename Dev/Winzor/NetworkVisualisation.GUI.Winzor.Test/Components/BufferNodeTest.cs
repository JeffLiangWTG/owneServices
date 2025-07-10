using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.GUI.Components;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.GUI.Services.NetworkNode;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.Winzor.Architecture.Test;
using Moq;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Extentions;

namespace NetworkVisualisation.GUI.Winzor.Test.Components;

class BufferNodeTest : LinkableNodeTest<BufferViewModel, BufferNode, BufferNodeModel>
{
	protected override string ShapeType => WinzorShapeTypes.Buffer;

	protected override BufferNodeModel CreateNodeModel(INetworkEntity entity)
	{
		return new BufferNodeModelBuilder().WithEntity(entity).Build();
	}

	protected override BufferNodeModel CreateNodeModel(BufferViewModel nodeViewModel)
	{
		return new BufferNodeModelBuilder().WithNodeViewModel(nodeViewModel).Build();
	}

	[Test]
	public void TestBorderWhenOnCriticalPath()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.IsOnCriticalPath = true; });
		var node = new BufferNodeModelBuilder().WithEntity(entity).Build();
		var renderedNode = RenderNode(node);

		var card = renderedNode.Find(".buffernode__card");

		Assert.That(card.GetAttribute("style"), Does.Contain("border: 2px solid red;"));
	}

	[Test]
	public void TestBorderWhenNotOnCriticalPath()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.IsOnCriticalPath = false; });
		var node = new BufferNodeModelBuilder().WithEntity(entity).Build();
		var renderedNode = RenderNode(node);

		var card = renderedNode.Find(".buffernode__card");

		Assert.That(card.GetAttribute("style"), Does.Contain("border: 1.5px solid black;"));
	}

	[Test]
	public void TestAdditionalDetailsText()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.AdditionalDetail = "AdditionalDetail"; });
		var bufferViewModel = new NetworkViewModelBuilder().Build()
			.CreateNodeViewModel(entity) as BufferViewModel;

		Assert.That(bufferViewModel, Is.Not.Null);

		var bufferNode = RenderNode(bufferViewModel);

		var description = bufferNode.Find(".additional-detail-content").InnerHtml;
		Assert.That(description, Is.EqualTo("AdditionalDetail"));
	}

	[Test]
	public void TextAreasRenderedAsDynamicTextAreas()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; });
		var bufferViewModel = new NetworkViewModelBuilder().Build()
			.CreateNodeViewModel(entity) as BufferViewModel;

		Assert.That(bufferViewModel, Is.Not.Null);

		var bufferNode = RenderNode(bufferViewModel);

		var textAreas = bufferNode.FindComponents<DynamicTextArea>();
		Assert.That(textAreas.Count, Is.EqualTo(2));

		Assert.That(textAreas[0].Find(".buffernode__diagramname"), Is.Not.Null);
		Assert.That(textAreas[1].Find(".buffernode__notes"), Is.Not.Null);
	}

	[Test]
	public async Task TestPenetrationPercentageLabelAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var diagram = NetworkTestCase.CreateDiagram(factory);
			diagram.SwitchToScaled();

			var buffer = factory.NewWithValidTestData<BMNCNBufferShape>();
			buffer.Name = "ProjectBuffer";
			var schedule = buffer.GetOrCreateSchedule();
			schedule.BNC_BufferPenetrationPercent = 20;

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var bufferEntity = buffer.AsEntity(network);

			var nwuc = new NetworkUserControlBuilder()
				.WithNodeViewModelProvider(new NodeViewModelProviderForTest())
				.WithNetwork(network)
				.Build();

			nwuc.AddScheduledNode(bufferEntity);

			return nwuc;
		});
		var penetrationPercentage = rendered.Find(".buffernode__penetrationlabel").TextContent;
		Assert.That(penetrationPercentage, Is.EqualTo("20 %"));
	}

	[TestCase(EntityState.HasWarnings)]
	[TestCase(EntityState.HasErrors)]
	[TestCase(EntityState.HasMessages)]
	public void TestHighlighterWithoutHasNotification(EntityState entityState)
	{
		var entity = Stub.Entity(e => {
			e.ShapeType = ShapeType;
			e.EntityState = entityState;
			e.HasNotifications = false;
		});

		var node = new BufferNodeModelBuilder().WithEntity(entity).Build();
		var bufferNode = RenderComponent<BufferNode>(parameters => parameters.Add(p => p.Node, node));

		Assert.Throws<ElementNotFoundException>(() => bufferNode.Find(".buffernode__notification"));
	}

	[Test]
	public void TestNoStatusIcon()
	{
		var entity = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Status = WorkStatus.None;
		});

		var node = new BufferNodeModelBuilder().WithEntity(entity).Build();
		var bufferNode = RenderComponent<BufferNode>(parameters => parameters.Add(p => p.Node, node));

		Assert.Throws<ElementNotFoundException>(() => bufferNode.Find(".buffernode__statusicon"));
	}

	[TestCase(WorkStatus.Startable, "asterisk.bmp")]
	[TestCase(WorkStatus.Suspended, "pause.bmp")]
	[TestCase(WorkStatus.Working, "play.bmp")]
	[TestCase(WorkStatus.Complete, "Complete-icon.png")]
	[TestCase(WorkStatus.Cancelled, "Cancel-icon.png")]
	public void TestStatusIcon(WorkStatus status, string iconFIle)
	{
		var entity = Stub.Entity(e =>
		{
			e.ShapeType = ShapeType;
			e.Status = status;
		});

		var node = new BufferNodeModelBuilder().WithEntity(entity).Build();
		var bufferNode = RenderComponent<BufferNode>(parameters => parameters.Add(p => p.Node, node));

		var icon = bufferNode.Find(".buffernode__statusicon");
		Assert.That(icon.GetAttribute("src"), Does.EndWith(iconFIle));
	}

	[Test]
	public void TestProgressBarIsRendered()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.Height = 10; e.Width = 10; });
		var networkViewModel = new NetworkViewModelBuilder().Build();
		var mockNode = new Mock<BufferViewModel>(entity, networkViewModel);
		var returnedProgressBar = new CargoWise.NetworkVisualisation.Business.NodeViewModel.ProgressBar(Color.Yellow, 35);
		mockNode.Setup(m => m.LowerBar).Returns(returnedProgressBar);

		var renderedNode = RenderNode(mockNode.Object);

		var progressBar = renderedNode.Find(".ncn__progressbar");
		Assert.That(progressBar, Is.Not.Null);
	}

	[Test]
	public void TestProgressBarDoesNotRenderWithoutLowerBar()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.Height = 10; e.Width = 10; });
		var networkViewModel = new NetworkViewModelBuilder().Build();
		var mockNode = new Mock<BufferViewModel>(entity, networkViewModel);

		var renderedNode = RenderNode(mockNode.Object);

		Assert.Throws<ElementNotFoundException>(() => renderedNode.Find(".ncn__progressbar"));
	}

	[Test]
	public void TestNodeNotificationsAreUpdated_WhenHasNotificationsChangedHasBeenNotified()
	{
		var entitySource = Stub.Entity(e => { e.ShapeType = ShapeType; e.X = 200; e.Y = 10; e.HasNotifications = true; e.EntityState = EntityState.HasWarnings; });
		var entityTarget = Stub.Entity(e => { e.ShapeType = ShapeType; e.X = 100; e.Y = 80; e.HasNotifications = true; e.EntityState = EntityState.HasWarnings; });
		var network = new NetworkBuilder().WithEntities(entitySource, entityTarget).Build();
		network.AddRelationship(entitySource, entityTarget);
		var userControl = new NetworkUserControlForTestBuilder().WithNetwork(network).Build();
		using var diagramModel = new NCNDiagramModelBuilder().WithNetworkUserControl(userControl).Build();
		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		Assert.That(() => diagram.FindAll(".buffernode__entitystateicon").Count, Is.EqualTo(2));

		diagramModel.Links.Remove(diagramModel.Links[0]);

		entitySource.HasNotifications = false;
		entitySource.EntityState = EntityState.None;
		entitySource.TriggerPropertyChanged(nameof(NetworkNodeData.HasNotifications));

		Assert.That(() => diagram.FindAll(".buffernode__entitystateicon").Count, Is.EqualTo(1));

		entityTarget.HasNotifications = false;
		entityTarget.EntityState = EntityState.None;
		entityTarget.TriggerPropertyChanged(nameof(NetworkNodeData.HasNotifications));

		Assert.That(() => diagram.FindAll(".buffernode__entitystateicon").Count, Is.EqualTo(0));
	}
}
