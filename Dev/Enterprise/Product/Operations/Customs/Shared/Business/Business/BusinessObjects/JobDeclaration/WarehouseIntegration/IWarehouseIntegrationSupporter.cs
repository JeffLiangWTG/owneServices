using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business.WarehouseExtensions
{
	public enum MessageAction { Original, Amendment, Withdrawal }

	public interface IWarehouseIntegrationSupporter : IStmALogParent, IWorkflowProvider
	{
		ZString EntryNumber { get; }
		ZGuid ClientPK { get; }
		OrgAddress WarehouseAddress { get; }
		GlbCompany Company { get; }
		ZString WarehouseTransactionStatus { get; set; }
		bool IsActive { get; }
		ISendsMessagesToCustoms MessageInitiator { get; }
		Notes Notes { get; }

		bool SupportsBondedWarehousing { get; }
		bool SupportModificationState { get; }
		bool IsBondedWarehousingDisabled { get; }
		bool IsOutwardBondedWarehousingEnabled { get; }
		bool IsInwardBondedWarehousingEnabled { get; }
		bool IsChangeOfOwnershipBondedWarehousingEnabled { get; }
		bool IsChangeOfRegimeWarehousingEnabled { get; }
		bool HasManualWhsUpdate { get; set; }
		bool IsIntoTemporaryImportEnabled { get; }
		bool IsOutOfTemporaryImportEnabled { get; }
		bool IsIntoTemporaryExportEnabled { get; }
		bool IsOutOfTemporaryExportEnabled { get; }
		bool IsIntoInwardProcessingEnabled { get; }
		bool IsOutOfInwardProcessingEnabled { get; }
		bool IsIntoOutwardProcessingEnabled { get; }
		bool IsOutOfOutwardProcessingEnabled { get; }

		ZString GetMessageErrorOfRequiredFieldsForBondedWarehousing(bool checkProduct, bool checkQuantity, bool checkEntryDetails);

		void DoActionOnOutwardAccepted(BusinessObject job);
		void DoActionOnOutwardCanceled(BusinessObject job);

		void UpdateHoldData(Shipment shipment, RecipientRoleType recipientRoleType);

		void PostUpdateBondedWarehouseInwardAction();
		void PostUpdateBondedWarehouseOutwardAction();
		void PostCancelBondedWarehouseInwardAction();
		void PostCancelBondedWarehouseOutwardAction();
	}
}
