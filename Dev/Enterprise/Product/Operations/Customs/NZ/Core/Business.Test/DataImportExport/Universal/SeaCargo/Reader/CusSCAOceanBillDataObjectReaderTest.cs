using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest.Testing;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	[TestedType(typeof(CusSCAOceanBillDataObjectReader))]
	partial class CusSCAOceanBillDataObjectReaderTest : CusSCAOceanBillDataObjectReaderTest<CusSCAOceanBill, CusSCAHouse, CusSCAContainer, CusSCAPackingLine>
	{
		protected override ZString ApplicationCodeMatchingXML => JobApplicationCodeList.Codes.TSW;

		protected override ITopLevelDataObjectReader GetReader(Shipment dataObject, Shipment hvlv, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new CusSCAOceanBillDataObjectReader(dataObject, hvlv, logger, factory);
		}

		protected override string OceanBillShipmentXML => TestFileHelper.ReadResourceFileContents("OceanBillShipment1.xml", Assembly.GetExecutingAssembly());

		public void TestFillDatesFromTransportLegs()
		{
			var logger = new DummyLogger();
			var shipment = ReadXMLIntoShipment(logger, OceanBillShipmentXML);
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.SeaOceanBill, null);
			shipment.DataContext = dataContext;

			shipment.DateCollection.Clear();
			shipment.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());

			var leg1 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance) { PortOfLoading = new UNLOCO() { Code = "NZCHC" }, PortOfDischarge = new UNLOCO() { Code = "USLAX" }, LegOrder = 1, ActualDeparture = ZDateTime.Today.AddDays(-3) };
			var leg2 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance) { PortOfLoading = new UNLOCO() { Code = "USLAX" }, PortOfDischarge = new UNLOCO() { Code = "AUSYD" }, LegOrder = 2, ActualArrival = ZDateTime.Today };
			shipment.TransportLegCollection.Add(leg1);
			shipment.TransportLegCollection.Add(leg2);

			Factory.SaveForTesting();
			var message = GetQueuedUniversalShipmentMessage(shipment);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var oceanBillQuery = new ZQuery(CusSCAOceanBillSchema.CB_OceanBill, "TEST OCEAN BILL");
			var oceanBillBO = Factory.LoadTop1<CusSCAOceanBill>(oceanBillQuery);

			AssertEquals(ZDateTime.Today.AddDays(-3), oceanBillBO.CB_DateOfDeparture);
			AssertEquals(ZDateTime.Today, oceanBillBO.CB_DateOfArrival);
		}

		public void TestUpdateExistingOceanBillSetsLineNumbers()
		{
			var existingOceanBill = CreateOceanBillMatchingXML();
			var existingBill = existingOceanBill.HouseBills.AddNew();
			existingBill.CA_ConsignmentNum = 1;
			existingBill.CA_HouseBill = "HBX";
			existingBill.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.Acknowledgement;
			Assert("Precondition existingBill Cannot Delete", !existingBill.CanDelete);

			var logger = new DummyLogger();
			var shipment = ReadXMLIntoShipment(logger, OceanBillShipmentXML);

			BusinessObject targetBO = null;
			GetReader(shipment, null, logger, Factory).ReadIntoBusinessObject(ref targetBO);
			Factory.SaveForTesting();
			var oceanBill = targetBO as CusSCAOceanBill;
			AssertSame("Existing Ocean Bill was updated", existingOceanBill, oceanBill);

			var oceanBillInOtherFactory = new BusinessObjectFactory().Load<CusSCAOceanBill>(oceanBill.PK);
			var houseBills = oceanBillInOtherFactory.HouseBills;
			AssertEquals("HouseBills.Count", 3, houseBills.Count);
			var house1 = houseBills[0];
			var house2 = houseBills[1];
			var house3 = houseBills[2];

			AssertEquals("house1 CA_ConsignmentNum", 1, house1.CA_ConsignmentNum);
			AssertEquals("house2 CA_ConsignmentNum", 2, house2.CA_ConsignmentNum);
			AssertEquals("house3 CA_ConsignmentNum", 3, house3.CA_ConsignmentNum);

			AssertEquals("house1 CA_HouseBill", "HBX", house1.CA_HouseBill);
			AssertEquals("house2 CA_HouseBill", "1", house2.CA_HouseBill);
			AssertEquals("house3 CA_HouseBill", "2", house3.CA_HouseBill);
		}

		public void TestUpdateExistingOceanBillRemovesUnwantedLines()
		{
			var existingOceanBill = CreateOceanBillMatchingXML();
			existingOceanBill.CB_RL_NKPortOfLoading = "NZCHC";
			existingOceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
			var existingBill1 = existingOceanBill.HouseBills.AddNew();
			existingBill1.CA_ConsignmentNum = 1;
			existingBill1.CA_HouseBill = "X";
			existingBill1.CA_ShipmentStatus = ZString.Empty;
			Assert("Precondition Bill1.CanDelete", existingBill1.CanDelete);

			var existingBill2 = existingOceanBill.HouseBills.AddNew();
			existingBill2.CA_ConsignmentNum = 2;
			existingBill2.CA_HouseBill = "1";
			existingBill2.CA_ShipmentStatus = ZString.Empty;
			Assert("Precondition Bill2.CanDelete", existingBill2.CanDelete);

			var existingBill3 = existingOceanBill.HouseBills.AddNew();
			existingBill3.CA_ConsignmentNum = 3;
			existingBill3.CA_HouseBill = "2";
			existingBill3.CA_ShipmentStatus = ZString.Empty;
			Assert("Precondition Bill3.CanDelete", existingBill3.CanDelete);
			Factory.SaveForTesting();

			var logger = new DummyLogger();
			var shipment = ReadXMLIntoShipment(logger, OceanBillShipmentXML);

			var readerFactory = new BusinessObjectFactory();
			readerFactory.RefreshEnabled = false;
			readerFactory.SuspendValidation();
			var readerUniversalFactory = new UniversalObjectFactory(readerFactory);

			BusinessObject targetBO = null;
			GetReader(shipment, null, logger, readerUniversalFactory).ReadIntoBusinessObject(ref targetBO);
			readerUniversalFactory.SaveForTesting();
			var oceanBill = targetBO as CusSCAOceanBill;
			AssertEquals("Existing Ocean Bill was updated", existingOceanBill.PK, oceanBill.PK);

			var oceanBillInOtherFactory = new BusinessObjectFactory().Load<CusSCAOceanBill>(oceanBill.PK);
			var houseBills = oceanBillInOtherFactory.HouseBills;
			AssertEquals("HouseBills.Count", 2, houseBills.Count);
			var house1 = houseBills[0];
			var house2 = houseBills[1];

			AssertEquals("house1 CA_ConsignmentNum", 1, house1.CA_ConsignmentNum);
			AssertEquals("house2 CA_ConsignmentNum", 2, house2.CA_ConsignmentNum);

			AssertEquals("house1 CA_HouseBill", "1", house1.CA_HouseBill);
			AssertEquals("house2 CA_HouseBill", "2", house2.CA_HouseBill);
		}

		public void TestHVLVImport_HandleUnmatchedBills_WhenTrueEnableHVLVMultiShipmentNZICRCRE()
		{
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var forwardingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				forwardingShipment.JS_UniqueConsignRef = "S00001539"; // value from File

				var otherShipment = Factory.NewWithValidTestData<ForwardingShipment>();

				var existingOceanBill = Factory.New<CusSCAOceanBill>();
				existingOceanBill.CB_OceanBill = "TEST BOL";
				existingOceanBill.CB_LloydsIMO = "9152741";
				existingOceanBill.CB_Voyage = "TEST";
				existingOceanBill.CB_MasterHouseBill = "HB1";
				existingOceanBill.CB_IsActive = true;

				var existingBill1_HVLV_ThisShipment = existingOceanBill.HouseBills.AddNew();
				existingBill1_HVLV_ThisShipment.CA_ShipmentStatus = ZString.Empty;
				existingBill1_HVLV_ThisShipment.CA_ConsignmentNum = 1;
				existingBill1_HVLV_ThisShipment.CA_HouseBill = "1";
				existingBill1_HVLV_ThisShipment.CA_IsHVLV = true;
				existingBill1_HVLV_ThisShipment.CA_JS = forwardingShipment.PK;

				var existingBill2_HVLV_OtherShipment = existingOceanBill.HouseBills.AddNew();
				existingBill2_HVLV_OtherShipment.CA_ShipmentStatus = ZString.Empty;
				existingBill2_HVLV_OtherShipment.CA_ConsignmentNum = 2;
				existingBill2_HVLV_OtherShipment.CA_HouseBill = "2";
				existingBill2_HVLV_OtherShipment.CA_IsHVLV = true;
				existingBill2_HVLV_OtherShipment.CA_JS = otherShipment.PK;

				var existingBill3_HVLV_ManuallyAdded = existingOceanBill.HouseBills.AddNew();
				existingBill3_HVLV_ManuallyAdded.CA_ShipmentStatus = ZString.Empty;
				existingBill3_HVLV_ManuallyAdded.CA_ConsignmentNum = 3;
				existingBill3_HVLV_ManuallyAdded.CA_HouseBill = "3";
				existingBill3_HVLV_ManuallyAdded.CA_IsHVLV = true;
				existingBill3_HVLV_ManuallyAdded.CA_JS = ZGuid.Empty;

				var existingBill4_STD_OtherShipment = existingOceanBill.HouseBills.AddNew();
				existingBill4_STD_OtherShipment.CA_ShipmentStatus = ZString.Empty;
				existingBill4_STD_OtherShipment.CA_ConsignmentNum = 4;
				existingBill4_STD_OtherShipment.CA_HouseBill = "4";
				existingBill4_STD_OtherShipment.CA_IsHVLV = false;
				existingBill4_STD_OtherShipment.CA_JS = otherShipment.PK;

				var existingBill5_STD_ManuallyAdded = existingOceanBill.HouseBills.AddNew();
				existingBill5_STD_ManuallyAdded.CA_ShipmentStatus = ZString.Empty;
				existingBill5_STD_ManuallyAdded.CA_ConsignmentNum = 5;
				existingBill5_STD_ManuallyAdded.CA_HouseBill = "5";
				existingBill5_STD_ManuallyAdded.CA_IsHVLV = false;
				existingBill5_STD_ManuallyAdded.CA_JS = ZGuid.Empty;

				Factory.SaveForTesting();

				var logger = new DummyLogger();
				var consolDataObject = ReadXMLIntoShipment(logger, TestFileHelper.ReadResourceFileContents("HVLVSeaShipment.xml", Assembly.GetExecutingAssembly()));

				var readerFactory = new BusinessObjectFactory();
				readerFactory.RefreshEnabled = false;
				readerFactory.SuspendValidation();
				var readerUniversalFactory = new UniversalObjectFactory(readerFactory);

				BusinessObject targetBO = null;
				var reader = new CusSCAOceanBillDataObjectReader(existingOceanBill, null, consolDataObject, consolDataObject.SubShipmentCollection.Single(), new DummyLogger(), readerUniversalFactory);
				((ITopLevelDataObjectReader)reader).ReadIntoBusinessObject(ref targetBO);

				readerUniversalFactory.SaveForTesting();
				var oceanBill = targetBO as CusSCAOceanBill;
				AssertEquals("Existing Ocean Bill was updated", existingOceanBill.PK, oceanBill.PK);

				var oceanBillInOtherFactory = new BusinessObjectFactory().Load<CusSCAOceanBill>(oceanBill.PK);
				var houseBills = oceanBillInOtherFactory.HouseBills;

				CombineAssertions(() =>
				{
					AssertNull("Bill1 is deleted as it is HVLV", houseBills.FirstOrDefault(x => ((CusSCAHouse)x).CA_HouseBill == "1"));
					AssertNotNull("Bill2 is deleted as it is HVLV", houseBills.FirstOrDefault(x => ((CusSCAHouse)x).CA_HouseBill == "2"));
					AssertNull("Bill3 is deleted as it is not on any shipment (manually added)", houseBills.FirstOrDefault(x => ((CusSCAHouse)x).CA_HouseBill == "3"));
					AssertNotNull("Bill4 is not deleted as it is Not HVLV but is attached to a shipment", houseBills.FirstOrDefault(x => ((CusSCAHouse)x).CA_HouseBill == "4"));
					AssertNull("Bill5 is deleted as it is Not attached to any shipment (manually added)", houseBills.FirstOrDefault(x => ((CusSCAHouse)x).CA_HouseBill == "5"));
				});
			}
		}

		public void TestHVLVImport_HandleUnmatchedBills_WhenFalseEnableHVLVMultiShipmentNZICRCRE()
		{
			var forwardingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			forwardingShipment.JS_UniqueConsignRef = "S00001539"; // value from File

			var otherShipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var existingOceanBill = Factory.New<CusSCAOceanBill>();
			existingOceanBill.CB_OceanBill = "TEST BOL";
			existingOceanBill.CB_LloydsIMO = "9152741";
			existingOceanBill.CB_Voyage = "TEST";
			existingOceanBill.CB_MasterHouseBill = "HB1";
			existingOceanBill.CB_IsActive = true;

			var existingBill1_HVLV_ThisShipment = existingOceanBill.HouseBills.AddNew();
			existingBill1_HVLV_ThisShipment.CA_ShipmentStatus = ZString.Empty;
			existingBill1_HVLV_ThisShipment.CA_ConsignmentNum = 1;
			existingBill1_HVLV_ThisShipment.CA_HouseBill = "1";
			existingBill1_HVLV_ThisShipment.CA_IsHVLV = true;
			existingBill1_HVLV_ThisShipment.CA_JS = forwardingShipment.PK;

			var existingBill2_HVLV_OtherShipment = existingOceanBill.HouseBills.AddNew();
			existingBill2_HVLV_OtherShipment.CA_ShipmentStatus = ZString.Empty;
			existingBill2_HVLV_OtherShipment.CA_ConsignmentNum = 2;
			existingBill2_HVLV_OtherShipment.CA_HouseBill = "2";
			existingBill2_HVLV_OtherShipment.CA_IsHVLV = true;
			existingBill2_HVLV_OtherShipment.CA_JS = otherShipment.PK;

			var existingBill3_HVLV_ManuallyAdded = existingOceanBill.HouseBills.AddNew();
			existingBill3_HVLV_ManuallyAdded.CA_ShipmentStatus = ZString.Empty;
			existingBill3_HVLV_ManuallyAdded.CA_ConsignmentNum = 3;
			existingBill3_HVLV_ManuallyAdded.CA_HouseBill = "3";
			existingBill3_HVLV_ManuallyAdded.CA_IsHVLV = true;
			existingBill3_HVLV_ManuallyAdded.CA_JS = ZGuid.Empty;

			var existingBill4_STD_OtherShipment = existingOceanBill.HouseBills.AddNew();
			existingBill4_STD_OtherShipment.CA_ShipmentStatus = ZString.Empty;
			existingBill4_STD_OtherShipment.CA_ConsignmentNum = 4;
			existingBill4_STD_OtherShipment.CA_HouseBill = "4";
			existingBill4_STD_OtherShipment.CA_IsHVLV = false;
			existingBill4_STD_OtherShipment.CA_JS = otherShipment.PK;

			var existingBill5_STD_ManuallyAdded = existingOceanBill.HouseBills.AddNew();
			existingBill5_STD_ManuallyAdded.CA_ShipmentStatus = ZString.Empty;
			existingBill5_STD_ManuallyAdded.CA_ConsignmentNum = 5;
			existingBill5_STD_ManuallyAdded.CA_HouseBill = "5";
			existingBill5_STD_ManuallyAdded.CA_IsHVLV = false;
			existingBill5_STD_ManuallyAdded.CA_JS = ZGuid.Empty;

			Factory.SaveForTesting();

			var logger = new DummyLogger();
			var consolDataObject = ReadXMLIntoShipment(logger, TestFileHelper.ReadResourceFileContents("HVLVSeaShipment.xml", Assembly.GetExecutingAssembly()));

			var readerFactory = new BusinessObjectFactory();
			readerFactory.RefreshEnabled = false;
			readerFactory.SuspendValidation();
			var readerUniversalFactory = new UniversalObjectFactory(readerFactory);

			BusinessObject targetBO = null;
			var reader = new CusSCAOceanBillDataObjectReader(existingOceanBill, null, consolDataObject, consolDataObject.SubShipmentCollection.Single(), new DummyLogger(), readerUniversalFactory);
			((ITopLevelDataObjectReader)reader).ReadIntoBusinessObject(ref targetBO);

			readerUniversalFactory.SaveForTesting();
			var oceanBill = targetBO as CusSCAOceanBill;

			var oceanBillInOtherFactory = new BusinessObjectFactory().Load<CusSCAOceanBill>(oceanBill.PK);
			var houseBills = oceanBillInOtherFactory.HouseBills;

			CombineAssertions(() =>
			{
				AssertNull("Bill1 is deleted as it is HVLV", houseBills.FirstOrDefault(x => ((CusSCAHouse)x).CA_HouseBill == "1"));
				AssertNull("Bill2 is deleted as it is HVLV", houseBills.FirstOrDefault(x => ((CusSCAHouse)x).CA_HouseBill == "2"));
				AssertNull("Bill3 is deleted as it is not on any shipment (manually added)", houseBills.FirstOrDefault(x => ((CusSCAHouse)x).CA_HouseBill == "3"));
				AssertNotNull("Bill4 is not deleted as it is Not HVLV but is attached to a shipment", houseBills.FirstOrDefault(x => ((CusSCAHouse)x).CA_HouseBill == "4"));
				AssertNull("Bill5 is deleted as it is Not attached to any shipment (manually added)", houseBills.FirstOrDefault(x => ((CusSCAHouse)x).CA_HouseBill == "5"));
			});
		}

		public void TestUpdateExistingOceanBillDoesntBlowUpWhenDataDefaultingIsOff()
		{
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				TestUpdateExistingOceanBillSetsLineNumbers();
				TestUpdateExistingOceanBillRemovesUnwantedLines();
			}
		}

		protected override void AssertHouseBill1AllPropertiesSet(CusSCAHouse houseBill)
		{
			base.AssertHouseBill1AllPropertiesSet(houseBill);
			AssertEquals("CA_ConsignmentNum", 1, houseBill.CA_ConsignmentNum);
		}

		protected override void AssertHouseBill2AllPropertiesSet(CusSCAHouse houseBill)
		{
			base.AssertHouseBill2AllPropertiesSet(houseBill);
			AssertEquals("CA_ConsignmentNum", 2, houseBill.CA_ConsignmentNum);
		}
	}
}
