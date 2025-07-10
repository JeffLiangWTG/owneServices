using System;
using System.Globalization;
using System.IO;
using System.ServiceModel;
using System.Text;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.RefDbRepo.USReferenceData.Services;
using static CargoWise.RefDbRepo.USReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.USReferenceData.Business.USIncomingMessageProcessor
{
	public static class USIncomingMessageQuery
	{
		public static ActionResult SendMessages(IeHubAdapter adapter, string identifier)
		{
			var messageType = ApplicationConfig.Instance.USIncomingMessageQueryMessageType;
			try
			{
				using (var stream = new MemoryStream(Encoding.Default.GetBytes(GenerateMessageContent(DateTime.UtcNow, messageType, ApplicationConfig.Instance.USIncomingMessageQueryFilerCode, ApplicationConfig.Instance.USIncomingMessageQueryPortCode, identifier))))
				using (var message = new eHubMessage(Guid.NewGuid(), ApplicationConfig.Instance.USIncomingMessageEHubClientID, ApplicationConfig.Instance.USIncomingMessageQueryMessageRecipientID, MessageSchemaType.Xml, ApplicationConfig.Instance.USIncomingMessageQueryMessageApplicationCode, messageType, stream))
				{
					adapter.Outbox.AddMessage(message);
					adapter.SendMessages();
				}
				return ActionResult.Successful;
			}
			catch (EndpointNotFoundException)
			{
				return ActionResult.Failed;
			}
		}

		public static string GenerateMessageContent(DateTime dateTime, string messageType, string filerCode, string portCode, string identifier)
		{
			var beginDate = GetBeginDate(identifier);
			return string.Format(CultureInfo.InvariantCulture, "<USCustoms><Header><![CDATA[A{3}{2}      {0:MMddyy}     {1:-2}]]></Header>" +
				"<Body><![CDATA[B  {3}{2}{1:-2}                                               USCREPO_{0:yyyyMMddHHmm} " +
				"{4}                                         {5}                             " +
				"Y  {3}{2}{1:-2}                                                                    ]]></Body>" +
				"<Footer><![CDATA[Z{3}{2}      {0:MMddyy}]]></Footer></USCustoms>"
				, dateTime, messageType, filerCode, portCode, identifier, beginDate);
		}

		static string GetBeginDate(string identifier)
		{
			var result = string.Empty;
			if (identifier == USIncomingMessageRequest.F111)
			{
				var lastUpdateDate = new LocalFileStorage(FirmsCode.LastUpdateDate).LoadAsDateTime(MMDDYY);
				result = lastUpdateDate == null ? "010100" : lastUpdateDate.Value.AddDays(-1).ToString(MMDDYY, CultureInfo.InvariantCulture);
			}
			return result.PadLeft(6, ' ');
		}
	}
}
