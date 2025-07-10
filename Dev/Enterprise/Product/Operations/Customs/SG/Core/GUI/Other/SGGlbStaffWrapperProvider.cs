using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.SG.V4.GUI
{
	public class SGGlbStaffWrapperProvider : GlbStaffWrapperProvider, MasterFiles.Integration.Customs.SG.ISGGlbStaffWrapperProvider
	{
		protected override StaffCredentialsUserControl GetNewUserControlCore()
		{
			return new SGStaffCredentialsUserControl();
		}

		protected override GlbStaffWrapper GetWrapperCore(GlbStaff staff)
		{
			return SGGlbStaffWrapper.Get(staff);
		}

		protected override ContinueWithSave ShowPreSaveDialogsCore(GlbStaffWrapper wrapper)
		{
			var result = base.ShowPreSaveDialogsCore(wrapper);

			if (result == ContinueWithSave.Yes)
			{
				var password = (wrapper as SGGlbStaffWrapper)?.AccessPassword;

				if (password != null && password.HasChanges)
				{
					var passwordsForSync = password.GetPasswordsForSync().ToArray();

					if (passwordsForSync.Any() && Globals.Message.Show(MessageForSyncPassword, "ACCESS", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
					{
						password.SyncPassword(passwordsForSync);
					}
				}
			}

			return result;
		}

		string MessageForSyncPassword => Res.GetString("B5E27954-BF1F-4482-9BBA-1960F1B512A5", "Would you like to apply the same change to other linked staff records which are using same ACCESS User ID in this company?");
	}
}
