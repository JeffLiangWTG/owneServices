using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;

internal class NetworkBuilder
{
	IDiagramEntity? diagramEntity;
	IEnumerable<INetworkEntity>? entities;
	IEnumerable<IEntityRelationship>? relationships;
	IEnumerable<INetworkAction>? customActions;

	public NetworkForTest Build()
	{
		return new ()
		{
			DiagramEntity = diagramEntity ?? Stub.Entity(),
			Entities = new ImpObservableCollection<INetworkEntity>(entities ?? Enumerable.Empty<INetworkEntity>()),
			Relationships = new List<IEntityRelationship>(relationships ?? Enumerable.Empty<IEntityRelationship>()),
			CustomNetworkActions = new List<INetworkAction>(customActions ?? Enumerable.Empty<INetworkAction>()),
		};
	}

	public NetworkBuilder WithDiagramEntity(IDiagramEntity value)
	{
		diagramEntity = value;
		return this;
	}

	public NetworkBuilder WithEntities(IEnumerable<INetworkEntity> value)
	{
		entities = value;
		return this;
	}

	public NetworkBuilder WithEntities(params INetworkEntity[] value)
	{
		return WithEntities(value.AsEnumerable());
	}

	public NetworkBuilder WithRelationships(IEnumerable<IEntityRelationship> value)
	{
		relationships = value;
		return this;
	}

	public NetworkBuilder WithCustomActions(params INetworkAction[] value)
	{
		customActions = value;
		return this;
	}
}
