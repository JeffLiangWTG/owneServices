using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public static class AddressParser
	{
		public static ZString GetMIDCodeFromAddressPK(ZGuid addressPK, BusinessObjectFactory factory)
		{
			var result = ZString.Empty;

			if (addressPK.IsValid)
			{
				var query = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.ManufacturerID);
				query.AddToFilter(OrgCusCodeSchema.OK_OA_PremisesAddress, addressPK);
				query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.UnitedStates);

				result = factory.LoadTop1<OrgCusCode>(query)?.OK_CustomsRegNo ?? ZString.Empty;
			}

			return result;
		}

		public static ZGuid GetAddressPKFromMatchingMIDCode(ZString mid)
		{
			return GetAddressPKFromMatchingCode(mid, OrgCusCode.USACodeTypes.ManufacturerID);
		}

		public static ZGuid GetAddressPKFromMatchingMIDCode(ZString mid, BusinessObjectFactory factory)
		{
			return GetAddressPKFromMatchingCode(mid, OrgCusCode.USACodeTypes.ManufacturerID, factory);
		}

		static ZGuid GetAddressPKFromMatchingCode(ZString code, ZString codeType, BusinessObjectFactory factory = null)
		{
			ZGuid result = ZGuid.Invalid;

			if (!code.IsEmpty)
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}
				ZQuery query = new ZQuery(OrgCusCodeSchema.OK_CodeType, codeType);
				query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, code);
				query.AddToFilter(OrgCusCodeSchema.OK_OA_PremisesAddress, SQLComparisonOperator.NotEqual, null);

				OrgCusCode cusCode = factory.LoadTop1<OrgCusCode>(query);
				if (cusCode != null)
				{
					result = cusCode.OK_OA_PremisesAddress;
				}
			}

			return result;
		}
	}
}
