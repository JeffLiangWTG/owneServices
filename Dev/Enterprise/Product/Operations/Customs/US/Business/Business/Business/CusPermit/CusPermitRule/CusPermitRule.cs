using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class CusPermitRule : BaseCusPermitRule, Integration.Customs.US.ICusPermitRule
	{
		public CusPermitRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Override Properties

		public override ZString CPR_RuleCode
		{
			get { return base.CPR_RuleCode; }
			set
			{
				var hasChanged = value != base.CPR_RuleCode;
				base.CPR_RuleCode = value;
				if (hasChanged)
				{
					PermitHeader?.Validation.ValidateCPH_Type();
				}
			}
		}

		public override ZGuid CPR_CPH_PermitHeader
		{
			get { return base.CPR_CPH_PermitHeader; }
			set
			{
				var hasChanged = value != base.CPR_CPH_PermitHeader;
				base.CPR_CPH_PermitHeader = value;
				if (hasChanged)
				{
					PermitHeader?.Validation.ValidateCPH_Type();
				}
			}
		}

		#endregion

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CPR_RuleCode = USPermitRuleCodeList.Codes.TAR;
		}

		protected override CusPermitRuleValidation GetNewValidation()
		{
			switch (CPR_RuleCode)
			{
				case USPermitRuleCodeList.Codes.ZST:
					return new FTZZoneStatusPermitRuleValidation(this);
				default:
					return base.GetNewValidation();
			}
		}

		#endregion
	}
}
