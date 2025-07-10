using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	partial class TermsOfDeliveryList
	{
		public static bool IsApplyForAgreedPlace(ZString code)
		{
			return Codes.CIP == code || Codes.CPT == code;
		}
	}
}
