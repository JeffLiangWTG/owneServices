#nullable enable
using System;
using System.Drawing;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Services.Events;
using CargoWise.NetworkVisualisation.GUI.Services.ProgressBarModel;
using CargoWise.NetworkVisualisation.GUI.Services.Utils;

namespace CargoWise.NetworkVisualisation.GUI.Models;

/// <summary>
/// This is the model for Progress Bar component, containing data and behavior for displaying progress bar in a network node.
/// </summary>
public class ProgressBarModel
{
	/// <summary>
	/// Event triggered when any Progress Bar property is changed.
	/// </summary>
	public event Action? Changed;

	/// <summary>
	/// Constructor for ProgressBarModel, it initializes the properties of progress bar model based on the provided service.
	/// </summary>
	/// <param name="service">Service abstraction of Business Object</param>
	public ProgressBarModel(IProgressBarModelService service)
	{
		Service = service;
		Data = Service.GetProgressBarModelData();

		Service.PropertyValueChanged += OnPropertyValueChanged;
	}

	IProgressBarModelService Service { get; }
	ProgressBarModelData Data { get; }

	#region Properties
	/// <summary>
	/// Height of the Progress Bar
	/// </summary>
	public int Height
	{
		get => Data.LowerBarHeight;
		set
		{
			if (Height != value)
			{
				Data.LowerBarHeight = value;
				NotifyPropertyChanged();
			}
		}
	}

	/// <summary>
	/// Progress percentage of the Progress Bar.
	/// </summary>
	public double PercentProgress
	{
		get => Data.Percent;
		set
		{
			if (PercentProgress != value)
			{
				Data.Percent = value;
				NotifyPropertyChanged();
			}
		}
	}

	/// <summary>
	/// Background color of the Progress Bar.
	/// </summary>
	public Color Background
	{
		get => Data.Background;
		set
		{
			if (Background != value)
			{
				Data.Background = value;
				NotifyPropertyChanged();
			}
		}
	}

	/// <summary>
	/// Tooltip to be shown on the Progress Bar.
	/// </summary>
	public string Tooltip
	{
		get => Data.LowerBarTooltip;
		set
		{
			if (Tooltip != value)
			{
				Data.LowerBarTooltip = value;
				NotifyPropertyChanged();
			}
		}
	}
	#endregion

	/// <summary>
	/// This is used to release event handlers for Progress Bar.
	/// </summary>
	public void Dispose()
	{
		Service.PropertyValueChanged -= OnPropertyValueChanged;
		Service.Dispose();
	}

	void NotifyPropertyChanged() => Changed?.Invoke();

	#region OnPropertyValueChanged

	void OnPropertyValueChanged(object? sender, PropertyValueChangedEventArgs e)
	{
		if (TryUpdateProperty(e))
		{
			NotifyPropertyChanged();
		}
	}

	bool TryUpdateProperty(PropertyValueChangedEventArgs e) => e.PropertyName switch
	{
		nameof(ProgressBarModelData.LowerBarTooltip) => Properties.TryUpdate(
			newValue:     (string)e.Value,
			currentValue: Data.LowerBarTooltip,
			set:          (value) => Data.LowerBarTooltip = value),
		nameof(NodeViewModel.LowerBarHeight) => Properties.TryUpdate((int)e.Value, Data.LowerBarHeight, (value) => Data.LowerBarHeight = value),
		nameof(ProgressBarModelData.Percent) => Properties.TryUpdate((double)e.Value, Data.Percent, (value) => Data.Percent = value),
		nameof(ProgressBarModelData.Background) => Properties.TryUpdate((Color)e.Value, Data.Background, (value) => Data.Background = value),
		_ => false,
	};

	#endregion
}
