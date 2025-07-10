using CargoWise.Types;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public interface IFreightStatusQuery
	{
		ZString StatusRequestCode { get; }
	}
}
