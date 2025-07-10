using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IINPB04
	{
		ZString ReferenceIdentifierQualifier { get; }
		ZString ReferenceIdentifier { get; }
	}
}
