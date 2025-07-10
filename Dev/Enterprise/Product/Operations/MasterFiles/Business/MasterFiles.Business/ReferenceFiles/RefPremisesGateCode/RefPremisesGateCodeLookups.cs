//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefPremisesGateCodeLookups
//
//    This class should be used for overriding collections in AutoRefPremisesGateCodeLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RefPremisesGateCodeLookups : AutoRefPremisesGateCodeLookups
	{
		public RefPremisesGateCodeLookups(AutoRefPremisesGateCode parent) : base(parent)
		{
		}

		#region Lists

		public CodeDescriptionPairList R5_DataProvider_List
		{
			get { return new PremiseGateCodeDataProviderList(); }
		}

		public CodeDescriptionPairList R5_OrgRegCode_List
		{
			get { return new PremiseGateCodeDataProviderList(); }
		}

		#endregion

	}
}
