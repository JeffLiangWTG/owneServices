using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	public interface IGateInOutStatusProvider
	{
		ZString GateInOutCustomsStatus { get; set; }
	}
}
