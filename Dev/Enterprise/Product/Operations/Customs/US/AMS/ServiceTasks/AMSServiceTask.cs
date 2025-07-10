using System.Threading;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService("UAM",
	"United States AMS Customs Messaging",
	"USC",
	typeof(Enterprise.Customs.US.AMS.ServiceTasks.AMSServiceTask),
	MinimumPeriod = "30Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding("UAM",
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.AMS,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	"US Customs AMS messages inbound"
	)]

[assembly: HostedServiceBusinessObjectBinding("UAM",
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.StowPlan,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	"US Customs Stow Plan mesages inbound"
	)]

namespace Enterprise.Customs.US.AMS.ServiceTasks
{
	public class AMSServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			if (!ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.UCMPServiceTaskAMS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Now))
			{
				var branch = GlbBranch.GetFirstActiveBranch();
				if (branch != null)
				{
					using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
					{
						using (var amsAndStowPlanIncomingMessageProcessor = new AMSIncomingMessageProcessor(Logger))
						{
							amsAndStowPlanIncomingMessageProcessor.ExecuteBatch(token);
						}

						using (var stowPlanIncomingMessageProcessor = new StowPlanIncomingMessageProcessor(Logger))
						{
							stowPlanIncomingMessageProcessor.ExecuteBatch(token);
						}
					}
				}
			}
		}
	}
}
