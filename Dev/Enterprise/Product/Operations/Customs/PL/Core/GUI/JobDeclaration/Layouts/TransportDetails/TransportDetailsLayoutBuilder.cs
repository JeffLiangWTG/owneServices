using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.GUI;

public class TransportDetailsLayoutBuilder : EU.GUI.Declaration.TransportDetailsLayoutBuilder<JobDeclaration>
{
	protected override void SetDefaultVisibilities()
	{
		base.SetDefaultVisibilities();

		var plBag = TransportDetailsControlBag.Instance;
		SetVisibility(plBag.TransportInlandRailUserControl, x => x.IsUCC6 && x.IsRailInland, x => x.JE_TransportModeInlandInfo, x => x.JE_MessageTypeInfo);
		SetVisibility(plBag.VesselUserControl, x => x.IsSea, x => x.JE_TransportModeInfo);
		SetVisibility(CommonBag.TransportInlandIDAndNationalityUserControl, x => x.IsFixedInstallationInland || x.IsOwnPropulsionInland || x.IsMailInland, x => x.JE_TransportModeInlandInfo, x => x.JE_MessageTypeInfo);
	}

	protected override bool AdditionalWagonNumbersUserControlVisibility(JobDeclaration declration) => declration.IsUCC6 && declration.IsRailInland && declration.IsExport;
	protected override bool InlandModeOfTransportDropEditVisibility(JobDeclaration declaration) => true;
}
