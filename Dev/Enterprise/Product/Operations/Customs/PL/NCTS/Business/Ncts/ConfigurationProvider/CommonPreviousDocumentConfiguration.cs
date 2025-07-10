using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public sealed class CommonPreviousDocumentConfiguration : EU.NCTS.Business.CommonPreviousDocumentConfiguration
{
	protected override ICommonPreviousDocumentValidationDecider GetCommonPreviousDocumentValidationDecider() => new CommonPreviousDocumentValidationDecider();

	protected override ICommonPreviousDocumentDepartureValidationDecider GetCommonPreviousDocumentDepartureValidationDecider() => new CommonPreviousDocumentDepartureValidationDecider();

	protected override ICommonPreviousDocumentArrivalValidationDecider GetCommonPreviousDocumentArrivalValidationDecider() => new CommonPreviousDocumentArrivalValidationDecider();
}
