using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Telematics.ServiceTasks.MessageTypeProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Telematics.ServiceTasks
{
	public class EHubMessageProcessor
	{
		public EHubMessageProcessor(ILogger logger, BusinessObjectFactory factory)
			: this(logger,
				factory,
				new ProtobufMessageTypeProcessor(logger, factory),
				new TelematicsXmlMessageTypeProcessor(logger)
			)
		{
		}

		internal EHubMessageProcessor(ILogger logger, BusinessObjectFactory factory, IMessageTypeProcessor protobufMessageTypeProcessor, IMessageTypeProcessor telematicsXmlMessageTypeProcessor)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_ = factory ?? throw new ArgumentNullException(nameof(factory));
			_ = protobufMessageTypeProcessor ?? throw new ArgumentNullException(nameof(protobufMessageTypeProcessor));
			_ = telematicsXmlMessageTypeProcessor ?? throw new ArgumentNullException(nameof(telematicsXmlMessageTypeProcessor));

			messageTypeProcessors = new Dictionary<string, IMessageTypeProcessor>
			{
				[protobufMessageTypeProcessor.MessageType] = protobufMessageTypeProcessor,
				[telematicsXmlMessageTypeProcessor.MessageType] = telematicsXmlMessageTypeProcessor,
			};
			factoryProvider = new BusinessObjectFactoryProvider(factory);
		}

		public static int MaximumRetryCount => 5;

		BusinessObjectFactory Factory => factoryProvider.Current;

		IEnumerable<ZGuid> GetMessagePKs()
		{
			var collection = new DynamicBusinessObjectCollection(Factory);
			var messageSubTypes = messageTypeProcessors
				.Keys
				.Select(s => (code: s, parameterName: $"@MessageSubType{s}"))
				.ToList();

			var messageSubTypeParams = string.Join(", ", messageSubTypes.Select(tuple => tuple.parameterName));

			collection.Load($@"
SELECT
	{EDIMessageSchema.Constants.PK}
FROM
	dbo.EDIMessage WITH (INDEX = NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc)
WHERE 1=1
	AND {EDIMessageSchema.Constants.EM_ApplicationCode} = @ApplicationCode
	AND {EDIMessageSchema.Constants.EM_MessageSubType} IN ({messageSubTypeParams})
	AND {EDIMessageSchema.Constants.EM_ReceiveTransmit} = @ReceiveTransmit
	AND {EDIMessageSchema.Constants.EM_Status} = 'QUE'
OPTION (MAXDOP 1)

"
				, queryParameters: new[]
					{
						ZSqlParameter.New("@ApplicationCode", ApplicationCodeList.Codes.Telematics, EDIMessageSchema.EM_ApplicationCode),
						ZSqlParameter.New("@ReceiveTransmit", ReceiveTransmitList.Codes.Receive, EDIMessageSchema.EM_ReceiveTransmit),
					}
					.Concat(messageSubTypes.Select(tuple => ZSqlParameter.New(tuple.parameterName, tuple.code, EDIMessageSchema.EM_MessageSubType)))
					.ToArray());

			return collection.Select(item => (ZGuid)item[EDIMessageSchema.Constants.PK]);
		}

		public void Run(CancellationToken token)
		{
			var counters =
			(
				eHubMessagesProcessed: 0,
				eHubMessagesFailed: 0,
				internalMessagesProcessed: messageTypeProcessors.Keys.ToDictionary(s => s, s => 0)
			);

			foreach (var messagePK in GetMessagePKs())
			{
				token.ThrowIfCancellationRequested();
				var message = Factory.Load<EDIMessage>(messagePK);

				try
				{
					using (DisposableEnvironment.ForBranch(message.EM_GB.ToGuid()))
					using (var textReader = message.GetEM_MessageTextReader())
					{
						var messageText = textReader.ReadToEnd();

						token.ThrowIfCancellationRequested();

						counters.internalMessagesProcessed[message.EM_MessageSubType] += messageTypeProcessors[message.EM_MessageSubType].Process(Factory, message.Interchange.EI_From, messageText);

						message.EM_Status = EDIMessageStatusList.Codes.Received;
						message.Interchange.EI_Status = EDIInterchangeStatusList.Codes.Received;
					}

					counters.eHubMessagesProcessed++;
					factoryProvider.SaveCurrentReclaimMemoryAndCreateNew();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					// Reload in new factory and mark message as failed
					factoryProvider.CreateNewAndReclaimMemoryWithoutSave();
					var failedMessage = Factory?.Load<EDIMessage>(messagePK);
					if (failedMessage != null)
					{
						failedMessage.Interchange.EI_RetryCount++;
						if (failedMessage.Interchange.EI_RetryCount >= MaximumRetryCount)
						{
							failedMessage.EM_Status = EDIMessageStatusList.Codes.Failed;
							failedMessage.Interchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
						}
					}
					else
					{
						logger.Log(LogType.Error, FormattableString.Invariant($"Can't reload eHub message [{messagePK}]."));
					}

					factoryProvider.SaveCurrentReclaimMemoryAndCreateNew();

					ErrorReporter.ReportOnce("Telematics processing errors", ex);
					counters.eHubMessagesFailed++;
				}
			}

			if (counters.eHubMessagesFailed > 0)
			{
				logger.Log(LogType.Warning, FormattableString.Invariant($"Processed {counters.eHubMessagesProcessed} eHub messages ({counters.eHubMessagesFailed} messages failed)."));
			}
			else if (counters.eHubMessagesProcessed > 0)
			{
				logger.Log(LogType.Information, FormattableString.Invariant($"Processed {counters.eHubMessagesProcessed} eHub messages containing: {string.Join(", ", counters.internalMessagesProcessed.Select(pair => FormattableString.Invariant($"{pair.Key} - {pair.Value} message(s)")))}."));
			}
		}

		readonly BusinessObjectFactoryProvider factoryProvider;
		readonly ILogger logger;
		internal readonly Dictionary<string, IMessageTypeProcessor> messageTypeProcessors;
	}
}
