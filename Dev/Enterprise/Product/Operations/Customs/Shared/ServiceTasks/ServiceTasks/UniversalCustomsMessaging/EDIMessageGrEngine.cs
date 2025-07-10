using System;
using CargoWise.Data.Utils;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Messaging.Business;
using Enterprise.Scheduler.GraphEngine;
using GrEngineLogOptions = Enterprise.Scheduler.GraphEngine.GrEngineLogOptions;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public class EDIMessageGrEngine : GraphEngine.ServiceTasks.EDIMessageGrEngine<EDIMessage, EDIMessageQueueState>
	{
		public EDIMessageGrEngine(LoggingInformation logger, string applicationCode, GraphEngine.ServiceTasks.GrEngineServiceSetting setting, ISqlApplicationLockProvider lockProvider = null, int maxConcurrentHandles = 1)
			: base(new EDIMessageQueueStateFactory(logger, applicationCode, GetLogOptions, maxConcurrentHandles), setting, GetBatchSize, GetReleaseBatchSize, GetCapacity, GetPreKeyBacklogSize, CreateNewPreEnqueuer, lockProvider)
		{
		}

		static GraphEngine.ServiceTasks.EDIMessageGraphPreEnqueuer<EDIMessage, EDIMessageQueueState> CreateNewPreEnqueuer(GrEngineServiceSetup<EDIMessageQueueState> setup) => new GraphEngine.ServiceTasks.EDIMessageGraphPreEnqueuer<EDIMessage, EDIMessageQueueState>(setup, GetCriticalDuration, GetResetDuration);
		static int GetBatchSize() => CustomsDataRegistry.Instance.UCKMessagesPerBatch.Value;
		static int GetReleaseBatchSize() => CustomsDataRegistry.Instance.UCIMessagesPerBatch.Value;
		static int GetCapacity() => CustomsDataRegistry.Instance.UCMMessageQueueCapacity.Value;
		static int GetPreKeyBacklogSize() => CustomsDataRegistry.Instance.UCKPreEnqueuerMaxBacklogSize.Value;
		static TimeSpan GetCriticalDuration() => TimeSpan.FromMilliseconds(CustomsDataRegistry.Instance.UCKPreEnqueuerIncreasePerformanceLevelDurationInMillis.Value);
		static TimeSpan GetResetDuration() => TimeSpan.FromMilliseconds(CustomsDataRegistry.Instance.UCKPreEnqueuerResetPerformanceLevelDurationInMillis.Value);
		static GrEngineLogOptions GetLogOptions()
		{
			var registry = CustomsDataRegistry.Instance.UCMExtendedUniversalLogging.Value;
			return new GrEngineLogOptions
			{
				FirstEnqueueLoad = registry.GetBoolFromCode(CustomsDataRegistry.ExtendedUniversalLoggingKeys.UCIFirstMessageLoad),
				MessageAtFront = registry.GetBoolFromCode(CustomsDataRegistry.ExtendedUniversalLoggingKeys.UCIMessageAtFront),
				ChainStatistics = registry.GetBoolFromCode(CustomsDataRegistry.ExtendedUniversalLoggingKeys.UCIChainStatistics),
				OldestMessage = registry.GetBoolFromCode(CustomsDataRegistry.ExtendedUniversalLoggingKeys.UCIShowOldest),
				EnqueueLoads = registry.GetBoolFromCode(CustomsDataRegistry.ExtendedUniversalLoggingKeys.UCIAllLoads),
				AllKeygenLoads = registry.GetBoolFromCode(CustomsDataRegistry.ExtendedUniversalLoggingKeys.UCKAllLoads),
				AllKeygenLocks = registry.GetBoolFromCode(CustomsDataRegistry.ExtendedUniversalLoggingKeys.UCKLocksTaken),
				AllWorkerLoads = registry.GetBoolFromCode(CustomsDataRegistry.ExtendedUniversalLoggingKeys.UCQAllLoads),
				AllWorkerLocks = registry.GetBoolFromCode(CustomsDataRegistry.ExtendedUniversalLoggingKeys.UCQLocksTaken),
				SetChainID = CustomsDataRegistry.Instance.UCMSetChainIDLogging.Value,
			};
		}

		protected override string WorkerServiceTaskCode => UniversalCustomsMessagingConstants.ServiceTaskCodes.Worker;
		protected override string MasterServiceTaskCode => UniversalCustomsMessagingConstants.ServiceTaskCodes.Master;
		protected override string KeyGenServiceTaskCode => UniversalCustomsMessagingConstants.ServiceTaskCodes.KeyGen;
	}
}
