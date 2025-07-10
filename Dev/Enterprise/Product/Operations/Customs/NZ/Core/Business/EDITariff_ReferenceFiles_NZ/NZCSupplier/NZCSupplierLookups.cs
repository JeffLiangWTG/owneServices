//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCSupplierLookups
//
//    This class should be used for overriding collections in AutoNZCSupplierLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCSupplierLookups : AutoNZCSupplierLookups
	{
		public NZCSupplierLookups(AutoNZCSupplier parent)
			: base(parent)
		{
		}

		public RefCountryCollection CountryList
		{
			get { return new RefCountryCollection(Factory); }
		}
	}
}
