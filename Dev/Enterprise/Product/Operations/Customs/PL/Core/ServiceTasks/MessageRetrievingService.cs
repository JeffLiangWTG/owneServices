using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.ServiceTasks;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using ServiceManager.Integration.ServiceTasks.CW;
using ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;
using Constants = Enterprise.Customs.PL.ServiceTasks.Constants;
using EDIInterchangeSchema = Enterprise.ZArchitecture.Schema.EDIInterchangeSchema.Constants;
using InboundInterchangeProcessorLegacy = Enterprise.Customs.PL.Business.InboundInterchangeProcessorLegacy;

[assembly: HostedService(
	code: ServiceTaskCodeList.Codes.MessageRetriever,
	description: ServiceTaskCodeList.Descriptions.MessageRetriever,
	category: ApplicationCode.PLCustoms,
	type: typeof(MessageRetrievingService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Poland,
	CanRunInAnyBranch = true,
	MinimumPeriod = "60Seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskCodeList.Codes.MessageRetriever,
	table: EDIInterchangeSchema.TableName,
	predicates:
	[
		$"{EDIInterchangeSchema.EI_Status}={EDIInterchangeStatusList.Codes.Queued}",
		$"{EDIInterchangeSchema.EI_ReceiveTransmit}={EDIInterchange.Direction.Receive}",
		$"{EDIInterchangeSchema.EI_IsActive}={Constants.Y}",
		$"{EDIInterchangeSchema.EI_ApplicationCode}={ApplicationCode.PLCustoms}"
	],
	queueName: $"{ServiceTaskCodeList.Descriptions.MessageRetriever} Import/Export")]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskCodeList.Codes.MessageRetriever,
	table: EDIInterchangeSchema.TableName,
	predicates:
	[
		$"{EDIInterchangeSchema.EI_Status}={EDIInterchangeStatusList.Codes.Queued}",
		$"{EDIInterchangeSchema.EI_ReceiveTransmit}={EDIInterchange.Direction.Receive}",
		$"{EDIInterchangeSchema.EI_IsActive}={Constants.Y}",
		$"{EDIInterchangeSchema.EI_ApplicationCode}={ApplicationCode.PLCustomsNCTS}"
	],
	queueName: $"{ServiceTaskCodeList.Descriptions.MessageRetriever} NCTS")]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskCodeList.Codes.MessageRetriever,
	table: EDIInterchangeSchema.TableName,
	predicates:
	[
		$"{EDIInterchangeSchema.EI_Status}={EDIInterchangeStatusList.Codes.Queued}",
		$"{EDIInterchangeSchema.EI_ReceiveTransmit}={EDIInterchange.Direction.Receive}",
		$"{EDIInterchangeSchema.EI_IsActive}={Constants.Y}",
		$"{EDIInterchangeSchema.EI_ApplicationCode}={ApplicationCode.PLCustomsExitControl}"
	],
	queueName: $"{ServiceTaskCodeList.Descriptions.MessageRetriever} Exit Control")]

namespace Enterprise.Customs.PL.ServiceTasks;

public class MessageRetrievingService : BranchInterchangeProcessorService
{
	protected override IEnumerable<string> ApplicationCodes => InboundInterchangeProcessorLegacy.SupportedApplicationCodes;

	protected override void SetupPreProcessData()
	{
		var factory = new BusinessObjectFactory();
		PLServiceTaskHelper.DeactivateServiceIf_UCMP_IsActive(factory, ServiceTaskCodeList.Codes.MessageRetriever, ServiceLogger);

		base.SetupPreProcessData();
	}

	protected override BranchInboundInterchangeProcessor GetNewBranchInboundInterchangeProcessor()
		=> new InboundInterchangeProcessorLegacy();

	[HostedServiceRequirement]
	public static string IsRequired()
		=> PLServiceTaskHelper.CheckCertificate()
		?? string.Empty;
}
