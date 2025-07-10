using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface ILPCOAuthorizedParty
	{
		ZString Name { get; }

		ZString ID { get; }

		ZString TypeCode { get; }
	}
}
