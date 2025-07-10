using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	public static class CaseNumberHelper
	{
		public static void CreateNewCaseNumberFromProvisionalPaymentCusEntryPayInfo(CusEntryHeader linkedCusEntryHeader, CusEntryPayInfo newPPPayInfo)
		{
			ZString[] matchingTransactionTypes = { "FOR", "PEN", "PPA", "PPC", "PPE", "PPG", "PPR", "PPT" };
			var isValiidTransactionType = matchingTransactionTypes.Contains(newPPPayInfo.C9_TransactionType);

			var caseNumbers = linkedCusEntryHeader?.EntryInstruction?.CaseNumbers;
			if (isValiidTransactionType && (!string.IsNullOrEmpty(newPPPayInfo.C9_PaymentReference) && !caseNumbers.Cast<CaseNumber>().Any(x => x.CY_Data == newPPPayInfo.C9_PaymentReference)))
			{
				_ = caseNumbers.AddNew(CaseNumberTypeList.Codes.ElectronicProvisionalPayments, newPPPayInfo.C9_PaymentReference);
			}
		}

		public static void DeleteCaseNumberFromProvisionalPaymentCusEntryPayInfo(CusEntryHeader linkedCusEntryHeader, CusEntryPayInfo pendingEntryPayInfo)
		{
			var caseNumbers = linkedCusEntryHeader?.EntryInstruction?.CaseNumbers.Cast<CaseNumber>();
			if (caseNumbers.Count(x => x.CY_Data == pendingEntryPayInfo.C9_PaymentReference && x.CY_Code == CaseNumberTypeList.Codes.ElectronicProvisionalPayments) == 1)
			{
				var caseNumber = caseNumbers.Single(x => x.CY_Data == pendingEntryPayInfo.C9_PaymentReference && x.CY_Code == CaseNumberTypeList.Codes.ElectronicProvisionalPayments);
				linkedCusEntryHeader.EntryInstruction.CaseNumbers.RemoveAndDelete(caseNumber);
			}
		}
	}
}
