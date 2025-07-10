using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Universal
{
	public static class RefCountryStatesExtensionMethods
	{
		public static ZString GetCustomsCodeFor(this RefCountryStates stateCode, ZString mapType, ZString dataGroupingCode)
		{
			var inputStateCode = stateCode.RW_RN_NKCountryCode + stateCode.RW_Code;
			var result = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(stateCode.Factory, dataGroupingCode, mapType, inputStateCode, ZDateTime.Empty);
			if (result.IsEmpty)
			{
				result = stateCode.RW_Code;
			}
			return result;
		}
	}
}
