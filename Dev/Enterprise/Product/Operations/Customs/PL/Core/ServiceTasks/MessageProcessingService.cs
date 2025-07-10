using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.ServiceTasks;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Messaging.Business;
using ServiceManager.Integration.ServiceTasks.CW;
using ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;
using Constants = Enterprise.Customs.PL.ServiceTasks.Constants;
using EDIMessageSchema = Enterprise.ZArchitecture.Schema.EDIMessageSchema.Constants;
using EDIMessageStatus = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes;

[assembly: HostedService(
	code: ServiceTaskCodeList.Codes.MessageProcessor,
	description: ServiceTaskCodeList.Descriptions.MessageProcessor,
	category: ApplicationCode.PLCustoms,
	typeof(MessageProcessingService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Poland,
	MinimumPeriod = "60Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskCodeList.Codes.MessageProcessor,
	table: EDIMessageSchema.TableName,
	predicates:
	[
		$"{EDIMessageSchema.EM_Status}={EDIMessageStatus.Queued}",
		$"{EDIMessageSchema.EM_ReceiveTransmit}={EDIInterchange.Direction.Receive}",
		$"{EDIMessageSchema.EM_IsActive}={Constants.Y}",
		$"{EDIMessageSchema.EM_ApplicationCode}={ApplicationCode.PLCustoms}",
		$"{EDIMessageSchema.EM_HeldUntilDate} IS PASTORNULL"
	],
	queueName: "Polish Customs Message Pre-Processing Import/Export")]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskCodeList.Codes.MessageProcessor,
	table: EDIMessageSchema.TableName,
	predicates:
	[
		$"{EDIMessageSchema.EM_Status}={EDIMessageStatus.Queued}",
		$"{EDIMessageSchema.EM_ReceiveTransmit}={EDIInterchange.Direction.Receive}",
		$"{EDIMessageSchema.EM_IsActive}={Constants.Y}",
		$"{EDIMessageSchema.EM_ApplicationCode}={ApplicationCode.PLCustomsNCTS}",
		EDIMessageSchema.EM_HeldUntilDate + " IS PASTORNULL"
	],
	queueName: "Polish Customs Message Pre-Processing NCTS")]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskCodeList.Codes.MessageProcessor,
	table: EDIMessageSchema.TableName,
	predicates:
	[
		$"{EDIMessageSchema.EM_Status}={EDIMessageStatus.Queued}",
		$"{EDIMessageSchema.EM_ReceiveTransmit}={EDIInterchange.Direction.Receive}",
		$"{EDIMessageSchema.EM_IsActive}={Constants.Y}",
		$"{EDIMessageSchema.EM_ApplicationCode}={ApplicationCode.PLCustomsExitControl}",
		EDIMessageSchema.EM_HeldUntilDate + " IS PASTORNULL"
	],
	queueName: "Polish Customs Message Pre-Processing Exit Control")]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskCodeList.Codes.MessageProcessor,
	table: EDIMessageSchema.TableName,
	predicates:
	[
		$"{EDIMessageSchema.EM_Status}={EDIMessageStatus.PreProcessedOK}",
		$"{EDIMessageSchema.EM_ReceiveTransmit}={EDIInterchange.Direction.Receive}",
		$"{EDIMessageSchema.EM_IsActive}={Constants.Y}",
		$"{EDIMessageSchema.EM_ApplicationCode}={ApplicationCode.PLCustoms}",
		$"{EDIMessageSchema.EM_HeldUntilDate} IS PASTORNULL"
	],
	queueName: "Polish Customs Message Processing Import/Export")]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskCodeList.Codes.MessageProcessor,
	table: EDIMessageSchema.TableName,
	predicates:
	[
		$"{EDIMessageSchema.EM_Status}={EDIMessageStatus.PreProcessedOK}",
		$"{EDIMessageSchema.EM_ReceiveTransmit}={EDIInterchange.Direction.Receive}",
		$"{EDIMessageSchema.EM_IsActive}={Constants.Y}",
		$"{EDIMessageSchema.EM_ApplicationCode}={ApplicationCode.PLCustomsNCTS}",
		$"{EDIMessageSchema.EM_HeldUntilDate} IS PASTORNULL"
	],
	queueName: "Polish Customs Message Processing NCTS")]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskCodeList.Codes.MessageProcessor,
	table: EDIMessageSchema.TableName,
	predicates:
	[
		$"{EDIMessageSchema.EM_Status}={EDIMessageStatus.PreProcessedOK}",
		$"{EDIMessageSchema.EM_ReceiveTransmit}={EDIInterchange.Direction.Receive}",
		$"{EDIMessageSchema.EM_IsActive}={Constants.Y}",
		$"{EDIMessageSchema.EM_ApplicationCode}={ApplicationCode.PLCustomsExitControl}",
		$"{EDIMessageSchema.EM_HeldUntilDate} IS PASTORNULL"
	],
	queueName: "Polish Customs Message Processing Exit Control")]

namespace Enterprise.Customs.PL.ServiceTasks;

public class MessageProcessingService : BranchMessageProcessorService
{
	protected override IEnumerable<ZString> ApplicationCodes => [
		ApplicationCode.PLCustoms,
		ApplicationCode.PLCustomsNCTS,
		ApplicationCode.PLCustomsExitControl,
	];

	protected override IEnumerable<ZString> MessageTypes => [];

	protected override void SetupPreProcessData()
	{
		var factory = new BusinessObjectFactory();
		PLServiceTaskHelper.DeactivateServiceIf_UCMP_IsActive(factory, ServiceTaskCodeList.Codes.MessageProcessor, ServiceLogger);

		base.SetupPreProcessData();
	}

	protected override BranchCustomsMessageProcessor GetNewBranchCustomsMessageProcessor() => new IncomingMessagesProcessingLegacyRouter(new MessageProcessorFactoryLegacyResolver());

	[HostedServiceRequirement]
	public static string IsRequired()
		=> PLServiceTaskHelper.CheckCertificate()
		?? string.Empty;
}
