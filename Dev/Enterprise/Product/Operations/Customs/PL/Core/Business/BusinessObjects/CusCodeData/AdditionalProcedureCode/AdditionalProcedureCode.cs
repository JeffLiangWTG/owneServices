using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.PL.Business.Declaration;

public class AdditionalProcedureCode : EU.Business.AdditionalProcedureCode
{
	public AdditionalProcedureCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public JobComInvoiceLine ParentAsJobComInvoiceLine => Parent as JobComInvoiceLine;

	public new AdditionalProcedureCodeValidation Validation => (AdditionalProcedureCodeValidation)base.Validation;
	protected override CusCodeDataValidation GetNewValidation()
	{
		return IsImport() ? new ImportAdditionalProcedureCodeValidation(this) : new AdditionalProcedureCodeValidation(this);
	}

	protected override CusCodeDataLookups GetNewLookups() => new AdditionalProcedureCodeLookups(this);

	public new AdditionalProcedureCodeLookups Lookups => (AdditionalProcedureCodeLookups)base.Lookups;

	ZBool IsImport() => ((ICanBeImportOrExport)Parent)?.IsImport ?? false;
}
