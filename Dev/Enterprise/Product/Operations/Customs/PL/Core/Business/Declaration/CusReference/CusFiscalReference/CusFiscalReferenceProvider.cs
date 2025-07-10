using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class CusFiscalReferenceProvider : EU.Business.Declaration.CusFiscalReferenceProvider
{
	protected CusFiscalReferenceProvider(ZString dataGroupingCode) : base(dataGroupingCode)
	{
	}

	protected override EU.Business.Declaration.CusFiscalReferenceLookups GetNewLookupsCore(EU.Business.Declaration.CusFiscalReference reference) =>
		new CusFiscalReferenceLookups(reference);

	protected override EU.Business.Declaration.CusFiscalReferenceValidation GetNewValidationCore(EU.Business.Declaration.CusFiscalReference reference) => new CusFiscalReferenceValidation(reference);

	protected override void RecalculateReferenceIfNeededCore(EU.Business.Declaration.CusFiscalReference reference)
	{
		var organisation = reference.Owner?.Header;
		if (organisation != null)
		{
			reference.CFR_Reference = organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.PolandCodeTypes.TIN);
		}
	}

	public override bool ReferenceIsReadOnly(EU.Business.Declaration.CusFiscalReference reference) => true;
}
