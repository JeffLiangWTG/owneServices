//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgInvTypeDeferredChargesLookups
//
//    This class should be used for overriding collections in AutoOrgInvTypeDeferredChargesLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgInvTypeDeferredChargesLookups : AutoOrgInvTypeDeferredChargesLookups
	{
		public OrgInvTypeDeferredChargesLookups(AutoOrgInvTypeDeferredCharges parent)
			: base(parent)
		{
		}

		#region Charge Groups

		public CodeDescriptionPairList ChargeGroupList
		{
			get { return new ChargeCodeGroupList(); }
		}

		#endregion
	}
}
