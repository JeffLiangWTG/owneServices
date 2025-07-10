using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Customs.PL.Business.Constants;
using ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;
using Constants = Enterprise.Customs.PL.ServiceTasks.Constants;
using CountryCodes = Enterprise.Core.Constants.CountryCodes;
using EDIMessageSchema = Enterprise.ZArchitecture.Schema.EDIMessageSchema.Constants;

[assembly: HostedService(
	code: ServiceTaskCodeList.Codes.MessageSender,
	description: ServiceTaskCodeList.Descriptions.MessageSender,
	category: ApplicationCode.PLCustoms,
	type: typeof(MessageSendingService),
	RequiresCompanyInCountry = CountryCodes.Poland,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskCodeList.Codes.MessageSender,
	table: EDIMessageSchema.TableName,
	predicates:
	[
		$"{EDIMessageSchema.EM_IsActive}={Constants.Y}",
		$"{EDIMessageSchema.EM_HeldUntilDate} IS PASTORNULL",
		$"{EDIMessageSchema.EM_ApplicationCode}={ApplicationCode.PLCustoms}",
		$"{EDIMessageSchema.EM_MessageType}={EUJobMessageTypeList.Codes.Import}",
		$"{EDIMessageSchema.EM_ReceiveTransmit}={EDIInterchange.Direction.Transmit}",
		$"{EDIMessageSchema.EM_Status}={EDIMessageStatusList.Codes.Queued}"
	],
	queueName: $"{ServiceTaskCodeList.Descriptions.MessageSender} - Import")]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskCodeList.Codes.MessageSender,
	table: EDIMessageSchema.TableName,
	predicates:
	[
		$"{EDIMessageSchema.EM_IsActive}={Constants.Y}",
		$"{EDIMessageSchema.EM_HeldUntilDate} IS PASTORNULL",
		$"{EDIMessageSchema.EM_ApplicationCode}={ApplicationCode.PLCustoms}",
		$"{EDIMessageSchema.EM_MessageType}={EUJobMessageTypeList.Codes.Export}",
		$"{EDIMessageSchema.EM_ReceiveTransmit}={EDIInterchange.Direction.Transmit}",
		$"{EDIMessageSchema.EM_Status}={EDIMessageStatusList.Codes.Queued}"
	],
	queueName: $"{ServiceTaskCodeList.Descriptions.MessageSender} - Export")]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskCodeList.Codes.MessageSender,
	table: EDIMessageSchema.TableName,
	predicates:
	[
		$"{EDIMessageSchema.EM_IsActive}={Constants.Y}",
		$"{EDIMessageSchema.EM_HeldUntilDate} IS PASTORNULL",
		$"{EDIMessageSchema.EM_ApplicationCode}={ApplicationCode.PLCustoms}",
		$"{EDIMessageSchema.EM_MessageType}={EdiMessageMessageType.CusPollingTransaction}",
		$"{EDIMessageSchema.EM_ReceiveTransmit}={EDIInterchange.Direction.Transmit}",
		$"{EDIMessageSchema.EM_Status}={EDIMessageStatusList.Codes.Queued}"
	],
	queueName: $"{ServiceTaskCodeList.Descriptions.MessageSender} - CusPollingTransaction")]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskCodeList.Codes.MessageSender,
	table: EDIMessageSchema.TableName,
	predicates:
	[
		$"{EDIMessageSchema.EM_IsActive}={Constants.Y}",
		$"{EDIMessageSchema.EM_HeldUntilDate} IS PASTORNULL",
		$"{EDIMessageSchema.EM_ApplicationCode}={ApplicationCode.PLCustomsNCTS}",
		$"{EDIMessageSchema.EM_MessageType}={EUJobMessageTypeList.Codes.NctsArrivalNotification}",
		$"{EDIMessageSchema.EM_ReceiveTransmit}={EDIInterchange.Direction.Transmit}",
		$"{EDIMessageSchema.EM_Status}={EDIMessageStatusList.Codes.Queued}"
	],
	queueName: $"{ServiceTaskCodeList.Descriptions.MessageSender} - Arrival")]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskCodeList.Codes.MessageSender,
	table: EDIMessageSchema.TableName,
	predicates:
	[
		$"{EDIMessageSchema.EM_IsActive}={Constants.Y}",
		$"{EDIMessageSchema.EM_HeldUntilDate} IS PASTORNULL",
		$"{EDIMessageSchema.EM_ApplicationCode}={ApplicationCode.PLCustomsNCTS}",
		$"{EDIMessageSchema.EM_MessageType}={EUJobMessageTypeList.Codes.NctsDeparture}",
		$"{EDIMessageSchema.EM_ReceiveTransmit}={EDIInterchange.Direction.Transmit}",
		$"{EDIMessageSchema.EM_Status}={EDIMessageStatusList.Codes.Queued}"
	],
	queueName: $"{ServiceTaskCodeList.Descriptions.MessageSender} - Departure")]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskCodeList.Codes.MessageSender,
	table: EDIMessageSchema.TableName,
	predicates:
	[
		$"{EDIMessageSchema.EM_IsActive}={Constants.Y}",
		$"{EDIMessageSchema.EM_HeldUntilDate} IS PASTORNULL",
		$"{EDIMessageSchema.EM_ApplicationCode}={ApplicationCode.PLCustomsExitControl}",
		$"{EDIMessageSchema.EM_ReceiveTransmit}={EDIInterchange.Direction.Transmit}",
		$"{EDIMessageSchema.EM_Status}={EDIMessageStatusList.Codes.Queued}"
	],
	queueName: $"{ServiceTaskCodeList.Descriptions.MessageSender} - Exit Control")]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskCodeList.Codes.MessageSender,
	table: EDIMessageSchema.TableName,
	predicates:
	[
		$"{EDIMessageSchema.EM_IsActive}={Constants.Y}",
		$"{EDIMessageSchema.EM_HeldUntilDate} IS PASTORNULL",
		$"{EDIMessageSchema.EM_ApplicationCode}={ApplicationCode.PLCustomsPUESCEmailSystem}",
		$"{EDIMessageSchema.EM_MessageType}={EdiMessageMessageType.Attachment}",
		$"{EDIMessageSchema.EM_ReceiveTransmit}={EDIInterchange.Direction.Transmit}",
		$"{EDIMessageSchema.EM_Status}={EDIMessageStatusList.Codes.Queued}"
	],
	queueName: $"{ServiceTaskCodeList.Descriptions.MessageSender} - Attachment")]

namespace Enterprise.Customs.PL.ServiceTasks;

public class MessageSendingService : Customs.ServiceTasks.CustomsServiceTask
{
	protected override void RunTaskCore(CancellationToken token)
	{
		var factory = new BusinessObjectFactory();
		PLServiceTaskHelper.DeactivateServiceIf_UCMP_IsActive(factory, ServiceTaskCodeList.Codes.MessageSender, ServiceLogger);

		foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(CountryCodes.Poland, factory))
		{
			token.ThrowIfCancellationRequested();
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				new Business.PLOutboundMessageProcessorLegacy(Logger).ProcessMessage(token);
			}
		}
	}

	[HostedServiceRequirement]
	public static string IsRequired()
		=> PLServiceTaskHelper.CheckCertificate()
		?? string.Empty;
}
