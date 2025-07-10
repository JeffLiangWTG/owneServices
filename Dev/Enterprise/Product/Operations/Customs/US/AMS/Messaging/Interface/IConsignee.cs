using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Messaging.Interface
{
	public interface IConsignee : IParty
	{
		ZString TypeCode { get; }
	}
}
