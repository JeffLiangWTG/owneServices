using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMReasonForAmendment : IAIMReasonForAmendment
	{
		public AIMReasonForAmendment()
		{
		}

		public AIMReasonForAmendment(string amendmentCode, string amendmentExplanation)
		{
			AmendmentCode = amendmentCode;
			AmendmentExplanation = amendmentExplanation;
		}

		public void SetAmendmentReason(string amendmentCode, string amendmentExplanation)
		{
			AmendmentCode = amendmentCode;
			AmendmentExplanation = amendmentExplanation;
		}

		public ZString AmendmentCode { get; private set; }
		public ZString AmendmentExplanation { get; private set; }
	}
}
