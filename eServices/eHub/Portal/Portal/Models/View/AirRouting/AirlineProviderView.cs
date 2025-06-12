using System;
using CargoWise.eHub.Portal.Models.Extensions;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Models.View
{
	public class AirlineProviderView : MessageTypeProviderView
	{
		public Guid AirlineId { get; set; }
		public string AirlineName { get; set; }
		public string RecipientAddress { get; set; }
		public string ClientPIMA { get; set; }
		public string MessagePriority { get; set; }
		public string DoubleSignatureCode { get; set; }

		public string ShipmentOrigin { get; set; }

		public string MessageType { get; set; }


		public AirlineProviderView(IeHubTransactionsContext context, Guid clientId, Guid messageTypeId, Guid airlineId, string recipientAddress, string clientPIMA, string messagePriority, string doubleSignatureCode, string shipmentOrigin)
			: base(context, clientId, messageTypeId)
		{
			AirlineId = airlineId;
			var airLine = context.GetClient(airlineId);
			AirlineName = airLine.CC_ID;
			RecipientAddress = recipientAddress;
			ClientPIMA = clientPIMA;
			MessagePriority = messagePriority;
			DoubleSignatureCode = doubleSignatureCode;
			ShipmentOrigin = shipmentOrigin;
			MessageType = context.GetMessageType(messageTypeId)?.DT_Code;
		}
	}
}