using System;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefCommodityCodeForm : ZForm
	{
		public RefCommodityCodeForm()
		{
		}

		public RefCommodityCodeForm(RefCommodityCode bO)
			: base(bO)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
		}

		void SetNMFCCodeFindBoxVisibility()
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.UnitedStates
				&& GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.Mexico
				&& GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.Canada)
			{
				NMFCCodeFindBox.Visible = false;
			}
		}

		void SetIsPersonalEffectsCheckBoxVisibility()
		{
			IsPersonalEffectsCheckBox.Visible = ReferenceFilesDataRegistry.Instance.ShowPersonalEffects.Value;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ControlDpiScalingHelper.SetTop(ref ButtonsUserControl, MainStatusBar.Top - ButtonsUserControl.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
			SetNMFCCodeFindBoxVisibility();
			SetIsPersonalEffectsCheckBoxVisibility();
		}
	}
}
