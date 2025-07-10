using System;
using System.Windows.Forms.Integration;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.GUI;

public static class WinFormsHost
{
	/// <summary>
	/// Entry point to Load a new Network Diagrams.
	/// This is used by Network Diagrams along with all of its entities - <see cref="IDiagramEntity" />,<see cref="INetworkRefresher"/>, <see cref="NodeViewModelProvider"/>, <see cref="IRibbonDataProvider"/>, <see cref="ElementHost"/>
	/// </summary>
	/// <param name="hostControl">The WPF control that will host the NetworkUserControl.</param>
	/// <param name="diagramEntity">A network entity that describes if this is a scaled diagram. If it is a scaled diagram- it defines the scale, resolution, non-scheduled section's visibility, etc.</param>
	/// <param name="networkProvider">Interface for the network - has a refresh token, used to refresh/load the network</param>
	public static NetworkUserControl HostNetworkUserControlInWinForms(ElementHost hostControl, IDiagramEntity diagramEntity, INetworkRefresher networkProvider)
	{
		return HostNetworkUserControlInWinForms(hostControl, diagramEntity, networkProvider, null, null, null);
	}
	/// <summary>
	/// Entry point to Load/Open Network Diagrams.
	/// Here we pass on the Diagram entity <see cref="IDiagramEntity" /> along with the host <see cref="ElementHost"/> and all the other providers like network <see cref="INetworkRefresher"/>, nodeViewModel <see cref="NodeViewModelProvider"/> and RibbonData <see cref="IRibbonDataProvider"/> so that the control is visible.
	/// Any exception while loading the Network Diagram is also caught here.
	/// </summary>
	/// <param name="hostControl">The WPF control that will host the NetworkUserControl.</param>
	/// <param name="diagramEntity">A network entity that describes if this is a scaled diagram. If it is a scaled diagram- it defines the scale, resolution, non-scheduled section's visibility, etc.</param>
	/// <param name="networkProvider">Interface for the network - has a refresh token, used to refresh/load the network</param>
	/// <param name="selector"> An optional parameter for selection logic, this is null for WINZOR</param>
	/// <param name="viewModelProvider">The NodeViewModel to define the Shape/Annotation</param>
	/// <param name="ribbonDataProvider">An Interface for the Ribbon ViewModel</param>
	public static NetworkUserControl HostNetworkUserControlInWinForms(ElementHost hostControl, IDiagramEntity diagramEntity, INetworkRefresher networkProvider, object selector, NodeViewModelProvider viewModelProvider, IRibbonDataProvider ribbonDataProvider)
	{
		try
		{
			var control = new NetworkUserControl(diagramEntity, networkProvider, viewModelProvider, ribbonDataProvider);
			control.SetDataContext(networkProvider.GetReloadedNetwork(), isReloading: false);
			HostControlInWinForms(hostControl, control);
			return control;
		}
		catch (Exception ex)
		{
			ExceptionReporter.Instance.ReportDeveloperException((NoResString)"Failed to load the NCN", ex);
			throw;
		}
	}

	static void HostControlInWinForms(ElementHost hostControl, NetworkUserControl wpfControl)
	{
		hostControl.Child = wpfControl;
		hostControl.VisibleChanged += (object sender, EventArgs e) =>
		{
			wpfControl.Visible = hostControl.Visible;
		};
	}
}
