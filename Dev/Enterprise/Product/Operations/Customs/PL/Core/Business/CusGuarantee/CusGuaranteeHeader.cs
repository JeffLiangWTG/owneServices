using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business;

public class CusGuaranteeHeader : EU.Business.CusGuaranteeHeader
{
	public CusGuaranteeHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusGuaranteeHeaderValidation Validation => (CusGuaranteeHeaderValidation)base.Validation;

	protected override CusPermitHeaderValidation GetNewValidation() => new CusGuaranteeHeaderValidation(this);

	[MaxLength(AutoCusPermitHeader.Schema.CPH_AccessCodeOrPasswordMaxLength)]
	public override ZString MainAccessCode
	{
		get => base.MainAccessCode;
		set => base.MainAccessCode = value;
	}
}
