using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.Integration;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;

namespace NetworkVisualisation.GUI.Winzor.Test.Extensions;

[Obsolete("Use a pragma disable if still using INetworkUserControl")]
public static class INetworkUserControlExtensions
{
	public static void Add(this INetworkUserControl networkUserControl, INetworkEntity iEntity)
	{
		networkUserControl.NetworkViewModel.Network.Entities.Add(iEntity);
		networkUserControl.NetworkViewModel.SetDefaultsForNewNode(iEntity, null, true);
		networkUserControl.NetworkViewModel.Network.Refresh(RefreshType.EntityAdded, iEntity);
	}
	public static void Add(this INetworkUserControl networkUserControl, EntityForTest entity)
		=> networkUserControl.Add(iEntity: entity);

	public static void AddRange(this INetworkUserControl networkUserControl, IEnumerable<INetworkEntity> entities)
	{
		foreach (var entity in entities)
		{
			networkUserControl.Add(entity);
		}
	}

	public static void AddRange(this INetworkUserControl networkUserControl, params INetworkEntity[] entityArgs)
		=> networkUserControl.AddRange(entities: entityArgs);

	public static void AddRange(this INetworkUserControl networkUserControl, bool scheduled, params INetworkEntity[] entityArgs)
		=> networkUserControl.AddRange(entities: entityArgs);

	public static void Link(this INetworkUserControl networkUserControl, INetworkEntity a, INetworkEntity b)
	{
		var relationship = networkUserControl.NetworkViewModel.Network.CreateRelationship(a, b) ?? throw new InvalidOperationException("Failed to create relationship.");

		var viewModelA = networkUserControl.ViewModelFor(a);
		var viewModelB = networkUserControl.ViewModelFor(b);

		if (viewModelA is null)
		{
			throw new InvalidOperationException($"Entity {nameof(a)} not found in network.");
		}
		if (viewModelB is null)
		{
			throw new InvalidOperationException($"Entity {nameof(b)} not found in network.");
		}

		var connection = new ConnectionViewModel()
		{
			SourceConnector = viewModelA.OutputConnectors[0],
			DestConnector = viewModelB.InputConnectors[0],
			Relationship = relationship
		};

		networkUserControl.NetworkViewModel.AddConnection(connection);
	}

	public static void Unlink(this INetworkUserControl networkUserControl, INetworkEntity a, INetworkEntity b)
	{
		var connection = networkUserControl.NetworkViewModel.Connections
			.Single(x => Equals(x.Relationship?.From, a) && Equals(x.Relationship?.To, b));
		networkUserControl.NetworkViewModel.Network.DeleteRelationship(connection.Relationship);
		networkUserControl.NetworkViewModel.RemoveConnection(connection);
	}

	public static bool AreLinked(this INetworkUserControl networkUserControl, INetworkEntity a, INetworkEntity b)
		=> networkUserControl.NetworkViewModel.Connections
			.Any(x => Equals(x.Relationship?.From, a) && Equals(x.Relationship?.To, b));

	public static void Remove(this INetworkUserControl networkUserControl, NodeViewModel nodeViewModel)
		=> networkUserControl.NetworkViewModel.DeleteNode(nodeViewModel);

	public static void Remove(this INetworkUserControl networkUserControl, INetworkEntity entity)
	{
		var viewModel = networkUserControl.ViewModelFor(entity) ?? throw new InvalidOperationException("Entity not found in network");
		networkUserControl.Remove(viewModel);
	}

	public static void RemoveRange(this INetworkUserControl networkUserControl, IEnumerable<INetworkEntity> entities)
	{
		foreach (var entity in entities)
		{
			networkUserControl.Remove(entity);
		}
	}

	public static void RemoveRange(this INetworkUserControl networkUserControl, params INetworkEntity[] entityArgs)
		=> networkUserControl.RemoveRange(entities: entityArgs);

	public static INetworkEntity? GetEntity(this INetworkUserControl networkUserControl, Guid id)
		=> networkUserControl.NetworkViewModel.Network.Entities.FirstOrDefault(entity => entity.EntityPK == id);

	public static NodeViewModel? ViewModelFor(this INetworkUserControl networkUserControl, INetworkEntity entity)
		=> networkUserControl.NetworkViewModel.GetNodeForEntity(entity);
}

public static class NetworkUserControlExtensions
{
	public static IEnumerable<INetworkEntity> GetEntities(this NetworkUserControl control)
		=> control.NetworkViewModel.Network.Entities;
	public static void Add(this NetworkUserControl control, INetworkEntity iEntity)
	{
		control.NetworkViewModel.Network.Entities.Add(iEntity);
		control.NetworkViewModel.SetDefaultsForNewNode(iEntity, null, centerNode: true);
		control.NetworkViewModel.Network.Refresh(RefreshType.EntityAdded, iEntity);
	}
	public static void Add(this NetworkUserControl control, EntityForTest entity)
		=> control.Add(iEntity: entity);

	public static void AddRange(this NetworkUserControl control, IEnumerable<INetworkEntity> entities)
	{
		foreach (var entity in entities)
		{
			control.Add(entity);
		}
	}

	public static void AddRange(this NetworkUserControl control, params INetworkEntity[] entityArgs)
		=> control.AddRange(entities: entityArgs);

	public static void Link(this NetworkUserControl control, INetworkEntity a, INetworkEntity b)
	{
		var relationship = control.NetworkViewModel.Network.CreateRelationship(a, b) ?? throw new InvalidOperationException("Failed to create relationship.");

		var viewModelA = control.ViewModelFor(a);
		var viewModelB = control.ViewModelFor(b);

		if (viewModelA is null)
		{
			throw new InvalidOperationException($"Entity {nameof(a)} not found in network.");
		}
		if (viewModelB is null)
		{
			throw new InvalidOperationException($"Entity {nameof(b)} not found in network.");
		}

		var connection = new ConnectionViewModel()
		{
			SourceConnector = viewModelA.OutputConnectors[0],
			DestConnector = viewModelB.InputConnectors[0],
			Relationship = relationship
		};

		control.NetworkViewModel.AddConnection(connection);
	}

	public static void Unlink(this NetworkUserControl control, INetworkEntity a, INetworkEntity b)
	{
		var connection = control.NetworkViewModel.Connections
			.Single(x => Equals(x.Relationship?.From, a) && Equals(x.Relationship?.To, b));
		control.NetworkViewModel.Network.DeleteRelationship(connection.Relationship);
		control.NetworkViewModel.RemoveConnection(connection);
	}

	public static bool AreLinked(this NetworkUserControl control, INetworkEntity a, INetworkEntity b)
		=> control.NetworkViewModel.Connections
			.Any(x => Equals(x.Relationship?.From, a) && Equals(x.Relationship?.To, b));

	public static void Remove(this NetworkUserControl control, NodeViewModel nodeViewModel)
		=> control.NetworkViewModel.DeleteNode(nodeViewModel);

	public static void Remove(this NetworkUserControl control, INetworkEntity entity)
	{
		var viewModel = control.ViewModelFor(entity) ?? throw new InvalidOperationException("Entity not found in network");
		control.Remove(viewModel);
	}

	public static void RemoveRange(this NetworkUserControl control, IEnumerable<INetworkEntity> entities)
	{
		foreach (var entity in entities)
		{
			control.Remove(entity);
		}
	}

	public static void RemoveRange(this NetworkUserControl control, params INetworkEntity[] entityArgs)
		=> control.RemoveRange(entities: entityArgs);

	public static INetworkEntity? GetEntity(this NetworkUserControl control, Guid id)
		=> control.GetEntities().FirstOrDefault(entity => entity.EntityPK == id);

	public static NodeViewModel? ViewModelFor(this NetworkUserControl control, INetworkEntity entity)
		=> control.NetworkViewModel.GetNodeForEntity(entity);

	public static NCNDiagramModel DiagramModelFor(this NetworkUserControl control, INetworkEntity entity)
	{
		if (entity.IsNonScheduled && control.NonScheduledDiagramControl is not null)
		{
			return control.NonScheduledDiagramControl.DiagramModel ?? throw new InvalidOperationException();
		}
		return control.MainDiagramControl.DiagramModel ?? throw new InvalidOperationException();
	}

	public static INetworkEntity? EntityFor(this NetworkUserControl control, NetworkNodeModel node)
		=> control.GetEntities().FirstOrDefault(entity => entity.EntityPK == node.EntityPK);

	public static T? ModelFor<T>(this NetworkUserControl control, INetworkEntity entity)
		where T : NetworkNodeModel
	{
		var result = control.DiagramModelFor(entity).Nodes
			.OfType<NetworkNodeModel>()
			.SingleOrDefault(x => x.EntityPK == entity.EntityPK);
		return result is null ? null : (T)result;
	}

	public static NetworkNodeModel? ModelFor(this NetworkUserControl control, INetworkEntity entity)
		=> control.ModelFor<NetworkNodeModel>(entity);

	public static NetworkLinkModel? LinkBetween(this NetworkUserControl control, INetworkEntity a, INetworkEntity b)
		=> control.DiagramModelFor(a).Links
			.OfType<NetworkLinkModel>()
			.SingleOrDefault(x => x.SourceEntityPK == a.EntityPK && x.TargetEntityPK == b.EntityPK);
}

