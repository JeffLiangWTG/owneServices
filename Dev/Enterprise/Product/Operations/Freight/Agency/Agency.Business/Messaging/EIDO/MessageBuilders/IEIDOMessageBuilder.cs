namespace Enterprise.Freight.Agency.Business
{
	public interface IEIDOMessageBuilder
	{
		string GenerateMessageText(IEIDOMessagingData data);
	}
}
