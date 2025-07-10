using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ISFHeaderRowValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckMasterBillPK()
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
			ForwardingConsol consol1 = CreateConsol("C1", "MB1");
			ForwardingConsol consol2 = CreateConsol("C2", "MB1");
			ForwardingConsol consol3 = CreateConsol("C3", "HB1");
			ForwardingConsol consol4 = CreateConsol("C4", "HB1");
			ForwardingConsol consol5 = CreateConsol("C5", "OB1");
			ForwardingConsol consol6 = CreateConsol("C6", "OB1");
			ForwardingConsol consol7 = CreateConsol("C7", "MB3");
			ForwardingConsol consol8 = CreateConsol("C8", "HB3");
			ForwardingConsol consol9 = CreateConsol("C9", "OB3");
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			headerRow.MasterBillPK = ZGuid.Empty;
			AssertNoError(headerRow.MasterBillPKInfo, ValidationConstants.ISFHeaderRow.ValidMasterBill);
			headerRow.MasterBillPK = ZGuid.Invalid;
			AssertHasError(headerRow.MasterBillPKInfo, ValidationConstants.ISFHeaderRow.ValidMasterBill);
			foreach (ZGuid masterBillPK in new ZGuid[] { ZGuid.Empty, houseBill1.PK, houseBill2.PK, ZGuid.Invalid })
			{
				headerRow.MasterBillPK = masterBillPK;
				AssertNoWarnings(headerRow.MasterBillPKInfo);
			}

			headerRow.MasterBillPK = masterBill1.PK;
			AssertEquals(ZGuid.Empty, headerRow.ConsolPK);
			string billAlreadyUsedInconsolMessage1 = ValidationConstants.ISFHeaderRow.BillAlreadyUsedInConsol(BillTypeList.Descriptions.MasterBillOfLading + ":MB1", 2, "'C1', 'C2'");
			string billAlreadyUsedInconsolMessage2 = ValidationConstants.ISFHeaderRow.BillAlreadyUsedInConsol(BillTypeList.Descriptions.MasterBillOfLading + ":MB3", 1, "'C7'");
			AssertHasWarning(headerRow.MasterBillPKInfo, billAlreadyUsedInconsolMessage1);
			AssertNoWarning(headerRow.MasterBillPKInfo, billAlreadyUsedInconsolMessage2);
			headerRow.MasterBillPK = masterBill2.PK;
			AssertEquals(ZGuid.Empty, headerRow.ConsolPK);
			AssertNoWarning(headerRow.MasterBillPKInfo, billAlreadyUsedInconsolMessage1);
			AssertNoWarning(headerRow.MasterBillPKInfo, billAlreadyUsedInconsolMessage2);
			headerRow.MasterBillPK = masterBill3.PK;
			AssertEquals(consol7.PK, headerRow.ConsolPK);
			AssertNoWarning(headerRow.MasterBillPKInfo, billAlreadyUsedInconsolMessage1);
			AssertNoWarning(headerRow.MasterBillPKInfo, billAlreadyUsedInconsolMessage2);
			headerRow.ConsolPK = ZGuid.Empty;
			AssertHasWarning(headerRow.MasterBillPKInfo, billAlreadyUsedInconsolMessage2);
			headerRow.MasterBillPK = oceanBill1.PK;
			AssertEquals(ZGuid.Empty, headerRow.ConsolPK);
			billAlreadyUsedInconsolMessage1 = ValidationConstants.ISFHeaderRow.BillAlreadyUsedInConsol(BillTypeList.Descriptions.OceanBillOfLading + ":OB1", 2, "'C5', 'C6'");
			billAlreadyUsedInconsolMessage2 = ValidationConstants.ISFHeaderRow.BillAlreadyUsedInConsol(BillTypeList.Descriptions.OceanBillOfLading + ":OB3", 1, "'C9'");
			AssertHasWarning(headerRow.MasterBillPKInfo, billAlreadyUsedInconsolMessage1);
			AssertNoWarning(headerRow.MasterBillPKInfo, billAlreadyUsedInconsolMessage2);
			headerRow.MasterBillPK = oceanBill2.PK;
			AssertEquals(ZGuid.Empty, headerRow.ConsolPK);
			AssertNoWarning(headerRow.MasterBillPKInfo, billAlreadyUsedInconsolMessage1);
			AssertNoWarning(headerRow.MasterBillPKInfo, billAlreadyUsedInconsolMessage2);
			headerRow.MasterBillPK = oceanBill3.PK;
			AssertEquals(consol9.PK, headerRow.ConsolPK);
			AssertNoWarning(headerRow.MasterBillPKInfo, billAlreadyUsedInconsolMessage1);
			AssertNoWarning(headerRow.MasterBillPKInfo, billAlreadyUsedInconsolMessage2);
			headerRow.ConsolPK = ZGuid.Empty;
			AssertHasWarning(headerRow.MasterBillPKInfo, billAlreadyUsedInconsolMessage2);
		}

		CusISFBill AddBill(CusISFHeader header, ZString billType, ZString billNumber)
		{
			CusISFBill bill = header.ReferenceDatas.AddNew();
			bill.BB_BillType = billType;
			bill.BB_BillNum = billNumber;
			return bill;
		}

		ForwardingConsol CreateConsol(ZString jobReference, ZString masterBill)
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = jobReference;
			consol.JK_MasterBillNum = masterBill;
			return consol;
		}
	}
}
