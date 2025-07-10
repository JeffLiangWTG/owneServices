using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.DeniedPartyScreening.Common;

namespace Enterprise.DeniedPartyScreening.Business
{
	public class CountryMatchModel : BaseMatchModel<ScreenedDeniedCountryItemModel>
	{
		public CountryMatchModel(List<ProfileCountryInfo> profileCountries, List<CountryMatchInfo> countyMatchInfos)
		{
			Argument.NotNull(profileCountries, nameof(profileCountries));
			Argument.NotNull(countyMatchInfos, nameof(countyMatchInfos));
			ProfileCountries = profileCountries;
			CountryMatchInfos = countyMatchInfos;
		}

		public List<ProfileCountryInfo> ProfileCountries { get; }

		public List<CountryMatchInfo> CountryMatchInfos { get; }

		public override List<ScreenedDeniedCountryItemModel> TotalScreenedDeniedItems
		{
			get
			{
				if (totalScreenedDeniedItems == null)
				{
					totalScreenedDeniedItems = new List<ScreenedDeniedCountryItemModel>();

					foreach (var profileCountry in ProfileCountries)
					{
						var matchedCoutries = CountryMatchInfos.Where(x => x.MatchingCountryId == profileCountry.ID).ToList();
						if (matchedCoutries.Count > 0)
						{
							matchedCoutries.ForEach(x => totalScreenedDeniedItems.Add(new ScreenedDeniedCountryItemModel(x, profileCountry)));
						}
						else
						{
							if (!string.IsNullOrWhiteSpace(profileCountry.CountryName))
							{
								totalScreenedDeniedItems.Add(new ScreenedDeniedCountryItemModel(profileCountry));
							}
						}
					}
				}
				return totalScreenedDeniedItems;
			}
		}
		List<ScreenedDeniedCountryItemModel> totalScreenedDeniedItems;
	}
}
