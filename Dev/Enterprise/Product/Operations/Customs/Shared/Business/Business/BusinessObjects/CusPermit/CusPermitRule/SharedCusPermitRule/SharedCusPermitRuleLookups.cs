using System.Collections;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public abstract class SharedCusPermitRuleLookups : CusPermitRuleLookups
	{
		public SharedCusPermitRuleLookups(SharedCusPermitRule parent) : base(parent)
		{
		}

		public new SharedCusPermitRule Parent => (SharedCusPermitRule)base.Parent;

		public SharedCusPermitHeader PermitHeader => Parent.PermitHeader;

		public virtual CodeDescriptionPairList PermitRuleCodes => PermitHeader?.GetCountrySpecificInstruction()?.GetRuleCodeList(PermitHeader.CPH_Type, PermitHeader.CPH_SubType);

		public virtual ICollection CPR_ValueFromList => Parent.PermitHeader?.GetCountrySpecificInstruction()?.GetLookupList(PermitHeader, Parent.CPR_RuleCode);

		public virtual ICollection CPR_ValueToList => new CodeDescriptionPairList();
	}
}
