using CargoWise.Types;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public interface IAIMReasonForAmendment
	{
		ZString AmendmentCode { get; }
		ZString AmendmentExplanation { get; }
	}
}
