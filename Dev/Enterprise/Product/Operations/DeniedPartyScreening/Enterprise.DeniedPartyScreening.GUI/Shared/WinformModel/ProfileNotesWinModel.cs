using Enterprise.DeniedPartyScreening.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public class ProfileNotesWinModel : ExpanderWinModel<ProfileNotesWinModel>
	{
		public ProfileNotesWinModel(ProfileNotesModel profileNotesModel)
		{
			ProfileNotesModel = profileNotesModel;
		}

		public string Content => ProfileNotesModel.ProfileNotes;

		public string Link => Res.GetString("0d310c99-8474-46e4-b7e2-d1b76eb6478e", "Open in New Window");

		public override string ExpanderTitle => Res.GetString("d261041f-a8d2-402a-abc1-f572854ac55c", "Profile Notes");

		public override string ExpanderDescription => string.Empty;

		public override bool IsExpanderEnabled => !string.IsNullOrWhiteSpace(Content);

		public ProfileNotesModel ProfileNotesModel { get; }

		public void OpenProfileNotesInNewWindow()
		{
			using (var profileNotesNewWindow = new ProfileNotesDetailForm(Content))
			{
				ZFormModaliser.ShowDialogWithoutDispose(profileNotesNewWindow);
			}
		}
	}
}
