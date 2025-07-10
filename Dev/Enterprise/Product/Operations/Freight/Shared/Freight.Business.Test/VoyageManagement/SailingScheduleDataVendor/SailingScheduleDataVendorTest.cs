using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Moq;

namespace Enterprise.Freight.Business.Testing
{
	sealed class SailingScheduleDataVendorTest : TestCaseWithFactory
	{
		#region Instance

		public void TestInstance_When_OnlineSailingSchedulesService_IsDisabled()
		{
			MockSailingScheduleDataVendor.UnregisterThisSubTypeOverride();

			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var onlineSailingSchedules = ObjectFactory.Get<Integration.SailingDataVendor.IOnlineSailingSchedulesDataVendor>() as SailingScheduleDataVendor;
				AssertEquals("prerequisite", false, onlineSailingSchedules.IsEnabled);

				GlbCompany.CurrentCompany.SetCountry("AU");
				AssertEquals("Australian clients should get the OneStop vendor", ObjectFactory.GetType<Integration.SailingDataVendor.IOneStopSailingScheduleDataVendor>(), SailingScheduleDataVendor.Instance.GetType());

				GlbCompany.CurrentCompany.SetCountry("ER");
				AssertEquals("Any country without a sailing schedule vendor should return a no-action vendor", typeof(NoActionSailingScheduleDataVendor), SailingScheduleDataVendor.Instance.GetType());

				GlbCompany.CurrentCompany.SetCountry("NZ");
				AssertEquals("NZ clients should get the OneStop vendor", ObjectFactory.GetType<Integration.SailingDataVendor.IOneStopSailingScheduleDataVendor>(), SailingScheduleDataVendor.Instance.GetType());

				GlbCompany.CurrentCompany.SetCountry("DE");
				AssertEquals("EU clients should get the Dakosy vendor", ObjectFactory.GetType<Integration.SailingDataVendor.IDakosySailingScheduleDataVendor>(), SailingScheduleDataVendor.Instance.GetType());

				GlbCompany.CurrentCompany.SetCountry("FR");
				AssertEquals("EU clients should get the Dakosy vendor", ObjectFactory.GetType<Integration.SailingDataVendor.IDakosySailingScheduleDataVendor>(), SailingScheduleDataVendor.Instance.GetType());
			}
		}

		public void TestInstance_When_OnlineSailingSchedulesService_IsEnabled()
		{
			MockSailingScheduleDataVendor.UnregisterThisSubTypeOverride();

			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var onlineSailingSchedules = ObjectFactory.Get<Integration.SailingDataVendor.IOnlineSailingSchedulesDataVendor>() as SailingScheduleDataVendor;
				AssertEquals("prerequisite", true, onlineSailingSchedules.IsEnabled);

				GlbCompany.CurrentCompany.SetCountry("DE");
				AssertEquals("EU clients should get the Dakosy vendor", ObjectFactory.GetType<Integration.SailingDataVendor.IDakosySailingScheduleDataVendor>(), SailingScheduleDataVendor.Instance.GetType());

				GlbCompany.CurrentCompany.SetCountry("FR");
				AssertEquals("EU clients should get the Dakosy vendor", ObjectFactory.GetType<Integration.SailingDataVendor.IDakosySailingScheduleDataVendor>(), SailingScheduleDataVendor.Instance.GetType());

				GlbCompany.CurrentCompany.SetCountry("AU");
				AssertEquals("AU clients should get the OneStop vendor", ObjectFactory.GetType<Integration.SailingDataVendor.IOneStopSailingScheduleDataVendor>(), SailingScheduleDataVendor.Instance.GetType());

				GlbCompany.CurrentCompany.SetCountry("NZ");
				AssertEquals("NZ clients should get the OneStop vendor", ObjectFactory.GetType<Integration.SailingDataVendor.IOneStopSailingScheduleDataVendor>(), SailingScheduleDataVendor.Instance.GetType());

				GlbCompany.CurrentCompany.SetCountry("JP");
				AssertEquals("JP clients should get the OnlineSchedule vendor", ObjectFactory.GetType<Integration.SailingDataVendor.IOnlineSailingSchedulesDataVendor>(), SailingScheduleDataVendor.Instance.GetType());

				GlbCompany.CurrentCompany.SetCountry("BR");
				AssertEquals("BR clients should get the OnlineSchedule vendor", ObjectFactory.GetType<Integration.SailingDataVendor.IOnlineSailingSchedulesDataVendor>(), SailingScheduleDataVendor.Instance.GetType());
			}
		}

		public void TestInstance_When_ImportingUXML()
		{
			MockSailingScheduleDataVendor.UnregisterThisSubTypeOverride();

			AssertInstanceWhenImportingUXML("UMI");
			AssertInstanceWhenImportingUXML("UMQ");
		}

		void AssertInstanceWhenImportingUXML(string serviceTaskCode)
		{
			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Env.Instance.TemporaryServiceTaskContext(serviceTaskCode, false))
			{
				GlbCompany.CurrentCompany.SetCountry("AU");
				AssertEquals(typeof(NoActionSailingScheduleDataVendor), SailingScheduleDataVendor.Instance.GetType());

				GlbCompany.CurrentCompany.SetCountry("NZ");
				AssertEquals(typeof(NoActionSailingScheduleDataVendor), SailingScheduleDataVendor.Instance.GetType());

				GlbCompany.CurrentCompany.SetCountry("DE");
				AssertEquals(typeof(NoActionSailingScheduleDataVendor), SailingScheduleDataVendor.Instance.GetType());

				GlbCompany.CurrentCompany.SetCountry("VN");
				AssertEquals(typeof(NoActionSailingScheduleDataVendor), SailingScheduleDataVendor.Instance.GetType());
			}
		}

		#endregion

		#region UpdateAllVoyageSailings

		public void TestUpdateAllVoyageSailings_WhenSettingVoyage()
		{
			Voyage.JV_VoyageFlight = "";
			Origin.JA_E_DEP = ZDateTime.Empty;
			Destination.JB_E_ARV = ZDateTime.Empty;

			MockSailingScheduleDataVendor.Instance.IsVendorDataCurrent = false;
			Voyage.JV_VoyageFlight = "Voyage";
			AssertEquals("No action taken if vendor data not current", ZDateTime.Empty, Origin.JA_E_DEP);

			MockSailingScheduleDataVendor.Instance.IsVendorDataCurrent = true;
			Voyage.JV_VoyageFlight = "";
			Voyage.JV_VoyageFlight = "Voyage";

			AssertEquals("Dates should be updated when voyage set", new ZDateTime(2000, 1, 1), Origin.JA_E_DEP);
			AssertEquals("Dates should be updated when voyage set", new ZDateTime(2000, 1, 5), Destination.JB_E_ARV);
		}

		public void TestUpdateAllVoyageSailings_WhenSettingVesselName()
		{
			Voyage.JV_RV_NKVessel = "";
			Origin.JA_E_DEP = ZDateTime.Empty;
			Destination.JB_E_ARV = ZDateTime.Empty;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Vessel";

			MockSailingScheduleDataVendor.Instance.IsVendorDataCurrent = false;
			Voyage.JV_RV_NKVessel = vessel.RV_FK;
			AssertEquals("No action taken if vendor data not current", ZDateTime.Empty, Origin.JA_E_DEP);

			MockSailingScheduleDataVendor.Instance.IsVendorDataCurrent = true;
			Voyage.JV_RV_NKVessel = "";
			Voyage.JV_RV_NKVessel = vessel.RV_FK;

			AssertEquals("Dates should be updated when vessel name set", new ZDateTime(2000, 1, 1), Origin.JA_E_DEP);
			AssertEquals("Dates should be updated when vessel name set", new ZDateTime(2000, 1, 5), Destination.JB_E_ARV);
		}

		#endregion

		#region UpdateVoyageOrigin

		public void TestUpdateVoyageOrigin_WhenSettingLoadPort()
		{
			MockSailingScheduleDataVendor.Instance.IsVendorDataCurrent = false;
			Origin.JA_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("No action taken if vendor data not current", ZDateTime.Empty, Origin.JA_E_DEP);
			Origin.JA_RL_NKPortOfLoading = "";

			MockSailingScheduleDataVendor.Instance.IsVendorDataCurrent = true;
			Origin.JA_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("Setting the load port should default ETD", new ZDateTime(2000, 1, 1), Origin.JA_E_DEP);
			AssertEquals("Setting the load port should default ATD", new ZDateTime(2000, 1, 2), Origin.JA_A_DEP);
			AssertEquals("Setting the load port should default Availability Date", new ZDateTime(2000, 1, 3), Voyage.Sailings[0].JX_JB_CTOAvailabilityDate);
			AssertEquals("Setting the load port should default Storage Date", new ZDateTime(2000, 1, 4), Voyage.Sailings[0].JX_JB_CTOStorageDate);
		}

		public void TestUpdateVoyageOrigin_WhenSettingSailingOrigin()
		{
			Sailing.JX_JA = ZGuid.Empty;
			Origin.JA_RL_NKPortOfLoading = "AUSYD";
			Origin.JA_E_DEP = ZDateTime.Empty;
			Origin.JA_A_DEP = ZDateTime.Empty;

			MockSailingScheduleDataVendor.Instance.IsVendorDataCurrent = false;
			Sailing.JX_JA = Origin.PK;
			AssertEquals("No action taken if vendor data not current", ZDateTime.Empty, Origin.JA_E_DEP);

			MockSailingScheduleDataVendor.Instance.IsVendorDataCurrent = true;
			Sailing.JX_JA = ZGuid.Empty;
			Sailing.JX_JA = Origin.PK;

			AssertEquals("Setting the origin fk should default ETD", new ZDateTime(2000, 1, 1), Origin.JA_E_DEP);
			AssertEquals("Setting the origin fk should default ATD", new ZDateTime(2000, 1, 2), Origin.JA_A_DEP);
			AssertEquals("Setting the origin fk should default Availability Date", new ZDateTime(2000, 1, 3), Voyage.Sailings[0].JX_JB_CTOAvailabilityDate);
			AssertEquals("Setting the origin fk should default Storage Date", new ZDateTime(2000, 1, 4), Voyage.Sailings[0].JX_JB_CTOStorageDate);
		}

		#endregion

		#region UpdateVoyageDestination

		public void UpdateVoyageDestination_WhenSettingDischargePort()
		{
			MockSailingScheduleDataVendor.Instance.IsVendorDataCurrent = false;
			Destination.JB_RL_NKPortOfDischarge = "AUMEL";
			AssertEquals("No action taken if vendor data not current", ZDateTime.Empty, Destination.JB_E_ARV);
			Destination.JB_RL_NKPortOfDischarge = "";

			MockSailingScheduleDataVendor.Instance.IsVendorDataCurrent = true;
			Destination.JB_RL_NKPortOfDischarge = "AUMEL";
			AssertEquals("Setting the discharge port should default ETA", new ZDateTime(2000, 1, 5), Destination.JB_E_ARV);
			AssertEquals("Setting the discharge port should default ATA", new ZDateTime(2000, 1, 6), Destination.JB_A_ARV);
			AssertEquals("Setting the discharge port should default Cut Off Date", new ZDateTime(2000, 1, 7), Voyage.Sailings[0].JX_JA_CTOCutOff);
			AssertEquals("Setting the discharge port should default Receival Commences Date", new ZDateTime(2000, 1, 8), Voyage.Sailings[0].JX_JA_CTOReceivalCommences);
		}

		public void UpdateVoyageDestination_WhenSettingSailingDestination()
		{
			Sailing.JX_JB = ZGuid.Empty;
			Destination.JB_RL_NKPortOfDischarge = "AUMEL";
			Destination.JB_E_ARV = ZDateTime.Empty;
			Destination.JB_A_ARV = ZDateTime.Empty;

			MockSailingScheduleDataVendor.Instance.IsVendorDataCurrent = false;
			Sailing.JX_JB = Destination.PK;
			AssertEquals("No action taken if vendor data not current", ZDateTime.Empty, Destination.JB_E_ARV);

			MockSailingScheduleDataVendor.Instance.IsVendorDataCurrent = true;
			Sailing.JX_JB = ZGuid.Empty;
			Sailing.JX_JB = Destination.PK;

			AssertEquals("Setting the destination fk should default ETA", new ZDateTime(2000, 1, 5), Destination.JB_E_ARV);
			AssertEquals("Setting the destination fk should default ATA", new ZDateTime(2000, 1, 6), Destination.JB_A_ARV);
			AssertEquals("Setting the destination fk should default Cut Off Date", new ZDateTime(2000, 1, 7), Voyage.Sailings[0].JX_JA_CTOCutOff);
			AssertEquals("Setting the destination fk should default Receival Commences Date", new ZDateTime(2000, 1, 8), Voyage.Sailings[0].JX_JA_CTOReceivalCommences);
		}

		#endregion

		#region Update Suppression

		public void TestUpdateVoyageOriginSuppression()
		{
			MockSailingScheduleDataVendor.Instance.IsVendorDataCurrent = true;

			Action<bool> assertOriginUpdateExecuted = (shouldCallUpdate) =>
			{
				AssertEquals("Precondition", !shouldCallUpdate, SailingScheduleDataVendor.IsOriginUpdateSuppressed(Origin.Factory));

				MockSailingScheduleDataVendor.Instance.UpdateVoyageOriginCalled = false;
				MockSailingScheduleDataVendor.Instance.UpdateVoyageOrigin(Origin);
				AssertEquals(shouldCallUpdate ? "Update was called" : "Update should be suppressed", shouldCallUpdate, MockSailingScheduleDataVendor.Instance.UpdateVoyageOriginCalled);
			};

			using (SailingScheduleDataVendor.SuppressVoyageOriginUpdate(Origin.Factory))
			{
				using (SailingScheduleDataVendor.SuppressVoyageOriginUpdate(Origin.Factory))
				{
					assertOriginUpdateExecuted(false);
				}
				assertOriginUpdateExecuted(false);
			}

			assertOriginUpdateExecuted(true);

			using (SailingScheduleDataVendor.SuppressVoyageOriginUpdate(new BusinessObjectFactory()))
			{
				assertOriginUpdateExecuted(true);
			}
		}

		public void TestUpdateVoyageDestinationSuppression()
		{
			MockSailingScheduleDataVendor.Instance.IsVendorDataCurrent = true;

			Action<bool> assertDestinationUpdateExecuted = (shouldCallUpdate) =>
			{
				AssertEquals("Precondition", !shouldCallUpdate, SailingScheduleDataVendor.IsDestinationUpdateSuppressed(Destination.Factory));

				MockSailingScheduleDataVendor.Instance.UpdateVoyageDestinationCalled = false;
				MockSailingScheduleDataVendor.Instance.UpdateVoyageDestination(Destination);
				AssertEquals(shouldCallUpdate ? "Update was called" : "Update should be suppressed", shouldCallUpdate, MockSailingScheduleDataVendor.Instance.UpdateVoyageDestinationCalled);
			};

			using (SailingScheduleDataVendor.SuppressVoyageDestinationUpdate(Destination.Factory))
			{
				using (SailingScheduleDataVendor.SuppressVoyageDestinationUpdate(Destination.Factory))
				{
					assertDestinationUpdateExecuted(false);
				}
				assertDestinationUpdateExecuted(false);
			}

			assertDestinationUpdateExecuted(true);

			using (SailingScheduleDataVendor.SuppressVoyageDestinationUpdate(new BusinessObjectFactory()))
			{
				assertDestinationUpdateExecuted(true);
			}
		}

		#endregion

		#region BusyIndicator

		public void TestBusyIndicator()
		{
			var busyIndicatorProvider = new Mock<IBusyIndicatorProvider>(MockBehavior.Strict);

			bool busyIndicatorDisposed = false;

			busyIndicatorProvider
				.Setup(m => m.NewBusyIndicator())
				.Returns(new DisposableAction(createAction: () => busyIndicatorDisposed = false,
					disposeAction: () => busyIndicatorDisposed = true));

			Origin.Factory.SetValue(() => busyIndicatorProvider.Object);

			MockSailingScheduleDataVendor.Instance.IsVendorDataCurrent = true;
			MockSailingScheduleDataVendor.Instance.UpdateVoyageOrigin(Origin);

			AssertEquals("BusyIndicator should have been used and then disposed", true, busyIndicatorDisposed);
		}

		#endregion

		#region Implementation

		JobVoyage Voyage
		{
			get
			{
				if (voyage == null)
				{
					voyage = Factory.New<JobVoyage>();
					voyage.JV_RV_NKVessel = RefVessel.RV_FK;
					voyage.JV_VoyageFlight = "Voyage";

					VoyageOrigin origin = voyage.Origins.AddNew();
					origin.JA_RL_NKPortOfLoading = "MYPKG";

					VoyageDestination destination = voyage.Destinations.AddNew();
					destination.JB_RL_NKPortOfDischarge = "AUPER";
				}
				return voyage;
			}
		}
		JobVoyage voyage;

		JobSailing Sailing
		{
			get { return sailing ?? (sailing = Voyage.Sailings[0]); }
		}
		JobSailing sailing;

		VoyageOrigin Origin
		{
			get { return origin ?? (origin = Voyage.Origins[0]); }
		}
		VoyageOrigin origin;

		VoyageDestination Destination
		{
			get { return destination ?? (destination = Voyage.Destinations[0]); }
		}
		VoyageDestination destination;

		RefVessel RefVessel
		{
			get
			{
				if (refVessel == null)
				{
					refVessel = Factory.New<RefVessel>();
					refVessel.RV_Name = "Vessel";
					refVessel.RV_LloydsNumber = "Lloyds";
				}
				return refVessel;
			}
		}
		RefVessel refVessel;

		protected override void SetUp()
		{
			base.SetUp();
			MockSailingScheduleDataVendor.RegisterThisSubTypeOverride();
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockSailingScheduleDataVendor.UnregisterThisSubTypeOverride();
		}

		#endregion
	}
}
