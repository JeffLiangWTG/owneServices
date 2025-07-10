using CargoWise.Common;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DeniedPartyScreening.Business
{
	public class ScreenedDeniedRegistrationCodeItemModel
	{
		public ScreenedDeniedRegistrationCodeItemModel(ProfileRegistrationCodeInfo profileRegistrationCodeInfo) : this(null, profileRegistrationCodeInfo, false)
		{
		}

		public ScreenedDeniedRegistrationCodeItemModel(RegistrationCodeMatchInfo registrationCodeMatchInfo, ProfileRegistrationCodeInfo profileRegistrationCodeInfo) : this(registrationCodeMatchInfo, profileRegistrationCodeInfo, true)
		{
		}

		protected ScreenedDeniedRegistrationCodeItemModel(RegistrationCodeMatchInfo registrationCodeMatchInfo, ProfileRegistrationCodeInfo profileRegistrationCodeInfo, bool requireRegCodeMatchInfo)
		{
			if (requireRegCodeMatchInfo)
			{
				Argument.NotNull(registrationCodeMatchInfo, nameof(registrationCodeMatchInfo));
			}

			Argument.NotNull(profileRegistrationCodeInfo, nameof(profileRegistrationCodeInfo));

			ProfileRegistrationCodeInfo = profileRegistrationCodeInfo;
			RegistrationCodeMatchInfo = registrationCodeMatchInfo;

			SetScoreInfo();
		}

		public ProfileRegistrationCodeInfo ProfileRegistrationCodeInfo { get; }

		public RegistrationCodeMatchInfo RegistrationCodeMatchInfo { get; }

		public ScoreGrades ScoreGrade { get; private set; }

		public MultilingualString DisplayScore { get; private set; }

		public int Score { get; private set; }

		void SetScoreInfo()
		{
			if (RegistrationCodeMatchInfo != null)
			{
				ScoreGrade = RegistrationCodeMatchInfo.MatchingRegistrationCodeScore == 100 ? ScoreGrades.High : ScoreGrades.Low;
				Score = RegistrationCodeMatchInfo.MatchingRegistrationCodeScore;
				DisplayScore = ScoreGrade == ScoreGrades.High ? ResString.GetMultilingualString("5FED0273-486E-4689-B9A3-A77E528C0D67", "Exact") : ResString.GetMultilingualString("98BE6E61-6C29-41C2-9E58-D240AE668F80", "None");
			}
			else
			{
				ScoreGrade = ScoreGrades.Low;
				Score = 0;
				DisplayScore = (NoResString)string.Empty;
			}
		}
	}
}
