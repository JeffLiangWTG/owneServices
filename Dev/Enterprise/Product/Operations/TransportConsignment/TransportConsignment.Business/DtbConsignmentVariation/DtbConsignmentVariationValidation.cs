//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDtbConsignmentVariationValidation
//
//    This class should be used for overriding validation in AutoDtbConsignmentVariationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentVariationValidation : AutoDtbConsignmentVariationValidation
	{
		public DtbConsignmentVariationValidation(AutoDtbConsignmentVariation parent) : base(parent)
		{
		}

		protected override void CheckLTV_Status()
		{
			base.CheckLTV_Status();

			ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.LTV_StatusInfo);
		}

		protected override void CheckLTV_Type()
		{
			base.CheckLTV_Type();

			ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.LTV_TypeInfo);
		}
	}
}
