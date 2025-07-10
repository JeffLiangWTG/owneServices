using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public interface IAllocateNumberSupporter
	{
		ZString GetConfirmMessagesWhenIsWaitingForResponseOrHasBeenLodgedAtCustoms();
		ZString GetReasonToStopProceeding();
		ZString GetReasonToStopProceedingWhenEmptyNumber();
		ZString GetExistingNumber();
		AllocateNumber GetNewAllocateNumber();
		void DoAllocate(ZString userEnteredNumber);
		void OnAllocatedNumberSaved();
		ZString NumberType { get; }
		bool LockNumberAllocationMutex { get; }
		void UnlockNumberAllocationMutex();
		string GetNumberAllocationMutexLockInfo();
	}
}
