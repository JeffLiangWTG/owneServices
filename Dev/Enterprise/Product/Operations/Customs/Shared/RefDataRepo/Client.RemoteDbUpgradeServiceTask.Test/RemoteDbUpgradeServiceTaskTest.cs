using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.RefDbRepo.Client.Common.ErrorReporting;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace CargoWise.RefDataRepo.Ent.Client.RemoteDbUpgradeServiceTask.Test
{
	[TestedType(typeof(ReferenceDataRemoteDbUpgradeServiceTask))]
	public class ReferenceDataRemoteDbUpgradeServiceTaskTest : ServiceTaskTestCase<ReferenceDataRemoteDbUpgradeServiceTask>
	{
		[TestDate(2016, 08, 30, 10, 0, 0)]
		public void TestNextUpdateWillBeSetAfterANumberOfContinuousFailure()
		{
			RemoteDatabaseRegistry.Instance.SingleRefDatabaseName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123");
			RemoteDatabaseRegistry.Instance.RemoteDatabaseServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, @"http://localhost/");
			RemoteDatabaseRegistry.Instance.UpgradeInterval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 59);
			RemoteDatabaseRegistry.Instance.NextUpgrade.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue);

			var logger = new Mock<Enterprise.Integration.ILogger>();
			var errorReporter = new Mock<IErrorReportingClientWrapper>();
			RemoteDatabaseRegistry.Instance.FailureCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			var task = new DummyReferenceDataRemoteDbUpgradeServiceTask(logger.Object, errorReporter.Object);
			task.SavedDataSetCount = 4;
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			AssertEquals(DateTime.MinValue, RemoteDatabaseRegistry.Instance.NextUpgrade.Value);
			AssertEquals(0, RemoteDatabaseRegistry.Instance.FailureCount.Value);

			RemoteDatabaseRegistry.Instance.FailureCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			task = new DummyReferenceDataRemoteDbUpgradeServiceTask(logger.Object, errorReporter.Object);
			task.SavedDataSetCount = 0;
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			AssertEquals(ZDateTime.UtcNow.AddMinutes(59), RemoteDatabaseRegistry.Instance.NextUpgrade.Value);
		}

		public void TestDoNotRunUntilNextUpdate()
		{
			var logger = new Mock<Enterprise.Integration.ILogger>();
			var errorReporter = new Mock<IErrorReportingClientWrapper>();
			RemoteDatabaseRegistry.Instance.NextUpgrade.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddHours(1));
			var task = new DummyReferenceDataRemoteDbUpgradeServiceTask(logger.Object, errorReporter.Object);
			InitialiseTaskSchedule(task);
			task.IsUpdateCalled = false;
			RunTaskSchedule(task);
			Assert(!task.IsUpdateCalled);

			RemoteDatabaseRegistry.Instance.SingleRefDatabaseName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123");
			RemoteDatabaseRegistry.Instance.NextUpgrade.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddHours(-1));
			RemoteDatabaseRegistry.Instance.RemoteDatabaseServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, @"http://localhost/");
			task.IsUpdateCalled = false;
			RunTaskSchedule(task);
			Assert(task.IsUpdateCalled);
		}

		public void TestDoNotRunUntilRegistered()
		{
			RemoteDatabaseRegistry.Instance.NextUpgrade.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddHours(-1));
			var registry = ObjectFactory.Get<IProductRegistration>();
			registry.KeyForTest.SystemIdForTest = null;
			RemoteDatabaseRegistry.Instance.ClientId = null;

			var logger = new Mock<Enterprise.Integration.ILogger>();
			var errorReporter = new Mock<IErrorReportingClientWrapper>();
			var task = new DummyReferenceDataRemoteDbUpgradeServiceTask(logger.Object, errorReporter.Object);
			InitialiseTaskSchedule(task);
			task.IsUpdateCalled = false;
			RunTaskSchedule(task);
			Assert(!task.IsUpdateCalled);

			registry.ResetKeyToDefault();
			task.IsUpdateCalled = false;
			RunTaskSchedule(task);
			Assert(task.IsUpdateCalled);
		}

		public void TestMandatoryConfiguration()
		{
			var attrib = GetHostedServiceAttributes().OfType<HostedServiceAttribute>().SingleOrDefault();
			AssertNotNull(attrib);
			Assert("Service must be configured as Mandatory", attrib.IsMandatory);
			AssertEquals("Should be able to run in any branch since any uses of CurrentBranch and CurrentDepartment will cause inconsistent behaviour, so should be carefully analysed.", true, attrib.CanRunInAnyBranch);
		}

		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
