//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusInBondHeaderLookups
//
//    This class should be used for overriding collections in AutoCusInBondHeaderLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusInBondHeaderLookups : AutoCusInBondHeaderLookups
	{
		public CusInBondHeaderLookups(AutoCusInBondHeader parent)
			: base(parent)
		{
		}

		public override GlbBranchCollection Branches
		{
			get { return new GlbBranchCollection(Factory, new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK)); }
		}

		public override OrgHeaderCollection Carriers
		{
			get { return new OrgHeaderCollection(Factory, new ZQuery(OrgHeaderSchema.OH_IsShippingProvider, true)); }
		}
	}
}
