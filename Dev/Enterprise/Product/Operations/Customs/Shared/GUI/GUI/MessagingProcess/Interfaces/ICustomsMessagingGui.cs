using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.MessagingProcess
{
	public interface ICustomsMessagingGui
	{
		ICustomsMessagingSupporter MessagingSupporter { get; }
		ZForm TopLevelBusinessObjectForm { get; }
	}
}
