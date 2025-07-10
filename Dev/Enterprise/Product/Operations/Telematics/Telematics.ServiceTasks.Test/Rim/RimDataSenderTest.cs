using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Telematics.ServiceTasks.Rim;
using Moq;
using NUnit.Framework;

namespace Enterprise.Telematics.ServiceTasks.Test.Rim
{
	class RimDataSenderTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			dataAccessorMock = new Mock<IDataAccessor>();
			dataProcessorMock = new Mock<IDataProcessor>();
			dataSenderMock = new Mock<IDataSender>();
			rimDataSender = new RimDataSender(Factory, dataAccessorMock.Object, dataProcessorMock.Object, dataSenderMock.Object);
		}

		public void TestWrongParamsCall()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new RimDataSender(null));
				AssertEquals("logger", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new RimDataSender(null, dataAccessorMock.Object, dataProcessorMock.Object, dataSenderMock.Object));
				AssertEquals("factory", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new RimDataSender(Factory, null, dataProcessorMock.Object, dataSenderMock.Object));
				AssertEquals("dataAccessor", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new RimDataSender(Factory, dataAccessorMock.Object, null, dataSenderMock.Object));
				AssertEquals("dataProcessor", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new RimDataSender(Factory, dataAccessorMock.Object, dataProcessorMock.Object, null));
				AssertEquals("dataSender", result.ParamName);
			});
		}

		[ExpectNoExceptions]
		public void TestRunSequence()
		{
			// Arrange
			var sequence = new MockSequence();
			dataAccessorMock
				.InSequence(sequence)
				.Setup(accessor => accessor.GetData(It.IsAny<IFactory>()));
			dataProcessorMock
				.InSequence(sequence)
				.Setup(processor => processor.Process(Factory, It.IsAny<IEnumerable<IDeviceData>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()));
			dataSenderMock
				.InSequence(sequence)
				.Setup(sender => sender.Send(It.IsAny<BusinessObjectFactory>(), It.IsAny<IEnumerable<IPortionedData>>()));

			// Act
			rimDataSender.Run(CancellationToken.None);

			// Assert
			AssertEquals(1, Factory.SaveCount);
		}

		[ExpectNoExceptions]
		public void TestRunProvidesValuesToDataAccessor()
		{
			// Arrange

			// Act
			rimDataSender.Run(CancellationToken.None);

			// Assert
			dataAccessorMock.Verify(accessor => accessor.GetData(It.IsAny<IFactory>()), Times.Once);
			dataAccessorMock.Verify(accessor => accessor.GetData(Factory), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestRunProvidesValuesToDataProcessor()
		{
			// Arrange
			var dataMock = new Mock<IEnumerable<IDeviceData>>();
			var cancellationToken = new CancellationToken();
			dataAccessorMock
				.Setup(accessor => accessor.GetData(It.IsAny<IFactory>()))
				.Returns(dataMock.Object);

			// Act
			rimDataSender.Run(cancellationToken);

			// Assert
			dataProcessorMock.Verify(processor => processor.Process(Factory, It.IsAny<IEnumerable<IDeviceData>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
			dataProcessorMock.Verify(processor => processor.Process(Factory, dataMock.Object, 10000, cancellationToken), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestRunProvidesValuesToDataSender()
		{
			// Arrange
			var dataMock = new Mock<IEnumerable<IPortionedData>>();
			dataProcessorMock
				.Setup(processor => processor.Process(Factory, It.IsAny<IEnumerable<IDeviceData>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
				.Returns(dataMock.Object);

			// Act
			rimDataSender.Run(CancellationToken.None);

			// Assert
			dataSenderMock.Verify(sender => sender.Send(Factory, dataMock.Object), Times.Once);
			dataSenderMock.VerifyNoOtherCalls();
		}

		Mock<IDataAccessor> dataAccessorMock;
		Mock<IDataProcessor> dataProcessorMock;
		Mock<IDataSender> dataSenderMock;
		RimDataSender rimDataSender;
	}
}
