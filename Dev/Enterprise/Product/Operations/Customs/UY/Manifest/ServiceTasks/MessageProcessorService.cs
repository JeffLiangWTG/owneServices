using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.UY.Manifest.Business;
using Enterprise.Customs.UY.Manifest.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskApplicationCodeList.Codes.UYP,
	ServiceTaskApplicationCodeList.Descriptions.UYP,
	UYMessageConstants.MessageServiceTaskCategory,
	typeof(MessageProcessorService),
	MinimumPeriod = "60Seconds",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Uruguay,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(ServiceTaskApplicationCodeList.Codes.UYP,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UYCustoms
	},
		ServiceTaskApplicationCodeList.Descriptions.UYP)]

namespace Enterprise.Customs.UY.Manifest.ServiceTasks
{
	public class MessageProcessorService : Customs.ServiceTasks.BranchMessageProcessorService
	{
		protected override IEnumerable<ZString> MessageTypes => new ZString[] { Business.MessageTypes.Codes.UYC, Business.MessageTypes.Codes.XER };

		protected override IEnumerable<ZString> ApplicationCodes => new ZString[] { EDIMessage.ApplicationCodes.UYCustoms };

		protected override BranchCustomsMessageProcessor GetNewBranchCustomsMessageProcessor() => new UYBranchMessageProcessor();

		[HostedServiceRequirement]
		public static string CheckUYCompanyHasCredentialsExist() => (MessageHostedServiceRequirement.CheckUYCompanyHasCertificate());
	}
}
