using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Common;

namespace Enterprise.eTail.Business.DeniedPartyScreening
{
	public class CountryMatchCollection : NonPersistentBusinessObjectCollection<CountryMatch>
	{
		public CountryMatchCollection(ProfileHeaderInfo headerInfo, DpsResponse response) : base()
		{
			BuildCollection(headerInfo, response);
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		public override bool ReadOnly => true;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		public void BuildCollection(ProfileHeaderInfo headerInfo, DpsResponse response)
		{
			if (headerInfo.ProfileCountries == null)
			{
				return;
			}

			foreach (var profileCountry in headerInfo.ProfileCountries)
			{
				var matchedCoutries = response.CountryMatches.Where(x => x.MatchingCountryId == profileCountry.ID).ToList();
				if (matchedCoutries.Count > 0)
				{
					matchedCoutries.ForEach(x => Add(new CountryMatch(x, profileCountry)));
				}
				else
				{
					if (!string.IsNullOrWhiteSpace(profileCountry.CountryName))
					{
						Add(new CountryMatch(null, profileCountry));
					}
				}
			}
		}
	}
}
