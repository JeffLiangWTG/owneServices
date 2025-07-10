#nullable enable
using CargoWise.ComponentModel;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.NetworkVisualisation.GUI;

/// <summary>
/// A child form that displays a <see cref="NetworkUserControl"/>.
/// </summary>
public class PopoutForm : ZChildForm
{
	public PopoutForm(NetworkUserControl control)
	{
		Text = string.IsNullOrEmpty(control.ViewModel?.NetworkModel.Name)
			? Res.GetString("1d7a1e68-87a3-43e8-a357-d3b7705438e6", "Untitled")
			: control.ViewModel.NetworkModel.Name;

		Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(1250);
		Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(700);

		Closed += (_, _) =>
		{
			var networkRefresher = control.ViewModel?.NetworkModel.Refresher;
			if (networkRefresher is not null)
			{
				networkRefresher.Refresh(RefreshType.Close);
				networkRefresher.Refresh(RefreshType.RefreshButton);
			}
		};

		Controls.Add(control);
	}
	protected override bool ShowStatusBar => false;

	protected override void UpdateStatusBar(string notification, INotificationType state)
	{
	}
}
