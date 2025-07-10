using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	/// <summary>
	/// Map unmapped universal charge codes
	/// </summary>
	public sealed partial class MapChargeCodesForm : ZChildForm
	{
		public MapChargeCodesForm()
		{
			InitializeComponent();
		}

		public MapChargeCodesForm(UniversalChargeCodeMapBizoCollection unmappedCharges) : base(unmappedCharges)
		{
			InitializeComponent();
			PostingButtonsUserControl.SaveButton.Visible = false;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl, true);
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var list = (UniversalChargeCodeMapBizoCollection)BusinessEntity;
			var requiredRights = list.RequiredSecurityRights(Env.Security);
			var missingRights = requiredRights.Where(x => !x.IsAllowed).ToList();
			if (missingRights.Count > 0)
			{
				MultilingualString message1 = ResString.GetMultilingualString("66CE46BF-D343-4013-AB6A-004602A400A1",
@"You do not have the appropriate security rights to run this function.
You can either cancel, or have a user with these security rights enter their credentials:");

				var checkpointMessage = MultilingualString.Join(System.Environment.NewLine, missingRights.Select(x => x.DisplayTextPathToSecurityRight).ToArray());

				MultilingualString message2 = ResString.GetMultilingualString("13C4368E-3001-4E04-87D5-8FBFB0EAC1CE",
@"Do you wish to have a user with higher rights enter their credentials?");

				var message = MultilingualString.Join(System.Environment.NewLine + System.Environment.NewLine, message1, checkpointMessage, message2);

				if (Globals.Message.Show(message, FormCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Hand) != DialogResult.Yes)
				{
					return ContinueWithSave.No;
				}

				SecurityOverridenLogin login = new SecurityOverridenLogin();
				if (ZFormModaliser.ShowDialogAndDispose(new LoginForm(login)) == DialogResult.OK)
				{
					if (login.UserSecurity == null)
					{
						Globals.Message.Show(Res.GetString("defeaa16-c1bf-4626-80c8-5208b892f706", "The login details entered are incorrect or password is expired."));
						return ContinueWithSave.No;
					}

					missingRights = list.RequiredSecurityRights(login.UserSecurity).Where(x => !x.IsAllowed).ToList();
					if (missingRights.Count > 0)
					{
						message1 = ResString.GetMultilingualString("A7067E86-8F13-452C-9869-8815CA6DC008", "The login details entered do not have security rights:");
						checkpointMessage = MultilingualString.Join(System.Environment.NewLine, missingRights.Select(x => x.DisplayTextPathToSecurityRight).ToArray());
						message = MultilingualString.Join(System.Environment.NewLine + System.Environment.NewLine, message1, checkpointMessage);
						Globals.Message.Show(message);
						return ContinueWithSave.No;
					}
				}
				else
				{
					return ContinueWithSave.No;
				}
			}

			return base.ShowPreSaveDialogs();
		}

		protected override void SaveInternal()
		{
			(BusinessEntity as UniversalChargeCodeMapBizoCollection)?.ApplyUniversalCodeToChargeCodeForSaving();
			base.SaveInternal();
			DialogResult = DialogResult.OK;
		}
	}
}
