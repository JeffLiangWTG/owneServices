using System.Threading;
using Enterprise.MailManager;
using Enterprise.MailManager.MailFilters;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"SCR",
	"Cargo Manifest Declaration (SG) Receiver",
	"SCM",
	typeof(Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage.CMDReceiverServiceTask),
	MaximumPeriod = "15minutes",
	RequiresCompanyInCountry = "SG",
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Minute",
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(
	"SCR",
	MailDBItemsSchema.Constants.TableName,
	new[]
	{
		MailDBItemsSchema.Constants.MI_Status + "=" + MailStatus.Queued,
		MailDBItemsSchema.Constants.MI_Direction + "=" + MailDirection.Receive,
		MailDBItemsSchema.Constants.MI_Application + "=" + MailFilterCodes.CMDMessage,
	},
	"Cargo Manifest Declaration (SG)"
)]

namespace Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage
{
	internal class CMDReceiverServiceTask : CMDServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			var receiver = new CMDMessageReceiver();
			try
			{
				receiver.Logger.OnLogInfoAdded += Log;
				receiver.Process();
			}
			finally
			{
				receiver.Logger.OnLogInfoAdded -= Log;
			}
		}
	}
}
