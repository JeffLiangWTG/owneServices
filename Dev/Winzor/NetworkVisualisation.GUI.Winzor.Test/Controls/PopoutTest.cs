using System.Drawing;
using System.Windows.Forms;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.Winzor.Architecture.Test;
using Moq;
using WinzorTestFramework;

namespace NetworkVisualisation.GUI.Winzor.Test.Controls;

public class PopoutTest : BunitTestContext
{
	static async Task<NetworkUserControl> CreateParentAsync(EnterpriseTestContext ctx)
	{
		var network = new DummyNetwork();

		var a = new Entity() { X = 100, Y = 100 };
		var b = new Entity() { Height = 500, Width = 500 };
		var c = new Entity();
		var d = new Entity();
		network.Entities.Add(a);
		network.Entities.Add(b);
		network.Entities.Add(c);
		network.Entities.Add(d);
		network.CreateRelationship(a, b);
		NetworkUserControl? control = null;
		var refresher = new NetworkRefresher();
		refresher.AssociateWithNetwork(network);
		network.Refresher = refresher;
		await ctx.RenderControlOnFormAsync(() =>
		{
			control = new NetworkUserControl(network.DiagramEntity, refresher);
			control.SetDataContext(network, false);
			return control;
		});
		return control!;
	}

	static async Task<NetworkUserControl> CreatePopoutAsync(EnterpriseTestContext ctx, NetworkUserControl parent)
	{
		var cut = await ctx.ShowFormInTestContextAsync(parent.PopOut);
		var result = (NetworkUserControl)cut.GetForm().ActiveControl!;
		Assert.That(result, Is.Not.SameAs(parent));
		return result;
	}

	[Test]
	public async Task TestPopoutInitialisesWithTheSameStateAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var parent = await CreateParentAsync(ctx);
		var parentName = parent.ViewModel!.DiagramEntity.Name;
		var parentNodes = parent.NetworkViewModel!.Nodes;
		var parentConnections = parent.NetworkViewModel.Connections;
		Assert.That(parentName, Is.EqualTo("Dummy Diagram Entity"));
		Assert.That(parentNodes.Count(), Is.EqualTo(4));
		Assert.That(parentConnections.Count(), Is.EqualTo(1));

		var parentFirstNode = parentNodes.First();
		var parentSecondNode = parentNodes.Skip(1).First();

		Assert.That(parentConnections.Single().Relationship.From, Is.EqualTo(parentFirstNode.Entity));
		Assert.That(parentConnections.Single().Relationship.To, Is.EqualTo(parentSecondNode.Entity));

		var popout = await CreatePopoutAsync(ctx, parent);
		Assert.That(popout, Is.Not.Null);
		var popupName = popout.ViewModel!.DiagramEntity.Name;
		var popupNodes = parent.NetworkViewModel.Nodes;
		var popupConnections = parent.NetworkViewModel.Connections;
		Assert.That(popupName, Is.EqualTo(parentName));
		Assert.That(popupNodes.Count(), Is.EqualTo(4));
		foreach (var (parentNode, popoutNode) in parentNodes.Zip(popupNodes))
		{
			Assert.That(parentNode.Entity.IsSameEntity(popoutNode.Entity), Is.True);
			Assert.That(popoutNode.X, Is.EqualTo(parentNode.X));
			Assert.That(popoutNode.Y, Is.EqualTo(parentNode.Y));
			Assert.That(popoutNode.Height, Is.EqualTo(parentNode.Height));
			Assert.That(popoutNode.Width, Is.EqualTo(parentNode.Width));
		}
		Assert.That(popupConnections.Count(), Is.EqualTo(1));
		Assert.That(popupConnections.Single().Relationship.From.IsSameEntity(parentConnections.Single().Relationship.From), Is.True);
		Assert.That(popupConnections.Single().Relationship.To.IsSameEntity(parentConnections.Single().Relationship.To), Is.True);

		// Note: these values are estimates
		Assert.That((popout.ParentForm!.Width, popout.ParentForm.Height), Is.EqualTo((1250, 700)));
	}

	[Test]
	public async Task TestPopoutDoesNotInheritSelectionAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var parent = await CreateParentAsync(ctx);
		await parent.InvokeWinzorDispatcherAsync(() => parent.NetworkViewModel!.SelectEntities(parent.NetworkViewModel.Network.Entities));
		Assert.That(parent.NetworkViewModel!.SelectedNodes.Count(), Is.EqualTo(4));

		var popout = await CreatePopoutAsync(ctx, parent);
		Assert.That(popout.NetworkViewModel!.SelectedNodes.Count(), Is.EqualTo(0));
	}

	[Test]
	public async Task TestPopoutFromAPopoutAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var parent = await CreateParentAsync(ctx);
		var popout = await CreatePopoutAsync(ctx, parent);
		var subPopout = await CreatePopoutAsync(ctx, popout);
	}

	[Test]
	public async Task TestCloseAllChildrenClosesAllPopoutsAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var parent = await CreateParentAsync(ctx);
		var popout = await CreatePopoutAsync(ctx, parent);
		var subPopoutA = await CreatePopoutAsync(ctx, popout);
		var subPopoutB = await CreatePopoutAsync(ctx, popout);
		var subPopoutC = await CreatePopoutAsync(ctx, subPopoutB);

		Assert.That(popout.IsDisposed, Is.False);
		Assert.That(subPopoutA.IsDisposed, Is.False);
		Assert.That(subPopoutB.IsDisposed, Is.False);
		Assert.That(subPopoutC.IsDisposed, Is.False);

		await parent.InvokeWinzorDispatcherAsync(() => parent.CloseAllChildren());

		Assert.That(popout.IsDisposed, Is.True);
		Assert.That(subPopoutA.IsDisposed, Is.True);
		Assert.That(subPopoutB.IsDisposed, Is.True);
		Assert.That(subPopoutC.IsDisposed, Is.True);
	}

	[Test]
	public async Task TestClosingAnyPopoutClosesAllPopoutsAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var parent = await CreateParentAsync(ctx);
		var popout = await CreatePopoutAsync(ctx, parent);
		var subPopoutA = await CreatePopoutAsync(ctx, popout);
		var subPopoutB = await CreatePopoutAsync(ctx, popout);
		var subPopoutC = await CreatePopoutAsync(ctx, subPopoutB);

		Assert.That(popout.IsDisposed, Is.False);
		Assert.That(subPopoutA.IsDisposed, Is.False);
		Assert.That(subPopoutB.IsDisposed, Is.False);
		Assert.That(subPopoutC.IsDisposed, Is.False);

		await subPopoutA.InvokeWinzorDispatcherAsync(() => subPopoutA.ParentForm!.Close());

		Assert.That(popout.IsDisposed, Is.True);
		Assert.That(subPopoutA.IsDisposed, Is.True);
		Assert.That(subPopoutB.IsDisposed, Is.True);
		Assert.That(subPopoutC.IsDisposed, Is.True);

		Assert.That(parent.IsDisposed, Is.False);
	}

	[Test]
	public async Task TestShapeInspectorVisibilitySyncsWithPopoutAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var parent = await CreateParentAsync(ctx);
		var popout = await CreatePopoutAsync(ctx, parent);
		await parent.InvokeWinzorDispatcherAsync(() => popout.NetworkViewModel!.Network.Refresh(RefreshType.ShapeInspectorVisibilityChanged));
		Assert.That(popout.ViewModel!.ShapeInspectorVisible, Is.True);
		Assert.That(parent.ViewModel!.ShapeInspectorVisible, Is.True);
	}

	[Test]
	public async Task TestNodePositionSyncsWithoutRefreshAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var parent = await CreateParentAsync(ctx);
		var popout = await CreatePopoutAsync(ctx, parent);
		var parentNode = parent.NetworkViewModel!.Nodes.First();
		Assert.That(parentNode.X, Is.Not.Zero);
		Assert.That(parentNode.Y, Is.Not.Zero);
		await parent.InvokeWinzorDispatcherAsync(() =>
		{
			parentNode.X *= 2;
			parentNode.Y *= 2;
		});
		var popoutNode = popout.NetworkViewModel!.Nodes.First();
		Assert.That(popoutNode.X, Is.EqualTo(parentNode.X));
		Assert.That(popoutNode.Y, Is.EqualTo(parentNode.Y));
	}

	[Test]
	public async Task TestNodeSizeSyncsWithoutRefreshAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var parent = await CreateParentAsync(ctx);
		var popout = await CreatePopoutAsync(ctx, parent);
		var parentNode = parent.NetworkViewModel!.Nodes.First();
		Assert.That(parent.Width, Is.Not.Zero);
		Assert.That(parentNode.Height, Is.Not.Zero);
		await parent.InvokeWinzorDispatcherAsync(() =>
		{
			parentNode.Width *= 2;
			parentNode.Height *= 2;
		});
		var popoutNode = popout.NetworkViewModel!.Nodes.First();
		Assert.That(popoutNode.X, Is.EqualTo(parentNode.X));
		Assert.That(popoutNode.Y, Is.EqualTo(parentNode.Y));
	}

	[Test]
	public async Task TestDeleteNodeSyncsOnEntityRemovedRefreshAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var parent = await CreateParentAsync(ctx);
		var popout = await CreatePopoutAsync(ctx, parent);
		var parentNode = parent.NetworkViewModel!.Nodes.First();
		Assert.That(parent.NetworkViewModel.Nodes.Count(), Is.EqualTo(4));
		Assert.That(popout.NetworkViewModel!.Nodes.Count(), Is.EqualTo(4));
		await parent.InvokeWinzorDispatcherAsync(() =>
		{
			parent.NetworkViewModel.DeleteNode(parentNode);
			parent.NetworkViewModel.Network.Refresh(RefreshType.EntityRemoved, parentNode.Entity);
		});
		Assert.That(parent.NetworkViewModel.Nodes.Count(), Is.EqualTo(3));
		Assert.That(popout.NetworkViewModel.Nodes.Count(), Is.EqualTo(3));
	}

	[Test]
	public async Task TestDeleteLinkDoesNotSyncWithRelationshipRemovedRefreshAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var parent = await CreateParentAsync(ctx);
		var popout = await CreatePopoutAsync(ctx, parent);
		Assert.That(parent.NetworkViewModel!.Connections.Count(), Is.EqualTo(1));
		Assert.That(popout.NetworkViewModel!.Connections.Count(), Is.EqualTo(1));
		await parent.InvokeWinzorDispatcherAsync(() =>
		{
			var connection = parent.NetworkViewModel.Connections.First();
			parent.NetworkViewModel.Network.DeleteRelationship(connection.Relationship);
			parent.NetworkViewModel.Network.Refresh(RefreshType.RelationshipRemoved, connection.Relationship.From as INetworkEntity, connection.Relationship.To as INetworkEntity);
			parent.NetworkViewModel.RemoveConnection(connection);
		});
		Assert.That(parent.NetworkViewModel.Connections.Count(), Is.EqualTo(0));
		Assert.That(popout.NetworkViewModel.Connections.Count(), Is.EqualTo(1));
	}

	[Test]
	public async Task TestAddLinkDoesNotSyncWithRelationshipAddedRefreshAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var parent = await CreateParentAsync(ctx);
		var popout = await CreatePopoutAsync(ctx, parent);
		Assert.That(parent.NetworkViewModel!.Connections.Count(), Is.EqualTo(1));
		Assert.That(popout.NetworkViewModel!.Connections.Count(), Is.EqualTo(1));
		await parent.InvokeWinzorDispatcherAsync(() =>
		{
			var first = parent.NetworkViewModel.Nodes.First();
			var last = parent.NetworkViewModel.Nodes.Last();
			var relationship = parent.NetworkViewModel.Network.CreateRelationship(first.Entity, last.Entity);
			parent.NetworkViewModel.Network.Refresh(RefreshType.RelationshipAdded, first.Entity, last.Entity);
			parent.NetworkViewModel.AddConnection(new ConnectionViewModel()
			{
				SourceConnector = first.OutputConnectors[0],
				DestConnector = last.InputConnectors[0],
				Relationship = relationship
			});
		});
		Assert.That(parent.NetworkViewModel.Connections.Count(), Is.EqualTo(2));
		Assert.That(popout.NetworkViewModel.Connections.Count(), Is.EqualTo(1));
	}

	[Test]
	public async Task TestPopoutFormTextIsNetworkModelNameAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var parent = await CreateParentAsync(ctx);
		await parent.InvokeWinzorDispatcherAsync(() => parent.ViewModel!.NetworkModel.Name = "Foo Bar");
		var popout = await CreatePopoutAsync(ctx, parent);
		Assert.That(popout.FindForm()?.Text, Is.EqualTo("Foo Bar"));
	}

	[Test]
	public async Task TestPopoutBackgroundColorAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var parent = await CreateParentAsync(ctx);
		var popout = await CreatePopoutAsync(ctx, parent);
		Assert.That(popout.FindForm()!.BackColor, Is.EqualTo(SystemColors.Control));
	}
}
