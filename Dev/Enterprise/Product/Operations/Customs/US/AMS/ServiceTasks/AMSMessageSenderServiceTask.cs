using System.Threading;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService("UAS",
	"United States AMS Customs Message Sender",
	"USC",
	typeof(Enterprise.Customs.US.AMS.ServiceTasks.AMSMessageSenderServiceTask),
	MinimumPeriod = "30Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding("UAS",
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.AMS,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	"US Customs AMS messages outbound")]

[assembly: HostedServiceBusinessObjectBinding("UAS",
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.StowPlan,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	"US Customs Stow Plan messages outbound")]

namespace Enterprise.Customs.US.AMS.ServiceTasks
{
	public class AMSMessageSenderServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			var branch = GlbBranch.GetFirstActiveBranch();
			if (branch != null)
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					AMSOutgoingMessageProcessor.ProcessMessage(token);
					StowPlanOutgoingProcessor.ProcessMessage(token);
				}
			}
		}

		AMSOutgoingMessageProcessor AMSOutgoingMessageProcessor
		{
			get { return fAMSOutgoingMessageProcessor ?? (fAMSOutgoingMessageProcessor = new AMSOutgoingMessageProcessor(Logger)); }
		}
		AMSOutgoingMessageProcessor fAMSOutgoingMessageProcessor;

		StowPlanOutgoingMessageProcessor StowPlanOutgoingProcessor
		{
			get { return fStowPlanOutgoingProcessor ?? (fStowPlanOutgoingProcessor = new StowPlanOutgoingMessageProcessor(Logger)); }
		}
		StowPlanOutgoingMessageProcessor fStowPlanOutgoingProcessor;
	}
}
