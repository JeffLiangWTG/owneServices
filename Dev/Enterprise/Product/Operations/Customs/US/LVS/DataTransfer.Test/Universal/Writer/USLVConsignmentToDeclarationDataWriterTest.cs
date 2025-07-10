using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.LVS.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.LVS.DataTransfer.Test
{
	class USLVConsignmentToDeclarationDataWriterTest : USLVConsignmentDataWriterTest
	{
		public void TestGetFromObjectFactory()
		{
			var writer = ObjectFactory.Get<ITopLevelDataObjectWriter>(
				"USLVConsignmentToDeclarationDataWriter",
				new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, Factory.New<CusUSLVClearance>())));
			AssertType<USLVConsignmentToDeclarationDataWriter>(writer);
		}

		public void TestHousebillToDeclarationDataMapping()
		{
			var consignment = SetupULSVConsignmentBO();
			consignment.Shipment.ULH_EntryFilerCode = "ZZZ";
			consignment.CE_EntryNum = "ENTRYNUM";

			var shipment = consignment.Shipment;
			var dataObject = new USLVConsignmentToDeclarationDataWriter(new DataWritingManager(new ActionInfo(null, consignment))).GetDataObject(consignment);

			CombineAssertions(() =>
			{
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
				AssertDataObjectHasExactAddInfo(dataObject, "EntryType", "86");
				AssertDataObjectHasExactAddInfo(dataObject, "FilerPhoneNumber", "02 9000 2224");
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
				AssertEquals(1, dataObject.TotalNoOfPacks);
				AssertEquals("PKG", dataObject.TotalNoOfPacksPackageType.Code);
				AssertEquals("EQ", dataObject.ContainerCollection[0].ContainerNumber);
				AssertEquals(CollectionContent.Complete, dataObject.PackingLineCollection.Content);
				var packingLine = dataObject.PackingLineCollection[0];
				AssertEquals("HouseBill1", packingLine.BillNumber);
				AssertEquals("HWB", packingLine.BillType.Code);
				AssertEquals("House Waybill", packingLine.BillType.Description);
				AssertEquals("EQ", packingLine.ContainerNumber);
				AssertEquals(1, packingLine.Link);
				AssertEquals(1L, packingLine.PackQty);
				AssertEquals("PKG", packingLine.PackType.Code);

				AssertEquals(4, packingLine.PackedItemCollection.Count);

				AssertEquals(3, dataObject.CommercialInfo.CommercialInvoiceCollection.Count);
				Assert(dataObject.CommercialInfo.CommercialInvoiceCollection.All(x => int.Parse(x.InvoiceNumber) >= 1 && int.Parse(x.InvoiceNumber) <= 3));
				AssertEquals(3, dataObject.CommercialInfo.CommercialInvoiceCollection.Distinct().Count());

				var invoiceAAA = dataObject.CommercialInfo.CommercialInvoiceCollection.Single(c => c.InvoiceCurrency.Code.Value == "AAA");
				AssertEquals(1, invoiceAAA.CommercialInvoiceLineCollection.Count);
				AssertEquals(111M, invoiceAAA.InvoiceAmount);
				AssertDataObjectHasExactAddInfo(invoiceAAA, "ReleaseEntryNumber", "ZZZENTRYNUM");
				var invoiceLineAAA_1 = invoiceAAA.CommercialInvoiceLineCollection[0];
				AssertEquals("1011", invoiceLineAAA_1.HarmonisedCode);
				AssertEquals(111M, invoiceLineAAA_1.CustomsValue);
				AssertEquals("GoodsDescription1", invoiceLineAAA_1.Description);
				AssertEquals("US", invoiceLineAAA_1.CountryOfOrigin.Code);
				AssertEquals(111M, invoiceLineAAA_1.LinePrice);
				AssertEquals("AA1", invoiceLineAAA_1.PartNo);
				AssertInvoiceLineAddInfo(invoiceLineAAA_1, consignment.CusUSLVItems[0], "Y", "Y", "US");

				var invoiceBBB = dataObject.CommercialInfo.CommercialInvoiceCollection.First(c => c.InvoiceCurrency.Code.Value == "BBB");
				AssertEquals(2, invoiceBBB.CommercialInvoiceLineCollection.Count);
				AssertEquals(222M, invoiceBBB.InvoiceAmount);
				var invoiceLineBBB_1 = invoiceBBB.CommercialInvoiceLineCollection.First(l => l.HarmonisedCode.Value == "2022");
				AssertEquals(111M, invoiceLineBBB_1.CustomsValue);
				AssertEquals("GoodsDescription2", invoiceLineBBB_1.Description);
				AssertEquals("US", invoiceLineBBB_1.CountryOfOrigin.Code);
				AssertEquals(111M, invoiceLineBBB_1.LinePrice);
				AssertEquals("BB1", invoiceLineBBB_1.PartNo);
				AssertInvoiceLineAddInfo(invoiceLineBBB_1, consignment.CusUSLVItems[1], "Y", "N", "US");
				var invoiceLineBBB_2 = invoiceBBB.CommercialInvoiceLineCollection.First(l => l.HarmonisedCode.Value == "3033");
				AssertEquals(111M, invoiceLineBBB_2.CustomsValue);
				AssertEquals("GoodsDescription3", invoiceLineBBB_2.Description);
				Assert(invoiceLineBBB_2.CountryOfOrigin.Code.Value.IsEmpty);
				AssertEquals(111M, invoiceLineBBB_2.LinePrice);
				AssertEquals("BB2", invoiceLineBBB_2.PartNo);
				AssertInvoiceLineAddInfo(invoiceLineBBB_2, consignment.CusUSLVItems[2], "N", "Y", "");

				var invoiceCCC = dataObject.CommercialInfo.CommercialInvoiceCollection.First(c => c.InvoiceCurrency.Code.Value == "CCC");
				AssertEquals(1, invoiceCCC.CommercialInvoiceLineCollection.Count);
				AssertEquals(111M, invoiceCCC.InvoiceAmount);
				var invoiceLineCCC_1 = invoiceCCC.CommercialInvoiceLineCollection.Single();
				AssertEquals("4044", invoiceLineCCC_1.HarmonisedCode);
				AssertEquals(111M, invoiceLineCCC_1.CustomsValue);
				AssertEquals("GoodsDescription4", invoiceLineCCC_1.Description);
				AssertEquals("US", invoiceLineCCC_1.CountryOfOrigin.Code);
				AssertEquals(111M, invoiceLineCCC_1.LinePrice);
				AssertEquals("CC1", invoiceLineCCC_1.PartNo);
				AssertInvoiceLineAddInfo(invoiceLineCCC_1, consignment.CusUSLVItems[3], "N", "N", "US");

				var importerAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(o => o.AddressType.Value == AddressTypes.Importer);
				AssertNotNull(importerAddress);
				AssertEquals("WUFSHIJNB", importerAddress.OrganizationCode);
			});
		}

		public void TestPGAIndicatorAndDisclaimReason()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var item = consignment.CusUSLVItems.AddNew();
			var fda = item.ItemPGAWrapperCollection.OfType<CusUSLVItemPGAWrapper>().FirstOrDefault(x => x.Agency == "FDA");
			fda.Indicator = "C";
			fda.DisclaimReason = "A";
			AssertEquals(1, item.CusUSLVItemPGAs.Count);

			var dataObject = new USLVConsignmentToDeclarationDataWriter(new DataWritingManager(new ActionInfo(null, consignment))).GetDataObject(consignment);
			var invoiceLineCollection = dataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection;
			AssertEquals(1, invoiceLineCollection.Count);
			var invoiceLine = invoiceLineCollection[0];
			var fdaDisclaimReasonList = invoiceLine.AddInfoCollection.OfType<UniversalDataBuss.DataObjects.Universal.AddInfo>().Where(x => x.Key.GetValueOrDefault() == LVSConstants.AddInfoConstants.FDADisclaimReason).ToList();
			AssertEquals(0, fdaDisclaimReasonList.Count);
			var fdaIndicatorList = invoiceLine.AddInfoCollection.OfType<UniversalDataBuss.DataObjects.Universal.AddInfo>().Where(x => x.Key.GetValueOrDefault() == LVSConstants.AddInfoConstants.FDAIndicator).ToList();
			AssertEquals(0, fdaIndicatorList.Count);

			var tariff = CreateTariffWithPGACodes("12345678", "FD4NM2");
			consignment.CusUSLVItems.RemoveAndDeleteAll();
			item = consignment.CusUSLVItems.AddNew();
			item.ULI_Tariff = tariff.UE_Tariff;
			AssertEquals(0, item.CusUSLVItemPGAs.Count);

			dataObject = new USLVConsignmentToDeclarationDataWriter(new DataWritingManager(new ActionInfo(null, consignment))).GetDataObject(consignment);
			invoiceLineCollection = dataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection;
			AssertEquals(1, invoiceLineCollection.Count);
			invoiceLine = invoiceLineCollection[0];
			fdaDisclaimReasonList = invoiceLine.AddInfoCollection.OfType<UniversalDataBuss.DataObjects.Universal.AddInfo>().Where(x => x.Key.GetValueOrDefault() == LVSConstants.AddInfoConstants.FDADisclaimReason).ToList();
			AssertEquals(0, fdaDisclaimReasonList.Count);
			fdaIndicatorList = invoiceLine.AddInfoCollection.OfType<UniversalDataBuss.DataObjects.Universal.AddInfo>().Where(x => x.Key.GetValueOrDefault() == LVSConstants.AddInfoConstants.FDAIndicator).ToList();
			AssertEquals(1, fdaIndicatorList.Count);
			AssertEquals(OGAIndicatorList.Codes.Declared, fdaIndicatorList[0].Value);

			fda = item.ItemPGAWrapperCollection.OfType<CusUSLVItemPGAWrapper>().FirstOrDefault(x => x.Agency == "FDA");
			fda.Indicator = OGAIndicatorList.Codes.Disclaimed;
			fda.DisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertEquals(1, item.CusUSLVItemPGAs.Count);

			dataObject = new USLVConsignmentToDeclarationDataWriter(new DataWritingManager(new ActionInfo(null, consignment))).GetDataObject(consignment);
			invoiceLineCollection = dataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection;
			AssertEquals(1, invoiceLineCollection.Count);
			invoiceLine = invoiceLineCollection[0];
			fdaDisclaimReasonList = invoiceLine.AddInfoCollection.OfType<UniversalDataBuss.DataObjects.Universal.AddInfo>().Where(x => x.Key.GetValueOrDefault() == LVSConstants.AddInfoConstants.FDADisclaimReason).ToList();
			AssertEquals(1, fdaDisclaimReasonList.Count);
			AssertEquals(PGADisclaimReasonList.Codes.A, fdaDisclaimReasonList[0].Value);
			fdaIndicatorList = invoiceLine.AddInfoCollection.OfType<UniversalDataBuss.DataObjects.Universal.AddInfo>().Where(x => x.Key.GetValueOrDefault() == LVSConstants.AddInfoConstants.FDAIndicator).ToList();
			AssertEquals(1, fdaIndicatorList.Count);
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, fdaIndicatorList[0].Value);
		}

		public void TestPopulateCommercialInvoices()
		{
			var sellerOH = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var consigneeOH = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);

			var consignmentBO = Factory.New<CusUSLVClearance>().CusUSLVConsignments.AddNew();
			consignmentBO.ULB_OA_Seller = sellerOH.MainAddress.PK;
			consignmentBO.ULB_OA_Consignee = consigneeOH.MainAddress.PK;
			_ = consignmentBO.CusUSLVItems.AddNew();

			var expectedInvoiceNumber = 10;
			var invoiceHeader = new USLVConsignmentToDeclarationDataWriter(new DataWritingManager(new ActionInfo(null, consignmentBO))).PopulateCommercialInvoices(consignmentBO, null, expectedInvoiceNumber).First();

			AssertEquals("Invoice Number", expectedInvoiceNumber.ToString(), invoiceHeader.InvoiceNumber);

			AssertOrganizationBO_WUFSHIJNB("Supplier", invoiceHeader.Supplier, AddressTypes.Supplier);
			AssertOrganizationBO_WUFSHIJNBExists(invoiceHeader.OrganizationAddressCollection, "Exporter", US.DataTransfer.Universal.Constants.AddressType.Exporter);
			AssertOrganizationBO_WUFSHIJNBExists(invoiceHeader.OrganizationAddressCollection, "Seller", US.DataTransfer.Universal.Constants.AddressType.Seller);

			AssertOrganizationBO_CRAHOLSYD("Buyer", invoiceHeader.Buyer, AddressTypes.Recipient);
			AssertOrganizationBO_CRAHOLSYDExists(invoiceHeader.OrganizationAddressCollection, "UltimateConsignee", Customs.DataTransfer.Universal.Constants.AddressTypes.UltimateConsignee);
			AssertOrganizationBO_CRAHOLSYDExists(invoiceHeader.OrganizationAddressCollection, "SoldToParty", Customs.DataTransfer.Universal.Constants.AddressTypes.SoldToParty);
		}

		USCTariff CreateTariffWithPGACodes(ZString tariff, ZString pgaCodes)
		{
			var result = Factory.New<USCTariff>();
			result.UE_Tariff = tariff;
			result.UE_PGACodes = pgaCodes;
			result.UE_DateFrom = new ZDateTime(2008, 1, 1);
			result.UE_DateTo = ZDateTime.Today.AddDays(1);
			return result;
		}

		protected void AssertInvoiceLineAddInfo(CommercialInvoiceLine line, CusUSLVItem item, string antiDumping, string countervailing, string countryOfOrigin)
		{
			var addInfoAntiDumping = line.AddInfoCollection.FirstOrDefault(a => a.Key.Value == LVSConstants.AddInfoConstants.ADD_NA);
			AssertNotNull(LVSConstants.AddInfoConstants.ADD_NA + "should exist", addInfoAntiDumping);
			AssertNotNull(LVSConstants.AddInfoConstants.ADD_NA + "should have value", addInfoAntiDumping.Value);

			if (addInfoAntiDumping != null && addInfoAntiDumping.Value.HasValue)
			{
				AssertEquals(antiDumping, addInfoAntiDumping.Value.Value);
			}

			var addInfoCountervailing = line.AddInfoCollection.FirstOrDefault(a => a.Key.Value == LVSConstants.AddInfoConstants.CVD_NA);
			AssertNotNull(LVSConstants.AddInfoConstants.CVD_NA + "should exist", addInfoCountervailing);
			AssertNotNull(LVSConstants.AddInfoConstants.CVD_NA + "should have value", addInfoCountervailing.Value);

			if (addInfoCountervailing != null && addInfoCountervailing.Value.HasValue)
			{
				AssertEquals(countervailing, addInfoCountervailing.Value.Value);
			}

			var addInfoCountryOfOrigin = line.AddInfoCollection.FirstOrDefault(a => a.Key.Value == LVSConstants.AddInfoConstants.UC_NKCountryOfOrigin);
			AssertNotNull(LVSConstants.AddInfoConstants.UC_NKCountryOfOrigin + "should exist", addInfoCountryOfOrigin);
			AssertNotNull(LVSConstants.AddInfoConstants.UC_NKCountryOfOrigin + "should have value", addInfoCountryOfOrigin.Value);
			AssertEquals(countryOfOrigin, addInfoCountryOfOrigin.Value);

			AssertAgencyAddInfo(line, item, "APHISDisclaimReason", "APHISInd", "APH", "AVS");
			AssertAgencyAddInfo(line, item, "CPSCDisclaimReason", "CPSCInd", "CPS", "CPS");
			AssertAgencyAddInfo(line, item, "DEADisclaimReason", "DEAInd", "DEA", "DEA");
			AssertAgencyAddInfo(line, item, "NHTDisclaimReason", "NHTSAIndicator", "NHT", "OFF");
			AssertAgencyAddInfo(line, item, "ODSDisclaimReason", "ODSInd", "EPA", "ODS");
			AssertAgencyAddInfo(line, item, "PSTDisclaimReason", "PSTIndicator", "EPA", "PS1");
			AssertAgencyAddInfo(line, item, "TSCADisclaimReason", "TSCAInd", "EPA", "TS1");
			AssertAgencyAddInfo(line, item, "VNEDisclaimReason", "VNEInd", "EPA", "VNE");
			AssertAgencyAddInfo(line, item, "FDADisclaimReason", "FDAIndicator", "FDA", "FDA");
			AssertAgencyAddInfo(line, item, "FWSDisclaimReason", "FWSInd", "FWS", "FWS");
			AssertAgencyAddInfo(line, item, "LaceyDisclaimReason", "LaceyIndicator", "APH", "APL");
			AssertAgencyAddInfo(line, item, "NMFS370DisclaimReason", "NMFS370Ind", "NMF", "370");
			AssertAgencyAddInfo(line, item, "NMFSAMRDisclaimReason", "NMFSAMRInd", "NMF", "AMR");
			AssertAgencyAddInfo(line, item, "NMFSHMSDisclaimReason", "NMFSHMSInd", "NMF", "HMS");
			AssertAgencyAddInfo(line, item, "OMCDisclaimReason", "OMCInd", "OMC", "OMC");
			AssertAgencyAddInfo(line, item, "TTBDisclaimReason", "TTBInd", "TTB", "TOB");
			AssertAgencyAddInfo(line, item, "AMSDisclaimReason", "AMSInd", "AMS", "MO8");
			AssertAgencyAddInfo(line, item, "NOPDisclaimReason", "NOPInd", "NOP", "OR1");
			AssertAgencyAddInfo(line, item, "FSISDisclaimReason", "FSISInd", "FSI", "FSI");
		}

		protected void AssertAgencyAddInfo(CommercialInvoiceLine line, CusUSLVItem uSLVItem, string disclaimReasonKey, string disclaimIndicatorKey, string agency, string agencyProgram)
		{
			var cusUSLVPGAs = uSLVItem.CusUSLVItemPGAs.ToArray<CusUSLVItemPGA>();
			var pgaItemBO = cusUSLVPGAs.Where(n => n.ULP_Agency == agency && n.ULP_AgencyProgram == agencyProgram).FirstOrDefault();
			var pgaItemWrapper = uSLVItem.ItemPGAWrapperCollection.OfType<CusUSLVItemPGAWrapper>().FirstOrDefault(n => n.Agency == agency && n.AgencyProgram == agencyProgram);

			if (!pgaItemWrapper.Requirement.IsEmpty)
			{
				var pgaItemReason = line.AddInfoCollection.FirstOrDefault(n => n.Key.ToString() == disclaimReasonKey);
				AssertNotNull(pgaItemReason);
				AssertEquals(pgaItemReason.Key, new PGAHelper(pgaItemBO).DisclaimReason);
				AssertEquals(pgaItemReason.Value, pgaItemWrapper.DisclaimReason);

				var pgaItemIndicator = line.AddInfoCollection.FirstOrDefault(n => n.Key.ToString() == disclaimIndicatorKey);
				AssertNotNull(pgaItemIndicator);
				AssertEquals(pgaItemIndicator.Key, PGAHelper.GetIndicatorCode(agency, agencyProgram));
				AssertEquals(pgaItemIndicator.Value, pgaItemWrapper.Indicator);
			}
		}

		protected void AssertDataObjectHasExactAddInfo(IAddInfoCollectionParent dataObject, string addInfoKey, object expectedAddInfoValue)
		{
			var addInfo = dataObject.AddInfoCollection.FirstOrDefault(a => a.Key.Value == addInfoKey);
			AssertNotNull(addInfoKey + "should exist", addInfo);
			if (addInfo != null)
			{
				AssertEquals(addInfoKey, expectedAddInfoValue, addInfo.Value);
			}
		}

		protected void AssertDataObjectHasExactDate(Shipment dataObject, DateType dateType, ZDateTime expectedDate)
		{
			var date = dataObject.DateCollection.FirstOrDefault(a => a.Type.Value == dateType);
			AssertNotNull(dateType + "should exist", date);
			if (date != null)
			{
				AssertEquals(dateType.ToString(), expectedDate, date.Value);
				Assert(!date.IsEstimate.Value);
			}
		}
		protected void AssertDataObjectExactAddInfoIsNull(Shipment dataObject, string addInfoKey)
		{
			var addInfo = dataObject.AddInfoCollection.FirstOrDefault(a => a.Key.Value == addInfoKey);
			AssertNull(addInfo);
		}
	}
}
