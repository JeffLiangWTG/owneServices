using CargoWise.Common;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DeniedPartyScreening.Business
{
	public class ScreenedDeniedNameItemModel
	{
		public ScreenedDeniedNameItemModel(ProfileNameInfo profileNameInfo) : this(null, profileNameInfo, false)
		{
		}

		public ScreenedDeniedNameItemModel(NameMatchInfo nameMatchInfo, ProfileNameInfo profileNameInfo) : this(nameMatchInfo, profileNameInfo, true)
		{
		}

		protected ScreenedDeniedNameItemModel(NameMatchInfo nameMatchInfo, ProfileNameInfo profileNameInfo, bool requireNameMatchInfo)
		{
			if (requireNameMatchInfo)
			{
				Argument.NotNull(nameMatchInfo, nameof(nameMatchInfo));
			}

			Argument.NotNull(profileNameInfo, nameof(profileNameInfo));
			ProfileNameInfo = profileNameInfo;
			NameMatchInfo = nameMatchInfo;

			SetScoreInfo();
		}

		public ProfileNameInfo ProfileNameInfo { get; }

		public NameMatchInfo NameMatchInfo { get; }

		public ScoreGrades ScoreGrade { get; private set; }

		public MultilingualString DisplayScore { get; private set; }

		public int Score { get; private set; }

		void SetScoreInfo()
		{
			if (NameMatchInfo != null)
			{
				var confidenceThreshold = OrganisationsDataRegistry.Instance.GetDPSMatchingConfidenceThreshold(NameMatchInfo.RequestName.NameType);
				if (NameMatchInfo.MatchingNameScore >= confidenceThreshold.HighThreshold)
				{
					ScoreGrade = ScoreGrades.High;
				}
				else if (NameMatchInfo.MatchingNameScore >= confidenceThreshold.MediumThreshold)
				{
					ScoreGrade = ScoreGrades.Medium;
				}
				else
				{
					ScoreGrade = ScoreGrades.Low;
				}

				Score = NameMatchInfo.MatchingNameScore;
			}
			else
			{
				ScoreGrade = ScoreGrades.Low;
				Score = 0;
			}

			DisplayScore = ScreenedDeniedAddressItemModel.GetDisplayScoreCore(Score);
		}
	}
}
