//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoExportAWBAccountingInformationLookups
//
//    This class should be used for overriding collections in AutoExportAWBAccountingInformationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class ExportAWBAccountingInformationLookups : AutoExportAWBAccountingInformationLookups
	{
		public ExportAWBAccountingInformationLookups(AutoExportAWBAccountingInformation parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList AccountingCodes
		{
			get { return GetNewAccountingCodesList(); }
		}

		protected virtual CodeDescriptionPairList GetNewAccountingCodesList()
		{
			return new CodeDescriptionPairList(OLookUpEditType.AWBAccountingCodes);
		}
	}
}
