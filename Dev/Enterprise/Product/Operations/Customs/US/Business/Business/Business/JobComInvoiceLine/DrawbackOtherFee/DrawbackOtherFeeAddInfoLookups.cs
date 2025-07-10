//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDrawbackOtherFeeAddInfoLookups
//
//    This class should be used for overriding collections in AutoDrawbackOtherFeeAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class DrawbackOtherFeeAddInfoLookups : AutoDrawbackOtherFeeAddInfoLookups
	{
		public DrawbackOtherFeeAddInfoLookups(AutoDrawbackOtherFeeAddInfo parent) : base(parent)
		{
		}

		public DrawbackOtherFeeTypesList OtherFeeTypes => Factory.GetCachedValue<DrawbackOtherFeeTypesList>();
	}
}
