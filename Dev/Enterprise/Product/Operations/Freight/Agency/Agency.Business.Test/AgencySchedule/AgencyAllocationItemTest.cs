using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal abstract class AgencyAllocationItemTest<T> : BaseAgencyTest where T : BusinessObject
	{
		public void TestProxyTonnes()
		{
			SlotAllocation allocation = SlotCollection.GetAllocation(ZGuid.Empty);
			allocation.SetAspect(AllocationAspectTypes.Tonnes, 249);
			AssertEquals(249m, Wrapper.Tonnes);
			allocation.SetAspect(AllocationAspectTypes.Tonnes, 321);
			AssertEquals(321m, Wrapper.Tonnes);
		}

		public void TestProxyVolume()
		{
			SlotAllocation allocation = SlotCollection.GetAllocation(ZGuid.Empty);
			allocation.SetAspect(AllocationAspectTypes.Volume, 249);
			AssertEquals(249m, Wrapper.Volume);
			allocation.SetAspect(AllocationAspectTypes.Volume, 321);
			AssertEquals(321m, Wrapper.Volume);
		}

		public void TestProxyArea()
		{
			SlotAllocation allocation = SlotCollection.GetAllocation(ZGuid.Empty);
			allocation.SetAspect(AllocationAspectTypes.Area, 249);
			AssertEquals(249m, Wrapper.Area);
			allocation.SetAspect(AllocationAspectTypes.Area, 321);
			AssertEquals(321m, Wrapper.Area);
		}

		public void TestProxyPowerPoints()
		{
			SlotAllocation allocation = SlotCollection.GetAllocation(ZGuid.Empty);
			allocation.SetAspect(AllocationAspectTypes.PowerPoints, 249);
			AssertEquals(249m, Wrapper.PowerPoints);
			allocation.SetAspect(AllocationAspectTypes.PowerPoints, 321);
			AssertEquals(321m, Wrapper.PowerPoints);
		}

		public void TestProxyTEU()
		{
			SlotAllocation allocation = SlotCollection.GetAllocation(ZGuid.Empty);
			allocation.SetAspect(AllocationAspectTypes.TEU, 249);
			AssertEquals(249m, Wrapper.TEU);
			allocation.SetAspect(AllocationAspectTypes.TEU, 321);
			AssertEquals(321m, Wrapper.TEU);
		}

		public void TestProxyUsedTonnes()
		{
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage(14, 23, 15, 32, 41, 51));
			AssertEquals(32m, Wrapper.UsedTonnes);
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage(41, 32, 16, 23, 14, 52));
			AssertEquals(23m, Wrapper.UsedTonnes);
		}

		public void TestProxyUsedVolume()
		{
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage(14, 23, 15, 32, 41, 51));
			AssertEquals(41m, Wrapper.UsedVolume);
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage(41, 32, 16, 23, 14, 52));
			AssertEquals(14m, Wrapper.UsedVolume);
		}

		public void TestProxyUsedArea()
		{
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage(14, 23, 15, 32, 41, 51));
			AssertEquals(51m, Wrapper.UsedArea);
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage(41, 32, 16, 23, 14, 52));
			AssertEquals(52m, Wrapper.UsedArea);
		}

		public void TestUsedProxyPowerPoints()
		{
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage(14, 23, 15, 32, 41, 51));
			AssertEquals(15, Wrapper.UsedPowerPoints);
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage(41, 32, 16, 23, 14, 52));
			AssertEquals(16, Wrapper.UsedPowerPoints);
		}

		public void TestProxyUsedTEU()
		{
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage(14, 23, 15, 32, 41, 51));
			AssertEquals(37m, Wrapper.UsedTEU);
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage(41, 32, 16, 23, 14, 52));
			AssertEquals(73m, Wrapper.UsedTEU);
		}

		public void TestProxyUsedGP_TEU()
		{
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage(14, 23, 15, 32, 41, 51));
			AssertEquals(14m, Wrapper.UsedGP_TEU);
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage(41, 32, 16, 23, 14, 52));
			AssertEquals(41m, Wrapper.UsedGP_TEU);
		}

		public void TestProxyUsedReefer_TEU()
		{
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage(14, 23, 15, 32, 41, 51));
			AssertEquals(23m, Wrapper.UsedReefer_TEU);
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage(41, 32, 16, 23, 14, 52));
			AssertEquals(32m, Wrapper.UsedReefer_TEU);
		}

		public void TestProxyOverridenTonnes()
		{
			SlotAllocation allocation = SlotCollection.GetAllocation(ZGuid.Empty);
			allocation.SetAspect(AllocationAspectTypes.Tonnes, 200);
			allocation.E0_UseDefaultOverAllocation = false;
			allocation.E0_OverAllocationPercent = 20;
			AssertEquals(240m, Wrapper.OverallocatedTonnes);
			allocation.SetAspect(AllocationAspectTypes.Tonnes, 300);
			AssertEquals(360m, Wrapper.OverallocatedTonnes);
		}

		public void TestProxyOverridenVolume()
		{
			SlotAllocation allocation = SlotCollection.GetAllocation(ZGuid.Empty);
			allocation.SetAspect(AllocationAspectTypes.Volume, 200);
			allocation.E0_UseDefaultOverAllocation = false;
			allocation.E0_OverAllocationPercent = 20;
			AssertEquals(240m, Wrapper.OverallocatedVolume);
			allocation.SetAspect(AllocationAspectTypes.Volume, 300);
			AssertEquals(360m, Wrapper.OverallocatedVolume);
		}

		public void TestProxyOverridenArea()
		{
			SlotAllocation allocation = SlotCollection.GetAllocation(ZGuid.Empty);
			allocation.SetAspect(AllocationAspectTypes.Area, 200);
			allocation.E0_UseDefaultOverAllocation = false;
			allocation.E0_OverAllocationPercent = 20;
			AssertEquals(240m, Wrapper.OverallocatedArea);
			allocation.SetAspect(AllocationAspectTypes.Area, 300);
			AssertEquals(360m, Wrapper.OverallocatedArea);
		}

		public void TestProxyOverridenTEU()
		{
			SlotAllocation allocation = SlotCollection.GetAllocation(ZGuid.Empty);
			allocation.SetAspect(AllocationAspectTypes.TEU, 200);
			allocation.E0_UseDefaultOverAllocation = false;
			allocation.E0_OverAllocationPercent = 20;
			AssertEquals(240m, Wrapper.OverallocatedTEU);
			allocation.SetAspect(AllocationAspectTypes.TEU, 300);
			AssertEquals(360m, Wrapper.OverallocatedTEU);
		}

		public void TestProxyOverrideOverallocationPercent()
		{
			SlotAllocation allocation = SlotCollection.GetAllocation(ZGuid.Empty);
			allocation.E0_UseDefaultOverAllocation = true;
			AssertEquals(false, Wrapper.OverrideOverallocationPercent);
			allocation.E0_UseDefaultOverAllocation = false;
			AssertEquals(true, Wrapper.OverrideOverallocationPercent);
			Wrapper.OverrideOverallocationPercent = false;
			AssertEquals(true, allocation.E0_UseDefaultOverAllocation);
		}

		public void TestProxyLoadedCargoWeight()
		{
			SlotAllocation allocation = SlotCollection.GetAllocation(ZGuid.Empty);
			allocation.E0_LoadedCargoWeight = 200;
			AssertEquals(allocation.E0_LoadedCargoWeight, Wrapper.LoadedCargoWeight);
			allocation.E0_LoadedCargoWeight = 300;
			AssertEquals(allocation.E0_LoadedCargoWeight, Wrapper.LoadedCargoWeight);
		}

		public void TestValidateTEU()
		{
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage());
			Wrapper.TEU = -1;
			AssertHasError(Wrapper.TEUInfo, "You can't have a negative allocation.");
			Wrapper.TEU = 0;
			Wrapper.PowerPoints = 0;
			AssertNoNotifications(Wrapper.TEUInfo);
			Wrapper.PowerPoints = 20;
			AssertHasError(Wrapper.TEUInfo, "If you specify power points then you also need to specify TEU.");
			Wrapper.TEU = 999999;
			AssertNoNotifications(Wrapper.TEUInfo);
			Wrapper.TEU = 1000000;
			AssertHasError(Wrapper.TEUInfo, "The number 1,000,000 is too large, the maximum value allowed for Allocated TEUs is 999,999.");
		}

		public void TestValidatePowerPoints()
		{
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage());
			Wrapper.PowerPoints = -1;
			AssertHasError(Wrapper.PowerPointsInfo, "You can't have a negative allocation.");
			Wrapper.TEU = 0;
			Wrapper.PowerPoints = 0;
			AssertNoNotifications(Wrapper.PowerPointsInfo);
			Wrapper.PowerPoints = 999999;
			AssertHasError(Wrapper.PowerPointsInfo, "If you specify power points then you also need to specify TEU.");
			Wrapper.TEU = 50;
			AssertNoNotifications(Wrapper.PowerPointsInfo);
			Wrapper.PowerPoints = 1000000;
			AssertHasError(Wrapper.PowerPointsInfo, "The number 1,000,000 is too large, the maximum value allowed for Allocated Power Points is 999,999.");
		}

		public void TestValidateTonnes()
		{
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage());
			Wrapper.Tonnes = -1;
			AssertHasError(Wrapper.TonnesInfo, "You can't have a negative allocation.");
			Wrapper.Tonnes = 0;
			AssertNoNotifications(Wrapper.TonnesInfo);
			Wrapper.TEU = 10;
			AssertHasWarning(Wrapper.TonnesInfo, "Enter your weight limit to prevent over booking.");
			Wrapper.Tonnes = 10;
			AssertNoNotifications(Wrapper.TonnesInfo);
			Wrapper.Tonnes = 0;
			Wrapper.TEU = 0;
			Wrapper.Volume = 10;
			AssertHasWarning(Wrapper.TonnesInfo, "Enter your weight limit to prevent over booking.");
			Wrapper.Tonnes = 999999;
			AssertNoNotifications(Wrapper.TonnesInfo);
			Wrapper.Tonnes = 1000000;
			AssertHasError(Wrapper.TonnesInfo, "The number 1,000,000 is too large, the maximum value allowed for Allocated Tonnes is 999,999.");
		}

		public void TestValidateVolume()
		{
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage());
			Wrapper.Volume = -1;
			AssertHasError(Wrapper.VolumeInfo, "You can't have a negative allocation.");
			Wrapper.Volume = 0;
			AssertNoNotifications(Wrapper.VolumeInfo);
			Wrapper.Volume = 999999;
			AssertNoNotifications(Wrapper.VolumeInfo);
			Wrapper.Volume = 1000000;
			AssertHasError(Wrapper.VolumeInfo, "The number 1,000,000 is too large, the maximum value allowed for Allocated Volume is 999,999.");
		}

		public void TestValidateArea()
		{
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage());
			Wrapper.Area = -1;
			AssertHasError(Wrapper.AreaInfo, "You can't have a negative allocation.");
			Wrapper.Area = 0;
			AssertNoNotifications(Wrapper.AreaInfo);
			Wrapper.Area = 999999;
			AssertNoNotifications(Wrapper.AreaInfo);
			Wrapper.Area = 1000000;
			AssertHasError(Wrapper.AreaInfo, "The number 1,000,000 is too large, the maximum value allowed for Allocated Area is 999,999.");
		}

		public void TestValidateTEU_AgainstUsage()
		{
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage(10, 10, 10, 20, 20, 15));
			Wrapper.ValidateTEU();
			AssertNoNotifications("No allocations", Wrapper.TEUInfo);
			Country.VoyageCountry.J0_AllocationMethod = AllocationMethod;
			Wrapper.ValidateTEU();
			AssertHasErrors("Shipments will not fit", Wrapper.TEUInfo);
			Wrapper.TEU = 20;
			AssertNoNotifications("Shipment will now fit", Wrapper.TEUInfo);
			Wrapper.TEU = 19;
			AssertHasErrors("Shipments will not fit again", Wrapper.TEUInfo);
			Wrapper.OverrideOverallocationPercent = true;
			Wrapper.OverallocationPercent = 50m;
			Wrapper.ValidateTEU();
			AssertHasWarnings("Shipments within overallocation", Wrapper.TEUInfo);
			Country.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.NotSet;
			Wrapper.ValidateTEU();
			AssertNoNotifications("allocations are no longer at this level, allocations on this level are now irrelivant", Wrapper.TEUInfo);
		}

		public void TestValidateTonnes_AgainstUsage()
		{
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage(10, 10, 10, 20, 20, 15));
			Wrapper.ValidateTonnes();
			AssertNoNotifications("No allocations", Wrapper.TonnesInfo);
			Country.VoyageCountry.J0_AllocationMethod = AllocationMethod;
			Wrapper.ValidateTonnes();
			AssertHasErrors("Shipments will not fit", Wrapper.TonnesInfo);
			Wrapper.Tonnes = 20;
			AssertNoNotifications("Shipment will now fit", Wrapper.TonnesInfo);
			Wrapper.Tonnes = 19;
			AssertHasErrors("Shipments will not fit again", Wrapper.TonnesInfo);
			Wrapper.OverrideOverallocationPercent = true;
			Wrapper.OverallocationPercent = 50;
			Wrapper.ValidateTonnes();
			AssertHasWarnings("Shipment within overallocation", Wrapper.TonnesInfo);
			Country.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.NotSet;
			Wrapper.ValidateTonnes();
			AssertNoNotifications("allocations are no longer at this level, allocations on this level are now irrelivant", Wrapper.TonnesInfo);
		}

		public void TestValidateVolume_AgainstUsage()
		{
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage(10, 10, 10, 20, 20, 15));
			Wrapper.ValidateVolume();
			AssertNoNotifications("No allocations", Wrapper.VolumeInfo);
			Country.VoyageCountry.J0_AllocationMethod = AllocationMethod;
			Wrapper.ValidateVolume();
			AssertHasErrors("Shipments will not fit", Wrapper.VolumeInfo);
			Wrapper.Volume = 20;
			AssertNoNotifications("Shipment will now fit", Wrapper.VolumeInfo);
			Wrapper.Volume = 19;
			AssertHasErrors("Shipments will not fit again", Wrapper.VolumeInfo);
			Wrapper.OverrideOverallocationPercent = true;
			Wrapper.OverallocationPercent = 50m;
			Wrapper.ValidateVolume();
			AssertHasWarnings("Shipment within overallocation", Wrapper.VolumeInfo);
			Country.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.NotSet;
			Wrapper.ValidateVolume();
			AssertNoNotifications("allocations are no longer at this level, allocations on this level are now irrelivant", Wrapper.VolumeInfo);
		}

		public void TestValidateArea_AgainstUsage()
		{
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage(10, 10, 10, 20, 20, 15));
			Wrapper.ValidateArea();
			AssertNoNotifications("No allocations", Wrapper.AreaInfo);
			Country.VoyageCountry.J0_AllocationMethod = AllocationMethod;
			Wrapper.ValidateArea();
			AssertHasErrors("Shipments will not fit", Wrapper.AreaInfo);
			Wrapper.Area = 15;
			AssertNoNotifications("Shipment will now fit", Wrapper.AreaInfo);
			Wrapper.Area = 14;
			AssertHasErrors("Shipments will not fit again", Wrapper.AreaInfo);
			Wrapper.OverrideOverallocationPercent = true;
			Wrapper.OverallocationPercent = 50m;
			Wrapper.ValidateArea();
			AssertHasWarnings("Shipment within overallocation", Wrapper.AreaInfo);
			Country.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.NotSet;
			Wrapper.ValidateArea();
			AssertNoNotifications("allocations are no longer at this level, allocations on this level are now irrelivant", Wrapper.AreaInfo);
		}

		public void TestReadonlynessFromSecuritySettings_True()
		{
			ReadonlynessFromSecuritySettingsTest(true);
		}

		public void TestReadonlynessFromSecuritySettings_False()
		{
			ReadonlynessFromSecuritySettingsTest(false);
		}

		void ReadonlynessFromSecuritySettingsTest(bool allowed)
		{
			Env.Security.SailingScheduleAllocationEdit.IsAllowed = allowed;
			Wrapper.OverrideOverallocationPercent = true;
			AssertEquals("IsAllowed", allowed, Env.Security.SailingScheduleAllocationEdit.IsAllowed);
			AssertEquals("TEU", !allowed, Wrapper.TEUInfo.ReadOnly);
			AssertEquals("PowerPoints", !allowed, Wrapper.PowerPointsInfo.ReadOnly);
			AssertEquals("Tonnes", !allowed, Wrapper.TonnesInfo.ReadOnly);
			AssertEquals("Volume", !allowed, Wrapper.VolumeInfo.ReadOnly);
			AssertEquals("OverallocationPercent", !allowed, Wrapper.OverallocationPercentInfo.ReadOnly);
			AssertEquals("OverrideOverallocationPercent", !allowed, Wrapper.OverrideOverallocationPercentInfo.ReadOnly);
		}

		public void TestRunPreSaveValidation()
		{
			AssertNoNotifications("Precondition: Should not have any errors", Wrapper);
			using (Wrapper.GetValidationSuspender())
			{
				Wrapper.TEU = -1;
				Wrapper.Tonnes = -1;
				Wrapper.Volume = -1;
				Wrapper.PowerPoints = -1;
				Wrapper.Area = -1;
			}
			Wrapper.RunPreSaveValidation();
			AssertHasErrors(Wrapper.TEUInfo);
			AssertHasErrors(Wrapper.PowerPointsInfo);
			AssertHasErrors(Wrapper.VolumeInfo);
			AssertHasErrors(Wrapper.TonnesInfo);
			AssertHasErrors(Wrapper.AreaInfo);
		}

		public void TestRefreshUsage()
		{
			int valueChangedCount = 0;
			EventHandler handler = delegate
			{
				valueChangedCount++;
			};
			ZPropertyInfo[] infos = GetUsageInfos(Wrapper);
			foreach (ZPropertyInfo info in infos)
			{
				info.ValueChanged += handler;
			}

			AssertEquals("Events should not have been raised by adding event handlers", 0, valueChangedCount);
			Wrapper.RefreshUsageBindings();
			AssertEquals("Events should have been raised", infos.Length, valueChangedCount);
			valueChangedCount = 0;
			foreach (ZPropertyInfo info in infos)
			{
				info.ValueChanged -= handler;
			}

			AssertEquals("Events should not have been raised by removing event handlers", 0, valueChangedCount);
		}

		public void TestUsageValidationFormating()
		{
			Country.VoyageCountry.J0_AllocationMethod = AllocationMethod;
			Country.GenericPrincipal.UsageProvider = new DummyAllocationUsageProvider(new AllocationUsage(4.44444444444m, 4.44444444444m, 4, 4.44444444444m, 4.44444444444m, 4.44444444444m));
			Wrapper.TEU = 0;
			Wrapper.PowerPoints = 0;
			Wrapper.Tonnes = 0;
			Wrapper.Volume = 0;
			Wrapper.Area = 0;
			Wrapper.OverrideOverallocationPercent = true;
			Wrapper.OverallocationPercent = 0;
			Wrapper.RunPreSaveValidation();
			AssertHasError(Wrapper.TEUInfo, "8.89 TEU have already been booked.");
			AssertHasError(Wrapper.PowerPointsInfo, "4 power points have already been booked.");
			AssertHasError(Wrapper.TonnesInfo, "4.444 tonnes have already been booked.");
			AssertHasError(Wrapper.VolumeInfo, "4.444 M3 have already been booked.");
			AssertHasError(Wrapper.AreaInfo, "4.444 M2 have already been booked.");
		}

		public void TestBizObj()
		{
			AssertNotNull("Precondition", PrincipalSpecificWrapper.BizObj);
			AssertEquals("Precondition", false, PrincipalSpecificWrapper.BizObj.IsDeleted);
			PrincipalSpecificWrapper.BizObj.Delete();
			AssertNull(PrincipalSpecificWrapper.BizObj);
		}

		#region Implementation
		void ValueChangedCountIncrement(object sender, EventArgs e)
		{
		}

		ZPropertyInfo[] GetUsageInfos(AgencyAllocationItem<T> item)
		{
			return new ZPropertyInfo[] { item.UsedTonnesInfo, item.UsedVolumeInfo, item.UsedGP_TEUInfo, item.UsedReefer_TEUInfo, item.UsedTEUInfo, item.UsedPowerPointsInfo, item.UsedAreaInfo, };
		}

		protected AgencyAllocationItem<T> Wrapper
		{
			get
			{
				if (wrapper == null)
				{
					wrapper = WrapAllocationParent(Country.GenericPrincipal, AllocationParent);
				}

				return wrapper;
			}
		}

		AgencyAllocationItem<T> wrapper;
		protected AgencyAllocationItem<T> PrincipalSpecificWrapper
		{
			get
			{
				if (principalSpecificWrapper == null)
				{
					principalSpecificWrapper = WrapAllocationParent(PrincipalWrapper, AllocationParent);
				}

				return principalSpecificWrapper;
			}
		}

		AgencyAllocationItem<T> principalSpecificWrapper;
		protected BusinessObject AllocationParent
		{
			get
			{
				if (allocationParent == null)
				{
					allocationParent = GetAllocationParentFromVoyage(SingleExportSailing.Voyage);
				}

				return allocationParent;
			}
		}

		BusinessObject allocationParent;
		protected SlotAllocationDependentCollection SlotCollection
		{
			get
			{
				if (slotCollection == null)
				{
					slotCollection = GetSlotCollection(AllocationParent);
				}

				return slotCollection;
			}
		}

		SlotAllocationDependentCollection slotCollection;
		protected AgencyPrincipal PrincipalWrapper
		{
			get
			{
				if (principalWrapper == null)
				{
					Country.Principals.Add(Principal);
					principalWrapper = Country.WrappedPrincipals.GetWrapperForTesting(Principal);
				}

				return principalWrapper;
			}
		}

		AgencyPrincipal principalWrapper;
		protected AgencyCountry Country
		{
			get
			{
				if (country == null)
				{
					country = new AgencyCountry(SingleExportSailing.Voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
				}

				return country;
			}
		}

		AgencyCountry country;
		protected OrgHeader Principal
		{
			get
			{
				if (principal == null)
				{
					principal = NewPrincipal();
				}

				return principal;
			}
		}

		OrgHeader principal;
		protected JobSailing SingleExportSailing
		{
			get
			{
				if (singleExportSailing == null)
				{
					JobVoyage voyage = Factory.New<JobVoyage>();
					voyage.Origins.AddNew().JA_RL_NKPortOfLoading = HomePort;
					voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = OverseasPort;
					voyage.GenerateSailings();
					singleExportSailing = voyage.Sailings.GetSailingFromLoadAndDischarge(HomePort, OverseasPort);
				}

				return singleExportSailing;
			}
		}

		JobSailing singleExportSailing;
		protected abstract ZString AllocationMethod { get; }

		protected abstract BusinessObject GetAllocationParentFromVoyage(JobVoyage voyage);
		protected abstract AgencyAllocationItem<T> WrapAllocationParent(AgencyPrincipal principal, BusinessObject allocationParent);
		protected abstract SlotAllocationDependentCollection GetSlotCollection(BusinessObject allocationParent);
		#endregion
	}
}
