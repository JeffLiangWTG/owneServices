using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.LandTransport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;

namespace Enterprise.TransportConsignment.ProductionRulesEngine
{
	public abstract class ValueDefaultingManager
	{
		protected ValueDefaultingManager(ILandTransportFactLoaderProvider factLoaderProvider)
		{
			FactLoaderProvider = Argument.NotNull(factLoaderProvider, nameof(factLoaderProvider));
		}

		public IZType FetchDefaultValue(IDtbConsignment entity, ICompany loginCompany, IBranch loginBranch, IDepartment loginDepartment)
		{
			var loader = FactLoaderProvider.GetFactLoader(ContextType);
			if (loader != null)
			{
				return FetchDefaultValue(entity, loginCompany, loginBranch, loginDepartment, loader);
			}
			return null;
		}

		public IZType FetchDefaultValue(IDtbConsignment entity, ICompany loginCompany, IBranch loginBranch, IDepartment loginDepartment, ILandTransportFactLoader loader)
		{
			if (entity is IJobInvoicingPlugIn jobInvoicingPlugIn && jobInvoicingPlugIn.InvoicingSupporter.Job != null)
			{
				var factsArray = loader.GetFacts(entity, loginCompany, loginBranch, loginDepartment).ToArray();
				if (factsArray != null && factsArray.Length > 0)
				{
					var invoicingSupporter = jobInvoicingPlugIn.InvoicingSupporter;
					var filter = ProductionRuleSetFilter.WithCompany(invoicingSupporter.Job.JH_GC.ToGuid());

					var rulesEngineService = ObjectFactory.Get<IProductionRulesEnginePushService>(nameof(IProductionRulesEnginePushService), new[] { invoicingSupporter.Job.Factory });
					var result = rulesEngineService.RunRulesEngine(ContextType, filter, factsArray, CancellationToken.None);

					if (result.Status == ResultStatus.Success)
					{
						var fact = result.Facts.OfType<BranchResultFact>().FirstOrDefault();
						if (fact != null)
						{
							return new ZGuid(fact.BranchToDefault.Fact.PK);
						}
					}
				}
			}
			return null;
		}

		abstract protected RulesContextType ContextType { get; }
		public IZType DefaultValue { get; protected set; }
		ILandTransportFactLoaderProvider FactLoaderProvider { get; }
	}
}
