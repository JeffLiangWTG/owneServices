using System.Threading;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(ServiceTaskApplicationCodeList.Codes.USABIOutbound,
	ServiceTaskApplicationCodeList.Descriptions.USABIOutbound,
	"USC",
	typeof(ABIOutgoingServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedStates + "," + Enterprise.Core.Constants.CountryCodes.PuertoRico,
	CanRunInAnyBranch = true,
	MinimumPeriod = "30Seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(ServiceTaskApplicationCodeList.Codes.USABIOutbound,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Transmit,
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsImport,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_MessageType + "!=" + ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	"US Customs ABI Messages Outbound"
	)]

namespace Enterprise.Customs.US.ServiceTasks
{
	public class ABIOutgoingServiceTask : Customs.ServiceTasks.NudgeCustomsServiceTask
	{
		protected override string CurrentServiceTaskCode => ServiceTaskApplicationCodeList.Codes.USABIOutbound;

		protected override void RunMainTask(CancellationToken token)
		{
			foreach (var company in GlbCompany.GetActiveCompanies(company => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(company.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates))
			{
				using (DisposableEnvironment.ForBranch(company.FirstActiveBranch.PK.ToGuid()))
				{
					new ABIOutgoingMessageProcessor(Logger).ProcessMessage(token);
				}
			}
		}
	}
}
