//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusPermitRuleExceptionLookups
//
//    This class should be used for overriding collections in AutoCusPermitRuleExceptionLookups
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
	public class CusPermitRuleExceptionLookups : AutoCusPermitRuleExceptionLookups
	{
		public CusPermitRuleExceptionLookups(AutoCusPermitRuleException parent) : base(parent)
		{
		}

		public new BaseCusPermitRuleException Parent => (BaseCusPermitRuleException)base.Parent;

		public virtual ICollection CPE_ValueFromList => Parent.PermitRule?.Lookups.CPR_ValueFromList ?? new CodeDescriptionPairList();
	}
}
