using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class FTZControlNumberSupporter : IAllocateNumberSupporter
	{
		public FTZControlNumberSupporter(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		#region IAllocateEntryNumber

		ZString IAllocateNumberSupporter.GetReasonToStopProceeding()
		{
			var result = ZString.Empty;
			if (declaration.HasFTZTransactionsWithCustoms && !declaration.FTZControlNumber.IsEmpty)
			{
				result = JobDeclaration.Constants.FTZControlNumberAllocation.FTZControlNumberAlreadyAllocated(declaration.FTZControlNumber);
			}
			return result;
		}

		void IAllocateNumberSupporter.UnlockNumberAllocationMutex()
		{
			declaration.UnlockFTZAdmissionNumberAllocationMutex();
		}

		bool IAllocateNumberSupporter.LockNumberAllocationMutex
		{
			get { return declaration.LockFTZAdmissionNumberAllocationMutex; }
		}

		string IAllocateNumberSupporter.GetNumberAllocationMutexLockInfo()
		{
			return declaration.GetFTZAdmissionNumberAllocationMutexInfo();
		}

		ZString IAllocateNumberSupporter.GetExistingNumber()
		{
			return declaration.FTZControlNumber;
		}

		AllocateNumber IAllocateNumberSupporter.GetNewAllocateNumber()
		{
			return null;
		}

		void IAllocateNumberSupporter.DoAllocate(ZString userEnteredNumber)
		{
			declaration.AllocateFTZControlNumberOnSaving = true;
		}

		void IAllocateNumberSupporter.OnAllocatedNumberSaved()
		{
			declaration.FTZControlNumberInfo.RefreshBinding();
		}

		ZString IAllocateNumberSupporter.NumberType
		{
			get { return "FTZ Control Number"; }
		}

		#endregion
	}
}
