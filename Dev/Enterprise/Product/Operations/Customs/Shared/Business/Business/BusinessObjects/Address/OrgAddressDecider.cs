using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public enum CargoAddressType
	{
		All,
		Delivery,
		Notify,
		Pickup,
	}

	public interface IAddress
	{
		OrgHeader Organisation { get; }
		OrgAddress OrgAddress { get; }
		ZString CompanyName { get; }
		ZString Address1 { get; }
		ZString Address2 { get; }
		ZString Address3 { get; }
		ZString City { get; }
		ZString State { get; }
		ZString PostCode { get; }
		ZString CountryCode { get; }
		ZString RelatedPortCode { get; }
		ZString Phone { get; }
		ZString Fax { get; }
	}

	public class OrgAddressDecider : IAddress
	{
		public OrgAddressDecider(OrgHeader master, CargoAddressType type)
		{
			fMaster = Argument.NotNull(master, nameof(master));
			fAddressType = type;
		}

		public const int MaximumLength = 35;
		const int Address3MaximumLength = 25;
		public const int PostcodeMaxLength = 9;
		public const int StateMaxLength = 25;

		#region IAddress Members

		public OrgHeader Organisation => fMaster;
		public OrgAddress OrgAddress => AddressWithFallback;

		public ZString CompanyName => fMaster.OH_FullName;

		public ZString Address1 => AddressWithFallback.OA_Address1.SubstringSafe(0, MaximumLength);

		public ZString Address2 => AddressWithFallback.OA_Address2.SubstringSafe(0, MaximumLength);

		public ZString Address3 => CombineStringsWithMax(City, State, System.Math.Min(Address3MaximumLength, MaximumLength));

		public ZString City => (Address?.CityFallback ?? fMaster.CityFallback).SubstringSafe(0, MaximumLength);

		public ZString State => AddressWithFallback.OA_State.SubstringSafe(0, StateMaxLength);

		public ZString PostCode => AddressWithFallback.OA_PostCode.SubstringSafe(0, PostcodeMaxLength);

		public ZString CountryCode => RelatedPortCode.SubstringSafe(0, 2);

		public ZString RelatedPortCode => AddressWithFallback.OA_RL_NKRelatedPortCode;

		public ZString Phone => AddressWithFallback.OA_Phone;

		public ZString Fax => AddressWithFallback.OA_Fax;

		#endregion

		#region Implementation

		protected OrgHeader fMaster;
		protected CargoAddressType fAddressType;
		public OrgAddress Address => address ?? InitializeAddress();
		OrgAddress address;

		public OrgAddress AddressWithFallback => Address ?? fMaster.MainAddress;

		protected OrgAddress InitializeAddress()
		{
			switch (fAddressType)
			{
				case CargoAddressType.Delivery:
					address = GetAddressInThisOrder(OrgConstants.AddressType.Delivery, OrgConstants.AddressType.PickupAndDelivery, OrgConstants.AddressType.Office, "");
					break;
				case CargoAddressType.Pickup:
					address = GetAddressInThisOrder(OrgConstants.AddressType.Pickup, OrgConstants.AddressType.PickupAndDelivery, OrgConstants.AddressType.Office, "");
					break;
			}
			return address;
		}

		protected virtual OrgAddress GetAddressInThisOrder(params string[] addressTypesInOrder)
		{
			OrgAddress result = null;
			foreach (string cargoAddressType in addressTypesInOrder)
			{
				OrgAddressList addressRetrieved = fMaster.Addresses.RetrieveAddressesWithAddressType(cargoAddressType);
				foreach (OrgAddress address in addressRetrieved)
				{
					if (address.AddressCapability.GetIsMainAddress(cargoAddressType))
					{
						result = address;
					}
				}

				if (result == null && addressRetrieved.Count > 0)//if no address is set as a default
				{
					result = addressRetrieved[0];
				}

				if (result != null)
				{
					break;
				}
			}
			return result;
		}

		protected ZString CombineStringsWithMax(ZString string1, ZString string2, int maxLength)
		{
			ZString result = string1 + " " + string2;
			if (maxLength > 0)
			{
				if (result.Length > maxLength)
				{
					result = result.Substring(0, maxLength);
				}
			}
			return result.TrimEnd();
		}

		#endregion
	}
}
