using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.MessageContext;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.ReplyMessageBuilder;

namespace CargoWise.eHub.Share.eHubServices.eHubSender.ReplyMessageBuilder
{
	public class USDISReplyMessageBuilder : CargoWise.eHub.Share.eHubServices.eHubSender.Common.ReplyMessageBuilder.ReplyMessageBuilder
	{
		readonly ServiceReply reply;

		public USDISReplyMessageBuilder(CargoWise.eHub.Share.eHubServices.eHubSender.Common.MessageContext.MessageContext context, ServiceReply reply)
			: base(context)
		{
			if (reply == null) throw new ArgumentNullException("reply");
			this.reply = reply;
		}

		public override string GetReply()
		{
			if (reply.ReplyDescription.Contains("<ReturnCode>SUCCESS</ReturnCode>")) return null;

			var error = new StringBuilder("USDIS submission errors. ");

			var document = XElement.Parse(reply.ReplyDescription);

			var errorDetailsList = document.Elements().FirstOrDefault(e => e.Name.LocalName == "ErrorDetailsList");
			if (errorDetailsList == null) return error.ToString();

			foreach (var errorDetails in errorDetailsList.Elements())
			{
				var errorCode = errorDetails.Elements().FirstOrDefault(e => e.Name.LocalName == "ErrorCode");
				var errorDescription = errorDetails.Elements().FirstOrDefault(e => e.Name.LocalName == "ErrorDescription");

				if (errorCode != null && !string.IsNullOrEmpty(errorCode.Value))
				{
					error.Append("Error code ").Append(errorCode.Value).Append(" - ");
				}

				if (errorDescription != null && !string.IsNullOrEmpty(errorDescription.Value))
				{
					error.Append(errorDescription.Value).Append(". ");
				}
			}

			return error.ToString();
		}
	}
}