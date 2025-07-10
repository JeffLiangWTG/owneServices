using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class CusInBondHeaderEntryNumberSupporter : IAllocateNumberSupporter
	{
		public CusInBondHeaderEntryNumberSupporter(CusInBondHeader cusInBondHeader)
		{
			this.cusInBondHeader = cusInBondHeader;
		}

		readonly CusInBondHeader cusInBondHeader;

		ZString IAllocateNumberSupporter.NumberType => Res.GetString("a5378187-e56b-47f5-9d84-38c17f08f7db", "Entry Number");

		bool IAllocateNumberSupporter.LockNumberAllocationMutex => cusInBondHeader.LockEntryNumberAllocationMutex;

		void IAllocateNumberSupporter.DoAllocate(ZString userEnteredNumber)
		{
			cusInBondHeader.AllocateEntryNumber(userEnteredNumber);
		}

		ZString IAllocateNumberSupporter.GetConfirmMessagesWhenIsWaitingForResponseOrHasBeenLodgedAtCustoms()
		{
			return Res.GetString("bdc3a98e-3879-4f2a-918b-a2ae7acf38ca", "The job is still waiting for a response. Allocating a new entry number means that this transhipment job will be treated as a new entry in the customs’ system. Do you want to proceed?");
		}

		ZString IAllocateNumberSupporter.GetExistingNumber()
		{
			return cusInBondHeader.EntryNumber;
		}

		AllocateNumber IAllocateNumberSupporter.GetNewAllocateNumber()
		{
			return AllocateNumber;
		}

		AllocateNumber AllocateNumber => allocateNumber ?? (allocateNumber = new TranshipmentAllocateNumber(cusInBondHeader));
		AllocateNumber allocateNumber;

		string IAllocateNumberSupporter.GetNumberAllocationMutexLockInfo()
		{
			return cusInBondHeader.GetEntryNumberAllocationMutexLockInfo();
		}

		ZString IAllocateNumberSupporter.GetReasonToStopProceeding()
		{
			cusInBondHeader.ReloadEntryNumber();
			return AllocateNumber.AllowedGeneratorDescriptionWhenAutoGenerate;
		}

		ZString IAllocateNumberSupporter.GetReasonToStopProceedingWhenEmptyNumber()
		{
			return AllocateNumber.AllowedGeneratorDescriptionWhenAutoGenerate;
		}

		void IAllocateNumberSupporter.OnAllocatedNumberSaved()
		{
			cusInBondHeader.EntryNumberInfo.RefreshBinding();
			cusInBondHeader.Validation.ValidateEntryNumber();
		}

		void IAllocateNumberSupporter.UnlockNumberAllocationMutex()
		{
			cusInBondHeader.UnlockEntryNumberAllocationMutex();
		}
	}
}
