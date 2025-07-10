using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.TR.Messaging.ServiceTaskApplicationCodeList.Codes.TRP,
	Enterprise.Customs.TR.Messaging.ServiceTaskApplicationCodeList.Descriptions.TRP,
	TRMessageConstants.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.TR.ServiceTasks.MessageProcessorService),
	MinimumPeriod = "60Seconds",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Turkey,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	Enterprise.Customs.TR.Messaging.ServiceTaskApplicationCodeList.Codes.TRP,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.TRCustoms,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	Enterprise.Customs.TR.Messaging.ServiceTaskApplicationCodeList.Descriptions.TRP)]

namespace Enterprise.Customs.TR.ServiceTasks
{
	public class MessageProcessorService : Customs.ServiceTasks.BranchMessageProcessorService
	{
		[HostedServiceRequirement]
		public static string IsRequired() => GlbExternalPasswordHelper.CheckAnyStaffHasCertificate();

		protected override IEnumerable<ZString> MessageTypes => Enumerable.Empty<ZString>();

		protected override IEnumerable<ZString> ApplicationCodes => new ZString[] { EDIMessage.ApplicationCodes.TRCustoms };

		protected override BranchCustomsMessageProcessor GetNewBranchCustomsMessageProcessor() => new TRBranchCustomsMessageProcessor();
	}
}
