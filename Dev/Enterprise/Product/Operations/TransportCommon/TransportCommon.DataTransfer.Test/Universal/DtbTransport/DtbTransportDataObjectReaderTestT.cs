using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.TransportCommon.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Constants = Enterprise.Core.Constants;
using ICusEntryNumAdditionalReferenceCollection = Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportCommon.DataTransfer.Universal.Testing
{
	public abstract class DtbTransportDataObjectReaderTest<T> : OrganizationAddressTestHelper
			where T : DtbTransport
	{
		#region TestAdditionalReferences

		public void TestAdditionalReferences()
		{
			var year = ZDateTime.Now.Year;

			var transportDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var additionalReferenceDataObject = new AdditionalReference();

			additionalReferenceDataObject.IssueDate = new ZDateTime(year, 1, 1);
			additionalReferenceDataObject.ReferenceNumber = "R1234";
			additionalReferenceDataObject.Type = new EntryType { Code = "TRF", Description = "Transport Reference Number" };

			transportDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			transportDataObject.AdditionalReferenceCollection.Add(additionalReferenceDataObject);

			var reader = GetNewReader(transportDataObject, Logger, Factory, null, transportDataObject);
			var transport = reader.ReadIntoBusinessObject();
			AssertEquals(1, transport.AdditionalReferenceNumbers.Count);

			var additionalReference = transport.AdditionalReferenceNumbers[0];
			AssertEquals("additionalReference.CE_EntryNum", "R1234", additionalReference.CE_EntryNum);
			AssertEquals("additionalReference.CE_EntryType", "TRF", additionalReference.CE_EntryType);
			AssertEquals("additionalReference.CE_IssueDate", new ZDateTime(year, 1, 1), additionalReference.CE_IssueDate);
		}

		#endregion

		#region TestAdditionalReferences_WithHouseBill

		public void TestAdditionalReferences_WithHouseBill()
		{
			var transportDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportDataObject.WayBillNumber = "12345";
			transportDataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, new WayBillTypeList());

			var reader = GetNewReader(transportDataObject, Logger, Factory, null, transportDataObject);
			var transport = reader.ReadIntoBusinessObject();
			var additionalReferences = GetAdditionalReferenceNumbersForWayBills(transport);
			AssertEquals(1, additionalReferences.Count);

			var additionalReference = additionalReferences[0];
			AssertEquals("additionalReference.CE_EntryNum", "12345", additionalReference.CE_EntryNum);
			AssertEquals("additionalReference.CE_EntryType", "HSB", additionalReference.CE_EntryType);
		}

		#endregion

		#region TestAdditionalReferences_WithMasterBill

		public void TestAdditionalReferences_WithMasterBill()
		{
			var transportDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportDataObject.WayBillNumber = "12345";
			transportDataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, new WayBillTypeList());

			var reader = GetNewReader(transportDataObject, Logger, Factory, null, transportDataObject);
			var transport = reader.ReadIntoBusinessObject();
			var additionalReferences = GetAdditionalReferenceNumbersForWayBills(transport);
			AssertEquals(1, additionalReferences.Count);

			var additionalReference = additionalReferences[0];
			AssertEquals("additionalReference.CE_EntryNum", "12345", additionalReference.CE_EntryNum);
			AssertEquals("additionalReference.CE_EntryType", "MAB", additionalReference.CE_EntryType);
		}

		#endregion

		#region TestAdditionalReferences_WithMasterBill_OnTopLevelDO

		public void TestAdditionalReferences_WithMasterBill_OnTopLevelDO()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			topLevelDataObject.WayBillNumber = "12345";
			topLevelDataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, new WayBillTypeList());

			var reader = GetNewReader(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, Factory, null, topLevelDataObject);
			var transport = reader.ReadIntoBusinessObject();
			var additionalReferences = GetAdditionalReferenceNumbersForWayBills(transport);
			AssertEquals(1, additionalReferences.Count);

			var additionalReference = additionalReferences[0];
			AssertEquals("additionalReference.CE_EntryNum", "12345", additionalReference.CE_EntryNum);
			AssertEquals("additionalReference.CE_EntryType", "MAB", additionalReference.CE_EntryType);
		}

		#endregion

		#region TestAdditionalReferences_AddDashInMasterBillForAirModeAndRemoveDashInNonAirMode

		public void TestAdditionalReferences_AddDashInMasterBillForAirModeAndRemoveDashInNonAirMode()
		{
			var transportDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportDataObject.TransportMode = new CodeDescriptionPair();
			transportDataObject.TransportMode.Code = Core.Constants.TransportModes.Air;
			transportDataObject.WayBillNumber = "777-88888";
			transportDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

			var masterBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
			var mab1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "123456", BillType = masterBillType };
			var mab2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456-789", BillType = masterBillType };
			var mab3 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "123-456 ", BillType = masterBillType };

			transportDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab1, mab2, mab3 });
			var reader = GetNewReader(transportDataObject, Logger, Factory, null, transportDataObject);
			var transport = reader.ReadIntoBusinessObject();
			var additionalReferencesForAirTransportMode = GetAdditionalReferenceNumbersForWayBills(transport);
			AssertEquals(3, additionalReferencesForAirTransportMode.Count);

			var houseBill = additionalReferencesForAirTransportMode[0];
			AssertEquals("777-88888", houseBill.CE_EntryNum);
			AssertEquals("HSB", houseBill.CE_EntryType);

			var masterBillReference1 = additionalReferencesForAirTransportMode[1];
			AssertEquals("123-456", masterBillReference1.CE_EntryNum);
			AssertEquals("MAB", masterBillReference1.CE_EntryType);

			var masterBillReference2 = additionalReferencesForAirTransportMode[2];
			AssertEquals("456-789", masterBillReference2.CE_EntryNum);
			AssertEquals("MAB", masterBillReference2.CE_EntryType);

			transportDataObject.TransportMode.Code = Core.Constants.TransportModes.Sea;
			transport = reader.ReadIntoBusinessObject();
			var additionalReferencesForSeaTransportMode = GetAdditionalReferenceNumbersForWayBills(transport);
			AssertEquals(3, additionalReferencesForSeaTransportMode.Count);

			houseBill = additionalReferencesForSeaTransportMode[0];
			AssertEquals("777-88888", houseBill.CE_EntryNum);
			AssertEquals("HSB", houseBill.CE_EntryType);

			masterBillReference1 = additionalReferencesForSeaTransportMode[1];
			AssertEquals("123456", masterBillReference1.CE_EntryNum);
			AssertEquals("MAB", masterBillReference1.CE_EntryType);

			masterBillReference2 = additionalReferencesForSeaTransportMode[2];
			AssertEquals("456789", masterBillReference2.CE_EntryNum);
			AssertEquals("MAB", masterBillReference2.CE_EntryType);
		}

		#endregion

		#region TestAdditionalReferences_OverridesMasterBillValues_ReplacesCurrentWithImported

		public void TestAdditionalReferences_OverridesMasterBillValues_ReplacesCurrentWithImported()
		{
			var transportDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportDataObject.TransportMode = new CodeDescriptionPair();
			transportDataObject.TransportMode.Code = Core.Constants.TransportModes.Air;
			transportDataObject.WayBillNumber = "456-34232";
			transportDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

			var masterBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
			var mab1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456-789", BillType = masterBillType };
			var mab2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "123-456", BillType = masterBillType };
			var mab3 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "987-654", BillType = masterBillType };

			transportDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab1, mab2, mab3 });
			var reader1 = GetNewReader(transportDataObject, Logger, Factory, null, transportDataObject);
			var transport = reader1.ReadIntoBusinessObject();
			var additionalReferencesForObject1 = GetAdditionalReferenceNumbersForWayBills(transport);
			AssertEquals(4, additionalReferencesForObject1.Count);

			var houseBill1 = additionalReferencesForObject1[0];
			AssertEquals("456-34232", houseBill1.CE_EntryNum);
			AssertEquals("HSB", houseBill1.CE_EntryType);

			var masterBillReferenceOne1 = additionalReferencesForObject1[1];
			AssertEquals("456-789", masterBillReferenceOne1.CE_EntryNum);
			AssertEquals("MAB", masterBillReferenceOne1.CE_EntryType);

			var masterBillReferenceOne2 = additionalReferencesForObject1[2];
			AssertEquals("123-456", masterBillReferenceOne2.CE_EntryNum);
			AssertEquals("MAB", masterBillReferenceOne2.CE_EntryType);

			var masterBillReferenceOne3 = additionalReferencesForObject1[3];
			AssertEquals("987-654", masterBillReferenceOne3.CE_EntryNum);
			AssertEquals("MAB", masterBillReferenceOne3.CE_EntryType);

			var mab4 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "111-111", BillType = masterBillType };
			var mab5 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "222-222", BillType = masterBillType };
			var mab6 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "333-333", BillType = masterBillType };
			transportDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab4, mab5, mab6 });
			transportDataObject.WayBillNumber = "111-444888";

			var reader2 = GetNewReader(transportDataObject, Logger, Factory, transport, transportDataObject);
			var newTransport = reader2.ReadIntoBusinessObject();
			var additionalReferencesForObject2 = GetAdditionalReferenceNumbersForWayBills(newTransport);
			AssertEquals(4, additionalReferencesForObject2.Count);

			var houseBill2 = additionalReferencesForObject2[0];
			AssertEquals("111-444888", houseBill2.CE_EntryNum);
			AssertEquals("HSB", houseBill2.CE_EntryType);

			var masterBillReferenceTwo1 = additionalReferencesForObject2[1];
			AssertEquals("111-111", masterBillReferenceTwo1.CE_EntryNum);
			AssertEquals("MAB", masterBillReferenceTwo1.CE_EntryType);

			var masterBillReferenceTwo2 = additionalReferencesForObject2[2];
			AssertEquals("222-222", masterBillReferenceTwo2.CE_EntryNum);
			AssertEquals("MAB", masterBillReferenceTwo2.CE_EntryType);

			var masterBillReferenceTwo3 = additionalReferencesForObject2[3];
			AssertEquals("333-333", masterBillReferenceTwo3.CE_EntryNum);
			AssertEquals("MAB", masterBillReferenceTwo3.CE_EntryType);
		}

		public void TestAdditionalReferences_OverridesMasterBillValues_ReplacesCurrentWithImported_NewWayBill()
		{
			var transportDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportDataObject.TransportMode = new CodeDescriptionPair();
			transportDataObject.TransportMode.Code = Core.Constants.TransportModes.Air;
			transportDataObject.WayBillNumber = "456-34232";
			transportDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };

			var reader = GetNewReader(transportDataObject, Logger, Factory, null, transportDataObject);
			var transport = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject = GetAdditionalReferenceNumbersForWayBills(transport);
			AssertEquals(1, additionalReferencesForObject.Count);

			var masterBill1 = additionalReferencesForObject[0];
			AssertEquals("456-34232", masterBill1.CE_EntryNum);
			AssertEquals("MAB", masterBill1.CE_EntryType);

			transportDataObject.WayBillNumber = "1564-4812";
			var reader2 = GetNewReader(transportDataObject, Logger, Factory, transport, transportDataObject);
			var newTransport = reader2.ReadIntoBusinessObject();
			additionalReferencesForObject = GetAdditionalReferenceNumbersForWayBills(newTransport);
			AssertEquals(1, additionalReferencesForObject.Count);

			var masterBill2 = additionalReferencesForObject[0];
			AssertEquals("1564-4812", masterBill2.CE_EntryNum);
			AssertEquals("MAB", masterBill2.CE_EntryType);
		}

		public void TestAdditionalReferences_OverridesMasterBillValues_ReplacesCurrentWithImported_EmptyMasterBill()
		{
			var transportDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportDataObject.TransportMode = new CodeDescriptionPair();
			transportDataObject.TransportMode.Code = Constants.TransportModes.Air;
			transportDataObject.WayBillNumber = "456-34232";
			transportDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };

			var reader = GetNewReader(transportDataObject, Logger, Factory, null, transportDataObject);
			var transport = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject1 = GetAdditionalReferenceNumbersForWayBills(transport);
			AssertEquals(1, additionalReferencesForObject1.Count);

			var masterBill = additionalReferencesForObject1[0];
			AssertEquals("456-34232", masterBill.CE_EntryNum);
			AssertEquals("MAB", masterBill.CE_EntryType);

			transportDataObject.WayBillNumber = "";
			var reader2 = GetNewReader(transportDataObject, Logger, Factory, transport, transportDataObject);
			var newTransport = reader2.ReadIntoBusinessObject();
			var additionalReferencesForObject2 = GetAdditionalReferenceNumbersForWayBills(newTransport);
			AssertEquals(0, additionalReferencesForObject2.Count);
		}

		public void TestAdditionalReferences_OverridesMasterBillValues_ReplacesCurrentWithImported_EmptyHouseBill()
		{
			var transportDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportDataObject.TransportMode = new CodeDescriptionPair();
			transportDataObject.TransportMode.Code = Constants.TransportModes.Air;
			transportDataObject.WayBillNumber = "456-34232";
			transportDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

			var reader = GetNewReader(transportDataObject, Logger, Factory, null, transportDataObject);
			var transport = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject1 = GetAdditionalReferenceNumbersForWayBills(transport);
			AssertEquals(1, additionalReferencesForObject1.Count);

			var masterBill = additionalReferencesForObject1[0];
			AssertEquals("456-34232", masterBill.CE_EntryNum);
			AssertEquals("HSB", masterBill.CE_EntryType);

			transportDataObject.WayBillNumber = "";
			reader = GetNewReader(transportDataObject, Logger, Factory, transport, transportDataObject);
			var newTransport = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject2 = GetAdditionalReferenceNumbersForWayBills(newTransport);
			AssertEquals(0, additionalReferencesForObject2.Count);
		}

		public void TestAdditionalReferences_ImportsNoDuplicates_HouseBill()
		{
			var houseBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };
			var transportDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportDataObject.TransportMode = new CodeDescriptionPair();
			transportDataObject.TransportMode.Code = Constants.TransportModes.Air;
			transportDataObject.WayBillNumber = "456-342";
			transportDataObject.WayBillType = houseBillType;

			var hsb1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456-342", BillType = houseBillType };
			var hsb2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456-342", BillType = houseBillType };
			var hsb3 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "987-654", BillType = houseBillType };

			transportDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { hsb1, hsb2, hsb3 });
			var reader = GetNewReader(transportDataObject, Logger, Factory, null, transportDataObject);
			var transport = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject = GetAdditionalReferenceNumbersForWayBills(transport);
			AssertEquals(1, additionalReferencesForObject.Count);

			var masterBill = additionalReferencesForObject[0];
			AssertEquals("456-342", masterBill.CE_EntryNum);
			AssertEquals("HSB", masterBill.CE_EntryType);
		}

		public void TestAdditionalReferences_ImportsNoDuplicates_MasterBill()
		{
			var masterBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
			var transportDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportDataObject.TransportMode = new CodeDescriptionPair();
			transportDataObject.TransportMode.Code = Constants.TransportModes.Air;
			transportDataObject.WayBillNumber = "456-342";
			transportDataObject.WayBillType = masterBillType;

			var mab1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456-342", BillType = masterBillType };
			var mab2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456-342", BillType = masterBillType };
			var mab3 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "987-654", BillType = masterBillType };

			transportDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab1, mab2, mab3 });
			var reader = GetNewReader(transportDataObject, Logger, Factory, null, transportDataObject);
			var transport = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject = GetAdditionalReferenceNumbersForWayBills(transport);
			AssertEquals(2, additionalReferencesForObject.Count);

			var masterBill1 = additionalReferencesForObject[0];
			AssertEquals("456-342", masterBill1.CE_EntryNum);
			AssertEquals("MAB", masterBill1.CE_EntryType);

			var masterBill2 = additionalReferencesForObject[1];
			AssertEquals("987-654", masterBill2.CE_EntryNum);
			AssertEquals("MAB", masterBill2.CE_EntryType);
		}

		public void TestAdditionalReferences_OverridesMasterBillValues_ReplacesCurrentWithImported_NoWayBillType()
		{
			var transportDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportDataObject.TransportMode = new CodeDescriptionPair();
			transportDataObject.TransportMode.Code = Constants.TransportModes.Air;
			transportDataObject.WayBillNumber = "456-34232";
			transportDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };

			var reader = GetNewReader(transportDataObject, Logger, Factory, null, transportDataObject);
			var transport = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject1 = GetAdditionalReferenceNumbersForWayBills(transport);
			AssertEquals(1, additionalReferencesForObject1.Count);

			var masterBill = additionalReferencesForObject1[0];
			AssertEquals("456-34232", masterBill.CE_EntryNum);
			AssertEquals("MAB", masterBill.CE_EntryType);

			transportDataObject.WayBillType = null;
			var reader2 = GetNewReader(transportDataObject, Logger, Factory, transport, transportDataObject);
			var newTransport = reader2.ReadIntoBusinessObject();
			var additionalReferencesForObject2 = GetAdditionalReferenceNumbersForWayBills(newTransport);
			AssertEquals(1, additionalReferencesForObject2.Count);

			var masterBillTwo = additionalReferencesForObject2[0];
			AssertEquals("456-34232", masterBillTwo.CE_EntryNum);
			AssertEquals("MAB", masterBillTwo.CE_EntryType);
		}

		public void TestAdditionalReferences_OverridesMasterBillValues_ReplacesCurrentWithImported_NoWayBillNumber()
		{
			var transportDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportDataObject.TransportMode = new CodeDescriptionPair();
			transportDataObject.TransportMode.Code = Constants.TransportModes.Air;
			transportDataObject.WayBillNumber = "456-34232";
			transportDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };

			var reader = GetNewReader(transportDataObject, Logger, Factory, null, transportDataObject);
			var transport = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject1 = GetAdditionalReferenceNumbersForWayBills(transport);
			AssertEquals(1, additionalReferencesForObject1.Count);

			var masterBill = additionalReferencesForObject1[0];
			AssertEquals("456-34232", masterBill.CE_EntryNum);
			AssertEquals("MAB", masterBill.CE_EntryType);

			transportDataObject.WayBillNumber = null;
			var reader2 = GetNewReader(transportDataObject, Logger, Factory, transport, transportDataObject);
			var newTransport = reader2.ReadIntoBusinessObject();
			var additionalReferencesForObject2 = GetAdditionalReferenceNumbersForWayBills(newTransport);
			AssertEquals(1, additionalReferencesForObject2.Count);

			var masterBillTwo = additionalReferencesForObject2[0];
			AssertEquals("456-34232", masterBillTwo.CE_EntryNum);
			AssertEquals("MAB", masterBillTwo.CE_EntryType);
		}

		public void TestAdditionalReferences_OverridesMasterBillValues_NoRefNumber()
		{
			var masterBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
			var transportDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportDataObject.TransportMode = new CodeDescriptionPair();
			transportDataObject.TransportMode.Code = Constants.TransportModes.Air;

			var mab1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456-342", BillType = masterBillType };
			transportDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab1 });
			var reader = GetNewReader(transportDataObject, Logger, Factory, null, transportDataObject);
			var transport = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject1 = GetAdditionalReferenceNumbersForWayBills(transport);
			AssertEquals(1, additionalReferencesForObject1.Count);

			var masterBill = additionalReferencesForObject1[0];
			AssertEquals("456-342", masterBill.CE_EntryNum);
			AssertEquals("MAB", masterBill.CE_EntryType);

			var mab2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = null, BillType = masterBillType };
			transportDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab2 });
			var reader2 = GetNewReader(transportDataObject, Logger, Factory, transport, transportDataObject);
			var newTransport = reader2.ReadIntoBusinessObject();
			var additionalReferencesForObject2 = GetAdditionalReferenceNumbersForWayBills(newTransport);
			AssertEquals(1, additionalReferencesForObject2.Count);

			var masterBillTwo = additionalReferencesForObject2[0];
			AssertEquals("456-342", masterBillTwo.CE_EntryNum);
			AssertEquals("MAB", masterBillTwo.CE_EntryType);
		}

		public void TestAdditionalReferences_OverridesMasterBillValues_EmptyRefNumber()
		{
			var masterBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
			var transportDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportDataObject.TransportMode = new CodeDescriptionPair();
			transportDataObject.TransportMode.Code = Constants.TransportModes.Air;

			var mab1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456-342", BillType = masterBillType };
			transportDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab1 });
			var reader = GetNewReader(transportDataObject, Logger, Factory, null, transportDataObject);
			var transport = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject1 = GetAdditionalReferenceNumbersForWayBills(transport);
			AssertEquals(1, additionalReferencesForObject1.Count);

			var masterBill = additionalReferencesForObject1[0];
			AssertEquals("456-342", masterBill.CE_EntryNum);
			AssertEquals("MAB", masterBill.CE_EntryType);

			var mab2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "", BillType = masterBillType };
			transportDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab2 });
			var reader2 = GetNewReader(transportDataObject, Logger, Factory, transport, transportDataObject);
			var newTransport = reader2.ReadIntoBusinessObject();
			var additionalReferencesForObject2 = GetAdditionalReferenceNumbersForWayBills(newTransport);
			AssertEquals(1, additionalReferencesForObject2.Count);

			var masterBillTwo = additionalReferencesForObject2[0];
			AssertEquals("456-342", masterBillTwo.CE_EntryNum);
			AssertEquals("MAB", masterBillTwo.CE_EntryType);
		}

		public void TestAdditionalReferences_OverridesMasterBillValues_NoRefBillType()
		{
			var masterBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
			var transportDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportDataObject.TransportMode = new CodeDescriptionPair();
			transportDataObject.TransportMode.Code = Constants.TransportModes.Air;

			var mab1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456-342", BillType = masterBillType };
			transportDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab1 });
			var reader = GetNewReader(transportDataObject, Logger, Factory, null, transportDataObject);
			var transport = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject1 = GetAdditionalReferenceNumbersForWayBills(transport);
			AssertEquals(1, additionalReferencesForObject1.Count);

			var masterBill = additionalReferencesForObject1[0];
			AssertEquals("456-342", masterBill.CE_EntryNum);
			AssertEquals("MAB", masterBill.CE_EntryType);

			var mab2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456-342", BillType = null };
			transportDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab2 });
			var reader2 = GetNewReader(transportDataObject, Logger, Factory, transport, transportDataObject);
			var newTransport = reader2.ReadIntoBusinessObject();
			var additionalReferencesForObject2 = GetAdditionalReferenceNumbersForWayBills(newTransport);
			AssertEquals(1, additionalReferencesForObject2.Count);

			var masterBillTwo = additionalReferencesForObject2[0];
			AssertEquals("456-342", masterBillTwo.CE_EntryNum);
			AssertEquals("HSB", masterBillTwo.CE_EntryType);
		}

		#endregion

		#region TestAdditionalReferences_LongNumber

		public void TestAdditionalReferences_LongNumber()
		{
			var transportDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportDataObject.TransportMode = new CodeDescriptionPair();
			transportDataObject.TransportMode.Code = Core.Constants.TransportModes.Air;
			transportDataObject.WayBillNumber = new ZString('1', 40);
			transportDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

			var masterBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
			var mab1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = new ZString('2', 40), BillType = masterBillType };
			var mab2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = new ZString('2', 40), BillType = masterBillType };

			transportDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab1, mab2 });
			var reader = GetNewReader(transportDataObject, Logger, Factory, null, transportDataObject);
			var transport = reader.ReadIntoBusinessObject();
			var additionalReferences = GetAdditionalReferenceNumbersForWayBills(transport);
			AssertEquals(2, additionalReferences.Count);

			var houseBill = additionalReferences[0];
			AssertEquals(new ZString('1', 35), houseBill.CE_EntryNum);
			AssertEquals("HSB", houseBill.CE_EntryType);

			var masterBillReference1 = additionalReferences[1];
			AssertEquals("222-" + new ZString('2', 31), masterBillReference1.CE_EntryNum);
			AssertEquals("MAB", masterBillReference1.CE_EntryType);
		}

		#endregion

		#region TestAdditionalReferences_WithWayBill

		public void TestAdditionalReferences_WithWayBill()
		{
			var transportDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportDataObject.WayBillNumber = "12345";
			transportDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

			var reader = GetNewReader(transportDataObject, Logger, Factory, null, transportDataObject);
			var transport = reader.ReadIntoBusinessObject();
			var additionalReferences = GetAdditionalReferenceNumbersForWayBills(transport);
			AssertEquals(1, additionalReferences.Count);

			var additionalReference = additionalReferences[0];
			AssertEquals("additionalReference.CE_EntryNum", "12345", additionalReference.CE_EntryNum);
			AssertEquals("If there is no WayBillNumberType the WayBill should be imported as HouseBill.", "HSB", additionalReference.CE_EntryType);
		}

		#endregion

		#region TestAdditionalReferences_WithWayBillsInAdditionalBillCollection

		public void TestAdditionalReferences_WithWayBillsInAdditionalBillCollection()
		{
			// i.e. Forwarding Shipment
			var transportDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportDataObject.WayBillNumber = "12345";
			transportDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

			var masterBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
			var mab1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456", BillType = masterBillType };
			var mab2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "789", BillType = masterBillType };

			transportDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab1, mab2 });
			var reader = GetNewReader(transportDataObject, Logger, Factory, null, transportDataObject);
			var transport = reader.ReadIntoBusinessObject();
			var additionalReferences = GetAdditionalReferenceNumbersForWayBills(transport);
			AssertEquals(3, additionalReferences.Count);

			var houseBill = additionalReferences[0];
			AssertEquals("12345", houseBill.CE_EntryNum);
			AssertEquals("HSB", houseBill.CE_EntryType);

			var masterBillReference1 = additionalReferences[1];
			AssertEquals("456", masterBillReference1.CE_EntryNum);
			AssertEquals("MAB", masterBillReference1.CE_EntryType);

			var masterBillReference2 = additionalReferences[2];
			AssertEquals("789", masterBillReference2.CE_EntryNum);
			AssertEquals("MAB", masterBillReference2.CE_EntryType);
		}

		#endregion

		#region TestPopulateNotes

		public void TestPopulateNotes()
		{
			var bookingShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingShipment.SetNoteCollection(() => new DataObjectList<Note>
			{
				new Note
				{
					Description = "Dangerous Goods Additional Handling Information",
					NoteText = "234",
					Visibility = new CodeDescriptionPair() { Code = nameof(StmNoteVisibility.PUB), Description = "Public" },
					NoteContext = new NoteContext() { Code = "123", Description = "123" },
					IsCustomDescription = false
				}
			});
			var reader = GetNewReader(bookingShipment, Logger, Factory, null, bookingShipment);
			var transportBO = reader.ReadIntoBusinessObject();
			var notes = transportBO.Notes.GetAllNotes();
			AssertEquals(1, notes.Count);
			var noteBO = (StmNote)notes.First();

			CombineAssertions(delegate
			{
				AssertEquals("noteBO.ST_Description", "Dangerous Goods Additional Handling Information", noteBO.ST_Description);
				AssertEquals("noteBO.ST_NoteDataAsText", "234", noteBO.ST_NoteDataAsText);
				AssertEquals("noteBO.ST_NoteContext", "123", noteBO.ST_NoteContext);
				AssertEquals("noteBO.ST_NoteType", "PUB", noteBO.ST_NoteType);
				AssertEquals("noteBO.ST_IsCustomDescription", false, noteBO.ST_IsCustomDescription);
			});
		}

		#endregion

		#region TestPopulateNotes_UnmatchedOrgs

		public void TestPopulateNotes_UnmatchedOrgs()
		{
			var bookingShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingShipment.SetNoteCollection(() => new DataObjectList<Note>
			{
				new Note
				{
					Description = "Unmatched Org Details",
					NoteText = @"Organisation Type: Consignor
Owner Code: 
EDI Code: 
Organisation Name: ASDFGAGDDGAGA
Address Line 1: ADSFAFFSS
Address Line 2: 344
City: FAFAF
Post Code: 24532
State or Province: ACT
Country: AU",
					Visibility = new CodeDescriptionPair() { Code = nameof(StmNoteVisibility.INT), Description = "INTERNAL" },
					NoteContext = new NoteContext() { Code = "AAA", Description = "AAA" },
					IsCustomDescription = false
				}
			});
			var reader = GetNewReader(bookingShipment, Logger, Factory, null, bookingShipment);
			var transportBO = reader.ReadIntoBusinessObject();
			var notes = transportBO.Notes.GetAllNotes();
			AssertEquals(1, notes.Count);
			var noteBO = (StmNote)notes.First();

			CombineAssertions(delegate
			{
				AssertEquals("noteBO.ST_Description", "Unmatched Org Details", noteBO.ST_Description);
				AssertEquals("noteBO.ST_NoteDataAsText", @"Organisation Type: Consignor
Owner Code: 
EDI Code: 
Organisation Name: ASDFGAGDDGAGA
Address Line 1: ADSFAFFSS
Address Line 2: 344
City: FAFAF
Post Code: 24532
State or Province: ACT
Country: AU", noteBO.ST_NoteDataAsText);
				AssertEquals("noteBO.ST_NoteContext", "AAA", noteBO.ST_NoteContext);
				AssertEquals("noteBO.ST_NoteType", "INT", noteBO.ST_NoteType);
				AssertEquals("noteBO.ST_IsCustomDescription", false, noteBO.ST_IsCustomDescription);
			});
		}

		#endregion

		#region Implementation

		protected abstract DtbTransport GetNewTransport();
		protected abstract DtbTransportDataObjectReader<T> GetNewReader(UniversalShipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory, DtbTransport transport, UniversalShipment topLevelDO);
		protected abstract ICusEntryNumAdditionalReferenceCollection GetAdditionalReferenceNumbersForWayBills(T transport);

		#endregion
	}
}
