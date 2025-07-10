using System.Threading;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"SCS",
	"Cargo Manifest Declaration (SG) Sender",
	"SCM",
	typeof(Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage.CMDSenderServiceTask),
	MaximumPeriod = "15minutes",
	RequiresCompanyInCountry = "SG",
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Minute",
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding("SCS",
	EDIInterchangeSchema.Constants.TableName,
	new[]
	{
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.SingaporeCMD,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Transmit,
		EDIInterchangeSchema.Constants.EI_IsActive        + "=Y",
		EDIInterchangeSchema.Constants.EI_Status          + "=" + EDIInterchange.Status.Queued,
	},
	"Cargo Manifest Declaration (SG) interchanges outbound")]
namespace Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage
{
	internal class CMDSenderServiceTask : CMDServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Singapore))
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						new CMDMessageSender(ServiceLogger).Process(token);
					});
				}
			}
		}
	}
}
