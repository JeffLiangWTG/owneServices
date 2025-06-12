using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using CargoWise.eHub.DataModel.Common;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubTransactionsContext : ContextBase
	{
		static eHubTransactionsContext()
		{
			Database.SetInitializer<eHubTransactionsContext>(null);
		}

		public eHubTransactionsContext()
		{
		}

		public eHubTransactionsContext(string connectionString)
			: base(connectionString)
		{
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
			modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
			modelBuilder.Entity<eHubMonitor>()
				.HasMany<eHubClient>(m => m.eHubClients)
				.WithMany(c => c.eHubMonitors)
				.Map(cs =>
				{
					cs.MapLeftKey("MC_MO");
					cs.MapRightKey("MC_CC");
					cs.ToTable("eHubMonitorClient");
				});
		}

		public virtual DbSet<ediProdClient> ediProdClients { get; set; }
		public virtual DbSet<eHubCertificate> eHubCertificates { get; set; }
		public virtual DbSet<eHubClient> eHubClients { get; set; }
		public virtual DbSet<eHubClientRegistration> eHubClientRegistrations { get; set; }
		public virtual DbSet<eHubClientSystem> eHubClientSystems { get; set; }
		public virtual DbSet<eHubClientSystemRegistration> eHubClientSystemRegistrations { get; set; }
		public virtual DbSet<eHubCodeMapKey> eHubCodeMapKeys { get; set; }
		public virtual DbSet<eHubCodeMapValue> eHubCodeMapValues { get; set; }
		public virtual DbSet<eHubCodeSet> eHubCodeSets { get; set; }
		public virtual DbSet<eHubCodeSetResult> eHubCodeSetResults { get; set; }
		public virtual DbSet<eHubError> eHubErrors { get; set; }
		public virtual DbSet<eHubInboxMessage> eHubInboxMessages { get; set; }
		public virtual DbSet<eHubInboxMessageDisplay> eHubInboxMessageDisplays { get; set; }
		public virtual DbSet<eHubInterfaceCounter> eHubInterfaceCounters { get; set; }
		public virtual DbSet<eHubITCustomsJobStatus> eHubITCustomsJobStatuses { get; set; }
		public virtual DbSet<eHubInboxMessageArchive> eHubInboxMessageArchives { get; set; }
		public virtual DbSet<eHubInboxXmlContent> eHubInboxXmlContents { get; set; }
		public virtual DbSet<eHubMessageType> eHubMessageTypes { get; set; }
		public virtual DbSet<eHubOutboxMessage> eHubOutboxMessages { get; set; }
		public virtual DbSet<eHubOutboxMessageDisplay> eHubOutboxMessageDisplays { get; set; }
		public virtual DbSet<eHubOutboxMessageArchive> eHubOutboxMessageArchives { get; set; }
		public virtual DbSet<eHubRegistrationType> eHubRegistrationTypes { get; set; }
		public virtual DbSet<eHubRoutingRule> eHubRoutingRules { get; set; }
		public virtual DbSet<eHubRoutingRuleFact> eHubRoutingRuleFacts { get; set; }
		public virtual DbSet<eHubServiceOperator> eHubServiceOperators { get; set; }
		public virtual DbSet<eHubServiceOperatorRegistration> eHubServiceOperatorRegistrations { get; set; }
		public virtual DbSet<eHubServiceProvider> eHubServiceProviders { get; set; }
		public virtual DbSet<eHubServiceProviderRequiredRegistration> eHubServiceProviderRequiredRegistrations { get; set; }
		public virtual DbSet<eHubSubscriptionAutoSubscribe> eHubSubscriptionAutoSubscribes { get; set; }
		public virtual DbSet<eHubSubscriptionBroadcaster> eHubSubscriptionBroadcasters { get; set; }
		public virtual DbSet<eHubSubscriptionLookup> eHubSubscriptionLookups { get; set; }
		public virtual DbSet<eHubSubscriptionType> eHubSubscriptionTypes { get; set; }
		public virtual DbSet<eHubSubscriptionValue> eHubSubscriptionValues { get; set; }
		public virtual DbSet<eHubTransformationMapping> eHubTransformationMapping { get; set; }
		public virtual DbSet<eHubTransformationSet> eHubTransformationSets { get; set; }
		public virtual DbSet<eHubTransformationType> eHubTransformationTypes { get; set; }
		public virtual DbSet<eHubZone> eHubZones { get; set; }
		public virtual DbSet<eHubAsyncPollingRegistration> eHubAsyncPollingRegistrations { get; set; }
		public virtual DbSet<eHubClientAuthorisation> eHubClientAuthorisations { get; set; }
		public virtual DbSet<eHubMonitor> eHubMonitors { get; set; }
		public virtual DbSet<eHubUSCustomsRegistry> eHubUSCustomsRegistries { get; set; }
		public virtual DbSet<eHubOutboxParkingFilter> eHubOutboxParkingFilters { get; set; }
		public virtual DbSet<eHubClientSubstitute> eHubClientSubstitutes { get; set; }
		public virtual DbSet<eHubOwner> eHubOwners { get; set; }
	}
}
