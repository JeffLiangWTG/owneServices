using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.GUI;

public class ChannelViewAction : DynamicGuiNetworkAction
{
	IEnumerable<INetworkAction> createChannelViewActions;

	readonly IDiagramChannelsControl DiagramChannelsControl;
	public ChannelViewAction(INetworkViewModel networkViewModel, NetworkUserControl control, int group = 0, int groupIndex = 0)
		: base(networkViewModel, control, new CommonNetworkActionExecutionStrategy(), group, groupIndex)
	{
		DiagramChannelsControl = Control?.MainDiagramControl;
	}

	protected override string IconName => "ChannelView";

	protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
	{
		return null;
	}

	protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("29314066-2FB0-4E94-B304-E82F637843A9", "Select a Channel and display the selected channel only");

	protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("9D7BC1BC-96FC-47C1-B671-BE863A47719B", "Enable Channel View");

	protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity)
	{
		return (Control?.HasChannels() ?? false) ? NetworkActionAccessibility.Allowed : new NetworkActionAccessibility(false, entity, () => (NoResString)"Activate when channels added");
	}

	protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

	protected override bool GetDefaultIsActivatedCore()
	{
		return false;
	}

	protected override IEnumerable<INetworkAction> GetChildActionsCore(INetworkEntity activeEntity)
	{
		return createChannelViewActions ??= GetActions();
	}

	IEnumerable<INetworkAction> GetActions()
	{
		foreach (var (channel, index) in DiagramChannelsControl.Channels.Select((c, i) => (c, i)))
		{
			yield return new ChildChannelAction(channel, index, base.NetworkViewModel, base.Control);
		}
	}

	protected override bool ShouldRecalculateAccessibilityOnModelChanged(RefreshArgs args) => true;

	protected override bool ShouldRecalculateNameAndDescriptionOnModelChanged(RefreshArgs args) => false;

	protected override bool ShouldRecalculateIsActivatedOnModelChanged(RefreshArgs args) => true;

	protected override bool ShouldRecalculateChildActionsOnModelChanged(RefreshArgs args) => true;
}
