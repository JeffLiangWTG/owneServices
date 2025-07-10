using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using PermitRuleCodeList = Enterprise.Customs.EU.Business.PermitRuleCodeList;

namespace Enterprise.Customs.PL.Business;

public class CusGuaranteeRule : EU.Business.CusGuaranteeRule
{
	public CusGuaranteeRule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override Customs.Business.AccessCodePinRuleValidation AccessCodePinRuleValidation => new AccessCodePinRuleValidation(this);

	[MaxLength(nameof(ValueFromMaxLength))]
	public override ZString CPR_ValueFrom
	{
		get => base.CPR_ValueFrom;
		set => base.CPR_ValueFrom = value;
	}

	public int ValueFromMaxLength => CPR_RuleCode.ToString() switch
	{
		PermitRuleCodeListForAccessCodes.Codes.DefaultAccessCodeDefaultPin or PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber => AutoCusPermitHeader.Schema.CPH_AccessCodeOrPasswordMaxLength,
		PermitRuleCodeList.Codes.CUS => MaxCustomsOfficeLength,
		_ => Schema.CPR_ValueFromMaxLength
	};

	static int MaxCustomsOfficeLength => 8;
}
