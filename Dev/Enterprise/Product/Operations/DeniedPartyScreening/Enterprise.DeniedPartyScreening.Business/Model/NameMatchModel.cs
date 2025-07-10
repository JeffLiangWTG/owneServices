using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.DeniedPartyScreening.Common;

namespace Enterprise.DeniedPartyScreening.Business
{
	public class NameMatchModel : BaseMatchModel<ScreenedDeniedNameItemModel>
	{
		public NameMatchModel(List<ProfileNameInfo> profileNames, List<NameMatchInfo> nameMatchInfos)
		{
			Argument.NotNull(profileNames, nameof(profileNames));
			Argument.NotNull(nameMatchInfos, nameof(nameMatchInfos));

			ProfileNames = profileNames;
			NameMatchInfos = nameMatchInfos;
		}

		public List<ProfileNameInfo> ProfileNames { get; }
		public List<NameMatchInfo> NameMatchInfos { get; }

		List<ScreenedDeniedNameItemModel> totalScreenedDeniedItems;
		public override List<ScreenedDeniedNameItemModel> TotalScreenedDeniedItems
		{
			get
			{
				if (totalScreenedDeniedItems == null)
				{
					totalScreenedDeniedItems = new List<ScreenedDeniedNameItemModel>();

					foreach (var profileName in ProfileNames)
					{
						var matchedNames = NameMatchInfos.Where(x => x.MatchingNameID == profileName.ID).ToList();
						if (matchedNames.Count > 0)
						{
							matchedNames.ForEach(x => totalScreenedDeniedItems.Add(new ScreenedDeniedNameItemModel(x, profileName)));
						}
						else
						{
							if (!string.IsNullOrWhiteSpace(profileName.FullName))
							{
								totalScreenedDeniedItems.Add(new ScreenedDeniedNameItemModel(profileName));
							}
						}
					}
				}

				return totalScreenedDeniedItems;
			}
		}
	}
}
