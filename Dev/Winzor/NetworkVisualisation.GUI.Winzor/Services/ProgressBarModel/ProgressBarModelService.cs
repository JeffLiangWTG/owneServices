using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using CargoWise.Common;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Services.Events;

namespace CargoWise.NetworkVisualisation.GUI.Services.ProgressBarModel;

public class ProgressBarModelService : IProgressBarModelService
{
	readonly NodeViewModel viewModel;

	public event EventHandler<PropertyValueChangedEventArgs> PropertyValueChanged;

	public ProgressBarModelService(NodeViewModel viewModel)
	{
		this.viewModel = viewModel;
		this.viewModel.PropertyChanged += OnPropertyValueChanged;
	}

	public ProgressBarModelData GetProgressBarModelData()
	{
		return new ProgressBarModelData
		{
			LowerBarHeight = viewModel.LowerBarHeight,
			Percent = viewModel.LowerBar?.percent ?? 0,
			Background = viewModel.LowerBar?.background ?? Color.Transparent,
			LowerBarTooltip = viewModel.LowerBarTooltip,
		};
	}

	public void Dispose()
	{
		viewModel.PropertyChanged -= OnPropertyValueChanged;
	}

	#region OnPropertyValueChanged

	readonly Dictionary<string, Func<NodeViewModel, object>> propertiesMap = new ()
	{
		// Progress Bar Data
		{ nameof(NodeViewModel.LowerBarHeight),    (vm) => vm.LowerBarHeight },
		{ nameof(NodeViewModel.LowerBarTooltip),   (vm) => vm.LowerBarTooltip },
		{ nameof(ProgressBarModelData.Percent),    (vm) => vm.LowerBar?.percent ?? 0 },
		{ nameof(ProgressBarModelData.Background), (vm) => vm.LowerBar?.background ?? Color.Transparent },
	};

	readonly Dictionary<string, string[]> impactedPropertiesMap = new ()
	{
		{
			nameof(NodeViewModel.LowerBar), new []
			{
				nameof(NodeViewModel.LowerBarHeight),
				nameof(ProgressBarModelData.Percent),
				nameof(ProgressBarModelData.Background),
			}
		}
	};

	void OnPropertyValueChanged(object s, PropertyChangedEventArgs e)
	{
		if (propertiesMap.TryGetValue(e.PropertyName, out var getValueFrom))
		{
			PropertyValueChanged?.Invoke(s, new PropertyValueChangedEventArgs(e.PropertyName, getValueFrom(viewModel)));
		}

		if (impactedPropertiesMap.TryGetValue(e.PropertyName, out var propertyNames))
		{
			propertyNames.ForEach(propertyName => OnPropertyValueChanged(s, new PropertyChangedEventArgs(propertyName)));
		}
	}

	#endregion
}
