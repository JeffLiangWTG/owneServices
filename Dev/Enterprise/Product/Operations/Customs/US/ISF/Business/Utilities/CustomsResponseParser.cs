using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;

namespace Enterprise.Customs.US.ISF.Business
{
	public class CustomsResponseParser
	{
		public CustomsResponseParser(US.Business.MQEDIMessage message)
		{
			this.message = message;
		}
		readonly US.Business.MQEDIMessage message;

		public ZString MessageTypeCode
		{
			get
			{
				if (messageTypeCodeCached == null)
				{
					messageTypeCodeCached = new CachedProperty<ZString>(Factory, delegate
						{
							ZString result = ZString.Empty;
							if (message != null)
							{
								foreach (ISFSF90 fs90 in message.GetMessageBlocks<ISFSF90>())
								{
									if (result.IsEmpty && !fs90.MessageTypeCode.IsEmpty)
									{
										result = fs90.MessageTypeCode;
										break;
									}
								}
							}
							return result;
						});
				}
				return messageTypeCodeCached.Value;
			}
		}
		CachedProperty<ZString> messageTypeCodeCached;

		BusinessObjectFactory Factory
		{
			get { return message.Factory; }
		}
	}
}
