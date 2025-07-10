using Enterprise.Core;
using Enterprise.Freight.Agency.ServiceTasks;
using Enterprise.MailManager;
using Enterprise.MailManager.MailFilters;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	EIDOMessageRetrieverServiceTask.Code,
	EIDOMessageRetrieverServiceTask.Description,
	ServiceTaskConstants.Category,
	typeof(EIDOMessageRetrieverServiceTask),
	MinimumPeriod = "30minutes",
	RequiresCompanyInCountry = Constants.CountryCodes.Australia,
	DefaultScheduleRunEvery = "30minutes",
	CanRunInAnyBranch = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	EIDOMessageRetrieverServiceTask.Code,
	MailDBItemsSchema.Constants.TableName,
	new[]
	{
		MailDBItemsSchema.Constants.MI_Status       + "=" + MailStatus.Queued,
		MailDBItemsSchema.Constants.MI_Direction    + "=" + MailDirection.Receive,
		MailDBItemsSchema.Constants.MI_Application  + "=" + MailFilterCodes.EIDOMessageRetriever
	}, "EIDO Message retrieval")]

namespace Enterprise.Freight.Agency.ServiceTasks
{
	public sealed class EIDOMessageRetrieverServiceTask : ProcessServiceTask
	{
		public const string Code = "EMR";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Assembly attribute, must remain a constant")]
		public const string Description = "E-IDO Message Retriever";

		public EIDOMessageRetrieverServiceTask()
			: base(new EIDOMessageRetriever()) { }
	}
}
