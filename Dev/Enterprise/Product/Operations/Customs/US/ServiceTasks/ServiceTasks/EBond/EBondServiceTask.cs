using System.Threading;
using CargoWise.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.EBond,
	Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Descriptions.EBond,
	"USC",
	typeof(Enterprise.Customs.US.ServiceTasks.EBondServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedStates + "," + Enterprise.Core.Constants.CountryCodes.PuertoRico,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Minute",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.EBond,
	EDIInterchangeSchema.Constants.TableName,
	new[] { EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USeBond },
	"US Customs eBond interchanges inbound"
)]

namespace Enterprise.Customs.US.ServiceTasks
{
	public class EBondServiceTask : Customs.ServiceTasks.NudgeCustomsServiceTask
	{
		protected override string CurrentServiceTaskCode => Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.EBond;

		protected override void RunMainTask(CancellationToken token)
		{
			DisposableEnvironment.GetActiveCompanies(USCustomsJurisdiction.Countries).ForEach(companyCode =>
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForCompany(companyCode))
				{
					new EBondIncomingMessageProcessor() { Logger = this.Logger }.ExecuteBatch(token);
				}
			});
		}
	}
}
