using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class JobShipmentCreatorTest : TestCaseWithFactory
	{
		public void TestGetShipment()
		{
			#region Setup data
			OrgHeader buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "BUYER COMPANY";
			buyer.OH_RL_NKClosestPort = "USCHI";
			buyer.MainAddress.OA_Address1 = "BUYER ADDRESS 1";
			buyer.MainAddress.OA_Address2 = "BUYER ADDRESS 2";
			buyer.MainAddress.OA_City = "CHICAGO";
			buyer.MainAddress.OA_State = "IL";
			OrgHeader seller = Factory.New<OrgHeader>();
			seller.OH_FullName = "SELLER COMPANY";
			seller.OH_RL_NKClosestPort = "AUSYD";
			seller.MainAddress.OA_Address1 = "SELLER ADDRESS 1";
			seller.MainAddress.OA_Address2 = "SELLER ADDRESS 2";
			seller.MainAddress.OA_City = "SYDNEY";
			seller.MainAddress.OA_State = "NSW";
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "IMPORTER COMPANY";
			importer.OH_RL_NKClosestPort = "USCHI";
			importer.MainAddress.OA_Address1 = "IMPORTER ADDRESS 1";
			importer.MainAddress.OA_Address2 = "IMPORTER ADDRESS 2";
			importer.MainAddress.OA_City = "CHICAGO";
			importer.MainAddress.OA_State = "IL";
			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "PAS1233343");
			var deliverer = Factory.New<OrgHeader>();
			deliverer.OH_FullName = "DELIVERER COMPANY";
			deliverer.OH_RL_NKClosestPort = "USLAX";
			deliverer.MainAddress.OA_Address1 = "DELIVERER ADDRESS 1";
			deliverer.MainAddress.OA_Address2 = "DELIVERER ADDRESS 2";
			deliverer.MainAddress.OA_City = "LAX";
			deliverer.MainAddress.OA_State = "LA";
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_OH_Importer = importer.PK;
			header.BF_MasterBill = "MB1";
			header.BF_RL_NKPlaceOfDelivery = "USNYC";
			header.BF_EstimatedValue = 1530m;
			header.BF_EstimatedQuantity = 235;
			header.BF_EstimatedQuantityUQ = ShippingOrPackingingUnitList.Codes.Carton;
			header.BF_EstimatedWeight = 3;
			header.BF_EstimatedWeightUQ = Core.Constants.Weight.Tonnes;
			CusISFBill masterBill = header.MasterBill;
			CusISFBill houseBill1 = AddBill(header, "HB1", BillTypeList.Codes.HouseBillOfLading);
			CusISFBill houseBill2 = AddBill(header, "HB2", BillTypeList.Codes.HouseBillOfLading);
			var manufacturer = header.DocAddresses.CreateWithAddressType(DocAddressType.Manufacturer);
			manufacturer.E2_CompanyName = "MANUFACTURER COMPANY";
			manufacturer.Address1 = "MANUFACTURER ADDRESS 1";
			manufacturer.Address2 = "MANUFACTURER ADDRESS 2";
			manufacturer.City = "SYDNEY";
			manufacturer.State = "NSW";
			JobDocAddress buyingParty = header.BuyingParty;
			buyingParty.E2_AddressOverride = true;
			buyingParty.E2_CompanyName = "BUYING COMPANY";
			buyingParty.E2_Address1 = "BUYING ADDRESS 1";
			buyingParty.E2_Address2 = "BUYING ADDRESS 2";
			buyingParty.E2_City = "SYDNEY";
			buyingParty.E2_Contact = "BOB THE BUILDER";
			buyingParty.E2_Email = "BOB@BUILDER.COM";
			buyingParty.E2_Fax = "+61 (2) 8456 6846";
			buyingParty.E2_Phone = "+61 (2) 8456 6855";
			buyingParty.E2_Postcode = "2214";
			buyingParty.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			buyingParty.E2_State = "NSW";
			buyingParty.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			buyingParty.E2_GovRegNum = "324325684";
			buyingParty.E2_Mobile = "+61 403 112 456";
			header.SellingParty.E2_OA_Address = seller.MainAddress.PK;
			header.MainShipToParty.E2_OA_Address = deliverer.MainAddress.PK;
			CusISFEquip container1 = header.Equipments.AddNew();
			container1.BE_ContainerNum = "TURE1";
			CusISFEquip container2 = header.Equipments.AddNew();
			container2.BE_ContainerNum = "TURE2";
			CusISFLine line1 = header.Lines.AddNew();
			line1.BL_HarmonisedNum = "1010.81.20";
			line1.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			line1.BL_ManufacturerDocAddressPK = manufacturer.PK;
			CusISFLine line2 = header.Lines.AddNew();
			line2.BL_HarmonisedNum = "2020.82.20";
			line2.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.NewZealand;
			line2.BL_ManufacturerDocAddressPK = manufacturer.PK;
			OrgHeader carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "CARRIER COMPANY";
			carrier.OH_RL_NKClosestPort = "USCHI";
			carrier.MainAddress.OA_Address1 = "CARRIER ADDRESS 1";
			carrier.MainAddress.OA_Address2 = "CARRIER ADDRESS 2";
			carrier.MainAddress.OA_City = "CHICAGO";
			carrier.MainAddress.OA_State = "IL";
			Transport transport = header.Transports.AddNew();
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_Vessel = "APL VESSEL";
			transport.JW_VoyageFlight = "328";
			transport.JW_RL_NKLoadPort = "AUBNE";
			transport.JW_RL_NKDiscPort = "USCHI";
			transport.JW_ETD = new ZDateTime(2009, 3, 1);
			transport.JW_ATD = new ZDateTime(2009, 3, 2);
			transport.JW_ETA = new ZDateTime(2009, 4, 1);
			transport.JW_ATA = new ZDateTime(2009, 4, 2);
			transport.CarrierPK = carrier.PK;
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Other;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKLastForeignPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "USCHI";
			consol.JK_MasterBillNum = "MB3";
			ForwardingContainer consolContainer1 = consol.Containers.AddNew();
			consolContainer1.JC_ContainerNum = "TURE1";
			consolContainer1.JC_RC = ZGuid.Empty;
			Factory.Save();
			#endregion
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			headerRow.MasterBillPK = masterBill.PK;
			AssertEquals(2, headerRow.Bills.Count);
			ISFBillRow billRow1 = headerRow.Bills[0];
			AssertEquals(2, billRow1.Lines.Count);
			billRow1.Lines[0].ContainerPK = ZGuid.Empty;
			billRow1.Lines[1].ContainerPK = headerRow.Containers.Cast<ISFContainerRow>().FirstOrDefault(x => x.Container.PK == container2.PK).PK;
			ISFBillRow billRow2 = headerRow.Bills[1];
			billRow2.SellingPartyPK = buyingParty.PK;
			AssertEquals(2, billRow2.Lines.Count);
			var orderLines = new List<ISFLineRow>(billRow2.Lines.Cast<ISFLineRow>().OrderBy(x => x.HarmonisedNum));
			orderLines[0].ContainerPK = headerRow.Containers.Cast<ISFContainerRow>().FirstOrDefault(x => x.Container.PK == container1.PK).PK;
			orderLines[1].ContainerPK = ZGuid.Empty;
			ForwardingShipment shipment1 = new JobShipmentCreator(billRow1).GetShipment(new BusinessObjectFactory(), ZGuid.Empty);
			AssertNotEquals(header.Factory, shipment1.Factory);
			AssertEquals(0, shipment1.Consols.Count);
			AssertEquals(ZString.Empty, shipment1.JS_UniqueConsignRef);
			AssertEquals(billRow1.HouseBill.BB_BillNum, shipment1.JS_HouseBill);
			AssertEquals(importer.PK, shipment1.ConsigneePK);
			AssertEquals(seller.PK, shipment1.ConsignorPK);
			AssertEquals(deliverer.PK, shipment1.ConsigneeDeliveryAddress.OrganisationPK);
			AssertEquals(manufacturer.Address1, shipment1.ManufacturerDocAddress.Address1);
			AssertEquals(Core.Constants.TransportModes.Sea, shipment1.JS_TransportMode);
			AssertEquals("USNYC", shipment1.JS_RL_NKDestination);
			AssertEquals(1530m, shipment1.JS_GoodsValue);
			AssertEquals(235, shipment1.JS_TotalPackageCount);
			AssertEquals(ShippingOrPackingingUnitList.Codes.Carton, shipment1.JS_F3_NKTotalCountPackType);
			AssertEquals(3m, shipment1.JS_ActualWeight);
			AssertEquals(Core.Constants.Weight.Tonnes, shipment1.JS_UnitOfWeight);
			AssertDocAddress(shipment1.BuyerDocAddress, ZGuid.Empty, "BUYING COMPANY", "BUYING ADDRESS 1", "BUYING ADDRESS 2", "2214", "SYDNEY", "NSW", Core.Constants.CountryCodes.Australia, "BOB THE BUILDER", "+61 (2) 8456 6855", "+61 (2) 8456 6846", "BOB@BUILDER.COM", DocAddressTypes.Codes.BuyerDocumentaryAddress);
			AssertDocAddress(header.SellingParty, shipment1.ConsignorDocumentaryAddress, DocAddressTypes.Codes.ConsignorDocumentaryAddress);
			AssertEquals(2, shipment1.OuterPackLines.Count);
			AssertPackLine(shipment1.OuterPackLines.ToArray<ForwardingPackLine>().First(x => x.JL_HarmonisedCode == "1010.81.20"), "1010.81.20", Core.Constants.CountryCodes.Australia, ZGuid.Empty);
			AssertPackLine(shipment1.OuterPackLines.ToArray<ForwardingPackLine>().First(x => x.JL_HarmonisedCode == "2020.82.20"), "2020.82.20", Core.Constants.CountryCodes.NewZealand, ZGuid.Empty);
			CusISFHeader header1 = shipment1.Factory.Load<CusISFHeader>(header.PK);
			AssertNull(header1.Logs.MostRecentLogByEventTime(Events.Transferred));
			shipment1.JS_UniqueConsignRef = "S1";
			StmALog transferToShipmentEvent1 = header1.Logs.MostRecentLogByEventTime(Events.Transferred);
			AssertEquals(BillTypeList.Codes.HouseBillOfLading + ":" + billRow1.HouseBill.BB_BillNum + " to S1", transferToShipmentEvent1.SL_Reference);
			ForwardingShipment shipment2 = new JobShipmentCreator(billRow2).GetShipment(new BusinessObjectFactory(), consol.PK);
			AssertNotEquals(header.Factory, shipment2.Factory);
			AssertNotEquals(shipment1.Factory, shipment2.Factory);
			AssertEquals(1, shipment2.Consols.Count);
			AssertNotNull(shipment2.Consols.FindByPK(consol.PK));
			AssertEquals(ZString.Empty, shipment2.JS_UniqueConsignRef);
			AssertEquals(billRow2.HouseBill.BB_BillNum, shipment2.JS_HouseBill);
			AssertEquals(importer.PK, shipment2.ConsigneePK);
			AssertEquals(buyingParty.OrganisationPK, shipment2.ConsignorPK);
			AssertEquals(deliverer.PK, shipment1.ConsigneeDeliveryAddress.OrganisationPK);
			AssertEquals(manufacturer.Address1, shipment1.ManufacturerDocAddress.Address1);
			AssertEquals(Core.Constants.TransportModes.Sea, shipment2.JS_TransportMode);
			AssertEquals("USNYC", shipment2.JS_RL_NKDestination);
			AssertEquals(1530m, shipment2.JS_GoodsValue);
			AssertEquals(235, shipment2.JS_TotalPackageCount);
			AssertEquals(ShippingOrPackingingUnitList.Codes.Carton, shipment2.JS_F3_NKTotalCountPackType);
			AssertEquals(3m, shipment2.JS_ActualWeight);
			AssertEquals(Core.Constants.Weight.Tonnes, shipment2.JS_UnitOfWeight);
			AssertDocAddress(shipment2.BuyerDocAddress, ZGuid.Empty, "BUYING COMPANY", "BUYING ADDRESS 1", "BUYING ADDRESS 2", "2214", "SYDNEY", "NSW", Core.Constants.CountryCodes.Australia, "BOB THE BUILDER", "+61 (2) 8456 6855", "+61 (2) 8456 6846", "BOB@BUILDER.COM", DocAddressTypes.Codes.BuyerDocumentaryAddress);
			AssertEquals(2, shipment2.OuterPackLines.Count);
			AssertPackLine(shipment2.OuterPackLines.ToArray<ForwardingPackLine>().First(x => x.JL_HarmonisedCode == "1010.81.20"), "1010.81.20", Core.Constants.CountryCodes.Australia, consolContainer1.PK);
			AssertPackLine(shipment2.OuterPackLines.ToArray<ForwardingPackLine>().First(x => x.JL_HarmonisedCode == "2020.82.20"), "2020.82.20", Core.Constants.CountryCodes.NewZealand, ZGuid.Empty);
			orderLines[0].ShouldCopy = false;
			ForwardingShipment shipment3 = new JobShipmentCreator(billRow2).GetShipment(new BusinessObjectFactory(), consol.PK);
			AssertNotEquals(header.Factory, shipment3.Factory);
			AssertNotEquals(shipment1.Factory, shipment3.Factory);
			AssertNotEquals(shipment2.Factory, shipment3.Factory);
			AssertEquals(1, shipment3.Consols.Count);
			AssertNotNull(shipment3.Consols.FindByPK(consol.PK));
			AssertEquals(ZString.Empty, shipment3.JS_UniqueConsignRef);
			AssertEquals(billRow2.HouseBill.BB_BillNum, shipment3.JS_HouseBill);
			AssertEquals(importer.PK, shipment3.ConsigneePK);
			AssertEquals(buyingParty.OrganisationPK, shipment3.ConsignorPK);
			AssertEquals(deliverer.PK, shipment1.ConsigneeDeliveryAddress.OrganisationPK);
			AssertEquals(manufacturer.Address1, shipment1.ManufacturerDocAddress.Address1);
			AssertEquals(Core.Constants.TransportModes.Sea, shipment3.JS_TransportMode);
			AssertEquals("USNYC", shipment3.JS_RL_NKDestination);
			AssertEquals(1530m, shipment3.JS_GoodsValue);
			AssertEquals(235, shipment3.JS_TotalPackageCount);
			AssertEquals(ShippingOrPackingingUnitList.Codes.Carton, shipment3.JS_F3_NKTotalCountPackType);
			AssertEquals(3m, shipment3.JS_ActualWeight);
			AssertEquals(Core.Constants.Weight.Tonnes, shipment3.JS_UnitOfWeight);
			AssertDocAddress(shipment3.BuyerDocAddress, ZGuid.Empty, "BUYING COMPANY", "BUYING ADDRESS 1", "BUYING ADDRESS 2", "2214", "SYDNEY", "NSW", Core.Constants.CountryCodes.Australia, "BOB THE BUILDER", "+61 (2) 8456 6855", "+61 (2) 8456 6846", "BOB@BUILDER.COM", DocAddressTypes.Codes.BuyerDocumentaryAddress);
			AssertEquals(1, shipment3.OuterPackLines.Count);
			AssertPackLine(shipment3.OuterPackLines[0], "2020.82.20", Core.Constants.CountryCodes.NewZealand, ZGuid.Empty);
		}

		public void TestCopyISFeDocsToShipment()
		{
			#region Setup Data
			var header = Factory.NewWithValidTestData<CusISFHeader>();
			AddBill(header, "HB1", BillTypeList.Codes.HouseBillOfLading);
			var headerRow = new ISFHeaderRow(header);
			AssertEquals(1, headerRow.Bills.Count);
			var billRow1 = headerRow.Bills[0];
			var header2 = Factory.NewWithValidTestData<CusISFHeader>();
			AddBill(header2, "HB1", BillTypeList.Codes.HouseBillOfLading);
			var headerRow2 = new ISFHeaderRow(header2);
			AssertEquals(1, headerRow2.Bills.Count);
			var billRow2 = headerRow2.Bills[0];
			CreateEDocsRows(header.PK.ToGuid());
			AssertEquals("one eDocs row is created", 1, ((IDocManagerSupport)header).DocManagerInfo.AllEDocs.Count);
			#endregion
			var shipment = new JobShipmentCreator(billRow1).GetShipment(Factory, ZGuid.Empty);
			AssertEquals(1, shipment.DocManagerInfo.AllEDocs.Count);
			AssertNotNull(shipment.DocManagerInfo.AllEDocs[0]);
			var edocs = shipment.DocManagerInfo.AllEDocs[0];
			AssertEquals("FileName", "FileName.xml", edocs.FileName);
			AssertEquals("DocType", "ACV", edocs.DocType);
			AssertEquals("DataType", "XML", edocs.DataType);
			var shipment2 = new JobShipmentCreator(billRow2).GetShipment(Factory, ZGuid.Empty);
			AssertEquals(0, shipment2.DocManagerInfo.AllEDocs.Count);
			AssertNull(shipment2.DocManagerInfo.AllEDocs[0]);
		}

		void AssertDocAddress(JobDocAddress docAddress, ZGuid addressPK, ZString companyName, ZString address1, ZString address2, ZString postCode, ZString city, ZString state, ZString countryCode, ZString contactName, ZString phone, ZString fax, ZString email, ZString addressType)
		{
			if (addressPK.IsEmpty)
			{
				AssertEquals(true, docAddress.E2_AddressOverride);
				AssertEquals(companyName, docAddress.E2_CompanyName);
				AssertEquals(address1, docAddress.E2_Address1);
				AssertEquals(address2, docAddress.E2_Address2);
				AssertEquals(postCode, docAddress.E2_Postcode);
				AssertEquals(city, docAddress.E2_City);
				AssertEquals(state, docAddress.E2_State);
				AssertEquals(countryCode, docAddress.E2_RN_NKCountryCode);
				AssertEquals(contactName, docAddress.E2_Contact);
				AssertEquals(phone, docAddress.E2_Phone);
				AssertEquals(fax, docAddress.E2_Fax);
				AssertEquals(email, docAddress.E2_Email);
			}
			else
			{
				AssertEquals(addressPK, docAddress.E2_OA_Address);
				AssertEquals(false, docAddress.E2_AddressOverride);
			}

			AssertEquals(addressType, docAddress.E2_AddressType);
		}

		void AssertPackLine(ForwardingPackLine packLine, ZString harmonisedCode, ZString origin, ZGuid containerPK)
		{
			AssertEquals(harmonisedCode, packLine.JL_HarmonisedCode);
			AssertEquals(origin, packLine.JL_RN_NKOrigin);
			AssertEquals(containerPK, packLine.JL_JC);
		}

		void AssertDocAddress(JobDocAddress sourceDocAddress, JobDocAddress docAddress, ZString addressType)
		{
			AssertNotEquals(sourceDocAddress.E2_ParentID, docAddress.E2_ParentID);
			AssertDocAddress(docAddress, sourceDocAddress.E2_AddressOverride ? ZGuid.Empty : sourceDocAddress.E2_OA_Address, sourceDocAddress.E2_CompanyName, sourceDocAddress.E2_Address1, sourceDocAddress.E2_Address2, sourceDocAddress.E2_Postcode, sourceDocAddress.E2_City, sourceDocAddress.E2_State, sourceDocAddress.E2_RN_NKCountryCode, sourceDocAddress.E2_Contact, sourceDocAddress.E2_Phone, sourceDocAddress.E2_Fax, sourceDocAddress.E2_Email, addressType);
		}

		CusISFBill AddBill(CusISFHeader header, ZString billNum, ZString billType)
		{
			CusISFBill bill = header.ReferenceDatas.AddNew();
			bill.BB_BillNum = billNum;
			bill.BB_BillType = billType;
			return bill;
		}

		void CreateEDocsRows(Guid parentFK)
		{
			// don't have access to the bizos here, so use sql to create
			Guid sM_PK = Guid.NewGuid();
			Guid sC_PK = Guid.NewGuid();
			byte[] imageData = new byte[] { 1, 2, 3, 4 }; //System.IO.File.ReadAllBytes(SamplePdfPath);
			string cmdString = "INSERT INTO " + StorageMainSchema.Constants.SqlSchemaName + "." + StorageMainSchema.Constants.TableName + " (" + StorageMainSchema.Constants.PK + ", " + StorageMainSchema.Constants.SM_Type + ", " + StorageMainSchema.Constants.SM_ParentFK + ", " + StorageMainSchema.Constants.SM_DB + " ) " + @" VALUES 
				(@SM_PK, 
				@SM_Type, 
				@SM_ParentFK,
				@SM_DB)";
			var cmd = CargoWise.Data.Db.Connection.Command(cmdString); // don't have access to BizOs here (they're built after us), use SQL to create
			cmd.AddParameterBasedOnDbColumn("@SM_PK", sM_PK, StorageMainSchema.PK);
			cmd.AddParameterBasedOnDbColumn("@SM_Type", "ORG", StorageMainSchema.SM_Type);
			cmd.AddParameterBasedOnDbColumn("@SM_ParentFK", parentFK, StorageMainSchema.SM_ParentFK);
			cmd.AddParameterBasedOnDbColumn("@SM_DB", 1, StorageMainSchema.SM_DB);
			cmd.ExecuteNonQuery();
			cmdString = string.Format("INSERT INTO {0}_SD001..", Db.DatabaseName.Trim()) + StorageDocsSchema.Constants.TableName + " (" + StorageDocsSchema.Constants.PK + ", " + StorageDocsSchema.Constants.SC_Date + ", " + StorageDocsSchema.Constants.SC_SystemCreateTimeUtc + ", " + StorageDocsSchema.Constants.SC_SystemLastEditTimeUtc + ", " + StorageDocsSchema.Constants.SC_DocType + ", " + StorageDocsSchema.Constants.SC_Desc + ", " + StorageDocsSchema.Constants.SC_FileName + ", " + StorageDocsSchema.Constants.SC_SM + ", " + StorageDocsSchema.Constants.SC_ImageData + ", " + StorageDocsSchema.Constants.SC_IsSystemGenerated + ", " + StorageDocsSchema.Constants.SC_DataType + " ) " + @" VALUES 
				(@SC_PK, 
				getdate(), 
				getdate(), 
				getdate(), 
				@SC_DocType,
				@SC_Desc,
				@SC_FileName,
				@SC_SM, 
				@SC_ImageData,
				@SC_IsSystemGenerated,
				@SC_DataType)";
			cmd = CargoWise.Data.Db.Connection.Command(cmdString); // don't have access to BizOs here (they're built after us), use SQL to create
			cmd.AddParameterBasedOnDbColumn("@SC_PK", sC_PK, StorageDocsSchema.PK);
			cmd.AddParameterBasedOnDbColumn("@SC_DocType", "ACV", StorageDocsSchema.SC_DocType);
			cmd.AddParameterBasedOnDbColumn("@SC_Desc", "This is a test", StorageDocsSchema.SC_Desc);
			cmd.AddParameterBasedOnDbColumn("@SC_FileName", "FileName", StorageDocsSchema.SC_FileName);
			cmd.AddParameterBasedOnDbColumn("@SC_SM", sM_PK, StorageDocsSchema.SC_SM);
			cmd.AddParameterBasedOnDbColumn("@SC_ImageData", imageData, StorageDocsSchema.SC_ImageData);
			cmd.AddParameterBasedOnDbColumn("@SC_IsSystemGenerated", "Y", StorageDocsSchema.SC_IsSystemGenerated);
			cmd.AddParameterBasedOnDbColumn("@SC_DataType", "XML", StorageDocsSchema.SC_DataType);
			cmd.ExecuteNonQuery();
		}
	}
}
