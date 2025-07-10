//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTransportBookingInstructionPackageDivotLookups
//
//    This class should be used for overriding collections in AutoTransportBookingInstructionPackageDivotLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------
namespace Enterprise.TransportBookings.Business
{
	using Common = TransportCommon.Business.Common;

	public class DtbBookingInstructionPkgDivotLookups : Common.DtbBookingInstructionPkgDivotLookups
	{
		public DtbBookingInstructionPkgDivotLookups(DtbBookingInstructionPkgDivot parent)
			: base(parent)
		{
		}
	}
}
