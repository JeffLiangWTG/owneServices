using CargoWise.Types;
using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaManifestHeaderEntryNumberSupporter : IAllocateNumberSupporter
	{
		public AsycudaManifestHeaderEntryNumberSupporter(AsycudaManifestHeader header)
		{
			this.header = header;
		}

		readonly AsycudaManifestHeader header;

		#region IAllocateEntryNumber

		ZString IAllocateNumberSupporter.NumberType => Res.GetString("67047A4D-4141-4554-AE63-6C770B11C3D4", "Entry Number");

		bool IAllocateNumberSupporter.LockNumberAllocationMutex => header.LockEntryNumberAllocationMutex;

		void IAllocateNumberSupporter.DoAllocate(ZString userEnteredNumber) => header.AllocateEntryNumber(userEnteredNumber);

		ZString IAllocateNumberSupporter.GetConfirmMessagesWhenIsWaitingForResponseOrHasBeenLodgedAtCustoms() => Res.GetString("421D4A28-127C-4864-A51E-CB17D2BE9B91", "The job is still waiting for a response. Allocating a new entry number means that this transhipment job will be treated as a new entry in the customs’ system. Do you want to proceed?");

		ZString IAllocateNumberSupporter.GetExistingNumber() => header.DeclarationNumber;

		AllocateNumber IAllocateNumberSupporter.GetNewAllocateNumber() => AllocateNumber;

		AllocateNumber AllocateNumber => allocateNumber ?? (allocateNumber = new AsycudaManifestHeaderAllocateNumber(header));
		AllocateNumber allocateNumber;

		string IAllocateNumberSupporter.GetNumberAllocationMutexLockInfo() => header.GetEntryNumberAllocationMutexLockInfo();

		ZString IAllocateNumberSupporter.GetReasonToStopProceeding()
		{
			header.ReloadEntryNumber();
			return AllocateNumber.AllowedGeneratorDescriptionWhenAutoGenerate;
		}

		ZString IAllocateNumberSupporter.GetReasonToStopProceedingWhenEmptyNumber() => AllocateNumber.AllowedGeneratorDescriptionWhenAutoGenerate;

		void IAllocateNumberSupporter.OnAllocatedNumberSaved()
		{
			header.Validation.ValidateDeclarationNumberDisplay();
			header.DeclarationNumberDisplayInfo.RefreshBinding();
		}

		void IAllocateNumberSupporter.UnlockNumberAllocationMutex() => header.UnlockEntryNumberAllocationMutex();

		#endregion
	}
}
