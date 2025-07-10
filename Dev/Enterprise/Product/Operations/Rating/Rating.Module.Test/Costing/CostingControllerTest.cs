using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(CostingController))]
	public class CostingControllerTest : RatingControllerTest<CostingController, Costing>
	{
		#region LoadBusinessEntity

		public void TestLoadBusinessEntity_OfType_RatingHeader()
		{
			var org = Helper.NewOrgHeader();
			var ratingHeader = Helper.NewCosting(org);
			Factory.Save();

			var controller = new CostingController();
			var reloadedRatingHeader = controller.LoadBusinessEntity_ForTest(Factory, ratingHeader.PK);

			AssertType<Costing>(ratingHeader);
			AssertType<Costing>(reloadedRatingHeader);
		}

		public void TestLoadBusinessEntity_OfType_RatingContract()
		{
			var org = Helper.NewOrgHeader();
			var ratingHeader = Helper.NewCosting(org);
			var ratingContract = Helper.NewRatingContract(org, "C1234");

			Factory.Save();

			var controller = new CostingController();
			var reloadedRatingHeader = controller.LoadBusinessEntity_ForTest(Factory, ratingContract.PK);

			AssertType<Costing>(ratingHeader);
			AssertType<Costing>(reloadedRatingHeader);
			AssertEquals("The rating contract's organisation's costing header must be the same as the returned value from LoadBusinessEntity", ratingHeader.PK, ((Costing)reloadedRatingHeader).PK);
		}

		public void TestLoadBusinessEntity_OfType_RatingContractAllocation()
		{
			var org = Helper.NewOrgHeader();
			var ratingHeader = Helper.NewCosting(org);
			var ratingContract = Helper.NewRatingContract(org, "C1234");
			var ratingContractAllocation = Helper.NewRatingContractAllocation(ratingContract, "AUSYD", "USORD");

			Factory.Save();

			var controller = new CostingController();
			var reloadedRatingHeader = controller.LoadBusinessEntity_ForTest(Factory, ratingContractAllocation.PK);

			AssertType<Costing>(ratingHeader);
			AssertType<Costing>(reloadedRatingHeader);
			AssertEquals("The rating contract allocation's rating contract's organisation's costing header must be the same as the returned value from LoadBusinessEntity", ratingHeader.PK, ((Costing)reloadedRatingHeader).PK);
		}

		public void TestLoadBusinessEntity_OfType_RatingContractAllocation_ButNoCostingFound()
		{
			var org = Helper.NewOrgHeader();
			var ratingContract = Helper.NewRatingContract(org, "C1234");
			var ratingContractAllocation = Helper.NewRatingContractAllocation(ratingContract, "AUSYD", "USORD");

			Factory.Save();

			var controller = new CostingController();
			var reloadedRatingHeader = controller.LoadBusinessEntity_ForTest(Factory, ratingContractAllocation.PK);

			AssertNull("No costing should be found", reloadedRatingHeader);
		}

		#endregion

		#region ShowEditForm

		public void TestShowEditForm_WithRatingHeader_FiltersNotSet()
		{
			var org = Helper.NewOrgHeader();
			var ratingHeader = Helper.NewCosting(org);

			Factory.Save();

			var controller = new CostingController();
			// Calling LoadBusinessEntity_ForTest makes the controller save some state regarding
			// the object related to the pk that was passed in. This simulates what occurs when
			// a URL for ShowEditForm is being handled
			controller.LoadBusinessEntity_ForTest(Factory, ratingHeader.PK);

			using (var form = controller.ShowEditForm(ratingHeader) as CostingForm)
			{
				var filterBizo = GUITestHelper.FindControl<RateEntryStripControl>(form.Controls).FilterBusinessObject;
				var startDates = GetFilters<ModuleDateFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.StartDate);
				var endDates = GetFilters<ModuleDateFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.EndDate);
				var contractNumbers = GetFilters<ModuleTextFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.CarrierContractNumber);
				var transportModes = GetFilters<ModuleTextFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.TransportMode);
				var locationFilters = GetFilters<ModuleLocationFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.OriginDestination);
				var containerTypeFilters = GetFilters<ModuleGuidFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.ContainerType);

				// When ShowEditForm is called with a bizo from a RatingHeader PK there is no
				// data available to fill the filters.
				Assert("The start date should be empty", startDates.IsNullOrEmpty());
				Assert("The end date should be empty", endDates.IsNullOrEmpty());
				Assert("The contract number should be empty", contractNumbers.IsNullOrEmpty());
				Assert("The transport mode should be empty", transportModes.IsNullOrEmpty());
				Assert("Container type should be empty", containerTypeFilters.IsNullOrEmpty());
				Assert("Because location is one of the default filters. If we dont apply our new filters from the RatingContract then we leave the previous filters untouched.", !locationFilters.IsNullOrEmpty());
			}
		}

		public void TestShowEditForm_WithRatingContract_FiltersSet()
		{
			var today = ZDate.Today;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);

			var org = Helper.NewOrgHeader();
			var ratingHeader = Helper.NewCosting(org);
			var ratingContract = Helper.NewRatingContract(org, "C1234", startDate: yesterday, endDate: tomorrow, transportMode: "SEA", containerType: ContainerTypes.DryStorage);

			Factory.Save();

			var controller = new CostingController();
			// Calling LoadBusinessEntity_ForTest makes the controller save some state regarding
			// the object related to the pk that was passed in. This simulates what occurs when
			// a URL for ShowEditForm is being handled
			controller.LoadBusinessEntity_ForTest(Factory, ratingContract.PK);

			using (var form = controller.ShowEditForm(ratingHeader) as CostingForm)
			{
				form.Show();
				Application.DoEvents();

				var filter = form.Controls.Cast<Control>().OfType<RateEntryFilterStripControl>().Single();
				var rateEntryStripControls = filter.Controls.Cast<Control>().OfType<RateEntryStripControl>(); // there can be more than one (one per tab - rating category)
				var filterBizo = rateEntryStripControls.Single(s => s.Collection.RateEntryType.IsEmpty).FilterBusinessObject; // empty RateEntryType corresponds to the Summary Rate tab
				var startDates = GetFilters<ModuleDateFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.StartDate);
				var endDates = GetFilters<ModuleDateFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.EndDate);
				var contractNumbers = GetFilters<ModuleTextFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.CarrierContractNumber);
				var transportModes = GetFilters<ModuleTextFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.TransportMode);
				var locationFilters = GetFilters<ModuleLocationFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.OriginDestination);
				var containerTypeFilters = GetFilters<ModuleGuidFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.ContainerType);

				var actualStartDates = startDates.Select(d => $"{d.Property1.Date}|{d.Property2.Date}").ToArray();
				AssertContainsExactElementsInAnyOrder(
					"The ModuleDateFilter contains two dates. Only the first one is set for the 'startDate'.",
					new[] { $"{yesterday}|{ZDate.Empty}" },
					actualStartDates
				);

				var actualEndDates = endDates.Select(d => $"{d.Property1.Date}|{d.Property2.Date}").ToArray();
				AssertContainsExactElementsInAnyOrder(
					"The ModuleDateFilter contains two dates. Only the last one is set for the 'endDate'.",
					new[] { $"{ZDate.Empty}|{tomorrow}" },
					actualEndDates
				);

				var actualContractNumbers = contractNumbers.Select(c => (string)c.Property).ToArray();
				AssertContainsExactElementsInAnyOrder(
					new[] { "C1234" },
					actualContractNumbers
				);

				var actualTransportModes = transportModes.Select(t => (string)t.Property).ToArray();
				AssertContainsExactElementsInAnyOrder(
					new[] { "SEA" },
					actualTransportModes
				);

				AssertEquals(
					"The Container type filter supports the 'Matches filters' Options.",
					true,
					containerTypeFilters.Single().IsFilterCollectionComparisonOperatorSelected()
				);

				AssertEquals(
					RefContainerFilterBusinessObject.Constants.Codes.ContainerType,
					containerTypeFilters.Single().SelectedFilters.ActiveModuleFilters.Single().OriginalCode
				);

				AssertEquals(0, locationFilters.Count());
			}
		}

		public void TestShowEditForm_WithRatingContract_FiltersSet_AlreadyOpenedForm()
		{
			var today = ZDate.Today;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);

			var org = Helper.NewOrgHeader();
			var ratingHeader = Helper.NewCosting(org);
			var ratingContract = Helper.NewRatingContract(org, "C1234", startDate: yesterday, endDate: tomorrow, transportMode: "SEA");

			Factory.Save();

			var controller = new TestCostingController();
			// Calling LoadBusinessEntity_ForTest makes the controller save some state regarding
			// the object related to the pk that was passed in. This simulates what occurs when
			// a URL for ShowEditForm is being handled
			controller.LoadBusinessEntity_ForTest(Factory, ratingContract.PK);

			using (var form = controller.ShowEditForm(ratingHeader) as TestCostingForm)
			{
				form.Show();
				Application.DoEvents();

				var filter = form.Controls.Cast<Control>().OfType<RateEntryFilterStripControl>().Single();
				var rateEntryStripControls = filter.Controls.Cast<Control>().OfType<RateEntryStripControl>(); // there can be more  than one. (one per tab (rating category))
				var filterBizo = rateEntryStripControls.Single(s => s.Collection.RateEntryType.IsEmpty).FilterBusinessObject; // empty RateEntryType corresponds to the Summary Rate tab

				var initialStartDate = GetFilters<ModuleDateFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.StartDate).Single().Property1;
				var initialEndDate = GetFilters<ModuleDateFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.EndDate).Single().Property2;

				// update the contract.
				ratingContract.RCT_StartDate = ratingContract.RCT_StartDate.AddMonths(1);
				ratingContract.RCT_EndDate = ratingContract.RCT_EndDate.AddMonths(1);
				Factory.Save();

				// Different controller, prepare the data again.
				controller = new TestCostingController();
				controller.LoadBusinessEntity_ForTest(Factory, ratingContract.PK);

				// Re-activate the form.
				form.DoActivate();
				Application.DoEvents();

				// Filters should have updated
				var laterStartDate = GetFilters<ModuleDateFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.StartDate).Single().Property1;
				var laterEndDate = GetFilters<ModuleDateFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.EndDate).Single().Property2;
				AssertEquals(yesterday.AddMonths(1).ToZDateTime(), laterStartDate);
				AssertEquals(tomorrow.AddMonths(1).ToZDateTime(), laterEndDate);
			}
		}

		public void TestShowEditForm_WithRatingContract_NoExpiryDate_FiltersSet()
		{
			var today = ZDate.Today;
			var yesterday = today.AddDays(-1);

			var org = Helper.NewOrgHeader();
			var ratingHeader = Helper.NewCosting(org);
			var ratingContract = Helper.NewRatingContract(org, "C1234", startDate: yesterday, endDate: ZDate.Empty, transportMode: "SEA");

			Factory.Save();

			var controller = new CostingController();
			// Calling LoadBusinessEntity_ForTest makes the controller save some state regarding
			// the object related to the pk that was passed in. This simulates what occurs when
			// a URL for ShowEditForm is being handled
			controller.LoadBusinessEntity_ForTest(Factory, ratingContract.PK);

			using (var form = controller.ShowEditForm(ratingHeader) as CostingForm)
			{
				form.Show();
				Application.DoEvents();

				var filter = form.Controls.Cast<Control>().OfType<RateEntryFilterStripControl>().Single();
				var rateEntryStripControls = filter.Controls.Cast<Control>().OfType<RateEntryStripControl>(); // there can be more than one (one per tab (rating category))
				var filterBizo = rateEntryStripControls.Single(s => s.Collection.RateEntryType.IsEmpty).FilterBusinessObject; // empty RateEntryType corresponds to the Summary Rate tab
				var startDates = GetFilters<ModuleDateFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.StartDate);
				var endDates = GetFilters<ModuleDateFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.EndDate);
				var contractNumbers = GetFilters<ModuleTextFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.CarrierContractNumber);
				var transportModes = GetFilters<ModuleTextFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.TransportMode);
				var locationFilters = GetFilters<ModuleLocationFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.OriginDestination);

				var expectedStartDates = new[] { $"{yesterday}|{ZDate.Empty}" };
				var actualStartDates = startDates.Select(d => $"{d.Property1.Date}|{d.Property2.Date}").ToArray();
				AssertContainsExactElementsInAnyOrder(
					"The ModuleDateFilter contains two dates. Only the first one is set for 'startDate'",
					expectedStartDates,
					actualStartDates
				);

				var expectedEndDates = new[] { $"{ZDate.Empty}|{ZDate.Empty}" };
				var actualEndDates = endDates.Select(d => $"{d.Property1.Date}|{d.Property2.Date}").ToArray();
				AssertContainsExactElementsInAnyOrder(
					"The ModuleDateFilter contains two dates. Only the last one is set for 'endDate'. In this case, there is no endDate",
					expectedEndDates,
					actualEndDates
				);

				var expectedContractNumbers = new[] { "C1234" };
				var actualContractNumbers = contractNumbers.Select(c => (string)c.Property).ToArray();
				AssertContainsExactElementsInAnyOrder(expectedContractNumbers, actualContractNumbers);

				var expectedTransportModes = new[] { "SEA" };
				var actualTransportModes = transportModes.Select(t => (string)t.Property).ToArray();
				AssertContainsExactElementsInAnyOrder(expectedTransportModes, actualTransportModes);

				AssertEquals(0, locationFilters.Count());
			}
		}

		public void TestShowEditForm_WithRatingContract_ButFormShowDifferentBizo_FiltersNotSet()
		{
			var today = ZDate.Today;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);

			var org = Helper.NewOrgHeader();
			var org2 = Helper.NewOrgHeader();
			var ratingHeader = Helper.NewCosting(org);
			var ratingHeader2 = Helper.NewCosting(org2);
			var ratingContract = Helper.NewRatingContract(org, "C1234", startDate: yesterday, endDate: tomorrow, transportMode: "SEA");

			Factory.Save();

			var controller = new CostingController();
			// Calling LoadBusinessEntity_ForTest makes the controller save some state regarding
			// the object related to the pk that was passed in. This simulates what occurs when
			// a URL for ShowEditForm is being handled
			controller.LoadBusinessEntity_ForTest(Factory, ratingContract.PK);

			using (var form = controller.ShowEditForm(ratingHeader2) as CostingForm)
			{
				var filterBizo = GUITestHelper.FindControl<RateEntryStripControl>(form.Controls).FilterBusinessObject;
				var startDates = GetFilters<ModuleDateFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.StartDate);
				var endDates = GetFilters<ModuleDateFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.EndDate);
				var contractNumbers = GetFilters<ModuleTextFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.CarrierContractNumber);
				var transportModes = GetFilters<ModuleTextFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.TransportMode);
				var locationFilters = GetFilters<ModuleLocationFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.OriginDestination);

				// When ShowEditForm is called with a bizo with a PK *different*
				// to the PK the LoadBusinessEntity got given, then do not use any
				// saved filter information.
				Assert("The start date should be empty", startDates.IsNullOrEmpty());
				Assert("The end date should be empty", endDates.IsNullOrEmpty());
				Assert("The contract number should be empty", contractNumbers.IsNullOrEmpty());
				Assert("The transport mode should be empty", transportModes.IsNullOrEmpty());
				Assert("Because location is one of the default filters. If we dont apply our new filters from the RatingContract then we leave the previous filters untouched.", !locationFilters.IsNullOrEmpty());
			}
		}

		public void TestShowEditForm_WithRatingContractAllocation_FiltersSet()
		{
			var today = ZDate.Today;
			var twoDaysAgo = today.AddDays(-2);
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var dayAfterTomorrow = today.AddDays(2);
			var container = Factory.LoadTop1(typeof(RefContainer), new ZQuery());

			var org = Helper.NewOrgHeader();
			var ratingHeader = Helper.NewCosting(org);
			var ratingContract = Helper.NewRatingContract(org, "C1234", startDate: twoDaysAgo, endDate: dayAfterTomorrow, transportMode: "SEA");
			var ratingContractAllocation = Helper.NewRatingContractAllocation(ratingContract, startDate: yesterday, endDate: tomorrow, originUNLOCO: "AUSYD", destinationUNLOCO: "USORD", container: container.PK);

			Factory.Save();

			var controller = new CostingController();
			// Calling LoadBusinessEntity_ForTest makes the controller save some state regarding
			// the object related to the pk that was passed in. This simulates what occurs when
			// a URL for ShowEditForm is being handled
			controller.LoadBusinessEntity_ForTest(Factory, ratingContractAllocation.PK);

			using (var form = controller.ShowEditForm(ratingHeader) as CostingForm)
			{
				form.Show();
				Application.DoEvents();

				var filter = form.Controls.Cast<Control>().OfType<RateEntryFilterStripControl>().Single();
				var rateEntryStripControls = filter.Controls.Cast<Control>().OfType<RateEntryStripControl>();
				var filterBizo = rateEntryStripControls.Single(s => s.Collection.RateEntryType.IsEmpty).FilterBusinessObject; // empty RateEntryType corresponds to the Summary Rate tab
				var startDates = GetFilters<ModuleDateFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.StartDate);
				var endDates = GetFilters<ModuleDateFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.EndDate);
				var contractNumbers = GetFilters<ModuleTextFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.CarrierContractNumber);
				var transportModes = GetFilters<ModuleTextFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.TransportMode);
				var locationFilters = GetFilters<ModuleLocationFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.OriginDestination);
				var containerTypeFilters = GetFilters<ModuleGuidFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.ContainerType);

				var actualStartDates = startDates.Select(d => $"{d.Property1.Date}|{d.Property2.Date}").ToArray();
				var expectedStartDates = new[] { $"{yesterday}|{ZDate.Empty}" };
				AssertContainsExactElementsInAnyOrder(
					"The ModuleDateFilter contains two dates. Only the first one is set for 'startDate'",
					expectedStartDates,
					actualStartDates
				);

				var actualEndDates = endDates.Select(d => $"{d.Property1.Date}|{d.Property2.Date}").ToArray();
				var expectedEndDates = new[] { $"{ZDate.Empty}|{tomorrow}" };
				AssertContainsExactElementsInAnyOrder(
					"The ModuleDateFilter contains two dates. Only the last one is set for 'endDate'",
					expectedEndDates,
					actualEndDates
				);

				var actualContractNumbers = contractNumbers.Select(c => (string)c.Property).ToArray();
				var expectedContractNumbers = new[] { "C1234" };
				AssertContainsExactElementsInAnyOrder(expectedContractNumbers, actualContractNumbers);

				var actualTransportModes = transportModes.Select(t => (string)t.Property).ToArray();
				var expectedTransportModes = new[] { "SEA" };
				AssertContainsExactElementsInAnyOrder(expectedTransportModes, actualTransportModes);

				var actualLocationFilters = locationFilters.Select(l => $"{(string)l.Property1}|{(string)l.Property2}").ToArray();
				var expectedLocationFilters = new[] { "AUSYD|USORD" };
				AssertContainsExactElementsInAnyOrder(expectedLocationFilters, actualLocationFilters);

				Assert(
					"The Container type filter supports the 'Matches filters' option",
					containerTypeFilters.Single().IsFilterCollectionComparisonOperatorSelected()
				);

				AssertEquals(
					RefContainerFilterBusinessObject.Constants.Codes.ContainerCode,
					containerTypeFilters.Single().SelectedFilters.ActiveModuleFilters.Single().OriginalCode
				);
			}
		}

		public void TestShowEditForm_WithRatingContractAllocationsSailingScheduleValues()
		{
			var today = ZDate.Today;
			var twoDaysAgo = today.AddDays(-2);
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var dayAfterTomorrow = today.AddDays(2);
			var container = Factory.LoadTop1(typeof(RefContainer), new ZQuery());

			var org = Helper.NewOrgHeader();
			var ratingHeader = Helper.NewCosting(org);
			var ratingContract = Helper.NewRatingContract(org, "C1234", startDate: twoDaysAgo, endDate: dayAfterTomorrow, transportMode: "SEA");
			var ratingContractAllocation = Helper.NewRatingContractAllocation(ratingContract, startDate: yesterday, endDate: tomorrow, originUNLOCO: "AUMEL", destinationUNLOCO: "USORD", container: container.PK);

			ratingContract.RCT_ContractType = RatingContractTypes.Provider;

			var jobSailing = Factory.New<IJobSailing>();
			var voyageOrigin = Factory.New<IVoyageOrigin>();
			var voyageDestination = Factory.New<IVoyageDestination>();
			var jobVoyage = Factory.New<IJobVoyage>();

			voyageOrigin.JA_JV = jobVoyage.PK;
			voyageDestination.JB_JV = jobVoyage.PK;
			jobSailing.JX_JA = voyageOrigin.PK;
			jobSailing.JX_JB = voyageDestination.PK;

			voyageOrigin.JA_RL_NKPortOfLoading = "AUSYD";
			voyageDestination.JB_RL_NKPortOfDischarge = "GBFXT";
			jobVoyage.JV_RV_NKVessel = "Titanic 2";
			jobVoyage.JV_VoyageFlight = "11111";
			jobSailing.JX_ServiceString = "hello I am a boat";

			ratingContractAllocation.RCA_JX_SailingSchedule = jobSailing.PK;

			Factory.Save();

			var controller = new CostingController();
			// Calling LoadBusinessEntity_ForTest makes the controller save some state regarding
			// the object related to the pk that was passed in. This simulates what occurs when
			// a URL for ShowEditForm is being handled
			controller.LoadBusinessEntity_ForTest(Factory, ratingContractAllocation.PK);

			using (var form = controller.ShowEditForm(ratingHeader) as CostingForm)
			{
				form.Show();
				Application.DoEvents();

				var filter = form.Controls.Cast<Control>().OfType<RateEntryFilterStripControl>().Single();
				var rateEntryStripControls = filter.Controls.Cast<Control>().OfType<RateEntryStripControl>();
				var filterBizo = rateEntryStripControls.Single(s => s.Collection.RateEntryType.IsEmpty).FilterBusinessObject; // empty RateEntryType corresponds to the Summary Rate tab
				var locationFilters = GetFilters<ModuleLocationFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.OriginDestination);

				var actual = locationFilters
					.Select(l => $"{(string)l.Property1}|{(string)l.Property2}")
					.ToArray();

				var expected = new[] { "AUSYD|GBFXT" };

				AssertContainsExactElementsInAnyOrder(expected, actual);
			}
		}

		public void TestShowEditForm_WithRatingContract_HazardousCommodityNotAllowed_FiltersSet()
		{
			var today = ZDate.Today;
			var twoDaysAgo = today.AddDays(-2);
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var dayAfterTomorrow = today.AddDays(2);
			var container = Factory.LoadTop1(typeof(RefContainer), new ZQuery());

			var org = Helper.NewOrgHeader();
			var ratingHeader = Helper.NewCosting(org);
			var ratingContract = Helper.NewRatingContract(org, "C1234", startDate: twoDaysAgo, endDate: dayAfterTomorrow, transportMode: "SEA", allowHazardousCommodities: false);
			var ratingContractAllocation = Helper.NewRatingContractAllocation(ratingContract, startDate: yesterday, endDate: tomorrow, originUNLOCO: "AUSYD", destinationUNLOCO: "USORD", container: container.PK);
			ratingContract.Factory.Save();

			var controller = new CostingController();
			// Calling LoadBusinessEntity_ForTest makes the controller save some state regarding
			// the object related to the pk that was passed in. This simulates what occurs when
			// a URL for ShowEditForm is being handled
			controller.LoadBusinessEntity_ForTest(Factory, ratingContractAllocation.PK);

			using (var form = controller.ShowEditForm(ratingHeader) as CostingForm)
			{
				form.Show();
				Application.DoEvents();

				var filter = form.Controls.Cast<Control>().OfType<RateEntryFilterStripControl>().Single();
				var rateEntryStripControls = filter.Controls.Cast<Control>().OfType<RateEntryStripControl>(); // there can be more than one. (one per tab (rating category))
				var filterBizo = rateEntryStripControls.Single(s => s.Collection.RateEntryType.IsEmpty).FilterBusinessObject; // empty RateEntryType corresponds to the Summary Rate tab
				var commodityCodeFilters = GetFilters<ModuleNkFilter>(filterBizo, RateEntryFilterUtility.Constants.Codes.CommodityCode);
				AssertEquals(2, commodityCodeFilters.Count());

				var blankCommodityFilter = commodityCodeFilters.ElementAt(0);
				AssertEquals(ModuleTextFilter.ComparisonConstants.IsBlank, blankCommodityFilter.ComparisonOperator);
				var nonHazardousCommodityFilter = commodityCodeFilters.ElementAt(1);
				Assert("The Commodity filter supports the 'Matches filters' options.", nonHazardousCommodityFilter.IsFilterCollectionComparisonOperatorSelected());
				AssertEquals(
					RefCommodityCodeFilterBusinessObject.FilterCodes.IsHazardous,
					nonHazardousCommodityFilter.SelectedFilters.ActiveModuleFilters.Single().OriginalCode
				);
			}
		}

		#endregion

		IEnumerable<TModuleFilter> GetFilters<TModuleFilter>(FilterStripBusinessObject strip, ZString code) where TModuleFilter : ModuleFilter =>
			strip.ActiveModuleFilters.OfType<TModuleFilter>().Where(x => x.OriginalCode == code);

		#region Checkpoints and security

		protected override SecurityCheckpoint DefaultCheckPointForView
		{
			get { return Env.Security.CostingRatesView; }
		}

		protected override SecurityCheckpoint DefaultCheckPointForEdit
		{
			get { return Env.Security.CostingRatesEdit; }
		}

		protected override SecurityCheckpoint DefaultCheckPointForDelete
		{
			get { return Env.Security.CostingRatesDelete; }
		}

		protected override SecurityCheckpoint DefaultCheckPointForCopy
		{
			get { return Env.Security.CostingRatesNew; }
		}

		protected override SecurityCheckpoint DefaultCheckPointForNew
		{
			get { return Env.Security.CostingRatesNew; }
		}

		public void TestCheckPointsForView()
		{
			AssertSecurityCheckPointsForView(Env.Security.GlobalCostingRatesView);
		}

		public void TestCheckPointsForNew()
		{
			AssertSecurityCheckPointsForNew(Env.Security.GlobalCostingRatesNew);
		}

		public void TestCheckPointsForEdit()
		{
			AssertSecurityCheckPointsForEdit(Env.Security.GlobalCostingRatesEdit);
		}

		public void TestCheckPointsForDelete()
		{
			AssertSecurityCheckPointsForDelete(Env.Security.GlobalCostingRatesDelete);
		}

		#region CRM Security

		protected override CRMSecurity DefaultCRMSecurity
		{
			get
			{
				return Env.Security.CostingRatesCRMSecurity;
			}
		}

		public void TestCRMSecurityCheckpoints()
		{
			var bizObjWithoutAccess = Factory.NewWithValidTestData<Costing>();
			bizObjWithoutAccess.Header.MiscServ.OM_GG_OrgSecurityGroup = Factory.NewWithValidTestData<GlbGroup>().PK;
			CRMSecurityProviderTest<Costing>.AssertController(new CostingController(), bizObjWithoutAccess, Env.Security.CostingRatesCRMSecurity);
		}

		#endregion

		#endregion

		#region Implementation

		protected override ControllerID GetControllerID() => ControllerIDs.Costing;

		protected override RatingHeader GetRatingHeader() => Helper.NewCosting(Helper.NewOrgHeader());

		protected override RatingHeader GetGlobalRatingHeader() => Helper.NewGlobalCosting(Helper.NewOrgHeader());

		#endregion
	}

	internal class TestCostingController : CostingController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new TestCostingForm((Costing)businessEntity);
		}
	}

	internal class TestCostingForm : CostingForm
	{
		public TestCostingForm(Costing costing) : base(costing)
		{
		}

		public void DoActivate()
		{
			OnActivated(EventArgs.Empty);
		}
	}
}
