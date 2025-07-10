using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IStatementRerouteResponse
	{
		ZString ErrorCode { get; }
		ZString MessageText { get; }
		ZInt TotalNumberOfReroutes { get; }
	}
}
