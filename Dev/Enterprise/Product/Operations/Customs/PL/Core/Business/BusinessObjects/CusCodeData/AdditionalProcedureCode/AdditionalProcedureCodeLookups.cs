using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;

public class AdditionalProcedureCodeLookups : EU.Business.AdditionalProcedureCodeLookups
{
	public AdditionalProcedureCodeLookups(EU.Business.AdditionalProcedureCode officeCode) : base(officeCode)
	{
	}

	public override CodeDescriptionPairList CY_CodeList => Parent?.ParentAsJobComInvoiceLine?.Lookups.ConcessionCodes ?? new CodeDescriptionPairList();

	protected new AdditionalProcedureCode Parent => (AdditionalProcedureCode)base.Parent;
}
