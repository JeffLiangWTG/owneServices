//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccOrgTaxRateLookups
//
//    This class should be used for overriding collections in AutoAccOrgTaxRateLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccOrgTaxRateLookups : AutoAccOrgTaxRateLookups
	{
		public AccOrgTaxRateLookups(AutoAccOrgTaxRate parent) : base(parent)
		{
		}

		public CodeDescriptionPairList Source => new AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods();
	}
}
