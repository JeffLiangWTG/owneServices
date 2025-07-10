using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface IBondedParty
	{
		ZString ID { get; }

		ZString BondedID { get; }

		ZString TypeCode { get; }

		ZString CustomsControlID { get; }
	}
}
