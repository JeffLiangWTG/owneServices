using System.Collections.Generic;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ZA.Business
{
	public class TransportModeTranslator : Customs.Business.TransportModeTranslator
	{
		protected override Dictionary<string, string> CargoWiseToWCO
		{
			get
			{
				var result = base.CargoWiseToWCO;
				result.Add(string.Empty, TransportModeCodeList.Codes.Unknown);
				return result;
			}
		}

		protected override Dictionary<string, string> WCOToCargoWise
		{
			get
			{
				var result = base.WCOToCargoWise;
				result[TransportModeCodeList.Codes.Unknown] = string.Empty;
				return result;
			}
		}
	}
}
