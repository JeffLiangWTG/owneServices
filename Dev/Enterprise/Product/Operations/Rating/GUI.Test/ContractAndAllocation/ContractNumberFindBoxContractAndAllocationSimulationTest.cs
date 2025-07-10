using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	sealed class ContractNumberFindBoxContractAndAllocationSimulationTest : RatingTestCase
	{
		[GuiTest]
		public void TestShowPopup_ContractAllocationSimulationForm_ContractButtonShown_AllocationButtonHidden()
		{
			var seenPopup = false;
			ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
			{
				if (dialog is ZForm form)
				{
					try
					{
						var contractButton = form.Controls.Find("SelectContractButton", true).Single() as ZButton;
						var allocationButton = form.Controls.Find("SelectAllocationRouteButton", true).Single() as ZButton;
						var filterStripControl = form.Controls.Find("ContractFilterStripControl", true).Single() as ZFilterStripCommonControl;

						AssertEquals("Contract button shown", true, contractButton.Visible);
						AssertEquals("Allocate button hidden", false, allocationButton.Visible);

						seenPopup = true;

						AssertFiltersPresent(filterStripControl);
					}
					catch
					{
						form.Close();
						throw;
					}
				}
			});

			simulation.ShowPopup();

			AssertEquals("Form was seen", true, seenPopup);
		}

		[GuiTest]
		public void TestShowPopup_ContractAllocationSimulationForm_ShowsContainerFilter()
		{
			var refContainer20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			rateEntry.TI_RC = refContainer20GP.PK;

			var seenPopup = false;
			ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
			{
				if (dialog is ZForm form)
				{
					try
					{
						var filterStripControl = form.Controls.Find("ContractFilterStripControl", true).Single() as ZFilterStripCommonControl;

						AssertFiltersPresent(filterStripControl, new[] { "Container Type (1)", "Container Type (2)" });

						var filters = filterStripControl.FilterBusinessObject.ActiveModuleFilters;
						var containerFilters = filters.Where(x => x.Code.StartsWith(CarrierContractFilterConstants.ContainerType));
						AssertEquals(2, containerFilters.Count());
						var containerBlankFilter = containerFilters.Single(f => (f as IModuleFilterWithComparisonOperator).ComparisonOperator == ModuleTextFilter.ComparisonConstants.IsBlank);
						var containerExactFilter = containerFilters.Single(f => (f as IModuleFilterWithComparisonOperator).ComparisonOperator == ModuleTextFilter.ComparisonConstants.Exact);
						AssertEquals("Container Type Filters should be in the same OR Group", containerExactFilter.OrCategory, containerBlankFilter.OrCategory);
						AssertEquals("DRY", ((ModuleTextFilter)containerExactFilter).Property);
						AssertEquals("Container Type Filters should be next to each other", 1, Math.Abs(filters.IndexOf(f => f.Code == "Container Type (1)") - filters.IndexOf(f => f.Code == "Container Type (2)")));

						seenPopup = true;
					}
					catch
					{
						form.Close();
						throw;
					}
				}
			});

			simulation.ShowPopup();

			AssertEquals("Form was seen", true, seenPopup);
		}

		[GuiTest]
		public void TestShowPopup_ContractAllocationSimulationForm_ShowsAllowHazardousFilter()
		{
			rateEntry.TI_RH_NKCommodityCode = "HAZ";

			var seenPopup = false;
			ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
			{
				if (dialog is ZForm form)
				{
					try
					{
						var filterStripControl = form.Controls.Find("ContractFilterStripControl", true).Single() as ZFilterStripCommonControl;

						AssertFiltersPresent(filterStripControl, new[] { CarrierContractFilterConstants.AllowHazardousCommodities });

						var filters = filterStripControl.FilterBusinessObject.ActiveModuleFilters;
						var allowHazFilter = filters.Single(x => x.Code.StartsWith(CarrierContractFilterConstants.AllowHazardousCommodities)) as ModuleFlagsFilter;
						AssertEquals(ZBool.True, allowHazFilter.Property0);

						seenPopup = true;
					}
					catch
					{
						form.Close();
						throw;
					}
				}
			});

			simulation.ShowPopup();

			AssertEquals("Form was seen", true, seenPopup);
		}

		public void TestUpdatingContractNumberOnRateEntry()
		{
			var actions = simulation as IRatingContractSimulationFormActions;
			AssertEquals(false, actions.IsEnabledAllocationToRoute);
			AssertEquals(true, actions.IsEnabledAllocationToContract);

			var ratingContract = new Mock<IRatingContract>();
			ratingContract.Setup(x => x.RCT_ContractNumber).Returns("1234");

			AssertEquals("Precondition", "666", contractInfo.Object.ContractNumber);
			actions.TryAllocateToContract(ratingContract.Object);
			contractInfo.VerifySet((x) => x.ContractNumber = "1234", Times.Once());
		}

		readonly string[] alwaysPresentFilters = new[]
		{
			CarrierContractFilterConstants.StartDate,
			CarrierContractFilterConstants.ExpiryDate,
			CarrierContractFilterConstants.ContractNumber,
			CarrierContractFilterConstants.ServiceProvider,
			CarrierContractFilterConstants.TransportMode,
			CarrierContractFilterConstants.AllocationRoutes,
		};

		void AssertFiltersPresent(ZFilterStripCommonControl contractFilterStripControl, IEnumerable<string> alsoCheckFor = null)
		{
			var filters = contractFilterStripControl.FilterBusinessObject.ActiveModuleFilters;
			var seenfilterCodes = filters.Select(x => (string)x.Code);
			var expectedFilters = alwaysPresentFilters.Concat(alsoCheckFor ?? Array.Empty<string>());
			AssertContainsExactElementsInAnyOrder(expectedFilters, seenfilterCodes);
		}

		protected override void SetUp()
		{
			var org = Helper.NewOrgHeader("aaa");
			var costing = Helper.NewCosting(org);
			rateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.FCL, "NOSHT", "USHIT", "BAF", 123);

			contractInfo = new Mock<IContractNumberFindBoxPopupSupport>();
			contractInfo.Setup(x => x.ContractNumber).Returns("666");
			contractInfo.Setup(y => y.RateEntry).Returns(rateEntry);

			simulation = new ContractNumberFindBoxContractAndAllocationSimulation(contractInfo.Object);

			disposables = new[] {
				FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true),
				FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)
			};
			ZFormModaliser.ShowDialogsInTest = true;
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposables.ForEach(x => x.Dispose());
		}

		RateEntry rateEntry;
		Mock<IContractNumberFindBoxPopupSupport> contractInfo;
		ContractNumberFindBoxContractAndAllocationSimulation simulation;
		IDisposable[] disposables;
	}
}
