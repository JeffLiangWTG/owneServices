using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class FormalDeclarationEntryNumberSupporter : IAllocateNumberSupporter
	{
		public FormalDeclarationEntryNumberSupporter(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		#region IAllocateEntryNumber

		ZString IAllocateNumberSupporter.GetReasonToStopProceeding()
		{
			declaration.CustomsEntryHeaders.Reload(true);
			declaration.ReloadEntryNumber();
			return AllocateNumber.AllowedGeneratorDescriptionWhenAutoGenerate;
		}

		public ZString GetReasonToStopProceedingWhenEmptyNumber() => AllocateNumber.AllowedGeneratorDescriptionWhenAutoGenerate;

		void IAllocateNumberSupporter.UnlockNumberAllocationMutex()
		{
			declaration.UnlockEntryNumberAllocationMutex();
		}

		bool IAllocateNumberSupporter.LockNumberAllocationMutex
		{
			get { return declaration.LockEntryNumberAllocationMutex; }
		}

		string IAllocateNumberSupporter.GetNumberAllocationMutexLockInfo()
		{
			return declaration.GetEntryNumberAllocationMutexLockInfo();
		}

		ZString IAllocateNumberSupporter.GetExistingNumber()
		{
			return declaration.EntryNumber;
		}

		AllocateNumber IAllocateNumberSupporter.GetNewAllocateNumber() => AllocateNumber;

		AllocateNumber AllocateNumber => allocateNumber ?? (allocateNumber = new JobDeclarationAllocateNumber(declaration));
		AllocateNumber allocateNumber;

		void IAllocateNumberSupporter.DoAllocate(ZString userEnteredNumber)
		{
			declaration.AllocateEntryNumber(userEnteredNumber);
		}

		void IAllocateNumberSupporter.OnAllocatedNumberSaved()
		{
			declaration.DeclarationNumberDisplayInfo.RefreshBinding();
			declaration.Validation.ValidateDeclarationNumberDisplay();
		}

		ZString IAllocateNumberSupporter.NumberType => Res.GetString("eab4cefd-fdce-472b-bec4-d3c9e8588c4c", "Entry Number");

		ZString IAllocateNumberSupporter.GetConfirmMessagesWhenIsWaitingForResponseOrHasBeenLodgedAtCustoms() => Res.GetString("341C7C69-FAC2-4BF8-91EB-9065CDFD44EC", "The job is still waiting for a response or has been acknowledged by the customs. Allocating a new entry number means that this declaration job will be treated as a new entry in the customs’ system. Do you want to proceed?");
		#endregion
	}
}
