using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.US.InBond.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using CodeDescriptionPairForTesting = Enterprise.Customs.DataTransfer.Universal.Testing.CodeDescriptionPairForTesting;

namespace Enterprise.Customs.US.InBond.Business.Universal.Testing
{
	partial class InBondDataObjectWriterTest
	{
		public void TestExportAirJob()
		{
			var foreignShipper = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var header = SetupCusInBondHeader(Factory.New<CusInBondHeader>());
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			var bill = SetupCusInBondBill(header.Bills.AddNew(), "OB3234");
			bill.ForeignShipper.E2_OA_Address = foreignShipper.MainAddress.PK;
			bill.Consignee.E2_OA_Address = consignee.MainAddress.PK;
			bill.NotifyParty.E2_AddressOverride = ZBool.True;
			bill.NotifyParty.E2_CompanyName = "BOB THE BUILDER";
			bill.NotifyParty.E2_Address1 = "ADDRESS 1";
			bill.NotifyParty.E2_Address2 = "ADDRESS 2";
			bill.NotifyParty.E2_City = "CITY BOB";
			bill.NotifyParty.E2_State = "STATE BOB";
			bill.NotifyParty.E2_Postcode = "39234";
			bill.NotifyParty.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Bahamas;
			var ref1 = bill.AdditionalReferences.AddNew();
			ref1.BR_Qualifier = ReferenceQualifierList.Codes.IN;
			ref1.BR_ReferenceNum = "I986";
			var moveHeader = SetupCusInBondMoveHeader(header.MovementHeaders.AddNew(), "INB3242");
			var moveDetail = SetupCusInBondMoveDetail(moveHeader.MovementDetails.AddNew(bill.PK), "IT32423");
			((IInBondQXHeader)moveHeader).Bills.ToArray();
			Factory.SaveForTesting();
			var writer = new CusInBondHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));
			var headerData = writer.GetDataObject(header);
			AssertInBondHeaderContents(headerData, CodeDescriptionPairForTesting.New(InBondBranch.GB_Code, InBondBranch.GB_BranchName),
				CodeDescriptionPairForTesting.New(US.Business.TransportTypeList.Codes.Air, US.Business.TransportTypeList.Descriptions.Air),
				CodeDescriptionPairForTesting.New("", null), APLVessel.RV_Code, CodeDescriptionPairForTesting.New(Core.Constants.CountryCodes.Jamaica, "Jamaica"),
				APLVessel.RV_LloydsNumber, "V324", CodeDescriptionPairForTesting.New(SeaForeignPort1.RL_Code, SeaForeignPort1.RL_PortName),
				CodeDescriptionPairForTesting.New(SeaLocalPort1.RL_Code, SeaLocalPort1.RL_PortName), new ZDateTime(2014, 2, 10), new ZDateTime(2014, 1, 19), ZBool.True, CarrierCode1,
				SeaLocalPort1ScheduleD.ZZD_Code, "F320", InBondHeaderTypeList.Codes.FullData, SeaForeignPort1ScheduleK.ZZD_Code);
			AssertEquals("headerData.TransportLegCollection.Count", 2, headerData.TransportLegCollection.Count);
			var preCarriageLeg = headerData.TransportLegCollection[0];
			AssertEquals("preCarriageLeg.LegOrder", (ZByte)1, preCarriageLeg.LegOrder);
			AssertNotNull("preCarriageLeg.PortOfLoading", preCarriageLeg.PortOfLoading);
			AssertEquals("preCarriageLeg.PortOfLoading.Code", Core.Constants.CountryCodes.Australia, preCarriageLeg.PortOfLoading.Code);
			AssertEquals("preCarriageLeg.PortOfLoading.Name", "Australia", preCarriageLeg.PortOfLoading.Name);
			AssertEquals("preCarriageLeg.ActualDeparture", new ZDateTime(2014, 1, 20), preCarriageLeg.ActualDeparture);
			AssertTransportLeg(headerData.TransportLegCollection[1], 2, APLVessel.RV_Code, APLVessel.RV_LloydsNumber, "V324", TransportMode.Air,
				CodeDescriptionPairForTesting.New(SeaForeignPort1.RL_Code, SeaForeignPort1.RL_PortName), CodeDescriptionPairForTesting.New(SeaLocalPort1.RL_Code, SeaLocalPort1.RL_PortName),
				new ZDateTime(2014, 1, 19), new ZDateTime(2014, 2, 10));
			AssertEquals("headerData.OrganizationAddressCollection.Count", 1, headerData.OrganizationAddressCollection.Count);
			AssertOrganizationBO_WUFSHIJNB("Importer", headerData.OrganizationAddressCollection[0], nameof(DocAddressType.ImporterDocumentaryAddress));
			AssertNotNull("headerData.NoteCollection", headerData.NoteCollection);
			AssertContainNote(headerData.NoteCollection, ZBool.True, "DUMMY NOTE", "HELLO WORLD");
			AssertContainNote(headerData.NoteCollection, ZBool.False, "DUMMY NOTE 2", "GOODBYE WORLD");
			AssertEquals("headerData.AdditionalBillCollection.Count", 1, headerData.AdditionalBillCollection.Count);
			var billData = headerData.AdditionalBillCollection[0];
			AssertInBondBillContents(billData, "HOB3234", CodeDescriptionPairForTesting.New(WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House), 110m, null, null, null, null, null, null, null, "OB3234", null);
			var organizationAddressCollection = billData.OrganizationAddressCollection;
			AssertEquals("billData.OrganizationAddressCollection.Count", 3, organizationAddressCollection.Count);
			AssertOrganizationBO_WUFSHIJNB("ForeignShipper", organizationAddressCollection.FirstOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.ForeignShipperDocumentaryAddress)), nameof(DocAddressType.ForeignShipperDocumentaryAddress));
			AssertOrganizationBO_CRAHOLSYD("Consignee", organizationAddressCollection.FirstOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.ConsigneeAddress)), nameof(DocAddressType.ConsigneeAddress));
			AssertAddress("NotifyParty", organizationAddressCollection.FirstOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.NotifyParty)), nameof(DocAddressType.NotifyParty), null, "BOB THE BUILDER", ZBool.True, "ADDRESS 1", "ADDRESS 2", "CITY BOB", "STATE BOB", "39234", Core.Constants.CountryCodes.Bahamas, "", "", "", "", "");
			AssertNotNull("billData.CustomsReferenceCollection", billData.CustomsReferenceCollection);
			AssertEquals("billData.CustomsReferenceCollection.Count", 1, billData.CustomsReferenceCollection.Count);
			AssertCustomsReferenceContents(billData.CustomsReferenceCollection[0], Constants.AdditionalReference.Type, ReferenceQualifierList.Codes.IN, "I986");
			AssertNotNull("headerData.InBondMoveHeaderCollection", headerData.InBondMoveHeaderCollection);
			AssertEquals("headerData.InBondMoveHeaderCollection.Count", 1, headerData.InBondMoveHeaderCollection.Count);
			var moveHeader1Data = headerData.InBondMoveHeaderCollection[0];
			AssertInBondMoveHeaderContents(moveHeader1Data, "INB3242");
			AssertNotNull("moveHeader1Data.InBondMoveDetailCollection", moveHeader1Data.InBondMoveDetailCollection);
			AssertEquals("moveHeader1Data.InBondMoveDetailCollection.Count", 1, moveHeader1Data.InBondMoveDetailCollection.Count);
			AssertInBondMoveDetailContentsForAir(moveHeader1Data.InBondMoveDetailCollection[0], "0001", 1, "IT32423");
		}

		protected void AssertInBondBillContents(AdditionalBill billData, ZString? billNumber)
		{
			AssertInBondBillContents(billData, billNumber, CodeDescriptionPairForTesting.New(WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master), 110, InBondManifestUQList.Codes.BAG, 1500m, Core.Constants.Weight.Kilograms, 1.5m, Core.Constants.Volume.CubicMetres, SeaForeignPort2ScheduleK.ZZD_Code, SeaLocalPort2ScheduleD.ZZD_Code, null, "INB3");
		}

		protected void AssertInBondBillContents(AdditionalBill billData, ZString? billNumber, ICodeDescription billType, ZDecimal? manifestQty, ZString? manifestUQ, ZDecimal? weight, ZString? weightUQ, ZDecimal? volume, ZString? volumeUQ, ZString? portOfLadingKCode, ZString? placeOfReceipt, ZString? parentBillNumber, ZString? issuerCode)
		{
			AssertNotNull("Precondition: billData", billData);
			CombineAssertions(delegate
			{
				AssertEquals("billData.BillNumber", billNumber, billData.BillNumber);
				AssertNotNull("billData.BillType", billData.BillType);
				AssertEquals("billData.ParentBillNumber", parentBillNumber, billData.ParentBillNumber);
				AssertEquals("billData.BillType.Code", billType.Code, billData.BillType.Code);
				AssertEquals("billData.BillType.Description", billType.Description, billData.BillType.Description);
				AssertEquals("billData.NoOfPacks", manifestQty, billData.NoOfPacks);
				AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.ManifestUnit)", manifestUQ, billData.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.ManifestUnit));
				AssertEquals("billData.AddInfoCollection.GetZDecimalValue(Constants.Bill.Weight)", weight, billData.AddInfoCollection.GetZDecimalValue(Constants.Bill.AddInfo.Weight));
				AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.WeightUnit)", weightUQ, billData.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.WeightUnit));
				AssertEquals("billData.AddInfoCollection.GetZDecimalValue(Constants.Bill.Volume)", volume, billData.AddInfoCollection.GetZDecimalValue(Constants.Bill.AddInfo.Volume));
				AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.VolumeUnit)", volumeUQ, billData.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.VolumeUnit));
				AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.PortOfLadingScheduleK)", portOfLadingKCode, billData.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.PortOfLadingScheduleK));
				AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.PlaceOfReceiptScheduleD)", placeOfReceipt, billData.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.PlaceOfReceiptScheduleD));
				AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.IssuerCode)", issuerCode, billData.AddInfoCollection.GetZStringValue(Constants.Bill.AddInfo.IssuerCode));
			});
		}

		CusInBondBill SetupCusInBondBill(CusInBondBill bill, ZString masterBillNumber)
		{
			return SetupCusInBondBill(bill, masterBillNumber, 110, InBondManifestUQList.Codes.BAG, 1500m, Core.Constants.Weight.Kilograms, 1.5m, Core.Constants.Volume.CubicMetres, SeaForeignPort2ScheduleK.ZZD_Code, SeaLocalPort2ScheduleD.ZZD_Code, "H" + masterBillNumber, "INB3");
		}

		CusInBondBill SetupCusInBondBill(CusInBondBill bill, ZString masterBillNumber, ZInt manifestQty, ZString manifestUQ, ZDecimal weight, ZString weightUQ, ZDecimal volume, ZString volumeUQ, ZString portOfLadingKCode, ZString placeOfReceipt, ZString houseBillNumber, ZString issuerCode)
		{
			bill.B0_MasterBillNumber = masterBillNumber;
			bill.B0_ManifestQty = manifestQty;
			bill.B0_ManifestUQ = manifestUQ;
			bill.B0_Weight = weight;
			bill.B0_WeightUQ = weightUQ;
			bill.B0_Volume = volume;
			bill.B0_VolumeUQ = volumeUQ;
			bill.B0_PortOfLadingKCode = portOfLadingKCode;
			bill.B0_PlaceOfReceipt = placeOfReceipt;
			bill.B0_HouseBillNumber = houseBillNumber;
			bill.B0_IssuerCode = issuerCode;
			return bill;
		}
	}
}
