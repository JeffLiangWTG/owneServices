//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobComInvoiceLineTaxLookups
//
//    This class should be used for overriding collections in AutoJobComInvoiceLineTaxLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class JobComInvoiceLineTaxLookups : AutoJobComInvoiceLineTaxLookups
	{
		public JobComInvoiceLineTaxLookups(AutoJobComInvoiceLineTax parent) : base(parent)
		{
		}

		public virtual CodeDescriptionPairList RateOverrideList => new CodeDescriptionPairList();
		public virtual CodeDescriptionPairList MOPList => new CodeDescriptionPairList();
		public virtual CodeDescriptionPairList MethodOfCalculationList => new CodeDescriptionPairList();
		public virtual ICodeDescriptionPairList TypeList => new CodeDescriptionPairList();
		public virtual CodeDescriptionPairList BaseQuantityUQList => new CodeDescriptionPairList();
	}
}
