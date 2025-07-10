using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.PreDriveChecklist;
using Moq;
using NUnit.Framework;

namespace Enterprise.Telematics.ServiceTasks.Test.PreDriveChecklist
{
	class TelematicsPreDriverChecklistAlertProcessorTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			checklistAccessorMock = new Mock<IChecklistAccessor>();
			checklistProcessorMock = new Mock<IChecklistProcessor>();
			loggerMock = new Mock<ILogger>();
			task = new TelematicsPreDriveChecklistAlertProcessor(Factory, checklistAccessorMock.Object, checklistProcessorMock.Object, loggerMock.Object);
		}

		public void TestWrongParamsCall()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new TelematicsPreDriveChecklistAlertProcessor(null, checklistAccessorMock.Object, checklistProcessorMock.Object, loggerMock.Object));
				AssertEquals("factory", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new TelematicsPreDriveChecklistAlertProcessor(Factory, null, checklistProcessorMock.Object, loggerMock.Object));
				AssertEquals("checklistAccessor", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new TelematicsPreDriveChecklistAlertProcessor(Factory, checklistAccessorMock.Object, null, loggerMock.Object));
				AssertEquals("checklistProcessor", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new TelematicsPreDriveChecklistAlertProcessor(Factory, checklistAccessorMock.Object, checklistProcessorMock.Object, null));
				AssertEquals("logger", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new TelematicsPreDriveChecklistAlertProcessor(null));
				AssertEquals("logger", result.ParamName);
			});
		}

		[ExpectNoExceptions]
		public void TestRunSequence()
		{
			// Arrange
			var checklists = new List<TelPreDriveChecklistHeader>();
			var mockSequence = new MockSequence();
			var cancellationToken = new CancellationToken();

			loggerMock
				.InSequence(mockSequence)
				.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()));
			checklistAccessorMock
				.InSequence(mockSequence)
				.Setup(accessor => accessor.GetChecklists(It.IsAny<BusinessObjectFactory>(), It.IsAny<CancellationToken>()))
				.Returns(checklists);
			loggerMock
				.InSequence(mockSequence)
				.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()));
			checklistProcessorMock
				.InSequence(mockSequence)
				.Setup(processor => processor.ProcessChecklists(It.IsAny<BusinessObjectFactory>(), It.IsAny<ICollection<TelPreDriveChecklistHeader>>(), It.IsAny<CancellationToken>()))
				.Returns(0);
			loggerMock
				.InSequence(mockSequence)
				.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()));

			// Act
			task.Run(cancellationToken);

			// Assert
			checklistAccessorMock.Verify(accessor => accessor.GetChecklists(Factory, cancellationToken), Times.Once);
			checklistProcessorMock.Verify(processor => processor.ProcessChecklists(Factory, checklists, cancellationToken), Times.Once);
			loggerMock.Verify(logger => logger.Log(LogType.Information, "Start processing Checklists"), Times.Once);
			loggerMock.Verify(logger => logger.Log(LogType.Information, "Processing 0 Checklists"), Times.Once);
			loggerMock.Verify(logger => logger.Log(LogType.Information, "Processed 0 Checklists"), Times.Once);
		}

		TelematicsPreDriveChecklistAlertProcessor task;
		Mock<IChecklistAccessor> checklistAccessorMock;
		Mock<IChecklistProcessor> checklistProcessorMock;
		Mock<ILogger> loggerMock;
	}
}
