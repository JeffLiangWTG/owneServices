using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Module;

sealed class PhaseStatusTypeFilter : EntryStatusFilter
{
	public PhaseStatusTypeFilter(ZString description,
		GetEntryStatusQueryDelegate queryDelegate,
		Func<CodeDescriptionPairList> getEntryStatusList,
		bool useFilterType = true)
		: base(description, queryDelegate, getEntryStatusList, useFilterType)
	{
	}

	public PhaseStatusTypeFilter(FilterCategory category, ModuleFilterCollection parentCollection)
	: base(category, parentCollection)
	{
	}

	protected override CodeDescriptionPairList GetTypeListCore() => new PhaseStatusFilterTypeList();

	protected override void ClearCore()
	{
		base.ClearCore();
		FilterType = PhaseStatusFilterTypeList.Codes.Any;
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		FilterType = PhaseStatusFilterTypeList.Codes.Any;
	}
}

