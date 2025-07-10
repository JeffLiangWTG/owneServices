namespace Enterprise.Freight.Agency.Business
{
	public interface IPortAuthorityMessageBuilder
	{
		string GenerateMessageText(IPortAuthorityMessagingData data);
	}
}
