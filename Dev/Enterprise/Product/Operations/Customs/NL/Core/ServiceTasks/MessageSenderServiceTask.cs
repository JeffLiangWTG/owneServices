using System.Threading;
using Enterprise.Customs.NL.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.NL.ServiceTasks.MessageSenderServiceTask.Code,
	Enterprise.Customs.NL.ServiceTasks.MessageSenderServiceTask.FrienldyName,
	Enterprise.Customs.NL.ServiceTasks.MessagingServiceTask.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.NL.ServiceTasks.MessageSenderServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Netherlands,
	MinimumPeriod = "60Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
)]

[assembly: HostedServiceBusinessObjectBinding(
	Enterprise.Customs.NL.ServiceTasks.MessageSenderServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + NLEDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + NLEDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + NLEDIMessage.ApplicationCodes.NLCustoms,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	"NL Customs Message Sender"
)]

namespace Enterprise.Customs.NL.ServiceTasks;

public class MessageSenderServiceTask : MessagingServiceTask
{
	public const string Code = "NLS";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Name")]
	public const string FrienldyName = "NL Customs Message Sender";

	protected override void RunTaskCore(CancellationToken token)
	{
		foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Netherlands))
		{
			token.ThrowIfCancellationRequested();
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				RunTaskHandleEmailSendFailure(() =>
				{
					var log = GetNewLogger();
					new NLOutgoingMessageProcessor(log).ProcessMessage(token);
				});
			}
		}
	}
}
