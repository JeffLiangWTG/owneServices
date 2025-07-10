namespace Enterprise.MasterFiles.Integration
{
	public interface IAccCashAdvanceFunctionalityChecker
	{
		bool IsReceivablesCashAdvanceFunctionalityEnabled { get; }

		bool IsPayablesCashAdvanceFunctionalityEnabled { get; }

		bool IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed { get; }

		bool IsManualSettingOfPayablesCashAdvanceRequestStatusToPaidAllowed { get; }
	}
}
