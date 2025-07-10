using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.DeniedPartyScreening.Common;

namespace Enterprise.DeniedPartyScreening.Business
{
	public class AddressMatchModel : BaseMatchModel<ScreenedDeniedAddressItemModel>
	{
		public AddressMatchModel(List<ProfileAddressInfo> profileAddressInfos, List<AddressMatchInfo> addressMatchInfos)
		{
			Argument.NotNull(profileAddressInfos, nameof(profileAddressInfos));
			Argument.NotNull(addressMatchInfos, nameof(addressMatchInfos));

			ProfileAddressInfos = profileAddressInfos;
			AddressMatchInfos = addressMatchInfos;
		}

		public IEnumerable<ProfileAddressInfo> ProfileAddressInfos { get; }
		public List<AddressMatchInfo> AddressMatchInfos { get; }

		List<ScreenedDeniedAddressItemModel> totalScreenedDeniedItems;

		public override List<ScreenedDeniedAddressItemModel> TotalScreenedDeniedItems
		{
			get
			{
				if (totalScreenedDeniedItems == null)
				{
					totalScreenedDeniedItems = new List<ScreenedDeniedAddressItemModel>();

					foreach (var profileAddress in ProfileAddressInfos)
					{
						var matchedAddresses = AddressMatchInfos.Where(x => x.MatchingAddressID == profileAddress.ID).ToList();
						if (matchedAddresses.Count > 0)
						{
							matchedAddresses.ForEach(x => totalScreenedDeniedItems.Add(new ScreenedDeniedAddressItemModel(x, profileAddress)));
						}
						else
						{
							if (AddressNotNullOrEmpty(profileAddress))
							{
								totalScreenedDeniedItems.Add(new ScreenedDeniedAddressItemModel(profileAddress));
							}
						}
					}
				}

				return totalScreenedDeniedItems;
			}
		}

		bool AddressNotNullOrEmpty(ProfileAddressInfo address)
		{
			return AddressNotNullOrEmpty(address.Street) ||
					AddressNotNullOrEmpty(address.City) ||
					AddressNotNullOrEmpty(address.StateProvince) ||
					AddressNotNullOrEmpty(address.PostCode) ||
					AddressNotNullOrEmpty(address.Country);
		}

		bool AddressNotNullOrEmpty(string addressPart)
		{
			return !string.IsNullOrWhiteSpace(addressPart);
		}
	}
}
