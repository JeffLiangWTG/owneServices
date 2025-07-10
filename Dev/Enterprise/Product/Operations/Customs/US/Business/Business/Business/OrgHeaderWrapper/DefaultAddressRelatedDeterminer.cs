using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.Business
{
	public static class DefaultAddressRelatedDeterminer
	{
		public static ZGuid GetMIDAddress(IOrgHeader orgHeader)
		{
			ZGuid result = ZGuid.Empty;
			OrgHeader organisation = orgHeader as OrgHeader;
			if (organisation != null)
			{
				result = organisation.MainAddress.PK;
				foreach (OrgAddress address in organisation.Addresses)
				{
					if (!address.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.ManufacturerID, Core.Constants.CountryCodes.UnitedStates).IsEmpty)
					{
						result = address.PK;
						break;
					}
				}
			}
			return result;
		}

		public static ZGuid GetFEIAddress(IOrgHeader orgHeader)
		{
			ZGuid result = ZGuid.Empty;
			OrgHeader organisation = orgHeader as OrgHeader;
			if (organisation != null)
			{
				result = organisation.MainAddress.PK;
				foreach (OrgAddress address in organisation.Addresses)
				{
					if (!address.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, Core.Constants.CountryCodes.UnitedStates).IsEmpty)
					{
						result = address.PK;
						break;
					}
				}
			}
			return result;
		}

		public static ZGuid GetCBPAddress(IOrgHeader orgHeader)
		{
			ZGuid result = ZGuid.Empty;
			OrgHeader organisation = orgHeader as OrgHeader;
			if (organisation != null)
			{
				result = organisation.MainAddress.PK;
				foreach (OrgAddress address in organisation.Addresses)
				{
					if (!address.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.CBPAssignedNumber, Core.Constants.CountryCodes.UnitedStates).IsEmpty)
					{
						result = address.PK;
						break;
					}
				}
			}
			return result;
		}

		public static ZGuid GetDUNS_FEIAddress(IOrgHeader orgHeader)
		{
			var result = ZGuid.Empty;
			var organisation = orgHeader as OrgHeader;
			if (organisation != null)
			{
				var addressesToDefault = organisation.Addresses.Cast<OrgAddress>().Where(x => x.CustomsCodes.Any(y => y.OK_CodeType == OrgCusCode.CodeTypes.DataUniversalNumberingSystem)).Take(2);
				if (addressesToDefault.Count() == 1)
				{
					result = addressesToDefault.FirstOrDefault().PK;
				}
				else if (!addressesToDefault.Any())
				{
					addressesToDefault = organisation.Addresses.Cast<OrgAddress>().Where(x => x.CustomsCodes.Any(y => y.OK_CodeType == OrgCusCode.CodeTypes.FDAEstablishmentIdentifier)).Take(2);
					if (addressesToDefault.Count() == 1)
					{
						result = addressesToDefault.FirstOrDefault().PK;
					}
				}

				if (result == ZGuid.Empty)
				{
					result = organisation.MainAddress.PK;
				}
			}
			return result;
		}

		public static ZGuid GetMainAddress(IOrgHeader orgHeader)
		{
			var organisation = orgHeader as OrgHeader;
			return organisation != null ? organisation.MainAddress.PK : ZGuid.Empty;
		}

		public static ZGuid GetCustomsAddressOfRecordAddress(IOrgHeader orgHeader)
		{
			return GetDefaultMainAddressOfType(orgHeader, OrgAddressType.CustomsAddressOfRecord);
		}

		public static ZGuid GetDeliveryAddress(IOrgHeader orgHeader)
		{
			return GetDefaultMainAddressOfType(orgHeader, OrgAddressType.Delivery);
		}

		static ZGuid GetDefaultMainAddressOfType(IOrgHeader orgHeader, OrgAddressType addressType)
		{
			var result = ZGuid.Empty;
			var organisation = orgHeader as OrgHeader;
			if (organisation != null)
			{
				var address = organisation.Addresses.DefaultAddressOfType(addressType);
				if (address != null)
				{
					result = address.PK;
				}
				else
				{
					result = organisation.MainAddress.PK;
				}
			}

			return result;
		}
	}
}
