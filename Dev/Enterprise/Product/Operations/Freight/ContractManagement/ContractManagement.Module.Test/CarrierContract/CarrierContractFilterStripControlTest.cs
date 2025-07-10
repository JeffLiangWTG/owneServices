using System;
using System.Reactive.Disposables;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ContractManagement.Business;
using Enterprise.ContractManagement.GUI;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;

namespace Enterprise.ContractManagement.Module.Testing
{
	class CarrierContractFilterStripControlTest : TestCaseWithFactory
	{
		IDisposable SetEnableCarrierContractAllocations(bool value)
		{
			var enableModules = FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			var enableAllocations = FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			var enableRating = FreightConfigurationRegistry.Instance.EnableCarrierContractTariffsAndRates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			return Disposable.Create(() =>
			{
				enableModules.Dispose();
				enableAllocations.Dispose();
				enableRating.Dispose();
			});
		}

		public void TestGridStyleWhenAllocationsModuleEnabled()
		{
			using (SetEnableCarrierContractAllocations(false))
			using (var control = new CarrierContractFilterStripControl())
			{
				AssertEquals("When not using allocations, grid anchor should be top | left | right.", control.Grid.Anchor, AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
				AssertEquals("When not using allocations, grid dock should be default.", control.Grid.Dock, DockStyle.None);
			}

			using (SetEnableCarrierContractAllocations(true))
			using (var control = new CarrierContractFilterStripControl())
			{
				AssertEquals("When using allocations, grid anchor should be default.", control.Grid.Anchor, AnchorStyles.Top | AnchorStyles.Left);
				AssertEquals("When using allocations, grid dock should be fill.", control.Grid.Dock, DockStyle.Fill);
			}
		}

		public void TestGridColumnsWhenAllocationsModuleEnabled()
		{
			var contractColumnCount = 7;
			var allocationColumnCount = 11;

			using (SetEnableCarrierContractAllocations(false))
			using (var control = new CarrierContractFilterStripControl())
			{
				AssertEquals("When not using allocations, none of the allocations columns should show.", control.Grid.ColumnStyles.Count, contractColumnCount);
			}

			using (SetEnableCarrierContractAllocations(true))
			using (var control = new CarrierContractFilterStripControl())
			{
				AssertEquals("When using allocations, allocations columns should show.", control.Grid.ColumnStyles.Count, contractColumnCount + allocationColumnCount);
			}
		}

		IDisposable SetMaxNumberOfRecordsToShowInDisplayGrids(int value) => SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value);

		public void TestGridShowsLimitedRecords()
		{
			var configurationMock = new Mock<IContractSimulationFormConfiguration>();
			var viewCarrierContractsManager = new ViewCarrierContractsManager(Factory, configurationMock.Object);

			var filterDefaultsMock = new Mock<IRatingContractSimulationFilterDefaults>();
			filterDefaultsMock.Setup(filterDefaults => filterDefaults.GetFilterDefaultsForContract()).Returns(new FilterBusinessObjectDefaults());

			var contract1 = Factory.NewWithValidTestData<RatingContract>();
			contract1.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			var contract2 = Factory.NewWithValidTestData<RatingContract>();
			contract2.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;

			Factory.Save();

			var notificationProviderMock = new Mock<IRatingContractSimulationNotificationProvider>();

			using (SetMaxNumberOfRecordsToShowInDisplayGrids(1))
			using (var filterStripControl = new CarrierContractFilterStripControl(configurationMock.Object))
			{
				filterStripControl.SetDataBinding(viewCarrierContractsManager, "");
				filterStripControl.FirePerformSearch();
				var message = ((UnitTestUserNotification)Globals.Message).LastMessage.ToString();
				AssertContains("The current value is set to 1.", message);
			}
		}

		public void TestGridShowsCorrectRecords()
		{
			var configurationMock = new Mock<IContractSimulationFormConfiguration>();
			var viewCarrierContractsManager = new ViewCarrierContractsManager(Factory, configurationMock.Object);

			var filterDefaultsMock = new Mock<IRatingContractSimulationFilterDefaults>();
			filterDefaultsMock.Setup(filterDefaults => filterDefaults.GetFilterDefaultsForContract()).Returns(new FilterBusinessObjectDefaults());

			var contract1 = Factory.NewWithValidTestData<RatingContract>();
			contract1.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			var contract2 = Factory.NewWithValidTestData<RatingContract>();
			contract2.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;

			Factory.Save();

			using (var filterStripControl = new CarrierContractFilterStripControl(configurationMock.Object))
			{
				filterStripControl.SetDataBinding(viewCarrierContractsManager, "");
				filterStripControl.FirePerformSearch();
				AssertContains("2 records", filterStripControl.GetToolStripRecordsFoundLabelText());
			}
		}

		public void TestAllocationRoutesGrid_GetsFilteredByContractFilterStrips()
		{
			var allocationID = "VERYNICE";
			var vessel = "DORSIA";

			var route = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			route.RCA_AllocationLineID = allocationID;
			route.RCA_RV_NKVessel = vessel;

			var contract = route.Contract;
			contract.RCT_ContractType = "PRO";
			contract.RCT_ContractNumber = "IMPRESSIVE";

			Factory.Save();

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ContractAndAllocationsAttachForm(new Mock<IContractSimulationFormConfiguration>().Object))
			{
				form.Show();

				var filterControl = GUITestHelper.FindControl<CarrierContractFilterStripControl>(form.Controls, "ContractFilterStripControl");

				var relatedRoutesFilter = (RelatedAllocationsOfContractFilter)filterControl.FilterBusinessObject["Allocation Routes"];
				relatedRoutesFilter.ComparisonOperator = ModuleGuidForeignCollectionFilter.ComparisonConstants.AnyMatch;
				relatedRoutesFilter.IsActive = true;

				relatedRoutesFilter.SelectedFilters.AddNkFilterStrip(AllocationRouteFilterConstants.Vessel, vessel);

				filterControl.FirePerformSearch();

				AssertEquals("Grid should have 1 contract", 1, filterControl.GridCollection.Count);

				filterControl.Grid.Select(0);
				var gridContract = (CarrierContractForUtilizationSimulation)filterControl.Grid.GetFirstSelectedRow();

				var relatedRoutesAdditionalFilterQuery = gridContract.AllocationRoutesForUtilizationSimulation.AdditionalFilter.LiteralTextADO;
				var expectedVesselQUery = $"RCA_RV_NKVessel = '{vessel}'";
				AssertContains("Filter should be applied to related collection grid", expectedVesselQUery, relatedRoutesAdditionalFilterQuery);
			}
		}

		public void TestAllocationRoutesGrid_GetsFilteredByContractFilterStrips_DifferentOrCategory()
		{
			var allocationID = "VERYNICE";
			var vessel = "DORSIA";
			var voyage = "HIP2BSQRED";

			var route = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			route.RCA_AllocationLineID = allocationID;
			route.RCA_RV_NKVessel = vessel;
			route.RCA_VoyageNumber = voyage;

			var contract = route.Contract;
			contract.RCT_ContractType = "PRO";
			contract.RCT_ContractNumber = "IMPRESSIVE";

			Factory.Save();

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ContractAndAllocationsAttachForm(new Mock<IContractSimulationFormConfiguration>().Object))
			{
				form.Show();

				var filterControl = GUITestHelper.FindControl<CarrierContractFilterStripControl>(form.Controls, "ContractFilterStripControl");

				var relatedRoutesFilter = (RelatedAllocationsOfContractFilter)filterControl.FilterBusinessObject["Allocation Routes"];
				relatedRoutesFilter.ComparisonOperator = ModuleGuidForeignCollectionFilter.ComparisonConstants.AnyMatch;
				relatedRoutesFilter.IsActive = true;

				var vesselFilterStrip = relatedRoutesFilter.SelectedFilters.AddNkFilterStrip(AllocationRouteFilterConstants.Vessel, vessel);
				vesselFilterStrip.OrCategory = FilterOrCategory.Red;

				var relatedRoutesFilter2 = filterControl.FilterBusinessObject.AddFilterStrip<RelatedAllocationsOfContractFilter>("Allocation Routes");
				relatedRoutesFilter2.ComparisonOperator = ModuleGuidForeignCollectionFilter.ComparisonConstants.AnyMatch;
				relatedRoutesFilter2.IsActive = true;

				var voyageFilterStrip = relatedRoutesFilter2.SelectedFilters.AddTextFilterStrip(AllocationRouteFilterConstants.VoyageNumber, voyage);
				voyageFilterStrip.OrCategory = FilterOrCategory.Blue;

				filterControl.FirePerformSearch();

				AssertEquals("Grid should have 1 contract", 1, filterControl.GridCollection.Count);

				filterControl.Grid.Select(0);
				var gridContract = (CarrierContractForUtilizationSimulation)filterControl.Grid.GetFirstSelectedRow();

				var relatedRoutesAdditionalFilterQuery = gridContract.AllocationRoutesForUtilizationSimulation.AdditionalFilter.LiteralTextADO;

				var expectedVesselQuery = $"(RCA_RV_NKVessel = '{vessel}' or (RCA_JX_SailingSchedule IN (SELECT JX_PK FROM dbo.JobSailing WHERE JX_JA IN (SELECT JA_PK FROM dbo.JobVoyOrigin WHERE JA_JV IN (SELECT JV_PK FROM dbo.JobVoyage WHERE JV_RV_NKVessel = '{vessel}' and JV_IsActive = 1))))) and (RCA_VoyageNumber = '{voyage}'";

				AssertContains("Filter should be applied to related collection grid", expectedVesselQuery, relatedRoutesAdditionalFilterQuery);
			}
		}

		public void TestAllocationRoutesGrid_GetsFilteredByContractFilterStrips_NoMatchingContract()
		{
			var allocationID = "VERYNICE";
			var vessel = "DORSIA";

			var route = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			var contract = route.Contract;
			contract.RCT_ContractType = "PRO";
			contract.RCT_ContractNumber = "IMPRESSIVE";

			Factory.Save();

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ContractAndAllocationsAttachForm(new Mock<IContractSimulationFormConfiguration>().Object))
			{
				form.Show();

				var filterControl = GUITestHelper.FindControl<CarrierContractFilterStripControl>(form.Controls, "ContractFilterStripControl");

				var relatedRoutesFilter = (RelatedAllocationsOfContractFilter)filterControl.FilterBusinessObject["Allocation Routes"];
				relatedRoutesFilter.ComparisonOperator = ModuleGuidForeignCollectionFilter.ComparisonConstants.AnyMatch;
				relatedRoutesFilter.IsActive = true;

				relatedRoutesFilter.SelectedFilters.AddTextFilterStrip(AllocationRouteFilterConstants.AllocationRouteID, allocationID);
				relatedRoutesFilter.SelectedFilters.AddNkFilterStrip(AllocationRouteFilterConstants.Vessel, vessel);

				filterControl.FirePerformSearch();

				AssertEquals("Grid should have no contracts", 0, filterControl.GridCollection.Count);
			}
		}
	}
}
