using Enterprise.Customs.Business;
using Enterprise.Customs.PL.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI;

public sealed partial class Phase5DeclarationAuthorizationsTabUserControl : ZUserControl
{
	public Phase5DeclarationAuthorizationsTabUserControl()
	{
		InitializeComponent();
		AuthorisationsGrid.FindBoxColumnModuleShowing += AuthorisationGrid_FindBoxColumnModuleShowing;
	}

	void AuthorisationGrid_FindBoxColumnModuleShowing(object sender, FindBoxColumnModuleShowingEventArgs e)
	{
		if (e.ColumnStyle is ZCodeFindBoxColumnStyle style && style.MappingName == nameof(CusAuthorizationUsage.AGC_Location))
		{
			style.PopupSelected -= Style_PopupSelected;
			style.PopupSelected += Style_PopupSelected;
		}
	}

	void Style_PopupSelected(object sender, ZArchitecture.GUI.Internal.EmbeddedModulePopup.SelectedEventArgs e)
	{
		if (e?.SelectedBusinessObjects[0] is CusAuthorisationRule authorisationRule)
		{
			if (AuthorisationsGrid.GetCurrent() is CusAuthorizationUsage usage
				&& usage.AGC_Number.IsEmpty && usage.AGC_Code.IsEmpty && usage.AGC_OH_Owner.IsEmpty)
			{
				var authorisationHeader = authorisationRule.AuthorisationHeader;
				usage.AGC_Number = authorisationHeader.CPH_Number;
				usage.AGC_Code = authorisationHeader.CPH_Type.Left(4);
				usage.AGC_OH_Owner = authorisationHeader.CPH_OH_PermitHolder;
			}
		}
	}
}

