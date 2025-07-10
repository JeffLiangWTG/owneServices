using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	public abstract class RerouteProcessor<TQRBlock, TQXBlock> : ACSABIProcessor
		where TQRBlock : MessageBlock, IStatementReroute
		where TQXBlock : MessageBlock, IStatementRerouteResponse
	{
		protected RerouteProcessor()
		{
		}

		public override void Process()
		{
			var qrs = new List<TQRBlock>();
			var qxs = new List<TQXBlock>();
			foreach (MessageBlock block in messageBlocks)
			{
				var dstqr = block as TQRBlock;
				if (dstqr != null)
				{
					if (dstqr.PreliminaryPeriodicMonthlyStatementRequest == "Y" || dstqr.FinalPeriodicMonthlyStatementRequest == "Y")
					{
						isPeriodicMonthly = true;
					}

					qrs.Add(dstqr);
				}
				else
				{
					var dstqx = block as TQXBlock;
					if (dstqx != null)
					{
						qxs.Add(dstqx);
					}
				}
			}

			GenerateEmailForResponse(qrs, qxs);
		}

		protected ZBool IsPeriodicMonthly
		{
			get { return isPeriodicMonthly; }
		}
		ZBool isPeriodicMonthly;

		void GenerateEmailForResponse(List<TQRBlock> qrs, List<TQXBlock> qxs)
		{
			EmailDef email;
			GenerateHtmlEmail(RerouteSubject,
				RerouteSubject,
				HtmlResponseEmailGenerator.DefaultResponseDescription,
				GetBodyDetail(qrs, qxs), "", out email, null);
			SendEmailToOriginalSenderOrGroupIfSenderInvalid(email, false, null, false);
		}
		protected abstract string RerouteSubject { get; }

		string GetBodyDetail(List<TQRBlock> qrs, List<TQXBlock> qxs)
		{
			return GetSendMessageData(qrs) + "<br />" + GetResponseMessageData(qxs);
		}

		string GetResponseMessageData(List<TQXBlock> qxs)
		{
			var creator = new HtmlTableCreator(new string[] { "Error/Notification", "Message", "Total No. Of Reroutes" });
			foreach (TQXBlock qx in qxs)
			{
				var code = qx.ErrorCode;
				var description = GetErrorDescription(code);

				var notification = code.IsEmpty ? "" : code + (!description.IsEmpty ? " - " + description : "");
				creator.WriteRow(new object[] { notification, qx.MessageText, qx.TotalNumberOfReroutes });
			}
			return "Response Data:<br />" + creator.ToHtml();
		}

		protected virtual ZString GetErrorDescription(ZString code)
		{
			var result = ZString.Empty;
			if (!code.IsEmpty)
			{
				result = MessageCalculator.GetLongDescription(code, ZString.Empty).TrimStart();
			}

			return result;
		}

		string GetSendMessageData(List<TQRBlock> qrs)
		{
			var creator = new HtmlTableCreator(SendMessageDataHeading);
			foreach (TQRBlock qr in qrs)
			{
				creator.WriteRow(WriteQRBlock(qr));
			}
			return "Requested Data:<br />" + creator.ToHtml();
		}

		protected virtual IReadOnlyList<string> SendMessageDataHeading
		{
			get { return new string[] { "Transmission Date", "Importer No.", "Client Branch", "Statement No.", "All", "Preliminary", "Final" }; }
		}

		protected virtual object[] WriteQRBlock(TQRBlock qr)
		{
			return new object[] {
					qr.TransmissionDate,
					qr.ImporterOfRecordNumber,
					qr.ClientBranch,
					qr.StatementNumber,
					qr.ScopeIndicator == "A" ? "Y" : "N",
					!qr.PreliminaryDailyStatementRequest.IsEmpty ? qr.PreliminaryDailyStatementRequest : qr.PreliminaryPeriodicMonthlyStatementRequest,
					!qr.FinalDailyStatementRequest.IsEmpty ? qr.FinalDailyStatementRequest : qr.FinalPeriodicMonthlyStatementRequest };
		}
	}
}
