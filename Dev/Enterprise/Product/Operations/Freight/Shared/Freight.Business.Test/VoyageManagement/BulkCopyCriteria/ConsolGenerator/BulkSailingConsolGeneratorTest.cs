using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	public class BulkSailingConsolGeneratorTest : TestCaseWithFactory
	{
		public void TestSetDefaultValues()
		{
			Env.Registry.FreightWeightUnit = Constants.Weight.Kilograms;
			Env.Registry.FreightVolumeUnit = Constants.Volume.CubicMetres;

			BulkSailingConsolGenerator generator = new BulkSailingConsolGeneratorForTest(Factory);
			AssertEquals("WeightUnit", Constants.Weight.Kilograms, generator.WeightUnit);
			AssertEquals("VolumeUnit", Constants.Volume.CubicMetres, generator.VolumeUnit);
		}

		public void TestPropertiesReadOnlyIfNotCreateConsol()
		{
			BulkSailingConsolGenerator generator = new BulkSailingConsolGeneratorForTest(Factory);
			generator.TemplateConsolPK = Factory.New<CommonConsol>().PK;
			AssertEquals("Precondition", false, generator.CreateConsol);
			AssertEquals(false, generator.CreateConsolInfo.ReadOnly);
			AssertEquals(true, generator.AllocateNeutralMasterInfo.ReadOnly);
			AssertEquals(true, generator.WeightInfo.ReadOnly);
			AssertEquals(true, generator.WeightUnitInfo.ReadOnly);
			AssertEquals(true, generator.VolumeInfo.ReadOnly);
			AssertEquals(true, generator.VolumeUnitInfo.ReadOnly);
			AssertEquals(true, generator.CopyShipmentsInfo.ReadOnly);
			AssertEquals(true, generator.CopyRoutingsInfo.ReadOnly);

			generator.CreateConsol = true;
			AssertEquals(false, generator.CreateConsolInfo.ReadOnly);
			AssertEquals(false, generator.AllocateNeutralMasterInfo.ReadOnly);
			AssertEquals(false, generator.WeightInfo.ReadOnly);
			AssertEquals(false, generator.WeightUnitInfo.ReadOnly);
			AssertEquals(false, generator.VolumeInfo.ReadOnly);
			AssertEquals(false, generator.VolumeUnitInfo.ReadOnly);
			AssertEquals(false, generator.CopyShipmentsInfo.ReadOnly);
			AssertEquals(false, generator.CopyRoutingsInfo.ReadOnly);

			generator.TemplateConsolPK = ZGuid.Empty;
			AssertEquals(true, generator.CopyShipmentsInfo.ReadOnly);
		}

		public void TestCopyShipments()
		{
			BulkSailingConsolGenerator generator = new BulkSailingConsolGeneratorForTest(Factory);
			AssertEquals(false, generator.CopyShipments);
			generator.TemplateConsolPK = Factory.New<CommonConsol>().PK;
			AssertEquals(true, generator.CopyShipments);
		}

		public void TestGenerateConsols_FromSailing()
		{
			CreateSailings();
			BaseJobSailingCollection sailings = new BaseJobSailingCollection(Factory);
			sailings.Add(sailing);
			sailings.Add(sailing2);

			BulkSailingConsolGenerator generator = new BulkSailingConsolGeneratorForTest(Factory);
			generator.Weight = 200m;
			generator.WeightUnit = "KG";
			generator.Volume = 2.3m;
			generator.VolumeUnit = "M3";
			generator.AllocateNeutralMaster = true;

			AssertEquals(0, generator.CreatedConsols.Count);
			generator.GenerateConsols(sailings);
			AssertEquals(2, generator.CreatedConsols.Count);

			AssertEquals(sailing.PK, generator.CreatedConsols[0].Transports[0].JW_JX);
			AssertEquals(Core.Constants.TransportModes.Air, generator.CreatedConsols[0].JK_TransportMode);
			AssertEquals("AUSYD", generator.CreatedConsols[0].JK_RL_NKLoadPort);
			AssertEquals("USLAX", generator.CreatedConsols[0].JK_RL_NKDischargePort);
			AssertEquals(200m, generator.CreatedConsols[0].JK_TotalShipmentActWeightCheck);
			AssertEquals("KG", generator.CreatedConsols[0].JK_TotalShipmentChargeableUnit);
			AssertEquals(2.3m, generator.CreatedConsols[0].JK_TotalShipmentActVolumeCheck);
			AssertEquals("M3", generator.CreatedConsols[0].JK_TotalShipmentActOtherUnit);
			AssertEquals(true, generator.CreatedConsols[0].JK_IsNeutralMaster);

			AssertEquals(sailing2.PK, generator.CreatedConsols[1].Transports[0].JW_JX);
			AssertEquals(Core.Constants.TransportModes.Air, generator.CreatedConsols[1].JK_TransportMode);
			AssertEquals("AUSYD", generator.CreatedConsols[0].JK_RL_NKLoadPort);
			AssertEquals("USLAX", generator.CreatedConsols[0].JK_RL_NKDischargePort);
			AssertEquals(200m, generator.CreatedConsols[1].JK_TotalShipmentActWeightCheck);
			AssertEquals("KG", generator.CreatedConsols[1].JK_TotalShipmentChargeableUnit);
			AssertEquals(2.3m, generator.CreatedConsols[1].JK_TotalShipmentActVolumeCheck);
			AssertEquals("M3", generator.CreatedConsols[1].JK_TotalShipmentActOtherUnit);
			AssertEquals(true, generator.CreatedConsols[1].JK_IsNeutralMaster);

			AssertEquals(true, generator.CreatedConsols.ReadOnly);
		}

		public void TestGenerateConsols_WithTemplateConsol()
		{
			CommonConsol existingConsol = Factory.New<CommonConsol>();
			existingConsol.JK_OA_CreditorAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			existingConsol.JK_TotalShipmentActVolumeCheck = 3.1m;
			existingConsol.JK_TotalShipmentChargeableUnit = Constants.Volume.CubicYards;
			existingConsol.Shipments.AddNew();
			existingConsol.Shipments.AddNew();

			CreateSailings();
			BaseJobSailingCollection sailings = new BaseJobSailingCollection(Factory);
			sailings.Add(sailing);
			sailings.Add(sailing2);

			Transport transport1 = existingConsol.Transports.AddNew();
			Transport transport2 = existingConsol.Transports.AddNew();

			BulkSailingConsolGenerator generator = new BulkSailingConsolGeneratorForTest(Factory);
			generator.Weight = 200m;
			generator.WeightUnit = Constants.Weight.Kilograms;
			generator.Volume = 0m;
			generator.VolumeUnit = Constants.Volume.CubicMetres;
			generator.AllocateNeutralMaster = true;
			generator.CopyRoutings = false;

			generator.TemplateConsolPK = existingConsol.PK;

			AssertEquals(0, generator.CreatedConsols.Count);
			generator.GenerateConsols(sailings);
			AssertEquals(2, generator.CreatedConsols.Count);

			AssertEquals(sailing.PK, generator.CreatedConsols[0].Transports[0].JW_JX);
			AssertEquals("", generator.CreatedConsols[0].JK_TransportMode);
			AssertEquals(200m, generator.CreatedConsols[0].JK_TotalShipmentActWeightCheck);
			AssertEquals(Constants.Weight.Kilograms, generator.CreatedConsols[0].JK_TotalShipmentActOtherUnit);
			AssertEquals(3.1m, generator.CreatedConsols[0].JK_TotalShipmentActVolumeCheck);
			AssertEquals(Constants.Volume.CubicYards, generator.CreatedConsols[0].JK_TotalShipmentChargeableUnit);
			AssertEquals(true, generator.CreatedConsols[0].JK_IsNeutralMaster);
			AssertEquals(GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK, generator.CreatedConsols[0].JK_OA_CreditorAddress);
			AssertEquals("2 shipments", 2, generator.CreatedConsols[0].Shipments.Count);
			AssertEquals("one transport", 1, generator.CreatedConsols[0].Transports.Count);

			AssertEquals(sailing2.PK, generator.CreatedConsols[1].Transports[0].JW_JX);
			AssertEquals("", generator.CreatedConsols[1].JK_TransportMode);
			AssertEquals(200m, generator.CreatedConsols[1].JK_TotalShipmentActWeightCheck);
			AssertEquals(Constants.Weight.Kilograms, generator.CreatedConsols[1].JK_TotalShipmentActOtherUnit);
			AssertEquals(3.1m, generator.CreatedConsols[1].JK_TotalShipmentActVolumeCheck);
			AssertEquals(Constants.Volume.CubicYards, generator.CreatedConsols[1].JK_TotalShipmentChargeableUnit);
			AssertEquals(true, generator.CreatedConsols[1].JK_IsNeutralMaster);
			AssertEquals(GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK, generator.CreatedConsols[1].JK_OA_CreditorAddress);
			AssertEquals("2 shipments", 2, generator.CreatedConsols[1].Shipments.Count);
			AssertEquals("1 transport", 1, generator.CreatedConsols[1].Transports.Count);
		}

		public void TestGenerateConsols_WithTemplateConsol_WOShipments()
		{
			CommonConsol existingConsol = Factory.New<CommonConsol>();
			existingConsol.JK_OA_CreditorAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			existingConsol.JK_TotalShipmentActVolumeCheck = 3.1m;
			existingConsol.Shipments.AddNew();
			existingConsol.Shipments.AddNew();

			CreateSailings();
			BaseJobSailingCollection sailings = new BaseJobSailingCollection(Factory);
			sailings.Add(sailing);
			sailings.Add(sailing2);

			BulkSailingConsolGenerator generator = new BulkSailingConsolGeneratorForTest(Factory);
			generator.Weight = 200m;
			generator.WeightUnit = "KG";
			generator.VolumeUnit = "M3";
			generator.AllocateNeutralMaster = true;

			generator.TemplateConsolPK = existingConsol.PK;
			generator.CopyShipments = false;

			AssertEquals(0, generator.CreatedConsols.Count);
			generator.GenerateConsols(sailings);
			AssertEquals(2, generator.CreatedConsols.Count);
			AssertEquals("No shipments", 0, generator.CreatedConsols[0].Shipments.Count);
			AssertEquals("No shipments", 0, generator.CreatedConsols[1].Shipments.Count);
		}

		public void TestGenerateConsols_WithTemplateConsol_WithTransports()
		{
			var curDate = ZDateTime.Today;

			var oldSailing = GetAirSailing(curDate.AddDays(-1), "AUSYD", "USLAX", curDate.AddDays(-1), curDate);

			var existingConsol = Factory.New<CommonConsol>();
			existingConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			existingConsol.JK_OA_CreditorAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			existingConsol.JK_TotalShipmentActVolumeCheck = 3.1m;
			existingConsol.Shipments.AddNew();
			existingConsol.Shipments.AddNew();
			existingConsol.Transports.MostInterestingTransport.JW_IsLinked = true;
			existingConsol.Transports.MostInterestingTransport.JW_JX = oldSailing.PK;

			CreateSailings();
			var sailings = new BaseJobSailingCollection(Factory);
			sailings.Add(sailing);

			var sailing3 = GetAirSailing(curDate.AddDays(11), "USLAX", "USCHI", curDate.AddDays(11), curDate.AddDays(13));
			sailing3.Origin.JA_CutOff = curDate.AddDays(10);
			sailing3.JX_DepotCutOff = curDate.AddDays(12);
			Factory.Save();

			var transport1 = existingConsol.Transports.AddNew();
			transport1.JW_IsLinked = true;
			transport1.JW_JX = sailing3.PK;
			var transport2 = existingConsol.Transports.AddNew();
			transport2.JW_ETD = curDate.AddDays(14);
			transport2.JW_ETA = curDate.AddDays(15);

			var generator = new BulkSailingConsolGeneratorForTest(Factory);
			generator.Weight = 200m;
			generator.WeightUnit = "KG";
			generator.VolumeUnit = "M3";
			generator.AllocateNeutralMaster = true;

			generator.TemplateConsolPK = existingConsol.PK;

			AssertEquals(0, generator.CreatedConsols.Count);
			generator.GenerateConsols(sailings);
			AssertEquals(1, generator.CreatedConsols.Count);

			AssertEquals("3 transports", 3, generator.CreatedConsols[0].Transports.Count);

			AssertEquals(curDate.AddDays(12), generator.CreatedConsols[0].Transports[1].JW_ETD);
			AssertEquals(curDate.AddDays(14), generator.CreatedConsols[0].Transports[1].JW_ETA);
			AssertEquals(curDate.AddDays(11), generator.CreatedConsols[0].Transports[1].JW_TerminalCutOff);
			AssertEquals(curDate.AddDays(13), generator.CreatedConsols[0].Transports[1].JW_DepotCutOff);
			Assert(generator.CreatedConsols[0].Transports[1].JW_IsLinked);
			Assert(!generator.CreatedConsols[0].Transports[1].JW_JX.IsEmpty);

			AssertEquals(curDate.AddDays(15), generator.CreatedConsols[0].Transports[2].JW_ETD);
			AssertEquals(curDate.AddDays(16), generator.CreatedConsols[0].Transports[2].JW_ETA);
			AssertEquals(ZDateTime.Empty, generator.CreatedConsols[0].Transports[2].JW_TerminalCutOff);
			AssertEquals(ZDateTime.Empty, generator.CreatedConsols[0].Transports[2].JW_DepotCutOff);
			AssertEquals(ZDateTime.Empty, generator.CreatedConsols[0].Transports[2].JW_VGMCutOff);
		}

		public void TestGenerateConsols_WithTemplateConsol_WithTransports_EmptyETD()
		{
			ZDateTime curDate = ZDateTime.Today;
			CommonConsol existingConsol = Factory.New<CommonConsol>();
			existingConsol.JK_OA_CreditorAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			existingConsol.JK_TotalShipmentActVolumeCheck = 3.1m;

			CreateSailings();
			BaseJobSailingCollection sailings = new BaseJobSailingCollection(Factory);
			sailings.Add(sailing);

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_FlightDate = curDate.AddDays(11);

			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_JV = voyage.PK;
			origin.JA_RL_NKPortOfLoading = "USLAX";
			origin.JA_E_DEP = curDate.AddDays(11);
			origin.JA_CutOff = curDate.AddDays(10);

			VoyageDestination destination = Factory.New<VoyageDestination>();
			destination.JB_JV = voyage.PK;
			destination.JB_RL_NKPortOfDischarge = "USCHI";
			destination.JB_E_ARV = curDate.AddDays(13);

			JobSailing sailing3 = Factory.New<JobSailing>();
			sailing3.JX_JA = origin.PK;
			sailing3.JX_JB = destination.PK;
			sailing3.JX_DepotCutOff = curDate.AddDays(12);

			Factory.Save();

			Transport transport1 = existingConsol.Transports.AddNew();
			transport1.JW_IsLinked = true;
			transport1.JW_JX = sailing3.PK;

			BulkSailingConsolGenerator generator = new BulkSailingConsolGeneratorForTest(Factory);
			generator.Weight = 200m;
			generator.WeightUnit = "KG";
			generator.VolumeUnit = "M3";
			generator.AllocateNeutralMaster = true;

			generator.TemplateConsolPK = existingConsol.PK;

			AssertEquals(0, generator.CreatedConsols.Count);
			generator.GenerateConsols(sailings);
			AssertEquals(1, generator.CreatedConsols.Count);

			AssertEquals("2 transports", 2, generator.CreatedConsols[0].Transports.Count);

			AssertEquals(ZDateTime.Empty, generator.CreatedConsols[0].Transports[1].JW_ETD);
			AssertEquals(ZDateTime.Empty, generator.CreatedConsols[0].Transports[1].JW_ETA);
			AssertEquals(ZDateTime.Empty, generator.CreatedConsols[0].Transports[1].JW_TerminalCutOff);
			AssertEquals(ZDateTime.Empty, generator.CreatedConsols[0].Transports[1].JW_DepotCutOff);
			AssertEquals(ZDateTime.Empty, generator.CreatedConsols[0].Transports[1].JW_VGMCutOff);
			Assert(generator.CreatedConsols[0].Transports[1].JW_IsLinked);
			Assert(generator.CreatedConsols[0].Transports[1].JW_JX.IsEmpty);
		}

		public void TestGenerateConsols_WithMultipleTransports()
		{
			ZDateTime now = ZDateTime.Now;
			now = new ZDateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

			JobSailing sailing1a = CreateFlight("QF123", "SGSIN", "JPNRT", now.AddHours(10), now.AddHours(14));
			JobSailing sailing1b = CreateFlight("QF124", "JPNRT", "USLAX", now.AddHours(16), now.AddHours(15));

			CommonConsol consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Air;
			consol1.JK_RL_NKLoadPort = "SGSIN";
			consol1.JK_RL_NKDischargePort = "USLAX";

			Transport transport1a = consol1.Transports[0];
			transport1a.JW_IsLinked = true;
			transport1a.JW_JX = sailing1a.PK;

			Transport transport1b = consol1.Transports.AddNew();
			transport1b.JW_IsLinked = true;
			transport1b.JW_JX = sailing1b.PK;

			Factory.Save();

			TimeSpan span = new TimeSpan(1, 1, 0, 0);
			ZDateTime next = now.Add(span);

			JobSailing sailing2a = CreateFlight("QF123", "SGSIN", "JPNRT", next.AddHours(10), next.AddHours(14));

			BulkSailingConsolGenerator generator = new BulkSailingConsolGeneratorForTest(Factory);
			generator.TemplateConsolPK = consol1.PK;
			generator.CreateConsol = true;
			generator.CopyRoutings = true;
			generator.CopyShipments = true;
			generator.GenerateConsols(new BaseJobSailingCollection(Factory) { sailing2a });

			AssertEquals("should have created a single consol", 1, generator.CreatedConsols.Count);
			CommonConsol consol2 = generator.CreatedConsols[0];

			AssertContainsExactElementsInAnyOrder("Load Ports",
				new string[] { "SGSIN", "JPNRT" },
				Array.ConvertAll(consol2.Transports.ToArray<Transport>(), (t) => t.JW_RL_NKLoadPort.ToString()));

			Transport transport2a = consol2.Transports.FindByLoadPort("SGSIN");
			Transport transport2b = consol2.Transports.FindByLoadPort("JPNRT");

			CombineAssertions(delegate
			{
				AssertEquals("consol2.JK_RL_NKLoadPort", "SGSIN", consol2.JK_RL_NKLoadPort);
				AssertEquals("consol2.JK_RL_NKDischargePort", "USLAX", consol2.JK_RL_NKDischargePort);

				AssertEquals("transport2a.JW_IsLinked", true, transport2a.JW_IsLinked);
				AssertEquals("transport2a.JW_VoyageFlight", "QF123", transport2a.JW_VoyageFlight);
				AssertEquals("transport2a.JW_RL_NKLoadPort", "SGSIN", transport2a.JW_RL_NKLoadPort);
				AssertEquals("transport2a.JW_RL_NKDiscPort", "JPNRT", transport2a.JW_RL_NKDiscPort);
				AssertEquals("transport2a.JW_ETD", next.AddHours(10), transport2a.JW_ETD);
				AssertEquals("transport2a.JW_ETA", next.AddHours(14), transport2a.JW_ETA);

				//AssertEquals("transport2b.JW_IsLinked", false, transport2b.JW_IsLinked);
				AssertEquals("transport2a.JW_VoyageFlight", "QF124", transport2b.JW_VoyageFlight);
				AssertEquals("transport2b.JW_RL_NKLoadPort", "JPNRT", transport2b.JW_RL_NKLoadPort);
				AssertEquals("transport2b.JW_RL_NKDiscPort", "USLAX", transport2b.JW_RL_NKDiscPort);
				AssertEquals("transport2b.JW_ETD", next.AddHours(16), transport2b.JW_ETD);
				AssertEquals("transport2b.JW_ETA", next.AddHours(15), transport2b.JW_ETA);
			});
		}

		public void TestSetTemplateConsol()
		{
			BulkSailingConsolGeneratorForTest2 bulkSailingConsolGenerator = new BulkSailingConsolGeneratorForTest2(Factory);
			AssertEquals(false, bulkSailingConsolGenerator.CopyShipments);
			AssertEquals(ZGuid.Empty, bulkSailingConsolGenerator.TemplateConsolPK);

			CommonConsol consol1 = Factory.New<CommonConsol>();
			CommonConsol consol2 = Factory.New<CommonConsol>();

			bulkSailingConsolGenerator.TemplateConsolPK = consol1.PK;
			AssertEquals(consol1, bulkSailingConsolGenerator.TemplateConsol);

			bulkSailingConsolGenerator.TemplateConsolPK = consol2.PK;
			AssertEquals(consol2, bulkSailingConsolGenerator.TemplateConsol);
		}

		[TestDate(2012, 07, 01)]
		public void TestSailingManagerDoesNotTryToRemoveVoyages()
		{
			var firstFlightDepartureDate = new ZDateTime(2012, 08, 01, 9, 10, 0);
			var secondFlightDepartureDate = firstFlightDepartureDate.AddDays(1);

			sailing = GetAirSailing(firstFlightDepartureDate.Date, "USLAX", "AUSYD", firstFlightDepartureDate, firstFlightDepartureDate.AddHours(3));
			sailing2 = GetAirSailing(secondFlightDepartureDate, "USLAX", "AUSYD", secondFlightDepartureDate, secondFlightDepartureDate.AddHours(3));

			var sailings = new BaseJobSailingCollection(Factory);
			sailings.Add(sailing);
			sailings.Add(sailing2);

			var generator = new BulkSailingConsolGeneratorForTest(Factory);

			generator.GenerateConsols(sailings);

			AssertNoExceptionThrown("should not throw CargoWise.EntityFramework.ZSaveException", Factory.Save);
			AssertEquals("should create 2 consols", 2, generator.CreatedConsols.Count);
			AssertNotEquals("consols should be attached to different sailings", generator.CreatedConsols[0].Transports.MostInterestingTransport.JW_JX,
				generator.CreatedConsols[1].Transports.MostInterestingTransport.JW_JX);
		}

		public void TestGenerateConsols_WithTemplateConsol_Road()
		{
			ZDateTime curDate = ZDateTime.Today;
			CommonConsol existingConsol = Factory.New<CommonConsol>();
			existingConsol.JK_AgentType = Constants.AgentType.Agent;
			existingConsol.JK_TransportMode = Constants.TransportModes.Road;
			existingConsol.JK_ConsolMode = Constants.ContainerModes.FTL;
			existingConsol.JK_OA_CreditorAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			existingConsol.JK_RL_NKLoadPort = "PLWRO";
			existingConsol.JK_RL_NKDischargePort = "DEFRA";
			existingConsol.JK_TotalShipmentCountCheck = 30;
			existingConsol.JK_TotalShipmentActWeightCheck = 20000m;
			existingConsol.JK_TotalShipmentActOtherUnit = Constants.Weight.Kilograms;
			existingConsol.JK_TotalShipmentActVolumeCheck = 62m;
			existingConsol.JK_TotalShipmentChargeableUnit = Constants.Volume.CubicMetres;
			existingConsol.JK_TotalShipmentChargableCheck = 25000m;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Road;
			voyage.JV_FlightDate = curDate.AddDays(11);

			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_JV = voyage.PK;
			origin.JA_RL_NKPortOfLoading = "PLWRO";
			origin.JA_E_DEP = curDate.AddDays(11);
			origin.JA_CutOff = curDate.AddDays(10);

			VoyageDestination destination = Factory.New<VoyageDestination>();
			destination.JB_JV = voyage.PK;
			destination.JB_RL_NKPortOfDischarge = "DEFRA";
			destination.JB_E_ARV = curDate.AddDays(12);

			JobSailing sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			sailing.JX_DepotCutOff = curDate.AddDays(12);

			Factory.Save();

			BulkSailingConsolGenerator generator = new BulkSailingConsolGeneratorForTest(Factory);
			generator.CreateConsol = true;
			generator.Weight = 1000m;
			generator.WeightUnit = Constants.Weight.Kilograms;
			generator.Volume = 5m;
			generator.VolumeUnit = Constants.Volume.CubicMetres;
			generator.TemplateConsolPK = existingConsol.PK;
			generator.CopyShipments = false;

			generator.GenerateConsols(new BaseJobSailingCollection(Factory) { sailing });

			AssertEquals("Mode of Transport", Constants.TransportModes.Road, generator.CreatedConsols[0].JK_TransportMode);
			AssertEquals("Unit of Pre-allocated Weight", Constants.Weight.Kilograms, generator.CreatedConsols[0].JK_TotalShipmentActOtherUnit);
			AssertEquals("Unit of Pre-allocated Volume", Constants.Volume.CubicMetres, generator.CreatedConsols[0].JK_TotalShipmentChargeableUnit);
		}

		#region Implementation

		protected JobSailing CreateFlight(ZString flightNo, ZString load, ZString discharge, ZDateTime etd, ZDateTime eta)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = flightNo;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = load;
			origin.JA_E_DEP = etd;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;
			destination.JB_E_ARV = eta;

			voyage.GenerateSailings();

			return voyage.Sailings[0];
		}

		void CreateSailings()
		{
			var today = ZDateTime.Today;
			sailing = GetAirSailing(Factory, today, "AUSYD", "USLAX", today, today.AddDays(10));
			sailing2 = GetAirSailing(today, "AUSYD", "USLAX", today, today.AddDays(10));

			Factory.Save();
		}

		protected BaseJobSailing GetAirSailing(ZDateTime flightDate, ZString load, ZString discharge, ZDateTime eTD, ZDateTime eTA)
		{
			return GetAirSailing(Factory, flightDate, load, discharge, eTD, eTA);
		}

		protected BaseJobSailing GetAirSailing(BusinessObjectFactory factory, ZDateTime flightDate, ZString load, ZString discharge, ZDateTime eTD, ZDateTime eTA)
		{
			var voyage = factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_FlightDate = flightDate;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = load;
			origin.JA_E_DEP = eTD;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;
			destination.JB_E_ARV = eTA;

			voyage.GenerateSailings();

			return voyage.Sailings[0];
		}

		class BulkSailingConsolGeneratorForTest2 : BulkSailingConsolGeneratorForTest
		{
			public BulkSailingConsolGeneratorForTest2(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public new CommonConsol TemplateConsol
			{
				get { return base.TemplateConsol; }
			}
		}

		BaseJobSailing sailing;
		BaseJobSailing sailing2;

		#endregion
	}
}
