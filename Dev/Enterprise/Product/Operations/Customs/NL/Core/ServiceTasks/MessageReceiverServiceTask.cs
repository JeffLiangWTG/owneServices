using System.Threading;
using Enterprise.Customs.NL.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.NL.ServiceTasks.MessageReceiverServiceTask.Code,
	Enterprise.Customs.NL.ServiceTasks.MessageReceiverServiceTask.FriendlyName,
	Enterprise.Customs.NL.ServiceTasks.MessagingServiceTask.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.NL.ServiceTasks.MessageReceiverServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Netherlands,
	CanRunInAnyBranch = true,
	MinimumPeriod = "60Seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.NL.ServiceTasks.MessageReceiverServiceTask.Code,
	EDIInterchangeSchema.Constants.TableName,
	new[] { EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
				 EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
				 EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				 EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.NLCustoms },
	"NL Customs interchanges inbound"
	)]

namespace Enterprise.Customs.NL.ServiceTasks;

public class MessageReceiverServiceTask : MessagingServiceTask
{
	public const string Code = "NLR";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Name")]
	public const string FriendlyName = "NL Customs Message Retriever";

	protected override void RunTaskCore(CancellationToken token)
	{
		using (DisposableEnvironment.ForBranch(GlbBranch.GetFirstActiveBranch(Core.Constants.CountryCodes.Netherlands).PK.ToGuid()))
		{
			token.ThrowIfCancellationRequested();
			RunTaskHandleEmailSendFailure(() =>
			{
				var logger = GetNewLogger();
				using (var processor = new NLInboundInterchangeProcessor(logger))
				{
					processor.ExecuteBatch(token);
				}
			});
		}
	}
}
