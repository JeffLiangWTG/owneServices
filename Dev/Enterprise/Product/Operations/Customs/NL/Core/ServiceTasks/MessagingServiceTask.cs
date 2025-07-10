using System;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.NL.ServiceTasks;

public abstract class MessagingServiceTask : Customs.ServiceTasks.CustomsServiceTask
{
	public const string MessageServiceTaskCategory = "NLC";

	protected LoggingInformation GetNewLogger()
	{
		LoggingInformation result = new LoggingInformation();
		result.OnLogInfoAdded += new LoggingInformation.LogInfoAdded(Logger_OnLogInfoAdded);
		return result;
	}

	[HostedServiceRequirement]
	public static string IsRequired()
	{
		var required = false;
		var nlCompanies = GlbCompany.GetActiveCompanies(CountryCodes.Netherlands, new BusinessObjectFactory());
		foreach (var company in nlCompanies)
		{
			var registryValue = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var submissionType = registryValue.SubmissionType;
			if (!submissionType.IsEmpty && !submissionType.EqualsIgnoringCase(DeclarationApplicationCodeList.Codes.Interfaced))
			{
				required = true;
				break;
			}
		}
		return required ? string.Empty : (NoResString)"There is no BLT configured in the Registry.";
	}

	void Logger_OnLogInfoAdded(string log, LogType logType)
	{
		ServiceLogger.Log(logType, log.Trim());
	}
}
