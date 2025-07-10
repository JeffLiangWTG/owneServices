using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(EventsFilterBusinessObject))]
	sealed class EventsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters_ComplexTest()
		{
			TestCaseHelper.ClearTable("StmEvent");

			var evnt1 = Factory.NewWithValidTestData<StmEvent>();
			evnt1.SE_Code = "XXX";
			evnt1.SE_Desc = "McLaren";
			evnt1.SE_IsCustomizable = false;
			evnt1.SE_IsRefernceFormatOverridden = false;

			var evnt2 = Factory.NewWithValidTestData<StmEvent>();
			evnt2.SE_Code = "Z01";
			evnt2.SE_Desc = "McLaren";
			evnt2.SE_IsCustomizable = true;
			evnt2.SE_IsRefernceFormatOverridden = true;

			var evnt3 = Factory.NewWithValidTestData<StmEvent>();
			evnt3.SE_Code = "Z02";
			evnt3.SE_Desc = "MU";
			evnt3.SE_IsCustomizable = true;
			evnt3.SE_IsRefernceFormatOverridden = false;

			Factory.Save();

			var filter = new EventsFilterBusinessObject();
			((ModuleTextFilter)filter[EventsFilterBusinessObject.Descriptions.EventCode]).Property = "Z02";
			((ModuleTextFilter)filter[EventsFilterBusinessObject.Descriptions.EventCode]).IsActive = true;

			var events = new StmEventCollection(Factory);
			events.AdditionalFilter = filter.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { evnt3 }, events);

			filter = new EventsFilterBusinessObject();
			((ModuleTextFilter)filter[EventsFilterBusinessObject.Descriptions.EventDescription]).Property = "McLaren";
			((ModuleTextFilter)filter[EventsFilterBusinessObject.Descriptions.EventDescription]).IsActive = true;
			events.AdditionalFilter = filter.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { evnt1, evnt2 }, events);

			filter = new EventsFilterBusinessObject();
			((ModuleFlagsFilter)filter[EventsFilterBusinessObject.Descriptions.IsCustomizable]).Property0 = true;
			((ModuleFlagsFilter)filter[EventsFilterBusinessObject.Descriptions.IsCustomizable]).IsActive = true;
			events.AdditionalFilter = filter.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { evnt2, evnt3 }, events);

			filter = new EventsFilterBusinessObject();
			((ModuleFlagsFilter)filter[EventsFilterBusinessObject.Descriptions.IsReferenceFormatOverridden]).Property0 = true;
			((ModuleFlagsFilter)filter[EventsFilterBusinessObject.Descriptions.IsReferenceFormatOverridden]).IsActive = true;
			events.AdditionalFilter = filter.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { evnt2 }, events);
		}

		public void TestFilters_ProductivityWiseModeEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = false;

			var filter = new EventsFilterBusinessObject();
			var events = new StmEventCollection(Factory);
			events.AdditionalFilter = filter.Filter;
			AssertNotContains("SE_IsVisibleToPW = 1", events.CompleteFilter.LiteralTextADO);

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			filter = new EventsFilterBusinessObject();
			((ModuleTextFilter)filter[EventsFilterBusinessObject.Descriptions.EventCode]).Property = Events.Arrival.Code;
			((ModuleTextFilter)filter[EventsFilterBusinessObject.Descriptions.EventCode]).IsActive = true;
			events.AdditionalFilter = filter.Filter;
			AssertContains("SE_IsVisibleToPW = 1", events.CompleteFilter.LiteralTextADO);
			AssertEquals("Valid PW event, Valid CW1 event", 1, events.Count);

			filter = new EventsFilterBusinessObject();
			((ModuleTextFilter)filter[EventsFilterBusinessObject.Descriptions.EventCode]).Property = Events.CargoAcceptedAtOriginDepot.Code;
			((ModuleTextFilter)filter[EventsFilterBusinessObject.Descriptions.EventCode]).IsActive = true;
			events.AdditionalFilter = filter.Filter;
			AssertContains("SE_IsVisibleToPW = 1", events.CompleteFilter.LiteralTextADO);
			AssertEquals("Invalid PW event, Valid CW1 event", 0, events.Count);

			filter = new EventsFilterBusinessObject();
			((ModuleTextFilter)filter[EventsFilterBusinessObject.Descriptions.EventCode]).Property = "ZZZ";
			((ModuleTextFilter)filter[EventsFilterBusinessObject.Descriptions.EventCode]).IsActive = true;
			events.AdditionalFilter = filter.Filter;
			AssertContains("SE_IsVisibleToPW = 1", events.CompleteFilter.LiteralTextADO);
			AssertEquals("Invalid PW event, Invalid CW1 event", 0, events.Count);

			filter = new EventsFilterBusinessObject();
			((ModuleTextFilter)filter[EventsFilterBusinessObject.Descriptions.EventCode]).Property = Events.CreditApprovalGranted.Code;
			((ModuleTextFilter)filter[EventsFilterBusinessObject.Descriptions.EventCode]).IsActive = true;
			events.AdditionalFilter = filter.Filter;
			AssertContains("SE_IsVisibleToPW = 1", events.CompleteFilter.LiteralTextADO);
			AssertEquals("Valid PW event, Valid CW1 event", 1, events.Count);

			filter = new EventsFilterBusinessObject();
			((ModuleTextFilter)filter[EventsFilterBusinessObject.Descriptions.EventCode]).Property = Events.CashbookItemUnTicked.Code;
			((ModuleTextFilter)filter[EventsFilterBusinessObject.Descriptions.EventCode]).IsActive = true;
			events.AdditionalFilter = filter.Filter;
			AssertContains("SE_IsVisibleToPW = 1", events.CompleteFilter.LiteralTextADO);
			AssertEquals("Valid PW event, Valid CW1 event", 1, events.Count);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EventsFilterBusinessObject();
		}

		#endregion
	}
}
