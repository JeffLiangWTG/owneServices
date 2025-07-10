using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Common;

namespace Enterprise.eTail.Business.DeniedPartyScreening
{
	public class NameMatchCollection : NonPersistentBusinessObjectCollection<NameMatch>
	{
		public NameMatchCollection(ProfileHeaderInfo headerInfo, DpsResponse response) : base()
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
			if (headerInfo.ProfileNames == null)
			{
				return;
			}

			foreach (var profileName in headerInfo.ProfileNames)
			{
				var matchedNames = response.NameMatches?.Where(x => x.MatchingNameID == profileName.ID).ToList() ?? new List<NameMatchInfo>();
				if (matchedNames.Count > 0)
				{
					matchedNames.ForEach(x => Add(new NameMatch(x, profileName)));
				}
				else
				{
					if (!string.IsNullOrWhiteSpace(profileName.FullName))
					{
						Add(new NameMatch(null, profileName));
					}
				}
			}
		}
	}
}
