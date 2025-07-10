//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoExportAWBRateLineLookups
//
//    This class should be used for overriding collections in AutoExportAWBRateLineLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class ExportAWBRateLineLookups : AutoExportAWBRateLineLookups
	{
		public ExportAWBRateLineLookups(AutoExportAWBRateLine parent)
			: base(parent)
		{
		}
	}
}
