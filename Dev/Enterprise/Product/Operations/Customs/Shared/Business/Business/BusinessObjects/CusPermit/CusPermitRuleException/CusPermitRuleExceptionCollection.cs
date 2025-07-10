using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusPermitRuleExceptionCollection : ActiveBusinessObjectCollection<BaseCusPermitRuleException>
	{
		public CusPermitRuleExceptionCollection(SharedCusPermitRule master)
			: base(master.Factory, master, new ZQuery(), CusPermitRuleExceptionSchema.CPE_CPR_PermitRule)
		{
		}

		protected override bool AllowNew => true;
	}
}
