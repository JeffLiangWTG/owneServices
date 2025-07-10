using System.Collections.ObjectModel;
using System.Windows.Forms;
using Blazor.Diagrams.Core.Models;
using Bunit.Rendering;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Components;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.GUI.Services.JobNode;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.Winzor.Architecture.Test;
using Microsoft.Playwright;
using NetworkVisualisation.GUI.Winzor.Test.Extensions;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace NetworkVisualisation.GUI.Winzor.Test.Components;

using static PlaywrightTestContext;

class NetworkPortTest : BunitTestContext
{
	[TestCase(NetworkPortModel.PortType.Input, NetworkPortModel.PortType.Input, false)]
	[TestCase(NetworkPortModel.PortType.Output, NetworkPortModel.PortType.Output, false)]
	[TestCase(NetworkPortModel.PortType.Input, NetworkPortModel.PortType.Output, true)]
	[TestCase(NetworkPortModel.PortType.Output, NetworkPortModel.PortType.Input, true)]
	public void TestCanAttachTo(NetworkPortModel.PortType typ1, NetworkPortModel.PortType type2, bool canLink)
	{
		var network = new NetworkViewModel(new DummyNetwork());
		var nodeModel = new NodeViewModel(new Entity(), network);
		using var jobNodeService1 = new JobNodeService(nodeModel, new NetworkUserControlForTest(Renderer, network));
		using var jobNodeService2 = new JobNodeService(nodeModel, new NetworkUserControlForTest(Renderer, network));
		var node1 = new JobNodeModel(jobNodeService1);
		var node2 = new JobNodeModel(jobNodeService2);
		var port1 = new NetworkPortModel(node1, typ1);
		var port2 = new NetworkPortModel(node2, type2);
		Assert.That(port1.CanAttachTo(port2), Is.EqualTo(canLink));
	}

	[Test]
	public void TestNotEnabled_PortCanAttachTo()
	{
		var network = new NetworkViewModel(new DummyNetwork());
		var nodeModel = new NodeViewModel(new Entity(), network);
		using var jobNodeService1 = new JobNodeService(nodeModel, new NetworkUserControlForTest(Renderer, network));
		using var jobNodeService2 = new JobNodeService(nodeModel, new NetworkUserControlForTest(Renderer, network));
		var node1 = new JobNodeModel(jobNodeService1);
		var node2 = new JobNodeModel(jobNodeService2);
		var port1 = new NetworkPortModel(node1, NetworkPortModel.PortType.Input);
		var port2 = new NetworkPortModel(node2, NetworkPortModel.PortType.Output);
		Assert.That(port1.CanAttachTo(port2), Is.True);

		port2.Enabled = false;

		Assert.That(port1.CanAttachTo(port2), Is.False);
	}

	[Test]
	public void TestSelfLink()
	{
		var network = new NetworkViewModel(new DummyNetwork());
		var nodeModel = new NodeViewModel(new Entity(), network);
		using var jobNodeService = new JobNodeService(nodeModel, new NetworkUserControlForTest(Renderer, network));
		var node = new JobNodeModel(jobNodeService);
		var port1 = new NetworkPortModel(node, NetworkPortModel.PortType.Input);
		var port2 = new NetworkPortModel(node, NetworkPortModel.PortType.Output);
		Assert.That(port1.CanAttachTo(port2), Is.False);
	}

	[Test]
	public void TestDuplicateLink()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity() { JobNumber = "job1" };
		var entity2 = new Entity() { JobNumber = "job2" };
		network.Entities.Add(entity1);
		network.Entities.Add(entity2);
		network.CreateRelationship(entity1, entity2);

		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);

		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var jobNodeModels = diagram.FindComponents<JobNode>().Select(c => c.Instance.Node!).OrderBy(n => n.JobNumber).ToList();
		Assert.That(jobNodeModels[0].Output.CanAttachTo(jobNodeModels[1].Input), Is.False);
		Assert.That(jobNodeModels[1].Input.CanAttachTo(jobNodeModels[0].Output), Is.False);
	}

	[Test]
	public async Task TestOverwriteResourceDependencyLinkAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var factory = new BusinessObjectFactory();
			var diagram = NetworkTestCase.CreateDiagram(factory);
			var shape1 = NetworkTestCase.CreateShape(diagram, "Shape 1");
			var shape2 = NetworkTestCase.CreateShape(diagram, "Shape 2");

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite).ExecuteForShapes(shape1, shape2);
			refresher.AssociateWithNetwork(network);

			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var jobNodeModels = rendered.FindComponents<JobNode>().Select(c => c.Instance.Node!).OrderBy(n => n.DiagramName).ToList();

		Assert.That(jobNodeModels[0].Output.CanAttachTo(jobNodeModels[1].Input), Is.True);
		Assert.That(jobNodeModels[1].Input.CanAttachTo(jobNodeModels[0].Output), Is.True);
	}

	[Test]
	public void TestOkCompatibilityImageOnMouseEnter()
	{
		using var diagram = new NCNDiagramModelForTest(Renderer);
		diagram.TryCanAttachToInProgressLinkReturnValue = true;

		var port = RenderComponent<NetworkPort>(parameters =>
		{
			parameters.Add(p => p.Port, new NetworkPortModel(new NodeModel(), NetworkPortModel.PortType.Input));
			parameters.Add(p => p.Diagram, diagram);
		});

		port.Find(".networkport__circle").MouseEnter();

		var img = port.Find("img");
		Assert.That(img.Attributes["src"]?.Value, Does.EndWith("tick_16.png"));
	}

	[Test]
	public void TestBadCompatibilityImageOnMouseEnter()
	{
		using var diagram = new NCNDiagramModelForTest(Renderer);
		diagram.TryCanAttachToInProgressLinkReturnValue = false;

		var port = RenderComponent<NetworkPort>(parameters =>
		{
			parameters.Add(p => p.Port, new NetworkPortModel(new NodeModel(), NetworkPortModel.PortType.Input));
			parameters.Add(p => p.Diagram, diagram);
		});

		port.Find(".networkport__circle").MouseEnter();

		var img = port.Find("img");
		Assert.That(img.Attributes["src"]?.Value, Does.EndWith("block_16.svg"));
	}

	[Test]
	public void TestNoCompatibilityImageOnMouseEnter()
	{
		using var diagram = new NCNDiagramModelForTest(Renderer);
		diagram.TryCanAttachToInProgressLinkReturnValue = null;

		var port = RenderComponent<NetworkPort>(parameters =>
		{
			parameters.Add(p => p.Port, new NetworkPortModel(new NodeModel(), NetworkPortModel.PortType.Input));
			parameters.Add(p => p.Diagram, diagram);
		});

		port.Find(".networkport__circle").MouseEnter();

		Assert.That(() => port.Find("img"), Throws.TypeOf<ElementNotFoundException>());
	}

	[Test]
	public void TestNoCompatibilityImageOnMouseLeave()
	{
		using var diagram = new NCNDiagramModelForTest(Renderer);
		diagram.TryCanAttachToInProgressLinkReturnValue = true;

		var port = RenderComponent<NetworkPort>(parameters =>
		{
			parameters.Add(p => p.Port, new NetworkPortModel(new NodeModel(), NetworkPortModel.PortType.Input));
			parameters.Add(p => p.Diagram, diagram);
		});

		port.Find(".networkport__circle").MouseLeave();

		Assert.That(() => port.Find("img"), Throws.TypeOf<ElementNotFoundException>());
	}

	[Test, WithPlaywrightPage]
	public async Task TestNetworkPortsHavePointerCursorInAllStatesAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var entity1 = new EntityForTest()
		{
			Name = "From Entity",
			X = 100,
			Y = 100,
			Height = 200,
			Width = 200,
		};
		var entity2 = new EntityForTest()
		{
			Name = "To Entity",
			X = 400,
			Y = 200,
			Height = 200,
			Width = 200,
		};
		var network = new NetworkBuilder().WithEntities(entity1,entity2).Build();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Width = 1250, Height = 700 };
			var networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			networkUserControl.Width = 1250;
			networkUserControl.Height = 700;
			networkUserControl.Link(entity1, entity2);

			form.Controls.Add(networkUserControl);
			return form;
		});

		var portsLocator = page
			.GetByTitle("Entity")
			.GetByTitle("Port");

		Assert.That(portsLocator.CountAsync, Is.EqualTo(4));

		foreach (var portLocator in await portsLocator.AllAsync())
		{
			await portLocator.HoverAsync(new LocatorHoverOptions() { Force = true });

			Assert.That(await portLocator.GetComputedStyleAsync("cursor"), Is.EqualTo("pointer"));

			var box = (await portLocator.BoundingBoxAsync())!;

			Assert.That(box, Is.Not.Null);

			var elementUnderCursor = (await Page.EvaluateHandleAsync(
					$"() => document.elementFromPoint({box.X + box.Width / 2}, {box.Y + box.Height / 2})"
				))
				.AsElement()!;

			Assert.That(await elementUnderCursor.GetComputedStyleAsync("cursor"), Is.EqualTo("pointer"));
		}
	}

	class NCNDiagramModelForTest : NCNDiagramModel
	{
		public NCNDiagramModelForTest(ITestRenderer renderer)
			: base(new ObservableCollection<NodeViewModel>(), new ObservableCollection<ConnectionViewModel>(), new NetworkUserControlForTest(renderer))
		{
		}

		internal override bool? TryCanAttachToInProgressLink(NetworkPortModel port) => TryCanAttachToInProgressLinkReturnValue;

		public bool? TryCanAttachToInProgressLinkReturnValue { private get; set; }
	}
}
