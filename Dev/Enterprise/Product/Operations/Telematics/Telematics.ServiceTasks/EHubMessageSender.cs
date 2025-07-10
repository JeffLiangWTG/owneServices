using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Telematics.ServiceTasks
{
	public class EHubMessageSender : IEHubMessageSender
	{
		public void Send(BusinessObjectFactory factory, IEnumerable<string> messages, IEnumerable<string> recipients)
		{
			_ = factory ?? throw new ArgumentNullException(nameof(factory));
			_ = messages ?? throw new ArgumentNullException(nameof(messages));
			_ = recipients ?? throw new ArgumentNullException(nameof(recipients));

			var messageList = messages.ToList();
			if (messageList.Count == 0)
			{
				return;
			}

			foreach (var recipient in recipients)
			{
				var interchange = factory.New<EDIInterchange>();
				interchange.EI_To = recipient;
				interchange.EI_From = Env.CurrentCompany.GetLicenceCode();
				interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.Telematics;
				interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
				interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;

				foreach (var message in messageList)
				{
					var ediMessage = interchange.ContainedMessages.AddNew();
					ediMessage.EM_ApplicationCode = ApplicationCodeList.Codes.Telematics;
					ediMessage.EM_MessageSubType = TelematicsMessageList.Codes.TelematicsXmlData;
					ediMessage.EM_MessageText = message;
					ediMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
					ediMessage.EM_Status = EDIInterchangeStatusList.Codes.eHubQueued;
					ediMessage.MessageNumberStrategy = new TelematicsMessageNumberStrategy(factory);
				}
			}
		}
	}
}
