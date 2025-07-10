using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	public abstract class FreightTestHelper
	{
		public static ZString CreateCountry(int number)
		{
			RefCountry country = new BusinessObjectFactory().New(typeof(RefCountry)) as RefCountry;
			country.RN_Desc = "D" + number;
			country.RN_Code = "C" + number;
			country.Factory.Save();

			return country.Code;
		}

		public static void CreateUNLocoInDB(string portCode, ZString countryCode, int number)
		{
			RefUNLOCO loco = new BusinessObjectFactory().New(typeof(RefUNLOCO)) as RefUNLOCO;
			loco.RL_PortName = "P" + number;
			loco.RL_Code = portCode;
			loco.RL_RN_NKCountryCode = countryCode;

			loco.Factory.Save();
		}

		public struct FreightImportPKs
		{
			public ZGuid OrgHeader;
			public ZGuid AirImpCartage;
			public ZGuid FCLImpCartage;
			public ZGuid LCLImpCartage;
			public ZGuid AirImportCustomsBroker;
			public ZGuid SeaImportCustomsBroker;
		}

		public static FreightImportPKs CreateImportOrgMiscServInDB()
		{
			FreightImportPKs testPKs = new FreightImportPKs();
			testPKs.OrgHeader = CreateOrgHeaderInDB(1);
			testPKs.AirImpCartage = CreateOrgHeaderInDB(2);
			testPKs.FCLImpCartage = CreateOrgHeaderInDB(3);
			testPKs.LCLImpCartage = CreateOrgHeaderInDB(4);
			testPKs.AirImportCustomsBroker = CreateOrgHeaderInDB(5);
			testPKs.SeaImportCustomsBroker = CreateOrgHeaderInDB(6);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			var mainHeader = factory.Load<OrgHeader>(testPKs.OrgHeader);

			mainHeader.SetRelatedParty(testPKs.AirImportCustomsBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);
			mainHeader.SetRelatedParty(testPKs.SeaImportCustomsBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty);
			mainHeader.SetRelatedParty(testPKs.AirImpCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);
			mainHeader.SetRelatedParty(testPKs.FCLImpCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			mainHeader.SetRelatedParty(testPKs.LCLImpCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);

			factory.Save();

			return testPKs;
		}

		public struct FreightExportPKs
		{
			public ZGuid OrgHeader;
			public ZGuid AirExpCartage;
			public ZGuid FCLExpCartage;
			public ZGuid LCLExpCartage;
			public ZGuid AirExportCustomsBroker;
			public ZGuid SeaExportCustomsBroker;
			public ZGuid Currency;
		}

		public static FreightExportPKs CreateExportOrgMiscServInDB()
		{
			FreightExportPKs testPKs = new FreightExportPKs();
			testPKs.OrgHeader = CreateOrgHeaderInDB(1);
			testPKs.AirExpCartage = CreateOrgHeaderInDB(2);
			testPKs.FCLExpCartage = CreateOrgHeaderInDB(3);
			testPKs.LCLExpCartage = CreateOrgHeaderInDB(4);
			testPKs.AirExportCustomsBroker = CreateOrgHeaderInDB(5);
			testPKs.SeaExportCustomsBroker = CreateOrgHeaderInDB(6);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			RefCurrency currency = factory.New(typeof(RefCurrency)) as RefCurrency;
			currency.FillWithValidTestData();

			testPKs.Currency = currency.PK;

			var mainHeader = factory.Load<OrgHeader>(testPKs.OrgHeader);
			mainHeader.SetRelatedParty(testPKs.AirExportCustomsBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty);
			mainHeader.SetRelatedParty(testPKs.SeaExportCustomsBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty);
			mainHeader.SetRelatedParty(testPKs.AirExpCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty);
			mainHeader.SetRelatedParty(testPKs.FCLExpCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			mainHeader.SetRelatedParty(testPKs.LCLExpCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);

			mainHeader.MiscServ.OM_RX_NKEXDefCurrency = factory.Load<RefCurrency>(testPKs.Currency).RX_Code;
			factory.Save();

			return testPKs;
		}

		public static ZGuid CreateOrgHeaderInDB(int number, string location)
		{
			OrgHeader header = new BusinessObjectFactory().New(typeof(OrgHeader)) as OrgHeader;
			header.OH_FullName = "F" + number;
			header.OH_RL_NKClosestPort = location;
			header.MainAddress.FillWithValidTestData();
			header.OH_Code = "C" + number;
			header.Factory.Save();

			return header.PK;
		}

		protected static ZGuid CreateOrgHeaderInDB(int number)
		{
			return CreateOrgHeaderInDB(number, ZString.Empty);
		}

		public static JobDocAddress CreateJobDocAddress(IDocAddresses parent, DocAddressType addressType, ZString orgName, ZString address1, ZString postCode, ZString city, ZString homePort, bool overrideAddress, BusinessObjectFactory factory)
		{
			JobDocAddress docAddress = parent.DocAddresses.FindOrCreateWithDocAddressType(addressType);

			if (overrideAddress)
			{
				docAddress.E2_AddressOverride = true;
				docAddress.E2_CompanyName = orgName;
				docAddress.E2_Address1 = address1;
				docAddress.E2_Postcode = postCode;
				docAddress.E2_City = city;
			}
			else
			{
				OrgHeader org = factory.New<OrgHeader>();
				org.OH_FullName = orgName;
				org.OH_RL_NKClosestPort = homePort;
				org.MainAddress.OA_Address1 = address1;
				org.MainAddress.OA_PostCode = postCode;
				org.MainAddress.OA_City = city;
				docAddress.E2_OA_Address = org.MainAddress.PK;
			}

			return docAddress;
		}

		public static T GetShipment<T>(ZString shipmentNo, ZString shipmentType, BusinessObjectFactory factory)
			where T : CommonShipment
		{
			return GetShipment<T>(shipmentNo, null, shipmentType, factory);
		}

		public static T GetShipment<T>(ZString shipmentNo, T master, ZString shipmentType, BusinessObjectFactory factory)
			where T : CommonShipment
		{
			var shipment = factory.New<T>();
			shipment.JS_UniqueConsignRef = shipmentNo;
			shipment.JS_ShipmentType = shipmentType;

			if (master != null)
			{
				shipment.JS_JS_ColoadMasterShipment = master.PK;
			}

			return shipment;
		}

		public static T GetConsol<T>(ZString consolNo, BusinessObjectFactory factory)
			where T : CommonConsol
		{
			var consol = factory.New<T>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_UniqueConsignRef = consolNo;

			return consol;
		}

		public static T GetConsol<T>(string consolNumber, string transportMode, string agentType, string portOfLoading, string portOfDischarge, ZDateTime etd, ZDateTime eta, BusinessObjectFactory factory, CommonShipment shipment = null)
			where T : CommonConsol
		{
			var consol = GetConsol<T>(consolNumber, factory);
			consol.JK_RL_NKLoadPort = portOfLoading;
			consol.JK_RL_NKDischargePort = portOfDischarge;
			consol.JK_TransportMode = transportMode;
			consol.JK_AgentType = agentType;
			consol.Transports[0].JW_ETD = etd;
			consol.Transports[0].JW_ETA = eta;

			if (shipment != null)
			{
				shipment.Consols.Add(consol);
			}

			return consol;
		}

		public static void AssertShipmentCollection(IEnumerable<BusinessObject> shipmentCollection, params CommonShipment[] expectedShipments)
		{
			AssertShipmentCollection(ZString.Empty, shipmentCollection, expectedShipments);
		}

		public static void AssertShipmentCollection(ZString message, IEnumerable<BusinessObject> shipmentCollection, params CommonShipment[] expectedShipments)
		{
			if (message.IsEmpty)
			{
				Assertion.AssertContainsExactElementsInAnyOrder(s => s.JS_UniqueConsignRef, expectedShipments, shipmentCollection.Cast<CommonShipment>());
			}
			else
			{
				Assertion.AssertContainsExactElementsInAnyOrder(message, s => s.JS_UniqueConsignRef, expectedShipments, shipmentCollection.Cast<CommonShipment>());
			}
		}

		public static void AssertConsolCollection(IEnumerable<BusinessObject> consolCollection, params CommonConsol[] expectedConsols)
		{
			AssertConsolCollection(ZString.Empty, consolCollection, expectedConsols);
		}

		public static void AssertConsolCollection(ZString message, IEnumerable<BusinessObject> consolCollection, params CommonConsol[] expectedConsols)
		{
			if (message.IsEmpty)
			{
				Assertion.AssertContainsExactElementsInAnyOrder(c => c.JK_UniqueConsignRef, expectedConsols, consolCollection.Cast<CommonConsol>());
			}
			else
			{
				Assertion.AssertContainsExactElementsInAnyOrder(message, c => c.JK_UniqueConsignRef, expectedConsols, consolCollection.Cast<CommonConsol>());
			}
		}

		public static ZQuery ConsigneeOrConsignorFilter(SchemaColumn columnToBeTrue, params ZGuid[] excludeThese)
		{
			var filter = new ZQuery(columnToBeTrue, true);

			if (excludeThese.Length > 1)
			{
				filter.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, excludeThese);
			}
			return filter;
		}

		/// <summary>
		/// as the test database would not rollback sequence that is created in the test, we should try to drop packline id sequence while the unit test runs setup or teardown.
		/// otherwise the sequence would be used in other unit test.
		/// </summary>
		public static void TryRemovePackLineIdSequence() => TryRemoveSequences("PackLine-1f19957f-fa8e-4ce0-a687-3b2b9d858f6d");

		public static void TryRemoveJobVoyageAndRelatedSequences() => TryRemoveSequences(
			"JobVoyage-f3f5050e-5781-4579-a871-b4d1ca0296c0",
			"JobVoyOrigin-02f4e24f-0bb6-423e-ad56-1bd4331c2de1",
			"JobVoyDestination-d8dc5422-2e82-4503-834f-9490647ecd5d",
			"JobSailing-7ba0187b-a3b1-45fd-9ebd-5e55369202e2");

		public static void AssertSingleDeveloperException(string reason, string expectedKey)
		{
			Assertion.AssertEquals("DeveloperException should have been reported, " + reason, expectedKey, ErrorReporter.LastKeyReported);
			Assertion.AssertEquals("Total Error Count should be 1", 1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		static void TryRemoveSequences(params string[] sequenceIDs)
		{
			if (sequenceIDs == null
				|| sequenceIDs.Length == 0)
			{
				return;
			}

			using (var adminConnection = Db.NewAdminConnection())
			{
				foreach (var sequenceID in sequenceIDs)
				{
					if (string.IsNullOrEmpty(sequenceID))
					{
						continue;
					}

					var sqlText = $@"
IF (OBJECT_ID('{sequenceID}', N'SO') IS NOT NULL)
BEGIN
	DROP SEQUENCE [{sequenceID}]
END";

					adminConnection.ExecuteNonQuery(sqlText);
				}
			}
		}
	}
}
