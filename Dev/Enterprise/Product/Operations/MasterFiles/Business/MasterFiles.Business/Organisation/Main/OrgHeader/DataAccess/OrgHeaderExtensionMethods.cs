using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class OrgHeaderExtensionMethods
	{
		public static ZString USLocalCustomsCarrierCode(this OrgHeader organisation, bool isTruck = false)
		{
			var result = ZString.Empty;
			if (organisation != null)
			{
				result = isTruck ?
				organisation.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, new ZString[] { OrgCusCode.CodeTypes.TruckCarrierCode, OrgCusCode.CodeTypes.CarrierCode }) :
				organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.UnitedStates);
			}
			return result;
		}

		public static IEnumerable<ZString> USLocalCustomsCarrierCodes(this OrgHeader organisation, bool isTruck = false)
		{
			if (organisation != null)
			{
				if (isTruck)
				{
					var cct = organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.TruckCarrierCode, Core.Constants.CountryCodes.UnitedStates);
					if (!cct.IsEmpty)
					{
						yield return cct;
					}
				}

				var ccp = organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.CarrierPrefixCode, Core.Constants.CountryCodes.UnitedStates);
				if (!ccp.IsEmpty)
				{
					yield return ccp;
				}

				var ccc = organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.UnitedStates);
				if (!ccc.IsEmpty)
				{
					yield return ccc;
				}
			}
		}
	}
}
