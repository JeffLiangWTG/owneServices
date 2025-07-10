using System.Collections.Generic;
using Enterprise.Customs.UY.Manifest.Business;
using Enterprise.Customs.UY.Manifest.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskApplicationCodeList.Codes.UYR,
	ServiceTaskApplicationCodeList.Descriptions.UYR,
	UYMessageConstants.MessageServiceTaskCategory,
	typeof(MessageRetrieverService),
	MinimumPeriod = "60Seconds",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Uruguay,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(ServiceTaskApplicationCodeList.Codes.UYR,
	EDIInterchangeSchema.Constants.TableName,
	new[]
	{
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchangeStatusList.Codes.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.UYCustoms
	},
		ServiceTaskApplicationCodeList.Descriptions.UYR)]

namespace Enterprise.Customs.UY.Manifest.ServiceTasks
{
	public class MessageRetrieverService : Customs.ServiceTasks.BranchInterchangeProcessorService
	{
		protected override IEnumerable<string> ApplicationCodes => new string[] { ApplicationCodeList.Codes.UYCustoms };

		protected override BranchInboundInterchangeProcessor GetNewBranchInboundInterchangeProcessor() => new UYCInboundInterchangeProcessor(ApplicationCodes);

		[HostedServiceRequirement]
		public static string CheckUYCompanyHasCredentialsExist() => (MessageHostedServiceRequirement.CheckUYCompanyHasCertificate());
	}
}
