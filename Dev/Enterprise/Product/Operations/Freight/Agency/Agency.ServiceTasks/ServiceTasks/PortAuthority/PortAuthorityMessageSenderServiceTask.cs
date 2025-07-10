using Enterprise.Freight.Agency.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	PortAuthorityMessageSenderServiceTask.Code,
	PortAuthorityMessageSenderServiceTask.Description,
	ServiceTaskConstants.Category,
	typeof(PortAuthorityMessageSenderServiceTask),
	MinimumPeriod = "30minutes",
	DefaultScheduleRunEvery = "30minutes",
	CanRunInAnyBranch = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	PortAuthorityMessageSenderServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status            + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit   + "=" + ReceiveTransmitList.Codes.Transmit,
		EDIMessageSchema.Constants.EM_IsActive          + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode   + "=" + ApplicationCodeList.Codes.PortAuthority
	}, "AU Port Authority outbound")]

namespace Enterprise.Freight.Agency.ServiceTasks
{
	public sealed class PortAuthorityMessageSenderServiceTask : ProcessServiceTask
	{
		public const string Code = "PMS";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "assembly attribute")]
		public const string Description = "Port Authority Message Sender";

		public PortAuthorityMessageSenderServiceTask()
			: base(new PortAuthorityInterchangeSender()) { }
	}
}
