using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Components;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.Winzor.Architecture.Test;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;

namespace NetworkVisualisation.GUI.Winzor.Test.Components;

public class NetworkLinkTest : BunitTestContext
{
	[Test]
	public void TestDeleteIconRendered()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity();
		var entity2 = new Entity();
		network.Entities.Add(entity1);
		network.Entities.Add(entity2);
		network.CreateRelationship(entity1, entity2);

		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);

		var control = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, control);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		Assert.That(diagram.Find(".networklink > foreignObject > .deleteicon"), Is.Not.Null);
	}

	[Test]
	public void TestDeleteIconTooltipForLink()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity();
		var entity2 = new Entity();
		network.Entities.Add(entity1);
		network.Entities.Add(entity2);
		network.CreateRelationship(entity1, entity2);

		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);

		var control = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, control);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var deleteIconComponent = diagram.Find(".networklink .deleteicon__deletecircle");

		var connection = networkModel.Connections.First();

		Assert.That(deleteIconComponent.GetAttribute("title"), Is.EqualTo(connection.DeleteTooltip));
	}

	[Test]
	public async Task LinkColorAppliedCorrectlyAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var factory = new BusinessObjectFactory();
			var diagram = NetworkTestCase.CreateDiagram(factory);
			var shape1 = NetworkTestCase.CreateShape(diagram, "Shape 1");
			var shape2 = NetworkTestCase.CreateShape(diagram, "Shape 2");
			var shape3 = NetworkTestCase.CreateShape(diagram, "Shape 3");
			var shape4 = NetworkTestCase.CreateShape(diagram, "Shape 4");
			var shape5 = NetworkTestCase.CreateShape(diagram, "Shape 5");

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite).ExecuteForShapes(shape1, shape2);

			var relationship2 = network.CreateRelationship(shape2, shape3);

			var relationship3 = network.CreateRelationship(shape3, shape4);
			network.SwitchToScaled();
			networkViewModel.ToggleApproval();
			relationship3.AsAttachment().Decouple();

			var relationship4 = network.CreateRelationship(shape4, shape5);
			refresher.AssociateWithNetwork(network);

			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var links = rendered.FindComponents<NetworkLink>().OrderBy(l => l.Instance.Link.DisplayText).ToList();

		var resourceDependencyLink = links[0];
		var dependencyLink = links[1];
		var decoupledLink = links[2];
		var unApprovedLink = links[3];

		Assert.That(resourceDependencyLink.Find("path").GetAttribute("stroke"), Is.EqualTo("orange"));
		Assert.That(resourceDependencyLink.Find("polygon").GetAttribute("style"), Does.Contain("fill:orange;"));
		Assert.That(resourceDependencyLink.Instance.Link.BackColor, Is.EqualTo("Orange"));

		Assert.That(dependencyLink.Find("path").GetAttribute("stroke"), Is.EqualTo("black"));
		Assert.That(dependencyLink.Find("polygon").GetAttribute("style"), Does.Contain("fill:black;"));
		Assert.That(dependencyLink.Instance.Link.BackColor, Is.EqualTo("Black"));

		Assert.That(decoupledLink.Find("path").GetAttribute("stroke"), Is.EqualTo("gray"));
		Assert.That(decoupledLink.Find("polygon").GetAttribute("style"), Does.Contain("fill:gray;"));
		Assert.That(decoupledLink.Instance.Link.BackColor, Is.EqualTo("Gray"));

		Assert.That(unApprovedLink.Find("path").GetAttribute("stroke"), Is.EqualTo("black"));
		Assert.That(unApprovedLink.Find("polygon").GetAttribute("style"), Does.Contain("fill:black;"));
		Assert.That(unApprovedLink.Instance.Link.BackColor, Is.EqualTo("Black"));
	}

	[Test]
	public async Task LinkVisibilityAppliedCorrectlyAsync()
	{
		using var ctx = new EnterpriseTestContext();

		NetworkAttachment visibleRelationship = null!;
		NetworkAttachment hiddenRelationship = null!;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var factory = new BusinessObjectFactory();
			var diagram = NetworkTestCase.CreateDiagram(factory);
			var shape1 = NetworkTestCase.CreateShape(diagram, "Shape 1");
			var shape2 = NetworkTestCase.CreateShape(diagram, "Shape 2");
			var shape3 = NetworkTestCase.CreateShape(diagram, "Shape 3");

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			visibleRelationship = (NetworkAttachment)network.CreateRelationship(shape1, shape2);
			hiddenRelationship = (NetworkAttachment)network.CreateRelationship(shape2, shape3);
			hiddenRelationship.AsAttachment().BNA_IsHidden = true;
			refresher.AssociateWithNetwork(network);

			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var links = rendered.FindComponents<NetworkLink>().OrderBy(l => l.Instance.Link.DisplayText).ToList();

		var visibleLink = links[0];
		var hiddenLink = links[1];

		Assert.That(visibleLink.Find(".networklink").GetAttribute("display"), Is.EqualTo(string.Empty));
		Assert.That(visibleLink.Instance.Link.IsVisible, Is.True);
		Assert.That(visibleLink.Instance.Link.IsVisible, Is.EqualTo(visibleRelationship.IsVisible));

		Assert.That(hiddenLink.Find(".networklink").GetAttribute("display"), Is.EqualTo("none"));
		Assert.That(hiddenLink.Instance.Link.IsVisible, Is.False);
		Assert.That(hiddenLink.Instance.Link.IsVisible, Is.EqualTo(hiddenRelationship.IsVisible));
	}

	[Test]
	public async Task LinkAppearanceAppliedCorrectlyAsync()
	{
		using var ctx = new EnterpriseTestContext();

		NetworkAttachment normalRelationship = null!;
		NetworkAttachment decoupledRelationship = null!;
		NetworkAttachment unApprovedRelationship = null!;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var factory = new BusinessObjectFactory();
			var diagram = NetworkTestCase.CreateDiagram(factory);
			var shape1 = NetworkTestCase.CreateShape(diagram, "Shape 1");
			var shape2 = NetworkTestCase.CreateShape(diagram, "Shape 2");
			var shape3 = NetworkTestCase.CreateShape(diagram, "Shape 3");
			var shape4 = NetworkTestCase.CreateShape(diagram, "Shape 4");

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			normalRelationship = (NetworkAttachment)network.CreateRelationship(shape1, shape2);

			decoupledRelationship = (NetworkAttachment)network.CreateRelationship(shape2, shape3);
			network.SwitchToScaled();
			networkViewModel.ToggleApproval();
			decoupledRelationship.AsAttachment().Decouple();

			unApprovedRelationship = (NetworkAttachment)network.CreateRelationship(shape3, shape4);

			refresher.AssociateWithNetwork(network);

			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var links = rendered.FindComponents<NetworkLink>().OrderBy(l => l.Instance.Link.DisplayText).ToList();

		var normalLink = links[0];
		var decoupledLink = links[1];
		var unApprovedLink = links[2];

		Assert.That(normalLink.Find("path").GetAttribute("stroke-dasharray"), Is.EqualTo(string.Empty));
		Assert.That(normalLink.Instance.Link.Appearance, Is.EqualTo(ArrowAppearance.Normal));
		Assert.That(normalLink.Instance.Link.Appearance, Is.EqualTo(normalRelationship.Appearance));

		Assert.That(decoupledLink.Find("path").GetAttribute("stroke-dasharray"), Is.EqualTo("4 8"));
		Assert.That(decoupledLink.Instance.Link.Appearance, Is.EqualTo(ArrowAppearance.Dotted));
		Assert.That(decoupledLink.Instance.Link.Appearance, Is.EqualTo(decoupledRelationship.Appearance));

		Assert.That(unApprovedLink.Find("path").GetAttribute("stroke-dasharray"), Is.EqualTo("10 4"));
		Assert.That(unApprovedLink.Instance.Link.Appearance, Is.EqualTo(ArrowAppearance.Dashed));
		Assert.That(unApprovedLink.Instance.Link.Appearance, Is.EqualTo(unApprovedRelationship.Appearance));
	}

	[Test]
	public async Task LinkTitleAppliedCorrectlyAsync()
	{
		using var ctx = new EnterpriseTestContext();

		NetworkAttachment resourceDependencyRelationship = null!;
		NetworkAttachment normalRelationship = null!;
		NetworkAttachment decoupledRelationship = null!;
		NetworkAttachment unApprovedRelationship = null!;
		string resourceDependencyRelationshipDisplayText = string.Empty;
		string normalRelationshipDisplayText = string.Empty;
		string decoupledRelationshipDisplayText = string.Empty!;
		string unApprovedRelationshipDisplayText = string.Empty;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var factory = new BusinessObjectFactory();
			var diagram = NetworkTestCase.CreateDiagram(factory);
			var shape1 = NetworkTestCase.CreateShape(diagram, "Shape 1");
			var shape2 = NetworkTestCase.CreateShape(diagram, "Shape 2");
			var shape3 = NetworkTestCase.CreateShape(diagram, "Shape 3");
			var shape4 = NetworkTestCase.CreateShape(diagram, "Shape 4");
			var shape5 = NetworkTestCase.CreateShape(diagram, "Shape 5");

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite).ExecuteForShapes(shape1, shape2);

			resourceDependencyRelationship = (NetworkAttachment)network.GetRelationship(shape1, shape2);

			normalRelationship = (NetworkAttachment)network.CreateRelationship(shape2, shape3);

			decoupledRelationship = (NetworkAttachment)network.CreateRelationship(shape3, shape4);
			network.SwitchToScaled();
			networkViewModel.ToggleApproval();
			decoupledRelationship.AsAttachment().Decouple();

			unApprovedRelationship = (NetworkAttachment)network.CreateRelationship(shape4, shape5);

			refresher.AssociateWithNetwork(network);

			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);

			resourceDependencyRelationshipDisplayText = resourceDependencyRelationship.DisplayText;
			normalRelationshipDisplayText = normalRelationship.DisplayText;
			decoupledRelationshipDisplayText = decoupledRelationship.DisplayText;
			unApprovedRelationshipDisplayText = unApprovedRelationship.DisplayText;

			return networkUserControl;
		});

		var links = rendered.FindComponents<NetworkLink>().OrderBy(l => l.Instance.Link.DisplayText).ToList();

		var resourceDependencyLink = links[0];
		var normalLink = links[1];
		var decoupledLink = links[2];
		var unApprovedLink = links[3];

		Assert.That(resourceDependencyLink.Find("title").TextContent, Does.Contain("Resource Dependency:"));
		Assert.That(resourceDependencyLink.Instance.Link.DisplayText, Does.Contain("Resource Dependency:"));
		Assert.That(resourceDependencyLink.Instance.Link.DisplayText, Is.EqualTo(resourceDependencyRelationshipDisplayText));

		Assert.That(normalLink.Find("title").TextContent, Is.EqualTo("Shape 2 -> Shape 3"));
		Assert.That(normalLink.Instance.Link.DisplayText, Is.EqualTo("Shape 2 -> Shape 3"));
		Assert.That(normalLink.Instance.Link.DisplayText, Is.EqualTo(normalRelationshipDisplayText));

		Assert.That(decoupledLink.Find("title").TextContent, Is.EqualTo("Shape 3 -> Shape 4 (this arrow has been decoupled)"));
		Assert.That(decoupledLink.Instance.Link.DisplayText, Is.EqualTo("Shape 3 -> Shape 4 (this arrow has been decoupled)"));
		Assert.That(decoupledLink.Instance.Link.DisplayText, Is.EqualTo(decoupledRelationshipDisplayText));

		Assert.That(unApprovedLink.Find("title").TextContent, Is.EqualTo("Shape 4 -> Shape 5 (this arrow has not been approved)"));
		Assert.That(unApprovedLink.Instance.Link.DisplayText, Is.EqualTo("Shape 4 -> Shape 5 (this arrow has not been approved)"));
		Assert.That(unApprovedLink.Instance.Link.DisplayText, Is.EqualTo(unApprovedRelationshipDisplayText));
	}
}
