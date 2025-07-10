using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.SailingDataVendor.Business;
using Enterprise.Freight.SailingDataVendor.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalSchedule = Enterprise.UniversalDataBuss.DataObjects.Universal.Schedule;

namespace Enterprise.Freight.SailingScheduleDataVendor.Testing
{
	sealed class UniversalScheduleImporterTest : TestCaseWithFactory
	{
		public void TestImportUniversalSchedule_Cancellation()
		{
			var schedule1 = Factory.New<JobVesselSchedule>();
			schedule1.EV_DataProvider = FreightConstants.VesselDataProviders.DAKOSY;
			schedule1.EV_RL_NKPortCode = "AUSYD";
			schedule1.EV_ShipOperatorVoyageIn = "AA4635AB";
			schedule1.EV_ShipName = "CAPTAIN DUDE";
			schedule1.EV_LineOperator = "ABC";

			var schedule2 = Factory.New<JobVesselSchedule>();
			schedule2.EV_DataProvider = FreightConstants.VesselDataProviders.DAKOSY;
			schedule2.EV_RL_NKPortCode = "AUSYD";
			schedule2.EV_ShipOperatorVoyageIn = "AA4635AB";
			schedule2.EV_LineOperator = "XYZ";

			var schedule3 = Factory.New<JobVesselSchedule>();
			schedule3.EV_DataProvider = FreightConstants.VesselDataProviders.DAKOSY;
			schedule3.EV_LineOperator = "ABC";
			schedule3.EV_RL_NKPortCode = "AUSYD";
			schedule3.EV_ShipOperatorVoyageOut = "AA4635AB";

			var schedule4 = Factory.New<JobVesselSchedule>();
			schedule4.EV_DataProvider = FreightConstants.VesselDataProviders.DAKOSY;
			schedule4.EV_LineOperator = "XYZ";
			schedule4.EV_RL_NKPortCode = "SGSIN";
			schedule4.EV_ShipOperatorVoyageIn = "AA4635AB";

			Factory.Save();

			var carrier = GetOrganizationAddress("158 Test St", "ABC", "ABC Company");
			var universalSchedule = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, true, "", "CAPTAIN DUDE", "AA4635AB", "HEEEEELP!!", carrier);
			universalSchedule.DischargeCollection.Add(GetDischargePort("AUSYD", new DateTime(2012, 09, 04), new DateTime(2012, 09, 05), new DateTime(2012, 09, 06), new DateTime(2012, 09, 11)));

			var importer = new UniversalScheduleImporter(Factory, new TestErrorLogger());
			importer.ImportUniversalSchedule(universalSchedule);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertNull(newFactory.Load<JobVesselSchedule>(schedule1.PK));
			AssertNotNull(newFactory.Load<JobVesselSchedule>(schedule2.PK));
			AssertNotNull(newFactory.Load<JobVesselSchedule>(schedule3.PK));
			AssertNotNull(newFactory.Load<JobVesselSchedule>(schedule4.PK));
		}

		public void TestImportUniversalSchedule()
		{
			AssertEquals(0, Factory.GetDatabaseCount(typeof(JobVesselRouting)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(JobVesselSchedule)));

			var importer = new UniversalScheduleImporter(Factory, new TestErrorLogger());
			var universalSchedule = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, false, "235235", "ADMIRAL VASILIY PUPKIN", "AA4635AB", "HEEEEELP!!");
			universalSchedule.DischargeCollection.Add(GetDischargePort("SGSIN", new DateTime(2012, 08, 24), new DateTime(2012, 08, 25), new DateTime(2012, 08, 26), new DateTime(2012, 08, 28)));
			universalSchedule.DischargeCollection.Add(GetDischargePort("AUSYD", new DateTime(2012, 09, 04), new DateTime(2012, 09, 05), new DateTime(2012, 09, 06), new DateTime(2012, 09, 11)));
			universalSchedule.LoadingCollection.Add(GetLoadPort("UAODS", new DateTime(2012, 08, 01), new DateTime(2012, 08, 03), new DateTime(2012, 08, 06), "ID1", new DateTime(2012, 08, 08), "depRef1"));
			universalSchedule.LoadingCollection.Add(GetLoadPort("NLRTM", new DateTime(2012, 08, 04), new DateTime(2012, 08, 07), new DateTime(2012, 08, 09), "ID2", new DateTime(2012, 08, 11), "depRef2"));

			AssertEquals(MessageStatus.Processed, importer.ImportUniversalSchedule(universalSchedule));

			Factory.Save();

			AssertEquals(4, Factory.GetDatabaseCount(typeof(JobVesselSchedule)));

			var schedules = Factory.Load<JobVesselSchedule>(new ZQuery()).OrderBy(r => r.EV_RL_NKPortCode).ToList();
			var schedule = schedules[0];
			AssertEquals("AUSYD", schedule.EV_RL_NKPortCode);
			AssertEquals(new DateTime(2012, 09, 04), schedule.EV_ETA);
			AssertEquals(ZDateTime.Empty, schedule.EV_ETD);
			AssertEquals(new DateTime(2012, 09, 05), schedule.EV_ActualArrival);
			AssertEquals(new DateTime(2012, 09, 06), schedule.EV_ImportAvailability);
			AssertEquals(new DateTime(2012, 09, 11), schedule.EV_ImportStorageCommences);
			AssertEquals("ADMIRAL VASILIY PUPKIN", schedule.EV_ShipName);
			AssertEquals("235235", schedule.EV_IMOLloydsNumber);
			AssertEquals("AA4635AB", schedule.EV_ShipOperatorVoyageIn);
			AssertEquals(ZString.Empty, schedule.EV_ShipOperatorVoyageOut);
			AssertEquals(ZString.Empty, schedule.EV_DataProviderReference);
			AssertEquals("HEEEEELP!!", schedule.EV_RadioCallSign);
			AssertEquals(FreightConstants.VesselDataProviders.DAKOSY, schedule.EV_DataProvider);

			schedule = schedules[1];
			AssertEquals("NLRTM", schedule.EV_RL_NKPortCode);
			AssertEquals(new DateTime(2012, 08, 04), schedule.EV_ETD);
			AssertEquals(ZDateTime.Empty, schedule.EV_ETA);
			AssertEquals(new DateTime(2012, 08, 07), schedule.EV_CargoCuttOff);
			AssertEquals(new DateTime(2012, 08, 09), schedule.EV_ActualDeparture);
			AssertEquals(new DateTime(2012, 08, 11), schedule.EV_ExportReceivalCommencementDate);
			AssertEquals("ID2", schedule.EV_TerminalID);
			AssertEquals("ADMIRAL VASILIY PUPKIN", schedule.EV_ShipName);
			AssertEquals("235235", schedule.EV_IMOLloydsNumber);
			AssertEquals("AA4635AB", schedule.EV_ShipOperatorVoyageOut);
			AssertEquals(ZString.Empty, schedule.EV_ShipOperatorVoyageIn);
			AssertEquals("depRef2", schedule.EV_DataProviderReference);
			AssertEquals("HEEEEELP!!", schedule.EV_RadioCallSign);
			AssertEquals(FreightConstants.VesselDataProviders.DAKOSY, schedule.EV_DataProvider);

			schedule = schedules[2];
			AssertEquals("SGSIN", schedule.EV_RL_NKPortCode);
			AssertEquals(new DateTime(2012, 08, 24), schedule.EV_ETA);
			AssertEquals(ZDateTime.Empty, schedule.EV_ETD);
			AssertEquals(new DateTime(2012, 08, 25), schedule.EV_ActualArrival);
			AssertEquals(new DateTime(2012, 08, 26), schedule.EV_ImportAvailability);
			AssertEquals(new DateTime(2012, 08, 28), schedule.EV_ImportStorageCommences);
			AssertEquals("ADMIRAL VASILIY PUPKIN", schedule.EV_ShipName);
			AssertEquals("235235", schedule.EV_IMOLloydsNumber);
			AssertEquals("AA4635AB", schedule.EV_ShipOperatorVoyageIn);
			AssertEquals(ZString.Empty, schedule.EV_ShipOperatorVoyageOut);
			AssertEquals(ZString.Empty, schedule.EV_DataProviderReference);
			AssertEquals("HEEEEELP!!", schedule.EV_RadioCallSign);
			AssertEquals(FreightConstants.VesselDataProviders.DAKOSY, schedule.EV_DataProvider);

			schedule = schedules[3];
			AssertEquals("UAODS", schedule.EV_RL_NKPortCode);
			AssertEquals(new DateTime(2012, 08, 01), schedule.EV_ETD);
			AssertEquals(ZDateTime.Empty, schedule.EV_ETA);
			AssertEquals(new DateTime(2012, 08, 03), schedule.EV_CargoCuttOff);
			AssertEquals(new DateTime(2012, 08, 06), schedule.EV_ActualDeparture);
			AssertEquals(new DateTime(2012, 08, 08), schedule.EV_ExportReceivalCommencementDate);
			AssertEquals("ID1", schedule.EV_TerminalID);
			AssertEquals("ADMIRAL VASILIY PUPKIN", schedule.EV_ShipName);
			AssertEquals("235235", schedule.EV_IMOLloydsNumber);
			AssertEquals("AA4635AB", schedule.EV_ShipOperatorVoyageOut);
			AssertEquals(ZString.Empty, schedule.EV_ShipOperatorVoyageIn);
			AssertEquals("depRef1", schedule.EV_DataProviderReference);
			AssertEquals("HEEEEELP!!", schedule.EV_RadioCallSign);
			AssertEquals(FreightConstants.VesselDataProviders.DAKOSY, schedule.EV_DataProvider);
		}

		public void TestImportUniversalSchedule_SavesDatesAsSmallDateTime()
		{
			AssertEquals(0, Factory.GetDatabaseCount(typeof(JobVesselRouting)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(JobVesselSchedule)));

			var importer = new UniversalScheduleImporter(Factory, new TestErrorLogger());
			var universalSchedule = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, false, "235235", "ADMIRAL VASILIY PUPKIN", "AA4635AB", "HEEEEELP!!");
			universalSchedule.DischargeCollection.Add(GetDischargePort("SGSIN", new DateTime(2012, 08, 24, 12, 30, 45), new DateTime(2012, 08, 25, 14, 30, 15), new DateTime(2012, 08, 26, 18, 30, 25), new DateTime(2012, 08, 28, 14, 25, 30)));

			AssertEquals(MessageStatus.Processed, importer.ImportUniversalSchedule(universalSchedule));

			Factory.Save();

			AssertEquals(1, Factory.GetDatabaseCount(typeof(JobVesselSchedule)));

			var anotherFactory = new BusinessObjectFactory();
			var schedules = anotherFactory.Load<JobVesselSchedule>(new ZQuery());
			AssertEquals(1, schedules.Length);

			var schedule = schedules[0];
			AssertEquals("SGSIN", schedule.EV_RL_NKPortCode);
			AssertEquals(new DateTime(2012, 08, 24, 12, 31, 00), schedule.EV_ETA);
			AssertEquals(ZDateTime.Empty, schedule.EV_ETD);
			AssertEquals(new DateTime(2012, 08, 25, 14, 30, 00), schedule.EV_ActualArrival);
			AssertEquals(new DateTime(2012, 08, 26, 18, 30, 00), schedule.EV_ImportAvailability);
			AssertEquals(new DateTime(2012, 08, 28, 14, 26, 00), schedule.EV_ImportStorageCommences);
			AssertEquals("ADMIRAL VASILIY PUPKIN", schedule.EV_ShipName);
			AssertEquals("235235", schedule.EV_IMOLloydsNumber);
			AssertEquals("AA4635AB", schedule.EV_ShipOperatorVoyageIn);
			AssertEquals(ZString.Empty, schedule.EV_ShipOperatorVoyageOut);
			AssertEquals(ZString.Empty, schedule.EV_DataProviderReference);
			AssertEquals("HEEEEELP!!", schedule.EV_RadioCallSign);
			AssertEquals(FreightConstants.VesselDataProviders.DAKOSY, schedule.EV_DataProvider);
		}

		public void TestImportUniversalSchedule_NoInvalidVarCharToDateTimeConversionException()
		{
			var importer = new UniversalScheduleImporter(Factory, new TestErrorLogger());

			var badETA = ZDateTime.MinSmallDateTimeValue.AddDays(-10).ToDateTime();
			var badATA = ZDateTime.MaxSmallDateTimeValue.AddDays(10).ToDateTime();
			var validDate = new DateTime(2012, 08, 24, 12, 31, 00);

			var universalScheduleWithBadDateData = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, false, "235235", "ADMIRAL VASILIY PUPKIN", "AA4635AB", "SOS");
			universalScheduleWithBadDateData.DischargeCollection.Add(GetDischargePort("SGSIN", badETA, badATA, validDate, validDate));

			AssertEquals(MessageStatus.Processed, importer.ImportUniversalSchedule(universalScheduleWithBadDateData));

			AssertNoExceptionThrown("Expected no exception converting invalid date", Factory.Save);

			var anotherFactory = new BusinessObjectFactory();
			var schedule = anotherFactory.LoadTop1<JobVesselSchedule>(new ZQuery());
			AssertNotNull("Expected to have created a schedule despite the times", schedule);

			AssertEquals("Expected to remain empty because no date wass given", ZDateTime.Empty, schedule.EV_ETD);
			AssertEquals("Expected invalid (before min) date time to be empty instead of invalid", ZDateTime.Empty, schedule.EV_ETA);
			AssertEquals("Expected invalid (beyond max) date time to be empty instead of invalid", ZDateTime.Empty, schedule.EV_ActualArrival);
			AssertEquals("Expected valid date to be imported fine", validDate, schedule.EV_ImportStorageCommences);
		}

		public void TestImportUniversalSchedule_LogsScheduleChanges()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "235235";
			vessel.RV_Name = "ADMIRAL VASILIY PUPKIN";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "AA4635AB";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NLRTM";

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";

			Factory.Save();

			AssertEquals(0, Factory.GetDatabaseCount(typeof(JobVesselSchedule)));

			var importer = new UniversalScheduleImporter(Factory, new TestErrorLogger());
			var universalSchedule = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, false, "235235", "ADMIRAL VASILIY PUPKIN", "AA4635AB", "HEEEEELP!!");
			universalSchedule.DischargeCollection.Add(GetDischargePort("SGSIN", new DateTime(2012, 08, 24, 12, 30, 00), new DateTime(2012, 08, 25, 14, 30, 00), new DateTime(2012, 08, 26, 18, 30, 00), new DateTime(2012, 08, 28, 14, 25, 00)));
			universalSchedule.LoadingCollection.Add(GetLoadPort("NLRTM", new DateTime(2012, 08, 04), new DateTime(2012, 08, 07), new DateTime(2012, 08, 09), "ID2", new DateTime(2012, 08, 11), "depRef2"));

			AssertEquals(MessageStatus.Processed, importer.ImportUniversalSchedule(universalSchedule));
			Factory.Save();

			AssertEquals(2, Factory.GetDatabaseCount(typeof(JobVesselSchedule)));

			var schedules = Factory.Load<JobVesselSchedule>(new ZQuery()).OrderBy(r => r.EV_RL_NKPortCode).ToList();
			AssertEquals(2, schedules.Count);

			var schedule = schedules[0];
			AssertEquals("NLRTM", schedule.EV_RL_NKPortCode);
			AssertEquals(new DateTime(2012, 08, 04), schedule.EV_ETD);
			AssertEquals(ZDateTime.Empty, schedule.EV_ETA);
			AssertEquals(new DateTime(2012, 08, 07), schedule.EV_CargoCuttOff);
			AssertEquals(new DateTime(2012, 08, 09), schedule.EV_ActualDeparture);
			AssertEquals(new DateTime(2012, 08, 11), schedule.EV_ExportReceivalCommencementDate);
			AssertEquals("ID2", schedule.EV_TerminalID);
			AssertEquals("ADMIRAL VASILIY PUPKIN", schedule.EV_ShipName);
			AssertEquals("235235", schedule.EV_IMOLloydsNumber);
			AssertEquals("AA4635AB", schedule.EV_ShipOperatorVoyageOut);
			AssertEquals(ZString.Empty, schedule.EV_ShipOperatorVoyageIn);
			AssertEquals("depRef2", schedule.EV_DataProviderReference);
			AssertEquals("HEEEEELP!!", schedule.EV_RadioCallSign);
			AssertEquals(FreightConstants.VesselDataProviders.DAKOSY, schedule.EV_DataProvider);

			schedule = schedules[1];
			AssertEquals("SGSIN", schedule.EV_RL_NKPortCode);
			AssertEquals(new DateTime(2012, 08, 24, 12, 30, 00), schedule.EV_ETA);
			AssertEquals(ZDateTime.Empty, schedule.EV_ETD);
			AssertEquals(new DateTime(2012, 08, 25, 14, 30, 00), schedule.EV_ActualArrival);
			AssertEquals(new DateTime(2012, 08, 26, 18, 30, 00), schedule.EV_ImportAvailability);
			AssertEquals(new DateTime(2012, 08, 28, 14, 25, 00), schedule.EV_ImportStorageCommences);
			AssertEquals("ADMIRAL VASILIY PUPKIN", schedule.EV_ShipName);
			AssertEquals("235235", schedule.EV_IMOLloydsNumber);
			AssertEquals("AA4635AB", schedule.EV_ShipOperatorVoyageIn);
			AssertEquals(ZString.Empty, schedule.EV_ShipOperatorVoyageOut);
			AssertEquals(ZString.Empty, schedule.EV_DataProviderReference);
			AssertEquals("HEEEEELP!!", schedule.EV_RadioCallSign);
			AssertEquals(FreightConstants.VesselDataProviders.DAKOSY, schedule.EV_DataProvider);

			AssertEquals("Origin dates were updated from Dakosy schedule feed", new DateTime(2012, 08, 04), origin.JA_E_DEP);
			AssertEquals("Origin dates were updated from Dakosy schedule feed", new DateTime(2012, 08, 09), origin.JA_A_DEP);
			AssertEquals("Origin dates were updated from Dakosy schedule feed", new DateTime(2012, 08, 07), origin.JA_CutOff);
			AssertEquals("Origin dates were updated from Dakosy schedule feed", new DateTime(2012, 08, 11), origin.JA_ReceivalCommences);

			AssertEquals("Destination dates were updated from Dakosy schedule feed", new DateTime(2012, 08, 24, 12, 30, 00), destination.JB_E_ARV);
			AssertEquals("Destination dates were updated from Dakosy schedule feed", new DateTime(2012, 08, 25, 14, 30, 00), destination.JB_A_ARV);
			AssertEquals("Destination dates were updated from Dakosy schedule feed", new DateTime(2012, 08, 26, 18, 30, 00), destination.JB_AvailabilityDate);
			AssertEquals("Destination dates were updated from Dakosy schedule feed", new DateTime(2012, 08, 28, 14, 25, 00), destination.JB_StorageDate);

			JobScheduleChangeLoggerTest.AssertDateChangeLogged(Factory, ScheduleDateTypes.Codes.ETD, ZDateTime.Empty, origin.JA_E_DEP, FreightConstants.VesselDataProviders.DAKOSY);
			JobScheduleChangeLoggerTest.AssertDateChangeLogged(Factory, ScheduleDateTypes.Codes.ATD, ZDateTime.Empty, origin.JA_A_DEP, FreightConstants.VesselDataProviders.DAKOSY);
			JobScheduleChangeLoggerTest.AssertDateChangeLogged(Factory, ScheduleDateTypes.Codes.FCLCutOff, ZDateTime.Empty, origin.JA_CutOff, FreightConstants.VesselDataProviders.DAKOSY);
			JobScheduleChangeLoggerTest.AssertDateChangeLogged(Factory, ScheduleDateTypes.Codes.FCLReceivalCommences, ZDateTime.Empty, origin.JA_ReceivalCommences, FreightConstants.VesselDataProviders.DAKOSY);

			JobScheduleChangeLoggerTest.AssertDateChangeLogged(Factory, ScheduleDateTypes.Codes.ETA, ZDateTime.Empty, destination.JB_E_ARV, FreightConstants.VesselDataProviders.DAKOSY);
			JobScheduleChangeLoggerTest.AssertDateChangeLogged(Factory, ScheduleDateTypes.Codes.ATA, ZDateTime.Empty, destination.JB_A_ARV, FreightConstants.VesselDataProviders.DAKOSY);
			JobScheduleChangeLoggerTest.AssertDateChangeLogged(Factory, ScheduleDateTypes.Codes.FCLAvailable, ZDateTime.Empty, destination.JB_AvailabilityDate, FreightConstants.VesselDataProviders.DAKOSY);
			JobScheduleChangeLoggerTest.AssertDateChangeLogged(Factory, ScheduleDateTypes.Codes.FCLStorage, ZDateTime.Empty, destination.JB_StorageDate, FreightConstants.VesselDataProviders.DAKOSY);
		}

		public void TestImportDuplicateUniversalSchedulesWithDifferentCarriers()
		{
			AssertEquals(0, Factory.GetDatabaseCount(typeof(JobVesselRouting)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(JobVesselSchedule)));

			var importer = new UniversalScheduleImporter(Factory, new TestErrorLogger());
			var carrier1 = GetOrganizationAddress("15 Test St", "ABC", "ABC COMPANY");
			var universalSchedule1 = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, false, "235235", "ADMIRAL VASILIY PUPKIN", "AA4635AB", "HEEEEELP!!", carrier1);
			universalSchedule1.DischargeCollection.Add(GetDischargePort("AUSYD", new DateTime(2012, 09, 04), new DateTime(2012, 09, 05), new DateTime(2012, 09, 06), new DateTime(2012, 09, 11)));
			universalSchedule1.LoadingCollection.Add(GetLoadPort("NLRTM", new DateTime(2012, 08, 04), new DateTime(2012, 08, 07), new DateTime(2012, 08, 09), "ID2", new DateTime(2012, 08, 11), "depRef2"));

			var carrier2 = GetOrganizationAddress("15 Test St", "XYZ", "XYZ COMPANY");
			var universalSchedule2 = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, false, "235235", "ADMIRAL VASILIY PUPKIN", "AA4635AB", "HEEEEELP!!", carrier2);
			universalSchedule2.DischargeCollection.Add(GetDischargePort("AUSYD", new DateTime(2012, 09, 04), new DateTime(2012, 09, 05), new DateTime(2012, 09, 06), new DateTime(2012, 09, 11)));
			universalSchedule2.LoadingCollection.Add(GetLoadPort("NLRTM", new DateTime(2012, 08, 04), new DateTime(2012, 08, 07), new DateTime(2012, 08, 09), "ID2", new DateTime(2012, 08, 11), "depRef2"));

			AssertEquals(MessageStatus.Processed, importer.ImportUniversalSchedule(universalSchedule1));
			AssertEquals(MessageStatus.Processed, importer.ImportUniversalSchedule(universalSchedule2));

			Factory.Save();

			AssertEquals(4, Factory.GetDatabaseCount(typeof(JobVesselSchedule)));

			var schedules = Factory.Load<JobVesselSchedule>(new ZQuery()).OrderBy(r => r.EV_RL_NKPortCode).ThenBy(r => r.EV_LineOperator).ToList();

			var schedule = schedules[0];
			AssertEquals("AUSYD", schedule.EV_RL_NKPortCode);
			AssertEquals(new DateTime(2012, 09, 04), schedule.EV_ETA);
			AssertEquals(ZDateTime.Empty, schedule.EV_ETD);
			AssertEquals(new DateTime(2012, 09, 05), schedule.EV_ActualArrival);
			AssertEquals(new DateTime(2012, 09, 06), schedule.EV_ImportAvailability);
			AssertEquals(new DateTime(2012, 09, 11), schedule.EV_ImportStorageCommences);
			AssertEquals("ADMIRAL VASILIY PUPKIN", schedule.EV_ShipName);
			AssertEquals("235235", schedule.EV_IMOLloydsNumber);
			AssertEquals("AA4635AB", schedule.EV_ShipOperatorVoyageIn);
			AssertEquals(ZString.Empty, schedule.EV_ShipOperatorVoyageOut);
			AssertEquals(ZString.Empty, schedule.EV_DataProviderReference);
			AssertEquals("HEEEEELP!!", schedule.EV_RadioCallSign);
			AssertEquals(FreightConstants.VesselDataProviders.DAKOSY, schedule.EV_DataProvider);
			AssertEquals("ABC", schedule.EV_LineOperator);
			AssertEquals("ABC COMPANY", schedule.EV_OperatorsDescription);

			schedule = schedules[1];
			AssertEquals("AUSYD", schedule.EV_RL_NKPortCode);
			AssertEquals(new DateTime(2012, 09, 04), schedule.EV_ETA);
			AssertEquals(ZDateTime.Empty, schedule.EV_ETD);
			AssertEquals(new DateTime(2012, 09, 05), schedule.EV_ActualArrival);
			AssertEquals(new DateTime(2012, 09, 06), schedule.EV_ImportAvailability);
			AssertEquals(new DateTime(2012, 09, 11), schedule.EV_ImportStorageCommences);
			AssertEquals("ADMIRAL VASILIY PUPKIN", schedule.EV_ShipName);
			AssertEquals("235235", schedule.EV_IMOLloydsNumber);
			AssertEquals("AA4635AB", schedule.EV_ShipOperatorVoyageIn);
			AssertEquals(ZString.Empty, schedule.EV_ShipOperatorVoyageOut);
			AssertEquals(ZString.Empty, schedule.EV_DataProviderReference);
			AssertEquals("HEEEEELP!!", schedule.EV_RadioCallSign);
			AssertEquals(FreightConstants.VesselDataProviders.DAKOSY, schedule.EV_DataProvider);
			AssertEquals("XYZ", schedule.EV_LineOperator);
			AssertEquals("XYZ COMPANY", schedule.EV_OperatorsDescription);

			schedule = schedules[2];
			AssertEquals("NLRTM", schedule.EV_RL_NKPortCode);
			AssertEquals(new DateTime(2012, 08, 04), schedule.EV_ETD);
			AssertEquals(ZDateTime.Empty, schedule.EV_ETA);
			AssertEquals(new DateTime(2012, 08, 07), schedule.EV_CargoCuttOff);
			AssertEquals(new DateTime(2012, 08, 09), schedule.EV_ActualDeparture);
			AssertEquals(new DateTime(2012, 08, 11), schedule.EV_ExportReceivalCommencementDate);
			AssertEquals("ID2", schedule.EV_TerminalID);
			AssertEquals("ADMIRAL VASILIY PUPKIN", schedule.EV_ShipName);
			AssertEquals("235235", schedule.EV_IMOLloydsNumber);
			AssertEquals("AA4635AB", schedule.EV_ShipOperatorVoyageOut);
			AssertEquals(ZString.Empty, schedule.EV_ShipOperatorVoyageIn);
			AssertEquals("depRef2", schedule.EV_DataProviderReference);
			AssertEquals("HEEEEELP!!", schedule.EV_RadioCallSign);
			AssertEquals(FreightConstants.VesselDataProviders.DAKOSY, schedule.EV_DataProvider);
			AssertEquals("ABC", schedule.EV_LineOperator);
			AssertEquals("ABC COMPANY", schedule.EV_OperatorsDescription);

			schedule = schedules[3];
			AssertEquals("NLRTM", schedule.EV_RL_NKPortCode);
			AssertEquals(new DateTime(2012, 08, 04), schedule.EV_ETD);
			AssertEquals(ZDateTime.Empty, schedule.EV_ETA);
			AssertEquals(new DateTime(2012, 08, 07), schedule.EV_CargoCuttOff);
			AssertEquals(new DateTime(2012, 08, 09), schedule.EV_ActualDeparture);
			AssertEquals(new DateTime(2012, 08, 11), schedule.EV_ExportReceivalCommencementDate);
			AssertEquals("ID2", schedule.EV_TerminalID);
			AssertEquals("ADMIRAL VASILIY PUPKIN", schedule.EV_ShipName);
			AssertEquals("235235", schedule.EV_IMOLloydsNumber);
			AssertEquals("AA4635AB", schedule.EV_ShipOperatorVoyageOut);
			AssertEquals(ZString.Empty, schedule.EV_ShipOperatorVoyageIn);
			AssertEquals("depRef2", schedule.EV_DataProviderReference);
			AssertEquals("HEEEEELP!!", schedule.EV_RadioCallSign);
			AssertEquals(FreightConstants.VesselDataProviders.DAKOSY, schedule.EV_DataProvider);
			AssertEquals("XYZ", schedule.EV_LineOperator);
			AssertEquals("XYZ COMPANY", schedule.EV_OperatorsDescription);
		}

		public void TestImportUniversalSchedulesWithSameCarrierAndVoyageNumberDifferentLloydsNumber()
		{
			AssertEquals(0, Factory.GetDatabaseCount(typeof(JobVesselRouting)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(JobVesselSchedule)));

			var importer = new UniversalScheduleImporter(Factory, new TestErrorLogger());
			var carrier1 = GetOrganizationAddress("15 Test St", "ABC", "ABC COMPANY");
			var universalSchedule1 = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, false, "235235", "ADMIRAL VASILIY PUPKIN", "AA4635AB", "HEEEEELP!!", carrier1);
			universalSchedule1.DischargeCollection.Add(GetDischargePort("AUSYD", new DateTime(2012, 09, 04), new DateTime(2012, 09, 05), new DateTime(2012, 09, 06), new DateTime(2012, 09, 11)));
			universalSchedule1.LoadingCollection.Add(GetLoadPort("NLRTM", new DateTime(2012, 08, 04), new DateTime(2012, 08, 07), new DateTime(2012, 08, 09), "ID2", new DateTime(2012, 08, 11), "depRef2"));

			var universalSchedule2 = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, false, "235236", "ADMIRAL VASILIY PUPKIN", "AA4635AB", "HEEEEELP!!", carrier1);
			universalSchedule2.DischargeCollection.Add(GetDischargePort("AUSYD", new DateTime(2012, 09, 05), new DateTime(2012, 09, 06), new DateTime(2012, 09, 07), new DateTime(2012, 09, 12)));
			universalSchedule2.LoadingCollection.Add(GetLoadPort("NLRTM", new DateTime(2012, 08, 05), new DateTime(2012, 08, 07), new DateTime(2012, 08, 08), "ID2", new DateTime(2012, 08, 12), "depRef2"));

			AssertEquals(MessageStatus.Processed, importer.ImportUniversalSchedule(universalSchedule1));
			AssertEquals(MessageStatus.Processed, importer.ImportUniversalSchedule(universalSchedule2));

			Factory.Save();

			AssertEquals(4, Factory.GetDatabaseCount(typeof(JobVesselSchedule)));

			var schedules = Factory.Load<JobVesselSchedule>(new ZQuery()).OrderBy(r => r.EV_RL_NKPortCode).ThenBy(r => r.EV_IMOLloydsNumber).ToList();

			var schedule = schedules[0];
			AssertEquals("AUSYD", schedule.EV_RL_NKPortCode);
			AssertEquals(new DateTime(2012, 09, 04), schedule.EV_ETA);
			AssertEquals(ZDateTime.Empty, schedule.EV_ETD);
			AssertEquals(new DateTime(2012, 09, 05), schedule.EV_ActualArrival);
			AssertEquals(new DateTime(2012, 09, 06), schedule.EV_ImportAvailability);
			AssertEquals(new DateTime(2012, 09, 11), schedule.EV_ImportStorageCommences);
			AssertEquals("ADMIRAL VASILIY PUPKIN", schedule.EV_ShipName);
			AssertEquals("235235", schedule.EV_IMOLloydsNumber);
			AssertEquals("AA4635AB", schedule.EV_ShipOperatorVoyageIn);
			AssertEquals(ZString.Empty, schedule.EV_ShipOperatorVoyageOut);
			AssertEquals(ZString.Empty, schedule.EV_DataProviderReference);
			AssertEquals("HEEEEELP!!", schedule.EV_RadioCallSign);
			AssertEquals(FreightConstants.VesselDataProviders.DAKOSY, schedule.EV_DataProvider);
			AssertEquals("ABC", schedule.EV_LineOperator);
			AssertEquals("ABC COMPANY", schedule.EV_OperatorsDescription);

			schedule = schedules[1];
			AssertEquals("AUSYD", schedule.EV_RL_NKPortCode);
			AssertEquals(new DateTime(2012, 09, 05), schedule.EV_ETA);
			AssertEquals(ZDateTime.Empty, schedule.EV_ETD);
			AssertEquals(new DateTime(2012, 09, 06), schedule.EV_ActualArrival);
			AssertEquals(new DateTime(2012, 09, 07), schedule.EV_ImportAvailability);
			AssertEquals(new DateTime(2012, 09, 12), schedule.EV_ImportStorageCommences);
			AssertEquals("ADMIRAL VASILIY PUPKIN", schedule.EV_ShipName);
			AssertEquals("235236", schedule.EV_IMOLloydsNumber);
			AssertEquals("AA4635AB", schedule.EV_ShipOperatorVoyageIn);
			AssertEquals(ZString.Empty, schedule.EV_ShipOperatorVoyageOut);
			AssertEquals(ZString.Empty, schedule.EV_DataProviderReference);
			AssertEquals("HEEEEELP!!", schedule.EV_RadioCallSign);
			AssertEquals(FreightConstants.VesselDataProviders.DAKOSY, schedule.EV_DataProvider);
			AssertEquals("ABC", schedule.EV_LineOperator);
			AssertEquals("ABC COMPANY", schedule.EV_OperatorsDescription);

			schedule = schedules[2];
			AssertEquals("NLRTM", schedule.EV_RL_NKPortCode);
			AssertEquals(new DateTime(2012, 08, 04), schedule.EV_ETD);
			AssertEquals(ZDateTime.Empty, schedule.EV_ETA);
			AssertEquals(new DateTime(2012, 08, 07), schedule.EV_CargoCuttOff);
			AssertEquals(new DateTime(2012, 08, 09), schedule.EV_ActualDeparture);
			AssertEquals(new DateTime(2012, 08, 11), schedule.EV_ExportReceivalCommencementDate);
			AssertEquals("ID2", schedule.EV_TerminalID);
			AssertEquals("ADMIRAL VASILIY PUPKIN", schedule.EV_ShipName);
			AssertEquals("235235", schedule.EV_IMOLloydsNumber);
			AssertEquals("AA4635AB", schedule.EV_ShipOperatorVoyageOut);
			AssertEquals(ZString.Empty, schedule.EV_ShipOperatorVoyageIn);
			AssertEquals("depRef2", schedule.EV_DataProviderReference);
			AssertEquals("HEEEEELP!!", schedule.EV_RadioCallSign);
			AssertEquals(FreightConstants.VesselDataProviders.DAKOSY, schedule.EV_DataProvider);
			AssertEquals("ABC", schedule.EV_LineOperator);
			AssertEquals("ABC COMPANY", schedule.EV_OperatorsDescription);

			schedule = schedules[3];
			AssertEquals("NLRTM", schedule.EV_RL_NKPortCode);
			AssertEquals(new DateTime(2012, 08, 05), schedule.EV_ETD);
			AssertEquals(ZDateTime.Empty, schedule.EV_ETA);
			AssertEquals(new DateTime(2012, 08, 07), schedule.EV_CargoCuttOff);
			AssertEquals(new DateTime(2012, 08, 08), schedule.EV_ActualDeparture);
			AssertEquals(new DateTime(2012, 08, 12), schedule.EV_ExportReceivalCommencementDate);
			AssertEquals("ID2", schedule.EV_TerminalID);
			AssertEquals("ADMIRAL VASILIY PUPKIN", schedule.EV_ShipName);
			AssertEquals("235236", schedule.EV_IMOLloydsNumber);
			AssertEquals("AA4635AB", schedule.EV_ShipOperatorVoyageOut);
			AssertEquals(ZString.Empty, schedule.EV_ShipOperatorVoyageIn);
			AssertEquals("depRef2", schedule.EV_DataProviderReference);
			AssertEquals("HEEEEELP!!", schedule.EV_RadioCallSign);
			AssertEquals(FreightConstants.VesselDataProviders.DAKOSY, schedule.EV_DataProvider);
			AssertEquals("ABC", schedule.EV_LineOperator);
			AssertEquals("ABC COMPANY", schedule.EV_OperatorsDescription);
		}

		public void TestImportUniversalSchedulesWithSameCarrierAndVoyageNumbersDifferentVesselNames()
		{
			AssertEquals(0, Factory.GetDatabaseCount(typeof(JobVesselRouting)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(JobVesselSchedule)));

			var importer = new UniversalScheduleImporter(Factory, new TestErrorLogger());
			var carrier1 = GetOrganizationAddress("15 Test St", "ABC", "ABC COMPANY");
			var universalSchedule1 = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, false, "", "ADMIRAL VASILIY PUPKIN", "AA4635AB", "HEEEEELP!!", carrier1);
			universalSchedule1.DischargeCollection.Add(GetDischargePort("AUSYD", new DateTime(2012, 09, 04), new DateTime(2012, 09, 05), new DateTime(2012, 09, 06), new DateTime(2012, 09, 11)));
			universalSchedule1.LoadingCollection.Add(GetLoadPort("NLRTM", new DateTime(2012, 08, 04), new DateTime(2012, 08, 07), new DateTime(2012, 08, 09), "ID2", new DateTime(2012, 08, 11), "depRef2"));

			var universalSchedule2 = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, false, "", "PUMPKIN PATCH", "AA4635AB", "HEEEEELP!!", carrier1);
			universalSchedule2.DischargeCollection.Add(GetDischargePort("AUSYD", new DateTime(2012, 09, 05), new DateTime(2012, 09, 06), new DateTime(2012, 09, 07), new DateTime(2012, 09, 12)));
			universalSchedule2.LoadingCollection.Add(GetLoadPort("NLRTM", new DateTime(2012, 08, 05), new DateTime(2012, 08, 07), new DateTime(2012, 08, 08), "ID2", new DateTime(2012, 08, 12), "depRef2"));

			AssertEquals(MessageStatus.Processed, importer.ImportUniversalSchedule(universalSchedule1));
			AssertEquals(MessageStatus.Processed, importer.ImportUniversalSchedule(universalSchedule2));

			Factory.Save();

			AssertEquals(4, Factory.GetDatabaseCount(typeof(JobVesselSchedule)));

			var schedules = Factory.Load<JobVesselSchedule>(new ZQuery()).OrderBy(r => r.EV_RL_NKPortCode).ThenBy(r => r.EV_VesselCode).ToList();

			var schedule = schedules[0];
			AssertEquals("AUSYD", schedule.EV_RL_NKPortCode);
			AssertEquals(new DateTime(2012, 09, 04), schedule.EV_ETA);
			AssertEquals(ZDateTime.Empty, schedule.EV_ETD);
			AssertEquals(new DateTime(2012, 09, 05), schedule.EV_ActualArrival);
			AssertEquals(new DateTime(2012, 09, 06), schedule.EV_ImportAvailability);
			AssertEquals(new DateTime(2012, 09, 11), schedule.EV_ImportStorageCommences);
			AssertEquals("ADMIRAL VASILIY PUPKIN", schedule.EV_ShipName);
			AssertEquals("", schedule.EV_IMOLloydsNumber);
			AssertEquals("AA4635AB", schedule.EV_ShipOperatorVoyageIn);
			AssertEquals(ZString.Empty, schedule.EV_ShipOperatorVoyageOut);
			AssertEquals(ZString.Empty, schedule.EV_DataProviderReference);
			AssertEquals("HEEEEELP!!", schedule.EV_RadioCallSign);
			AssertEquals(FreightConstants.VesselDataProviders.DAKOSY, schedule.EV_DataProvider);
			AssertEquals("ABC", schedule.EV_LineOperator);
			AssertEquals("ABC COMPANY", schedule.EV_OperatorsDescription);

			schedule = schedules[1];
			AssertEquals("AUSYD", schedule.EV_RL_NKPortCode);
			AssertEquals(new DateTime(2012, 09, 05), schedule.EV_ETA);
			AssertEquals(ZDateTime.Empty, schedule.EV_ETD);
			AssertEquals(new DateTime(2012, 09, 06), schedule.EV_ActualArrival);
			AssertEquals(new DateTime(2012, 09, 07), schedule.EV_ImportAvailability);
			AssertEquals(new DateTime(2012, 09, 12), schedule.EV_ImportStorageCommences);
			AssertEquals("PUMPKIN PATCH", schedule.EV_ShipName);
			AssertEquals("", schedule.EV_IMOLloydsNumber);
			AssertEquals("AA4635AB", schedule.EV_ShipOperatorVoyageIn);
			AssertEquals(ZString.Empty, schedule.EV_ShipOperatorVoyageOut);
			AssertEquals(ZString.Empty, schedule.EV_DataProviderReference);
			AssertEquals("HEEEEELP!!", schedule.EV_RadioCallSign);
			AssertEquals(FreightConstants.VesselDataProviders.DAKOSY, schedule.EV_DataProvider);
			AssertEquals("ABC", schedule.EV_LineOperator);
			AssertEquals("ABC COMPANY", schedule.EV_OperatorsDescription);

			schedule = schedules[2];
			AssertEquals("NLRTM", schedule.EV_RL_NKPortCode);
			AssertEquals(new DateTime(2012, 08, 04), schedule.EV_ETD);
			AssertEquals(ZDateTime.Empty, schedule.EV_ETA);
			AssertEquals(new DateTime(2012, 08, 07), schedule.EV_CargoCuttOff);
			AssertEquals(new DateTime(2012, 08, 09), schedule.EV_ActualDeparture);
			AssertEquals(new DateTime(2012, 08, 11), schedule.EV_ExportReceivalCommencementDate);
			AssertEquals("ID2", schedule.EV_TerminalID);
			AssertEquals("ADMIRAL VASILIY PUPKIN", schedule.EV_ShipName);
			AssertEquals("", schedule.EV_IMOLloydsNumber);
			AssertEquals("AA4635AB", schedule.EV_ShipOperatorVoyageOut);
			AssertEquals(ZString.Empty, schedule.EV_ShipOperatorVoyageIn);
			AssertEquals("depRef2", schedule.EV_DataProviderReference);
			AssertEquals("HEEEEELP!!", schedule.EV_RadioCallSign);
			AssertEquals(FreightConstants.VesselDataProviders.DAKOSY, schedule.EV_DataProvider);
			AssertEquals("ABC", schedule.EV_LineOperator);
			AssertEquals("ABC COMPANY", schedule.EV_OperatorsDescription);

			schedule = schedules[3];
			AssertEquals("NLRTM", schedule.EV_RL_NKPortCode);
			AssertEquals(new DateTime(2012, 08, 05), schedule.EV_ETD);
			AssertEquals(ZDateTime.Empty, schedule.EV_ETA);
			AssertEquals(new DateTime(2012, 08, 07), schedule.EV_CargoCuttOff);
			AssertEquals(new DateTime(2012, 08, 08), schedule.EV_ActualDeparture);
			AssertEquals(new DateTime(2012, 08, 12), schedule.EV_ExportReceivalCommencementDate);
			AssertEquals("ID2", schedule.EV_TerminalID);
			AssertEquals("PUMPKIN PATCH", schedule.EV_ShipName);
			AssertEquals("", schedule.EV_IMOLloydsNumber);
			AssertEquals("AA4635AB", schedule.EV_ShipOperatorVoyageOut);
			AssertEquals(ZString.Empty, schedule.EV_ShipOperatorVoyageIn);
			AssertEquals("depRef2", schedule.EV_DataProviderReference);
			AssertEquals("HEEEEELP!!", schedule.EV_RadioCallSign);
			AssertEquals(FreightConstants.VesselDataProviders.DAKOSY, schedule.EV_DataProvider);
			AssertEquals("ABC", schedule.EV_LineOperator);
			AssertEquals("ABC COMPANY", schedule.EV_OperatorsDescription);
		}

		public void TestImportUniversalScheduleWithEmptyVoyageOrVesselDetails()
		{
			var logger = new TestErrorLogger();
			var importer = new UniversalScheduleImporter(Factory, logger);
			var universalSchedule = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, false, "235235", "CAPTAIN DUDE", null, "HEEEEELP!!");

			AssertExceptionThrown<DataObjectReadFailureException>("Voyage Number is empty.", () => importer.ImportUniversalSchedule(universalSchedule));
			logger.ClearLogs();

			universalSchedule = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, false, null, null, "AA4635AB", "HEEEEELP!!");
			AssertExceptionThrown<DataObjectReadFailureException>("Vessel Name and Lloyds Number are empty.", () => importer.ImportUniversalSchedule(universalSchedule));
			logger.ClearLogs();

			universalSchedule = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, false, null, "VANCOUVER", "AA4635AB", "HEEEEELP!!");
			AssertEquals(MessageStatus.Processed, importer.ImportUniversalSchedule(universalSchedule));
			AssertEquals(false, logger.HasErrors);

			logger.ClearLogs();

			universalSchedule = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, false, null, null, null, "HEEEEELP!!");
			AssertExceptionThrown<DataObjectReadFailureException>(@"Voyage Number is empty.
Vessel Name and Lloyds Number are empty.", () => importer.ImportUniversalSchedule(universalSchedule));
		}

		public void TestImportUniversalSchedule_DeleteNonIncludedPorts()
		{
			var schedule1 = Factory.New<JobVesselSchedule>();
			schedule1.EV_DataProvider = FreightConstants.VesselDataProviders.DAKOSY;
			schedule1.EV_RL_NKPortCode = "AUSYD";
			schedule1.EV_ShipOperatorVoyageIn = "AA4635AB";
			schedule1.EV_ShipName = "Pablo Escobear";
			schedule1.EV_LineOperator = "ABC";
			schedule1.EV_IMOLloydsNumber = "LLOY1";

			var schedule2 = Factory.New<JobVesselSchedule>();
			schedule2.EV_DataProvider = FreightConstants.VesselDataProviders.DAKOSY;
			schedule2.EV_RL_NKPortCode = "AUBNE";
			schedule2.EV_ShipOperatorVoyageIn = "AA4635AB";
			schedule2.EV_ShipName = "Pablo Escobear";
			schedule2.EV_LineOperator = "ABC";
			schedule2.EV_IMOLloydsNumber = "LLOY2";

			Factory.Save();

			AssertEquals("Precondition: Should start with 2 JobVesselSchedule.", 2, Factory.GetDatabaseCount(typeof(JobVesselSchedule)));

			var carrier = GetOrganizationAddress("158 Test St", "ABC", "ABC Company");
			var universalSchedule = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, true, "", "Pablo Escobear", "AA4635AB", "HEEEEELP!!", carrier);
			universalSchedule.DischargeCollection.Add(GetDischargePort("AUSYD", new DateTime(2018, 02, 04), new DateTime(2018, 02, 05), new DateTime(2018, 02, 06), new DateTime(2018, 02, 11)));
			universalSchedule.SetLoadingCollection(() => null);
			universalSchedule.IsCancellation = false;

			var importer = new UniversalScheduleImporter(Factory, new TestErrorLogger());
			importer.ImportUniversalSchedule(universalSchedule);

			Factory.Save();

			var sydSchedule = Factory.Load<JobVesselSchedule>(schedule1.PK);
			var bneSchedule = Factory.Load<JobVesselSchedule>(schedule2.PK);

			AssertNotNull("SYD schedule should not have been deleted.", sydSchedule);
			AssertNull("BNE schedule should have been deleted.", bneSchedule);
		}

		public void TestImportUniversalSchedule_Performance()
		{
			var schedule1 = Factory.New<JobVesselSchedule>();
			schedule1.EV_DataProvider = FreightConstants.VesselDataProviders.DAKOSY;
			schedule1.EV_RL_NKPortCode = "AUSYD";
			schedule1.EV_ShipOperatorVoyageIn = "AA4635AB";
			schedule1.EV_ShipName = "Pablo Escobear";
			schedule1.EV_LineOperator = "ABC";

			var schedule2 = Factory.New<JobVesselSchedule>();
			schedule2.EV_DataProvider = FreightConstants.VesselDataProviders.DAKOSY;
			schedule2.EV_RL_NKPortCode = "AUBNE";
			schedule2.EV_ShipOperatorVoyageIn = "AA4635AB";
			schedule2.EV_ShipName = "Pablo Escobear";
			schedule2.EV_LineOperator = "ABC";

			var schedule3 = Factory.New<JobVesselSchedule>();
			schedule3.EV_DataProvider = FreightConstants.VesselDataProviders.DAKOSY;
			schedule3.EV_RL_NKPortCode = "AUMEL";
			schedule3.EV_ShipOperatorVoyageIn = "AA4635AB";
			schedule3.EV_ShipName = "Pablo Escobear";
			schedule3.EV_LineOperator = "ABC";

			var carrier = GetOrganizationAddress("158 Test St", "ABC", "ABC Company");
			var universalSchedule = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, true, "", "Pablo Escobear", "AA4635AB", "HEEEEELP!!", carrier);
			universalSchedule.DischargeCollection.Add(GetDischargePort("AUSYD", new DateTime(2018, 02, 04), new DateTime(2018, 02, 05), new DateTime(2018, 02, 06), new DateTime(2018, 02, 11)));
			universalSchedule.DischargeCollection.Add(GetDischargePort("AUBNE", new DateTime(2018, 02, 05), new DateTime(2018, 02, 06), new DateTime(2018, 02, 07), new DateTime(2018, 02, 12)));
			universalSchedule.DischargeCollection.Add(GetDischargePort("AUMEL", new DateTime(2018, 02, 05), new DateTime(2018, 02, 06), new DateTime(2018, 02, 07), new DateTime(2018, 02, 12)));
			universalSchedule.LoadingCollection.Add(GetLoadPort("AUMEL", new DateTime(2018, 02, 01), new DateTime(2018, 02, 03), new DateTime(2018, 02, 06), "ID1", new DateTime(2012, 08, 08), "depRef1"));

			var importer = new UniversalScheduleImporter(Factory, new TestErrorLogger());

			importer.ImportUniversalSchedule(universalSchedule);

			AssertTableHitCount("Processing Schedules should hit the JobVesselSchedule twice - once each for discharge and loading.",
				2, "JobVesselSchedule");
		}

		public void TestImportUniversalSchedule_InvalidETAETD()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "235235";
			vessel.RV_Name = "ADMIRAL VASILIY PUPKIN";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "AA4635AB";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NLRTM";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(-1);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			destination.JB_E_ARV = ZDateTime.Today;

			voyage.GenerateSailings();
			Factory.Save();
			AssertEquals(1, voyage.Sailings.Count);

			var importer = new UniversalScheduleImporter(Factory, new TestErrorLogger());
			var universalSchedule = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, false, "235235", "ADMIRAL VASILIY PUPKIN", "AA4635AB", "HEEEEELP!!");
			universalSchedule.DischargeCollection.Add(GetDischargePort("SGSIN", DateTime.Today, DateTime.Today, DateTime.Today, DateTime.Today));
			universalSchedule.LoadingCollection.Add(GetLoadPort("NLRTM", DateTime.Today.AddDays(1), DateTime.Today.AddDays(1), DateTime.Today.AddDays(1), "ID2", DateTime.Today.AddDays(1), "depRef2"));

			AssertExceptionThrown(typeof(DataObjectReadFailureException), () => importer.ImportUniversalSchedule(universalSchedule));
			Assert(!voyage.HasChanges);
		}

		public void TestImportUniversalSchedule_InvalidETAETD_ExistingJobVesselSchedules()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "235235";
			vessel.RV_Name = "ADMIRAL VASILIY PUPKIN";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "AA4635AB";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NLRTM";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(1);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(2);

			voyage.GenerateSailings();
			Factory.Save();
			AssertEquals(1, voyage.Sailings.Count);

			var importer = new UniversalScheduleImporter(Factory, new TestErrorLogger());
			var universalSchedule1 = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, false, "235235", "ADMIRAL VASILIY PUPKIN", "AA4635AB", "HEEEEELP!!");
			universalSchedule1.DischargeCollection.Add(GetDischargePort("SGSIN", DateTime.Today.AddDays(2), DateTime.Today.AddDays(2), DateTime.Today.AddDays(2), DateTime.Today.AddDays(2)));
			universalSchedule1.LoadingCollection.Add(GetLoadPort("NLRTM", DateTime.Today.AddDays(1), DateTime.Today.AddDays(1), DateTime.Today.AddDays(1), "ID2", DateTime.Today.AddDays(1), "depRef2"));
			voyage.GenerateSailings();
			Factory.Save();
			AssertEquals(1, voyage.Sailings.Count);

			var universalSchedule2 = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, false, "235235", "ADMIRAL VASILIY PUPKIN", "AA4635AB", "HEEEEELP!!");
			universalSchedule2.DischargeCollection.Add(GetDischargePort("SGSIN", DateTime.Today, DateTime.Today, DateTime.Today, DateTime.Today));
			universalSchedule2.LoadingCollection.Add(GetLoadPort("NLRTM", DateTime.Today.AddDays(1), DateTime.Today.AddDays(1), DateTime.Today.AddDays(1), "ID2", DateTime.Today.AddDays(1), "depRef2"));

			AssertExceptionThrown(typeof(DataObjectReadFailureException), () => importer.ImportUniversalSchedule(universalSchedule2));
			Assert(!voyage.HasChanges);
		}

		public void TestImportUniversalSchedule_ValidETAETD_MultipleOriginDestination()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "235235";
			vessel.RV_Name = "ADMIRAL VASILIY PUPKIN";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "AA4635AB";

			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "NLRTM";
			origin1.JA_E_DEP = new ZDateTime(2024, 10, 1);

			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUSYD";
			origin2.JA_E_DEP = new ZDateTime(2024, 10, 10);

			var destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AEDUJ";
			destination1.JB_E_ARV = new ZDateTime(2024, 10, 5);

			var destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "SGSIN";
			destination2.JB_E_ARV = new ZDateTime(2024, 10, 15);

			voyage.GenerateSailings();
			Factory.Save();
			AssertEquals(3, voyage.Sailings.Count);

			var importer = new UniversalScheduleImporter(Factory, new TestErrorLogger());
			var universalSchedule = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, false, "235235", "ADMIRAL VASILIY PUPKIN", "AA4635AB", "HEEEEELP!!");
			universalSchedule.DischargeCollection.Add(GetDischargePort("AEDUJ", new DateTime(2024, 10, 2), new DateTime(2024, 10, 2), new DateTime(2024, 10, 2), new DateTime(2024, 10, 2)));
			universalSchedule.LoadingCollection.Add(GetLoadPort("NLRTM", new DateTime(2024, 10, 16), new DateTime(2024, 10, 16), new DateTime(2024, 10, 16), "ID2", new DateTime(2024, 10, 16), "depRef2"));

			AssertNoExceptionThrown(() => importer.ImportUniversalSchedule(universalSchedule));
			Factory.Save();
			voyage.GenerateSailings();
			AssertEquals(1, voyage.Sailings.Count);
		}

		public void TestImportUniversalSchedule_InvalidETAETD_MultipleOriginDestination()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "235235";
			vessel.RV_Name = "ADMIRAL VASILIY PUPKIN";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "AA4635AB";

			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "NLRTM";
			origin1.JA_E_DEP = new ZDateTime(2024, 10, 1);

			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUSYD";
			origin2.JA_E_DEP = new ZDateTime(2024, 10, 10);

			var destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AEDUJ";
			destination1.JB_E_ARV = new ZDateTime(2024, 10, 5);

			var destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "SGSIN";
			destination2.JB_E_ARV = new ZDateTime(2024, 10, 15);

			voyage.GenerateSailings();
			Factory.Save();
			AssertEquals(3, voyage.Sailings.Count);

			var importer = new UniversalScheduleImporter(Factory, new TestErrorLogger());
			var universalSchedule = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, false, "235235", "ADMIRAL VASILIY PUPKIN", "AA4635AB", "HEEEEELP!!");
			universalSchedule.DischargeCollection.Add(GetDischargePort("SGSIN", new DateTime(2024, 10, 4), new DateTime(2024, 10, 4), new DateTime(2024, 10, 4), new DateTime(2024, 10, 4)));
			universalSchedule.LoadingCollection.Add(GetLoadPort("NLRTM", new DateTime(2024, 10, 6), new DateTime(2024, 10, 6), new DateTime(2024, 10, 6), "ID2", new DateTime(2024, 10, 6), "depRef2"));

			AssertExceptionThrown(typeof(DataObjectReadFailureException), () => importer.ImportUniversalSchedule(universalSchedule));
			Assert(!voyage.HasChanges);
		}

		public void TestImportUniversalSchedule_MultipleETAETD_SameOriginDestination()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "235235";
			vessel.RV_Name = "ADMIRAL VASILIY PUPKIN";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "AA4635AB";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NLRTM";
			origin.JA_E_DEP = new ZDateTime(2024, 10, 1);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AEDUJ";
			destination.JB_E_ARV = new ZDateTime(2024, 10, 5);

			voyage.GenerateSailings();
			Factory.Save();
			AssertEquals(1, voyage.Sailings.Count);

			var importer = new UniversalScheduleImporter(Factory, new TestErrorLogger());
			var universalSchedule = GetUniversalSchedule(FreightConstants.VesselDataProviders.DAKOSY, false, "235235", "ADMIRAL VASILIY PUPKIN", "AA4635AB", "HEEEEELP!!");
			universalSchedule.DischargeCollection.Add(GetDischargePort("AEDUJ", new DateTime(2024, 10, 5), new DateTime(2024, 10, 5), new DateTime(2024, 10, 2), new DateTime(2024, 10, 2)));
			universalSchedule.DischargeCollection.Add(GetDischargePort("AEDUJ", new DateTime(2024, 10, 10), new DateTime(2024, 10, 10), new DateTime(2024, 10, 2), new DateTime(2024, 10, 2)));
			universalSchedule.LoadingCollection.Add(GetLoadPort("NLRTM", new DateTime(2024, 10, 1), new DateTime(2024, 10, 1), new DateTime(2024, 10, 16), "ID2", new DateTime(2024, 10, 16), "depRef1"));
			universalSchedule.LoadingCollection.Add(GetLoadPort("NLRTM", new DateTime(2024, 10, 6), new DateTime(2024, 10, 6), new DateTime(2024, 10, 16), "ID2", new DateTime(2024, 10, 16), "depRef2"));

			AssertNoExceptionThrown(() => importer.ImportUniversalSchedule(universalSchedule));
			Factory.Save();
			voyage.GenerateSailings();
			AssertEquals(1, voyage.Sailings.Count);
		}

		#region Implementation

		#region Get Discharge Port

		Discharge GetDischargePort(string code, DateTime estimatedArrival, DateTime actualArrival, DateTime availability, DateTime storage)
		{
			var result = new Discharge();
			result.Port = new UNLOCO();
			result.Port.Code = code;
			result.EstimatedArrival = estimatedArrival;
			result.ActualArrival = actualArrival;
			result.FCLAvailability = availability;
			result.FCLStorage = storage;

			return result;
		}

		#endregion

		#region Get Load Port

		Loading GetLoadPort(string code, DateTime estimatedDeparture, DateTime cargoCutOff, DateTime actualDeparture, string terminalId, DateTime receivalCommences, string departureReference)
		{
			var result = new Loading();
			result.Port = new UNLOCO();
			result.Port.Code = code;
			result.EstimatedDeparture = estimatedDeparture;
			result.FCLCutOff = cargoCutOff;
			result.ActualDeparture = actualDeparture;
			result.TerminalCode = terminalId;
			result.FCLReceivalCommences = receivalCommences;
			result.DepartureReference = departureReference;
			return result;
		}

		#endregion

		#region Get Universal Schedule

		UniversalSchedule GetUniversalSchedule(string dataProvider, bool isCancellation, string lloydsNumber, string vesselName, string voyageNumber, string callSign)
		{
			return GetUniversalSchedule(dataProvider, isCancellation, lloydsNumber, vesselName, voyageNumber, callSign, null);
		}

		UniversalSchedule GetUniversalSchedule(string dataProvider, bool isCancellation, string lloydsNumber, string vesselName, string voyageNumber, string callSign, OrganizationAddress carrier)
		{
			var result = new UniversalSchedule(DefaultDataObjectWriterStrategy.TestInstance);
			result.DataProvider = dataProvider;
			result.IsCancellation = isCancellation;
			result.Transport = new ScheduleTransport();
			result.Transport.Sea = new Sea();
			result.Transport.Sea.Vessel = new Vessel();
			result.Transport.Sea.Vessel.LloydsNumber = lloydsNumber;
			result.Transport.Sea.Vessel.VesselName = vesselName;
			result.Transport.Sea.Vessel.CallSign = callSign;
			result.Transport.Sea.VoyageNumber = voyageNumber;
			result.SetLoadingCollection(() => new List<Loading>());
			result.SetDischargeCollection(() => new List<Discharge>());
			result.Carrier = carrier;

			return result;
		}

		#endregion

		#region Get Organization Address

		OrganizationAddress GetOrganizationAddress(ZString address, ZString orgCode, ZString companyName)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = orgCode;
			orgHeader.OH_FullName = companyName;

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = address;
			orgAddress.OA_OH = orgHeader.PK;

			Factory.Save();

			var result = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			result.Address1 = address;
			result.OrganizationCode = orgCode;
			result.CompanyName = companyName;

			return result;
		}

		#endregion

		#endregion
	}
}
