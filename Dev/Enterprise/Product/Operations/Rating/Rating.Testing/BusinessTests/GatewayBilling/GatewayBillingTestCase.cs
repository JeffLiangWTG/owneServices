using System.Collections.Generic;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.RatingTests.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Gateway
{
	public class GatewayBillingTestCase
	{
		public string ScenarioDescription { get; set; }

		public GatewayBillingTestCaseUserContext UserContext { get; set; } = new GatewayBillingTestCaseUserContext();
		public AutoratingParameters AssertionParameters { get; set; } = new AutoratingParameters();
		public IEnumerable<RegistryConfiguration> GatewayConfigurations { get; set; }
		public AssertionWithHtml.VoidParameterlessDelegate preconditionAssertions { get; set; }
	}

	public class GatewayBillingTestCaseUserContext
	{
		public GlbBranch Branch { get; set; }
		public string DepartmentCode { get; set; } = "GEA";
	}

	public class AutoratingParameters
	{
		public OrgHeader LocalClient { get; set; }
		public OrgHeader Agent { get; set; }
		public Job Job { get; set; }
		public IGenericJobCostPlugIn CostsSupporter { get; set; }
		public bool DeleteExistingCosts { get; set; } = true;

		public bool AutorateRevenue { get; set; } = true;
		public bool AutorateCosts { get; set; } = true;
		public bool IsConsolLevelChargeExcluded { get; set; }
		public bool StandaloneShipmentOnly { get; set; }

		public IAutoRatingGUIInteractor Interactor { get; set; }

		public Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>> ExpectedInvoicingCharges { get; set; }
		public IEnumerable<AssertionCharge> ExpectedCharges { get; set; }
		public IEnumerable<AssertionCost> ExpectedCosts { get; set; }
		public string[] ExpectedWarnings { get; set; }
		public string[] ExpectedErrors { get; set; }
	}

	public class RegistryConfiguration
	{
		public RegistryConfiguration(
			string loginGatewayAgentRole,
			string shipmentDirection,
			string autoratingJob,
			string autoratingRule,
			string ictServiceProvider)
		{
			LoginGatewayAgentRole = loginGatewayAgentRole;
			ShipmentDirection = shipmentDirection;
			AutoratingJob = autoratingJob;
			AutoratingRule = autoratingRule;
			ICTServiceProvider = ictServiceProvider;
		}

		public string LoginGatewayAgentRole { get; set; }
		public string ShipmentDirection { get; set; }
		public string AutoratingJob { get; set; }
		public string AutoratingRule { get; set; }
		public string ICTServiceProvider { get; set; }
	}
}
