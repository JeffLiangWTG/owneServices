using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IAccChargeTypeOverride
	{
		ZGuid PK { get; }
		ZGuid AN_AC_ChargeCode { get; }
		ZString AN_ChargeType { get; }
		ZString AN_InvoiceType { get; }
		ZString AN_JobDirection { get; }
		ZString AN_JobType { get; }
		ZDecimal AN_MarginPercentage { get; }
		bool IsDisbursement { get; }
		bool IsRevenue { get; }
		bool IsMargin { get; }
		bool HasOveriddenInvoiceType { get; }
	}
}
