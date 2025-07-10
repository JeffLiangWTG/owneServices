using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class OfficeCode : EuOfficeCode
{
	public OfficeCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[List(nameof(Lookups) + "." + nameof(OfficeCodeLookups.OfficeTypeLookupList))]
	public override ZString CY_Code
	{
		get => base.CY_Code;
		set => base.CY_Code = value;
	}

	public new OfficeCodeLookups Lookups => (OfficeCodeLookups)base.Lookups;

	protected override CusCodeDataLookups GetNewLookups() => new OfficeCodeLookups(this);

	protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobDeclaration));

	protected override CusCodeDataValidation GetNewValidation()
	{
		var declaration = Declaration;
		return declaration?.IsExport ?? false
			? new ExportJobDeclarationOfficeCodeValidation(this)
			: declaration?.IsImport ?? false
				? new ImportJobDeclarationOfficeCodeValidation(this)
				: new OfficeCodeValidation(this);
	}

	internal JobDeclaration Declaration => (JobDeclaration)Parent;
}
