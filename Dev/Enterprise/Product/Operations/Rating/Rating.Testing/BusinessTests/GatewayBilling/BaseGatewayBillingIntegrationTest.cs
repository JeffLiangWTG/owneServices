using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.RatingTests.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Gateway
{
	public abstract class BaseGatewayBillingIntegrationTest : BaseRatingIntegrationTest
	{
		#region Autorate and Assert

		protected void AutorateAndAssert<T>(T jobParent, params GatewayBillingTestCase[] testCases)
			where T : IJobHeaderParent, IBusiness => AutorateAndAssert(jobParent, testCases.ToList());

		protected abstract void AutorateAndAssert<T>(T jobParent, IEnumerable<GatewayBillingTestCase> testCases)
			where T : IJobHeaderParent, IBusiness;

		protected IDisposable SetTemporaryUserContext(GlbBranch branch) =>
			SetTemporaryUserContext(new GatewayBillingTestCaseUserContext { Branch = branch });

		protected IDisposable SetTemporaryUserContext(GatewayBillingTestCaseUserContext userContext)
		{
			var branchPK = userContext.Branch != null ? userContext.Branch.PK.ToGuid() : Env.CurrentBranchPK;
			var nonMiscDepartmentPK = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, userContext.DepartmentCode)).PK.ToGuid();

			return Env.SetTemporaryUserContext(Env.CurrentUserPK, branchPK, nonMiscDepartmentPK);
		}

		protected IDisposable SetTemporaryGatewayConfiguration(GatewayBillingTestCase testCase)
		{
			if (testCase.GatewayConfigurations == null)
			{
				return DisposableAction.NoAction;
			}

			var newConfigurationCollection = new AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection();

			foreach (var config in testCase.GatewayConfigurations)
			{
				newConfigurationCollection.Add(
					new AutoratingIntercompanyTariffsForGatewayJobConfiguration()
					{
						LoginAgentRole = config.LoginGatewayAgentRole,
						ShipmentDirection = config.ShipmentDirection,
						AutoratingJob = config.AutoratingJob,
						AutoratingRule = config.AutoratingRule,
						ICTServiceProvider = config.ICTServiceProvider
					});
			}

			return DataRegistryRating.Instance.AutoratingIntercompanyTariffsForGatewayJobConfiguration
				.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newConfigurationCollection);
		}

		#endregion

		#region SetUp/TearDown

		protected override void SetUp()
		{
			base.SetUp();

			var priorities = new RatesPrioritiesCollection();
			priorities.AddNew(RatingDebtorOrgTypes.SAG);
			priorities.AddNew(RatingDebtorOrgTypes.RAG);

			tempPriorities = RatingDataRegistry.Instance.GatewayCollectPriorities.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, priorities);
			tempAutorateGatewayBilling = RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			tempRatesServiceSearch = DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled());
		}

		protected override void TearDown()
		{
			tempPriorities.Dispose();
			tempAutorateGatewayBilling.Dispose();
			tempRatesServiceSearch.Dispose();

			base.TearDown();
		}

		IDisposable tempPriorities = DisposableAction.NoAction;
		IDisposable tempAutorateGatewayBilling = DisposableAction.NoAction;
		IDisposable tempRatesServiceSearch = DisposableAction.NoAction;

		#endregion

		protected GatewayScenarioGenerator ScenarioGenerator =>
			scenarioGenerator ?? (scenarioGenerator = new GatewayScenarioGenerator(this, Factory));
		GatewayScenarioGenerator scenarioGenerator;
	}
}
