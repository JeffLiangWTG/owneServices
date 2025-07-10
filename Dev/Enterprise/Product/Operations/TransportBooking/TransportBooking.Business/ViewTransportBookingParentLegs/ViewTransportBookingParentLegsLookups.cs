//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewTransportBookingParentLegsLookups
//
//    This class should be used for overriding collections in AutoViewTransportBookingParentLegsLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;
namespace Enterprise.TransportBookings.Business
{
	public class ViewTransportBookingParentLegsLookups : AutoViewTransportBookingParentLegsLookups
	{
		public ViewTransportBookingParentLegsLookups(AutoViewTransportBookingParentLegs parent)
			: base(parent) { }

		public RefUNLOCOCollection RefUNLOCOs
		{
			get { return refUNLOCOs ?? (refUNLOCOs = new RefUNLOCOCollection(Factory)); }
		}
		RefUNLOCOCollection refUNLOCOs;

		public RefVesselCollection RefVessels
		{
			get { return refVessels ?? (refVessels = new RefVesselCollection(Factory)); }
		}
		RefVesselCollection refVessels;
	}
}
