using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingModuleConsol))]
	sealed class ModuleConsolTest : EnterpriseBusinessObjectTestCase
	{
		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.ForwardingConsol);
			}
		}

		#endregion

		#region JK_RoutingStatus

		public void TestJK_RoutingStatus()
		{
			ForwardingModuleConsol consol = Factory.New<ForwardingModuleConsol>();
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "SGSIN";

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUBNE";
			transport.JW_RL_NKDiscPort = "SGSIN";
			AssertEquals(true, consol.JK_RoutingComplete);

			transport.JW_RL_NKDiscPort = "";
			AssertEquals(false, consol.JK_RoutingComplete);

			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKDiscPort = "SGSIN";
			AssertEquals(true, consol.JK_RoutingComplete);

			transport.JW_RL_NKLoadPort = "NZAKL";
			AssertEquals(false, consol.JK_RoutingComplete);
		}

		#endregion

		#region JK_CRN

		public void TestJK_CRN()
		{
			const string TestCRNNumber = "1S019201820";
			ForwardingModuleConsol consol = new ForwardingModuleConsolCollection(Factory).AddNew();
			AssertEquals("By default there should be no CRN", "", consol.JK_CRN);

			BusinessObject cRN = consol.CusEntryNums.AddNew();
			cRN[CusEntryNumSchema.Constants.CE_ParentID] = consol.PK;
			cRN[CusEntryNumSchema.Constants.CE_ParentTable] = consol.TableName;
			cRN[CusEntryNumSchema.Constants.CE_RN_NKCountryCode] = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cRN[CusEntryNumSchema.Constants.CE_EntryType] = CusEntryNumberTypes.Australia.CRN;
			cRN[CusEntryNumSchema.Constants.CE_EntryIsSystemGenerated] = true;
			cRN[CusEntryNumSchema.Constants.CE_EntryNum] = TestCRNNumber;

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			ForwardingModuleConsolCollection collection = new ForwardingModuleConsolCollection(factory2);
			collection.Load(new ZQuery(JobConsolSchema.PK, consol.PK));
			consol = collection[0];

			AssertEquals("CRN should be new TestCRNNumber", TestCRNNumber, consol.JK_CRN);
			cRN.Delete();
			factory2.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ConsolDomainService.GetInstance(Factory).ModuleConsolCollection = null;
			ForwardingModuleConsolCollection freshCollection = new ForwardingModuleConsolCollection(Factory);
			freshCollection.Load(new ZQuery(JobConsolSchema.PK, consol.PK));
			consol = freshCollection[0];
			AssertEquals("CRN should be blank again", "", consol.JK_CRN);
		}

		#endregion

		#region Test Customs Entry Number

		public void TestRetrieveEntryNumberOnAU()
		{
			string currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			GlbCompany.CurrentCompany.SetCountry("AU");
			ForwardingModuleConsol consol = new ForwardingModuleConsolCollection(Factory).AddNew();
			AssertEquals("Entry number retrieved for AU", 0, consol.CusEntryNums.Count);

			CusEntryNumber entryNumberForAU = Factory.New<CusEntryNumber>();
			entryNumberForAU.CE_EntryNum = "EntryNumForAU";
			entryNumberForAU.CE_ParentID = consol.PK;
			entryNumberForAU.CE_ParentTable = consol.TableName;
			entryNumberForAU.CE_RN_NKCountryCode = "AU";

			CusEntryNumber entryNumberForNZ = Factory.New<CusEntryNumber>();
			entryNumberForNZ.CE_EntryNum = "EntryNumForNZ";
			entryNumberForNZ.CE_ParentID = consol.PK;
			entryNumberForNZ.CE_ParentTable = consol.TableName;
			entryNumberForNZ.CE_RN_NKCountryCode = "NZ";

			Factory.Save();
			consol.CusEntryNums.Load();
			AssertEquals("Entry number retrieved for AU", entryNumberForAU.CE_EntryNum, consol.JK_CRN);

			GlbCompany.CurrentCompany.SetCountry(currentCountry);
		}

		public void TestEntryStatus()
		{
			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryStatus = "XYZ";
			entryNumber.CE_EntryNum = "123456";
			entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			ForwardingModuleConsol consol = new ForwardingModuleConsolCollection(Factory).AddNew();
			entryNumber.CE_ParentID = consol.PK;
			entryNumber.CE_ParentTable = consol.TableName;
			Factory.Save();
			AssertEquals("Entry Number", entryNumber.CE_EntryStatus, consol.JK_EntryStatus);
		}

		#endregion

		#region JK_SecurityStatus

		public void TestJK_SecurityStatus_NotAir()
		{
			var consol = new ForwardingModuleConsolCollection(Factory).AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(ZString.Empty, consol.JK_SecurityStatus);

			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals(ZString.Empty, consol.JK_SecurityStatus);

			consol.JK_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals(ZString.Empty, consol.JK_SecurityStatus);
		}

		public void TestJK_SecurityStatus_EU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var consol = new ForwardingModuleConsolCollection(Factory).AddNew();
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;

				var transport = consol.Transports[0];
				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport.JW_RL_NKLoadPort = "USLAX";
				transport.JW_RL_NKDiscPort = "GBLON";
				transport.JW_IsCargoOnly = false;

				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertEquals("SupplyChainSecurityConfiguration is not enabled", ZString.Empty, consol.JK_SecurityStatus);
					ClearJK_SecurityStatusCache(consol);
				}

				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("SupplyChainSecurityConfiguration is not ExportForAviationSecurityPurposes", ZString.Empty, consol.JK_SecurityStatus);
					ClearJK_SecurityStatusCache(consol);

					transport.JW_RL_NKLoadPort = "GBMAN";
					AssertEquals("No shipments", ZString.Empty, consol.JK_SecurityStatus);
					ClearJK_SecurityStatusCache(consol);

					var shipment1 = consol.Shipments.AddNew();
					shipment1.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
					AssertEquals("Shipment inspection is unknown.",
						Forwarding.AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft,
						consol.JK_SecurityStatus);
					ClearJK_SecurityStatusCache(consol);

					shipment1.JS_InspectionTypeCode = "XRY";
					AssertEquals("Shipment inspection is not unknown.",
						Forwarding.AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft,
						consol.JK_SecurityStatus);
					ClearJK_SecurityStatusCache(consol);

					shipment1.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;
					AssertEquals("Shipment inspection is approved.",
						Forwarding.AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft,
						consol.JK_SecurityStatus);
				}
			}
		}

		void ClearJK_SecurityStatusCache(ForwardingModuleConsol consol)
		{
			var jk_SecurityStatusInfo = typeof(ForwardingModuleConsol).GetField("jk_SecurityStatus", BindingFlags.NonPublic | BindingFlags.Instance);
			AssertNotNull(jk_SecurityStatusInfo);
			jk_SecurityStatusInfo.SetValue(consol, null);
		}

		public void TestJK_SecurityStatus_CH()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CH"))
			{
				var inspectionTypes = FreightDataRegistry.Instance.ShipmentInspectionTypes_EU.Value;
				((ShipmentInspectionType)inspectionTypes.Types.FindByCode("NUC")).AllowedOnPassengerFlights = false;

				using (FreightDataRegistry.Instance.ShipmentInspectionTypes_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
				{
					var consol = new ForwardingModuleConsolCollection(Factory).AddNew();
					consol.JK_AgentType = Core.Constants.AgentType.Agent;
					consol.JK_TransportMode = Core.Constants.TransportModes.Air;

					consol.JK_RL_NKLoadPort = "CHGVA";
					var shipment1 = consol.Shipments.AddNew();
					shipment1.JS_TransportMode = "AIR";
					shipment1.JS_RL_NKOrigin = "GBLON";
					shipment1.JS_RL_NKDestination = "USLAX";

					var transport = consol.Transports[0];
					transport.JW_TransportMode = "AIR";
					transport.JW_RL_NKLoadPort = "CHGVA";
					transport.JW_RL_NKDiscPort = "USLAX";
					transport.JW_IsCargoOnly = false;

					shipment1.JS_InspectionTypeCode = "NUC";

					AssertEquals("SCO defaults for consols loading in Switzerland",
						Forwarding.AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly,
						consol.JK_SecurityStatus);
				}
			}
		}

		public void TestJK_SecurityStatus_UK()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var inspectionTypes = FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.Value;
				((ShipmentInspectionType)inspectionTypes.Types.FindByCode("NUC")).AllowedOnPassengerFlights = false;

				using (FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
				{
					var consol = new ForwardingModuleConsolCollection(Factory).AddNew();
					consol.JK_AgentType = Core.Constants.AgentType.Agent;
					consol.JK_TransportMode = Core.Constants.TransportModes.Air;

					consol.JK_RL_NKLoadPort = "GBLHR";
					var shipment1 = consol.Shipments.AddNew();
					shipment1.JS_TransportMode = "AIR";
					shipment1.JS_RL_NKOrigin = "GBLON";
					shipment1.JS_RL_NKDestination = "USLAX";

					var transport = consol.Transports[0];
					transport.JW_TransportMode = "AIR";
					transport.JW_RL_NKLoadPort = "GBLHR";
					transport.JW_RL_NKDiscPort = "USLAX";
					transport.JW_IsCargoOnly = false;

					shipment1.JS_InspectionTypeCode = "NUC";

					AssertEquals("NSC defaults for consols loading in UK",
						Forwarding.AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft,
						consol.JK_SecurityStatus);
				}
			}
		}

		public void TestJK_SecurityStatus_JP()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				var inspectionTypes = FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.Value;
				((ShipmentInspectionType)inspectionTypes.Types.FindByCode("NUC")).AllowedOnPassengerFlights = false;

				using (FreightDataRegistry.Instance.ShipmentInspectionTypes_Japan.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
				{
					var consol = new ForwardingModuleConsolCollection(Factory).AddNew();
					consol.JK_AgentType = Core.Constants.AgentType.Agent;
					consol.JK_TransportMode = Core.Constants.TransportModes.Air;

					consol.JK_RL_NKLoadPort = "JPOSA";
					var shipment1 = consol.Shipments.AddNew();
					shipment1.JS_TransportMode = "AIR";
					shipment1.JS_RL_NKOrigin = "JPOSA";
					shipment1.JS_RL_NKDestination = "USLAX";

					var transport = consol.Transports[0];
					transport.JW_TransportMode = "AIR";
					transport.JW_RL_NKLoadPort = "GBLHR";
					transport.JW_RL_NKDiscPort = "USLAX";
					transport.JW_IsCargoOnly = false;

					shipment1.JS_InspectionTypeCode = "NUC";

					AssertEquals("SCO defaults for consols loading in Japan",
						Forwarding.AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly,
						consol.JK_SecurityStatus);
				}
			}
		}

		#endregion

		public void TestJK_TotalLoadingMeters()
		{
			var consol = Factory.New<ForwardingModuleConsol>();

			var shipments1 = consol.Shipments.AddNew();
			shipments1.JS_LoadingMeters = 1.21;

			var shipments2 = consol.Shipments.AddNew();
			shipments2.JS_LoadingMeters = 2.25;

			AssertEquals("TotalLoadingMeters", (ZDecimal)3.46, consol.JK_TotalLoadingMeters);
		}

		public void TestJK_Calc_PossibleOversize()
		{
			var consol = Factory.New<ForwardingModuleConsol>();

			var shipments1 = consol.Shipments.AddNew();
			shipments1.JS_TransportMode = Core.Constants.TransportModes.Air;

			var pack1 = shipments1.OuterPackLines.AddNew();
			pack1.JL_Length = 200m;
			pack1.JL_Width = 100m;
			pack1.JL_Height = 80m;
			pack1.JL_UnitOfDimension = Core.Constants.Length.Centimetres;

			var shipments2 = consol.Shipments.AddNew();
			shipments2.JS_TransportMode = Core.Constants.TransportModes.Air;

			var pack2 = shipments2.OuterPackLines.AddNew();
			pack2.JL_Length = 150m;
			pack2.JL_Width = 150m;
			pack2.JL_Height = 300m;
			pack2.JL_UnitOfDimension = Core.Constants.Length.Centimetres;

			AssertEquals("JK_Calc_PossibleOversize", true, consol.JK_Calc_PossibleOversize);
		}

		public void TestLightValidationDisabled()
		{
			var consol = Factory.New<ForwardingModuleConsol>();
			Assert(!consol.LightValidationEnabled);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var consol = Factory.New<ForwardingModuleConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			return consol;
		}
		#endregion
	}
}
