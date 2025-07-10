using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface IMessageSender
	{
		BusinessObject Parent { get; }
		IBusinessObjectCollection Messages { get; }
		ZString JobReference { get; }
	}
}
