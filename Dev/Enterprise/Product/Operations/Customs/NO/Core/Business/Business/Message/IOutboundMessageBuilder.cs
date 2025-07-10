namespace Enterprise.Customs.NO.Business
{
	public interface IOutboundMessageBuilder
	{
		OutboundEDIMessage Create(IMessageInformationProvider informationProvider);
	}
}
