using System.Globalization;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V4.Business.CMDMessaging;
using Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	CMDInboundMessageServiceTask.CMDInboundXDCMessageServiceCode,
	CMDInboundMessageServiceTask.CMDInboundXDCMessageServiceName,
	"SCM",
	typeof(CMDInboundMessageServiceTask),
	MinimumPeriod = "5minute",
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Singapore,
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(CMDInboundMessageServiceTask.CMDInboundXDCMessageServiceCode,
	EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_IsActive    + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.SingaporeCMD,
		EDIMessageSchema.Constants.EM_MessageType     + "=" + EDIMessageTypeList.Codes.XDC },
		CMDInboundMessageServiceTask.CMDInboundXDCMessageServiceName)]

namespace Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage
{
	internal class CMDInboundMessageServiceTask : CMDServiceTask
	{
		public const string CMDInboundXDCMessageServiceCode = "SCP";
		public const string CMDInboundXDCMessageServiceName = "Cargo Manifest Declaration (SG) Message Processor";

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
						ServiceLogger.Information(string.Format(CultureInfo.CurrentCulture, "SCP Processing for Company {0}.", company.GC_Code));

						var processor = new BatchCMDMessageProcessor(Logger);
						processor.ExecuteBatch(token);
					}
				}
			}
		}
	}
}
