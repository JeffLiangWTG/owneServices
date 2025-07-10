using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public sealed class NctsSupportingDocumentConfiguration : EU.NCTS.Business.NctsSupportingDocumentConfiguration
{
	protected override INctsSupportingDocumentDeparturePhase5ValidationDecider GetNctsSupportingDocumentDeparturePhase5ValidationDecider() => new NctsSupportingDocumentDeparturePhase5ValidationDecider();
}
