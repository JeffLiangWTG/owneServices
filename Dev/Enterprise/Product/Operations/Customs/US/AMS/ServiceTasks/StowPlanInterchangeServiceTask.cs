using System.Threading;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService("USP",
	"United States Stow Plan Customs Interchange Messaging",
	"USC",
	typeof(Enterprise.Customs.US.AMS.ServiceTasks.StowPlanInterchangeServiceTask),
	MinimumPeriod = "30Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding("USP",
	EDIInterchangeSchema.Constants.TableName,
	new[]
	{
		EDIInterchangeSchema.Constants.EI_Status          + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive        + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.StowPlan
	},
	"US Customs Stow Plan interchanges inbound"
	)]

namespace Enterprise.Customs.US.AMS.ServiceTasks
{
	public class StowPlanInterchangeServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			using (var inboundInterchangeProcessor = new StowPlanInterchangeProcessor(Logger))
			{
				inboundInterchangeProcessor.ExecuteBatch(token);
			}
		}
	}
}
