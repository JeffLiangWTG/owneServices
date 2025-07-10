using System.IO;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.ILReferenceData.Services;
using CargoWise.xTMessaging.Integration;
using CargoWise.xTMessaging.Shared;
using SharedConstants = CargoWise.xTMessaging.Integration.Constants;

namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public class BaseReceiveHandler : BasicReceiveHandler, IPostReceiveHandlerSupport
	{
		public BaseReceiveHandler(ILogger logger) : base(logger)
		{
		}

		protected override (bool Success, string ErrorMessage) HandleMessage(MetaDataHelper metaDataHelper, Stream payload, long xTInternalMsgID)
		{
			var messageSubType = metaDataHelper.MetaData[SharedConstants.CustomMsgAttributes.MessageSubType];

			switch (messageSubType)
			{
				case Constants.MessageSubType.CustomCodes:
					logger.Log(LogType.Information, $"Processor:{nameof(SYSTBL_NG_9001_MSG_SystemTablesResponseReceiverHandlerBase)}");
					var processResult = new SYSTBL_NG_9001_MSG_SystemTablesResponseReceiverHandlerBase(new DateTimeProvider(), logger).ProcessMessage(metaDataHelper, payload, xTInternalMsgID);
					logger.Log(LogType.Information, $"Processing result:{processResult.Success}-{processResult.ErrorMessage}");
					return processResult;
				default:
					return (false, $"Unknown message subtype: {messageSubType}");
			}
		}

		public void PostReceiveHandler()
		{
			TradeGroupsProcessor.GenerateFiles(DateTimeUtil.GetNow, logger);
		}
	}
}
