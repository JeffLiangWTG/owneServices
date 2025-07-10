//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusUSLVItemPGAValidation
//
//    This class should be used for overriding validation in AutoCusUSLVItemPGAValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVItemPGAValidation : AutoCusUSLVItemPGAValidation
	{
		public CusUSLVItemPGAValidation(AutoCusUSLVItemPGA parent) : base(parent)
		{
		}

		protected override void CheckULP_DisclaimReason()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ULP_DisclaimReasonInfo);

			var pga = Parent as CusUSLVItemPGA;
			var item = pga.ParentItem;
			AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.ULP_DisclaimReasonInfo, OGAIndicatorList.Codes.Disclaimed, item.PGARequirementIndicator.IsPGAProgramRequired(pga.AgencyCode));
		}
	}
}
