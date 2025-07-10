using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class CommunicationWrapper : ICommunication
	{
		public CommunicationWrapper(ZString id, ZString typeID)
		{
			ID = id;
			TypeID = typeID;
		}

		public ZString ID { get; }
		public ZString TypeID { get; }
	}
}
