using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	class InBondAndVesselEventMessageCodeListTest : TestCaseWithFactory
	{
		public void TestGetBillActionCodeList()
		{
			var list = InBondAndVesselEventMessageCodeList.GetBillActionCodeList(Factory, ActionCode.InBondArrival);
			AssertEquals("Should Be Cached", InBondAndVesselEventMessageCodeList.GetBillActionCodeList(Factory, ActionCode.InBondArrival), list);
			AssertEquals(6, list.Count);
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.ArriveInBond, InBondAndVesselEventMessageCodeList.Descriptions.ArriveInBond, list.GetDescriptionFromCode(InBondAndVesselEventMessageCodeList.Codes.ArriveInBond));
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.ArriveInBondByBillOfLading, InBondAndVesselEventMessageCodeList.Descriptions.ArriveInBondByBillOfLading, list.GetDescriptionFromCode(InBondAndVesselEventMessageCodeList.Codes.ArriveInBondByBillOfLading));
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.ArriveInBondByContainer, InBondAndVesselEventMessageCodeList.Descriptions.ArriveInBondByContainer, list.GetDescriptionFromCode(InBondAndVesselEventMessageCodeList.Codes.ArriveInBondByContainer));
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.CancelInBondArrival, InBondAndVesselEventMessageCodeList.Descriptions.CancelInBondArrival, list.GetDescriptionFromCode(InBondAndVesselEventMessageCodeList.Codes.CancelInBondArrival));
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.CancelInBondArrivalByBillOfLading, InBondAndVesselEventMessageCodeList.Descriptions.CancelInBondArrivalByBillOfLading, list.GetDescriptionFromCode(InBondAndVesselEventMessageCodeList.Codes.CancelInBondArrivalByBillOfLading));
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.CancelInBondArrivalByContainer, InBondAndVesselEventMessageCodeList.Descriptions.CancelInBondArrivalByContainer, list.GetDescriptionFromCode(InBondAndVesselEventMessageCodeList.Codes.CancelInBondArrivalByContainer));
			list = InBondAndVesselEventMessageCodeList.GetBillActionCodeList(Factory, ActionCode.InBondExportation);
			AssertEquals("Should Be Cached", InBondAndVesselEventMessageCodeList.GetBillActionCodeList(Factory, ActionCode.InBondExportation), list);
			AssertEquals(6, list.Count);
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.ExportInBond, InBondAndVesselEventMessageCodeList.Descriptions.ExportInBond, list.GetDescriptionFromCode(InBondAndVesselEventMessageCodeList.Codes.ExportInBond));
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.ExportInBondByBillOfLading, InBondAndVesselEventMessageCodeList.Descriptions.ExportInBondByBillOfLading, list.GetDescriptionFromCode(InBondAndVesselEventMessageCodeList.Codes.ExportInBondByBillOfLading));
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.ExportInBondByContainer, InBondAndVesselEventMessageCodeList.Descriptions.ExportInBondByContainer, list.GetDescriptionFromCode(InBondAndVesselEventMessageCodeList.Codes.ExportInBondByContainer));
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.CancelInBondExport, InBondAndVesselEventMessageCodeList.Descriptions.CancelInBondExport, list.GetDescriptionFromCode(InBondAndVesselEventMessageCodeList.Codes.CancelInBondExport));
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.CancelInBondExportByBillOfLading, InBondAndVesselEventMessageCodeList.Descriptions.CancelInBondExportByBillOfLading, list.GetDescriptionFromCode(InBondAndVesselEventMessageCodeList.Codes.CancelInBondExportByBillOfLading));
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.CancelInBondExportByContainer, InBondAndVesselEventMessageCodeList.Descriptions.CancelInBondExportByContainer, list.GetDescriptionFromCode(InBondAndVesselEventMessageCodeList.Codes.CancelInBondExportByContainer));
			list = InBondAndVesselEventMessageCodeList.GetBillActionCodeList(Factory, ActionCode.InBondTransferOfLiability);
			AssertEquals("Should Be Cached", InBondAndVesselEventMessageCodeList.GetBillActionCodeList(Factory, ActionCode.InBondTransferOfLiability), list);
			AssertEquals(2, list.Count);
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.TransferOfInBondLiability, InBondAndVesselEventMessageCodeList.Descriptions.TransferOfInBondLiability, list.GetDescriptionFromCode(InBondAndVesselEventMessageCodeList.Codes.TransferOfInBondLiability));
			AssertEquals(InBondAndVesselEventMessageCodeList.Codes.CancelTransferOfLiability, InBondAndVesselEventMessageCodeList.Descriptions.CancelTransferOfLiability, list.GetDescriptionFromCode(InBondAndVesselEventMessageCodeList.Codes.CancelTransferOfLiability));
			list = InBondAndVesselEventMessageCodeList.GetBillActionCodeList(Factory, ActionCode.GeneralOrderStatus);
			AssertEquals("Should Be Cached", InBondAndVesselEventMessageCodeList.GetBillActionCodeList(Factory, ActionCode.GeneralOrderStatus), list);
			AssertEquals(0, list.Count);
		}

		public void TestIsCancelInBondArrival()
		{
			AssertEquals(false, InBondAndVesselEventMessageCodeList.IsCancelInBondArrival("!@"));
			AssertEquals(true, InBondAndVesselEventMessageCodeList.IsCancelInBondArrival(InBondAndVesselEventMessageCodeList.Codes.CancelInBondArrival));
			AssertEquals(true, InBondAndVesselEventMessageCodeList.IsCancelInBondArrival(InBondAndVesselEventMessageCodeList.Codes.CancelInBondArrivalByBillOfLading));
			AssertEquals(true, InBondAndVesselEventMessageCodeList.IsCancelInBondArrival(InBondAndVesselEventMessageCodeList.Codes.CancelInBondArrivalByContainer));
		}

		public void TestIsCancelInBondExportation()
		{
			AssertEquals(false, InBondAndVesselEventMessageCodeList.IsCancelInBondExportation("!@"));
			AssertEquals(true, InBondAndVesselEventMessageCodeList.IsCancelInBondExportation(InBondAndVesselEventMessageCodeList.Codes.CancelInBondExport));
			AssertEquals(true, InBondAndVesselEventMessageCodeList.IsCancelInBondExportation(InBondAndVesselEventMessageCodeList.Codes.CancelInBondExportByBillOfLading));
			AssertEquals(true, InBondAndVesselEventMessageCodeList.IsCancelInBondExportation(InBondAndVesselEventMessageCodeList.Codes.CancelInBondExportByContainer));
		}

		public void TestIsCancelInBondTransferOfLiability()
		{
			AssertEquals(false, InBondAndVesselEventMessageCodeList.IsCancelInBondTransferOfLiability("!@"));
			AssertEquals(true, InBondAndVesselEventMessageCodeList.IsCancelInBondTransferOfLiability(InBondAndVesselEventMessageCodeList.Codes.CancelTransferOfLiability));
		}
	}
}
