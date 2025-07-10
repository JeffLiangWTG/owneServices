//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSHFCHeaderAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSHFCHeaderAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USHFCHeaderAddInfoLookups : AutoUSHFCHeaderAddInfoLookups
	{
		public USHFCHeaderAddInfoLookups(AutoUSHFCHeaderAddInfo parent) : base(parent)
		{
		}

		public CodeDescriptionPairList HFCCertifyingIndividualList
		{
			get
			{
				return Factory.GetCachedValue("HFCCertifyingIndividualList", delegate
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(EntityRoleCodeList.Codes.Importer, EntityRoleCodeList.Descriptions.Importer);
					list.AddPair(EntityRoleCodeList.Codes.Consignee, EntityRoleCodeList.Descriptions.Consignee);
					list.AddPair(EntityRoleCodeList.Codes.CustomsBroker, EntityRoleCodeList.Descriptions.CustomsBroker);
					return list;
				});
			}
		}
	}
}

