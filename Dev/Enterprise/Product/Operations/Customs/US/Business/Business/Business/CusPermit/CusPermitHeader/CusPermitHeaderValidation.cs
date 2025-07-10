using System.Linq;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CusPermitHeaderValidation : BaseCusPermitHeaderValidation
	{
		public CusPermitHeaderValidation(CusPermitHeader parent) : base(parent)
		{
		}

		public new CusPermitHeader Parent
		{
			get { return (CusPermitHeader)base.Parent; }
		}

		protected override void CheckCPH_Type()
		{
			base.CheckCPH_Type();
			if (Parent.CPH_Type == PermitTypeList.Codes.FTZ && !Parent.CusPermitRules.OfType<CusPermitRule>().Any(x => x.CPR_RuleCode == USPermitRuleCodeList.Codes.TAR))
			{
				Parent.CPH_TypeInfo.AddError(TariffPermitRequiredForFTZ);
			}
		}

		internal static string TariffPermitRequiredForFTZ => Res.GetString("8D45B99F-96F4-442F-9F92-915EEA11EAD9", "FTZ (Foreign Trading Zone) permits require at least one TAR (Tariff) rule.");
	}
}
