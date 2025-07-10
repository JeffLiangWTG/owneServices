using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class InBondWPActionCodeListTest : TestCase
	{
		public void TestList()
		{
			Assert(InBondWPActionCodeList.IsDiversionRequest(InBondWPActionCodeList.Codes.DiversionRequest));
			Assert(!InBondWPActionCodeList.IsDiversionRequest(InBondWPActionCodeList.Codes.ArriveContainerAtDestination));

			Assert("ArriveBillOfLadingAtDestination", InBondWPActionCodeList.IsArrivalAction(InBondWPActionCodeList.Codes.ArriveBillOfLadingAtDestination));
			Assert("ArriveContainerAtDestination", InBondWPActionCodeList.IsArrivalAction(InBondWPActionCodeList.Codes.ArriveContainerAtDestination));
			Assert("ArriveEntireInBondAtDestination", InBondWPActionCodeList.IsArrivalAction(InBondWPActionCodeList.Codes.ArriveEntireInBondAtDestination));
			Assert("IsArrivalAction", !InBondWPActionCodeList.IsArrivalAction(InBondWPActionCodeList.Codes.ExportBillOfLadingFromDestinationPort));

			Assert("ExportBillOfLadingFromDestinationPort", InBondWPActionCodeList.IsExportationAction(InBondWPActionCodeList.Codes.ExportBillOfLadingFromDestinationPort));
			Assert("ExportBillOfLadingFromDestinationPort", InBondWPActionCodeList.IsExportationAction(InBondWPActionCodeList.Codes.ExportBillOfLadingFromDestinationPort));
			Assert("ExportBillOfLadingFromDestinationPort", InBondWPActionCodeList.IsExportationAction(InBondWPActionCodeList.Codes.ExportBillOfLadingFromDestinationPort));
			Assert("IsExportationAction", !InBondWPActionCodeList.IsExportationAction(InBondWPActionCodeList.Codes.TransferOfInBondLiabilityForBillOfLading));

			Assert("TransferOfInBondLiabilityForBillOfLading", InBondWPActionCodeList.IsTOLAction(InBondWPActionCodeList.Codes.TransferOfInBondLiabilityForBillOfLading));
			Assert("TransferOfInBondLiabilityForContainer", InBondWPActionCodeList.IsTOLAction(InBondWPActionCodeList.Codes.TransferOfInBondLiabilityForContainer));
			Assert("TransferOfInBondLiabilityForEntireInBond", InBondWPActionCodeList.IsTOLAction(InBondWPActionCodeList.Codes.TransferOfInBondLiabilityForEntireInBond));

			Assert("FDADataSubmissionForInBond", InBondWPActionCodeList.IsFDATransmissionAction(InBondWPActionCodeList.Codes.FDADataSubmissionForInBond));
			Assert("FDADataSubmissionForOtherThanInBond", InBondWPActionCodeList.IsFDATransmissionAction(InBondWPActionCodeList.Codes.FDADataSubmissionForOtherThanInBond));
			Assert("IsFDATransmissionAction", !InBondWPActionCodeList.IsFDATransmissionAction(InBondWPActionCodeList.Codes.ArriveBillOfLadingAtDestination));

			Assert(InBondWPActionCodeList.IsBillLevel(InBondWPActionCodeList.Codes.ArriveBillOfLadingAtDestination));
			Assert(InBondWPActionCodeList.IsBillLevel(InBondWPActionCodeList.Codes.ArriveContainerAtDestination));
			Assert(InBondWPActionCodeList.IsBillLevel(InBondWPActionCodeList.Codes.ExportBillOfLadingFromDestinationPort));
			Assert(InBondWPActionCodeList.IsBillLevel(InBondWPActionCodeList.Codes.ExportContainerFromDestinationPort));

			Assert(InBondWPActionCodeList.IsContainerLevel(InBondWPActionCodeList.Codes.ArriveContainerAtDestination));
			Assert(InBondWPActionCodeList.IsContainerLevel(InBondWPActionCodeList.Codes.ExportContainerFromDestinationPort));
		}
	}
}
