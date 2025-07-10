using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Module;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.NL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Module.Testing;

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

	public void TestPhaseStatusFilter()
	{
		var declaration1 = CreateDeclarationWithHeader(CustomsEntryPhaseStatusList.Codes._513);
		var declaration2 = CreateDeclarationWithHeader(CustomsEntryPhaseStatusList.Codes._514);
		declaration2.ActiveEntryHeaders.AddNew().CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._513;
		CreateDeclarationWithHeader(CustomsEntryPhaseStatusList.Codes._514);

		Factory.Save();

		var filterBizObj = CreateAndConfigurefilterBizObj(DeclarationFilterConstants.PhaseStatusText, CustomsEntryPhaseStatusList.Codes._513);

		var filteredDecs = Factory.Load<BaseJobDeclaration>(filterBizObj.Filter);

		AssertEquals("Total Count", 2, filteredDecs.Length);
		AssertEquals("Declaration with single entry header found.", true, filteredDecs.Any(d => d.PK == declaration1.PK));
		AssertEquals("Declaration with multiple entry headers found.", true, filteredDecs.Any(d => d.PK == declaration2.PK));

		JobDeclaration CreateDeclarationWithHeader(string phaseStatus)
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_PhaseStatus = phaseStatus;
			return declaration;
		}

		JobDeclarationFilterBusinessObject CreateAndConfigurefilterBizObj(string moduleTextFilterName, string propertyValue)
		{
			var filterBizObj = new JobDeclarationFilterBusinessObject();
			var phaseStatusFilter = (EntryStatusFilter)filterBizObj[moduleTextFilterName];
			phaseStatusFilter.IsActive = true;
			phaseStatusFilter.Property = propertyValue;
			return filterBizObj;
		}
	}

	public override void TestCombinedMessageStatus()
	{
		var declaration1 = Factory.New<BaseJobDeclaration>();
		var entryHeader0 = declaration1.CustomsEntryHeaders.AddNew();
		entryHeader0.CH_Status = "ACC";

		var declaration2 = Factory.New<BaseJobDeclaration>();
		var entryHeader1 = declaration2.CustomsEntryHeaders.AddNew();
		entryHeader1.CH_Status = "ACC";
		var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
		entryHeader2.CH_Status = "SNT";

		var declaration3 = Factory.New<BaseJobDeclaration>();
		var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
		entryHeader3.CH_Status = "ACC";
		var entryHeader4 = declaration3.CustomsEntryHeaders.AddNew();
		entryHeader4.CH_Status = "ACC";

		var declaration4 = Factory.New<BaseJobDeclaration>();
		var entryHeader5 = declaration4.CustomsEntryHeaders.AddNew();
		entryHeader5.CH_Status = ZString.Empty;
		var entryHeader6 = declaration4.CustomsEntryHeaders.AddNew();
		entryHeader6.CH_Status = ZString.Empty;

		var declaration5 = Factory.New<BaseJobDeclaration>();
		var entryHeader7 = declaration5.CustomsEntryHeaders.AddNew();
		entryHeader7.CH_Status = "ACC";

		var declaration6 = Factory.New<BaseJobDeclaration>();
		var entryHeader8 = declaration6.CustomsEntryHeaders.AddNew();
		entryHeader8.CH_Status = "SNT";

		Factory.Save();

		var filterBizObj = new JobDeclarationFilterBusinessObject();
		var filter = (EntryStatusFilter)filterBizObj[DeclarationFilterConstants.MessageStatusText];

		filter.IsActive = true;
		filter.Property = "ACC";
		filter.FilterType = EntryStatusFilterTypeList.Codes.All;

		var filteredDecs = Factory.Load<BaseJobDeclaration>(filterBizObj.Filter);
		AssertContainsExactElementsInAnyOrder(
			new[] { declaration1.PK, declaration3.PK, declaration5.PK },
			filteredDecs.Select(x => x.PK)
		);

		filter.Property = "ACC";
		filter.FilterType = EntryStatusFilterTypeList.Codes.Any;
		filteredDecs = Factory.Load<BaseJobDeclaration>(filterBizObj.Filter);
		AssertContainsExactElementsInAnyOrder(
			new[] { declaration1.PK, declaration2.PK, declaration3.PK, declaration5.PK },
			filteredDecs.Select(x => x.PK)
		);

		filter.FilterType = EntryStatusFilterTypeList.Codes.Any;
		filter.Property = DeclarationFilterConstants.EntryStatus.NotSentForFilter;
		filteredDecs = Factory.Load<BaseJobDeclaration>(filterBizObj.Filter);
		AssertContainsExactElementsInAnyOrder(
			new[] { declaration4.PK },
			filteredDecs.Select(x => x.PK)
		);
	}
}
