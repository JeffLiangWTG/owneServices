using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusPermitRuleCollection : ActiveBusinessObjectCollection<BaseCusPermitRule>
	{
		public CusPermitRuleCollection(BaseCusPermitHeader master)
			: base(master.Factory, master, new ZQuery(), CusPermitRuleSchema.CPR_CPH_PermitHeader)
		{
		}

		public CusPermitRuleCollection(BaseCusPermitHeader master, ZQuery query)
			: base(master.Factory, master, query, CusPermitRuleSchema.CPR_CPH_PermitHeader)
		{
		}

		public CusPermitRuleCollection(BaseCusPermitHeader master, ZQuery query, ZString ruleCode)
						: base(master.Factory, master, query, CusPermitRuleSchema.CPR_CPH_PermitHeader)
		{
			this.ruleCode = ruleCode;
		}
		readonly ZString ruleCode;

		protected override void SetDefaultsForNewElementCore(BaseCusPermitRule newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			if (!ruleCode.IsEmpty)
			{
				newElement.CPR_RuleCode = ruleCode;
			}
		}

		protected override bool AllowNew => true;
	}
}
