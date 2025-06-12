using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CargoWise.eHub.Portal.Models.Extensions;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Models.View
{
	public class MessageTypeProviderView : ClientProviderView
	{
		public Guid MessageTypeId { get; set; }
		public string MessageTypeName { get; set; }

		public MessageTypeProviderView(IeHubTransactionsContext context, Guid clientId, Guid messageTypeId)
			: base(context, clientId)
		{
			MessageTypeId = messageTypeId;
			var messageType = context.GetMessageType(messageTypeId);
			MessageTypeName = messageType.DT_Code;
		}
	}
}