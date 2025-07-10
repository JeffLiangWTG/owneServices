using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IENS90
	{
		ZDecimal GetTotal(bool deferred);
	}
}
