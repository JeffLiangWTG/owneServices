//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobVoyOriginValidation
//
//    This class should be used for overriding validation in AutoJobVoyOriginValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Common.Business
{
	public class JobVoyOriginValidation : AutoJobVoyOriginValidation
	{
		public JobVoyOriginValidation(AutoJobVoyOrigin parent)
			: base(parent) { }
	}
}
