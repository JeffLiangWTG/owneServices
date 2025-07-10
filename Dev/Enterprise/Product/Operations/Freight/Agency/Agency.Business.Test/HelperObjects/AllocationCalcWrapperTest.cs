using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AllocationCalcWrapperTest : BaseAgencyTest
	{
		public void TestDataRefreshIssue()
		{
			JobSailing sailingInOtherFactory;
			{
				BusinessObjectFactory otherFactory = new BusinessObjectFactory();
				JobVoyage voyageInOtherFactory = otherFactory.New<JobVoyage>();
				voyageInOtherFactory.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyageInOtherFactory.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
				sailingInOtherFactory = voyageInOtherFactory.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "NLAMS");
				otherFactory.Save();
			}

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = sailingInOtherFactory.PK;
			AssertEquals(AllocationMethodList.Codes.NotSet, shipment.Allocation.AllocationMethod);
			sailingInOtherFactory.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Ignore;
			sailingInOtherFactory.Factory.Save();
			shipment.Allocation.UpdateAllocatedValues();
			AssertEquals(AllocationMethodList.Codes.Ignore, shipment.Allocation.AllocationMethod);
		}

		public void TestAllocation()
		{
			SetupShipment(true, false, false, true);
			AssertEquals("Allocated TEU", 9m, wrapper.Allocated_TEU);
			AssertEquals("Allocated Power Points", 3m, wrapper.Allocated_PowerPoints);
			AssertEquals("Allocated Tonnes", 50m, wrapper.Allocated_Tonnes);
			AssertEquals("Allocated Volume", 40m, wrapper.Allocated_Volume);
			AssertEquals("Allocated Area", 12m, wrapper.Allocated_Area);
			AssertEquals("Allocation Method", AllocationMethodList.Codes.Sailing, wrapper.AllocationMethod);
			FreightConfigurationRegistry.Instance.DefaultOverAllocationPercent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			AssertEquals("Allocated TEU", 9.9m, wrapper.Allocated_TEU);
			AssertEquals("Allocated Power Points", 3m, wrapper.Allocated_PowerPoints);
			AssertEquals("Allocated Tonnes", 55m, wrapper.Allocated_Tonnes);
			AssertEquals("Allocated Volume", 44m, wrapper.Allocated_Volume);
			AssertEquals("Allocated Area", 13.2m, wrapper.Allocated_Area);
			FreightConfigurationRegistry.Instance.DefaultOverAllocationPercent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 50);
			AssertEquals("Allocated TEU", 13.5m, wrapper.Allocated_TEU);
			AssertEquals("Allocated Power Points", 4m, wrapper.Allocated_PowerPoints);
			AssertEquals("Allocated Tonnes", 75m, wrapper.Allocated_Tonnes);
			AssertEquals("Allocated Volume", 60m, wrapper.Allocated_Volume);
			AssertEquals("Allocated Area", 18m, wrapper.Allocated_Area);
		}

		public void TestAvailable()
		{
			SetupShipment(true, false, false, true);
			AssertEquals("Available TEU", 5m, wrapper.Available_TEU);
			AssertEquals("Available Power Points", 2m, wrapper.Available_PowerPoints);
			AssertEquals("Available Tonnes", 19.55m, wrapper.Available_Tonnes);
			AssertEquals("Available Volume", 16m, wrapper.Available_Volume);
			AssertEquals("Available Area", 4m, wrapper.Available_Area);
			NewBulkShipment(ExportSailing, principal, false, true, 10, 10);
			Factory.Save();
			AssertEquals("Available TEU should not have refreshed yet", 5m, wrapper.Available_TEU);
			AssertEquals("Available Power Points sholud not have refreshed", 2m, wrapper.Available_PowerPoints);
			AssertEquals("Available Tonnes should not have refreshed yet", 19.55m, wrapper.Available_Tonnes);
			AssertEquals("Available Volume should not have refreshed yet", 16m, wrapper.Available_Volume);
			AssertEquals("Available Area should not have refreshed yet", 4m, wrapper.Available_Area);
			wrapper.UpdateAllocatedValues();
			AssertEquals("Available TEU should have refreshed", 5m, wrapper.Available_TEU);
			AssertEquals("Available Power Points sholud have refreshed", 2m, wrapper.Available_PowerPoints);
			AssertEquals("Available Tonnes should have refreshed", 9.55m, wrapper.Available_Tonnes);
			AssertEquals("Available Volume should have refreshed", 6m, wrapper.Available_Volume);
			AssertEquals("Available Area should have refreshed", 4m, wrapper.Available_Area);
			FreightConfigurationRegistry.Instance.DefaultOverAllocationPercent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			wrapper.UpdateAllocatedValues();
			AssertEquals("Available TEU should take into acount overallocation", 14m, wrapper.Available_TEU);
			AssertEquals("Available Power Points sholud take into acount overallocation", 5m, wrapper.Available_PowerPoints);
			AssertEquals("Available Tonnes should take into acount overallocation", 59.55m, wrapper.Available_Tonnes);
			AssertEquals("Available Volume should take into acount overallocation", 46m, wrapper.Available_Volume);
			AssertEquals("Available Area should take into acount overallocation", 16m, wrapper.Available_Area);
		}

		public void TestRequired()
		{
			SetupShipment(true, false, false, true);
			AssertEquals("Required TEU", 0m, wrapper.Required_TEU);
			AssertEquals("Required Power Points", 0m, wrapper.Required_PowerPoints);
			AssertEquals("Required Tonnes", 1.3m, wrapper.Required_Tonnes);
			AssertEquals("Required Volume", 8m, wrapper.Required_Volume);
			AssertEquals("Required Area", 16m, wrapper.Required_Area);
			shipment.JS_UnitOfWeight = Constants.Weight.Tonnes;
			shipment.JS_ActualWeight = 50;
			AssertEquals("Required Tonnes Updates", 50m, wrapper.Required_Tonnes);
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_ActualVolume = 10;
			AssertEquals("Required Volume Updates", 10m, wrapper.Required_Volume);
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			container.JC_RC = RC_20GP_PK;
			container.JC_ContainerCount = 2;
			container.JC_GrossWeight = 2000;
			AssertEquals("Required TEU Updates", 2m, wrapper.Required_TEU);
			AssertEquals("Required Tonnes Updates", 2m, wrapper.Required_Tonnes);
			AssertEquals("Required Power Points Updates", 0m, wrapper.Required_PowerPoints);
			container.JC_RC = RC_40RE_PK;
			AssertEquals("Required TEU Updates", 4m, wrapper.Required_TEU);
			AssertEquals("Required Tonnes Updates", 6.14m, wrapper.Required_Tonnes);
			AssertEquals("Required Power Points Updates", 2m, wrapper.Required_PowerPoints);
			foreach (string mode in new string[] { Constants.ContainerModes.BreakBulk, Constants.ContainerModes.RollOnRollOff })
			{
				shipment.JS_PackingMode = mode;
				shipment.ShippingContainers.RemoveAndDeleteAll();
				var topLevelPack = shipment.ShippingContainers.AddNew();
				topLevelPack.JC_ContainerCount = 2;
				topLevelPack.JC_TotalUnitOfMeasure = Constants.Length.Metres;
				topLevelPack.JC_TotalLength = 4;
				topLevelPack.JC_TotalWidth = 3;
				AssertEquals("Required Area Updates", 24m, wrapper.Required_Area);
				topLevelPack.JC_ContainerCount = 3;
				AssertEquals("Required Area Updates", 36m, wrapper.Required_Area);
			}
		}

		public void TestValueChangedEvents_Booked()
		{
			SetupShipment(false, true, false, true);
			HookHitCounts();
			AgencyShipmentContainer container = shipment.BookedContainers[0];
			AssertAndClearHitCounts("Nothing has happened yet", 0, 0);
			container.JC_RC = RC_20RE_PK;
			AssertAndClearHitCounts("Container type changed", 0, 1);
			container.JC_ContainerCount = 4;
			AssertAndClearHitCounts("Container Count changed", 0, 1);
			container.JC_GrossWeight = 50000;
			AssertAndClearHitCounts("container weight changed", 0, 1);
			container.Delete();
			AssertAndClearHitCounts("container deleted", 0, 1);
			container = shipment.BookedContainers.AddNew();
			AssertAndClearHitCounts("container added", 0, 1);
			container.JC_RC = RC_20GP_PK;
			AssertAndClearHitCounts("container type changed", 0, 1);
			AgencyShipmentPackLine packline = shipment.OuterPackLines.AddNew();
			AssertAndClearHitCounts("packline added", 0, 1);
			packline.JL_PackageCount = 4;
			AssertAndClearHitCounts("package count changed", 0, 1);
			packline.JL_UnitOfDimension = Constants.Length.Yards;
			AssertAndClearHitCounts("package dimention unit changed", 0, 1);
			packline.JL_Length = 2;
			AssertAndClearHitCounts("package length changed", 0, 1);
			packline.JL_Width = 2;
			AssertAndClearHitCounts("package width changed", 0, 1);
			shipment.JS_ActualWeight = 5000;
			AssertAndClearHitCounts("shipment weight changed", 0, 1);
			shipment.JS_ActualVolume = 5000;
			AssertAndClearHitCounts("shipment volume changed", 0, 1);
			shipment.JS_UnitOfVolume = Core.Constants.Volume.MegaLitre;
			AssertAndClearHitCounts("unit of volume changed", 0, 1);
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Tonnes;
			AssertAndClearHitCounts("unit of weight changed", 0, 1);
			shipment.JS_JX = ExportSailing1.PK;
			AssertAndClearHitCounts("sailing changed", 1, 0);
		}

		public void TestValueChangedEvents_Confirmed()
		{
			SetupShipment(true, true, false, true);
			HookHitCounts();
			AgencyShipmentContainer container = shipment.RealContainers[0];
			AssertAndClearHitCounts("Nothing has happened yet", 0, 0);
			container.JC_RC = RC_20RE_PK;
			AssertAndClearHitCounts("Container type changed", 0, 1);
			container.JC_ContainerCount = 4;
			AssertAndClearHitCounts("Container Count changed", 0, 1);
			container.JC_GrossWeight = 50000;
			AssertAndClearHitCounts("container weight changed", 0, 1);
			container.Delete();
			AssertAndClearHitCounts("container deleted", 0, 1);
			container = shipment.RealContainers.AddNew();
			AssertAndClearHitCounts("container added", 0, 1);
			container.JC_RC = RC_20GP_PK;
			AssertAndClearHitCounts("container type changed", 0, 1);
			AgencyShipmentPackLine packline = shipment.OuterPackLines.AddNew();
			AssertAndClearHitCounts("packline added", 0, 1);
			packline.JL_PackageCount = 4;
			AssertAndClearHitCounts("package count changed", 0, 1);
			packline.JL_UnitOfDimension = Constants.Length.Yards;
			AssertAndClearHitCounts("package dimention unit changed", 0, 1);
			packline.JL_Length = 2;
			AssertAndClearHitCounts("package length changed", 0, 1);
			packline.JL_Width = 2;
			AssertAndClearHitCounts("package width changed", 0, 1);
			shipment.JS_ActualWeight = 5000;
			AssertAndClearHitCounts("shipment weight changed", 0, 1);
			shipment.JS_ActualVolume = 5000;
			AssertAndClearHitCounts("shipment volume changed", 0, 1);
			shipment.JS_UnitOfVolume = Core.Constants.Volume.MegaLitre;
			AssertAndClearHitCounts("unit of volume changed", 0, 1);
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Tonnes;
			AssertAndClearHitCounts("unit of weight changed", 0, 1);
			shipment.JS_JX = ExportSailing1.PK;
			AssertAndClearHitCounts("sailing changed", 1, 0);
		}

		[ExpectNoExceptions]
		public void TestDeletePrincipalOnSailingDoesNotCauseExceptionOnShipment()
		{
			var principal = NewPrincipal();
			Factory.Save();
			var sailing = ExportSailing;
			var originCountry = sailing.Origin.VoyageCountry;
			originCountry.J0_AllocationMethod = AllocationMethodList.Codes.Country;
			originCountry.J0_AllocationsByPrincipal = true;
			var allocation = originCountry.SlotAllocations.GetAllocation(principal.PK);
			allocation.SetAspect(AllocationAspectTypes.TEU, 29);
			allocation.SetAspect(AllocationAspectTypes.PowerPoints, 33);
			allocation.SetAspect(AllocationAspectTypes.Tonnes, 40);
			allocation.SetAspect(AllocationAspectTypes.Volume, 38);
			allocation.SetAspect(AllocationAspectTypes.Area, 22);
			var shipment = NewFCLShipment(sailing, principal, false, true, 2, 1);
			shipment.RunPreSaveValidation();
			originCountry.SlotAllocations.RemoveAllocation(principal.PK);
			shipment.RunPreSaveValidation();
		}

		[UseGlobalAllocations(false)]
		public void TestValidation_Import_Local()
		{
			GenericValidationTest(true, false);
		}

		[UseGlobalAllocations(true)]
		public void TestValidation_Import_Global()
		{
			GenericValidationTest(true, true);
		}

		[UseGlobalAllocations(false)]
		public void TestValidation_Export()
		{
			GenericValidationTest(false, false);
		}

		#region Implementation
		void GenericValidationTest(bool import, bool global)
		{
			principal = NewPrincipal();
			Factory.Save();
			sailing = import ? ImportSailing : ExportSailing;
			AgencyShipment shipment = NewShipment(Factory, sailing, principal, false, true);
			shipment.JS_PackingMode = Constants.ContainerModes.Bulk;
			shipment.JS_UnitOfWeight = Constants.Weight.Tonnes;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_ActualVolume = 0m;
			shipment.JS_ActualWeight = 0m;
			NewFCLShipment(sailing, principal, false, true, 2, 1); // 4 teu, 9.8 tonnes, 1 power point
			NewBulkShipment(sailing, principal, false, true, 20, 20); // 20 tonnes, 20 cubic meters
			NewRORShipment(sailing, principal, false, true, 1); // 0.65 tonnes, 4 cubic meters, 8 square meters
																// 4 teu, 1 power point, 30.45 tonnes, 24 cubic meters, 8 square meters.
			wrapper = new AllocationCalcWrapper(shipment);
			wrapper.UpdateAllocatedValues();
			CheckValidiationWhenRequirementIsEmpty(import, global, AllocationAspectTypes.TEU, "TEU");
			CheckValidiationWhenRequirementIsEmpty(import, global, AllocationAspectTypes.PowerPoints, "Power Points");
			CheckValidiationWhenRequirementIsEmpty(import, global, AllocationAspectTypes.Tonnes, "Tonnes");
			CheckValidiationWhenRequirementIsEmpty(import, global, AllocationAspectTypes.Volume, "Volume");
			CheckValidiationWhenRequirementIsEmpty(import, global, AllocationAspectTypes.Area, "Area");
			Factory.Save();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			for (int i = 7; i > 0; i--)
			{
				AgencyShipmentContainer container = shipment.RealContainers.AddNew();
				container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
				container.JC_GrossWeightUQ = Constants.Weight.Kilograms;
				container.JC_GrossWeight = 10000m;
			}

			CheckValidationWhenRequirementIsNonEmpty(import, global, AllocationAspectTypes.TEU, 11m, "TEU");
			CheckValidationWhenRequirementIsNonEmpty(import, global, AllocationAspectTypes.PowerPoints, 8m, "Power Points");
			CheckValidationWhenRequirementIsNonEmpty(import, global, AllocationAspectTypes.Tonnes, 100.45m, "Tonnes");
			shipment.JS_PackingMode = Constants.ContainerModes.BreakBulk;
			var topLevelPack = shipment.TopLevelPacks.AddNew();
			topLevelPack.JC_GrossVolumeUQ = Constants.Volume.CubicMetres;
			topLevelPack.JC_GrossVolumeUQ = Constants.Weight.Kilograms;
			topLevelPack.JC_TotalUnitOfMeasure = Constants.Length.Metres;
			topLevelPack.JC_TotalLength = 5;
			topLevelPack.JC_TotalWidth = 4;
			topLevelPack.JC_TotalHeight = 3;
			shipment.JS_ActualVolume = 60;
			shipment.JS_ActualWeight = 15000;
			CheckValidationWhenRequirementIsNonEmpty(import, global, AllocationAspectTypes.Volume, 84m, "Volume");
			CheckValidationWhenRequirementIsNonEmpty(import, global, AllocationAspectTypes.Area, 28m, "Area");
		}

		void CheckValidiationWhenRequirementIsEmpty(bool import, bool global, string code, string name)
		{
			ZPropertyInfo info = GetRequiredPropertyInfo(code);
			VoyageCountry originCountry = sailing.Origin.VoyageCountry;
			originCountry.J0_AllocationMethod = AllocationMethodList.Codes.Country;
			SlotAllocation allocation = originCountry.SlotAllocations.GetAllocation(ZGuid.Empty);
			allocation.SetAspect(code, 0m);
			wrapper.UpdateAllocatedValues();
			wrapper.RunPreSaveValidation();
			AssertNoNotifications(info);
		}

		void CheckValidationWhenRequirementIsNonEmpty(bool import, bool global, string code, decimal totalRequired, string name)
		{
			ZPropertyInfo info = GetRequiredPropertyInfo(code);
			VoyageCountry originCountry = sailing.Origin.VoyageCountry;
			originCountry.J0_AllocationMethod = AllocationMethodList.Codes.Ignore;
			SlotAllocation allocation = originCountry.SlotAllocations.GetAllocation(ZGuid.Empty);
			allocation.E0_OverAllocationPercent = 0;
			allocation.SetAspect(code, 0m);
			wrapper.UpdateAllocatedValues();
			wrapper.RunPreSaveValidation();
			AssertNoNotifications(name, info);
			originCountry.J0_AllocationMethod = AllocationMethodList.Codes.Country;
			wrapper.UpdateAllocatedValues();
			wrapper.RunPreSaveValidation();
			if (global || !import)
			{
				AssertHasError(name, info, string.Format("The {0} allocation is not specified.", name));
			}
			else
			{
				AssertNoNotifications(name, info);
			}

			allocation.SetAspect(code, totalRequired);
			wrapper.UpdateAllocatedValues();
			wrapper.RunPreSaveValidation();
			AssertNoNotifications(name, info);
			allocation.SetAspect(code, totalRequired - 1);
			wrapper.UpdateAllocatedValues();
			wrapper.RunPreSaveValidation();
			if (global || !import)
			{
				AssertHasWarning(name, info, string.Format("The {0} allocation is insufficient.", name));
			}
			else
			{
				AssertNoNotifications(name, info);
			}

			ZDecimal allocated = Math.Floor(totalRequired / 4) - 1;
			ZInt percent = (ZInt)Math.Ceiling((100m * totalRequired / allocated) - 100);
			allocation.SetAspect(code, allocated);
			wrapper.UpdateAllocatedValues();
			wrapper.RunPreSaveValidation();
			if (global || !import)
			{
				AssertHasError(name, info, string.Format("This {0} allocation would require an over allocation percentage of {1}% which is really excessive. Please ensure the values entered for both the allocation and all the jobs are reasonable.", name, percent));
			}
			else
			{
				AssertNoNotifications(name, info);
			}

			allocated = Math.Ceiling(totalRequired / 4);
			allocation.SetAspect(code, allocated);
			wrapper.UpdateAllocatedValues();
			wrapper.RunPreSaveValidation();
			if (global || !import)
			{
				AssertHasWarning(name, info, string.Format("The {0} allocation is insufficient.", name));
			}
			else
			{
				AssertNoNotifications(name, info);
			}

			allocation.SetAspect(code, allocated + 1);
			wrapper.UpdateAllocatedValues();
			wrapper.RunPreSaveValidation();
			if (global || !import)
			{
				AssertHasWarning(name, info, string.Format("The {0} allocation is insufficient.", name));
			}
			else
			{
				AssertNoNotifications(name, info);
			}
		}

		ZPropertyInfo GetRequiredPropertyInfo(string code)
		{
			switch (code)
			{
				case AllocationAspectTypes.TEU:
					return wrapper.Required_TEUInfo;
				case AllocationAspectTypes.PowerPoints:
					return wrapper.Required_PowerPointsInfo;
				case AllocationAspectTypes.Tonnes:
					return wrapper.Required_TonnesInfo;
				case AllocationAspectTypes.Volume:
					return wrapper.Required_VolumeInfo;
				case AllocationAspectTypes.Area:
					return wrapper.Required_AreaInfo;
				default:
					throw new ArgumentOutOfRangeException(code);
			}
		}

		void SetupShipment(bool confirmed, bool fcl, bool import, bool populateAllocation)
		{
			principal = NewPrincipal();
			Factory.Save();
			sailing = import ? ImportSailing : ExportSailing;
			VoyageCountry originCountry = sailing.Origin.VoyageCountry;
			originCountry.J0_AllocationMethod = AllocationMethodList.Codes.Sailing;
			SlotAllocation allocation = sailing.SlotAllocations.GetAllocation(ZGuid.Empty);
			if (populateAllocation)
			{
				allocation.SetAspect(AllocationAspectTypes.TEU, 9);
				allocation.SetAspect(AllocationAspectTypes.PowerPoints, 3);
				allocation.SetAspect(AllocationAspectTypes.Tonnes, 50);
				allocation.SetAspect(AllocationAspectTypes.Volume, 40);
				allocation.SetAspect(AllocationAspectTypes.Area, 12);
				NewFCLShipment(sailing, principal, false, true, 2, 1); // 4 teu, 9.8 tonnes, 1 power point
				NewBulkShipment(sailing, principal, false, true, 20, 20); // 20 tonnes, 20 cubic meters
				NewRORShipment(sailing, principal, false, true, 1); // 0.65 tonnes, 4 cubic meters, 8 square meters
																	// 4 teu, 30.45 tonnes, 1 power point, 24 cubic meters, 8 square meters
			}

			Factory.Save();
			if (fcl)
			{
				// 3 teu, 7.4 tonne, 1 power point
				shipment = NewFCLShipment(sailing, principal, false, confirmed, 1, 1);
			}
			else
			{
				// 1.3 tonnes, 8 cubic meters, 16 square meters
				shipment = NewRORShipment(sailing, principal, false, confirmed, 2);
			}

			wrapper = new AllocationCalcWrapper(shipment);
		}

		void AssertAndClearHitCounts(string messagePrefix, int expectedAllocatedHits, int expectedRequiredHits)
		{
			CombineAssertions(delegate
			{
				foreach (ZPropertyInfo info in GetAllocatedPropertyInfos())
				{
					AssertEquals(info.Name, expectedAllocatedHits, GetHitCount(info.Name));
				}

				foreach (ZPropertyInfo info in GetRequiredPropertyInfos())
				{
					AssertEquals(info.Name, expectedRequiredHits, GetHitCount(info.Name));
				}
			});
			hitCounts = null;
		}

		void HookHitCounts()
		{
			foreach (ZPropertyInfo info in GetAllocatedPropertyInfos())
			{
				string name = info.Name;
				info.ValueChanged += delegate
				{
					IncrementHitCount(name);
				};
			}

			foreach (ZPropertyInfo info in GetRequiredPropertyInfos())
			{
				string name = info.Name;
				info.ValueChanged += delegate
				{
					IncrementHitCount(name);
				};
			}
		}

		ZPropertyInfo[] GetAllocatedPropertyInfos()
		{
			return new ZPropertyInfo[] { wrapper.AllocationMethodInfo, wrapper.Allocated_TEUInfo, wrapper.Allocated_PowerPointsInfo, wrapper.Allocated_TonnesInfo, wrapper.Allocated_VolumeInfo, wrapper.Allocated_AreaInfo, wrapper.Available_TEUInfo, wrapper.Available_PowerPointsInfo, wrapper.Available_TonnesInfo, wrapper.Available_VolumeInfo, wrapper.Available_AreaInfo, };
		}

		ZPropertyInfo[] GetRequiredPropertyInfos()
		{
			return new ZPropertyInfo[] { wrapper.Required_TEUInfo, wrapper.Required_PowerPointsInfo, wrapper.Required_TonnesInfo, wrapper.Required_VolumeInfo, wrapper.Required_AreaInfo, };
		}

		int GetHitCount(string label)
		{
			int result;
			if (hitCounts == null || !hitCounts.TryGetValue(label, out result))
			{
				result = 0;
			}

			return result;
		}

		void IncrementHitCount(string label)
		{
			int existing;
			if (hitCounts == null)
			{
				hitCounts = new Dictionary<string, int>();
				hitCounts[label] = 1;
			}
			else if (hitCounts.TryGetValue(label, out existing))
			{
				hitCounts[label] = existing + 1;
			}
			else
			{
				hitCounts[label] = 1;
			}
		}

		Dictionary<string, int> hitCounts;
		AgencyShipment shipment;
		JobSailing sailing;
		AllocationCalcWrapper wrapper;
		OrgHeader principal;
		#endregion
	}
}
