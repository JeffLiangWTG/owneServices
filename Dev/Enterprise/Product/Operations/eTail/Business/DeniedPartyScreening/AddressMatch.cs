using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Common;
using static Enterprise.eTail.Business.DeniedPartyScreening.ProfileHeader;

namespace Enterprise.eTail.Business.DeniedPartyScreening
{
	public class AddressMatch : NonPersistentBusinessObject
	{
		public AddressMatch(AddressMatchInfo addressMatchInfo, ProfileAddressInfo profileAddressInfo) : base()
		{
			Argument.NotNull(profileAddressInfo, nameof(profileAddressInfo));

			AddressMatchInfo = addressMatchInfo;
			ProfileAddressInfo = profileAddressInfo;
		}

		public AddressMatchInfo AddressMatchInfo { get; }

		public ProfileAddressInfo ProfileAddressInfo { get; }

		public ZString Name => GetFormattedAddress(ProfileAddressInfo);

		public ZInt Score => AddressMatchInfo?.MatchingAddressScore ?? 0;

		public ZString Type => Res.GetString("5e3f9db0-7b71-4690-97a2-599f5608d292", "Address");

		public RiskLevel Level
		{
			get
			{
				if (AddressMatchInfo == null)
				{
					return RiskLevel.Low;
				}

				if (AddressMatchInfo.MatchingAddressScore >= 85)
				{
					return RiskLevel.High;
				}
				else if (AddressMatchInfo.MatchingAddressScore >= 65)
				{
					return RiskLevel.Medium;
				}
				else
				{
					return RiskLevel.Low;
				}
			}
		}

		string GetFormattedAddress(ProfileAddressInfo address)
		{
			return GetAddressElement(address.Country) +
				GetAddressElement(address.PostCode) +
				GetAddressElement(address.PostCode) +
				GetAddressElement(address.StateProvince) +
				GetAddressElement(address.City) +
				GetAddressElement(address.Street);
		}

		string GetAddressElement(string addressElement)
		{
			if (!string.IsNullOrWhiteSpace(addressElement))
			{
				return addressElement;
			}

			return string.Empty;
		}
	}
}
