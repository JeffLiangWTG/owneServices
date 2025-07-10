using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.RimProcessors;
using Moq;
using WTG.Telematics.Common;
using WTG.Telematics.Data.CargoWiseOne;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsXmlMessageProcessors.RimProcessors
{
	public class TcaRegistrationMessageProcessorTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			loggerMock = new Mock<ILogger>();
			rimRegistrationMessageProcessorMock = new Mock<ITcaRegistrationProcessor<RimRegistrationMessage>>();
			revokeRimRegistrationMessageProcessorMock = new Mock<ITcaRegistrationProcessor<RevokeRimRegistrationMessage>>();
			processor = new TcaRegistrationMessageProcessor(loggerMock.Object, rimRegistrationMessageProcessorMock.Object, revokeRimRegistrationMessageProcessorMock.Object);
		}

		Mock<ILogger> loggerMock;
		Mock<ITcaRegistrationProcessor<RimRegistrationMessage>> rimRegistrationMessageProcessorMock;
		Mock<ITcaRegistrationProcessor<RevokeRimRegistrationMessage>> revokeRimRegistrationMessageProcessorMock;
		TcaRegistrationMessageProcessor processor;

		public void TestRimRegistrationMessage()
		{
			CombineAssertions(() =>
			{
				Test(new RimRegistrationMessage
				{
					DeviceAssignmentTime = new DateTimeOffset(2020, 1, 2, 3, 4, 5, 6, TimeSpan.FromHours(10)),
					DeviceId = "01020304",
				});
				Test(new RimRegistrationMessage
				{
					DeviceAssignmentTime = new DateTimeOffset(2020, 6, 5, 4, 3, 2, 1, TimeSpan.FromHours(11)),
					DeviceId = "04030201",
				});
			});

			void Test(RimRegistrationMessage message)
			{
				// Arrange
				rimRegistrationMessageProcessorMock
					.Setup(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<RimRegistrationMessage>()))
					.Returns(1);

				// Act
				var result = processor.Process(Factory, XmlDataSerializer.Serialize(message));

				// Assert
				AssertEquals(1, result);
				rimRegistrationMessageProcessorMock.Verify(
					processor => processor.Process(
						Factory,
						It.Is<RimRegistrationMessage>(
							value => value.DeviceId == message.DeviceId && value.DeviceAssignmentTime == value.DeviceAssignmentTime)),
					Times.Once);
			}
		}

		public void TestRevokeRimRegistrationMessage()
		{
			CombineAssertions(() =>
			{
				Test(new RevokeRimRegistrationMessage
				{
					DeviceRevokeTime = new DateTimeOffset(2020, 1, 2, 3, 4, 5, 6, TimeSpan.FromHours(10)),
					DeviceId = "01020304",
				});
				Test(new RevokeRimRegistrationMessage
				{
					DeviceRevokeTime = new DateTimeOffset(2020, 6, 5, 4, 3, 2, 1, TimeSpan.FromHours(11)),
					DeviceId = "04030201",
				});
			});

			void Test(RevokeRimRegistrationMessage message)
			{
				// Arrange
				revokeRimRegistrationMessageProcessorMock.Reset();
				revokeRimRegistrationMessageProcessorMock
					.Setup(processor => processor.Process(It.IsAny<BusinessObjectFactory>(), It.IsAny<RevokeRimRegistrationMessage>()))
					.Returns(1);

				// Act
				var result = processor.Process(Factory, XmlDataSerializer.Serialize(message));

				// Assert
				AssertEquals(1, result);
				revokeRimRegistrationMessageProcessorMock.Verify(
					processor => processor.Process(
						Factory,
						It.Is<RevokeRimRegistrationMessage>(
							value => value.DeviceId == message.DeviceId && value.DeviceRevokeTime == value.DeviceRevokeTime)),
					Times.Once);
			}
		}

		public void TestInvalidMessages()
		{
			CombineAssertions(() =>
			{
				Test(null);
				Test(string.Empty);
				Test("<SomethingInvalid />");
				Test(
					"<RimRegistrationMessage DeviceId=\"04030201\" DeviceAssignmentTime=\"2020-01-02 03:04:05.006 +10:00\" />\r\n" +
					"<RevokeRimRegistrationMessage DeviceId=\"04030201\" DeviceRevokeTime=\"2020-01-02 03:04:05.006 +10:00\" />");
			});

			void Test(string message)
			{
				// Arrange
				// Act
				var result = processor.Process(Factory, message);

				// Assert
				AssertEquals(0, result);
			}
		}

		public void TestWrongParamsCall()
		{
			// Arrange
			// Act
			// Assert
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => new TcaRegistrationMessageProcessor(null, rimRegistrationMessageProcessorMock.Object, revokeRimRegistrationMessageProcessorMock.Object));
				AssertEquals("logger", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new TcaRegistrationMessageProcessor(loggerMock.Object, null, revokeRimRegistrationMessageProcessorMock.Object));
				AssertEquals("rimRegistrationMessageProcessor", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new TcaRegistrationMessageProcessor(loggerMock.Object, rimRegistrationMessageProcessorMock.Object, null));
				AssertEquals("revokeRimRegistrationMessageProcessor", result.ParamName);
			});
		}
	}
}
