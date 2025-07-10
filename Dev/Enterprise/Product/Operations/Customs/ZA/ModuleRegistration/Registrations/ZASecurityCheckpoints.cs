using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.ModuleRegistration;

public static class ZASecurityCheckpoints
{
	public static readonly CheckpointLookupKey ZACustomsStatement = new(nameof(ZACustomsStatement));
	public static readonly CheckpointLookupKey ZA404ProofOfPayment = new(nameof(ZA404ProofOfPayment));
	public static readonly CheckpointLookupKey ZA404ProofOfPaymentCustomiseDocuments = new(nameof(ZA404ProofOfPaymentCustomiseDocuments));
	public static readonly CheckpointLookupKey ZACustomsResponse = new(nameof(ZACustomsResponse));
	public static readonly CheckpointLookupKey ZAOutturnAndGateInOut = new(nameof(ZAOutturnAndGateInOut));
	public static readonly CheckpointLookupKey ZAGenralMessage = new(nameof(ZAGenralMessage));
	public static readonly CheckpointLookupKey ZAWarehouseOperatorTransactions = new(nameof(ZAWarehouseOperatorTransactions));
}
