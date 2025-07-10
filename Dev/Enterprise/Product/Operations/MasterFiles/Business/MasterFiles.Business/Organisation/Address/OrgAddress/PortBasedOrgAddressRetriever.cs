using System.Linq;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	class PortBasedOrgAddressRetriever
	{
		public PortBasedOrgAddressRetriever(OrgHeader org, ZString port, params string[] addressTypes)
		{
			this.port = port;
			this.addresses = org.Addresses;
			this.addressTypes = addressTypes;

			if (!port.IsEmpty)
			{
				this.addressPredicates = new AddressPredicate[]
				{
					PortSpecificAndMain,
					PortSpecificAndCorrectType,
					PortSpecific,
					CountrySpecificAndMain,
					CountrySpecificAndCorrectType,
					CountrySpecific,
					CorrectTypeAndMain,
					CorrectType
				};
			}
			else
			{
				this.addressPredicates = new AddressPredicate[]
				{
					CorrectTypeAndMain,
					CorrectType
				};
			}
		}

		public OrgAddress BestAddress
		{
			get
			{
				OrgAddress result = null;
				int resultPredicateScore = 0;
				var orderedAddresses = addresses.Cast<OrgAddress>().OrderBy(x => x.OA_SystemCreateTimeUtc).ToArray();
				foreach (string addressType in addressTypes)
				{
					OrgAddress typeResult = null;
					int typePredicateScore = 0;
					foreach (AddressPredicate predicate in addressPredicates)
					{
						foreach (OrgAddress address in orderedAddresses)
						{
							typePredicateScore = predicate(address, addressType);
							if (typePredicateScore > 0)
							{
								typeResult = address;
								break;
							}
						}
						if (typePredicateScore > 0)
						{
							break;
						}
					}
					if (typePredicateScore > resultPredicateScore)
					{
						result = typeResult;
						resultPredicateScore = typePredicateScore;
					}
				}
				if (result == null)
				{
					result = addresses.MainAddress;
				}

				return result;
			}
		}

		#region Predicates

		delegate int AddressPredicate(OrgAddress address, string addressType);

		int IncrementOperationalAddresses(int currentScore, int increment, string addressType)
		{
			return currentScore > 0 && (addressType == OrgAddressType.Delivery || addressType == OrgAddressType.Pickup || addressType == OrgAddressType.PickupAndDelivery) ?
				currentScore + increment : currentScore;
		}

		#region Port Specific

		int PortSpecificAndMain(OrgAddress address, string addressType)
		{
			return IncrementOperationalAddresses(PortSpecific(address) && address.AddressCapability.GetIsMainAddress(addressType) ? 10 : 0, 1, addressType);
		}

		int PortSpecificAndCorrectType(OrgAddress address, string addressType)
		{
			return IncrementOperationalAddresses(PortSpecific(address) && address.AddressCapability.GetCapabilityEnabled(addressType) ? 9 : 0, 2, addressType);
		}

		int PortSpecific(OrgAddress address, string addressType)
		{
			return IncrementOperationalAddresses(PortSpecific(address) ? 8 : 0, 1, addressType);
		}

		bool PortSpecific(OrgAddress address)
		{
			return address.OA_RL_NKRelatedPortCode == port;
		}

		#endregion

		#region Country Specific

		int CountrySpecificAndMain(OrgAddress address, string addressType)
		{
			return IncrementOperationalAddresses(CountrySpecific(address) && address.AddressCapability.GetIsMainAddress(addressType) ? 6 : 0, 1, addressType);
		}

		int CountrySpecificAndCorrectType(OrgAddress address, string addressType)
		{
			return IncrementOperationalAddresses(CountrySpecific(address) && address.AddressCapability.GetCapabilityEnabled(addressType) ? 5 : 0, 2, addressType);
		}

		int CountrySpecific(OrgAddress address, string addressType)
		{
			return IncrementOperationalAddresses(CountrySpecific(address) ? 4 : 0, 1, addressType);
		}

		bool CountrySpecific(OrgAddress address)
		{
			return address.OA_RL_NKRelatedPortCode.SubstringSafe(0, 2) == port.SubstringSafe(0, 2);
		}

		#endregion

		#region Other

		int CorrectTypeAndMain(OrgAddress address, string addressType)
		{
			return IncrementOperationalAddresses(address.AddressCapability.GetIsMainAddress(addressType) ? 2 : 0, 1, addressType);
		}

		int CorrectType(OrgAddress address, string addressType)
		{
			return IncrementOperationalAddresses(address.AddressCapability.GetCapabilityEnabled(addressType) ? 1 : 0, 1, addressType);
		}

		#endregion

		#endregion

		#region Implementation

		readonly ZString port;
		readonly OrgAddressDependentCollection addresses;
		readonly string[] addressTypes;
		readonly AddressPredicate[] addressPredicates;

		#endregion
	}
}
