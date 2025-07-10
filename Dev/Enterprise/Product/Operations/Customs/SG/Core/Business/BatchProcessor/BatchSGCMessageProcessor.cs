using System.Collections.Generic;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor
{
	public class BatchSGCMessageProcessor : BaseMessageProcessor
	{
		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			List<ApplicationTypeMessageProcessor> result = base.GetMessageProcessors();
			result.Add(new SGInboundEDIMessageProcessor(Logger, ApplicationCodeList.Codes.SGCustomsTradenet4));
			result.Add(new SGInboundXMLMessageProcessor(Logger, ApplicationCodeList.Codes.SGCustomsTradenetXML));

			return result;
		}
	}
}
