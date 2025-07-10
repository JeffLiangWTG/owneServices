using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface I7501Errors
	{
		ZString LineNumber { get; }
		ZString Code { get; }
		ZString NarrativeMessage { get; }
		bool IsError { get; }
	}

	/// <summary>
	/// Shows for 7501 Status
	/// </summary>
	public interface I7501Status
	{
		ZString Code { get; }
		ZString NarrativeMessage { get; }
		bool IsMessageStatus { get; }
		ZDateTime StatusDate { get; }
	}

	public interface IInBondErrors
	{
		ZString NarrativeMessage { get; }
	}
}
