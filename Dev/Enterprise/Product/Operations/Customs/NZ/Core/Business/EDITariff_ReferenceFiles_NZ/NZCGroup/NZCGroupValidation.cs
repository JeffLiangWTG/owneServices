//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCGroupValidation
//
//    This class should be used for overriding validation in AutoNZCGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCGroupValidation : AutoNZCGroupValidation
	{
		public NZCGroupValidation(AutoNZCGroup parent) : base(parent)
		{
		}
	}
}
