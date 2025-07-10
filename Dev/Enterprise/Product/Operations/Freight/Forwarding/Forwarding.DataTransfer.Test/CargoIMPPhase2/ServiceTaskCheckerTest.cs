using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.ZArchitecture;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ServiceTaskCheckerTest : TestCaseWithFactory
	{
		public void TestIsServiceTaskActive()
		{
			var serviceManagerQuerier = new Mock<IServiceManagerQuerier>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(serviceManagerQuerier.Object))
			{
				serviceManagerQuerier.Setup(s => s.CheckStateOfNamedServiceTask(FreightConstants.ServiceTask.CargoIMPPhase2)).Returns(ServiceTaskStatus.ServiceTaskIsInactive);
				Assert(!ServiceTaskChecker.IsServiceTaskActive);
				serviceManagerQuerier.VerifyAll();

				serviceManagerQuerier.Setup(s => s.CheckStateOfNamedServiceTask(FreightConstants.ServiceTask.CargoIMPPhase2)).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);
				Assert(ServiceTaskChecker.IsServiceTaskActive);
				serviceManagerQuerier.VerifyAll();
			}
		}

		public void TestIsServiceTaskEnvironmentValid()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPPasswordEncrypted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			NotificationBuffer notifications = new NotificationBuffer();
			Assert(!ServiceTaskChecker.IsServiceTaskEnvironmentValid(notifications));
			AssertMultilineASCIIEquals("Notifications", @"Error: 'Sender Identification (PIMA)' registry item must be set
Error: 'FTP Server Address' registry item must be set
Error: 'FTP User Name' registry item must be set
Error: 'FTP Password' registry item must be set", notifications.AsString);
			Assert(!ServiceTaskChecker.IsServiceTaskEnvironmentValid());

			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "user");
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPPasswordEncrypted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "password");
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PIMA");

			notifications.Clear();
			Assert(!ServiceTaskChecker.IsServiceTaskEnvironmentValid(notifications));
			AssertMultilineASCIIEquals("Notifications", "Error: 'FTP Server Address' registry item must be set", notifications.AsString);
			Assert(!ServiceTaskChecker.IsServiceTaskEnvironmentValid());

			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ftp.server.com");
			notifications.Clear();
			Assert(ServiceTaskChecker.IsServiceTaskEnvironmentValid(notifications));
			Assert(!notifications.HasErrors);
			Assert(ServiceTaskChecker.IsServiceTaskEnvironmentValid());
		}
	}
}
