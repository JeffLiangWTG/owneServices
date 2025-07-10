using CargoWise.Types;

namespace Enterprise.Customs.Business.MessagingProcess.Declaration
{
	interface IJobDeclarationBondedWarehouseAutomation
	{
		ZString GetPendingTransactionError();
		bool ExecutePreAction();
		void ExecutePostAction();
		void ExecuteRestoreAction();
		bool PrepareForProcessing(InventoryAutomationAction inventoryAutomationAction, bool additionalBondedWarehouseRequirement);
	}
}
