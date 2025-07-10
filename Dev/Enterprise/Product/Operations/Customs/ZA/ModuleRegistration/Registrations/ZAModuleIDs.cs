using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.ModuleRegistration;

public static class ZAModuleIDs
{
	public static readonly ModuleIdentifier CustomsResponse = new ModuleIdentifier(ModuleId.CustomsResponse, ResString.GetMultilingualString("ModuleId.CustomsResponse", "Customs Response"));
	public static readonly ModuleIdentifier CustomsStatement = new ModuleIdentifier(ModuleId.ZACustomsStatement, ResString.GetMultilingualString("ModuleId.ZACustomsStatement", "Customs Statement"));
	public static readonly ModuleIdentifier GenralMessage = new ModuleIdentifier(ModuleId.ZAGenralMessage, ResString.GetMultilingualString("ModuleId.ZAGenralMessage", "GENRAL Messages"));
	public static readonly ModuleIdentifier OutturnAndGateInOut = new ModuleIdentifier(ModuleId.ZAOutturnAndGateInOut, ResString.GetMultilingualString("ModuleId.ZAOutturnAndGateInOut", "Outturn & Gate In/Out"));
	public static readonly ModuleIdentifier WarehouseOperatorTransactions = new ModuleIdentifier(ModuleId.ZAWarehouseOperatorTransactions, ResString.GetMultilingualString("ModuleId.ZAWarehouseOperatorTransactions", "Warehouse Operator Transactions"));
	public static readonly ModuleIdentifier ZA404ProofOfPayment = new ModuleIdentifier(ModuleId.ZA404ProofOfPayment, ResString.GetMultilingualString("ModuleId.ZA404ProofOfPayment", "VAT 404 – Proof of Payment"));
}
