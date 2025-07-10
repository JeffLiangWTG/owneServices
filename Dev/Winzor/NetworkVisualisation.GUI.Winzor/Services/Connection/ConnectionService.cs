using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Extensions;
using CargoWise.NetworkVisualisation.GUI.Services.Events;
using Enterprise.BufferManagement.NetworkVisualisation.Business;

namespace CargoWise.NetworkVisualisation.GUI.Services.Connection;

public class ConnectionService : IConnectionService
{
	public event EventHandler<PropertyValueChangedEventArgs> PropertyValueChanged;

	readonly ConnectionViewModel viewModel;
	readonly IWinzorDispatch dispatcher;

	public ConnectionService(ConnectionViewModel viewModel, IWinzorDispatch dispatcher)
	{
		this.viewModel = viewModel;
		this.dispatcher = dispatcher;

		if (this.viewModel.Relationship is NetworkAttachment nwa)
		{
			nwa.Attachment.PropertyChanged += OnPropertyValueChanged;
		}
	}

	public void Dispose()
	{
		if (viewModel.Relationship is NetworkAttachment nwa)
		{
			nwa.Attachment.PropertyChanged -= OnPropertyValueChanged;
		}
	}

	NetworkAttachment Attachment => viewModel.Relationship as NetworkAttachment;

	#region IConnectionViewModel

	public ConnectionData GetConnectionModelData()
	{
		dispatcher.AssertInWinzorThread();

		return new ConnectionData
		{
			DeleteTooltip = viewModel.DeleteTooltip,
			DisplayText = Attachment?.DisplayText,
			BackColor = Attachment?.BackColor,
			IsVisible = Attachment?.IsVisible ?? true,
			Appearance = Attachment?.Appearance ?? default,
			IsResourceDependency = Attachment?.IsResourceDependency ?? default,
		};
	}

	public Guid GetSourceConnectorPK()
	{
		dispatcher.AssertInWinzorThread();

		return viewModel.SourceConnector.ParentNode.Entity.EntityPK;
	}

	public Guid GetTargetConnectorPK()
	{
		dispatcher.AssertInWinzorThread();

		return viewModel.DestConnector.ParentNode.Entity.EntityPK;
	}

	#endregion

	#region OnPropertyValueChanged

	readonly Dictionary<string, Func<ConnectionViewModel, object>> propertiesMap = new()
	{
		// Attachment
		{ nameof(BMNCNAttachment.Appearance),           (vm) => (vm.Relationship as NetworkAttachment)?.Appearance ?? default },
		{ nameof(BMNCNAttachment.BackColor),            (vm) => (vm.Relationship as NetworkAttachment)?.BackColor },
		{ nameof(BMNCNAttachment.DisplayText),          (vm) => (vm.Relationship as NetworkAttachment)?.DisplayText },
		{ nameof(BMNCNAttachment.IsResourceDependency), (vm) => (vm.Relationship as NetworkAttachment)?.IsResourceDependency ?? default },
		{ nameof(BMNCNAttachment.IsVisible),            (vm) => (vm.Relationship as NetworkAttachment)?.IsVisible ?? true },
	};

	void OnPropertyValueChanged(object s, PropertyChangedEventArgs e)
	{
		if (propertiesMap.TryGetValue(e.PropertyName, out var getValueFrom))
		{
			PropertyValueChanged?.Invoke(s, new PropertyValueChangedEventArgs(e.PropertyName, getValueFrom(viewModel)));
		}
	}

	#endregion
}
