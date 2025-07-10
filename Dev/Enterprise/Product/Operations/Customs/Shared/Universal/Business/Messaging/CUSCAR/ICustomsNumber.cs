using CargoWise.Types;

namespace Enterprise.Customs.Universal.Messaging.CUSCAR
{
	public interface ICustomsNumber
	{
		ZString Type { get; }
		ZString Number { get; }
	}
}
