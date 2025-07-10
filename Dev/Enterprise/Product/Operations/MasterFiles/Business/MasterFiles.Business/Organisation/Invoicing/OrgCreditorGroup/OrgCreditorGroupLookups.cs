//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCreditorGroupLookups
//
//    This class should be used for overriding collections in AutoOrgCreditorGroupLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCreditorGroupLookups : AutoOrgCreditorGroupLookups
	{
		public OrgCreditorGroupLookups(AutoOrgCreditorGroup parent) : base(parent)
		{
		}

		#region Hold Options

		public CodeDescriptionPairList HoldOptions
		{
			get
			{
				return AccountingMasterFilesConstants.CreditorGroupConstants.HoldOptions;
			}
		}

		#endregion
	}
}
