using CargoWise.Types;

namespace Enterprise.Customs.TR.Business
{
	public partial class TRMessageTypes
	{
		public static bool NeedToCreateInterchangeHeaderText(ZString messsageType)
		{
			return messsageType == Codes.TCD
				|| messsageType == Codes.TRB
				|| messsageType == Codes.TRD
				|| messsageType == Codes.TRE
				|| messsageType == Codes.TRI
				|| messsageType == Codes.TRL
				|| messsageType == Codes.TRQ
				|| messsageType == Codes.TRS
				|| messsageType == Codes.TSP;
		}

		public static bool NeedToPreprocessMessageText(ZString messsageType)
		{
			return messsageType == Codes.T1O || messsageType == Codes.T2O || messsageType == Codes.T3O || messsageType == Codes.TRM;
		}
	}
}
