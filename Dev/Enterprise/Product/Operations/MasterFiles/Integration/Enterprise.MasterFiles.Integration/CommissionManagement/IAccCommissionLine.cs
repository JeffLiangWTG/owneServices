using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IAccCommissionLine
	{
		ZGuid PK { get; }
		ZGuid CL0_CAT { get; set; }
		ZString CL0_CommissionType { get; }
		ZDecimal CL0_EntityCommissionAmount { get; }
		ZDecimal CL0_EntityPercentage { get; }
		ZString CL0_GS_NKStaff { get; }
		ZGuid CL0_OH_Party { get; }
		ZString CL0_RX_NKCommissionCurrency { get; }
		ZString CL0_RX_NKTransactionCurrency { get; }
		ZDecimal CL0_ShareCommissionAmount { get; }
		ZByte CL0_SharePortion { get; }
		ZShort CL0_ShareTotal { get; }
		ZDecimal CL0_TotalCommissionableAmount { get; }
		ZDecimal CL0_TransactionAmount { get; }
		ZBool CL0_ShouldReinstate { get; }
		ZBool IsPaid { get; }

		void Cancel();
	}
}
