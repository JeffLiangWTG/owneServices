using Enterprise.Freight.Agency.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	CMMMessageProcessorServiceTask.Code,
	CMMMessageProcessorServiceTask.Description,
	ServiceTaskConstants.Category,
	typeof(CMMMessageProcessorServiceTask),
	MinimumPeriod = "30minutes",
	DefaultScheduleRunEvery = "30minutes",
	CanRunInAnyBranch = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	CMMMessageProcessorServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status            + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit   + "=" + ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive          + "=" + "Y",
		EDIMessageSchema.Constants.EM_ApplicationCode   + "=" + EDIInterchange.ApplicationCodes.ContainerManagement
	}, "Container Management Messages")]

namespace Enterprise.Freight.Agency.ServiceTasks
{
	public sealed class CMMMessageProcessorServiceTask : ProcessServiceTask
	{
		public const string Code = "CMP";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Assembly attribute, must remain a constant")]
		public const string Description = "Container Management Message Processor";

		public CMMMessageProcessorServiceTask()
			: base(new CMMBaseMessageProcessor()) { }
	}
}
