//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoExportAWBSpecialHandlingLookups
//
//    This class should be used for overriding collections in AutoExportAWBSpecialHandlingLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class ExportAWBSpecialHandlingLookups : AutoExportAWBSpecialHandlingLookups
	{
		public ExportAWBSpecialHandlingLookups(AutoExportAWBSpecialHandling parent)
			: base(parent)
		{
		}

		public virtual AWBSpecialHandlingCodeDescriptionPairList SpecialHandlingCodeDescriptionList
		{
			get
			{
				return new AWBSpecialHandlingCodeDescriptionPairList();
			}
		}

		public virtual AWBSpecialHandlingCodeDescriptionPairList SpecialHandlingCodeDescriptionListInAirLine
		{
			get
			{
				var specialHandlingCodeDescriptionPairList = new AWBSpecialHandlingCodeDescriptionPairList();
				specialHandlingCodeDescriptionPairList.Clear();
				return specialHandlingCodeDescriptionPairList;
			}
		}
	}
}
