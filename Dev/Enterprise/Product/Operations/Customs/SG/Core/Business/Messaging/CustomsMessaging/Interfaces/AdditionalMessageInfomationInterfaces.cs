using System;
using System.Collections.Generic;

using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business
{
	public delegate void MessageEventHandler(MessageEventArgs srgs);

	public class MessageEventArgs : EventArgs
	{
		public MessageEventArgs(string messageText)
			: base()
		{
			this.MessageText = messageText;
		}

		public string MessageText { get; set; }
	}

	public interface IAdditionalMessageInformation
	{
		ZBool ExtendingTemporaryImportPeriod { get; }

		ZDecimal GSTRefundAmount { get; }
		ZDecimal DutyRefundAmount { get; }
		ZDecimal ExciseRefundAmount { get; }

		ZString Broker { get; }
		ZString CancellationCode { get; }
		ZString ReasonForAmending { get; }
		ZString ReasonForExtendingTemporaryImportPeriod { get; }
		ZString RefundCode { get; }
		ZString ReasonForRefund { get; }
		ZString UpdateIndicator { get; }

		IEnumerable<ICusAttachment> SupportingDocuments { get; }
		event MessageEventHandler OnMessageCreated;
		string MessageCreated(string messageText);
	}
}
