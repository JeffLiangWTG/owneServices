using System.Threading;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.MailFilters;
using Enterprise.Recruitment.Registry;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Recruitment.ServiceTasks.Emailing.MiddleManServiceTask.Code,
	"Human Resources Email Forwarding Service Task",
	"HRM",
	typeof(Enterprise.Recruitment.ServiceTasks.Emailing.MiddleManServiceTask),
	MinimumPeriod = "1minute",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "5minutes",
	ActiveByDefault = true
	)]

[assembly: MailSubscriber(typeof(Enterprise.Recruitment.ServiceTasks.Emailing.MiddleManServiceTask))]
namespace Enterprise.Recruitment.ServiceTasks.Emailing
{
	public class MiddleManServiceTask : ServiceProviderImpl
	{
		public const string Code = "MMT";

		NotificationBuffer Buffer;

		[HostedServiceRequirement]
		public static string CheckRecruitmentModuleIsEnabled()
		{
			if (RecruitmentDataRegistry.Instance.RecruitmentModuleEnabled.Value)
			{
				return string.Empty;
			}

			return (NoResString)"Recruitment module is disabled. This task will not run.";
		}

		[HostedServiceRequirement]
		public static string CheckMiddleManForwardingAddressIsSpecified()
		{
			if (string.IsNullOrEmpty(RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.Value))
			{
				return (NoResString)"No middle-man address specified. This task will not run.";
			}

			return string.Empty;
		}

		[HostedServiceRequirement]
		public static string CheckReceivedMailDocType()
		{
			if (string.IsNullOrEmpty(RecruitmentDataRegistry.Instance.ReceivedMailDocType.Value))
			{
				return (NoResString)"No document type to attach. This task will not run.";
			}

			return string.Empty;
		}

		public override void RunTask(CancellationToken token)
		{
			using (Env.Instance.SuspendBranchAccessError())
			{
				Buffer = new NotificationBuffer(ServiceLogger.GetTaskNotificationSubscriber());

				var processor = new MailBatchProcessor(mailFilter, (mail, p) =>
				{
					if (EmailForwarderUtilities.ProcessEmail(mail, ServiceLogger))
					{
						Buffer.Notify(new InfoNotification((NoResString)"Finished processing email from " + mail.MI_From));
						return MailProcessingResult.Delete;
					}
					else
					{
						Buffer.Notify(new ErrorNotification(ErrorType.Error, (NoResString)"Failed processing email from " + mail.MI_From));
						mail.MI_LastAttemptDateTime = ZDateTime.Now.ToDateTime();
						mail.Factory.Save();
						return MailProcessingResult.MarkFailed;
					}
				});

				processor.Process(Buffer, token);
			}
		}

		readonly IMailFilter mailFilter = CreateMailFilter();

		[MailFilter(Code)]
		public static IMailFilter CreateMailFilter()
			=> new MiddleManMailFilter(RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.Value);
	}
}
