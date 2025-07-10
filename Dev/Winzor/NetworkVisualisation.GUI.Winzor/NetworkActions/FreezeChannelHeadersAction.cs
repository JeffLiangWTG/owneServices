using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.GUI;

public class FreezeChannelHeadersAction : DynamicGuiNetworkAction
{
	public FreezeChannelHeadersAction(INetworkViewModel networkViewModel, NetworkUserControl control, int group = 0, int groupIndex = 0)
		: base(networkViewModel, control, new CommonNetworkActionExecutionStrategy(), group, groupIndex)
	{
	}

	protected override string IconName => "LeftPanelClose";

	protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
	{
		Control?.ToggleFreezeChannelHeaders();
		NetworkViewModel.Network.Refresh(RefreshType.ToogleFreezeChannelHeadersAndTimeLabels);
		return null;
	}

	protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("f6c4c451-ac62-4bd5-be9f-597405c95bd1", "Freeze channel headers so they will stay in view while scrolling");

	protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("2ba4d544-2f0d-4565-92f7-e256001fec19", "Toggle Freeze Channel Headers");

	protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity)
	{
		return Control?.IsDiagramScaled() == true ? NetworkActionAccessibility.Allowed : new NetworkActionAccessibility(false, entity, () => (NoResString)"Diagram is Not Scaled Diagram");
	}

	protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

	protected override bool IsActivatedCore(INetworkEntity activeEntity)
	{
		return Control?.IsChannelHeadersFrozen() ?? false;
	}

	protected override bool GetDefaultIsActivatedCore() => false;

	protected override bool ShouldRecalculateAccessibilityOnModelChanged(RefreshArgs args) => true;

	protected override bool ShouldRecalculateNameAndDescriptionOnModelChanged(RefreshArgs args) => false;

	protected override bool ShouldRecalculateIsActivatedOnModelChanged(RefreshArgs args) => true;

	protected override bool ShouldRecalculateChildActionsOnModelChanged(RefreshArgs args) => false;
}
