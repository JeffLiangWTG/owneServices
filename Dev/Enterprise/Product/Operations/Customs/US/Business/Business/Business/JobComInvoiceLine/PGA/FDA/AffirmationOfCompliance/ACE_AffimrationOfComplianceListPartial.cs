using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	partial class ACE_AffirmationOfComplianceList
	{
		public static bool CanBeSentWithPNS(ZString code)
		{
			var result = code == Codes.SFR
				|| code == Codes.UFR
				|| code == Codes.IFR
				|| code == Codes.TFR
				|| code == Codes.ORN
				|| code == Codes.SRN
				|| code == Codes.CFR
				|| code == Codes.GFR
				|| code == Codes.LFR
				|| code == Codes.RNO
				|| code == Codes.CAN
				|| code == Codes.VFT
				|| code == Codes.VES
				|| code == Codes.PFR
				|| code == Codes.FME;
			return result;
		}
	}
}
