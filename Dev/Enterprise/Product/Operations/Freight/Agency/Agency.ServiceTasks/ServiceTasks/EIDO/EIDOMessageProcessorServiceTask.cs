using Enterprise.Core;
using Enterprise.Freight.Agency.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	EIDOMessageProcessorServiceTask.Code,
	EIDOMessageProcessorServiceTask.Description,
	ServiceTaskConstants.Category,
	typeof(EIDOMessageProcessorServiceTask),
	MinimumPeriod = "30minutes",
	RequiresCompanyInCountry = Constants.CountryCodes.Australia,
	DefaultScheduleRunEvery = "30minutes",
	CanRunInAnyBranch = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	EIDOMessageProcessorServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status            + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit   + "=" + ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive          + "=" + "Y",
		EDIMessageSchema.Constants.EM_ApplicationCode   + "=" + EDIInterchange.ApplicationCodes.EIDO
	}, "EIDO Messages")]

namespace Enterprise.Freight.Agency.ServiceTasks
{
	public sealed class EIDOMessageProcessorServiceTask : ProcessServiceTask
	{
		public const string Code = "EMP";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Assembly attribute, must remain a constant")]
		public const string Description = "E-IDO Message Processor";

		public EIDOMessageProcessorServiceTask()
			: base(new EIDOBaseMessageProcessor()) { }
	}
}
