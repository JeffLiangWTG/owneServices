using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.Processor;

namespace Enterprise.Customs.US.Business
{
	internal class UCMPMessageProcessorFactory : CommonUCMPMessageProcessorFactory
	{
		public static IKeysForBlockingParallelProcessingProvider GetKeysForBlockingParallelProcessingProvider(string messageType)
		{
			return messageType switch
			{
				ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQueryResponse => new ACEEntrySummaryQueryProcessor(),
				ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse => new ACEEntrySummaryQueryProcessor(),
				ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification => new NotificationOfStatusProcessor(),
				ACEApplicationIdentifierCodeList.Codes.DailyStatement => new ACEDailyStatementProcessor(),
				ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus => new SimplifiedEntryStatusNotificationProcessor(),
				ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification => new EntrySummaryStatusNotificationProcessor(),
				ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement => new PeriodicMonthlyStatementProcessor(),
				ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation => new CourtesyNoticeProcessor(),
				ApplicationIdentifierCodeList.Codes.TemporaryImportationBondDuetoExpire => new TemporaryImportBondExpiryNotificationProcessor(),
				_ => null
			};
		}
	}
}
