using CargoWise.Types;
using Enterprise.Customs.Business.MessagingProcess;

namespace Enterprise.Customs.TR.Business.MessagingProcess
{
	public interface ITRCustomsMessageGenerator : ICustomsMessageGenerator
	{
		ZBool IsMessageSigningRequired { get; }
	}
}
