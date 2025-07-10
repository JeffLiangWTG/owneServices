using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Freight.Integration.Agency;
using static Enterprise.Integration.Customs.JP.AFR;
using Constants = Enterprise.Core.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobSailing))]
	sealed class JobSailingTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPurgeLinksOfAFRHeadersWhenDelete()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "CNSHA";
			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			var header = Factory.New<IJPAFRHeader>();
			header.JPH_ParentId = sailing.PK;
			header.JPH_ParentTableCode = sailing.TablePrefix;
			Factory.Save();

			var humanReadableName = sailing.HumanReadableName;

			sailing.Delete();
			AssertEquals(header.JPH_ParentId, Guid.Empty);
			AssertEquals(header.JPH_ParentTableCode, string.Empty);

			var log = (header as IStmALogProvider).Logs.MostRecentLogByEventTime(Events.DeletedARecordInTheSystem);
			AssertNotNull("Should create a DEL log.", log);
			AssertEquals($"Parent {humanReadableName} is deleted.", log.SL_Reference);
		}

		public void TestOnSaving_UpdateRelatedBookingsCO2eStatus()
		{
			// Arrange
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "CNSHA";
			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			sailing.SetCO2ePerTonneInKg(1m);
			sailing.SetCO2eStatus(CO2eStatusList.Codes.Current);

			var booking = Factory.New<CommonShipment>();
			booking.JS_IsBooking = true;
			booking.JS_IsForwardRegistered = false;
			booking.JS_RL_NKLoadPort = "AUSYD";
			booking.JS_RL_NKDischargePort = "CNSHA";
			booking.JS_JX = sailing.PK;
			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(booking.PK, Factory);
			((ICO2eProvider)quotedBooking).SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();

			// Act & Assert
			var newFactory = new BusinessObjectFactory();
			var newSailing = newFactory.Load<JobSailing>(sailing.PK);
			newSailing.Origin.JA_RL_NKPortOfLoading = "AUBNE";
			newFactory.Save();
			AssertEquals(CO2eStatusList.Codes.NotCurrent, newSailing.GetCO2eStatus());
			CO2eTestHelper.AssertSTUEvent(newSailing, "JA_RL_NKPortOfLoading [AUSYD]->[AUBNE]");

			var newBooking = newFactory.Load<IQuotedBooking>(booking.PK);
			AssertEquals(CO2eStatusList.Codes.NotCurrent, ((ICO2eProvider)newBooking).GetCO2eStatus());
			CO2eTestHelper.AssertSTUEvent((IStmALogParent)newBooking, "Sailing/Flight");
		}

		public void TestDeclarations()
		{
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_DeclarationReference] = "DECLA1";
			var container1 = Factory.NewWithValidTestData<CommonContainer>();
			var cusContainer1 = (Enterprise.Integration.Customs.Shared.IBaseCusContainer)((BusinessObjectCollection)declaration["CusContainers"]).AddNew();
			cusContainer1.CO_JC = container1.PK;

			var declarationTransport = Factory.NewWithValidTestData<Transport>();
			declarationTransport.ParentType = declaration.GetType();
			declarationTransport.JW_ParentGUID = declaration.PK;
			declarationTransport.JW_ParentType = Constants.TransportParentTypes.Declaration;
			declarationTransport.JW_IsLinked = true;
			declarationTransport.JW_JX = Sailing.PK;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { container1.PK }, Sailing.FreightContainers.Select(c => c.PK));

			declarationTransport.JW_IsLinked = false;
			AssertCollectionNotContains(container1.PK, Sailing.FreightContainers.Select(c => c.PK));
		}

		public void TestJX_ShowOnlyNonTranship()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];
			Factory.Save();

			sailing.JX_ShowOnlyNonTranship = true;
			AssertEquals(true, sailing.JX_ShowOnlyNonTranship);
			AssertEquals(true, sailing.UnAllocatedPackLines.ShowOnlyNonTranship);

			sailing.JX_ShowOnlyNonTranship = false;
			AssertEquals(false, sailing.JX_ShowOnlyNonTranship);
			AssertEquals(false, sailing.UnAllocatedPackLines.ShowOnlyNonTranship);

			sailing.JX_ShowOnlyNonTranship = true;
			AssertEquals(true, sailing.JX_ShowOnlyNonTranship);
			AssertEquals(true, sailing.UnAllocatedPackLines.ShowOnlyNonTranship);
		}

		public void TestJX_ShowOnlyReceived()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];
			Factory.Save();

			sailing.JX_ShowOnlyReceived = true;
			AssertEquals(true, sailing.JX_ShowOnlyReceived);
			AssertEquals(true, sailing.UnAllocatedPackLines.ShowOnlyReceived);

			sailing.JX_ShowOnlyReceived = false;
			AssertEquals(false, sailing.JX_ShowOnlyReceived);
			AssertEquals(false, sailing.UnAllocatedPackLines.ShowOnlyReceived);

			sailing.JX_ShowOnlyReceived = true;
			AssertEquals(true, sailing.JX_ShowOnlyReceived);
			AssertEquals(true, sailing.UnAllocatedPackLines.ShowOnlyReceived);
		}

		public void TestJX_ShowOnlyThisSailing()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];
			Factory.Save();

			sailing.JX_ShowOnlyThisSailing = true;
			AssertEquals(true, sailing.JX_ShowOnlyThisSailing);
			AssertEquals(true, sailing.UnAllocatedPackLines.ShowOnlyThisSailing);

			sailing.JX_ShowOnlyThisSailing = false;
			AssertEquals(false, sailing.JX_ShowOnlyThisSailing);
			AssertEquals(false, sailing.UnAllocatedPackLines.ShowOnlyThisSailing);

			sailing.JX_ShowOnlyThisSailing = true;
			AssertEquals(true, sailing.JX_ShowOnlyThisSailing);
			AssertEquals(true, sailing.UnAllocatedPackLines.ShowOnlyThisSailing);
		}

		public void TestISlotAllocationParentCode()
		{
			ISlotAllocationParent parent = Factory.New<JobSailing>();
			AssertEquals(JobSailingSchema.Constants.Prefix, parent.Code);
		}

		public void TestClone()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			Factory.Save();

			JobSailing sailing = voyage.Sailings[0];
			AssertNotEquals(ZString.Empty, sailing.JX_UniqueReference);

			SlotAllocation allocation = sailing.SlotAllocations.GetAllocation(ZGuid.Empty);
			allocation.SetAspect("AS1", 23);

			JobSailing clonedSailing = (JobSailing)voyage.Sailings[0].Clone();
			SlotAllocation allocationClone = clonedSailing.SlotAllocations.GetAllocation(ZGuid.Empty);

			AssertNotEquals(allocation.PK, allocationClone.PK);
			AssertEquals(23m, allocationClone.GetAspect("AS1"));
			AssertEquals(ZString.Empty, clonedSailing.JX_UniqueReference);
		}

		public void TestOnLoaded()
		{
			Sailing.OnLoaded();
			Assert("expected JX_JA to be readonly", Sailing.JX_JAInfo.ReadOnly);
			Assert("expected JX_JB to be readonly", Sailing.JX_JBInfo.ReadOnly);
		}

		public void TestGetVoyageBusinessObject()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			JobVoyage testVoyage = factory.New(typeof(JobVoyage)) as JobVoyage;
			JobVoyage testVoyage2 = factory.New(typeof(JobVoyage)) as JobVoyage;
			JobSailing testSailing;// = TestFactory.New(typeof(JobSailing)) as JobSailing;

			var vessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First();

			testVoyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			testVoyage.JV_VoyageFlight = "234";
			testVoyage.JV_RV_NKVessel = vessel.RV_FK;

			VoyageOrigin testOrigin1 = factory.New<VoyageOrigin>();
			testVoyage.Origins.Add(testOrigin1);
			testOrigin1.JA_JV = testVoyage.PK;
			testOrigin1.JA_E_DEP = new ZDateTime(2004, 1, 20);
			testOrigin1.JA_RL_NKPortOfLoading = "AUSYD";

			VoyageOrigin testOrigin2 = factory.New<VoyageOrigin>();
			testVoyage.Origins.Add(testOrigin2);
			testOrigin2.JA_E_DEP = new ZDateTime(2004, 1, 20);
			testOrigin2.JA_RL_NKPortOfLoading = "AUBNE";

			VoyageDestination testDestination1 = factory.New<VoyageDestination>();
			testVoyage.Destinations.Add(testDestination1);
			testDestination1.JB_E_ARV = new ZDateTime(2004, 2, 20);
			testDestination1.JB_RL_NKPortOfDischarge = "USLAX";

			VoyageDestination testDestination2 = factory.New<VoyageDestination>();
			testVoyage.Destinations.Add(testDestination2);
			testDestination2.JB_E_ARV = new ZDateTime(2004, 2, 20);
			testDestination2.JB_RL_NKPortOfDischarge = "USSFO";

			testVoyage.GenerateSailings();

			testSailing = testVoyage.Sailings[0];

			AssertEquals("Expecting voyage to have 4 sailings.", 4, testVoyage.Sailings.Count);

			JobVoyage voy = testSailing.GetVoyageBusinessObject();

			AssertEquals("correct voyage returned, no error expected", voy.PK, testVoyage.PK);
			Assert("Comparing different voyages, should not be equal", voy.PK != testVoyage2.PK);

			AssertEquals("Expecting voyage to have 2 origins.", 2, testVoyage.Origins.Count);

			AssertEquals("Expecting voyage to have 4 sailings.", 4, testVoyage.Sailings.Count);
		}

		public void TestCopyDetailToOtherSailingWithSameOrigin_ZString()
		{
			GenericTestCopyDetailToOtherSailingsWithSameOrigin(JobSailingSchema.JX_ReservedMasterBill.Name, "blah", "snth", ZString.Empty);
		}

		public void GenericTestCopyDetailToOtherSailingsWithSameOrigin(string propertyName, object value1, object value2, object emptyValue)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();

			VoyageOrigin origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUBNE";

			VoyageOrigin origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "NZAKL";

			VoyageDestination destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "NZAKL";

			VoyageDestination destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "SGSIN";

			voyage.GenerateSailings();

			JobSailing sailing1 = voyage.Sailings.GetSailingFromLoadAndDischarge(origin1.JA_RL_NKPortOfLoading, destination1.JB_RL_NKPortOfDischarge);
			JobSailing sailing2 = voyage.Sailings.GetSailingFromLoadAndDischarge(origin1.JA_RL_NKPortOfLoading, destination2.JB_RL_NKPortOfDischarge);
			JobSailing sailing3 = voyage.Sailings.GetSailingFromLoadAndDischarge(origin2.JA_RL_NKPortOfLoading, destination2.JB_RL_NKPortOfDischarge);

			ZDateTime dateRef = ZDateTime.Today;

			sailing1[propertyName] = value1;
			sailing2[propertyName] = value2;

			AssertEquals("Sailing1." + propertyName, value1, sailing1[propertyName]);
			AssertEquals("Sailing2." + propertyName, value2, sailing2[propertyName]);
			AssertEquals("Sailing3." + propertyName, emptyValue, sailing3[propertyName]);

			sailing2.CopyDetailToOtherSailingsWithSameOrigin(propertyName);

			AssertEquals("Sailing1." + propertyName, value2, sailing1[propertyName]);
			AssertEquals("Sailing2." + propertyName, value2, sailing2[propertyName]);
			AssertEquals("Sailing3." + propertyName, emptyValue, sailing3[propertyName]);
		}

		public void TestOrigin()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			JobSailing testSailing = testFactory.New(typeof(JobSailing)) as JobSailing;
			VoyageOrigin testOrigin = testFactory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageOrigin testOrigin2 = testFactory.New(typeof(VoyageOrigin)) as VoyageOrigin;

			testSailing.JX_JA = testOrigin.PK;

			AssertEquals("TestSailing.JX_JA and TestOrigin are equal, no error expected", testSailing.Origin.PK, testSailing.JX_JA);
			Assert("TestSailing.JX_JA and TestOrigin2 are not equal, error expected", testSailing.Origin.PK != testOrigin2.PK);
			Assert("TestSailing.JX_JA and TestOrigin2 are not equal, error expected", testSailing.JX_JA != testOrigin2.PK);
		}

		public void TestDestination()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			JobSailing testSailing = testFactory.New(typeof(JobSailing)) as JobSailing;
			VoyageDestination testDestination = testFactory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageDestination testDestination2 = testFactory.New(typeof(VoyageDestination)) as VoyageDestination;

			testSailing.JX_JB = testDestination.PK;

			AssertEquals("TestSailing.JX_JB and TestDestination are equal, no error expected", testSailing.Destination.PK, testSailing.JX_JB);
			Assert("TestSailing.JX_JA and TestDestination2 are not equal, error expected", testSailing.Destination.PK != testDestination2.PK);
			Assert("TestSailing.JX_JA and TestDestination2 are not equal, error expected", testSailing.JX_JA != testDestination2.PK);
		}

		public void TestVoyage()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			JobVoyage testVoyage = testFactory.New(typeof(JobVoyage)) as JobVoyage;
			JobVoyage testVoyage2 = testFactory.New(typeof(JobVoyage)) as JobVoyage;

			VoyageDestination testDestination = testFactory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageDestination testDestination2 = testFactory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageOrigin testOrigin = testFactory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageOrigin testOrigin2 = testFactory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			JobSailing testSailing = testFactory.New(typeof(JobSailing)) as JobSailing;

			testSailing.JX_JA = testOrigin.PK;
			testSailing.JX_JB = testDestination.PK;

			testDestination.JB_JV = testVoyage.PK;
			testOrigin.JA_JV = testVoyage.PK;

			JobVoyage voy = testSailing.Voyage;

			AssertEquals("correct voyage returned, no error expected", testVoyage.PK, voy.PK);
			Assert("Comparing different voyages, should not be equal", voy.PK != testVoyage2.PK);
		}

		public void TestAllBookingsOnSailing()
		{
			JobSailing sailing = (JobSailing)GetNewBusinessObject();

			CommonShipment s1 = Factory.New<CommonShipment>();
			s1.JS_JX = sailing.PK;
			s1.JS_PackingMode = Constants.ContainerModes.FCL;
			s1.JS_IsBooking = true;
			s1.JS_IsForwardRegistered = false;

			CommonShipment s2 = Factory.New<CommonShipment>();
			s2.JS_JX = sailing.PK;
			s2.JS_IsBooking = true;
			s2.JS_IsForwardRegistered = false;

			CommonShipment s3 = Factory.New<CommonShipment>();
			s3.JS_JX = sailing.PK;
			s3.JS_IsForwardRegistered = true;

			CommonShipment s4 = Factory.New<CommonShipment>();
			s4.JS_JX = sailing.PK;
			s4.JS_IsCancelled = true;
			s4.JS_IsBooking = true;
			s4.JS_IsForwardRegistered = false;

			CommonShipment s5 = Factory.New<CommonShipment>();
			s5.JS_JX = sailing.PK;
			s5.JS_IsBooking = true;
			s5.JS_IsForwardRegistered = false;

			Factory.Save();
			AssertEquals(3, sailing.AllBookingsOnSailing.Count);
		}

		public void TestScheduleDateChangeLogged()
		{
			Sailing.Factory.Save();
			Sailing.Destination.JB_AvailabilityDate = new ZDateTime(2000, 1, 1);
			Factory.Save();
			JobScheduleChangeLoggerTest.AssertLastDateChangeLogged(Factory, ScheduleDateTypes.Codes.FCLAvailable, ZDateTime.Empty, Sailing.JX_JB_CTOAvailabilityDate);
		}

		public void TestHumanReadableName()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			VoyageDestination destination = voyage.Destinations.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUMEL";
			destination.JB_RL_NKPortOfDischarge = "MYPKG";
			JobSailing sailing = voyage.Sailings.AddNew();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			AssertEquals("Flight schedule", "Flight Port Pair (Load='AUMEL' Discharge='MYPKG')", sailing.HumanReadableName);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			AssertEquals("Sailing schedule", "Sailing Port Pair (Load='AUMEL' Discharge='MYPKG')", sailing.HumanReadableName);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Road;
			AssertEquals("Trucking journey", "Trucking Port Pair (Load='AUMEL' Discharge='MYPKG')", sailing.HumanReadableName);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Rail;
			AssertEquals("Rail journey", "Rail Port Pair (Load='AUMEL' Discharge='MYPKG')", sailing.HumanReadableName);

			voyage.JV_AirSeaRoad = "xxx";
			AssertEquals("Default for when no transport mode", "Port Pair (Load='AUMEL' Discharge='MYPKG')", sailing.HumanReadableName);
		}

		public void TestEditLogRaisedOnJobVoyageOnSave()
		{
			Sailing.Factory.Save();
			AssertEquals("No edit log initially", null, Voyage.Logs.MostRecentLogByEventTime(Events.EditedARecord));

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobSailing loadedSailing = newFactory.Load<JobSailing>(Sailing.PK);
			loadedSailing.Destination.JB_AvailabilityDate = ZDateTime.Now;
			newFactory.Save();
			AssertNotNull("Edit log raised on edit of JobSailing", loadedSailing.Voyage.Logs.MostRecentLogByEventTime(Events.EditedARecord));
		}

		public void TestLCLCutOff()
		{
			JobVoyage testVoyage = Factory.New(typeof(JobVoyage)) as JobVoyage;
			JobVoyage testVoyage2 = Factory.New(typeof(JobVoyage)) as JobVoyage;
			JobSailing sydSanSailing = Factory.New(typeof(JobSailing)) as JobSailing;
			JobSailing sydLaxSailing = Factory.New(typeof(JobSailing)) as JobSailing;

			var vessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First();

			testVoyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			testVoyage.JV_VoyageFlight = "234";
			testVoyage.JV_RV_NKVessel = vessel.RV_FK;

			VoyageOrigin testOrigin1 = Factory.New<VoyageOrigin>();
			testVoyage.Origins.Add(testOrigin1);
			testOrigin1.JA_JV = testVoyage.PK;
			testOrigin1.JA_E_DEP = new ZDateTime(2004, 1, 20);
			testOrigin1.JA_RL_NKPortOfLoading = "AUSYD";

			VoyageOrigin testOrigin2 = Factory.New<VoyageOrigin>();
			testVoyage.Origins.Add(testOrigin2);
			testOrigin2.JA_E_DEP = new ZDateTime(2004, 1, 20);
			testOrigin2.JA_RL_NKPortOfLoading = "AUBNE";

			VoyageDestination testDestination1 = Factory.New<VoyageDestination>();
			testVoyage.Destinations.Add(testDestination1);
			testDestination1.JB_E_ARV = new ZDateTime(2004, 2, 20);
			testDestination1.JB_RL_NKPortOfDischarge = "USLAX";

			VoyageDestination testDestination2 = Factory.New<VoyageDestination>();
			testVoyage.Destinations.Add(testDestination2);
			testDestination2.JB_E_ARV = new ZDateTime(2004, 2, 20);
			testDestination2.JB_RL_NKPortOfDischarge = "USSFO";

			testVoyage.GenerateSailings();

			foreach (JobSailing sailing in testVoyage.Sailings)
			{
				if ((sailing.Origin.JA_RL_NKPortOfLoading == "AUSYD")
					&& (sailing.Destination.JB_RL_NKPortOfDischarge == "USSFO"))
				{
					sydSanSailing = sailing;
				}
				else if ((sailing.Origin.JA_RL_NKPortOfLoading == "AUSYD")
					&& (sailing.Destination.JB_RL_NKPortOfDischarge == "USLAX"))
				{
					sydLaxSailing = sailing;
				}
			}

			sydSanSailing.JX_IsPublished = true;
			sydSanSailing.JX_DepotCutOff = new ZDateTime(2004, 1, 17);

			AssertEquals("Expecting CFS cut off date to copy to other sailings.", new ZDateTime(2004, 1, 17), sydLaxSailing.JX_DepotCutOff);

			JobSailingCollection gridSailings = new JobSailingCollection(Factory);
			for (int i = 0; i < testVoyage.Sailings.Count; i++)
			{
				gridSailings.AddNew();
			}

			AssertEquals(4, testVoyage.Sailings.Count);
			AssertEquals(4, gridSailings.Count);

			for (int i = 0; i < gridSailings.Count; i++)
			{
				AssertEquals("Not expecting sailing to have changes.", false, gridSailings[i].HasChanges);
			}

			gridSailings[0].CopyPersistentValuesFromForTest(sydLaxSailing);

			AssertEquals("Expecting sailing to have changes.", true, gridSailings[0].HasChanges);

			for (int i = 1; i < gridSailings.Count; i++)
			{
				AssertEquals("Not expecting sailing to have changes.", false, gridSailings[i].HasChanges);
			}
		}

		public void TestInvalidDates()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Enterprise.Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_FK;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];
			sailing.JX_DepotReceivalCommences = ZDateTime.Invalid;
			AssertEquals("JX_DepotReceivalCommences", ZDateTime.Invalid, sailing.JX_DepotReceivalCommences);
		}

		public void TestDates_DateTimeKind_Unspecified()
		{
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Sailing.JX_DepotReceivalCommences.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Sailing.JX_DepotCutOff.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Sailing.JX_DepotAvailabilityDate.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Sailing.JX_DepotStorageDate.Kind);
		}

		public void TestVoyageNumber()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			JobVoyage testVoyage = testFactory.New(typeof(JobVoyage)) as JobVoyage;
			VoyageDestination testDestination = testFactory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageOrigin testOrigin = testFactory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			JobSailing testSailing = testFactory.New(typeof(JobSailing)) as JobSailing;
			testSailing.JX_JA = testOrigin.PK;
			testSailing.JX_JB = testDestination.PK;
			testDestination.JB_JV = testVoyage.PK;
			testOrigin.JA_JV = testVoyage.PK;

			AssertEquals("Voyage Number should be blank", ZString.Empty, testSailing.JX_JV_VoyageFlight);

			testVoyage.JV_VoyageFlight = "12345";
			AssertEquals("Voyage number should be 12345", "12345", testSailing.JX_JV_VoyageFlight);

			testVoyage.JV_VoyageFlight = "123456";
			AssertEquals("different voyage number, voyage number should be 123456", "123456", testSailing.JX_JV_VoyageFlight);
		}

		public void TestJX_JA_E_DEP()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			JobVoyage testVoyage = testFactory.New(typeof(JobVoyage)) as JobVoyage;
			VoyageDestination testDestination = testFactory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageOrigin testOrigin = testFactory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			JobSailing testSailing = testFactory.New(typeof(JobSailing)) as JobSailing;
			testDestination.JB_JV = testVoyage.PK;
			testOrigin.JA_JV = testVoyage.PK;
			testSailing.JX_JA = testOrigin.PK;
			testSailing.JX_JB = testDestination.PK;

			AssertEquals("Departure date should be blank", testSailing.JX_JA_E_DEP, ZDateTime.Empty);

			testOrigin.JA_E_DEP = new ZDateTime(2003, 11, 27);
			AssertEquals("Departure date should be 27/11/2003", new ZDateTime(2003, 11, 27), testSailing.JX_JA_E_DEP);

			testOrigin.JA_E_DEP = new ZDateTime(2003, 11, 28);
			AssertEquals("Different date, Departure date should be 28/11/2003", new ZDateTime(2003, 11, 28), testSailing.JX_JA_E_DEP);
		}

		public void TestJX_JB_E_ARV()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			JobVoyage testVoyage = testFactory.New(typeof(JobVoyage)) as JobVoyage;
			VoyageDestination testDestination = testFactory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageOrigin testOrigin = testFactory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			JobSailing testSailing = testFactory.New(typeof(JobSailing)) as JobSailing;
			testDestination.JB_JV = testVoyage.PK;
			testOrigin.JA_JV = testVoyage.PK;
			testSailing.JX_JA = testOrigin.PK;
			testSailing.JX_JB = testDestination.PK;

			AssertEquals("Arrival date should be blank", testSailing.JX_JB_E_ARV, ZDateTime.Empty);

			testDestination.JB_E_ARV = new ZDateTime(2003, 11, 27);
			AssertEquals("Arrival date should be 27/11/2003", new ZDateTime(2003, 11, 27), testSailing.JX_JB_E_ARV);

			testDestination.JB_E_ARV = new ZDateTime(2003, 11, 28);
			AssertEquals("Different date, Arrival date should 28/11/2003", new ZDateTime(2003, 11, 28), testSailing.JX_JB_E_ARV);
		}

		public void TestJX_JA_CallsScheduleDataVendor()
		{
			MockSailingScheduleDataVendor.RegisterThisSubTypeOverride();
			try
			{
				Sailing.JX_JA = ZGuid.Empty;
				Sailing.JX_JA = Origin.PK;
				AssertEquals("Expected SailingScheduleDataVendor.UpdateVoyageOrigin to be called", true, MockSailingScheduleDataVendor.Instance.UpdateVoyageOriginCalled);
			}
			finally
			{
				MockSailingScheduleDataVendor.UnregisterThisSubTypeOverride();
			}
		}

		public void TestJX_JB_CallsScheduleDataVendor()
		{
			MockSailingScheduleDataVendor.RegisterThisSubTypeOverride();
			try
			{
				Sailing.JX_JB = ZGuid.Empty;
				Sailing.JX_JB = Destination.PK;
				AssertEquals("Expected SailingScheduleDataVendor.UpdateVoyageDestination to be called", true, MockSailingScheduleDataVendor.Instance.UpdateVoyageDestinationCalled);
			}
			finally
			{
				MockSailingScheduleDataVendor.UnregisterThisSubTypeOverride();
			}
		}

		public void TestPropergatesPortOfLoadingChange()
		{
			Voyage = Factory.New<JobVoyage>();

			Origin = Voyage.Origins.AddNew();

			Destination = Voyage.Destinations.AddNew();

			JobSailing sailing = Voyage.Sailings.AddNew();
			sailing.JX_JA = Origin.PK;
			sailing.JX_JB = Destination.PK;
			sailing.JX_JA_RL_NKPortOfLoadingInfo.ValueChanged += new EventHandler(JX_JA_RL_NKPortOfLoadingInfo_ValueChanged);

			JX_JA_RL_NKPortOfLoadingInfo_ChangeCount = 0;
			Origin.JA_RL_NKPortOfLoading = "AUBNE";
			AssertEquals("Value changed should have been fired once", 1, JX_JA_RL_NKPortOfLoadingInfo_ChangeCount);

			Origin2 = Voyage.Origins.AddNew();
			JX_JA_RL_NKPortOfLoadingInfo_ChangeCount = 0;
			sailing.JX_JA = Origin2.PK;
			AssertEquals("Value changed should have been fired again", 1, JX_JA_RL_NKPortOfLoadingInfo_ChangeCount);

			JX_JA_RL_NKPortOfLoadingInfo_ChangeCount = 0;
			Origin.JA_RL_NKPortOfLoading = "AUBNE";
			AssertEquals("Value changed should not have been fired", 0, JX_JA_RL_NKPortOfLoadingInfo_ChangeCount);

			JX_JA_RL_NKPortOfLoadingInfo_ChangeCount = 0;
			Origin2.JA_RL_NKPortOfLoading = "AUBNE";
			AssertEquals("Value changed should have been fired once (Origin2)", 1, JX_JA_RL_NKPortOfLoadingInfo_ChangeCount);
		}

		public void TestTotalWeight()
		{
			SetupTestTotals();

			Shipment1.JS_ActualWeight = new ZDecimal(2.3);
			Shipment2.JS_ActualWeight = new ZDecimal(56.2);
			Shipment4.JS_ActualWeight = new ZDecimal(14.0);

			AssertEquals(new ZDecimal(58.5), Sailing.TotalWeight);
		}

		public void TestTotalVolume()
		{
			SetupTestTotals();
			Shipment1.JS_ActualVolume = new ZDecimal(5.2);
			Shipment2.JS_ActualVolume = new ZDecimal(5);
			Shipment4.JS_ActualVolume = new ZDecimal(4.8);

			AssertEquals(new ZDecimal(10.2), Sailing.TotalVolume);
		}

		public void TestReceivedPackages()
		{
			SetupTestTotals();
			Shipment1.JS_OuterPacks = new ZInt(3);
			Shipment2.JS_OuterPacks = new ZInt(5);
			Shipment3.JS_OuterPacks = new ZInt(4);

			Shipment1.JS_A_RCV = new ZDateTime(2003, 12, 19);
			Shipment1.JS_InterimReceipt = ZString.Empty;
			Shipment2.JS_A_RCV = ZDateTime.Empty;
			Shipment2.JS_InterimReceipt = "123123";
			Shipment3.JS_A_RCV = ZDateTime.Empty;
			Shipment3.JS_InterimReceipt = ZString.Empty;

			AssertEquals(new ZInt(8), Sailing.ReceivedPackages);

			Shipment3.JS_InterimReceipt = "345432";

			AssertEquals(new ZInt(12), Sailing.ReceivedPackages);
		}

		public void TestReceivedWeight()
		{
			SetupTestTotals();
			Shipment1.JS_ActualWeight = new ZDecimal(3000);
			Shipment2.JS_ActualWeight = new ZDecimal(590);
			Shipment3.JS_ActualWeight = new ZDecimal(440);

			Shipment1.JS_A_RCV = new ZDateTime(2003, 12, 19);
			Shipment1.JS_InterimReceipt = ZString.Empty;
			Shipment2.JS_A_RCV = ZDateTime.Empty;
			Shipment2.JS_InterimReceipt = "123123";
			Shipment3.JS_A_RCV = ZDateTime.Empty;
			Shipment3.JS_InterimReceipt = ZString.Empty;

			AssertEquals(new ZDecimal(3590), Sailing.ReceivedWeight);

			Shipment3.JS_A_RCV = new ZDateTime(2003, 12, 19);

			AssertEquals(new ZDecimal(4030), Sailing.ReceivedWeight);
		}

		public void TestReceivedVolume()
		{
			SetupTestTotals();
			Shipment1.JS_ActualVolume = new ZDecimal(30.91);
			Shipment2.JS_ActualVolume = new ZDecimal(5.7);
			Shipment3.JS_ActualVolume = new ZDecimal(8.72);

			Shipment1.JS_A_RCV = new ZDateTime(2003, 12, 19);
			Shipment1.JS_InterimReceipt = ZString.Empty;
			Shipment2.JS_A_RCV = ZDateTime.Empty;
			Shipment2.JS_InterimReceipt = "123123";
			Shipment3.JS_A_RCV = ZDateTime.Empty;
			Shipment3.JS_InterimReceipt = ZString.Empty;

			AssertEquals(new ZDecimal(36.61), Sailing.ReceivedVolume);

			Shipment3.JS_InterimReceipt = "345432";

			AssertEquals(new ZDecimal(45.33), Sailing.ReceivedVolume);
		}

		public void TestIsReferenced()
		{
			Assert("Precondition: Not expecting new sailing to be referenced.", !Sailing.IsReferenced());

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_JX = Sailing.PK;
			Assert("Expecting Sailing to be referenced, it has a booking on it.", Sailing.IsReferenced());

			Transport transport = Factory.New<CommonConsol>().Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = Sailing.PK;
			Assert("Expecting Sailing to be referenced, it has a booking and Transport on it.", Sailing.IsReferenced());

			shipment.JS_JX = ZGuid.Empty;
			Assert("Expecting Sailing to be referenced, it has a transport on it.", Sailing.IsReferenced());

			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_JX = Sailing.PK;
			Assert("Expecting Sailing to be referenced, it has a transport and container on it.", Sailing.IsReferenced());

			transport.JW_JX = ZGuid.Empty;
			Assert("Expecting Sailing to be referenced, it has a container on it.", Sailing.IsReferenced());

			transport.JW_JX = Sailing.PK;
			shipment.JS_JX = Sailing.PK;
			Assert("Expecting Sailing to be referenced, it has a shipment, transport and container on it.", Sailing.IsReferenced());

			shipment.JS_JX = ZGuid.Empty;
			transport.JW_JX = ZGuid.Empty;
			container.JC_JX = ZGuid.Empty;
			Assert("Not expecting sailing to be referenced.", !Sailing.IsReferenced());

			var localTransportJob = (BusinessObject)Factory.New<ICommonCartage>();
			localTransportJob[JobCartageSchema.JJ_JX_Sailing] = Sailing.PK;
			Assert("Expecting sailing to be referenced by Local Transport.", Sailing.IsReferenced());

			localTransportJob[JobCartageSchema.JJ_JX_Sailing] = ZGuid.Empty;
			Assert("Not expecting sailing to be referenced.", !Sailing.IsReferenced());
		}

		public void TestIsReferencedWhenJS_IsCancelledIsTrue()
		{
			Assert("Precondition: Not expecting new sailing to be referenced.", !Sailing.IsReferenced());

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_JX = Sailing.PK;
			shipment.JS_IsCancelled = ZBool.True;
			Assert("Expecting Sailing to be referenced, it has a shipment on it.", Sailing.IsReferenced());
		}

		public void TestIsReferencedByRatingContractAllocationLine()
		{
			Assert("Precondition: Not expecting new sailing to be referenced.", !Sailing.IsReferenced());
			var line = Factory.New<IRatingContractAllocationLine>();
			line.RCA_JX_SailingSchedule = Sailing.PK;
			Assert("Expecting Sailing to be referenced, it has an allocation line on it.", Sailing.IsReferenced());
		}

		public void TestGetReferencingJobNumbers()
		{
			AssertEquals(ZString.Empty, Sailing.GetReferencingJobNumbers());

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_UniqueConsignRef = "CONSOL1";

			var consolTransport = consol.Transports.AddNew();
			consolTransport.JW_IsLinked = true;
			consolTransport.JW_JX = Sailing.PK;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "SHIPMENT1";

			var shipmentTransport = shipment.Transports.AddNew();
			shipmentTransport.JW_IsLinked = true;
			shipmentTransport.JW_JX = Sailing.PK;

			var booking = Factory.NewWithValidTestData<CommonShipment>();
			booking.JS_UniqueConsignRef = "BOOKING1";
			booking.JS_JX = Sailing.PK;

			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_DeclarationReference] = "DECLA1";

			var declarationTransport = Factory.NewWithValidTestData<Transport>();
			declarationTransport.ParentType = declaration.GetType();
			declarationTransport.JW_ParentGUID = declaration.PK;
			declarationTransport.JW_ParentType = Constants.TransportParentTypes.Declaration;
			declarationTransport.JW_IsLinked = true;
			declarationTransport.JW_JX = Sailing.PK;

			Factory.Save();

			string expectedMessage = @"There are jobs referencing Sailing Port Pair (Load='AUSYD' Discharge='USLAX')
Consol CONSOL1
Declaration DECLA1
Shipment SHIPMENT1
Shipment BOOKING1";

			AssertEquals(expectedMessage, Sailing.GetReferencingJobNumbers().ToString());
		}

		public void TestReferencesAfterLoadingVoyageRelatedJobs()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_UniqueConsignRef = "CONSOL1";

			var consolTransport = consol.Transports.AddNew();
			consolTransport.JW_IsLinked = true;
			consolTransport.JW_JX = Sailing.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var sailing = newFactory.Load<JobSailing>(Sailing.PK);

			AssertEquals("Consol is shown in related jobs", "CONSOL1", sailing.Voyage.RelatedJobs[0].VJV_JobNumber);
			AssertContains("Consol CONSOL1", sailing.GetReferencingJobNumbers());
			Assert(sailing.IsReferenced());
		}

		public void TestRelatedSailings()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			JobVoyage v1 = factory.New<JobVoyage>();
			VoyageOrigin o1 = factory.New<VoyageOrigin>();
			o1.JA_RL_NKPortOfLoading = "AUSYD";
			o1.JA_E_DEP = ZDateTime.Today;
			v1.Origins.Add(o1);
			VoyageDestination d1 = factory.New<VoyageDestination>();
			d1.JB_RL_NKPortOfDischarge = "CHRRC";
			v1.Destinations.Add(d1);

			v1.GenerateSailings();
			JobSailing s1 = v1.Sailings[0];
			s1.JX_IsPublished = true;

			JobVoyage v2 = factory.New<JobVoyage>();
			VoyageOrigin o2 = factory.New<VoyageOrigin>();
			o2.JA_RL_NKPortOfLoading = "AUSYD";
			o2.JA_E_DEP = ZDateTime.Today;
			v2.Origins.Add(o2);
			VoyageDestination d2 = factory.New<VoyageDestination>();
			d2.JB_RL_NKPortOfDischarge = "CHRRC";
			v2.Destinations.Add(d2);
			v2.Sailings[0].JX_IsPublished = true;

			JobVoyage v3 = factory.New<JobVoyage>();
			VoyageOrigin o3 = factory.New<VoyageOrigin>();
			o3.JA_RL_NKPortOfLoading = "AUSYD";
			o3.JA_E_DEP = ZDateTime.Today;
			v3.Origins.Add(o3);
			VoyageDestination d3 = factory.New<VoyageDestination>();
			d3.JB_RL_NKPortOfDischarge = "CHRRC";
			v3.Destinations.Add(d3);
			VoyageDestination d3b = factory.New<VoyageDestination>();
			d3b.JB_RL_NKPortOfDischarge = "CHBSL";
			v3.Destinations.Add(d3b);
			v3.Sailings[0].JX_IsPublished = true;

			Helper = new SailingsForTestClasses(factory);

			JobVoyage v4 = factory.New<JobVoyage>();
			v4.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			VoyageOrigin o4 = factory.New<VoyageOrigin>();
			o4.JA_RL_NKPortOfLoading = "AUSYD";
			o4.JA_E_DEP = ZDateTime.Today;
			v4.Origins.Add(o4);
			VoyageDestination d4 = factory.New<VoyageDestination>();
			d4.JB_RL_NKPortOfDischarge = "CHRRC";
			v4.Destinations.Add(d4);
			v4.Sailings[0].JX_IsPublished = true;

			JobVoyage v5 = factory.New<JobVoyage>();
			VoyageOrigin o5 = factory.New<VoyageOrigin>();
			o5.JA_RL_NKPortOfLoading = "AUSYD";
			o5.JA_E_DEP = ZDateTime.Today;
			v5.Origins.Add(o5);
			VoyageDestination d5 = factory.New<VoyageDestination>();
			d5.JB_RL_NKPortOfDischarge = "CHRRC";
			v5.Destinations.Add(d5);
			v5.Sailings[0].JX_IsPublished = ZBool.False;

			factory.Save();

			var sai1 = factory.Load<JobSailingForTest>(s1.PK);

			AssertEquals(3, sai1.RelatedSailingsForTest.Count);
		}

		public void TestSupportedDataContexts()
		{
			AssertEquals("Constants.DataContext.Sailing is Supported", true, Sailing.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.Sailing)));
			AssertEquals("Constants.DataContext.LoadListConsol is Supported", true, Sailing.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.LoadListConsol)));
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.BookingLoadList, Sailing.DocumentSupporter.BusinessContext);
		}

		public void TestGetDocBusinessObjects()
		{
			AssertEquals(typeof(JobSailing), Sailing.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.LoadListConsol, null)[0].WrappedObject.GetType());
			AssertEquals(typeof(JobSailing), Sailing.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.Sailing, null)[0].WrappedObject.GetType());
		}

		public void TestOverlaps()
		{
			AssertEquals("no eta/etd set, assume it overlaps", true, Sailing.Overlaps(Origin2));

			Origin2.JA_E_DEP = ZDateTime.Today;
			AssertEquals("no sailing eta/etd set, assume it overlaps", true, Sailing.Overlaps(Origin2));

			Origin.JA_E_DEP = ZDateTime.Today.AddDays(1);
			AssertEquals("Sailing origin happens after other origin", false, Sailing.Overlaps(Origin2));

			Origin2.JA_E_DEP = ZDateTime.Today.AddDays(5);
			AssertEquals("Sailing origin happens before other origin", true, Sailing.Overlaps(Origin2));

			Destination.JB_E_ARV = ZDateTime.Today.AddDays(4);
			AssertEquals("Sailing destination happens before other origin", false, Sailing.Overlaps(Origin2));

			Origin2.JA_E_DEP = ZDateTime.Today.AddDays(3);
			AssertEquals("Sailing destination happens after other origin", true, Sailing.Overlaps(Origin2));

			Origin2.JA_E_DEP = ZDateTime.Empty;
			AssertEquals("Sailing destination happens after other origin", true, Sailing.Overlaps(Origin2));

			Origin2.JA_E_DEP = ZDateTime.Invalid;
			AssertEquals("Sailing destination happens after other origin", true, Sailing.Overlaps(Origin2));

			Origin2.JA_E_DEP = ZDateTime.Today;
			Origin.JA_E_DEP = ZDateTime.Invalid;
			AssertEquals("Sailing destination happens after other origin", true, Sailing.Overlaps(Origin2));

			Origin.JA_E_DEP = ZDateTime.Today;
			Destination.JB_E_ARV = ZDateTime.Invalid;
			AssertEquals("Sailing destination happens after other origin", true, Sailing.Overlaps(Origin2));
		}

		public void TestScheduleChangeParent()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();

			IScheduleChangeParent parent = voyage.Sailings[0];

			CombineAssertions(delegate
			{
				AssertEquals("DestinationPort", "NLAMS", parent.DestinationPort);
				AssertEquals("Factory", Factory, parent.Factory);
				AssertEquals("OriginPort", "AUBNE", parent.OriginPort);
				AssertEquals("PK", voyage.Sailings[0].PK, parent.PK);
				AssertEquals("SailingRefColumn", JobSailingSchema.PK, parent.SailingRefColumn);
				AssertEquals("TablePrefix", JobSailingSchema.Constants.Prefix, parent.TablePrefix);
				AssertEquals("Voyage", voyage, parent.Voyage);
			});
		}

		public void TestAdditionalValidation()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.JK_RL_NKDischargePort = "INBOM";

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VESSEL 111";

			var transport = consol.Transports[0];
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = "AIR";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "111S";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSMV";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			AssertNotNull(sailing.AdditionalValidation);
			AssertType(typeof(TransportSailingAdditionalValidation), sailing.AdditionalValidation);
		}

		public void TestUpdateRealtedBookedAgencyBookingEvent()
		{
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USCHI";
			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];

			var agencyBooking1 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking1.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking1.JS_JX = sailing.PK;
			agencyBooking1.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var mainSeaLeg = agencyBooking1.Transports.Cast<Transport>()
				.FirstOrDefault(transport =>
					transport.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel &&
					transport.JW_TransportMode == Core.Constants.TransportModes.Sea);
			mainSeaLeg.JW_ATD = ZDateTime.Today;

			var agencyBooking2 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking2.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking2.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var agencybooking2MainTrasnport = agencyBooking2.Transports.AddNew();
			agencybooking2MainTrasnport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			agencybooking2MainTrasnport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			agencybooking2MainTrasnport.JW_IsLinked = false;

			var agencybooking2OtherTrasnport = agencyBooking2.Transports.AddNew();
			agencybooking2OtherTrasnport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			agencybooking2OtherTrasnport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			agencybooking2OtherTrasnport.JW_IsLinked = true;
			agencybooking2OtherTrasnport.JW_JX = voyage.Sailings[0].PK;

			var agencyBooking3 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking3.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			agencyBooking3.JS_JX = sailing.PK;
			agencyBooking3.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var billOfLading1 = (CommonShipment)Factory.New<IBillOfLading>();
			billOfLading1.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			billOfLading1.JS_JX = sailing.PK;
			billOfLading1.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var agencyBooking4 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking4.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking4.JS_JX = sailing.PK;

			var booking4MainSeaLeg = agencyBooking4.Transports.Cast<Transport>()
				.FirstOrDefault(transport =>
					transport.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel &&
					transport.JW_TransportMode == Core.Constants.TransportModes.Sea);
			booking4MainSeaLeg.JW_ATD = ZDateTime.Today;

			var agencyBooking5 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			agencyBooking5.JS_JX = sailing.PK;
			agencyBooking5.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var booking5MainSeaLeg = agencyBooking1.Transports.Cast<Transport>()
				.FirstOrDefault(transport =>
					transport.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel &&
					transport.JW_TransportMode == Core.Constants.TransportModes.Sea);
			booking5MainSeaLeg.JW_ATD = ZDateTime.Today;

			Factory.Save();

			AssertEquals(sailing.PK, agencyBooking1.JS_JX);
			AssertEquals(ZGuid.Empty, agencyBooking2.JS_JX);
			AssertEquals(sailing.PK, agencyBooking3.JS_JX);
			AssertEquals(sailing.PK, billOfLading1.JS_JX);
			AssertEquals(sailing.PK, agencybooking2OtherTrasnport.JW_JX);
			AssertEquals(sailing.PK, agencyBooking4.JS_JX);
			AssertEquals(sailing.PK, agencyBooking5.JS_JX);

			Assert(!agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

			var registry = new Mock<IAgencyRegistry>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(registry.Object))
			{
				registry.Setup(m => m.ElectronicBookingAndShippingInstructions).Returns(true);
				registry.Setup(m => m.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(ZGuid.Empty)).Returns(true);

				sailing.JX_DepotReceivalCommences = new ZDateTime(2023, 09, 05);
				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				Factory.Save();

				AssertEquals(1, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				sailing.JX_DepotCutOff = new ZDateTime(2023, 09, 06);
				Factory.Save();

				AssertEquals(2, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}
		}

		#region CO2e

		public void TestCO2ePerTonneInKgForBinding()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];

			AssertEquals("Precondition: CO2ePerTonneInKg default value is 0", 0m, sailing.GetCO2ePerTonneInKg());
			AssertEquals("Precondition: CO2ePerTonneInKg default value is empty", ZString.Empty, sailing.CO2ePerTonneInKgForBinding);

			sailing.SetCO2eStatus(CO2eStatusList.Codes.NotCalculated);
			AssertEquals(ZString.Empty, sailing.CO2ePerTonneInKgForBinding);

			sailing.SetCO2eStatus(CO2eStatusList.Codes.Current);
			sailing.SetCO2ePerTonneInKg(1234.5678m);
			AssertEquals("1,234.568", sailing.CO2ePerTonneInKgForBinding);

			sailing.SetCO2ePerTonneInKg(0.12345678m);
			AssertEquals("Setting number with 8 digits after decimal, should be rounded to 7 digits after decimal", "0.123", sailing.CO2ePerTonneInKgForBinding);

			sailing.SetCO2ePerTonneInKg(0m);
			AssertEquals(ZString.Empty, sailing.CO2ePerTonneInKgForBinding);

			sailing.SetCO2eStatus(CO2eStatusList.Codes.Pending);
			AssertEquals("Pending", sailing.CO2ePerTonneInKgForBinding);
		}

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent_WhenCO2eStatusCurrent()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = "SEA";
			voyage.JV_VoyageFlight = "VOY111";
			voyage.JV_AircraftType = "N95";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			Factory.Save();

			var sailing = voyage.Sailings[0];
			var origin = voyage.Origins[0];
			var destination = voyage.Destinations[0];

			void UpdateRelatedPropertyAndAssert(string message, Action updateProperty, string stuReason)
			{
				TestDateAttribute.AddMinutes(1);
				sailing.SetCO2eStatus(CO2eStatusList.Codes.Current);
				sailing.Validation.ValidateCO2ePerTonneInKgForBinding();
				AssertNoWarning("Pre-condition", sailing.CO2ePerTonneInKgForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
				updateProperty.Invoke();
				AssertEquals(message, CO2eStatusList.Codes.NotCurrent, sailing.GetCO2eStatus());
				AssertHasWarning(sailing.CO2ePerTonneInKgForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
				CO2eTestHelper.AssertSTUEvent(sailing, stuReason);
			}

			UpdateRelatedPropertyAndAssert("Load Port", () => origin.JA_RL_NKPortOfLoading = "AUSYD",
				"JA_RL_NKPortOfLoading [AUBNE]->[AUSYD]");
			UpdateRelatedPropertyAndAssert("Discharge Port", () => destination.JB_RL_NKPortOfDischarge = "NZAKL",
				"JB_RL_NKPortOfDischarge [SGSIN]->[NZAKL]");
			UpdateRelatedPropertyAndAssert("Voyage/Flight", () => voyage.JV_VoyageFlight = "VOY222",
				"JV_VoyageFlight [VOY111]->[VOY222]");
			UpdateRelatedPropertyAndAssert("Aircraft Type", () => voyage.JV_AircraftType = "N98",
				"JV_AircraftType [N95]->[N98]");

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			UpdateRelatedPropertyAndAssert("Carrier", () => voyage.JV_OH_Line = carrier.PK,
				$"JV_OH_Line [{ZGuid.Empty}]->[{carrier.PK}]");

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "vessel000";
			UpdateRelatedPropertyAndAssert("Vessel", () => voyage.JV_RV_NKVessel = vessel.RV_FK,
				"JV_RV_NKVessel []->[vessel000]");
		}

		#endregion

		#region JX_OnlineScheduleStatus

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJX_OnlineScheduleStatus_UpdateStatusWithMatch()
		{
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				voyage.JV_VoyageFlight = "QF1234";

				var origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "AUSYD";
				origin.JA_E_DEP = ZDate.Today;

				var destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = "USJFK";
				destination.JB_E_ARV = ZDate.Today;

				voyage.GenerateSailings();

				var sailing = voyage.Sailings[0];

				sailing.TryMatchAgainstOnlineFlights();
				AssertEquals("Matching enabled: Sailing should be matched", Constants.FlightScheduleStatus.Matched, sailing.JX_OnlineScheduleStatus);
				AssertNotEquals("Matching enabled: Matched Schedule should be set", ScheduleInfo.Empty, sailing.MatchedSchedule);
			});
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJX_OnlineScheduleStatus_AutomaticMatching()
		{
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				voyage.JV_VoyageFlight = "QF1234";

				var origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "AUSYD";
				origin.JA_E_DEP = ZDate.Today;

				var destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = "USJFK";
				destination.JB_E_ARV = ZDate.Today;

				voyage.GenerateSailings();

				var sailing = voyage.Sailings[0];

				sailing.TryMatchAgainstOnlineFlights();
				AssertEquals("Automatic Matching enabled: Sailing should be matched", Constants.FlightScheduleStatus.Matched, sailing.JX_OnlineScheduleStatus);
				AssertNotEquals("Automatic Matching enabled: Matched Schedule should be set", ScheduleInfo.Empty, sailing.MatchedSchedule);
			});
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJX_OnlineScheduleStatus_AutomaticMatching_NotAirTransport()
		{
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
				voyage.JV_VoyageFlight = "QF1234";

				var origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "AUSYD";
				origin.JA_E_DEP = ZDate.Today;

				var destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = "USJFK";
				destination.JB_E_ARV = ZDate.Today;

				voyage.GenerateSailings();

				var sailing = voyage.Sailings[0];

				sailing.TryMatchAgainstOnlineFlights();
				AssertEquals("Transport is Sea: should not be matched", Constants.FlightScheduleStatus.Unknown, sailing.JX_OnlineScheduleStatus);
				AssertEquals("Transport is Sea: Matched Schedule should not be set", ScheduleInfo.Empty, sailing.MatchedSchedule);
			});
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJX_OnlineScheduleStatus_AutomaticMatching_Disabled_WhenNotUserInteractive()
		{
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var originalUserInteractive = Globals.IsUserInteractive;

				using (new DisposableAction(() => Globals.IsUserInteractive = false, () => Globals.IsUserInteractive = originalUserInteractive))
				{
					var voyage = Factory.New<JobVoyage>();
					voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
					voyage.JV_VoyageFlight = "QF1234";

					var origin = voyage.Origins.AddNew();
					origin.JA_RL_NKPortOfLoading = "AUSYD";
					origin.JA_E_DEP = ZDate.Today;

					var destination = voyage.Destinations.AddNew();
					destination.JB_RL_NKPortOfDischarge = "USJFK";
					destination.JB_E_ARV = ZDate.Today;

					voyage.GenerateSailings();

					var sailing = voyage.Sailings[0];

					sailing.TryMatchAgainstOnlineFlights();
					AssertEquals("Transport is Sea: should not be matched", Constants.FlightScheduleStatus.Unknown, sailing.JX_OnlineScheduleStatus);
					AssertEquals("Transport is Sea: Matched Schedule should not be set", ScheduleInfo.Empty, sailing.MatchedSchedule);
				}
			});
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJV_AircraftType_UpdatesOnMatch_UnSaved()
		{
			// Arrange
			var aircraftType = new ZString("14Z");
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var sailing = CreateSailingForMatching();

				// Act
				sailing.TryMatchAgainstOnlineFlights();

				// Assert
				AssertEquals("Sailing aircraft type should be populated", aircraftType, sailing.Voyage.JV_AircraftType);
				AssertEquals("Sailing aircraft type should be set to updated by GCC", true, sailing.Voyage.IsLastAircraftUpdatedFromGSS);
			}, aircraftType: aircraftType);
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJV_AircraftType_UpdatesOnMatch_Saved()
		{
			// Arrange
			var aircraftType = new ZString("14Z");
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var sailing = CreateSailingForMatching();
				Factory.Save();

				// Act
				sailing.TryMatchAgainstOnlineFlights();

				// Assert
				AssertEquals("Sailing aircraft type should be populated", aircraftType, sailing.Voyage.JV_AircraftType);
			}, aircraftType: aircraftType);
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJV_AircraftType_UserUpdatedAircraftType()
		{
			// Arrange
			var aircraftType = new ZString("14Z");
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var sailing = CreateSailingForMatching();

				// Act
				sailing.Voyage.JV_AircraftTypeForBinding = "15Z"; // Simulate user entered value

				// Assert
				AssertEquals("Aircraft type should be user updated", expected: false, sailing.Voyage.IsLastAircraftUpdatedFromGSS);

				// Act
				sailing.TryMatchAgainstOnlineFlights();

				// Assert
				AssertNotEquals("Sailing aircraft type should remained unchanged", aircraftType, sailing.Voyage.JV_AircraftType);
			}, aircraftType: aircraftType);
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJV_AircraftType_SavedAircraftType()
		{
			// Arrange
			var aircraftType = new ZString("14Z");
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				voyage.JV_VoyageFlight = "QF1234";
				voyage.JV_AircraftType = "15Z";

				var origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "AUSYD";
				origin.JA_E_DEP = ZDate.Today;

				var destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = "NZAKL";
				destination.JB_E_ARV = ZDate.Today;

				voyage.GenerateSailings();
				var sailing = voyage.Sailings[0];
				Factory.Save();

				// Act
				sailing.TryMatchAgainstOnlineFlights();

				// Assert
				AssertEquals("Sailing aircraft type should remained unchanged", "15Z", sailing.Voyage.JV_AircraftType);
			}, aircraftType: aircraftType);
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJV_AircraftType_SavedEmptyAircraftType()
		{
			// Arrange
			var aircraftType = new ZString("14Z");
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var sailing = CreateSailingForMatching();
				sailing.Voyage.JV_AircraftTypeForBinding = ZString.Empty;
				Factory.Save();

				// Act
				sailing.TryMatchAgainstOnlineFlights();

				// Assert
				AssertEquals("Sailing aircraft type should be matched", aircraftType, sailing.Voyage.JV_AircraftType);
			}, aircraftType: aircraftType);
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJV_AircraftType_MatchAfterEmpty()
		{
			// Arrange
			var aircraftType = new ZString("14Z");
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var sailing = CreateSailingForMatching();

				// Act
				sailing.Voyage.JV_AircraftTypeForBinding = "15Z";
				sailing.Voyage.JV_AircraftTypeForBinding = ZString.Empty;
				sailing.TryMatchAgainstOnlineFlights();

				// Assert
				AssertEquals("Sailing aircraft type should be matched", aircraftType, sailing.Voyage.JV_AircraftType);
			}, aircraftType: aircraftType);
		}

		#endregion

		#region TestFetchForLoad

		[DeveloperOnlyTest]
		public override void TestFetchForLoad()
		{
			#region Set up sailings

			var sailingPKs = new List<ZGuid>();
			var factory1 = new BusinessObjectFactory();

			for (int i = 0; i < 12; i++)
			{
				var carrier = factory1.NewWithValidTestData<OrgHeader>();

				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "vessel" + i;

				var voyage = factory1.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "voyage" + i;
				voyage.JV_OH_Line = carrier.PK;
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "SGSIN";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";

				voyage.GenerateSailings();

				foreach (JobSailing sailing in voyage.Sailings)
				{
					sailingPKs.Add(sailing.PK);
				}
			}

			factory1.Save();

			#endregion

			var factory2 = new BusinessObjectFactory();
			factory2.ResetDatabaseLoadCount();

			var sailings = factory2.Load<JobSailing>(new ZQuery(JobSailingSchema.PK, sailingPKs));

			foreach (var sailing in sailings)
			{
				// Simulate initialising direction dependent GUI items by checking linked Origin and Discharge ports
				_ = sailing.JX_JA_RL_NKPortOfLoading;
				_ = sailing.JX_JB_RL_NKPortOfDischarge;
			}

			// JobSailing: 1
			// JobVoyDestination: 1
			// JobVoyOrigin: 1
			AssertMaxDbHits(3, factory2);
		}

		#endregion

		#region Test Classes

		public class JobSailingForTest : JobSailing
		{
			public JobSailingForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public JobSailingCollection RelatedSailingsForTest
			{
				get { return RelatedSailings; }
			}
		}

		[TestedType(typeof(JobSailingDocumentSupporter))]
		class JobSailingDocumentSupporterTest : DocumentSupporterTest
		{
			protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
			{
				return Factory.New<JobSailing>();
			}
		}

		#endregion

		#region Implementation

		JobSailing Sailing;
		JobSailing Sailing2;
		VoyageOrigin Origin;
		VoyageOrigin Origin2;
		VoyageDestination Destination;
		JobVoyage Voyage;
		CommonShipment Shipment1;
		CommonShipment Shipment2;
		CommonShipment Shipment3;
		CommonShipment Shipment4;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in SetUp")]
		SailingsForTestClasses Helper;

		protected override void SetUp()
		{
			base.SetUp();

			var vessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First();

			Voyage = Factory.New<JobVoyage>();
			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			Voyage.JV_VoyageFlight = "234";
			Voyage.JV_RV_NKVessel = vessel.RV_FK;

			Origin = Voyage.Origins.AddNew();
			Origin.JA_RL_NKPortOfLoading = "AUSYD";

			Origin2 = Voyage.Origins.AddNew();
			Origin2.JA_RL_NKPortOfLoading = "AUBNE";

			Destination = Voyage.Destinations.AddNew();
			Destination.JB_JV = Voyage.PK;
			Destination.JB_RL_NKPortOfDischarge = "USLAX";

			Sailing = Voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "USLAX");
			Sailing2 = Voyage.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "USLAX");

			if (Sailing == null)
			{
				Fail("Sailing not set");
			}

			if (Sailing2 == null)
			{
				Fail("Sailing2 not set");
			}

			Factory.Save();

			Helper = new SailingsForTestClasses(Factory);
		}

		void SetupTestTotals()
		{
			SetUp();
			Sailing2 = Factory.New<JobSailing>();
			Shipment1 = Factory.New<CommonShipment>();
			Shipment2 = Factory.New<CommonShipment>();
			Shipment3 = Factory.New<CommonShipment>();
			Shipment4 = Factory.New<CommonShipment>();
			Shipment1.JS_JX = Sailing.PK;
			Shipment2.JS_JX = Sailing.PK;
			Shipment3.JS_JX = Sailing.PK;
			Shipment4.JS_JX = Sailing2.PK;

			Shipment1.JS_IsBooking = true;
			Shipment2.JS_IsBooking = true;
			Shipment3.JS_IsBooking = true;
			Shipment4.JS_IsBooking = true;

			Shipment1.JS_IsForwardRegistered = false;
			Shipment2.JS_IsForwardRegistered = false;
			Shipment3.JS_IsForwardRegistered = false;
			Shipment4.JS_IsForwardRegistered = false;
		}

		int JX_JA_RL_NKPortOfLoadingInfo_ChangeCount;
		void JX_JA_RL_NKPortOfLoadingInfo_ValueChanged(object sender, EventArgs e)
		{
			JX_JA_RL_NKPortOfLoadingInfo_ChangeCount++;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var vessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First();

			JobVoyage voyage1 = factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage1.JV_VoyageFlight = "234";
			voyage1.JV_RV_NKVessel = vessel.RV_FK;

			VoyageOrigin origin1 = factory.New<VoyageOrigin>();
			origin1.JA_JV = voyage1.PK;
			origin1.JA_RL_NKPortOfLoading = "AUSYD";

			VoyageDestination destination1 = factory.New<VoyageDestination>();
			destination1.JB_JV = voyage1.PK;
			destination1.JB_RL_NKPortOfDischarge = "USLAX";

			BaseJobSailing sailing1 = factory.New<BaseJobSailing>();
			sailing1.JX_JA = origin1.PK;
			sailing1.JX_JB = destination1.PK;

			return sailing1;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			Voyage = Factory.New<JobVoyage>();
			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;

			Origin = Factory.New<VoyageOrigin>();
			Origin.JA_JV = Voyage.PK;
			Origin.JA_RL_NKPortOfLoading = "AUSYD";

			Destination = Factory.New<VoyageDestination>();
			Destination.JB_JV = Voyage.PK;
			Destination.JB_RL_NKPortOfDischarge = "USLAX";

			Sailing = Factory.New<JobSailing>();
			Sailing.JX_JA = Origin.PK;
			Sailing.JX_JB = Destination.PK;

			Factory.Save();

			return Sailing;
		}

		JobSailing CreateSailingForMatching()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "QF1234";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDate.Today;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_E_ARV = ZDate.Today;

			voyage.GenerateSailings();

			return voyage.Sailings[0];
		}

		#endregion
	}
}
