using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusAuthorisationRuleCollection : ActiveBusinessObjectCollection<CusAuthorisationRule>
	{
		public CusAuthorisationRuleCollection(CusAuthorisationHeader master) : base(master.Factory, master, new ZQuery(CusPermitRuleSchema.CPR_CPR_Rule, SQLComparisonOperator.Equal, null), CusPermitRuleSchema.CPR_CPH_PermitHeader)
		{
		}

		protected override bool AllowNew => true;
	}
}
