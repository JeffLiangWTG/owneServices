using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedService(
	Enterprise.Customs.NL.ServiceTasks.MessageProcessorServiceTask.Code,
	Enterprise.Customs.NL.ServiceTasks.MessageProcessorServiceTask.FriendlyName,
	Enterprise.Customs.NL.ServiceTasks.MessagingServiceTask.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.NL.ServiceTasks.MessageProcessorServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Netherlands,
	MinimumPeriod = "60Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.NL.ServiceTasks.MessageProcessorServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_Status + "=" + Enterprise.Messaging.Business.EDIMessage.Status.Queued,
				 EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
				 EDIMessageSchema.Constants.EM_IsActive + "=Y",
				 EDIMessageSchema.Constants.EM_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.NLCustoms,
				 EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL" },
	"NL Customs Response Messages"
	)]

namespace Enterprise.Customs.NL.ServiceTasks;

public class MessageProcessorServiceTask : BranchMessageProcessorService
{
	public const string Code = "NLP";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task name")]
	public const string FriendlyName = "NL Customs Message Processor";
	protected override IEnumerable<ZString> MessageTypes => Enumerable.Empty<ZString>();

	protected override IEnumerable<ZString> ApplicationCodes => new ZString[] { EDIMessage.ApplicationCodes.NLCustoms };

	protected override BranchCustomsMessageProcessor GetNewBranchCustomsMessageProcessor() => new NLCBranchCustomsMessageProcessor();

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
}
