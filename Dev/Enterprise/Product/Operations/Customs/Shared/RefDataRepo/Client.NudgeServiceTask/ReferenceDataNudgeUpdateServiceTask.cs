using System.Threading;
using CargoWise.Application;
using CargoWise.RefDataRepo.Ent.Client.NudgeServiceTask;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Integration.Customs.Shared;

[assembly: HostedService(ApplicationConfiguration.ServiceTask.Code,
	"Reference Data Nudge Updater",
	ApplicationConfiguration.ServiceTask.Category,
	typeof(ReferenceDataNudgeUpdateServiceTask),
	IsMandatory = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1hour",
	DefaultScheduleRunEvery = "1hour",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(ApplicationConfiguration.ServiceTask.Code,
	EDIInterchangeSchema.Constants.TableName,
	new[]
	{
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchangeStatusList.Codes.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.RefDataNudgeUpdate,
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchangeTypeList.Codes.GenericMessageDelivery
	},
	"Reference Data Nudge Updater")]
namespace CargoWise.RefDataRepo.Ent.Client.NudgeServiceTask
{
	public class ReferenceDataNudgeUpdateServiceTask : Enterprise.Customs.ServiceTasks.CustomsServiceTask
	{
		public ReferenceDataNudgeUpdateServiceTask()
			: this(GetWrapper())
		{
		}

		public ReferenceDataNudgeUpdateServiceTask(INudgeUpdaterManagerWrapper nudgeUpdaterWrapper)
		{
			dataSetUpdaterWrapper = nudgeUpdaterWrapper;
		}

		protected override void RunTaskCore(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();

			dataSetUpdaterWrapper.SetLogger(ServiceLogger);
			dataSetUpdaterWrapper.RefDataSetUpdaterWrapper.SetLogger(ServiceLogger);
			dataSetUpdaterWrapper.SRDbDataSetUpdaterWrapper.SetLogger(ServiceLogger);
			using (var processor = new RENInboundInterchangeProcessor(dataSetUpdaterWrapper, Logger))
			{
				processor.ExecuteBatch(token);
			}
		}

		static INudgeUpdaterManagerWrapper GetWrapper()
		{
			return ObjectFactory.Get<INudgeUpdaterManagerWrapper>();
		}

		readonly INudgeUpdaterManagerWrapper dataSetUpdaterWrapper;
	}
}
