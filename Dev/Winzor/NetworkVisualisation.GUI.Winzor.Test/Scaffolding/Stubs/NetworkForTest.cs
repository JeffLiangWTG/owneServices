using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Moq;

namespace NetworkVisualisation.GUI.Winzor.Test.Scaffolding;

public class NetworkForTest : INetwork
{
	public bool DeleteEntity(IProposedNetworkEntity entity)
	{
		return Entities.Remove((INetworkEntity)entity);
	}

	public IEnumerable<IProposedNetworkEntity> HideEntity(IProposedNetworkEntity entity)
	{
		Entities.Remove((INetworkEntity)entity);
		HiddenEntities.Add(entity);

		return new[] { entity };
	}

	public virtual IEnumerable<INetworkEntity> ShowEntity(IProposedNetworkEntity entity, INetworkEntity parentEntity)
	{
		if (HiddenEntities.Contains(entity))
		{
			HiddenEntities.Remove(entity);
		}
		INetworkEntity proposedNetworkEntity = (INetworkEntity)entity;
		Entities.Add(proposedNetworkEntity);

		return new[] { proposedNetworkEntity };
	}

	public virtual bool DeleteRelationship(IEntityRelationship relationship)
	{
		foreach (var entity in Entities)
		{
			var existingRelationship = entity.Links.FirstOrDefault(r => r.From == relationship.From && r.To == relationship.To);
			if (existingRelationship != null)
			{
				((Entity)entity).Links.Remove(existingRelationship);
			}
		}

		return true;
	}

	public virtual IEntityRelationship CreateRelationship(IProposedNetworkEntity sourceEntity, IProposedNetworkEntity destEntity)
	{
		var relationship = new Relationship { From = sourceEntity, To = destEntity };
		Relationships.Add(relationship);
		return relationship;
	}

	public virtual bool HideRelationship(IEntityRelationship relationship)
	{
		if (relationship.From != null && relationship.From.Links.Contains(relationship))
		{
			((Entity)relationship.From).Links.Remove(relationship);
		}
		if (relationship.To != null && relationship.To.Links.Contains(relationship))
		{
			((Entity)relationship.To).Links.Remove(relationship);
		}

		HiddenRelationships.Add(relationship);

		return true;
	}

	public virtual IEntityRelationship? ShowRelationship(IProposedNetworkEntity sourceEntity, IProposedNetworkEntity destEntity)
	{
		var relationship = HiddenRelationships.FirstOrDefault(r => r.From == sourceEntity && r.To == destEntity);
		if (relationship != null)
		{
			HiddenRelationships.Remove(relationship);
		}

		return relationship;
	}

	public virtual IEntityRelationship? GetRelationship(IProposedNetworkEntity sourceEntity, IProposedNetworkEntity destEntity)
	{
		return Relationships.FirstOrDefault(r => r.From.IsSameEntity(sourceEntity) && r.To.IsSameEntity(destEntity));
	}

	public virtual bool TryHandlePaste(IEnumerable<INetworkEntity> selectedEntities)
	{
		throw new NotImplementedException();
	}

	public virtual void ModifyAffinities(IDiagramEntity diagramEntity)
	{
		throw new NotImplementedException();
	}

	public virtual void EditEntity(IProposedNetworkEntity entity)
	{
		throw new NotImplementedException();
	}

	public virtual void ViewEntity(IProposedNetworkEntity entity)
	{
		throw new NotImplementedException();
	}

	public virtual IEnumerable<INetworkEntity> PickAndImportEntities(IProposedNetworkEntity parentEntity)
	{
		throw new NotImplementedException();
	}

	public virtual IDisposable SuspendRefreshingOnEntityCountChanged()
	{
		return Mock.Of<IDisposable>();
	}

	public virtual IEnumerable<INetworkAction> GetCustomNetworkActions(INetworkViewModel networkViewModel)
	{
		return CustomNetworkActions;
	}

	public virtual IEnumerable<INetworkAction> GetCreateEntityActions(INetworkViewModel networkViewModel)
	{
		return CreateEntityActions;
	}

	public virtual bool TryCopyShapeStateToClipBoard(IEnumerable<INetworkEntity> shapeStates)
	{
		throw new NotImplementedException();
	}

	public virtual INetworkActionResult PasteShapeFromClipBoard(INetworkViewModel networkViewModel)
	{
		throw new NotImplementedException();
	}

	internal NetworkForTest AddRelationship(EntityForTest source, EntityForTest target)
	{
		var relationship = CreateRelationship(source, target);
		source.Links = new[] { relationship };
		source.PostRequisiteLinks = new[] { relationship };

		target.Links = new[] { relationship };
		target.PreRequisiteLinks = new[] { relationship };

		return this;
	}

	public virtual string Name { get; set; } = "A NetworkForTest";
	public virtual bool IsReadOnly { get; set; }
	public virtual IDiagramEntity DiagramEntity { get; set; } = new EntityForTest();

	public virtual IObservableReloadableCollection<INetworkEntity> Entities { get; set;  } =
		new ImpObservableSet<INetworkEntity>();

	public virtual INetworkScaleDescriptor? ScaleDescriptor { get; set; }
	public virtual IEntityPositionStrategy EntityPositionStrategy { get; set; } = new EntityPositionStrategy();
	public virtual INetworkEntityController? Controller { get; set; }
	public virtual INetworkRefresher? Refresher { get; set; }

	public IList<IProposedNetworkEntity> HiddenEntities { get; set; } =
		new List<IProposedNetworkEntity>();

	public IList<IEntityRelationship> HiddenRelationships { get; set; } = new List<IEntityRelationship>();

	public IList<IEntityRelationship> Relationships { get; set; } = new List<IEntityRelationship>();

	public IList<INetworkAction> CustomNetworkActions { get; set; } = new List<INetworkAction>();
	public IList<INetworkAction> CreateEntityActions { get; set; } = new List<INetworkAction>();

	public NetworkForTest()
	{
		var refresher = new NetworkRefresher();
		refresher.AssociateWithNetwork(this);
		Refresher = refresher;
	}
}
