using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWiseOne.ResourceStrings;
using Moq;

namespace NetworkVisualisation.GUI.Winzor.Test.Scaffolding;

[DebuggerDisplay("PK: {EntityPK}", Name = "{Name}")]
public class EntityForTest : IDiagramEntity, IScheduledNetworkEntity
{
	public virtual event PropertyChangedEventHandler? PropertyChanged;

	public virtual bool IsSameEntity(IProposedNetworkEntity other)
	{
		if (other is INetworkEntity otherEntity)
		{
			return otherEntity.EntityPK == EntityPK;
		}
		return false;
	}

	public virtual string Name { get; set; } = "";
	public virtual string JobNumber { get; set; } = "";
	public virtual string JobName { get; set; } = "";
	public virtual bool JobName_ReadOnly { get; set; }
	public virtual string Description { get; set; } = "";

#pragma warning disable CW1178 // Do Not Invoke Old Res.GetString Methods; for tests that don't require translation
	public virtual string CompletionCriteria { get; set; } =
		Res.GetString("0677e975-d2fa-4e27-9cdd-8ce06e8834cc", "Completion Criteria");
#pragma warning restore CW1178 // Do Not Invoke Old Res.GetString Methods; for tests that don't require translation

	public virtual WorkStatus Status { get; set; } = WorkStatus.None;
	public virtual string StatusName { get; set; } = "";
	public virtual string StatusDescription { get; set; } = "";
	public virtual bool IsStartable { get; set; }
	public virtual string EstimateSummary { get; set; } = "";
	public virtual bool IsOnCriticalPath { get; set; }
	public virtual bool CanUnlinkEntity { get; set; }

	EntityForTest? parent;
	public virtual EntityForTest? Parent
	{
		get => parent;
		set
		{
			parent?.children.Remove(this);
			parent = value;
			value?.children.Add(this);
		}
	}
	IProposedNetworkEntity? IProposedNetworkEntity.Parent => Parent;

	public virtual IEnumerable<IEntityRelationship> Links { get; set; } = Array.Empty<IEntityRelationship>();

	public virtual IEnumerable<IEntityRelationship> PreRequisiteLinks { get; set; } = Array.Empty<IEntityRelationship>();

	public virtual IEnumerable<IEntityRelationship> PostRequisiteLinks { get; set; } = Array.Empty<IEntityRelationship>();

	public virtual bool CanDeleteUnderlyingEntity { get; set; }
	public virtual bool IsHandledByVisualiser { get; set; } = true;
	public virtual bool Equals(INetworkEntity? other) => this == other;
	public virtual bool CanCreateRelationship(INetworkEntity other) => false;

	public virtual void SwapNonScheduledState()
	{
	}

	public virtual IDisposable SuspendSettingHasChanges() => Mock.Of<IDisposable>();

	public virtual Guid EntityPK { get; set; } = Guid.NewGuid();
	public virtual string ShapeType { get; set; } = WinzorShapeTypes.Shape;
	public virtual EntityState EntityState { get; set; } = EntityState.None;
	public virtual double X { get; set; }
	public virtual double Y { get; set; }
	public virtual double Width { get; set; }
	public virtual double Height { get; set; }
	public virtual int ZIndex { get; set; }
	public virtual int CornerRadius { get; set; }
	public virtual Color BackColor { get; set; }
	public virtual Color ForeColor { get; set; }
	public virtual bool IsLeafEntity { get; set; }
	public virtual bool HasLinkedEntity { get; set; }
	public virtual bool IsLinkedToWorkflow { get; set; }
	public virtual bool IsPositioned { get; set; }
	public virtual bool IsNonScheduled { get; set; }
	public virtual bool IsDiagramWithRibbon { get; set; }
	public virtual bool IsDeleted { get; set; }
	public virtual string AdditionalDetail { get; set; } = "";
	public virtual string AdditionalDetailTooltip { get; set; } = "";
	public virtual string Notes { get; set; } = "";

	public virtual IObservableReloadableCollection<IAffinity> AvailableAffinities { get; set; } =
		new ImpObservableCollection<IAffinity>();

	public virtual IObservableReloadableCollection<IAffinity> AppliedAffinities { get; set; } =
		new ImpObservableCollection<IAffinity>();

	public virtual bool HasNotifications { get; set; }

	public virtual IEnumerable<IEntityNotification> EntityNotifications { get; set; } =
		Array.Empty<IEntityNotification>();

	public virtual NetworkActions SupportedActions { get; set; } = NetworkActions.StyleDiagram;
	public virtual bool IsCalculationSuspended { get; set; }

	readonly List<INetworkEntity> children = new List<INetworkEntity>();

	public virtual IEnumerable<INetworkEntity> Children {
		get => children;
		set
		{
			foreach (var entity in children)
			{
				if (entity is EntityForTest tEntity)
				{
					tEntity.Parent = null;
				}
			}
			children.Clear();
			foreach (var entity in value)
			{
				if (entity is EntityForTest tEntity)
				{
					tEntity.Parent = this;
				}
				else
				{
					children.Add(entity);
				}
			}
		}
	}

	public virtual INetworkPin? Pin { get; set; }
	public virtual bool CanHaveChildren { get; set; }

	public virtual IObservableReloadableCollection<IProposedNetworkEntity> HiddenEntities { get; set; } =
		new ImpObservableCollection<IProposedNetworkEntity>();

	public virtual IObservableReloadableCollection<IEntityRelationship> HiddenRelationships { get; set; } =
		new ImpObservableCollection<IEntityRelationship>();

	public virtual void CreateAffinityLink(INetworkEntity networkEntity, IAffinity affinity)
	{
	}

	public virtual void RemoveAffinityLink(INetworkEntity networkEntity, IAffinity affinity)
	{
	}
	public virtual bool IsDiagramScaled { get; set; }
	public virtual int Scale { get; set; } = 60;
	public virtual int ResolutionIncrement { get; set; } = 60;
	public virtual int ScaleUnitPixelSize { get; set; } = 100;
	public virtual bool IsDiagramSurfaceFixed { get; set; }
	public virtual bool ShouldShowNonScheduledSection { get; set; } = true;
	public virtual double NonScheduledSectionWidth { get; set; }
	public virtual bool ShapeInspectorVisible { get; set; }
	public virtual ScrollPositionStates ScrollPositions { get; set; }
	public virtual IEnumerable<IDiagramChannel> DiagramChannels { get; set; } = Array.Empty<IDiagramChannel>();
#pragma warning disable CW1050
	public virtual int ExplicitDurationMinutes { get; set; }
	public virtual int RemainingDurationMinutes { get; set; }
#pragma warning restore CW1050
	public virtual DateTime ScheduledStartTimeLocal { get; set; }
	public virtual DateTime ScheduledEndTimeLocal { get; set; }
	public virtual bool IsCriticalPath { get; set; }

	public void TriggerPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
