using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ISFBillRowValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBillPK()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFBill masterBill1 = AddBill(header, BillTypeList.Codes.MasterBillOfLading, "MB1");
			CusISFBill masterBill2 = AddBill(header, BillTypeList.Codes.MasterBillOfLading, "MB2");
			CusISFBill masterBill3 = AddBill(header, BillTypeList.Codes.MasterBillOfLading, "MB3");
			CusISFBill houseBill1 = AddBill(header, BillTypeList.Codes.HouseBillOfLading, "HB1");
			CusISFBill houseBill2 = AddBill(header, BillTypeList.Codes.HouseBillOfLading, "HB2");
			CusISFBill houseBill3 = AddBill(header, BillTypeList.Codes.HouseBillOfLading, "HB3");
			CusISFBill oceanBill1 = AddBill(header, BillTypeList.Codes.OceanBillOfLading, "OB1");
			CusISFBill oceanBill2 = AddBill(header, BillTypeList.Codes.OceanBillOfLading, "OB2");
			CusISFBill oceanBill3 = AddBill(header, BillTypeList.Codes.OceanBillOfLading, "OB3");
			ForwardingShipment shipment1 = CreateShipment("S1", "MB1");
			ForwardingShipment shipment2 = CreateShipment("S2", "MB1");
			ForwardingShipment shipment3 = CreateShipment("S3", "HB1");
			ForwardingShipment shipment4 = CreateShipment("S4", "HB1");
			ForwardingShipment shipment5 = CreateShipment("S5", "OB1");
			ForwardingShipment shipment6 = CreateShipment("S6", "OB1");
			ForwardingShipment shipment7 = CreateShipment("S7", "MB3");
			ForwardingShipment shipment8 = CreateShipment("S8", "HB3");
			ForwardingShipment shipment9 = CreateShipment("S9", "OB3");
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			ISFBillRow billRow1 = headerRow.Bills.AddNew();
			ISFBillRow billRow2 = headerRow.Bills.AddNew();
			billRow1.BillPK = houseBill1.PK;
			billRow2.BillPK = ZGuid.Invalid;
			AssertHasError(billRow2.BillPKInfo, ValidationConstants.ISFBillRow.ValidBill);
			billRow2.BillPK = ZGuid.Empty;
			AssertHasError(billRow2.BillPKInfo, ValidationConstants.ISFBillRow.ValidBill);
			billRow2.BillPK = houseBill1.PK;
			string billAlreadyUsedInExistingRowMessage = ValidationConstants.ISFBillRow.BillAlreadyUsedInExistingRow(BillTypeList.Descriptions.HouseBillOfLading + ":HB1");
			string billAlreadyUsedInShipmentMessage1 = ValidationConstants.ISFBillRow.BillAlreadyUsedInShipment("HB1", 2, "'S3', 'S4'");
			string billAlreadyUsedInShipmentMessage2 = ValidationConstants.ISFBillRow.BillAlreadyUsedInShipment("HB3", 1, "'S8'");
			AssertNoError(billRow2.BillPKInfo, ValidationConstants.ISFBillRow.ValidBill);
			AssertHasError(billRow2.BillPKInfo, billAlreadyUsedInExistingRowMessage);
			AssertHasWarning(billRow2.BillPKInfo, billAlreadyUsedInShipmentMessage1);
			AssertNoWarning(billRow2.BillPKInfo, billAlreadyUsedInShipmentMessage2);
			billRow2.BillPK = houseBill2.PK;
			AssertNoError(billRow2.BillPKInfo, billAlreadyUsedInExistingRowMessage);
			AssertNoWarning(billRow2.BillPKInfo, billAlreadyUsedInShipmentMessage1);
			AssertNoWarning(billRow2.BillPKInfo, billAlreadyUsedInShipmentMessage2);
			billRow2.BillPK = houseBill3.PK;
			AssertNoError(billRow2.BillPKInfo, billAlreadyUsedInExistingRowMessage);
			AssertNoWarning(billRow2.BillPKInfo, billAlreadyUsedInShipmentMessage1);
			AssertHasWarning(billRow2.BillPKInfo, billAlreadyUsedInShipmentMessage2);
			billRow1.BillPK = masterBill1.PK;
			billRow2.BillPK = masterBill1.PK;
			billAlreadyUsedInExistingRowMessage = ValidationConstants.ISFBillRow.BillAlreadyUsedInExistingRow(BillTypeList.Descriptions.MasterBillOfLading + ":MB1");
			AssertHasError(billRow2.BillPKInfo, billAlreadyUsedInExistingRowMessage);
			AssertNoWarnings(billRow2.BillPKInfo);
			billRow2.BillPK = masterBill2.PK;
			AssertNoError(billRow2.BillPKInfo, billAlreadyUsedInExistingRowMessage);
			AssertNoWarnings(billRow2.BillPKInfo);
			billRow2.BillPK = masterBill3.PK;
			AssertNoError(billRow2.BillPKInfo, billAlreadyUsedInExistingRowMessage);
			AssertNoWarnings(billRow2.BillPKInfo);
			billRow1.BillPK = oceanBill1.PK;
			billRow2.BillPK = oceanBill1.PK;
			billAlreadyUsedInExistingRowMessage = ValidationConstants.ISFBillRow.BillAlreadyUsedInExistingRow(BillTypeList.Descriptions.OceanBillOfLading + ":OB1");
			AssertHasError(billRow2.BillPKInfo, billAlreadyUsedInExistingRowMessage);
			AssertNoWarnings(billRow2.BillPKInfo);
			billRow2.BillPK = oceanBill2.PK;
			AssertNoError(billRow2.BillPKInfo, billAlreadyUsedInExistingRowMessage);
			AssertNoWarnings(billRow2.BillPKInfo);
			billRow2.BillPK = oceanBill3.PK;
			AssertNoError(billRow2.BillPKInfo, billAlreadyUsedInExistingRowMessage);
			AssertNoWarnings(billRow2.BillPKInfo);
		}

		CusISFBill AddBill(CusISFHeader header, ZString billType, ZString billNumber)
		{
			CusISFBill bill = header.ReferenceDatas.AddNew();
			bill.BB_BillType = billType;
			bill.BB_BillNum = billNumber;
			return bill;
		}

		ForwardingShipment CreateShipment(ZString jobReference, ZString houseBill)
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = jobReference;
			shipment.JS_HouseBill = houseBill;
			return shipment;
		}
	}
}
