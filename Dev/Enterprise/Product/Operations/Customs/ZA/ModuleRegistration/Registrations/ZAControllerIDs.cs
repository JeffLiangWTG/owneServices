using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.ModuleRegistration;

public static class ZAControllerIDs
{
	public static readonly ControllerID ZA404ProofOfPayment = new ControllerID("ZA404ProofOfPayment");
	public static readonly ControllerID CustomsResponse = new ControllerID("ZACustomsResponse");
	public static readonly ControllerID EntryHeader = new ControllerID("ZAEntryHeader");
	public static readonly ControllerID OrganisationDetailsPlugIn = ControllerIDs.Customs.ZA.OrganisationDetailsPlugIn;
	public static readonly ControllerID OutturnAndGateInOut = new ControllerID("ZAOutturnAndGateInOut");
	public static readonly ControllerID CALINFMessagingPlugin = ControllerIDs.Customs.ZA.CALINFMessagingPlugin;
	public static readonly ControllerID RefCusTariff = new ControllerID("ZARefCusTariff");
	public static readonly ControllerID GenralMessage = new ControllerID("ZAGenralMessage");
	public static readonly ControllerID WarehouseOperatorTransactions = new ControllerID("ZAWarehouseOperatorTransactions");
}
