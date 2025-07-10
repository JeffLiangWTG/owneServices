using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest.Testing
{
	[TestsSubclassesOf(typeof(CusSCAOceanBillDataObjectReader<,,,>))]
	public abstract class CusSCAOceanBillDataObjectReaderTest<TCusSCAOceanBill, TCusSCAHouse, TCusSCAContainer, TCusSCAPivot> : TestCaseWithFactoryAndMessagingHelpers
			where TCusSCAOceanBill : BaseCusSCAOceanBill
			where TCusSCAHouse : BaseCusSCAHouse
			where TCusSCAContainer : BaseCusSCAContainer
			where TCusSCAPivot : BaseCusSCAPivot
	{
		public static Shipment ReadXMLIntoShipment(IXmlImportLogger logger, string xml)
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
			}
			return shipment;
		}

		public void TestGetExistingBusinessObject()
		{
			var oceanBill1 = CreateOceanBillMatchingXML();
			oceanBill1.CB_MasterHouseBill = "MH1";
			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Table = CusSCAOceanBillSchema.Constants.TableName;
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log.SL_SE_NKEvent = Events.AddedARecordToTheSystemCode;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log.SL_Parent = oceanBill1.PK;
				log.SL_EventTime = ZDateTime.Today.AddHours(-1);
			}

			var oceanBill4 = CreateOceanBillMatchingXML();
			oceanBill4.CB_MasterHouseBill = "MH1";
			var log2 = Factory.New<StmALog>();
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_Table = CusSCAOceanBillSchema.Constants.TableName;
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log2.SL_SE_NKEvent = Events.AddedARecordToTheSystemCode;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log2.SL_Parent = oceanBill4.PK;
				log2.SL_EventTime = ZDateTime.Today.AddHours(-2);
			}

			var oceanBill2 = CreateOceanBillMatchingXML();
			oceanBill2.CB_MasterHouseBill = "MH2";

			var oceanBill3 = CreateOceanBillMatchingXML();
			oceanBill3.CB_MasterHouseBill = "MH3";

			var shipment1 = GetShipment();
			shipment1.SetAdditionalBillCollection(() => new List<AdditionalBill>());
			var masterBill = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillType = new WayBillType() { Code = WayBillTypeList.Codes.MasterHouse }, ParentBillNumber = "TEST OCEAN BILL", BillNumber = "MH1" };
			shipment1.AdditionalBillCollection.Add(masterBill);

			var shipment2 = GetShipment();
			shipment2.ShipmentType = new CodeDescriptionPair() { Code = AgentType.CoLoad };
			shipment2.BookingConfirmationReference = "MH2";

			var shipment3 = GetShipment();
			shipment3.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment3.SubShipmentCollection.Add(hvlvShipment);
			hvlvShipment.ShipmentType = new CodeDescriptionPair() { Code = ShipmentTypes.HighVolumeLowValueLegacy };
			var dataContext = DataContextFactory.New();
			dataContext.SetWorkflowInfo(new WorkflowInfo()
			{
				RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.HSA } }
			});
			shipment3.DataContext = dataContext;

			var logger = new DummyLogger();

			AssertEquals(oceanBill1, GetReader(shipment1, null, logger, Factory).GetExistingBusinessObject());
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = ZDateTime.Today.AddHours(-5);
			}
			AssertEquals(oceanBill4, GetReader(shipment1, null, logger, Factory).GetExistingBusinessObject());

			AssertEquals(oceanBill2, GetReader(shipment2, null, logger, Factory).GetExistingBusinessObject());

			hvlvShipment.WayBillNumber = "MH3";
			AssertEquals(oceanBill3, GetReader(shipment3, hvlvShipment, logger, Factory).GetExistingBusinessObject());
		}

		public void TestGetExistingBusinessObject_WhenInHVLVProcessAndParentBillIsEmpty_UseTRFLogWithRefNumberToFindExactOceanBill()
		{
			AssertGetExistingBusinessObject_WhenInHVLVProcessAndParentBillIsEmpty_UseTRFLogToFindExactOceanBill(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber);
		}

		public void TestGetExistingBusinessObject_WhenInHVLVProcessAndParentBillIsEmpty_UseTRFLogWithJobNumberToFindExactOceanBill()
		{
			AssertGetExistingBusinessObject_WhenInHVLVProcessAndParentBillIsEmpty_UseTRFLogToFindExactOceanBill(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber);
		}

		void AssertGetExistingBusinessObject_WhenInHVLVProcessAndParentBillIsEmpty_UseTRFLogToFindExactOceanBill(string jobNumberEventReferenceCode)
		{
			var oceanBillWithTRFLog = CreateOceanBillMatchingXML();
			oceanBillWithTRFLog.CB_MasterHouseBill = string.Empty;

			var hvlvForwardingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			hvlvForwardingShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			Factory.SaveForTesting();

			var universalShipment = GetShipment();
			var hvlvUniversalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hvlvUniversalShipment.ShipmentType = new CodeDescriptionPair() { Code = ShipmentTypes.HighVolumeLowValueLegacy };
			hvlvUniversalShipment.DataContext = DataContextFactory.New();
			hvlvUniversalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, hvlvForwardingShipment.JobNumber);

			var logger = new DummyLogger();

			AssertNull("Could not find ocean bill when no TRF log in Ocean Bill and parent bill is empty", GetReader(universalShipment, hvlvUniversalShipment, logger, Factory).GetExistingBusinessObject());

			oceanBillWithTRFLog.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, ShipmentTypes.HighVolumeLowValue),
				new KeyValuePair<string, string>(jobNumberEventReferenceCode, hvlvForwardingShipment.JobNumber)
			});

			var oceanBillWithoutTRFLog = CreateOceanBillMatchingXML();
			oceanBillWithoutTRFLog.CB_MasterHouseBill = string.Empty;

			Factory.SaveForTesting();

			AssertEquals("Find exact ocean bill by TRF Log when parent bill is empty", oceanBillWithTRFLog, GetReader(universalShipment, hvlvUniversalShipment, logger, Factory).GetExistingBusinessObject());
		}

		public void TestPopulateHVLVShipmentConsolidation()
		{
			var logger = new DummyLogger();
			var shipment = ReadXMLIntoShipment(logger, TestFileHelper.GetFileContents("HVLVShipmentConsolidation"));
			BusinessObject targetBO = null;
			MultipleTopLevelObjectReadersWrapper.CreateWrapper(shipment, true, hls => new[] { GetReader(shipment, hls, logger, Factory) }).ReadIntoBusinessObject(ref targetBO);
			Factory.SaveForTesting();
			var oceanBill = targetBO as TCusSCAOceanBill;
			var houseBills = CusSCADataObjectHelper.LoadHouseBills<TCusSCAHouse>(oceanBill, Factory.BOFactory);
			AssertEquals(11, houseBills.Count());
			Assert("CA_IsHVLV should be true for house bill created from HVLV sea cargo", houseBills.All(houseBill => houseBill.CA_IsHVLV));
			var container = CusSCADataObjectHelper.LoadContainers<TCusSCAContainer>(oceanBill, Factory.BOFactory).FirstOrDefault();
			AssertEquals(Core.Constants.ContainerModes.FCL, container.CN_ContainerMode);
		}

		public void TestPopulateOceanBillIgnoringPackingLineWithNoMatchingContainerOrBill()
		{
			var logger = new DummyLogger();
			var shipment = ReadXMLIntoShipment(logger, TestFileHelper.GetFileContents("OceanBillShipmentWithOrphanPacking.xml"));
			BusinessObject targetBO = null;
			GetReader(shipment, null, logger, Factory).ReadIntoBusinessObject(ref targetBO);
			Factory.SaveForTesting();
			var oceanBill = targetBO as TCusSCAOceanBill;
			var house1 = CusSCADataObjectHelper.LoadHouseBills<TCusSCAHouse>(oceanBill, Factory.BOFactory).ElementAt(0);
			var house2 = CusSCADataObjectHelper.LoadHouseBills<TCusSCAHouse>(oceanBill, Factory.BOFactory).ElementAt(1);
			AssertEquals("Pivots.Count", 1, CusSCADataObjectHelper.LoadPackages<TCusSCAPivot>(house1, Factory.BOFactory).Count());
			AssertEquals("Pivots.Count", 0, CusSCADataObjectHelper.LoadPackages<TCusSCAPivot>(house2, Factory.BOFactory).Count());
		}

		public void TestPopulateOceanBill()
		{
			var logger = new DummyLogger();
			var shipment = ReadXMLIntoShipment(logger, OceanBillShipmentXML);
			BusinessObject targetBO = null;
			GetReader(shipment, null, logger, Factory).ReadIntoBusinessObject(ref targetBO);
			Factory.SaveForTesting();
			var oceanBill = targetBO as TCusSCAOceanBill;
			AssertNotNull("Ocean Bill was created", oceanBill);
			AssertAllPropertiesSet(oceanBill);
		}

		public void TestPopulateOceanBill_UppercasesHoueBill()
		{
			var logger = new DummyLogger();
			var xml = TestFileHelper.GetFileContents("OceanBillShipment1");// Don't use overridable XML
			var shipment = ReadXMLIntoShipment(logger, xml);
			BusinessObject targetBO = null;
			GetReader(shipment, null, logger, Factory).ReadIntoBusinessObject(ref targetBO);
			Factory.SaveForTesting();
			var oceanBill = targetBO as TCusSCAOceanBill;
			AssertNotNull("Ocean Bill was created", oceanBill);
			var houseBills = CusSCADataObjectHelper.LoadHouseBills<TCusSCAHouse>(oceanBill, Factory.BOFactory);
			AssertContainsExactElementsInAnyOrder("HouseBills should all be uppercase",
				houseBills.Select(bill => bill.CA_HouseBill.ToUpper()),
				houseBills.Select(bill => bill.CA_HouseBill));
		}

		public void TestUpdateExistingOceanBill()
		{
			var logger = new DummyLogger();
			var shipment = ReadXMLIntoShipment(logger, OceanBillShipmentXML);
			var existingOceanBill = CreateOceanBillMatchingXML();
			BusinessObject targetBO = null;
			GetReader(shipment, null, logger, Factory).ReadIntoBusinessObject(ref targetBO);
			Factory.SaveForTesting();
			var oceanBill = targetBO as TCusSCAOceanBill;
			AssertEquals("Existing Ocean Bill was updated", existingOceanBill, oceanBill);
			AssertAllPropertiesSet(oceanBill);
		}

		protected virtual void AssertHouseBill1AllPropertiesSet(TCusSCAHouse houseBill)
		{
			AssertEquals("CA_IsGSTPrePaid", "Y", houseBill.CA_IsGSTPrePaid);
			AssertEquals("CA_VendorIdentifier", "1234567", houseBill.CA_VendorIdentifier);
			AssertEquals("CA_RL_NK_PortOfDestination", "AUSYD", houseBill.CA_RL_NK_PortOfDestination);
			AssertEquals("CA_HouseBill", "1", houseBill.CA_HouseBill);
			AssertEquals("CA_RN_NKGoodsOrigin", "ES", houseBill.CA_RN_NKGoodsOrigin);
			AssertEquals("CA_RL_NK_PortOfOrigin", "USCHI", houseBill.CA_RL_NK_PortOfOrigin);
			AssertEquals("CA_RL_NKLoadPort", "NZABY", houseBill.CA_RL_NKLoadPort);
			AssertEquals("CA_RL_NKDischargePort", "BA4CA", houseBill.CA_RL_NKDischargePort);
			AssertEquals("CA_GoodsValue", 110.11m, houseBill.CA_GoodsValue);
			AssertEquals("CA_RX_NKGoodsCurrency", "USD", houseBill.CA_RX_NKGoodsCurrency);
			AssertEquals("GoodsLocation", goodsLocationOrgAddress1, houseBill.GoodsLocation);
		}

		protected virtual void AssertHouseBill2AllPropertiesSet(TCusSCAHouse houseBill)
		{
			AssertEquals("CA_IsGSTPrePaid", "N", houseBill.CA_IsGSTPrePaid);
			AssertEquals("CA_VendorIdentifier", "2345678", houseBill.CA_VendorIdentifier);
			AssertEquals("CA_RL_NK_PortOfDestination", "AUSYD", houseBill.CA_RL_NK_PortOfDestination);
			AssertEquals("CA_HouseBill", "2", houseBill.CA_HouseBill);
			AssertEquals("CA_RN_NKGoodsOrigin", "AD", houseBill.CA_RN_NKGoodsOrigin);
			AssertEquals("CA_RL_NK_PortOfOrigin", "AIBLP", houseBill.CA_RL_NK_PortOfOrigin);
			AssertEquals("CA_RL_NKLoadPort", "CA2KS", houseBill.CA_RL_NKLoadPort);
			AssertEquals("CA_RL_NKDischargePort", "FIAAI", houseBill.CA_RL_NKDischargePort);
			AssertEquals("CA_GoodsValue", 220.22m, houseBill.CA_GoodsValue);
			AssertEquals("CA_RX_NKGoodsCurrency", "AUD", houseBill.CA_RX_NKGoodsCurrency);
			AssertEquals("GoodsLocation", goodsLocationOrgAddress2, houseBill.GoodsLocation);
		}

		protected virtual void AssertContainer1AllPropertiesSet(TCusSCAContainer container)
		{
			AssertEquals("CN_ContainerNumber", "APLU1234", container.CN_ContainerNumber);
			AssertEquals("CN_RC_NKContainerType", "REFC", container.CN_RC_NKContainerType);
			AssertEquals("CN_ContainerMode", "AIR", container.CN_ContainerMode);
			AssertEquals("CN_SealNumber", "SEAL1", container.CN_SealNumber);
			AssertEquals("PackLocation", packLocationOrgAddress1, container.PackLocation);
		}

		protected virtual void AssertContainer2AllPropertiesSet(TCusSCAContainer container)
		{
			AssertEquals("CN_ContainerNumber", "APLU4321", container.CN_ContainerNumber);
			AssertEquals("CN_ContainerMode", "AIR", container.CN_ContainerMode);
			AssertEquals("CN_SealNumber", "SEAL2", container.CN_SealNumber);
			AssertEquals("PackLocation", packLocationOrgAddress2, container.PackLocation);
		}

		protected virtual void AssertPivot1AllPropertiesSet(TCusSCAPivot pivot)
		{
			AssertEquals("CV_PackageCount", 1, pivot.CV_PackageCount);
			AssertEquals("CV_Weight", 2m, pivot.CV_Weight);
			AssertEquals("CV_WeightUQ", Core.Constants.Weight.Kilograms, pivot.CV_WeightUQ);
			AssertEquals("CV_GoodsDescription", "GOODS DESCRIPTION 1", pivot.CV_GoodsDescription);
			AssertEquals("CV_MarksAndNumbers", "MARKS AND NUMBERS 1", pivot.CV_MarksAndNumbers);
			AssertEquals("CV_GoodsValue", 10m, pivot.CV_GoodsValue);
			AssertEquals("CV_RX_NKGoodsCurrency", "USD", pivot.CV_RX_NKGoodsCurrency);
		}

		protected virtual void AssertPivot2AllPropertiesSet(TCusSCAPivot pivot)
		{
			AssertEquals("CV_PackageCount", 10, pivot.CV_PackageCount);
			AssertEquals("CV_Weight", 20m, pivot.CV_Weight);
			AssertEquals("CV_WeightUQ", Core.Constants.Weight.Kilograms, pivot.CV_WeightUQ);
			AssertEquals("CV_GoodsDescription", "GOODS DESCRIPTION 2", pivot.CV_GoodsDescription);
			AssertEquals("CV_MarksAndNumbers", "MARKS AND NUMBERS 2", pivot.CV_MarksAndNumbers);
			AssertEquals("CV_GoodsValue", 20m, pivot.CV_GoodsValue);
			AssertEquals("CV_RX_NKGoodsCurrency", "AUD", pivot.CV_RX_NKGoodsCurrency);
		}

		protected virtual string OceanBillShipmentXML => TestFileHelper.GetFileContents("OceanBillShipment1");

		protected abstract ITopLevelDataObjectReader GetReader(Shipment dataObject, Shipment hvlv, IXmlImportLogger logger, UniversalObjectFactory factory);

		protected abstract ZString ApplicationCodeMatchingXML { get; }

		protected TCusSCAOceanBill CreateOceanBillMatchingXML()
		{
			var result = Factory.New<TCusSCAOceanBill>();
			result.CB_OceanBill = "TEST OCEAN BILL";
			result.CB_ApplicationCode = ApplicationCodeMatchingXML;
			result.CB_LloydsIMO = "9143245";
			result.CB_Voyage = "VGE N1";
			result.CB_MasterHouseBill = "PARENT BILL";
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			CreateShippingLine();
			CreatePackLocations();
			CreateGoodsLocations();

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "REFC";
			refContainer.RC_Description = "A REFRENCE CONTAINER";
			refContainer.RC_ISOType = "ISO";
		}

		void CreateShippingLine()
		{
			shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_FullName = "CargoWise";

			var shippingLineMainAddress = shippingLine.MainAddress;
			shippingLineMainAddress.OA_Address1 = "Unit 1";
			shippingLineMainAddress.OA_Address2 = "23 Test Street";
			shippingLineMainAddress.OA_City = "Sydney";
			shippingLineMainAddress.OA_State = "NSW";
			shippingLineMainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			shippingLineMainAddress.OA_PostCode = "2049";
			shippingLineMainAddress.OA_Phone = "0212345678";
			shippingLineMainAddress.OA_Fax = "0212345679";
			shippingLineMainAddress.OA_Email = "test.email@cargowise.com";
		}

		void CreatePackLocations()
		{
			packLocationOrgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			packLocationOrgHeader1.OH_Code = "CPK1";
			packLocationOrgHeader1.OH_FullName = "Pack Line OrgHeader 1";

			packLocationOrgAddress1 = packLocationOrgHeader1.MainAddress;
			packLocationOrgAddress1.OA_Code = "Pack Location 1";
			packLocationOrgAddress1.OA_Address1 = "Pack Location 1";

			packLocationOrgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			packLocationOrgHeader2.OH_Code = "CPK2";
			packLocationOrgHeader2.OH_FullName = "Pack Line OrgHeader 2";

			packLocationOrgAddress2 = packLocationOrgHeader2.MainAddress;
			packLocationOrgAddress2.OA_Code = "Pack Location 2";
			packLocationOrgAddress2.OA_Address1 = "Pack Location 2";
		}

		void CreateGoodsLocations()
		{
			goodsLocationOrgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			goodsLocationOrgHeader1.OH_Code = "GLC1";
			goodsLocationOrgHeader1.OH_FullName = "Goods Location OrgHeader 1";

			goodsLocationOrgAddress1 = goodsLocationOrgHeader1.MainAddress;
			goodsLocationOrgAddress1.OA_Code = "Goods Location 1";
			goodsLocationOrgAddress1.OA_Address1 = "Goods Location 1";

			goodsLocationOrgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			goodsLocationOrgHeader2.OH_Code = "GLC2";
			goodsLocationOrgHeader2.OH_FullName = "Goods Location OrgHeader 2";

			goodsLocationOrgAddress2 = goodsLocationOrgHeader2.MainAddress;
			goodsLocationOrgAddress2.OA_Code = "Goods Location 2";
			goodsLocationOrgAddress2.OA_Address1 = "Goods Location 2";

			goodsLocationOrgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			goodsLocationOrgHeader3.OH_Code = "GLC3";
			goodsLocationOrgHeader3.OH_FullName = "Goods Location OrgHeader 3";

			goodsLocationOrgAddress3 = goodsLocationOrgHeader3.MainAddress;
			goodsLocationOrgAddress3.OA_Code = "Goods Location 3";
			goodsLocationOrgAddress3.OA_Address1 = "Goods Location 3";
		}

		void AssertAllPropertiesSet(TCusSCAOceanBill oceanBill)
		{
			AssertEquals("Branch.GB_Code", "BNE", oceanBill.Branch.GB_Code);
			AssertEquals("CB_LloydsIMO", ApplicationCodeMatchingXML, oceanBill.CB_ApplicationCode);
			AssertEquals("CB_LloydsIMO", "9143245", oceanBill.CB_LloydsIMO);
			AssertEquals("CB_RL_NKPortOfDischarge", "AUSYD", oceanBill.CB_RL_NKPortOfDischarge);
			AssertEquals("CB_RL_NKPortOfFirstArrival", "AUMEL", oceanBill.CB_RL_NKPortOfFirstArrival);
			AssertEquals("CB_RL_NKPortOfLoading", "NZCHC", oceanBill.CB_RL_NKPortOfLoading);
			AssertEquals("CB_VesselName", "ADELAIDE EXPRESS", oceanBill.CB_VesselName);
			AssertEquals("CB_Voyage", "VGE N1", oceanBill.CB_Voyage);
			AssertEquals("CB_OceanBill", "TEST OCEAN BILL", oceanBill.CB_OceanBill);
			AssertEquals("CB_MasterHouseBill", "PARENT BILL", oceanBill.CB_MasterHouseBill);
			AssertEquals("CB_ResponsiblePartyID", "29002589460", oceanBill.CB_ResponsiblePartyID);
			AssertEquals("CB_PrincipalID", "41065894724", oceanBill.CB_PrincipalID);
			AssertEquals("CB_DateOfDeparture", new ZDateTime(2013, 8, 26), oceanBill.CB_DateOfDeparture);
			AssertEquals("CB_DateOfFirstArrival", new ZDateTime(2013, 8, 27), oceanBill.CB_DateOfFirstArrival);
			AssertEquals("CB_DateOfArrival", new ZDateTime(2013, 8, 28), oceanBill.CB_DateOfArrival);
			AssertEquals("ShippingLine", shippingLine, oceanBill.ShippingLine);
			AssertEquals("GoodsLocation", goodsLocationOrgAddress3, oceanBill.GoodsLocation);

			var houseBills = CusSCADataObjectHelper.LoadHouseBills<TCusSCAHouse>(oceanBill, Factory.BOFactory).OrderBy(x => x.CA_HouseBill);
			AssertEquals("HouseBills.Count", 2, houseBills.Count());
			var house1 = houseBills.ElementAt(0);
			var house2 = houseBills.ElementAt(1);
			AssertHouseBill1AllPropertiesSet(house1);
			AssertHouseBill2AllPropertiesSet(house2);
			var pivot1 = CusSCADataObjectHelper.LoadPackages<TCusSCAPivot>(house1, Factory.BOFactory).FirstOrDefault();
			AssertNotNull(pivot1);
			AssertPivot1AllPropertiesSet(pivot1);
			var pivot2 = CusSCADataObjectHelper.LoadPackages<TCusSCAPivot>(house2, Factory.BOFactory).FirstOrDefault();
			AssertNotNull(pivot2);
			AssertPivot2AllPropertiesSet(pivot2);
			var containers = CusSCADataObjectHelper.LoadContainers<TCusSCAContainer>(oceanBill, Factory.BOFactory);
			AssertEquals("Containers.Count", 2, containers.Count());
			var container1 = containers.FirstOrDefault(x => x.PK == pivot1.CV_CN);
			AssertNotNull(container1);
			AssertContainer1AllPropertiesSet(container1);
			var container2 = containers.FirstOrDefault(x => x.PK == pivot2.CV_CN);
			AssertNotNull(container2);
			AssertContainer2AllPropertiesSet(container2);
		}

		Shipment GetShipment()
		{
			var result = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			result.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master };
			result.WayBillNumber = "TEST OCEAN BILL";
			result.LloydsIMO = "9143245";
			result.VoyageFlightNo = "VGE N1";
			return result;
		}

		OrgHeader shippingLine;

		OrgHeader packLocationOrgHeader1;

		OrgHeader packLocationOrgHeader2;

		OrgAddress packLocationOrgAddress1;

		OrgAddress packLocationOrgAddress2;

		OrgHeader goodsLocationOrgHeader1;

		OrgHeader goodsLocationOrgHeader2;

		OrgHeader goodsLocationOrgHeader3;

		OrgAddress goodsLocationOrgAddress1;

		OrgAddress goodsLocationOrgAddress2;

		OrgAddress goodsLocationOrgAddress3;

		protected override void TearDown()
		{
			base.TearDown();
			testFileHelper?.Dispose();
			testFileHelper = null;
		}

		protected TestFileHelper TestFileHelper => testFileHelper ??= new ();
		TestFileHelper testFileHelper;
	}
}
