using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Module;
using Enterprise.Customs.NO.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Module.Testing;

[TestedType(typeof(JobDeclarationFilterBusinessObject))]
sealed class JobDeclarationFilterBusinessObjectTest : Customs.Module.Testing.JobDeclarationFilterBusinessObjectTest
{
	public void TestLookups()
	{
		var filterBizObj = new JobDeclarationFilterBusinessObject();
		AssertType<JobDeclarationFilterLookups>("Lookups of correct type", filterBizObj.Lookups);
	}

	public void TestFilters()
	{
		var filter = GetNewFilterStripBusinessObject();
		AssertNotNull(filter[DeclarationFilterConstants.PhaseStatusText]);
	}

	public void TestPhaseStatusFilterTypeAll()
	{
		var declaration1 = CreateDeclarationWithHeader(CustomsEntryPhaseStatusList.Codes.Finalized);
		declaration1.ActiveEntryHeaders.AddNew().CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.Reminder;
		var declaration2 = CreateDeclarationWithHeader(CustomsEntryPhaseStatusList.Codes.Finalized);
		declaration2.ActiveEntryHeaders.AddNew().CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.Finalized;
		var declaration3 = CreateDeclarationWithHeader(CustomsEntryPhaseStatusList.Codes.Finalized);

		Factory.Save();

		var filterBizObj = CreateFilterBizObjAndSetPhaseStatusFilterType(PhaseStatusFilterTypeList.Codes.All, CustomsEntryPhaseStatusList.Codes.Finalized);

		var filteredResult = Factory.Load<JobDeclaration>(filterBizObj.Filter);

		CombineAssertions(() =>
		{
			AssertEquals("Total Count", 2, filteredResult.Length);
			AssertEquals("Declaration with Only One Header with Phase Status Finalized should be returned", expected: true, filteredResult.Any(d => d.PK == declaration3.PK));
			AssertEquals("Declaration with Multiple Entry Headers with Phase Statuses Finalized should be returned.", expected: true, filteredResult.Any(d => d.PK == declaration2.PK));
		});
	}

	public void TestPhaseStatusFilterTypeAny()
	{
		var declaration1 = CreateDeclarationWithHeader(CustomsEntryPhaseStatusList.Codes.Finalized);
		declaration1.ActiveEntryHeaders.AddNew().CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.Reminder;
		var declaration2 = CreateDeclarationWithHeader(CustomsEntryPhaseStatusList.Codes.Finalized);
		declaration2.ActiveEntryHeaders.AddNew().CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.Finalized;
		var declaration3 = CreateDeclarationWithHeader(CustomsEntryPhaseStatusList.Codes.Reminder);

		Factory.Save();

		var filterBizObj = CreateFilterBizObjAndSetPhaseStatusFilterType(PhaseStatusFilterTypeList.Codes.Any, CustomsEntryPhaseStatusList.Codes.Reminder);

		var filteredResult = Factory.Load<BaseJobDeclaration>(filterBizObj.Filter);

		CombineAssertions(() =>
		{
			AssertEquals("Total Count", 2, filteredResult.Length);
			AssertEquals("Declaration with Only One Header with Phase Status Reminder should be returned", expected: true, filteredResult.Any(d => d.PK == declaration3.PK));
			AssertEquals("Declaration with Multiple Entry Headers with Phase Statuses Reminder and Finalized should be returned.", expected: true, filteredResult.Any(d => d.PK == declaration1.PK));
		});
	}

	JobDeclaration CreateDeclarationWithHeader(string phaseStatus)
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_PhaseStatus = phaseStatus;
		return declaration;
	}

	JobDeclarationFilterBusinessObject CreateFilterBizObjAndSetPhaseStatusFilterType(string type, string propertyValue)
	{
		var filterBizObj = new JobDeclarationFilterBusinessObject();
		var phaseStatusFilter = (PhaseStatusTypeFilter)filterBizObj[DeclarationFilterConstants.PhaseStatusText];
		phaseStatusFilter.IsActive = true;
		phaseStatusFilter.Property = propertyValue;
		phaseStatusFilter.FilterType = type;
		return filterBizObj;
	}
}
