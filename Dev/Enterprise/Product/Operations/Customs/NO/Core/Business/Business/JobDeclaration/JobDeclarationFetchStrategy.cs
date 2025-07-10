using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NO.Business;

sealed class JobDeclarationFetchStrategy(JobDeclaration declaration) : Customs.Business.FetchStrategies.BaseJobDeclarationFetchStrategy(declaration)
{
	protected override void FetchForViewDeclaration(TableColumn[] columns)
	{
		base.FetchForViewDeclaration(columns);
		foreach (var tableColumn in columns)
		{
			if (tableColumn is { ColumnName: JobDeclaration.Schema.PhaseStatus } or { ColumnName: JobDeclaration.Schema.PhaseStatusDescription })
			{
				Factory.AddFetchHint(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, BusinessObject.PK);
			}
		}
	}
}
