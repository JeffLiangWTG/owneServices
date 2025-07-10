using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Common;

namespace Enterprise.eTail.Business.DeniedPartyScreening
{
	public class AddressMatchCollection : NonPersistentBusinessObjectCollection<AddressMatch>
	{
		public AddressMatchCollection(ProfileHeaderInfo headerInfo, DpsResponse response) : base()
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
			if (headerInfo.ProfileAddresses == null)
			{
				return;
			}

			foreach (var profileAddress in headerInfo.ProfileAddresses)
			{
				var matchedAddresses = response.AddressMatches.Where(x => x.MatchingAddressID == profileAddress.ID).ToList();
				if (matchedAddresses.Count > 0)
				{
					matchedAddresses.ForEach(x => Add(new AddressMatch(x, profileAddress)));
				}
				else
				{
					if (AddressNotNullOrEmpty(profileAddress))
					{
						Add(new AddressMatch(null, profileAddress));
					}
				}
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
