using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface ICriticalValidation
	{
		void RegisterOnSavingCheck();
		void RunOnSavingCheck();
		void RunDeletedObjectOnSavingCheck();
		void RunAfterSavingCheck();
	}

	public interface ISupportCriticalValidation : IConflictWithCriticalFields
	{
		ICriticalValidation CriticalValidation { get; }
	}
}
