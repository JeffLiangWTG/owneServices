using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.LVS.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.US.DataTransfer.Universal.Constants;

namespace Enterprise.Customs.US.LVS.DataTransfer.Test
{
	class USLVConsignmentDataWriterTest : USLVDataWriterTestHelper
	{
		public void TestULSVConsignmentDataMapping()
		{
			var consignmentBo = SetupULSVConsignmentBO();
			var consignmentData = new USLVConsignmentDataWriter(new DataWritingManager(new ActionInfo(null, consignmentBo))).GetDataObject(consignmentBo);
			AssertTestToConsignment(consignmentBo, consignmentData);

			TestSetupConsigneeAndSellerDetailManually(consignmentBo);
		}

		public void TestOrganizationAddressCollection_WhenNotCreatingConsolidatedSummary_ShouldNotCreateTempOrgs()
		{
			var consignmentBO = Factory.New<CusUSLVConsignment>();
			consignmentBO.ULB_HouseBill = "HBL12345";
			SetupConsigneeAndSellerDetailManually(consignmentBO);

			var actionInfo = new ActionInfo(null, consignmentBO);
			var writer = new USLVConsignmentDataWriter(new DataWritingManager(actionInfo));

			var consignmentDataObject = writer.GetDataObject(consignmentBO);
			var tempOrgs = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsTempAccount, true));

			CombineAssertions(() =>
			{
				AssertEquals("Number of temporary orgs", 0, tempOrgs.Length);
				AssertConsigneeAndSellerManually(consignmentDataObject, consignmentBO);
			});
		}

		public void TestOrganizationAddressCollection_WhenCreatingConsolidatedSummary_ShouldCreateTempOrgs()
		{
			var consignmentBO = Factory.New<CusUSLVConsignment>();
			consignmentBO.ULB_HouseBill = "HBL12345^@)";
			SetupConsigneeAndSellerDetailManually(consignmentBO);

			var actionInfo = new ActionInfo(null, consignmentBO);
			actionInfo.ActionType = WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage;

			var writer = new USLVConsignmentDataWriter(new DataWritingManager(actionInfo));
			var consignmentDataObject = writer.GetDataObject(consignmentBO);
			var tempOrgs = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsTempAccount, true));

			CombineAssertions(() =>
			{
				AssertEquals("Number of temporary orgs", 2, tempOrgs.Length);

				var consigneeOrg = tempOrgs.SingleOrDefault(x => x.OH_FullName == "cName");
				AssertNotNull("Consignee org", consigneeOrg);
				AssertEquals("Consignee main address", consigneeOrg.MainAddress.PK, consignmentBO.ULB_OA_Consignee);
				
				var sellerOrg = tempOrgs.SingleOrDefault(x => x.OH_FullName == "sName");
				AssertNotNull("Seller org", sellerOrg);
				AssertEquals("Seller main address", sellerOrg.MainAddress.PK, consignmentBO.ULB_OA_Seller);

				AssertConsigneeAndSellerTemporaryOrgContents(consignmentBO, consigneeOrg, sellerOrg);
				AssertOrganizationAddressContents(consignmentDataObject, Constants.AddressTypes.UltimateConsignee, consigneeOrg.MainAddress);
				AssertOrganizationAddressContents(consignmentDataObject, AddressType.Seller, sellerOrg.MainAddress);

				var consigneeRegNums = (OrgCusCode[])consigneeOrg.CustomsCodes.Find(new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.EmployerIdentificationNumber));
				AssertEquals("Number of EIN registration number", 0, consigneeRegNums.Length);

				var sellerRegNums = (OrgCusCode[])sellerOrg.CustomsCodes.Find(new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.ManufacturerID));
				AssertEquals("Number of MID registration number", 0, sellerRegNums.Length);
			});
		}

		public void TestOrganizationAddressCollection_WhenCreatingConsolidatedSummary_ShouldCreateRegNums()
		{
			var consignmentBO = Factory.New<CusUSLVConsignment>();
			consignmentBO.ULB_HouseBill = "HBL12345";
			consignmentBO.ULB_ConsigneeIdentifier = "Consignee1234";
			consignmentBO.ULB_SellerIdentifier = "Seller1234";
			SetupConsigneeAndSellerDetailManually(consignmentBO);

			var actionInfo = new ActionInfo(null, consignmentBO);
			actionInfo.ActionType = WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage;

			var writer = new USLVConsignmentDataWriter(new DataWritingManager(actionInfo));
			var consignmentDataObject = writer.GetDataObject(consignmentBO);
			var tempOrgs = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsTempAccount, true));

			CombineAssertions(() =>
			{
				AssertEquals("Number of temporary orgs", 2, tempOrgs.Length);

				var consigneeOrg = tempOrgs.SingleOrDefault(x => x.OH_FullName == "cName");
				AssertNotNull("Consignee org", consigneeOrg);

				var consigneeRegNums = (OrgCusCode[])consigneeOrg.CustomsCodes.Find(new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.EmployerIdentificationNumber));
				AssertEquals("EIN registration number", 1, consigneeRegNums.Length);
				AssertEquals("EIN code", "Consignee1234", consigneeRegNums[0].OK_CustomsRegNo);
				AssertEquals("EIN country", "US", consigneeRegNums[0].OK_RN_NKCodeCountry);

				var sellerOrg = tempOrgs.SingleOrDefault(x => x.OH_FullName == "sName");
				AssertNotNull("Seller org", sellerOrg);

				var sellerRegNums = (OrgCusCode[])sellerOrg.CustomsCodes.Find(new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.ManufacturerID));
				AssertEquals("MID registration number", 1, sellerRegNums.Length);
				AssertEquals("MID code", "Seller1234", sellerRegNums[0].OK_CustomsRegNo);
				AssertEquals("MID country", "US", sellerRegNums[0].OK_RN_NKCodeCountry);
				AssertEquals("MID premises address", sellerOrg.Addresses.FirstOrDefault().PK, sellerRegNums[0].OK_OA_PremisesAddress);
			});
		}

		protected CusUSLVConsignment SetupULSVConsignmentBO()
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var clearance = USLVClearanceDataWriterTest.SetupULSVClearanceBO();
			var seller = GetOrganizationBO_WUFSHIJNB(factory).MainAddress;
			var consignee = GetOrganizationBO_INTHEMSYD(factory).MainAddress;

			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_OwnerReferenceNumber = "8974578";
			consignment.ULB_HouseBill = "HouseBill1";
			consignment.ULB_HouseBillIssuerSCAC = "SCAC";
			consignment.ULB_EquipmentNumber = "EQ";
			consignment.ULB_NumberOfPacks = 1;
			consignment.ULB_PackType = "PKG";
			consignment.ULB_NonAMSIndicator = true;
			consignment.ULB_IsResponsePending = true;
			consignment.ULB_SubmittedDate = new ZDate(2020, 08, 04);
			consignment.ULB_MessageStatus = "4";
			consignment.ULB_OA_Consignee = consignee.PK;
			consignment.ULB_OA_Seller = seller.PK;
			consignment.ULB_ConsigneeQualifier = "AB";
			consignment.ULB_ConsigneeIdentifier = "BC";
			consignment.CE_RailReferenceNumber = "RRN123";
			SetCommercialInfo(consignment);
			factory.Save();
			return consignment;
		}

		void SetupConsigneeAndSellerDetailManually(CusUSLVConsignment consignment)
		{
			var consigneeCountry = Factory.New<RefCountry>();
			consigneeCountry.RN_Code = "CC";
			consignment.ULB_OA_Consignee = ZGuid.Empty;
			consignment.ULB_ConsigneeName = "cName";
			consignment.ULB_ConsigneeAddress1 = "cAddress1";
			consignment.ULB_ConsigneeAddress2 = "cAddress2";
			consignment.ULB_ConsigneeCity = "cCity";
			consignment.ULB_ConsigneePostCode = "cCode";
			consignment.ULB_ConsigneeState = "cState";
			consignment.ULB_RN_NKConsigneeCountry = consigneeCountry.RN_Code;

			var sellerCountry = Factory.New<RefCountry>();
			sellerCountry.RN_Code = "SC";
			consignment.ULB_OA_Seller = ZGuid.Empty;
			consignment.ULB_SellerName = "sName";
			consignment.ULB_SellerAddress1 = "sAddress1";
			consignment.ULB_SellerAddress2 = "sAddress2";
			consignment.ULB_SellerCity = "sCity";
			consignment.ULB_SellerPostCode = "sCode";
			consignment.ULB_SellerState = "sState";
			consignment.ULB_RN_NKSellerCountry = sellerCountry.RN_Code;
		}

		protected void SetCommercialInfo(CusUSLVConsignment consignment)
		{
			var uSLVItem1 = Factory.New<CusUSLVItem>();
			uSLVItem1.ULI_RN_NKCountryOfOrigin = "US";
			uSLVItem1.ULI_GoodsDescription = "GoodsDescription1";
			uSLVItem1.ULI_Tariff = "1011";
			uSLVItem1.ULI_GoodsValue = 111m;
			uSLVItem1.ULI_RX_NKCurrency = "AAA";
			uSLVItem1.ULI_AntiDumping = true;
			uSLVItem1.ULI_Countervailing = true;
			uSLVItem1.ULI_PartNo = "AA1";
			SetPGA(uSLVItem1);
			consignment.CusUSLVItems.Add(uSLVItem1);
			var uSLVItem2 = Factory.New<CusUSLVItem>();
			uSLVItem2.ULI_RN_NKCountryOfOrigin = "US";
			uSLVItem2.ULI_GoodsDescription = "GoodsDescription2";
			uSLVItem2.ULI_Tariff = "2022";
			uSLVItem2.ULI_GoodsValue = 111m;
			uSLVItem2.ULI_RX_NKCurrency = "BBB";
			uSLVItem2.ULI_AntiDumping = true;
			uSLVItem2.ULI_Countervailing = false;
			uSLVItem2.ULI_PartNo = "BB1";
			consignment.CusUSLVItems.Add(uSLVItem2);
			SetPGA(uSLVItem2);
			var uSLVItem3 = Factory.New<CusUSLVItem>();
			uSLVItem3.ULI_GoodsDescription = "GoodsDescription3";
			uSLVItem3.ULI_Tariff = "3033";
			uSLVItem3.ULI_GoodsValue = 111m;
			uSLVItem3.ULI_RX_NKCurrency = "BBB";
			uSLVItem3.ULI_AntiDumping = false;
			uSLVItem3.ULI_Countervailing = true;
			uSLVItem3.ULI_PartNo = "BB2";
			consignment.CusUSLVItems.Add(uSLVItem3);
			SetPGA(uSLVItem3);
			var uSLVItem4 = Factory.New<CusUSLVItem>();
			uSLVItem4.ULI_RN_NKCountryOfOrigin = "US";
			uSLVItem4.ULI_GoodsDescription = "GoodsDescription4";
			uSLVItem4.ULI_Tariff = "4044";
			uSLVItem4.ULI_GoodsValue = 111m;
			uSLVItem4.ULI_RX_NKCurrency = "CCC";
			uSLVItem4.ULI_AntiDumping = false;
			uSLVItem4.ULI_Countervailing = false;
			uSLVItem4.ULI_PartNo = "CC1";
			SetPGA(uSLVItem4);
			consignment.CusUSLVItems.Add(uSLVItem4);
		}

		void SetPGA(CusUSLVItem uSLVitem)
		{
			var pga1 = uSLVitem.CusUSLVItemPGAs.AddNew();
			pga1.ULP_Agency = "APH";
			pga1.ULP_AgencyProgram = "AVS";
			pga1.ULP_DisclaimReason = "A";

			var pga2 = uSLVitem.CusUSLVItemPGAs.AddNew();
			pga2.ULP_Agency = "CPS";
			pga2.ULP_AgencyProgram = "CPS";
			pga2.ULP_DisclaimReason = "C";

			var pga3 = uSLVitem.CusUSLVItemPGAs.AddNew();
			pga3.ULP_Agency = "DEA";
			pga3.ULP_AgencyProgram = "DEA";
			pga3.ULP_DisclaimReason = "D";

			var pga4 = uSLVitem.CusUSLVItemPGAs.AddNew();
			pga4.ULP_Agency = "NHT";
			pga4.ULP_AgencyProgram = "OFF";
			pga4.ULP_DisclaimReason = "N";

			var pga5 = uSLVitem.CusUSLVItemPGAs.AddNew();
			pga5.ULP_Agency = "EPA";
			pga5.ULP_AgencyProgram = "ODS";
			pga5.ULP_DisclaimReason = "O";

			var pga6 = uSLVitem.CusUSLVItemPGAs.AddNew();
			pga6.ULP_Agency = "EPA";
			pga6.ULP_AgencyProgram = "PS1";
			pga6.ULP_DisclaimReason = "P";

			var pga7 = uSLVitem.CusUSLVItemPGAs.AddNew();
			pga7.ULP_Agency = "EPA";
			pga7.ULP_AgencyProgram = "TS1";
			pga7.ULP_DisclaimReason = "T";

			var pga8 = uSLVitem.CusUSLVItemPGAs.AddNew();
			pga8.ULP_Agency = "EPA";
			pga8.ULP_AgencyProgram = "VNE";
			pga8.ULP_DisclaimReason = "V";

			var pga9 = uSLVitem.CusUSLVItemPGAs.AddNew();
			pga9.ULP_Agency = "FDA";
			pga9.ULP_AgencyProgram = "FDA";
			pga9.ULP_DisclaimReason = "F";

			var pga10 = uSLVitem.CusUSLVItemPGAs.AddNew();
			pga10.ULP_Agency = "FWS";
			pga10.ULP_AgencyProgram = "FWS";
			pga10.ULP_DisclaimReason = "W";

			var pga11 = uSLVitem.CusUSLVItemPGAs.AddNew();
			pga11.ULP_Agency = "APH";
			pga11.ULP_AgencyProgram = "APL";
			pga11.ULP_DisclaimReason = "H";

			var pga12 = uSLVitem.CusUSLVItemPGAs.AddNew();
			pga12.ULP_Agency = "NMF";
			pga12.ULP_AgencyProgram = "370";
			pga12.ULP_DisclaimReason = "3";

			var pga13 = uSLVitem.CusUSLVItemPGAs.AddNew();
			pga13.ULP_Agency = "NMF";
			pga13.ULP_AgencyProgram = "AMR";
			pga13.ULP_DisclaimReason = "M";

			var pga14 = uSLVitem.CusUSLVItemPGAs.AddNew();
			pga14.ULP_Agency = "NMF";
			pga14.ULP_AgencyProgram = "HMS";
			pga14.ULP_DisclaimReason = "S";

			var pga15 = uSLVitem.CusUSLVItemPGAs.AddNew();
			pga15.ULP_Agency = "OMC";
			pga15.ULP_AgencyProgram = "OMC";
			pga15.ULP_DisclaimReason = "M";

			var pga16 = uSLVitem.CusUSLVItemPGAs.AddNew();
			pga16.ULP_Agency = "TTB";
			pga16.ULP_AgencyProgram = "TOB";
			pga16.ULP_DisclaimReason = "T";

			var pga17 = uSLVitem.CusUSLVItemPGAs.AddNew();
			pga17.ULP_Agency = "AMS";
			pga17.ULP_AgencyProgram = "MO8";
			pga17.ULP_DisclaimReason = "8";

			var pga18 = uSLVitem.CusUSLVItemPGAs.AddNew();
			pga18.ULP_Agency = "FSI";
			pga18.ULP_AgencyProgram = "FSI";
			pga18.ULP_DisclaimReason = "F";

			var pga19 = uSLVitem.CusUSLVItemPGAs.AddNew();
			pga19.ULP_Agency = "NOP";
			pga19.ULP_AgencyProgram = "OR1";
			pga19.ULP_DisclaimReason = "9";
		}

		protected void AssertTestToConsignment(CusUSLVConsignment consignment, Shipment shipment)
		{
			CombineAssertions(() =>
			{
				AssertEquals("consignment.ULB_OwnerReferenceNumber", consignment.ULB_OwnerReferenceNumber, shipment.OwnerRef);
				AssertEquals("consignment.ULB_HouseBill", consignment.ULB_HouseBill, shipment.WayBillNumber);
				AssertEquals("consignment.ULB_NumberOfPacks", consignment.ULB_NumberOfPacks.ToZInt(), shipment.TotalNoOfPacks);
				AssertEquals("consignment.ULB_PackType", consignment.ULB_PackType, shipment.TotalNoOfPacksPackageType.Code);

				AssertHouseAdditionalBill("House Additional Bill", consignment, shipment, consignment.ULB_HouseBill, new WayBillType() { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House });

				AssertAddInfo("WayBillIssuerSCAC", shipment.AddInfoCollection, LVSConstants.AddInfoConstants.WayBillIssuerSCAC, consignment.ULB_HouseBillIssuerSCAC);
				AssertAddInfo("NonAMS", shipment.AddInfoCollection, LVSConstants.AddInfoConstants.NonAMS, consignment.ULB_NonAMSIndicator ? YesNoList.Codes.Yes : YesNoList.Codes.No);
				AssertAddInfo("ConsigneeType", shipment.AddInfoCollection, LVSConstants.AddInfoConstants.ConsigneeType, consignment.ULB_ConsigneeQualifier);
				AssertAddInfo("ConsigneeReference", shipment.AddInfoCollection, LVSConstants.AddInfoConstants.ConsigneeReference, consignment.ULB_ConsigneeIdentifier);

				AssertContainer(shipment, consignment.ULB_EquipmentNumber.IsEmpty ? null : consignment.ULB_EquipmentNumber);
				AssertOrganizationAddressContents(shipment, Constants.AddressTypes.UltimateConsignee, consignment.Consignee);
				AssertEquals("CE_EntryNum", consignment.CE_EntryNum.IsEmpty ? null : (ZString?)consignment.CE_EntryNum, shipment.EntryNumberCollection?.FirstOrDefault()?.Number);
				AssertOrganizationAddressContents(shipment, AddressType.Seller, consignment.Seller);
				AssertCommercialInvoice(shipment, consignment);
			});
		}

		protected void AssertContainer(Shipment shipment, ZString? expectedContainerNumber)
		{
			var container = shipment.ContainerCollection?.FirstOrDefault();
			AssertEquals("container.ContainerNumber", expectedContainerNumber, container?.ContainerNumber);
		}

		protected void AssertCommercialInvoice(Shipment shipment, CusUSLVConsignment consignment)
		{
			var listItems = new List<CusUSLVItem>();
			consignment.CusUSLVItems.CopyToList(listItems);
			var commercialInvoiceArr = listItems.Select(n => n.ULI_RX_NKCurrency).ToArray().Distinct();
			AssertEquals("InvoiceCount", shipment.CommercialInfo.CommercialInvoiceCollection.Count, commercialInvoiceArr.Count());

			foreach (var item in commercialInvoiceArr)
			{
				var commercialInvoiceBO = listItems.Where(n => n.ULI_RX_NKCurrency == item);
				var commercialInvoice = shipment.CommercialInfo.CommercialInvoiceCollection.FirstOrDefault(n => n.InvoiceCurrency.Code.ToString() == item).CommercialInvoiceLineCollection;
				AssertEquals(commercialInvoice.Count, commercialInvoiceBO.Count());
				AssertCommercialInvoiceLine(commercialInvoice, consignment);
			}

			var uSLVItem = (CusUSLVItem)consignment.CusUSLVItems.FirstOrDefault();
			AssertAgencyAddInfo(shipment, uSLVItem, "APHISDisclaimReason", "APH", "AVS");
			AssertAgencyAddInfo(shipment, uSLVItem, "CPSCDisclaimReason", "CPS", "CPS");
			AssertAgencyAddInfo(shipment, uSLVItem, "DEADisclaimReason", "DEA", "DEA");
			AssertAgencyAddInfo(shipment, uSLVItem, "NHTDisclaimReason", "NHT", "OFF");
			AssertAgencyAddInfo(shipment, uSLVItem, "ODSDisclaimReason", "EPA", "ODS");
			AssertAgencyAddInfo(shipment, uSLVItem, "PSTDisclaimReason", "EPA", "PS1");
			AssertAgencyAddInfo(shipment, uSLVItem, "TSCADisclaimReason", "EPA", "TS1");
			AssertAgencyAddInfo(shipment, uSLVItem, "VNEDisclaimReason", "EPA", "VNE");
			AssertAgencyAddInfo(shipment, uSLVItem, "FDADisclaimReason", "FDA", "FDA");
			AssertAgencyAddInfo(shipment, uSLVItem, "FWSDisclaimReason", "FWS", "FWS");
			AssertAgencyAddInfo(shipment, uSLVItem, "LaceyDisclaimReason", "APH", "APL");
			AssertAgencyAddInfo(shipment, uSLVItem, "NMFS370DisclaimReason", "NMF", "370");
			AssertAgencyAddInfo(shipment, uSLVItem, "NMFSAMRDisclaimReason", "NMF", "AMR");
			AssertAgencyAddInfo(shipment, uSLVItem, "NMFSHMSDisclaimReason", "NMF", "HMS");
			AssertAgencyAddInfo(shipment, uSLVItem, "OMCDisclaimReason", "OMC", "OMC");
			AssertAgencyAddInfo(shipment, uSLVItem, "TTBDisclaimReason", "TTB", "TOB");
			AssertAgencyAddInfo(shipment, uSLVItem, "AMSDisclaimReason", "AMS", "MO8");
			AssertAgencyAddInfo(shipment, uSLVItem, "NOPDisclaimReason", "NOP", "OR1");
			AssertAgencyAddInfo(shipment, uSLVItem, "FSISDisclaimReason", "FSI", "FSI");
		}

		void AssertCommercialInvoiceLine(IEnumerable<CommercialInvoiceLine> invoiceLines, CusUSLVConsignment consignment)
		{
			foreach (var invoiceLine in invoiceLines)
			{
				var cusUSLVItem = consignment.CusUSLVItems.Cast<CusUSLVItem>().FirstOrDefault(n => n.ULI_GoodsDescription == (ZString)invoiceLine.Description);
				AssertEquals(cusUSLVItem.ULI_RN_NKCountryOfOrigin, invoiceLine.CountryOfOrigin?.Code);
				AssertEquals(cusUSLVItem.ULI_Tariff, invoiceLine.HarmonisedCode);
				AssertEquals(cusUSLVItem.ULI_GoodsValue, invoiceLine.LinePrice);
				AssertAddInfo("ULI_AntiDumping", invoiceLine.AddInfoCollection, LVSConstants.AddInfoConstants.ADD_NA, cusUSLVItem.ULI_AntiDumping ? YesNoList.Codes.Yes : YesNoList.Codes.No);
				AssertAddInfo("ULI_Countervailing", invoiceLine.AddInfoCollection, LVSConstants.AddInfoConstants.CVD_NA, cusUSLVItem.ULI_Countervailing ? YesNoList.Codes.Yes : YesNoList.Codes.No);
			}
		}

		protected void AssertHouseAdditionalBill(ZString message, CusUSLVConsignment consignment, Shipment shipment, ZString billNumber, WayBillType expectedValue)
		{
			var additionalBill = shipment.AdditionalBillCollection.FirstOrDefault(x => x.BillNumber.HasValue && x.BillNumber.Value == billNumber);
			AssertEquals(message, expectedValue.Code, additionalBill.BillType.Code);
			AssertEquals(message, expectedValue.Description, additionalBill.BillType.Description);
			AssertAddInfo(message, additionalBill.AddInfoCollection, LVSConstants.AddInfoConstants.UI_NKBillIssuerSCAC, consignment.ULB_HouseBillIssuerSCAC);
		}

		void AssertAgencyAddInfo(Shipment shipment, CusUSLVItem uSLVItem, string disclaimReasonKey, string agency, string agencyProgram)
		{
			var cusUSLVPGAs = uSLVItem.CusUSLVItemPGAs.ToArray<CusUSLVItemPGA>();
			var pagItemReason = shipment.CommercialInfo.CommercialInvoiceCollection.FirstOrDefault().CommercialInvoiceLineCollection.FirstOrDefault().AddInfoCollection.FirstOrDefault(n => n.Key.ToString() == disclaimReasonKey);
			var pagItemBO = cusUSLVPGAs.FirstOrDefault(n => n.ULP_Agency == agency && n.ULP_AgencyProgram == agencyProgram);
			AssertEquals(pagItemReason.Value, pagItemBO.ULP_DisclaimReason);
		}

		void TestSetupConsigneeAndSellerDetailManually(CusUSLVConsignment consignment)
		{
			SetupConsigneeAndSellerDetailManually(consignment);
			var consignmentData = new USLVConsignmentDataWriter(new DataWritingManager(new ActionInfo(null, consignment))).GetDataObject(consignment);
			AssertConsigneeAndSellerManually(consignmentData, consignment);
		}

		void AssertConsigneeAndSellerManually(Shipment shipment, CusUSLVConsignment consignment)
		{
			var addressConsignee = shipment.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == Constants.AddressTypes.UltimateConsignee);
			AssertNotNull(addressConsignee);
			AssertEquals(addressConsignee.CompanyName, consignment.ULB_ConsigneeName);
			AssertEquals(addressConsignee.Address1, consignment.ULB_ConsigneeAddress1);
			AssertEquals(addressConsignee.Address2, consignment.ULB_ConsigneeAddress2);
			AssertEquals(addressConsignee.City, consignment.ULB_ConsigneeCity);
			AssertEquals(addressConsignee.Postcode, consignment.ULB_ConsigneePostCode);
			AssertEquals(addressConsignee.Country.Code, consignment.ULB_RN_NKConsigneeCountry);

			var addressSeller = shipment.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == AddressType.Seller);
			AssertNotNull(addressSeller);
			AssertEquals(addressSeller.CompanyName, consignment.ULB_SellerName);
			AssertEquals(addressSeller.Address1, consignment.ULB_SellerAddress1);
			AssertEquals(addressSeller.Address2, consignment.ULB_SellerAddress2);
			AssertEquals(addressSeller.City, consignment.ULB_SellerCity);
			AssertEquals(addressSeller.Postcode, consignment.ULB_SellerPostCode);
			AssertEquals(addressSeller.Country.Code, consignment.ULB_RN_NKSellerCountry);
		}

		void AssertConsigneeAndSellerTemporaryOrgContents(CusUSLVConsignment consignment, OrgHeader consigneeOrg, OrgHeader sellerOrg)
		{
			Environment.Env.Registry.CanUserEditOrganisationCode = true;
			consigneeOrg.IgnoreValidationSuspended = true;
			sellerOrg.IgnoreValidationSuspended = true;

			Assert(consigneeOrg.OH_IsTempAccount);
			AssertStartsWith("Code keeps right 3 alphanumeric characters", "_345", consigneeOrg.OH_Code);
			consigneeOrg.Validation.ValidateOH_Code();
			AssertNoNotifications(consigneeOrg.OH_CodeInfo);
			Assert(consigneeOrg.OH_IsConsignee);
			var addressConsignee = consigneeOrg.MainAddress;
			AssertNotNull(addressConsignee);
			AssertEquals(addressConsignee.CompanyName, "cName");
			AssertEquals(addressConsignee.Address1, "cAddress1");
			AssertEquals(addressConsignee.Address2, "cAddress2");
			AssertEquals(addressConsignee.City, "cCity");
			AssertEquals(addressConsignee.State, "cState");
			AssertEquals(addressConsignee.Postcode, "cCode");
			AssertEquals(addressConsignee.Country.Code, "CC");

			Assert(sellerOrg.OH_IsTempAccount);
			AssertStartsWith("Code keeps right 3 alphanumeric characters", "_345", sellerOrg.OH_Code);
			sellerOrg.Validation.ValidateOH_Code();
			AssertNoNotifications(consigneeOrg.OH_CodeInfo);
			Assert(sellerOrg.OH_IsConsignor);
			var addressSeller = sellerOrg.MainAddress;
			AssertNotNull(addressSeller);
			AssertEquals(addressSeller.CompanyName, "sName");
			AssertEquals(addressSeller.Address1, "sAddress1");
			AssertEquals(addressSeller.Address2, "sAddress2");
			AssertEquals(addressSeller.City, "sCity");
			AssertEquals(addressSeller.State, "sState");
			AssertEquals(addressSeller.Postcode, "sCode");
			AssertEquals(addressSeller.Country.Code, "SC");
		}
	}
}
