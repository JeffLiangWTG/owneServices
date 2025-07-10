using CargoWise.Application;
using CargoWise.ComponentModel;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public static class ServiceTaskChecker
	{
		public static bool IsServiceTaskActive
		{
			get
			{
				return ObjectFactory.Get<IServiceManagerQuerier>()
					.CheckStateOfNamedServiceTask(FreightConstants.ServiceTask.CargoIMPPhase2) == ServiceTaskStatus.AtLeastOneHostIsRunningHealthily;
			}
		}

		public static bool IsServiceTaskEnvironmentValid(INotifications notifications)
		{
			bool result = true;
			if (ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ServiceProvider.Value == CargoIMPPhase2ServiceProviderList.Codes.Traxon)
			{
				result &= CheckRegistryItemIsNotEmpty(ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress, notifications);
				result &= CheckRegistryItemIsNotEmpty(ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPServer, notifications);
				result &= CheckRegistryItemIsNotEmpty(ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPUserName, notifications);
				result &= CheckRegistryItemIsNotEmpty(ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPPasswordEncrypted, notifications);
			}

			return result;
		}

		public static bool IsServiceTaskEnvironmentValid()
		{
			return IsServiceTaskEnvironmentValid(null);
		}

		static bool CheckRegistryItemIsNotEmpty(StringRegistryItem registryItem, INotifications notifications)
		{
			bool result = true;
			if (string.IsNullOrEmpty(registryItem.Value))
			{
				if (notifications != null)
				{
					notifications.Add(new ErrorNotification(ErrorType.Error, Res.GetString("f83b209d-b6d5-4d34-9a3a-4131e7645163", "'{0}' registry item must be set", registryItem.Caption)));
				}

				result = false;
			}

			return result;
		}
	}
}
