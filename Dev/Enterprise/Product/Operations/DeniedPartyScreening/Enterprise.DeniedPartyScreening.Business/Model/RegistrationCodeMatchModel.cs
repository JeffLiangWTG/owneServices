using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.DeniedPartyScreening.Common;

namespace Enterprise.DeniedPartyScreening.Business
{
	public class RegistrationCodeMatchModel : BaseMatchModel<ScreenedDeniedRegistrationCodeItemModel>
	{
		public RegistrationCodeMatchModel(List<ProfileRegistrationCodeInfo> profileRegistrationCodeInfos, List<RegistrationCodeMatchInfo> registrationCodeMatchInfos)
		{
			Argument.NotNull(profileRegistrationCodeInfos, nameof(profileRegistrationCodeInfos));
			Argument.NotNull(registrationCodeMatchInfos, nameof(registrationCodeMatchInfos));

			ProfileRegistrationCodeInfos = profileRegistrationCodeInfos;
			RegistrationCodeMatchInfos = registrationCodeMatchInfos;
		}

		public List<ProfileRegistrationCodeInfo> ProfileRegistrationCodeInfos { get; }

		public List<RegistrationCodeMatchInfo> RegistrationCodeMatchInfos { get; }

		List<ScreenedDeniedRegistrationCodeItemModel> totalScreenedDeniedItems;

		public override List<ScreenedDeniedRegistrationCodeItemModel> TotalScreenedDeniedItems
		{
			get
			{
				if (totalScreenedDeniedItems == null)
				{
					totalScreenedDeniedItems = new List<ScreenedDeniedRegistrationCodeItemModel>();

					foreach (var profileRegistrationCode in ProfileRegistrationCodeInfos)
					{
						var matchedRegCodes = RegistrationCodeMatchInfos.Where(x => x.MatchingRegistrationCodeID == profileRegistrationCode.ID).ToList();
						if (matchedRegCodes.Count > 0)
						{
							matchedRegCodes.ForEach(x => totalScreenedDeniedItems.Add(new ScreenedDeniedRegistrationCodeItemModel(x, profileRegistrationCode)));
						}
						else
						{
							if (!string.IsNullOrWhiteSpace(profileRegistrationCode.IdNumber))
							{
								totalScreenedDeniedItems.Add(new ScreenedDeniedRegistrationCodeItemModel(profileRegistrationCode));
							}
						}
					}
				}

				return totalScreenedDeniedItems;
			}
		}
	}
}
