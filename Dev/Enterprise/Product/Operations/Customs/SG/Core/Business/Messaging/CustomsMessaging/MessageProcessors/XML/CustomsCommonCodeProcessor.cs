using System;
using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	class CustomsCommonCodeProcessor : BaseResponseSectionProcessor<CustomsCommonCode>
	{
		public const string MessageType = "REF";
		public const string MessageName = "Reference Data message";

		public CustomsCommonCodeProcessor(LoggingInformation logger, CustomsCommonCode customsCommonCode)
			: base(logger, customsCommonCode, MessageType, MessageName)
		{
			Argument.NotNull(customsCommonCode, nameof(customsCommonCode));
		}

		protected override string DoProcessingReturningStatusCore(EDIMessage incomingMessage)
		{
			Logger.Log("Processing CUSCOM Message");

			var result = EDIMessage.Status.Failed;
			try
			{
				RefDataRepoSender.SendIfNeeded(incomingMessage, "CLASET", RefDataRepoSender.ContentType.Xml);
				result = EDIMessage.Status.Received;
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ErrorReporter.ReportOnce("CustomsCommonCodeProcessor.DoProcessingReturningStatus", "CLASET Message Processing Failure. Incoming message PK: " + this.incomingMessage.PK, e);
			}

			return result;
		}

		protected override string URN => string.Empty;

		protected override string GetCommonAccessReference() => CommonAccessReferenceCodeList.Codes.CUSCOM;

		protected override void SetEntryStatus()
		{
			Logger.Log("CustomsCommonCode is not related to any CusEntryHeader - should not be accessing this method.");
		}
	}
}
