using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface ICommunication
	{
		ZString ID { get; }

		ZString TypeID { get; }
	}
}
