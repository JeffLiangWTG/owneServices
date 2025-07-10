using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	[DependentBusinessObject(typeof(CusPermitHeader), "CusPermitRules")]
	public class CusPermitRule : Customs.Business.BaseCusPermitRule, Integration.Customs.ZA.ICusPermitRule
	{
		public CusPermitRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CPR_RuleCode = PermitRuleCodeList.Codes.TAR;
		}

		#endregion
	}
}
