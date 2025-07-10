using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal.Testing
{
	sealed class ISFEventContextReaderTest : TestCaseWithFactory
	{
		public void TestEventContextValues()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_JobReference = "ISF0000002";
			header.BF_CustomsReference = "ABC-12345678901";
			CreateISFBill(header, BillTypeList.Codes.MasterBillOfLading, "MHB12345");
			CreateISFBill(header, BillTypeList.Codes.MasterBillOfLading, "MHB12345");
			CreateISFBill(header, BillTypeList.Codes.OceanBillOfLading, "MWB12345");
			CreateISFBill(header, BillTypeList.Codes.OceanBillOfLading, "MWB23456");
			CreateISFBill(header, BillTypeList.Codes.HouseBillOfLading, "HWB12345");
			CreateISFBill(header, BillTypeList.Codes.HouseBillOfLading, "HWB23456");
			Factory.Save();
			var manager = header.GetUniversalDataContextManager() as IEventDataContextManager;
			var eventContextValues = string.Join(System.Environment.NewLine, manager.EventContextValues.Select(x => x.Key + " - " + x.Value).ToArray());
			AssertMultilineASCIIEquals("manager.EventContextValues", @"
DeclarationReference - ISF0000002
EntryNumber - ABC-12345678901
EntryNumberType - ISF
EntryNumberCountryOfIssue - US
MBOLNumber - MWB12345
MBOLNumber - MWB23456
HBOLNumber - HWB12345
HBOLNumber - HWB23456".Trim(), eventContextValues);
		}

		void CreateISFBill(CusISFHeader header, ZString billType, ZString billNumber)
		{
			var bill = header.ReferenceDatas.AddNew();
			bill.BB_BillType = billType;
			bill.BB_BillNum = billNumber;
		}
	}
}
