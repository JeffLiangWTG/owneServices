using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business.Testing
{
	internal abstract class SailingFilterBuilderTest<T> : BaseFreightTest
			where T : BusinessObject
	{
		public void TestIsArchived()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "TITANIC";

			var voyage1 = CreateVoyage(vessel, "ABC123", Core.Constants.TransportModes.Sea);
			var voyage2 = CreateVoyage(vessel, "ABC123", Core.Constants.TransportModes.Sea, true);

			var sailing1 = GetOrCreateSailing(voyage1, "AUSYD", "MDKIV");
			var sailing2 = GetOrCreateSailing(voyage2, "AUSYD", "MDKIV");

			Factory.Save();

			MarkProxiedJobsAsLinked();
			var bOs1 = CreateBusinessObjects(sailing1);
			var bOs2 = CreateBusinessObjects(sailing2);
			var linkedBOs2 = bOs2.Where(IsLinked).ToArray();
			var nonLinkedBOs2 = bOs2.Except(linkedBOs2).ToArray();

			Factory.Save();

			Builder.Reset();
			Builder.Vessel = "TITANIC";
			Builder.VoyageFlight = "ABC123";
			Builder.IncludeArchived = false;
			var found = Load(GetFilter(Builder));

			AssertCollectionContains("", bOs1, found);
			AssertCollectionContains("", nonLinkedBOs2, found);
			AssertCollectionNotContains("", linkedBOs2, found);

			Builder.IncludeArchived = true;
			found = Load(GetFilter(Builder));

			AssertCollectionContains("", bOs1, found);
			AssertCollectionContains("", bOs2, found);
		}

		protected virtual void MarkProxiedJobsAsLinked()
		{
		}

		public void TestTransportMode()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "vessel";

			JobVoyage voyage1 = CreateVoyage(vessel, "voyage", Core.Constants.TransportModes.Sea);
			JobVoyage voyage2 = CreateVoyage(vessel, "voyage", Core.Constants.TransportModes.Rail);

			JobSailing sailing1 = GetOrCreateSailing(voyage1, HomePort, OverseasPort);
			JobSailing sailing2 = GetOrCreateSailing(voyage2, HomePort, OverseasPort);

			Factory.Save();

			T[] bOs1 = CreateBusinessObjects(sailing1);
			T[] bOs2 = CreateBusinessObjects(sailing2);

			Factory.Save();

			Builder.Reset();
			T[] found;

			Builder.TransportMode = Core.Constants.TransportModes.Sea;
			found = Load(GetFilter(Builder));
			AssertCollectionContains("", bOs1, found);
			AssertCollectionNotContains("", bOs2, found);

			Builder.TransportMode = Core.Constants.TransportModes.Rail;
			found = Load(GetFilter(Builder));
			AssertCollectionNotContains("", bOs1, found);
			AssertCollectionContains("", bOs2, found);

			Builder.TransportMode = "";
			found = Load(GetFilter(Builder));
			AssertCollectionContains("", bOs1, found);
			AssertCollectionContains("", bOs2, found);
		}

		public void TestVessel()
		{
			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Name = "GreenBeard";

			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Name = "BlueBoast";

			JobVoyage voyage1 = CreateVoyage(vessel1, "1234", Core.Constants.TransportModes.Sea);
			JobVoyage voyage2 = CreateVoyage(vessel2, "1234", Core.Constants.TransportModes.Sea);

			JobSailing sailing1 = GetOrCreateSailing(voyage1, HomePort, OverseasPort);
			JobSailing sailing2 = GetOrCreateSailing(voyage2, HomePort, OverseasPort);

			Factory.Save();

			T[] bOs1 = CreateBusinessObjects(sailing1);
			T[] bOs2 = CreateBusinessObjects(sailing2);

			Factory.Save();

			Builder.Reset();
			T[] found;

			Builder.Vessel = "GreenBeard";
			found = Load(GetFilter(Builder));
			AssertCollectionContains("", bOs1, found);
			AssertCollectionNotContains("", bOs2, found);

			Builder.Vessel = "BlueBoast";
			found = Load(GetFilter(Builder));
			AssertCollectionNotContains("", bOs1, found);
			AssertCollectionContains("", bOs2, found);

			Builder.Vessel = "";
			found = Load(GetFilter(Builder));
			AssertCollectionContains("", bOs1, found);
			AssertCollectionContains("", bOs2, found);

			Builder.VoyageFlight = "";
			Builder.Vessel = "";
			Builder.VoyageFlightComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			found = Load(GetFilter(Builder));
			AssertCollectionContains("Contains BOs1", bOs1, found);
			AssertCollectionContains("Contains BOs2", bOs2, found);

			Builder.Vessel = "e";
			Builder.VoyageFlightComparisonOperator = SQLComparisonOperator.Contains;
			found = Load(GetFilter(Builder));
			AssertCollectionContains("Contains BOs1", bOs1, found);
			AssertCollectionContains("Contains BOs2", bOs2, found);

			Builder.Vessel = "ee";
			Builder.VoyageFlightComparisonOperator = SQLComparisonOperator.Contains;
			found = Load(GetFilter(Builder));
			AssertCollectionContains("Contains BOs1", bOs1, found);
			AssertCollectionNotContains("Not contains BOs2", bOs2, found);
		}

		public void TestVoyage()
		{
			JobVoyage voyage1 = CreateVoyage(TestVessel1, "1234", Core.Constants.TransportModes.Sea);
			JobVoyage voyage2 = CreateVoyage(TestVessel1, "4321", Core.Constants.TransportModes.Sea);

			JobSailing sailing1 = GetOrCreateSailing(voyage1, HomePort, OverseasPort);
			JobSailing sailing2 = GetOrCreateSailing(voyage2, HomePort, OverseasPort);

			Factory.Save();

			T[] bOs1 = CreateBusinessObjects(sailing1);
			T[] bOs2 = CreateBusinessObjects(sailing2);

			Factory.Save();

			Builder.Reset();
			T[] found;

			Builder.VoyageFlight = "1234";
			found = Load(GetFilter(Builder));
			AssertCollectionContains("", bOs1, found);
			AssertCollectionNotContains("", bOs2, found);

			Builder.VoyageFlight = "4321";
			found = Load(GetFilter(Builder));
			AssertCollectionNotContains("", bOs1, found);
			AssertCollectionContains("", bOs2, found);

			Builder.VoyageFlight = "";
			found = Load(GetFilter(Builder));
			AssertCollectionContains("", bOs1, found);
			AssertCollectionContains("", bOs2, found);

			Builder.VoyageFlight = "";
			Builder.VoyageFlightComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			found = Load(GetFilter(Builder));
			AssertCollectionContains("Contains BOs1", bOs1, found);
			AssertCollectionContains("Contains BOs2", bOs2, found);

			Builder.VoyageFlight = "2";
			Builder.VoyageFlightComparisonOperator = SQLComparisonOperator.Contains;
			found = Load(GetFilter(Builder));
			AssertCollectionContains("Contains BOs1", bOs1, found);
			AssertCollectionContains("Contains BOs2", bOs2, found);

			Builder.VoyageFlight = "23";
			Builder.VoyageFlightComparisonOperator = SQLComparisonOperator.Contains;
			found = Load(GetFilter(Builder));
			AssertCollectionContains("Contains BOs1", bOs1, found);
			AssertCollectionNotContains("Not contains BOs2", bOs2, found);
		}

		public void TestLoadPort()
		{
			JobVoyage voyage = CreateVoyage(TestVessel1, "2345", Core.Constants.TransportModes.Sea);
			JobSailing exportSailing = GetOrCreateSailing(voyage, HomePort, OverseasPort);
			JobSailing exportSailing1 = GetOrCreateSailing(voyage, AlternateHomePort, OverseasPort);
			JobSailing importSailing = GetOrCreateSailing(voyage, OverseasPort2, HomePort);

			Factory.Save();

			T[] bOs1 = CreateBusinessObjects(exportSailing);
			T[] bOs2 = CreateBusinessObjects(exportSailing1);
			T[] bOs3 = CreateBusinessObjects(importSailing);

			Factory.Save();

			Builder.Reset();
			T[] found;

			Builder.LoadPort = HomePort;
			found = Load(GetFilter(Builder));
			AssertCollectionContains("", bOs1, found);
			AssertCollectionNotContains("", bOs2, found);
			AssertCollectionNotContains("", bOs3, found);

			Builder.LoadPort = HomePort.SubstringSafe(0, 2);
			found = Load(GetFilter(Builder));
			AssertCollectionContains("", bOs1, found);
			AssertCollectionContains("", bOs2, found);
			AssertCollectionNotContains("", bOs3, found);

			Builder.LoadPort = "";
			found = Load(GetFilter(Builder));
			AssertCollectionContains("", bOs1, found);
			AssertCollectionContains("", bOs2, found);
			AssertCollectionContains("", bOs3, found);
		}

		public void TestLoadPort_Zone()
		{
			JobVoyage voyage = CreateVoyage(TestVessel1, "2345", Core.Constants.TransportModes.Sea);
			JobSailing sailing1 = GetOrCreateSailing(voyage, "AUSYD", "USLAX");
			JobSailing sailing2 = GetOrCreateSailing(voyage, "USLAX", "AUSYD");

			Factory.Save();

			T[] bOs1 = CreateBusinessObjects(sailing1);
			T[] bOs2 = CreateBusinessObjects(sailing2);

			Factory.Save();

			Builder.Reset();
			T[] found;

			Builder.LoadPort = "AU";
			found = Load(GetFilter(Builder));
			AssertCollectionContains("", bOs1, found);
			AssertCollectionNotContains("", bOs2, found);

			Builder.LoadPort = "AUEC";
			found = Load(GetFilter(Builder));
			AssertCollectionContains("", bOs1, found);
			AssertCollectionNotContains("", bOs2, found);

			Builder.LoadPort = "";
			found = Load(GetFilter(Builder));
			AssertCollectionContains("", bOs1, found);
			AssertCollectionContains("", bOs2, found);

			Builder.LoadPort = "IN";
			found = Load(GetFilter(Builder));
			AssertCollectionNotContains("", bOs1, found);
			AssertCollectionNotContains("", bOs2, found);
		}

		public void TestDischargePort()
		{
			JobVoyage voyage = CreateVoyage(TestVessel1, "2345", Core.Constants.TransportModes.Sea);
			JobSailing importSailing = GetOrCreateSailing(voyage, OverseasPort, HomePort);
			JobSailing importSailing1 = GetOrCreateSailing(voyage, OverseasPort, AlternateHomePort);
			JobSailing exportSailing = GetOrCreateSailing(voyage, HomePort, OverseasPort2);

			Factory.Save();

			T[] bOs1 = CreateBusinessObjects(importSailing);
			T[] bOs2 = CreateBusinessObjects(importSailing1);
			T[] bOs3 = CreateBusinessObjects(exportSailing);

			Factory.Save();

			Builder.Reset();
			T[] found;

			Builder.DischargePort = HomePort;
			found = Load(GetFilter(Builder));
			AssertCollectionContains("", bOs1, found);
			AssertCollectionNotContains("", bOs2, found);
			AssertCollectionNotContains("", bOs3, found);

			Builder.DischargePort = HomePort.SubstringSafe(0, 2);
			found = Load(GetFilter(Builder));
			AssertCollectionContains("", bOs1, found);
			AssertCollectionContains("", bOs2, found);
			AssertCollectionNotContains("", bOs3, found);

			Builder.DischargePort = "";
			found = Load(GetFilter(Builder));
			AssertCollectionContains("", bOs1, found);
			AssertCollectionContains("", bOs2, found);
			AssertCollectionContains("", bOs3, found);
		}

		public void TestDischargePort_Zone()
		{
			JobVoyage voyage = CreateVoyage(TestVessel1, "2345", Core.Constants.TransportModes.Sea);
			JobSailing sailing1 = GetOrCreateSailing(voyage, "AUSYD", "USLAX");
			JobSailing sailing2 = GetOrCreateSailing(voyage, "USLAX", "AUSYD");

			Factory.Save();

			T[] bOs1 = CreateBusinessObjects(sailing1);
			T[] bOs2 = CreateBusinessObjects(sailing2);

			Factory.Save();

			Builder.Reset();
			T[] found;

			Builder.DischargePort = "US";
			found = Load(GetFilter(Builder));
			AssertCollectionContains("", bOs1, found);
			AssertCollectionNotContains("", bOs2, found);

			Builder.DischargePort = "USAR";
			found = Load(GetFilter(Builder));
			AssertCollectionContains("", bOs1, found);
			AssertCollectionNotContains("", bOs2, found);

			Builder.DischargePort = "";
			found = Load(GetFilter(Builder));
			AssertCollectionContains("", bOs1, found);
			AssertCollectionContains("", bOs2, found);

			Builder.DischargePort = "IN";
			found = Load(GetFilter(Builder));
			AssertCollectionNotContains("", bOs1, found);
			AssertCollectionNotContains("", bOs2, found);
		}

		public virtual void TestDateFilter()
		{
			ZDateTime today = ZDateTime.Today;

			ExportSailing.Origin.JA_E_DEP = today.AddDays(-1);
			ExportSailing.Origin.JA_A_DEP = today;
			ExportSailing.Destination.JB_E_ARV = today.AddDays(1);
			ExportSailing.Destination.JB_A_ARV = today.AddDays(2);

			Factory.Save();

			T[] bOs = CreateBusinessObjects(ExportSailing);

			Factory.Save();

			Builder.Reset();
			T[] found;

			found = Load(GetFilter(Builder));
			foreach (BusinessObject bo in bOs)
			{
				AssertCollectionContains(bo, found);
			}

			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ETD, DateComparisonOperator.HasDateInRange, today.AddDays(-1), today.AddDays(-1));
			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ATD, DateComparisonOperator.HasDateInRange, today, today);
			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ETA, DateComparisonOperator.HasDateInRange, today.AddDays(1), today.AddDays(1));
			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ATA, DateComparisonOperator.HasDateInRange, today.AddDays(2), today.AddDays(2));
		}

		public void TestDateFilter_RespectsTimePortion()
		{
			var today = ZDateTime.Today;

			var etd = today.AddDays(-1).AddHours(9);
			var atd = today.AddHours(9);
			var eta = today.AddDays(1).AddHours(13);
			var ata = today.AddDays(2).AddHours(13);

			ExportSailing.Origin.JA_E_DEP = etd;
			ExportSailing.Origin.JA_A_DEP = atd;
			ExportSailing.Destination.JB_E_ARV = eta;
			ExportSailing.Destination.JB_A_ARV = ata;

			Factory.Save();

			var bizObjs = CreateBusinessObjects(ExportSailing);

			Factory.Save();
			Builder.Reset();
			var found = Load(GetFilter(Builder));
			AssertCollectionContains("Precondition - Empty filter should match all records", bizObjs, found);

			Builder.Reset();
			Builder.SetDateRange(SailingFilterBuilder.Dates.ATD, DateComparisonOperator.HasDateInRange, atd.AddHours(-2), atd.AddHours(2));
			found = Load(GetFilter(Builder));
			AssertCollectionContains("Expected to find all Morning Records", bizObjs, found);

			Builder.Reset();
			Builder.SetDateRange(SailingFilterBuilder.Dates.ATD, DateComparisonOperator.HasDateInRange, atd.AddHours(2), atd.AddHours(5));
			found = Load(GetFilter(Builder));
			AssertCollectionNotContains("Expected to NOT find any Morning Records", bizObjs, found);
		}

		public void TestDateFilter_HasNoDate()
		{
			ZDateTime today = ZDateTime.Today;

			ExportSailing.Origin.JA_E_DEP = ZDateTime.Empty;
			ExportSailing.Origin.JA_A_DEP = ZDateTime.Empty;
			ExportSailing.Destination.JB_E_ARV = ZDateTime.Empty;
			ExportSailing.Destination.JB_A_ARV = ZDateTime.Empty;

			Factory.Save();

			T[] bOs = CreateBusinessObjects(ExportSailing);

			Factory.Save();

			Builder.Reset();
			T[] found;

			found = Load(GetFilter(Builder));
			foreach (BusinessObject bo in bOs)
			{
				AssertCollectionContains(bo, found);
			}

			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ETD, DateComparisonOperator.HasNoDateEntered, today.AddDays(-1), today.AddDays(-1));
			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ATD, DateComparisonOperator.HasNoDateEntered, today, today);
			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ETA, DateComparisonOperator.HasNoDateEntered, today.AddDays(1), today.AddDays(1));
			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ATA, DateComparisonOperator.HasNoDateEntered, today.AddDays(2), today.AddDays(2));

			bOs = System.Array.Empty<T>();
			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ETD, DateComparisonOperator.HasDateEntered, today.AddDays(-1), today.AddDays(-1));
			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ATD, DateComparisonOperator.HasDateEntered, today, today);
			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ETA, DateComparisonOperator.HasDateEntered, today.AddDays(1), today.AddDays(1));
			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ATA, DateComparisonOperator.HasDateEntered, today.AddDays(2), today.AddDays(2));
		}

		public void TestDateFilter_HasDate()
		{
			ZDateTime today = ZDateTime.Today;

			ExportSailing.Origin.JA_E_DEP = today;
			ExportSailing.Origin.JA_A_DEP = today;
			ExportSailing.Destination.JB_E_ARV = today;
			ExportSailing.Destination.JB_A_ARV = today;

			Factory.Save();

			T[] bOs = CreateBusinessObjects(ExportSailing);

			Factory.Save();

			Builder.Reset();
			T[] found;

			found = Load(GetFilter(Builder));
			foreach (BusinessObject bo in bOs)
			{
				AssertCollectionContains(bo, found);
			}

			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ETD, DateComparisonOperator.HasDateEntered, today.AddDays(-1), today.AddDays(-1));
			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ATD, DateComparisonOperator.HasDateEntered, today, today);
			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ETA, DateComparisonOperator.HasDateEntered, today.AddDays(1), today.AddDays(1));
			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ATA, DateComparisonOperator.HasDateEntered, today.AddDays(2), today.AddDays(2));

			bOs = System.Array.Empty<T>();
			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ETD, DateComparisonOperator.HasNoDateEntered, today.AddDays(-1), today.AddDays(-1));
			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ATD, DateComparisonOperator.HasNoDateEntered, today, today);
			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ETA, DateComparisonOperator.HasNoDateEntered, today.AddDays(1), today.AddDays(1));
			AssertDateFilterEdgeConditions(bOs, SailingFilterBuilder.Dates.ATA, DateComparisonOperator.HasNoDateEntered, today.AddDays(2), today.AddDays(2));
		}

		#region Implementation

		protected void AssertDateFilterEdgeConditions(T[] bOs, SailingFilterBuilder.Dates dateType, DateComparisonOperator comparisonOperator, ZDateTime lowerEdge, ZDateTime upperEdge)
		{
			T[] found;

			if (comparisonOperator == DateComparisonOperator.HasDateInRange)
			{
				Builder.SetDateRange(dateType, comparisonOperator, lowerEdge.AddDays(-2), lowerEdge.AddDays(-1));
				found = Load(GetFilter(Builder));
				AssertCollectionNotContains(bOs, found);

				Builder.SetDateRange(dateType, comparisonOperator, lowerEdge.AddDays(-2), lowerEdge);
				found = Load(GetFilter(Builder));
				foreach (BusinessObject bo in bOs)
				{
					AssertCollectionContains(bo, found);
				}

				Builder.SetDateRange(dateType, comparisonOperator, upperEdge.AddDays(1), upperEdge.AddDays(2));
				found = Load(GetFilter(Builder));
				AssertCollectionNotContains(bOs, found);

				Builder.SetDateRange(dateType, comparisonOperator, upperEdge, upperEdge.AddDays(2));
				found = Load(GetFilter(Builder));
				foreach (BusinessObject bo in bOs)
				{
					AssertCollectionContains(bo, found);
				}
			}
			else if (comparisonOperator == DateComparisonOperator.HasDateEntered || comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				Builder.SetDateRange(dateType, comparisonOperator, ZDateTime.Empty, ZDateTime.Empty);
				found = Load(GetFilter(Builder));
				foreach (BusinessObject bo in bOs)
				{
					AssertCollectionContains(bo, found);
				}
			}
		}

		protected void AssertCollectionContains(string message, T[] expected, T[] found)
		{
			foreach (T expectedBO in expected)
			{
				AssertCollectionContains(message, expectedBO, found);
			}
		}

		protected void AssertCollectionNotContains(string message, T[] notExpected, T[] found)
		{
			foreach (T notExpectedBO in notExpected)
			{
				AssertCollectionNotContains(message, notExpectedBO, found);
			}
		}

		protected JobVoyage CreateVoyage(RefVessel vessel, ZString voyageFlight, ZString transportMode, bool isArchived = false)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = voyageFlight;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_AirSeaRoad = transportMode;
			voyage.JV_IsActive = !isArchived;
			return voyage;
		}

		protected JobSailing GetOrCreateSailing(JobVoyage voyage, ZString load, ZString disc)
		{
			JobSailing sailing = voyage.Sailings.GetSailingFromLoadAndDischarge(load, disc);

			if (sailing == null)
			{
				VoyageOrigin origin = voyage.Origins.GetOriginFromLoading(load);
				if (origin == null)
				{
					origin = voyage.Origins.AddNew();
					origin.JA_RL_NKPortOfLoading = load;
				}

				VoyageDestination destination = voyage.Destinations.GetDestinationFromDischarge(disc);
				if (destination == null)
				{
					destination = voyage.Destinations.AddNew();
					destination.JB_RL_NKPortOfDischarge = disc;
				}

				voyage.GenerateSailings();
				sailing = voyage.Sailings.GetSailingFromLoadAndDischarge(load, disc);
			}

			return sailing;
		}

		protected SailingFilterBuilder Builder
		{
			get
			{
				if (fBuilder == null)
				{
					fBuilder = new SailingFilterBuilder(Factory);
				}

				return fBuilder;
			}
		}
		SailingFilterBuilder fBuilder;

		protected virtual T[] Load(ZQuery query)
		{
			return Factory.Load<T>(query);
		}

		protected abstract T[] CreateBusinessObjects(JobSailing sailing);
		protected abstract ZQuery GetFilter(SailingFilterBuilder builder);
		protected abstract bool IsLinked(T bo);

		#endregion
	}
}
