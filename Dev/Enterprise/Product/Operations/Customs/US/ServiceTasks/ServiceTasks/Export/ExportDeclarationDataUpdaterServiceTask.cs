using System.Threading;
using CargoWise.Common;
using Enterprise.Core;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.ExportDeclarationDataUpdater,
	Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Descriptions.ExportDeclarationDataUpdater,
	"USC",
	typeof(Enterprise.Customs.US.ServiceTasks.ExportDeclarationDataUpdaterServiceTask),
	RequiresCompanyInCountry = Constants.CountryCodes.UnitedStates + "," + Constants.CountryCodes.PuertoRico,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Hour",
	DefaultScheduleRunEvery = "1hour",
	ActiveByDefault = true
	)]

namespace Enterprise.Customs.US.ServiceTasks
{
	public class ExportDeclarationDataUpdaterServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		[HostedServiceRequirement]
		public static string CheckEDUServiceTaskRegistryEnabled() => HostedServiceRequirementAttribute.CheckValueIsNotEqualTo(USCustomsDataRegistry.Instance.EnableExportDeclarationDataUpdaterServiceTask, false);

		protected override void RunTaskCore(CancellationToken token)
		{
			DisposableEnvironment.GetActiveCompanies(USCustomsJurisdiction.Countries).ForEach(companyCode =>
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForCompany(companyCode))
				{
					var processor = new ExportDeclarationDataUpdaterProcessor(Logger);
					processor.ExecuteBatch(token);
				}
			});
		}
	}
}
