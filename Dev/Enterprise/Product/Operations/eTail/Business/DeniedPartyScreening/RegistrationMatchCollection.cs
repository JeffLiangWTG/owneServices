using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Common;

namespace Enterprise.eTail.Business.DeniedPartyScreening
{
	public class RegistrationMatchCollection : NonPersistentBusinessObjectCollection<RegistrationMatch>
	{
		public RegistrationMatchCollection(ProfileHeaderInfo headerInfo, DpsResponse response) : base()
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
			if (headerInfo.ProfileRegistrationCodes == null)
			{
				return;
			}

			foreach (var profileRegistrationCode in headerInfo.ProfileRegistrationCodes)
			{
				var matchedRegCodes = response.RegistrationCodeMatches.Where(x => x.MatchingRegistrationCodeID == profileRegistrationCode.ID).ToList();
				if (matchedRegCodes.Count > 0)
				{
					matchedRegCodes.ForEach(x => Add(new RegistrationMatch(x, profileRegistrationCode)));
				}
				else
				{
					if (!string.IsNullOrWhiteSpace(profileRegistrationCode.IdNumber))
					{
						Add(new RegistrationMatch(null, profileRegistrationCode));
					}
				}
			}
		}
	}
}
