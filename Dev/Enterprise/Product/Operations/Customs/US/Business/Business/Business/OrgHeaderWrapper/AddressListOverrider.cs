using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	public static class AddressListOverrider
	{
		public static ZAddressList ShowManufacturerIDInAddressList(BusinessObjectFactory factory, ZAddressList addressList)
		{
			return ShowInAddressList(factory, addressList, OrgCusCode.USACodeTypes.ManufacturerID);
		}

		public static ZAddressList ShowFDAEstablishmentIdentifierInAddressList(BusinessObjectFactory factory, ZAddressList addressList)
		{
			return ShowInAddressList(factory, addressList, OrgCusCode.CodeTypes.FDAEstablishmentIdentifier);
		}

		static ZAddressList ShowInAddressList(BusinessObjectFactory factory, ZAddressList addressList, ZString codeType)
		{
			ZAddressList result = new ZAddressList();

			foreach (ZAddressItem item in addressList)
			{
				OrgAddress address = factory.Load<OrgAddress>(item.PK);
				ZString usageComment = item.UsageComment;
				ZString code = address.CustomsCodes.GetCustomsRegNo(codeType, Core.Constants.CountryCodes.UnitedStates);
				if (!code.IsEmpty)
				{
					usageComment = code + " (" + usageComment + ")";
				}

				result.AddAddress(item.PK, usageComment, item.AddressDescription, item.Capabilities);
			}

			return result;
		}

		public static ZAddressList ShowCBPNoInAddressList(BusinessObjectFactory factory, ZAddressList addressList)
		{
			return ShowInAddressList(factory, addressList, OrgCusCode.USACodeTypes.CBPAssignedNumber);
		}
	}
}
