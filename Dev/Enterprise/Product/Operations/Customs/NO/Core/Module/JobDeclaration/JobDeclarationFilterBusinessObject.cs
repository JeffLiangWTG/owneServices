using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Module;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NO.Module;

public class JobDeclarationFilterBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject
{
	public new JobDeclarationFilterLookups Lookups => (JobDeclarationFilterLookups)base.Lookups;

	protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups() => new JobDeclarationFilterLookups(this);

	protected override ModuleFilterCollection GetModuleFiltersCore()
	{
		var filters = base.GetModuleFiltersCore();
		AddPhaseStatusFilter(filters);
		return filters;
	}

	void AddPhaseStatusFilter(ModuleFilterCollection filters)
	{
		var phaseStatusFilter = new PhaseStatusTypeFilter(DeclarationFilterConstants.PhaseStatusText,
			new JobDeclarationEntryStatusFilterHelper(GetPhaseStatusQuery, JobDeclarationFilter.GetEntryPhaseStatusQueryAllEntries).GetEntryStatusFilter,
			GetPhaseStatusList,
			useFilterType: true)
			.WithMaxLengthOf<PhaseStatusTypeFilter>(CusEntryHeaderSchema.CH_PhaseStatus);

		phaseStatusFilter.MultilingualDescription = ResString.GetMultilingualString("270C01C8-0DDE-4A37-9598-0CA3A2E4EAA0", DeclarationFilterConstants.PhaseStatusText);

		filters.AddCustomFilter(phaseStatusFilter);
	}

	protected ZQuery GetPhaseStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
	{
		var result = new ZQuery();

		var entryHeaderSubQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
		entryHeaderSubQuery.AddToFilter(CusEntryHeaderSchema.CH_PhaseStatus, comparisonOperator, value);

		var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
		declarationQuery.AddSubQuery(entryHeaderSubQuery, JoinCondition.And);

		_ = result.AddToFilter(declarationQuery, JoinCondition.And);

		return result;
	}

	CodeDescriptionPairList GetPhaseStatusList() => Factory.GetCachedValue<CustomsEntryPhaseStatusList>();
}
