using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Core;
using Enterprise.eManifest.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eManifest.DataTransfer.Testing
{
	sealed class SupplierBookingWriteTest : TestCaseWithFactory
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCanWriteSupplierBookingHeaderWithoutShipment()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "QUICK STUFF CALIFORNIA";
			consignor.OH_RL_NKClosestPort = "USLAX";
			consignor.OH_Code = "QUISTULAX";

			var consignorAddress = consignor.MainAddress;
			consignorAddress.OA_Address1 = "545 TRANSIENT WAY";
			consignorAddress.OA_Address2 = "(OFFICE AT RIGHT SIDE OF BUILDING)";
			consignorAddress.OA_City = "LOS ANGELES PARK";
			consignorAddress.OA_State = "CA";
			consignorAddress.OA_PostCode = "90437";
			consignorAddress.OA_Phone = "0011 1 328 298 3109";
			consignorAddress.OA_Fax = "0011 1 328 299 3289";
			consignorAddress.OA_Email = "quickstuff@california.com";

			var consignorContact = consignor.Contacts.AddNew();
			consignorContact.OC_ContactName = "John Muller";

			var dispatchOrg = Factory.New<OrgHeader>();
			dispatchOrg.OH_FullName = "DISPATCH ORG";
			dispatchOrg.OH_RL_NKClosestPort = "USLAX";
			dispatchOrg.OH_Code = "DISPATLAX";

			var dispatchAddress = dispatchOrg.MainAddress;
			dispatchAddress.OA_Address1 = "7 BECKS ST";
			dispatchAddress.OA_City = "LOS ANGELES";
			dispatchAddress.OA_State = "CA";
			dispatchAddress.OA_PostCode = "90438";
			dispatchAddress.OA_Phone = "0011 1 709 394 5354";
			dispatchAddress.OA_Email = "abc@dispatch.com";

			var dispatchContact = dispatchOrg.Contacts.AddNew();
			dispatchContact.OC_ContactName = "David";

			var header = Factory.New<SupplierBookingHeader>();
			header.DH_OA_Consignor = consignorAddress.PK;
			header.DH_OC_ConsignorContact = consignorContact.PK;
			header.DH_OA_DispatchAddress = dispatchAddress.PK;
			header.DH_GrossWeightInKg = 120;
			header.DH_CubicInM3 = 3.2;
			header.DH_PiecesManifested = 22;
			header.DH_SupplierReference = "AHHGOTSUNSHINE";

			#region Line # 1-1

			var line11 = header.BookingLines.AddNew();
			line11.DL_ConsigneeName = "FIGHTERS OF FOO";
			line11.DL_ConsigneeAddress1 = "123 FIGHTING ALLEY";
			line11.DL_ConsigneeAddress2 = "";
			line11.DL_ConsigneeCity = "FOOVILLE";
			line11.DL_ConsigneePostCode = "2010";
			line11.DL_ConsigneeState = "NSW";
			line11.DL_RN_NKConsigneeCountryCode = "AU";
			line11.DL_ConsigneePhone = "02 9009 9009";
			line11.DL_ConsigneeContact = "FOO-FIGHT MIGHTY";

			line11.DL_ConsignorName = "RANDOM C";
			line11.DL_ConsignorAddress1 = "5 WHATEVER ROAD";
			line11.DL_ConsignorAddress2 = "";
			line11.DL_ConsignorCity = "LOS ANGELES";
			line11.DL_ConsignorPostCode = "90438";
			line11.DL_ConsignorState = "CA";
			line11.DL_RN_NKConsignorCountryCode = "US";
			line11.DL_ConsignorPhone = "0011 1 328 709 5167";
			line11.DL_ConsignorContact = "ABC";

			line11.DL_ConsigneeReference = "ABERGLOVES#1";
			line11.DL_PiecesManifested = 2;
			line11.DL_F3_NKPackType = "PKG";
			line11.DL_GrossWeight = 1.23;
			line11.DL_GrossWeightUQ = "KG";
			line11.DL_Cubic = 0.02;
			line11.DL_CubicUQ = "M3";
			line11.DL_GoodsDescription = "GLOVES";
			line11.DL_GoodsValue = 120.44;
			line11.DL_RX_NKGoodsValueCurrency = "AUD";
			line11.DL_MarksAndNumbers = "BASHED CRAB ENTERPRISES";
			line11.DL_OrderTrackingNumber = "Ref_1123456789012345678901234567890";
			line11.DL_IsDeliveryTransportSelfBooked = true;
			line11.DL_SignatureRequired = true;
			line11.DL_VendorIdentifier = "SELLINGCRAPCO";

			#endregion

			#region Line # 1-2

			var line12 = header.BookingLines.AddNew();
			line12.DL_ConsigneeName = "FLOGGERS FOR FEE";
			line12.DL_ConsigneeAddress1 = "UNIT 12/43";
			line12.DL_ConsigneeAddress2 = "312 BILLING PLACE";
			line12.DL_ConsigneeCity = "FEEVILLE";
			line12.DL_ConsigneePostCode = "2011";
			line12.DL_ConsigneeState = "TAS";
			line12.DL_RN_NKConsigneeCountryCode = "AU";
			line12.DL_ConsigneePhone = "07 6009 6009";
			line12.DL_ConsigneeContact = "FEE-FLOG MONEY";

			line12.DL_ConsignorName = "FLAPPY";
			line12.DL_ConsignorAddress1 = "4 MANU ST";
			line12.DL_ConsignorAddress2 = "";
			line12.DL_ConsignorCity = "LOS ANGELES";
			line12.DL_ConsignorPostCode = "90436";
			line12.DL_ConsignorState = "CA";
			line12.DL_RN_NKConsignorCountryCode = "US";
			line12.DL_ConsignorPhone = "0011 1 520 417 1016";
			line12.DL_ConsignorContact = "ZJ";

			line12.DL_ConsigneeReference = "ABERMITS#2";
			line12.DL_PiecesManifested = 4;
			line12.DL_F3_NKPackType = "BAG";
			line12.DL_GrossWeight = 2.45;
			line12.DL_GrossWeightUQ = "KG";
			line12.DL_Cubic = 0.03;
			line12.DL_CubicUQ = "M3";
			line12.DL_GoodsDescription = "MITS";
			line12.DL_GoodsValue = 103.55;
			line12.DL_RX_NKGoodsValueCurrency = "AUD";
			line12.DL_MarksAndNumbers = "A CUNNING STUNT INDEED";
			line12.DL_IsDeliveryTransportSelfBooked = true;
			line12.DL_SignatureRequired = true;
			line12.DL_IsHazardous = true;
			line12.DL_IsTimber = true;
			line12.DL_VendorIdentifier = "VENDORSHMENDOR";

			#endregion

			#region Line # 1-3

			var line13 = header.BookingLines.AddNew();

			line13.DL_ConsigneeReference = "ABERMITS#3";
			line13.DL_PiecesManifested = 4;
			line13.DL_F3_NKPackType = "UNT";
			line13.DL_GrossWeight = 2.45;
			line13.DL_GrossWeightUQ = "KG";
			line13.DL_Cubic = 0.03;
			line13.DL_CubicUQ = "M3";
			line13.DL_GoodsDescription = "MITS";
			line13.DL_GoodsValue = 103.55;
			line13.DL_RX_NKGoodsValueCurrency = "AUD";
			line13.DL_MarksAndNumbers = "A CUNNING STUNT INDEED";
			line13.DL_SignatureRequired = false;
			line13.DL_RequiresFumigation = true;
			line13.DL_IsPerishable = true;

			var shpB = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shpB[JobShipmentSchema.JS_UniqueConsignRef] = "S0000000B";
			shpB[JobShipmentSchema.JS_ShipmentType] = Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			line13.DL_JS_ApprovedShipment = shpB.PK;

			#endregion

			#region Line # 1-4

			var line14 = header.BookingLines.AddNew();

			line14.DL_ConsigneeReference = "ABERMITS#4";
			line14.DL_PiecesManifested = 4;
			line14.DL_GrossWeight = 2.45;
			line14.DL_GrossWeightUQ = "KG";
			line14.DL_Cubic = 0.03;
			line14.DL_CubicUQ = "M3";
			line14.DL_GoodsDescription = "MITS";
			line14.DL_GoodsValue = 103.55;
			line14.DL_RX_NKGoodsValueCurrency = "AUD";
			line14.DL_MarksAndNumbers = "A CUNNING STUNT INDEED";
			line14.DL_IsPersonalEffects = true;

			var shpC = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shpC[JobShipmentSchema.JS_UniqueConsignRef] = "S0000000C";
			shpC[JobShipmentSchema.JS_ShipmentType] = Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			shpC[JobShipmentSchema.JS_RL_NKOrigin] = "USLAX";
			shpC[JobShipmentSchema.JS_RL_NKDestination] = "AUSYD";
			line14.DL_JS_ApprovedShipment = shpC.PK;

			#endregion

			Factory.Save();

			var headerManager = header.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var headerWriter = headerManager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));
			var headerData = (UniversalShipment)headerWriter.GetDataObject(header);

			AssertEquals("shipmentData DataSource Key", "|||eManifest|QUISTULAX~AHHGOTSUNSHINE", headerData.DataContext.GetDataSourceKey());
			AssertNotNull("shipmentData.SubShipmentCollection", headerData.SubShipmentCollection);

			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				ObjectFactory.New<IXmlWriter>().WriteXML(headerData, stream);

				using (var reader = new StreamReader(stream))
				{
					string result = reader.ReadToEnd();
					string expectedShipment = File.ReadAllText(TestFiles.GetPathFor("eManifestExportFromBookingHeader.xml"));
					AssertMultilineASCIIEquals("Serialized Shipment", expectedShipment.Trim(), result);
				}
			}
		}
	}
}
