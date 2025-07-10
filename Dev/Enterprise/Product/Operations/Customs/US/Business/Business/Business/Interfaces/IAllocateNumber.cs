using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface IAllocateNumberSupporter
	{
		ZString GetReasonToStopProceeding();
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

namespace Enterprise.Customs.US.Business.EntryNumber
{
	public static class IAllocateNumberExtension
	{
		public static IAllocateNumberSupporter GetAllocateNumberSupporter(this JobDeclaration declaration)
		{
			if ((declaration.IsImport && !declaration.IsImportByExternalBroker) || declaration.IsDrawback)
			{
				return new FormalDeclarationEntryNumberSupporter(declaration);
			}
			else if (declaration.IsRecon)
			{
				return declaration.ReconDeclaration;
			}
			else if (declaration.IsProtest)
			{
				return declaration.Protest;
			}
			return null;
		}

		class FormalDeclarationEntryNumberSupporter : IAllocateNumberSupporter
		{
			public FormalDeclarationEntryNumberSupporter(JobDeclaration declaration)
			{
				this.declaration = declaration;
			}

			readonly JobDeclaration declaration;

			#region IAllocateEntryNumber

			ZString IAllocateNumberSupporter.GetReasonToStopProceeding()
			{
				declaration.ReloadExistingDataRelatedToImportEntryNumberAllocation();
				return declaration.DisallowAllocateImportEntryNumber;
			}

			void IAllocateNumberSupporter.UnlockNumberAllocationMutex()
			{
				declaration.UnlockImportEntryNumberAllocationMutex();
			}

			bool IAllocateNumberSupporter.LockNumberAllocationMutex
			{
				get { return declaration.LockImportEntryNumberAllocationMutex; }
			}

			string IAllocateNumberSupporter.GetNumberAllocationMutexLockInfo()
			{
				return declaration.GetImportEntryNumberAllocationMutexLockInfo();
			}

			ZString IAllocateNumberSupporter.GetExistingNumber()
			{
				return declaration.ImportEntryNumber;
			}

			AllocateNumber IAllocateNumberSupporter.GetNewAllocateNumber()
			{
				var args = new AllocateNumberArgs();
				args.MaxLength = 8;
				args.EntryFilerCode = declaration.US_EntryFilerCode;
				args.Branch = declaration.Branch;
				args.Factory = declaration.Factory;
				args.ValidateNumber = ((info, validator) => validator.ValidateFormalEntryNumber(info));
				return new AllocateNumber(args);
			}

			void IAllocateNumberSupporter.DoAllocate(ZString userEnteredNumber)
			{
				declaration.AllocateEntryNumber(userEnteredNumber);
			}

			void IAllocateNumberSupporter.OnAllocatedNumberSaved()
			{
				declaration.UpdateConsolidatedEntryNoForReleaseEntry();
				declaration.DecEntryNumberInfo.RefreshBinding();
			}

			ZString IAllocateNumberSupporter.NumberType
			{
				get { return "Entry Number"; }
			}

			#endregion
		}
	}
}
