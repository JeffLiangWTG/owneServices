using System.Data.Entity.Core.Objects;

namespace CargoWise.eHub.Portal.Models.eHubTransactions
{
	public interface IeHubTransactionsContext
	{
		IObjectSet<ediProdAllOrg> ediProdAllOrgs { get; }
		IObjectSet<ediProdClient> ediProdClients { get; }
		IObjectSet<eHubClient> eHubClients { get; }
		IObjectSet<eHubCodeSet> eHubCodeSets { get; }
		IObjectSet<eHubCodeSetResult> eHubCodeSetResults { get; }
		IObjectSet<eHubCodeMapKey> eHubCodeMapKeys { get; }
		IObjectSet<eHubCodeMapValue> eHubCodeMapValues { get; }
		IObjectSet<eHubMessageType> eHubMessageTypes { get; }
		IObjectSet<eHubTransformationSet> eHubTransformationSets { get; }
		IObjectSet<eHubTransformationMapping> eHubTransformationMappings { get; }
		IObjectSet<eHubTransformationType> eHubTransformationTypes { get; }
		IObjectSet<eHubZone> eHubZones { get; }
		IObjectSet<eHubAirConnection> eHubAirConnections { get; }
		IObjectSet<eHubAirDefaultServiceProvider> eHubAirDefaultServiceProviders { get; }
		IObjectSet<eHubAirServiceProviderMapping> eHubAirServiceProviderMappings { get; }
		IObjectSet<eHubUSCustomsRegistry> eHubUSCustomsRegistry { get; }
		IObjectSet<eHubAirConnectionPerBranch> eHubAirConnectionPerBranches { get; }
		IObjectSet<eHubMessageReferenceRegistry> eHubMessageReferenceRegistries { get; }
		IObjectSet<eHubSubscriptionAutoSubscribe> eHubSubscriptionAutoSubscribes { get; }
		IObjectSet<eHubSubscriptionLookup> eHubSubscriptionLookups { get; }
		IObjectSet<eHubSubscriptionType> eHubSubscriptionTypes { get; }
		IObjectSet<eHubSubscriptionValue> eHubSubscriptionValues { get; }
		IObjectSet<eHubSubscriptionBroadcaster> eHubSubscriptionBroadcasters { get; }
		IObjectSet<eHubRegistrationType> eHubRegistrationTypes { get; }
		IObjectSet<eHubClientRegistration> eHubClientRegistrations { get; }
		IObjectSet<eHubServiceOperator> eHubServiceOperators { get; }
		IObjectSet<eHubServiceOperatorRegistration> eHubServiceOperatorRegistrations { get; }
		IObjectSet<eHubServiceProvider> eHubServiceProviders { get; }
		IObjectSet<eHubServiceProviderRequiredRegistration> eHubServiceProviderRequiredRegistrations { get; }
		IObjectSet<eHubRoutingRule> eHubRoutingRules { get; }
		IObjectSet<eHubRoutingRuleFact> eHubRoutingRuleFacts { get; }
        IObjectSet<eHubCertificate> eHubCertificates { get; }
        IObjectSet<eHubClientSystem> eHubClientSystems { get; }
        IObjectSet<eHubClientSystemRegistration> eHubClientSystemRegistrations { get; }
		IObjectSet<eHubITCustomsJobStatu> eHubITCustomsJobStatus { get; }
		IObjectSet<eHubAsyncPollingRegistration> eHubAsyncPollingRegistrations { get; }
		IObjectSet<eHubClientAuthorisation> eHubClientAuthorisations { get; }

		bool LazyLoadingEnabled { get; set; }

		int SaveChanges();
	}
}
