using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Extensions;
using CargoWise.NetworkVisualisation.GUI.Services.Events;
using CargoWise.NetworkVisualisation.GUI.Services.ProgressBarModel;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;

namespace CargoWise.NetworkVisualisation.GUI.Services.NetworkNode;

public class NetworkNodeService : INetworkNodeService
{
	protected readonly NodeViewModel viewModel;
	protected readonly IWinzorDispatch winzorDispatcher;

	public event EventHandler<PropertyValueChangedEventArgs> PropertyValueChanged;

	public NetworkNodeService(NodeViewModel viewModel, IWinzorDispatch winzorDispatcher)
	{
		this.viewModel = viewModel;
		this.viewModel.PropertyChanged += OnPropertyValueChanged;
		this.winzorDispatcher = winzorDispatcher;
	}

	public void Dispose()
	{
		viewModel.PropertyChanged -= OnPropertyValueChanged;
	}

	IProgressBarModelService progressBarService;
	public IProgressBarModelService GetProgressBarService() => progressBarService ??= new ProgressBarModelService(viewModel);

	#region INetworkNodeViewModel

	public NetworkNodeData GetNetworkNodeData()
	{
		winzorDispatcher.AssertInWinzorThread();

		return new NetworkNodeData
		{
			EntityPK = viewModel.Entity.EntityPK,
			DiagramName = viewModel.DiagramName,
			Notes = viewModel.Notes,
			NotesPlaceholder = viewModel.NotesPlaceholder,
			ToolTip = viewModel.ToolTip,
			DeleteTooltip = viewModel.DeleteTooltip,
			ZIndex = viewModel.ZIndex,
			IsSelected = viewModel.IsSelected,
			HasNotifications = viewModel.Entity.HasNotifications,
			HasNotificationsForState = GetHasNotificationsForState(viewModel.Entity.EntityState),
			X = viewModel.X,
			Y = viewModel.Y,
			Width = viewModel.Width,
			Height = viewModel.Height,
			StatusColors = viewModel.StatusColors,
			Entity = viewModel.Entity,
			EntityState = viewModel.Entity.EntityState,
			EntityStateTooltip = GetTooltipForEntityState(viewModel.Entity.EntityState),
			IsLocked = GetIsLocked(viewModel.Entity.EntityState),
			StatusTextWeight = viewModel.StatusTextWeight,
			ForegroundColor = viewModel.ForegroundColor,
			HasProgressBar = viewModel.LowerBar != null,
		};
	}

	public string GetTooltipForEntityState(EntityState entityState) => viewModel.Entity.GetTooltipForEntityState(entityState);

	public async Task MoveAsync(double x, double y)
	{
		await winzorDispatcher.InvokeWinzorDispatcherAsync(() =>
		{
			Batch(() =>
			{
				viewModel.X = Math.Max(x, 0);
				viewModel.Y = Math.Max(y, 0);
				viewModel.Network.Refresh(RefreshType.EntityCoordinates);
				viewModel.OnDragCompleted();
			});

			// position may be adjusted for scaled diagrams without notifications
			OnPropertyValueChanged(this, new PropertyChangedEventArgs(nameof(NodeViewModel.X)));
			OnPropertyValueChanged(this, new PropertyChangedEventArgs(nameof(NodeViewModel.Y)));
		});
	}

	public async Task ResizeAsync(double x, double y, double width, double height)
	{
		await winzorDispatcher.InvokeWinzorDispatcherAsync(() =>
		{
			using (viewModel.NetworkViewModel.PerformNodeResize())
			{
				Batch(() =>
				{
					viewModel.X = Math.Max(x, 0);
					viewModel.Y = Math.Max(y, 0);
					viewModel.Width = Math.Max(width, NodeViewModel.MinAllowedNodeWidth);
					viewModel.Height = Math.Max(height, NodeViewModel.MinAllowedNodeHeight);
					viewModel.Network.Refresh(RefreshType.EntityCoordinates);
					viewModel.OnResizeCompleted();
				});
			}

			// position may be adjusted for scaled diagrams without notifications
			OnPropertyValueChanged(this, new PropertyChangedEventArgs(nameof(NodeViewModel.X)));
			OnPropertyValueChanged(this, new PropertyChangedEventArgs(nameof(NodeViewModel.Y)));
		});
	}

	public async Task ShowViewAsync()
	{
		await winzorDispatcher.InvokeWinzorDispatcherAsync(() => viewModel.NetworkViewModel.Network.ViewEntity(viewModel.Entity));
	}

	public async Task UpdateNotesAsync(string notes)
	{
		await winzorDispatcher.InvokeWinzorDispatcherAsync(() => viewModel.Notes = notes);
	}

	public async Task UpdateIsSelectedAsync(bool selected)
	{
		await winzorDispatcher.InvokeWinzorDispatcherAsync(() => viewModel.IsSelected = selected);
	}

	public async Task UpdateDiagramNameAsync(string diagramName)
	{
		await winzorDispatcher.InvokeWinzorDispatcherAsync(() => viewModel.DiagramName = diagramName);
	}

	public async Task<IEnumerable<NetworkActionMenuItem>> GetContextMenuItemsAsync()
	{
		IEnumerable<NetworkActionMenuItem> items = null;
		await winzorDispatcher.InvokeWinzorDispatcherAsync(() =>
		{
			viewModel.MenuItems.Reload();
			items = viewModel.MenuItems.ToImmutableArray();
		});
		return items ?? Array.Empty<NetworkActionMenuItem>();
	}

	static bool GetHasNotificationsForState(EntityState state)
	{
		return state.HasFlag(EntityState.HasWarnings)
			|| state.HasFlag(EntityState.HasMessages)
			|| state.HasFlag(EntityState.HasErrors);
	}

	static bool GetIsLocked(EntityState state) => state.HasFlag(EntityState.Fixed);

	#endregion

	#region OnPropertyValueChanged

	readonly Dictionary<string, Func<NodeViewModel, object>> propertiesMap = new()
	{
		// Network Data
		{ nameof(NodeViewModel.X),                             (vm) => vm.X },
		{ nameof(NodeViewModel.Y),                             (vm) => vm.Y },
		{ nameof(NodeViewModel.Width),                         (vm) => vm.Width },
		{ nameof(NodeViewModel.Height),                        (vm) => vm.Height },
		{ nameof(NodeViewModel.DiagramName),                   (vm) => vm.DiagramName },
		{ nameof(NodeViewModel.StatusTextWeight),              (vm) => vm.StatusTextWeight },
		{ nameof(NodeViewModel.Notes),                         (vm) => vm.Notes },
		{ nameof(NodeViewModel.NotesPlaceholder),              (vm) => vm.NotesPlaceholder },
		{ nameof(NodeViewModel.ForegroundColor),               (vm) => vm.ForegroundColor },
		{ nameof(NodeViewModel.StatusColors),                  (vm) => vm.StatusColors },
		{ nameof(NodeViewModel.ToolTip),                       (vm) => vm.ToolTip },
		{ nameof(NodeViewModel.DeleteTooltip),                 (vm) => vm.DeleteTooltip },
		{ nameof(NodeViewModel.ZIndex),                        (vm) => vm.ZIndex },
		{ nameof(NodeViewModel.IsSelected),                    (vm) => vm.IsSelected },
		{ nameof(NodeViewModel.Entity.HasNotifications),       (vm) => vm.Entity.HasNotifications },
		{ nameof(NodeViewModel.Entity.EntityState),            (vm) => vm.Entity.EntityState },
		{ nameof(NetworkNodeData.HasProgressBar),              (vm) => vm.LowerBar != null },
		// Linkable Data
		{ nameof(NodeViewModel.Entity.IsOnCriticalPath),       (vm) => vm.Entity.IsOnCriticalPath },
		// Job Data
		{ nameof(NodeViewModel.JobName),                       (vm) => vm.JobName },
		{ nameof(NodeViewModel.JobNumber),                     (vm) => vm.JobNumber },
		{ nameof(NodeViewModel.JobNumberReadableText),         (vm) => vm.JobNumberReadableText },
		{ nameof(NodeViewModel.AppliedAttributesReadableText), (vm) => vm.AppliedAttributesReadableText },
		{ nameof(NodeViewModel.DurationReadableText),          (vm) => vm.DurationReadableText },
		{ nameof(NodeViewModel.CompletionCriteria),            (vm) => vm.CompletionCriteria },
		{ nameof(NodeViewModel.CompletionCriteriaPlaceholder), (vm) => vm.CompletionCriteriaPlaceholder },
		{ nameof(NodeViewModel.CompletionCriteriaTextColor),   (vm) => vm.CompletionCriteriaTextColor },
		{ nameof(NodeViewModel.ShowScheduleDetails),           (vm) => vm.ShowScheduleDetails },
		{ nameof(NodeViewModel.ShowJobBar),                    (vm) => vm.ShowJobBar },
		{ nameof(NodeViewModel.ShowJobBarAffinities),          (vm) => vm.ShowJobBarAffinities },
		{ nameof(NodeViewModel.HasLinkedEntity),               (vm) => vm.HasLinkedEntity },
		{ nameof(NodeViewModel.StartDateReadableText),         (vm) => vm.StartDateReadableText },
		{ nameof(NodeViewModel.RemainingDurationReadableText), (vm) => vm.RemainingDurationReadableText },
		{ nameof(NodeViewModel.FinishDateReadableText),        (vm) => vm.FinishDateReadableText },
		{ nameof(NodeViewModel.DurationTooltip),               (vm) => vm.DurationTooltip },
		{ nameof(NodeViewModel.StartDateToolTip),              (vm) => vm.StartDateToolTip },
		{ nameof(NodeViewModel.RemainingDurationToolTip),      (vm) => vm.RemainingDurationToolTip },
		{ nameof(NodeViewModel.FinishDateToolTip),             (vm) => vm.FinishDateToolTip },
		{ nameof(NodeViewModel.JobNumberTooltip),              (vm) => vm.JobNumberTooltip },
		{ nameof(NodeViewModel.AppliedAttributesTooltip),      (vm) => vm.AppliedAttributesTooltip },
		{ nameof(NetworkNodeData.EntityStateTooltip),          (vm) => vm.Entity.GetTooltipForEntityState(vm.Entity.EntityState) },
		{ nameof(NetworkNodeData.HasNotificationsForState),    (vm) => GetHasNotificationsForState(vm.Entity.EntityState) },
		{ nameof(NetworkNodeData.IsLocked),                    (vm) => GetIsLocked(vm.Entity.EntityState) },
		// Buffer Data
		{ nameof(NodeViewModel.Entity.AdditionalDetail),       (vm) => vm.Entity.AdditionalDetail },
		{ nameof(BufferViewModel.PenetrationPercentLabel),     (vm) => (vm as BufferViewModel)?.PenetrationPercentLabel },
		// Job and Buffer Data
		{ nameof(NodeViewModel.Description),                   (vm) => vm.Description },
		{ nameof(NodeViewModel.StatusTooltip),                 (vm) => vm.StatusTooltip },
		{ nameof(NodeViewModel.Status),                        (vm) => vm.Status },
	 };

	readonly Dictionary<string, string[]> impactedPropertiesMap = new()
	{
		{
			nameof(NodeViewModel.Y), new []
			{
				nameof(NodeViewModel.StatusColors),
			}
		},
		{
			nameof(NodeViewModel.Entity.EntityState), new []
			{
				nameof(NetworkNodeData.EntityStateTooltip),
				nameof(NetworkNodeData.HasNotificationsForState),
				nameof(NetworkNodeData.IsLocked),
			}
		},
		{
			nameof(NodeViewModel.Entity.HasNotifications), new []
			{
				nameof(NodeViewModel.Entity.EntityState),
			}
		},
		{
			nameof(NodeViewModel.JobName), new []
			{
				nameof(NodeViewModel.HasLinkedEntity),
				nameof(NodeViewModel.ShowJobBar),
				nameof(NodeViewModel.ShowJobBarAffinities),
				nameof(NodeViewModel.DeleteTooltip),
			}
		},
		{
			nameof(NodeViewModel.LowerBar), new []
			{
				nameof(NetworkNodeData.HasProgressBar),
			}
		}
	};

	void OnPropertyValueChanged(object s, PropertyChangedEventArgs e)
	{
		if (s is ShapeNetworkEntity shapeNetworkEntity && shapeNetworkEntity.ShapeSafe == null)
		{
			return;
		}

		if (suspendPropertyChanged)
		{
			suspendedEventArgs.Enqueue(e); // to be notified when suspension is resumed
			return;
		}

		if (propertiesMap.TryGetValue(e.PropertyName, out var getValueFrom))
		{
			PropertyValueChanged?.Invoke(s, new PropertyValueChangedEventArgs(e.PropertyName, getValueFrom(viewModel)));
		}

		if (impactedPropertiesMap.TryGetValue(e.PropertyName, out var impactedProperties))
		{
			impactedProperties.ForEach(propertyName => OnPropertyValueChanged(s, new PropertyChangedEventArgs(propertyName)));
		}
	}

	#endregion

	#region Batch

	readonly Queue<PropertyChangedEventArgs> suspendedEventArgs = new();

	bool suspendPropertyChanged;
	void Batch(Action action)
	{
		var suspend = suspendPropertyChanged;

		try
		{
			suspendPropertyChanged = true;
			action();
		}
		finally
		{
			suspendPropertyChanged = suspend;
			NotifySuspendedChanges();
		}
	}

	void NotifySuspendedChanges()
	{
		while (suspendedEventArgs.TryDequeue(out var e))
		{
			OnPropertyValueChanged(this, e);
		}
	}

	#endregion
}
