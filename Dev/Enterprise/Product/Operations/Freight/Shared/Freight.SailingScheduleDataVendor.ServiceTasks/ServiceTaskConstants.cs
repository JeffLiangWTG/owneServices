using Enterprise.Registry.Business;

namespace Enterprise.Freight.SailingScheduleDataVendor.ServiceTasks
{
	internal static class ServiceTaskConstants
	{
		public static string ContainerEventSubscriptionEmailAddress
		{
			get
			{
				//#if DEBUG
				if (FreightDataRegistry.Instance.ComTracTestMode.Value)
				{
					return "stop20@test.1-stop.biz";
				}
				//#endif
				return "alerts@edi.1-stop.biz";
			}
		}

		public const string ContainerEventSubscriptionEmailSubjectPrefix = "Container notification request ";
	}
}
