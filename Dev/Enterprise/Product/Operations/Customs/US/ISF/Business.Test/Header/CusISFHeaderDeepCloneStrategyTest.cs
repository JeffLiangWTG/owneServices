using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class CusISFHeaderDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestCusISFHeaderNotCopyShipmentPK()
		{
			var consignor = Factory.New<OrgHeader>();
			var consignee = Factory.New<OrgHeader>();
			var manufacturer = Factory.New<OrgHeader>();

			#region CreateConsol
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "OIF";
			org.OH_FullName = "ISFORG";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "VOCC";
			cusCode.OK_RN_NKCodeCountry = "US";
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
			consol.JK_ConsolMode = "BCN";

			#endregion

			#region CreateShipment

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_E_DEP = ZDateTime.Today;
			shipment.JS_RL_NKDestination = "USNYK";
			shipment.JS_E_ARV = ZDateTime.Today.AddDays(19);
			shipment.JS_GoodsValue = 1400m;
			shipment.JS_RX_NKGoodsValueCurr = "USD";
			shipment.JS_TotalPackageCount = 150;
			shipment.JS_F3_NKTotalCountPackType = ShippingOrPackingingUnitList.Codes.Roll;
			shipment.JS_ActualWeight = 1.51m;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Tonnes;

			consol.Shipments.Add(shipment);
			shipment.ManufacturerDocAddress.OrganisationPK = manufacturer.PK;
			shipment.JS_ShipmentType = "STD";

			#endregion

			var creator = new ISFFromShipmentCreator(Factory);
			creator.ShipmentPK = shipment.PK;
			var isf = creator.Create(Factory);
			AssertEquals("Should be null. Only copy shipmentPK when HVLV shipment type", ZGuid.Empty, isf.BF_JS_Shipment);
		}

		public void TestCusISFHeaderClone()
		{
			CusISFHeader header = GetHeaderToClone();
			AssertNotEquals(ZGuid.Empty, header.BF_OH_Importer);
			AssertNotEquals(ZString.Empty, header.BF_CustomsReference);
			AssertNotEquals(ZString.Empty, header.BF_CustomsStatus);
			AssertNotEquals(ZString.Empty, header.BF_JobReference);
			AssertEquals(true, header.BF_IsCancelled);
			Factory.Save();
			CusISFHeader clonedHeader = (CusISFHeader)new CusISFHeaderDeepCloneStrategy(header).Clone(new BusinessObjectCloneArgs(System.Array.Empty<string>(), true));
			AssertCusISFHeader(header, clonedHeader);
			AssertEquals(ZGuid.Empty, clonedHeader.BF_JS_Shipment);
			AssertCusISFBills(clonedHeader.ReferenceDatas);
			AssertCusISFEquips(header.Equipments, clonedHeader.Equipments);
			AssertCusISFLines(header.Lines, clonedHeader.Lines);
			AssertDocAddresses(header, clonedHeader);
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CusISFHeader reloadedHeader = newFactory.Load<CusISFHeader>(clonedHeader.PK);
			AssertEquals(ZGuid.Empty, reloadedHeader.BF_JS_Shipment);
			AssertCusISFHeader(header, reloadedHeader);
			AssertCusISFBills(reloadedHeader.ReferenceDatas);
			AssertCusISFEquips(header.Equipments, reloadedHeader.Equipments);
			AssertCusISFLines(header.Lines, reloadedHeader.Lines);
			AssertDocAddresses(header, reloadedHeader);
			AssertCusISFNotes(header, reloadedHeader);
		}

		void AssertDocAddresses(CusISFHeader header, CusISFHeader clonedHeader)
		{
			AssertEquals(header.DocAddresses.Count, clonedHeader.DocAddresses.Count);
			AssertDocAddress(header.MainShipToParty, clonedHeader.MainShipToParty);
			header.DocAddresses.Sort(ISFDocAddress.Schema.AddressCaption, System.ComponentModel.ListSortDirection.Descending);
			clonedHeader.DocAddresses.Sort(ISFDocAddress.Schema.AddressCaption, System.ComponentModel.ListSortDirection.Descending);
			for (int i = 0; i < header.DocAddresses.Count; i++)
			{
				ISFDocAddress docAddress = header.DocAddresses[0];
				ISFDocAddress clonedDocAddress = clonedHeader.DocAddresses[0];
				AssertDocAddress(docAddress, clonedDocAddress);
			}
		}

		void AssertDocAddress(ISFDocAddress docAddress, ISFDocAddress clonedDocAddress)
		{
			AssertNotEquals(docAddress.PK, clonedDocAddress.PK);
			AssertNotEquals(docAddress.E2_ParentID, clonedDocAddress.E2_ParentID);
			AssertEquals(docAddress.E2_AddressOverride, clonedDocAddress.E2_AddressOverride);
			AssertEquals(docAddress.E2_OA_Address, clonedDocAddress.E2_OA_Address);
			AssertEquals(docAddress.E2_CompanyName, clonedDocAddress.E2_CompanyName);
			AssertEquals(docAddress.E2_Address1, clonedDocAddress.E2_Address1);
		}

		void AssertCusISFLines(CusISFLineCollection lines, CusISFLineCollection clonedLines)
		{
			AssertEquals(lines.Count, clonedLines.Count);
			for (int i = 0; i < lines.Count; i++)
			{
				CusISFLine line = lines[i];
				CusISFLine clonedLine = clonedLines[i];
				AssertNotEquals(line.BL_BF, clonedLine.BL_BF);
				AssertEquals(line.BL_HarmonisedNum, clonedLine.BL_HarmonisedNum);
				AssertEquals(line.BL_OP, clonedLine.BL_OP);
				AssertEquals(line.BL_RN_NKGoodsOrigin, clonedLine.BL_RN_NKGoodsOrigin);
				AssertEquals(line.BL_TextProductCode, clonedLine.BL_TextProductCode);
				if (line.BL_ManufacturerDocAddressPK.IsEmpty)
				{
					AssertEquals(ZGuid.Empty, clonedLine.BL_ManufacturerDocAddressPK);
				}
				else
				{
					AssertNotEquals(ZGuid.Empty, clonedLine.BL_ManufacturerDocAddressPK);
				}
			}
		}

		void AssertCusISFEquips(CusISFEquipCollection equipments, CusISFEquipCollection clonedEquipments)
		{
			AssertEquals("Containers should not be copied", 0, clonedEquipments.Count);
		}

		void AssertCusISFBills(CusISFBillCollection clonedReferenceDatas)
		{
			AssertEquals(2, clonedReferenceDatas.Count);
			clonedReferenceDatas.ApplySort(CusISFBill.Schema.BB_BillType, System.ComponentModel.ListSortDirection.Descending);
			CusISFBill clonedBill = clonedReferenceDatas[0];
			AssertEquals(BillTypeList.Codes.SuretyCode, clonedBill.BB_BillType);
			AssertEquals("798", clonedBill.BB_BillNum);
			AssertEquals(ZString.Empty, clonedBill.BB_CustomsStatus);
			clonedBill = clonedReferenceDatas[1];
			AssertEquals(BillTypeList.Codes.FullNameOfISFImporter, clonedBill.BB_BillType);
			AssertEquals("BOB THE BUILDER", clonedBill.BB_BillNum);
			AssertEquals(ZString.Empty, clonedBill.BB_CustomsStatus);
		}

		void AssertCusISFHeader(CusISFHeader header, CusISFHeader clonedHeader)
		{
			AssertEquals(GlbBranch.CurrentBranch.PK, clonedHeader.BF_GB);
			AssertNotEquals("The cloned ISF should default the BF_GB to its current Branch, not that of the copied ISF", header.BF_GB, clonedHeader.BF_GB);
			AssertEquals(ZString.Empty, clonedHeader.BF_CustomsReference);
			AssertEquals("NOT", clonedHeader.BF_CustomsStatus);
			AssertEquals(ZString.Empty, clonedHeader.BF_OwnerReference);
			AssertEquals(ZDateTime.Empty, clonedHeader.BF_FirstAcceptedDate);
			AssertEquals(ZDateTime.Empty, clonedHeader.BF_LastAcceptedDate);
			AssertNotEquals(header.BF_JobReference, clonedHeader.BF_JobReference);
			AssertEquals(SubmissionTypeList.Codes.ISF10, clonedHeader.BF_EntryType);
			AssertEquals(ShipmentTypeList.Codes.FTZShipments, clonedHeader.BF_ShipmentType);
			AssertEquals("CGNEE123", clonedHeader.BF_ConsigneeCode);
			AssertEquals(CodeTypeList.Codes.DUNS, clonedHeader.BF_ConsigneeCodeType);
			AssertEquals(Core.Constants.CountryCodes.Andorra, clonedHeader.BF_CountryOfIssue);
			AssertEquals(new ZDateTime(1978, 3, 4), clonedHeader.BF_DateOfBirth);
			AssertEquals("", clonedHeader.BF_EntryNumber);
			AssertEquals("", clonedHeader.BF_HouseBill);
			AssertEquals(header.BF_OH_Importer, clonedHeader.BF_OH_Importer);
			AssertEquals("IMP32324", clonedHeader.BF_ImporterCode);
			AssertEquals(CodeTypeList.Codes.DUNSPlus4, clonedHeader.BF_ImporterCodeType);
			AssertEquals("", clonedHeader.BF_MasterBill);
			AssertEquals("", clonedHeader.BF_OceanBill);
			AssertEquals("USLAX", clonedHeader.BF_RL_NKPlaceOfDelivery);
			AssertEquals("USCHI", clonedHeader.BF_RL_NKPortOfUnload);
			AssertEquals("CQTP", clonedHeader.BF_SCAC);
			AssertEquals("BNHLD123", clonedHeader.BF_BondNumberOrHolder);
			AssertEquals(ISFBondActivityCodeList.Codes.ISFBond16, clonedHeader.BF_BondActivityCode);
			AssertEquals(Enterprise.Customs.US.Business.BondTypeList.Codes.SingleTransactionBond, clonedHeader.BF_BondType);
			AssertEquals("798", clonedHeader.BF_SuretyCode);
			AssertEquals(ZString.Empty, clonedHeader.BF_BondReferenceNumber);
			AssertEquals(ShipmentSubTypeList.Codes.InformalShipments, clonedHeader.BF_ShipmentSubType);
			AssertEquals("WENDY THE DESTROYER", clonedHeader.BF_ConsigneeFullName);
			AssertEquals(323, clonedHeader.BF_EstimatedQuantity);
			AssertEquals(4983m, clonedHeader.BF_EstimatedValue);
			AssertEquals(34, clonedHeader.BF_EstimatedWeight);
			AssertEquals(Core.Constants.Weight.Kilotonnes, clonedHeader.BF_EstimatedWeightUQ);
		}

		void AssertCusISFNotes(CusISFHeader header, CusISFHeader clonedHeader)
		{
			var notesFromCusISFHeader = clonedHeader.Notes.FindByDescription("TestISFClone");
			AssertEquals(0, notesFromCusISFHeader.Length);
		}

		CusISFHeader GetHeaderToClone()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader importer = factory.New<OrgHeader>();
			importer.OH_FullName = "IMPORTER COMPANY";
			importer.OH_RL_NKClosestPort = "USCHI";
			importer.MainAddress.OA_Address1 = "IMPORTER ADDRESS 1";
			importer.MainAddress.OA_Address2 = "IMPORTER ADDRESS 2";
			importer.MainAddress.OA_City = "CHICAGO";
			importer.MainAddress.OA_State = "IL";
			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "PAS1233343");
			OrgHeader consolidator = factory.New<OrgHeader>();
			consolidator.OH_FullName = "CONSOLIDATOR COMPANY";
			consolidator.OH_RL_NKClosestPort = "USCHI";
			consolidator.MainAddress.OA_Address1 = "CONSOLIDATOR ADDRESS 1";
			consolidator.MainAddress.OA_Address2 = "CONSOLIDATOR ADDRESS 2";
			consolidator.MainAddress.OA_City = "CHICAGO";
			consolidator.MainAddress.OA_State = "IL";
			RefCountry unitedStates = factory.Load<RefCountry>(Core.Constants.CountryGuids.UnitedStates);
			OrgHeader manufacturer1 = factory.New<OrgHeader>();
			manufacturer1.OH_FullName = "MANUFACTURER1 COMPANY";
			manufacturer1.OH_RL_NKClosestPort = "AUSYD";
			manufacturer1.MainAddress.OA_Address1 = "MANUFACTURER1 ADDRESS 1";
			manufacturer1.MainAddress.OA_Address2 = "MANUFACTURER1 ADDRESS 2";
			manufacturer1.MainAddress.OA_City = "SYDNEY";
			manufacturer1.MainAddress.OA_State = "NSW";
			manufacturer1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "653478956", unitedStates);
			US.Business.OrgSupplierPart part = (US.Business.OrgSupplierPart)factory.New<Integration.Customs.US.IOrgSupplierPart>();
			part.OP_PartNum = "DUMMYTEST1PART";
			part.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			GlbCompany company = Factory.New<GlbCompany>();
			GlbBranch branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_Code = "~12";
			branch.GB_RL_NKHomePort = "USLAX";
			factory.Save();
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_GB = branch.PK;
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_ShipmentType = ShipmentTypeList.Codes.FTZShipments;
			header.BF_OH_Importer = importer.PK;
			header.BF_ImporterCodeType = CodeTypeList.Codes.DUNSPlus4;
			header.BF_ImporterCode = "IMP32324";
			header.BF_ImporterFullName = "BOB THE BUILDER";
			header.BF_ConsigneeCodeType = CodeTypeList.Codes.DUNS;
			header.BF_ConsigneeCode = "CGNEE123";
			header.BF_CountryOfIssue = Core.Constants.CountryCodes.Andorra;
			header.BF_CustomsReference = "ZZZ32523";
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFAdd;
			header.BF_DateOfBirth = new ZDateTime(1978, 3, 4);
			header.BF_EntryNumber = "CBP65913212";
			header.BF_HouseBill = "HB3243";
			header.BF_JobReference = "BZZ23432";
			header.BF_MasterBill = "MB32342";
			header.BF_OceanBill = "OB32423";
			header.BF_RL_NKPlaceOfDelivery = "USLAX";
			header.BF_RL_NKPortOfUnload = "USCHI";
			header.BF_SCAC = "CQTP";
			header.BF_BondNumberOrHolder = "BNHLD123";
			header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.ISFBond16;
			header.BF_BondType = Enterprise.Customs.US.Business.BondTypeList.Codes.SingleTransactionBond;
			header.BF_SuretyCode = "798";
			header.BF_BondReferenceNumber = "BD23432";
			header.BF_OwnerReference = "OWNERREF";
			header.BF_FirstAcceptedDate = ZDateTime.Now;
			header.BF_LastAcceptedDate = ZDateTime.Now;
			header.BF_IsCancelled = true;
			header.BF_ShipmentSubType = ShipmentSubTypeList.Codes.InformalShipments;
			header.BF_ConsigneeFullName = "WENDY THE DESTROYER";
			header.BF_EstimatedQuantity = 323;
			header.BF_EstimatedValue = 4983m;
			header.BF_EstimatedWeight = 34;
			header.BF_EstimatedWeightUQ = Core.Constants.Weight.Kilotonnes;
			AddBill(header, "MB1232112", BillTypeList.Codes.MasterBillOfLading);
			AddBill(header, "XJF69783256", BillTypeList.Codes.USCBPEntryNumber);
			header.SellingParty.E2_AddressOverride = true;
			header.SellingParty.E2_CompanyName = "SELLING COMPANY";
			header.SellingParty.E2_Address1 = "SELLING ADDRESS 1";
			header.SellingParty.E2_Address2 = "SELLING ADDRESS 2";
			header.SellingParty.E2_City = "SYDNEY";
			header.SellingParty.E2_Contact = "BOB THE BUILDER";
			header.SellingParty.E2_Email = "BOB@BUILDER.COM";
			header.SellingParty.E2_Fax = "+61 (2) 8456 6846";
			header.SellingParty.E2_Phone = "+61 (2) 8456 6855";
			header.SellingParty.E2_Postcode = "2214";
			header.SellingParty.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			header.SellingParty.E2_State = "NSW";
			header.SellingParty.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			header.SellingParty.E2_GovRegNum = "324325684";
			header.SellingParty.E2_Mobile = "+61 403 112 456";
			ISFDocAddress sellingParty2 = header.DocAddresses.AddNew(DocAddressType.SellingParty);
			sellingParty2.OrganisationPK = importer.PK;
			sellingParty2.E2_OA_Address = importer.MainAddress.PK;
			ISFDocAddress buyingParty = header.BuyingParty;
			buyingParty.E2_AddressOverride = true;
			buyingParty.E2_CompanyName = "BUYING COMPANY";
			buyingParty.E2_Address1 = "BUYING ADDRESS 1";
			buyingParty.E2_Address2 = "BUYING ADDRESS 2";
			buyingParty.E2_City = "SYDNEY";
			buyingParty.E2_Contact = "BUYER THE BUILDER";
			buyingParty.E2_Email = "BUYER@BUILDER.COM";
			buyingParty.E2_Fax = "+61 (2) 6953 6846";
			buyingParty.E2_Phone = "+61 (2) 6953 6855";
			buyingParty.E2_Postcode = "2200";
			buyingParty.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			buyingParty.E2_State = "NSW";
			buyingParty.E2_GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			buyingParty.E2_GovRegNum = "912-21-4568";
			buyingParty.E2_Mobile = "+61 403 864 456";
			ISFDocAddress stuffingLocation = header.StuffingLocation;
			stuffingLocation.E2_AddressOverride = true;
			stuffingLocation.E2_CompanyName = "STUFFING LOCATION COMPANY";
			stuffingLocation.E2_Address1 = "STUFFING LOCATION ADDRESS 1";
			stuffingLocation.E2_Address2 = "STUFFING LOCATION ADDRESS 2";
			stuffingLocation.E2_City = "CHICAGO";
			stuffingLocation.E2_Contact = "STUFFER THE BUILDER";
			stuffingLocation.E2_Email = "STUFFER@BUILDER.COM";
			stuffingLocation.E2_Postcode = "61022";
			stuffingLocation.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			ISFDocAddress consolidatorDocAddress = header.Consolidator;
			consolidatorDocAddress.E2_OA_Address = consolidator.MainAddress.PK;
			ISFDocAddress bookingPartyDocAddress = header.BookingParty;
			bookingPartyDocAddress.E2_AddressOverride = true;
			bookingPartyDocAddress.E2_CompanyName = "BOOKING PARTY COMPANY";
			bookingPartyDocAddress.E2_Address1 = "BOOKING PARTY ADDRESS 1";
			bookingPartyDocAddress.E2_Address2 = "BOOKING PARTY ADDRESS 2";
			bookingPartyDocAddress.E2_City = "MELBOURNE";
			bookingPartyDocAddress.E2_Contact = "WENDY THE BUILDER";
			bookingPartyDocAddress.E2_Email = "WENDY@BUILDER.COM";
			bookingPartyDocAddress.E2_Fax = "+61 (3) 8456 6846";
			bookingPartyDocAddress.E2_Phone = "+61 (3) 8456 6855";
			bookingPartyDocAddress.E2_Postcode = "3014";
			bookingPartyDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			bookingPartyDocAddress.E2_State = "VIC";
			bookingPartyDocAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			bookingPartyDocAddress.E2_GovRegNum = "326465678";
			bookingPartyDocAddress.E2_Mobile = "+61 403 112 695";
			ISFDocAddress shipToParty = header.MainShipToParty;
			shipToParty.E2_AddressOverride = true;
			shipToParty.E2_CompanyName = "SHIP TO PARTY COMPANY";
			shipToParty.E2_Address1 = "SHIP TO PARTY ADDRESS 1";
			shipToParty.E2_Address2 = "SHIP TO PARTY ADDRESS 2";
			shipToParty.E2_City = "MELBOURNE";
			shipToParty.E2_Contact = "SHIP TO THE BUILDER";
			shipToParty.E2_Email = "SHIPTO@BUILDER.COM";
			shipToParty.E2_Fax = "+61 (3) 8456 6846";
			shipToParty.E2_Phone = "+61 (3) 8456 6855";
			shipToParty.E2_Postcode = "3014";
			shipToParty.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			shipToParty.E2_State = "VIC";
			shipToParty.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			shipToParty.E2_GovRegNum = "869345684";
			shipToParty.E2_Mobile = "+61 403 112 695";
			CusISFEquip container1 = header.Equipments.AddNew();
			container1.BE_ContainerNum = "TURE2323333";
			container1.BE_EquipCode = "2B";
			container1.BE_ContainerISO = "20FR";
			CusISFEquip container2 = header.Equipments.AddNew();
			container2.BE_ContainerNum = "TURE1110111";
			container2.BE_EquipCode = "4D";
			container2.BE_ContainerISO = "40FR";
			ISFDocAddress manufacturer1DocAddress = header.ManufacturerAddresses.AddNew();
			manufacturer1DocAddress.E2_AddressType = DocAddressTypes.Codes.Manufacturer;
			manufacturer1DocAddress.E2_OA_Address = manufacturer1.MainAddress.PK;
			ISFDocAddress manufacturer2DocAddress = header.ManufacturerAddresses.AddNew();
			manufacturer2DocAddress.E2_AddressType = DocAddressTypes.Codes.Manufacturer;
			manufacturer2DocAddress.E2_AddressOverride = true;
			manufacturer2DocAddress.E2_CompanyName = "MANUFACTURER2 COMPANY";
			manufacturer2DocAddress.E2_Address1 = "MANUFACTURER2 ADDRESS 1";
			manufacturer2DocAddress.E2_Address2 = "MANUFACTURER2 ADDRESS 2";
			manufacturer2DocAddress.E2_City = "MELBOURNE";
			manufacturer2DocAddress.E2_Contact = "BOB THE DESTROYER";
			manufacturer2DocAddress.E2_Email = "BOB@DOOM.COM";
			manufacturer2DocAddress.E2_Fax = "+61 (2) 6666 6666";
			manufacturer2DocAddress.E2_Phone = "+61 (2) 9999 9999";
			manufacturer2DocAddress.E2_Postcode = "3666";
			manufacturer2DocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			manufacturer2DocAddress.E2_State = "VIR";
			manufacturer2DocAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			manufacturer2DocAddress.E2_GovRegNum = "324328694";
			manufacturer2DocAddress.E2_Mobile = "+61 433 666 666";
			CusISFLine line1 = header.Lines.AddNew();
			line1.BL_HarmonisedNum = "10.10.8120";
			line1.BL_ManufacturerDocAddressPK = manufacturer1DocAddress.PK;
			line1.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			line1.CustomAttribute1 = "LN1ATTRIB1";
			CusISFLine line2 = header.Lines.AddNew();
			line2.BL_HarmonisedNum = "20.20.8220";
			line2.BL_ManufacturerDocAddressPK = manufacturer2DocAddress.PK;
			line2.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			line2.CustomAttribute1 = "LN2ATTRIB1";
			line2.CustomAttribute2 = "LN2ATTRIB2";
			CusISFLine line3 = header.Lines.AddNew();
			line3.BL_HarmonisedNum = "30.30.8320";
			line3.BL_ManufacturerDocAddressPK = manufacturer1DocAddress.PK;
			line3.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			line3.BL_TextProductCode = part.OP_PartNum;
			line3.CustomAttribute2 = "LN3ATTRIB2";
			CusISFLine line4 = header.Lines.AddNew();
			line4.BL_HarmonisedNum = "40.40.8420";
			line4.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.NewZealand;
			header.Notes.AddNew(true, "TestISFClone", "note text");
			return header;
		}

		void AddBill(CusISFHeader header, ZString billNum, ZString billType)
		{
			CusISFBill bill = header.ReferenceDatas.AddNew();
			bill.BB_BillNum = billNum;
			bill.BB_BillType = billType;
		}
	}
}
