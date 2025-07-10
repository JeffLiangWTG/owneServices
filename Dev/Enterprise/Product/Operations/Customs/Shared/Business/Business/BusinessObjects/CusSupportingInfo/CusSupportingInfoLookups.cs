//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusSupportingInfoLookups
//
//    This class should be used for overriding collections in AutoCusSupportingInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusSupportingInfoLookups : AutoCusSupportingInfoLookups
	{
		public CusSupportingInfoLookups(AutoCusSupportingInfo parent)
			: base(parent)
		{
		}

		protected new CusSupportingInfo Parent => (CusSupportingInfo)base.Parent;

		public virtual ICollection CodeList => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList CustomsOfficeList => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList ProcedureList => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList StatusList => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList SubTypeList => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList TypeList => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList UnitOfQuantityList => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList UnitOfQuantity2List => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList UnitOfQuantity3List => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList IssuerTypeList => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList PackTypeList => new CodeDescriptionPairList();
	}
}
