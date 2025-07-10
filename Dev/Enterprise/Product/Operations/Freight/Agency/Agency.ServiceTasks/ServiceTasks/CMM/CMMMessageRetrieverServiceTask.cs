using Enterprise.Freight.Agency.ServiceTasks;
using Enterprise.MailManager;
using Enterprise.MailManager.MailFilters;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	CMMMessageRetrieverServiceTask.Code,
	CMMMessageRetrieverServiceTask.Description,
	ServiceTaskConstants.Category,
	typeof(CMMMessageRetrieverServiceTask),
	MinimumPeriod = "30minutes",
	DefaultScheduleRunEvery = "30minutes",
	CanRunInAnyBranch = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	CMMMessageRetrieverServiceTask.Code,
	MailDBItemsSchema.Constants.TableName,
	new[]
	{
		MailDBItemsSchema.Constants.MI_Status       + "=" + MailStatus.Queued,
		MailDBItemsSchema.Constants.MI_Direction    + "=" + MailDirection.Receive,
		MailDBItemsSchema.Constants.MI_Application  + "=" + MailFilterCodes.CMMMessage
	}, "CMM Message retrieval")]

namespace Enterprise.Freight.Agency.ServiceTasks
{
	public sealed class CMMMessageRetrieverServiceTask : ProcessServiceTask
	{
		public CMMMessageRetrieverServiceTask()
			: base(new CMMMessageRetriever()) { }

		public const string Code = "CMR";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Assembly attribute, must remain a constant")]
		public const string Description = "Container Management Message Retriever";
	}
}
