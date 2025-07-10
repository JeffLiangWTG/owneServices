using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer.Testing
{
	sealed class SGAsycudaForCustomsDeclarationDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestExportBillForSGDeclaration()
		{
			var orgAddress = Factory.LoadTop1<OrgAddress>(new ZQuery());
			var testHelper = new ASYCUDA.Business.UniversalDataTransfer.Testing.AsycudaWriterTest();
			var bill = GetBill();
			var header = bill.Header;
			header.AMA_OA_ShippingAgent = orgAddress.PK;
			var writer = new SGAsycudaForCustomsDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, bill)), new SGAsycudaManifestHeaderDataObjectWriterHelper(bill.Header));
			var shipment = testHelper.AssertExportBillForDeclaration(writer, bill, MessageTypeCodeList.Codes.INP, DeclarationTypeCodeList.Codes.SFZ, "HAWB1234", WayBillTypeList.Codes.House, CargoPackingCodeList.Codes.PackingType5, 0, Core.Constants.TransportModes.Air, null, "BT123", "Wrights' Flyer", "BA123", 4);
			AssertEquals("AdditionalBillCollection.Count", 2, shipment.AdditionalBillCollection.Count);
			testHelper.AssertContents(shipment.AdditionalBillCollection[0], "HAWB1234", WayBillTypeList.Codes.House, "MB2");
			testHelper.AssertContents(shipment.AdditionalBillCollection[1], "MB2", WayBillTypeList.Codes.Master, null);
			var inwardCarrierAgentData = shipment.OrganizationAddressCollection.FirstOrDefault(Constants.AddressType.InwardCarrierAgent);
			var outwardCarrierAgentData = shipment.OrganizationAddressCollection.FirstOrDefault(Constants.AddressType.OutwardCarrierAgent);
			AssertEquals(header.ShippingAgent.Header.OH_FullName, inwardCarrierAgentData.CompanyName);
			AssertNull(outwardCarrierAgentData);

			header.AMA_ManifestType = Constants.ManifestType.Export;
			var addInfos = new[]
			{
				new KeyValuePair<ZString, ZString>(SG.V4.Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_OutwardTransportMode), Core.Constants.TransportModes.Air),
				new KeyValuePair<ZString, ZString>(SG.V4.Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_OutwardMAWB), "MB2"),
				new KeyValuePair<ZString, ZString>(SG.V4.Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_OutwardHAWB), "HAWB1234"),
				new KeyValuePair<ZString, ZString>(SG.V4.Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_OutwardVoyageFlightNo), "BA123"),
				new KeyValuePair<ZString, ZString>(SG.V4.Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_OutwardVesselName), "Wrights' Flyer")
			};
			AssertEquals("bill.ABL_ShipmentType", ShipmentTypeList.Codes.Export22, bill.ABL_ShipmentType);
			writer = new SGAsycudaForCustomsDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, bill)), new SGAsycudaManifestHeaderDataObjectWriterHelper(bill.Header));
			shipment = testHelper.AssertExportBillForDeclaration(writer, bill, MessageTypeCodeList.Codes.OUT, DeclarationTypeCodeList.Codes.DRT, null, null, CargoPackingCodeList.Codes.PackingType5, 0, null, addInfos, "BT123", null, null, 4);
			AssertNull("AdditionalBillCollection", shipment.AdditionalBillCollection);
			inwardCarrierAgentData = shipment.OrganizationAddressCollection.FirstOrDefault(Constants.AddressType.InwardCarrierAgent);
			outwardCarrierAgentData = shipment.OrganizationAddressCollection.FirstOrDefault(Constants.AddressType.OutwardCarrierAgent);
			AssertEquals(header.ShippingAgent.Header.OH_FullName, outwardCarrierAgentData.CompanyName);
			AssertNull(inwardCarrierAgentData);
		}

		public void TestPopulateFieldsForOVR()
		{
			var bill = GetBill();
			var header = bill.Header;
			var orgAddress = Factory.LoadTop1<OrgAddress>(new ZQuery());
			header.AMA_OA_ShippingAgent = orgAddress.PK;

			bill.GSTNReferenceNo = "";
			bill.Packs[0].PackedItem.GSTPaid = Customs.Business.YesNoList.Codes.No;

			var writer = new SGAsycudaForCustomsDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, bill)), new SGAsycudaManifestHeaderDataObjectWriterHelper(header));
			var shipment = writer.GetDataObject(bill);

			CombineAssertions(() =>
			{
				AssertEquals("MessageType", "INP", shipment.MessageType.Code);
				AssertEquals("MessageSubType", "SFZ", shipment.MessageSubType.Code);

				var placeOfReceiptMappedName = SG.V4.Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_US_NKPlaceOfReceipt);
				AssertEquals("SG_US_NKPlaceOfReceipt", null, shipment.AddInfoCollection?.GetZStringValue(placeOfReceiptMappedName));
				var placeOfCargoReleaseMappedName = SG.V4.Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_US_NKPlaceOfCargoRelease);
				AssertEquals("SG_US_NKPlaceOfCargoRelease", null, shipment.AddInfoCollection?.GetZStringValue(placeOfCargoReleaseMappedName));

				var invoiceDataObject = shipment.CommercialInfo.CommercialInvoiceCollection[0];
				var invoiceLineDataObject = invoiceDataObject.CommercialInvoiceLineCollection[0];
				AssertNull("GSTNReferenceNo", invoiceLineDataObject.CustomsReferenceCollection);
				AssertNull("GSTPaymentIndicator", invoiceLineDataObject.AdditionalLineTariffDetailCollection);
			});

			bill.GSTNReferenceNo = "ABC123";
			bill.Packs[0].PackedItem.GSTPaid = Customs.Business.YesNoList.Codes.Yes;

			writer = new SGAsycudaForCustomsDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, bill)), new SGAsycudaManifestHeaderDataObjectWriterHelper(header));
			shipment = writer.GetDataObject(bill);

			CombineAssertions(() =>
			{
				AssertEquals("MessageType", "INP", shipment.MessageType.Code);
				AssertEquals("MessageSubType", "APS", shipment.MessageSubType.Code);

				var placeOfReceiptMappedName = SG.V4.Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_US_NKPlaceOfReceipt);
				AssertEquals("SG_US_NKPlaceOfReceipt", "OVR", shipment.AddInfoCollection?.GetZStringValue(placeOfReceiptMappedName));
				var placeOfCargoReleaseMappedName = SG.V4.Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_US_NKPlaceOfCargoRelease);
				AssertEquals("SG_US_NKPlaceOfCargoRelease", null, shipment.AddInfoCollection?.GetZStringValue(placeOfCargoReleaseMappedName));

				var invoiceDataObject = shipment.CommercialInfo.CommercialInvoiceCollection[0];
				var invoiceLineDataObject = invoiceDataObject.CommercialInvoiceLineCollection[0];
				var reference = invoiceLineDataObject.CustomsReferenceCollection?.FirstOrDefault(x => (x.Type?.Code ?? "") == Common.SG.CusCodeDataTypeList.Codes.CASCode1);
				AssertEquals("GSTNReferenceNo", "ABC123", reference.Reference);
				AssertEquals("GSTPaymentIndicator", "OVR", invoiceLineDataObject.AdditionalLineTariffDetailCollection[0].Tariff);
			});
		}

		AsycudaBill GetBill()
		{
			var testHelper = new ASYCUDA.Business.UniversalDataTransfer.Testing.AsycudaWriterTest();
			var bill = testHelper.CreateBillForExportBillForDeclaration(Core.Constants.CountryCodes.Singapore, "SGVLI", SGManifestTypes.Codes.MGI, Core.Constants.TransportModes.Air);
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			Factory.SaveForTesting();
			return Factory.Load<AsycudaBill>(bill.PK);
		}
	}
}
