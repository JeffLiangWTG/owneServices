using System;
using System.Data.Entity.Core.Objects;
using System.Linq;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Tests.Fakes
{
	public class TestContext : IeHubTransactionsContext
	{
		Exception saveException;

		int saveChangesCount;

		public TestContext()
		{
			saveChangesCount = 0;
		}

		public TestContext(Exception saveException)
		{
			this.saveException = saveException;
			saveChangesCount = 0;
		}

		public int SaveChanges()
		{
			if (saveException != null) throw saveException;
			saveChangesCount++;
			return 1;
		}

		public IObjectSet<ediProdAllOrg> ediProdAllOrgs
		{
			get { return _ediProdAllOrgs ?? (_ediProdAllOrgs = new TestObjectSet<ediProdAllOrg>()); }
			set { _ediProdAllOrgs = value as TestObjectSet<ediProdAllOrg>; }
		}
		private TestObjectSet<ediProdAllOrg> _ediProdAllOrgs;

		public IObjectSet<eHubClient> eHubClients
		{
			get { return _eHubClients ?? (_eHubClients = new TestObjectSet<eHubClient>()); }
			set { _eHubClients = value as TestObjectSet<eHubClient>; }
		}
		private TestObjectSet<eHubClient> _eHubClients;

		public IObjectSet<eHubCodeSet> eHubCodeSets
		{
			get { return _eHubCodeSets ?? (_eHubCodeSets = new TestObjectSet<eHubCodeSet>()); }
			set { _eHubCodeSets = value as TestObjectSet<eHubCodeSet>; }
		}
		private TestObjectSet<eHubCodeSet> _eHubCodeSets;

		public IObjectSet<eHubMessageType> eHubMessageTypes
		{
			get { return _eHubMessageType ?? (_eHubMessageType = new TestObjectSet<eHubMessageType>()); }
			set { _eHubMessageType = value as TestObjectSet<eHubMessageType>; }
		}
		private TestObjectSet<eHubMessageType> _eHubMessageType;

		public IObjectSet<eHubTransformationMapping> eHubTransformationMappings
		{
			get { return _eHubTransformationMappings ?? (_eHubTransformationMappings = new TestObjectSet<eHubTransformationMapping>()); }
			set { _eHubTransformationMappings = value as TestObjectSet<eHubTransformationMapping>; }
		}
		private TestObjectSet<eHubTransformationMapping> _eHubTransformationMappings;

		public IObjectSet<eHubTransformationSet> eHubTransformationSets
		{
			get { return _eHubTransformationSets ?? (_eHubTransformationSets = new TestObjectSet<eHubTransformationSet>()); }
			set { _eHubTransformationSets = value as TestObjectSet<eHubTransformationSet>; }
		}
		private TestObjectSet<eHubTransformationSet> _eHubTransformationSets;

		public IObjectSet<eHubTransformationType> eHubTransformationTypes
		{
			get { return _eHubTransformationTypes ?? (_eHubTransformationTypes = new TestObjectSet<eHubTransformationType>()); }
			set { _eHubTransformationTypes = value as TestObjectSet<eHubTransformationType>; }
		}
		private TestObjectSet<eHubTransformationType> _eHubTransformationTypes;

		public IObjectSet<eHubCodeSetResult> eHubCodeSetResults
		{
			get { return _eHubCodeSetResults ?? (_eHubCodeSetResults = new TestObjectSet<eHubCodeSetResult>()); }
			set { _eHubCodeSetResults = value as TestObjectSet<eHubCodeSetResult>; }
		}
		private TestObjectSet<eHubCodeSetResult> _eHubCodeSetResults;

		public IObjectSet<eHubCodeMapKey> eHubCodeMapKeys
		{
			get { return _eHubCodeMapKeys ?? (_eHubCodeMapKeys = new TestObjectSet<eHubCodeMapKey>()); }
			set { _eHubCodeMapKeys = value as TestObjectSet<eHubCodeMapKey>; }
		}
		private TestObjectSet<eHubCodeMapKey> _eHubCodeMapKeys;

		public IObjectSet<eHubCodeMapValue> eHubCodeMapValues
		{
			get { return _eHubCodeMapValues ?? (_eHubCodeMapValues = new TestObjectSet<eHubCodeMapValue>()); }
			set { _eHubCodeMapValues = value as TestObjectSet<eHubCodeMapValue>; }
		}
		private TestObjectSet<eHubCodeMapValue> _eHubCodeMapValues;


		public IObjectSet<eHubZone> eHubZones
		{
			get { return _eHubZones ?? (_eHubZones = new TestObjectSet<eHubZone>()); }
			set { _eHubZones = value as TestObjectSet<eHubZone>; }
		}
		private TestObjectSet<eHubZone> _eHubZones;

		public IObjectSet<eHubAirConnection> eHubAirConnections
		{
			get { return _eHubAirConnections ?? (_eHubAirConnections = new TestObjectSet<eHubAirConnection>()); }
			set { _eHubAirConnections = value as TestObjectSet<eHubAirConnection>; }
		}
		private TestObjectSet<eHubAirConnection> _eHubAirConnections;

		public IObjectSet<eHubAirDefaultServiceProvider> eHubAirDefaultServiceProviders
		{
			get { return _eHubAirDefaultServiceProviders ?? (_eHubAirDefaultServiceProviders = new TestObjectSet<eHubAirDefaultServiceProvider>()); }
			set { _eHubAirDefaultServiceProviders = value as TestObjectSet<eHubAirDefaultServiceProvider>; }
		}
		private TestObjectSet<eHubAirDefaultServiceProvider> _eHubAirDefaultServiceProviders;

		public IObjectSet<eHubAirServiceProviderMapping> eHubAirServiceProviderMappings
		{
			get { return _eHubAirServiceProviderMappings ?? (_eHubAirServiceProviderMappings = new TestObjectSet<eHubAirServiceProviderMapping>()); }
			set { _eHubAirServiceProviderMappings = value as TestObjectSet<eHubAirServiceProviderMapping>; }
		}
		private TestObjectSet<eHubAirServiceProviderMapping> _eHubAirServiceProviderMappings;

		public IObjectSet<eHubUSCustomsRegistry> eHubUSCustomsRegistry
		{
			get { return _eHubUSCustomsRegistry ?? (_eHubUSCustomsRegistry = new USCustomsRegistrySet(this)); }
		}
		private TestObjectSet<eHubUSCustomsRegistry> _eHubUSCustomsRegistry;

		public IObjectSet<eHubITCustomsJobStatu> eHubITCustomsJobStatus
		{
			get { return _eHubITCustomsJobStatus ?? (_eHubITCustomsJobStatus = new TestObjectSet<eHubITCustomsJobStatu>()); }
		}
		private TestObjectSet<eHubITCustomsJobStatu> _eHubITCustomsJobStatus;

		public IObjectSet<eHubAirConnectionPerBranch> eHubAirConnectionPerBranches
		{
			get { return _eHubAirConnectionPerBranches ?? (_eHubAirConnectionPerBranches = new TestObjectSet<eHubAirConnectionPerBranch>()); }
		}
		private TestObjectSet<eHubAirConnectionPerBranch> _eHubAirConnectionPerBranches;

		public IObjectSet<eHubMessageReferenceRegistry> eHubMessageReferenceRegistries
		{
			get { return _eHubMessageReferenceRegistries ?? (_eHubMessageReferenceRegistries = new TestObjectSet<eHubMessageReferenceRegistry>()); }
		}
		private TestObjectSet<eHubMessageReferenceRegistry> _eHubMessageReferenceRegistries;

		public IObjectSet<eHubSubscriptionAutoSubscribe> eHubSubscriptionAutoSubscribes
		{
			get { return _eHubSubscriptionAutoSubscribes ?? (_eHubSubscriptionAutoSubscribes = new TestObjectSet<eHubSubscriptionAutoSubscribe>()); }
		}
		private TestObjectSet<eHubSubscriptionAutoSubscribe> _eHubSubscriptionAutoSubscribes;

		public IObjectSet<eHubSubscriptionLookup> eHubSubscriptionLookups
		{
			get { return _eHubSubscriptionLookups ?? (_eHubSubscriptionLookups = new TestObjectSet<eHubSubscriptionLookup>()); }
		}
		private TestObjectSet<eHubSubscriptionLookup> _eHubSubscriptionLookups;

		public IObjectSet<eHubSubscriptionType> eHubSubscriptionTypes
		{
			get { return _eHubSubscriptionTypes ?? (_eHubSubscriptionTypes = new TestObjectSet<eHubSubscriptionType>()); }
		}
		private TestObjectSet<eHubSubscriptionType> _eHubSubscriptionTypes;

		public IObjectSet<eHubSubscriptionValue> eHubSubscriptionValues
		{
			get { return _eHubSubscriptionValues ?? (_eHubSubscriptionValues = new TestObjectSet<eHubSubscriptionValue>()); }
		}
		private TestObjectSet<eHubSubscriptionValue> _eHubSubscriptionValues;

		public IObjectSet<eHubSubscriptionBroadcaster> eHubSubscriptionBroadcasters
		{
			get { return _eHubSubscriptionBroadcasters ?? (_eHubSubscriptionBroadcasters = new TestObjectSet<eHubSubscriptionBroadcaster>()); }
		}
		private TestObjectSet<eHubSubscriptionBroadcaster> _eHubSubscriptionBroadcasters;

		class USCustomsRegistrySet : TestObjectSet<eHubUSCustomsRegistry>, IObjectSet<eHubUSCustomsRegistry>
		{
			TestContext context;
			public USCustomsRegistrySet(TestContext context) { this.context = context; }

			void IObjectSet<eHubUSCustomsRegistry>.DeleteObject(eHubUSCustomsRegistry entity)
			{
				base.DeleteObject(entity);
				var client = context.eHubClients.Where(c => c.CC_PK == entity.ER_CC_Client).First();
				foreach (var r in client.eHubUSCustomsRegistry)
				{
					if (entity.ER_ApplicationCode == r.ER_ApplicationCode && entity.ER_Name == r.ER_Name)
					{
						client.eHubUSCustomsRegistry.Remove(r);
						break;
					}
				}
			}
		}

		public IObjectSet<eHubRegistrationType> eHubRegistrationTypes
		{
			get { return _eHubRegistrationTypes ?? (_eHubRegistrationTypes = new TestObjectSet<eHubRegistrationType>()); }
		}
		private TestObjectSet<eHubRegistrationType> _eHubRegistrationTypes;

		public IObjectSet<eHubClientRegistration> eHubClientRegistrations
		{
			get { return _eHubClientRegistrations ?? (_eHubClientRegistrations = new TestObjectSet<eHubClientRegistration>()); }
		}
		private TestObjectSet<eHubClientRegistration> _eHubClientRegistrations;

		public IObjectSet<eHubServiceOperator> eHubServiceOperators
		{
			get { return _eHubServiceOperators ?? (_eHubServiceOperators = new TestObjectSet<eHubServiceOperator>()); }
		}
		private TestObjectSet<eHubServiceOperator> _eHubServiceOperators;

		public IObjectSet<eHubServiceOperatorRegistration> eHubServiceOperatorRegistrations
		{
			get { return _eHubServiceOperatorRegistrations ?? (_eHubServiceOperatorRegistrations = new TestObjectSet<eHubServiceOperatorRegistration>()); }
		}
		private TestObjectSet<eHubServiceOperatorRegistration> _eHubServiceOperatorRegistrations;

		public IObjectSet<eHubServiceProvider> eHubServiceProviders
		{
			get { return _eHubServiceProviders ?? (_eHubServiceProviders = new TestObjectSet<eHubServiceProvider>()); }
		}
		private TestObjectSet<eHubServiceProvider> _eHubServiceProviders;

		public IObjectSet<eHubServiceProviderRequiredRegistration> eHubServiceProviderRequiredRegistrations
		{
			get { return _eHubServiceProviderRequiredRegistrations ?? (_eHubServiceProviderRequiredRegistrations = new TestObjectSet<eHubServiceProviderRequiredRegistration>()); }
		}
		private TestObjectSet<eHubServiceProviderRequiredRegistration> _eHubServiceProviderRequiredRegistrations;

		public IObjectSet<eHubRoutingRule> eHubRoutingRules
		{
			get { return _eHubRoutingRules ?? (_eHubRoutingRules = new TestObjectSet<eHubRoutingRule>()); }
		}
		private TestObjectSet<eHubRoutingRule> _eHubRoutingRules;

		public IObjectSet<eHubRoutingRuleFact> eHubRoutingRuleFacts
		{
			get { return _eHubRoutingRuleFacts ?? (_eHubRoutingRuleFacts = new TestObjectSet<eHubRoutingRuleFact>()); }
		}
		private TestObjectSet<eHubRoutingRuleFact> _eHubRoutingRuleFacts;

		public IObjectSet<eHubCertificate> eHubCertificates
		{
			get { return _eHubCertificates ?? (_eHubCertificates = new TestObjectSet<eHubCertificate>()); }
		}
		private TestObjectSet<eHubCertificate> _eHubCertificates;

		public IObjectSet<eHubClientSystem> eHubClientSystems
		{
			get { return _eHubClientSystems ?? (_eHubClientSystems = new TestObjectSet<eHubClientSystem>()); }
		}
		private TestObjectSet<eHubClientSystem> _eHubClientSystems;

		public IObjectSet<eHubClientSystemRegistration> eHubClientSystemRegistrations
		{
			get { return _eHubClientSystemRegistrations ?? (_eHubClientSystemRegistrations = new TestObjectSet<eHubClientSystemRegistration>()); }
		}

		private TestObjectSet<eHubClientSystemRegistration> _eHubClientSystemRegistrations;

		public IObjectSet<eHubAsyncPollingRegistration> eHubAsyncPollingRegistrations
		{
			get { return _eHubAsyncPollingRegistrations ?? (_eHubAsyncPollingRegistrations = new TestObjectSet<eHubAsyncPollingRegistration>()); }
		}

		private TestObjectSet<eHubAsyncPollingRegistration> _eHubAsyncPollingRegistrations;

		public IObjectSet<eHubClientAuthorisation> eHubClientAuthorisations
		{
			get { return _eHubClientAuthorisations ?? (_eHubClientAuthorisations = new TestObjectSet<eHubClientAuthorisation>()); }
		}

		private TestObjectSet<eHubClientAuthorisation> _eHubClientAuthorisations;

		public IObjectSet<ediProdClient> ediProdClients
		{
			get { return _ediProdClients ?? (_ediProdClients = new TestObjectSet<ediProdClient>()); }
		}

		public bool LazyLoadingEnabled { get; set; }

		private TestObjectSet<ediProdClient> _ediProdClients;

		public int GetSaveChangesCount()
		{
			return saveChangesCount;
		}
	}
}
