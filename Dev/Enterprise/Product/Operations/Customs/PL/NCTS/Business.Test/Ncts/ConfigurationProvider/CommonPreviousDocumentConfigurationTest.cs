using System;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

public class CommonPreviousDocumentConfigurationTest : EU.NCTS.Business.Testing.CommonPreviousDocumentConfigurationTestCase<CommonPreviousDocumentConfiguration>
{
	protected override Type ExpectedCommonPreviousDocumentDepartureValidationDeciderType => typeof(CommonPreviousDocumentDepartureValidationDecider);
	protected override Type ExpectedCommonPreviousDocumentArrivalValidationDeciderType => typeof(CommonPreviousDocumentArrivalValidationDecider);
}
