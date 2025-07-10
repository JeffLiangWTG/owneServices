using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public sealed class BillConfiguration : EU.NCTS.Business.BillConfiguration
{
	#region Additional Document Validation Decider

	protected override INctsBillAdditionalDocumentValidationDecider GetBillAdditionalDocumentPhase5ValidationDecider() => new NctsBillAdditionalDocumentPhase5ValidationDecider();

	#endregion

	protected override INctsBillArrivalPhase5ValidationDecider GetBillArrivalPhase5ValidationDecider() => new NctsBillArrivalPhase5ValidationDecider();

	protected override INctsBillPhase5ValidationDecider GetBillDeparturePhase5ValidationDecider() => new NctsBillDeparturePhase5ValidationDecider();
}
