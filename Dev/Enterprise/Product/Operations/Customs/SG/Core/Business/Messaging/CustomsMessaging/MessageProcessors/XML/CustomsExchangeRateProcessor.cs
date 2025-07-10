using System;
using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	class CustomsExchangeRateProcessor : BaseResponseSectionProcessor<CustomsExchangeRate>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1057:DoNotHardcodeMailserverName", Justification = "This is a code value")]
		public const string MessageType = "XCH";
		public const string MessageName = "Exchange Rates message";

		public CustomsExchangeRateProcessor(LoggingInformation logger, CustomsExchangeRate customsExchangeRate)
			: base(logger, customsExchangeRate, MessageType, MessageName)
		{
			Argument.NotNull(customsExchangeRate, nameof(customsExchangeRate));
		}

		protected override string DoProcessingReturningStatusCore(EDIMessage incomingMessage)
		{
			Logger.Log("Processing CUSEXR Message");

			var result = EDIMessage.Status.Failed;
			try
			{
				RefDataRepoSender.SendIfNeeded(incomingMessage, "IFTRIN", RefDataRepoSender.ContentType.Xml);
				result = EDIMessage.Status.Received;
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ErrorReporter.ReportOnce("CustomsExchangeRateProcessor.DoProcessingReturningStatus", "IFTRIN Message Processing Failure. Incoming message PK: " + this.incomingMessage.PK, e);
			}

			return result;
		}

		protected override string URN => string.Empty;

		protected override string GetCommonAccessReference() => CommonAccessReferenceCodeList.Codes.CUSEXR;

		protected override void SetEntryStatus()
		{
			Logger.Log("CustomsExchangeRate has no related CusEntryHeader");
		}
	}
}
