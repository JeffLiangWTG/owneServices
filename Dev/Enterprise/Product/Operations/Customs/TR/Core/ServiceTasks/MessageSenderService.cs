using System.Threading;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskApplicationCodeList.Codes.TRS,
	ServiceTaskApplicationCodeList.Descriptions.TRS,
	TRMessageConstants.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.TR.ServiceTasks.MessageSenderService),
	MinimumPeriod = "1Minute",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Turkey,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	ServiceTaskApplicationCodeList.Codes.TRS,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.TRCustoms,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	ServiceTaskApplicationCodeList.Descriptions.TRS)]

namespace Enterprise.Customs.TR.ServiceTasks
{
	public class MessageSenderService : MessagingService
	{
		[HostedServiceRequirement]
		public static string IsRequired() => GlbExternalPasswordHelper.CheckAnyStaffHasCertificate();

		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Turkey))
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						var log = GetNewLogger();
						new TRCOutgoingMessageProcessor(log).ProcessMessage(token);
					});
				}
			}
		}
	}
}
