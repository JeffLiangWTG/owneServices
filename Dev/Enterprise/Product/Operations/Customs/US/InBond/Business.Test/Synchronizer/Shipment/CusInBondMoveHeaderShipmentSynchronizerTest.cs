using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondMoveHeaderShipmentSynchronizerTest : SynchroniserTestCase
	{
		public void TestSynchronizePortsFor61()
		{
			consol.Transports.RemoveAndDeleteAll();
			var transport = consol.Transports[0];
			transport.JW_TransportMode = "SEA";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USPHL";
			transport = consol.Transports.AddNew();
			transport.JW_TransportMode = "ROA";
			transport.JW_RL_NKLoadPort = "USPHL";
			transport.JW_RL_NKDiscPort = "USCHI";
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_E_DEP = new ZDateTime(2011, 04, 03);
			shipment.JS_E_ARV = new ZDateTime(2011, 04, 15);
			Factory.Save();
			var inBondHeader = Factory.New<CusInBondHeader>();
			inBondHeader.BH_ParentID = shipment.PK;
			inBondHeader.BH_ParentTableCode = shipment.TablePrefix;
			inBondHeader.Synchroniser.Synchronise(true);
			AssertEquals(1, inBondHeader.MovementHeaders.Count);
			var moveHeader = inBondHeader.MovementHeaders[0];
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertEquals("US Destination", "3901", moveHeader.BM_DestinationPortCode);
			AssertEquals("Foreing Dest", "", moveHeader.BM_ForeignDestPortKCode);
			shipment.JS_RL_NKDestination = "USLAX";
			transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = "AIR";
			transport.JW_RL_NKLoadPort = "USCHI";
			transport.JW_RL_NKDiscPort = "USLAX";
			AssertEquals("US Destination - multiple AIR mapping", ZString.Empty, moveHeader.BM_DestinationPortCode);
			AssertEquals("Foreing Dest", "", moveHeader.BM_ForeignDestPortKCode);
		}

		public void TestSynchronizePortsFor62()
		{
			consol.Transports.RemoveAndDeleteAll();
			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = "SEA";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USPHL";
			transport = consol.Transports.AddNew();
			transport.JW_TransportMode = "ROA";
			transport.JW_RL_NKLoadPort = "USPHL";
			transport.JW_RL_NKDiscPort = "USCHI";
			transport = consol.Transports.AddNew();
			transport.JW_TransportMode = "AIR";
			transport.JW_RL_NKLoadPort = "USCHI";
			transport.JW_RL_NKDiscPort = "GBTIL";
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_E_DEP = new ZDateTime(2011, 04, 03);
			shipment.JS_E_ARV = new ZDateTime(2011, 04, 15);
			Factory.Save();
			var inBondHeader = Factory.New<CusInBondHeader>();
			inBondHeader.BH_ParentID = shipment.PK;
			inBondHeader.BH_ParentTableCode = shipment.TablePrefix;
			inBondHeader.Synchroniser.Synchronise(true);
			AssertEquals(1, inBondHeader.MovementHeaders.Count);
			var moveHeader = inBondHeader.MovementHeaders[0];
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			AssertEquals("US Destination", "3901", moveHeader.BM_DestinationPortCode);
			AssertEquals("Foreing Dest", "41380", moveHeader.BM_ForeignDestPortKCode);
		}

		public void TestSynchronizePortsFor63()
		{
			consol.Transports.RemoveAndDeleteAll();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_E_DEP = new ZDateTime(2011, 04, 03);
			shipment.JS_E_ARV = new ZDateTime(2011, 04, 15);
			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = "SEA";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USPHL";
			transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = "SEA";
			transport.JW_RL_NKLoadPort = "USPHL";
			transport.JW_RL_NKDiscPort = "GBTIL";
			Factory.Save();
			var inBondHeader = Factory.New<CusInBondHeader>();
			inBondHeader.BH_ParentID = shipment.PK;
			inBondHeader.BH_ParentTableCode = shipment.TablePrefix;
			inBondHeader.Synchroniser.Synchronise(true);
			AssertEquals(1, inBondHeader.MovementHeaders.Count);
			var moveHeader = inBondHeader.MovementHeaders[0];
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			AssertEquals("US Destination", ZString.Empty, moveHeader.BM_DestinationPortCode);
			AssertEquals("Foreing Dest", "41380", moveHeader.BM_ForeignDestPortKCode);
			transport.JW_RL_NKDiscPort = "USLAX";
			transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = "SEA";
			transport.JW_RL_NKLoadPort = "USLAX";
			var port = Factory.NewWithValidTestData<RefUNLOCO>();
			port.RL_Code = "ITAA~";
			var map = Factory.NewWithValidTestData<RefLocoMap>();
			map.RY_SystemUsage = "SCK";
			map.RY_LocalPortCode = "89991";
			map.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			map.RY_RL_NKLocoPort = port.RL_Code;
			transport.JW_RL_NKDiscPort = port.RL_Code;
			AssertEquals("US Destination", ZString.Empty, moveHeader.BM_DestinationPortCode);
			AssertEquals("Foreing Dest", "89991", moveHeader.BM_ForeignDestPortKCode);
		}

		public void TestBM_DestinationPortcodeInShipmentSynchronizer()
		{
			var port = Factory.NewWithValidTestData<RefUNLOCO>();
			port.RL_Code = "UST~";
			var map = Factory.NewWithValidTestData<RefLocoMap>();
			map.RY_SystemUsage = "SEA";
			map.RY_LocalPortCode = "88991";
			map.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			map.RY_RL_NKLocoPort = port.RL_Code;
			consol.Transports.RemoveAndDeleteAll();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_E_DEP = new ZDateTime(2021, 06, 03);
			shipment.JS_E_ARV = new ZDateTime(2021, 06, 15);
			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = "SEA";
			transport.JW_RL_NKLoadPort = "USPHL";
			transport.JW_RL_NKDiscPort = "USLAX";
			var transport2 = shipment.Transports.AddNew();
			transport2.JW_TransportMode = "SEA";
			transport2.JW_RL_NKLoadPort = "USLAX";
			transport2.JW_RL_NKDiscPort = port.RL_Code;
			var transport3 = shipment.Transports.AddNew();
			transport3.JW_TransportMode = "SEA";
			transport3.JW_RL_NKLoadPort = "AUSYD";
			transport3.JW_RL_NKDiscPort = "USPHL";
			Factory.Save();
			var inBondHeader = Factory.New<CusInBondHeader>();
			inBondHeader.BH_ParentID = shipment.PK;
			inBondHeader.BH_ParentTableCode = shipment.TablePrefix;
			inBondHeader.Synchroniser.Synchronise(true);
			AssertEquals(1, inBondHeader.MovementHeaders.Count);
			var moveHeader = inBondHeader.MovementHeaders[0];
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertEquals("US Destination", "2704", moveHeader.BM_DestinationPortCode);
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			AssertEquals("US Destination", "2704", moveHeader.BM_DestinationPortCode);
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			AssertEquals("US Destination", ZString.Empty, moveHeader.BM_DestinationPortCode);
			transport.JW_ETD = new ZDateTime(2022, 07, 01);
			transport.JW_ETA = new ZDateTime(2022, 07, 02);
			transport2.JW_ETD = new ZDateTime(2022, 07, 05);
			transport2.JW_ETA = new ZDateTime(2022, 07, 06);
			transport3.JW_ETD = new ZDateTime(2022, 07, 03);
			transport3.JW_ETA = new ZDateTime(2022, 07, 04);
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertEquals("US Destination", "8899", moveHeader.BM_DestinationPortCode);
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			AssertEquals("US Destination", "8899", moveHeader.BM_DestinationPortCode);
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			AssertEquals("US Destination", ZString.Empty, moveHeader.BM_DestinationPortCode);
		}

		public void TestSynchronizeBM_MonetaryValue()
		{
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_E_DEP = new ZDateTime(2011, 04, 03);
			shipment.JS_E_ARV = new ZDateTime(2011, 04, 15);
			Factory.Save();
			var inBondHeader = Factory.New<CusInBondHeader>();
			inBondHeader.BH_ParentID = shipment.PK;
			inBondHeader.BH_ParentTableCode = shipment.TablePrefix;
			inBondHeader.Synchroniser.Synchronise(true);
			AssertEquals(1, inBondHeader.MovementHeaders.Count);
			var moveHeader = inBondHeader.MovementHeaders[0];
			AssertEquals("BM_MonetaryValue", 0m, moveHeader.BM_MonetaryValue);
			shipment.JS_GoodsValue = 46.25m;
			inBondHeader.Synchroniser.Synchronise(true);
			AssertEquals("BM_MonetaryValue", 46.25m, moveHeader.BM_MonetaryValue);
		}

		public void TestSynchronizeAfterMovementHeaderDeleted()
		{
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_E_DEP = new ZDateTime(2011, 04, 03);
			shipment.JS_E_ARV = new ZDateTime(2011, 04, 15);
			Factory.Save();
			var inBondHeader = Factory.New<CusInBondHeader>();
			inBondHeader.BH_ParentID = shipment.PK;
			inBondHeader.BH_ParentTableCode = shipment.TablePrefix;
			inBondHeader.Synchroniser.Synchronise(true);
			AssertEquals("MovementHeaders.Count", 1, inBondHeader.MovementHeaders.Count);
			var moveHeader = inBondHeader.MovementHeaders[0];
			moveHeader.Delete();
			inBondHeader.Synchroniser.Synchronise(true);
			AssertEquals("MovementHeaders.Count", 0, inBondHeader.MovementHeaders.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USCHI";
			shipment = consol.Shipments.AddNew();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMP001";
			importer.OH_FullName = "Importer One Co. Ltd";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = importer.PK;
		}

		ForwardingConsol consol;
		ForwardingShipment shipment;
	}
}
