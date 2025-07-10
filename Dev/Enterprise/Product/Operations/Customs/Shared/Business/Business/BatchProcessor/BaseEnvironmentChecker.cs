using System.Collections.Generic;
using Enterprise.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Customs.Business.BatchProcessor
{
	public abstract class BaseEnvironmentChecker
	{
		protected BaseEnvironmentChecker()
		{
		}

		public string[] CheckEverythingRequiredToRunIsInPlace()
		{
			var failureDescriptionsList = new List<string>();
			CheckEverythingRequiredToRunIsInPlaceCore(failureDescriptionsList);
			return failureDescriptionsList.ToArray();
		}

		protected virtual void CheckEverythingRequiredToRunIsInPlaceCore(List<string> failureDescriptionsList)
		{
			CheckSMTPSettings(failureDescriptionsList);
			CheckPOP3Settings(failureDescriptionsList);
		}

		protected void CheckSMTPSettings(List<string> failureDescriptionsList)
		{
			AddIfNotNullOrEmpty(HostedServiceRequirementAttribute.CheckValueIsNotNullOrEmptyString(Env.Registry.RawRegistry.SMTPServer), failureDescriptionsList);
			AddIfNotNullOrEmpty(HostedServiceRequirementAttribute.CheckValueIsNotEqualTo(Env.Registry.RawRegistry.SMTPPort, 0), failureDescriptionsList);
		}

		protected void CheckPOP3Settings(List<string> failureDescriptionsList)
		{
			AddIfNotNullOrEmpty(HostedServiceRequirementAttribute.CheckValueIsNotNullOrEmptyString(Env.Registry.RawRegistry.MailServer), failureDescriptionsList);
			AddIfNotNullOrEmpty(HostedServiceRequirementAttribute.CheckValueIsNotEqualTo(Env.Registry.RawRegistry.MailServerPort, 0), failureDescriptionsList);
		}

		protected void AddIfNotNullOrEmpty(string result, List<string> failureDescriptionsList)
		{
			if (!string.IsNullOrEmpty(result))
			{
				failureDescriptionsList.Add(result);
			}
		}
	}
}
