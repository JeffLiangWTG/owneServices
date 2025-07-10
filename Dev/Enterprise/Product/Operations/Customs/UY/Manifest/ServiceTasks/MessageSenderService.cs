using System.Threading;
using Enterprise.Customs.UY.Manifest.Business;
using Enterprise.Customs.UY.Manifest.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskApplicationCodeList.Codes.UYS,
	ServiceTaskApplicationCodeList.Descriptions.UYS,
	UYMessageConstants.MessageServiceTaskCategory,
	typeof(MessageSenderService),
	MinimumPeriod = "1Minute",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Uruguay,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(ServiceTaskApplicationCodeList.Codes.UYS,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UYCustoms,
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_MessageType     + "=" + MessageTypes.Codes.UYC
	},
	ServiceTaskApplicationCodeList.Descriptions.UYS)]

namespace Enterprise.Customs.UY.Manifest.ServiceTasks
{
	public class MessageSenderService : MessagingService
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Uruguay))
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						var log = GetNewLogger();
						new UYCOutgoingMessageProcessor(log).ProcessMessage(token);
					});
				}
			}
		}

		[HostedServiceRequirement]
		public static string CheckUYCompanyHasCredentialsExist() => (MessageHostedServiceRequirement.CheckUYCompanyHasCertificate());
	}
}
