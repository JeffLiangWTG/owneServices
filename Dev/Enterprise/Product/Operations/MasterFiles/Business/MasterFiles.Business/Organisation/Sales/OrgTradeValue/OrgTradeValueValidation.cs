//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgTradeValueValidation
//
//    This class should be used for overriding validation in AutoOrgTradeValueValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class OrgTradeValueValidation : AutoOrgTradeValueValidation
	{
		public OrgTradeValueValidation(AutoOrgTradeValue parent) : base(parent)
		{
		}
	}
}
