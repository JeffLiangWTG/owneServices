using Enterprise.Core;
using Enterprise.Freight.Agency.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	EIDOMessageSenderServiceTask.Code,
	EIDOMessageSenderServiceTask.Description,
	ServiceTaskConstants.Category,
	typeof(EIDOMessageSenderServiceTask),
	MinimumPeriod = "30minutes",
	RequiresCompanyInCountry = Constants.CountryCodes.Australia,
	DefaultScheduleRunEvery = "30minutes",
	CanRunInAnyBranch = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	EIDOMessageSenderServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status            + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit   + "=" + ReceiveTransmitList.Codes.Transmit,
		EDIMessageSchema.Constants.EM_IsActive          + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode   + "=" + ApplicationCodeList.Codes.EIDO
	}, "EIDO Message Send")]

namespace Enterprise.Freight.Agency.ServiceTasks
{
	public sealed class EIDOMessageSenderServiceTask : ProcessServiceTask
	{
		public const string Code = "EMS";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "assembly attribute")]
		public const string Description = "E-IDO Message Sender";

		public EIDOMessageSenderServiceTask()
			: base(new EIDOInterchangeSender()) { }
	}
}
