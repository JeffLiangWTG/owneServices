using System;
using System.Collections.Generic;
#if NET48
using eServices.eHubDataModel.eHubTransactions;
#else
using eServices.eHubDataModel.eHubTransactionsCore;
#endif

namespace eServices.eHubRoutingRuleEngine
{
	public class ServiceProvider
	{
		public ServiceProvider(eHubClient serviceClient, eHubClient providerClient)
		{
			this.eHubServiceProvider = new eHubServiceProvider { eHubClient_Service = serviceClient, eHubClient_Provider = providerClient };
			this.serviceClient = serviceClient;
			this.providerClient = providerClient;
		}

		internal ServiceProvider(eHubServiceProvider serviceProvider)
		{
			this.eHubServiceProvider = serviceProvider;
			this.serviceClient = serviceProvider.eHubClient_Service;
			this.providerClient = serviceProvider.eHubClient_Provider;
			this.routingRule = Criterion.LoadRuleType(serviceProvider.eHubRoutingRule);
			this.RequiredRegistrations = this.eHubServiceProvider.eHubServiceProviderRequiredRegistrations;
			this.RequiredRegistrations.ForEach(r => r.eHubRegistrationType = r.eHubRegistrationType);
		}

		internal void Delete()
		{
			if (this.routingRule != null)
				this.routingRule.Delete();
			if (this.persistingContext != null)
				this.persistingContext.eHubServiceProviders.Remove(this.eHubServiceProvider);
		}

		internal void SetRuleContexts(Rule rule, eHubTransactionsContext persistingContext)
		{
			this.persistingContext = persistingContext;
			if (this.routingRule != null)
				this.routingRule.SetRuleContexts(rule, persistingContext);
		}

		eHubClient serviceClient;
		public eHubClient ServiceClient
		{
			get { return serviceClient; }
			set 
			{
				this.serviceClient = value;
				this.eHubServiceProvider.eHubClient_Service = value; 
			}
		}

		eHubClient providerClient;
		public eHubClient ProviderClient
		{
			get { return providerClient; }
			set 
			{
				this.providerClient = value;
				this.eHubServiceProvider.eHubClient_Provider = value;
			}
		}

		public List<eHubServiceProviderRequiredRegistration> RequiredRegistrations { get; private set; }

		private Criterion routingRule;
		public Criterion RoutingRule
		{
			get { return this.routingRule; }
			set 
			{
				if (!Object.ReferenceEquals(value, this.routingRule))
				{
					if (this.routingRule != null)
						this.routingRule.Delete();
					if (value == null)
						this.eHubServiceProvider.eHubRoutingRule = null;
					else
						this.eHubServiceProvider.eHubRoutingRule = value.eHubRoutingRule;
					this.routingRule = value;
				}
			}
		}

		public bool AllowFallback
		{
			get { return eHubServiceProvider.SP_AllowFallback; }
			set { eHubServiceProvider.SP_AllowFallback = value; }
		}

		internal eHubServiceProvider eHubServiceProvider;
		eHubTransactionsContext persistingContext;
	}
}
