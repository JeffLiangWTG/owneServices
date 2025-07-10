using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Telematics.ServiceTasks.MessageTypeProcessors;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Telematics.ServiceTasks.Test
{
	public abstract class EHubMessageProcessorTest : TestCase
	{
		protected override void SetUp()
		{
			factory = new BusinessObjectFactory();
			loggerMock = new Mock<ILogger>();
			protobufMessageTypeProcessorMock = new Mock<IMessageTypeProcessor>();
			protobufMessageTypeProcessorMock.SetupGet(processor => processor.MessageType).Returns(TelematicsMessageList.Codes.ProtobufData);
			telematicsXmlMessageTypeProcessorMock = new Mock<IMessageTypeProcessor>();
			telematicsXmlMessageTypeProcessorMock.SetupGet(processor => processor.MessageType).Returns(TelematicsMessageList.Codes.TelematicsXmlData);
			ehubMessageProcessor = new EHubMessageProcessor(loggerMock.Object, factory, protobufMessageTypeProcessorMock.Object, telematicsXmlMessageTypeProcessorMock.Object);
		}

		EHubMessageProcessor ehubMessageProcessor;
		BusinessObjectFactory factory;
		Mock<ILogger> loggerMock;
		Mock<IMessageTypeProcessor> protobufMessageTypeProcessorMock;
		Mock<IMessageTypeProcessor> telematicsXmlMessageTypeProcessorMock;

		public class CommonTest : EHubMessageProcessorTest
		{
			public void TestMaximumRetryCount()
			{
				AssertEquals(5, EHubMessageProcessor.MaximumRetryCount);
			}

			public void TestWrongConstructorParamsCall()
			{
				CombineAssertions(() =>
				{
					var result = AssertExceptionThrown<ArgumentNullException>(() => new EHubMessageProcessor(null, factory));
					AssertEquals("logger", result.ParamName);

					result = AssertExceptionThrown<ArgumentNullException>(() => new EHubMessageProcessor(loggerMock.Object, null));
					AssertEquals("factory", result.ParamName);

					result = AssertExceptionThrown<ArgumentNullException>(() => new EHubMessageProcessor(loggerMock.Object, factory, null, telematicsXmlMessageTypeProcessorMock.Object));
					AssertEquals("protobufMessageTypeProcessor", result.ParamName);

					result = AssertExceptionThrown<ArgumentNullException>(() => new EHubMessageProcessor(loggerMock.Object, factory, protobufMessageTypeProcessorMock.Object, null));
					AssertEquals("telematicsXmlMessageTypeProcessor", result.ParamName);
				});
			}

			public void TestTypeProcessors()
			{
				var messageTypeProcessors = new EHubMessageProcessor(loggerMock.Object, factory).messageTypeProcessors;

				var result = messageTypeProcessors.Values.Select(processor => processor.GetType());

				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						typeof(ProtobufMessageTypeProcessor),
						typeof(TelematicsXmlMessageTypeProcessor),
					},
					result);
			}

			public void TestAssignsProcessorsInConstructor()
			{
				// Arrange

				// Act
				var result = ehubMessageProcessor.messageTypeProcessors.Values;

				// Assert
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						protobufMessageTypeProcessorMock.Object,
						telematicsXmlMessageTypeProcessorMock.Object,
					},
					result);
				protobufMessageTypeProcessorMock.VerifyGet(processor => processor.MessageType, Times.Once);
				telematicsXmlMessageTypeProcessorMock.VerifyGet(processor => processor.MessageType, Times.Once);
			}

			public void TestProperIndexExists()
			{
				AssertEquals(true, IndexLoader.Exists(Db.Connection, EDIMessageSchema.Constants.SqlSchemaName, EDIMessageSchema.Constants.TableName
					, EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));
			}
		}

		[UseSnapshotProtection]
		public abstract class EHubMessageProcessorDatabaseTest : EHubMessageProcessorTest
		{
			protected override void SetUp()
			{
				base.SetUp();
				AssertEquals(0, MessagesCount());
			}

			protected static int MessagesCount()
			{
				return Db.Connection.ExecuteScalar<int>($"SELECT COUNT(*) FROM {EDIMessageSchema.Constants.SqlSchemaName}.{EDIMessageSchema.Constants.TableName}");
			}

			protected static void DeleteMessages()
			{
				Db.Connection.ExecuteNonQuery($"DELETE {EDIMessageSchema.Constants.SqlSchemaName}.{EDIMessageSchema.Constants.TableName}");
			}
		}

		public abstract class MessageFilteringTest : EHubMessageProcessorDatabaseTest
		{
			public class ApplicationCode : MessageFilteringTest
			{
				public void TestLoads()
				{
					Test(ApplicationCodeList.Codes.Telematics);

					void Test(string appCode)
					{
						// Arrange
						protobufMessageTypeProcessorMock.Reset();
						telematicsXmlMessageTypeProcessorMock.Reset();
						DeleteMessages();

						EHubHelpers.CreateEHubMessage(factory, appCode, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, TelematicsMessageList.Codes.ProtobufData, "blah", ReceiveTransmitList.Codes.Receive);
						EHubHelpers.CreateEHubMessage(factory, appCode, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, TelematicsMessageList.Codes.TelematicsXmlData, "blah", ReceiveTransmitList.Codes.Receive);
						factory.Save();

						// Act
						ehubMessageProcessor.Run(CancellationToken.None);

						// Assert
						AssertNoExceptionThrown(() =>
						{
							AssertEquals(2, MessagesCount());
							protobufMessageTypeProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()), Times.Once);
							telematicsXmlMessageTypeProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()), Times.Once);
						});
					}
				}

				public void TestDoesNotLoad()
				{
					CombineAssertions(() =>
					{
						Test(ApplicationCodeList.Codes.Unknown);
						Test(ApplicationCodeList.Codes.AUCustomsNEXDOC);
						Test(ApplicationCodeList.Codes.CACustoms);
						Test(ApplicationCodeList.Codes.Pentant);
					});

					void Test(string appCode)
					{
						// Arrange
						DeleteMessages();

						EHubHelpers.CreateEHubMessage(factory, appCode, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, TelematicsMessageList.Codes.ProtobufData, "blah", ReceiveTransmitList.Codes.Receive);
						EHubHelpers.CreateEHubMessage(factory, appCode, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, TelematicsMessageList.Codes.TelematicsXmlData, "blah", ReceiveTransmitList.Codes.Receive);
						factory.Save();

						// Act
						ehubMessageProcessor.Run(CancellationToken.None);

						// Assert
						AssertNoExceptionThrown(() =>
						{
							AssertEquals(2, MessagesCount());
							protobufMessageTypeProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()), Times.Never);
							telematicsXmlMessageTypeProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()), Times.Never);
						});
					}
				}
			}

			public class Status : MessageFilteringTest
			{
				public void TestLoads()
				{
					Test(EDIMessageStatusList.Codes.Queued);

					void Test(string status)
					{
						// Arrange
						protobufMessageTypeProcessorMock.Reset();
						telematicsXmlMessageTypeProcessorMock.Reset();
						DeleteMessages();

						EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, status, true, EDIMessageTypeList.Codes.XMS, TelematicsMessageList.Codes.ProtobufData, "blah", ReceiveTransmitList.Codes.Receive);
						EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, status, true, EDIMessageTypeList.Codes.XMS, TelematicsMessageList.Codes.TelematicsXmlData, "blah", ReceiveTransmitList.Codes.Receive);
						factory.Save();

						// Act
						ehubMessageProcessor.Run(CancellationToken.None);

						// Assert
						AssertNoExceptionThrown(() =>
						{
							AssertEquals(2, MessagesCount());
							protobufMessageTypeProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()), Times.Once);
							telematicsXmlMessageTypeProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()), Times.Once);
						});
					}
				}

				public void TestDoesNotLoad()
				{
					CombineAssertions(() =>
					{
						Test(EDIMessageStatusList.Codes.Acknowledged);
						Test(EDIMessageStatusList.Codes.Cancelled);
						Test(EDIMessageStatusList.Codes.Discarded);
						Test(EDIMessageStatusList.Codes.Failed);
						Test(EDIMessageStatusList.Codes.Error);
						Test(EDIMessageStatusList.Codes.Pending);
						Test(EDIMessageStatusList.Codes.PreProcessedOK);
						Test(EDIMessageStatusList.Codes.ProcessedOK);
						Test(EDIMessageStatusList.Codes.Received);
						Test(EDIMessageStatusList.Codes.Recognised);
					});

					void Test(string status)
					{
						// Arrange
						DeleteMessages();

						EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, status, true, EDIMessageTypeList.Codes.XMS, TelematicsMessageList.Codes.ProtobufData, "blah", ReceiveTransmitList.Codes.Receive);
						EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, status, true, EDIMessageTypeList.Codes.XMS, TelematicsMessageList.Codes.TelematicsXmlData, "blah", ReceiveTransmitList.Codes.Receive);
						factory.Save();

						// Act
						ehubMessageProcessor.Run(CancellationToken.None);

						// Assert
						AssertNoExceptionThrown(() =>
						{
							AssertEquals(2, MessagesCount());
							protobufMessageTypeProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()), Times.Never);
							telematicsXmlMessageTypeProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()), Times.Never);
						});
					}
				}
			}

			public class SubType : MessageFilteringTest
			{
				public void TestLoads()
				{
					CombineAssertions(() =>
					{
						Test(TelematicsMessageList.Codes.ProtobufData, protobufMessageTypeProcessorMock);
						Test(TelematicsMessageList.Codes.TelematicsXmlData, telematicsXmlMessageTypeProcessorMock);
					});

					void Test(string subType, Mock<IMessageTypeProcessor> messageTypeProcessorMock)
					{
						// Arrange
						messageTypeProcessorMock.Reset();
						DeleteMessages();

						EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, subType, "blah", ReceiveTransmitList.Codes.Receive);
						EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, subType, "blah", ReceiveTransmitList.Codes.Receive);
						EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, subType, "blah", ReceiveTransmitList.Codes.Receive);
						factory.Save();

						// Act
						ehubMessageProcessor.Run(CancellationToken.None);

						// Assert
						AssertNoExceptionThrown(() =>
						{
							AssertEquals(3, MessagesCount());
							messageTypeProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()), Times.Exactly(3));
						});
					}
				}

				public void TestDoesNotLoad()
				{
					CombineAssertions(() =>
					{
						Test(ReceiveTransmitList.Codes.Transmit);
						Test(ReceiveTransmitList.Codes.Internal);
					});

					void Test(string subType)
					{
						// Arrange
						DeleteMessages();

						EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, subType, "blah", ReceiveTransmitList.Codes.Receive);
						EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, subType, "blah", ReceiveTransmitList.Codes.Receive);
						EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, subType, "blah", ReceiveTransmitList.Codes.Receive);
						factory.Save();

						// Act
						ehubMessageProcessor.Run(CancellationToken.None);

						// Assert
						AssertNoExceptionThrown(() =>
						{
							AssertEquals(3, MessagesCount());
							protobufMessageTypeProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()), Times.Never);
							telematicsXmlMessageTypeProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()), Times.Never);
						});
					}
				}
			}

			public class Receive : MessageFilteringTest
			{
				public void TestLoads()
				{
					Test(ReceiveTransmitList.Codes.Receive);

					void Test(string receiveTransmit)
					{
						// Arrange
						protobufMessageTypeProcessorMock.Reset();
						telematicsXmlMessageTypeProcessorMock.Reset();
						DeleteMessages();

						EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, TelematicsMessageList.Codes.ProtobufData, "blah", receiveTransmit);
						EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, TelematicsMessageList.Codes.TelematicsXmlData, "blah", receiveTransmit);
						factory.Save();

						// Act
						ehubMessageProcessor.Run(CancellationToken.None);

						// Assert
						AssertNoExceptionThrown(() =>
						{
							AssertEquals(2, MessagesCount());
							protobufMessageTypeProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()), Times.Once);
							telematicsXmlMessageTypeProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()), Times.Once);
						});
					}
				}

				public void TestDoesNotLoad()
				{
					CombineAssertions(() =>
					{
						Test("ABC");
						Test("BBC");
						Test("CNN");
					});

					void Test(string receiveTransmit)
					{
						// Arrange
						DeleteMessages();

						EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, TelematicsMessageList.Codes.ProtobufData, "blah", receiveTransmit);
						EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, TelematicsMessageList.Codes.TelematicsXmlData, "blah", receiveTransmit);
						factory.Save();

						// Act
						ehubMessageProcessor.Run(CancellationToken.None);

						// Assert
						AssertNoExceptionThrown(() =>
						{
							AssertEquals(2, MessagesCount());
							protobufMessageTypeProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()), Times.Never);
							telematicsXmlMessageTypeProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()), Times.Never);
						});
					}
				}
			}
		}

		public class MessageProcessing : EHubMessageProcessorDatabaseTest
		{
			protected override void SetUp()
			{
				base.SetUp();
				testCaseSource = new[]
				{
					(TelematicsMessageList.Codes.ProtobufData, protobufMessageTypeProcessorMock),
					(TelematicsMessageList.Codes.TelematicsXmlData, telematicsXmlMessageTypeProcessorMock),
				};
			}

			public void TestProvidesCorrectValuesToProcessors()
			{
				// Arrange
				var pks = new[]
				{
					EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, TelematicsMessageList.Codes.ProtobufData, "TelematicsMessageList.Codes.ProtobufData", ReceiveTransmitList.Codes.Receive),
					EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, TelematicsMessageList.Codes.TelematicsXmlData, "TelematicsMessageList.Codes.TelematicsXmlData", ReceiveTransmitList.Codes.Receive),
				};
				factory.Save();

				// Act
				ehubMessageProcessor.Run(CancellationToken.None);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					AssertEquals(pks.Length, MessagesCount());
					protobufMessageTypeProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()), Times.Once);
					protobufMessageTypeProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", "TelematicsMessageList.Codes.ProtobufData"), Times.Once);
					telematicsXmlMessageTypeProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()), Times.Once);
					telematicsXmlMessageTypeProcessorMock.Verify(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", "TelematicsMessageList.Codes.TelematicsXmlData"), Times.Once);
				});
			}

			public void TestMessageStatusIsSetToReceivedOnSuccess()
			{
				CombineAssertions(() =>
				{
					foreach (var (messageSubType, messageTypeProcessorMock) in testCaseSource)
					{
						Test(messageSubType, messageTypeProcessorMock);
					}
				});

				void Test(string messageSubType, Mock<IMessageTypeProcessor> messageTypeProcessorMock)
				{
					// Arrange
					DeleteMessages();

					var pks = new[]
					{
						EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, messageSubType, "blah", ReceiveTransmitList.Codes.Receive),
						EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, messageSubType, "blah", ReceiveTransmitList.Codes.Receive),
					};
					factory.Save();

					// Act
					ehubMessageProcessor.Run(CancellationToken.None);

					// Assert
					AssertNoExceptionThrown(() =>
					{
						AssertEquals(pks.Length, MessagesCount());
						foreach (var pk in pks)
						{
							var message = factory.Load<EDIMessage>(pk);
							AssertEquals(EDIMessageStatusList.Codes.Received, message.EM_Status);
							AssertEquals(EDIInterchangeStatusList.Codes.Received, message.Interchange.EI_Status);
						}
					});
				}
			}

			public void TestMessageRetryCountIsIncrementedOnException()
			{
				CombineAssertions(() =>
				{
					foreach (var (messageSubType, messageTypeProcessorMock) in testCaseSource)
					{
						Test(messageSubType, messageTypeProcessorMock);
					}
				});

				void Test(string messageSubType, Mock<IMessageTypeProcessor> messageTypeProcessorMock)
				{
					// Arrange
					DeleteMessages();

					var pk = EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, messageSubType, "blah", ReceiveTransmitList.Codes.Receive);
					messageTypeProcessorMock.Setup(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()))
						.Throws<Exception>();
					factory.Save();

					var message = factory.Load<EDIMessage>(pk);

					for (var i = 0; i < EHubMessageProcessor.MaximumRetryCount - 1; i++)
					{
						// Act
						ehubMessageProcessor.Run(CancellationToken.None);

						// Assert
						AssertNoExceptionThrown(() =>
						{
							message.Reload();
							AssertEquals("Message should remain marked as queued", EDIMessageStatusList.Codes.Queued, message.EM_Status);
							AssertEquals("Interchange should remain marked as queued", EDIInterchangeStatusList.Codes.Queued, message.Interchange.EI_Status);
							AssertEquals("Interchange retry count should be incremented", i + 1, message.Interchange.EI_RetryCount);
							AssertEquals(1, ErrorReporter.TotalErrorCount);
						});
					}

					ErrorReporter.Clear();
				}
			}

			public void TestMessageIsSetToFailOnMaxRetryReached()
			{
				CombineAssertions(() =>
				{
					foreach (var (messageSubType, messageTypeProcessorMock) in testCaseSource)
					{
						Test(messageSubType, messageTypeProcessorMock);
					}
				});

				void Test(string messageSubType, Mock<IMessageTypeProcessor> messageTypeProcessorMock)
				{
					// Arrange
					DeleteMessages();

					var pk = EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, messageSubType, "blah", ReceiveTransmitList.Codes.Receive);
					messageTypeProcessorMock.Setup(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()))
						.Throws<Exception>();

					var message = factory.Load<EDIMessage>(pk);
					message.Interchange.EI_RetryCount = EHubMessageProcessor.MaximumRetryCount - 1;
					factory.Save();

					// Act
					ehubMessageProcessor.Run(CancellationToken.None);

					// Assert
					AssertNoExceptionThrown(() =>
					{
						message.Reload();
						AssertEquals("Message should be marked as failed", EDIMessageStatusList.Codes.Failed, message.EM_Status);
						AssertEquals("Interchange should be marked as failed", EDIInterchangeStatusList.Codes.Failed, message.Interchange.EI_Status);
						AssertEquals(1, ErrorReporter.TotalErrorCount);
					});

					ErrorReporter.Clear();
				}
			}

			public void TestReportsEachException()
			{
				CombineAssertions(() =>
				{
					foreach (var (messageSubType, messageTypeProcessorMock) in testCaseSource)
					{
						Test(messageSubType, messageTypeProcessorMock);
					}
				});

				void Test(string messageSubType, Mock<IMessageTypeProcessor> messageTypeProcessorMock)
				{
					// Arrange
					loggerMock.Reset();
					DeleteMessages();

					var exceptions = new[] { new Exception(), new InvalidOperationException() };
					var pks = Enumerable.Range(0, exceptions.Length + 1)
						.Select(i => EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, messageSubType, "blah", ReceiveTransmitList.Codes.Receive))
						.ToArray();
					messageTypeProcessorMock
						.SetupSequence(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()))
						.Throws(exceptions[0])
						.Returns(1)
						.Throws(exceptions[1]);
					factory.Save();
					var errorReporterMock = new Mock<IErrorReporter>();

					using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
					{
						// Act
						ehubMessageProcessor.Run(CancellationToken.None);

						// Assert
						AssertEquals(pks.Length, MessagesCount());
						AssertFailedMessage(pks[0]);
						AssertProcessedMessage(pks[1]);
						AssertFailedMessage(pks[2]);
						AssertAndClearErrorReporter();
						AssertNoErrorsShouldBeLoggedAsErrorReporterDoesIt();
					}

					void AssertFailedMessage(ZGuid pk)
					{
						var failedMessage = factory.Load<EDIMessage>(pk);
						AssertEquals(EDIMessageStatusList.Codes.Queued, failedMessage.EM_Status);
						AssertEquals(EDIInterchangeStatusList.Codes.Queued, failedMessage.Interchange.EI_Status);
					}

					void AssertProcessedMessage(ZGuid pk)
					{
						var processedMessage = factory.Load<EDIMessage>(pk);
						AssertEquals(EDIMessageStatusList.Codes.Received, processedMessage.EM_Status);
						AssertEquals(EDIInterchangeStatusList.Codes.Received, processedMessage.Interchange.EI_Status);
					}

					void AssertAndClearErrorReporter()
					{
						AssertEquals(exceptions.Length, ErrorReporter.TotalErrorCount);
						foreach (var exception in exceptions)
						{
							errorReporterMock.Verify(reporter => reporter.Report(It.IsAny<string>(), It.IsAny<string>(), exception), Times.Once);
						}
						errorReporterMock.Verify(reporter => reporter.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Exactly(exceptions.Length));
						ErrorReporter.Clear();
					}

					void AssertNoErrorsShouldBeLoggedAsErrorReporterDoesIt()
					{
						AssertNoExceptionThrown(() =>
						{
							loggerMock.Verify(logger => logger.Log(LogType.Error, It.IsAny<string>()), Times.Never);
							loggerMock.Verify(logger => logger.Log(LogType.Error, It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
							loggerMock.Verify(logger => logger.Log(LogType.Warning, It.IsAny<string>()), Times.Once);
						});
					}
				}
			}

			public void TestEveryProcessorCountersAreLogged()
			{
				// Arrange
				EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, TelematicsMessageList.Codes.ProtobufData, "aaa", ReceiveTransmitList.Codes.Receive);
				EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, TelematicsMessageList.Codes.TelematicsXmlData, "bbb", ReceiveTransmitList.Codes.Receive);
				EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, TelematicsMessageList.Codes.TelematicsXmlData, "ccc", ReceiveTransmitList.Codes.Receive);
				EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, TelematicsMessageList.Codes.TelematicsXmlData, "ddd", ReceiveTransmitList.Codes.Receive);

				protobufMessageTypeProcessorMock
					.Setup(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()))
					.Returns(1);
				telematicsXmlMessageTypeProcessorMock
					.SetupSequence(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), "TELEMATIC", It.IsAny<string>()))
					.Returns(1)
					.Returns(3)
					.Returns(16);
				factory.Save();

				// Act
				ehubMessageProcessor.Run(CancellationToken.None);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					loggerMock.Verify(logger => logger.Log(LogType.Information, It.Is<string>(s =>
						s.Contains("PBD - 1 "))), Times.Once);
					loggerMock.Verify(logger => logger.Log(LogType.Information, It.Is<string>(s =>
						s.Contains("TXD - 20 "))), Times.Once);
				});
			}

			public void TestNoEHubMessagesNoLogs()
			{
				// Arrange

				// Act
				ehubMessageProcessor.Run(CancellationToken.None);

				// Assert
				AssertNoExceptionThrown(() => { loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Never); });
			}

			IEnumerable<(string messageSubType, Mock<IMessageTypeProcessor> messageTypeProcessorMock)> testCaseSource;

			public void TestEnvironmentIsSetForCurrentBranch()
			{
				// Arrange
				EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, TelematicsMessageList.Codes.ProtobufData, "TelematicsMessageList.Codes.ProtobufData", ReceiveTransmitList.Codes.Receive);
				EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, TelematicsMessageList.Codes.TelematicsXmlData, "TelematicsMessageList.Codes.TelematicsXmlData", ReceiveTransmitList.Codes.Receive);
				factory.Save();

				// Act
				using (Env.Instance.TemporaryServiceTaskContext("TEL", canRunInAnyBranch: true))
				{
					ehubMessageProcessor.Run(CancellationToken.None);
				}

				// Assert
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
		}
	}
}
