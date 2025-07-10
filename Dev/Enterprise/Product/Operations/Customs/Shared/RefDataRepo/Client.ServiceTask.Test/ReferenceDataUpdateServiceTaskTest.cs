using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace CargoWise.RefDataRepo.Ent.Client.ServiceTask.Test
{
	[TestedType(typeof(ReferenceDataUpdateServiceTask))]
	public class ReferenceDataUpdateServiceTaskTest : ServiceTaskTestCase<ReferenceDataUpdateServiceTask>
	{
		public void TestDoNotRunUntilNextUpdate()
		{
			RefDataRepoRegistry.Instance.NextUpdate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddHours(1));
			var task = new DummyReferenceDataUpdateServiceTask();
			InitialiseTaskSchedule(task);
			task.IsUpdateAllCalled = false;
			RunTaskSchedule(task);
			Assert(!task.IsUpdateAllCalled);

			RefDataRepoRegistry.Instance.NextUpdate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddHours(-1));
			RefDataRepoRegistry.Instance.RefDbRepoServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, @"http://localhost/");
			task.IsUpdateAllCalled = false;
			RunTaskSchedule(task);
			Assert(task.IsUpdateAllCalled);
		}

		public void TestDoNotRunUntilRegistered()
		{
			RefDataRepoRegistry.Instance.NextUpdate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddHours(-1));
			var registry = ObjectFactory.Get<IProductRegistration>();
			registry.KeyForTest.SystemIdForTest = null;
			RefDataRepoRegistry.Instance.ClientId = null;

			var task = new DummyReferenceDataUpdateServiceTask();
			InitialiseTaskSchedule(task);
			task.IsUpdateAllCalled = false;
			RunTaskSchedule(task);
			Assert(!task.IsUpdateAllCalled);

			RefDataRepoRegistry.Instance.NextUpdate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddHours(-1));
			registry.ResetKeyToDefault();
			task.IsUpdateAllCalled = false;
			RunTaskSchedule(task);
			Assert(task.IsUpdateAllCalled);
		}

		[TestDate(2016, 08, 30, 10, 0, 0)]
		public void TestNextUpdateWillBeSetAfterSuccessfulUpdate()
		{
			RefDataRepoRegistry.Instance.RefDbRepoServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, @"http://localhost/");
			RefDataRepoRegistry.Instance.UpdateInterval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 59);
			RefDataRepoRegistry.Instance.NextUpdate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue);
			var task = new DummyReferenceDataUpdateServiceTask();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			AssertEquals(ZDateTime.UtcNow.AddMinutes(59), RefDataRepoRegistry.Instance.NextUpdate.Value);
		}

		[TestDate(2016, 08, 30, 10, 0, 0)]
		public void TestNextUpdateWillBeSetAfterANumberOfContinuousFailure()
		{
			RefDataRepoRegistry.Instance.RefDbRepoServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, @"http://localhost/");
			RefDataRepoRegistry.Instance.UpdateInterval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 59);
			RefDataRepoRegistry.Instance.NextUpdate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue);

			RefDataRepoRegistry.Instance.FailureCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			var task = new DummyReferenceDataUpdateServiceTask(new DummyDataSetUpdaterManagerWrapper { UpdateResult = false, SavedDataSetCount = 1 });
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			AssertEquals(DateTime.MinValue, RefDataRepoRegistry.Instance.NextUpdate.Value);
			AssertEquals(0, RefDataRepoRegistry.Instance.FailureCount.Value);

			RefDataRepoRegistry.Instance.FailureCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			task = new DummyReferenceDataUpdateServiceTask(new DummyDataSetUpdaterManagerWrapper { UpdateResult = false });
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			AssertEquals(ZDateTime.UtcNow.AddMinutes(59), RefDataRepoRegistry.Instance.NextUpdate.Value);
			AssertEquals(0, RefDataRepoRegistry.Instance.FailureCount.Value);
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
