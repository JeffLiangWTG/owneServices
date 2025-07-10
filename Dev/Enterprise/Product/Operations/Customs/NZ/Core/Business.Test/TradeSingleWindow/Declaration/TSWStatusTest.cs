using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	using System.Data;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;

	public class TSWStatusTest : TestCaseWithFactory
	{
		public void TestCalculateCombinedMovementStatus()
		{
			WriteOffDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			WriteOffDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			WriteOffDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			WriteOffDeclaration.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.PP;
			WriteOffDeclaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			var entryHeader = WriteOffDeclaration.CusEntryHeader;
			entryHeader.CH_NZCSMovementStatus = "";
			entryHeader.CH_MPIBioMovementStatus = "";
			var statusCalculator = BusinessEntityTestStatus("", "", "", "");
			AssertEquals("99", statusCalculator.CalculateCombinedMovementStatus());
			entryHeader.CH_NZCSMovementStatus = LowValueConsignmentStatusList.Codes.InternationalTranshipmentApproved;
			AssertEquals("29", statusCalculator.CalculateCombinedMovementStatus());
			entryHeader.CH_MPIBioMovementStatus = LowValueConsignmentStatusList.Codes.InternationalTranshipmentApproved;
			AssertEquals("22", statusCalculator.CalculateCombinedMovementStatus());

			var mawb = Factory.New<CusMAWB>();
			var hawb = Factory.New<TestHAWB>();
			hawb.CS_CM = mawb.PK;
			hawb.CS_CustomsMovementStatus = "";
			hawb.CS_BioMovementStatus = "";
			statusCalculator = new TSWStatus(hawb);
			AssertEquals("99", statusCalculator.CalculateCombinedMovementStatus(hawb));
			hawb.CS_CustomsMovementStatus = LowValueConsignmentStatusList.Codes.InternationalTranshipmentApproved;
			AssertEquals("29", statusCalculator.CalculateCombinedMovementStatus(hawb));
			hawb.CS_BioMovementStatus = LowValueConsignmentStatusList.Codes.InternationalTranshipmentApproved;
			AssertEquals("22", statusCalculator.CalculateCombinedMovementStatus(hawb));

			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = Factory.New<TestSCAHouse>();
			houseBill.CA_CB = oceanBill.PK;
			houseBill.CA_CustomsMovementStatus = "";
			houseBill.CA_BioMovementStatus = "";
			statusCalculator = new TSWStatus(houseBill);
			AssertEquals("99", statusCalculator.CalculateCombinedMovementStatus(houseBill));
			houseBill.CA_CustomsMovementStatus = LowValueConsignmentStatusList.Codes.InternationalTranshipmentApproved;
			AssertEquals("29", statusCalculator.CalculateCombinedMovementStatus(houseBill));
			houseBill.CA_BioMovementStatus = LowValueConsignmentStatusList.Codes.InternationalTranshipmentApproved;
			AssertEquals("22", statusCalculator.CalculateCombinedMovementStatus(houseBill));
		}

		public void TestErrorStatus()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			var statusCalculator = TestStatus(StatusList.Codes.EntryRejected, TSWStatus.StatusCodes.Error, ResponsibleGovernmentAgencyList.Codes.NZCS);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("GetCombinedStatus", TSWEntryStatusList.Codes.EPP, JobDeclaration.JE_TSWCombinedStatus);
			AssertEquals("CalculateEntryStatus", FormalEntryStatusList.Codes.EntryInError, statusCalculator.CalculateEntryStatus);
		}

		public void TestClearedStatus()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.PCC;
			var statusCalculator = TestStatus(StatusList.Codes.EntryClearedCashToPayPriorToDelivery, TSWStatus.StatusCodes.Cleared, ResponsibleGovernmentAgencyList.Codes.NZCS);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("GetCombinedStatus", TSWEntryStatusList.Codes.CCC, JobDeclaration.JE_TSWCombinedStatus);
			AssertEquals("CalculateEntryStatus", FormalEntryStatusList.Codes.EntryCleared, statusCalculator.CalculateEntryStatus);
		}

		public void TestClearedWithDOStatus()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.PCC;
			JobDeclaration.CusEntryHeader.CH_NZCSStatus = StatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecified;
			var statusCalculator = TestStatus(StatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecified, TSWStatus.StatusCodes.Cleared, ResponsibleGovernmentAgencyList.Codes.NZCS);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("GetCombinedStatus", TSWEntryStatusList.Codes.CCC, JobDeclaration.JE_TSWCombinedStatus);
			AssertEquals("CalculateEntryStatus - DeliveryOrderReceived", FormalEntryStatusList.Codes.DeliveryOrderReceived, statusCalculator.CalculateEntryStatus);
		}

		public void TestEntryRestoredStatus()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.PCC;
			var entryHeader = JobDeclaration.CusEntryHeader;
			entryHeader.CH_NZCSStatus = StatusList.Codes.EntryRestored;
			var statusCalculator = TestStatus(StatusList.Codes.EntryRestored, TSWStatus.StatusCodes.ResponseReceipt, ResponsibleGovernmentAgencyList.Codes.NZCS);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("GetCombinedStatus", TSWEntryStatusList.Codes.RES, JobDeclaration.JE_TSWCombinedStatus);
			AssertEquals("CalculateEntryStatus", FormalEntryStatusList.Codes.EntryRestored, statusCalculator.CalculateEntryStatus);
		}

		public void TestCancellationInErrorStatus()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			JobDeclaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCP;
			var statusCalculator = TestStatus(StatusList.Codes.EntryRejected, TSWStatus.StatusCodes.Error, ResponsibleGovernmentAgencyList.Codes.NZCS);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("GetCombinedStatus", TSWEntryStatusList.Codes.DCE, JobDeclaration.JE_TSWCombinedStatus);
			AssertEquals("CalculateEntryStatus", FormalEntryStatusList.Codes.EntryInError, statusCalculator.CalculateEntryStatus);
		}

		public void TestCancellationClearedStatus()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			JobDeclaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCP;
			var statusCalculator = TestStatus(StatusList.Codes.EntryCancelled, TSWStatus.StatusCodes.Cleared, ResponsibleGovernmentAgencyList.Codes.NZCS);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("GetCombinedStatus", TSWEntryStatusList.Codes.DCC, JobDeclaration.JE_TSWCombinedStatus);
			JobDeclaration.JE_EntryStatus = statusCalculator.CalculateEntryStatus;
			AssertEquals("CalculateEntryStatus", FormalEntryStatusList.Codes.EntryCancelled, JobDeclaration.JE_EntryStatus);
		}

		public void TestNonCancellationResponseToCancelledSubmission()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			JobDeclaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCI;
			var statusCalculator = TestStatus(StatusList.Codes.EntryRoutedToDocumentVerificationDocumentsRequiredAsSpecified, TSWStatus.StatusCodes.Inspection, ResponsibleGovernmentAgencyList.Codes.NZCS);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("GetCombinedTSWStatus - should not be cancelled - routed for inspection", TSWEntryStatusList.Codes.DCI, JobDeclaration.JE_TSWCombinedStatus);
			JobDeclaration.JE_EntryStatus = statusCalculator.CalculateEntryStatus;
			AssertEquals("Declaration Entry Status - should not be cancelled", FormalEntryStatusList.Codes.InspectionsAuditRequirements, JobDeclaration.JE_EntryStatus);
		}

		public void TestAdjustmentStatus()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.PCC;
			var statusCalculator = TestStatus(StatusList.Codes.AdjustmentAccepted, TSWStatus.StatusCodes.AdjustmentAccepted, ResponsibleGovernmentAgencyList.Codes.NZCS);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("GetCombinedStatus", TSWEntryStatusList.Codes.TCC, JobDeclaration.JE_TSWCombinedStatus);
			AssertEquals("CalculateEntryStatus", FormalEntryStatusList.Codes.AdjustmentAccepted, statusCalculator.CalculateEntryStatus);
		}

		public void TestCreditAdviceStatus()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.PCC;
			var statusCalculator = TestStatus(StatusList.Codes.RefundApprovedAmountAsSpecified, TSWStatus.StatusCodes.CreditAdvice, ResponsibleGovernmentAgencyList.Codes.NZCS);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("GetCombinedStatus", TSWEntryStatusList.Codes.ACC, JobDeclaration.JE_TSWCombinedStatus);
			AssertEquals("CalculateEntryStatus", FormalEntryStatusList.Codes.CreditAdvice, statusCalculator.CalculateEntryStatus);
		}

		public void TestClearedDeliveryOrderStatus()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.PCC;
			var entryHeader = JobDeclaration.CusEntryHeader;
			entryHeader.CH_NZCSStatus = StatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecifiedEntryRoutedToDocumentAudit;
			var statusCalculator = TestStatus(StatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecifiedEntryRoutedToDocumentAudit, TSWStatus.StatusCodes.Cleared, ResponsibleGovernmentAgencyList.Codes.NZCS);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("GetCombinedStatus", TSWEntryStatusList.Codes.CCC, JobDeclaration.JE_TSWCombinedStatus);
			AssertEquals("CalculateEntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, statusCalculator.CalculateEntryStatus);
		}

		public void TestClearedDeliveryOnPaymentStatus()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.PCC;
			var entryHeader = JobDeclaration.CusEntryHeader;
			entryHeader.CH_NZCSStatus = StatusList.Codes.EntryClearedCashToPayPriorToDelivery;
			var statusCalculator = TestStatus(StatusList.Codes.EntryClearedCashToPayPriorToDelivery, TSWStatus.StatusCodes.Cleared, ResponsibleGovernmentAgencyList.Codes.NZCS);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("GetCombinedStatus", TSWEntryStatusList.Codes.CCC, JobDeclaration.JE_TSWCombinedStatus);
			AssertEquals("CalculateEntryStatus", FormalEntryStatusList.Codes.DeliveryOnPayment, statusCalculator.CalculateEntryStatus);
		}

		public void TestResponseReceivedStatus()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.PCC;
			var statusCalculator = TestStatus(StatusList.Codes.Acknowledgement, TSWStatus.StatusCodes.ResponseReceipt, ResponsibleGovernmentAgencyList.Codes.NZCS);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("GetCombinedStatus", TSWEntryStatusList.Codes.RCC, JobDeclaration.JE_TSWCombinedStatus);
			AssertEquals("CalculateEntryStatus", FormalEntryStatusList.Codes.ResponseReceived, statusCalculator.CalculateEntryStatus);
		}

		public void TestExportRejectionStatus()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			var statusCalculator = TestStatus(StatusList.Codes.EntryRejected, TSWStatus.StatusCodes.Error, ResponsibleGovernmentAgencyList.Codes.NZCS);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("GetCombinedStatus", TSWEntryStatusList.Codes.REJ, JobDeclaration.JE_TSWCombinedStatus);
			AssertEquals("CalculateEntryStatus", FormalEntryStatusList.Codes.EntryInError, statusCalculator.CalculateEntryStatus);
		}

		public void TestExportInspectionStatus()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			var statusCalculator = TestStatus(StatusList.Codes.EntryRoutedToInspectionsEvaluationPleaseAwaitRequirements, TSWStatus.StatusCodes.Inspection, ResponsibleGovernmentAgencyList.Codes.NZCS);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("GetCombinedStatus", TSWEntryStatusList.Codes.IAR, JobDeclaration.JE_TSWCombinedStatus);
			AssertEquals("CalculateEntryStatus", FormalEntryStatusList.Codes.InspectionsAuditRequirements, statusCalculator.CalculateEntryStatus);
		}

		public void TestExportCreditAdviceStatus()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			var statusCalculator = TestStatus(StatusList.Codes.RefundApprovedAmountAsSpecified, TSWStatus.StatusCodes.CreditAdvice, ResponsibleGovernmentAgencyList.Codes.NZCS);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("GetCombinedStatus", TSWEntryStatusList.Codes.CRE, JobDeclaration.JE_TSWCombinedStatus);
			AssertEquals("CalculateEntryStatus", FormalEntryStatusList.Codes.CreditAdvice, statusCalculator.CalculateEntryStatus);
		}

		public void TestExportClearedStatus()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			var statusCalculator = TestStatus(StatusList.Codes.CustomsClearanceGiven873, TSWStatus.StatusCodes.Cleared, ResponsibleGovernmentAgencyList.Codes.NZCS);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("GetCombinedStatus", TSWEntryStatusList.Codes.CLR, JobDeclaration.JE_TSWCombinedStatus);
			AssertEquals("CalculateEntryStatus", FormalEntryStatusList.Codes.EntryCleared, statusCalculator.CalculateEntryStatus);
		}

		public void TestMPIFoodClearedIPIEntry()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			var statusCalculator = TestStatus(StatusList.Codes.MPIFoodCleared, TSWStatus.StatusCodes.Cleared, ResponsibleGovernmentAgencyList.Codes.MPIFOOD);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("GetCombinedStatus", TSWEntryStatusList.Codes.NPC, JobDeclaration.JE_TSWCombinedStatus);
			AssertEquals("CalculateEntryStatus", FormalEntryStatusList.Codes.AgencyResponsePending, statusCalculator.CalculateEntryStatus);
		}

		public void TestMPIBioClearedIPIEntry()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			var statusCalculator = TestStatus(StatusList.Codes.MPIBiosecurityCleared, TSWStatus.StatusCodes.Cleared, ResponsibleGovernmentAgencyList.Codes.MPIBIO);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("GetCombinedStatus", TSWEntryStatusList.Codes.NCP, JobDeclaration.JE_TSWCombinedStatus);
			AssertEquals("CalculateEntryStatus", FormalEntryStatusList.Codes.AgencyResponsePending, statusCalculator.CalculateEntryStatus);
		}

		public void TestIPICombinedStatus()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			var statusCalculator = TestStatus(StatusList.Codes.MPIFoodCleared, TSWStatus.StatusCodes.Cleared, ResponsibleGovernmentAgencyList.Codes.MPIFOOD);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("GetCombinedStatus", TSWEntryStatusList.Codes.NPC, JobDeclaration.JE_TSWCombinedStatus);
			JobDeclaration.JE_EntryStatus = statusCalculator.CalculateEntryStatus;
			AssertEquals("CalculateEntryStatus", FormalEntryStatusList.Codes.AgencyResponsePending, JobDeclaration.JE_EntryStatus);

			statusCalculator = TestStatus(StatusList.Codes.MPIBiosecurityCleared, TSWStatus.StatusCodes.Cleared, ResponsibleGovernmentAgencyList.Codes.MPIBIO);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("GetCombinedStatus", TSWEntryStatusList.Codes.NCC, JobDeclaration.JE_TSWCombinedStatus);
			JobDeclaration.JE_EntryStatus = statusCalculator.CalculateEntryStatus;
			AssertEquals("CalculateEntryStatus", FormalEntryStatusList.Codes.EntryCleared, JobDeclaration.JE_EntryStatus);
		}

		public void TestWriteOffEntryStatusIsConsistentWithMultipleAgencyResponses()
		{
			/*
			// JE_EntryStatus is sometimes showing Consignment Written Off/Cleared, when the TSW combined status reflects 1 agency may have a hold on the consignment
			//	e.g. NZCS - Written Off / BIO - Consignment Held	- see B00003591 in NZ UAT
			*/
			WriteOffDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			WriteOffDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			WriteOffDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			WriteOffDeclaration.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.Cleared;
			var entryHeader = WriteOffDeclaration.CusEntryHeader;
			entryHeader.CH_MPIBioStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			entryHeader.CH_NZCSStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;

			var statusCalculator = BusinessEntityTestStatus(StatusList.Codes.CustomsCargoReportNotification, TSWStatus.StatusCodes.Cleared, ResponsibleGovernmentAgencyList.Codes.NZCS, LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff);
			WriteOffDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedWriteOffStatus;
			AssertEquals("CombinedStatus remains unchanged", LowValueConsignmentStatusList.Codes.Cleared, WriteOffDeclaration.JE_TSWCombinedStatus);
			WriteOffDeclaration.JE_EntryStatus = statusCalculator.GetWriteOffEntryStatus;
			AssertEquals("Calculated Entry Status", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, WriteOffDeclaration.JE_EntryStatus);
		}

		public void TestWriteOffEntryStatus()
		{
			WriteOffDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			WriteOffDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			WriteOffDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			WriteOffDeclaration.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.PP;
			WriteOffDeclaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			var entryHeader = WriteOffDeclaration.CusEntryHeader;
			entryHeader.CH_MPIBioStatus = LowValueConsignmentStatusList.Codes.AgencyResponsePending;
			entryHeader.CH_NZCSStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;

			var statusCalculator = BusinessEntityTestStatus(StatusList.Codes.CustomsCargoReportNotification, TSWStatus.StatusCodes.Cleared, ResponsibleGovernmentAgencyList.Codes.NZCS, LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff);
			WriteOffDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedWriteOffStatus;
			AssertEquals("CombinedStatus shows Bio response is pending", LowValueConsignmentStatusList.Codes.CP, WriteOffDeclaration.JE_TSWCombinedStatus);
			WriteOffDeclaration.JE_EntryStatus = statusCalculator.GetWriteOffEntryStatus;
			AssertEquals("Calculated Entry Status", LowValueConsignmentStatusList.Codes.AgencyResponsePending, WriteOffDeclaration.JE_EntryStatus);

			entryHeader.CH_MPIBioStatus = LowValueConsignmentStatusList.Codes.ConsignmentHeld;
			statusCalculator = BusinessEntityTestStatus(StatusList.Codes.MPIBiosecurityDirectionsGivenCleared, TSWStatus.StatusCodes.Cleared, ResponsibleGovernmentAgencyList.Codes.MPIBIO, LowValueConsignmentStatusList.Codes.ConsignmentHeld);
			WriteOffDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedWriteOffStatus;
			AssertEquals("CombinedStatus shows Bio response is held", LowValueConsignmentStatusList.Codes.CH, WriteOffDeclaration.JE_TSWCombinedStatus);
			WriteOffDeclaration.JE_EntryStatus = statusCalculator.GetWriteOffEntryStatus;
			AssertEquals("Calculated Entry Status", LowValueConsignmentStatusList.Codes.ConsignmentHeld, WriteOffDeclaration.JE_EntryStatus);
		}

		public void TestICR_CIRStatus()
		{
			WriteOffDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			WriteOffDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			WriteOffDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			WriteOffDeclaration.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.PP;
			WriteOffDeclaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			var entryHeader = WriteOffDeclaration.CusEntryHeader;
			entryHeader.CH_MPIBioStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			entryHeader.CH_NZCSStatus = LowValueConsignmentStatusList.Codes.ConsolidationIcrRequired;

			var statusCalculator = BusinessEntityTestStatus(StatusList.Codes.CustomsCargoReportNotification, TSWStatus.WriteOffStatusCodes.CDR, ResponsibleGovernmentAgencyList.Codes.NZCS, LowValueConsignmentStatusList.Codes.ConsolidationIcrRequired);
			WriteOffDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedWriteOffStatus;
			AssertEquals("CombinedStatus shows Bio response is written off", LowValueConsignmentStatusList.Codes.IP, WriteOffDeclaration.JE_TSWCombinedStatus);
			WriteOffDeclaration.JE_EntryStatus = statusCalculator.GetWriteOffEntryStatus;
			AssertEquals("Calculated Entry Status shows CIR", LowValueConsignmentStatusList.Codes.ConsolidationIcrRequired, WriteOffDeclaration.JE_EntryStatus);
		}

		public void TestImportEntryStatusIsConsistentWithMultipleAgencyResponses()
		{
			/*
			// JE_EntryStatus is sometimes showing a declaration has a Delivery Order when the TSW combined status reflects 1 agency may have a hold on the entry
			//	e.g. MPI Food Cleared / NZCS - Delivery Order / MPI Biosecurity Inspection / Audit requirements
			// Entry status in this case should not show Delivery Order		(see comment below - Entry Status will again show the Delivery Order always if it has been received)
			//
			//	See WI00212010 - NZ Customs have actually now decided they do want the DO to show for users even if other MPI statuses are not clear. They have asked us to reverse the change we made for the WI that created this test case.
			*/
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_EntryStatus = "STC";
			JobDeclaration.CusEntryHeader.CH_NZCSStatus = StatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecified;

			var statusCalculator = TestStatus(StatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecified, TSWStatus.StatusCodes.Cleared, ResponsibleGovernmentAgencyList.Codes.NZCS);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("CombinedStatus should show Agency responses are still pending", TSWEntryStatusList.Codes.CPP, JobDeclaration.JE_TSWCombinedStatus);
			JobDeclaration.JE_EntryStatus = statusCalculator.CalculateEntryStatus;
			AssertEquals("CalculateEntryStatus - Once a DOR has been received from Customs that will ALWAYS display in the Entry Status", FormalEntryStatusList.Codes.DeliveryOrderReceived, JobDeclaration.JE_EntryStatus);

			statusCalculator = TestStatus(StatusList.Codes.MPIBiosecurityDirectionsGiven, TSWStatus.StatusCodes.Inspection, ResponsibleGovernmentAgencyList.Codes.MPIBIO);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("CombinedStatus should still show Agency response & Inspection are still pending", TSWEntryStatusList.Codes.CIP, JobDeclaration.JE_TSWCombinedStatus);
			JobDeclaration.JE_EntryStatus = statusCalculator.CalculateEntryStatus;
			AssertEquals("CalculateEntryStatus - Once a DOR has been received from Customs that will ALWAYS display in the Entry Status", FormalEntryStatusList.Codes.DeliveryOrderReceived, JobDeclaration.JE_EntryStatus);

			statusCalculator = TestStatus(StatusList.Codes.MPIFoodCleared, TSWStatus.StatusCodes.Cleared, ResponsibleGovernmentAgencyList.Codes.MPIFOOD);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("CombinedStatus should still show Agency (MPI Biosecurity) Inspection is required", TSWEntryStatusList.Codes.CIC, JobDeclaration.JE_TSWCombinedStatus);
			JobDeclaration.JE_EntryStatus = statusCalculator.CalculateEntryStatus;
			AssertEquals("CalculateEntryStatus - Once a DOR has been received from Customs that will ALWAYS display in the Entry Status", FormalEntryStatusList.Codes.DeliveryOrderReceived, JobDeclaration.JE_EntryStatus);

			statusCalculator = TestStatus(StatusList.Codes.MPIBiosecurityCleared, TSWStatus.StatusCodes.Cleared, ResponsibleGovernmentAgencyList.Codes.MPIBIO);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("CombinedStatus should now show all Agency responses are cleard", TSWEntryStatusList.Codes.CCC, JobDeclaration.JE_TSWCombinedStatus);
			JobDeclaration.JE_EntryStatus = statusCalculator.CalculateEntryStatus;
			AssertEquals("CalculateEntryStatus of DOR from Customs now equates to All Agency Clearance received", FormalEntryStatusList.Codes.DeliveryOrderReceived, JobDeclaration.JE_EntryStatus);
		}

		public void TestCombinedImportStatusFromReplacement()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_EntryStatus = "STC";
			JobDeclaration.CusEntryHeader.CH_NZCSStatus = StatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecified;

			var statusCalculator = TestStatus(StatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecified, TSWStatus.StatusCodes.Cleared, ResponsibleGovernmentAgencyList.Codes.NZCS);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("CombinedStatus should show Agency responses are still pending", TSWEntryStatusList.Codes.CPP, JobDeclaration.JE_TSWCombinedStatus);

			statusCalculator = TestStatus(StatusList.Codes.MPIBiosecurityDirectionsGiven, TSWStatus.StatusCodes.Inspection, ResponsibleGovernmentAgencyList.Codes.MPIBIO);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("CombinedStatus should still show Agency response & Inspection are still pending", TSWEntryStatusList.Codes.CIP, JobDeclaration.JE_TSWCombinedStatus);

			statusCalculator = TestStatus(StatusList.Codes.MPIFoodCleared, TSWStatus.StatusCodes.Cleared, ResponsibleGovernmentAgencyList.Codes.MPIFOOD);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("CombinedStatus should still show Agency (MPI Biosecurity) Inspection is required", TSWEntryStatusList.Codes.CIC, JobDeclaration.JE_TSWCombinedStatus);

			// simulate replacement message being sent
			JobDeclaration.JE_EntryStatus = "STC";
			JobDeclaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.PIC;
			statusCalculator = TestStatus(StatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecified, TSWStatus.StatusCodes.Cleared, ResponsibleGovernmentAgencyList.Codes.NZCS);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("CombinedStatus should update with Customs response but still show Agency (MPI Biosecurity) Inspection is required", TSWEntryStatusList.Codes.CIC, JobDeclaration.JE_TSWCombinedStatus);

			statusCalculator = TestStatus(StatusList.Codes.MPIBiosecurityCleared, TSWStatus.StatusCodes.Cleared, ResponsibleGovernmentAgencyList.Codes.MPIBIO);
			JobDeclaration.JE_TSWCombinedStatus = statusCalculator.GetCombinedStatus;
			AssertEquals("CombinedStatus should now show all Agency responses are cleard", TSWEntryStatusList.Codes.CCC, JobDeclaration.JE_TSWCombinedStatus);
		}

		TestDeclaration JobDeclaration
		{
			get
			{
				if (jobDeclaration == null)
				{
					jobDeclaration = Factory.NewWithValidTestData<TestDeclaration>();
				}

				return jobDeclaration;
			}
		}
		TestDeclaration jobDeclaration;

		#region TestDeclaration with Interface

		public class TestDeclaration : JobDeclaration, ITSWStatus
		{
			public TestDeclaration(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public string MsgResponseStatus
			{
				get { return fMsgResponseStatus; }
				set
				{
					fMsgResponseStatus = value;
				}
			}
			string fMsgResponseStatus;

			public string EnterpriseStatus
			{
				get { return fEnterpriseStatus; }
				set
				{
					fEnterpriseStatus = value;
				}
			}
			string fEnterpriseStatus;

			public string ResponsibleAgency
			{
				get { return fResponsibleAgency; }
				set
				{
					fResponsibleAgency = value;
				}
			}
			string fResponsibleAgency;

			JobDeclaration ITSWStatus.Declaration => this;

			ZString ITSWStatus.ResponseStatus => MsgResponseStatus;

			ZString ITSWStatus.EnterpriseStatus => EnterpriseStatus;

			ZString ITSWStatus.Agency => ResponsibleAgency;
		}

		#endregion

		TestWriteOffDeclaration WriteOffDeclaration
		{
			get
			{
				if (writeOffDeclaration == null)
				{
					writeOffDeclaration = Factory.NewWithValidTestData<TestWriteOffDeclaration>();
				}

				return writeOffDeclaration;
			}
		}
		TestWriteOffDeclaration writeOffDeclaration;

		#region TestDeclaration with IWriteOffStatus Interface

		public class TestWriteOffDeclaration : JobDeclaration, IWriteOffStatus
		{
			public TestWriteOffDeclaration(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public string MsgResponseStatus
			{
				get { return fMsgResponseStatus; }
				set
				{
					fMsgResponseStatus = value;
				}
			}
			string fMsgResponseStatus;

			public string EnterpriseStatus
			{
				get { return fEnterpriseStatus; }
				set
				{
					fEnterpriseStatus = value;
				}
			}
			string fEnterpriseStatus;

			public string ResponsibleAgency
			{
				get { return fResponsibleAgency; }
				set
				{
					fResponsibleAgency = value;
				}
			}
			string fResponsibleAgency;

			JobDeclaration ITSWStatus.Declaration => this;

			ZString ITSWStatus.ResponseStatus => MsgResponseStatus;

			ZString ITSWStatus.EnterpriseStatus => EnterpriseStatus;

			ZString ITSWStatus.Agency => ResponsibleAgency;

			Declaration.ECIWriteOff.CusEntryHeader IWriteOffStatus.EntryHeader => IsTSWWriteOff ? (Declaration.ECIWriteOff.CusEntryHeader)CusEntryHeader : null;

			ZString IWriteOffStatus.CustomsStatus => JE_EntryStatus;

			public ZString GoodsClearanceStatus
			{
				get { return fGoodsClearanceStatus; }
				set
				{
					fGoodsClearanceStatus = value;
				}
			}
			string fGoodsClearanceStatus;

			ZString IWriteOffStatus.CombinedStatus => JE_TSWCombinedStatus;
		}

		#endregion

		#region TestHAWB

		public class TestHAWB : CusHAWB, ITSWStatus
		{
			public TestHAWB(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public string MsgResponseStatus
			{
				get { return fMsgResponseStatus; }
				set
				{
					fMsgResponseStatus = value;
				}
			}
			string fMsgResponseStatus;

			public string EnterpriseStatus
			{
				get { return fEnterpriseStatus; }
				set
				{
					fEnterpriseStatus = value;
				}
			}
			string fEnterpriseStatus;

			public string ResponsibleAgency
			{
				get { return fResponsibleAgency; }
				set
				{
					fResponsibleAgency = value;
				}
			}
			string fResponsibleAgency;

			JobDeclaration ITSWStatus.Declaration => null;

			ZString ITSWStatus.ResponseStatus => MsgResponseStatus;

			ZString ITSWStatus.EnterpriseStatus => EnterpriseStatus;

			ZString ITSWStatus.Agency => ResponsibleAgency;
		}

		#endregion

		#region TestSCAHouse

		public class TestSCAHouse : CusSCAHouse, ITSWStatus
		{
			public TestSCAHouse(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public string MsgResponseStatus
			{
				get { return fMsgResponseStatus; }
				set
				{
					fMsgResponseStatus = value;
				}
			}
			string fMsgResponseStatus;

			public string EnterpriseStatus
			{
				get { return fEnterpriseStatus; }
				set
				{
					fEnterpriseStatus = value;
				}
			}
			string fEnterpriseStatus;

			public string ResponsibleAgency
			{
				get { return fResponsibleAgency; }
				set
				{
					fResponsibleAgency = value;
				}
			}
			string fResponsibleAgency;

			JobDeclaration ITSWStatus.Declaration => null;

			ZString ITSWStatus.ResponseStatus => MsgResponseStatus;

			ZString ITSWStatus.EnterpriseStatus => EnterpriseStatus;

			ZString ITSWStatus.Agency => ResponsibleAgency;
		}

		#endregion

		TSWStatus TestStatus(string msgResponseStatus, string enterpriseStatus, string responsibleAgency)
		{
			JobDeclaration.MsgResponseStatus = msgResponseStatus;
			JobDeclaration.EnterpriseStatus = enterpriseStatus;
			JobDeclaration.ResponsibleAgency = responsibleAgency;
			return new TSWStatus(JobDeclaration);
		}

		TSWStatus BusinessEntityTestStatus(string msgResponseStatus, string enterpriseStatus, string responsibleAgency, string clearanceStatus)
		{
			WriteOffDeclaration.MsgResponseStatus = msgResponseStatus;
			WriteOffDeclaration.EnterpriseStatus = enterpriseStatus;
			WriteOffDeclaration.ResponsibleAgency = responsibleAgency;
			WriteOffDeclaration.GoodsClearanceStatus = clearanceStatus;
			var businessEntity = (IWriteOffStatus)WriteOffDeclaration;
			return new TSWStatus(businessEntity);
		}
	}
}
