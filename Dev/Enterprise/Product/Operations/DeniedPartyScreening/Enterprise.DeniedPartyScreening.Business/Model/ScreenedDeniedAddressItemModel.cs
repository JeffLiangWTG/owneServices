using CargoWise.Common;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DeniedPartyScreening.Business
{
	public class ScreenedDeniedAddressItemModel
	{
		public ScreenedDeniedAddressItemModel(ProfileAddressInfo profileAddressInfo) : this(null, profileAddressInfo, false)
		{
		}

		public ScreenedDeniedAddressItemModel(AddressMatchInfo addressMatchInfo, ProfileAddressInfo profileAddressInfo) : this(addressMatchInfo, profileAddressInfo, true)
		{
		}

		protected ScreenedDeniedAddressItemModel(AddressMatchInfo addressMatchInfo, ProfileAddressInfo profileAddressInfo, bool requireAddressMatchInfo)
		{
			if (requireAddressMatchInfo)
			{
				Argument.NotNull(addressMatchInfo, nameof(addressMatchInfo));
			}

			Argument.NotNull(profileAddressInfo, nameof(profileAddressInfo));

			AddressMatchInfo = addressMatchInfo;
			ProfileAddressInfo = profileAddressInfo;

			SetScoreInfo();
		}

		public ProfileAddressInfo ProfileAddressInfo { get; }

		public AddressMatchInfo AddressMatchInfo { get; }

		public ScoreGrades ScoreGrade { get; private set; }

		public MultilingualString DisplayScore { get; private set; }

		public int Score { get; private set; }

		void SetScoreInfo()
		{
			if (AddressMatchInfo != null)
			{
				if (AddressMatchInfo.MatchingAddressScore >= DeniedPartyConstants.MatchScores.AddressOnlyBalanced)
				{
					ScoreGrade = ScoreGrades.High;
				}
				else if (AddressMatchInfo.MatchingAddressScore >= DeniedPartyConstants.MatchScores.AddressOnlyComprehensive)
				{
					ScoreGrade = ScoreGrades.Medium;
				}
				else
				{
					ScoreGrade = ScoreGrades.Low;
				}

				Score = AddressMatchInfo.MatchingAddressScore;
			}
			else
			{
				ScoreGrade = ScoreGrades.Low;
				Score = 0;
			}

			DisplayScore = GetDisplayScoreCore(Score);
		}

		public static MultilingualString GetDisplayScoreCore(int score)
		{
			return score == 0 ? (NoResString)string.Empty : (NoResString)(score + "%");
		}
	}
}
