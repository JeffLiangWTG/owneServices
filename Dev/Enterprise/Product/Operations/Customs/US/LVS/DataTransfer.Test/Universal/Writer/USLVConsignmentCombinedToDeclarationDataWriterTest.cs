using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.LVS.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Customs.US.DataTransfer.Universal.Constants;

namespace Enterprise.Customs.US.LVS.DataTransfer.Test
{
	class USLVConsignmentCombinedToDeclarationDataWriterTest : USLVConsignmentToDeclarationDataWriterTest
	{
		public void TestGetFromObjectFactory_Combined()
		{
			var writer = ObjectFactory.Get<ITopLevelDataObjectWriter>(
				"USLVConsignmentCombinedToDeclarationDataWriter",
				new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, Factory.New<CusUSLVClearance>())));
			AssertType<USLVConsignmentCombinedToDeclarationDataWriter>(writer);
		}

		public void TestConsignmentCombinedToDeclarationDataMapping()
		{
			var consignment1 = SetupULSVConsignmentBO();
			consignment1.ULB_ConvertAction = ULBConvertActionList.Codes.Combined;
			var clearance = consignment1.Shipment;

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var seller = GetOrganizationBO_WUFSHIJNB(factory).MainAddress;
			var consignee = GetOrganizationBO_INTHEMSYD(factory).MainAddress;

			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.ULB_OwnerReferenceNumber = "8974578";
			consignment2.ULB_HouseBill = "HouseBill2";
			consignment2.ULB_HouseBillIssuerSCAC = "SCAC";
			consignment2.ULB_EquipmentNumber = "EQ";
			consignment2.ULB_NumberOfPacks = 2;
			consignment2.ULB_PackType = "PKG";
			consignment2.ULB_NonAMSIndicator = true;
			consignment2.ULB_IsResponsePending = true;
			consignment2.ULB_SubmittedDate = new ZDate(2020, 08, 04);
			consignment2.ULB_MessageStatus = "4";
			consignment2.ULB_OA_Consignee = consignee.PK;
			consignment2.ULB_OA_Seller = seller.PK;
			consignment2.ULB_ConsigneeQualifier = "AB";
			consignment2.ULB_ConsigneeIdentifier = "BC";
			consignment2.CE_RailReferenceNumber = "RRN123";
			consignment2.ULB_ConvertAction = ULBConvertActionList.Codes.Combined;
			SetCommercialInfo(consignment2);
			factory.Save();

			var dataObject = new USLVConsignmentCombinedToDeclarationDataWriter(new DataWritingManager(new ActionInfo(null, consignment1))).GetDataObject(consignment1);

			AssertEquals("SEA", dataObject.TransportMode.Code);
			AssertEquals("CNT", dataObject.CustomsContainerMode.Code);
			AssertEquals("AUSYD", dataObject.PortOfLoading.Code);
			AssertEquals("USLAX", dataObject.PortOfDischarge.Code);
			AssertEquals("UNITED KINGDOM", dataObject.VesselName);
			AssertEquals("V1", dataObject.VoyageFlightNo);
			AssertDataObjectHasExactAddInfo(dataObject, "MasterWayBillNumber", "MasterBill1");
			AssertDataObjectHasExactAddInfo(dataObject, "MasterWayBillIssuerSCAC", "SCAC");
			AssertDataObjectHasExactAddInfo(dataObject, "UI_NKCarrierSCAC", "CCAC");
			AssertDataObjectHasExactAddInfo(dataObject, "SchDLoading", "60267");
			AssertDataObjectHasExactAddInfo(dataObject, "SchDEntry", "1233");
			AssertDataObjectHasExactAddInfo(dataObject, "SchDArrival", "2704");
			AssertDataObjectHasExactAddInfo(dataObject, "EntryDate", "2020-08-10");
			AssertDataObjectHasExactAddInfo(dataObject, "FilerName", "ALAN PARSON");
			AssertDataObjectHasExactAddInfo(dataObject, "FilerPhoneNumber", "02 9000 2224");
			AssertDataObjectExactAddInfoIsNull(dataObject, "EntryType");
			AssertDataObjectHasExactAddInfo(dataObject, "EntryMode", "RLF");
			AssertDataObjectHasExactAddInfo(dataObject, "HasMPF", "N");
			AssertDataObjectHasExactAddInfo(dataObject, "NonAMS", "Y");
			AssertDataObjectHasExactAddInfo(dataObject, "EnableCRL", "Y");
			var additionalRef = dataObject.AdditionalReferenceCollection.FirstOrDefault();
			AssertEquals("RRN123", additionalRef.ReferenceNumber);
			AssertEquals("RRN", additionalRef.Type.Code);
			AssertDataObjectHasExactDate(dataObject, DateType.LoadingDate, new ZDateTime(2020, 8, 4));
			AssertDataObjectHasExactDate(dataObject, DateType.DischargeDate, new ZDateTime(2020, 8, 11));
			AssertEquals("8974578", dataObject.OwnerRef);
			AssertEquals("HouseBill1", dataObject.WayBillNumber);
			AssertEquals(3, dataObject.TotalNoOfPacks);
			AssertEquals("PKG", dataObject.TotalNoOfPacksPackageType.Code);
			AssertEquals("EQ", dataObject.ContainerCollection[0].ContainerNumber);
			AssertEquals(CollectionContent.Complete, dataObject.PackingLineCollection.Content);

			var packingLine1 = dataObject.PackingLineCollection[0];
			AssertEquals("HouseBill1", packingLine1.BillNumber);
			AssertEquals("HWB", packingLine1.BillType.Code);
			AssertEquals("House Waybill", packingLine1.BillType.Description);
			AssertEquals("EQ", packingLine1.ContainerNumber);
			AssertEquals(1, packingLine1.Link);
			AssertEquals(1L, packingLine1.PackQty);
			AssertEquals("PKG", packingLine1.PackType.Code);
			AssertEquals(4, packingLine1.PackedItemCollection.Count);

			var packingLine2 = dataObject.PackingLineCollection[1];
			AssertEquals("HouseBill2", packingLine2.BillNumber);
			AssertEquals("HWB", packingLine2.BillType.Code);
			AssertEquals("House Waybill", packingLine2.BillType.Description);
			AssertEquals("EQ", packingLine2.ContainerNumber);
			AssertEquals(2, packingLine2.Link);
			AssertEquals(2L, packingLine2.PackQty);
			AssertEquals("PKG", packingLine2.PackType.Code);
			AssertEquals(4, packingLine2.PackedItemCollection.Count);

			AssertEquals(6, dataObject.CommercialInfo.CommercialInvoiceCollection.Count);
			AssertEquals(6, dataObject.CommercialInfo.CommercialInvoiceCollection.Distinct().Count());

			var invoiceAAA_1 = dataObject.CommercialInfo.CommercialInvoiceCollection.First(c => c.InvoiceCurrency.Code.Value == "AAA");
			AssertEquals("HouseBill1AAA", invoiceAAA_1.InvoiceNumber);
			AssertEquals(1, invoiceAAA_1.CommercialInvoiceLineCollection.Count);
			AssertEquals(111M, invoiceAAA_1.InvoiceAmount);
			var invoiceLineAAA1_1 = invoiceAAA_1.CommercialInvoiceLineCollection[0];
			AssertEquals("1011", invoiceLineAAA1_1.HarmonisedCode);
			AssertEquals(111M, invoiceLineAAA1_1.CustomsValue);
			AssertEquals("GoodsDescription1", invoiceLineAAA1_1.Description);
			AssertEquals("US", invoiceLineAAA1_1.CountryOfOrigin.Code);
			AssertEquals(111M, invoiceLineAAA1_1.LinePrice);
			AssertEquals("AA1", invoiceLineAAA1_1.PartNo);
			AssertInvoiceLineAddInfo(invoiceLineAAA1_1, consignment1.CusUSLVItems[0], "Y", "Y", "US");
			AssertInvoiceHeaderOrganizationAddress(invoiceAAA_1, "WUFSHIJNB", "INTHEMSYD");

			var invoiceBBB_1 = dataObject.CommercialInfo.CommercialInvoiceCollection.First(c => c.InvoiceCurrency.Code.Value == "BBB");
			AssertEquals("HouseBill1BBB", invoiceBBB_1.InvoiceNumber);
			AssertEquals(2, invoiceBBB_1.CommercialInvoiceLineCollection.Count);
			AssertEquals(222M, invoiceBBB_1.InvoiceAmount);
			var invoiceLineBBB1_1 = invoiceBBB_1.CommercialInvoiceLineCollection.First(l => l.HarmonisedCode.Value == "2022");
			AssertEquals(111M, invoiceLineBBB1_1.CustomsValue);
			AssertEquals("GoodsDescription2", invoiceLineBBB1_1.Description);
			AssertEquals("US", invoiceLineBBB1_1.CountryOfOrigin.Code);
			AssertEquals(111M, invoiceLineBBB1_1.LinePrice);
			AssertEquals("BB1", invoiceLineBBB1_1.PartNo);
			AssertInvoiceLineAddInfo(invoiceLineBBB1_1, consignment1.CusUSLVItems[1], "Y", "N", "US");
			var invoiceLineBBB1_2 = invoiceBBB_1.CommercialInvoiceLineCollection.First(l => l.HarmonisedCode.Value == "3033");
			AssertEquals(111M, invoiceLineBBB1_2.CustomsValue);
			AssertEquals("GoodsDescription3", invoiceLineBBB1_2.Description);
			Assert(invoiceLineBBB1_2.CountryOfOrigin.Code.Value.IsEmpty);
			AssertEquals(111M, invoiceLineBBB1_2.LinePrice);
			AssertEquals("BB2", invoiceLineBBB1_2.PartNo);
			AssertInvoiceLineAddInfo(invoiceLineBBB1_2, consignment1.CusUSLVItems[2], "N", "Y", "");
			AssertInvoiceHeaderOrganizationAddress(invoiceBBB_1, "WUFSHIJNB", "INTHEMSYD");

			var invoiceCCC_1 = dataObject.CommercialInfo.CommercialInvoiceCollection.First(c => c.InvoiceCurrency.Code.Value == "CCC");
			AssertEquals("HouseBill1CCC", invoiceCCC_1.InvoiceNumber);
			AssertEquals(1, invoiceCCC_1.CommercialInvoiceLineCollection.Count);
			AssertEquals(111M, invoiceCCC_1.InvoiceAmount);
			var invoiceLineCCC1_1 = invoiceCCC_1.CommercialInvoiceLineCollection.Single();
			AssertEquals("4044", invoiceLineCCC1_1.HarmonisedCode);
			AssertEquals(111M, invoiceLineCCC1_1.CustomsValue);
			AssertEquals("GoodsDescription4", invoiceLineCCC1_1.Description);
			AssertEquals("US", invoiceLineCCC1_1.CountryOfOrigin.Code);
			AssertEquals(111M, invoiceLineCCC1_1.LinePrice);
			AssertEquals("CC1", invoiceLineCCC1_1.PartNo);
			AssertInvoiceLineAddInfo(invoiceLineCCC1_1, consignment1.CusUSLVItems[3], "N", "N", "US");
			AssertInvoiceHeaderOrganizationAddress(invoiceCCC_1, "WUFSHIJNB", "INTHEMSYD");

			var invoiceAAA_2 = dataObject.CommercialInfo.CommercialInvoiceCollection.Where(c => c.InvoiceCurrency.Code.Value == "AAA").ToList()[1];
			AssertEquals("HouseBill2AAA", invoiceAAA_2.InvoiceNumber);
			AssertEquals(1, invoiceAAA_2.CommercialInvoiceLineCollection.Count);
			AssertEquals(111M, invoiceAAA_2.InvoiceAmount);
			var invoiceLineAAA2_1 = invoiceAAA_2.CommercialInvoiceLineCollection[0];
			AssertEquals("1011", invoiceLineAAA2_1.HarmonisedCode);
			AssertEquals(111M, invoiceLineAAA2_1.CustomsValue);
			AssertEquals("GoodsDescription1", invoiceLineAAA2_1.Description);
			AssertEquals("US", invoiceLineAAA2_1.CountryOfOrigin.Code);
			AssertEquals(111M, invoiceLineAAA2_1.LinePrice);
			AssertEquals("AA1", invoiceLineAAA2_1.PartNo);
			AssertInvoiceLineAddInfo(invoiceLineAAA2_1, consignment2.CusUSLVItems[0], "Y", "Y", "US");
			AssertInvoiceHeaderOrganizationAddress(invoiceAAA_2, "WUFSHIJNB", "INTHEMSYD");

			var invoiceBBB_2 = dataObject.CommercialInfo.CommercialInvoiceCollection.Where(c => c.InvoiceCurrency.Code.Value == "BBB").ToList()[1];
			AssertEquals("HouseBill2BBB", invoiceBBB_2.InvoiceNumber);
			AssertEquals(2, invoiceBBB_2.CommercialInvoiceLineCollection.Count);
			AssertEquals(222M, invoiceBBB_2.InvoiceAmount);
			var invoiceLineBBB2_1 = invoiceBBB_2.CommercialInvoiceLineCollection.First(l => l.HarmonisedCode.Value == "2022");
			AssertEquals(111M, invoiceLineBBB2_1.CustomsValue);
			AssertEquals("GoodsDescription2", invoiceLineBBB2_1.Description);
			AssertEquals("US", invoiceLineBBB2_1.CountryOfOrigin.Code);
			AssertEquals(111M, invoiceLineBBB2_1.LinePrice);
			AssertEquals("BB1", invoiceLineBBB2_1.PartNo);
			AssertInvoiceLineAddInfo(invoiceLineBBB2_1, consignment2.CusUSLVItems[1], "Y", "N", "US");
			var invoiceLineBBB2_2 = invoiceBBB_2.CommercialInvoiceLineCollection.First(l => l.HarmonisedCode.Value == "3033");
			AssertEquals(111M, invoiceLineBBB2_2.CustomsValue);
			AssertEquals("GoodsDescription3", invoiceLineBBB2_2.Description);
			Assert(invoiceLineBBB2_2.CountryOfOrigin.Code.Value.IsEmpty);
			AssertEquals(111M, invoiceLineBBB2_2.LinePrice);
			AssertEquals("BB2", invoiceLineBBB2_2.PartNo);
			AssertInvoiceLineAddInfo(invoiceLineBBB2_2, consignment2.CusUSLVItems[2], "N", "Y", "");
			AssertInvoiceHeaderOrganizationAddress(invoiceBBB_2, "WUFSHIJNB", "INTHEMSYD");

			var invoiceCCC_2 = dataObject.CommercialInfo.CommercialInvoiceCollection.Where(c => c.InvoiceCurrency.Code.Value == "CCC").ToList()[1];
			AssertEquals("HouseBill2CCC", invoiceCCC_2.InvoiceNumber);
			AssertEquals(1, invoiceCCC_2.CommercialInvoiceLineCollection.Count);
			AssertEquals(111M, invoiceCCC_2.InvoiceAmount);
			var invoiceLineCCC2_1 = invoiceCCC_2.CommercialInvoiceLineCollection.Single();
			AssertEquals("4044", invoiceLineCCC2_1.HarmonisedCode);
			AssertEquals(111M, invoiceLineCCC2_1.CustomsValue);
			AssertEquals("GoodsDescription4", invoiceLineCCC2_1.Description);
			AssertEquals("US", invoiceLineCCC2_1.CountryOfOrigin.Code);
			AssertEquals(111M, invoiceLineCCC2_1.LinePrice);
			AssertEquals("CC1", invoiceLineCCC2_1.PartNo);
			AssertInvoiceLineAddInfo(invoiceLineCCC2_1, consignment2.CusUSLVItems[3], "N", "N", "US");
			AssertInvoiceHeaderOrganizationAddress(invoiceCCC_2, "WUFSHIJNB", "INTHEMSYD");

			var importerAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(o => o.AddressType.Value == AddressTypes.Importer);
			AssertNotNull(importerAddress);
			AssertEquals("WUFSHIJNB", importerAddress.OrganizationCode);
		}

		void AssertInvoiceHeaderOrganizationAddress(CommercialInvoiceHeader invoiceHeader, string seller, string consignee)
		{
			var invoiceConsignee = invoiceHeader.OrganizationAddressCollection.FirstOrDefault(o => o.AddressType.Value == Constants.AddressTypes.UltimateConsignee);
			AssertNotNull(invoiceConsignee);
			AssertEquals(consignee, invoiceConsignee.OrganizationCode);

			var invoiceSeller = invoiceHeader.OrganizationAddressCollection.FirstOrDefault(o => o.AddressType.Value == AddressType.Seller);
			AssertNotNull(invoiceSeller);
			AssertEquals(seller, invoiceSeller.OrganizationCode);
		}
	}
}
