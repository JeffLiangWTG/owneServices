using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.GUI;

public class FreezeTimeLabelsAction : DynamicGuiNetworkAction
{
	public FreezeTimeLabelsAction(INetworkViewModel networkViewModel, NetworkUserControl control, INetworkActionExecutionStrategy executionStrategy = null, int group = 0, int groupIndex = 0)
		: base(networkViewModel, control, new CommonNetworkActionExecutionStrategy(), group, groupIndex)
	{
	}

	protected override string IconName => "BottomPanelClose";

	protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
	{
		Control?.ToggleFreezeTimeLabels();
		NetworkViewModel.Network.Refresh(RefreshType.ToogleFreezeChannelHeadersAndTimeLabels);
		return null;
	}

	protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("49a213d5-4a66-4bfc-ad61-45d103b2bec0", "Freeze time labels so they will stay in view while scrolling. This feature is only available in 100% zoom.");

	protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("8db11b83-9b72-4707-a226-609e6d7ecd3b", "Toggle Freeze Time Labels");

	protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity)
	{
		return (Control?.IsDiagramScaled() == true && Control?.ViewModel.ContentScale == 1) ? NetworkActionAccessibility.Allowed :
			new NetworkActionAccessibility(false, entity, () =>
			(NoResString)"Diagram is Not Scaled Diagram or Diagram Zoom is not 100%");
	}

	protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

	protected override bool IsActivatedCore(INetworkEntity activeEntity)
	{
		return Control?.IsTimeLabelsFrozen() ?? false;
	}

	protected override bool GetDefaultIsActivatedCore()
	{
		return false;
	}

	protected override bool ShouldRecalculateAccessibilityOnModelChanged(RefreshArgs args) => true;

	protected override bool ShouldRecalculateNameAndDescriptionOnModelChanged(RefreshArgs args) => false;

	protected override bool ShouldRecalculateIsActivatedOnModelChanged(RefreshArgs args) => true;

	protected override bool ShouldRecalculateChildActionsOnModelChanged(RefreshArgs args) => false;
}
