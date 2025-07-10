using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Scheduler.GraphEngine;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	sealed class KeyGenProcessingManager : ProcessingManager
	{
		public KeyGenProcessingManager() : base(UniversalCustomsMessagingConstants.ServiceTaskCodes.KeyGen) { }

		protected override BusinessObjectFactory GetNewFactory() => new ReadOnlyBusinessObjectFactory();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message string")]
		protected override void ProcessMessage(EDIMessage message)
		{
			Log(string.Format(CultureInfo.InvariantCulture, "Calculating Keys for Message ({0})", GetMessageLoggingDetail(message)), LogType.Information);
			MessageProcessor.ProcessMessage(message);
			if (lastProcessingTime == null || lastProcessingTime.Value < ZDateTime.UtcNow.AddMinutes(-3))
			{
				lastProcessingTime = ZDateTime.UtcNow;
				MessageProcessor.GrEngine.NudgeMaster();
			}
		}
		ZDateTime? lastProcessingTime;

		protected override void ProcessBatchCleanUp()
		{
			base.ProcessBatchCleanUp();
			if (lastProcessingTime.HasValue)
			{
				lastProcessingTime = null;
				MessageProcessor.GrEngine.NudgeMaster();
			}
		}

		protected override UniversalCustomsApplicationTypeMessageProcessor GetMessageProcessor(string applicationCode)
		{
			IUniversalCustomsMessageProcessor messageProcessor = null;
			if (ShouldUseUCMP(applicationCode))
			{
				messageProcessor = UniversalCustomsMessagingSubscribers.GetMessageProcessor(applicationCode);
			}
			return messageProcessor == null ? null : new KeyGenUniversalCustomsApplicationTypeMessageProcessor(Logger, applicationCode, messageProcessor);
		}

		bool ShouldUseUCMP(string applicationCode)
		{
			// Add to this switch if we want to support a switch back option
			// The rule is if a EDIMessageQueueState is created then the EDIMessage is no longer processed by old service task.

			switch (applicationCode)
			{
				case EDIMessage.ApplicationCodes.SouthAfricanCustoms:
					return ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now);
				case EDIMessage.ApplicationCodes.PLCustoms:
					return ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.Poland, ZDateTime.Now);
				case EDIMessage.ApplicationCodes.USCustomsImport:
					return ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Now);
				case EDIMessage.ApplicationCodes.AMS:
				case EDIMessage.ApplicationCodes.StowPlan:
					return ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.UCMPServiceTaskAMS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Now);
				case EDIMessage.ApplicationCodes.TaiwanCustoms:
					return ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.Taiwan, ZDateTime.Now);
				default:
					return true;
			}
		}

		new KeyGenUniversalCustomsApplicationTypeMessageProcessor MessageProcessor => (KeyGenUniversalCustomsApplicationTypeMessageProcessor)base.MessageProcessor;
		protected override int MessagesPerExecution => CustomsDataRegistry.Instance.UCKMessagesPerExecution.Value;

		protected override bool ShouldSaveMessage(bool shouldMessageBeProcessedInASeparateFactory, EDIMessage message, bool isLastMessage) => false;

		protected override IDisposable GetMessageToProcess(bool shouldMessageBeProcessedInASeparateFactory, EDIMessage message, out EDIMessage messageToProcess)
		{
			messageToProcess = message;
			return null;
		}

		protected override BaseMessageProcessor<EDIMessage>.DisposableBatch DequeueMessages(ZQuery query)
		{
			var processor = MessageProcessor;
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCode);
			return CreatePreKeyBatch(processor.GrEngine, BaseMessageProcessor.GetNewFactory(), query);
		}

		BaseMessageProcessor<EDIMessage>.DisposableBatch CreatePreKeyBatch(EDIMessageGrEngine grEngine, BusinessObjectFactory factory, ZQuery query)
		{
			void LogLoadedItems(EDIMessage[] messages)
			{
				if (messages.Length > 0 && grEngine.Setup.LogOptions.AllKeygenLoads)
				{
					var applicationCode = messages[0].EM_ApplicationCode;
					Logger.Log(FormattableString.Invariant($"{applicationCode} - Messages loaded ({messages.Length}): {string.Join(", ", messages.Select(m => m.EM_MessageNum))}"));
				}
			}

			void LogLockTaken(string lockStr)
			{
				if (grEngine.Setup.LogOptions.AllKeygenLocks)
				{
					Logger.Log(FormattableString.Invariant($"Lock taken: {lockStr}"));
				}
			}

			var lockLogger = new AppLockLogger<EDIMessage>(LogLockTaken, LogLoadedItems);
			var result = grEngine.PreEnqueuer.LoadPreKeyBatch(factory, nameof(CreatePreKeyBatch), query, GetCheckFilter(), grEngine.BatchSize, lockLogger);
			return new BaseMessageProcessor<EDIMessage>.DisposableBatch(result.Values.ToArray(), new NullNotifiedDisposable(result));
		}

		ZQuery GetCheckFilter()
		{
			var prequeuedQuery = new ZDBOnlyQuery(typeof(EDIMessage));
			prequeuedQuery.AddFilterAndZSQLParameterCollection(
				FormattableString.Invariant($"EM_PK NOT IN (SELECT EQS_EM FROM dbo.EDIMessageQueueState WITH (INDEX(NR_RX__EQS_EM), FORCESEEK) WHERE EQS_ApplicationCode = {EDIMessageQueueStateFactory.ApplicationCodeParam} AND EQS_Status IN ('{QueueStatusCodes.Codes.PreKey}', '{QueueStatusCodes.Codes.Queued}', '{QueueStatusCodes.Codes.Blocked}'))"),
				new ZSqlParameterCollection(ZSqlParameter.New(EDIMessageQueueStateFactory.ApplicationCodeParam, ApplicationCode, EDIMessageQueueStateSchema.EQS_ApplicationCode))
			);
			return prequeuedQuery;
		}
	}
}
