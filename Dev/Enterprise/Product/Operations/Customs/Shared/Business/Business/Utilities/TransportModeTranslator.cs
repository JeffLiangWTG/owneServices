using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.Business
{
	public class TransportModeTranslator
	{
		public ZString TranslateToWCOCode(ZString input, bool returnOriginalInput = false)
		{
			if (CargoWiseToWCO.TryGetValue(input, out var result))
			{
				return result;
			}
			else
			{
				return returnOriginalInput ? input : ZString.Empty;
			}
		}

		public ZString TranslateToCargoWiseCode(ZString input, bool returnOriginalInput = false)
		{
			if (WCOToCargoWise.TryGetValue(input, out var result))
			{
				return result;
			}
			else
			{
				return returnOriginalInput ? input : ZString.Empty;
			}
		}

		protected virtual Dictionary<string, string> CargoWiseToWCO => new Dictionary<string, string>()
		{
			{ Core.Constants.TransportModes.Air,  TransportModeCodeList.Codes.Air },
			{ Core.Constants.TransportModes.FixedTransportInstallations,  TransportModeCodeList.Codes.FixedInstallations },
			{ Core.Constants.TransportModes.InlandWaterwayTransport,  TransportModeCodeList.Codes.InlandWater },
			{ Core.Constants.TransportModes.Mail,  TransportModeCodeList.Codes.Mail },
			{ Core.Constants.TransportModes.Other,  TransportModeCodeList.Codes.Other },
			{ Core.Constants.TransportModes.Rail,  TransportModeCodeList.Codes.Rail },
			{ Core.Constants.TransportModes.Road,  TransportModeCodeList.Codes.Road },
			{ Core.Constants.TransportModes.Sea,  TransportModeCodeList.Codes.Sea },
			{ Core.Constants.TransportModes.Unknown,  TransportModeCodeList.Codes.Unknown },
		};

		protected virtual Dictionary<string, string> WCOToCargoWise => new Dictionary<string, string>()
		{
			{ TransportModeCodeList.Codes.Air, Core.Constants.TransportModes.Air },
			{ TransportModeCodeList.Codes.FixedInstallations, Core.Constants.TransportModes.FixedTransportInstallations },
			{ TransportModeCodeList.Codes.InlandWater, Core.Constants.TransportModes.InlandWaterwayTransport },
			{ TransportModeCodeList.Codes.Mail, Core.Constants.TransportModes.Mail },
			{ TransportModeCodeList.Codes.Other, Core.Constants.TransportModes.Other },
			{ TransportModeCodeList.Codes.Rail, Core.Constants.TransportModes.Rail },
			{ TransportModeCodeList.Codes.Road, Core.Constants.TransportModes.Road },
			{ TransportModeCodeList.Codes.Sea, Core.Constants.TransportModes.Sea },
			{ TransportModeCodeList.Codes.Unknown, Core.Constants.TransportModes.Unknown },
		};
	}
}
