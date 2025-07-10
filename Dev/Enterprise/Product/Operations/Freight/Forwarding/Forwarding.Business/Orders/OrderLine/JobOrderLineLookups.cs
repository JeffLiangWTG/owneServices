//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobOrderLineLookups
//
//    This class should be used for overriding collections in AutoJobOrderLineLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobOrderLineLookups : AutoJobOrderLineLookups
	{
		public JobOrderLineLookups(AutoJobOrderLine parent) : base(parent)
		{
		}

		public override OrgHeaderCollection Suppliers
		{
			get { return new ConsignorCollection(Factory); }
		}

		public CodeDescriptionPairList JO_OuterPackUnitOfDimension_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Length); }
		}

		public OrgHeaderCollection OrgHeader_List
		{
			get => new OrgHeaderCollection(Factory);
		}
	}
}
