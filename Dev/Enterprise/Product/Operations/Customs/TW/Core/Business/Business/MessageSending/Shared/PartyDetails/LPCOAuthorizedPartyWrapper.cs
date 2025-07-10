using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class LPCOAuthorizedPartyWrapper : ILPCOAuthorizedParty
	{
		public LPCOAuthorizedPartyWrapper(ZString id, ZString typeCode)
		{
			ID = id;
			TypeCode = typeCode;
		}

		public LPCOAuthorizedPartyWrapper(ZString id) : this(id, ZString.Empty) { }

		public ZString Name { get; }

		public ZString ID { get; }

		public ZString TypeCode { get; }
	}
}
