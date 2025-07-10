using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Integration
{
	public interface IDocDataObjectWithoutUIMessageSender
	{
		bool SendMessage(BusinessObject bizObj, INotifications notifications);
	}
}
