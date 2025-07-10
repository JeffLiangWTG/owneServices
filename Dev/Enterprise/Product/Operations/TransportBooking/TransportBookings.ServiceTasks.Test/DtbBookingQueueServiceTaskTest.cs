using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Application;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.TransportBookings.ServiceTasks.Test
{
	[TestedType(typeof(DtbBookingQueueServiceTask))]
	public class DtbBookingQueueServiceTaskTest : ServiceTaskTestCase<DtbBookingQueueServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(DtbBookingQueueSchema.Constants.TableName, null)
				};
			}
		}

		public void TestServiceTaskAssemblyHasCorrectAttributes()
		{
			var xx1 = Assembly.GetAssembly(typeof(DtbBookingQueueServiceTask))
				.GetCustomAttributes(typeof(HostedServiceAttribute), false)
				.OfType<HostedServiceAttribute>();
			var hostedServiceAttribute = xx1.FirstOrDefault((HostedServiceAttribute a) => a.Type == typeof(DtbBookingQueueServiceTask));
			var hostedServiceBusinessObjectBindingAttribute = Assembly.GetAssembly(typeof(DtbBookingQueueServiceTask))
				.GetCustomAttributes(typeof(HostedServiceBusinessObjectBindingAttribute), false)
				.OfType<HostedServiceBusinessObjectBindingAttribute>()
				.FirstOrDefault((HostedServiceBusinessObjectBindingAttribute a) => a.Table == "DtbBookingQueue");

			CombineAssertions(() =>
			{
				AssertNotNull("HostedService attribute should be specified on DtbBookingQueueServiceTask", hostedServiceAttribute);
				AssertNotNull("HostedServiceBusinessObjectBinding attribute should be specified on DtbBookingQueueServiceTask", hostedServiceBusinessObjectBindingAttribute);
			});

			CombineAssertions(() =>
			{
				AssertEquals("KMQ", hostedServiceAttribute.Code);
				AssertEquals("Transport Job Generator", hostedServiceAttribute.Description);
				AssertEquals("DOM", hostedServiceAttribute.Category);
				AssertEquals(true, hostedServiceAttribute.IsMandatory);
				AssertEquals(false, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals("5minutes", hostedServiceAttribute.MinimumPeriod);
				AssertEquals(true, hostedServiceAttribute.CanRunInAnyBranch);
				AssertEquals(true, hostedServiceAttribute.ActiveByDefault);
				AssertEquals("15minutes", hostedServiceAttribute.DefaultScheduleRunEvery);

				AssertEquals("KMQ", hostedServiceBusinessObjectBindingAttribute.ServiceTaskCode);
				AssertNull("Queue Name should be null, QueueName for service task is taken from HostedServiceQueueProvider assembly attribute instead", hostedServiceBusinessObjectBindingAttribute.QueueName);
				AssertEquals("HostedServiceBusinessObjectBinding attribute should have non-null Predicates populated with an empty array", 0, hostedServiceBusinessObjectBindingAttribute.Predicates.Length);
			});
		}

		public void TestServiceTaskCallsRunnerRunWithCancellationTokenAndCatchesErrors()
		{
			var mockRunner = new Mock<IDtbBookingQueueRunner>(MockBehavior.Loose);
			using (ObjectFactory.Substitute(mockRunner.Object))
			{
				var logger = new TestServiceLogger();
				var serviceTask = new DtbBookingQueueServiceTask();
				AssertNotNull("Precondition", serviceTask);
				serviceTask.ServiceLogger = logger;

				var tokenSource = new CancellationTokenSource();
				var token = tokenSource.Token;
				serviceTask.RunTask(token);

				mockRunner.Verify(r => r.Run(token, logger), Times.Once, "ServiceTask.RunTask() should call runner.Run with the passed in token and serviceTask.ServiceLogger");
				var normalRunExpectedLogStart = @"Information|Transport Job Generator service task started.
Information|Transport Job Generator service task ended.";
				AssertStartsWith("Logger should have logged start and end messages", normalRunExpectedLogStart, logger.ToString());

				logger.ClearLog();
				var cancelledException = new OperationCanceledException("Operation cancelled", token);
				mockRunner.Setup(r => r.Run(token, logger)).Throws(() => cancelledException);

				var errorRunExpectedLogStart = @"Information|Transport Job Generator service task started.
Error|Transport Job Generator service task ended abruptly.
Exception: System.OperationCanceledException
Exception Message: Operation cancelled.
Stack Trace:";
				AssertNoExceptionThrown("Service task Run() method will catch and handle exception", () => serviceTask.RunTask(token));
				AssertStartsWith("Logger should have logged error message", errorRunExpectedLogStart, logger.ToString());
			}
		}
	}
}
