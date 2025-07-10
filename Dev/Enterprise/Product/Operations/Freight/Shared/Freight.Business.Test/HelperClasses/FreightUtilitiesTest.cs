using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class FreightUtilitiesTest : BaseFreightTest
	{
		public void TestCreateScheduleFromRoutingSecurityCheckpoint()
		{
			AssertEquals(Env.Security.SailingScheduleCreateFromJob, FreightUtilities.GetCreateScheduleFromJobSecurityCheckpoint(Constants.TransportModes.Sea));
			AssertEquals(Env.Security.FlightScheduleCreateFromJob, FreightUtilities.GetCreateScheduleFromJobSecurityCheckpoint(Constants.TransportModes.Air));
			AssertEquals(Env.Security.TruckScheduleCreateFromJob, FreightUtilities.GetCreateScheduleFromJobSecurityCheckpoint(Constants.TransportModes.Road));
			AssertEquals(Env.Security.RailScheduleCreateFromJob, FreightUtilities.GetCreateScheduleFromJobSecurityCheckpoint(Constants.TransportModes.Rail));
			AssertEquals(null, FreightUtilities.GetCreateScheduleFromJobSecurityCheckpoint("XXX"));
		}

		public void TestEditScheduleSecurityCheckpoint()
		{
			AssertEquals(Env.Security.SailingScheduleEdit, FreightUtilities.GetEditScheduleSecurityCheckpoint(Constants.TransportModes.Sea));
			AssertEquals(Env.Security.FlightScheduleEdit, FreightUtilities.GetEditScheduleSecurityCheckpoint(Constants.TransportModes.Air));
			AssertEquals(Env.Security.TruckScheduleEdit, FreightUtilities.GetEditScheduleSecurityCheckpoint(Constants.TransportModes.Road));
			AssertEquals(Env.Security.RailScheduleEdit, FreightUtilities.GetEditScheduleSecurityCheckpoint(Constants.TransportModes.Rail));
			AssertEquals(null, FreightUtilities.GetEditScheduleSecurityCheckpoint("XXX"));
		}

		public void TestAllowScheduleCreation()
		{
			var mappings = new[]
			{
				new { Mode = Constants.TransportModes.Sea, Checkpoint = Env.Security.SailingScheduleCreateFromJob },
				new { Mode = Constants.TransportModes.Air, Checkpoint = Env.Security.FlightScheduleCreateFromJob },
				new { Mode = Constants.TransportModes.Road, Checkpoint = Env.Security.TruckScheduleCreateFromJob },
				new { Mode = Constants.TransportModes.Rail, Checkpoint = Env.Security.RailScheduleCreateFromJob },
			};

			foreach (var map in mappings)
			{
				map.Checkpoint.IsAllowed = false;
			}

			foreach (var map in mappings)
			{
				map.Checkpoint.IsAllowed = true;
				AssertEquals(map.Mode + ": allowed", true, FreightUtilities.AllowCreateScheduleFromJob(map.Mode));

				map.Checkpoint.IsAllowed = false;
				AssertEquals(map.Mode + ": allowed", false, FreightUtilities.AllowCreateScheduleFromJob(map.Mode));
			}
		}

		public void TestCalculateVolume()
		{
			AssertEquals("Multiplier is 0", 0m, FreightUtilities.CalculateVolume(1.5m, 0, 50m, 50m, 50m, Constants.Length.Centimetres, Constants.Volume.CubicMetres, JobPackLinesSchema.JL_ActualVolume.Scale));
			AssertEquals("Calculate Volume from Lgth, Wdth, Hgt, Pkgs.", 1.25m, FreightUtilities.CalculateVolume(1.5m, 10, 50m, 50m, 50m, Constants.Length.Centimetres, Constants.Volume.CubicMetres, JobPackLinesSchema.JL_ActualVolume.Scale));
			AssertEquals("Dimension unit changed to CM.", 1.25m, FreightUtilities.CalculateVolume(1.5m, 10, 0.5m, 0.5m, 0.5m, Constants.Length.Metres, Constants.Volume.CubicMetres, JobPackLinesSchema.JL_ActualVolume.Scale));
			AssertEquals("Volume unit changed to CF.", 44.143m, ZArchitecture.Core.Utilities.Round(FreightUtilities.CalculateVolume(1.5m, 10, 0.5m, 0.5m, 0.5m, Constants.Length.Metres, Constants.Volume.CubicFeet, JobPackLinesSchema.JL_ActualVolume.Scale), 3));
		}

		public void TestCalculateVolume_OverridenRoundingFuncShouldBeInvoked()
		{
			// 5 x 1.22 x 1.46 x 1.05 = 9.3513
			var defaultRoundingResult = FreightUtilities.CalculateVolume(9.3513m, 5, 1.22m, 1.46m, 1.05m, Constants.Length.Metres, Constants.Volume.CubicMetres, JobPackLinesSchema.JL_ActualVolume.Scale);
			AssertEquals("Default rounding when there is no overriden method", 9.351m, defaultRoundingResult);

			var overriddenRoundingResult = FreightUtilities.CalculateVolume(9.3513m, 5, 1.22m, 1.46m, 1.05m,
				Constants.Length.Metres, Constants.Volume.CubicMetres, JobPackLinesSchema.JL_ActualVolume.Scale,
				overridenRoundingFunc: Math.Ceiling);
			AssertEquals("Overriden rounding function execution should replace default rounding", 10m, overriddenRoundingResult);
		}

		public void TestShipmentContainerMode()
		{
			AssertEquals("LSE -> LSE.", Constants.ContainerModes.Loose, FreightUtilities.ShipmentContainerMode(Constants.ContainerModes.Loose, Constants.TransportModes.Air));
			AssertEquals("GRP -> LCL.", Constants.ContainerModes.LCL, FreightUtilities.ShipmentContainerMode(Constants.ContainerModes.Groupage, Constants.TransportModes.Sea));
			AssertEquals("SEA/BCN -> BCN.", Constants.ContainerModes.BuyersConsol, FreightUtilities.ShipmentContainerMode(Constants.ContainerModes.BuyersConsol, Constants.TransportModes.Sea));
			AssertEquals("AIR/BCN -> BCN.", Constants.ContainerModes.BuyersConsol, FreightUtilities.ShipmentContainerMode(Constants.ContainerModes.BuyersConsol, Constants.TransportModes.Air));
			AssertEquals("OTH -> blank.", "", FreightUtilities.ShipmentContainerMode(Constants.ContainerModes.Other, Constants.TransportModes.Air));
		}

		public void TestIsValidWeightUnit()
		{
			AssertEquals("Valid WeightUnit.", true, FreightUtilities.IsValidWeightUnit("LB"));
			AssertEquals("Invalid WeightUnit.", false, FreightUtilities.IsValidWeightUnit("+1"));
			AssertEquals("Blank WeightUnit.", false, FreightUtilities.IsValidWeightUnit(""));
			AssertEquals("Volume Unit should be invalid.", false, FreightUtilities.IsValidWeightUnit("M3"));
		}

		public void TestIsValidVolumeUnit()
		{
			AssertEquals("Valid VolumeUnit.", true, FreightUtilities.IsValidVolumeUnit("CF"));
			AssertEquals("Invalid VolumeUnit.", false, FreightUtilities.IsValidVolumeUnit("+1"));
			AssertEquals("Blank VolumeUnit.", false, FreightUtilities.IsValidVolumeUnit(""));
			AssertEquals("Weight Unit should be invalid.", false, FreightUtilities.IsValidVolumeUnit("KG"));
		}

		public void TestIsValidDimensionUnit()
		{
			AssertEquals("Valid Unit.", true, FreightUtilities.IsValidDimensionUnit("CM"));
			AssertEquals("Invalid Unit.", false, FreightUtilities.IsValidDimensionUnit("+1"));
			AssertEquals("Blank Unit.", false, FreightUtilities.IsValidDimensionUnit(""));
			AssertEquals("Weight Unit should be invalid.", false, FreightUtilities.IsValidDimensionUnit("KG"));
		}

		#region ShipmentHBLDeliveryMode

		public void TestShipmentHBLDeliveryMode()
		{
			var hblDeliveryModeList = FreightUtilities.ShipmentHBLDeliveryMode("").HBLDeliveryModesList;
			AssertEquals(0, hblDeliveryModeList.Count);

			hblDeliveryModeList = FreightUtilities.ShipmentHBLDeliveryMode("XXX").HBLDeliveryModesList;
			AssertEquals(0, hblDeliveryModeList.Count);

			hblDeliveryModeList = FreightUtilities.ShipmentHBLDeliveryMode("FCL").HBLDeliveryModesList;
			AssertEquals(9, hblDeliveryModeList.Count);
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "CFS/CFS");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "CFS/CY");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "CFS/DOOR");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "CY/CY");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "CY/CFS");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "CY/DOOR");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/DOOR");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/CFS");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/CY");

			hblDeliveryModeList = FreightUtilities.ShipmentHBLDeliveryMode("LCL").HBLDeliveryModesList;
			AssertEquals(4, hblDeliveryModeList.Count);
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "CFS/CFS");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "CFS/DOOR");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/DOOR");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/CFS");

			hblDeliveryModeList = FreightUtilities.ShipmentHBLDeliveryMode("BCN").HBLDeliveryModesList;
			AssertEquals(6, hblDeliveryModeList.Count);
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "CFS/CFS");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "CFS/DOOR");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "CFS/CY");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/DOOR");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/CFS");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/CY");

			hblDeliveryModeList = FreightUtilities.ShipmentHBLDeliveryMode("SCN").HBLDeliveryModesList;
			AssertEquals(6, hblDeliveryModeList.Count);
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "CFS/CFS");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "CFS/DOOR");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "CY/CFS");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/DOOR");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/CFS");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "CY/DOOR");

			hblDeliveryModeList = FreightUtilities.ShipmentHBLDeliveryMode("BLK").HBLDeliveryModesList;
			AssertEquals(4, hblDeliveryModeList.Count);
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/DOOR");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/PORT");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "PORT/PORT");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "PORT/DOOR");

			hblDeliveryModeList = FreightUtilities.ShipmentHBLDeliveryMode("BBK").HBLDeliveryModesList;
			AssertEquals(4, hblDeliveryModeList.Count);
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/DOOR");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/PORT");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "PORT/PORT");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "PORT/DOOR");

			hblDeliveryModeList = FreightUtilities.ShipmentHBLDeliveryMode("ROR").HBLDeliveryModesList;
			AssertEquals(4, hblDeliveryModeList.Count);
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/DOOR");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/PORT");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "PORT/PORT");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "PORT/DOOR");

			hblDeliveryModeList = FreightUtilities.ShipmentHBLDeliveryMode("LQD").HBLDeliveryModesList;
			AssertEquals(4, hblDeliveryModeList.Count);
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/DOOR");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/PORT");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "PORT/PORT");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "PORT/DOOR");

			hblDeliveryModeList = FreightUtilities.ShipmentHBLDeliveryMode("LSE").HBLDeliveryModesList;
			AssertEquals(9, hblDeliveryModeList.Count);
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "CFS/DOOR");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "CFS/DOOR");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/CFS");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/DOOR");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "ARPT/DOOR");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "ARPT/CFS");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/ARPT");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "CFS/ARPT");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "ARPT/ARPT");

			hblDeliveryModeList = FreightUtilities.ShipmentHBLDeliveryMode("ULD").HBLDeliveryModesList;
			AssertEquals(9, hblDeliveryModeList.Count);
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "CFS/DOOR");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "CFS/DOOR");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/CFS");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/DOOR");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "ARPT/DOOR");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "ARPT/CFS");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "DOOR/ARPT");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "CFS/ARPT");
			AssertContainsHBLDeliveryMode(hblDeliveryModeList, "ARPT/ARPT");
		}

		void AssertContainsHBLDeliveryMode(CodeDescriptionPairList hblDeliveryModeList, ZString code)
		{
			AssertEquals(true, hblDeliveryModeList.ContainsCode(code));
		}

		#endregion

		#region Aviation Security

		public void TestIsCountryWithAviationSecurityValidation()
		{
			AssertEquals("US", true, FreightUtilities.IsCountryWithAviationSecurityValidation("US"));
			AssertEquals("AU", false, FreightUtilities.IsCountryWithAviationSecurityValidation("AU"));
			AssertEquals("JP", true, FreightUtilities.IsCountryWithAviationSecurityValidation("JP"));
		}

		public void TestGetCountrySpecificInspectionTypeDefault()
		{
			using (FreightDataRegistry.Instance.ShipmentInspectionTypeDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "XRY"))
			using (FreightDataRegistry.Instance.ShipmentInspectionTypeDefault_Japan.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "DIP"))
			using (FreightDataRegistry.Instance.ShipmentInspectionTypeDefault_HongKong.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "HMR"))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
				{
					AssertEquals("XRY", FreightUtilities.SupplyChainSecurityConfiguration.InspectionTypeDefault);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
				{
					AssertEquals("DIP", FreightUtilities.SupplyChainSecurityConfiguration.InspectionTypeDefault);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
				{
					AssertEquals("HMR", FreightUtilities.SupplyChainSecurityConfiguration.InspectionTypeDefault);

					using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_HK.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
					{
						AssertEquals("Blank if registry is disabled", ZString.Empty, FreightUtilities.SupplyChainSecurityConfiguration.InspectionTypeDefault);
					}
				}
			}
		}

		public void TestCountrySpecificDefaultsForCurrentCountry()
		{
			BusinessObject jpCompany = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			BusinessObject jpBranch = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));

			jpCompany[GlbCompanySchema.Constants.GC_RN_NKCountryCode] = Constants.CountryCodes.Japan;
			jpBranch[GlbBranchSchema.Constants.GB_GC] = jpCompany.PK;

			Factory.Save();

			using (FreightDataRegistry.Instance.ShipmentInspectionTypeDefault_Australia.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "XRY"))
			using (FreightDataRegistry.Instance.ShipmentInspectionTypeDefault_Japan.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "DIP"))
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, jpBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
				{
					AssertEquals("DIP", FreightUtilities.SupplyChainSecurityConfiguration.InspectionTypeDefault);
				}

				AssertEquals("XRY", FreightUtilities.SupplyChainSecurityConfiguration.InspectionTypeDefault);
			}
		}

		public void TestInspectionType_Approved_Description()
		{
			AssertEquals("Approved/Known Shipper", FreightUtilities.InspectionType_Approved_Description);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				AssertEquals("Approved (Known or Account Consignor/Regulated Agent)", FreightUtilities.InspectionType_Approved_Description);
			}
		}

		#endregion
	}
}
