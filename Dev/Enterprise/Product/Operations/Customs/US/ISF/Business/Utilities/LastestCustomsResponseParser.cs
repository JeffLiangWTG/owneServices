using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.ISF.Business
{
	public class LastestCustomsResponseParser
	{
		public LastestCustomsResponseParser(CusISFHeader header)
		{
			this.header = header;
		}
		readonly CusISFHeader header;

		public US.Business.MQEDIMessage LastestResponse
		{
			get
			{
				if (lastestResponseCached == null)
				{
					lastestResponseCached = new CachedProperty<US.Business.MQEDIMessage>(Factory, delegate
						{
							return (US.Business.MQEDIMessage)header.Messages.GetLastMessage(US.Business.EDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse, US.Business.EDIMessage.Direction.Receive);
						});
				}
				return lastestResponseCached.Value;
			}
		}
		CachedProperty<US.Business.MQEDIMessage> lastestResponseCached;

		public ZString LatestMessageTypeCode
		{
			get { return LatestMessageParser.MessageTypeCode; }
		}

		CustomsResponseParser LatestMessageParser
		{
			get
			{
				if (latestMessageParserCached == null)
				{
					latestMessageParserCached = new CachedProperty<CustomsResponseParser>(Factory, delegate
						{
							return new CustomsResponseParser(LastestResponse);
						});
				}
				return latestMessageParserCached.Value;
			}
		}
		CachedProperty<CustomsResponseParser> latestMessageParserCached;

		BusinessObjectFactory Factory
		{
			get { return header.Factory; }
		}
	}
}
