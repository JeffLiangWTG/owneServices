using System.Threading;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService("UAI",
	"United States AMS Customs Interchange Messaging",
	"USC",
	typeof(Enterprise.Customs.US.AMS.ServiceTasks.AMSInterchangeServiceTask),
	MinimumPeriod = "30Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding("UAI",
	EDIInterchangeSchema.Constants.TableName,
	new[]
	{
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "="  + EDIInterchange.ApplicationCodes.AMS
	},
	"US Customs AMS interchanges inbound"
	)]

namespace Enterprise.Customs.US.AMS.ServiceTasks
{
	public class AMSInterchangeServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			using (var inboundInterchangeProcessor = new AMSInboundInterchangeProcessor(Logger))
			{
				inboundInterchangeProcessor.ExecuteBatch(token);
			}
		}
	}
}
