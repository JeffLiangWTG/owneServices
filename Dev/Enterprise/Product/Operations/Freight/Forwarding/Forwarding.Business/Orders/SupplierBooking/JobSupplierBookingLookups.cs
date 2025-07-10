//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobSupplierBookingLookups
//
//    This class should be used for overriding collections in AutoJobSupplierBookingLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobSupplierBookingLookups : AutoJobSupplierBookingLookups
	{
		public JobSupplierBookingLookups(AutoJobSupplierBooking parent) : base(parent)
		{
		}

		#region Organisations

		public virtual OrgHeaderCollection Organisations
		{
			get
			{
				return new OrgHeaderCollection(Factory);
			}
		}

		#endregion
	}
}
