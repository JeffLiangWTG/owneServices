using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CustomsRuleRuleCollection : ActiveBusinessObjectCollection<CustomsRuleRule>
	{
		public CustomsRuleRuleCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public CustomsRuleRuleCollection(CustomsRule master)
			: base(master.Factory, master, new ZQuery(), CusPermitRuleSchema.CPR_CPH_PermitHeader)
		{
		}
	}
}
