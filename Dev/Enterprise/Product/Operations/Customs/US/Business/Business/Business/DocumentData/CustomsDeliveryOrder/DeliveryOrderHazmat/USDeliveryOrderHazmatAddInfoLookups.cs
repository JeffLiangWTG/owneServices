//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSDeliveryOrderHazmatAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSDeliveryOrderHazmatAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;
namespace Enterprise.Customs.US.Business
{
	public class USDeliveryOrderHazmatAddInfoLookups : AutoUSDeliveryOrderHazmatAddInfoLookups
	{
		public USDeliveryOrderHazmatAddInfoLookups(AutoUSDeliveryOrderHazmatAddInfo parent)
			: base(parent)
		{
		}

		public UNDGSubstanceCollection UNDGSubstances
		{
			get { return new UNDGSubstanceCollection(Factory); }
		}

		protected new USDeliveryOrderHazmatAddInfo Parent
		{
			get { return (USDeliveryOrderHazmatAddInfo)base.Parent; }
		}
	}
}
