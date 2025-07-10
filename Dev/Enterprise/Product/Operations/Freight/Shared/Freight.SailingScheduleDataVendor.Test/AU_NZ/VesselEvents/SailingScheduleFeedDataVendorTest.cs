using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	class SailingScheduleFeedDataVendorTest : TestCaseWithFactory
	{
		public void TestIsEnabled()
		{
			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Empty;
			AssertEquals("IsEnabled=false when 1-Stop data has never been received", false, DataVendor.IsEnabled);

			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = new ZDateTime(2005, 1, 2);
			AssertEquals("IsEnabled=true when 1-Stop data has been received at any time", true, DataVendor.IsEnabled);
		}

		public void TestIsVendorDataCurrent()
		{
			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now.AddHours(-47);
			AssertEquals("IsVendorDataCurrent=true when 1-Stop data has been received within 48 hours", true, DataVendor.IsEnabled);

			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now.AddHours(-49);
			AssertEquals("IsVendorDataCurrent=false when 1-Stop data hasn't been received for over 48 hours", false, DataVendor.IsVendorDataCurrent);
		}

		[TestDate(2005, 1, 12)]
		public void TestStatus()
		{
			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now.AddHours(-1);
			AssertEquals("Sailing schedule feed up to date (rcvd. 1 hr ago)", DataVendor.Status);
			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now.AddHours(-2);
			AssertEquals("OneStopSailingScheduleDataVendor.Status4", "Sailing schedule feed up to date (rcvd. 2 hrs ago)", DataVendor.Status);
		}

		[TestDate(2005, 1, 12)]
		public void TestStatus_WhenNoOneStopSchedulesEverImported()
		{
			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Empty;
			AssertEquals("Sailing schedule feed not active. Contact CargoWise.", DataVendor.Status);
		}

		[TestDate(2005, 1, 12)]
		public void TestStatus_WhenOneStopDataNotCurrent()
		{
			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now.AddHours(-49);
			AssertEquals("Sailing schedule feed not current (09-Jan-05)", DataVendor.Status);
		}

		#region UpdateVoyageOrigin

		public void TestUpdateVoyageOrigin()
		{
			var loadPort1 = NewJobVesselSchedule("AUSYD", "Lloyds", "Voyage");
			loadPort1.EV_ETD = new ZDateTime(2001, 1, 1);
			loadPort1.EV_ActualDeparture = new ZDateTime(2001, 1, 2);
			loadPort1.EV_ImportAvailability = new ZDateTime(2001, 1, 5);
			loadPort1.EV_ImportStorageCommences = new ZDateTime(2001, 1, 6);

			Factory.Save();

			DataVendor.UpdateVoyageOrigin(Origin);

			AssertEquals("JA_E_DEP updated without Carrier info.", loadPort1.EV_ETD, Origin.JA_E_DEP);
			AssertEquals("JA_A_DEP updated without Carrier info.", loadPort1.EV_ActualDeparture, Origin.JA_A_DEP);
			AssertEquals("JX_JA_CTOCutOff updated without Carrier info.", loadPort1.EV_CargoCuttOff, Voyage.Sailings[0].JX_JA_CTOCutOff);
			AssertEquals("JX_JA_CTOReceivalCommences updated without Carrier info.", loadPort1.EV_ExportReceivalCommencementDate, Voyage.Sailings[0].JX_JA_CTOReceivalCommences);

			var loadPort2 = NewJobVesselSchedule("AUSYD", "Lloyds", "Voyage");
			loadPort2.EV_ETD = new ZDateTime(2000, 1, 1);
			loadPort2.EV_ActualDeparture = new ZDateTime(2000, 1, 2);
			loadPort2.EV_ImportAvailability = new ZDateTime(2000, 1, 5);
			loadPort2.EV_ImportStorageCommences = new ZDateTime(2000, 1, 6);
			loadPort2.EV_DataProvider = DataProvider;
			loadPort2.EV_LineOperator = "LO";

			Voyage.JV_OH_Line = LineOperator.PK;

			Factory.Save();

			DataVendor.UpdateVoyageOrigin(Origin);

			AssertEquals("JA_E_DEP updated with Carrier info.", loadPort2.EV_ETD, Origin.JA_E_DEP);
			AssertEquals("JA_A_DEP updated with Carrier info.", loadPort2.EV_ActualDeparture, Origin.JA_A_DEP);
			AssertEquals("JX_JA_CTOCutOff updated with Carrier info.", loadPort2.EV_CargoCuttOff, Voyage.Sailings[0].JX_JA_CTOCutOff);
			AssertEquals("JX_JA_CTOReceivalCommences updated with Carrier info.", loadPort2.EV_ExportReceivalCommencementDate, Voyage.Sailings[0].JX_JA_CTOReceivalCommences);
		}

		[ExpectNoExceptions]
		public void TestUpdateVoyageOrigin_WhenVoyageNull()
		{
			Origin.JA_JV = ZGuid.Empty;
			DataVendor.UpdateVoyageOrigin(Origin);
		}

		#endregion

		#region UpdateVoyageDestination

		public void TestUpdateVoyageDestination()
		{
			var dischargePort1 = NewJobVesselSchedule("AUMEL", "Lloyds", "Voyage");
			dischargePort1.EV_ETA = new ZDateTime(2001, 1, 3);
			dischargePort1.EV_ActualArrival = new ZDateTime(2001, 1, 4);
			dischargePort1.EV_CargoCuttOff = new ZDateTime(2001, 1, 7);
			dischargePort1.EV_ExportReceivalCommencementDate = new ZDateTime(2001, 1, 8);

			Factory.Save();

			DataVendor.UpdateVoyageDestination(Destination);

			AssertEquals("JB_E_ARV updated without Carrier info.", dischargePort1.EV_ETA, Destination.JB_E_ARV);
			AssertEquals("JB_A_ARV updated without Carrier info.", dischargePort1.EV_ActualArrival, Destination.JB_A_ARV);
			AssertEquals("JX_JA_FCLAvailabilityDate updated without Carrier info.", dischargePort1.EV_ImportAvailability, Voyage.Sailings[0].JX_JB_CTOAvailabilityDate);
			AssertEquals("JX_JA_FCLStorageDate updated without Carrier info.", dischargePort1.EV_ImportStorageCommences, Voyage.Sailings[0].JX_JB_CTOStorageDate);

			var dischargePort2 = NewJobVesselSchedule("AUMEL", "Lloyds", "Voyage");
			dischargePort2.EV_ETA = new ZDateTime(2000, 1, 3);
			dischargePort2.EV_ActualArrival = new ZDateTime(2000, 1, 4);
			dischargePort2.EV_CargoCuttOff = new ZDateTime(2000, 1, 7);
			dischargePort2.EV_ExportReceivalCommencementDate = new ZDateTime(2000, 1, 8);
			dischargePort2.EV_DataProvider = DataProvider;
			dischargePort2.EV_LineOperator = "LO";

			Voyage.JV_OH_Line = LineOperator.PK;

			Factory.Save();

			DataVendor.UpdateVoyageDestination(Destination);

			AssertEquals("JB_E_ARV updated with Carrier info.", dischargePort2.EV_ETA, Destination.JB_E_ARV);
			AssertEquals("JB_A_ARV updated with Carrier info.", dischargePort2.EV_ActualArrival, Destination.JB_A_ARV);
			AssertEquals("JX_JA_FCLAvailabilityDate updated with Carrier info.", dischargePort2.EV_ImportAvailability, Voyage.Sailings[0].JX_JB_CTOAvailabilityDate);
			AssertEquals("JX_JA_FCLStorageDate updated with Carrier info.", dischargePort2.EV_ImportStorageCommences, Voyage.Sailings[0].JX_JB_CTOStorageDate);
		}

		[ExpectNoExceptions]
		public void TestUpdateVoyageDestination_WhenVoyageNull()
		{
			Destination.JB_JV = ZGuid.Empty;
			DataVendor.UpdateVoyageDestination(Destination);
		}

		#endregion

		#region Implementation

		protected virtual ZString DataProvider
		{
			get { return FreightConstants.VesselDataProviders.OneStop; }
		}

		protected virtual SailingScheduleFeedDataVendor DataVendor
		{
			get
			{
				if (oneStopVendor == null)
				{
					oneStopVendor = new SailingScheduleFeedDataVendor();
				}

				return oneStopVendor;
			}
		}
		SailingScheduleFeedDataVendor oneStopVendor;

		protected virtual JobVoyage Voyage
		{
			get
			{
				if (voyage == null)
				{
					voyage = Factory.New<JobVoyage>();
					voyage.JV_RV_NKVessel = RefVessel.RV_FK;
					voyage.JV_VoyageFlight = "Voyage";

					VoyageOrigin origin = voyage.Origins.AddNew();
					origin.JA_RL_NKPortOfLoading = "AUSYD";
					VoyageDestination destination = voyage.Destinations.AddNew();
					destination.JB_RL_NKPortOfDischarge = "AUMEL";
				}

				return voyage;
			}
		}
		JobVoyage voyage;

		protected virtual VoyageOrigin Origin
		{
			get
			{
				if (origin == null)
				{
					origin = Voyage.Origins[0];
				}

				return origin;
			}
		}
		VoyageOrigin origin;

		VoyageDestination Destination
		{
			get
			{
				if (destination == null)
				{
					destination = Voyage.Destinations[0];
				}

				return destination;
			}
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

		protected virtual OrgHeader LineOperator
		{
			get
			{
				if (lineOperator == null)
				{
					lineOperator = Factory.NewWithValidTestData<OrgHeader>();
					lineOperator.OH_IsShippingLine = true;
					lineOperator.OH_IsShippingProvider = true;

					OrgCusCode oneStopCode = lineOperator.CustomsCodes.AddNew();
					oneStopCode.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
					oneStopCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
					oneStopCode.OK_CustomsRegNo = "LO";

					Factory.Save();
				}

				return lineOperator;
			}
		}
		OrgHeader lineOperator;

		protected virtual JobVesselSchedule NewJobVesselSchedule(ZString portName, ZString lloydsNumber, ZString voyage)
		{
			JobVesselSchedule result = Factory.New<JobVesselSchedule>();
			result.EV_RL_NKPortCode = portName;
			result.EV_IMOLloydsNumber = lloydsNumber;
			result.EV_ShipOperatorVoyageIn = voyage;
			result.EV_ShipOperatorVoyageOut = voyage;
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			oldOneStopSailingSchedulesLastReceived = FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived;
			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now;
			AssertEquals("Vendor data must be current otherwise some operations will be unfunctional", true, DataVendor.IsVendorDataCurrent);
		}

		protected override void TearDown()
		{
			base.TearDown();
			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = oldOneStopSailingSchedulesLastReceived;
		}

		ZDateTime oldOneStopSailingSchedulesLastReceived;

		#endregion
	}
}
