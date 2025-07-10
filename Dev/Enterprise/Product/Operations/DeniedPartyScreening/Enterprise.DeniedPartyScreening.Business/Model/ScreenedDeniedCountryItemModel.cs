using CargoWise.Common;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DeniedPartyScreening.Business
{
	public class ScreenedDeniedCountryItemModel
	{
		public ScreenedDeniedCountryItemModel(ProfileCountryInfo profileCountryInfo) : this(null, profileCountryInfo, false)
		{
		}

		public ScreenedDeniedCountryItemModel(CountryMatchInfo countryMatchInfo, ProfileCountryInfo profileCountryInfo) : this(countryMatchInfo, profileCountryInfo, true)
		{
		}

		protected ScreenedDeniedCountryItemModel(CountryMatchInfo countryMatchInfo, ProfileCountryInfo profileCountryInfo, bool requireCountryMatchInfo)
		{
			if (requireCountryMatchInfo)
			{
				Argument.NotNull(countryMatchInfo, nameof(countryMatchInfo));
			}
			Argument.NotNull(profileCountryInfo, nameof(profileCountryInfo));
			ProfileCountryInfo = profileCountryInfo;
			CountryMatchInfo = countryMatchInfo;
			SetScoreInfo();
		}

		public ProfileCountryInfo ProfileCountryInfo { get; }

		public CountryMatchInfo CountryMatchInfo { get; }

		public ScoreGrades ScoreGrade { get; private set; }

		public MultilingualString DisplayScore { get; private set; }

		public int Score { get; private set; }

		void SetScoreInfo()
		{
			if (CountryMatchInfo != null)
			{
				ScoreGrade = ScoreGrades.High;
				Score = CountryMatchInfo.MatchingCountryScore;
				DisplayScore = ResString.GetMultilingualString("4B6A9AD3-0FF5-4725-82C9-BEAD4C2A5FAB", "Exact");
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
