using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(BulkMovementsApplicator))]
	internal class BulkMovementsApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestDontAddDuplicates()
		{
			ZDateTime epoch = new ZDateTime(2011, 08, 16, 13, 51, 0);
			OrgHeader amsDepot = Factory.NewWithValidTestData<OrgHeader>();
			amsDepot.OH_RL_NKClosestPort = "NLAMS";
			RefContainerStock stock1 = Factory.New<RefContainerStock>();
			stock1.R6_ContainerNum = "TEST4100013";
			stock1.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			RefContainerStock stock2 = Factory.New<RefContainerStock>();
			stock2.R6_ContainerNum = "TEST4100029";
			stock2.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			ContainerMovement movement1 = stock1.Movements.AddNew();
			movement1.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			movement1.E9_MovementDate = epoch.AddSeconds(1);
			Factory.Save();
			Applicator.MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			Applicator.MovementDate = epoch.AddSeconds(29);
			Applicator.DepotAddressPK = amsDepot.MainAddress.PK;
			RefContainerStock[] targets = new[] { stock1, stock2 };
			const string expectedLog = "WARNING: [HL TEST4100013] already has a [HL Wharf Gate In] movement at 16-Aug-11 13:51";
			const string expectedMovements1 = @"
+000: <null> - WGI []
";
			const string expectedMovements2 = @"
+000: NLAMS - WGI []
";
			CombineAssertions(delegate
			{
				ApplyApplicator(targets, expectedLog, true);
				AssertMultilineASCIIEquals("Stock 1", expectedMovements1, Render_BySeconds(epoch, stock1.Movements));
				AssertMultilineASCIIEquals("Stock 2", expectedMovements2, Render_BySeconds(epoch, stock2.Movements));
			});
		}

		public void TestSailingSelection()
		{
			ZDateTime epoch = ZDateTime.Today.AddDays(-100);
			OrgHeader amsDepot = Factory.NewWithValidTestData<OrgHeader>();
			amsDepot.OH_RL_NKClosestPort = "NLAMS";
			OrgHeader lonDepot = Factory.NewWithValidTestData<OrgHeader>();
			lonDepot.OH_RL_NKClosestPort = "GBLON";
			OrgHeader sinDepot = Factory.NewWithValidTestData<OrgHeader>();
			sinDepot.OH_RL_NKClosestPort = "SGSIN";
			OrgHeader cnsDepot = Factory.NewWithValidTestData<OrgHeader>();
			cnsDepot.OH_RL_NKClosestPort = "AUCNS";
			OrgHeader sydDepot = Factory.NewWithValidTestData<OrgHeader>();
			sydDepot.OH_RL_NKClosestPort = "AUSYD";
			OrgHeader aklDepot = Factory.NewWithValidTestData<OrgHeader>();
			aklDepot.OH_RL_NKClosestPort = "NZAKL";
			JobSailing[] sailings = { NewSailing("NORDCLOUD", "001", "NLAMS", epoch.AddDays(0), "GBLON", epoch.AddDays(5)), NewSailing("NORDCLOUD", "002", "GBLON", epoch.AddDays(10), "SGSIN", epoch.AddDays(20)), NewSailing("MAJAPAHIT", "003", "SGSIN", epoch.AddDays(24), "AUBNE", epoch.AddDays(28)), NewSailing("MAJAPAHIT", "004", "AUBNE", epoch.AddDays(36), "AUSYD", epoch.AddDays(40)), NewSailing("BANOWATI", "005", "AUSYD", epoch.AddDays(45), "NZAKL", epoch.AddDays(50)), };
			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_JX = sailings[2].PK;
			Transport[] transports = new Transport[sailings.Length];
			for (int i = 0; i < sailings.Length; i++)
			{
				if (sailings[i].PK == bill.JS_JX)
				{
					continue;
				}

				Transport transport = bill.Transports.AddNew();
				transport.JW_IsLinked = true;
				transport.JW_JX = sailings[i].PK;
				transports[i] = transport;
			}

			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = "TEST4100013";
			container.JC_RC = stock.R6_RC;
			Factory.Save();
			var list = new[] { new
			{
			Depot = amsDepot, Days = -5, Type = ContainerMovementTypes.Codes.YardGateOut
			}

			, new
			{
			Depot = amsDepot, Days = -4, Type = ContainerMovementTypes.Codes.DepotGateIn
			}

			, new
			{
			Depot = amsDepot, Days = -3, Type = ContainerMovementTypes.Codes.DepotGateOut
			}

			, new
			{
			Depot = amsDepot, Days = -2, Type = ContainerMovementTypes.Codes.WharfGateIn
			}

			, new
			{
			Depot = amsDepot, Days = -1, Type = ContainerMovementTypes.Codes.Load
			}

			, new
			{
			Depot = lonDepot, Days = 5, Type = ContainerMovementTypes.Codes.Discharge
			}

			, new
			{
			Depot = lonDepot, Days = 6, Type = ContainerMovementTypes.Codes.Load
			}

			, new
			{
			Depot = sinDepot, Days = 23, Type = ContainerMovementTypes.Codes.Discharge
			}

			, new
			{
			Depot = sinDepot, Days = 24, Type = ContainerMovementTypes.Codes.Load
			}

			, new
			{
			Depot = cnsDepot, Days = 27, Type = ContainerMovementTypes.Codes.Discharge
			}

			, new
			{
			Depot = cnsDepot, Days = 37, Type = ContainerMovementTypes.Codes.Load
			}

			, new
			{
			Depot = sydDepot, Days = 40, Type = ContainerMovementTypes.Codes.Discharge
			}

			, new
			{
			Depot = sydDepot, Days = 46, Type = ContainerMovementTypes.Codes.Load
			}

			, new
			{
			Depot = aklDepot, Days = 51, Type = ContainerMovementTypes.Codes.Discharge
			}

			, new
			{
			Depot = aklDepot, Days = 52, Type = ContainerMovementTypes.Codes.WharfGateOut
			}

			, new
			{
			Depot = aklDepot, Days = 53, Type = ContainerMovementTypes.Codes.DepotGateIn
			}

			, new
			{
			Depot = aklDepot, Days = 54, Type = ContainerMovementTypes.Codes.DepotGateOut
			}

			, new
			{
			Depot = aklDepot, Days = 55, Type = ContainerMovementTypes.Codes.YardGateIn
			}

			, };
			BusinessObject[] targets = { container };
			foreach (var settings in list)
			{
				Applicator.MovementType = settings.Type;
				Applicator.MovementDate = epoch.AddDays(settings.Days);
				Applicator.DepotAddressPK = settings.Depot.MainAddress.PK;
				SimulateRun(targets, false);
			}

			Factory.Save();
			ContainerMovement[] movements = stock.Movements.ToArray();
			Array.Sort(movements, (m1, m2) => m1.E9_MovementDate.CompareTo(m2.E9_MovementDate));
			const string expected1 = @"
-05: NLAMS - YGO [NORDCLOUD/001]
-04: NLAMS - DGI [NORDCLOUD/001]
-03: NLAMS - DGO [NORDCLOUD/001]
-02: NLAMS - WGI [NORDCLOUD/001]
-01: NLAMS - LOD [NORDCLOUD/001]
+05: GBLON - DIS [NORDCLOUD/001]
+06: GBLON - LOD [NORDCLOUD/002]
+23: SGSIN - DIS [NORDCLOUD/002]
+24: SGSIN - LOD [MAJAPAHIT/003]
+27: AUCNS - DIS [MAJAPAHIT/003]
+37: AUCNS - LOD [MAJAPAHIT/004]
+40: AUSYD - DIS [MAJAPAHIT/004]
+46: AUSYD - LOD [BANOWATI/005]
+51: NZAKL - DIS [BANOWATI/005]
+52: NZAKL - WGO [BANOWATI/005]
+53: NZAKL - DGI [BANOWATI/005]
+54: NZAKL - DGO [BANOWATI/005]
+55: NZAKL - YGI [BANOWATI/005]
";
			const string expected2 = @"
-05: NLAMS - YGO [NORDCLOUD/002]
-04: NLAMS - DGI [NORDCLOUD/002]
-03: NLAMS - DGO [NORDCLOUD/002]
-02: NLAMS - WGI [NORDCLOUD/002]
-01: NLAMS - LOD [NORDCLOUD/002]
+05: GBLON - DIS [NORDCLOUD/002]
+06: GBLON - LOD [NORDCLOUD/002]
+23: SGSIN - DIS [NORDCLOUD/002]
+24: SGSIN - LOD [MAJAPAHIT/003]
+27: AUCNS - DIS [MAJAPAHIT/003]
+37: AUCNS - LOD [MAJAPAHIT/004]
+40: AUSYD - DIS [MAJAPAHIT/004]
+46: AUSYD - LOD [MAJAPAHIT/004]
+51: NZAKL - DIS [MAJAPAHIT/004]
+52: NZAKL - WGO [MAJAPAHIT/004]
+53: NZAKL - DGI [MAJAPAHIT/004]
+54: NZAKL - DGO [MAJAPAHIT/004]
+55: NZAKL - YGI [MAJAPAHIT/004]
";
			AssertMultilineASCIIEquals("", expected1, Render_ByDay(epoch, movements));
			transports[0].JW_IsLinked = false;
			transports[4].JW_IsLinked = false;
			stock.Movements.DeleteAll();
			Factory.Save();
			foreach (var settings in list)
			{
				Applicator.MovementType = settings.Type;
				Applicator.MovementDate = epoch.AddDays(settings.Days);
				Applicator.DepotAddressPK = settings.Depot.MainAddress.PK;
				SimulateRun(targets, false);
			}

			Factory.Save();
			movements = stock.Movements.ToArray();
			Array.Sort(movements, (m1, m2) => m1.E9_MovementDate.CompareTo(m2.E9_MovementDate));
			AssertMultilineASCIIEquals("", expected2, Render_ByDay(epoch, movements));
		}

		public void TestRunOnContainers()
		{
			ZDateTime now = new ZDateTime(2010, 6, 7, 12, 30, 00);
			ZGuid container1bPK;
			ZGuid container1cPK;
			ZGuid container2aPK;
			ZGuid container2bPK;
			{
				BusinessObjectFactory setupFactory = new BusinessObjectFactory();
				OrgHeader depot1 = setupFactory.NewWithValidTestData<OrgHeader>();
				depot1.OH_Code = "Depot1";
				OrgHeader depot2 = setupFactory.NewWithValidTestData<OrgHeader>();
				depot2.OH_Code = "Depot2";
				JobVoyage voyage1 = setupFactory.New<JobVoyage>();
				voyage1.JV_RV_NKVessel = RefVessel.LookupVesselByName("BANOWATI", Factory).First().RV_FK;
				voyage1.JV_VoyageFlight = "V1234N";
				voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
				JobVoyage voyage2 = setupFactory.New<JobVoyage>();
				voyage2.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
				voyage2.JV_VoyageFlight = "V1234S";
				voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
				voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
				BillOfLading bill1 = setupFactory.New<BillOfLading>();
				bill1.JS_JX = voyage1.Sailings[0].PK;
				BillOfLading bill2 = setupFactory.New<BillOfLading>();
				bill2.JS_JX = voyage2.Sailings[0].PK;
				RefContainerStock stock1 = setupFactory.New<RefContainerStock>();
				stock1.R6_ContainerNum = "TEST4100013";
				stock1.R6_RC = setupFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				RefContainerStock stock2 = setupFactory.New<RefContainerStock>();
				stock2.R6_ContainerNum = "TEST4100029";
				stock2.R6_RC = setupFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				BillOfLadingContainer container1b = bill1.RealContainers.AddNew();
				container1b.JC_ContainerNum = stock2.R6_ContainerNum;
				BillOfLadingContainer container1c = bill1.RealContainers.AddNew();
				container1c.JC_ContainerNum = "TEST4100034";
				container1c.JC_RC = setupFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				if (container1c.Stock != null)
				{
					container1c.Stock.Delete();
				}

				BillOfLadingContainer container2a = bill2.RealContainers.AddNew();
				container2a.JC_ContainerNum = stock1.R6_ContainerNum;
				BillOfLadingContainer container2b = bill2.RealContainers.AddNew();
				container2b.JC_ContainerNum = stock2.R6_ContainerNum;
				setupFactory.Save();
				Applicator.MovementType = ContainerMovementTypes.Codes.Load;
				Applicator.MovementDate = now.AddDays(-2);
				Applicator.DepotAddressPK = depot1.MainAddress.PK;
				ApplyApplicator(new BusinessObject[] { container1b, container1c }, "WARNING: Adding [HL TEST4100034] to the Container Manager Module");
				Applicator.MovementType = ContainerMovementTypes.Codes.Discharge;
				Applicator.MovementDate = now.AddDays(-1);
				Applicator.DepotAddressPK = depot2.MainAddress.PK;
				ApplyApplicator(new BusinessObject[] { container2a, container2b }, "");
				setupFactory.Save();
				container1bPK = container1b.PK;
				container1cPK = container1c.PK;
				container2aPK = container2a.PK;
				container2bPK = container2b.PK;
			}

			{
				// The results need to be asserted in another factory for some bugs to show.
				BusinessObjectFactory assertFactory = new BusinessObjectFactory();
				BillOfLadingContainer container1b = assertFactory.Load<BillOfLadingContainer>(container1bPK);
				BillOfLadingContainer container1c = assertFactory.Load<BillOfLadingContainer>(container1cPK);
				BillOfLadingContainer container2a = assertFactory.Load<BillOfLadingContainer>(container2aPK);
				BillOfLadingContainer container2b = assertFactory.Load<BillOfLadingContainer>(container2bPK);
				RefContainerStock stock1 = RefContainerStock.Load(assertFactory, "TEST4100013");
				RefContainerStock stock2 = RefContainerStock.Load(assertFactory, "TEST4100029");
				RefContainerStock stock3 = RefContainerStock.Load(assertFactory, "TEST4100034");
				CombineAssertions(delegate
				{
					AssertContainsExactElementsInAnyOrder("Movements 1", new string[] { "Depot2-DIS-MAJAPAHIT/V1234S) 2010-Jun-06 12:30", }, Array.ConvertAll(stock1.Movements.ToArray(), Render));
					AssertContainsExactElementsInAnyOrder("Movements 2", new string[] { "Depot2-DIS-MAJAPAHIT/V1234S) 2010-Jun-06 12:30", "Depot1-LOD-BANOWATI/V1234N) 2010-Jun-05 12:30", }, Array.ConvertAll(stock2.Movements.ToArray(), Render));
					AssertContainsExactElementsInAnyOrder("Movements 3", new string[] { "Depot1-LOD-BANOWATI/V1234N) 2010-Jun-05 12:30", }, Array.ConvertAll(stock3.Movements.ToArray(), Render));
				});
			}
		}

		public void TestRunOnStock()
		{
			ZDateTime now = new ZDateTime(2010, 6, 7, 12, 30, 00);
			{
				BusinessObjectFactory setupFactory = new BusinessObjectFactory();
				OrgHeader depot1 = setupFactory.NewWithValidTestData<OrgHeader>();
				depot1.OH_Code = "Depot1";
				OrgHeader depot2 = setupFactory.NewWithValidTestData<OrgHeader>();
				depot2.OH_Code = "Depot2";
				RefContainerStock stock1 = setupFactory.New<RefContainerStock>();
				stock1.R6_ContainerNum = "TEST4100013";
				stock1.R6_RC = setupFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				RefContainerStock stock2 = setupFactory.New<RefContainerStock>();
				stock2.R6_ContainerNum = "TEST4100029";
				stock2.R6_RC = setupFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				RefContainerStock stock3 = setupFactory.New<RefContainerStock>();
				stock3.R6_ContainerNum = "Test4100034";
				stock3.R6_RC = setupFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				setupFactory.Save();
				Applicator.MovementType = ContainerMovementTypes.Codes.Discharge;
				Applicator.MovementDate = now.AddDays(-2);
				Applicator.DepotAddressPK = depot1.MainAddress.PK;
				ApplyApplicator(new BusinessObject[] { stock1, stock2 }, "");
				Applicator.MovementType = ContainerMovementTypes.Codes.Load;
				Applicator.MovementDate = now.AddDays(-1);
				Applicator.DepotAddressPK = depot2.MainAddress.PK;
				ApplyApplicator(new BusinessObject[] { stock2, stock3 }, "");
				setupFactory.Save();
			}

			{
				// The results need to be asserted in another factory for some bugs to show.
				BusinessObjectFactory assertFactory = new BusinessObjectFactory();
				RefContainerStock stock1 = RefContainerStock.Load(assertFactory, "TEST4100013");
				RefContainerStock stock2 = RefContainerStock.Load(assertFactory, "TEST4100029");
				RefContainerStock stock3 = RefContainerStock.Load(assertFactory, "TEST4100034");
				CombineAssertions(delegate
				{
					AssertContainsExactElementsInAnyOrder("Movements 1", new string[] { "Depot1-DIS-<null>) 2010-Jun-05 12:30", }, Array.ConvertAll(stock1.Movements.ToArray(), Render));
					AssertContainsExactElementsInAnyOrder("Movements 2", new string[] { "Depot1-DIS-<null>) 2010-Jun-05 12:30", "Depot2-LOD-<null>) 2010-Jun-06 12:30", }, Array.ConvertAll(stock2.Movements.ToArray(), Render));
					AssertContainsExactElementsInAnyOrder("Movements 3", new string[] { "Depot2-LOD-<null>) 2010-Jun-06 12:30", }, Array.ConvertAll(stock3.Movements.ToArray(), Render));
				});
			}
		}

		public void TestNoVoyage()
		{
			const string expected = "WARNING: No voyage found for [HL V00000100].";
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_UniqueConsignRef = "V00000100";
			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = stock.R6_ContainerNum;
			container.JC_RC = stock.R6_RC;
			Factory.Save();
			ApplyApplicator(new BusinessObject[] { container }, expected);
		}

		public void TestInvalidContainerNumber_NotAllowed()
		{
			AgencyRegistry.Instance.AllowNonStandardContainerNumbersInContainerManager.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			const string expected = "ERROR: [HL FROG0000001] is not a valid container number.";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();
			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_UniqueConsignRef = "V00000100";
			bill.JS_JX = voyage.Sailings[0].PK;
			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = "FROG0000001";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Factory.Save();
			ApplyApplicator(new BusinessObject[] { container }, expected);
		}

		public void TestInvalidContainerNumber_Allowed()
		{
			AgencyRegistry.Instance.AllowNonStandardContainerNumbersInContainerManager.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			const string expected = "WARNING: Adding [HL FROG0000001] to the Container Manager Module";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();
			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_UniqueConsignRef = "V00000100";
			bill.JS_JX = voyage.Sailings[0].PK;
			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = "FROG0000001";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			if (container.Stock != null)
			{
				container.Stock.Delete();
			}

			Factory.Save();
			ApplyApplicator(new BusinessObject[] { container }, expected);
		}

		public void TestInvalidContainerNumber_MissingContainerNumber()
		{
			AgencyRegistry.Instance.AllowNonStandardContainerNumbersInContainerManager.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			const string expected = "ERROR: Found a [HL container] without a container number.";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();
			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_UniqueConsignRef = "V00000100";
			bill.JS_JX = voyage.Sailings[0].PK;
			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = "";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Factory.Save();
			ApplyApplicator(new BusinessObject[] { container }, expected);
		}

		public void TestMissingContainerType()
		{
			const string expected = "ERROR: [HL TEST4100013] does not have a container type.";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();
			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_UniqueConsignRef = "V00000100";
			bill.JS_JX = voyage.Sailings[0].PK;
			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = "TEST4100013";
			container.JC_RC = ZGuid.Empty;
			Factory.Save();
			ApplyApplicator(new BusinessObject[] { container }, expected);
		}

		public void TestMissingStock()
		{
			const string expected = "WARNING: Adding [HL TEST4100013] to the Container Manager Module";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();
			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_UniqueConsignRef = "V00000100";
			bill.JS_JX = voyage.Sailings[0].PK;
			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = "TEST4100013";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			if (container.Stock != null)
			{
				container.Stock.Delete();
			}

			Factory.Save();
			ApplyApplicator(new BusinessObject[] { container }, expected);
		}

		#region Implementation
		static string Render_ByDay(ZDateTime epoch, IEnumerable<ContainerMovement> movements)
		{
			return Render(epoch, movements, (m) => ((int)Math.Floor(m.TotalDays)).ToString("+00;-00"));
		}

		static string Render_BySeconds(ZDateTime epoch, IEnumerable<ContainerMovement> movements)
		{
			return Render(epoch, movements, (m) => ((int)Math.Floor(m.TotalSeconds)).ToString("+000;-000"));
		}

		static string Render(ZDateTime epoch, IEnumerable<ContainerMovement> movements, Converter<TimeSpan, string> spanFormatter)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine();
			foreach (ContainerMovement movement in movements)
			{
				builder.Append(spanFormatter(movement.E9_MovementDate - epoch));
				builder.Append(": ");
				if (movement.Depot == null || movement.Depot.EffectiveRelatedPortCode == null)
				{
					builder.Append("<null>");
				}
				else
				{
					builder.Append(movement.Depot.EffectiveRelatedPortCode.RL_Code);
				}

				builder.Append(" - ");
				builder.Append(movement.E9_MovementType);
				builder.Append(" [");
				if (movement.Voyage != null)
				{
					builder.Append(movement.Voyage.JV_RV_NKVessel);
					builder.Append("/");
					builder.Append(movement.Voyage.JV_VoyageFlight);
				}

				builder.AppendLine("]");
			}

			return builder.ToString();
		}

		string Render(ContainerMovement movement)
		{
			OrgAddress address;
			OrgHeader depot;
			JobVoyage voyage;
			StringBuilder builder = new StringBuilder();
			if ((address = movement.Depot) == null || (depot = address.Header) == null)
			{
				builder.Append("<null>");
			}
			else
			{
				builder.Append(depot.OH_Code);
			}

			builder.Append("-");
			builder.Append(movement.E9_MovementType);
			builder.Append("-");
			if ((voyage = movement.Voyage) == null)
			{
				builder.Append("<null>");
			}
			else
			{
				builder.Append(voyage.JV_RV_NKVessel);
				builder.Append("/");
				builder.Append(voyage.JV_VoyageFlight);
			}

			builder.Append(") ");
			builder.Append(movement.E9_MovementDate.ToString("yyyy-MMM-dd hh:mm"));
			return builder.ToString();
		}

		JobSailing NewSailing(ZString vessel, ZString voyage, ZString origin, ZDateTime etd, ZString destination, ZDateTime eta)
		{
			JobVoyage voy = Factory.New<JobVoyage>();
			voy.JV_RV_NKVessel = vessel;
			voy.JV_VoyageFlight = voyage;
			VoyageOrigin voyOrigin = voy.Origins.AddNew();
			voyOrigin.JA_RL_NKPortOfLoading = origin;
			voyOrigin.JA_E_DEP = etd;
			VoyageDestination voyDest = voy.Destinations.AddNew();
			voyDest.JB_RL_NKPortOfDischarge = destination;
			voyDest.JB_E_ARV = eta;
			return voy.Sailings[0];
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BulkMovementsApplicator(Factory);
		}
		#endregion

		new BulkMovementsApplicator Applicator => (BulkMovementsApplicator)base.Applicator;
	}
}
