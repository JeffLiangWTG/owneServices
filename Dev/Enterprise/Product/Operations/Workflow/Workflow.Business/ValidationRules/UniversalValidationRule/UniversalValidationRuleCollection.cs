using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	public class UniversalValidationRuleCollection : ActiveBusinessObjectCollection<UniversalValidationRule>
	{
		public UniversalValidationRuleCollection(UniversalValidationRuleSet parent)
			: base(parent.Factory, parent, new ZQuery(), UniversalValidationRuleSchema.VR_VRS_Parent)
		{
		}

		protected override void SetDefaultsForNewElementCore(UniversalValidationRule rule)
		{
			base.SetDefaultsForNewElementCore(rule);
			rule.VR_Sequence = this.MaxOrDefault(x => x.VR_Sequence) + 1;
		}
	}
}
