using System;
using CargoWise.Common;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public class PreScreenMenuItem : ZMenuItem
	{
		public PreScreenMenuItem(IHVLVPrescreeningDataProvider dataProvider)
			: base(MenuItemCaption)
		{
			this.dataProvider = dataProvider;
		}

		readonly IHVLVPrescreeningDataProvider dataProvider;

		protected ZForm Form => (ZForm)ParentControl.FindForm();

		protected sealed override void OnClick(EventArgs e)
		{
			base.OnClick(e);

			if (UserPromptCheckingHelper.CheckBusinessObjectHasNoChangesOrNotify(dataProvider.Entity, SaveBeforePreScreening))
			{
				using (var progressForm = new ProgressForm())
				{
					progressForm.ShowCancelButton = false;
					progressForm.ShowProgressBar = true;
					progressForm.CaptionResourceString = Res.GetData("1D0BDA31-FE0D-4422-AA61-53FE432AABD4", "Pre-Screening in progress...");
					progressForm.ShowModalTo(Form);

					var provider = new ETailPreScreeningProvider(dataProvider, progressForm.SetStatusAndPercentComplete);
					provider.Screen();
					provider.SyncScreeningResult();
					provider.AddLog();

					if (!provider.ScreeningResult.ErrorMessageDetail.IsNullOrEmpty())
					{
						Globals.Message.Show(provider.ScreeningResult.ErrorMessageDetail);
					}
				}
			}
		}

		static string SaveBeforePreScreening => Res.GetString("46409c40-8188-4cf2-8ce5-a16ca2ae5191", "Please save the form before Pre-Screening");

		static MultilingualString MenuItemCaption => ResString.GetMultilingualString("ebd2672e-9c5a-4572-91f8-6ee1a2c12020", "Pre-Screen HVLV Details");
	}
}
