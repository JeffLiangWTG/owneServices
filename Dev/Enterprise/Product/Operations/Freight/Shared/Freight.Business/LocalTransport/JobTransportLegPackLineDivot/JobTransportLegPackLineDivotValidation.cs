//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobTransportLegPackLineDivotValidation
//
//    This class should be used for overriding validation in AutoJobTransportLegPackLineDivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Business
{
	public class JobTransportLegPackLineDivotValidation : AutoJobTransportLegPackLineDivotValidation
	{
		public JobTransportLegPackLineDivotValidation(AutoJobTransportLegPackLineDivot parent) : base(parent)
		{
		}
	}
}
