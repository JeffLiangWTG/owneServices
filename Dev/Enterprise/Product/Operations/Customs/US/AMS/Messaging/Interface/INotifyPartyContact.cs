using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Messaging.Interface
{
	public interface INotifyPartyContact
	{
		ZString ContactName { get; }
		ZString CommNumberQualifier { get; }
		ZString CommunicationsNumber { get; }
		ZString CommNumberQualifier2 { get; }
		ZString CommunicationsNumber2 { get; }
	}
}
