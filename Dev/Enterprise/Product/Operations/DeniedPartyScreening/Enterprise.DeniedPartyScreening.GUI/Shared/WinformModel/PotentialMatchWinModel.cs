using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public class PotentialMatchWinModel
	{
		static readonly MultilingualString HighText = ResString.GetMultilingualString("DFF9C8EC-18B4-4A70-A0E2-647FA45F81C6", "High");

		static readonly MultilingualString MediumText = ResString.GetMultilingualString("060E672F-DF2E-4D63-9E4D-30733E45AAA0", "Medium");

		static readonly MultilingualString RiskText = ResString.GetMultilingualString("EE6B9989-BDD6-440D-9DC0-018E5F8B944E", "Risk - Review Required");

		public PotentialMatchWinModel(PotentialMatchModel potentialMatchModel)
		{
			Argument.NotNull(potentialMatchModel, nameof(potentialMatchModel));

			PotentialMatchModel = potentialMatchModel;
			Factory = potentialMatchModel.Factory;

			NameMatchWinModel = new NameMatchWinModel(PotentialMatchModel.NameMatchModel, MatchViewVisibility);
			AddressMatchWinModel = new AddressMatchWinModel(PotentialMatchModel.AddressMatchModel, MatchViewVisibility);
			RegistrationCodeWinModel = new RegistrationCodeMatchWinModel(PotentialMatchModel.RegistrationCodeMatchModel, MatchViewVisibility);
			ProfileNotesWinModel = new ProfileNotesWinModel(PotentialMatchModel.ProfileNotesModel);
			SourceListNamesWinModel = new SourceListNamesWinModel(PotentialMatchModel.SourceListNamesModel);

			SetEntityTypeIcon();
			SetScoreGrade();
		}

		public PotentialMatchModel PotentialMatchModel { get; }

		public BusinessObjectFactory Factory { get; }

		public string ProfileName => PotentialMatchModel.ProfileName;

		public DpsImageSources EntityTypeIcon { get; private set; }

		public DpsImageSources WarningIcon { get; private set; }

		public DpsImageSources RequireReviewIcon => DpsImageSources.WarningWhite;

		public string ScoreGradeText { get; private set; }

		public string ScoreGradeReviewText { get; private set; }

		public ScoreGrades ScoreGrade => PotentialMatchModel.ScoreGrade;

		public NameMatchWinModel NameMatchWinModel { get; }

		public AddressMatchWinModel AddressMatchWinModel { get; }

		public RegistrationCodeMatchWinModel RegistrationCodeWinModel { get; }

		public ProfileNotesWinModel ProfileNotesWinModel { get; }

		public SourceListNamesWinModel SourceListNamesWinModel { get; }

		public bool IsValid => SourceListNamesWinModel.ExcludedSourceListNames.Count + SourceListNamesWinModel.IncludedSourceListNames.Count > 0;

		public bool IsExcluded => SourceListNamesWinModel.IncludedSourceListNames.Count == 0;

		bool MatchViewVisibility => !(PotentialMatchModel.TypeOfEntity == DeniedPartyConstants.ScreeningNameTypes.Country && PotentialMatchModel.AddressMatchInfos.Count == 0 &&
					PotentialMatchModel.NameMatchInfos.Count == 0 && PotentialMatchModel.RegistrationCodeMatchInfos.Count == 0);

		void SetEntityTypeIcon()
		{
			switch (PotentialMatchModel.TypeOfEntity)
			{
				case DeniedPartyConstants.ScreeningNameTypes.Person:
					EntityTypeIcon = DpsImageSources.Person;
					break;
				case DeniedPartyConstants.ScreeningNameTypes.Vessel:
					EntityTypeIcon = DpsImageSources.Vessel;
					break;
				case DeniedPartyConstants.ScreeningNameTypes.Country:
					EntityTypeIcon = DpsImageSources.Country;
					break;
				default:
					EntityTypeIcon = DpsImageSources.Organization;
					break;
			}
		}

		void SetScoreGrade()
		{
			if (ScoreGrade == ScoreGrades.High)
			{
				WarningIcon = DpsImageSources.WarningRed;
				ScoreGradeText = HighText;
				ScoreGradeReviewText = HighText + " " + RiskText;
			}
			else if (ScoreGrade == ScoreGrades.Medium)
			{
				WarningIcon = DpsImageSources.WarningOrange;
				ScoreGradeText = MediumText;
				ScoreGradeReviewText = MediumText + " " + RiskText;
			}
			else
			{
				WarningIcon = DpsImageSources.None;
				ScoreGradeText = string.Empty;
				ScoreGradeReviewText = string.Empty;
			}
		}
	}
}
