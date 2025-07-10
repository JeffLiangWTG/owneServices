//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCCarrierAndFIRMSLookups
//
//    This class should be used for overriding collections in AutoUSCCarrierAndFIRMSLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCCarrierAndFIRMSLookups : AutoUSCCarrierAndFIRMSLookups
	{
		public USCCarrierAndFIRMSLookups(AutoUSCCarrierAndFIRMS parent)
			: base(parent)
		{
		}
	}
}
