using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest.Testing
{
	[TestsSubclassesOf(typeof(CusSCAOceanBillDataObjectWriter<,,,>))]
	public abstract class CusSCAOceanBillDataObjectWriterTest<TCusSCAOceanBill, TCusSCAHouse, TCusSCAContainer, TCusSCAPivot> : TestCaseWithFactory
			where TCusSCAOceanBill : BaseCusSCAOceanBill
			where TCusSCAHouse : BaseCusSCAHouse
			where TCusSCAContainer : BaseCusSCAContainer
			where TCusSCAPivot : BaseCusSCAPivot
	{
		public void TestEndToEnd()
		{
			using (var stream = (SubStreamableStream)new MemoryStream())
			using (var streamReader = new StreamReader(stream))
			{
				var writer = GetWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.HSA, OceanBill)));
				ObjectFactory.Get<IXmlWriter>().WriteXML(writer.GetDataObject(OceanBill), stream);
				stream.Position = 0;
				AssertMultilineASCIIEquals("XML is as expected", OceanBillShipmentXML, streamReader.ReadToEnd());
			}
		}

		protected abstract ITopLevelDataObjectWriter GetWriter(IDataWritingManager writeManager);

		protected abstract string OceanBillShipmentXML { get; }

		protected virtual TCusSCAOceanBill GetNewOceanBill() => Factory.New<TCusSCAOceanBill>();

		protected virtual TCusSCAHouse GetNewHouseBill() => Factory.New<TCusSCAHouse>();

		protected virtual TCusSCAContainer GetNewContainer() => Factory.New<TCusSCAContainer>();

		protected virtual TCusSCAPivot GetNewPivot() => Factory.New<TCusSCAPivot>();

		protected TCusSCAOceanBill OceanBill { get; private set; }

		protected virtual TCusSCAOceanBill CreateOceanBill()
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_FullName = "CargoWise";

			var goodsLocation = Factory.NewWithValidTestData<OrgAddress>();
			goodsLocation.OA_Code = "GLC";
			goodsLocation.OA_Address1 = "Goods Location";

			var shippingLineMainAddress = shippingLine.MainAddress;
			shippingLineMainAddress.OA_Address1 = "Unit 1";
			shippingLineMainAddress.OA_Address2 = "23 Test Street";
			shippingLineMainAddress.OA_City = "Sydney";
			shippingLineMainAddress.OA_State = "NSW";
			shippingLineMainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			shippingLineMainAddress.OA_PostCode = "2049";
			shippingLineMainAddress.OA_Phone = "+61212345678";
			shippingLineMainAddress.OA_Fax = "0212345679";
			shippingLineMainAddress.OA_Email = "test.email@cargowise.com";

			var oceanBill = GetNewOceanBill();
			oceanBill.CB_LloydsIMO = "9143245";
			oceanBill.CB_MasterHouseBill = "PARENT BILL";
			oceanBill.CB_OceanBill = "TEST OCEAN BILL";
			oceanBill.CB_OH_ShippingLine = shippingLine.PK;
			oceanBill.CB_OA_GoodsLocation = goodsLocation.PK;
			oceanBill.CB_PrincipalID = "41065894724";
			oceanBill.CB_ResponsiblePartyID = "29002589460";
			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
			oceanBill.CB_RL_NKPortOfFirstArrival = "AUMEL";
			oceanBill.CB_RL_NKPortOfLoading = "NZCHC";
			oceanBill.CB_VesselName = "ADELAIDE EXPRESS";
			oceanBill.CB_Voyage = "VGE N1";
			oceanBill.CB_DateOfArrival = new ZDateTime(2013, 08, 28);
			oceanBill.CB_DateOfFirstArrival = new ZDateTime(2013, 08, 27);
			oceanBill.CB_MessageReference = "X00003366";

			oceanBill.Notes.AddNew(true, "I am the note description.", "I am the note text.");

			return oceanBill;
		}

		protected virtual TCusSCAHouse AddHouseBill1(TCusSCAOceanBill oceanBill)
		{
			var house1Consignee = Factory.New<OrgHeader>();
			house1Consignee.OH_FullName = "House 1 Consignee";
			var house1ConsigneeMainAddress = house1Consignee.MainAddress;
			house1ConsigneeMainAddress.OA_Address1 = "Unit 2";
			house1ConsigneeMainAddress.OA_Address2 = "35 Test Street";
			house1ConsigneeMainAddress.OA_City = "Melbourne";
			house1ConsigneeMainAddress.OA_State = "VIC";
			house1ConsigneeMainAddress.OA_RL_NKRelatedPortCode = "AUMEL";
			house1ConsigneeMainAddress.OA_PostCode = "3001";
			house1ConsigneeMainAddress.OA_Phone = "+61312345678";
			house1ConsigneeMainAddress.OA_Fax = "0312345679";
			house1ConsigneeMainAddress.OA_Email = "house1.consignee@cargowise.com";

			var house1Consignor = Factory.New<OrgHeader>();
			house1Consignor.OH_FullName = "House 1 Consignor";
			var house1ConsignorMainAddress = house1Consignor.MainAddress;
			house1ConsignorMainAddress.OA_Address1 = "Unit 3";
			house1ConsignorMainAddress.OA_Address2 = "38 Test Street";
			house1ConsignorMainAddress.OA_City = "Melbourne";
			house1ConsignorMainAddress.OA_State = "VIC";
			house1ConsignorMainAddress.OA_RL_NKRelatedPortCode = "AUMEL";
			house1ConsignorMainAddress.OA_PostCode = "3002";
			house1ConsignorMainAddress.OA_Phone = "+61352345679";
			house1ConsignorMainAddress.OA_Fax = "0352345670";
			house1ConsignorMainAddress.OA_Email = "house1.consignor@cargowise.com";

			var house1Notify = Factory.New<OrgHeader>();
			house1Notify.OH_FullName = "House 1 Notify";
			var house1NotifyMainAddress = house1Notify.MainAddress;
			house1NotifyMainAddress.OA_Address1 = "Unit 4";
			house1NotifyMainAddress.OA_Address2 = "234 Test Ave";
			house1NotifyMainAddress.OA_City = "Gold Coast";
			house1NotifyMainAddress.OA_State = "VIC";
			house1NotifyMainAddress.OA_RL_NKRelatedPortCode = "AUGOC";
			house1NotifyMainAddress.OA_PostCode = "4217";
			house1NotifyMainAddress.OA_Phone = "+61716349871";
			house1NotifyMainAddress.OA_Fax = "0716349872";
			house1NotifyMainAddress.OA_Email = "house1.notify@cargowise.com";

			var house1GoodsLocation = Factory.New<OrgHeader>();
			house1GoodsLocation.OH_FullName = "House 1 Goods Location";
			var house1GoodsLocationMainAddress = house1GoodsLocation.MainAddress;
			house1GoodsLocationMainAddress.OA_Address1 = "Unit 5";
			house1GoodsLocationMainAddress.OA_Address2 = "40 Test Street";
			house1GoodsLocationMainAddress.OA_City = "Melbourne";
			house1GoodsLocationMainAddress.OA_State = "VIC";
			house1GoodsLocationMainAddress.OA_RL_NKRelatedPortCode = "AUMEL";
			house1GoodsLocationMainAddress.OA_PostCode = "3005";
			house1GoodsLocationMainAddress.OA_Phone = "+61352345680";
			house1GoodsLocationMainAddress.OA_Fax = "0352345674";
			house1GoodsLocationMainAddress.OA_Email = "house5.consignor@cargowise.com";

			var houseBill1 = GetNewHouseBill();
			houseBill1.CA_CB = oceanBill.PK;
			houseBill1.CA_OH_Consignee = house1Consignee.PK;
			houseBill1.CA_OH_Consignor = house1Consignor.PK;
			houseBill1.CA_OH_Notify = house1Notify.PK;
			houseBill1.CA_OA_GoodsLocation = house1GoodsLocationMainAddress.PK;
			houseBill1.CA_HouseBill = "1";
			houseBill1.CA_IsMasterHouse = true;
			houseBill1.CA_MasterHouseBill = "ABC";
			houseBill1.CA_MessageStatus = "NOT";
			houseBill1.CA_RL_NK_PortOfDestination = "AUSYD";
			houseBill1.CA_RL_NK_PortOfOrigin = "AUSYD";
			houseBill1.CA_RL_NKLoadPort = "AUSYD";
			houseBill1.CA_RL_NKDischargePort = "AUSYD";
			houseBill1.CA_ShipmentStatus = "NOT";
			houseBill1.CA_GoodsValue = 100m;
			houseBill1.CA_RX_NKGoodsCurrency = "USD";
			houseBill1.CA_RN_NKGoodsOrigin = "AU";
			houseBill1.CA_VendorIdentifier = "1234567";
			houseBill1.CA_IsGSTPrePaid = "N";

			return houseBill1;
		}

		protected virtual TCusSCAHouse AddHouseBill2(TCusSCAOceanBill oceanBill)
		{
			var house2Consignee = Factory.New<OrgHeader>();
			house2Consignee.OH_FullName = "House 2 Consignee";
			var house2ConsigneeMainAddress = house2Consignee.MainAddress;
			house2ConsigneeMainAddress.OA_Address1 = "Unit 22";
			house2ConsigneeMainAddress.OA_Address2 = "352 Test Street";
			house2ConsigneeMainAddress.OA_City = "Melbourne";
			house2ConsigneeMainAddress.OA_State = "VIC";
			house2ConsigneeMainAddress.OA_RL_NKRelatedPortCode = "AUMEL";
			house2ConsigneeMainAddress.OA_PostCode = "3021";
			house2ConsigneeMainAddress.OA_Phone = "+61322245678";
			house2ConsigneeMainAddress.OA_Fax = "0322245679";
			house2ConsigneeMainAddress.OA_Email = "house2.consignee@cargowise.com";

			var house2Consignor = Factory.New<OrgHeader>();
			house2Consignor.OH_FullName = "House 2 Consignor";
			var house2ConsignorMainAddress = house2Consignor.MainAddress;
			house2ConsignorMainAddress.OA_Address1 = "Unit 32";
			house2ConsignorMainAddress.OA_Address2 = "382 Test Street";
			house2ConsignorMainAddress.OA_City = "Melbourne";
			house2ConsignorMainAddress.OA_State = "VIC";
			house2ConsignorMainAddress.OA_RL_NKRelatedPortCode = "AUMEL";
			house2ConsignorMainAddress.OA_PostCode = "3022";
			house2ConsignorMainAddress.OA_Phone = "+61322245679";
			house2ConsignorMainAddress.OA_Fax = "0322245670";
			house2ConsignorMainAddress.OA_Email = "house2.consignor@cargowise.com";

			var house2GoodsLocation = Factory.New<OrgHeader>();
			house2GoodsLocation.OH_FullName = "House 1 Goods Location";
			var house2GoodsLocationMainAddress = house2GoodsLocation.MainAddress;
			house2GoodsLocationMainAddress.OA_Address1 = "Unit 5";
			house2GoodsLocationMainAddress.OA_Address2 = "340 Test Street";
			house2GoodsLocationMainAddress.OA_City = "Melbourne";
			house2GoodsLocationMainAddress.OA_State = "VIC";
			house2GoodsLocationMainAddress.OA_RL_NKRelatedPortCode = "AUMEL";
			house2GoodsLocationMainAddress.OA_PostCode = "3025";
			house2GoodsLocationMainAddress.OA_Phone = "+61352345688";
			house2GoodsLocationMainAddress.OA_Fax = "0352345676";
			house2GoodsLocationMainAddress.OA_Email = "house5.consignor@cargowise.com";

			var houseBill2 = GetNewHouseBill();
			houseBill2.CA_CB = oceanBill.PK;
			houseBill2.CA_OH_Consignee = house2Consignee.PK;
			houseBill2.CA_OH_Consignor = house2Consignor.PK;
			houseBill2.CA_OA_GoodsLocation = house2GoodsLocationMainAddress.PK;
			houseBill2.CA_HouseBill = "2";
			houseBill2.CA_IsMasterHouse = false;
			houseBill2.CA_MessageStatus = "NOT";
			houseBill2.CA_RL_NK_PortOfDestination = "AUSYD";
			houseBill2.CA_RL_NK_PortOfOrigin = "AUSYD";
			houseBill2.CA_RL_NKLoadPort = "AUSYD";
			houseBill2.CA_RL_NKDischargePort = "AUSYD";
			houseBill2.CA_ShipmentStatus = "NOT";
			houseBill2.CA_GoodsValue = 200m;
			houseBill2.CA_RX_NKGoodsCurrency = "USD";
			houseBill2.CA_RN_NKGoodsOrigin = "AU";
			houseBill2.CA_VendorIdentifier = "2345678";
			houseBill2.CA_IsGSTPrePaid = "Y";

			return houseBill2;
		}

		protected virtual TCusSCAContainer AddContainer1(TCusSCAOceanBill oceanBill)
		{
			var packLocation = Factory.NewWithValidTestData<OrgAddress>();
			packLocation.OA_Code = "PLC1";
			packLocation.OA_Address1 = "Pack Location 1";

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "REFC";
			refContainer.RC_Description = "A REFRENCE CONTAINER";
			refContainer.RC_ISOType = "ISO";

			var container = GetNewContainer();
			container.CN_CB = oceanBill.PK;
			container.CN_ContainerNumber = "APLU1234";
			container.CN_RC_NKContainerType = "REFC";
			container.CN_ContainerMode = "AIR";
			container.CN_SealNumber = "M123";
			container.CN_OA_PackLocation = packLocation.PK;
			return container;
		}

		protected virtual TCusSCAContainer AddContainer2(TCusSCAOceanBill oceanBill)
		{
			var packLocation = Factory.NewWithValidTestData<OrgAddress>();
			packLocation.OA_Code = "PLC2";
			packLocation.OA_Address1 = "Pack Location 2";

			var result = GetNewContainer();
			result.CN_CB = oceanBill.PK;
			result.CN_ContainerMode = "AIR";
			result.CN_ContainerNumber = "APLU4321";
			result.CN_SealNumber = "C123";
			result.CN_OA_PackLocation = packLocation.PK;
			return result;
		}

		protected virtual TCusSCAPivot AddPackage1(TCusSCAContainer container, TCusSCAHouse bill)
		{
			var result = GetNewPivot();
			result.CV_LineNo = 1;
			result.CV_CA = bill.PK;
			result.CV_CN = container.PK;
			result.CV_PackageCount = 1;
			result.CV_HarmonisedTariffNums = "123.456.789";
			result.CV_PackageType = "BG";
			result.CV_Weight = 2m;
			result.CV_WeightUQ = "KG";
			result.CV_Volume = 4m;
			result.CV_GoodsDescription = "GOODS DESCRIPTION 1";
			result.CV_MarksAndNumbers = "MARKS AND NUMBERS 1";
			result.CV_GoodsValue = 10m;
			result.CV_RX_NKGoodsCurrency = "USD";
			return result;
		}

		protected virtual TCusSCAPivot AddPackage2(TCusSCAContainer container, TCusSCAHouse bill)
		{
			var result = GetNewPivot();
			result.CV_LineNo = 2;
			result.CV_CA = bill.PK;
			result.CV_CN = container.PK;
			result.CV_PackageCount = 10;
			result.CV_HarmonisedTariffNums = "234.567.890";
			result.CV_PackageType = "BG";
			result.CV_Weight = 20m;
			result.CV_WeightUQ = "KG";
			result.CV_Volume = 40m;
			result.CV_GoodsDescription = "GOODS DESCRIPTION 2";
			result.CV_MarksAndNumbers = "MARKS AND NUMBERS 2";
			result.CV_GoodsValue = 20m;
			result.CV_RX_NKGoodsCurrency = "USD";
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			OceanBill = CreateOceanBill();
			var house1 = AddHouseBill1(OceanBill);
			var house2 = AddHouseBill2(OceanBill);
			var container1 = AddContainer1(OceanBill);
			var container2 = AddContainer2(OceanBill);
			AddPackage1(container1, house1);
			AddPackage2(container2, house2);

			Factory.Save();
		}

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
