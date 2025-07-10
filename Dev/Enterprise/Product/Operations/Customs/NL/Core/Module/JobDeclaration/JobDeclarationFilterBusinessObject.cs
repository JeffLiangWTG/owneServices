using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.NL.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.NL.Module;

public class JobDeclarationFilterBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject
{
	public new JobDeclarationFilterLookups Lookups => (JobDeclarationFilterLookups)base.Lookups;

	protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups() => new JobDeclarationFilterLookups(this);

	protected override ModuleFilterCollection GetModuleFiltersCore()
	{
		var filters = base.GetModuleFiltersCore();
		AddEntryPhaseStatusFilter(filters);
		return filters;
	}

	void AddEntryPhaseStatusFilter(ModuleFilterCollection filters)
	{
		var showFilterType = DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(GlbCompany.CurrentCompany.Country.RN_Code, GlbCompany.CurrentCompany.PK);
		var phaseStatusFilter = new EntryStatusFilter(DeclarationFilterConstants.PhaseStatusText,
			new JobDeclarationEntryStatusFilterHelper(GetEntryPhaseStatusQuery, JobDeclarationFilter.GetEntryPhaseStatusQueryAllEntries).GetEntryStatusFilter, Lookups.EntryPhaseStatusList, useFilterType: true).WithMaxLengthOf<EntryStatusFilter>(CusEntryHeaderSchema.CH_PhaseStatus);
		phaseStatusFilter.ShowFilterType = showFilterType;
		phaseStatusFilter.MultilingualDescription = ResString.GetMultilingualString("825FB869-46F6-4DBA-8C3D-62477766A67B", DeclarationFilterConstants.PhaseStatusText);
		phaseStatusFilter.ComparisonOperatorChanged += CustomsEntryStatusFilter_ComparisonOperatorChanged;
		filters.AddCustomFilter(phaseStatusFilter);
	}

	protected ZQuery GetEntryPhaseStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
	{
		if (value == DeclarationFilterConstants.EntryStatus.NotSentForFilter)
		{
			value = ZString.Empty;
		}
		var result = new ZQuery();
		var entryHeaderResult = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
		var cusEntryFilter = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
		cusEntryFilter.AddToFilter(CusEntryHeaderSchema.CH_PhaseStatus, comparisonOperator, value);
		entryHeaderResult.AddSubQuery(cusEntryFilter, JoinCondition.And);
		result.AddToFilter(entryHeaderResult, JoinCondition.Or);
		return result;
	}

	protected override void AddMessageStatusFilter(ModuleFilterCollection filters)
	{
		var messageStatusFilter = new EntryStatusFilter(
			DeclarationFilterConstants.MessageStatusText,
			new JobDeclarationEntryStatusFilterHelper(GetMessageStatusQuery, JobDeclarationFilter.GetMessageStatusQueryAllEntries).GetEntryStatusFilter,
			Lookups.MessageStatusList,
			useFilterType: true
		).WithMaxLengthOf<EntryStatusFilter>(CusEntryHeaderSchema.CH_Status);

		messageStatusFilter.ShowFilterType = true;
		messageStatusFilter.MultilingualDescription = ResString.GetMultilingualString(
			"5C9B2DD5-EE9E-465F-80CC-64EFFBBDF7F2",
			DeclarationFilterConstants.MessageStatusText
		);
		messageStatusFilter.ComparisonOperatorChanged += CustomsEntryStatusFilter_ComparisonOperatorChanged;

		filters.AddCustomFilter(messageStatusFilter);
	}

	protected override bool ShouldExcludeComparisonOperatorsFromMessageStatusFilter => false;
}
