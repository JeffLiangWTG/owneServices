using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.ServiceManager.Business;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.NZ.Business
{
	sealed class ServiceTasksHelperTest : TestCaseWithFactory
	{
		public void TestCheckRequiredServiceTasksAreActive() => CombineAssertions(() =>
		{
			var querierMock = new Mock<IServiceManagerQuerier>();
			querierMock.Setup(q => q.CheckStateOfNamedServiceTask("NCS")).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(querierMock.Object))
			using (NZCustomsDataRegistry.Instance.EnableNZServiceTaskCheckForSendingMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				const string serviceTaskInactiveMessage = "Please have your Administrator check the tasks with these codes. NCS: ServiceTaskIsInactive";
				AssertEquals("Service task NSC doesn't exist", string.Empty, ServiceTasksHelper.CheckRequiredServiceTasksAreActive());

				var serviceTask = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				serviceTask.S5_IsActive = false;
				serviceTask.S5_ScheduleType = "NCS";
				serviceTask.S5_TypeOfDocument = "NZC";
				Factory.Save();

				querierMock.Setup(x => x.CheckStateOfNamedServiceTask("NCS")).Returns(ServiceTaskStatus.ServiceTaskIsInactive);

				AssertEquals("Service task NSC not active", serviceTaskInactiveMessage, ServiceTasksHelper.CheckRequiredServiceTasksAreActive());

				serviceTask.S5_IsActive = true;
				Factory.Save();

				querierMock.Setup(x => x.CheckStateOfNamedServiceTask("NCS")).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

				AssertEquals("Service task NSC active", string.Empty, ServiceTasksHelper.CheckRequiredServiceTasksAreActive());
			}

			using (NZCustomsDataRegistry.Instance.EnableNZServiceTaskCheckForSendingMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Service task NSC Warning", string.Empty, ServiceTasksHelper.CheckRequiredServiceTasksAreActive());
			}
		});
	}
}
