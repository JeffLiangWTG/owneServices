using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.GraphEngine.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.Scheduler.GraphEngine;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	class ProcessingManagerTest : TestCaseWithFactory
	{
		[UseSnapshotProtection]
		public void TestMessageReleasingManagerApplicationLock()
		{
			SqlApplicationLock sqlAppLock = null;
			var connection = Db.Connection;
			using (RunNonTransactioned())
			using (new DisposableAction(() =>
				{
					AssertEquals("Lock ", true, connection.TryGetLock(ApplicationCodeForTesting + nameof(MessageReleasingManager), out sqlAppLock));
				}, () =>
				{
					sqlAppLock?.Dispose();
				}))
			{
				var nzBranch = SetupBranch(Factory, Core.Constants.CountryCodes.NewZealand);
				var dec = Factory.New<BaseJobDeclaration>();
				var messageProcessor = new UniversalCustomsMessageProcessorTestClass();
				messageProcessor.GetLinkedBusinessObjectMetaDataForTesting = (_, _) => new LinkedBusinessObjectMetaData(dec.TableName, dec.PK, dec.JE_GB, dec.JE_DeclarationReference);
				messageProcessor.GetBranchForTesting = (_, _, _) => nzBranch.PK;
				messageProcessor.GetSerializationKeysResultForTesting = (_, _, _) => new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { "DEF2" });

				var message = CreateMessage();
				Factory.Save();

				using (new UCMPProcessorsRegistrationSubstitute((ApplicationCodeForTesting, messageProcessor)))
				using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
				{
					new KeyGenProcessingManager().ExecuteBatch(CancellationToken.None);
					message.Reload();
					AssertEquals("message.EM_Status", EDIMessage.Status.PreProcessedOK, message.EM_Status);
					var task = Task.Factory.StartNew(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							var messageReleasingManager = new MessageReleasingManager();
							messageReleasingManager.ExecuteBatch(CancellationToken.None);
							AssertContains(messageReleasingManager.Logger.DebugLogStrings, ($"Another task is currently processing messages for {ApplicationCodeForTesting}.", 1));
						}
					});
					task.Wait();
					message.Reload();
					AssertEquals("message.EM_Status", EDIMessage.Status.PreProcessedOK, message.EM_Status);
				}
			}
		}

		public void TestSettingEM_LinkObjectInProcessMessage()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			Factory.Save();
			var outgoingMessage = CreateMessage(Factory, messageNum: "MNO1234", messageOwner: declaration.JE_DeclarationReference);
			outgoingMessage.EM_LinkedObject = declaration;
			Factory.Save();

			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			var incomingMessage = CreateMessage(Factory, messageNum: "MNI1234", messageOwner: declaration.JE_DeclarationReference);
			Factory.Save();

			var messageProcessor = new UniversalCustomsMessageProcessorTestClass();
			messageProcessor.GetLinkedBusinessObjectMetaDataForTesting = (message, _) =>
			{
				if (message.EM_MessageSubType == "PRE")
				{
					var dec = message.Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, message.EM_MessageOwner));
					return new LinkedBusinessObjectMetaData(dec.TableName, dec.PK, dec.JE_GB, dec.JobNumber);
				}
				return LinkedBusinessObjectMetaData.Empty;
			};

			messageProcessor.GetBranchForTesting = (message, _, linkedBusinessObjectBranchPk) =>
			{
				if (message.EM_MessageSubType == "PRE")
				{
					return linkedBusinessObjectBranchPk;
				}
				return message.EM_GB;
			};

			messageProcessor.GetSerializationKeysResultForTesting = (message, _, _) => new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { message.EM_MessageOwner });

			messageProcessor.ProcessMessageForTesting = ProcessMessageForTesting;

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			using (new UCMPProcessorsRegistrationSubstitute((ApplicationCodeForTesting, messageProcessor)))
			{
				CombineAssertions("Report when set in ProcessMessage", () =>
				{
					new KeyGenProcessingManager().ExecuteBatch(CancellationToken.None);
					incomingMessage.Reload();
					AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.PreProcessedOK, incomingMessage.EM_Status);
					new MessageReleasingManager().ExecuteBatch(CancellationToken.None);

					new WorkerProcessingManager().ExecuteBatch(CancellationToken.None);
					incomingMessage.Reload();
					AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.Received, incomingMessage.EM_Status);
					AssertEquals("ErrorReporter.LastMessageReported", $"{typeof(UniversalCustomsMessageProcessorTestClass).FullName} is setting EM_LinkedObject data in ProcessMessage when it should have done in UCK", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				});

				CombineAssertions("No report if set in UCK", () =>
				{
					incomingMessage = CreateMessage(Factory, messageNum: "MNI1235", messageOwner: declaration.JE_DeclarationReference);
					incomingMessage.EM_MessageSubType = "PRE";
					Factory.Save();

					new KeyGenProcessingManager().ExecuteBatch(CancellationToken.None);
					incomingMessage.Reload();
					AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.PreProcessedOK, incomingMessage.EM_Status);
					new MessageReleasingManager().ExecuteBatch(CancellationToken.None);

					new WorkerProcessingManager().ExecuteBatch(CancellationToken.None);
					incomingMessage.Reload();
					AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.Received, incomingMessage.EM_Status);
					AssertEquals("ErrorReporter.LastMessageReported", string.Empty, ErrorReporter.LastMessageReported);
				});

				CombineAssertions("Suspend report", () =>
				{
					incomingMessage = CreateMessage(Factory, messageNum: "MNI1236", messageOwner: declaration.JE_DeclarationReference);
					incomingMessage.EM_MessageSubType = "SPD";
					Factory.Save();

					new KeyGenProcessingManager().ExecuteBatch(CancellationToken.None);
					incomingMessage.Reload();
					AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.PreProcessedOK, incomingMessage.EM_Status);
					new MessageReleasingManager().ExecuteBatch(CancellationToken.None);

					new WorkerProcessingManager().ExecuteBatch(CancellationToken.None);
					incomingMessage.Reload();
					AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.Received, incomingMessage.EM_Status);
					AssertEquals("ErrorReporter.LastMessageReported", string.Empty, ErrorReporter.LastMessageReported);
				});
			}

			void ProcessMessageForTesting(EDIMessage message, LoggingInformation logger, IUniversalCustomsMessageProcessorHelper helper)
			{
				var dec = message.Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, message.EM_MessageOwner));
				using (message.EM_MessageSubType == "SPD" ? helper.SuspendReportSettingEM_LinkedObject() : null)
				{
					message.EM_LinkedObject = dec;
					message.EM_Status = EDIMessage.Status.Received;
				}
			}
		}

		public void TestMaximumRetryReached()
		{
			var message = CreateMessage();
			message.EM_RetryCount = 11;
			Factory.Save();

			using (new UCMPProcessorsRegistrationSubstitute((ApplicationCodeForTesting, new UniversalCustomsMessageProcessorTestClass())))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				CombineAssertions("Pre", () =>
				{
					new KeyGenProcessingManager().ExecuteBatch(CancellationToken.None);
					new MessageReleasingManager().ExecuteBatch(CancellationToken.None);
					message.Reload();
					AssertEquals("message.EM_Status", EDIMessage.Status.PreProcessedOK, message.EM_Status);
					AssertEquals("message.EM_RetryCount", (ZByte)11, message.EM_RetryCount);
				});

				CombineAssertions("Max reach", () =>
				{
					var manager = new WorkerProcessingManager();
					manager.ExecuteBatch(CancellationToken.None);
					message.Reload();
					AssertEquals("message.EM_Status", EDIMessage.Status.Failed, message.EM_Status);
					AssertEquals("message.EM_RetryCount", (ZByte)11, message.EM_RetryCount);

					AssertContains(manager.Logger.UserLogStrings, ("Failed processing Message (T#1-MT1-MN1234|MSG OWN)", 1));
				});
			}
		}

		public void TestShouldMessageBeProcessedInASeparateFactory_ExceptionHandling()
		{
			var message = CreateMessage();
			Factory.Save();

			var processor = new UniversalCustomsMessageProcessorTestClass();
			processor.ShouldMessageBeProcessedInASeparateFactoryForTesting = (_) => throw new Exception("Some Exception");
			using (new UCMPProcessorsRegistrationSubstitute((ApplicationCodeForTesting, processor)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				AssertEquals(message.EM_Status, EDIMessageStatusList.Codes.Queued);
				new KeyGenProcessingManager().ExecuteBatch(CancellationToken.None);
				message.Reload();
				AssertEquals(message.EM_Status, EDIMessageStatusList.Codes.Failed);
				AssertEquals("Some Exception", ErrorReporter.LastExceptionReported.Message);
				ErrorReporter.Clear();
			}
		}

		public void TestHeldMessageAreNotProcessed()
		{
			var message = CreateMessage();
			message.EM_HeldUntilDate = ZDateTime.UtcNow.AddDays(1);
			Factory.Save();

			using (new UCMPProcessorsRegistrationSubstitute((ApplicationCodeForTesting, new UniversalCustomsMessageProcessorTestClass())))
			{
				CombineAssertions(() =>
				{
					var manager = new KeyGenProcessingManager();
					manager.ExecuteBatch(CancellationToken.None);
					message.Reload();
					AssertEquals("message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);

					AssertEquals("Logs", 0, manager.Logger.UserLogStrings.Count);
				});
			}
		}

		public void TestHandleIncorrectlySetupInUniversalCustomsMessageProcessors()
		{
			using (new UCMPProcessorsRegistrationSubstitute((ApplicationCodeForTesting, new UniversalCustomsMessageProcessorTestClass())))
			{
				var manager = new WorkerProcessingManager();
				AssertNoExceptionThrown(() => manager.ExecuteBatch(CancellationToken.None));
			}
		}

		public void TestFactorySaveAlerter()
		{
			var message = CreateMessage();
			Factory.Save();

			var messageProcessor = new UniversalCustomsMessageProcessorTestClass();
			messageProcessor.ProcessMessageForTesting = (m, l, h) => m.Factory.Save();

			using (new UCMPProcessorsRegistrationSubstitute((ApplicationCodeForTesting, messageProcessor)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				CombineAssertions(() =>
				{
					new KeyGenProcessingManager().ExecuteBatch(CancellationToken.None);
					new MessageReleasingManager().ExecuteBatch(CancellationToken.None);
					message.Reload();
					AssertEquals("message.EM_Status", EDIMessage.Status.PreProcessedOK, message.EM_Status);
					AssertEquals("message.EM_RetryCount", ZByte.Zero, message.EM_RetryCount);

					var manager = new WorkerProcessingManager();
					manager.ExecuteBatch(CancellationToken.None);

					AssertEquals("UCMP is responsible for all Factory Save calls. Factory.Save should not be called by subscribers.", ExceptionReporterTestListener.Instance.GetExceptionMessage(0));
					ExceptionReporterTestListener.Instance.Clear();
				});
			}
		}

		public void TestFactorySaveAlerter_OtherFactoriesSkipped()
		{
			var message = CreateMessage();
			Factory.Save();

			var messageProcessor = new UniversalCustomsMessageProcessorTestClass();
			messageProcessor.ProcessMessageForTesting = (m, l, h) => new BusinessObjectFactory().Save();

			using (new UCMPProcessorsRegistrationSubstitute((ApplicationCodeForTesting, messageProcessor)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				CombineAssertions(() =>
				{
					new KeyGenProcessingManager().ExecuteBatch(CancellationToken.None);
					new MessageReleasingManager().ExecuteBatch(CancellationToken.None);
					message.Reload();
					AssertEquals("message.EM_Status", EDIMessage.Status.PreProcessedOK, message.EM_Status);
					AssertEquals("message.EM_RetryCount", ZByte.Zero, message.EM_RetryCount);

					var manager = new WorkerProcessingManager();
					manager.ExecuteBatch(CancellationToken.None);

					AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
				});
			}
		}

		public void TestHandlingFailureException()
		{
			var message = CreateMessage();
			Factory.Save();

			var messageProcessor = new UniversalCustomsMessageProcessorTestClass();
			messageProcessor.ProcessMessageForTesting = (m, l, h) => throw new InvalidOperationException("Message ABC");
			using (new UCMPProcessorsRegistrationSubstitute((ApplicationCodeForTesting, messageProcessor)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				CombineAssertions("Pre", () =>
				{
					new KeyGenProcessingManager().ExecuteBatch(CancellationToken.None);
					new MessageReleasingManager().ExecuteBatch(CancellationToken.None);
					message.Reload();
					AssertEquals("message.EM_Status", EDIMessage.Status.PreProcessedOK, message.EM_Status);
					AssertEquals("message.EM_RetryCount", ZByte.Zero, message.EM_RetryCount);
				});

				CombineAssertions(() =>
				{
					var manager = new WorkerProcessingManager();
					manager.ExecuteBatch(CancellationToken.None);
					message.Reload();
					AssertEquals("message.EM_Status", EDIMessage.Status.Failed, message.EM_Status);
					AssertEquals("message.EM_RetryCount", (ZByte)1, message.EM_RetryCount);

					AssertContains(manager.Logger.UserLogStrings,
						("Exception processing message (T#1-MT1-MN1234|MSG OWN): [Message ABC]", 1),
						("Error Processing Incoming EDI Message: T#1-MT1-MN1234\r\nException occurred 1 times whilst processing a message individually. The message's status has been set to 'Failed'.\r\nSystem.InvalidOperationException: Message ABC",
							1));

					if (ErrorReporter.LastMessageReported.Contains("Error Processing Incoming EDI Message: T#1-MT1-MN1234"))
					{
						ErrorReporter.Clear();
					}
				});
			}
		}

		public void TestHandlingExceptionWithRetry()
		{
			var message = CreateMessage();
			Factory.Save();

			var messageProcessor = new UniversalCustomsMessageProcessorTestClass();
			messageProcessor.ProcessMessageForTesting = (m, l, h) => throw new TransactionException("Message ABC");
			var retryAttempts = eAdaptorRegistry.Instance.RetryAttemptsOnUniversalXMLProcessingRecoverableErrors.Value;
			using (new UCMPProcessorsRegistrationSubstitute((ApplicationCodeForTesting, messageProcessor)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				CombineAssertions("Pre", () =>
				{
					new KeyGenProcessingManager().ExecuteBatch(CancellationToken.None);
					new MessageReleasingManager().ExecuteBatch(CancellationToken.None);
					message.Reload();
					AssertEquals("message.EM_Status", EDIMessage.Status.PreProcessedOK, message.EM_Status);
					AssertEquals("message.EM_RetryCount", ZByte.Zero, message.EM_RetryCount);
				});

				CombineAssertions("Handle exception", () =>
				{
					var manager = new WorkerProcessingManager();
					manager.ExecuteBatch(CancellationToken.None);
					message.Reload();
					AssertEquals("message.EM_Status", EDIMessage.Status.Failed, message.EM_Status);
					AssertEquals("message.EM_RetryCount", (ZByte)retryAttempts, message.EM_RetryCount);

					AssertContains(manager.Logger.UserLogStrings,
						("Exception processing message (T#1-MT1-MN1234|MSG OWN): [Message ABC]", retryAttempts),
						($"Error Processing Incoming EDI Message: T#1-MT1-MN1234\r\nException occurred {retryAttempts} times whilst processing a message individually. The message's status has been set to 'Failed'.\r\nCargoWise.Data.TransactionException: Message ABC",
							1));
					if (ErrorReporter.LastMessageReported.Contains("Error Processing Incoming EDI Message: T#1-MT1-MN1234"))
					{
						ErrorReporter.Clear();
					}
				});
			}
		}

		public void TestHandlingMissingUniversalCustomsMessageProcessor()
		{
			var message = CreateMessage();
			Factory.Save();

			using (new UCMPProcessorsRegistrationSubstitute((ApplicationCodeForTesting, null)))
			{
				AssertNoExceptionThrown(() => new KeyGenProcessingManager().ExecuteBatch(CancellationToken.None));
				message.Reload();
				AssertEquals("message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("message.EM_RetryCount", ZByte.Zero, message.EM_RetryCount);
			}
		}

		public void TestHandlingOfDiscardMessage()
		{
			var dateTime = ZDateTime.UtcNow.AddHours(-1);
			var message = CreateMessage(Factory, ApplicationCodeForTesting, "MT1", "MN1234A", "MSG1", dateTime);
			Factory.Save();

			var messageProcessor = new UniversalCustomsMessageProcessorTestClass();
			messageProcessor.GetLinkedBusinessObjectMetaDataForTesting = (_, _) => ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, (NoResString)"Bad data");

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			using (new UCMPProcessorsRegistrationSubstitute((ApplicationCodeForTesting, messageProcessor)))
			{
				var grEngine = new EDIMessageGrEngine(new LoggingInformation(), ApplicationCodeForTesting, GrEngineServiceSetting.KeyGen);
				var processingManager = new KeyGenProcessingManager();
				CombineAssertions("KeyGen", () =>
				{
					processingManager.ExecuteBatch(CancellationToken.None);
					message.Reload();
					AssertMessage(message, GlbBranch.CurrentBranch.PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.Discarded);
					var note = message.Notes.FindByDescription("Discard Reason").Single();
					AssertEquals("note.ST_NoteDataAsText", "Bad data", note.ST_NoteDataAsText);
					AssertContains(processingManager.Logger.UserLogStrings.Cast<string>(),
						new[]
						{
							"Calculating Keys for Message",
							"Status set to Discarded",
						}, @$"Calculating Keys for Message (T#1-MT1-MN1234A|MSG1)
Status set to Discarded due to the following reason: Bad data
");

					var queueStates = grEngine.Dequeuer.LoadAll().OrderBy(x => x.ParentSystemCreateTimeUtc).ToArray();
					AssertEquals("queueStates.Length", 0, queueStates.Length);
				});
			}
		}

		public void TestShouldUseUCMP_USCustomsImport_DependsOnRegistry()
		{
			var message = CreateMessage(Factory, EDIMessage.ApplicationCodes.USCustomsImport, "MT1", "MN1234", "MSG OWN");
			Factory.Save();

			var messageProcessors = new Hashtable();
			var messageProcessor = new UniversalCustomsMessageProcessorTestClass();
			messageProcessors.Add(EDIMessage.ApplicationCodes.USCustomsImport, new TestObjectHandle(messageProcessor));
			using (new UCMPProcessorsRegistrationSubstitute(("USI", messageProcessor)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, value: false))
				{
					CombineAssertions(() =>
					{
						var manager = new KeyGenProcessingManager();
						manager.ExecuteBatch(CancellationToken.None);
						message.Reload();
						AssertEquals("message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
					});
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, value: true))
				{
					CombineAssertions(() =>
					{
						var manager = new KeyGenProcessingManager();
						manager.ExecuteBatch(CancellationToken.None);
						message.Reload();
						AssertEquals("message.EM_Status", EDIMessage.Status.PreProcessedOK, message.EM_Status);
					});
				}
			}
		}

		public void TestEndToEnd()
		{
			var nzBranch = SetupBranch(Factory, Core.Constants.CountryCodes.NewZealand);
			var auBranch = SetupBranch(Factory, Core.Constants.CountryCodes.Australia);
			var dec = Factory.New<BaseJobDeclaration>();
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var dateTime = ZDateTime.UtcNow.AddHours(-1);
			var minutes = 0;
			var message1 = CreateMessage(Factory, ApplicationCodeForTesting, "MT1", "MN1234A", "MSG1", dateTime.AddMinutes(++minutes));
			var message2 = CreateMessage(Factory, ApplicationCodeForTesting, "MT2", "MN1234B", "MSG1", dateTime.AddMinutes(++minutes));
			var message3 = CreateMessage(Factory, ApplicationCodeForTesting, "MT1", "MN1234C", "MSG2", dateTime.AddMinutes(++minutes));
			var message4 = CreateMessage(Factory, ApplicationCodeForTesting, "MT1", "MN1234D", "MSG2", dateTime.AddMinutes(++minutes));
			var message5 = CreateMessage(Factory, ApplicationCodeForTesting, "MT1", "MN1234E", "MSG1", dateTime.AddMinutes(++minutes));
			var message8 = CreateMessage(Factory, ApplicationCodeForTesting, "MT1", "MN1234H", "MSG3", dateTime.AddMinutes(++minutes));
			Factory.Save();
			CustomsDataRegistry.Instance.UCKMessagesPerExecution.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			CustomsDataRegistry.Instance.UCQMessagesPerExecution.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			CustomsDataRegistry.Instance.UCIMessagesPerExecution.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			CustomsDataRegistry.Instance.ParallelUCMQueueHistoryInHours.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 50);
			var pairs = CustomsDataRegistry.Instance.UCMExtendedUniversalLogging.Value;
			pairs.Set(CustomsDataRegistry.ExtendedUniversalLoggingKeys.UCIFirstMessageLoad, true);
			pairs.Set(CustomsDataRegistry.ExtendedUniversalLoggingKeys.UCIMessageAtFront, true);
			pairs.Set(CustomsDataRegistry.ExtendedUniversalLoggingKeys.UCIChainStatistics, true);
			pairs.Set(CustomsDataRegistry.ExtendedUniversalLoggingKeys.UCIShowOldest, true);
			pairs.Set(CustomsDataRegistry.ExtendedUniversalLoggingKeys.UCIAllLoads, true);
			pairs.Set(CustomsDataRegistry.ExtendedUniversalLoggingKeys.UCKAllLoads, true);
			pairs.Set(CustomsDataRegistry.ExtendedUniversalLoggingKeys.UCKLocksTaken, true);
			pairs.Set(CustomsDataRegistry.ExtendedUniversalLoggingKeys.UCQAllLoads, true);
			pairs.Set(CustomsDataRegistry.ExtendedUniversalLoggingKeys.UCQLocksTaken, true);
			CustomsDataRegistry.Instance.UCMExtendedUniversalLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pairs);

			var messageProcessor = new UniversalCustomsMessageProcessorTestClass();
			messageProcessor.GetLinkedBusinessObjectMetaDataForTesting = (message, _) =>
			{
				switch (message.EM_MessageOwner)
				{
					case "MSG1":
						return new LinkedBusinessObjectMetaData(dec.TableName, dec.PK, dec.JE_GB, dec.JE_DeclarationReference);
					case "MSG3":
						return new LinkedBusinessObjectMetaData(message.EM_LinkTable, message.EM_LinkUniqueID, message.EM_GB, ZString.Empty);
					default:
						return new LinkedBusinessObjectMetaData(invoice.TableName, invoice.PK, invoice.JZ_GB, invoice.JobNumber);
				}
			};
			messageProcessor.GetBranchForTesting = (message, _, _) =>
			{
				switch (message.EM_MessageOwner)
				{
					case "MSG1":
						return auBranch.PK;
					case "MSG3":
						return message.EM_GB;
					default:
						return nzBranch.PK;
				}
			};
			messageProcessor.GetSerializationKeysResultForTesting = (message, _, _) =>
			{
				switch (message.EM_MessageOwner)
				{
					case "MSG1":
						return new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, message.EM_MessageType == "MT1" ? new HashSet<string> { "ABC1", "TR1" } : ["ABC1"]);
					case "MSG3":
						return new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { "GHI3" });
					default:
						return new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { "DEF2" });
				}
			};
			messageProcessor.ProcessMessageForTesting = ProcessMessageForTesting;

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			using (new UCMPProcessorsRegistrationSubstitute((ApplicationCodeForTesting, messageProcessor)))
			{
				var grEngine = new EDIMessageGrEngine(new LoggingInformation(), ApplicationCodeForTesting, GrEngineServiceSetting.KeyGen);
				var processingManager = new KeyGenProcessingManager();
				CombineAssertions("KeyGen", () =>
				{
					processingManager.ExecuteBatch(CancellationToken.None);
					message1.Reload();
					AssertMessage(message1, auBranch.PK, dec.PK, dec.TableName, EDIMessage.Status.PreProcessedOK);
					message2.Reload();
					AssertMessage(message2, auBranch.PK, dec.PK, dec.TableName, EDIMessage.Status.PreProcessedOK);
					message3.Reload();
					AssertMessage(message3, nzBranch.PK, invoice.PK, invoice.TableName, EDIMessage.Status.PreProcessedOK);
					message4.Reload();
					AssertMessage(message4, nzBranch.PK, invoice.PK, invoice.TableName, EDIMessage.Status.PreProcessedOK);
					message5.Reload();
					AssertMessage(message5, auBranch.PK, dec.PK, dec.TableName, EDIMessage.Status.PreProcessedOK);
					message8.Reload();
					AssertMessage(message8, GlbBranch.CurrentBranch.PK, ZGuid.Empty, ZString.Empty, EDIMessage.Status.PreProcessedOK);

					AssertContains(processingManager.Logger.UserLogStrings.Cast<string>(),
						new[]
						{
							"T#1 - Messages loaded",
							"Lock taken:",
							"Calculating Keys for Message",
						}, @$"T#1 - Messages loaded (6): MN1234A, MN1234B, MN1234C, MN1234D, MN1234E, MN1234H
Lock taken: CREATEPREKEYBATCH,{message1.PK.ToString().ToUpperInvariant()}
Lock taken: CREATEPREKEYBATCH,{message2.PK.ToString().ToUpperInvariant()}
Lock taken: CREATEPREKEYBATCH,{message3.PK.ToString().ToUpperInvariant()}
Lock taken: CREATEPREKEYBATCH,{message4.PK.ToString().ToUpperInvariant()}
Lock taken: CREATEPREKEYBATCH,{message5.PK.ToString().ToUpperInvariant()}
Lock taken: CREATEPREKEYBATCH,{message8.PK.ToString().ToUpperInvariant()}
Calculating Keys for Message (T#1-MT1-MN1234A|MSG1)
Calculating Keys for Message (T#1-MT2-MN1234B|MSG1)
Calculating Keys for Message (T#1-MT1-MN1234C|MSG2)
Calculating Keys for Message (T#1-MT1-MN1234D|MSG2)
Calculating Keys for Message (T#1-MT1-MN1234E|MSG1)
Calculating Keys for Message (T#1-MT1-MN1234H|MSG3)
");

					var queueStates = grEngine.Dequeuer.LoadAll().OrderBy(x => x.ParentSystemCreateTimeUtc).ToArray();
					AssertEquals("queueStates.Length", 6, queueStates.Length);
					ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[0], message1.PK.ToGuid(), "MN1234A", new[] { "ABC1", "TR1" }, Guid.Empty, QueueStatusCodes.Codes.PreKey);
					ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[1], message2.PK.ToGuid(), "MN1234B", new[] { "ABC1" }, Guid.Empty, QueueStatusCodes.Codes.PreKey);
					ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[2], message3.PK.ToGuid(), "MN1234C", new[] { "DEF2" }, Guid.Empty, QueueStatusCodes.Codes.PreKey);
					ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[3], message4.PK.ToGuid(), "MN1234D", new[] { "DEF2" }, Guid.Empty, QueueStatusCodes.Codes.PreKey);
					ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[4], message5.PK.ToGuid(), "MN1234E", new[] { "ABC1", "TR1" }, Guid.Empty, QueueStatusCodes.Codes.PreKey);
					ProcessingManagerTest.AssertEDIMessageQueueState(queueStates[5], message8.PK.ToGuid(), "MN1234H", new[] { "GHI3" }, Guid.Empty, QueueStatusCodes.Codes.PreKey);
				});

				var message6 = CreateMessage(Factory, ApplicationCodeForTesting, "MT1", "MN1234F", "MSG2", dateTime.AddMinutes(++minutes));
				var message7 = CreateMessage(Factory, ApplicationCodeForTesting, "MT1", "MN1234G", "MSG1", dateTime.AddMinutes(++minutes));
				Factory.Save();

				CombineAssertions("Flipper", () =>
				{
					var flipperManager = new MessageReleasingManager();
					flipperManager.ExecuteBatch(CancellationToken.None);

					message1.Reload();
					AssertEquals("message1.EM_Status", EDIMessage.Status.PreProcessedOK, message1.EM_Status);
					message2.Reload();
					AssertEquals("message2.EM_Status", EDIMessage.Status.PreProcessedOK, message2.EM_Status);
					message3.Reload();
					AssertEquals("message3.EM_Status", EDIMessage.Status.PreProcessedOK, message3.EM_Status);
					message4.Reload();
					AssertEquals("message4.EM_Status", EDIMessage.Status.PreProcessedOK, message4.EM_Status);
					message5.Reload();
					AssertEquals("message5.EM_Status", EDIMessage.Status.PreProcessedOK, message5.EM_Status);
					message6.Reload();
					AssertEquals("message6.EM_Status", EDIMessage.Status.Queued, message6.EM_Status);
					message7.Reload();
					AssertEquals("message7.EM_Status", EDIMessage.Status.Queued, message7.EM_Status);
					message8.Reload();
					AssertEquals("message8.EM_Status", EDIMessage.Status.PreProcessedOK, message8.EM_Status);
					var all6MessagesDetails = $"Time:{SqlFormatInfo.ToSqlDateTimeString(message1.EM_SystemCreateTimeUtc.ToDateTime())}-Number:MN1234A, Time:{SqlFormatInfo.ToSqlDateTimeString(message2.EM_SystemCreateTimeUtc.ToDateTime())}-Number:MN1234B, Time:{SqlFormatInfo.ToSqlDateTimeString(message3.EM_SystemCreateTimeUtc.ToDateTime())}-Number:MN1234C, Time:{SqlFormatInfo.ToSqlDateTimeString(message4.EM_SystemCreateTimeUtc.ToDateTime())}-Number:MN1234D, Time:{SqlFormatInfo.ToSqlDateTimeString(message5.EM_SystemCreateTimeUtc.ToDateTime())}-Number:MN1234E, Time:{SqlFormatInfo.ToSqlDateTimeString(message8.EM_SystemCreateTimeUtc.ToDateTime())}-Number:MN1234H";
					var messageAtFront =
						$@"Messages at front of queue: Time:{SqlFormatInfo.ToSqlDateTimeString(message1.EM_SystemCreateTimeUtc.ToDateTime())}-Number:MN1234A, Time:{SqlFormatInfo.ToSqlDateTimeString(message3.EM_SystemCreateTimeUtc.ToDateTime())}-Number:MN1234C, Time:{SqlFormatInfo.ToSqlDateTimeString(message8.EM_SystemCreateTimeUtc.ToDateTime())}-Number:MN1234H
Oldest message: Time:{SqlFormatInfo.ToSqlDateTimeString(message1.EM_SystemCreateTimeUtc.ToDateTime())}-Number:MN1234A
Number of chains: 2
Chain Lengths: 2, 3";
					AssertContains(flipperManager.Logger.UserLogStrings,
						new[]
						{
							"Messages loaded",
							"Entities",
							"Loaded messages",
							"Messages at",
							"Oldest message:",
							"Number of chains",
							"Chain Length",
							"Releasing messages for",
							"Could not release any of the",
							"Messages Released:",
							"Deleting ",
						}, $@"Releasing messages for T#1.
Messages loaded (6): {all6MessagesDetails}
Entities loaded (6)
Entities added (6)
Loaded messages for the first time: {all6MessagesDetails}
{messageAtFront}
{messageAtFront}
Could not release any of the [6] items remaining. Waiting for items to be processed.
{messageAtFront}
{messageAtFront}
Messages Released: 6 Messages Loaded: 6
");

					var queueStates = grEngine.Dequeuer.LoadAll().OrderBy(x => x.ParentSystemCreateTimeUtc).ToArray();
					AssertEquals("queueStates.Length", 6, queueStates.Length);
					var chainId1 = queueStates[0].ChainID;
					var chainId2 = queueStates[2].ChainID;
					var chainId3 = queueStates[5].ChainID;
					AssertNotEquals("ChainID1", Guid.Empty, chainId1);
					AssertNotEquals("ChainID2", Guid.Empty, chainId2);
					AssertEquals("ChainID3", Guid.Empty, chainId3);
					AssertNotEquals("ChainID1 and ChainID2", chainId1, chainId2);
					AssertEDIMessageQueueState(queueStates[0], message1.PK.ToGuid(), "MN1234A", new[] { "ABC1", "TR1" }, chainId1, QueueStatusCodes.Codes.Queued);
					AssertEDIMessageQueueState(queueStates[1], message2.PK.ToGuid(), "MN1234B", new[] { "ABC1" }, chainId1, QueueStatusCodes.Codes.Blocked);
					AssertEDIMessageQueueState(queueStates[2], message3.PK.ToGuid(), "MN1234C", new[] { "DEF2" }, chainId2, QueueStatusCodes.Codes.Queued);
					AssertEDIMessageQueueState(queueStates[3], message4.PK.ToGuid(), "MN1234D", new[] { "DEF2" }, chainId2, QueueStatusCodes.Codes.Blocked);
					AssertEDIMessageQueueState(queueStates[4], message5.PK.ToGuid(), "MN1234E", new[] { "ABC1", "TR1" }, chainId1, QueueStatusCodes.Codes.Blocked);
					AssertEDIMessageQueueState(queueStates[5], message8.PK.ToGuid(), "MN1234H", new[] { "GHI3" }, chainId3, QueueStatusCodes.Codes.Queued);
				});

				CombineAssertions("Worker", () =>
				{
					var workerManager = new WorkerProcessingManager();
					workerManager.ExecuteBatch(CancellationToken.None);

					message1.Reload();
					AssertEquals("message1.EM_Status", EDIMessage.Status.Acknowledged, message1.EM_Status);
					message2.Reload();
					AssertEquals("message2.EM_Status", EDIMessage.Status.Acknowledged, message2.EM_Status);
					message3.Reload();
					AssertEquals("message3.EM_Status", EDIMessage.Status.Recognised, message3.EM_Status);
					message4.Reload();
					AssertEquals("message4.EM_Status", EDIMessage.Status.Recognised, message4.EM_Status);
					message5.Reload();
					AssertEquals("message5.EM_Status", EDIMessage.Status.Acknowledged, message5.EM_Status);
					message6.Reload();
					AssertEquals("message6.EM_Status", EDIMessage.Status.Queued, message6.EM_Status);
					message7.Reload();
					AssertEquals("message7.EM_Status", EDIMessage.Status.Queued, message7.EM_Status);
					message8.Reload();
					AssertEquals("message8.EM_Status", EDIMessage.Status.Captured, message8.EM_Status);

					var queueStates = grEngine.Dequeuer.LoadAll().OrderBy(x => x.ParentSystemCreateTimeUtc).ToArray();
					AssertEquals("queueStates.Length", 6, queueStates.Length);
					var queueState1 = queueStates[0];
					var queueState3 = queueStates[2];
					var queueState6 = queueStates[5];
					var chainId1 = queueState1.ChainID;
					var chainId2 = queueState3.ChainID;
					var chainId3 = queueState6.ChainID;
					AssertNotEquals("ChainID1", Guid.Empty, chainId1);
					AssertNotEquals("ChainID2", Guid.Empty, chainId2);
					AssertEquals("ChainID3", Guid.Empty, chainId3);
					AssertNotEquals("ChainID1 and ChainID2", chainId1, chainId2);
					AssertEDIMessageQueueState(queueState1, message1.PK.ToGuid(), "MN1234A", new[] { "ABC1", "TR1" }, chainId1, QueueStatusCodes.Codes.Notify);
					AssertEDIMessageQueueState(queueStates[1], message2.PK.ToGuid(), "MN1234B", new[] { "ABC1" }, chainId1, QueueStatusCodes.Codes.Notify);
					AssertEDIMessageQueueState(queueState3, message3.PK.ToGuid(), "MN1234C", new[] { "DEF2" }, chainId2, QueueStatusCodes.Codes.Notify);
					AssertEDIMessageQueueState(queueStates[3], message4.PK.ToGuid(), "MN1234D", new[] { "DEF2" }, chainId2, QueueStatusCodes.Codes.Notify);
					AssertEDIMessageQueueState(queueStates[4], message5.PK.ToGuid(), "MN1234E", new[] { "ABC1", "TR1" }, chainId1, QueueStatusCodes.Codes.Notify);
					AssertEDIMessageQueueState(queueState6, message8.PK.ToGuid(), "MN1234H", new[] { "GHI3" }, chainId3, QueueStatusCodes.Codes.Notify);
				});
			}

			void ProcessMessageForTesting(EDIMessage message, LoggingInformation logger, IUniversalCustomsMessageProcessorHelper helper)
			{
				ZString status;
				switch (message.EM_MessageOwner)
				{
					case "MSG1":
						status = EDIMessage.Status.Acknowledged;
						break;
					case "MSG3":
						status = EDIMessage.Status.Captured;
						break;
					default:
						status = EDIMessage.Status.Recognised;
						break;
				}
				message.EM_Status = status;
				AssertEquals("Message should be processed in correct environment", message.EM_GB, GlbBranch.CurrentBranch.PK);
			}
		}

		void AssertContains(StringCollection logs, string[] logsOfInterested, string logsInExpectedOrger)
		{
			var actualLog = new StringBuilder();
			var fullLog = new StringBuilder();
			foreach (var log in logs)
			{
				var trimmedLog = log.Trim();
				fullLog.AppendLine(trimmedLog);
				if (logsOfInterested.Any(trimmedLog.Contains))
				{
					actualLog.AppendLine(trimmedLog.TrimStart());
				}
			}

			AssertEquals(fullLog.ToString(), logsInExpectedOrger, actualLog.ToString());
		}

		EDIMessage CreateMessage() => CreateMessage(Factory);

		void AssertContains(StringCollection logs, params (string expectLog, int expectedCount)[] expectedLogs)
		{
			var list = expectedLogs.Select(x => new LogData { Log = x.expectLog, ExpectedCount = x.expectedCount, })
				.ToArray();
			var actualLog = new StringBuilder();
			foreach (var log in logs)
			{
				actualLog.AppendLine(log);
				foreach (var logData in list)
				{
					if (log.Contains(logData.Log))
					{
						logData.ActualCount++;
					}
				}
			}
			var fullLog = actualLog.ToString();

			list.ForEach(logData =>
			{
				AssertEquals($"Should have this log '{logData.Log}':\r\n{fullLog}", logData.ExpectedCount, logData.ActualCount);
			});
		}

		class LogData
		{
			public string Log;
			public int ExpectedCount;
			public int ActualCount;
		}

		public static void AssertContains(IEnumerable<string> logs, string[] logsOfInterested, string logsInExpectedOrder)
		{
			var actualLog = new StringBuilder();
			var fullLog = new StringBuilder();
			foreach (var log in logs)
			{
				var trimmedLog = log.Trim();
				fullLog.AppendLine(trimmedLog);
				if (logsOfInterested.Any(trimmedLog.Contains))
				{
					actualLog.AppendLine(trimmedLog.TrimStart());
				}
			}

			AssertContains(fullLog.ToString(), logsInExpectedOrder, actualLog.ToString());
		}

		public static void AssertMessage(EDIMessage message, ZGuid branchPK, ZGuid linkUniqueID, ZString linkTable, ZString status, string messageSuffix = "")
		{
			AssertEquals(messageSuffix + "EM_GB", branchPK, message.EM_GB);
			AssertEquals(messageSuffix + "EM_LinkUniqueID", linkUniqueID, message.EM_LinkUniqueID);
			AssertEquals(messageSuffix + "EM_LinkTable", linkTable, message.EM_LinkTable);
			AssertEquals(messageSuffix + "EM_Status", status, message.EM_Status);
		}

		public static void AssertEDIMessageQueueState(EDIMessageQueueState queueState, Guid messagePK, string parentMessageNumber, IEnumerable<string> keys, Guid chainId, string status, string messageSuffix = "")
		{
			AssertEquals(messageSuffix + "queueState.MessagePK", messagePK, queueState.MessagePK);
			AssertEquals(messageSuffix + "queueState.ParentMessageNumber", parentMessageNumber, queueState.ParentMessageNumber);
			AssertContainsExactElementsInExactOrder(messageSuffix + "queueState.Keys", keys, queueState.Keys);
			AssertEquals(messageSuffix + "queueState.ChainID", chainId, queueState.ChainID);
			AssertEquals(messageSuffix + "queueState.Status", status, queueState.Status);
		}

		public static GlbBranch SetupBranch(BusinessObjectFactory factory, ZString countryCode)
		{
			var company = factory.New<GlbCompany>();
			company.GC_Code = countryCode + "%";
			company.GC_RN_NKCountryCode = countryCode;
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var branch = company.Branches.AddNew();
			branch.GB_Code = countryCode + "$";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			return branch;
		}

		public static EDIMessage CreateMessage(BusinessObjectFactory factory, ZString applicationCode, ZString messageType, ZString messageNum, ZString messageOwner, ZDateTime? systemCreateTimeUtc = null)
		{
			var message = factory.New<EDIMessage>();
			message.EM_ApplicationCode = applicationCode;
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageNum = messageNum;
			message.EM_MessageOwner = messageOwner;
			if (systemCreateTimeUtc.HasValue)
			{
				message.EM_SystemCreateTimeUtc = systemCreateTimeUtc.Value;
			}
			return message;
		}

		public const string ApplicationCodeForTesting = "T#1";

		public static EDIMessage CreateMessage(BusinessObjectFactory factory, string messageNum = "MN1234", string messageOwner = "MSG OWN") =>
			CreateMessage(factory, ApplicationCodeForTesting, "MT1", messageNum, messageOwner);
	}
}
