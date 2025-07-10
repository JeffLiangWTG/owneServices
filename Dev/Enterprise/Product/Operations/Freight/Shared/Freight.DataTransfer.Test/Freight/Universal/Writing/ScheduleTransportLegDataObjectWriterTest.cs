using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class ScheduleTransportLegDataObjectWriterTest : TestCaseWithFactory
	{
		#region WriteDataObject

		public void TestWriteDataObject()
		{
			var schedule = SetupSchedule();
			var writer = new ScheduleTransportLegDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, schedule)));
			var scheduleDataObject = writer.GetDataObject(schedule);

			AssertSchedule(scheduleDataObject);
		}

		public void TestWriteDataObject_Air()
		{
			var schedule = SetupAirSchedule();
			var writer = new ScheduleTransportLegDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, schedule)));
			var scheduleDataObject = writer.GetDataObject(schedule);

			AssertAirSchedule(scheduleDataObject);
		}

		#endregion

		#region AssertSchedule

		public void AssertSchedule(TransportLeg scheduleDataObject)
		{
			AssertNotNull("Precondition: scheduleDataObject", scheduleDataObject);

			var helper = new LocalCartageTestHelper(Factory);

			AssertEquals(ZByte.ParseSafe("1", ZByte.Zero), scheduleDataObject.LegOrder);
			AssertEquals(TransportMode.Sea, scheduleDataObject.TransportMode.Value);
			AssertEquals(LegType.Main, scheduleDataObject.LegType.Value);
			AssertEquals(helper.TestVessel1.RV_Name, scheduleDataObject.VesselName);
			AssertEquals(helper.TestVessel1.RV_LloydsNumber, scheduleDataObject.VesselLloydsIMO);
			AssertEquals("111", scheduleDataObject.VoyageFlightNo);

			AssertEquals(carrier.OH_FullName, scheduleDataObject.Carrier.CompanyName);

			AssertEquals(LocalCartageTestHelper.HomePort, scheduleDataObject.PortOfLoading.Code);
			AssertEquals(new ZDateTime(2012, 1, 1), scheduleDataObject.EstimatedDeparture);
			AssertEquals(new ZDateTime(2012, 1, 2), scheduleDataObject.ActualDeparture);
			AssertEquals(new ZDateTime(2012, 1, 3), scheduleDataObject.FCLReceivalCommences);
			AssertEquals(new ZDateTime(2012, 1, 4), scheduleDataObject.FCLCutOff);
			AssertEquals(new ZDateTime(2012, 1, 5), scheduleDataObject.LCLReceivalCommences);
			AssertEquals(new ZDateTime(2012, 1, 6), scheduleDataObject.LCLCutOff);
			AssertEquals("DEPBERTH", scheduleDataObject.DepartureBerth);
			AssertEquals("DEPREF", scheduleDataObject.DepartureReference);
			AssertEquals(new ZDateTime(2011, 12, 30), scheduleDataObject.EstimatedArrivalInPortOfLoading);
			AssertEquals(new ZDateTime(2011, 12, 31), scheduleDataObject.ActualArrivalInPortOfLoading);
			AssertEquals(new ZDateTime(2011, 12, 29), scheduleDataObject.DocumentCutOff);
			AssertEquals(new ZDateTime(2011, 12, 27), scheduleDataObject.HazzardReceivalCommences);
			AssertEquals(new ZDateTime(2011, 12, 28), scheduleDataObject.HazzardCutOffDate);
			AssertEquals(new ZDateTime(2011, 12, 26), scheduleDataObject.VGMCutOff);

			AssertEquals(departureCTO.OH_FullName, scheduleDataObject.DepartureCTO.CompanyName);

			AssertEquals(LocalCartageTestHelper.OverseasPort, scheduleDataObject.PortOfDischarge.Code);
			AssertEquals(new ZDateTime(2012, 1, 7), scheduleDataObject.EstimatedArrival);
			AssertEquals(new ZDateTime(2012, 1, 8), scheduleDataObject.ActualArrival);
			AssertEquals(new ZDateTime(2012, 1, 9), scheduleDataObject.FCLAvailability);
			AssertEquals(new ZDateTime(2012, 1, 10), scheduleDataObject.FCLStorage);
			AssertEquals(new ZDateTime(2012, 1, 11), scheduleDataObject.LCLAvailability);
			AssertEquals(new ZDateTime(2012, 1, 12), scheduleDataObject.LCLStorageDate);

			AssertEquals(new ZDateTime(2011, 12, 29), scheduleDataObject.ScheduledDeparture);
			AssertEquals(new ZDateTime(2012, 1, 2), scheduleDataObject.ScheduledArrivalInPortOfLoading);
			AssertEquals(new ZDateTime(2012, 1, 9), scheduleDataObject.ScheduledArrival);

			AssertEquals("ARVBERTH", scheduleDataObject.ArrivalBerth);
			AssertEquals("ARVREF", scheduleDataObject.ArrivalReference);

			AssertEquals(arrivalCTO.OH_FullName, scheduleDataObject.ArrivalCTO.CompanyName);

			AssertEquals(new ZDateTime(2012, 1, 15), scheduleDataObject.EmptyReceivalCommences);
			AssertEquals(new ZDateTime(2012, 1, 16), scheduleDataObject.EmptyCutOff);
			AssertEquals(new ZDateTime(2012, 1, 17), scheduleDataObject.ReeferReceivalCommences);
			AssertEquals(new ZDateTime(2012, 1, 18), scheduleDataObject.ReeferCutOff);
		}

		public JobSailing SetupSchedule()
		{
			var helper = new LocalCartageTestHelper(Factory);
			var schedule = helper.CreateSailing(helper.TestVessel1, "111", LocalCartageTestHelper.HomePort, LocalCartageTestHelper.OverseasPort, ZDateTime.Today);

			carrier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			schedule.Voyage.JV_OH_Line = carrier.PK;

			schedule.Origin.JA_E_DEP = new ZDateTime(2012, 1, 1);
			schedule.Origin.JA_A_DEP = new ZDateTime(2012, 1, 2);
			schedule.Origin.JA_ReceivalCommences = new ZDateTime(2012, 1, 3);
			schedule.Origin.JA_CutOff = new ZDateTime(2012, 1, 4);
			schedule.JX_DepotReceivalCommences = new ZDateTime(2012, 1, 5);
			schedule.JX_DepotCutOff = new ZDateTime(2012, 1, 6);
			schedule.Origin.JA_Berth = "DEPBERTH";
			schedule.Origin.JA_DepartReference = "DEPREF";
			schedule.Origin.JA_E_ARV = new ZDateTime(2011, 12, 30);
			schedule.Origin.JA_A_ARV = new ZDateTime(2011, 12, 31);
			schedule.Origin.JA_DocumentaryCutoff = new ZDateTime(2011, 12, 29);
			schedule.Origin.JA_DGReceivalCommences = new ZDateTime(2011, 12, 27);
			schedule.Origin.JA_DGCutOff = new ZDateTime(2011, 12, 28);
			schedule.Origin.JA_VGMCutOff = new ZDateTime(2011, 12, 26);
			schedule.Origin.JA_EmptyReceivalCommences = new ZDateTime(2012, 1, 15);
			schedule.Origin.JA_EmptyCutOff = new ZDateTime(2012, 1, 16);
			schedule.Origin.JA_ReeferReceivalCommences = new ZDateTime(2012, 1, 17);
			schedule.Origin.JA_ReeferCutOff = new ZDateTime(2012, 1, 18);
			schedule.Origin.JA_S_DEP = new ZDate(2011, 12, 29);
			schedule.Origin.JA_S_ARV = new ZDate(2012, 1, 2);

			departureCTO = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, carrier.PK));
			schedule.Origin.JA_OA_DepartureCTOAddress = departureCTO.MainAddress.PK;

			schedule.Destination.JB_E_ARV = new ZDateTime(2012, 1, 7);
			schedule.Destination.JB_A_ARV = new ZDateTime(2012, 1, 8);
			schedule.Destination.JB_AvailabilityDate = new ZDateTime(2012, 1, 9);
			schedule.Destination.JB_StorageDate = new ZDateTime(2012, 1, 10);
			schedule.JX_DepotAvailabilityDate = new ZDateTime(2012, 1, 11);
			schedule.JX_DepotStorageDate = new ZDateTime(2012, 1, 12);
			schedule.Destination.JB_Berth = "ARVBERTH";
			schedule.Destination.JB_ArrivalReference = "ARVREF";
			schedule.Destination.JB_S_ARV = new ZDateTime(2012, 1, 9);

			var arrivalCTOQuery = new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, carrier.PK);
			arrivalCTOQuery.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, departureCTO.PK);
			arrivalCTO = Factory.LoadTop1<OrgHeader>(arrivalCTOQuery);
			schedule.Destination.JB_OA_ArrivalCTOAddress = arrivalCTO.MainAddress.PK;

			return schedule;
		}

		public void AssertAirSchedule(TransportLeg scheduleDataObject)
		{
			AssertNotNull("Precondition: scheduleDataObject", scheduleDataObject);

			var helper = new LocalCartageTestHelper(Factory);

			AssertEquals(ZByte.ParseSafe("1", ZByte.Zero), scheduleDataObject.LegOrder);
			AssertEquals(TransportMode.Air, scheduleDataObject.TransportMode.Value);
			AssertEquals(LegType.Main, scheduleDataObject.LegType.Value);
			AssertEquals(true, scheduleDataObject.IsCargoOnly.Value);
			AssertEquals("AAA", scheduleDataObject.AircraftType.Code);

			AssertEquals(carrier.OH_FullName, scheduleDataObject.Carrier.CompanyName);
		}

		public JobSailing SetupAirSchedule()
		{
			var helper = new LocalCartageTestHelper(Factory);
			var schedule = helper.CreateSailing(helper.TestVessel1, "111", LocalCartageTestHelper.HomePort, LocalCartageTestHelper.OverseasPort, ZDateTime.Today, "AIR");

			carrier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			schedule.Voyage.JV_OH_Line = carrier.PK;
			schedule.Voyage.JV_IsCargoOnly = true;
			schedule.Voyage.JV_AircraftType = "AAA";

			return schedule;
		}

		OrgHeader carrier;

		OrgHeader departureCTO;

		OrgHeader arrivalCTO;

		#endregion

		#region Local Cartage Class

		class LocalCartageTestHelper : CartageTestHelper
		{
			public LocalCartageTestHelper(BusinessObjectFactory factory)
				: base(factory) { }

			#region Ports

			public static ZString HomePort
			{
				get
				{
					if (!GlbBranch.CurrentBranch.GB_RL_NKHomePort.IsEmpty)
					{
						return GlbBranch.CurrentBranch.GB_RL_NKHomePort;
					}
					return "AUSYD";
				}
			}

			public static ZString OverseasPort
			{
				get
				{
					if (GlbBranch.CurrentBranch.GB_RL_NKHomePort != "SGSIN")
					{
						return "SGSIN";
					}
					else
					{
						return "USLAX";
					}
				}
			}

			#endregion

			#region TestVessel1

			public RefVessel TestVessel1
			{
				get
				{
					if (fTestVessel1 == null)
					{
						var vessel = RefVessel.LookupVesselByName("APL EMERALD", Factory).FirstOrDefault();

						if (vessel == null)
						{
							vessel = Factory.New<RefVessel>();
							vessel.RV_LloydsNumber = "8610033";
							vessel.RV_Name = "APL EMERALD";
						}

						fTestVessel1 = vessel;
					}

					return fTestVessel1;
				}
			}
			RefVessel fTestVessel1;

			#endregion

			#region Create Sailing

			public JobSailing CreateSailing(RefVessel vessel, ZString voyage, ZString load, ZString discharge, ZDateTime eTD, string transportMode = Core.Constants.TransportModes.Sea)
			{
				JobVoyage resultVoyage = Factory.New<JobVoyage>();
				resultVoyage.JV_AirSeaRoad = transportMode;
				resultVoyage.JV_RV_NKVessel = vessel.RV_FK;
				resultVoyage.JV_VoyageFlight = voyage;

				VoyageOrigin origin = resultVoyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = load;
				origin.JA_E_DEP = eTD;

				VoyageDestination destination = resultVoyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = discharge;

				resultVoyage.GenerateSailings();
				return resultVoyage.Sailings[0];
			}

			#endregion
		}

		#endregion
	}
}
