using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	public static class EntryPayInfoHelper
	{
		public static void AddEntryPayInfo(CusEntryHeader linkedCusEntryHeader, ZDateTime dateTimeOfPayment, string paymentParty, ZString transactionType, ZDecimal dutyAmount, ZString incomingMessageNum, ZString paymentStatus)
		{
			var addedPayInfo = linkedCusEntryHeader.EntryPayInfos.AddNew();
			using (addedPayInfo.SuspendSettingHasChanges())
			using (addedPayInfo.GetValidationSuspender())
			{
				addedPayInfo.C9_PaymentDate = dateTimeOfPayment;
				addedPayInfo.C9_CusResReceived = true;
				addedPayInfo.C9_RemAdvReceived = false;
				addedPayInfo.C9_PaymentParty = paymentParty;
				addedPayInfo.C9_TransactionType = transactionType;
				addedPayInfo.C9_PaymentAmount = dutyAmount;
				addedPayInfo.C9_IncomingPayResponseNo = incomingMessageNum;
				addedPayInfo.C9_PaymentStatus = paymentStatus;
				((ILightValidationInternals)addedPayInfo).IsValid = true;
			}
			CaseNumberHelper.CreateNewCaseNumberFromProvisionalPaymentCusEntryPayInfo(linkedCusEntryHeader, addedPayInfo);
		}

		public static void AddEntryPayInfoFromOutGoingCUSDECMessage(CusEntryHeader linkedCusEntryHeader, ZAMessage cusdecEDIMessage, ZDateTime dateTimeOfPayment, ZString paymentParty, ZString paymentStatus)
		{
			var cusdecMessageNum = cusdecEDIMessage.EM_MessageNum;
			ZDecimal dutyAmount = cusdecEDIMessage.CustomsDutyNoS1P2BAfter + cusdecEDIMessage.S1P2BDutyAfter - cusdecEDIMessage.CustomsDutyNoS1P2BBefore - cusdecEDIMessage.S1P2BDutyBefore;
			ZDecimal vatAmount = cusdecEDIMessage.ValueAddedTaxAfter - cusdecEDIMessage.ValueAddedTaxBefore;
			ZDecimal prpAmount = cusdecEDIMessage.ProvisionalPaymentAmountAfter - cusdecEDIMessage.ProvisionalPaymentAmountBefore;
			ZDecimal penAmount = cusdecEDIMessage.PenaltyAmountAfter - cusdecEDIMessage.PenaltyAmountBefore;

			AddEntryPayInfo(linkedCusEntryHeader, dateTimeOfPayment, paymentParty, "DTY", dutyAmount, cusdecMessageNum, paymentStatus);
			AddEntryPayInfo(linkedCusEntryHeader, dateTimeOfPayment, paymentParty, "VAT", vatAmount, cusdecMessageNum, paymentStatus);
			AddEntryPayInfo(linkedCusEntryHeader, dateTimeOfPayment, PaymentMethodCodeList.Codes.Cash, "OTH", prpAmount, cusdecMessageNum, paymentStatus);
			AddEntryPayInfo(linkedCusEntryHeader, dateTimeOfPayment, PaymentMethodCodeList.Codes.Cash, "PEN", penAmount, cusdecMessageNum, paymentStatus);
		}
	}
}
