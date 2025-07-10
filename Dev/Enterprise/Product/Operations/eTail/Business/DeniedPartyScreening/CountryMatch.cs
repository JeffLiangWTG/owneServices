using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Common;

namespace Enterprise.eTail.Business.DeniedPartyScreening
{
	public class CountryMatch : NonPersistentBusinessObject
	{
		public CountryMatch(CountryMatchInfo countryMatchInfo, ProfileCountryInfo profileCountryInfo) : base()
		{
			Argument.NotNull(profileCountryInfo, nameof(profileCountryInfo));

			ProfileCountryInfo = profileCountryInfo;
			CountryMatchInfo = countryMatchInfo;
		}

		public ProfileCountryInfo ProfileCountryInfo { get; }

		public CountryMatchInfo CountryMatchInfo { get; }

		public ZString Name => ProfileCountryInfo.CountryName;

		public ZInt Score => CountryMatchInfo?.MatchingCountryScore ?? 0;

		public ZString Type => Res.GetString("dc76d8c6-db2b-4ac8-b5ac-e095adbd44cd", "Country");
	}
}
