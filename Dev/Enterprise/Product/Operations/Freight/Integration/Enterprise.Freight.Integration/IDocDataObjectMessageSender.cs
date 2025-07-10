using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Integration
{
	public interface IDocDataObjectMessageSender
	{
		bool SendMessage(BusinessObject bizObj, IStmMenuItem menuItem, INotifications notifications);
	}
}
