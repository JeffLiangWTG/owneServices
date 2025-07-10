using System.Threading;
using Enterprise.Customs.US.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.USABIInbound,
	Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Descriptions.USABIInbound,
	"USC",
	typeof(Enterprise.Customs.US.ServiceTasks.US1ServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "30Seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.USABIInbound,
	EDIInterchangeSchema.Constants.TableName,
	new[]
	{
		EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.USCustomsImport
	}, "US Customs Interchanges Inbound")]

namespace Enterprise.Customs.US.ServiceTasks
{
	public class US1ServiceTask : Customs.ServiceTasks.NudgeCustomsServiceTask
	{
		protected override string CurrentServiceTaskCode => Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.USABIInbound;
		protected override void RunMainTask(CancellationToken token)
		{
			IInboundInterchangeProcessor processor = new ABIInboundInterchangeProcessor(Logger);
			processor.Execute(token);
		}
	}
}
