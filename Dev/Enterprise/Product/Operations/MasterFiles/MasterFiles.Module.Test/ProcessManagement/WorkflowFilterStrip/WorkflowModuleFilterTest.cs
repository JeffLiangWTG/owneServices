using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(WorkflowModuleFilter))]
	public class WorkflowModuleFilterTest : ModuleFilterTestCase<WorkflowModuleFilter>
	{
		#region Test Xml Serialise

		public void TestDeserializePropertiesFromToXml()
		{
			DummyFilterStripBusinessObject filterStripBizO = new DummyFilterStripBusinessObject();

			filterStripBizO.AddModuleFilterForTest(Filter);

			FilterStrip strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = Filter.Description;

			Filter.MilestoneEvent = "jmp";
			Filter.DatesToFilter = "ACT";
			Filter.EventReference = "abcde";
			Filter.EventReferenceComparisonOption = "contains";

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, "layoutName", false, false, SaveColumnLayout.Ignore);

			Filter.MilestoneEvent = "ARV"; // To change last used value
			Filter.DatesToFilter = "EST";
			Filter.EventReference = "12345";
			Filter.EventReferenceComparisonOption = "starts with";

			strip.FilterDescription = "";
			strip.Delete();

			WorkflowModuleFilter loadedFilter = (WorkflowModuleFilter)filterStripBizO[Filter.Description];

			filterStripBizO.LoadLayout(savedLayout);

			AssertEquals("jmp", loadedFilter.MilestoneEvent);
			AssertEquals("ACT", loadedFilter.DatesToFilter);
			AssertEquals("abcde", loadedFilter.EventReference);
			AssertEquals("contains", loadedFilter.EventReferenceComparisonOption);
		}

		#endregion

		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion

		#region TestClearResetsToDefaults

		public void TestClearResetsToDefaults()
		{
			ZString searchValue = "Today";
			ZDateTime dateValue1 = new ZDateTime(2006, 12, 25);
			ZDateTime dateValue2 = new ZDateTime(2006, 12, 26);
			ZString eventValue = "ADD";
			ZString eventReference = "abcde";
			ZString eventReferenceOption = "contains";

			var allFilters = new DummyFilterBizOForWorkflowMilestoneDate();
			var filterDate = (WorkflowModuleFilter)allFilters["date"];

			filterDate.PropertySearch = searchValue;
			filterDate.Property1 = dateValue1;
			filterDate.Property2 = dateValue2;
			filterDate.MilestoneEvent = eventValue;
			filterDate.EventReference = eventReference;
			filterDate.EventReferenceComparisonOption = eventReferenceOption;

			AssertEquals("Precondition", searchValue, filterDate.PropertySearch);
			AssertEquals("Precondition", dateValue1, filterDate.Property1);
			AssertEquals("Precondition", dateValue2, filterDate.Property2);
			AssertEquals("Precondition", eventValue, filterDate.MilestoneEvent);
			AssertEquals("Precondition", eventReference, filterDate.EventReference);
			AssertEquals("Precondition", eventReferenceOption, filterDate.EventReferenceComparisonOption);

			filterDate.Clear();
			AssertEquals("", filterDate.PropertySearch);
			AssertEquals(ZDateTime.Empty, filterDate.Property1);
			AssertEquals(ZDateTime.Empty, filterDate.Property2);
			AssertEquals("", filterDate.MilestoneEvent);
			AssertEquals("", filterDate.EventReference);
			AssertEquals("starts with", filterDate.EventReferenceComparisonOption);
		}

		#endregion

		#region TestIsEmpty

		public void TestIsEmpty()
		{
			var allFilters = new DummyFilterBizOForWorkflowMilestoneDate();
			var filterDate = (WorkflowModuleFilter)allFilters["date"];

			filterDate.Property1 = ZDateTime.Today;
			filterDate.Property2 = ZDateTime.Today;

			filterDate.PropertySearch = "";
			filterDate.MilestoneEvent = "";
			AssertEquals(true, filterDate.IsEmpty);

			filterDate.PropertySearch = "Tomorrow";
			AssertEquals(false, filterDate.IsEmpty);

			filterDate.PropertySearch = "crap data";
			AssertEquals(true, filterDate.IsEmpty);

			filterDate.PropertySearch = WorkflowModuleFilter.SpecifiedDateRange;
			AssertEquals(false, filterDate.IsEmpty);

			filterDate.Property1 = ZDateTime.Empty;
			filterDate.Property2 = ZDateTime.Empty;
			AssertEquals(true, filterDate.IsEmpty);

			filterDate.PropertySearch = "Tomorrow";
			AssertEquals(false, filterDate.IsEmpty);

			filterDate.PropertySearch = "";
			AssertEquals(true, filterDate.IsEmpty);

			filterDate.MilestoneEvent = "ADD";
			AssertEquals(false, filterDate.IsEmpty);

			filterDate.MilestoneEvent = "";
			AssertEquals(true, filterDate.IsEmpty);

			filterDate.EventReference = "abcde";
			AssertEquals(false, filterDate.IsEmpty);

			filterDate.EventReference = "";
			filterDate.EventReferenceComparisonOption = "exact";
			AssertEquals(true, filterDate.IsEmpty);

			filterDate.EventReferenceComparisonOption = "is blank";
			AssertEquals(false, filterDate.IsEmpty);

			filterDate.EventReferenceComparisonOption = "is not blank";
			AssertEquals(false, filterDate.IsEmpty);

			filterDate.EventReferenceComparisonOption = "contains";
			AssertEquals(true, filterDate.IsEmpty);
		}

		#endregion

		#region Test event reference comparison option

		public void TestEventReferenceComparisonOption()
		{
			var allFilters = new DummyFilterBizOForWorkflowMilestoneDate();
			var filterDate = (WorkflowModuleFilter)allFilters["date"];

			AssertEquals("starts with", filterDate.EventReferenceComparisonOption);
			Assert(!filterDate.EventReferenceInfo.ReadOnly);

			filterDate.EventReference = "abcde";

			filterDate.EventReferenceComparisonOption = "contains";
			AssertEquals("abcde", filterDate.EventReference);
			Assert(!filterDate.EventReferenceInfo.ReadOnly);

			filterDate.EventReferenceComparisonOption = "is blank";
			AssertEquals("", filterDate.EventReference);
			Assert(filterDate.EventReferenceInfo.ReadOnly);

			filterDate.EventReferenceComparisonOption = "is not blank";
			Assert(filterDate.EventReferenceInfo.ReadOnly);

			filterDate.EventReferenceComparisonOption = "exact";
			Assert(!filterDate.EventReferenceInfo.ReadOnly);

			filterDate.EventReferenceComparisonOption = "something";
			AssertEquals("?", filterDate.EventReferenceComparisonOption);
		}

		#endregion

		#region Test UseDatesToFilter

		public void TestUseDatesToFilter()
		{
			var support = new WorkflowModuleFilter("For Test", typeof(DummyBusinessObject), WorkflowModuleFilterTypes.MilestoneDate);
			AssertEquals(true, support.UseDatesToFilter);

			support = new WorkflowModuleFilter("For Test", typeof(DummyBusinessObject), WorkflowModuleFilterTypes.MilestoneLastCompleted);
			AssertEquals(false, support.UseDatesToFilter);

			support = new WorkflowModuleFilter("For Test", typeof(DummyBusinessObject), WorkflowModuleFilterTypes.MilestoneNext);
			AssertEquals(false, support.UseDatesToFilter);
		}

		#endregion

		#region TestQuery

		#region TestParentTableCodeIsNotAddedToFinalQueryWhenRelatedParentSubQueriesAreUsed

		public void TestParentTableCodeIsNotAddedToFinalQueryWhenRelatedParentSubQueriesAreUsed()
		{
			var allFilters = new DummyFilterBizOForWorkflowMilestoneDate();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["date"];
			milestoneFilter.MilestoneEvent = "EV1";
			milestoneFilter.DatesToFilter = ZString.Empty;
			milestoneFilter.PropertySearch = "";
			milestoneFilter.IsActive = true;

			string parentTableCodeSQL = string.Format("{0} = '{1}'", ProcessTasksSchema.Constants.P9_ParentTableCode, DummyBizoSchema.Constants.Prefix);
			AssertEquals("ParentTableCode added to query", true, allFilters.Filter.LiteralTextSqlFormatted.Contains(parentTableCodeSQL));

			milestoneFilter.RelatedParentSubQueries = System.Array.Empty<ZDBOnlySubQuery>();
			AssertEquals("ParentTableCode added to query", true, allFilters.Filter.LiteralTextSqlFormatted.Contains(parentTableCodeSQL));

			milestoneFilter.RelatedParentSubQueries = new[] { new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0) };
			AssertEquals("ParentTableCode not added to query", false, allFilters.Filter.LiteralTextSqlFormatted.Contains(parentTableCodeSQL));
		}

		#endregion

		#region TestDateAndTypeQueries

		public void TestDateAndTypeQueries()
		{
			var bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO3 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO4 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO5 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
			var milestone2 = bizO2.WorkflowItems.Milestones.AddNew();
			var milestone3 = bizO3.WorkflowItems.Milestones.AddNew();
			var milestone4 = bizO4.WorkflowItems.Milestones.AddNew();
			var milestone5 = bizO5.WorkflowItems.Milestones.AddNew();

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2000, 3, 4));
			milestone3.SetMilestoneActualDateForTest(new ZDateTime(2000, 5, 6));
			milestone4.SetMilestoneActualDateForTest(new ZDateTime(2000, 7, 8));
			milestone5.SetMilestoneActualDateForTest(ZDateTime.Empty);

			milestone1.TriggerConditions.TriggerEventCode = "EV1";
			milestone2.TriggerConditions.TriggerEventCode = "EV1";
			milestone3.TriggerConditions.TriggerEventCode = "EV2";
			milestone4.TriggerConditions.TriggerEventCode = "EV2";
			milestone5.TriggerConditions.TriggerEventCode = "EV2";

			Factory.Save();

			var allFilters = new DummyFilterBizOForWorkflowMilestoneDate();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["date"];
			milestoneFilter.MilestoneEvent = "EV1";
			milestoneFilter.DatesToFilter = ZString.Empty;
			milestoneFilter.PropertySearch = "";
			milestoneFilter.IsActive = true;

			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(allFilters.Filter);

			Assert(collection.Contains(bizO1));
			Assert(collection.Contains(bizO2));
			Assert(!collection.Contains(bizO3));
			Assert(!collection.Contains(bizO4));
			Assert(!collection.Contains(bizO5));

			milestoneFilter.MilestoneEvent = "EV1";
			milestoneFilter.DatesToFilter = ZString.Empty;
			milestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			milestoneFilter.Property1 = new ZDateTime(2000, 2, 1);
			milestoneFilter.Property2 = new ZDateTime(2000, 6, 1);
			milestoneFilter.IsActive = true;

			collection.Load(allFilters.Filter);

			Assert(!collection.Contains(bizO1));
			Assert(collection.Contains(bizO2));
			Assert(!collection.Contains(bizO3));
			Assert(!collection.Contains(bizO4));
			Assert(!collection.Contains(bizO5));

			milestoneFilter.MilestoneEvent = "EV2";
			milestoneFilter.DatesToFilter = ZString.Empty;
			milestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			milestoneFilter.Property1 = new ZDateTime(2000, 2, 1);
			milestoneFilter.Property2 = new ZDateTime(2000, 6, 1);
			milestoneFilter.IsActive = true;

			collection.Load(allFilters.Filter);

			Assert(!collection.Contains(bizO1));
			Assert(!collection.Contains(bizO2));
			Assert(collection.Contains(bizO3));
			Assert(!collection.Contains(bizO4));
			Assert(!collection.Contains(bizO5));

			milestoneFilter.MilestoneEvent = "EV2";
			milestoneFilter.DatesToFilter = ZString.Empty;
			milestoneFilter.PropertySearch = "";
			milestoneFilter.IsActive = true;

			collection.Load(allFilters.Filter);

			Assert(!collection.Contains(bizO1));
			Assert(!collection.Contains(bizO2));
			Assert(collection.Contains(bizO3));
			Assert(collection.Contains(bizO4));
			Assert(collection.Contains(bizO5));
		}

		#endregion

		#region TestDateQuery

		[TestDate(2010, 11, 22, 17, 00, 00)]
		public void TestDateQuery()
		{
			var bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO3 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
			var milestone2 = bizO2.WorkflowItems.Milestones.AddNew();
			var milestone3 = bizO3.WorkflowItems.Milestones.AddNew();

			milestone1.TriggerConditions.TriggerEventCode = "EV1";
			milestone2.TriggerConditions.TriggerEventCode = "EV1";
			milestone3.TriggerConditions.TriggerEventCode = "EV1";

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2010, 11, 21, 16, 24, 00));
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2010, 11, 22, 16, 24, 00));
			milestone3.SetMilestoneActualDateForTest(new ZDateTime(2010, 11, 23, 16, 24, 00));

			Factory.Save();

			var allFilters = new DummyFilterBizOForWorkflowMilestoneDate();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["date"];
			milestoneFilter.MilestoneEvent = "EV1";
			milestoneFilter.DatesToFilter = ZString.Empty;
			milestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			milestoneFilter.Property1 = new ZDateTime(2010, 11, 22);
			milestoneFilter.Property2 = new ZDateTime(2010, 11, 22);
			milestoneFilter.IsActive = true;

			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(allFilters.Filter);

			AssertEquals(1, collection.Count);
			AssertNull(collection.FindByPK(bizO1.PK));
			AssertNotNull(collection.FindByPK(bizO2.PK));
			AssertNull(collection.FindByPK(bizO3.PK));

			milestoneFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Yesterday;

			collection.Load(allFilters.Filter);

			AssertEquals(1, collection.Count);
			AssertNotNull(collection.FindByPK(bizO1.PK));
			AssertNull(collection.FindByPK(bizO2.PK));
			AssertNull(collection.FindByPK(bizO3.PK));

			milestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			milestoneFilter.Property1 = new ZDateTime(2010, 11, 22, 09, 00, 00);
			milestoneFilter.Property2 = new ZDateTime(2010, 11, 22, 18, 00, 00);

			collection.Load(allFilters.Filter);

			AssertEquals(1, collection.Count);
			AssertNull(collection.FindByPK(bizO1.PK));
			AssertNotNull(collection.FindByPK(bizO2.PK));
			AssertNull(collection.FindByPK(bizO3.PK));
		}

		#endregion

		#region TestDateQueryChecksParentTableCode

		public void TestDateQueryChecksParentTableCode()
		{
			var bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
			var milestone2 = bizO2.WorkflowItems.Milestones.AddNew();

			milestone1.TriggerConditions.TriggerEventCode = "EV1";
			milestone2.TriggerConditions.TriggerEventCode = "EV1";

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2013, 06, 06));
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2013, 06, 06));

			milestone2.P9_ParentTableCode = "P0";

			Factory.Save();

			var allFilters = new DummyFilterBizOForWorkflowMilestoneDate();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["date"];
			milestoneFilter.MilestoneEvent = "EV1";
			milestoneFilter.DatesToFilter = ZString.Empty;
			milestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			milestoneFilter.Property1 = new ZDateTime(2013, 06, 01);
			milestoneFilter.Property2 = new ZDateTime(2013, 07, 01);
			milestoneFilter.IsActive = true;

			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(allFilters.Filter);

			Assert(collection.Contains(bizO1));
			Assert(!collection.Contains(bizO2));
		}

		#endregion

		#region TestDateQueryActualAndEstimateLogic

		[TestDate(2017, 07, 03)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestDateQueryEstimateLogic_UsesUtc()
		{
			var bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
			milestone1.SetMilestoneScheduledDateForTest(ZDateTimeOffset.Now.AddMinutes(1));

			Factory.Save();

			var allFilters = new DummyFilterBizOForWorkflowMilestoneDate();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["date"];
			milestoneFilter.MilestoneEvent = ZString.Empty;
			milestoneFilter.DatesToFilter = ZString.Empty;
			milestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			milestoneFilter.Property1 = ZDateTime.Now;
			milestoneFilter.Property2 = ZDateTime.Now.AddMinutes(2);
			milestoneFilter.IsActive = true;

			AssertEquals(bizO1.PK, Factory.Load<DummyWithWorkflow>(allFilters.Filter).Single().PK);
		}

		public void TestDateQueryActualAndEstimateLogic()
		{
			var bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO3 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO4 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO5 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
			var milestone2 = bizO2.WorkflowItems.Milestones.AddNew();
			var milestone3 = bizO3.WorkflowItems.Milestones.AddNew();
			var milestone4 = bizO4.WorkflowItems.Milestones.AddNew();
			var milestone5 = bizO5.WorkflowItems.Milestones.AddNew();

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 8, 2));
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2000, 8, 4));
			milestone3.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 6));
			milestone4.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 8));
			milestone5.SetMilestoneActualDateForTest(ZDateTime.Empty);
			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 9, 2)));
			milestone2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 3, 4)));
			milestone3.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 9, 6)));
			milestone4.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 3, 8)));
			milestone5.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 10, 8)));

			Factory.Save();

			var allFilters = new DummyFilterBizOForWorkflowMilestoneDate();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["date"];
			milestoneFilter.MilestoneEvent = ZString.Empty;
			milestoneFilter.DatesToFilter = ZString.Empty;
			milestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			milestoneFilter.Property1 = new ZDateTime(2000, 7, 1);
			milestoneFilter.Property2 = new ZDateTime(2000, 11, 1);
			milestoneFilter.IsActive = true;

			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(allFilters.Filter);

			Assert(collection.Contains(bizO1));
			Assert(collection.Contains(bizO2));
			Assert(!collection.Contains(bizO3));
			Assert(!collection.Contains(bizO4));
			Assert(collection.Contains(bizO5));
		}

		#endregion

		#region TestDateQueryDatesToFilter

		[TestDate(2010, 11, 23, 11, 02, 00)]
		public void TestDateQueryDatesToFilter()
		{
			var bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO3 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
			var milestone2 = bizO2.WorkflowItems.Milestones.AddNew();
			var milestone3 = bizO3.WorkflowItems.Milestones.AddNew();

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2010, 11, 23, 10, 00, 00));
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2010, 11, 24, 10, 00, 00));
			milestone3.SetMilestoneActualDateForTest(ZDateTime.Empty);

			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2010, 11, 22, 10, 00, 00)));
			milestone2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2010, 11, 23, 10, 00, 00)));
			milestone3.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2010, 11, 23, 10, 00, 00)));

			milestone1.P9_OriginalScheduledDateLocalForBinding = new ZDateTimeOffset(new ZDateTime(2010, 11, 22, 10, 00, 00));
			milestone2.P9_OriginalScheduledDateLocalForBinding = new ZDateTimeOffset(new ZDateTime(2010, 11, 22, 20, 00, 00));
			milestone3.P9_OriginalScheduledDateLocalForBinding = new ZDateTimeOffset(new ZDateTime(2010, 11, 23, 23, 00, 00));

			Factory.Save();

			var allFilters = new DummyFilterBizOForWorkflowMilestoneDate();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["date"];
			milestoneFilter.MilestoneEvent = ZString.Empty;
			milestoneFilter.DatesToFilter = ZString.Empty;
			milestoneFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			milestoneFilter.IsActive = true;

			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(allFilters.Filter);

			AssertEquals(2, collection.Count);
			Assert(collection.Contains(bizO1));
			Assert(!collection.Contains(bizO2));
			Assert(collection.Contains(bizO3));

			milestoneFilter.DatesToFilter = nameof(DatesToFilterTypes.LST);
			collection.Load(allFilters.Filter);

			AssertEquals(2, collection.Count);
			Assert(collection.Contains(bizO1));
			Assert(!collection.Contains(bizO2));
			Assert(collection.Contains(bizO3));

			milestoneFilter.DatesToFilter = nameof(DatesToFilterTypes.ALL);
			collection.Load(allFilters.Filter);

			AssertEquals(3, collection.Count);
			Assert(collection.Contains(bizO1));
			Assert(collection.Contains(bizO2));
			Assert(collection.Contains(bizO3));

			milestoneFilter.DatesToFilter = nameof(DatesToFilterTypes.ACT);
			collection.Load(allFilters.Filter);

			AssertEquals(1, collection.Count);
			Assert(collection.Contains(bizO1));
			Assert(!collection.Contains(bizO2));
			Assert(!collection.Contains(bizO3));

			milestoneFilter.DatesToFilter = nameof(DatesToFilterTypes.EST);
			collection.Load(allFilters.Filter);

			AssertEquals(2, collection.Count);
			Assert(!collection.Contains(bizO1));
			Assert(collection.Contains(bizO2));
			Assert(collection.Contains(bizO3));

			milestoneFilter.DatesToFilter = nameof(DatesToFilterTypes.OES);
			collection.Load(allFilters.Filter);

			AssertEquals(1, collection.Count);
			Assert(!collection.Contains(bizO1));
			Assert(!collection.Contains(bizO2));
			Assert(collection.Contains(bizO3));
		}

		#endregion

		#region TestNextMilestoneQuery

		public void TestNextMilestoneQuery()
		{
			DummyWithWorkflow bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			DummyWithWorkflow bizO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			DummyWithWorkflow bizO3 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			DummyWithWorkflow bizO4 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			DummyWithWorkflow bizO5 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			ProcessTask milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone2 = bizO2.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone3 = bizO3.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone4 = bizO4.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone5 = bizO5.WorkflowItems.Milestones.AddNew();

			milestone1.TriggerConditions.TriggerEventCode = Events.AvailableFrom.Code;
			milestone2.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			milestone3.TriggerConditions.TriggerEventCode = Events.AvailableFrom.Code;
			milestone4.TriggerConditions.TriggerEventCode = Events.AvailableFrom.Code;
			milestone5.TriggerConditions.TriggerEventCode = Events.AvailableFrom.Code;

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			milestone2.SetMilestoneActualDateForTest(ZDateTime.Empty);
			milestone3.SetMilestoneActualDateForTest(ZDateTime.Empty);
			milestone4.SetMilestoneActualDateForTest(ZDateTime.Empty);
			milestone5.SetMilestoneActualDateForTest(ZDateTime.Empty);

			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));
			milestone2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2001, 1, 2)));
			milestone3.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));
			milestone4.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));
			milestone5.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(1999, 1, 2)));

			Factory.Save();

			DummyFilterBizOForWorkflowNextMilestone filterNext = new DummyFilterBizOForWorkflowNextMilestone();
			((WorkflowModuleFilter)filterNext["next"]).DatesToFilter = ZString.Empty;
			((WorkflowModuleFilter)filterNext["next"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((WorkflowModuleFilter)filterNext["next"]).Property1 = new ZDateTime(2000, 1, 1);
			((WorkflowModuleFilter)filterNext["next"]).Property2 = new ZDateTime(2000, 6, 1);
			((WorkflowModuleFilter)filterNext["next"]).IsActive = true;

			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(filterNext.Filter);

			Assert(!collection.Contains(bizO1));
			Assert(!collection.Contains(bizO2));
			Assert(collection.Contains(bizO3));
			Assert(collection.Contains(bizO4));
			Assert(!collection.Contains(bizO5));
		}

		#endregion

		#region TestNextMilestoneQueryChecksParentTableCode

		public void TestNextMilestoneQueryChecksParentTableCode()
		{
			var bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
			var milestone2 = bizO2.WorkflowItems.Milestones.AddNew();

			milestone1.TriggerConditions.TriggerEventCode = Events.AvailableFrom.Code;
			milestone2.TriggerConditions.TriggerEventCode = Events.AvailableFrom.Code;
			milestone1.SetMilestoneActualDateForTest(ZDateTime.Empty);
			milestone2.SetMilestoneActualDateForTest(ZDateTime.Empty);
			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));
			milestone2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));

			milestone2.P9_ParentTableCode = "P0";

			Factory.Save();

			var allFilters = new DummyFilterBizOForWorkflowNextMilestone();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["next"];
			milestoneFilter.DatesToFilter = ZString.Empty;
			milestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			milestoneFilter.Property1 = new ZDateTime(2000, 1, 1);
			milestoneFilter.Property2 = new ZDateTime(2000, 6, 1);
			milestoneFilter.IsActive = true;

			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(allFilters.Filter);

			Assert(collection.Contains(bizO1));
			Assert(!collection.Contains(bizO2));
		}

		#endregion

		#region TestNextMilestoneQuery_WithEmptyDate

		public void TestNextMilestoneQuery_WithEmptyDate()
		{
			DummyWithWorkflow bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			DummyWithWorkflow bizO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			DummyWithWorkflow bizO3 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			DummyWithWorkflow bizO4 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			DummyWithWorkflow bizO5 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			ProcessTask milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone2 = bizO2.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone3 = bizO3.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone4 = bizO4.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone5 = bizO5.WorkflowItems.Milestones.AddNew();

			milestone1.TriggerConditions.TriggerEventCode = Events.AvailableFrom.Code;
			milestone2.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			milestone3.TriggerConditions.TriggerEventCode = Events.AvailableFrom.Code;
			milestone4.TriggerConditions.TriggerEventCode = Events.AvailableFrom.Code;
			milestone5.TriggerConditions.TriggerEventCode = Events.AvailableFrom.Code;

			milestone5.P9_Type = Core.Constants.Workflow.ExceptionType;

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			milestone2.SetMilestoneActualDateForTest(ZDateTime.Empty);
			milestone3.SetMilestoneActualDateForTest(ZDateTime.Empty);
			milestone4.SetMilestoneActualDateForTest(ZDateTime.Empty);
			milestone5.SetMilestoneActualDateForTest(ZDateTime.Empty);

			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));
			milestone2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2001, 1, 2)));
			milestone3.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));
			milestone4.SetMilestoneScheduledDateForTest(ZDateTimeOffset.Empty);
			milestone5.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));

			Factory.Save();

			DummyFilterBizOForWorkflowNextMilestone filterNext = new DummyFilterBizOForWorkflowNextMilestone();
			((WorkflowModuleFilter)filterNext["next"]).MilestoneEvent = "";
			((WorkflowModuleFilter)filterNext["next"]).DatesToFilter = ZString.Empty;
			((WorkflowModuleFilter)filterNext["next"]).PropertySearch = "";
			((WorkflowModuleFilter)filterNext["next"]).IsActive = true;

			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(filterNext.Filter);

			Assert(collection.Contains(bizO1));
			Assert(collection.Contains(bizO2));
			Assert(collection.Contains(bizO3));
			Assert(collection.Contains(bizO4));
			Assert(collection.Contains(bizO5));

			filterNext = new DummyFilterBizOForWorkflowNextMilestone();
			((WorkflowModuleFilter)filterNext["next"]).MilestoneEvent = Events.AvailableFrom.Code;
			((WorkflowModuleFilter)filterNext["next"]).DatesToFilter = ZString.Empty;
			((WorkflowModuleFilter)filterNext["next"]).PropertySearch = "";
			((WorkflowModuleFilter)filterNext["next"]).IsActive = true;

			collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(filterNext.Filter);

			Assert(!collection.Contains(bizO1));
			Assert(!collection.Contains(bizO2));
			Assert(collection.Contains(bizO3));
			Assert(collection.Contains(bizO4));
			Assert(!collection.Contains(bizO5));
		}

		#endregion

		#region TestNextMilestoneQuery_ParentJobSelectionLogic

		public void TestNextMilestoneQuery_ParentJobSelectionLogic()
		{
			DummyWithWorkflow bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			ProcessTask milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone2 = bizO1.WorkflowItems.Milestones.AddNew();

			milestone1.SetMilestoneActualDateForTest(ZDateTime.Empty);
			milestone2.SetMilestoneActualDateForTest(ZDateTime.Empty);

			milestone1.TriggerConditions.TriggerEventCode = Events.AvailableFrom.Code;
			milestone2.TriggerConditions.TriggerEventCode = Events.Arrival.Code;

			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));
			milestone2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2001, 1, 2)));

			Factory.Save();

			DummyFilterBizOForWorkflowNextMilestone filterNext = new DummyFilterBizOForWorkflowNextMilestone();
			((WorkflowModuleFilter)filterNext["next"]).MilestoneEvent = Events.AvailableFrom.Code;
			((WorkflowModuleFilter)filterNext["next"]).DatesToFilter = ZString.Empty;
			((WorkflowModuleFilter)filterNext["next"]).PropertySearch = "";
			((WorkflowModuleFilter)filterNext["next"]).IsActive = true;

			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(filterNext.Filter);

			Assert(collection.Contains(bizO1));

			filterNext = new DummyFilterBizOForWorkflowNextMilestone();
			((WorkflowModuleFilter)filterNext["next"]).MilestoneEvent = Events.Arrival.Code;
			((WorkflowModuleFilter)filterNext["next"]).DatesToFilter = ZString.Empty;
			((WorkflowModuleFilter)filterNext["next"]).PropertySearch = "";
			((WorkflowModuleFilter)filterNext["next"]).IsActive = true;

			collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(filterNext.Filter);

			Assert(!collection.Contains(bizO1));
		}

		#endregion

		#region TestNextMilestoneQuery_PublishedMilestones

		public void TestNextMilestoneQuery_PublishedMilestones()
		{
			bool originalGlobalsIsWeb = Globals.IsWeb;

			try
			{
				Globals.IsWeb = true;
				DummyWithWorkflow bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
				DummyWithWorkflow bizO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
				DummyWithWorkflow bizO3 = Factory.NewWithValidTestData<DummyWithWorkflow>();
				DummyWithWorkflow bizO4 = Factory.NewWithValidTestData<DummyWithWorkflow>();
				DummyWithWorkflow bizO5 = Factory.NewWithValidTestData<DummyWithWorkflow>();

				ProcessTask milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
				ProcessTask milestone2 = bizO2.WorkflowItems.Milestones.AddNew();
				ProcessTask milestone3 = bizO3.WorkflowItems.Milestones.AddNew();
				ProcessTask milestone4 = bizO4.WorkflowItems.Milestones.AddNew();
				ProcessTask milestone5 = bizO5.WorkflowItems.Milestones.AddNew();

				milestone1.TriggerConditions.TriggerEventCode = Events.AvailableFrom.Code;
				milestone2.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
				milestone3.TriggerConditions.TriggerEventCode = Events.AvailableFrom.Code;
				milestone4.TriggerConditions.TriggerEventCode = Events.AvailableFrom.Code;
				milestone5.TriggerConditions.TriggerEventCode = Events.AvailableFrom.Code;

				milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
				milestone2.SetMilestoneActualDateForTest(ZDateTime.Empty);
				milestone3.SetMilestoneActualDateForTest(ZDateTime.Empty);
				milestone3.P9_IsPublished = false;
				milestone4.SetMilestoneActualDateForTest(ZDateTime.Empty);
				milestone5.SetMilestoneActualDateForTest(ZDateTime.Empty);

				milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));
				milestone2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2001, 1, 2)));
				milestone3.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));
				milestone4.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));
				milestone5.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(1999, 1, 2)));

				Factory.Save();

				DummyFilterBizOForWorkflowNextMilestone filterNext = new DummyFilterBizOForWorkflowNextMilestone();
				((WorkflowModuleFilter)filterNext["next"]).MilestoneEvent = ZString.Empty;
				((WorkflowModuleFilter)filterNext["next"]).DatesToFilter = ZString.Empty;
				((WorkflowModuleFilter)filterNext["next"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				((WorkflowModuleFilter)filterNext["next"]).Property1 = new ZDateTime(2000, 1, 1);
				((WorkflowModuleFilter)filterNext["next"]).Property2 = new ZDateTime(2000, 6, 1);
				((WorkflowModuleFilter)filterNext["next"]).IsActive = true;

				DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
				collection.Load(filterNext.Filter);

				Assert(!collection.Contains(bizO1));
				Assert(!collection.Contains(bizO2));
				Assert(!collection.Contains(bizO3));
				Assert(collection.Contains(bizO4));
				Assert(!collection.Contains(bizO5));
			}
			finally
			{
				Globals.IsWeb = originalGlobalsIsWeb;
			}
		}

		#endregion

		#region TestLastCompletedQuery

		public void TestLastCompletedQuery()
		{
			DummyWithWorkflow bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			DummyWithWorkflow bizO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			DummyWithWorkflow bizO3 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			DummyWithWorkflow bizO4 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			DummyWithWorkflow bizO5 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			ProcessTask milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone2 = bizO2.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone3 = bizO3.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone4 = bizO4.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone5 = bizO5.WorkflowItems.Milestones.AddNew();

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2000, 2, 2));
			milestone3.SetMilestoneActualDateForTest(new ZDateTime(2000, 3, 2));
			milestone4.SetMilestoneActualDateForTest(ZDateTime.Empty);
			milestone5.SetMilestoneActualDateForTest(new ZDateTime(2000, 2, 2));

			Factory.Save();

			DummyFilterBizOForWorkflowLastCompletedMilestone filterLast = new DummyFilterBizOForWorkflowLastCompletedMilestone();
			((WorkflowModuleFilter)filterLast["last c"]).MilestoneEvent = "";
			((WorkflowModuleFilter)filterLast["last c"]).DatesToFilter = ZString.Empty;
			((WorkflowModuleFilter)filterLast["last c"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((WorkflowModuleFilter)filterLast["last c"]).Property1 = new ZDateTime(2000, 2, 1);
			((WorkflowModuleFilter)filterLast["last c"]).Property2 = new ZDateTime(2000, 2, 6);
			((WorkflowModuleFilter)filterLast["last c"]).IsActive = true;

			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(filterLast.Filter);

			Assert(!collection.Contains(bizO1));
			Assert(collection.Contains(bizO2));
			Assert(!collection.Contains(bizO3));
			Assert(!collection.Contains(bizO4));
			Assert(collection.Contains(bizO5));
		}

		#endregion

		#region TestLastCompletedQueryChecksParentTableCode

		public void TestLastCompletedQueryChecksParentTableCode()
		{
			var bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
			var milestone2 = bizO2.WorkflowItems.Milestones.AddNew();

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));

			milestone2.P9_ParentTableCode = "P0";

			Factory.Save();

			var allFilters = new DummyFilterBizOForWorkflowLastCompletedMilestone();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["last c"];
			milestoneFilter.MilestoneEvent = "";
			milestoneFilter.DatesToFilter = ZString.Empty;
			milestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			milestoneFilter.Property1 = new ZDateTime(2000, 1, 1);
			milestoneFilter.Property2 = new ZDateTime(2000, 2, 1);
			milestoneFilter.IsActive = true;

			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(allFilters.Filter);

			Assert(collection.Contains(bizO1));
			Assert(!collection.Contains(bizO2));
		}

		#endregion

		#region TestLastCompletedQuery_WithEmptyDate

		public void TestLastCompletedQuery_WithEmptyDate()
		{
			DummyWithWorkflow bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			DummyWithWorkflow bizO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			DummyWithWorkflow bizO3 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			DummyWithWorkflow bizO4 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			ProcessTask milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone2 = bizO2.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone3 = bizO3.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone4 = bizO4.WorkflowItems.Milestones.AddNew();

			milestone4.P9_Type = Core.Constants.Workflow.ExceptionType;

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			milestone3.SetMilestoneActualDateForTest(ZDateTime.Empty);
			milestone4.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));

			milestone1.TriggerConditions.TriggerEventCode = "EV1";
			milestone2.TriggerConditions.TriggerEventCode = "EV2";
			milestone3.TriggerConditions.TriggerEventCode = "EV1";
			milestone4.TriggerConditions.TriggerEventCode = "EV1";

			Factory.Save();

			DummyFilterBizOForWorkflowLastCompletedMilestone filterLast = new DummyFilterBizOForWorkflowLastCompletedMilestone();
			((WorkflowModuleFilter)filterLast["last c"]).MilestoneEvent = "";
			((WorkflowModuleFilter)filterLast["last c"]).DatesToFilter = ZString.Empty;
			((WorkflowModuleFilter)filterLast["last c"]).PropertySearch = "";
			((WorkflowModuleFilter)filterLast["last c"]).IsActive = true;

			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(filterLast.Filter);

			Assert(collection.Contains(bizO1));
			Assert(collection.Contains(bizO2));
			Assert(collection.Contains(bizO3));
			Assert(collection.Contains(bizO4));

			filterLast = new DummyFilterBizOForWorkflowLastCompletedMilestone();
			((WorkflowModuleFilter)filterLast["last c"]).MilestoneEvent = "EV1";
			((WorkflowModuleFilter)filterLast["last c"]).DatesToFilter = ZString.Empty;
			((WorkflowModuleFilter)filterLast["last c"]).PropertySearch = "";
			((WorkflowModuleFilter)filterLast["last c"]).IsActive = true;

			collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(filterLast.Filter);

			Assert(collection.Contains(bizO1));
			Assert(!collection.Contains(bizO2));
			Assert(!collection.Contains(bizO3));
			Assert(!collection.Contains(bizO4));
		}

		#endregion

		#region TestLastCompletedQuery_ParentJobSelectionLogic

		public void TestLastCompletedQuery_ParentJobSelectionLogic()
		{
			DummyWithWorkflow bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			ProcessTask milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone2 = bizO1.WorkflowItems.Milestones.AddNew();

			milestone1.TriggerConditions.TriggerEventCode = "EV1";
			milestone2.TriggerConditions.TriggerEventCode = "EV2";

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2001, 1, 2));

			Factory.Save();

			DummyFilterBizOForWorkflowLastCompletedMilestone filterNext = new DummyFilterBizOForWorkflowLastCompletedMilestone();
			((WorkflowModuleFilter)filterNext["last c"]).MilestoneEvent = "EV1";
			((WorkflowModuleFilter)filterNext["last c"]).DatesToFilter = ZString.Empty;
			((WorkflowModuleFilter)filterNext["last c"]).PropertySearch = "";
			((WorkflowModuleFilter)filterNext["last c"]).IsActive = true;

			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(filterNext.Filter);

			Assert(!collection.Contains(bizO1));

			filterNext = new DummyFilterBizOForWorkflowLastCompletedMilestone();
			((WorkflowModuleFilter)filterNext["last c"]).MilestoneEvent = "EV2";
			((WorkflowModuleFilter)filterNext["last c"]).DatesToFilter = ZString.Empty;
			((WorkflowModuleFilter)filterNext["last c"]).PropertySearch = "";
			((WorkflowModuleFilter)filterNext["last c"]).IsActive = true;

			collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(filterNext.Filter);

			Assert(collection.Contains(bizO1));
		}

		#endregion

		#region TestMilestoneDateQuery_WorksCorrectlyWithTime

		public void TestMilestoneDateQuery_WorksCorrectlyWithTime()
		{
			var bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO3 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
			var milestone2 = bizO2.WorkflowItems.Milestones.AddNew();
			var milestone3 = bizO3.WorkflowItems.Milestones.AddNew();

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 1, 23, 59, 59));
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			milestone3.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 4, 0, 1, 1));

			Factory.Save();

			var allFilters = new DummyFilterBizOForWorkflowMilestoneDate();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["date"];
			milestoneFilter.MilestoneEvent = ZString.Empty;
			milestoneFilter.DatesToFilter = ZString.Empty;
			milestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			milestoneFilter.Property1 = new ZDateTime(2000, 1, 2);
			milestoneFilter.Property2 = new ZDateTime(2000, 1, 2);
			milestoneFilter.IsActive = true;

			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(allFilters.Filter);

			Assert(!collection.Contains(bizO1));
			Assert(collection.Contains(bizO2));
			Assert(!collection.Contains(bizO3));
		}

		#endregion

		#region TestMilestoneDateQuery_TodayDateRange_OnlyTodaysDate

		[TestDate(2010, 11, 24, 11, 02, 00)]
		public void TestMilestoneDateQuery_TodayDateRange_OnlyTodaysDate()
		{
			var bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO3 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
			var milestone2 = bizO2.WorkflowItems.Milestones.AddNew();
			var milestone3 = bizO3.WorkflowItems.Milestones.AddNew();
			var exception1 = bizO3.WorkflowItems.Exceptions.AddNew();

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2010, 11, 24, 10, 00, 00));
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2010, 11, 24, 23, 59, 59));
			milestone3.SetMilestoneActualDateForTest(new ZDateTime(2010, 11, 25, 00, 00, 00));
			exception1.SetMilestoneActualDateForTest(new ZDateTime(2010, 11, 24, 10, 00, 00));

			Factory.Save();

			var allFilters = new DummyFilterBizOForWorkflowMilestoneDate();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["date"];
			milestoneFilter.MilestoneEvent = ZString.Empty;
			milestoneFilter.DatesToFilter = ZString.Empty;
			milestoneFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			milestoneFilter.IsActive = true;

			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(allFilters.Filter);

			AssertEquals(2, collection.Count);
			Assert(collection.Contains(bizO1));
			Assert(collection.Contains(bizO2));
			Assert(!collection.Contains(bizO3));
		}

		#endregion

		#region TestMilestoneDateQuery_SpecifiedDateRange_CorrectToDate

		[TestDate(2010, 11, 24, 11, 02, 00)]
		public void TestMilestoneDateQuery_SpecifiedDateRange_CorrectToDate()
		{
			var bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO3 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO4 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
			var milestone2 = bizO2.WorkflowItems.Milestones.AddNew();
			var milestone3 = bizO3.WorkflowItems.Milestones.AddNew();
			var milestone4 = bizO4.WorkflowItems.Milestones.AddNew();

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2010, 11, 24, 10, 00, 00));
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2010, 11, 24, 23, 59, 59));
			milestone3.SetMilestoneActualDateForTest(new ZDateTime(2010, 11, 23, 1, 00, 00));
			milestone4.SetMilestoneActualDateForTest(new ZDateTime(2010, 11, 25, 00, 1, 00));

			Factory.Save();

			var allFilters = new DummyFilterBizOForWorkflowMilestoneDate();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["date"];
			milestoneFilter.MilestoneEvent = ZString.Empty;
			milestoneFilter.DatesToFilter = ZString.Empty;
			milestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			milestoneFilter.Property1 = new ZDateTime(2010, 11, 23);
			milestoneFilter.Property2 = new ZDateTime(2010, 11, 24);
			milestoneFilter.IsActive = true;

			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(allFilters.Filter);

			AssertEquals(3, collection.Count);
			Assert(collection.Contains(bizO1));
			Assert(collection.Contains(bizO2));
			Assert(collection.Contains(bizO3));
			Assert(!collection.Contains(bizO4));
		}

		#endregion

		#region TestMilestoneDateQuery_HasNoDate_OnlyReturnsEmpty

		[TestDate(2010, 11, 24, 11, 02, 00)]
		public void TestMilestoneDateQuery_HasNoDate_OnlyReturnsEmpty()
		{
			var bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO3 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
			var milestone2 = bizO2.WorkflowItems.Milestones.AddNew();
			var milestone3 = bizO3.WorkflowItems.Milestones.AddNew();

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2010, 11, 24, 10, 00, 00));
			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2010, 11, 24, 10, 00, 00)));
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2010, 11, 24, 23, 59, 59));
			milestone3.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2010, 11, 24, 23, 59, 59)));

			Factory.Save();

			var allFilters = new DummyFilterBizOForWorkflowMilestoneDate();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["date"];
			milestoneFilter.MilestoneEvent = ZString.Empty;
			milestoneFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			milestoneFilter.IsActive = true;
			milestoneFilter.DatesToFilter = nameof(DatesToFilterTypes.ALL);

			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(allFilters.Filter);

			AssertEquals(2, collection.Count);
			Assert(!collection.Contains(bizO1));
			Assert(collection.Contains(bizO2));
			Assert(collection.Contains(bizO3));
		}

		#endregion

		#region TestLastCompletedQuery_PublishedMilestones

		public void TestLastCompletedQuery_PublishedMilestones()
		{
			bool originalGlobalsIsWeb = Globals.IsWeb;

			try
			{
				Globals.IsWeb = true;

				DummyWithWorkflow bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
				DummyWithWorkflow bizO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
				DummyWithWorkflow bizO3 = Factory.NewWithValidTestData<DummyWithWorkflow>();
				DummyWithWorkflow bizO4 = Factory.NewWithValidTestData<DummyWithWorkflow>();
				DummyWithWorkflow bizO5 = Factory.NewWithValidTestData<DummyWithWorkflow>();

				ProcessTask milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
				ProcessTask milestone2 = bizO2.WorkflowItems.Milestones.AddNew();
				ProcessTask milestone3 = bizO3.WorkflowItems.Milestones.AddNew();
				ProcessTask milestone4 = bizO4.WorkflowItems.Milestones.AddNew();
				ProcessTask milestone5 = bizO5.WorkflowItems.Milestones.AddNew();

				milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
				milestone2.SetMilestoneActualDateForTest(new ZDateTime(2000, 2, 2));
				milestone2.P9_IsPublished = false;
				milestone3.SetMilestoneActualDateForTest(new ZDateTime(2000, 3, 2));
				milestone4.SetMilestoneActualDateForTest(ZDateTime.Empty);
				milestone5.SetMilestoneActualDateForTest(new ZDateTime(2000, 2, 2));

				Factory.Save();

				DummyFilterBizOForWorkflowLastCompletedMilestone filterLast = new DummyFilterBizOForWorkflowLastCompletedMilestone();
				((WorkflowModuleFilter)filterLast["last c"]).MilestoneEvent = "";
				((WorkflowModuleFilter)filterLast["last c"]).DatesToFilter = ZString.Empty;
				((WorkflowModuleFilter)filterLast["last c"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				((WorkflowModuleFilter)filterLast["last c"]).Property1 = new ZDateTime(2000, 2, 1);
				((WorkflowModuleFilter)filterLast["last c"]).Property2 = new ZDateTime(2000, 2, 6);
				((WorkflowModuleFilter)filterLast["last c"]).IsActive = true;

				DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
				collection.Load(filterLast.Filter);

				Assert(!collection.Contains(bizO1));
				Assert(!collection.Contains(bizO2));
				Assert(!collection.Contains(bizO3));
				Assert(!collection.Contains(bizO4));
				Assert(collection.Contains(bizO5));
			}
			finally
			{
				Globals.IsWeb = originalGlobalsIsWeb;
			}
		}

		#endregion

		#region Test event reference query

		public void TestEventReferenceQuery()
		{
			var allFilters = new DummyFilterBizOForWorkflowMilestoneDate();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["date"];

			milestoneFilter.EventReferenceComparisonOption = "exact";
			milestoneFilter.EventReference = "abcde";
			Assert(milestoneFilter.Query.LiteralTextSqlFormatted.Replace("\r\n", " ").Replace("\t", "").Replace("  ", " ").Contains("dbo.CLRUncompressAsString ( P9_Notes ) = 'abcde'"));
			Assert(milestoneFilter.Query.LiteralTextADO.Contains("P9_Notes = 'abcde'")); // Stored procedure can't be called in ADO, and anyway this is db-only query

			milestoneFilter.EventReferenceComparisonOption = "is blank";
			Assert(milestoneFilter.Query.LiteralTextSqlFormatted.Replace("\r\n", " ").Replace("\t", "").Replace("  ", " ").Contains("P9_Notes is NULL"));

			milestoneFilter.EventReferenceComparisonOption = "is not blank";
			Assert(milestoneFilter.Query.LiteralTextSqlFormatted.Replace("\r\n", " ").Replace("\t", "").Replace("  ", " ").Contains("P9_Notes is not NULL"));
		}

		#endregion

		#region TestCopyTransient

		public void TestCopyTransient()
		{
			var allFilters = new DummyFilterBizOForWorkflowLastCompletedMilestone();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["last c"];
			milestoneFilter.MilestoneEvent = "ARV";
			milestoneFilter.DatesToFilter = "ACT";
			milestoneFilter.EventReferenceComparisonOption = "exact";
			milestoneFilter.EventReference = "abcde";
			milestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			milestoneFilter.Property1 = new ZDateTime(2010, 11, 23);
			milestoneFilter.Property2 = new ZDateTime(2010, 11, 24);
			milestoneFilter.IsActive = true;

			var allFilters2 = new DummyFilterBizOForWorkflowLastCompletedMilestone();
			var milestoneFilter2 = (WorkflowModuleFilter)allFilters2["last c"];

			milestoneFilter2.CopyTransientProperties(milestoneFilter);

			CombineAssertions(() =>
			{
				AssertEquals(milestoneFilter.MilestoneEvent, milestoneFilter2.MilestoneEvent);
				AssertEquals(milestoneFilter.DatesToFilter, milestoneFilter2.DatesToFilter);
				AssertEquals(milestoneFilter.EventReferenceComparisonOption, milestoneFilter2.EventReferenceComparisonOption);
				AssertEquals(milestoneFilter.EventReference, milestoneFilter2.EventReference);
				AssertEquals(milestoneFilter.PropertySearch, milestoneFilter2.PropertySearch);
				AssertEquals(milestoneFilter.Property1, milestoneFilter2.Property1);
				AssertEquals(milestoneFilter.Property2, milestoneFilter2.Property2);
				AssertEquals(milestoneFilter.IsActive, milestoneFilter2.IsActive);
			});
		}

		#endregion

		#endregion

		#region Implementation

		protected override WorkflowModuleFilter GetNewModuleFilter()
		{
			return new WorkflowModuleFilter("moo", typeof(DummyBusinessObject), WorkflowModuleFilterTypes.MilestoneDate);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Dates; }
		}

		#endregion
	}
}
