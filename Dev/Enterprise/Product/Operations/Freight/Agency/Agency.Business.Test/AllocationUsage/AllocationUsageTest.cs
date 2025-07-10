using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	internal class AllocationUsageTest : BaseAgencyTest
	{
		public void TestLoadForOriginDoesNotThrowException()
		{
			var sailing = Factory.New<JobSailing>();
			var origin = Factory.New<VoyageOrigin>();
			var destination = Factory.New<VoyageDestination>();
			var voyage = Factory.New<JobVoyage>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			origin.JA_JV = voyage.PK;
			origin.JA_RL_NKPortOfLoading = "AUXXX";
			AssertNoExceptionThrown(() => AllocationUsage.LoadForOrigin(sailing, ZGuid.Empty, null));
		}

		public void TestGetAspectValue()
		{
			CombineAssertions(delegate
			{
				AllocationUsage usage = new AllocationUsage(1, 2, 3, 4, 5, 6, 7);
				AssertEquals("TEU", 3m, usage.GetAspectValue(AllocationAspectTypes.TEU));
				AssertEquals("Power Points", 4m, usage.GetAspectValue(AllocationAspectTypes.PowerPoints));
				AssertEquals("Tonnes", 5m, usage.GetAspectValue(AllocationAspectTypes.Tonnes));
				AssertEquals("Volume", 6m, usage.GetAspectValue(AllocationAspectTypes.Volume));
				AssertEquals("Area", 7m, usage.GetAspectValue(AllocationAspectTypes.Area));
				AssertExceptionThrown("Unknown", typeof(ArgumentOutOfRangeException), delegate
				{
					usage.GetAspectValue("XXX");
				});
			});
		}

		public void TestMax()
		{
			AllocationUsage[] usages0 = Array.Empty<AllocationUsage>();
			AllocationUsage[] usages1 = new AllocationUsage[] { new AllocationUsage(10, 11, 9, 12, 13, 14) };
			AllocationUsage[] usages2 = new AllocationUsage[] { new AllocationUsage(0, 0, 0, 0, 0, 0), new AllocationUsage(10, 10, 10, 10, 10, 10), new AllocationUsage(5, 3, 2, 15, 10, 4), new AllocationUsage(9, 11, 6, 10, 13, 12) };
			AssertAllocationUsage("Empty", AllocationUsage.Max(usages0), 0, 0, 0, 0, 0, 0, 0);
			AssertAllocationUsage("Single Usage", AllocationUsage.Max(usages1), 12, 13, 14, 21, 10, 11, 9);
			AssertAllocationUsage("Array", AllocationUsage.Max(usages2), 15, 13, 12, 20, 10, 11, 10);
		}

		public void TestSum()
		{
			AllocationUsage[] usages0 = Array.Empty<AllocationUsage>();
			AllocationUsage[] usages1 = new AllocationUsage[] { new AllocationUsage(10, 11, 9, 12, 13, 14) };
			AllocationUsage[] usages2 = new AllocationUsage[] { new AllocationUsage(0, 0, 0, 0, 0, 0), new AllocationUsage(10, 10, 10, 10, 10, 10), new AllocationUsage(5, 3, 2, 15, 10, 3), new AllocationUsage(9, 11, 6, 10, 13, 12) };
			AssertAllocationUsage("Empty", AllocationUsage.Sum(usages0), 0, 0, 0, 0, 0, 0, 0);
			AssertAllocationUsage("Single Usage", AllocationUsage.Sum(usages1), 12, 13, 14, 21, 10, 11, 9);
			AssertAllocationUsage("Array", AllocationUsage.Sum(usages2), 35, 33, 25, 48, 24, 24, 18);
		}

		public void HasAspectExcedingThatFor()
		{
			AllocationUsage usage = new AllocationUsage(1, 1, 1, 1, 1, 1);
			AssertEquals(true, usage.HasAspectExceedingThatFor(new AllocationUsage(2, 0, 0, 0, 0, 0)));
			AssertEquals(true, usage.HasAspectExceedingThatFor(new AllocationUsage(0, 2, 0, 0, 0, 0)));
			AssertEquals(true, usage.HasAspectExceedingThatFor(new AllocationUsage(0, 0, 2, 0, 0, 0)));
			AssertEquals(true, usage.HasAspectExceedingThatFor(new AllocationUsage(0, 0, 0, 2, 0, 0)));
			AssertEquals(true, usage.HasAspectExceedingThatFor(new AllocationUsage(0, 0, 0, 0, 2, 0)));
			AssertEquals(true, usage.HasAspectExceedingThatFor(new AllocationUsage(0, 0, 0, 0, 0, 2)));
			AssertEquals(true, usage.HasAspectExceedingThatFor(new AllocationUsage()));
			AssertEquals(false, usage.HasAspectExceedingThatFor(new AllocationUsage(1, 1, 1, 1, 1, 1)));
			AssertEquals(false, usage.HasAspectExceedingThatFor(new AllocationUsage(2, 1, 1, 1, 1, 1)));
			AssertEquals(false, usage.HasAspectExceedingThatFor(new AllocationUsage(1, 2, 1, 1, 1, 1)));
			AssertEquals(false, usage.HasAspectExceedingThatFor(new AllocationUsage(1, 1, 2, 1, 1, 1)));
			AssertEquals(false, usage.HasAspectExceedingThatFor(new AllocationUsage(1, 1, 1, 2, 1, 1)));
			AssertEquals(false, usage.HasAspectExceedingThatFor(new AllocationUsage(1, 1, 1, 1, 2, 1)));
			AssertEquals(false, usage.HasAspectExceedingThatFor(new AllocationUsage(1, 1, 1, 1, 1, 2)));
			AssertEquals(false, usage.HasAspectExceedingThatFor(new AllocationUsage(2, 2, 2, 2, 2, 2)));
		}

		public void TestIsEmpty()
		{
			AssertEquals("Empty Usage", true, (new AllocationUsage()).IsEmpty);
			AssertEquals("Has 1 GP TEU", false, (new AllocationUsage(1, 0, 0, 0, 0, 0)).IsEmpty);
			AssertEquals("Has 1 Reefer TEU", false, (new AllocationUsage(0, 1, 0, 0, 0, 0)).IsEmpty);
			AssertEquals("Uses 1 power point", false, (new AllocationUsage(0, 0, 1, 0, 0, 0)).IsEmpty);
			AssertEquals("Weighs 1 Tonne", false, (new AllocationUsage(0, 0, 0, 1, 0, 0)).IsEmpty);
			AssertEquals("Occupies 1 cubic meter", false, (new AllocationUsage(0, 0, 0, 0, 1, 0)).IsEmpty);
			AssertEquals("Occupies 1 square meter", false, (new AllocationUsage(0, 0, 0, 0, 0, 1)).IsEmpty);
		}

		public void TestSqlSupportsAllWeightUnits()
		{
			const decimal Tollorance = 0.001m;
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			BillOfLading shipment = Factory.New<BillOfLading>();
			shipment.JS_JX = sailing.PK;
			shipment.JS_PackingMode = Constants.ContainerModes.Bulk;
			Dictionary<string, IList<string>> result = new Dictionary<string, IList<string>>();
			foreach (string unit in Constants.Weight.Codes)
			{
				decimal tonnes = 0.1m;
				decimal value = Constants.Weight.Convert(0.1m, Constants.Weight.Tonnes, unit);
				while (value > 900000m)
				{
					tonnes /= 10;
					value = Constants.Weight.Convert(tonnes, Constants.Weight.Tonnes, unit);
				}

				while (value < 0.01m)
				{
					tonnes *= 10;
					value = Constants.Weight.Convert(tonnes, Constants.Weight.Tonnes, unit);
				}

				shipment.JS_UnitOfWeight = unit;
				shipment.JS_ActualWeight = value;
				string error = null;
				try
				{
					Factory.Save();
				}
				catch (Exception ex)
				{
					error = "Caught exception while saving:\r\n" + ex.ToString();
				}

				if (error == null)
				{
					AllocationUsage usage = AllocationUsage.LoadForSailing(sailing, ZGuid.Empty, null);
					if (usage.Tonnes == 0m)
					{
						error = "Not supported.";
					}
					else if (Math.Abs(usage.Tonnes - tonnes) > Tollorance)
					{
						error = string.Format("evaluated to {0} but {1} was expected (Tollorance: {2})", usage.Tonnes, tonnes, Tollorance);
					}
				}

				if (error != null)
				{
					string description = Constants.Weight.GetDescription(unit, Constants.PluralState.Plural);
					if (string.IsNullOrEmpty(description))
					{
						description = "<unknown description>";
					}

					result.Add(string.Format("{0} - {1}", unit, description), new string[] { error });
				}
			}

			AssertGroupedErrorList("These units of weight have errors (dont forget, reports should also be updated)", result);
		}

		public void TestSqlSupportsAllVolumeUnits()
		{
			const decimal Tollorance = 0.001m;
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			BillOfLading shipment = Factory.New<BillOfLading>();
			shipment.JS_JX = sailing.PK;
			shipment.JS_PackingMode = Constants.ContainerModes.Bulk;
			Dictionary<string, IList<string>> result = new Dictionary<string, IList<string>>();
			foreach (string unit in Constants.Volume.Codes)
			{
				decimal cubicMetres = 0.1m;
				decimal value = Constants.Volume.Convert(cubicMetres, Constants.Volume.CubicMetres, unit);
				while (value > 900000m)
				{
					cubicMetres /= 10;
					value = Constants.Volume.Convert(cubicMetres, Constants.Volume.CubicMetres, unit);
				}

				while (value < 0.01m)
				{
					cubicMetres *= 10;
					value = Constants.Volume.Convert(cubicMetres, Constants.Volume.CubicMetres, unit);
				}

				shipment.JS_UnitOfVolume = unit;
				shipment.JS_ActualVolume = value;
				shipment.JS_ActualWeight = 0; // because forwarding's chargeable calculation is screwing us over.
				string error = null;
				try
				{
					Factory.Save();
				}
				catch (Exception ex)
				{
					error = "Caught exception while saving:\r\n" + ex.ToString();
				}

				if (error == null)
				{
					AllocationUsage usage = AllocationUsage.LoadForSailing(sailing, ZGuid.Empty, null);
					if (usage.Volume == 0m)
					{
						error = "Not supported.";
					}
					else if (Math.Abs(usage.Volume - cubicMetres) > Tollorance)
					{
						error = string.Format("evaluated to {0} but {1} was expected (Tollorance: {2})", usage.Volume, cubicMetres, Tollorance);
					}
				}

				if (error != null)
				{
					string description = Constants.Volume.GetDescription(unit, Constants.PluralState.Plural);
					if (string.IsNullOrEmpty(description))
					{
						description = "<unknown description>";
					}

					result.Add(string.Format("{0} - {1}", unit, description), new string[] { error });
				}
			}

			AssertGroupedErrorList("These units of weight have errors (dont forget, reports should also be updated)", result);
		}

		public void TestSqlSupportsAllLengthUnits()
		{
			const decimal Tollorance = 0.001m;
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			BillOfLading shipment = Factory.New<BillOfLading>();
			shipment.JS_JX = sailing.PK;
			shipment.JS_PackingMode = Constants.ContainerModes.BreakBulk;
			Dictionary<string, IList<string>> result = new Dictionary<string, IList<string>>();
			foreach (string unit in Constants.Length.Codes)
			{
				decimal value = 1m;
				decimal meters = Constants.Length.Convert(value, unit, Constants.Length.Metres);
				decimal squareMeters = meters * meters;
				while (squareMeters < 1m)
				{
					value *= 10;
					meters = Constants.Length.Convert(value, unit, Constants.Length.Metres);
					squareMeters = meters * meters;
				}

				while (squareMeters > 900000m)
				{
					value /= 10;
					meters = Constants.Length.Convert(value, unit, Constants.Length.Metres);
					squareMeters = meters * meters;
				}

				shipment.ShippingContainers.RemoveAndDeleteAll();
				var topLevelPack = shipment.ShippingContainers.AddNew();
				topLevelPack.JC_ContainerCount = 1;
				topLevelPack.JC_TotalUnitOfMeasure = unit;
				topLevelPack.JC_TotalLength = value;
				topLevelPack.JC_TotalWidth = value;
				topLevelPack.JC_TotalHeight = value;
				topLevelPack.JC_GrossVolume = 0;
				string error = null;
				try
				{
					Factory.Save();
				}
				catch (Exception ex)
				{
					error = "Caught exception while saving:\r\n" + ex.ToString();
				}

				if (error == null)
				{
					AllocationUsage usage = AllocationUsage.LoadForSailing(sailing, ZGuid.Empty, null);
					if (usage.Area == 0m)
					{
						error = "Not supported.";
					}
					else if (Math.Abs(usage.Area - squareMeters) > Tollorance)
					{
						error = string.Format("evaluated to {0} but {1} was expected (Tollorance: {2})", usage.Area, squareMeters, Tollorance);
					}
				}

				if (error != null)
				{
					string description = Constants.Length.GetDescription(unit, Constants.PluralState.Plural);
					if (string.IsNullOrEmpty(description))
					{
						description = "<unknown description>";
					}

					result.Add(string.Format("{0} - {1}", unit, description), new string[] { error });
				}
			}

			AssertGroupedErrorList("These units of weight have errors.", result);
		}

		public void TestLoadForSailing_Sailing()
		{
			GenericLoadForSailingTest_Sailing(false, false);
		}

		public void TestLoadForSailing_Sailing_ExcludeShipment()
		{
			GenericLoadForSailingTest_Sailing(true, false);
		}

		public void TestLoadForSailing_Sailing_ByPrincipal()
		{
			GenericLoadForSailingTest_Sailing(false, true);
		}

		public void TestLoadForSailing_Sailing_ByPrincipal_ExcludeShipment()
		{
			GenericLoadForSailingTest_Sailing(true, true);
		}

		public void GenericLoadForSailingTest_Sailing(bool excludeShipment, bool byPrincipal)
		{
			SetShipments();
			// Principal1
			// Sailing1: 50 Tonnes, 50 cubic meters
			// Sailing2: 15.25 Tonnes, 4 cubic meters, 4 GP TEUs, 2 Reefer TEUs, 1 power point, 8 square meters
			// Sailing3: 0 Tonnes, 0 cubic meters
			// Sailing4: 0 Tonnes, 0 cubic meters
			// Total
			// Sailing1: 51.95 Tonnes, 62 cubic meters, 24 square meters
			// Sailing2: 59.25 Tonnes, 48 cubic meters, 4 GP TEUs, 2 Reefer TEUs, 1 power point, 8 square meters
			// Sailing3: 22.2 Tonnes, 0 cubic meters, 3 GP TEUs, 6 Reefer TEUs, 3 power points
			// Sailing4: 0 Tonnes, 0 cubic meters
			AgencyShipment shipmentToExclude = null;
			ZGuid principalPK = ZGuid.Empty;
			if (excludeShipment)
			{
				shipmentToExclude = NewBulkShipment(Sailing1, Principal1, false, true, 25, 25);
			}

			Factory.Save();
			if (byPrincipal)
			{
				AssertAllocationUsage("Sailing1 ", AllocationUsage.LoadForSailing(Sailing1, Principal1.PK, shipmentToExclude), 50m, 50m, 0m, 0m, 0m, 0m, 0, Sailing1);
				AssertAllocationUsage("Sailing2 ", AllocationUsage.LoadForSailing(Sailing2, Principal1.PK, shipmentToExclude), 15.25m, 4m, 8m, 6m, 4m, 2m, 1, Sailing2);
				AssertAllocationUsage("Sailing3 ", AllocationUsage.LoadForSailing(Sailing3, Principal1.PK, shipmentToExclude), 0m, 0m, 0m, 0m, 0m, 0m, 0, Sailing3);
				AssertAllocationUsage("Sailing4 ", AllocationUsage.LoadForSailing(Sailing4, Principal1.PK, shipmentToExclude), 0m, 0m, 0m, 0m, 0m, 0m, 0, Sailing4);
			}
			else
			{
				AssertAllocationUsage("Sailing1 ", AllocationUsage.LoadForSailing(Sailing1, ZGuid.Empty, shipmentToExclude), 51.95m, 62m, 24m, 0m, 0m, 0m, 0, Sailing1);
				AssertAllocationUsage("Sailing2 ", AllocationUsage.LoadForSailing(Sailing2, ZGuid.Empty, shipmentToExclude), 59.25m, 48m, 8m, 6m, 4m, 2m, 1, Sailing2);
				AssertAllocationUsage("Sailing3 ", AllocationUsage.LoadForSailing(Sailing3, ZGuid.Empty, shipmentToExclude), 22.2m, 0m, 0m, 9m, 3m, 6m, 3, Sailing3);
				AssertAllocationUsage("Sailing4 ", AllocationUsage.LoadForSailing(Sailing4, ZGuid.Empty, shipmentToExclude), 0m, 0m, 0m, 0m, 0m, 0m, 0, Sailing4);
			}
		}

		public void TestLoadForSailing_Voyage()
		{
			GenericLoadForSailingTest_Voyage(false);
		}

		public void TestLoadForSailing_Voyage_ByPrincipal()
		{
			GenericLoadForSailingTest_Voyage(true);
		}

		public void GenericLoadForSailingTest_Voyage(bool byPrincipal)
		{
			Dictionary<ZGuid, AllocationUsage> map = new Dictionary<ZGuid, AllocationUsage>();
			SetShipments();
			// Principal1
			// Sailing1: 50 Tonnes, 50 cubic meters
			// Sailing2: 15.25 Tonnes, 4 cubic meters, 4 GP TEUs, 2 Reefer TEUs, 1 power point, 8 square meters
			// Sailing3: 0 Tonnes, 0 cubic meters
			// Sailing4: 0 Tonnes, 0 cubic meters
			// Total
			// Sailing1: 51.95 Tonnes, 62 cubic meters, 24 square meters
			// Sailing2: 59.25 Tonnes, 48 cubic meters, 4 GP TEUs, 2 Reefer TEUs, 1 power point, 8 square meters
			// Sailing3: 22.2 Tonnes, 0 cubic meters, 3 GP TEUs, 6 Reefer TEUs, 3 power points
			// Sailing4: 0 Tonnes, 0 cubic meters
			ZGuid principalPK = ZGuid.Empty;
			if (byPrincipal)
			{
				principalPK = Principal1.PK;
			}

			AllocationUsage[] usages = AllocationUsage.LoadForSailing(Voyage, principalPK);
			foreach (AllocationUsage usage in usages)
			{
				map.Add(usage.ParentPK, usage);
			}

			if (byPrincipal)
			{
				AssertAllocationUsage("Map[Sailing1.PK].", map[Sailing1.PK], 50m, 50m, 0m, 0m, 0m, 0m, 0, Sailing1);
				AssertAllocationUsage("Map[Sailing2.PK].", map[Sailing2.PK], 15.25m, 4m, 8m, 6m, 4m, 2m, 1, Sailing2);
				AssertEquals("Only return usages for sailings that have shipments attacted (Sailing3)", false, map.ContainsKey(Sailing3.PK));
				AssertEquals("Only return usages for sailings that have shipments attacted (Sailing4)", false, map.ContainsKey(Sailing4.PK));
			}
			else
			{
				AssertAllocationUsage("Map[Sailing1.PK].", map[Sailing1.PK], 51.95m, 62m, 24m, 0m, 0m, 0m, 0, Sailing1);
				AssertAllocationUsage("Map[Sailing2.PK].", map[Sailing2.PK], 59.25m, 48m, 8m, 6m, 4m, 2m, 1, Sailing2);
				AssertAllocationUsage("Map[Sailing3.PK].", map[Sailing3.PK], 22.2m, 0, 0m, 9m, 3m, 6m, 3, Sailing3);
				AssertEquals("Only return usages for sailings that have shipments attacted", false, map.ContainsKey(Sailing4.PK));
			}
		}

		public void TestLoadForOrigin_Origin()
		{
			GenericLoadForOriginTest_Origin(false, false);
		}

		public void TestLoadForOrigin_Origin_ExcludeShipment()
		{
			GenericLoadForOriginTest_Origin(true, false);
		}

		public void TestLoadForOrigin_Origin_ByPrincipal()
		{
			GenericLoadForOriginTest_Origin(false, true);
		}

		public void TestLoadForOrigin_Origin_ByPrincipal_ExcludeShipment()
		{
			GenericLoadForOriginTest_Origin(true, true);
		}

		public void GenericLoadForOriginTest_Origin(bool excludeShipment, bool byPrincipal)
		{
			SetShipments();
			// Principal1
			// Sailing1: 50 Tonnes, 50 cubic meters
			// Sailing2: 15.25 Tonnes, 4 cubic meters, 4 GP TEUs, 2 Reefer TEUs, 1 power point, 8 square meters
			// Sailing3: 0 Tonnes, 0 cubic meters
			// Sailing4: 0 Tonnes, 0 cubic meters
			//
			// Origin1: 65.25 Tonnes, 54 cubic meters, 4 GP TEUs, 2 Reefer TEUs, 1 power point, 8 square meters (Sailing1 + Sailing2)
			// Origin2: 0 Tonnes, 0 cubic meters (Sailing3)
			// Origin3: 0 Tonnes, 0 cubic meters (Sailing4)
			// Total
			// Sailing1: 51.95 Tonnes, 62 cubic meters, 24 square meters
			// Sailing2: 59.25 Tonnes, 48 cubic meters, 4 GP TEUs, 2 Reefer TEUs, 1 power point, 8 square meters
			// Sailing3: 22.2 Tonnes, 0 cubic meters, 3 GP TEUs, 6 Reefer TEUs, 3 power points
			// Sailing4: 0 Tonnes, 0 cubic meters
			//
			// Origin1: 111.2 Tonnes, 110 cubic meters, 4 GP TEUs, 2 Reefer TEUs, 1 power point, 32 square meters (Sailing1 + Sailing2)
			// Origin2: 22.2 Tonnes, 0 cubic meters, 3 GP TEUs, 6 Reefer TEUs, 3 power points (Sailing3)
			// Origin3: 0 Tonnes, 0 cubic meters (Sailing4)
			AgencyShipment shipmentToExclude = null;
			if (excludeShipment)
			{
				shipmentToExclude = NewBulkShipment(Sailing1, Principal1, false, true, 25, 25);
			}

			Factory.Save();
			if (byPrincipal)
			{
				AssertAllocationUsage("Origin1 ", AllocationUsage.LoadForOrigin(Origin1, Principal1.PK, shipmentToExclude), 65.25m, 54m, 8m, 6m, 4m, 2m, 1, Origin1);
				AssertAllocationUsage("Origin2 ", AllocationUsage.LoadForOrigin(Origin2, Principal1.PK, shipmentToExclude), 0m, 0m, 0m, 0m, 0m, 0m, 0, Origin2);
				AssertAllocationUsage("Origin3 ", AllocationUsage.LoadForOrigin(Origin3, Principal1.PK, shipmentToExclude), 0m, 0m, 0m, 0m, 0m, 0m, 0, Origin3);
			}
			else
			{
				AssertAllocationUsage("Origin1 ", AllocationUsage.LoadForOrigin(Origin1, ZGuid.Empty, shipmentToExclude), 111.2m, 110m, 32m, 6m, 4m, 2m, 1, Origin1);
				AssertAllocationUsage("Origin2 ", AllocationUsage.LoadForOrigin(Origin2, ZGuid.Empty, shipmentToExclude), 22.2m, 0m, 0m, 9m, 3m, 6m, 3, Origin2);
				AssertAllocationUsage("Origin3 ", AllocationUsage.LoadForOrigin(Origin3, ZGuid.Empty, shipmentToExclude), 0m, 0m, 0m, 0m, 0m, 0m, 0, Origin3);
			}
		}

		public void TestLoadForOrigin_Sailing()
		{
			GenericLoadForOriginTest_Sailing(false, false);
		}

		public void TestLoadForOrigin_Sailing_ExcludeShipment()
		{
			GenericLoadForOriginTest_Sailing(true, false);
		}

		public void TestLoadForOrigin_Sailing_ByPrincipal()
		{
			GenericLoadForOriginTest_Sailing(false, true);
		}

		public void TestLoadForOrigin_Sailing_ByPrincipal_ExcludeShipment()
		{
			GenericLoadForOriginTest_Sailing(true, true);
		}

		public void GenericLoadForOriginTest_Sailing(bool excludeShipment, bool byPrincipal)
		{
			SetShipments();
			// Principal1
			// Sailing1: 50 Tonnes, 50 cubic meters
			// Sailing2: 15.25 Tonnes, 4 cubic meters, 4 GP TEUs, 2 Reefer TEUs, 1 power point, 8 square meters
			// Sailing3: 0 Tonnes, 0 cubic meters
			// Sailing4: 0 Tonnes, 0 cubic meters
			//
			// Origin1: 65.25 Tonnes, 54 cubic meters, 4 GP TEUs, 2 Reefer TEUs, 1 power point, 8 square meters (Sailing1 + Sailing2)
			// Origin2: 50 Tonnes, 50 cubic meters (Sailing1 + Sailing3)
			// Origin3: 0 Tonnes, 0 cubic meters (Sailing4)
			// Total
			// Sailing1: 51.95 Tonnes, 62 cubic meters, 24 square meters
			// Sailing2: 59.25 Tonnes, 48 cubic meters, 4 GP TEUs, 2 Reefer TEUs, 1 power point, 8 square meters
			// Sailing3: 22.2 Tonnes, 0 cubic meters, 3 GP TEUs, 6 Reefer TEUs, 3 power points
			// Sailing4: 0 Tonnes, 0 cubic meters
			//
			// Origin1: 111.2 Tonnes, 110 cubic meters, 4 GP TEUs, 2 Reefer TEUs, 1power point, 32 square meters (Sailing1 + Sailing2)
			// Origin2: 74.15 Tonnes, 62 cubic meters, 3 GP TEUs, 6 Reefer TEUs, 24 square meters (Sailing1 + Sailing3)
			// Origin3: 0 Tonnes, 0 cubic meters (Sailing4)
			AgencyShipment shipmentToExclude = null;
			ZGuid principalPK = ZGuid.Empty;
			if (byPrincipal)
			{
				principalPK = Principal1.PK;
			}

			if (excludeShipment)
			{
				JobSailing sailing5 = Voyage.Sailings.GetSailingFromLoadAndDischarge(Origin1.JA_RL_NKPortOfLoading, Sailing4.Destination.JB_RL_NKPortOfDischarge);
				shipmentToExclude = NewBulkShipment(sailing5, Principal1, false, true, 25, 25);
			}

			Factory.Save();
			AllocationUsage usage1 = AllocationUsage.LoadForOrigin(Sailing4, principalPK, shipmentToExclude); // no shipments
			AssertAllocationUsage("usage1", usage1, 0m, 0m, 0m, 0m, 0m, 0m, 0);
			AllocationUsage usage2 = AllocationUsage.LoadForOrigin(Sailing1, principalPK, shipmentToExclude);
			if (byPrincipal)
			{
				AssertAllocationUsage("usage2", usage2, 65.25m, 54m, 8m, 6m, 4m, 2m, 1);
			}
			else
			{
				AssertAllocationUsage("usage2", usage2, 111.2m, 110m, 32m, 9m, 4m, 6m, 3);
			}
		}

		public void TestLoadReliventToSailing_None()
		{
			GenericTestLoadReliventToSailingTest_None(false, false);
		}

		public void TestLoadReliventToSailing_None_ExcludingShipment()
		{
			GenericTestLoadReliventToSailingTest_None(true, false);
		}

		public void TestLoadReliventToSailing_None_ByPrincipal()
		{
			GenericTestLoadReliventToSailingTest_None(false, true);
		}

		public void TestLoadReliventToSailing_None_ByPrincipal_ExcludingShipment()
		{
			GenericTestLoadReliventToSailingTest_None(true, true);
		}

		public void GenericTestLoadReliventToSailingTest_None(bool excludeShipment, bool byPrincipal)
		{
			SetShipments();
			AgencyShipment shipmentToExclude = null;
			ZGuid principalPK = ZGuid.Empty;
			if (excludeShipment)
			{
				shipmentToExclude = NewBulkShipment(Sailing1, Principal1, false, true, 10, 10);
			}

			if (byPrincipal)
			{
				principalPK = Principal1.PK;
			}

			Factory.Save();
			Sailing1.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.NotSet;
			AllocationUsageSet set;
			set = AllocationUsage.LoadRelevantToSailing(Sailing1, principalPK, shipmentToExclude);
			AssertNull("Allocation method set to none, return null", set);
		}

		public void TestLoadReliventToSailing_Sailing()
		{
			GenericLoadReliventToSailingTest_Sailing(false, false);
		}

		public void TestLoadReliventToSailing_Sailing_ExcludingShipment()
		{
			GenericLoadReliventToSailingTest_Sailing(true, false);
		}

		public void TestLoadReliventToSailing_Sailing_ByPrincipal()
		{
			GenericLoadReliventToSailingTest_Sailing(false, true);
		}

		public void TestLoadReliventToSailing_Sailing_ByPrincipal_ExcludingShipment()
		{
			GenericLoadReliventToSailingTest_Sailing(true, true);
		}

		public void GenericLoadReliventToSailingTest_Sailing(bool excludeShipment, bool byPrincipal)
		{
			SetShipments();
			AgencyShipment shipmentToExclude = null;
			ZGuid principalPK = ZGuid.Empty;
			if (excludeShipment)
			{
				shipmentToExclude = NewBulkShipment(Sailing1, Principal1, false, true, 10, 10);
			}

			if (byPrincipal)
			{
				principalPK = Principal1.PK;
			}

			Factory.Save();
			Sailing1.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Sailing;
			AllocationUsageSet set = AllocationUsage.LoadRelevantToSailing(Sailing1, principalPK, shipmentToExclude);
			AssertEquals("The SlotAllocation should be linked to the sailing", Sailing1.PK, set.Allocation.E0_ParentID);
			AssertEquals("PrincipalPK", principalPK, set.Allocation.E0_OH_Principal);
			if (byPrincipal)
			{
				AssertEquals("Set.Used.Tonnes", 50m, set.Used.Tonnes);
			}
			else
			{
				AssertEquals("Set.Used.Tonnes", 51.95m, set.Used.Tonnes);
			}
		}

		public void TestLoadReliventToSailing_Origin()
		{
			GenericLoadReliventToSailingTest_Origin(false, false);
		}

		public void TestLoadReliventToSailing_Origin_ExcludingShipment()
		{
			GenericLoadReliventToSailingTest_Origin(true, false);
		}

		public void TestLoadReliventToSailing_Origin_ByPrincipal()
		{
			GenericLoadReliventToSailingTest_Origin(false, true);
		}

		public void TestLoadReliventToSailing_Origin_ByPrincipal_ExcludingShipment()
		{
			GenericLoadReliventToSailingTest_Origin(true, true);
		}

		public void GenericLoadReliventToSailingTest_Origin(bool excludeShipment, bool byPrincipal)
		{
			SetShipments();
			AgencyShipment shipmentToExclude = null;
			ZGuid principalPK = ZGuid.Empty;
			if (excludeShipment)
			{
				shipmentToExclude = NewBulkShipment(Sailing1, Principal1, false, true, 10, 10);
			}

			if (byPrincipal)
			{
				principalPK = Principal1.PK;
			}

			Factory.Save();
			Sailing1.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Origin;
			AllocationUsageSet set = AllocationUsage.LoadRelevantToSailing(Sailing1, principalPK, shipmentToExclude);
			AssertEquals("The SlotAllocation should be linked to the Origin", Sailing1.Origin.PK, set.Allocation.E0_ParentID);
			AssertEquals("PrincipalPK", principalPK, set.Allocation.E0_OH_Principal);
			if (byPrincipal)
			{
				AssertEquals("Set.Used.Tonnes", 65.25m, set.Used.Tonnes);
			}
			else
			{
				AssertEquals("Set.Used.Tonnes", 111.2m, set.Used.Tonnes);
			}
		}

		public void TestLoadReliventToSailing_Country()
		{
			GenericLoadReliventToSailingTest_Country(false, false);
		}

		public void TestLoadReliventToSailing_Country_ExcludingShipment()
		{
			GenericLoadReliventToSailingTest_Country(true, false);
		}

		public void TestLoadReliventToSailing_Country_ByPrincipal()
		{
			GenericLoadReliventToSailingTest_Country(false, true);
		}

		public void TestLoadReliventToSailingTest_Country_ByPrincipal_ExcludingShipment()
		{
			GenericLoadReliventToSailingTest_Country(true, true);
		}

		public void GenericLoadReliventToSailingTest_Country(bool excludeShipment, bool byPrincipal)
		{
			SetShipments();
			AgencyShipment shipmentToExclude = null;
			ZGuid principalPK = ZGuid.Empty;
			if (excludeShipment)
			{
				JobSailing sailing5 = Voyage.Sailings.GetSailingFromLoadAndDischarge(Origin1.JA_RL_NKPortOfLoading, Sailing4.Destination.JB_RL_NKPortOfDischarge);
				shipmentToExclude = NewBulkShipment(sailing5, Principal1, false, true, 25, 25);
			}

			if (byPrincipal)
			{
				principalPK = Principal1.PK;
			}

			Factory.Save();
			Sailing1.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Country;
			AllocationUsageSet set = AllocationUsage.LoadRelevantToSailing(Sailing1, principalPK, shipmentToExclude);
			AssertEquals("The SlotAllocation should be linked to the country", Sailing1.Origin.VoyageCountry.PK, set.Allocation.E0_ParentID);
			AssertEquals("PrincipalPK", principalPK, set.Allocation.E0_OH_Principal);
			if (byPrincipal)
			{
				AssertEquals("Set.Used.Tonnes", 65.25m, set.Used.Tonnes);
			}
			else
			{
				AssertEquals("Set.Used.Tonnes", 111.2m, set.Used.Tonnes);
			}
		}

		public void TestLoadFromShipment_BLK()
		{
			SetSailings();
			AgencyShipment shipment1 = Factory.New<AgencyShipment>();
			shipment1.JS_PackingMode = Constants.ContainerModes.Bulk;
			shipment1.JS_UnitOfWeight = Constants.Weight.Tonnes;
			shipment1.JS_ActualWeight = 50;
			shipment1.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment1.JS_ActualVolume = 30;
			shipment1.ShippingContainers.RemoveAndDeleteAll();
			var topLevelPack1a = shipment1.ShippingContainers.AddNew();
			topLevelPack1a.JC_ContainerCount = 1;
			topLevelPack1a.JC_TotalUnitOfMeasure = Constants.Length.Metres;
			topLevelPack1a.JC_TotalLength = 7;
			topLevelPack1a.JC_TotalWidth = 3;
			topLevelPack1a.JC_TotalHeight = 1;
			var topLevelPack1b = shipment1.ShippingContainers.AddNew();
			topLevelPack1b.JC_ContainerCount = 2;
			topLevelPack1b.JC_TotalUnitOfMeasure = Constants.Length.Centimetres;
			topLevelPack1b.JC_TotalLength = 300;
			topLevelPack1b.JC_TotalWidth = 300;
			topLevelPack1b.JC_TotalHeight = 50;
			AgencyShipment shipment2 = Factory.New<AgencyShipment>();
			shipment2.JS_PackingMode = Constants.ContainerModes.Bulk;
			shipment2.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment2.JS_UnitOfVolume = Constants.Volume.Litre;
			shipment2.JS_JX = Sailing1.PK;
			shipment2.JS_ActualWeight = 40000;
			shipment2.JS_ActualVolume = 40000;
			shipment2.ShippingContainers.RemoveAndDeleteAll();
			var topLevelPack2a = shipment2.ShippingContainers.AddNew();
			topLevelPack2a.JC_ContainerCount = 2;
			topLevelPack2a.JC_TotalLength = 5;
			topLevelPack2a.JC_TotalWidth = 2;
			topLevelPack2a.JC_TotalHeight = 2;
			AllocationUsage usage1 = AllocationUsage.LoadFromShipment(shipment1);
			AssertAllocationUsage("Usage1", usage1, 50m, 30m, 0m, 0m, 0m, 0m, 0);
			AssertEquals("Usage1.ParentPK", ZGuid.Empty, usage1.ParentPK);
			AllocationUsage usage2 = AllocationUsage.LoadFromShipment(shipment2);
			AssertAllocationUsage("Usage2", usage2, 40m, 40m, 0m, 0m, 0m, 0m, 0, shipment2.Sailing);
		}

		public void TestLoadFromShipment_FCL_Booking()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AgencyShipmentContainer container1 = shipment.BookedContainers.AddNew();
			container1.JC_RC = RC_20GP_PK;
			container1.JC_ContainerCount = 5;
			container1.JC_GrossWeightUQ = Constants.Weight.Kilograms;
			container1.JC_GrossWeight = 32000;
			AgencyShipmentContainer container2 = shipment.BookedContainers.AddNew();
			container2.JC_RC = RC_40RE_PK;
			container2.JC_GrossWeightUQ = Constants.Weight.Tonnes;
			container2.JC_GrossWeight = 20;
			AgencyShipmentContainer ignoredContainer = shipment.RealContainers.AddNew();
			ignoredContainer.JC_RC = RC_40GP_PK;
			ignoredContainer.JC_GrossWeightUQ = Constants.Weight.Tonnes;
			ignoredContainer.JC_GrossWeight = 30000;
			AllocationUsage usage = AllocationUsage.LoadFromShipment(shipment);
			AssertAllocationUsage("Usage", usage, 52m, 0m, 0m, 7m, 5m, 2m, 1);
			AssertEquals("Usage.ParentPK", ZGuid.Empty, usage.ParentPK);
			SetSailings();
			shipment.JS_JX = Sailing1.PK;
			usage = AllocationUsage.LoadFromShipment(shipment);
			AssertAllocationUsage("Usage", usage, 52m, 0m, 0m, 7m, 5m, 2m, 1, shipment.Sailing);
		}

		public void TestLoadFromShipment_FCL_Real()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AgencyShipmentContainer container1 = shipment.RealContainers.AddNew();
			container1.JC_RC = RC_20GP_PK;
			container1.JC_ContainerCount = 5;
			container1.JC_GrossWeightUQ = Constants.Weight.Kilograms;
			container1.JC_GrossWeight = 32000;
			AgencyShipmentContainer container2 = shipment.RealContainers.AddNew();
			container2.JC_RC = RC_40RE_PK;
			container2.JC_GrossWeightUQ = Constants.Weight.Tonnes;
			container2.JC_GrossWeight = 20;
			AgencyShipmentContainer ignoredContainer = shipment.BookedContainers.AddNew();
			ignoredContainer.JC_RC = RC_20RE_PK;
			ignoredContainer.JC_GrossWeightUQ = Constants.Weight.Tonnes;
			ignoredContainer.JC_GrossWeight = 20000;
			AllocationUsage usage = AllocationUsage.LoadFromShipment(shipment);
			AssertAllocationUsage("Usage", usage, 52m, 0m, 0m, 7m, 5m, 2m, 1);
			AssertEquals("Usage.ParentPK", ZGuid.Empty, usage.ParentPK);
			SetSailings();
			shipment.JS_JX = Sailing1.PK;
			usage = AllocationUsage.LoadFromShipment(shipment);
			AssertAllocationUsage("Usage", usage, 52m, 0m, 0m, 7m, 5m, 2m, 1, shipment.Sailing);
		}

		public void TestLoadFromShipment_BBK()
		{
			AssertLoadFromShipment(Constants.ContainerModes.BreakBulk);
		}

		public void TestLoadFromShipment_ROR()
		{
			AssertLoadFromShipment(Constants.ContainerModes.RollOnRollOff);
		}

		void AssertLoadFromShipment(ZString mode)
		{
			SetSailings();
			AgencyShipment shipment1 = Factory.New<AgencyShipment>();
			shipment1.JS_PackingMode = mode;
			shipment1.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment1.JS_UnitOfVolume = Constants.Volume.Litre;
			shipment1.JS_ActualWeight = 834;
			shipment1.JS_ActualVolume = 4000;
			var vehicle1 = shipment1.ShippingContainers.AddNew();
			vehicle1.JC_ContainerCount = 2;
			vehicle1.JC_TotalUnitOfMeasure = Constants.Length.Metres;
			vehicle1.JC_TotalLength = 2;
			vehicle1.JC_TotalWidth = 1;
			vehicle1.JC_TotalHeight = 1;
			AgencyShipment shipment2 = Factory.New<AgencyShipment>();
			shipment2.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			shipment2.JS_JX = Sailing1.PK;
			shipment2.JS_UnitOfWeight = Constants.Weight.Tonnes;
			shipment2.JS_ActualWeight = 1;
			shipment2.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment2.JS_ActualVolume = 8;
			var vehicle2 = shipment2.ShippingContainers.AddNew();
			vehicle2.JC_ContainerCount = 1;
			vehicle2.JC_TotalUnitOfMeasure = Constants.Length.Centimetres;
			vehicle2.JC_TotalLength = 250;
			vehicle2.JC_TotalWidth = 200;
			vehicle2.JC_TotalHeight = 160;
			AllocationUsage usage1 = AllocationUsage.LoadFromShipment(shipment1);
			AssertAllocationUsage("Usage1", usage1, 0.834m, 4m, 4m, 0m, 0m, 0m, 0);
			AssertEquals("Usage1.ParentPK", ZGuid.Empty, usage1.ParentPK);
			AllocationUsage usage2 = AllocationUsage.LoadFromShipment(shipment2);
			AssertAllocationUsage("Usage2", usage2, 1m, 8m, 5m, 0m, 0m, 0m, 0, shipment2.Sailing);
		}

		public void TestLoadFromShipmentWithBadUnitsDontThrowException()
		{
			AgencyShipment bulkShipment = NewBulkShipment(ImportSailing, null, false, true, 30, 30);
			bulkShipment.JS_UnitOfWeight = "xz";
			bulkShipment.JS_UnitOfVolume = "zx";
			AgencyShipment fclShipment = NewFCLShipment(ImportSailing, null, false, true, 1, 1);
			foreach (AgencyShipmentContainer container in fclShipment.RealContainers)
			{
				container.JC_GrossWeightUQ = "zz";
			}

			AllocationUsage bulkUsage = AllocationUsage.LoadFromShipment(bulkShipment);
			AssertEquals("Return a weight of 0 if the unit of weight is invalid", 0m, bulkUsage.Tonnes);
			AssertEquals("Return a volume of 0 if the unit of volume is invalid", 0m, bulkUsage.Volume);
			AllocationUsage fclUsage = AllocationUsage.LoadFromShipment(fclShipment);
			AssertEquals("Return a weight of 0 if the unit of weight is invalid", 0m, fclUsage.Tonnes);
			AssertEquals("Return a volume of 0 if the unit of volume is invalid", 0m, fclUsage.Volume);
		}

		public void TestLoadFromShipmentCannotFindTotalUnitOfMeasure()
		{
			var shipment = NewBulkShipment(ImportSailing, null, false, true, 30, 30);
			shipment.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			var vehicle1 = shipment.ShippingContainers.AddNew();
			vehicle1.JC_TotalLength = 2;
			vehicle1.JC_TotalWidth = 1;
			vehicle1.JC_TotalHeight = 1;
			vehicle1.JC_TotalUnitOfMeasure = Constants.Length.Feet;
			var allocationUsage = AllocationUsage.LoadFromShipment(shipment);
			AssertEquals("Return area if the unit of measure is valid", 0.185806m, allocationUsage.Area);
			var vehicle2 = shipment.ShippingContainers.AddNew();
			vehicle2.JC_TotalLength = 2;
			vehicle2.JC_TotalWidth = 1;
			vehicle2.JC_TotalHeight = 1;
			vehicle2.JC_TotalUnitOfMeasure = "Q";
			allocationUsage = AllocationUsage.LoadFromShipment(shipment);
			AssertEquals("Return area 0 if the unit of measure is invalid for all the containers", 0m, allocationUsage.Area);
			foreach (var unit in Constants.Length.Codes)
			{
				vehicle2.JC_TotalUnitOfMeasure = unit;
				AssertNoExceptionThrown(() => AllocationUsage.LoadFromShipment(shipment));
			}
		}

		public void TestShipmentExclusionInSync()
		{
			SetSailings();
			List<AgencyShipment> shipments = new List<AgencyShipment>();
			shipments.Add(NewBulkShipment(Sailing1, Principal1, false, ShipmentStatusList.Codes.Confirmed, 50, 50)); // 50 Tonnes, 50 cubic meters
			shipments.Add(NewBulkShipment(Sailing2, Principal2, false, ShipmentStatusList.Codes.Confirmed, 44, 44)); // 44 Tonnes, 44 cubic meters
			shipments.Add(NewFCLShipment(Sailing2, Principal1, false, ShipmentStatusList.Codes.Booked, 3, 1)); // 12.2 Tonnes (3*(2.28 + 0.12) + 1*(4.35 + 0.65)), 0 cubic meters, 3 GP TEUs, 2 Reefer TEUs, 1 power point
			var duplicated = NewFCLShipment(Sailing2, Principal1, false, ShipmentStatusList.Codes.Booked, 1, 0, "FAKE1"); // 2.34 Tonnes (Tare: 2.28 Net: 0.06), 0 cubic meters, 1 GP TEUs, 0 Reefer TEUs, 0 power point
			duplicated.BookedContainers[0].JC_GrossWeight = 2340m;
			shipments.Add(duplicated);
			duplicated = NewFCLShipment(Sailing2, Principal1, false, ShipmentStatusList.Codes.Booked, 1, 0, "FAKE1"); // 2.34 Tonnes (Tare: 2.28 Net: 0.06), 0 cubic meters, 1 GP TEUs, 0 Reefer TEUs, 0 power point
			duplicated.BookedContainers[0].JC_GrossWeight = 2340m;
			shipments.Add(duplicated);
			shipments.Add(NewFCLShipment(Sailing3, Principal2, false, ShipmentStatusList.Codes.Confirmed, 3, 2)); // 17.2 Tonnes (3*(2.28 + 0.12) + 2*(4.35 + 0.65)), 0 cubic meters, 3 GP TEUs, 4 Reefer TEUs, 2 power points
			duplicated = NewFCLShipment(Sailing3, Principal2, false, ShipmentStatusList.Codes.Confirmed, 0, 1, "FAKE1"); // 4.7 Tonnes (Tare: 4.35 Net: 0.35), 0 cubic meters, 0 GP TEUs, 2 Reefer TEUs, 1 power points
			duplicated.RealContainers[0].JC_GrossWeight = 4.7m;
			shipments.Add(duplicated);
			duplicated = NewFCLShipment(Sailing3, Principal2, false, ShipmentStatusList.Codes.Confirmed, 0, 1, "FAKE1"); // 4.65 Tonnes (Tare: 4.35 Net: 0.30), 0 cubic meters, 0 GP TEUs, 2 Reefer TEUs, 1 power points
			duplicated.RealContainers[0].JC_GrossWeight = 4.65m;
			shipments.Add(duplicated);
			shipments.Add(NewRORShipment(Sailing2, Principal1, false, ShipmentStatusList.Codes.Confirmed, 1)); // 0.65 Tonnes, 4 cubic meters, 8 square meters
			shipments.Add(NewRORShipment(Sailing1, Principal2, false, ShipmentStatusList.Codes.Booked, 3)); // 1.95 Tonnes, 12 cubic meters, 24 square meters
																											// TOTAL
																											// Sailing1: 51.95 (50 + 1.95) Tonnes, 62 (50 + 12) cubic meters, 24 square meters
																											// Sailing2: 59.25 (44 + 14.6 + 0.65) Tonnes, 48 (44 + 4) cubic meters, 4 GP TEU, 2 Reefer TEUs, 1 power point, 8 square meters
																											// Sailing3: 22.2 Tonnes, 0 cubic meters, 3 GP TEUs, 6 Reefer TEUs, 3 power points
																											// Sailing4: 0 Tonnes, 0 cubic meters
			Factory.Save();
			for (int i = 0; i < shipments.Count; i++)
			{
				var shipment = shipments[i];
				var total1 = AllocationUsage.LoadForSailing(shipment.Sailing, ZGuid.Empty, null);
				var total2 = AllocationUsage.Sum(new AllocationUsage[] { AllocationUsage.LoadFromShipment(shipment), AllocationUsage.LoadForSailing(shipment.Sailing, ZGuid.Empty, shipment), });
				AssertAllocationUsage(string.Format("shipments[{0}]", i), total1, total2);
			}
		}

		public void TestAdd()
		{
			var usage1 = new AllocationUsage(1m, 2m, 3, 4m, 5m, 6m);
			var usage2 = new AllocationUsage(7m, 8m, 9, 10m, 11m, 13m);
			usage1.Add(usage2);
			AssertAllocationUsage("Usage1", usage1, 14m, 16m, 19m, 18m, 8m, 10m, 12);
		}

		#region Implementation
		OrgHeader Principal1;
		OrgHeader Principal2;
		public void SetPrincipals()
		{
			Principal1 = NewPrincipal();
			Principal2 = NewPrincipal();
			Factory.Save();
		}

		JobVoyage Voyage;
		VoyageOrigin Origin1;
		VoyageOrigin Origin2;
		VoyageOrigin Origin3;
		JobSailing Sailing1;
		JobSailing Sailing2;
		JobSailing Sailing3;
		JobSailing Sailing4;
		public void SetSailings()
		{
			Voyage = Factory.New<JobVoyage>();
			Origin1 = Voyage.Origins.AddNew();
			Origin1.JA_RL_NKPortOfLoading = "AUSYD";
			Origin1.JA_E_DEP = ZDateTime.Now.AddDays(10);
			Origin2 = Voyage.Origins.AddNew();
			Origin2.JA_RL_NKPortOfLoading = "AUBNE";
			Origin2.JA_E_DEP = ZDateTime.Now.AddDays(16);
			Origin3 = Voyage.Origins.AddNew();
			Origin3.JA_RL_NKPortOfLoading = "SGSIN";
			Origin3.JA_E_DEP = ZDateTime.Now.AddDays(22);
			VoyageDestination destination1 = Voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AUBNE";
			destination1.JB_E_ARV = ZDateTime.Now.AddDays(14);
			VoyageDestination destination2 = Voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "NZAKL";
			destination2.JB_E_ARV = ZDateTime.Now.AddDays(20);
			VoyageDestination destination3 = Voyage.Destinations.AddNew();
			destination3.JB_RL_NKPortOfDischarge = "SGSIN";
			destination3.JB_E_ARV = ZDateTime.Now.AddDays(26);
			VoyageDestination destination4 = Voyage.Destinations.AddNew();
			destination4.JB_RL_NKPortOfDischarge = "NLAMS";
			destination4.JB_E_ARV = ZDateTime.Now.AddDays(32);
			Voyage.GenerateSailings();
			Sailing1 = Voyage.Sailings.GetSailingFromLoadAndDischarge(Origin1.JA_RL_NKPortOfLoading, destination2.JB_RL_NKPortOfDischarge);
			Sailing2 = Voyage.Sailings.GetSailingFromLoadAndDischarge(Origin1.JA_RL_NKPortOfLoading, destination1.JB_RL_NKPortOfDischarge);
			Sailing3 = Voyage.Sailings.GetSailingFromLoadAndDischarge(Origin2.JA_RL_NKPortOfLoading, destination2.JB_RL_NKPortOfDischarge);
			Sailing4 = Voyage.Sailings.GetSailingFromLoadAndDischarge(Origin3.JA_RL_NKPortOfLoading, destination4.JB_RL_NKPortOfDischarge);
		}

		public void SetShipments()
		{
			SetPrincipals();
			SetSailings();
			AgencyShipment bkd;
			AgencyShipment rel;
			NewBulkShipment(Sailing1, Principal1, false, ShipmentStatusList.Codes.Confirmed, 50, 50); // 50 Tonnes, 50 cubic meters
			NewBulkShipment(Sailing1, Principal2, true, ShipmentStatusList.Codes.Booked, 55, 55); // not counted (canceled)
			NewBulkShipment(Sailing1, Principal1, false, ShipmentStatusList.Codes.WaitListed, 60, 60); // not counted (wait listed)
			NewBulkShipment(Sailing2, Principal2, false, ShipmentStatusList.Codes.Confirmed, 44, 44); // 44 Tonnes, 44 cubic meters
			NewBulkShipment(Sailing2, Principal2, false, ShipmentStatusList.Codes.WebBooking, 23, 32); // not counted (web booking)
			bkd = NewFCLShipment(Sailing2, Principal1, false, ShipmentStatusList.Codes.Booked, 3, 1); // 12.2 Tonnes (3*(2.28 + 0.12) + 1*(4.35 + 0.65)), 0 cubic meters, 3 GP TEUs, 2 Reefer TEUs, 1 power point
			var duplicated = NewFCLShipment(Sailing2, Principal1, false, ShipmentStatusList.Codes.Booked, 1, 0, "FAKE1"); // 2.34 Tonnes (Tare: 2.28 Net: 0.06), 0 cubic meters, 1 GP TEUs, 0 Reefer TEUs, 0 power point
			duplicated.BookedContainers[0].JC_GrossWeight = 2340m;
			duplicated = NewFCLShipment(Sailing2, Principal1, false, ShipmentStatusList.Codes.Booked, 1, 0, "FAKE1"); // 2.34 Tonnes (Tare: 2.28 Net: 0.06), 0 cubic meters, 1 GP TEUs, 0 Reefer TEUs, 0 power point
			duplicated.BookedContainers[0].JC_GrossWeight = 2340m;
			NewFCLShipment(Sailing2, Principal2, true, ShipmentStatusList.Codes.Confirmed, 2, 0); // not counted (canceled)
			NewFCLShipment(Sailing1, Principal1, false, ShipmentStatusList.Codes.WaitListed, 0, 2); // not counted (wait listed)
			rel = NewFCLShipment(Sailing3, Principal2, false, ShipmentStatusList.Codes.Confirmed, 3, 2); // 17.2 Tonnes (3*(2.28 + 0.12) + 2*(4.35 + 0.65)), 0 cubic meters, 3 GP TEUs, 4 Reefer TEUs, 2 power points
			duplicated = NewFCLShipment(Sailing3, Principal2, false, ShipmentStatusList.Codes.Confirmed, 0, 1, "FAKE1"); // 4.7 Tonnes (Tare: 4.35 Net: 0.35), 0 cubic meters, 0 GP TEUs, 2 Reefer TEUs, 1 power points
			duplicated.RealContainers[0].JC_GrossWeight = 4.7m;
			duplicated = NewFCLShipment(Sailing3, Principal2, false, ShipmentStatusList.Codes.Confirmed, 0, 1, "FAKE1"); // 4.65 Tonnes (Tare: 4.35 Net: 0.30), 0 cubic meters, 0 GP TEUs, 2 Reefer TEUs, 1 power points
			duplicated.RealContainers[0].JC_GrossWeight = 4.65m;
			NewFCLShipment(Sailing3, Principal1, false, ShipmentStatusList.Codes.WebBooking, 4, 1); // not counted (web booking)
			NewRORShipment(Sailing2, Principal1, false, ShipmentStatusList.Codes.Confirmed, 1); // 0.65 Tonnes, 4 cubic meters, 8 square meters
			NewRORShipment(Sailing1, Principal2, true, ShipmentStatusList.Codes.Confirmed, 4); // not counted (canceled)
			NewRORShipment(Sailing3, Principal1, false, ShipmentStatusList.Codes.WaitListed, 3); // not counted (wait listed)
			NewRORShipment(Sailing1, Principal2, false, ShipmentStatusList.Codes.Booked, 3); // 1.95 Tonnes, 12 cubic meters, 24 square meters
			NewRORShipment(Sailing1, Principal1, false, ShipmentStatusList.Codes.WebBooking, 3); // not counted (web booking)
			AgencyShipmentContainer ignoredContainer1 = bkd.RealContainers.AddNew(); // ignore "real" containers on a booking (shouldn't exist anyway).
			ignoredContainer1.JC_RC = RC_20GP_PK;
			ignoredContainer1.JC_Calc_NetWeight = 20000;
			AgencyShipmentContainer ignoredContainer2 = rel.BookedContainers.AddNew(); // ignore "booked" containers on a bill ("real" containers should superceed "booked" containers)
			ignoredContainer2.JC_RC = RC_20RE_PK;
			ignoredContainer2.JC_Calc_NetWeight = 25000;
			// Principal1
			// Sailing1: 50 Tonnes, 50 cubic meters
			// Sailing2: 15.25 (14.6 + 0.65) Tonnes, 4 cubic meters, 4 GP TEUs, 2 Reefer TEUs, 1 power point, 8 square meters
			// Sailing3: 0 Tonnes, 0 cubic meters
			// Sailing4: 0 Tonnes, 0 cubic meters
			// TOTAL
			// Sailing1: 51.95 (50 + 1.95) Tonnes, 62 (50 + 12) cubic meters, 24 square meters
			// Sailing2: 59.25 (44 + 14.6 + 0.65) Tonnes, 48 (44 + 4) cubic meters, 4 GP TEU, 2 Reefer TEUs, 1 power point, 8 square meters
			// Sailing3: 22.2 Tonnes, 0 cubic meters, 3 GP TEUs, 6 Reefer TEUs, 3 power points
			// Sailing4: 0 Tonnes, 0 cubic meters
			Factory.Save();
		}
		#endregion
	}
}
