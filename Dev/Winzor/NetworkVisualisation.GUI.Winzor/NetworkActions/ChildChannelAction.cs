using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.GUI;

public class ChildChannelAction : DynamicGuiNetworkAction
{
	readonly IDiagramChannel channel;

	public ChildChannelAction(IDiagramChannel channel, int channelIndex, INetworkViewModel networkViewModel, NetworkUserControl control, int group = 0, int groupIndex = 0)
		: base(networkViewModel, control, new CommonNetworkActionExecutionStrategy(), group, groupIndex)
	{
		this.channel = channel;
	}

	protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
	{
		Control.MainDiagramControl.OnChannelSelected(channel);
		NetworkViewModel.Network.Refresh(RefreshType.ChannelView);
		return null;
	}

	protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("91d627d6-3fc9-4d57-853b-6fef3db5a9f6", "Select a Channel and display the selected channel only");

	protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("7911b946-d626-4944-8ea8-c7bdbc8a66a", "{0}", channel.Name);

	protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

	protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

	protected override bool ShouldRecalculateAccessibilityOnModelChanged(RefreshArgs args) => false;

	protected override bool ShouldRecalculateNameAndDescriptionOnModelChanged(RefreshArgs args) => false;

	protected override bool ShouldRecalculateIsActivatedOnModelChanged(RefreshArgs args) => true;

	protected override bool ShouldRecalculateChildActionsOnModelChanged(RefreshArgs args) => false;
}
