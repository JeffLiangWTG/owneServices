//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCusAccountLookups
//
//    This class should be used for overriding collections in AutoOrgCusAccountLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections;
using CargoWise.Common.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCusAccountLookups : AutoOrgCusAccountLookups
	{
		public OrgCusAccountLookups(AutoOrgCusAccount parent) : base(parent)
		{
		}

		[SuppressWeaklyTypedCollectionMessage] // This lookup needs to be a ZZRefCusCodeListCombinedCollection in FR
		public virtual IList IssuerList => Factory.GetCachedValue<CodeDescriptionPairList>();

		public virtual CodeDescriptionPairList CodeList => Factory.GetCachedValue<CodeDescriptionPairList>();

		public virtual CodeDescriptionPairList AccountList => Factory.GetCachedValue<CodeDescriptionPairList>();

		public virtual CodeDescriptionPairList AccountTypeList => Factory.GetCachedValue<CodeDescriptionPairList>();

		public virtual CodeDescriptionPairList ReportingPeriodList => Factory.GetCachedValue<CodeDescriptionPairList>();
	}
}
