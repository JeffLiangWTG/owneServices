using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	partial class AirManifestDataObjectReaderTest : DataTransfer.Universal.AirManifest.Testing.AirManifestDataObjectReaderTestHelper
	{
		public void TestReturnETailHAWBReaderWhenIsHVLV()
		{
			var mawbDataObject = SetupAirCargoMaster("MB2343", ZString.Empty);
			var hawbDataObject = SetupAirCargoHouse("HB4523");
			var mawbBO = Factory.New<CusMAWB>();
			var logger = new DummyLogger();
			var reader = new CusMAWBDataObjectReader(mawbDataObject, null, logger, Factory, ZGuid.Empty);
			var hawbReader = GetHAWBReader(reader);
			AssertType<CusHAWBDataObjectReader>(hawbReader);

			reader = new CusMAWBDataObjectReader(mawbDataObject, hawbDataObject, logger, Factory, ZGuid.Empty);
			hawbReader = GetHAWBReader(reader);
			AssertType<ETailCusHAWBDataObjectReader>(hawbReader);

			DataObjectReader GetHAWBReader(CusMAWBDataObjectReader cusMAWBDataObjectReader) =>
				((IEnumerable<DataTransfer.Universal.AirManifest.CusHAWBDataObjectReader<CusMAWB, CusHAWB, DataTransfer.Universal.AirManifest.AirManifestDataObjectReaderHelper>>)typeof(CusMAWBDataObjectReader).
				InvokeMember("GetNewCusHAWBDataObjectReaders",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod,
				null,
				cusMAWBDataObjectReader,
				new object[] { hawbDataObject, mawbDataObject, mawbBO })).Single();
		}

		public void TestCreateNewUniversalDataObjectReaderHelper_WhenIsHVLV()
		{
			var mawbDataObject = SetupAirCargoMaster("MB2343", ZString.Empty);
			var hawbDataObject = SetupAirCargoHouse("HB4523");
			var logger = new DummyLogger();
			var reader = new CusMAWBDataObjectReader(mawbDataObject, hawbDataObject, logger, Factory, ZGuid.Empty);
			var getHelperMethod = typeof(CusMAWBDataObjectReader).GetMethod("CreateNewUniversalDataObjectReaderHelper", BindingFlags.Instance | BindingFlags.NonPublic);
			var helper = getHelperMethod.Invoke(reader, null);
			AssertType<ETailAirManifestDataObjectReaderHelper>(helper);
		}

		public void TestResponsiblePartyAddressTypeFallbackToShippingLineAddress()
		{
			var mawbDataObject = SetupAirCargoMaster("MB2343", ZString.Empty);
			var hawbDataObject = SetupAirCargoHouse("HB4523");
			var logger = new DummyLogger();
			var reader = new CusMAWBDataObjectReader(mawbDataObject, null, logger, Factory, ZGuid.Empty);
			var shippingLineAddressType = GetResponsiblePartyAddressType(reader);

			AssertEquals(AddressTypes.ShippingLine, shippingLineAddressType);

			reader = new CusMAWBDataObjectReader(mawbDataObject, hawbDataObject, logger, Factory, ZGuid.Empty);
			shippingLineAddressType = GetResponsiblePartyAddressType(reader);

			AssertEquals(nameof(DocAddressType.ShippingLineAddress), shippingLineAddressType);

			string GetResponsiblePartyAddressType(CusMAWBDataObjectReader cusMAWBDataObjectReader) =>
				typeof(CusMAWBDataObjectReader).
				InvokeMember("ResponsiblePartyAddressType",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty,
				null,
				cusMAWBDataObjectReader,
				null).ToString();
		}

		public void TestFillDatesFromTransportLegs()
		{
			var mawbDataObject = SetupAirCargoMaster("MB2343", ZString.Empty);
			mawbDataObject.PortOfLoading = new UNLOCO() { Code = "NZAKL" };
			mawbDataObject.PortOfDischarge = new UNLOCO() { Code = "AUSYD" };
			mawbDataObject.Branch = Branch.New(CurrentCompanySecondBranch);
			mawbDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			var leg1 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance) { PortOfLoading = new UNLOCO() { Code = "NZAKL" }, PortOfDischarge = new UNLOCO() { Code = "USLAX" }, LegOrder = 1, ActualDeparture = ZDateTime.Today.AddDays(-3) };
			var leg2 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance) { PortOfLoading = new UNLOCO() { Code = "USLAX" }, PortOfDischarge = new UNLOCO() { Code = "AUSYD" }, LegOrder = 2, ActualArrival = ZDateTime.Today };
			mawbDataObject.TransportLegCollection.Add(leg1);
			mawbDataObject.TransportLegCollection.Add(leg2);

			Factory.SaveForTesting();
			var message = GetQueuedUniversalShipmentMessage(mawbDataObject);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var mawbQuery = new ZQuery(CusMAWBSchema.CM_MAWB, "MB2343");
			var mawbBO = Factory.LoadTop1<CusMAWB>(mawbQuery);

			AssertEquals(ZDateTime.Today.AddDays(-3), mawbBO.CM_DepartureDate);
			AssertEquals(ZDateTime.Today, mawbBO.CM_ArrivalDate);

			Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestUpdateExistingMAWB()
		{
			var mawb = Factory.BOFactory.New<CusMAWB>();
			mawb.CM_FlightNo = "QF344";
			mawb.CM_ArrivalDate = new ZDateTime(2014, 04, 15, 12, 45, 00);
			mawb.CM_MAWB = "MB2343";
			mawb.CM_RL_NKDischargePort = "NZAKL";
			mawb.CM_RL_NKLoadPort = "AUSYD";

			var hawb1 = mawb.ChildBills.AddNew();
			hawb1.CS_HAWB = "HB-X";
			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "HB-Y";
			var hawb3 = mawb.ChildBills.AddNew();
			hawb3.CS_HAWB = "HB-Z";
			AssertEquals(1, hawb1.CS_ConsignmentNum);
			AssertEquals(2, hawb2.CS_ConsignmentNum);
			AssertEquals(3, hawb3.CS_ConsignmentNum);
			Factory.SaveForTesting();

			var mawbDataObject = SetupAirCargoMaster("MB2343", ZString.Empty);
			mawbDataObject.VoyageFlightNo = "QF344";
			mawbDataObject.SetDateCollection(() => new List<Date>());
			mawbDataObject.DateCollection.Add(new Date() { Type = DateType.DischargeDate, Value = new ZDateTime(2014, 04, 15, 10, 12, 00) });

			// Message to delete HB-X and add HB-E.
			var hawbDataObject = SetupAirCargoHouse("HB-E");
			var hawbDataObject2 = SetupAirCargoHouse("HB-Y");
			var hawbDataObject3 = SetupAirCargoHouse("HB-Z");
			mawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject);
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject2);
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject3);

			var message = GetQueuedUniversalShipmentMessage(mawbDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();

			var otherFactory = new BusinessObjectFactory();
			var importedHawbY = otherFactory.LoadTop1<CusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "HB-Y"));
			AssertEquals("HB-Y", 1, importedHawbY.CS_ConsignmentNum);
			var importedHawbZ = otherFactory.LoadTop1<CusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "HB-Z"));
			AssertEquals("HB-Z", 2, importedHawbZ.CS_ConsignmentNum);
			var importedHawbE = otherFactory.LoadTop1<CusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "HB-E"));
			AssertEquals("HB-E", 3, importedHawbE.CS_ConsignmentNum);

			var mawbBO = new BusinessObjectFactory().Load<CusMAWB>(mawb.PK);
			var orderedHawbs = mawbBO.ChildBills.Cast<CusHAWB>().OrderBy(x => x.CS_ConsignmentNum).ToArray();

			AssertEquals("HB-Y", 1, orderedHawbs[0].CS_ConsignmentNum);
			AssertEquals("HB-Z", 2, orderedHawbs[1].CS_ConsignmentNum);
			AssertEquals("HB-E", 3, orderedHawbs[2].CS_ConsignmentNum);
			AssertEquals("HB-Y", orderedHawbs[0].CS_HAWB);
			AssertEquals("HB-Z", orderedHawbs[1].CS_HAWB);
			AssertEquals("HB-E", orderedHawbs[2].CS_HAWB);
			AssertEquals("mawbBO.ChildBills.Count", 3, mawbBO.ChildBills.Count);
		}

		public void TestGetReasonForNotAbleToUpdateFromNZCusMAWB()
		{
			var mawb = Factory.BOFactory.New<CusMAWB>();
			mawb.CM_FlightNo = "QF344";
			mawb.CM_ArrivalDate = new ZDateTime(2014, 04, 15, 12, 45, 00);
			mawb.CM_MAWB = "MB2343";
			mawb.CM_RL_NKDischargePort = "NZAKL";
			mawb.CM_RL_NKLoadPort = "AUSYD";

			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "HB2343";
			hawb.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;

			Factory.SaveForTesting();

			var hawbDataObject = SetupAirCargoHouse("HB2343");
			var mawbDataObject = SetupAirCargoMaster("MB2343", ZString.Empty);
			mawbDataObject.VoyageFlightNo = "QF344";
			mawbDataObject.SetDateCollection(() => new List<Date>());
			mawbDataObject.DateCollection.Add(new Date() { Type = DateType.DischargeDate, Value = new ZDateTime(2014, 04, 15, 10, 12, 00) });
			mawbDataObject.PortOfLoading = new UNLOCO() { Code = "AUMEL" };
			mawbDataObject.PortOfDischarge = new UNLOCO() { Code = "NZHLZ" };
			mawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject);

			var message = GetQueuedUniversalShipmentMessage(mawbDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("Logs", @"Successfully loaded matching CusMAWB.
Error - Cannot populate CusMAWB because:
There is an attempt to update Discharge Port from 'NZAKL' to 'NZHLZ' on this Master/Sub-Master while it has at least one House Bill with an active messaging.

Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
		}

		public void TestImportingCusMAWBData()
		{
			var shippingLine = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var mawbDataObject = SetupAirCargoMaster("MB2343", ZString.Empty);
			mawbDataObject.Branch = Branch.New(CurrentCompanySecondBranch);

			mawbDataObject.SetDateCollection(() => new List<Date>());
			mawbDataObject.DateCollection.Add(Date.New(DateType.LoadingDate, ZBool.True, new ZDateTime(2010, 1, 4)));
			mawbDataObject.DateCollection.Add(Date.New(DateType.LoadingDate, ZBool.False, new ZDateTime(2010, 1, 3)));
			mawbDataObject.DateCollection.Add(Date.New(DateType.FirstArrivalInCountry, ZBool.True, new ZDateTime(2010, 1, 6)));
			mawbDataObject.DateCollection.Add(Date.New(DateType.FirstArrivalInCountry, ZBool.False, new ZDateTime(2010, 1, 5)));
			mawbDataObject.DateCollection.Add(Date.New(DateType.DischargeDate, ZBool.True, new ZDateTime(2010, 1, 7)));
			mawbDataObject.DateCollection.Add(Date.New(DateType.DischargeDate, ZBool.False, new ZDateTime(2010, 1, 8)));

			mawbDataObject.SetNoteCollection(() => new DataObjectList<Note>());
			mawbDataObject.NoteCollection.Add(SetupNote());
			mawbDataObject.NoteCollection.Add(SetupNote2());

			mawbDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			mawbDataObject.OrganizationAddressCollection.Add(SetupOrganizationAddress(AddressTypes.Forwarder));
			var shippingLineData = SetupOrganizationAddress(AddressTypes.ShippingLine, null, null, shippingLine.OH_FullName, shippingLine.MainAddress.OA_Address1, shippingLine.MainAddress.OA_Address2,
				shippingLine.MainAddress.OA_City, shippingLine.MainAddress.OA_State, shippingLine.MainAddress.OA_PostCode, UNLOCO.New(shippingLine.MainAddress.EffectiveRelatedPortCode), Country.New(shippingLine.MainAddress.RelatedCountry), shippingLine.Contacts[0].OC_ContactName, shippingLine.MainAddress.OA_Phone);
			mawbDataObject.OrganizationAddressCollection.Add(shippingLineData);

			mawbDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>
			{
				CustomizedField.New("CustomField1", (ZString)"CustomField1"),
				CustomizedField.New("CustomField2PART1", (ZString)"CustomField2PART1"),
				CustomizedField.New("CustomField2PART2", (ZString)"CustomField2PART2")
			});

			var hawbDataObject1 = SetupAirCargoHouse("HB2343", ZBool.False, null);
			var hawbDataObject2 = SetupAirCargoHouse("HB8965", ZBool.False, null);

			mawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject1);
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject2);
			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(mawbDataObject);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var mawbQuery = new ZQuery(CusMAWBSchema.CM_MAWB, "MB2343");
			var mawbBO = Factory.LoadTop1<CusMAWB>(mawbQuery);

			AssertNotNull("mawbBO", mawbBO);

			#region Check Contents of mawb Business Object

			CombineAssertions(delegate
			{
				AssertContents(mawbBO, "MB2343", ZString.Empty);
				AssertEquals("mawbBO.CM_ApplicationCode", Enterprise.Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff, mawbBO.CM_ApplicationCode);
				AssertEquals("mawbBO.CM_IsCTOMAWB", ZBool.False, mawbBO.CM_IsCTOMAWB);
				AssertEquals("mawbBO.CM_DepartureDate", new ZDateTime(2010, 1, 3), mawbBO.CM_DepartureDate);
				AssertEquals("mawbBO.CM_ArrivalDate", new ZDateTime(2010, 1, 8), mawbBO.CM_ArrivalDate);
				AssertEquals("mawbBO.CM_OH_ResponsibleParty", shippingLine.PK, mawbBO.CM_OH_ResponsibleParty);
				AssertEquals("mawbBO.CustomsFields1", "CustomField1", mawbBO.GetUserDefinedValue<ZString>("CustomField1"));
				AssertEquals("mawbBO.CustomsFields2Part1", "CustomField2PART1", mawbBO.GetUserDefinedValue<ZString>("CustomField2PART1"));
				AssertEquals("mawbBO.CustomsFields2Part2", "CustomField2PART2", mawbBO.GetUserDefinedValue<ZString>("CustomField2PART2"));
				mawbBO.ChildBills.Load();
				AssertEquals("mawbBO.ChildBills.Count", 2, mawbBO.ChildBills.Count);
				var hawbBO1 = mawbBO.ChildBills[0];
				var hawbBO2 = mawbBO.ChildBills[1];
				if (hawbBO2.CS_HAWB == "HB2343")
				{
					hawbBO1 = mawbBO.ChildBills[1];
					hawbBO2 = mawbBO.ChildBills[0];
				}
				AssertEquals("hawbBO.CS_HAWB", "HB2343", hawbBO1.CS_HAWB);
				AssertEquals("subhawbBO.CS_HAWB", "HB8965", hawbBO2.CS_HAWB);

				AssertMultilineASCIIEquals("logger.Logs", @"
No matching CusMAWB found, creating new CusMAWB.
Populating CusMAWB...
Matching 'ShippingLine':- Matched to 'WUFSHIJNB' address 'Level 2, Building G' with a score of 250.
No matching StmNote found, creating new StmNote.
Populating StmNote...
No matching StmNote found, creating new StmNote.
Populating StmNote...
Warning - Description(value: WORM EATER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Added Consignment: (HAWB: HB2343) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Added Consignment: (HAWB: HB8965) from UniversalShipment.
Added AirCargo Report (MAWB: MB2343) from UniversalShipment.
Successfully saved AirCargo Report (MAWB: MB2343 Job#: X00001000) with 2 x StmNote, 2 x CusHAWB.
".Trim(), message.GetLogNoteText());
			});

			#endregion

			Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestCusMAWBReaderWhenDataDefaultingIsOff()
		{
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				TestImportingCusMAWBData();
				TestUpdateExistingMAWB();
			}
		}
	}
}
