using System.Threading;
using Enterprise.Customs.US.DIS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService("DIS",
	"US DIS",
	"USC",
	typeof(Enterprise.Customs.US.DIS.ServiceTasks.USDISServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedStates + "," + Enterprise.Core.Constants.CountryCodes.PuertoRico,
	MinimumPeriod = "30Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding("DIS",
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsDIS,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	"US Customs DIS messages outbound"
	)]

[assembly: HostedServiceBusinessObjectBinding("DIS",
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsDIS,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	"US Customs DIS messages inbound"
	)]

[assembly: HostedServiceBusinessObjectBinding("DIS",
	EDIInterchangeSchema.Constants.TableName,
	new[]
	{
		EDIInterchangeSchema.Constants.EI_Status          + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive        + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsDIS
	},
	"US Customs DIS interchanges inbound"
	)]

namespace Enterprise.Customs.US.DIS.ServiceTasks
{
	public class USDISServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var company in GlbCompany.GetActiveCompanies(company => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(company.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates))
			{
				using (DisposableEnvironment.ForBranch(company.FirstActiveBranch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						new OutgoingMessageProcessor(Logger).ProcessMessage(token);
						new InboundInterchangeProcessor(Logger).ExecuteBatch(token);
						new InboundMessageProcessor() { Logger = Logger }.ExecuteBatch(token);
					});
				}
			}
		}
	}
}
