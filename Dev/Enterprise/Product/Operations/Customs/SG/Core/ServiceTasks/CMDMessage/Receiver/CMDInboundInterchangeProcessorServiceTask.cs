using System.Globalization;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	CMDInboundInterchangeProcessorServiceTask.CMDInterchangeRetrieverServiceCode,
	CMDInboundInterchangeProcessorServiceTask.CMDInterchangeRetrieverServiceName,
	"SCM",
	typeof(CMDInboundInterchangeProcessorServiceTask),
	MinimumPeriod = "5minute",
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Singapore,
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(CMDInboundInterchangeProcessorServiceTask.CMDInterchangeRetrieverServiceCode,
	EDIInterchangeSchema.Constants.TableName,
	new[] { EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.SingaporeCMD },
		"Cargo Manifest Declaration (SG) interchanges inbound")]

namespace Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage
{
	internal class CMDInboundInterchangeProcessorServiceTask : CMDServiceTask
	{
		public const string CMDInterchangeRetrieverServiceCode = "SGI";
		public const string CMDInterchangeRetrieverServiceName = "Cargo Manifest Declaration (SG) Interchange Processor";

		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var company in new GlbCompany.Loader(new BusinessObjectFactory()).LoadCompanies(Core.Constants.CountryCodes.Singapore))
			{
				token.ThrowIfCancellationRequested();
				var branch = company.FirstActiveBranch;
				if (branch != null)
				{
					using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
					{
						ServiceLogger.Information(string.Format(CultureInfo.CurrentCulture, "SGI Processing for Company {0}.", company.GC_Code));
						using (var sender = new CMDInterchangeProcessor())
						{
							try
							{
								sender.Logger.OnLogInfoAdded += Log;
								sender.ExecuteBatch(token);
							}
							finally
							{
								sender.Logger.OnLogInfoAdded -= Log;
							}
						}
					}
				}
			}
		}
	}
}
