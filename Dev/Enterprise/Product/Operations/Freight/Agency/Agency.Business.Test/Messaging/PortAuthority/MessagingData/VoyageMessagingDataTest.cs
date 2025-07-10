using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal sealed class VoyageMessagingDataTest : BaseAgencyTest
	{
		public void TestGenerationPerformance_Export()
		{
			Env.Registry.ConcatenateMultipleFetchHintTypes = true;
			var voyageFactory = new BusinessObjectFactory();
			var loadedSailing = voyageFactory.Load<JobSailing>(CreateDataForPerformanceTest());
			AssertNotNull("hit collection", loadedSailing.Voyage.Origins);
			AssertNotNull("hit collection", loadedSailing.Voyage.Destinations);
			AssertNotNull("hit collection", loadedSailing.Voyage.Sailings);
			AssertNotNull("hit business object", loadedSailing.Voyage.Vessel);
			voyageFactory.ResetDatabaseLoadCount();

			var message = new PortAuthority(loadedSailing.Voyage);
			message.Port = loadedSailing.Origin.JA_RL_NKPortOfLoading;
			message.Direction = Constants.PortDirection.Load;
			message.MessageType = PortMessageTypeList.Codes.Original;

			var voyageFactoryHits = voyageFactory.TableSelects.Sum(s => s.Value);

			var factory = new BusinessObjectFactory();

			IPortAuthorityMessagingData data = VoyageMessagingData.New(message, factory);
			AssertMaxDbHits("VoyageFactory (nothing further should be loaded in this factory).", voyageFactoryHits, voyageFactory);
			// DO NOT INCREASE THIS WITHOUT FIRST DISCUSSING WITH THE AGENCY TEAM!!!
			AssertMaxDbHits(new Dictionary<string, int>
			{
				{ UNDGDataItemSchema.Constants.TableName, 20 },
				{ OrgAddressSchema.Constants.TableName, 14 },
				{ OrgHeaderSchema.Constants.TableName, 16 },
				{ CusInBondHeaderSchema.Constants.TableName, 10 },
				{ ProcessTasksSchema.Constants.TableName, 10 },
				{ JobVoyageSchema.Constants.TableName, 5 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 3 },
				{ JobVoyDestinationSchema.Constants.TableName, 2 },
				{ JobVoyOriginSchema.Constants.TableName, 2 },
				{ OrgCompanyDataSchema.Constants.TableName, 2 },
				{ CusContainerSchema.Constants.TableName, 1 },
				{ CusEntryNumSchema.Constants.TableName, 1 },
				{ EDIMessageSchema.Constants.TableName, 1 },
				{ GenAddOnColumnSchema.Constants.TableName, 1 },
				{ JobConsolTransportSchema.Constants.TableName, 1 },
				{ JobContainerSchema.Constants.TableName, 1 },
				{ JobContainerPackPivotSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ JobHeaderSchema.Constants.TableName, 1 },
				{ JobPackLinesSchema.Constants.TableName, 1 },
				{ JobSailingSchema.Constants.TableName, 1 },
				{ JobShipmentSchema.Constants.TableName, 1 },
				{ JobTradeLaneVoyageSchema.Constants.TableName, 1 },
				{ JobVoyCountrySchema.Constants.TableName, 1 },
				{ RefCommodityCodeSchema.Constants.TableName, 1 },
				{ RefContainerSchema.Constants.TableName, 1 },
				{ RefContainerStockSchema.Constants.TableName, 1 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ RefVesselSchema.Constants.TableName, 1 },
				{ RefCountryRulesSchema.Constants.TableName, 4 },
				{ StmNoteSchema.Constants.TableName, 10 },
				{ StmALogSchema.Constants.TableName, 10 },
				{ JobCO2eSchema.Constants.TableName, 3 },
			}, factory);
		}

		public void TestGenerationPerformance_Import()
		{
			Env.Registry.ConcatenateMultipleFetchHintTypes = true;
			BusinessObjectFactory voyageFactory = new BusinessObjectFactory();
			JobSailing loadedSailing = voyageFactory.Load<JobSailing>(CreateDataForPerformanceTest());
			AssertNotNull("hit collection", loadedSailing.Voyage.Origins);
			AssertNotNull("hit collection", loadedSailing.Voyage.Destinations);
			AssertNotNull("hit collection", loadedSailing.Voyage.Sailings);
			AssertNotNull("hit business object", loadedSailing.Voyage.Vessel);
			voyageFactory.ResetDatabaseLoadCount();

			var message = new PortAuthority(loadedSailing.Voyage);
			message.Port = loadedSailing.Destination.JB_RL_NKPortOfDischarge;
			message.Direction = Constants.PortDirection.Discharge;
			message.MessageType = PortMessageTypeList.Codes.Original;

			var voyageFactoryHits = voyageFactory.TableSelects.Sum(s => s.Value);

			var factory = new BusinessObjectFactory();
			IPortAuthorityMessagingData data = VoyageMessagingData.New(message, factory);
			AssertMaxDbHits("VoyageFactory (nothing further should be loaded in this factory).", voyageFactoryHits, voyageFactory);
			// DO NOT INCREASE THIS WITHOUT FIRST DISCUSSING WITH THE AGENCY TEAM!!!
			AssertMaxDbHits("PassedInFactory (this is the factory for loading shipments etc).", 88, Factory);
			/*
			UNDGDataItem: 20
			OrgAddress: 13
			OrgHeader: 13
			JobVoyage: 5
			RefCountry: 4
			OrgAddressCapability: 3
			JobVoyDestination: 2
			JobVoyOrigin: 2
			OrgCompanyData: 2
			RefCurrency: 2
			RefPackType: 2
			CusContainer: 1
			CusEntryNum: 1
			EDIMessage: 1
			GenAddOnColumn: 1
			JobConsolTransport: 1
			JobContainer: 1
			JobContainerPackPivot: 1
			JobDocAddress: 1
			JobHeader: 1
			JobPackLines: 1
			JobSailing: 1
			JobShipment: 1
			JobTradeLaneVoyage: 1
			JobVoyCountry: 1
			RefCommodityCode: 1
			RefContainer: 1
			RefContainerStock: 1
			RefServiceLevel: 1
			RefUNLOCO: 1
			RefVessel: 1

			Hits: 88
			*/
		}
		ZGuid CreateDataForPerformanceTest()
		{
			ZDateTime now = ZDateTime.Now;

			BusinessObjectFactory creationFactory = new BusinessObjectFactory();

			OrgHeader shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_IsShippingLine = true;
			shippingLine.OH_IsShippingProvider = true;

			var vessel1 = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First();
			var vessel2 = RefVessel.LookupVesselByName("BANOWATI", Factory).First();
			var vessel3 = RefVessel.LookupVesselByName("CONDOR", Factory).First();

			JobVoyage preVoyage = creationFactory.New<JobVoyage>();
			preVoyage.JV_RV_NKVessel = vessel1.RV_FK;
			preVoyage.JV_VoyageFlight = "x42";
			preVoyage.JV_OH_Line = shippingLine.PK;

			JobSailing preSailing = FindOrCreateSailing(preVoyage, OverseasPort, HomePort);
			preSailing.Origin.JA_E_DEP = now.AddDays(1);
			preSailing.Destination.JB_E_ARV = now.AddDays(2);

			JobVoyage voyage = creationFactory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel2.RV_FK;
			voyage.JV_VoyageFlight = "y42";
			voyage.JV_OH_Line = shippingLine.PK;

			JobSailing sailing = FindOrCreateSailing(voyage, HomePort, OverseasPort2);
			sailing.Origin.JA_E_DEP = now.AddDays(3);
			sailing.Destination.JB_E_ARV = now.AddDays(4);

			JobVoyage postVoyage = creationFactory.New<JobVoyage>();
			postVoyage.JV_RV_NKVessel = vessel3.RV_FK;
			postVoyage.JV_VoyageFlight = "z42";

			JobSailing postSailing = FindOrCreateSailing(postVoyage, OverseasPort2, OverseasPort3);
			postSailing.Origin.JA_E_DEP = now.AddDays(5);
			postSailing.Destination.JB_E_ARV = now.AddDays(6);

			string[] packtypes = new string[]
			{
				Constants.PkgUnit.Bag,
				Constants.PkgUnit.Box,
				Constants.PkgUnit.Keg,
				Constants.PkgUnit.Drum,
			};

			for (int i = 0; i < 10; i++)
			{
				AgencyShipment shipment;

				switch (i & 3)
				{
					case 0:
						shipment = NewShipmentWithoutErrors(creationFactory, sailing, false, true);
						shipment.JS_RL_NKOrigin = HomePort;
						shipment.JS_RL_NKDestination = OverseasPort2;
						break;

					case 1:
						{
							shipment = NewShipmentWithoutErrors(creationFactory, sailing, false, true);
							shipment.JS_RL_NKOrigin = OverseasPort;
							shipment.JS_RL_NKDestination = OverseasPort3;

							Transport precarrage = shipment.Transports.AddNew();
							precarrage.JW_IsLinked = true;
							precarrage.JW_JX = preSailing.PK;

							Transport postcarrage = shipment.Transports.AddNew();
							postcarrage.JW_IsLinked = true;
							postcarrage.JW_JX = postSailing.PK;
						}
						break;

					case 2:
						{
							shipment = NewShipmentWithoutErrors(creationFactory, preSailing, false, true);
							shipment.JS_RL_NKOrigin = OverseasPort;
							shipment.JS_RL_NKDestination = OverseasPort2;

							Transport t = shipment.Transports.AddNew();
							t.JW_IsLinked = true;
							t.JW_JX = sailing.PK;
						}
						break;

					case 3:
						{
							shipment = NewShipmentWithoutErrors(creationFactory, postSailing, false, true);
							shipment.JS_RL_NKOrigin = HomePort;
							shipment.JS_RL_NKDestination = OverseasPort3;

							Transport t = shipment.Transports.AddNew();
							t.JW_IsLinked = true;
							t.JW_JX = sailing.PK;
						}
						break;

					default:
						throw new InvalidOperationException();
				}

				shipment.JS_HouseBill = "Generic OBL" + i;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = NewConsignor(creationFactory).MainAddress.PK;
				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = NewConsignee(creationFactory).MainAddress.PK;
				AddContainer(shipment, "cont " + ((i & ~1) + 1), RC_20GP_PK, true);
				AddContainer(shipment, "cont " + ((i & ~1) + 2), RC_40RE_PK, true);

				for (int j = 0; j < shipment.OuterPackLines.Count; j++)
				{
					shipment.OuterPackLines[j].JL_F3_NKPackType = packtypes[(i + j) % packtypes.Length];
				}
			}

			creationFactory.Save();

			return sailing.PK;
		}

		public void TestGenerationWithMissingLloydsNumber()
		{
			ExportSailing.Vessel.RV_LloydsNumber = "";

			var message = new PortAuthority(ExportSailing.Voyage);
			message.Port = ExportSailing.Origin.JA_RL_NKPortOfLoading;
			message.Direction = Constants.PortDirection.Load;

			VoyageMessagingData.New(message);
			AssertEquals("Issues number", 1, message.Issues.Count);
			AssertIssue(message.Issues[0], ExportSailing.Vessel.PK, "RV", ZString.Format("The vessel '{0}' has no Lloyds number entered.", ExportSailing.JX_JV_NKVessel), ZString.Empty);
		}

		void AssertIssue(PortMessageIssue issue, ZGuid expectedTarget, ZString expectedTargetCode, ZString expectedText, ZString expectedDetails)
		{
			AssertEquals("TargetPK", expectedTarget, issue.TargetPK);
			AssertEquals("TargetCode", expectedTargetCode, issue.TargetCode);
			AssertEquals("Text", expectedText, issue.Text);
			AssertEquals("Detail", expectedDetails, issue.Detail);
		}

		public void TestGenerationWithValidationErrors()
		{
			AgencyShipment shipment = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment.JS_UniqueConsignRef = "V00000000";
			AddContainer(shipment, ContainerNum1, ZGuid.Empty, false); // missing container type is an error.

			shipment.RunPreSaveValidation();
			AssertEquals("precondition:", true, shipment.HasErrors);
			AssertNoMessageErrors("precondition:", shipment);

			Factory.Save();

			var message = new PortAuthority(ExportSailing.Voyage);
			message.Port = ExportSailing.Origin.JA_RL_NKPortOfLoading;
			message.Direction = Constants.PortDirection.Load;

			VoyageMessagingData.New(message);
			AssertEquals("Issues number", 1, message.Issues.Count);
			AssertIssue(message.Issues[0], shipment.PK, "JS", "V00000000 has errors.", "Container Type: Please enter a Container Type.");
		}

		public void TestGenerationWithValidationMessageErrors()
		{
			AgencyShipment shipment = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment.JS_UniqueConsignRef = "V00000000";
			AddContainer(shipment, "", RC_20GP_PK, false); // missing container number is a message error.

			shipment.RunPreSaveValidation();
			AssertNoErrors("precondition:", shipment);
			AssertEquals("precondition:", true, shipment.HasMessageErrors);

			Factory.Save();

			var message = new PortAuthority(ExportSailing.Voyage);
			message.Port = ExportSailing.Origin.JA_RL_NKPortOfLoading;
			message.Direction = Constants.PortDirection.Load;

			VoyageMessagingData.New(message);
			AssertEquals("Issues number", 1, message.Issues.Count);
			AssertIssue(message.Issues[0], shipment.PK, "JS", "V00000000 has message errors.", "Container Number: You have not entered a Container Number.");
		}

		public void TestMessageVesselVoyage()
		{
			IPortAuthorityMessagingData data;

			ExportSailing.Voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			ExportSailing.Voyage.JV_VoyageFlight = "Blah";

			data = GetExportData();
			AssertEquals(TestVessel1.RV_Name, data.VesselName);
			AssertEquals(TestVessel1.RV_LloydsNumber, data.VesselLloyds);

			data = GetImportData();
			AssertEquals(TestVessel1.RV_Name, data.VesselName);
			AssertEquals(TestVessel1.RV_LloydsNumber, data.VesselLloyds);
		}

		public void TestMessageLoadDischarge()
		{
			IPortAuthorityMessagingData data;

			data = GetExportData();
			AssertEquals(ExportSailing.JX_JA_RL_NKPortOfLoading, data.Load);
			AssertEquals("", data.Discharge);

			data = GetImportData();
			AssertEquals("", data.Load);
			AssertEquals(ExportSailing.JX_JB_RL_NKPortOfDischarge, data.Discharge);
		}

		[TestDate(2007, 5, 10, 13, 32, 0)]
		public void TestMessageMessagePrepared()
		{
			AssertEquals(TestDateAttribute.Date, GetExportData().MessagePrepared);
			AssertEquals(TestDateAttribute.Date, GetImportData().MessagePrepared);
		}

		public void TestMessageIsWaybill()
		{
			IDictionary<string, IPortAuthorityConsignmentData> consignments;

			AgencyShipment shipment1 = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment1.JS_HouseBill = "shipment1";
			shipment1.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;

			AgencyShipment shipment2 = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment2.JS_HouseBill = "shipment2";
			shipment2.JS_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;

			Factory.Save();

			consignments = GetConsignmentHash(GetExportData());
			AssertEquals(true, consignments["shipment1"].IsWaybill);
			AssertEquals(false, consignments["shipment2"].IsWaybill);

			consignments = GetConsignmentHash(GetImportData());
			AssertEquals(true, consignments["shipment1"].IsWaybill);
			AssertEquals(false, consignments["shipment2"].IsWaybill);
		}

		public void TestMessagePackingMode()
		{
			IDictionary<string, IPortAuthorityConsignmentData> consignments;

			AgencyShipment shipment1 = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment1.JS_HouseBill = "shipment1";
			shipment1.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;

			AgencyShipment shipment2 = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment2.JS_HouseBill = "shipment2";
			shipment2.JS_PackingMode = Constants.ContainerModes.FCL;

			Factory.Save();

			consignments = GetConsignmentHash(GetExportData());
			AssertEquals(Constants.ContainerModes.RollOnRollOff, consignments["shipment1"].PackingMode);
			AssertEquals(Constants.ContainerModes.FCL, consignments["shipment2"].PackingMode);

			consignments = GetConsignmentHash(GetImportData());
			AssertEquals(Constants.ContainerModes.RollOnRollOff, consignments["shipment1"].PackingMode);
			AssertEquals(Constants.ContainerModes.FCL, consignments["shipment2"].PackingMode);
		}

		public void TestConsignmentFilter()
		{
			IDictionary<string, IPortAuthorityConsignmentData> consignment;
			ZDateTime now = ZDateTime.Now;

			var vessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First();

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "x42";

			JobSailing sailing0 = FindOrCreateSailing(voyage, OverseasPort4, HomePort);
			sailing0.Origin.JA_E_DEP = now.AddDays(0);
			sailing0.Destination.JB_E_ARV = now.AddDays(1);

			JobSailing sailing1 = FindOrCreateSailing(voyage, HomePort, OverseasPort);
			sailing1.Origin.JA_E_DEP = now.AddDays(2);
			sailing1.Destination.JB_E_ARV = now.AddDays(3);

			JobSailing sailing2 = FindOrCreateSailing(voyage, HomePort, OverseasPort2);
			sailing2.Origin.JA_E_DEP = now.AddDays(2);
			sailing2.Destination.JB_E_ARV = now.AddDays(5);

			JobSailing sailing3 = FindOrCreateSailing(voyage, AlternateHomePort, OverseasPort2);
			sailing3.Origin.JA_E_DEP = now.AddDays(4);
			sailing3.Destination.JB_E_ARV = now.AddDays(5);

			JobSailing sailing4 = FindOrCreateSailing(voyage, OverseasPort2, OverseasPort3);
			sailing4.Origin.JA_E_DEP = now.AddDays(6);
			sailing4.Destination.JB_E_ARV = now.AddDays(7);

			AgencyShipment shipment0 = NewShipmentWithoutErrors(sailing0, false, true);
			shipment0.JS_HouseBill = "shipment0";
			shipment0.JS_RL_NKOrigin = OverseasPort;
			shipment0.JS_RL_NKDestination = OverseasPort2;

			Transport transport0 = shipment0.Transports.AddNew();
			transport0.JW_IsLinked = true;
			transport0.JW_JX = sailing2.PK;

			AgencyShipment shipment1 = NewShipmentWithoutErrors(sailing1, false, true);
			shipment1.JS_HouseBill = "shipment1";
			shipment1.JS_RL_NKOrigin = HomePort;
			shipment1.JS_RL_NKDestination = OverseasPort4;

			AgencyShipment shipment2 = NewShipmentWithoutErrors(sailing2, false, true);
			shipment2.JS_HouseBill = "shipment2";
			shipment2.JS_RL_NKOrigin = HomePort;
			shipment2.JS_RL_NKDestination = OverseasPort;

			AgencyShipment shipment3 = NewShipmentWithoutErrors(sailing3, false, true);
			shipment3.JS_HouseBill = "shipment3";
			shipment3.JS_RL_NKOrigin = AlternateHomePort;
			shipment3.JS_RL_NKDestination = OverseasPort2;

			AgencyShipment shipment4 = NewShipmentWithoutErrors(sailing4, false, true);
			shipment4.JS_HouseBill = "shipment4";
			shipment4.JS_RL_NKOrigin = HomePort;
			shipment4.JS_RL_NKDestination = OverseasPort3;

			Transport transport4 = shipment4.Transports.AddNew();
			transport4.JW_IsLinked = true;
			transport4.JW_JX = sailing2.PK;

			NewShipmentWithoutErrors(sailing1, true, true).JS_HouseBill = "decoy1";
			NewShipmentWithoutErrors(sailing2, true, false).JS_HouseBill = "decoy2";
			NewShipmentWithoutErrors(sailing3, false, false).JS_HouseBill = "decoy3";

			Factory.Save();

			consignment = GetConsignmentHash(GetExportData(sailing1));
			AssertEquals("Should have 2 consignments", 4, consignment.Count);
			AssertEquals("Export shipment0", true, consignment.ContainsKey("shipment0"));
			AssertEquals("Export shipment1", true, consignment.ContainsKey("shipment1"));
			AssertEquals("Export shipment2", true, consignment.ContainsKey("shipment2"));
			AssertEquals("Export shipment3", false, consignment.ContainsKey("shipment3"));
			AssertEquals("Export shipment4", true, consignment.ContainsKey("shipment4"));
			AssertEquals("Export decoy1", false, consignment.ContainsKey("decoy1"));
			AssertEquals("Export decoy1", false, consignment.ContainsKey("decoy2"));
			AssertEquals("Export decoy1", false, consignment.ContainsKey("decoy3"));

			consignment = GetConsignmentHash(GetImportData(sailing3));
			AssertEquals("Should have 2 consignments", 4, consignment.Count);
			AssertEquals("Import shipment0", true, consignment.ContainsKey("shipment0"));
			AssertEquals("Import shipment1", false, consignment.ContainsKey("shipment1"));
			AssertEquals("Import shipment2", true, consignment.ContainsKey("shipment2"));
			AssertEquals("Import shipment3", true, consignment.ContainsKey("shipment3"));
			AssertEquals("Import shipment4", true, consignment.ContainsKey("shipment4"));
			AssertEquals("Import decoy1", false, consignment.ContainsKey("decoy1"));
			AssertEquals("Import decoy1", false, consignment.ContainsKey("decoy2"));
			AssertEquals("Import decoy1", false, consignment.ContainsKey("decoy3"));
		}

		public void TestShipmentsFromSlotVoyagesWouldBeIncluded()
		{
			var shipment1 = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment1.JS_HouseBill = "shipment1";

			var shipment2 = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment2.JS_HouseBill = "shipment2";

			var mainVoyage = ExportSailing.Voyage;
			mainVoyage.JV_VoyageType = Constants.VoyageType.MainVoyage;

			Func<string, AgencyShipment> cloneMainVoyageAndCreateNewShipmentOnExportSailing = (houseBill) =>
			{
				var slotVoyage = mainVoyage.Clone() as JobVoyage;
				slotVoyage.JV_OH_Line = NewCarrier().PK;

				CombineAssertions("Precondition: slot voyage setup", () =>
				{
					AssertEquals(mainVoyage.JV_AirSeaRoad, slotVoyage.JV_AirSeaRoad);
					AssertEquals(mainVoyage.JV_VoyageFlight, slotVoyage.JV_VoyageFlight);
					AssertEquals(mainVoyage.JV_RV_NKVessel, slotVoyage.JV_RV_NKVessel);
				});

				var exportSailingOnSlotVoyage = slotVoyage.Sailings.GetSailingFromLoadAndDischarge(ExportSailing.Origin.JA_RL_NKPortOfLoading, ExportSailing.Destination.JB_RL_NKPortOfDischarge);

				var shipmentOnExportSailing = NewShipmentWithoutErrors(exportSailingOnSlotVoyage, false, true);
				shipmentOnExportSailing.JS_HouseBill = houseBill;

				return shipmentOnExportSailing;
			};

			var shipmentOnSlot1 = cloneMainVoyageAndCreateNewShipmentOnExportSailing("shipmentslot1");
			var shipmentOnSlot2 = cloneMainVoyageAndCreateNewShipmentOnExportSailing("shipmentslot2");

			var shipmentUnrelated1 = cloneMainVoyageAndCreateNewShipmentOnExportSailing("shipmentunrelated1");
			shipmentUnrelated1.Sailing.Voyage.JV_VoyageFlight = "2013";

			var shipmentUnrelated2 = cloneMainVoyageAndCreateNewShipmentOnExportSailing("shipmentunrelated2");
			shipmentUnrelated2.Sailing.Voyage.JV_AirSeaRoad = Constants.TransportModes.Rail;

			Factory.Save();

			var consignments = GetConsignmentHash(GetExportData());

			AssertContainsExactElementsInAnyOrder("Shipments from both main and slot voyages are included",
				new[] { "shipment1", "shipment2", "shipmentslot1", "shipmentslot2" },
				consignments.Keys);
		}

		public void TestConsignmentNumber()
		{
			NewShipmentWithoutErrors(ExportSailing, false, true).JS_HouseBill = "shipment1";
			NewShipmentWithoutErrors(ExportSailing, false, true).JS_HouseBill = "shipment2";

			Factory.Save();

			int i;

			i = 1;
			foreach (IPortAuthorityConsignmentData consignment in GetExportData().Consignments)
			{
				AssertEquals(i++, consignment.ConsignmentNumber);
			}

			i = 1;
			foreach (IPortAuthorityConsignmentData consignment in GetImportData().Consignments)
			{
				AssertEquals(i++, consignment.ConsignmentNumber);
			}
		}

		public void TestConsignmentPorts()
		{
			using (RowFactory.SetCachedTables())
			{
				ZDateTime now = ZDateTime.Now;
				IDictionary<string, IPortAuthorityConsignmentData> consignments;

				var vessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First();

				JobVoyage voyage = Factory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "x42";

				JobSailing sailing = FindOrCreateSailing(voyage, HomePort, OverseasPort);
				sailing.Origin.JA_E_DEP = now.AddDays(1);
				sailing.Destination.JB_E_ARV = now.AddDays(2);

				AgencyShipment shipment = NewShipmentWithoutErrors(sailing, false, true);
				shipment.JS_HouseBill = "shipment";
				shipment.JS_RL_NKOrigin = AlternateHomePort;
				shipment.JS_RL_NKDestination = OverseasPort2;

				Factory.Save();

				consignments = GetConsignmentHash(GetExportData(sailing));
				AssertEquals("", consignments["shipment"].CountryOfOrigin);
				AssertEquals(AlternateHomePort, consignments["shipment"].PortOfOrigin);
				AssertEquals("", consignments["shipment"].PortOfLoading);
				AssertEquals(OverseasPort, consignments["shipment"].PortOfDischarge);
				AssertEquals("", consignments["shipment"].PortOfDestination);
				AssertEquals(OverseasPort2, consignments["shipment"].CountryOfDestination);

				consignments = GetConsignmentHash(GetImportData(sailing));
				AssertEquals(AlternateHomePort, consignments["shipment"].CountryOfOrigin);
				AssertEquals("", consignments["shipment"].PortOfOrigin);
				AssertEquals(HomePort, consignments["shipment"].PortOfLoading);
				AssertEquals("", consignments["shipment"].PortOfDischarge);
				AssertEquals(OverseasPort2, consignments["shipment"].PortOfDestination);
				AssertEquals("", consignments["shipment"].CountryOfDestination);
			}
		}

		public void TestConsignmentPortsWhenMessagePortIsTranshipmentPort()
		{
			using (RowFactory.SetCachedTables())
			{
				var now = ZDateTime.Now;

				var vessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First();

				var voyage = Factory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "x42";

				var sailing = FindOrCreateSailing(voyage, HomePort, OverseasPort);
				sailing.Origin.JA_E_DEP = now.AddDays(1);
				sailing.Destination.JB_E_ARV = now.AddDays(2);

				var sailing2 = FindOrCreateSailing(voyage, OverseasPort, OverseasPort2);
				sailing2.Origin.JA_E_DEP = now.AddDays(1);
				sailing2.Destination.JB_E_ARV = now.AddDays(2);

				var shipment = NewShipmentWithoutErrors(sailing, false, true);
				shipment.JS_HouseBill = "shipment";
				shipment.JS_RL_NKOrigin = AlternateHomePort;
				shipment.JS_RL_NKDestination = OverseasPort3;

				var transport2 = shipment.Transports.AddNew();
				transport2.JW_IsLinked = true;
				transport2.JW_JX = sailing2.PK;

				Factory.Save();

				var consignments = GetConsignmentHash(GetExportData(sailing));
				AssertEquals("", consignments["shipment"].CountryOfOrigin);
				AssertEquals(AlternateHomePort, consignments["shipment"].PortOfOrigin);
				AssertEquals("", consignments["shipment"].PortOfLoading);
				AssertEquals(OverseasPort, consignments["shipment"].PortOfDischarge);
				AssertEquals("", consignments["shipment"].PortOfDestination);
				AssertEquals(OverseasPort3, consignments["shipment"].CountryOfDestination);

				consignments = GetConsignmentHash(GetImportData(sailing));
				AssertEquals(AlternateHomePort, consignments["shipment"].CountryOfOrigin);
				AssertEquals("", consignments["shipment"].PortOfOrigin);
				AssertEquals(HomePort, consignments["shipment"].PortOfLoading);
				AssertEquals("", consignments["shipment"].PortOfDischarge);
				AssertEquals(OverseasPort3, consignments["shipment"].PortOfDestination);
				AssertEquals("", consignments["shipment"].CountryOfDestination);

				consignments = GetConsignmentHash(GetExportData(sailing2));
				AssertEquals("", consignments["shipment"].CountryOfOrigin);
				AssertEquals(AlternateHomePort, consignments["shipment"].PortOfOrigin);
				AssertEquals("", consignments["shipment"].PortOfLoading);
				AssertEquals(OverseasPort2, consignments["shipment"].PortOfDischarge);
				AssertEquals("", consignments["shipment"].PortOfDestination);
				AssertEquals(OverseasPort3, consignments["shipment"].CountryOfDestination);

				consignments = GetConsignmentHash(GetImportData(sailing2));
				AssertEquals(AlternateHomePort, consignments["shipment"].CountryOfOrigin);
				AssertEquals("", consignments["shipment"].PortOfOrigin);
				AssertEquals(OverseasPort, consignments["shipment"].PortOfLoading);
				AssertEquals("", consignments["shipment"].PortOfDischarge);
				AssertEquals(OverseasPort3, consignments["shipment"].PortOfDestination);
				AssertEquals("", consignments["shipment"].CountryOfDestination);
			}
		}

		public void TestConsignmentNamesAndAddresses()
		{
			IDictionary<string, IPortAuthorityConsignmentData> consignments;

			AgencyShipment shipment1 = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment1.JS_HouseBill = "shipment1";
			SetupAddress(shipment1.ConsignorDocumentaryAddress, false, 'A');
			SetupAddress(shipment1.ConsigneeDocumentaryAddress, false, 'B');

			AgencyShipment shipment2 = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment2.JS_HouseBill = "shipment2";
			SetupAddress(shipment2.ConsignorDocumentaryAddress, true, 'C');
			SetupAddress(shipment2.ConsigneeDocumentaryAddress, true, 'D');

			Factory.Save();

			const string Format = "Company {0}\nAddress1 {0}\nAddress2 {0}\nCity {0}\nNSW PC {0}";

			consignments = GetConsignmentHash(GetExportData());
			AssertMultilineASCIIEquals("", string.Format(Format, 'A'), consignments["shipment1"].ConsignorNameAndAddress);
			AssertMultilineASCIIEquals("", string.Format(Format, 'C'), consignments["shipment2"].ConsignorNameAndAddress);
			AssertMultilineASCIIEquals("", "", consignments["shipment1"].ConsigneeNameAndAddress);
			AssertMultilineASCIIEquals("", "", consignments["shipment2"].ConsigneeNameAndAddress);

			consignments = GetConsignmentHash(GetImportData());
			AssertMultilineASCIIEquals("", "", consignments["shipment1"].ConsignorNameAndAddress);
			AssertMultilineASCIIEquals("", "", consignments["shipment2"].ConsignorNameAndAddress);
			AssertMultilineASCIIEquals("", string.Format(Format, 'B'), consignments["shipment1"].ConsigneeNameAndAddress);
			AssertMultilineASCIIEquals("", string.Format(Format, 'D'), consignments["shipment2"].ConsigneeNameAndAddress);
		}

		public void TestConsignmentOnlySelectedPrincipalInMessageAreIncluded()
		{
			IDictionary<string, IPortAuthorityConsignmentData> consignments;

			AgencyShipment shipment1 = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment1.JS_HouseBill = "shipment1";
			SetupAddress(shipment1.ConsignorDocumentaryAddress, false, 'A');
			SetupAddress(shipment1.ConsigneeDocumentaryAddress, false, 'B');

			AgencyShipment shipment2 = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment2.JS_HouseBill = "shipment2";
			shipment2.JS_OH_DeliveryAgent = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();
			SetupAddress(shipment2.ConsignorDocumentaryAddress, true, 'C');
			SetupAddress(shipment2.ConsigneeDocumentaryAddress, true, 'D');

			Factory.Save();

			const string Format = "Company {0}\nAddress1 {0}\nAddress2 {0}\nCity {0}\nNSW PC {0}";

			var message = new PortAuthority(ExportSailing.Voyage);
			message.Port = ExportSailing.Origin.JA_RL_NKPortOfLoading;
			message.Direction = Constants.PortDirection.Load;
			message.DeliverTo3rdParty = false;
			message.PrincipalPK = shipment1.JS_OH_DeliveryAgent;

			consignments = GetConsignmentHash(VoyageMessagingData.New(message));

			AssertEquals(1, consignments.Count);
			AssertMultilineASCIIEquals("", string.Format(Format, 'A'), consignments["shipment1"].ConsignorNameAndAddress);
			AssertMultilineASCIIEquals("", "", consignments["shipment1"].ConsigneeNameAndAddress);
		}

		public void TestGoodsFilter()
		{
			ZDateTime now = ZDateTime.Now;

			IDictionary<string, IPortAuthorityGoodsData> goods;

			var vessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First();

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "x42";

			JobSailing sailing1 = FindOrCreateSailing(voyage, HomePort, OverseasPort);
			sailing1.Origin.JA_E_DEP = now.AddDays(1);
			sailing1.Destination.JB_E_ARV = now.AddDays(3);

			JobSailing sailing2 = FindOrCreateSailing(voyage, HomePort, OverseasPort2);
			sailing2.Destination.JB_E_ARV = now.AddDays(4);

			JobSailing sailing3 = FindOrCreateSailing(voyage, AlternateHomePort, OverseasPort2);
			sailing3.Origin.JA_E_DEP = now.AddDays(2);

			AgencyShipment shipment1 = NewShipmentWithoutErrors(sailing1, false, true);
			shipment1.JS_HouseBill = "shipment1";
			AddPackline(shipment1, "g1", "marks1");
			AddPackline(shipment1, "g2", "marks2");

			AgencyShipment shipment2 = NewShipmentWithoutErrors(sailing2, false, true);
			shipment2.JS_HouseBill = "shipment2";
			AddPackline(shipment2, "g3", "marks3");
			AddPackline(shipment2, "g4", "marks4");

			AgencyShipment shipment3 = NewShipmentWithoutErrors(sailing3, false, true);
			shipment3.JS_HouseBill = "shipment3";
			AddPackline(shipment3, "g5", "marks5");
			AddPackline(shipment3, "g6", "marks6");

			Factory.Save();

			goods = GetGoodsHash(GetExportData(sailing1));
			AssertEquals("should have found 4 goods lines", 4, goods.Count);
			AssertEquals(true, goods.ContainsKey("shipment1-g1"));
			AssertEquals(true, goods.ContainsKey("shipment1-g2"));
			AssertEquals(true, goods.ContainsKey("shipment2-g3"));
			AssertEquals(true, goods.ContainsKey("shipment2-g4"));
			AssertEquals(false, goods.ContainsKey("shipment3-g5"));
			AssertEquals(false, goods.ContainsKey("shipment3-g6"));

			goods = GetGoodsHash(GetImportData(sailing3));
			AssertEquals("should have found 4 goods lines", 4, goods.Count);
			AssertEquals(false, goods.ContainsKey("shipment1-g1"));
			AssertEquals(false, goods.ContainsKey("shipment1-g2"));
			AssertEquals(true, goods.ContainsKey("shipment2-g3"));
			AssertEquals(true, goods.ContainsKey("shipment2-g4"));
			AssertEquals(true, goods.ContainsKey("shipment3-g5"));
			AssertEquals(true, goods.ContainsKey("shipment3-g6"));
		}

		public void TestGoodsDescriptionIncludesHarmonisedCode()
		{
			IDictionary<string, IPortAuthorityGoodsData> goods;

			AgencyShipment shipment = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment.JS_HouseBill = "shipment";

			AddPackline(shipment, "g1", "marks1").JL_HarmonisedCode = "12345";
			AddPackline(shipment, "g2", "marks2").JL_HarmonisedCode = "";

			Factory.Save();

			goods = GetGoodsHash(GetExportData());
			AssertEquals(2, goods.Count);
			AssertEquals("packlines with a harmonised code should put it on the first line", true, goods.ContainsKey("shipment-12345\ng1"));
			AssertEquals("packlines without a harmonised code should not add an empty line", true, goods.ContainsKey("shipment-g2"));
		}

		public void TestGoodsLineNumber()
		{
			AgencyShipment shipment1 = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment1.JS_HouseBill = "shipment1";
			AddPackline(shipment1, "g1", "marks1");
			AddPackline(shipment1, "g2", "marks2");

			AgencyShipment shipment2 = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment2.JS_HouseBill = "shipment2";
			AddPackline(shipment2, "g3", "marks3");
			AddPackline(shipment2, "g4", "marks4");

			Factory.Save();

			foreach (IPortAuthorityConsignmentData consignment in GetExportData().Consignments)
			{
				int next = 1;

				foreach (IPortAuthorityGoodsData data in consignment.Goods)
				{
					AssertEquals(consignment.BillOfLading + "-" + data.GoodsDescription, next, data.ItemNumber);
					next++;
				}
			}
		}

		public void TestGoodsMarksAndNumbers()
		{
			IDictionary<string, IPortAuthorityGoodsData> goods;

			AgencyShipment shipment = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment.JS_HouseBill = "shipment";

			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "TEST4100013";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			AddPackline(shipment, "g1", "marks1");
			AddPackline(shipment, "g2", "\n\n\n\n\n\n\n\n\n\nmarks2");

			AgencyShipmentPackLine packline3 = AddPackline(shipment, "g3", "\r\n");
			packline3.JL_JC = container.PK;
			AssertEquals("precondition:", "\r\n", packline3.JL_MarksAndNumbers);

			Factory.Save();

			shipment.RunPreSaveValidation();
			AssertNoErrors(shipment);
			AssertNoMessageErrors(shipment);

			goods = GetGoodsHash(GetExportData());
			AssertEquals("marks1", goods["shipment-g1"].MarksAndNumbers);
			AssertEquals("marks2", goods["shipment-g2"].MarksAndNumbers);
			AssertEquals("", goods["shipment-g3"].MarksAndNumbers);

			goods = GetGoodsHash(GetImportData());
			AssertEquals("marks1", goods["shipment-g1"].MarksAndNumbers);
			AssertEquals("marks2", goods["shipment-g2"].MarksAndNumbers);
			AssertEquals("", goods["shipment-g3"].MarksAndNumbers);
		}

		public void TestGoodsContainerNumber()
		{
			IDictionary<string, IPortAuthorityGoodsData> goods;

			AgencyShipment shipment = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment.JS_HouseBill = "shipment";
			shipment.OuterPackLines.RemoveAndDeleteAll();

			AgencyShipmentContainer container1 = AddContainer(shipment, ContainerNum1, RC_20GP_PK, false);
			AgencyShipmentContainer container2 = AddContainer(shipment, ContainerNum2, RC_20GP_PK);

			AddPackline(shipment, "g1", "marks1").JL_JC = container2.PK;
			AddPackline(shipment, "g2", "marks2").JL_JC = container2.PK;
			AddPackline(shipment, "g3", "marks3");

			Factory.Save();

			goods = GetGoodsHash(GetExportData());
			AssertContainsExactElementsInAnyOrder(new string[] { ContainerNum2 }, goods["shipment-g1"].ContainerNumbers);
			AssertEquals(ContainerNum2, goods["shipment-g1"].ContainerNumber);
			AssertContainsExactElementsInAnyOrder(new string[] { ContainerNum2 }, goods["shipment-g2"].ContainerNumbers);
			AssertEquals(ContainerNum2, goods["shipment-g2"].ContainerNumber);
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), goods["shipment-g3"].ContainerNumbers);
			AssertEquals(string.Empty, goods["shipment-g3"].ContainerNumber);

			goods = GetGoodsHash(GetImportData());
			AssertContainsExactElementsInAnyOrder(new string[] { ContainerNum2 }, goods["shipment-g1"].ContainerNumbers);
			AssertEquals(ContainerNum2, goods["shipment-g1"].ContainerNumber);
			AssertContainsExactElementsInAnyOrder(new string[] { ContainerNum2 }, goods["shipment-g2"].ContainerNumbers);
			AssertEquals(ContainerNum2, goods["shipment-g2"].ContainerNumber);
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), goods["shipment-g3"].ContainerNumbers);
			AssertEquals(string.Empty, goods["shipment-g3"].ContainerNumber);
		}

		public void TestGoodsContainerNumber_ROR()
		{
			IDictionary<string, IPortAuthorityGoodsData> goods;

			AgencyShipment shipment = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment.JS_HouseBill = "shipment";
			shipment.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
			shipment.OuterPackLines.RemoveAndDeleteAll();

			AgencyShipmentContainer container1 = AddContainer(shipment, ContainerNum1, RC_20GP_PK);

			AddPackline(shipment, "g1", "marks1").JL_JC = container1.PK;

			Factory.Save();

			goods = GetGoodsHash(GetExportData());
			AssertEquals(ContainerNum1, goods.FirstOrDefault().Value.ContainerNumber);

			goods = GetGoodsHash(GetImportData());
			AssertEquals(ContainerNum1, goods.FirstOrDefault().Value.ContainerNumber);
		}

		public void TestGoodsWeightAndVolume()
		{
			IDictionary<string, IPortAuthorityGoodsData> goods;
			AgencyShipment shipment = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment.JS_HouseBill = "shipment";

			AgencyShipmentPackLine packline1 = AddPackline(shipment, "g1", "marks1");
			packline1.JL_ActualVolume = 5m;
			packline1.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			packline1.JL_ActualWeight = 6m;
			packline1.JL_ActualWeightUQ = Constants.Weight.Tonnes;

			AgencyShipmentPackLine packline2 = AddPackline(shipment, "g2", "marks2");
			packline2.JL_ActualVolume = 500m;
			packline2.JL_ActualVolumeUQ = Constants.Volume.Litre;
			packline2.JL_ActualWeight = 750m;
			packline2.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			Factory.Save();

			goods = GetGoodsHash(GetExportData());
			AssertEquals(5m, goods["shipment-g1"].CubicMetres);
			AssertEquals(6000m, goods["shipment-g1"].Kilograms);
			AssertEquals(0.5m, goods["shipment-g2"].CubicMetres);
			AssertEquals(750m, goods["shipment-g2"].Kilograms);

			goods = GetGoodsHash(GetImportData());
			AssertEquals(5m, goods["shipment-g1"].CubicMetres);
			AssertEquals(6000m, goods["shipment-g1"].Kilograms);
			AssertEquals(0.5m, goods["shipment-g2"].CubicMetres);
			AssertEquals(750m, goods["shipment-g2"].Kilograms);
		}

		public void TestGoodsPackageCodeAndCount()
		{
			IDictionary<string, IPortAuthorityGoodsData> goods;

			AgencyShipment shipment = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment.JS_HouseBill = "shipment";

			AgencyShipmentPackLine packline1 = AddPackline(shipment, "g1", "marks1");
			packline1.JL_PackageCount = 5;
			packline1.JL_F3_NKPackType = Constants.PkgUnit.Pallet;

			AgencyShipmentPackLine packline2 = AddPackline(shipment, "g2", "marks2");
			packline2.JL_PackageCount = 7;
			packline2.JL_F3_NKPackType = Constants.PkgUnit.Keg;

			Factory.Save();

			goods = GetGoodsHash(GetExportData());
			AssertEquals(5, goods["shipment-g1"].PackageCount);
			AssertEquals("PK", goods["shipment-g1"].PackageCode);

			goods = GetGoodsHash(GetImportData());
			AssertEquals(7, goods["shipment-g2"].PackageCount);
			AssertEquals("PK", goods["shipment-g2"].PackageCode);
		}

		public void TestGoodsAddDummyForEmptyContainers()
		{
			IDictionary<string, IPortAuthorityGoodsData> goods;

			string[] containers1 = new string[] { ContainerNum1 };
			string[] containers1And4 = new string[] { ContainerNum1, ContainerNum4 };

			RefContainer ref20GP = Factory.Load<RefContainer>(RC_20GP_PK);

			AgencyShipment shipment1 = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment1.JS_HouseBill = "shipment1";
			shipment1.JS_PackingMode = Constants.ContainerModes.FCL;
			AddContainer(shipment1, ContainerNum1, RC_20GP_PK, false);
			AddContainer(shipment1, ContainerNum4, RC_20GP_PK, false);

			AgencyShipment shipment2 = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment2.JS_HouseBill = "shipment2";
			shipment2.JS_PackingMode = Constants.ContainerModes.FCL;
			AddContainer(shipment2, ContainerNum1, RC_20GP_PK, false);

			AgencyShipment shipment3 = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment3.JS_HouseBill = "shipment3";
			shipment3.JS_PackingMode = Constants.ContainerModes.FCL;
			AddContainer(shipment3, ContainerNum2, RC_20GP_PK, true);

			AgencyShipment shipment4 = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment4.JS_HouseBill = "shipment4";
			shipment4.JS_PackingMode = Constants.ContainerModes.FCL;

			Factory.Save();

			goods = GetGoodsHash(GetExportData());
			AssertEquals("shipment1 relates to 2 containers", 2, goods["shipment1-EMPTY CONTAINER"].PackageCount);
			AssertEquals("shipment1 should have the correct package code", "BX", goods["shipment1-EMPTY CONTAINER"].PackageCode);
			AssertEquals("empty container weight is that of the container", 2 * ref20GP.RC_TareWeight, goods["shipment1-EMPTY CONTAINER"].Kilograms);
			AssertContainsExactElementsInAnyOrder("both containers 1 & 4 are empty on shipment1", containers1And4, goods["shipment1-EMPTY CONTAINER"].ContainerNumbers);

			AssertEquals("shipment2 relates to 1 container", 1, goods["shipment2-EMPTY CONTAINER"].PackageCount);
			AssertEquals("shipment2 should have the correct package code", "BX", goods["shipment2-EMPTY CONTAINER"].PackageCode);
			AssertEquals("empty container weight is that of the container", 1 * ref20GP.RC_TareWeight, goods["shipment2-EMPTY CONTAINER"].Kilograms);
			AssertContainsExactElementsInAnyOrder("only container1 is empty on shipment2", containers1, goods["shipment2-EMPTY CONTAINER"].ContainerNumbers);

			AssertEquals("shipment3 did not have an empty container", false, goods.ContainsKey("shipment3-EMPTY CONTAINER"));
			AssertEquals("shipment4 did not have an empty container", false, goods.ContainsKey("shipment4-EMPTY CONTAINER"));

			goods = GetGoodsHash(GetImportData());
			AssertEquals("shipment1 relates to 2 containers", 2, goods["shipment1-EMPTY CONTAINER"].PackageCount);
			AssertEquals("shipment1 should have the correct package code", "BX", goods["shipment2-EMPTY CONTAINER"].PackageCode);
			AssertEquals("empty container weight is that of the container", 2 * ref20GP.RC_TareWeight, goods["shipment1-EMPTY CONTAINER"].Kilograms);
			AssertContainsExactElementsInAnyOrder("both containers 1 & 4 are empty on shipment1", containers1And4, goods["shipment1-EMPTY CONTAINER"].ContainerNumbers);

			AssertEquals("shipment2 relates to 1 container", 1, goods["shipment2-EMPTY CONTAINER"].PackageCount);
			AssertEquals("shipment2 should have the correct package code", "BX", goods["shipment2-EMPTY CONTAINER"].PackageCode);
			AssertEquals("empty container weight is that of the container", 1 * ref20GP.RC_TareWeight, goods["shipment2-EMPTY CONTAINER"].Kilograms);
			AssertContainsExactElementsInAnyOrder("only container1 is empty on shipment2", containers1, goods["shipment2-EMPTY CONTAINER"].ContainerNumbers);

			AssertEquals("shipment3 did not have an empty container", false, goods.ContainsKey("shipment3-EMPTY CONTAINER"));
			AssertEquals("shipment4 did not have an empty container", false, goods.ContainsKey("shipment4-EMPTY CONTAINER"));
		}

		public void TestGoodsHarmonisedCode()
		{
			IDictionary<string, IPortAuthorityGoodsData> goods;
			AgencyShipment shipment = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment.JS_HouseBill = "shipment";

			AgencyShipmentPackLine packline1 = AddPackline(shipment, "g1", "marks1");
			packline1.JL_HarmonisedCode = "100";

			AgencyShipmentPackLine packline2 = AddPackline(shipment, "g2", "marks2");
			packline2.JL_HarmonisedCode = "200";

			Factory.Save();

			goods = GetGoodsHash(GetExportData(true));
			AssertEquals("100", goods["shipment-100\ng1"].HarmonisedCode);
			AssertEquals("200", goods["shipment-200\ng2"].HarmonisedCode);

			goods = GetGoodsHash(GetImportData(true));
			AssertEquals("100", goods["shipment-100\ng1"].HarmonisedCode);
			AssertEquals("200", goods["shipment-200\ng2"].HarmonisedCode);

			goods = GetGoodsHash(GetExportData(false));
			AssertEquals("100", goods["shipment-100\ng1"].HarmonisedCode);
			AssertEquals("200", goods["shipment-200\ng2"].HarmonisedCode);

			goods = GetGoodsHash(GetImportData(false));
			AssertEquals("100", goods["shipment-100\ng1"].HarmonisedCode);
			AssertEquals("200", goods["shipment-200\ng2"].HarmonisedCode);
		}

		public void TestGoodsTopLevelPacks()
		{
			var shipment = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment.JS_HouseBill = "shipment";
			shipment.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;

			var topLevelPack1 = AddTopLevelPack(shipment, "one", "marks1");
			topLevelPack1.JC_ContainerCount = 10;
			topLevelPack1.JC_GrossWeight = 100m;
			topLevelPack1.JC_GrossVolume = 1.1m;
			topLevelPack1.JC_HarmonisedCode = "XXX";

			var topLevelPack2 = AddTopLevelPack(shipment, "two", "marks2");
			topLevelPack2.JC_ContainerCount = 20;
			topLevelPack2.JC_GrossWeight = 200m;
			topLevelPack2.JC_GrossVolume = 2.2m;

			Factory.Save();

			var goods = GetGoodsHash(GetExportData(true));
			AssertEquals("Goods for top level packs only", 2, goods.Count);

			var goodsData1 = goods["shipment-XXX\none"];
			CombineAssertions(() =>
			{
				AssertEquals(false, goodsData1.ContainerNumbers.Any());
				AssertEquals("marks1", goodsData1.MarksAndNumbers);
				AssertEquals("XXX\none", goodsData1.GoodsDescription);

				AssertEquals(10, goodsData1.PackageCount);
				AssertEquals("PK", goodsData1.PackageCode);

				AssertEquals(100m, goodsData1.Kilograms);
				AssertEquals(1.1m, goodsData1.CubicMetres);

				AssertEquals("XXX", goodsData1.HarmonisedCode);
			});

			var goodsData2 = goods["shipment-two"];
			CombineAssertions(() =>
			{
				AssertEquals(false, goodsData2.ContainerNumbers.Any());
				AssertEquals("marks2", goodsData2.MarksAndNumbers);
				AssertEquals("two", goodsData2.GoodsDescription);

				AssertEquals(20, goodsData2.PackageCount);
				AssertEquals("PK", goodsData2.PackageCode);

				AssertEquals(200m, goodsData2.Kilograms);
				AssertEquals(2.2m, goodsData2.CubicMetres);
			});
		}

		public void TestGoods_ArrivalContainerYardAddress()
		{
			ZDateTime now = ZDateTime.Now;
			var vessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First();

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "x24";

			JobSailing dischargeSailing = FindOrCreateSailing(voyage, HomePort, "AUSYD");
			dischargeSailing.Origin.JA_E_DEP = now.AddDays(1);
			dischargeSailing.Destination.JB_E_ARV = now.AddDays(3);

			AgencyShipment shipment = NewShipmentWithoutErrors(dischargeSailing, false, true);
			shipment.JS_HouseBill = "shipment";
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var container = AddContainer(shipment, ContainerNum1, RC_20RE_PK);
			AddPackline(shipment, "g1", "marks1").JL_JC = container.PK;

			var consigneeDocumentaryAddress = Factory.NewWithValidTestData<OrgHeader>();
			consigneeDocumentaryAddress.MainAddress.Postcode = "2222";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeDocumentaryAddress.MainAddress.PK;

			Factory.Save();

			var goods = GetGoodsHash(GetImportData(dischargeSailing));
			AssertEquals("ArrivalContainerYardAddress", "POSTCODE-2222", goods.FirstOrDefault().Value.ContainerYardAddress);

			var consigneeDeliveryAddress = shipment.DocAddresses.CreateWithAddressType(DocAddressType.ConsigneePickupDeliveryAddress);
			consigneeDeliveryAddress.Postcode = "3333";

			Factory.Save();

			goods = GetGoodsHash(GetImportData(dischargeSailing));
			AssertEquals("ArrivalContainerYardAddress", "POSTCODE-3333", goods.FirstOrDefault().Value.ContainerYardAddress);

			var arrivalContainerYardAddress = Factory.NewWithValidTestData<OrgAddress>();
			arrivalContainerYardAddress.CompanyName = "AU CONTAINER YARD";
			arrivalContainerYardAddress.Address1 = "98-100";
			arrivalContainerYardAddress.Address2 = "CHAPEL STREET";
			arrivalContainerYardAddress.City = "MARRICKVILLE";
			arrivalContainerYardAddress.Postcode = "2204";

			container.JC_OA_ArrivalContainerYardAddress = arrivalContainerYardAddress.PK;

			Factory.Save();

			goods = GetGoodsHash(GetImportData(dischargeSailing));
			AssertEquals("ArrivalContainerYardAddress", "POSTCODE-3333\nECP-AU CONTAINER YARD\n98-100\nCHAPEL STREET\nMARRICKVILLE 2204", goods.FirstOrDefault().Value.ContainerYardAddress);
		}

		public void TestGoods_DepartureContainerYardAddress()
		{
			ZDateTime now = ZDateTime.Now;
			var vessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First();

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "x24";

			JobSailing loadSailing = FindOrCreateSailing(voyage, "AUSYD", OverseasPort);
			loadSailing.Origin.JA_E_DEP = now.AddDays(1);
			loadSailing.Destination.JB_E_ARV = now.AddDays(3);

			AgencyShipment shipment = NewShipmentWithoutErrors(loadSailing, false, true);
			shipment.JS_HouseBill = "shipment";
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var container = AddContainer(shipment, ContainerNum1, RC_20RE_PK);
			AddPackline(shipment, "g1", "marks1").JL_JC = container.PK;

			var consignorDocumentaryAddress = Factory.NewWithValidTestData<OrgHeader>();
			consignorDocumentaryAddress.MainAddress.Postcode = "2222";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignorDocumentaryAddress.MainAddress.PK;

			Factory.Save();

			var goods = GetGoodsHash(GetExportData(loadSailing));
			AssertEquals("DepartureContainerYardAddress", "POSTCODE-2222", goods.FirstOrDefault().Value.ContainerYardAddress);

			var consignorPickupAddress = shipment.DocAddresses.CreateWithAddressType(DocAddressType.ConsignorPickupDeliveryAddress);
			consignorPickupAddress.Postcode = "3333";

			Factory.Save();

			goods = GetGoodsHash(GetExportData(loadSailing));
			AssertEquals("DepartureContainerYardAddress", "POSTCODE-3333", goods.FirstOrDefault().Value.ContainerYardAddress);

			var departureContainerYardAddress = Factory.NewWithValidTestData<OrgAddress>();
			departureContainerYardAddress.CompanyName = "MARITIME CONTAINER YARD";
			departureContainerYardAddress.OA_CompanyNameOverride = "MARITIME CONTAINER YARD";
			departureContainerYardAddress.Address1 = "20";
			departureContainerYardAddress.Address2 = "CANAL ROAD";
			departureContainerYardAddress.City = "ST PETERS";
			departureContainerYardAddress.Postcode = "2204";

			container.JC_OA_DepartureContainerYardAddress = departureContainerYardAddress.PK;

			Factory.Save();

			goods = GetGoodsHash(GetExportData(loadSailing));
			AssertEquals("DepartureContainerYardAddress", "POSTCODE-3333\nECP-MARITIME CONTAINER YARD\n20\nCANAL ROAD\nST PETERS 2204", goods.FirstOrDefault().Value.ContainerYardAddress);
		}

		public void TestEquipmentMatchAndFilter()
		{
			ZDateTime now = ZDateTime.Now;
			IDictionary<string, IPortAuthorityEquipmentData> equipment;

			var vessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First();

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "x24";

			JobSailing sailing1 = FindOrCreateSailing(voyage, HomePort, OverseasPort);
			sailing1.Origin.JA_E_DEP = now.AddDays(1);
			sailing1.Destination.JB_E_ARV = now.AddDays(3);

			JobSailing sailing2 = FindOrCreateSailing(voyage, HomePort, OverseasPort2);
			sailing2.Destination.JB_E_ARV = now.AddDays(4);

			JobSailing sailing3 = FindOrCreateSailing(voyage, AlternateHomePort, OverseasPort2);
			sailing3.Origin.JA_E_DEP = now.AddDays(2);

			AgencyShipment shipment1 = NewShipmentWithoutErrors(sailing1, false, true);
			shipment1.JS_HouseBill = "shipment1";
			shipment1.JS_PackingMode = Constants.ContainerModes.FCL;
			AddContainer(shipment1, ContainerNum1, RC_20GP_PK, false);
			AddContainer(shipment1, ContainerNum4, RC_20GP_PK, false);

			AgencyShipment shipment2 = NewShipmentWithoutErrors(sailing2, false, true);
			shipment2.JS_HouseBill = "shipment2";
			shipment2.JS_PackingMode = Constants.ContainerModes.FCL;
			AddContainer(shipment2, ContainerNum1, RC_20GP_PK, false);
			AddContainer(shipment2, ContainerNum2, RC_20GP_PK, false);

			AgencyShipment shipment3 = NewShipmentWithoutErrors(sailing3, false, true);
			shipment3.JS_HouseBill = "shipment3";
			shipment3.JS_PackingMode = Constants.ContainerModes.FCL;
			AddContainer(shipment3, ContainerNum3, RC_20GP_PK, false);

			Factory.Save();

			equipment = GetEquipmentHash(GetExportData(sailing1));
			AssertEquals("Should have found 3 distinct containers", 3, equipment.Count);
			AssertEquals("Export Container1", true, equipment.ContainsKey(ContainerNum1));
			AssertEquals("Export Container2", true, equipment.ContainsKey(ContainerNum2));
			AssertEquals("Export Container3", false, equipment.ContainsKey(ContainerNum3));
			AssertEquals("Export Container4", true, equipment.ContainsKey(ContainerNum4));

			equipment = GetEquipmentHash(GetImportData(sailing3));
			AssertEquals("Should have found 3 distinct containers", 3, equipment.Count);
			AssertEquals("Import Container1", true, equipment.ContainsKey(ContainerNum1));
			AssertEquals("Import Container2", true, equipment.ContainsKey(ContainerNum2));
			AssertEquals("Import Container3", true, equipment.ContainsKey(ContainerNum3));
			AssertEquals("Import Container4", false, equipment.ContainsKey(ContainerNum4));
		}

		public void TestEquipmenttIsoCode()
		{
			IDictionary<string, IPortAuthorityEquipmentData> equipment;

			AgencyShipment shipment = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AddContainer(shipment, ContainerNum1, RC_40GP_PK, false);
			AddContainer(shipment, ContainerNum2, RC_20RE_PK, false);

			Factory.Save();

			equipment = GetEquipmentHash(GetExportData());
			AssertEquals("42G0", equipment[ContainerNum1].ContainerISOCode);
			AssertEquals("22R0", equipment[ContainerNum2].ContainerISOCode);

			equipment = GetEquipmentHash(GetImportData());
			AssertEquals("42G0", equipment[ContainerNum1].ContainerISOCode);
			AssertEquals("22R0", equipment[ContainerNum2].ContainerISOCode);
		}

		public void TestEquipmentIsEmpty()
		{
			IDictionary<string, IPortAuthorityEquipmentData> equipment;

			AgencyShipment shipment = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AddContainer(shipment, ContainerNum1, RC_40GP_PK, false); // not packed == empty
			AddContainer(shipment, ContainerNum2, RC_20RE_PK, true); // packed == not empty

			Factory.Save();

			equipment = GetEquipmentHash(GetExportData());
			AssertEquals(true, equipment[ContainerNum1].IsEmpty);
			AssertEquals(false, equipment[ContainerNum2].IsEmpty);
		}

		public void TestEquipmentStatus()
		{
			IDictionary<string, IPortAuthorityEquipmentData> equipment;

			var vessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First();

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "123";

			JobSailing sailing = FindOrCreateSailing(voyage, "AUBNE", "SGSIN");

			AgencyShipment shipment = NewShipmentWithoutErrors(ExportSailing, false, true);
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NLAMS";

			AddContainer(shipment, ContainerNum1, RC_20GP_PK, false);
			AddContainer(shipment, ContainerNum2, RC_20GP_PK, false);

			Factory.Save();

			equipment = GetEquipmentHash(GetExportData());
			AssertEquals(PortAuthorityContainerStatus.Export, equipment[ContainerNum1].ContainerStatus);
			AssertEquals(PortAuthorityContainerStatus.Export, equipment[ContainerNum2].ContainerStatus);

			equipment = GetEquipmentHash(GetImportData());
			AssertEquals(PortAuthorityContainerStatus.Transhipment, equipment[ContainerNum1].ContainerStatus);
			AssertEquals(PortAuthorityContainerStatus.Transhipment, equipment[ContainerNum2].ContainerStatus);

			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "SGPUB";

			Factory.Save();

			equipment = GetEquipmentHash(GetExportData());
			AssertEquals(PortAuthorityContainerStatus.Transhipment, equipment[ContainerNum1].ContainerStatus);
			AssertEquals(PortAuthorityContainerStatus.Transhipment, equipment[ContainerNum2].ContainerStatus);

			equipment = GetEquipmentHash(GetImportData());
			AssertEquals(PortAuthorityContainerStatus.Import, equipment[ContainerNum1].ContainerStatus);
			AssertEquals(PortAuthorityContainerStatus.Import, equipment[ContainerNum2].ContainerStatus);
		}

		#region Implementation

		IPortAuthorityMessagingData GetImportData()
		{
			return GetImportData(ExportSailing, false);
		}

		IPortAuthorityMessagingData GetImportData(bool for3rdParty)
		{
			return GetImportData(ExportSailing, for3rdParty);
		}

		IPortAuthorityMessagingData GetImportData(JobSailing sailing)
		{
			return GetImportData(sailing, false);
		}

		IPortAuthorityMessagingData GetImportData(JobSailing sailing, bool for3rdParty)
		{
			var message = new PortAuthority(sailing.Voyage);
			message.Port = sailing.Destination.JB_RL_NKPortOfDischarge;
			message.Direction = Constants.PortDirection.Discharge;
			message.DeliverTo3rdParty = for3rdParty;

			return VoyageMessagingData.New(message);
		}

		IPortAuthorityMessagingData GetExportData()
		{
			return GetExportData(ExportSailing, false);
		}

		IPortAuthorityMessagingData GetExportData(bool for3rdParty)
		{
			return GetExportData(ExportSailing, for3rdParty);
		}

		IPortAuthorityMessagingData GetExportData(JobSailing sailing)
		{
			return GetExportData(sailing, false);
		}

		IPortAuthorityMessagingData GetExportData(JobSailing sailing, bool for3rdParty)
		{
			var message = new PortAuthority(sailing.Voyage);
			message.Port = sailing.Origin.JA_RL_NKPortOfLoading;
			message.Direction = Constants.PortDirection.Load;
			message.DeliverTo3rdParty = for3rdParty;

			return VoyageMessagingData.New(message);
		}

		IDictionary<string, IPortAuthorityEquipmentData> GetEquipmentHash(IPortAuthorityMessagingData data)
		{
			IDictionary<string, IPortAuthorityEquipmentData> result = new Dictionary<string, IPortAuthorityEquipmentData>();

			foreach (IPortAuthorityEquipmentData equipment in data.Equipment)
			{
				if (result.ContainsKey(equipment.ContainerNumber))
				{
					// This *DOES* need to be unique.
					throw new ApplicationException("duplicate container found (container not merged correctly): " + equipment.ContainerNumber);
				}

				result.Add(equipment.ContainerNumber, equipment);
			}

			return result;
		}

		IDictionary<string, IPortAuthorityConsignmentData> GetConsignmentHash(IPortAuthorityMessagingData data)
		{
			IDictionary<string, IPortAuthorityConsignmentData> result = new Dictionary<string, IPortAuthorityConsignmentData>();

			foreach (IPortAuthorityConsignmentData consignment in data.Consignments)
			{
				if (result.ContainsKey(consignment.BillOfLading))
				{
					// only needs to be unique here for the benifit of testing. does not actualy need to be unique in real life.
					throw new ApplicationException("duplicate ocean bill of lading found (broken test data): " + consignment.BillOfLading);
				}

				result.Add(consignment.BillOfLading.ToLower(), consignment);
			}

			return result;
		}

		IDictionary<string, IPortAuthorityGoodsData> GetGoodsHash(IPortAuthorityMessagingData data)
		{
			IDictionary<string, IPortAuthorityGoodsData> result = new Dictionary<string, IPortAuthorityGoodsData>();

			foreach (IPortAuthorityConsignmentData consignment in data.Consignments)
			{
				foreach (IPortAuthorityGoodsData goods in consignment.Goods)
				{
					string key = consignment.BillOfLading.ToLower() + "-" + goods.GoodsDescription;

					if (result.ContainsKey(key))
					{
						// only needs to be unique here for the benifit of testing. does not actualy need to be unique in real life.
						throw new ApplicationException("duplicate key found (broken test data): " + consignment.BillOfLading);
					}

					result.Add(key, goods);
				}
			}

			return result;
		}

		void SetupAddress(JobDocAddress address, bool overridden, char c)
		{
			if (overridden)
			{
				address.E2_AddressOverride = true;
				address.E2_CompanyName = "Company " + c;
				address.E2_Address1 = "Address1 " + c;
				address.E2_Address2 = "Address2 " + c;
				address.E2_City = "City " + c;
				address.E2_State = "NSW";
				address.E2_Postcode = "PC " + c;
				address.E2_RN_NKCountryCode = "AU";
			}
			else
			{
				OrgAddress oAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
				oAddress.Header.OH_FullName = "Company " + c;
				oAddress.OA_Address1 = "Address1 " + c;
				oAddress.OA_Address2 = "Address2 " + c;
				oAddress.OA_City = "City " + c;
				oAddress.OA_State = "NSW";
				oAddress.OA_PostCode = "PC " + c;
				oAddress.OA_RN_NKCountryCode = "AU";

				address.E2_AddressOverride = false;
				address.E2_OA_Address = oAddress.PK;
			}
		}

		AgencyShipmentContainer AddContainer(AgencyShipment shipment, ZString containerNum, ZGuid containerType)
		{
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = containerNum;
			container.JC_RC = containerType;

			if (shipment.IsTopLevelPacksMode)
			{
				container.JC_ContainerMode = Core.Constants.ContainerModes.RollOnRollOff;
			}

			return container;
		}
		AgencyShipmentContainer AddContainer(AgencyShipment shipment, ZString containerNum, ZGuid containerType, bool pack)
		{
			AgencyShipmentContainer container = AddContainer(shipment, containerNum, containerType);

			if (pack)
			{
				AgencyShipmentPackLine packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_JC = container.PK;
				packLine.JL_DetailedDescription = "Goods Description";
			}
			else
			{
				container.JC_IsEmptyContainer = true;
			}

			return container;
		}

		AgencyShipmentPackLine AddPackline(AgencyShipment shipment, ZString description, ZString marks)
		{
			AgencyShipmentPackLine packline = shipment.OuterPackLines.AddNew();
			packline.JL_DetailedDescription = description;
			packline.JL_MarksAndNumbers = marks;
			return packline;
		}

		AgencyShipmentContainer AddTopLevelPack(AgencyShipment shipment, ZString description, ZString marks)
		{
			var topLevelPack = shipment.TopLevelPacks.AddNew();
			topLevelPack.JC_Description = description;
			topLevelPack.JC_MarksAndNumbers = marks;

			return topLevelPack;
		}

		AgencyShipment NewShipmentWithoutErrors(JobSailing sailing, ZBool isCanceled, ZBool isConfirmed)
		{
			return NewShipmentWithoutErrors(Factory, sailing, isCanceled, isConfirmed);
		}
		AgencyShipment NewShipmentWithoutErrors(BusinessObjectFactory factory, JobSailing sailing, ZBool isCanceled, ZBool isConfirmed)
		{
			sailing.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Ignore;

			OrgAddress ctoAddress;
			if ((ctoAddress = sailing.Destination.ArrivalCTOAddress) == null)
			{
				ctoAddress = factory.NewWithValidTestData<OrgAddress>();
				ctoAddress.Header.OH_Code = "SNTH CTO " + (orgNum++);
				sailing.Destination.JB_OA_ArrivalCTOAddress = ctoAddress.PK;
			}
			SetAcosCodeIfNotSet(ctoAddress.Header, "cto");

			OrgHeader shippingLine;
			if ((shippingLine = sailing.Line) == null)
			{
				shippingLine = factory.NewWithValidTestData<OrgHeader>();
				shippingLine.OH_IsShippingLine = true;
				shippingLine.OH_IsShippingProvider = true;
				shippingLine.OH_Code = "SNTH SL " + (orgNum++);
				sailing.Voyage.JV_OH_Line = shippingLine.PK;
			}
			SetAcosCodeIfNotSet(shippingLine, "line");

			AgencyShipment shipment = NewShipment(factory, sailing, Principal, isCanceled, isConfirmed);
			shipment.JS_HouseBill = "Generic OBL";
			shipment.JS_GoodsDescription = "Shipment description";
			shipment.JS_OA_BookedShippingLineAddress = sailing.Voyage.Line.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = Consignor.MainAddress.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = Consignee.MainAddress.PK;
			if (!isConfirmed)
			{
				shipment.BookingPartyDocumentaryAddress.E2_OA_Address = Consignor.MainAddress.PK;
			}
			shipment.OuterPackLines.RemoveAndDeleteAll();

			SetAcosCodeIfNotSet(shipment.Principal, "principal");

			shipment.RunPreSaveValidation();
			AssertNoErrors("precondition:", shipment);
			AssertNoMessageErrors("precondition:", shipment);
			return shipment;
		}
		int orgNum;

		OrgHeader Principal
		{
			get
			{
				if (principal == null)
				{
					SetupOrgs();
				}
				return principal;
			}
		}

		OrgHeader Consignor
		{
			get
			{
				if (consignor == null)
				{
					SetupOrgs();
				}
				return consignor;
			}
		}

		OrgHeader Consignee
		{
			get
			{
				if (consignee == null)
				{
					SetupOrgs();
				}
				return consignee;
			}
		}

		void SetupOrgs()
		{
			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			OrgHeader principalInOtherFactory = GetCurrentBranchOrgProxy();
			OrgHeader consignorInOtherFactory = NewConsignor(otherFactory);
			OrgHeader consigneeInOtherFactory = NewConsignee(otherFactory);

			otherFactory.Save();
			principalInOtherFactory.Factory.Save();

			principal = Factory.Load<OrgHeader>(principalInOtherFactory.PK);
			consignor = Factory.Load<OrgHeader>(consignorInOtherFactory.PK);
			consignee = Factory.Load<OrgHeader>(consigneeInOtherFactory.PK);
		}

		OrgHeader principal;
		OrgHeader consignor;
		OrgHeader consignee;

		const string ContainerNum1 = "FAKE4100011";
		const string ContainerNum2 = "FAKE4100027";
		const string ContainerNum3 = "FAKE4100032";
		const string ContainerNum4 = "FAKE4100046";
		#endregion
	}
}
