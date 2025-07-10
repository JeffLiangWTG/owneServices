using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal class AgencyAllocationPluginTest : BaseAgencyTest
	{
		public void TestDBHitsOnShowPlugin()
		{
			ZGuid voyagePK;
			{
				string[] ports = { "NZAKL", "NZWEL", "AUSYD", "AUBNE", "AUCNS", "SGSIN", "MYBAG", "GBLON", };
				BusinessObjectFactory createFactory = new BusinessObjectFactory();
				JobVoyage voyage = createFactory.New<JobVoyage>();
				voyagePK = voyage.PK;
				for (int i = 1; i < ports.Length; i++)
				{
					VoyageOrigin origin = voyage.Origins.AddNew();
					origin.JA_RL_NKPortOfLoading = ports[i - 1];
					origin.JA_E_DEP = ZDateTime.Now.AddDays((i << 2) + 1);
					VoyageDestination destination = voyage.Destinations.AddNew();
					destination.JB_RL_NKPortOfDischarge = ports[i];
					destination.JB_E_ARV = ZDateTime.Now.AddDays((i << 2) + 3);
					origin.SlotAllocations.GetAllocation(ZGuid.Empty);
				}

				voyage.Countries.GetCountry("NZ", true);
				voyage.Countries.GetCountry("AU", true);
				voyage.Countries.GetCountry("SG", true);
				voyage.Countries.GetCountry("MY", true);
				voyage.GenerateSailings();
				foreach (JobSailing sailing in voyage.Sailings)
				{
					sailing.SlotAllocations.GetAllocation(ZGuid.Empty);
				}

				foreach (VoyageCountry country in voyage.Countries)
				{
					country.SlotAllocations.GetAllocation(ZGuid.Empty);
				}

				createFactory.Save();
			}

			{
				JobVoyage voyage = Factory.Load<JobVoyage>(voyagePK);
				using (ZJobVoyageForm form = new ZJobVoyageForm(voyage))
				{
					form.Show();
					Application.DoEvents();
					Factory.ResetDatabaseLoadCount();
					form.PlugIns.SelectPlugInTabPage(ControllerIDs.AgencyAllocation);
					// JobSlotAllocation: 1
					// JobVoyCountry: 1
					AssertMaxDbHits(2, Factory);
				}
			}
		}

		[UseGlobalAllocations(true)]
		public void TestRefresh_Global()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAKL";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "";
			using (AgencyAllocationPlugin plugin = new AgencyAllocationPlugin(voyage))
			{
				AgencyCountryCollection collection = GetCountryCollection(plugin);
				AssertLoadedCountries("not loaded", collection);
				plugin.OnUserControlShown();
				AssertLoadedCountries("loaded", collection, "AU", "NZ");
			}
		}

		[UseGlobalAllocations(false)]
		public void TestRefresh_Local()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAKL";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "";
			using (AgencyAllocationPlugin plugin = new AgencyAllocationPlugin(voyage))
			{
				AgencyCountryCollection collection = GetCountryCollection(plugin);
				AssertLoadedCountries("not loaded", collection);
				plugin.OnUserControlShown();
				AssertLoadedCountries("loaded", collection, "AU");
			}
		}

		public void TestSavePath_NoProblemsExpected()
		{
			JobVoyage voyage = GetVoyageWithoutNotifications();
			AssertEquals("Precondition: the voyage should not yet be in the database", false, voyage.IsInDatabase);
			using (ZJobVoyageForm form = new ZJobVoyageForm(voyage))
			{
				form.Show();
				Application.DoEvents();
				form.PlugIns.SelectPlugInTabPage(ControllerIDs.AgencyAllocation);
				AgencyCountryCollection collection = GetCountryCollection(form);
				AssertEquals("Should not be holding the mutex before clicking save", false, collection.Mutex.HasLock);
				form.FireSaveButton();
				AssertEquals("Should not be holding the mutex after clicking save", false, collection.Mutex.HasLock);
			}

			AssertEquals("Should Be Saved", true, voyage.IsInDatabase);
		}

		public void TestSavePath_LockedByAnother()
		{
			var voyage = GetVoyageWithoutNotifications();
			Env.Security.SailingScheduleAllocationEdit.IsAllowed = true;
			var mutex = new AgencyAllocationMutex(voyage);
			try
			{
				mutex.Lock();
				AssertEquals("Precondition: the test needs to be holding the mutex", true, mutex.HasLock);
				AssertEquals("Precondition: the voyage should not yet be in the database", false, voyage.IsInDatabase);
				using (var form = new ZJobVoyageForm(voyage))
				{
					form.Show();
					Application.DoEvents();
					UserIdleWorker.Flush();
					form.PlugIns.SelectPlugInTabPage(ControllerIDs.AgencyAllocation);
					var collection = GetCountryCollection(form);
					AssertEquals("Should not be holding the mutex before clicking save", false, collection.Mutex.HasLock);
					form.FireSaveButton();
					AssertEquals("Should not be holding the mutex after clicking save", false, collection.Mutex.HasLock);
					AssertEquals("Should not have been saved", false, voyage.IsInDatabase);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.FireSaveButton();
					AssertEquals("Should have been saved", true, voyage.IsInDatabase);
				}
			}
			finally
			{
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}
			}
		}

		public void TestSavePath_WithValidationErrors()
		{
			JobVoyage voyage = GetVoyageWithoutNotifications();
			AssertEquals("Precondition: the voyage should not yet be in the database", false, voyage.IsInDatabase);
			Env.Security.SailingScheduleAllocationEdit.IsAllowed = true;
			using (ZJobVoyageForm form = new ZJobVoyageForm(voyage))
			{
				form.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();
				form.PlugIns.SelectPlugInTabPage(ControllerIDs.AgencyAllocation);
				AgencyCountryCollection collection = GetCountryCollection(form);
				VoyageCountry currentCountry = collection.Voyage.Countries.GetCountry("AU", true);
				SlotAllocation allocation = currentCountry.SlotAllocations.GetAllocation(ZGuid.Empty);
				allocation.SetAspect(AllocationAspectTypes.TEU, -1);
				AssertEquals("Should not be holding the mutex before clicking save", false, collection.Mutex.HasLock);
				form.FireSaveButton();
				AssertEquals("Should not be holding the mutex after clicking save", false, collection.Mutex.HasLock);
			}

			AssertEquals("Should Not Be Saved", false, voyage.IsInDatabase);
		}

		public void TestSavePath_WithSaveErrors()
		{
			JobVoyage voyage = GetVoyageWithoutNotifications();
			AssertEquals("Precondition: the voyage should not yet be in the database", false, voyage.IsInDatabase);
			Env.Security.SailingScheduleAllocationEdit.IsAllowed = true;
			AgencyShipment shipmentToBreakSaveWithoutBreakingValidation = NewBulkShipment(null, null, false, false, 50, 50);
			shipmentToBreakSaveWithoutBreakingValidation.JS_OA_BookedShippingLineAddress = ZGuid.NewZGuid();
			using (ZJobVoyageForm form = new ZJobVoyageForm(voyage))
			{
				form.Show();
				Application.DoEvents();
				form.PlugIns.SelectPlugInTabPage(ControllerIDs.AgencyAllocation);
				AgencyCountryCollection collection = GetCountryCollection(form);
				AssertEquals("Should not be holding the mutex before clicking save", false, collection.Mutex.HasLock);
				form.FireSaveButton();
				AssertEquals("Should not be holding the mutex after clicking save", false, collection.Mutex.HasLock);
			}

			AssertEquals("Should Not be Saved", false, voyage.IsInDatabase);
			ErrorReporter.Clear(); // reports an error about constraint violation
		}

		[UseGlobalAllocations(true)]
		public void TestUserControl_Global()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			using (ZJobVoyageForm form = new ZJobVoyageForm(voyage))
			{
				form.Show();
				UserIdleWorker.Flush();
				form.PlugIns.SelectPlugInTabPage(ControllerIDs.AgencyAllocation);
				ZPlugIn plugin = form.PlugIns.GetPlugIn(ControllerIDs.AgencyAllocation);
				AssertType(typeof(TopLevelVoyageAllocationControl), plugin.UserControl);
			}
		}

		[UseGlobalAllocations(false)]
		public void TestUSerControl_Local()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			using (ZJobVoyageForm form = new ZJobVoyageForm(voyage))
			{
				form.Show();
				UserIdleWorker.Flush();
				form.PlugIns.SelectPlugInTabPage(ControllerIDs.AgencyAllocation);
				ZPlugIn plugin = form.PlugIns.GetPlugIn(ControllerIDs.AgencyAllocation);
				AssertType(typeof(OuterVoyageAllocationControl), plugin.UserControl);
			}
		}

		[UseGlobalAllocations(false)]
		public void TestSavePath_WithUsageErrors_Country()
		{
			GenericSavePath_WithUsageErrorsTest(AllocationMethodList.Codes.Country);
		}

		[UseGlobalAllocations(false)]
		public void TestSavePath_WithUsageErrors_Origin()
		{
			GenericSavePath_WithUsageErrorsTest(AllocationMethodList.Codes.Origin);
		}

		[UseGlobalAllocations(false)]
		public void TestSavePath_WithUsageErrors_Sailing()
		{
			GenericSavePath_WithUsageErrorsTest(AllocationMethodList.Codes.Sailing);
		}

		public void GenericSavePath_WithUsageErrorsTest(string mode)
		{
			JobVoyage voyage = GetVoyageWithoutNotifications();
			NewBulkShipment(voyage.Sailings[0], null, false, true, 50, 50);
			Env.Security.SailingScheduleAllocationEdit.IsAllowed = true;
			Factory.Save();
			using (ZJobVoyageForm form = new ZJobVoyageForm(voyage))
			{
				form.Show();
				UserIdleWorker.Flush();
				form.PlugIns.SelectPlugInTabPage(ControllerIDs.AgencyAllocation);
				ZPlugIn plugin = form.PlugIns.GetPlugIn(ControllerIDs.AgencyAllocation);
				OuterVoyageAllocationControl.TestHelper helper = new OuterVoyageAllocationControl.TestHelper((OuterVoyageAllocationControl)plugin.UserControl);
				AgencyCountryCollection collection = GetCountryCollection(form);
				voyage.Countries.GetCountry("AU", true).J0_AllocationMethod = mode;
				helper.TabControl.SelectedTab = helper.GenericTab;
				AssertEquals("Should not be holding the mutex before clicking save", false, collection.Mutex.HasLock);
				form.FireSaveButton();
				AssertEquals("Should not be holding the mutex after clicking save", false, collection.Mutex.HasLock);
			}

			AssertEquals("Should Not Be Saved", true, voyage.HasChanges);
		}

		#region Implementation
		void AssertLoadedCountries(string message, AgencyCountryCollection collection, params string[] expectedCountryCodes)
		{
			AssertContainsExactElementsInAnyOrder(message, expectedCountryCodes, Array.ConvertAll(collection.ToArray<AgencyCountry>(), (c) => c.CountryCode.ToString()));
		}

		AgencyCountryCollection GetCountryCollection(ZForm form)
		{
			return GetCountryCollection(form.PlugIns.GetPlugIn(ControllerIDs.AgencyAllocation));
		}

		AgencyCountryCollection GetCountryCollection(ZPlugIn plugin)
		{
			return (AgencyCountryCollection)plugin.BusinessEntity;
			//return (AgencyCountryCollection)typeof(AgencyAllocationPlugin).InvokeMember("schedule", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, plugin, new object[] { });
		}
		#endregion
	}
}
