//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSAIILineAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSAIILineAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class USAIILineAddInfoLookups : AutoUSAIILineAddInfoLookups
	{
		public USAIILineAddInfoLookups(AutoUSAIILineAddInfo parent)
			: base(parent)
		{
		}

		public TSCAIndicatorList US_TSCAIndicatorList => Factory.GetCachedValue<TSCAIndicatorList>();

		public QtyDiffReasonList US_QtyDiffReasonCodeList => Factory.GetCachedValue<QtyDiffReasonList>();

		public ABIUnitOfMeasureList US_UnitOfMeasureList => Factory.GetCachedValue<ABIUnitOfMeasureList>();

		public RefCurrencyCollection CurrencyList => new RefCurrencyCollection(Factory);
	}
}
