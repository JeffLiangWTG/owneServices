using CargoWise.Types;
using CoreConstants = Enterprise.Core.Constants;

namespace Enterprise.Customs.Universal.Helper
{
	public static class CodeListHelper
	{
		public static ZString TranslateToWCOContainerModeCode(this ZString input)
		{
			var result = ZString.Empty;
			switch (input)
			{
				case CoreConstants.ContainerModes.Empty:
					result = ContainerModeCodeList.Codes.Empty;
					break;
				case CoreConstants.ContainerModes.FCL:
					result = ContainerModeCodeList.Codes.FullSingleConsignment;
					break;
				case CoreConstants.ContainerModes.LCL:
					result = ContainerModeCodeList.Codes.FullMixedConsignment;
					break;
				case "FCG":
					result = ContainerModeCodeList.Codes.Full;
					break;
			}
			return result;
		}

		/// <summary>
		/// e.g. "IMP" --> "23" 
		/// </summary>
		/// <param name="wtgCode"></param>
		/// <returns></returns>
		public static ZString ConvertWtgShipmentTypeCodeToAsycudaBolNatureCode(this ZString wtgCode)
		{
			switch (wtgCode)
			{
				case ShipmentTypeList.Codes.Export22:
					return BolNatureList.Codes.Export;
				case ShipmentTypeList.Codes.Import23:
					return BolNatureList.Codes.Import;
				case ShipmentTypeList.Codes.Transit24:
					return BolNatureList.Codes.Transit;
				case ShipmentTypeList.Codes.Transhipment28:
					return BolNatureList.Codes.Transhipment;
			}
			return ZString.Empty;
		}
	}
}
