using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.Business.Testing.ServiceTasks
{
	sealed class ServiceTaskHelperTest : TestCaseWithFactory
	{
		public void TestLogTaskStarting()
		{
			Mock<ILogger> loggerMock = new Mock<ILogger>();
			ServiceTaskHelper.LogTaskStarting(loggerMock.Object, "123", "Description");
			AssertNoExceptionThrown( () =>
			{
				loggerMock.Verify(logger => logger.Log(It.Is<LogType>(logType => logType == LogType.Debug), It.Is<string>(message => message.StartsWith("Starting service task"))), Times.Once);
			});
		}

		public void TestLogTaskFinished()
		{
			Mock<ILogger> loggerMock = new Mock<ILogger>();
			ServiceTaskHelper.LogTaskFinished(loggerMock.Object, "123", "Description");
			AssertNoExceptionThrown(() =>
			{
				loggerMock.Verify(logger => logger.Log(It.Is<LogType>(logType => logType == LogType.Debug), It.Is<string>(message => message.StartsWith("Finished service task"))), Times.Once);
			});
		}

		public void TestNudgeServiceTaskDelay()
		{
			var taskCode = "123";

			var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(nameof(IServiceTaskNudger), serviceTaskNudgerMock.Object))
			{
				AssertNudgeServiceTaskScheduling(TimeSpan.FromMinutes(10), false);
				AssertNudgeServiceTaskScheduling(TimeSpan.FromMinutes(-10), false);
				AssertNudgeServiceTaskScheduling(TimeSpan.FromMinutes(-10), true);
			}

			void AssertNudgeServiceTaskScheduling(TimeSpan taskDelay, bool ignoreNudgeIfInPast)
			{
				serviceTaskNudgerMock.Reset();

				ServiceTaskHelper.NudgeServiceTaskDelay(new Mock<ILogger>().Object, taskCode, taskDelay, ignoreNudgeIfInPast);

				AssertNoExceptionThrown(() =>
				{
					if (taskDelay > TimeSpan.Zero)
					{
						serviceTaskNudgerMock.Verify(nudger => nudger.NudgeServiceTask(It.Is<string>(code => code == taskCode), It.Is<TimeSpan?>(delay => delay.HasValue && delay.Value == taskDelay)), Times.Once);
					}
					else
					{
						if (ignoreNudgeIfInPast)
						{
							serviceTaskNudgerMock.Verify(nudger => nudger.NudgeServiceTask(It.IsAny<string>(), It.IsAny<TimeSpan?>()), Times.Never);
						}
						else
						{
							serviceTaskNudgerMock.Verify(nudger => nudger.NudgeServiceTask(It.Is<string>(code => code == taskCode), It.Is<TimeSpan?>(delay => !delay.HasValue)), Times.Once);
						}
					}
				});
			}
		}

		[TestDate(2024, 12, 1, 12, 0, 0)]
		public void TestNudgeServiceTaskTime()
		{
			var taskCode = "123";

			var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(nameof(IServiceTaskNudger), serviceTaskNudgerMock.Object))
			{
				AssertNudgeServiceTaskScheduling(ZDateTime.UtcNow.AddMinutes(10), false);
				AssertNudgeServiceTaskScheduling(ZDateTime.UtcNow.AddMinutes(-10), false);
				AssertNudgeServiceTaskScheduling(ZDateTime.UtcNow.AddMinutes(-10), true);
			}

			void AssertNudgeServiceTaskScheduling(ZDateTime taskDateTime, bool ignoreNudgeIfInPast)
			{
				serviceTaskNudgerMock.Reset();

				ServiceTaskHelper.NudgeServiceTaskTime(new Mock<ILogger>().Object, taskCode, taskDateTime, ignoreNudgeIfInPast);

				AssertNoExceptionThrown(() =>
				{
					if (taskDateTime > ZDateTime.UtcNow)
					{
						var taskDelay = taskDateTime - ZDateTime.UtcNow;
						serviceTaskNudgerMock.Verify(nudger => nudger.NudgeServiceTask(It.Is<string>(code => code == taskCode), It.Is<TimeSpan?>(delay => delay.HasValue && delay.Value == taskDelay)), Times.Once);
					}
					else
					{
						if (ignoreNudgeIfInPast)
						{
							serviceTaskNudgerMock.Verify(nudger => nudger.NudgeServiceTask(It.IsAny<string>(), It.IsAny<TimeSpan?>()), Times.Never);
						}
						else
						{
							serviceTaskNudgerMock.Verify(nudger => nudger.NudgeServiceTask(It.Is<string>(code => code == taskCode), It.Is<TimeSpan?>(delay => !delay.HasValue)), Times.Once);
						}
					}
				});
			}
		}
	}
}
