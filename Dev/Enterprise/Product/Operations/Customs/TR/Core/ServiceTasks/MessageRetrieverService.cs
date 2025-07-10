using System.Collections.Generic;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskApplicationCodeList.Codes.TRR,
	ServiceTaskApplicationCodeList.Descriptions.TRR,
	TRMessageConstants.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.TR.ServiceTasks.MessageRetrieverService),
	MinimumPeriod = "60Seconds",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Turkey,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(ServiceTaskApplicationCodeList.Codes.TRR,
	EDIInterchangeSchema.Constants.TableName,
	new[] {
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + ApplicationCodeList.Codes.TRCustoms
	},
	"TR Customs Message Retriever Service"
	)]

namespace Enterprise.Customs.TR.ServiceTasks
{
	public class MessageRetrieverService : Customs.ServiceTasks.BranchInterchangeProcessorService
	{
		[HostedServiceRequirement]
		public static string IsRequired() => GlbExternalPasswordHelper.CheckAnyStaffHasCertificate();

		protected override IEnumerable<string> ApplicationCodes => new string[] { ApplicationCodeList.Codes.TRCustoms };

		protected override BranchInboundInterchangeProcessor GetNewBranchInboundInterchangeProcessor() => new TRInboundInterchangeProcessor(ApplicationCodes);
	}
}
