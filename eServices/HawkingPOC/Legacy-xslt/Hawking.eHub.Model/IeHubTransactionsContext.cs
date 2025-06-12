using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Hawking.eHub.Model.eHubTransactions
{
    public interface IeHubTransactionsContext : IDisposable
    {
        DbSet<eHubAirConnection> eHubAirConnection { get; set; }
        DbSet<eHubAirConnectionPerBranch> eHubAirConnectionPerBranch { get; set; }
        DbSet<eHubAirDefaultServiceProvider> eHubAirDefaultServiceProvider { get; set; }
        DbSet<eHubAirReferenceNumber> eHubAirReferenceNumber { get; set; }
        DbSet<eHubAirServiceProviderMapping> eHubAirServiceProviderMapping { get; set; }
        DbSet<eHubCertificate> eHubCertificate { get; set; }
        DbSet<eHubClient> eHubClient { get; set; }
        DbSet<eHubClientAccessRestriction> eHubClientAccessRestriction { get; set; }
        DbSet<eHubClientBatching> eHubClientBatching { get; set; }
        DbSet<eHubClientCode> eHubClientCode { get; set; }
        DbSet<eHubClientDialogue> eHubClientDialogue { get; set; }
        DbSet<eHubClientRegistration> eHubClientRegistration { get; set; }
        DbSet<eHubClientSystem> eHubClientSystem { get; set; }
        DbSet<eHubClientSystemRegistration> eHubClientSystemRegistration { get; set; }
        DbSet<eHubCodeMapKey> eHubCodeMapKey { get; set; }
        DbSet<eHubCodeMapValue> eHubCodeMapValue { get; set; }
        DbSet<eHubCodeSet> eHubCodeSet { get; set; }
        DbSet<eHubCodeSetResult> eHubCodeSetResult { get; set; }
        DbSet<eHubCounter> eHubCounter { get; set; }
        DbSet<eHubInterfaceCounter> eHubInterfaceCounter { get; set; }
        DbSet<eHubITCustomsJobStatus> eHubITCustomsJobStatus { get; set; }
        DbSet<eHubMessageCopyRegistry> eHubMessageCopyRegistry { get; set; }
        DbSet<eHubMessageReferenceRegistry> eHubMessageReferenceRegistry { get; set; }
        DbSet<eHubMessageType> eHubMessageType { get; set; }
        DbSet<eHubRegistrationType> eHubRegistrationType { get; set; }
        DbSet<eHubRoutingRule> eHubRoutingRule { get; set; }
        DbSet<eHubRoutingRuleFact> eHubRoutingRuleFact { get; set; }
        DbSet<eHubSequenceNumber> eHubSequenceNumber { get; set; }
        DbSet<eHubServiceOperator> eHubServiceOperator { get; set; }
        DbSet<eHubServiceOperatorRegistration> eHubServiceOperatorRegistration { get; set; }
        DbSet<eHubServiceProvider> eHubServiceProvider { get; set; }
        DbSet<eHubServiceProviderRequiredRegistration> eHubServiceProviderRequiredRegistration { get; set; }
        DbSet<eHubSubscriptionAutoSubscribe> eHubSubscriptionAutoSubscribe { get; set; }
        DbSet<eHubSubscriptionBroadcaster> eHubSubscriptionBroadcaster { get; set; }
        DbSet<eHubSubscriptionLookup> eHubSubscriptionLookup { get; set; }
        DbSet<eHubSubscriptionType> eHubSubscriptionType { get; set; }
        DbSet<eHubSubscriptionValue> eHubSubscriptionValue { get; set; }
        DbSet<eHubTransformationMapping> eHubTransformationMapping { get; set; }
        DbSet<eHubTransformationSet> eHubTransformationSet { get; set; }
        DbSet<eHubTransformationType> eHubTransformationType { get; set; }
        DbSet<eHubUSCustomsRegistry> eHubUSCustomsRegistry { get; set; }
        DbSet<eHubZone> eHubZone { get; set; }

        DbConnection GetDbConnection();
    }
}
