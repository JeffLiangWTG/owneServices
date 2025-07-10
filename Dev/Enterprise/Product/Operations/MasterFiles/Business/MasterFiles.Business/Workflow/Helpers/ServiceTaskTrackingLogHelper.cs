using CargoWise.Application;
using CargoWise.Workflow;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	static class ServiceTaskTrackingLogHelper
	{
		public static void AddServiceTaskDetails(EventLogReferenceBuilder refBuilder)
		{
			var serviceTaskCode = Env.Instance.ServiceTaskCode;
			if (!string.IsNullOrEmpty(serviceTaskCode))
			{
				refBuilder.AddMandatory(Constants.ServiceTaskCode, serviceTaskCode);

				if (serviceTaskCode == "LWK")
				{
					var subscriberName = ObjectFactory.Get<ICurrentLogSubscriberTracker>().CurrentName;
					if (subscriberName != null)
					{
						refBuilder.AddMandatory(Constants.LogSubscriberNameCode, subscriberName);
					}
				}
			}
		}

		static class Constants
		{
			public const string ServiceTaskCode = "SRV";
			public const string LogSubscriberNameCode = "SUB";
		}
	}
}
