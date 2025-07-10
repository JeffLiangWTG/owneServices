using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(BaseJobSailing))]
	sealed class BaseJobSailingTest : EnterpriseBusinessObjectTestCase
	{
		public void TestStowPlanMessageStatus()
		{
			var voyage = Factory.New<JobVoyage>();
			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			origin1.JA_E_DEP = ZDate.Today.AddDays(5);
			origin1.JA_VGMCutOff = ZDate.Today.AddDays(6);
			var dest1 = voyage.Destinations.AddNew();
			dest1.JB_RL_NKPortOfDischarge = "NZAKL";
			dest1.JB_E_ARV = ZDate.Today.AddDays(7);
			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "NZAKL";
			origin2.JA_E_DEP = ZDate.Today.AddDays(9);
			var dest2 = voyage.Destinations.AddNew();
			dest2.JB_RL_NKPortOfDischarge = "USLAX";
			dest2.JB_E_ARV = ZDate.Today.AddDays(11);
			dest2.StowPlanMessageStatus = MessageStatusListSTW.Codes.Acceptance;

			voyage.GenerateSailings();

			var sailing1 = voyage.Sailings.Cast<JobSailing>().FirstOrDefault(x => x.JX_JA_RL_NKPortOfLoading == "AUSYD" && x.JX_JB_RL_NKPortOfDischarge == "NZAKL");
			AssertEquals(ZString.Empty, sailing1.StowPlanMessageStatus);
			var sailing2 = voyage.Sailings.Cast<JobSailing>().FirstOrDefault(x => x.JX_JA_RL_NKPortOfLoading == "AUSYD" && x.JX_JB_RL_NKPortOfDischarge == "USLAX");
			AssertEquals(MessageStatusListSTW.Codes.Acceptance, sailing2.StowPlanMessageStatus);
			AssertEquals(ZDate.Today.AddDays(6), sailing2.JX_JA_VGMCutOff);
			AssertEquals(MessageStatusListSTW.Descriptions.Acceptance, sailing2.StowPlanMessageStatusDescription);
			var sailing3 = voyage.Sailings.Cast<JobSailing>().FirstOrDefault(x => x.JX_JA_RL_NKPortOfLoading == "NZAKL" && x.JX_JB_RL_NKPortOfDischarge == "USLAX");
			AssertEquals(MessageStatusListSTW.Codes.Acceptance, sailing3.StowPlanMessageStatus);
			AssertEquals(MessageStatusListSTW.Descriptions.Acceptance, sailing3.StowPlanMessageStatusDescription);
		}

		#region Read Only Properties

		public void TestProxyPropertiesReadonly()
		{
			ZPropertyInfo[] infos = new ZPropertyInfo[]
			{
				Sailing.JX_JV_IsCharteredInfo,
				Sailing.JX_JV_NKVesselInfo,
				Sailing.JX_JV_OH_LineInfo,
				Sailing.JX_JV_VoyageFlightInfo,
				Sailing.JX_JV_VoyageTypeInfo,
				Sailing.JX_JV_AircraftTypeInfo,

				Sailing.JX_JA_DGFCLCutOffInfo,
				Sailing.JX_JA_DGFCLReceivalCommencesInfo,
				Sailing.JX_JA_DocumentaryCutoffInfo,
				Sailing.JX_JA_VGMCutOffInfo,
				Sailing.JX_JA_E_DEPInfo,
				Sailing.JX_JA_A_DEPInfo,
				Sailing.JX_JA_E_ARVInfo,
				Sailing.JX_JA_A_ARVInfo,
				Sailing.JX_JA_CTOCutOffInfo,
				Sailing.JX_JA_CTOReceivalCommencesInfo,
				Sailing.JX_JA_EmptyReceivalCommencesInfo,
				Sailing.JX_JA_EmptyCutOffInfo,
				Sailing.JX_JA_ReeferReceivalCommencesInfo,
				Sailing.JX_JA_ReeferCutOffInfo,
				Sailing.JX_JA_RL_NKPortOfLoadingInfo,

				Sailing.JX_JB_E_ARVInfo,
				Sailing.JX_JB_A_ARVInfo,
				Sailing.JX_JB_CTOAvailabilityDateInfo,
				Sailing.JX_JB_CTOStorageDateInfo,
				Sailing.JX_JB_RL_NKPortOfDischargeInfo,
			};

			List<string> nonReadOnlyProperties = new List<string>();

			foreach (ZPropertyInfo info in infos)
			{
				if (!info.ReadOnly)
				{
					nonReadOnlyProperties.Add(info.Name);
				}
			}

			IDictionary<string, IList<string>> result = new Dictionary<string, IList<string>>();

			if (nonReadOnlyProperties.Count > 0)
			{
				result.Add("These properties should be readonly", nonReadOnlyProperties);
			}

			AssertGroupedErrorList(result);
		}

		public void TestReadonlyProperties()
		{
			Assert("expected JX_JA to be readonly", Sailing.JX_JAInfo.ReadOnly);
			Assert("expected JX_JB to be readonly", Sailing.JX_JBInfo.ReadOnly);
			Assert("expected JX_UniqueReference to be readonly", Sailing.JX_UniqueReferenceInfo.ReadOnly);
		}

		#endregion

		#region TestGetVoyageBusinessObject

		public void TestGetVoyageBusinessObject()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			JobVoyage testVoyage = factory.New(typeof(JobVoyage)) as JobVoyage;
			JobVoyage testVoyage2 = factory.New(typeof(JobVoyage)) as JobVoyage;
			BaseJobSailing testSailing;

			testVoyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			testVoyage.JV_VoyageFlight = "234";
			testVoyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First().RV_FK;

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

		#endregion

		#region TestJX_JV_IsChartered

		public void TestJX_JV_IsChartered()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];

			voyage.JV_IsChartered = true;
			AssertEquals(true, sailing.JX_JV_IsChartered);

			voyage.JV_IsChartered = false;
			AssertEquals(false, sailing.JX_JV_IsChartered);
		}

		#endregion

		#region Related Business Objects

		#region TestOrigin
		public void TestOrigin()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			BaseJobSailing testSailing = testFactory.New(typeof(BaseJobSailing)) as BaseJobSailing;
			VoyageOrigin testOrigin = testFactory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageOrigin testOrigin2 = testFactory.New(typeof(VoyageOrigin)) as VoyageOrigin;

			testSailing.JX_JA = testOrigin.PK;

			AssertEquals("TestSailing.JX_JA and TestOrigin are equal, no error expected", testSailing.Origin.PK, testSailing.JX_JA);
			Assert("TestSailing.JX_JA and TestOrigin2 are not equal, error expected", testSailing.Origin.PK != testOrigin2.PK);
			Assert("TestSailing.JX_JA and TestOrigin2 are not equal, error expected", testSailing.JX_JA != testOrigin2.PK);
		}
		#endregion

		#region TestDestination
		public void TestDestination()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			BaseJobSailing testSailing = testFactory.New(typeof(BaseJobSailing)) as BaseJobSailing;
			VoyageDestination testDestination = testFactory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageDestination testDestination2 = testFactory.New(typeof(VoyageDestination)) as VoyageDestination;

			testSailing.JX_JB = testDestination.PK;

			AssertEquals("TestSailing.JX_JB and TestDestination are equal, no error expected", testSailing.Destination.PK, testSailing.JX_JB);
			Assert("TestSailing.JX_JA and TestDestination2 are not equal, error expected", testSailing.Destination.PK != testDestination2.PK);
			Assert("TestSailing.JX_JA and TestDestination2 are not equal, error expected", testSailing.JX_JA != testDestination2.PK);
		}
		#endregion

		#region TestVoyage
		public void TestVoyage()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			JobVoyage testVoyage = testFactory.New(typeof(JobVoyage)) as JobVoyage;
			JobVoyage testVoyage2 = testFactory.New(typeof(JobVoyage)) as JobVoyage;

			VoyageDestination testDestination = testFactory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageDestination testDestination2 = testFactory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageOrigin testOrigin = testFactory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageOrigin testOrigin2 = testFactory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			BaseJobSailing testSailing = testFactory.New(typeof(BaseJobSailing)) as BaseJobSailing;

			testSailing.JX_JA = testOrigin.PK;
			testSailing.JX_JB = testDestination.PK;

			testDestination.JB_JV = testVoyage.PK;
			testOrigin.JA_JV = testVoyage.PK;

			JobVoyage voy = testSailing.Voyage;

			AssertEquals("correct voyage returned, no error expected", testVoyage.PK, voy.PK);
			Assert("Comparing different voyages, should not be equal", voy.PK != testVoyage2.PK);
		}
		#endregion

		#region TestVessel
		public void TestVessel()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			JobVoyage testVoyage = testFactory.New(typeof(JobVoyage)) as JobVoyage;
			VoyageDestination testDestination = testFactory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageOrigin testOrigin = testFactory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			BaseJobSailing testSailing = testFactory.New(typeof(BaseJobSailing)) as BaseJobSailing;
			RefVessel testVessel = testFactory.New(typeof(RefVessel)) as RefVessel;
			RefVessel testVessel2 = testFactory.New(typeof(RefVessel)) as RefVessel;

			testSailing.JX_JA = testOrigin.PK;
			testSailing.JX_JB = testDestination.PK;
			testDestination.JB_JV = testVoyage.PK;
			testOrigin.JA_JV = testVoyage.PK;

			Assert("No Vessel, sailing.vessel should be null", testSailing.Vessel == null);

			testVessel.RV_Name = "ABCDE";
			testVoyage.JV_RV_NKVessel = testVessel.RV_FK;

			AssertEquals("Correct vessel, no error expected", testVessel, testSailing.Vessel);
			Assert("Comparing different vessels, should not be equal", testSailing.Vessel != testVessel2);
		}

		#endregion

		#endregion

		#region Test other properties

		#region TestVoyageNumber
		public void TestVoyageNumber()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			JobVoyage testVoyage = testFactory.New(typeof(JobVoyage)) as JobVoyage;
			VoyageDestination testDestination = testFactory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageOrigin testOrigin = testFactory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			BaseJobSailing testSailing = testFactory.New(typeof(BaseJobSailing)) as BaseJobSailing;
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
		#endregion

		#region Test Port Dates
		public void TestPortDates()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			JobVoyage testVoyage = testFactory.New(typeof(JobVoyage)) as JobVoyage;
			VoyageDestination testDestination = testFactory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageOrigin testOrigin = testFactory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			BaseJobSailing testSailing = testFactory.New(typeof(BaseJobSailing)) as BaseJobSailing;
			testDestination.JB_JV = testVoyage.PK;
			testOrigin.JA_JV = testVoyage.PK;
			testSailing.JX_JA = testOrigin.PK;
			testSailing.JX_JB = testDestination.PK;

			AssertEquals("Load ETA should be blank", testSailing.JX_JA_E_ARV, ZDateTime.Empty);
			AssertEquals("Load ATA should be blank", testSailing.JX_JA_A_ARV, ZDateTime.Empty);
			AssertEquals("Load ETD should be blank", testSailing.JX_JA_E_DEP, ZDateTime.Empty);
			AssertEquals("Load ATD should be blank", testSailing.JX_JA_A_DEP, ZDateTime.Empty);
			AssertEquals("Discharge ETA should be blank", testSailing.JX_JB_E_ARV, ZDateTime.Empty);
			AssertEquals("Discharge ATA should be blank", testSailing.JX_JB_A_ARV, ZDateTime.Empty);

			testOrigin.JA_E_ARV = new ZDateTime(2011, 6, 1);
			testOrigin.JA_A_ARV = new ZDateTime(2011, 6, 5);
			testOrigin.JA_E_DEP = new ZDateTime(2011, 6, 9);
			testOrigin.JA_A_DEP = new ZDateTime(2011, 6, 14);
			testDestination.JB_E_ARV = new ZDateTime(2011, 6, 20);
			testDestination.JB_A_ARV = new ZDateTime(2011, 6, 26);

			AssertEquals("Load ETA should be 1/6/2011", new ZDateTime(2011, 6, 1), testSailing.JX_JA_E_ARV);
			AssertEquals("Load ATA should be 5/6/2011", new ZDateTime(2011, 6, 5), testSailing.JX_JA_A_ARV);
			AssertEquals("Load ETD should be 9/6/2011", new ZDateTime(2011, 6, 9), testSailing.JX_JA_E_DEP);
			AssertEquals("Load ATD should be 14/6/2011", new ZDateTime(2011, 6, 14), testSailing.JX_JA_A_DEP);
			AssertEquals("Discharge ETA should be 20/6/2011", new ZDateTime(2011, 6, 20), testSailing.JX_JB_E_ARV);
			AssertEquals("Discharge ATA should be 26/6/2011", new ZDateTime(2011, 6, 26), testSailing.JX_JB_A_ARV);
		}
		#endregion

		public void TestLoadCountry()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			JobVoyage testVoyage = testFactory.New(typeof(JobVoyage)) as JobVoyage;
			VoyageDestination testDestination = testFactory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageOrigin testOrigin = testFactory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			BaseJobSailing testSailing = testFactory.New(typeof(BaseJobSailing)) as BaseJobSailing;

			testSailing.JX_JA = testOrigin.PK;
			testSailing.JX_JB = testDestination.PK;
			testDestination.JB_JV = testVoyage.PK;
			testOrigin.JA_JV = testVoyage.PK;

			testOrigin.JA_RL_NKPortOfLoading = "AUSYD";

			AssertEquals("Expecting Load Country to be AU", "AU", testSailing.JX_Calc_LoadCountry);
		}

		public void TestDischargeCountry()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			JobVoyage testVoyage = testFactory.New(typeof(JobVoyage)) as JobVoyage;
			VoyageDestination testDestination = testFactory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageOrigin testOrigin = testFactory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			BaseJobSailing testSailing = testFactory.New(typeof(BaseJobSailing)) as BaseJobSailing;

			testSailing.JX_JA = testOrigin.PK;
			testSailing.JX_JB = testDestination.PK;
			testDestination.JB_JV = testVoyage.PK;
			testOrigin.JA_JV = testVoyage.PK;

			testDestination.JB_RL_NKPortOfDischarge = "AUSYD";

			AssertEquals("Expecting Discharge Country to be AU", "AU", testSailing.JX_Calc_DischargeCountry);
		}

		public void TestJX_JV_OH_Line()
		{
			OrgHeader shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_IsShippingLine = true;

			JobVoyage testVoyage = Factory.New(typeof(JobVoyage)) as JobVoyage;
			VoyageDestination testDestination = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageOrigin testOrigin = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			BaseJobSailing testSailing = Factory.New(typeof(BaseJobSailing)) as BaseJobSailing;

			testSailing.JX_JA = testOrigin.PK;
			testSailing.JX_JB = testDestination.PK;
			testDestination.JB_JV = testVoyage.PK;
			testOrigin.JA_JV = testVoyage.PK;

			Assert("Expecting Sailing's shipping line to be empty.", testSailing.JX_JV_OH_Line.IsEmpty);

			testVoyage.JV_OH_Line = shippingLine.PK;

			AssertEquals("Expecting Sailing's shipping line to be ShippingLine.", shippingLine.PK, testSailing.JX_JV_OH_Line);
		}

		#endregion

		#region TestLCLTextNullReferenceException

		public void TestLCLTextObjectNullReferenceException()
		{
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			sailing.Origin.JA_E_DEP = new ZDateTime(2013, 03, 05);
			sailing.Origin.JA_RL_NKPortOfLoading = "AUSYD";
			sailing.Destination.JB_A_ARV = new ZDateTime(2013, 03, 15);
			sailing.Destination.JB_RL_NKPortOfDischarge = "NZAKL";

			Factory.Save();
			sailing.Voyage.Delete();

			AssertNoExceptionThrown(() => sailing.JX_DepotReceivalCommences = new ZDateTime(2013, 03, 18));
		}

		#endregion

		#region Reference Number Fountain

		public void TestReferenceNumberGeneration()
		{
			var destination1 = Factory.New<VoyageDestination>();
			destination1.JB_JV = Voyage.PK;
			destination1.JB_RL_NKPortOfDischarge = "CNSHA";

			var sailing1 = Factory.New<BaseJobSailing>();
			sailing1.JX_JA = Origin.PK;
			sailing1.JX_JB = destination1.PK;
			Factory.Save();

			AssertEquals("SA00000001", Sailing.JX_UniqueReference);
			AssertEquals("SA00000002", sailing1.JX_UniqueReference);
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

			var sailings = factory2.Load<BaseJobSailing>(new ZQuery(JobSailingSchema.PK, sailingPKs));

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

		#region Exchange Rate

		public void TestExchangeRate()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "ITNAP";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			AssertEquals(ZString.Empty, sailing.ExchangeRate);

			var exchangeRate1 = voyage.ExRates.AddNew();
			exchangeRate1.E8_RX_NKExCurrency = "EUR";
			exchangeRate1.E8_VoyageExchangeRate = 0.75m;
			exchangeRate1.E8_RL_NKPort = "ITNAP";
			AssertEquals("EUR ITNAP 0.75", sailing.ExchangeRate);

			var exchangeRate2 = voyage.ExRates.AddNew();
			exchangeRate2.E8_RX_NKExCurrency = "USD";
			exchangeRate2.E8_VoyageExchangeRate = 0.6m;
			exchangeRate2.E8_RL_NKPort = "AUSYD";
			AssertEquals("EUR ITNAP 0.75; USD AUSYD 0.60", sailing.ExchangeRate);

			var exchangeRate3 = voyage.ExRates.AddNew();
			exchangeRate3.E8_RX_NKExCurrency = "SGD";
			exchangeRate3.E8_VoyageExchangeRate = 0.4m;
			exchangeRate3.E8_RL_NKPort = ZString.Empty;
			AssertEquals("EUR ITNAP 0.75; USD AUSYD 0.60; SGD 0.40", sailing.ExchangeRate);

			var exchangeRate4 = voyage.ExRates.AddNew();
			exchangeRate4.E8_RX_NKExCurrency = "HKD";
			exchangeRate4.E8_VoyageExchangeRate = 1m;
			exchangeRate4.E8_RL_NKPort = ZString.Empty;
			AssertEquals("EUR ITNAP 0.75; USD AUSYD 0.60; HKD 1.00; SGD 0.40", sailing.ExchangeRate);

			var exchangeRate5 = voyage.ExRates.AddNew();
			exchangeRate5.E8_RX_NKExCurrency = "EUR";
			exchangeRate5.E8_VoyageExchangeRate = 0.72m;
			exchangeRate5.E8_RL_NKPort = "ITAV8";
			AssertEquals("EUR ITNAP 0.75; USD AUSYD 0.60; HKD 1.00; SGD 0.40", sailing.ExchangeRate);

			var exchangeRate6 = voyage.ExRates.AddNew();
			exchangeRate6.E8_RX_NKExCurrency = "EUR";
			exchangeRate6.E8_VoyageExchangeRate = 0.73m;
			exchangeRate6.E8_RL_NKPort = ZString.Empty;
			AssertEquals("EUR ITNAP 0.75; USD AUSYD 0.60; EUR 0.73; HKD 1.00; SGD 0.40", sailing.ExchangeRate);

			var exchangeRate7 = voyage.ExRates.AddNew();
			exchangeRate7.E8_RX_NKExCurrency = "EUR";
			exchangeRate7.E8_VoyageExchangeRate = 0.74m;
			exchangeRate7.E8_RL_NKPort = "AUSYD";
			AssertEquals("EUR ITNAP 0.75; EUR AUSYD 0.74; USD AUSYD 0.60; HKD 1.00; SGD 0.40", sailing.ExchangeRate);

			var exchangeRate8 = voyage.ExRates.AddNew();
			exchangeRate8.E8_RX_NKExCurrency = "USD";
			exchangeRate8.E8_VoyageExchangeRate = 0.61m;
			exchangeRate8.E8_RL_NKPort = ZString.Empty;
			AssertEquals("EUR ITNAP 0.75; EUR AUSYD 0.74; USD AUSYD 0.60; HKD 1.00; SGD 0.40; USD 0.61", sailing.ExchangeRate);

			var exchangeRate9 = voyage.ExRates.AddNew();
			exchangeRate9.E8_RX_NKExCurrency = "USD";
			exchangeRate9.E8_VoyageExchangeRate = 0.62m;
			exchangeRate9.E8_RL_NKPort = "ITNAP";
			AssertEquals("EUR ITNAP 0.75; USD ITNAP 0.62; EUR AUSYD 0.74; USD AUSYD 0.60; HKD 1.00; SGD 0.40", sailing.ExchangeRate);
		}

		#endregion

		#region Implementation

		BaseJobSailing Sailing;
		VoyageOrigin Origin;
		VoyageDestination Destination;
		JobVoyage Voyage;

		protected override void SetUp()
		{
			base.SetUp();
			FreightTestHelper.TryRemoveJobVoyageAndRelatedSequences();

			Voyage = Factory.New<JobVoyage>();
			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			Voyage.JV_VoyageFlight = "234";
			Voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First().RV_FK;

			Origin = Factory.New<VoyageOrigin>();
			Origin.JA_JV = Voyage.PK;
			Origin.JA_RL_NKPortOfLoading = "AUSYD";

			Destination = Factory.New<VoyageDestination>();
			Destination.JB_JV = Voyage.PK;
			Destination.JB_RL_NKPortOfDischarge = "USLAX";

			Sailing = Factory.New<BaseJobSailing>();
			Sailing.JX_JA = Origin.PK;
			Sailing.JX_JB = Destination.PK;

			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			FreightTestHelper.TryRemoveJobVoyageAndRelatedSequences();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			JobVoyage voyage1 = factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage1.JV_VoyageFlight = "234";
			voyage1.JV_RV_NKVessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First().RV_FK;

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

			Sailing = Factory.New<BaseJobSailing>();
			Sailing.JX_JA = Origin.PK;
			Sailing.JX_JB = Destination.PK;
			return Sailing;
		}

		#endregion
	}
}
