using CargoWise.Application;
using Enterprise.Customs.NZ.Registry;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.NZ.Business
{
	static class ServiceTasksHelper
	{
		public static string CheckRequiredServiceTasksAreActive()
		{
			var result = string.Empty;
			if (NZCustomsDataRegistry.Instance.EnableNZServiceTaskCheckForSendingMessage.Value)
			{
				if (ObjectFactory.Get<IServiceManagerQuerier>().CheckStateOfNamedServiceTask("NCS") <= ServiceTaskStatus.ServiceTaskIsInactive)
				{
					result = "Please have your Administrator check the tasks with these codes. NCS: ServiceTaskIsInactive";
				}
			}
			return result;
		}
	}
}
