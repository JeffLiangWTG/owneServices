using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
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
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Rating.GUI.Testing
{
	public class RateChooserContractsAndAllocationSimulationTest : RatingTestCase
	{
		[GuiTest]
		public void TestShowPopup_ContractAllocationSimulationForm_ContractButtonHiddenAndAllocationButtonHidden()
		{
			var refContainer20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var aalshi = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "AALSHI"));
			IEnumerable<IForwardingContainer> GetContainers()
			{
				var container1 = new Mock<IForwardingContainer>();
				container1.Setup(x => x.JC_ContainerCount).Returns(5);
				container1.Setup(x => x.JC_RC).Returns(refContainer20GP.PK);

				yield return container1.Object;
			}

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var effectiveDate = ZDateTime.Now;
				var destination = "USHIT";
				var origin = "NOSHT";
				var serviceProviderPK = aalshi.PK;
				var contractNumber = "ToiletPaper";
				var containers = new Mock<IForwardingContainerCollection>();
				containers.Setup(x => x.GetEnumerator()).Returns(GetContainers().GetEnumerator());

				var simulation = new RateChooserContractsAndAllocationSimulation(contractNumber, serviceProviderPK, origin, destination, effectiveDate);

				var seenPopup = false;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					if (dialog is ZForm form)
					{
						try
						{
							AssertEquals("Correct form shown", "ContractAndAllocationsAttachForm", form.Name);

							var contractButton = form.Controls.Find("SelectContractButton", true).Single() as ZButton;
							var allocationButton = form.Controls.Find("SelectAllocationRouteButton", true).Single() as ZButton;
							var filterStripControl = form.Controls.Find("ContractFilterStripControl", true).Single() as ZFilterStripCommonControl;

							AssertEquals("Contract button hidden", false, contractButton.Visible);
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
		}

		[GuiTest]
		public void TestShowPopup_RateChooserAllocationSimulationForm_ShowsAllowHazardousFilter()
		{
			RateEntry rateEntry;
			RateChooserContractsAndAllocationSimulation simulation;
			var aalshi = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "AALSHI"));
			var effectiveDate = ZDateTime.Now;
			var destination = "USHIT";
			var origin = "NOSHT";
			var serviceProviderPK = aalshi.PK;
			var contractNumber = "YES";

			var org = Helper.NewOrgHeader("aaa");
			var costing = Helper.NewCosting(org);
			rateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.FCL, "NOSHT", "USHIT", "BAF", 123);
			rateEntry.TI_RH_NKCommodityCode = "HAZ";
			var commodity = rateEntry.Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, rateEntry.TI_RH_NKCommodityCode);
			var seenPopup = false;

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				simulation = new RateChooserContractsAndAllocationSimulation(contractNumber, serviceProviderPK, origin, destination, effectiveDate, commodity);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					if (dialog is ZForm form)
					{
						try
						{
							var filterStripControl = form.Controls.Find("ContractFilterStripControl", true).Single() as ZFilterStripCommonControl;
							var filters = filterStripControl.FilterBusinessObject.ActiveModuleFilters;
							var allowHazFilter = filters.Single(x => x.Code.StartsWith(CarrierContractFilterConstants.AllowHazardousCommodities)) as ModuleFlagsFilter;
							AssertEquals(ZBool.True, allowHazFilter.Property2);

							seenPopup = true;
						}
						catch
						{
							form.Close();
							throw;
						}
					}
				});
			}

			simulation.ShowPopup();

			AssertEquals("Form was seen", true, seenPopup);
		}

		void AssertFiltersPresent(ZFilterStripCommonControl contractFilterStripControl, IEnumerable<string> alsoCheckFor = null)
		{
			var filters = contractFilterStripControl.FilterBusinessObject.ActiveModuleFilters;
			var seenfilterCodes = filters.Select(x => (string)x.Code);
			var expectedFilters = alwaysPresentFilters.Concat(alsoCheckFor ?? Array.Empty<string>());
			AssertContainsExactElementsInAnyOrder(expectedFilters, seenfilterCodes);
		}

		readonly string[] alwaysPresentFilters = new[]
		{
			CarrierContractFilterConstants.ContractNumber,
			CarrierContractFilterConstants.ExpiryDate,
			CarrierContractFilterConstants.ServiceProvider,
			CarrierContractFilterConstants.StartDate,
			CarrierContractFilterConstants.AllocationRoutes
		};
	}
}
