using System;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class SecurityDeclarationUserControl : ZUserControl
	{
		public SecurityDeclarationUserControl()
		{
			InitializeComponent();
			ToggleControlsVisibilityBasedOnCountry();
		}

		void OverrideValuesCheckBox_Click(object sender, EventArgs args)
		{
			if (CurrentDataItem is IAWBParent awbParent
				&& !awbParent.IsOverrideAllowed)
			{
				Globals.Message.ShowError(
					Env.Security.GetErrorMessageForNotAllowed(
						awbParent.GetType() == typeof(ForwardingConsol)
							? Env.Security.MaintainConsolAWBOverride
							: Env.Security.MaintainShipmentAWBOverride));
			}
		}

		void ToggleControlsVisibilityBasedOnCountry()
		{
			dateTimeOfScheduledArrivalDateEdit.Visible = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.UnitedKingdom;
			chooseAdditionalSecurityInformationStatementDropEdit.Visible = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Australia;
		}
	}
}
