#nullable enable
using System;
using System.ComponentModel;
using Blazor.Diagrams.Core.Models.Base;
using CargoWise.NetworkVisualisation.GUI.Models;
using Microsoft.AspNetCore.Components;

namespace CargoWise.NetworkVisualisation.GUI.Components;

public abstract class NetworkNodeBase<T> : ComponentBase, IDisposable where T : NetworkNodeModel
{
	[Parameter]
	public T? Node { get; set; }

	protected override void OnInitialized()
	{
		if (Node is not null)
		{
			Node.PropertyChangedFromServer += OnPropertyChangedFromServer;
			Node.Changed += OnChanged;
		}
	}

	protected virtual void OnPropertyChangedFromServer(object? sender, PropertyChangedEventArgs e)
	{
	}

	void OnChanged(Model model)
	{
		InvokeAsync(StateHasChanged);
	}

	public virtual void Dispose()
	{
		if (Node is not null)
		{
			Node.PropertyChangedFromServer -= OnPropertyChangedFromServer;
			Node.Changed -= OnChanged;
		}
	}
}
