using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusCalculationRuleLookups : AutoCusCalculationRuleLookups
	{
		public CusCalculationRuleLookups(AutoCusCalculationRule parent) : base(parent)
		{
		}

		public virtual CodeDescriptionPairList RuleTypeList => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList TransportModeList => new CodeDescriptionPairList();
	}
}
