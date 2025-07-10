//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRouteSegmentValidation
//
//    This class should be used for overriding validation in AutoRouteSegmentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.OceanCarrier.Business
{
	public sealed class RouteSegmentValidation : AutoRouteSegmentValidation
	{
		public RouteSegmentValidation(AutoRouteSegment parent) : base(parent)
		{
		}
	}
}
